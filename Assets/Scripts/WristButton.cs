using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

public class WristButton : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField, Tooltip("Button deadzone")]
    private float deadZone = 0.05f;

    [SerializeField, Tooltip("The max x value")]
    private float maxPosX;


    [Header("References")]
    [SerializeField, Tooltip("The configurable joint allowing the button to be pressed")]
    private ConfigurableJoint cj;

    [SerializeField, Tooltip("The object that should be followed")]
    private FollowObjectWithOffset follow;


    [Header("Events")]
    [SerializeField, Tooltip("Activates on the button being pressed")]
    private UnityEvent onPressed;
    [Button]
    private void unlockButton() { UnlockButton(); }

    // private vars
    [SerializeField]
    private Vector3 _startPos; // the starting local position
    private Rigidbody rb; // the rigidbody of the object
    private float cd = 0.25f; // current press cooldown remaining in seconds
    private float cd2 = 0f; // current release cooldown remaining in seconds
    [SerializeField]
    private bool isPressed = true; // is the button currently pressed
    private bool ignoreTrigger = false; // ignore the trigger
    private bool lockInPressed = true;


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
        //Debug.Log(transform.localPosition.z);
        //Debug.Log(cd2);
        //Debug.Log(GetValue());
        //Vector3 eulerRotation = transform.localRotation.eulerAngles;
        //transform.localRotation = Quaternion.Euler(0, 0, eulerRotation.z);
        transform.localPosition = new Vector3(0f, 0f, transform.localPosition.z);
        if (lockInPressed)
        {
            transform.localPosition = _startPos;
        }
        if (cd > 0)
            cd -= Time.deltaTime;
        if (cd <= 0 && GetValue() <= 0.25f && !isPressed)
        {
            rb.isKinematic = true;
            isPressed = true;
            transform.localPosition = _startPos;
            onPressed.Invoke();
        }
        if (cd2 > 0 && GetValue() >= 1.5f && !ignoreTrigger)
            rb.isKinematic = false;
        if (!lockInPressed && transform.localPosition.z >= maxPosX && cd2 <= 0)
        {
            Debug.Log("In If");
            Vector3 tmp = transform.localPosition;
            tmp.z = maxPosX;
            transform.localPosition = tmp;
            rb.isKinematic = true;
            isPressed = false;
        }
        if (cd2 > 0)
            cd2 -= Time.deltaTime;
    }

    // converts the position on the x axis to a float between 0 and 2 representing how far on its linear limits the joint is
    // 0f - button is fully pressed
    // 1f - button is fully released
    private float GetValue()
    {
        //Debug.Log(Vector3.Distance(_startPos, transform.localPosition) / cj.linearLimit.limit);
        //var val = Vector3.Distance(_startPos, transform.localPosition) / cj.linearLimit.limit;
        Vector3 maxPos = new Vector3(_startPos.x, _startPos.y, maxPosX);
        var maxDist = Vector3.Distance(_startPos, maxPos);
        var currDist = Vector3.Distance(_startPos, transform.localPosition);
        var val = currDist / maxDist;

        //Debug.Log(string.Format("{0}, {1}, {2}", maxDist, currDist, val));

        if (Mathf.Abs(val) < deadZone)
        {
            return (0);
        }
        return (Mathf.Clamp(val, 0f, 1f));
    }

    // Releases the button when locked in pressed position
    public void UnlockButton()
    {
        lockInPressed = false;
        rb.isKinematic = false;
        cd = 0.25f;
        isPressed = false;
        transform.localPosition = new Vector3(0f, 0f, maxPosX);
        Debug.Log("Unlock Button");
    }

    // Prevents the button from immediately locking on being released
    public void BumpCooldown()
    {
        cd2 = 0.05f;
        if (!isPressed && !ignoreTrigger)
            rb.isKinematic = false;
    }

    // Sets the ignoreTrigger var to true
    public void InfiniteCooldown()
    {
        ignoreTrigger = true;
    }

    // sets the ignoreTrigger value to false
    public void ResetCooldown()
    {
        ignoreTrigger = false;
    }
}
