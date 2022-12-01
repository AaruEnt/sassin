using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Autohand;

// Controls the climbing functionality for the player
// Placed on the topmost object for the XR Rig
public class ClimbManager : MonoBehaviour
{
    // Global Vars
    public static List<Autohand.Hand> climbing = new List<Autohand.Hand>(); // Global variable used for keeping track of which hand is anchored


    [Header("References")]
    [SerializeField, Tooltip("")]
    private Rigidbody rb;
    [SerializeField]
    private AutoHandPlayer character;

    public float climbingDrag = 1f;
    public float climbSpeed = 1f;
    public Vector3 climbMultiplier = Vector3.one;
    public Text txt;
    
    
    // private vars
    private int startLayer; // Layer the player is on before climbing
    private bool movePrevEnabled = true; // Was the move var enabled before climbing
    private bool isFinishedClimbing = true; // Is the player finished climbing

    private Vector3 climbAxis;

    // Set charactercontroller reference and grab starting layer
    void Start()
    {
        //startLayer = gameObject.layer;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        string strbuilder = "";
        if (!climbing.Any() && isFinishedClimbing) {
            // Debug.Log(string.Format("enabled? {0}", movePrevEnabled));
            //movePrevEnabled = character.useMovement;
        }
        if (climbing.Any()) {
            // disable gravity and player stick movement while climbing
            if (isFinishedClimbing) {
                isFinishedClimbing = false;
                rb.useGravity = false;
                //character.useGrounding = false;
                character.tempDisableGrounding = true;
            }
            character.useMovement = false;
            strbuilder += "In Climb\n";
            Climb();
            //gameObject.layer = 12; // No collision layer
        } else {
            // "move.enabled = movePrevEnabled" in case movement was already disabled
            if (!isFinishedClimbing) {
                isFinishedClimbing = true;
                //gameObject.layer = startLayer;
                //character.useMovement = movePrevEnabled;
                character.useMovement = true;
                //character.useGrounding = true;
                character.tempDisableGrounding = false;
                rb.useGravity = true;
                climbAxis = Vector3.zero;
            }
        }

        if(climbAxis != Vector3.zero) {
                rb.velocity = Vector3.MoveTowards(rb.velocity, climbAxis, climbSpeed * Time.fixedDeltaTime);
                rb.velocity *= 1 - climbingDrag * Time.fixedDeltaTime;
                rb.velocity *= -1;
        }
        txt.text = strbuilder + string.Format("climb axis: {0}\nRB Constraints: {1}\nRB Velocity: {2}\nGravity: {3}\nMovement: {4}\nGrounding: {5}\n", climbAxis.ToString(), rb.constraints, rb.velocity, rb.useGravity, character.useMovement, character.tempDisableGrounding);
    }

    public void Climb() {
        Hand handInUse = climbing.Last(); // Returns the most recent climber added to the list
        if (!handInUse) {
            return;
        }
        climbAxis = Vector3.zero;
        var offset = Vector3.Scale(handInUse.body.position - handInUse.moveTo.position, Vector3.one);
        offset = Vector3.Scale(offset, climbMultiplier);
        climbAxis += offset;
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
