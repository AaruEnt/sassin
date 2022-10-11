using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WristButton : MonoBehaviour
{
    public ConfigurableJoint cj;
    public float deadZone = 0.05f;
    public FollowObjectWithOffset follow;
    public UnityEvent onPressed;

    private Vector3 _startPos;
    private Rigidbody rb;
    private float cd = 0.5f;
    private bool isPressed = false;

    // Start is called before the first frame update
    void Start()
    {
        _startPos = transform.localPosition;
        if (!cj)
            cj = GetComponent<ConfigurableJoint>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (cd > 0)
            cd -= Time.deltaTime;
        if (cd <= 0 && GetValue() <= 0.5f && !isPressed) {
            rb.isKinematic = true;
            follow.followOn = false;
            isPressed = true;
            transform.localPosition = _startPos;
            onPressed.Invoke();
        }
    }

    private float GetValue() {
        //Debug.Log(Vector3.Distance(_startPos, transform.localPosition) / cj.linearLimit.limit);
        var val = Vector3.Distance(_startPos, transform.localPosition) / cj.linearLimit.limit;

        if (Mathf.Abs(val) < deadZone) {
            return (0);
        }
        return (Mathf.Clamp(val, 0f, 2f));
    }

    public void UnlockButton() {
        rb.isKinematic = false;
        follow.followOn = true;
        cd = 1f;
        isPressed = false;
    }
}
