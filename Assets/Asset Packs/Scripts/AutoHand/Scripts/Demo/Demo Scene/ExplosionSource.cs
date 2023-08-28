using UnityEngine;

namespace Autohand.Demo{
    public class ExplosionSource : MonoBehaviour{
        public float radius = 1;
        public float force = 10;

        public void Explode(bool destroy)
        {
            UnityEngine.Debug.Log("called");
            var hits = Physics.OverlapSphere(transform.position, radius);
            foreach(var hit in hits) {
                UnityEngine.Debug.Log(hit.GetComponent<Collider>().transform.name);
                var rb = hit.GetComponent<Rigidbody>();
                if(rb != null)
                    rb.AddExplosionForce(force, transform.position, radius);
            }
            if(destroy)
                Destroy(gameObject);
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}