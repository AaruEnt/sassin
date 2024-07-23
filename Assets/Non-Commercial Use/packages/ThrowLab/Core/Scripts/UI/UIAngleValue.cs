using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CloudFine.ThrowLab.UI
{
    [RequireComponent(typeof(Image))]
    public class UIAngleValue : MonoBehaviour
    {

        private Image image;

        private void Awake()
        {
            image = GetComponent<Image>();
        }
        public void SetRange(float range)
        {
            image.fillAmount = range / 180f;
            image.transform.rotation = Quaternion.Euler(0, 0, range + 180);
        }
    }
}
