using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CloudFine.ThrowLab.UI
{
    public class UIThrowTracker : MonoBehaviour
    {
        public Text _distanceText;
        public Text _angleText;
        public Text _speedText;

        public Button showHideButton;
        public Button clearButton;

        public Image visibility;

        public Sprite visibleSprite;
        public Sprite invisibleSprite;

        public void OnEnable()
        {
            transform.SetAsFirstSibling();
        }

        public void SetAngle(float angle)
        {
            _angleText.text = angle.ToString("0") + "°";
        }

        public void SetSpeed(float speed)
        {
            _speedText.text = speed.ToString("0.0") + " m/s";
        }

        public void UpdateDistance(float distance)
        {
            _distanceText.text = distance.ToString("0.0") + " m";
        }

        public void RefreshVisibilityButton(bool showing)
        {
            visibility.sprite = showing ? visibleSprite : invisibleSprite;
        }
    }
}