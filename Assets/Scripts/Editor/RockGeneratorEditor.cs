using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace RockTools
{
    [CustomEditor(typeof(RockGenerator))]
    public class RockGeneratorEditor : Editor
    {
        private const float CHANGE_SENSITIVITY = 0.0001f;
        private RockGenerator rockGen;
        private SerializedProperty pRockType;
        private SerializedProperty pMaterial;
        private SerializedProperty pDensity;
        private SerializedProperty pRadius;
        private SerializedProperty pAsymmetry;
        private SerializedProperty pWave;
        private SerializedProperty pDecentralize;
        private SerializedProperty pScaleLocal;
        private SerializedProperty pScaleRandom;
        private SerializedProperty pScaleByDistance;
        private SerializedProperty pTallness;
        private SerializedProperty pFlatness;
        private SerializedProperty pWideness;
        private SerializedProperty pRotation;
        private SerializedProperty pRotationLocal;
        private SerializedProperty pRotationRnd;
        private SerializedProperty prRotationRndRange;

        private ERockType tmpRockType;
        private Object tmpMaterial;
        private int tmpDensity;
        private float tmpRadius;
        private float tmpAsymmetry;
        private float tmpWave;
        private float tmpDecentralize;
        private float tmpScaleLocal;
        private AnimationCurve tmpScaleByDistance;
        private float tmpTallness;
        private float tmpFlatness;
        private float tmpWideness;
        private float tmpRotation;
        private float tmpRotationLocal;
        private float tmpRotationZRnd;

        private bool optimize = true;
        private bool crop = true;
        private bool addCollider;

        private void OnEnable()
        {
            rockGen = target as RockGenerator;

            pRockType = serializedObject.FindProperty("type");
            pMaterial = serializedObject.FindProperty("material");
            pDensity = serializedObject.FindProperty("density");
            pRadius = serializedObject.FindProperty("radius");
            pAsymmetry = serializedObject.FindProperty("asymmetry");
            pWave = serializedObject.FindProperty("wave");
            pDecentralize = serializedObject.FindProperty("decentralize");
            pScaleLocal = serializedObject.FindProperty("scaleLocal");
            pScaleByDistance = serializedObject.FindProperty("scaleByDistance");
            pTallness = serializedObject.FindProperty("tallness");
            pFlatness = serializedObject.FindProperty("flatness");
            pWideness = serializedObject.FindProperty("wideness");
            pRotation = serializedObject.FindProperty("rotation");
            pRotationLocal = serializedObject.FindProperty("rotationLocal");
            pRotationRnd = serializedObject.FindProperty("rotationRnd");

            SceneView.duringSceneGui += DuringSceneGui;

            tmpRockType = (ERockType) pRockType.intValue;
            tmpMaterial = pMaterial.objectReferenceValue;
            tmpDensity = pDensity.intValue;
            tmpRadius = pRadius.floatValue;
            tmpAsymmetry = pAsymmetry.floatValue;
            tmpWave = pWave.floatValue;
            tmpDecentralize = pDecentralize.floatValue;
            tmpScaleLocal = pScaleLocal.floatValue;
            tmpScaleByDistance = pScaleByDistance.animationCurveValue;
            tmpTallness = pTallness.floatValue;
            tmpFlatness = pFlatness.floatValue;
            tmpWideness = pWideness.floatValue;
            tmpRotation = pRotation.floatValue;
            tmpRotationLocal = pRotationLocal.floatValue;
            tmpRotationZRnd = pRotationRnd.floatValue;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= DuringSceneGui;
        }

        public override void OnInspectorGUI()
        {
            if (rockGen == null)
                return;

            if (!rockGen.isActiveAndEnabled)
                EditorGUILayout.HelpBox("Please enable rock generator's game object before editing!", MessageType.Warning);

            EditorGUI.BeginDisabledGroup(!rockGen.isActiveAndEnabled);

            if (GUILayout.Button("Randomize"))
                rockGen.Randomize();

            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Bake", EditorStyles.boldLabel);
            optimize = EditorGUILayout.Toggle("Optimize", optimize);
            crop = EditorGUILayout.Toggle("Crop", crop);
            addCollider = EditorGUILayout.Toggle("Add Collider", addCollider);

            if (GUILayout.Button("Bake"))
            {
                PreBake(Bake);
                GUIUtility.ExitGUI();
                return;
            }

            if (tmpRockType != (ERockType) pRockType.intValue)
                rockGen.UpdateMeshes();

            if (tmpMaterial != pMaterial.objectReferenceValue)
                rockGen.UpdateMaterials();

            else if (tmpDensity != pDensity.intValue)
                rockGen.UpdateDensities();
            else if (Math.Abs(tmpRadius - pRadius.floatValue) > CHANGE_SENSITIVITY)
                rockGen.UpdateRadius();
            else if (Math.Abs(tmpAsymmetry - pAsymmetry.floatValue) > CHANGE_SENSITIVITY)
            {
                rockGen.UpdateScales();
                rockGen.UpdateRotations();
            }
            else if (Math.Abs(tmpWave - pWave.floatValue) > CHANGE_SENSITIVITY)
                rockGen.UpdateScales();
            else if (Math.Abs(tmpDecentralize - pDecentralize.floatValue) > CHANGE_SENSITIVITY)
            {
                rockGen.UpdatePositions();
                rockGen.UpdateRotations();
            }

            else if (Math.Abs(tmpScaleLocal - pScaleLocal.floatValue) > CHANGE_SENSITIVITY)
                rockGen.UpdateScales();
            else if (!tmpScaleByDistance.Equals(pScaleByDistance.animationCurveValue))
                rockGen.UpdateScales();
            else if (Math.Abs(tmpTallness - pTallness.floatValue) > CHANGE_SENSITIVITY)
                rockGen.UpdateScales();
            else if (Math.Abs(tmpFlatness - pFlatness.floatValue) > CHANGE_SENSITIVITY)
                rockGen.UpdateScales();
            else if (Math.Abs(tmpWideness - pWideness.floatValue) > CHANGE_SENSITIVITY)
                rockGen.UpdateScales();

            else if (Math.Abs(tmpRotation - pRotation.floatValue) > CHANGE_SENSITIVITY)
                rockGen.UpdateRotations();
            else if (Math.Abs(tmpRotationLocal - pRotationLocal.floatValue) > CHANGE_SENSITIVITY)
                rockGen.UpdateRotations();
            else if (Math.Abs(tmpRotationZRnd - pRotationRnd.floatValue) > CHANGE_SENSITIVITY)
                rockGen.UpdateRotations();

            tmpRockType = (ERockType) pRockType.intValue;
            tmpMaterial = pMaterial.objectReferenceValue;
            tmpDensity = pDensity.intValue;
            tmpRadius = pRadius.floatValue;
            tmpAsymmetry = pAsymmetry.floatValue;
            tmpWave = pWave.floatValue;
            tmpDecentralize = pDecentralize.floatValue;
            tmpScaleLocal = pScaleLocal.floatValue;
            tmpScaleByDistance = pScaleByDistance.animationCurveValue;
            tmpTallness = pTallness.floatValue;
            tmpFlatness = pFlatness.floatValue;
            tmpWideness = pWideness.floatValue;
            tmpRotation = pRotation.floatValue;
            tmpRotationLocal = pRotationLocal.floatValue;
            tmpRotationZRnd = pRotationRnd.floatValue;

            EditorGUI.EndDisabledGroup();
        }

        private void DuringSceneGui(SceneView obj)
        {
            if (rockGen != null)
                Handles.DrawWireDisc(rockGen.transform.position, Vector3.up, tmpRadius);
        }

        private async void PreBake(Action<string> PreBakeDone)
        {
            var scenePath = SceneManager.GetActiveScene().path;

            // check if scene has a valid path
            if (string.IsNullOrEmpty(scenePath))
            {
                if (EditorUtility.DisplayDialog("The untitled scene needs saving",
                    "You need to save the scene before baking rock.", "Save Scene", "Cancel"))
                    scenePath = EditorUtility.SaveFilePanel("Save Scene", "Assets/", "", "unity");

                scenePath = FileUtil.GetProjectRelativePath(scenePath);

                if (string.IsNullOrEmpty(scenePath))
                {
                    Debug.LogWarning("Scene was not saved, bake canceled.");
                    return;
                }

                var saveOK = EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), scenePath);

                if (!saveOK)
                {
                    Debug.LogWarning("Scene was not saved, bake canceled.");
                    return;
                }

                AssetDatabase.Refresh();
                await Task.Delay(100);
            }

            scenePath = SceneManager.GetActiveScene().path;
            if (string.IsNullOrEmpty(scenePath))
                return;

            var assetPath = $"{Path.ChangeExtension(scenePath, null)}-generated-mesh/baked-rock.asset";
            var assetDir = Path.GetDirectoryName(assetPath);

            if (string.IsNullOrEmpty(assetDir))
                return;

            if (!Directory.Exists(assetDir))
            {
                Directory.CreateDirectory(assetDir);
                AssetDatabase.Refresh();
                await Task.Delay(100);
            }

            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
            PreBakeDone.Invoke(assetPath);
        }

        private void Bake(string path)
        {
            var bakedMeshFilter = new GameObject("Baked-Rock").AddComponent<MeshFilter>();
            var bakedMeshRenderer = bakedMeshFilter.gameObject.AddComponent<MeshRenderer>();
            bakedMeshRenderer.sharedMaterial = rockGen.pRoot.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;
            var parameters = new BakeParameters {addCollider = addCollider, crop = crop, path = path, mergeVerticesThreshold = 0.1f, generateSecondaryUVSet = true, optimize = optimize};
            RockBaker.Bake(rockGen, parameters, bakedMeshFilter);
        }
    }
}