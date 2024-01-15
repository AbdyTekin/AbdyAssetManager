using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using Object = UnityEngine.Object;

namespace AbdyManagement
{
    public class AssetManagerEditor : EditorWindow
    {
        private bool isRenaming;

        [MenuItem("Tools/Abdy/Asset Manager")]
        public static void OpenEditorWindow()
        {
            AssetManagerEditor wnd = GetWindow<AssetManagerEditor>();
            wnd.titleContent = new GUIContent("Asset Manager Editor");
            wnd.maxSize = new Vector2(1024, 512);
            wnd.minSize = wnd.maxSize;
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Resources/UI Toolkit/UI Document/AssetManagerEditorWindow.uxml"
                );
            VisualElement tree = visualTree.Instantiate();
            tree.style.flexGrow = 1;

            root.Add(tree);

            HandleLayerList();
            HandleGroupList();
            HandleAssetList();
        }

        private void HandleLayerList()
        {
            var layerList = rootVisualElement.Q<ListView>("layer-list");

            var items = Resources.LoadAll<AssetLayerSO>("Scriptables").ToList();
            layerList.itemsSource = items;

            layerList.makeItem = () => new VisualElement();

            layerList.bindItem = (e, i) =>
            {
                var label = new Label { text = items[i].name, focusable = true, style = { height = 30.0f, unityTextAlign = TextAnchor.MiddleLeft } };
                label.AddToClassList("layerListLabel");
                e.Add(label);
                label.RegisterCallback<MouseDownEvent>(evt =>
                {
                    if (evt.clickCount == 2 && evt.button == 0)
                    {
                        StartRename();
                    }
                });

                label.RegisterCallback<KeyDownEvent>(evt =>
                {
                    if (evt.keyCode == KeyCode.F2)
                    {
                        StartRename();
                    }
                });

                void StartRename()
                {
                    var textField = new TextField { value = label.text, style = { height = 27.5f, unityTextAlign = TextAnchor.MiddleLeft } };
                    e.Remove(label);
                    textField.AddToClassList("layerListTextField");
                    e.Add(textField);
                    textField.Q<VisualElement>().Focus();
                    textField.RegisterCallback<FocusOutEvent>(evt => UpdateAssetLayerNameOnFocusOut(textField, items[i], label, e));
                    textField.RegisterCallback<KeyDownEvent>(evt =>
                    {
                        if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                        {
                            UpdateAssetLayerNameOnFocusOut(textField, items[i], label, e);
                        }
                    });
                    layerList.ClearSelection();
                    isRenaming = true;
                }
            };

            layerList.Rebuild();

            layerList.selectionChanged += (evt) =>
            {
                if (isRenaming)
                {
                    layerList.ClearSelection();
                }
                else
                {
                    // Add group list logic
                }
            };
        }

        private void UpdateAssetLayerNameOnFocusOut(TextField textField, AssetLayerSO assetLayer, Label label, VisualElement container)
        {
            string newName = textField.value.Trim();

            if (!string.IsNullOrEmpty(newName) && newName != assetLayer.name)
            {
                UpdateAssetLayerName(newName, assetLayer);
            }

            container.Remove(textField);
            label.text = newName;
            container.Add(label);

            isRenaming = false;
        }

        private void UpdateAssetLayerName(string newName, AssetLayerSO assetLayer)
        {
            string assetPath = AssetDatabase.GetAssetPath(assetLayer);
            string newAssetName = newName;
            if (!string.IsNullOrEmpty(newAssetName) && !newAssetName.Equals(assetLayer.name))
            {
                Undo.RecordObject(assetLayer, "Rename Asset Layer");
                EditorUtility.SetDirty(assetLayer);
                AssetDatabase.RenameAsset(assetPath, newAssetName);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                assetLayer.name = newAssetName;
            }
        }

        #region Groups&Assets&Layers
        private void HandleGroupList()
        {
            SetDynamicColumnWidths("group-list", 0.75f, 0.25f);
        }

        private void HandleAssetList()
        {
            SetDynamicColumnWidths("asset-list", 0.5f, 0.5f);
        }

        private void SetDynamicColumnWidths(string tag, params float[] widths)
        {
            var listView = rootVisualElement.Q<MultiColumnListView>(tag);

            if (listView == null)
            {
                Debug.LogError($"There isn't any multi column list tagged: {tag}");
            }
            if (widths.Length != listView.columns.Count)
            {
                Debug.LogError("Column count not matching!");
            }

            float totalFlexRatio = widths.Sum();

            Action updateColumnWidths = () =>
            {
                float totalWidth = listView.resolvedStyle.width;

                for (int i = 0; i < widths.Length; i++)
                {
                    listView.columns[i].width = ((totalWidth * widths[i]) / totalFlexRatio);
                }
            };

            listView.RegisterCallback<GeometryChangedEvent>(evt => updateColumnWidths());
            updateColumnWidths();
        }
        #endregion
    }
}

