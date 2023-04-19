using UnityEngine;

namespace RockTools
{
    public class RuntimeDemoTest : MonoBehaviour
    {
        private RockGenerator _rockGenerator;

        private void Start()
        {
            _rockGenerator = RockGenerator.GetInstance();
            _rockGenerator.Initialize();
        }

        private void OnGUI()
        {
            if (_rockGenerator == null)
                return;

            var rect = new Rect(20, 20, 100, 20);

            _rockGenerator.pType = (ERockType) GUI.HorizontalSlider(rect, (float) _rockGenerator.pType, 0, 1);
            rect.y += 30;
            _rockGenerator.pDensity = (int) GUI.HorizontalSlider(rect, _rockGenerator.pDensity, 1, 150);
            rect.y += 30;
            _rockGenerator.pRadius = GUI.HorizontalSlider(rect, _rockGenerator.pRadius, 1, 5);
            rect.y += 30;
            _rockGenerator.pAsymmetry = GUI.HorizontalSlider(rect, _rockGenerator.pAsymmetry, -1, 1);
            rect.y += 30;
            _rockGenerator.pWave = GUI.HorizontalSlider(rect, _rockGenerator.pWave, 0, 1);
            rect.y += 30;
            _rockGenerator.pDecentralize = GUI.HorizontalSlider(rect, _rockGenerator.pDecentralize, 0, 1);
            rect.y += 30;
        }
    }
}