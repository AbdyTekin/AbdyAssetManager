using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static AbdyManagement.AssetLayerSO;
using static UnityEditor.Experimental.GraphView.GraphView;
using Object = UnityEngine.Object;

namespace AbdyManagement
{
    public class AssetManagerEditor : EditorWindow
    {
        private bool isRenaming;
        private int lastSelectedLayerIndex;

        [MenuItem("Tools/Abdy/Asset Manager")]
        public static void OpenEditorWindow()
        {
            AssetManagerEditor wnd = GetWindow<AssetManagerEditor>();
            wnd.titleContent = new GUIContent("Asset Manager Editor");
            wnd.minSize = new Vector2(512, 256);
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

            CreateSectionLists();
        }

        private void CreateSectionLists()
        {
            var layerList = rootVisualElement.Q<ListView>("layer-list");
            var groupList = rootVisualElement.Q<MultiColumnListView>("group-list");
            var assetList = rootVisualElement.Q<MultiColumnListView>("asset-list");
            SetDynamicColumnWidths("group-list", 0.70f, 0.25f);
            SetDynamicColumnWidths("asset-list", 0.5f, 0.5f);

            DefineLists(layerList, groupList, assetList);

            var items = Resources.LoadAll<AssetLayerSO>("Scriptables").ToList();
            layerList.itemsSource = items;
            layerList.Rebuild();

            layerList.selectionChanged += (evt) =>
            {
                if (isRenaming)
                {
                    layerList.ClearSelection();
                }
                else
                {
                    CreateGroupList(layerList.selectedItem as AssetLayerSO);
                }
            };
        }

        private void CreateGroupList(AssetLayerSO layer)
        {
            var groupList = rootVisualElement.Q<MultiColumnListView>("group-list");

            groupList.itemsSource = layer.groups;

            groupList.selectionChanged += (evt) =>
            {
                if (isRenaming)
                {
                    groupList.ClearSelection();
                }
                else
                {
                    CreateAssetList((groupList.selectedItem as AssetGroupData<Object>).assets);
                }
            };
        }

        private void CreateAssetList(List<AssetData<Object>> list)
        {
            var assetList = rootVisualElement.Q<MultiColumnListView>("asset-list");

            assetList.itemsSource = list;
        }

        private void DefineLists(ListView layerList, MultiColumnListView groupList, MultiColumnListView assetList)
        {
            #region Layer List View
            layerList.makeItem = () => new VisualElement();

            layerList.bindItem = (e, i) =>
            {
                if (layerList.itemsSource is not List<AssetLayerSO> layers) return;
                var label = new Label { text = layers[i].name, focusable = true, style = { height = 30.0f, unityTextAlign = TextAnchor.MiddleLeft } };
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
                    isRenaming = true;
                    var textField = new TextField { value = label.text, style = { height = 27.5f, unityTextAlign = TextAnchor.MiddleLeft } };
                    e.Remove(label);
                    textField.AddToClassList("layerListTextField");
                    e.Add(textField);
                    textField.Q<VisualElement>().Focus();
                    textField.RegisterCallback<FocusOutEvent>(evt =>
                    {
                        UpdateAssetLayerNameOnFocusOut(textField, layers[i], label, e);
                        layerList.SetSelection(lastSelectedLayerIndex);

                    });
                    textField.RegisterCallback<KeyDownEvent>(evt =>
                    {
                        if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                        {
                            UpdateAssetLayerNameOnFocusOut(textField, layers[i], label, e);
                            layerList.SetSelection(lastSelectedLayerIndex);
                        }
                    });
                    lastSelectedLayerIndex = layerList.selectedIndex;
                    layerList.ClearSelection();
                }
            };
            #endregion

            #region Group Multi Column List View
            groupList.columns["name"].makeCell = () => new Label();
            groupList.columns["type"].makeCell = () => new Label();

            groupList.columns["name"].bindCell = (VisualElement e, int index) =>
            {
                if (groupList.itemsSource is not List<AssetGroupData<Object>> groups) return;
                (e as Label).text = groups[index].groupName;
            };

            groupList.columns["type"].bindCell = (VisualElement e, int index) =>
            {
                if (groupList.itemsSource is not List<AssetGroupData<Object>> groups) return;
                (e as Label).text = groups[index].assets.Count == 0 ? "null" : groups[index].assets[0].asset.GetType().Name;
            };
            #endregion

            #region Asset Multi Column List View
            assetList.columns["name"].makeCell = () => new Label();
            assetList.columns["reference"].makeCell = () => new ObjectField();

            assetList.columns["name"].bindCell = (VisualElement e, int index) =>
            {
                if (assetList.itemsSource is not List<AssetData<Object>> assets) return;
                (e as Label).text = assets[index].name;
            };

            assetList.columns["reference"].bindCell = (VisualElement e, int index) =>
            {
                if (assetList.itemsSource is not List<AssetData<Object>> assets) return;
                (e as ObjectField).objectType = assets[index].asset.GetType();

                (e as ObjectField).value = assets[index].asset;

            };
            #endregion
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

        #region DynamicColumn
        private void SetDynamicColumnWidths(string tag, params float[] widths)
        {
            var listView = rootVisualElement.Q<MultiColumnListView>(tag);
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

