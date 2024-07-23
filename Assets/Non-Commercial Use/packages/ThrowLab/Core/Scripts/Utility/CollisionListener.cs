using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CloudFine
{

    public class CollisionListener : MonoBehaviour
    {

        public Action<Collision> CollisionEnter;
        public Action<Collision> CollisionExit;

        private void OnCollisionEnter(Collision collision)
        {
            if (CollisionEnter != null) CollisionEnter.Invoke(collision);
        }
        private void OnCollisionExit(Collision collision)
        {
            if (CollisionExit != null) CollisionExit.Invoke(collision);
        }
    }
}