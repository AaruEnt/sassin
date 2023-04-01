using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand;

public class AddSpringJoint : MonoBehaviour
{
    [SerializeField, Tooltip("The force to add to the player on letting go")]
    private float releaseThrust = 3f;
    private SpringJoint joint;

    private List<Hand> grabbingHands = new List<Hand>();
    private Transform playerFacingTransform;
    private Transform parentObj;
    private Rigidbody targetRB;
    private AutoHandPlayer player;
    private MomentumController momentum;

    private float savedMomentum = 0f;

    // Creates a spring joint between this object and the player
    public void CreateSpringJoint(Hand h, Grabbable g)
    {
        grabbingHands.Add(h);
        if (joint != null)
            return;
        playerFacingTransform = h.transform.parent;
        parentObj = playerFacingTransform.parent;

        foreach (Transform t in parentObj.transform)
        {
            targetRB = t.GetComponent<Rigidbody>();
            if (targetRB)
                break;
        }

        player = parentObj.GetComponentInChildren<AutoHandPlayer>();
        momentum = parentObj.GetComponentInChildren<MomentumController>();


        //player.stickMovementDisabled = true;
        savedMomentum = momentum.GetMomentum();

        joint = targetRB.gameObject.AddComponent<SpringJoint>();
        joint.connectedBody = GetComponent<Rigidbody>();
        joint.massScale = 4.5f;
        joint.spring = 200f;
        joint.damper = 100f;
        joint.maxDistance = 0.2f;
    }

    // Removes the spring joint if all hands are removed from this object
    public void DeleteJoint(Hand h, Grabbable g)
    {
        if (grabbingHands.Contains(h))
            grabbingHands.Remove(h);
        if (grabbingHands.Count > 0)
            return;
        //player.stickMovementDisabled = false;
        momentum.SetMomentum(savedMomentum);
        Destroy(joint);
    }

    // Sends the player flying away from this object on the joint being deleted
    public void AddForce()
    {
        if (grabbingHands.Count > 0)
            return;
        targetRB.AddForce((playerFacingTransform.transform.forward * releaseThrust) + (playerFacingTransform.transform.up * (releaseThrust / 2)), ForceMode.Impulse);
    }
}
