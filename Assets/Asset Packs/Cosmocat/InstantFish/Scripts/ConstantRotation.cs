using UnityEngine;

namespace Cosmocat.InstantFish
{
    public class ConstantRotation : MonoBehaviour
    {
        public Vector3 degreesPerSecond = Vector3.zero;
        public float dragSpeed = 2;
        private Vector3 dragOrigin;

        private void Start()
        {
        
        }

        void Update()
        {
            var deg = degreesPerSecond;
            if (Input.GetMouseButtonDown(0))
            {
                dragOrigin = Input.mousePosition;
            }
            if (Input.GetMouseButton(0))
            {
                Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
                deg = new Vector3(0f, -pos.x * dragSpeed, 0f);
            }

            transform.Rotate(
                deg.x * Time.deltaTime,
                deg.y * Time.deltaTime,
                deg.z * Time.deltaTime);
        }


    }

}
