using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class WristButtonv2 : MonoBehaviour
{
    public float thrust = 2f;
    [Button]
    private void moveUp() { MoveUp(); }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void MoveUp()
    {
        var rb = GetComponent<Rigidbody>();

        rb.AddRelativeForce(transform.up * thrust, ForceMode.Impulse);
    }
}
