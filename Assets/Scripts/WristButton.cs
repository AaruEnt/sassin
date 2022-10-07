using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WristButton : MonoBehaviour
{
    public ConfigurableJoint cj;
    public float deadZone = 0.05f;
    public FollowObjectWithOffset follow;

    private Vector3 _startPos;
    private Rigidbody rb;
    private float cd = 0.5f;

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
        if (cd <= 0 && GetValue() <= 0.5f) {
            rb.isKinematic = true;
            follow.followOn = false;
            transform.localPosition = _startPos;
        }
    }

    private float GetValue() {
        Debug.Log(Vector3.Distance(_startPos, transform.localPosition) / cj.linearLimit.limit);
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
    }
}
