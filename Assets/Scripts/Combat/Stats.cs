using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour
{
    // health
    public float health = 20f;
    // maximum health
    public float maxHealth = 20f;
    // unarmed damage
    public float damage = 2f;

    void OnDamageReceived(float damage) {
        if (damage > 0)
            health -= damage;
        if (health <= 0)
            OnKill();
    }

    void OnHeal(float healAmount) {
        if (healAmount > 0)
            health += healAmount;
        if (health >= maxHealth)
            health = maxHealth;
    }

    void OnKill() {
        Destroy(this.gameObject);
    }

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
