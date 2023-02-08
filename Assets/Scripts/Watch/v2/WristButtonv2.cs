using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

public class WristButtonv2 : MonoBehaviour
{
    public float thrust = 2f;
    [Button]
    private void moveUp() { MoveUp(); }
    public UnityEvent OnPressed;

    private Vector3 _startPos;
    private bool isPressed = true; // watch starts pressed
    private float cd = 0f;


    // Start is called before the first frame update
    void Start()
    {
        _startPos = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(transform.localPosition);
        if (cd > 0)
            cd -= Time.deltaTime;
        if (cd <= 0 && transform.localPosition.y <= _startPos.y + 0.05f && !isPressed)
        {
            OnPressed.Invoke();
            isPressed = true;
        }
    }
    
    public void MoveUp()
    {
        var rb = GetComponent<Rigidbody>();

        rb.isKinematic = false;

        rb.AddRelativeForce(transform.up * thrust, ForceMode.Impulse);
        isPressed = false;
        cd = 0.5f;
    }

    public void SetToStartPos()
    {
        transform.localPosition = _startPos;
    }

    void OnCollisionEnter(Collision col)
    {
        //Debug.Log(col.gameObject.name);
        //if (col.gameObject.tag != "Watch")
       //     return;
       // if (isPressed)
        //    return;
        //if (cd > 0)
        //{
        //    isPressed = true;
        //    OnPressed.Invoke();
        //}
    }

    internal bool GetIsPressed()
    {
        return isPressed;
    }
}
