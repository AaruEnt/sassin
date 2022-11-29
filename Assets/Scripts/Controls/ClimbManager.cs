using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Autohand;

// Controls the climbing functionality for the player
// Placed on the topmost object for the XR Rig
public class ClimbManager : MonoBehaviour
{
    // Global Vars
    public static List<Climber> climbing = new List<Climber>(); // Global variable used for keeping track of which hand is anchored


    [Header("References")]
    [SerializeField, Tooltip("")]
    private Rigidbody rb;
    [SerializeField]
    private AutoHandPlayer character;
    
    
    // private vars
    private int startLayer; // Layer the player is on before climbing
    private bool movePrevEnabled = true; // Was the move var enabled before climbing
    private bool isFinishedClimbing = true; // Is the player finished climbing

    // Set charactercontroller reference and grab starting layer
    void Start()
    {
        //startLayer = gameObject.layer;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!climbing.Any() && isFinishedClimbing) {
            // Debug.Log(string.Format("enabled? {0}", movePrevEnabled));
            //movePrevEnabled = move.enabled;
        }
        if (climbing.Any()) {
            // disable gravity and player stick movement while climbing
            isFinishedClimbing = false;
            //move.enabled = false;
            Climb();
            gameObject.layer = 12; // No collision layer
        } else {
            // "move.enabled = movePrevEnabled" in case movement was already disabled
            if (!isFinishedClimbing) {
                isFinishedClimbing = true;
                //gameObject.layer = startLayer;
                //move.enabled = movePrevEnabled;
            }
        }
    }

    public void Climb() {
        Climber climberInUse = climbing.Last(); // Returns the most recent climber added to the list
        if (!climberInUse)
            return;
        // Way less intimidating than it looks
        // InputDevices.GetDeviceAtXRNode(                                              = Get the InputDevice object using an XRNode
        // climbingHand.gameObject.GetComponent<PhysicsHandController>().controllerNode = Get the XRNode stored in the PhysicsHandController script
        // .TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 velocity);      = Get the velocity associated with the InputDevice and save it to variable velocity
        //InputDevices.GetDeviceAtXRNode(climbingHand.gameObject.GetComponent<HandInfo>().controllerNode).TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 velocity);

        // Inverted velocity to anchor player body to hand holding climb point
        // rotation corrects for rotation, time corrects for time
        //character.Move(transform.rotation * -velocity * Time.deltaTime);
    }
}
