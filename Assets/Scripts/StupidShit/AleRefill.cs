using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AleRefill : MonoBehaviour
{
    public float localYMin = 0f;
    public float localYMax = 0f;
    public float speed = 0.1f;
    public GameObject aleCollider;
    bool particleCollided = false;

    // Update is called once per frame
    void Update()
    {
        if (particleCollided && aleCollider.transform.localPosition.z <= localYMax)
        {
            Vector3 newPos = aleCollider.transform.localPosition;
            newPos.z += Time.deltaTime * speed;
            aleCollider.transform.localPosition = newPos;
        }
        particleCollided = false;
    }

    void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Ale"))
        {
            particleCollided = true;
        }
    }
}
