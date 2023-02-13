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
    public GameObject hint;
    public Rigidbody rb;

    private Vector3 _startPos;
    [SerializeField]
    private bool isPressed = true; // watch starts pressed
    [SerializeField]
    private bool isReleased = false;
    private float cd = 0f; // moveup cd
    private float cd2 = 0f; // movedown cd


    // Start is called before the first frame update
    void Start()
    {
        _startPos = transform.localPosition;
        OnPressed.AddListener(Ping);
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(transform.localPosition);
        if (cd > 0)
            cd -= Time.deltaTime;
        if (cd2 > 0)
            cd2 -= Time.deltaTime;
        if (isReleased && transform.localPosition.y <= _startPos.y + 0.05f && !isPressed)
        {
            OnPressed.Invoke();
            isPressed = true;
            isReleased = false;
            cd2 = 0.5f;
            SetToStartPos();
        }
    }
    
    public void MoveUp()
    {
        Vector3 dir = new Vector3(0, 0, 1);
        Debug.Log("Moving");
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = false;

        rb.AddRelativeForce(dir * thrust, ForceMode.Impulse);
        isPressed = false;
        cd = 0.5f;
    }

    public void SetToStartPos()
    {
        //Debug.Log("Called");
        if (isReleased)
            return;
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
        if (cd2 > 0f)
            return false;
        if (cd > 0f)
            return false;
        return isPressed;
    }

    private void Ping()
    {
        //Debug.Log("Pressed");
    }

    public void Released()
    {
        isReleased = true;
    }
}
