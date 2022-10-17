using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Distraction : MonoBehaviour
{
    // Radius of sound ping
    public float overlapSphereRadius = 30f;
    // Max suspicion added
    public float addedSuspicion = 1.1f;
    // cooldown before can ping again.
    private float cooldown = 0f;

    // Update is called once per frame
    void Update()
    {
        if (cooldown > 0)
            cooldown -= Time.deltaTime;
        else
            cooldown = 0f;
    }

    void OnCollisionEnter (Collision col) {
        if ((col.gameObject.tag == "Wall" || col.gameObject.tag == "Ground") && cooldown == 0f) {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, overlapSphereRadius);
            foreach (var collider in hitColliders) {
                if (collider.gameObject.tag == "Enemy") {
                    Enemy e = collider.gameObject.GetComponent<Enemy>();
                    if (!e) 
                        Debug.Log(string.Format("No enemy script found on enemy {0}", e.gameObject.name));
                    else
                        e.attractAttention(transform, addedSuspicion); // Needs to calculate based on distance
                } else if (collider.gameObject.tag == "Civillian") {
                    Civillian c = collider.gameObject.GetComponent<Civillian>();
                    if (!c) 
                        Debug.Log(string.Format("No enemy script found on enemy {0}", c.gameObject.name));
                    else
                        c.attractAttention(transform, addedSuspicion); // Needs to calculate based on distance
                }
            }
        }
    }
}
