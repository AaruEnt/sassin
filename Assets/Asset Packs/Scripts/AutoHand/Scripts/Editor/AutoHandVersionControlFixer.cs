using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class AutoHandVersionControlFixer {
    static AutoHandVersionControlFixer() {
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode) {
#if UNITY_2022_1_OR_NEWER
#else
        var colliderFixers = GameObject.FindObjectsOfType<BoxColliderSerializationFixer>();
        foreach(var fixer in colliderFixers) {
            fixer.ApplyColliderSizesRecursive();
        }
#endif
    }
}