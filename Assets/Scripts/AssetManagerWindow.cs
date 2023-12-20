using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace AbdyManagement
{
    public class AssetManagerWindow : EditorWindow
    {
        #region Sections & Textures & Colors & Style
        Rect footerSection, layerMainSection, layerOptionsSection, groupSection, assetSection;
        Texture2D footerSectionTexture, layerMainSectionTexture, layerOptionsSectionTexture, groupSectionTexture, assetSectionTexture;
        Color footerSectionColor = new(40f / 255f, 40f / 255f, 40f / 255f, 1f);
        Color layerMainSectionColor = new(60f / 255f, 60f / 255f, 60f / 255f, 1f);
        Color layerOptionsSectionColor = new(70f / 255f, 70f / 255f, 70f / 255f, 1f);
        Color groupSectionColor = new(75f / 255f, 75f / 255f, 75f / 255f, 1f);
        Color assetSectionColor = new(90f / 255f, 90f / 255f, 90f / 255f, 1f);
        GUISkin skin;
        #endregion

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

        private int frameWidth = 4;
        private string version = "1.0.0";

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

        #region Initialize Data
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
        #endregion

        void ChangeSelectedLayer(SerializedObject layer)
        {
            selectedAssetLayer = layer;
            InitializeSelectedData();
        }

        #region Initialize Textures
        void InitTextures()
        {
            footerSectionTexture = new(1, 1);
            footerSectionTexture.SetPixel(0, 0, footerSectionColor);
            footerSectionTexture.Apply();

            layerMainSectionTexture = new(1, 1);
            layerMainSectionTexture.SetPixel(0, 0, layerMainSectionColor);
            layerMainSectionTexture.Apply();

            layerOptionsSectionTexture = new(1, 1);
            layerOptionsSectionTexture.SetPixel(0, 0, layerOptionsSectionColor);
            layerOptionsSectionTexture.Apply();

            groupSectionTexture = new(1, 1);
            groupSectionTexture.SetPixel(0, 0, groupSectionColor);
            groupSectionTexture.Apply();

            assetSectionTexture = new(1, 1);
            assetSectionTexture.SetPixel(0, 0, assetSectionColor);
            assetSectionTexture.Apply();
        }
        #endregion

        void OnGUI()
        {
            DrawLayouts();

            DrawFooterSection();
            DrawLayerMainSection();
            DrawLayerOptionsSection();
            DrawGroupSection();
            DrawAssetSection();
        }

        #region Draw Layouts
        void DrawLayouts()
        {
            footerSection.x = 0f;
            footerSection.y = position.height - 20;
            footerSection.width = position.width;
            footerSection.height = 20;

            layerMainSection.x = 0f;
            layerMainSection.y = 0f;
            layerMainSection.width = position.width / 5;
            layerMainSection.height = (position.height - footerSection.height) / 2;

            layerOptionsSection.x = 0f;
            layerOptionsSection.y = layerMainSection.y + layerMainSection.height;
            layerOptionsSection.width = layerMainSection.width;
            layerOptionsSection.height = position.height - (layerMainSection.height + footerSection.height);

            groupSection.x = layerMainSection.x + layerMainSection.width;
            groupSection.y = 0f;
            groupSection.width = layerMainSection.width * 1.62f;
            groupSection.height = position.height - footerSection.height;

            assetSection.x = groupSection.x + groupSection.width;
            assetSection.y = 0f;
            assetSection.width = position.width - (groupSection.width + layerMainSection.width);
            assetSection.height = position.height - footerSection.height;

            GUI.DrawTexture(footerSection, footerSectionTexture);
            GUI.DrawTexture(layerMainSection, layerMainSectionTexture);
            GUI.DrawTexture(layerOptionsSection, layerOptionsSectionTexture);
            GUI.DrawTexture(groupSection, groupSectionTexture);
            GUI.DrawTexture(assetSection, assetSectionTexture);

            EditorGUI.DrawRect(new Rect(0, 0, frameWidth, position.height), footerSectionColor);
            EditorGUI.DrawRect(new Rect(layerMainSection.width, 0, frameWidth, position.height), footerSectionColor);
            EditorGUI.DrawRect(new Rect(layerMainSection.width + groupSection.width, 0, frameWidth, position.height), footerSectionColor);
            EditorGUI.DrawRect(new Rect(position.width - frameWidth, 0, frameWidth, position.height), footerSectionColor);
            EditorGUI.DrawRect(new Rect(0, layerMainSection.height, layerOptionsSection.width, frameWidth), footerSectionColor);
        }
        #endregion

        void DrawLayerMainSection()
        {
            GUILayout.BeginArea(layerMainSection);
            GUILayout.Label("Layers", skin.GetStyle("h_1"));
            EditorGUI.DrawRect(new Rect(frameWidth, 15, layerMainSection.width - frameWidth, 1), Color.gray);
            EditorGUILayout.Space(10);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(layerMainSection.width), GUILayout.Height(layerMainSection.height - 20));

            for (int i = 0; i < assetDataSOList.Count; i++)
            {
                GUILayout.BeginHorizontal();
                Rect elementRect = EditorGUILayout.GetControlRect(GUILayout.Width(layerMainSection.width - 38), GUILayout.Height(22));
                Rect elementRenameRect = new (elementRect.x + elementRect.width - 15, elementRect.y + 3, 14,elementRect.height * 14 / 20);
                elementRect.x += 4;
                AssetLayerSO data = assetDataSOList[i];
                bool isSelected = selectedLayerIndex == i;
                bool isClicked = Event.current.type == EventType.MouseDown && elementRect.Contains(Event.current.mousePosition);
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
                    layerRenamingText = EditorGUI.TextField(elementRect, layerRenamingText, skin.textField);
                    bool enterPressed = Event.current.keyCode == KeyCode.Return;

                    if (enterPressed || (Event.current.type == EventType.MouseDown && !elementRect.Contains(Event.current.mousePosition)))
                    {
                        isRenamingLayer = false;

                        if (!string.IsNullOrWhiteSpace(layerRenamingText) && layerRenamingText != data.name)
                        {
                            Undo.RecordObject(data, "Rename Asset Layer");
                            data.name = layerRenamingText;
                            EditorUtility.SetDirty(data);
                            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(data), layerRenamingText);
                            AssetDatabase.SaveAssets();
                        }
                    }
                }
                else
                {
                    GUI.color = isSelected ? footerSectionColor : Color.white;
                    GUI.Box(elementRect, "");
                    GUI.color = Color.white;
                    EditorGUI.LabelField(elementRect, data.name, isSelected ? skin.GetStyle("l_t_1") : skin.GetStyle("l_t_2"));
                    if (isClicked && Event.current.button == 0)
                    {
                        selectedLayerIndex = i;
                        ChangeSelectedLayer(new SerializedObject(data));
                        Event.current.Use();
                    }
                }
                if (isSelected)
                {
                    GUI.DrawTexture(elementRenameRect, skin.box.normal.background);

                }
                GUILayout.EndHorizontal();
                GUILayout.Space(2);
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }


        void DrawLayerOptionsSection()
        {
            GUILayout.BeginArea(layerOptionsSection);

            GUILayout.Space(5);
            GUILayout.Label("Layer Options", skin.GetStyle("h_1"));
            EditorGUI.DrawRect(new Rect(frameWidth, 20, layerOptionsSection.width - frameWidth, 1), Color.gray);
            EditorGUILayout.Space(10);

            for (int i = 0; i < sceneLayerMaskOfSelectedLayer.arraySize; i++)
            {
                SerializedProperty sceneLayerMaskLayer = sceneLayerMaskOfSelectedLayer.GetArrayElementAtIndex(i);

                if (sceneLayerMaskLayer.objectReferenceValue != null )
                {
                    GUILayout.Label(sceneLayerMaskLayer.objectReferenceValue.name.ToString(), skin.GetStyle("lo_t_1"));
                }
                else
                {
                    GUILayout.Label("Null");
                }

            }

            GUILayout.EndArea();
        }

        void DrawGroupSection()
        {
            GUILayout.BeginArea(groupSection);

            GUILayout.Label("Groups", skin.GetStyle("h_1"));
            EditorGUI.DrawRect(new Rect(frameWidth, 15, groupSection.width - frameWidth, 1), Color.gray);
            EditorGUILayout.Space(10);

            foreach (SerializedProperty property in assetCategoriesOfSelectedLayer)
            {
                if (property.arraySize != 0)
                {
                    for (int i = 0; i < property.arraySize; i++)
                    {
                        SerializedProperty _arrayElement = property.GetArrayElementAtIndex(i);

                        GUILayout.Label(_arrayElement.FindPropertyRelative("groupName").stringValue, skin.GetStyle("g_t_1"));
                    }
                }
            }

            GUILayout.EndArea();
        }

        void DrawAssetSection()
        {
            GUILayout.BeginArea(assetSection);

            GUILayout.Label("Assets", skin.GetStyle("h_1"));
            EditorGUI.DrawRect(new Rect(frameWidth, 15, assetSection.width - frameWidth * 2, 1), Color.gray);
            EditorGUILayout.Space(10);

            foreach (SerializedProperty property in AssetsOfSelectedCategory)
            {
                GUILayout.Label(property.FindPropertyRelative("name").stringValue, skin.GetStyle("a_t_1"));
            }

            GUILayout.EndArea();
        }

        void DrawFooterSection()
        {
            GUILayout.BeginArea(footerSection);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Abdy Asset Manager", skin.GetStyle("f_t_1"));
            GUILayout.Label("V" + version, skin.GetStyle("f_t_2"));
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}
