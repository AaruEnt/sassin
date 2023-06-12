using UnityEngine;

namespace Cosmocat.InstantFish
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]    
    public class FishFin : MonoBehaviour
    {
        [ReadOnly] public MeshFilter meshFilter;
        [ReadOnly] public MeshRenderer meshRenderer;
        
        void Awake()
        {
            Refresh();            
        }

        public void Refresh()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
        }
    }

}
