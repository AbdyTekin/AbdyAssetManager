using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static AbdyManagement.AssetLayerSO;
using Object = UnityEngine.Object;

namespace AbdyManagement
{
    public class AssetManagerEditor : EditorWindow
    {
        private bool isRenaming;
        private int lastSelectedLayerIndex;

        private Dictionary<int, EventCallback<MouseDownEvent>> groupMouseDownCallbacks = new();
        private Dictionary<int, EventCallback<KeyDownEvent>> groupKeyDownCallbacks = new();
        private Dictionary<int, EventCallback<ContextClickEvent>> groupContextClickCallbacks = new();
        private Dictionary<int, EventCallback<MouseDownEvent>> assetMouseDownCallbacks = new();
        private Dictionary<int, EventCallback<KeyDownEvent>> assetKeyDownCallbacks = new();
        private Dictionary<int, EventCallback<ContextClickEvent>> assetContextClickCallbacks = new();
        private Dictionary<int, EventCallback<ChangeEvent<Object>>> assetChangedCallbacks = new();

        #region Constant Strings
        private const string s_editorName = "Asset Manager Editor";
        private const string s_menuPath = "Tools/Abdy/Asset Manager";
        private const string s_layerList = "layer-list";
        private const string s_groupList = "group-list";
        private const string s_assetList = "asset-list";
        private const string s_scriptablesPath = "Assets/Resources/Scriptables";
        private const string s_scriptablesPathInResources = "Scriptables";
        private const string s_uxmlFileLocation = "Assets/Resources/UI Toolkit/UI Document/AssetManagerEditorWindow.uxml";
        private const string s_layerTextFieldClass = "layerListTextField";
        private const string s_groupTextFieldClass = "groupListTextField";
        private const string s_assetTextFieldClass = "assetListTextField";
        #endregion

        [MenuItem(s_menuPath)]
        public static void OpenEditorWindow()
        {
            var wnd = GetWindow<AssetManagerEditor>();
            wnd.titleContent = new GUIContent(s_editorName);
            wnd.minSize = new Vector2(512, 256);
        }

        private void CreateGUI()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(s_uxmlFileLocation);
            VisualElement tree = visualTree.Instantiate();
            tree.style.flexGrow = 1;
            rootVisualElement.Add(tree);
            CreateLists();
        }

        private void CreateLists()
        {
            var layerList = rootVisualElement.Q<ListView>(s_layerList);
            var groupList = rootVisualElement.Q<MultiColumnListView>(s_groupList);
            var assetList = rootVisualElement.Q<MultiColumnListView>(s_assetList);

            #region Layer List View
            layerList.itemsSource = Resources.LoadAll<AssetLayerSO>(s_scriptablesPathInResources).ToList();
            MyAssetPostprocessor.OnFileUpdate += () =>
            {
                layerList.itemsSource = Resources.LoadAll<AssetLayerSO>(s_scriptablesPathInResources).ToList();
                layerList.Rebuild();
            };

            layerList.makeItem = () => new Label();

            layerList.bindItem = (visual, index) =>
            {
                if (layerList.itemsSource is not List<AssetLayerSO> layers) return;
                var label = visual as Label;
                label.text = layers[index].name;
                label.focusable = true ;
                label.AddToClassList("layerListLabel");

                label.RegisterCallback<MouseDownEvent>(evt => MouseDownEventCallback(evt, layerList, index, visual, label, s_layerTextFieldClass));
                label.RegisterCallback<KeyDownEvent>(evt => KeyDownEventCallback(evt, layerList, index, visual, label, s_layerTextFieldClass));
                label.RegisterCallback<ContextClickEvent>(evt => ContextClickEventCallback(layerList, index, visual, label, s_layerTextFieldClass));
            };

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
            #endregion

            #region Group Multi Column List View
            groupList.columns["name"].makeCell = () => new Label();
            groupList.columns["type"].makeCell = () => new Label();

            groupList.columns["name"].bindCell = (VisualElement visual, int index) =>
            {
                if (groupList.itemsSource is not List<AssetGroupData<Object>> groups) return;
                var label = visual as Label;
                label.text = groups[index].groupName;
                label.focusable = true;
                label.AddToClassList("groupListLabel1");

                EventCallback<MouseDownEvent> mouseDownCallback = evt =>
                {
                    if (evt.clickCount == 2 && evt.button == 0)
                    {
                        StartRename();
                    }
                };
                EventCallback<KeyDownEvent> keyDownCallback = evt =>
                {
                    if (evt.keyCode == KeyCode.F2)
                    {
                        StartRename();
                    }
                    else if (evt.keyCode == KeyCode.Delete)
                    {
                        Delete();
                    }
                };
                EventCallback<ContextClickEvent> contextClickCallback = evt =>
                {
                    GenericMenu menu = new();
                    menu.AddItem(new GUIContent("Rename"), false, () => StartRename());
                    menu.AddItem(new GUIContent("Delete"), false, () => Delete());
                    menu.ShowAsContext();
                };

                //label.RegisterCallback(mouseDownCallback);
                //label.RegisterCallback(keyDownCallback);
                //label.RegisterCallback(contextClickCallback);

                label.RegisterCallback<MouseDownEvent>(evt => MouseDownEventCallback(evt, groupList, index, visual, label, s_groupTextFieldClass));
                label.RegisterCallback<KeyDownEvent>(evt => KeyDownEventCallback(evt, groupList, index, visual, label, s_groupTextFieldClass));
                label.RegisterCallback<ContextClickEvent>(evt => ContextClickEventCallback(groupList, index, visual, label, s_groupTextFieldClass));

                groupMouseDownCallbacks[index] = mouseDownCallback;
                groupKeyDownCallbacks[index] = keyDownCallback;
                groupContextClickCallbacks[index] = contextClickCallback;

                void StartRename()
                {
                    isRenaming = true;
                    var textField = new TextField { value = label.text };
                    textField.AddToClassList("groupListTextField");
                    visual.Add(textField);
                    textField.Q<VisualElement>().Focus();
                    textField.RegisterCallback<FocusOutEvent>(evt =>
                    {
                        ChangeNameOnFocusOut(textField);
                    });
                    textField.RegisterCallback<KeyDownEvent>(evt =>
                    {
                        if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                        {
                            ChangeNameOnFocusOut(textField);
                        }
                    });
                    groupList.ClearSelection();
                }

                void ChangeNameOnFocusOut(TextField textField)
                {
                    string newName = textField.value.Trim();

                    if (!string.IsNullOrEmpty(newName) && newName != groups[index].groupName)
                    {
                        groups[index].groupName = newName;
                    }

                    visual.Remove(textField);

                    label.text = newName;

                    isRenaming = false;
                    groupList.SetSelection(-1);
                }

                void Delete()
                {
                    groups.RemoveAt(index);
                    groupList.Rebuild();
                }
            };

            groupList.columns["name"].unbindCell = (e, i) =>
            {
                var label = e as Label;

                if (groupMouseDownCallbacks.TryGetValue(i, out var oldMouseDownCallback))
                {
                    label.UnregisterCallback(oldMouseDownCallback);
                }
                if (groupKeyDownCallbacks.TryGetValue(i, out var oldKeyDownCallback))
                {
                    label.UnregisterCallback(oldKeyDownCallback);
                }
                if (groupContextClickCallbacks.TryGetValue(i, out var oldContextClickCallback))
                {
                    label.UnregisterCallback(oldContextClickCallback);
                }
            };

            groupList.columns["type"].bindCell = (VisualElement e, int index) =>
            {
                if (groupList.itemsSource is not List<AssetGroupData<Object>> groups) return;
                (e as Label).text = groups[index].assets.Count == 0 ? "null" : groups[index].assets[0].asset.GetType().Name;
                (e as Label).AddToClassList("groupListLabel2");
            };

            groupList.selectionChanged += (evt) =>
            {
                if (groupList.selectedIndex == -1)
                {
                    return;
                }
                else if (isRenaming)
                {
                    groupList.ClearSelection();
                }
                else
                {
                    CreateAssetList((groupList.selectedItem as AssetGroupData<Object>).assets);
                }
            };

            SetDynamicColumnWidths(s_groupList, 0.65f, 0.35f);
            #endregion

            #region Asset Multi Column List View
            assetList.columns["name"].makeCell = () => new Label();
            assetList.columns["reference"].makeCell = () => new ObjectField();

            assetList.columns["name"].bindCell = (VisualElement visual, int i) =>
            {
                if (assetList.itemsSource is not List<AssetData<Object>> assets) return;
                var label = visual as Label;
                label.text = assets[i].name;
                label.focusable = true;
                label.AddToClassList("assetListLabel");


                EventCallback<MouseDownEvent> mouseDownCallback = evt =>
                {
                    if (evt.clickCount == 2 && evt.button == 0)
                    {
                        StartRename();
                    }
                };
                EventCallback<KeyDownEvent> keyDownCallback = evt =>
                {
                    if (evt.keyCode == KeyCode.F2)
                    {
                        StartRename();
                    }
                    else if (evt.keyCode == KeyCode.Delete)
                    {
                        Delete();
                    }
                };
                EventCallback<ContextClickEvent> contextClickCallback = evt =>
                {
                    GenericMenu menu = new();
                    menu.AddItem(new GUIContent("Rename"), false, () => StartRename());
                    menu.AddItem(new GUIContent("Delete"), false, () => Delete());
                    menu.ShowAsContext();
                };

                label.RegisterCallback(mouseDownCallback);
                label.RegisterCallback(keyDownCallback);
                label.RegisterCallback(contextClickCallback);

                assetMouseDownCallbacks[i] = mouseDownCallback;
                assetKeyDownCallbacks[i] = keyDownCallback;
                assetContextClickCallbacks[i] = contextClickCallback;

                void StartRename()
                {
                    isRenaming = true;
                    var textField = new TextField { value = label.text };
                    textField.AddToClassList("assetListTextField");
                    visual.Add(textField);
                    textField.Q<VisualElement>().Focus();
                    textField.RegisterCallback<FocusOutEvent>(evt =>
                    {
                        ChangeNameOnFocusOut(textField);
                    });
                    textField.RegisterCallback<KeyDownEvent>(evt =>
                    {
                        if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                        {
                            ChangeNameOnFocusOut(textField);
                        }
                    });
                    assetList.ClearSelection();
                }

                void ChangeNameOnFocusOut(TextField textField)
                {
                    string newName = textField.value.Trim();

                    if (!string.IsNullOrEmpty(newName) && newName != assets[i].name)
                    {
                        assets[i].name = newName;
                    }

                    visual.Remove(textField);

                    label.text = newName;

                    isRenaming = false;
                    assetList.SetSelection(-1);
                }

                void Delete()
                {
                    assets.RemoveAt(i);
                    assetList.Rebuild();
                }
            };

            assetList.columns["name"].unbindCell = (e, i) =>
            {
                var label = e as Label;

                if (assetMouseDownCallbacks.TryGetValue(i, out var oldMouseDownCallback))
                {
                    label.UnregisterCallback(oldMouseDownCallback);
                }
                if (assetKeyDownCallbacks.TryGetValue(i, out var oldKeyDownCallback))
                {
                    label.UnregisterCallback(oldKeyDownCallback);
                }
                if (assetContextClickCallbacks.TryGetValue(i, out var oldContextClickCallback))
                {
                    label.UnregisterCallback(oldContextClickCallback);
                }
            };

            assetList.columns["reference"].bindCell = (e, i) =>
            {
                if (assetList.itemsSource is not List<AssetData<Object>> assets) return;
                var objectField = e as ObjectField;
                objectField.objectType = assets[i].asset.GetType();

                objectField.value = assets[i].asset;

                EventCallback<ChangeEvent<Object>> assetChangedCallback = evt =>
                {
                    assets[i].asset = evt.newValue;
                };

                assetChangedCallbacks[i] = assetChangedCallback;

                objectField.RegisterCallback(assetChangedCallback);
            };

            assetList.columns["reference"].unbindCell = (e, i) =>
            {
                var objectField = e as ObjectField;

                if (assetChangedCallbacks.TryGetValue(i, out var assetChangedCallback))
                {
                    objectField.UnregisterCallback(assetChangedCallback);
                }
            };

            assetList.selectionChanged += (evt) =>
            {
                if (groupList.selectedIndex == -1)
                {
                    return;
                }
                else if (isRenaming)
                {
                    groupList.ClearSelection();
                }
            };

            SetDynamicColumnWidths(s_assetList, 0.5f, 0.5f);
            #endregion
        }

        private void CreateLayerList(AssetLayerSO layer)
        {
            var groupList = rootVisualElement.Q<MultiColumnListView>(s_layerList);

            groupList.itemsSource = layer.groups;
            groupList.ClearSelection();

            CreateAssetList(null);
        }

        private void CreateGroupList(AssetLayerSO layer)
        {
            var groupList = rootVisualElement.Q<MultiColumnListView>(s_groupList);

            groupList.itemsSource = layer.groups;
            groupList.ClearSelection();

            CreateAssetList(null);
        }

        private void CreateAssetList(List<AssetData<Object>> list)
        {
            var assetList = rootVisualElement.Q<MultiColumnListView>(s_assetList);

            assetList.itemsSource = list;
            assetList.ClearSelection();
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

        #region Callbacks

        private void MouseDownEventCallback(MouseDownEvent evt, BaseVerticalCollectionView list, int index, VisualElement visual, Label label, string styleClass)
        {
            if (evt.clickCount == 2 && evt.button == 0)
            {
                StartRename(list, index, list.itemsSource[index] as AssetLayerSO, visual, label, styleClass);
            }
        }

        private void KeyDownEventCallback(KeyDownEvent evt, BaseVerticalCollectionView list, int index, VisualElement visual, Label label, string styleClass)
        {
            if (evt.keyCode == KeyCode.F2)
            {
                StartRename(list, index, list.itemsSource[index] as AssetLayerSO, visual, label, styleClass);
            }
            else if (evt.keyCode == KeyCode.Delete)
            {
                Delete(list, index);
            }
        }

        private void ContextClickEventCallback(BaseVerticalCollectionView list, int index, VisualElement visual, Label label, string styleClass)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Rename"), false, () => StartRename(list, index, list.itemsSource[index] as AssetLayerSO, visual, label, styleClass));
            menu.AddItem(new GUIContent("Delete"), false, () => Delete(list, index));
            menu.ShowAsContext();
        }

        #endregion

        #region Callback Utilities
        private void StartRename(BaseVerticalCollectionView list, int index, AssetLayerSO layer, VisualElement visual, Label label, string styleClass)
        {
            isRenaming = true;
            var textField = new TextField { value = label.text };
            textField.AddToClassList(styleClass);
            visual.Add(textField);
            textField.Q<VisualElement>().Focus();
            textField.RegisterCallback<FocusOutEvent>(evt => ChangeNameOnFocusOut(list, index, layer, visual, label, textField));
            textField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    ChangeNameOnFocusOut(list, index, layer, visual, label, textField);
                }
            });
            lastSelectedLayerIndex = list.selectedIndex;
            list.ClearSelection();
        }

        private void ChangeNameOnFocusOut(BaseVerticalCollectionView list, int index, AssetLayerSO layer, VisualElement visual, Label label, TextField textField)
        {
            string newName = textField.value.Trim();

            if (list is ListView)
            {
                if (!string.IsNullOrEmpty(newName) && newName != layer.name)
                {
                    UpdateAssetLayerName(newName, layer);
                }
            }
            else if (list is MultiColumnListView)
            {
                (list.itemsSource[index] as AssetGroupData<Object>).groupName = newName;
            }

            visual.Remove(textField);
            label.text = newName;

            isRenaming = false;
            list.SetSelection(lastSelectedLayerIndex);
        }

        private void Delete(BaseVerticalCollectionView list, int index)
        {
            if (list is ListView)
            {
                DeleteAssetLayer(list.itemsSource[index] as AssetLayerSO);
                var items = Resources.LoadAll<AssetLayerSO>(s_scriptablesPathInResources).ToList();
                list.itemsSource = items;
            }
            else if (list is MultiColumnListView)
            {
                list.RemoveAt(index);
            }

            list.Rebuild();
        }

        private void UpdateAssetLayerName(string newName, AssetLayerSO assetLayer)
        {
            string assetPath = AssetDatabase.GetAssetPath(assetLayer);
            string newAssetName = newName;
            if (!string.IsNullOrEmpty(newAssetName) && !newAssetName.Equals(assetLayer.name))
            {
                AssetDatabase.RenameAsset(assetPath, newAssetName);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                assetLayer.name = newAssetName;
            }
        }

        private void DeleteAssetLayer(AssetLayerSO assetLayer)
        {
            string assetPath = AssetDatabase.GetAssetPath(assetLayer);
            if (!assetPath.Equals(string.Empty))
            {
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        #endregion

        #region AssetPostprocessor
        class MyAssetPostprocessor : AssetPostprocessor
        {
            public static event Action OnFileUpdate;
            static void OnPostprocessAllAssets(
                string[] imported,
                string[] deleted,
                string[] moved,
                string[] movedFromAssetPaths)
            {
                string[] _assets = new[] { imported, deleted, moved, movedFromAssetPaths }
                        .SelectMany(arr => arr)
                        .ToArray();
                foreach (string asset in _assets)
                {
                    if (asset.StartsWith(s_scriptablesPath))
                    {
                        OnFileUpdate?.Invoke();
                        break;
                    }
                }
            }
        }
        #endregion
    }
}

