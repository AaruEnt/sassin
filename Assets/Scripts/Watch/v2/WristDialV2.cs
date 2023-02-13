using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand;

public class WristDialV2 : PhysicsGadgetHingeAngleReader
{
    public int numberOfOptions;
    public int dialPosToTimezone;
    public Collider grabCol;
    public Collider baseCol;

    [SerializeField, Tooltip("Reference to the time zone manager in the scene")]
    private TimeZoneManager manager;

    public List<GameObject> ignoreCollision;
    private List<Collider> ignoreColliders;
    private HingeJoint hJoint;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        foreach (var obj in ignoreCollision)
        {
            foreach (var col in obj.transform.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(grabCol, col);
                Physics.IgnoreCollision(baseCol, col);
            }
        }
        hJoint = GetComponent<HingeJoint>();
    }

    // Update is called once per frame
    void Update()
    {
        dialPosToTimezone = ConvertValueToPos();
    }

    int ConvertValueToPos()
    {
        //Debug.Log(GetValueReal());
        float val = GetValueReal();
        float num = 1f / numberOfOptions;

        int res = 0;
        float tmp = 0f;

        

        while (tmp <= val && tmp <= 100f)
        {
            tmp += num;
            res += 1;
        }

        res = res > numberOfOptions ? numberOfOptions : res <= 0 ? 1 : res;

        //Debug.Log(string.Format("{0}, {1}", val, res));

        return (res);
    }

    public float GetValueReal()
    {
        Vector3 angles = transform.localRotation.eulerAngles;
        float maxAngle = hJoint.limits.max - hJoint.limits.min;
        //Debug.Log(string.Format("{0}, {1}", angles, maxAngle));
        return angles.y / maxAngle;
    }

    public void OnSetTimeZone()
    {
        manager.SetTimeZone(dialPosToTimezone);
    }
}
