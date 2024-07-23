using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CloudFine.ThrowLab.UI
{
    [RequireComponent(typeof(Button))]
    public class UIStepperSide : UIBehaviour, IPointerClickHandler, ISubmitHandler
    {
        Button button { get { return GetComponent<Button>(); } }

        UIStepper stepper { get { return GetComponentInParent<UIStepper>(); } }

        bool leftmost { get { return button == stepper.sides[0]; } }

        protected UIStepperSide()
        { }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            Press();
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            Press();
        }

        private void Press()
        {
            if (!button.IsActive() || !button.IsInteractable())
                return;

            if (leftmost)
            {
                stepper.StepDown();
            }
            else
            {
                stepper.StepUp();
            }
        }
    }
}