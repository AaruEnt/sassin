using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CloudFine.ThrowLab.UI
{
    public class UITabBar : MonoBehaviour
    {

        public Transform _frontTabRoot;
        public Transform _backTabRoot;

        public Button[] _tabButtons;

        private void Awake()
        {
            SetTab(0);
        }
        public void SetTab(int tab)
        {
            for (int i = 0; i < _tabButtons.Length; i++)
            {
                if (i == tab)
                {
                    _tabButtons[i].transform.SetParent(_frontTabRoot);
                }
                else
                {
                    _tabButtons[i].transform.SetParent(_backTabRoot);
                }
            }
        }
    }
}
