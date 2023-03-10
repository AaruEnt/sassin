using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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

    public bool destroyOnDeath = true;
    public bool reloadSceneOnDeath = false;

    public Text debugText;

    private List<GameObject> currCollisions = new List<GameObject>();

    public SkinnedMeshRenderer mesh;

    public Animator anim;

    public Rigidbody[] ragdoll;

    private float stabCD = 0f;

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
        OnDeath.Invoke();
        if (anim)
            anim.SetBool("IsDead", true);
        if (destroyOnDeath)
            Destroy(this.gameObject);
        else if (reloadSceneOnDeath)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        else
        {
            var rb = GetComponent<Rigidbody>();
            var mr = GetComponent<MeshRenderer>();
            var e = GetComponent<Enemy>();
            var a = GetComponent<UnityEngine.AI.NavMeshAgent>();

            if (rb)
            {
                rb.isKinematic = false;
                rb.constraints = RigidbodyConstraints.None;
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
        //Debug.Log("Collision enter " + col.gameObject.name);

        //if (col.body)
        //    Debug.Log(col.body.gameObject.name);

        if (col.gameObject.tag == "Effect" || col.body.gameObject.tag == "Effect" || (col.gameObject.tag == "Enemy" && gameObject.tag != "Enemy"))
        { // Includes spells and weapons that deal damage, heal, or create some form of effect
            Collider hitCol = col.contacts[0].thisCollider;
            WeakPoint w = hitCol.gameObject.GetComponent<WeakPoint>();
            //Debug.Log(string.Format("collider a: {0}, collider b: {2}", one.gameObject.name, two.gameObject.name));

            if ((currCollisions.Contains(col.body.gameObject) && !w) || stabCD > 0)
                return;

            currCollisions.Add(col.body.gameObject);

            Spell s = col.gameObject.GetComponent<Spell>();
            if (s)
            {
                if (w)
                {
                    w.OnWeakPointHit.Invoke();
                    OnDamageReceived(s.damage * w.damageMod);
                    Debug.Log("Hit weakpoint");
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
                    //Debug.Log(vel);
                }
                vel = vel > 2 ? 2 : vel;
                if (we)
                {
                    if (w)
                    {
                        w.OnWeakPointHit.Invoke();
                        OnDamageReceived(we.damage * w.damageMod * vel);
                        Debug.Log("Hit weakpoint");
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
            Debug.Log("Collision exit " + col.gameObject.name);
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
}
