using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField, Tooltip("health")]
    internal float health = 20f;

    [SerializeField, Tooltip("maximum health")]
    internal float maxHealth = 20f;

    [SerializeField, Tooltip("unarmed damage")]
    internal float damage = 2f;

    // When damage is received
    void OnDamageReceived(float damage) {
        if (damage > 0)
            health -= damage;
        if (health <= 0)
            OnKill();
    }

    // When damage is healed
    void OnHeal(float healAmount) {
        if (healAmount > 0)
            health += healAmount;
        if (health >= maxHealth)
            health = maxHealth;
    }

    // When health reaches 0 or less
    void OnKill() {
        Destroy(this.gameObject);
    }

    // When collision is detected, check if it affects health
    void OnCollisionEnter(Collision col) {
        if (col.gameObject.tag == "Effect") { // Includes spells and weapons that deal damage, heal, or create some form of effect
            Collider hitCol = col.contacts[0].thisCollider;
            //Debug.Log(string.Format("collider a: {0}, collider b: {2}", one.gameObject.name, two.gameObject.name));

            Spell s = col.gameObject.GetComponent<Spell>();
            if (s) {
                WeakPoint w = hitCol.gameObject.GetComponent<WeakPoint>();
                if (w) {
                    OnDamageReceived(s.damage * w.damageMod);
                    Debug.Log("Hit weakpoint");
                }
                else
                    OnDamageReceived(s.damage);
            }
        }
    }
}
