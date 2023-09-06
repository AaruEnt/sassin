using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField, Tooltip("The base damage dealt by this object")]
    public float damage;
    [SerializeField, Tooltip("If enabled, this does not scale damage to speed, and instead always applies max damage")]
    internal bool speedOverride = false;

    public void SetDamage(float dmg) { damage = dmg; }
}
