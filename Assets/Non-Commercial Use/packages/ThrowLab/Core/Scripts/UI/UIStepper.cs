using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CloudFine.ThrowLab.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class UIStepper : UIBehaviour
    {
        public Button[] sides
        {
            get
            {
                if (_sides == null || _sides.Length == 0)
                {
                    _sides = GetSides();
                }
                return _sides;
            }
        }
        private Button[] _sides;

        [SerializeField]
        private float _value = 0;
        public float value { get { return _value; } set {
                if(_value != value)
                {
                    onValueChanged.Invoke(value);
                }
                _value = value; }
        }

        [SerializeField]
        private float _minimum = 0;
        public float minimum { get { return _minimum; } set { _minimum = value; } }

        [SerializeField]
        private float _maximum = 100;
        public float maximum { get { return _maximum; } set { _maximum = value; } }

        [SerializeField]
        private float _step = 1;
        public float step { get { return _step; } set { _step = value; } }

        [SerializeField]
        private bool _wrap = false;
        public bool wrap { get { return _wrap; } set { _wrap = value; } }

        [SerializeField]
        private Graphic _separator;
        public Graphic separator { get { return _separator; } set { _separator = value; } }

        private float _separatorWidth = 0;
        private float separatorWidth
        {
            get
            {
                if (_separatorWidth == 0 && separator)
                {
                    _separatorWidth = separator.rectTransform.rect.width;
                    var image = separator.GetComponent<Image>();
                    if (image)
                        _separatorWidth /= image.pixelsPerUnit;
                }
                return _separatorWidth;
            }
        }

        [Serializable]
        public class StepperValueChangedEvent : UnityEvent<float> { }

        // Event delegates triggered on click.
        [SerializeField]
        private StepperValueChangedEvent _onValueChanged = new StepperValueChangedEvent();
        public StepperValueChangedEvent onValueChanged
        {
            get { return _onValueChanged; }
            set { _onValueChanged = value; }
        }

        protected UIStepper()
        { }


        private Button[] GetSides()
        {
            var buttons = GetComponentsInChildren<Button>();
            if (buttons.Length != 2)
            {
                throw new InvalidOperationException("A stepper must have two Button children");
            }

            if (!wrap)
            {
                DisableAtExtremes(buttons);
            }

            return buttons;
        }

        public void StepUp()
        {
            Step(step);
        }

        public void StepDown()
        {
            Step(-step);
        }

        private void Step(float amount)
        {
            value += amount;

            if (wrap)
            {
                if (value > maximum) value = minimum;
                if (value < minimum) value = maximum;
            }
            else
            {
                value = Mathf.Max(minimum, value);
                value = Mathf.Min(maximum, value);

                DisableAtExtremes(sides);
            }
        }

        private void DisableAtExtremes(Button[] sides)
        {
            sides[0].interactable = wrap || value > minimum;
            sides[1].interactable = wrap || value < maximum;
        }

        private void RecreateSprites(Button[] sides)
        {
            for (int i = 0; i < 2; i++)
            {
                if (sides[i].image == null)
                    continue;

                var sprite = sides[i].image.sprite;
                if (sprite.border.x == 0 || sprite.border.z == 0)
                    continue;

                var rect = sprite.rect;
                var border = sprite.border;

                if (i == 0)
                {
                    rect.xMax = border.z;
                    border.z = 0;
                }
                else
                {
                    rect.xMin = border.x;
                    border.x = 0;
                }

                sides[i].image.sprite = Sprite.Create(sprite.texture, rect, sprite.pivot, sprite.pixelsPerUnit, 0, SpriteMeshType.FullRect, border);
            }
        }

        
    }

    
}