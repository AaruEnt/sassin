using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

public class WristButtonv2 : MonoBehaviour
{
    [SerializeField, Tooltip("The thrust used to push up the button")]
    private float thrust = 2f;
    [Button]
    private void moveUp() { MoveUp(); }
    public UnityEvent OnPressed;
    public GameObject hint;
    [SerializeField, Tooltip("The rigidbody of the button")]
    internal Rigidbody rb;

    private Vector3 _startPos;
    [SerializeField]
    private bool isPressed = true; // watch starts pressed
    [SerializeField]
    internal bool isReleased = false;
    private float cd = 0f; // moveup cd
    private float cd2 = 0f; // movedown cd


    // Start is called before the first frame update
    void Start()
    {
        _startPos = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
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
    
    // Moves the button up
    public void MoveUp()
    {
        Vector3 dir = new Vector3(0, 0, 1);
        Debug.Log("Moving");
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = false;
        StartCoroutine(IgnoreAllCollision(0.25f));

        rb.AddRelativeForce(dir * thrust, ForceMode.Impulse);
        isPressed = false;
        cd = 0.5f;
    }

    // locks the button to the local start position (down/pressed)
    public void SetToStartPos()
    {
        if (isReleased)
            return;
        transform.localPosition = _startPos;
    }

    // returns if the button is currently pressed
    internal bool GetIsPressed()
    {
        if (cd2 > 0f)
            return false;
        if (cd > 0f)
            return false;
        return isPressed;
    }

    // releases the button from the press lock
    public void Released()
    {
        isReleased = true;
    }

    private IEnumerator IgnoreAllCollision(float iTime)
    {
        rb.detectCollisions = false;
        yield return new WaitForSeconds(iTime);
        rb.detectCollisions = true;
    }
}
