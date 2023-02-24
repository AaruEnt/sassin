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

    public bool destroyOnDeath = true;
    public bool reloadSceneOnDeath = false;

    public Text debugText;

    private List<GameObject> currCollisions = new List<GameObject>();

    void Update()
    {
        if (debugText)
        {
            debugText.text = string.Format("Health: {0}/{1}", health, maxHealth);
        }
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

            if (mr)
            {
                Color newColor = new Color(0.8f, 0.5f, 0.5f, 1f);
                mr.material.color = newColor;
            }

            if (e)
                Destroy(e);

            if (a)
                a.enabled = false;
        }
    }

    // When collision is detected, check if it affects health
    void OnCollisionEnter(Collision col)
    {
        //Debug.Log("Collision enter " + col.gameObject.name);
        if (currCollisions.Contains(col.gameObject))
            return;

        currCollisions.Add(col.gameObject);
        if (col.gameObject.tag == "Effect" || (col.body && col.body.gameObject.tag == "Effect"))
        { // Includes spells and weapons that deal damage, heal, or create some form of effect
            Collider hitCol = col.contacts[0].thisCollider;
            //Debug.Log(string.Format("collider a: {0}, collider b: {2}", one.gameObject.name, two.gameObject.name));

            Spell s = col.gameObject.GetComponent<Spell>();
            if (s)
            {
                WeakPoint w = hitCol.gameObject.GetComponent<WeakPoint>();
                if (w)
                {
                    OnDamageReceived(s.damage * w.damageMod);
                    Debug.Log("Hit weakpoint");
                }
                else
                    OnDamageReceived(s.damage);
            }
            else
            {
                Weapon we = col.body.gameObject.GetComponent<Weapon>();
                if (we)
                {
                    WeakPoint wp = hitCol.gameObject.GetComponent<WeakPoint>();
                    if (wp)
                    {
                        OnDamageReceived(we.damage * wp.damageMod);
                        Debug.Log("Hit weakpoint");
                    }
                    else
                        OnDamageReceived(we.damage);
                }
            }
        }
    }

    void OnCollisionExit(Collision col)
    {
        //Debug.Log("Collision exit " + col.gameObject.name);
        currCollisions.Remove(col.gameObject);
    }
}
