using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Com.Aaru.Sassin {
    public class PlayerUI : MonoBehaviour
    {
        #region Private Fields

        [Tooltip("UI Text to display Player's Name")]
        [SerializeField]
        private UnityEngine.UI.Text playerNameText;

        [Tooltip("UI Slider to display Player's Health")]
        [SerializeField]
        private Slider playerHealthSlider;

        private Stats target;

        private bool parentSet = false;

        #endregion

        #region MonoBehaviour Callbacks


        void Update()
        {
            // Reflect the Player Health
            if (playerHealthSlider != null)
            {
                playerHealthSlider.value = target.health / target.maxHealth;
            }

            if (target == null)
            {
                Destroy(this.gameObject);
                return;
            }
            if (!parentSet)
            {
                Transform tmp = target.gameObject.transform.Find("UICanvas");
                if (tmp)
                {
                    this.transform.SetParent(tmp, false);
                    parentSet = true;
                }
                
            }
        }

        #endregion

        #region Public Methods

        public void SetTarget(Stats _target)
        {
            if (_target == null)
            {
                UnityEngine.Debug.LogError("<Color=Red><a>Missing</a></Color> PlayMakerManager target for PlayerUI.SetTarget.", this);
                return;
            }
            // Cache references for efficiency
            target = _target;
            if (playerNameText != null)
            {
                playerNameText.text = target.photonView.Owner.NickName;
            }
        }

        #endregion

    }
}
