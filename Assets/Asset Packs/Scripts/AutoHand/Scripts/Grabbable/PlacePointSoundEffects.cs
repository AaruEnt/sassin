using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand {
    public class PlacePointSoundEffects : MonoBehaviour {
        public PlacePoint placePoint;
        public AudioSource audioSource;
        public AudioClip highlightSound;
        public AudioClip unhighlightSound;
        public AudioClip placeSound;
        public AudioClip removeSound;

        float startDelay = 1f;
        Coroutine waitToActivateRountine;

        void OnEnable() {
            if(audioSource == null)
                audioSource = GetComponent<AudioSource>();
            waitToActivateRountine = StartCoroutine(WaitToActivate());
        }

        void OnDisable() {
            if(waitToActivateRountine != null) {
                StopCoroutine(waitToActivateRountine);
            }
            else {
                placePoint.OnHighlight.RemoveListener(OnHighlight);
                placePoint.OnStopHighlight.RemoveListener(OnUnhighlight);
                placePoint.OnPlace.RemoveListener(OnPlace);
                placePoint.OnRemove.RemoveListener(OnRemove);
            }
        }

        IEnumerator WaitToActivate() {
            yield return new WaitForSeconds(startDelay);
            placePoint.OnHighlight.AddListener(OnHighlight);
            placePoint.OnStopHighlight.AddListener(OnUnhighlight);
            placePoint.OnPlace.AddListener(OnPlace);
            placePoint.OnRemove.AddListener(OnRemove);
        }

        void OnHighlight(PlacePoint placePoint, Grabbable grabbable) {
            if(highlightSound != null && audioSource.enabled && audioSource.gameObject.activeInHierarchy && placePoint.enabled)
                audioSource.PlayOneShot(highlightSound);
        }


        void OnUnhighlight(PlacePoint placePoint, Grabbable grabbable) {
            if(unhighlightSound != null && audioSource.enabled && audioSource.gameObject.activeInHierarchy && placePoint.enabled)
                audioSource.PlayOneShot(unhighlightSound);
        }

        void OnPlace(PlacePoint placePoint, Grabbable grabbable) {
            if(placeSound != null && audioSource.enabled && audioSource.gameObject.activeInHierarchy && placePoint.enabled)
                audioSource.PlayOneShot(placeSound);
        }

        void OnRemove(PlacePoint placePoint, Grabbable grabbable) {
            if(removeSound != null && audioSource.enabled && audioSource.gameObject.activeInHierarchy && placePoint.enabled)
                audioSource.PlayOneShot(removeSound);
        }
    }
}
