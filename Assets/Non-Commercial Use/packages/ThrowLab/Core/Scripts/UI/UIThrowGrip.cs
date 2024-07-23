using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace CloudFine.ThrowLab.UI
{
    public class UIThrowGrip : MonoBehaviour {

        public Image[] gripLevels;
        public Image[] hands;
        public Text noHandsWarning;

        private bool[] gripStates = new bool[2];

        public Sprite openHand;
        public Sprite closedHand;

        private List<GrabThresholdModifier> grips;

        public Slider grabBeginSlider;
        public Slider grabEndSlider;

        public bool beginEndEqual = false;

        private float grabBegin
        {
            get
            {
                return PlayerPrefs.GetFloat("GRAB_BEGIN", .5f);
            }
            set
            {
                {
                    PlayerPrefs.SetFloat("GRAB_BEGIN", value);
                }
            }
        }

        private float grabEnd
        {
            get
            {
                return PlayerPrefs.GetFloat("GRAB_END", .5f);
            }
            set
            {
                {
                    PlayerPrefs.SetFloat("GRAB_END", value);
                }
            }
        }

        // Use this for initialization
        void Start() {
            grips = FindObjectsOfType<GrabThresholdModifier>().ToList();
            ShowHidePanel(grips.Count > 0);

            grabEndSlider.GetComponent<UISliderEvents>().onPointerUp.AddListener(SliderReleased);
            grabBeginSlider.GetComponent<UISliderEvents>().onPointerUp.AddListener(SliderReleased);

            grabEndSlider.value = grabEnd;
            grabBeginSlider.value = grabBegin;

            SliderReleased();
        }

        // Update is called once per frame
        void Update() {
            for(int i = 0; i<grips.Count; i++)
            {
                if (gripLevels[i])
                {
                    gripLevels[i].fillAmount = grips[i].GripValue();
                }

                //grips[i].SetGrabThreshold(grabBeginSlider.value);
                //grips[i].SetReleaseThreshold(grabEndSlider.value);

                if (gripStates[i] && grips[i].GripValue() <= grabEndSlider.value)
                {
                    gripStates[i] = false;
                }
                else if (!gripStates[i] && grips[i].GripValue() >= grabBeginSlider.value)
                {
                    gripStates[i] = true;
                }

                hands[i].sprite = gripStates[i] ? closedHand : openHand;
            }

        }

        private void SliderReleased()
        {
            for (int i = 0; i < grips.Count; i++)
            {
                grips[i].SetGrabThreshold(grabBeginSlider.value);
                grips[i].SetReleaseThreshold(grabEndSlider.value);
            }
        }

        public void SetGripBegin(float val)
        {
            grabEndSlider.value = beginEndEqual? val : Mathf.Min(grabEnd, grabBeginSlider.value);
            grabBegin = val;
            grabEnd = grabEndSlider.value;
        }

        public void SetGripEnd(float val)
        {
            grabBeginSlider.value = beginEndEqual ? val : Mathf.Max(grabEndSlider.value, grabBegin);
            grabBegin = grabBeginSlider.value;
            grabEnd = val;
        }

        protected void ShowHidePanel(bool show)
        {
            noHandsWarning.enabled = !show;
            grabEndSlider.interactable = show;
            grabBeginSlider.interactable = show;

            Color c = show ? Color.white : new Color(1, 1, 1, .25f);

            foreach (Graphic select in GetComponentsInChildren<Graphic>(includeInactive: true))
            {
                if (select.GetComponent<UIColorMeTag>() != null)
                {
                    select.color = c;
                }
            }
        }

    }
}