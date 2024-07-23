using UnityEngine;
using UnityEngine.UI;

namespace CloudFine.ThrowLab.UI
{
    [RequireComponent(typeof(Text))]
    public class UIValueText : MonoBehaviour
    {
        private Text _text;
        public string _preDecorator = "";
        public string _toStringPattern = "0.0";
        public string _postDecorator = "";

        private void Awake()
        {
            _text = GetComponent<Text>();
        }

        public void SetValue(float value)
        {
            _text.text = _preDecorator + value.ToString(_toStringPattern) + _postDecorator;
        }
    }
}