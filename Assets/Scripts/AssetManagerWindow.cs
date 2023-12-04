using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace AbdyManagement
{
    public class AssetManagerWindow : EditorWindow
    {
        Rect headerSection, layerMainSection, layerOptionsSection, categorySection, assetSection;
        Texture2D headerSectionTexture, layerMainSectionTexture, layerOptionsSectionTexture, categorySectionTexture, assetSectionTexture;
        Color headerSectionColor = new(45f / 255f, 45f / 255f, 45f / 255f, 1f);
        Color layerMainSectionColor = new(60f / 255f, 60f / 255f, 60f / 255f, 1f);
        Color layerOptionsSectionColor = new(70f / 255f, 70f / 255f, 70f / 255f, 1f);
        Color categorySectionColor = new(75f / 255f, 75f / 255f, 75f / 255f, 1f);
        Color assetSectionColor = new(90f / 255f, 90f / 255f, 90f / 255f, 1f);

        GUISkin skin;

        List<AssetLayerSO> assetDataSOList;

        SerializedObject selectedAssetLayer;
        List<SerializedProperty> assetCategoriesOfSelectedLayer;
        SerializedProperty sceneLayerMaskOfSelectedLayer;

        SerializedProperty selectedAssetGroup;
        List<SerializedProperty> AssetsOfSelectedCategory;

        SerializedProperty selectedAsset;

        Vector2 scrollPosition;

        private int selectedLayerIndex = -1;
        private bool isRenamingLayer = false;
        private string layerRenamingText = "";

        [MenuItem("Window/Abdy Asset Manager")]
        private static void OpenWindow()
        {
            AssetManagerWindow window = GetWindow<AssetManagerWindow>();
            window.minSize = new Vector2(600, 300);
            window.Show();
        }

        void OnEnable()
        {
            InitData();
            InitTextures();

            skin = Resources.Load<GUISkin>("GuiStyles/LevelDesignerSkin");
        }

        void InitData()
        {
            assetDataSOList = Resources.LoadAll<AssetLayerSO>("Scriptables/").ToList();
            if (assetDataSOList.Count > 0 ) selectedAssetLayer = new(assetDataSOList[0]);
            InitializeSelectedData();
        }

        void InitializeSelectedData()
        {
            if (selectedAssetLayer != null)
            {
                sceneLayerMaskOfSelectedLayer = selectedAssetLayer.FindProperty("sceneLayerMask");
                assetCategoriesOfSelectedLayer = new List<SerializedProperty>
                {
                    selectedAssetLayer.FindProperty("prefabs"),
                    selectedAssetLayer.FindProperty("audioClips"),
                    selectedAssetLayer.FindProperty("scriptableObjects"),
                    selectedAssetLayer.FindProperty("materials"),
                    selectedAssetLayer.FindProperty("sprites"),
                    selectedAssetLayer.FindProperty("fonts"),
                    selectedAssetLayer.FindProperty("animations"),
                    selectedAssetLayer.FindProperty("textures"),
                    selectedAssetLayer.FindProperty("shaders"),
                    selectedAssetLayer.FindProperty("meshes"),
                    selectedAssetLayer.FindProperty("scenes"),
                };

                selectedAsset = selectedAssetGroup = null;
                AssetsOfSelectedCategory = new();
                foreach (SerializedProperty property in assetCategoriesOfSelectedLayer)
                {
                    if (property.arraySize != 0)
                    {
                        selectedAssetGroup = property.GetArrayElementAtIndex(0);

                        for (int i = 0; i < selectedAssetGroup.FindPropertyRelative("data").arraySize; i++)
                        {
                            SerializedProperty _data = selectedAssetGroup.FindPropertyRelative("data").GetArrayElementAtIndex(i);
                            AssetsOfSelectedCategory.Add(_data);
                        }

                        selectedAsset = AssetsOfSelectedCategory?[0];
                        break;
                    }
                }
            }
        }

        void ChangeSelectedLayer(SerializedObject layer)
        {
            selectedAssetLayer = layer;
            InitializeSelectedData();
        }

        void InitTextures()
        {
            headerSectionTexture = new(1, 1);
            headerSectionTexture.SetPixel(0, 0, headerSectionColor);
            headerSectionTexture.Apply();

            layerMainSectionTexture = new(1, 1);
            layerMainSectionTexture.SetPixel(0, 0, layerMainSectionColor);
            layerMainSectionTexture.Apply();

            layerOptionsSectionTexture = new(1, 1);
            layerOptionsSectionTexture.SetPixel(0, 0, layerOptionsSectionColor);
            layerOptionsSectionTexture.Apply();

            categorySectionTexture = new(1, 1);
            categorySectionTexture.SetPixel(0, 0, categorySectionColor);
            categorySectionTexture.Apply();

            assetSectionTexture = new(1, 1);
            assetSectionTexture.SetPixel(0, 0, assetSectionColor);
            assetSectionTexture.Apply();
        }

        void OnGUI()
        {
            DrawLayouts();

            DrawHeaderSection();
            DrawLayerMainSection();
            DrawLayerOptionsSection();
            DrawCategorySection();
            DrawAssetSection();
        }

        void DrawLayouts()
        {
            headerSection.x = 0f;
            headerSection.y = 0f;
            headerSection.width = position.width;
            headerSection.height = 40;

            layerMainSection.x = 0f;
            layerMainSection.y = headerSection.height;
            layerMainSection.width = position.width / 5;
            layerMainSection.height = (position.height - headerSection.height) / 2;

            layerOptionsSection.x = 0f;
            layerOptionsSection.y = layerMainSection.y + layerMainSection.height;
            layerOptionsSection.width = layerMainSection.width;
            layerOptionsSection.height = position.height - (layerMainSection.y + layerMainSection.height);

            categorySection.x = layerMainSection.x + layerMainSection.width;
            categorySection.y = headerSection.height;
            categorySection.width = layerMainSection.width * 1.62f;
            categorySection.height = position.height;

            assetSection.x = categorySection.x + categorySection.width;
            assetSection.y = headerSection.height;
            assetSection.width = position.width - categorySection.width;
            assetSection.height = position.height;

            GUI.DrawTexture(headerSection, headerSectionTexture);
            GUI.DrawTexture(layerMainSection, layerMainSectionTexture);
            GUI.DrawTexture(layerOptionsSection, layerOptionsSectionTexture);
            GUI.DrawTexture(categorySection, categorySectionTexture);
            GUI.DrawTexture(assetSection, assetSectionTexture);
        }

        void DrawHeaderSection()
        {
            GUILayout.BeginArea(headerSection);

            GUILayout.Label("Abdy Asset Manager", skin.GetStyle("header1"));

            GUILayout.EndArea();
        }

        void DrawLayerMainSection()
        {
            GUILayout.BeginArea(layerMainSection);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(layerMainSection.width), GUILayout.Height(layerMainSection.height));

            for (int i = 0; i < assetDataSOList.Count; i++)
            {
                AssetLayerSO data = assetDataSOList[i];
                bool isSelected = selectedLayerIndex == i;

                Rect labelRect = GUILayoutUtility.GetRect(new GUIContent(data.name), EditorStyles.label);
                bool isClicked = Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition);
                bool isDoubleClick = isClicked && Event.current.clickCount == 2;
                bool isF2Pressed = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F2;

                if ((isSelected && isF2Pressed) || isDoubleClick)
                {
                    isRenamingLayer = true;
                    layerRenamingText = data.name;
                    Event.current.Use();
                }

                if (isRenamingLayer && isSelected)
                {

                    GUI.SetNextControlName("RenamingField");
                    layerRenamingText = EditorGUI.TextField(labelRect, layerRenamingText);
                    bool enterPressed = Event.current.keyCode == KeyCode.Return;

                    if (enterPressed || (Event.current.type == EventType.MouseDown && !labelRect.Contains(Event.current.mousePosition)))
                    {
                        isRenamingLayer = false;

                        if (!string.IsNullOrWhiteSpace(layerRenamingText) && layerRenamingText != data.name)
                        {
                            Undo.RecordObject(data, "Rename Asset Layer");
                            data.name = layerRenamingText;
                            EditorUtility.SetDirty(data);
                            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(data), layerRenamingText); // Rename the asset file
                            AssetDatabase.SaveAssets();
                        }
                    }
                }
                else
                {
                    // Draw label and handle selection
                    EditorGUI.LabelField(labelRect, data.name, isSelected ? EditorStyles.whiteLabel : EditorStyles.label);
                    if (isClicked && Event.current.button == 0)
                    {
                        selectedLayerIndex = i;
                        ChangeSelectedLayer(new SerializedObject(data));
                        Event.current.Use();
                    }
                }
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }


        void DrawLayerOptionsSection()
        {
            GUILayout.BeginArea(layerOptionsSection);

            for (int i = 0; i < sceneLayerMaskOfSelectedLayer.arraySize; i++)
            {
                SerializedProperty sceneLayerMaskLayer = sceneLayerMaskOfSelectedLayer.GetArrayElementAtIndex(i);

                if (sceneLayerMaskLayer.objectReferenceValue != null )
                {
                    GUILayout.Label(sceneLayerMaskLayer.objectReferenceValue.name.ToString());
                }
                else
                {
                    GUILayout.Label("Null");
                }

            }

            GUILayout.EndArea();
        }

        void DrawCategorySection()
        {
            GUILayout.BeginArea(categorySection);

            foreach (SerializedProperty property in assetCategoriesOfSelectedLayer)
            {
                if (property.arraySize != 0)
                {
                    for (int i = 0; i < property.arraySize; i++)
                    {
                        SerializedProperty _arrayElement = property.GetArrayElementAtIndex(i);

                        GUILayout.Label(_arrayElement.FindPropertyRelative("groupName").stringValue);
                    }
                }
            }

            GUILayout.EndArea();
        }
        void DrawAssetSection()
        {
            GUILayout.BeginArea(assetSection);

            foreach (SerializedProperty property in AssetsOfSelectedCategory)
            {
                GUILayout.Label(property.FindPropertyRelative("name").stringValue);
            }

            GUILayout.EndArea();
        }
    }
}
