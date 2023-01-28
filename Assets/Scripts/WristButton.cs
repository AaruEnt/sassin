using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    

    // private vars
    private Vector3 _startPos; // the starting local position
    private Rigidbody rb; // the rigidbody of the object
    private float cd = 0.25f; // current press cooldown remaining in seconds
    private float cd2 = 0f; // current release cooldown remaining in seconds
    private bool isPressed = true; // is the button currently pressed
    private bool ignoreTrigger = false; // ignore the trigger

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

    // converts the position on the x axis to a float between 0 and 2 representing how far on its linear limits the joint is
    // 0f - button is fully pressed
    // 2f - button is fully released
    // 1f - button is halfway pressed
    private float GetValue() {
        //Debug.Log(Vector3.Distance(_startPos, transform.localPosition) / cj.linearLimit.limit);
        var val = Vector3.Distance(_startPos, transform.localPosition) / cj.linearLimit.limit;

        if (Mathf.Abs(val) < deadZone) {
            return (0);
        }
        return (Mathf.Clamp(val, 0f, 2f));
    }

    // Releases the button when locked in pressed position
    public void UnlockButton() {
        rb.isKinematic = false;
        follow.followOn = true;
        cd = 0.25f;
        isPressed = false;
    }

    // Prevents the button from immediately locking on being released
    public void BumpCooldown() {
        cd2 = 0.01f;
    }

    // Sets the ignoreTrigger var to true
    public void InfiniteCooldown() {
        ignoreTrigger = true;
    }

    // sets the ignoreTrigger value to false
    public void ResetCooldown() {
        ignoreTrigger = false;
    }
}
