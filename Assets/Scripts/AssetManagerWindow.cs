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
        List<SerializedProperty> assetLayerCategoriesOfSelected;

        SerializedProperty selectedAssetGroup;
        List<SerializedProperty> categoryAssetsOfSelected;

        SerializedProperty selectedAsset;

        Vector2 scrollPosition;

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

            if (assetDataSOList.Count > 0)
            {
                selectedAssetLayer = new(assetDataSOList[0]);
                selectedAssetGroup = selectedAssetLayer.FindProperty("sprites").GetArrayElementAtIndex(0);
                selectedAsset = selectedAssetGroup.FindPropertyRelative("data").GetArrayElementAtIndex(0);

                assetLayerCategoriesOfSelected = new List<SerializedProperty>
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

                categoryAssetsOfSelected = new();
                for (int i = 0; i < selectedAssetGroup.FindPropertyRelative("data").arraySize; i++)
                {
                    SerializedProperty _data = selectedAssetGroup.FindPropertyRelative("data").GetArrayElementAtIndex(i);
                    categoryAssetsOfSelected.Add(_data);
                }
            }

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

            foreach (AssetLayerSO data in assetDataSOList)
            {
                GUILayout.Label(data.ToString());
            }

            GUILayout.EndArea();
        }

        void DrawLayerOptionsSection()
        {
            GUILayout.BeginArea(layerOptionsSection);

            GUILayout.EndArea();
        }

        void DrawCategorySection()
        {
            GUILayout.BeginArea(categorySection);

            foreach (SerializedProperty property in assetLayerCategoriesOfSelected)
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

            foreach (SerializedProperty property in categoryAssetsOfSelected)
            {
                GUILayout.Label(property.FindPropertyRelative("name").stringValue);
            }

            GUILayout.EndArea();
        }
    }
}
