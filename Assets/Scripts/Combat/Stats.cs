using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using NaughtyAttributes;
using Autohand;

public class Stats : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField, Tooltip("health")]
    internal float health = 20f;

    [SerializeField, Tooltip("maximum health")]
    internal float maxHealth = 20f;

    [SerializeField, Tooltip("unarmed damage")]
    internal float damage = 2f;

    public UnityEvent OnTakeDamage;
    public UnityEvent OnDeath;
    public AutoHandPlayer player;

    [SerializeField, Tooltip("Destroy the gameObject immediately on death")]
    private bool destroyOnDeath = true;

    [SerializeField, Tooltip("Reload the scene immediately on death. Used mainly for the player.")]
    private bool reloadSceneOnDeath = false;

    [SerializeField, Tooltip("Move to spawn on death, with a short stun. Used mainly for the player.")]
    private bool moveToSpawnOnDeath = false;

    [SerializeField, ShowIf("moveToSpawnOnDeath")]
    private float respawnTimer = 5f;

    [SerializeField, ShowIf("moveToSpawnOnDeath")]
    private GameObject trackedObjects;

    [SerializeField, ShowIf("moveToSpawnOnDeath")]
    private GameObject respawnBarrier;

    private Vector3 _trackedObjectsStartPos;

    [SerializeField, ShowIf("moveToSpawnOnDeath")]
    private Volume volume;

    [SerializeField, Tooltip("If the character only takes damage from hits to weak points.")]
    private bool weakpointDamageOnly = false;

    [SerializeField, Tooltip("Optional, used to visually display health")]
    private Text debugText;

    private List<GameObject> currCollisions = new List<GameObject>();

    [SerializeField, Tooltip("Optional, sets the mesh to be soft red on death")]
    private SkinnedMeshRenderer mesh;

    [SerializeField, Tooltip("Optional animator, used to stop all animations on death")]
    private Animator anim;

    [SerializeField, Tooltip("Optional, used to set isKinematic false on death")]
    private Rigidbody[] ragdoll;

    private float stabCD = 0f;

    private Vector3 _startPos;

    private float timer = 0f;

    void Start()
    {
        _startPos = transform.position;
        _trackedObjectsStartPos = trackedObjects.transform.position;
    }

    void Update()
    {
        if (debugText)
        {
            debugText.text = string.Format("Health: {0}/{1}", health, maxHealth);
        }
        if (stabCD > 0)
            stabCD -= Time.deltaTime;
    }

    // When damage is received
    internal void OnDamageReceived(float damage)
    {
        OnTakeDamage.Invoke();
        if (damage > 0)
            health -= damage;
        if (health <= 0)
            OnKill();
    }

    // When damage is healed
    void OnHeal(float healAmount)
    {
        if (healAmount > 0)
            health += healAmount;
        if (health >= maxHealth)
            health = maxHealth;
    }

    // When health reaches 0 or less
    void OnKill()
    {
        // Invoke the OnDeath event before anything is destroyed
        OnDeath.Invoke();

        // Set the animator to start the death animation, if applicable
        if (anim)
            anim.SetBool("IsDead", true);

        // Start processing death events
        if (destroyOnDeath)
            Destroy(this.gameObject);
        else if (reloadSceneOnDeath)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        else if (moveToSpawnOnDeath)
        {
            transform.position = _startPos;
            trackedObjects.transform.position = _trackedObjectsStartPos;
            var profile = volume?.profile;
            if (!profile)
                throw new System.NullReferenceException(nameof(UnityEngine.Rendering.VolumeProfile));
            ColorAdjustments CA;
            if (profile.TryGet<ColorAdjustments>(out CA))
            {
                VolumeParameter<float> sat = new VolumeParameter<float>();
                sat.value = -100f;
                CA.saturation.SetValue(sat);
                health = maxHealth;
            }
            player.useMovement = false;
            timer = 0f;
            StartCoroutine(Respawn());
        }
        else
        {
            var rb = GetComponent<Rigidbody>();
            var mr = GetComponent<MeshRenderer>();
            var e = GetComponent<Enemy>();
            var a = GetComponent<UnityEngine.AI.NavMeshAgent>();

            if (rb)
            {
                //rb.isKinematic = false;
                //rb.constraints = RigidbodyConstraints.None;
                Destroy(rb);
            }

            if (mesh)
            {
                Color newColor = new Color(0.8f, 0.5f, 0.5f, 1f);
                mesh.material.color = newColor;
            }

            if (mr)
            {
                Color newColor = new Color(0.8f, 0.5f, 0.5f, 1f);
                mr.material.color = newColor;
            }

            if (e)
                Destroy(e);

            if (a)
                a.enabled = false;

            if (anim)
            {
                anim.enabled = false;
            }

            if (ragdoll != null && ragdoll.Length > 0)
            {
                foreach (var r in ragdoll)
                {
                    r.isKinematic = false;
                }
            }
        }
    }

    // When collision is detected, check if it affects health
    internal void OnCollisionEnter(Collision col)
    {
        if (col.body as Rigidbody == null)
            return;

        if (col.gameObject.tag == "Effect" || col.body.gameObject.tag == "Effect" || (col.gameObject.tag == "Enemy" && gameObject.tag != "Enemy"))
        { // Includes spells and weapons that deal damage, heal, or create some form of effect
            Collider hitCol = col.contacts[0].thisCollider;
            WeakPoint w = hitCol.gameObject.GetComponent<WeakPoint>();

            if ((currCollisions.Contains(col.body.gameObject) && !w) || stabCD > 0)
                return;

            if (!w && weakpointDamageOnly)
                return;

            currCollisions.Add(col.body.gameObject);

            Spell s = col.gameObject.GetComponent<Spell>();
            if (s)
            {
                if (w)
                {
                    w.OnWeakPointHit.Invoke();
                    float tmpDamage = s.damage * w.damageMod;
                    OnDamageReceived(tmpDamage > w.minDamage ? tmpDamage : w.minDamage);
                }
                else
                    OnDamageReceived(s.damage);
            }
            else
            {
                Weapon we = col.body.gameObject.GetComponent<Weapon>();
                Rigidbody velRB = col.body as Rigidbody;
                float vel = 1f;
                if (velRB)
                {
                    vel = velRB.velocity.magnitude;
                }
                vel = vel > 2 ? 2 : vel;
                if (we)
                {
                    if (w)
                    {
                        w.OnWeakPointHit.Invoke();
                        float tmpDamage = we.damage * w.damageMod;
                        OnDamageReceived(tmpDamage > w.minDamage ? tmpDamage : w.minDamage);
                    }
                    else
                        OnDamageReceived(we.damage * vel);
                }
            }
        }
    }

    internal void OnCollisionExit(Collision col)
    {
        if (col.body as Rigidbody == null)
            return;
        if (currCollisions.Contains(col.body.gameObject))
            currCollisions.Remove(col.body.gameObject);
        stabCD = 0.25f;
    }

    internal void OnCollisionStay(Collision col)
    {
        if (col.body as Rigidbody != null && col.body.gameObject.CompareTag("Effect") && !currCollisions.Contains(col.body.gameObject))
        {
            currCollisions.Add(col.body.gameObject);
        }
    }

    internal void ManuallyRemoveCollision(GameObject gm)
    {
        if (currCollisions.Contains(gm))
            currCollisions.Remove(gm);
    }

    public void DebugKill()
    {
        OnDamageReceived(health);
    }

    public IEnumerator Respawn()
    {
        var profile = volume?.profile;
        ColorAdjustments CA;
        profile.TryGet<ColorAdjustments>(out CA);

        VolumeParameter<float> sat = new VolumeParameter<float>();

        if (respawnBarrier)
            respawnBarrier.SetActive(true);
            
        while (timer < respawnTimer)
        {
            float blendVal = ((timer / respawnTimer) * 50);
            timer += Time.deltaTime;
            sat.value = -100f + blendVal;
            CA.saturation.SetValue(sat);
            yield return null;
        }
        player.useMovement = true;
        sat.value = 0f;
        CA.saturation.SetValue(sat);
        if (respawnBarrier)
            respawnBarrier.SetActive(false);
    }
}
