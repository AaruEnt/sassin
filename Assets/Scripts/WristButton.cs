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
    public float maxPosX;

    private Vector3 _startPos;
    private Rigidbody rb;
    private float cd = 0.25f;
    private float cd2 = 0f;
    private bool isPressed = true;
    private bool ignoreTrigger = false;

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
        //Debug.Log(transform.localPosition.x);
        //Debug.Log(cd2);
        if (cd > 0)
            cd -= Time.deltaTime;
        if (cd <= 0 && GetValue() <= 0.5f && !isPressed) {
            rb.isKinematic = true;
            follow.followOn = false;
            isPressed = true;
            transform.localPosition = _startPos;
            onPressed.Invoke();
        }
        if (cd2 >= 0 && GetValue() >= 1.5f && !ignoreTrigger)
            rb.isKinematic = false;
        if (transform.localPosition.x <= maxPosX && cd2 <= 0) {
            Debug.Log("In If");
            Vector3 tmp = transform.localPosition;
            tmp.x = maxPosX;
            transform.localPosition = tmp;
            rb.isKinematic = true;
        }
        if (cd2 > 0)
            cd2 -= Time.deltaTime;
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
        cd = 0.25f;
        isPressed = false;
    }

    public void BumpCooldown() {
        cd2 = 0.01f;
    }

    public void InfiniteCooldown() {
        ignoreTrigger = true;
    }

    public void ResetCooldown() {
        ignoreTrigger = false;
    }
}
