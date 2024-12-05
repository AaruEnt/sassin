using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand.Demo
{
    public class ToggleHandProjection : MonoBehaviour
    {
        public void DisableGripProjection()
        {
            var projections = AutoHandExtensions.CanFindObjectsOfType<HandProjector>(true);

            foreach (var projection in projections)
            {
                projection.gameObject.SetActive(false);
                if (projection.useGrabTransition)
                    projection.enabled = false;
            }
        }

        public void EnableGripProjection()
        {
            var projections = AutoHandExtensions.CanFindObjectsOfType<HandProjector>(true);
            foreach (var projection in projections)
            {
                projection.gameObject.SetActive(true);
                if (projection.useGrabTransition)
                    projection.enabled = true;
            }
        }

        public void DisableHighlightProjection()
        {
            var projections = AutoHandExtensions.CanFindObjectsOfType<HandProjector>(true);
            foreach (var projection in projections)
            {
                projection.gameObject.SetActive(false);
                if (!projection.useGrabTransition)
                    projection.enabled = false;
            }
        }

        public void EnableHighlightProjection()
        {
            var projections = AutoHandExtensions.CanFindObjectsOfType<HandProjector>(true);

            foreach (var projection in projections)
            {
                projection.gameObject.SetActive(true);
                if (!projection.useGrabTransition)
                    projection.enabled = true;
            }
        }
    }
}