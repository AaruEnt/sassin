using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
#if !UNITY_ANDROID
using Valve.VR;
#endif
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
#if !UNITY_ANDROID
            SteamVR.instance.overlay.GetKeyboardText(stringBuilder, 256);
#else
            if (keyboard != null)
                stringBuilder.Append(keyboard.text);
#endif
            string value = stringBuilder.ToString();
            _inputField.text = value;
        }
    }

    public void ShowKeyboard()
    {
#if !UNITY_ANDROID
        SteamVR.instance.overlay.ShowKeyboard(0, 0, 0, "Description", 12, "", 0);
#else
        keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
        TouchScreenKeyboard.hideInput = true;
#endif
        isKeyboardShown = true;
    }

    public void HideKeyboard()
    {
#if !UNITY_ANDROID
        SteamVR.instance.overlay.HideKeyboard();
#endif
    }
}
