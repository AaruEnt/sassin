using UnityEngine;
using System.Collections;

public class AnimatedUVs : MonoBehaviour {
		public float speedY = 0.5F;
		public float speedx = 0.0F;
		private float offsety = 0.0F;
		private float offsetx = 0.0F;
		private Renderer rend;
		void Start() {
				rend = GetComponent<Renderer>();
		}
		void Update() {
				offsety += Time.deltaTime * speedY;
				offsetx += Time.deltaTime * speedx;
				rend.material.SetTextureOffset("_MainTex", new Vector2(offsetx,offsety));
		}
}