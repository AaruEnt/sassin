using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using Valve.VR;
using System.Text;

public class Keyboard : MonoBehaviour
{
    private InputField _inputField;
    private TouchScreenKeyboard keyboard;
    private bool isKeyboardShown = false;

    [Button]
    private void showKeyboard() { ShowKeyboard(); }

    void Start()
    {
        _inputField = GetComponent<InputField>();
    }

    void Update()
    {
        if (isKeyboardShown)
        {
            StringBuilder stringBuilder = new StringBuilder(256);
            SteamVR.instance.overlay.GetKeyboardText(stringBuilder, 256);
            string value = stringBuilder.ToString();
            _inputField.text = value;
        }
    }

    public void ShowKeyboard()
    {
        SteamVR.instance.overlay.ShowKeyboard(0, 0, 0, "Description", 12, "", 0);
        isKeyboardShown = true;
    }

    public void HideKeyboard()
    {
        SteamVR.instance.overlay.HideKeyboard();
    }
}
