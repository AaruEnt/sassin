using UnityEngine;

namespace Cosmocat.InstantFish
{
    [RequireComponent(typeof(Rigidbody))]
    public class SwimThrough : MonoBehaviour
    {
        public float dragSpeed = 2;
        public float moveSpeed = 2;

        private Vector3 dragOrigin;
        private Rigidbody rigidbody;

        void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();

        }

        void Start()
        {

        }

        void Update()
        {
            if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
            {
                rigidbody.AddForce(transform.forward * moveSpeed);                
            }
            if (Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W))
            {
                rigidbody.AddForce(-transform.forward * moveSpeed);
            }
            if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A))
            {
                rigidbody.AddForce(transform.right * moveSpeed);
            }
            if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
            {
                rigidbody.AddForce(-transform.right * moveSpeed);
            }

            var deg = Vector3.zero;
            if (Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.E))
            {
                deg.z = 20f;
            }
            if (Input.GetKey(KeyCode.E) && !Input.GetKey(KeyCode.Q))
            {
                deg.z = -20f;
            }

            if (Input.GetMouseButtonDown(0))
            {
                dragOrigin = Input.mousePosition;
            }
            if (Input.GetMouseButton(0))
            {
                Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
                deg.x = -pos.y * dragSpeed;
                deg.y =  pos.x * dragSpeed;
            }

            transform.Rotate(
                deg.x * Time.deltaTime,
                deg.y * Time.deltaTime,
                deg.z * Time.deltaTime);

        }
    }

}
