using System.Collections.Generic;
using UnityEditor;
using static UnityEngine.ParticleSystem;
using UnityEngine;

namespace AbdyManagement
{
    public class AssetManagerWindow : EditorWindow
    {
        Rect headerSection, listSection, spawnSection;
        Texture2D headerSectionTexture, listSectionTexture, spawnSectionTexture;
        Color headerSectionColor = new(60f / 255f, 60f / 255f, 60f / 255f, 1f);
        Color listSectionColor = new(85f / 255f, 85f / 255f, 85f / 255f, 1f);
        Color spawnSectionColor = new(60f / 255f, 60f / 255f, 60f / 255f, 1f);

        GUISkin skin;

        //LevelDesignerData designerData;
        SerializedObject serializedDesignerData;

        Vector2 scrollPosition;

        [MenuItem("Window/Abdy Asset Manager")]
        private static void OpenWindow()
        {
            AssetManagerWindow window = GetWindow<AssetManagerWindow>();
            window.minSize = new Vector2(400, 600);
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
            //designerData = CreateInstance<LevelDesignerData>();
            //serializedDesignerData = new(designerData);
        }

        void InitTextures()
        {
            headerSectionTexture = new(1, 1);
            headerSectionTexture.SetPixel(0, 0, headerSectionColor);
            headerSectionTexture.Apply();

            listSectionTexture = new(1, 1);
            listSectionTexture.SetPixel(0, 0, listSectionColor);
            listSectionTexture.Apply();

            spawnSectionTexture = new(1, 1);
            spawnSectionTexture.SetPixel(0, 0, spawnSectionColor);
            spawnSectionTexture.Apply();
        }

        void OnGUI()
        {
            DrawLayouts();
            DrawHeader();
            DrawListSettings();
            DrawSpawnSettings();
        }

        void DrawLayouts()
        {
            headerSection.x = 0f;
            headerSection.y = 0f;
            headerSection.width = position.width;
            headerSection.height = 40;

            listSection.x = 0f;
            listSection.y = headerSection.height;
            listSection.width = position.width;
            listSection.height = (position.height / 1.5f) - headerSection.height;

            spawnSection.x = 0f;
            spawnSection.y = listSection.height + headerSection.height;
            spawnSection.width = position.width;
            spawnSection.height = position.height - listSection.height - headerSection.height;

            GUI.DrawTexture(headerSection, headerSectionTexture);
            GUI.DrawTexture(listSection, listSectionTexture);
            GUI.DrawTexture(spawnSection, spawnSectionTexture);
        }

        void DrawHeader()
        {
            GUILayout.BeginArea(headerSection);

            GUILayout.Label("Abdy Level Designer", skin.GetStyle("header1"));

            GUILayout.EndArea();
        }

        void DrawListSettings()
        {
            /*
            List<SpawnedPlatformInfo> _map = GameObject.Find("LevelMap")?.GetComponent<LevelMap>().Map;

            GUILayout.BeginArea(listSection);

            if (_map != null && _map.Count > 0)
            {
                Color originalColor = GUI.backgroundColor;
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(listSection.width), GUILayout.Height(listSection.height));

                float totalWidth = position.width - 20;
                float fieldHeight = 24;
                GUILayout.BeginHorizontal();
                GUI.Label(new Rect(totalWidth * 0.07f, fieldHeight / 2 - 9, totalWidth * 0.2f, 18), "Type", skin.GetStyle("text1"));
                GUI.Label(new Rect(totalWidth * 0.33f, fieldHeight / 2 - 9, totalWidth * 0.12f, 18), "Distance", skin.GetStyle("text1"));
                GUI.Label(new Rect(totalWidth * 0.48f, fieldHeight / 2 - 9, totalWidth * 0.12f, 18), "Angle", skin.GetStyle("text1"));
                GUILayout.EndHorizontal();
                GUILayout.Space(24);
                for (int i = 0; i < _map.Count; i++)
                {
                    GUILayout.Space(2);
                    GUILayout.BeginHorizontal();

                    // Create a rectangle that encompasses the entire row
                    Rect rowRect = EditorGUILayout.GetControlRect(GUILayout.Width(totalWidth), GUILayout.Height(fieldHeight));
                    if (Selection.activeGameObject && IsEqualOrChild(Selection.activeGameObject.transform, _map[i].GameObject))
                    {
                        GUI.backgroundColor = Color.black;
                    }
                    GUI.Box(rowRect, "");
                    GUI.backgroundColor = originalColor;
                    GUIStyle _style1 = new(GUI.skin.button)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        font = skin.GetStyle("text4").font,
                        fontSize = skin.GetStyle("text4").fontSize
                    };
                    GUIStyle _style2 = new(GUI.skin.textField)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        font = skin.GetStyle("text4").font,
                        fontSize = skin.GetStyle("text4").fontSize
                    };

                    EditorGUI.LabelField(new Rect(rowRect.x + (totalWidth * 0.01f), rowRect.y + fieldHeight / 2 - 9, totalWidth * 0.2f, 18), i.ToString(), skin.GetStyle("text3"));
                    EditorGUI.LabelField(new Rect(rowRect.x + (totalWidth * 0.07f), rowRect.y + fieldHeight / 2 - 9, totalWidth * 0.2f, 18), _map[i].Name.ToString(), skin.GetStyle("text4"));

                    float _oldDistance = _map[i].Distance;
                    _map[i].Distance = EditorGUI.FloatField(new Rect(rowRect.x + totalWidth * 0.33f, rowRect.y + fieldHeight / 2 - 9, totalWidth * 0.12f, 18), _map[i].Distance, _style2);
                    if (_oldDistance != _map[i].Distance)
                    {
                        Vector3 _offset = (_map[i].Distance - _oldDistance) * _map[i].GameObject.GetComponent<PlatformBrain>().InDirection;
                        for (int j = i; j < _map.Count; j++)
                        {
                            _map[j].GameObject.transform.position += _offset;
                        }
                    }
                    float _oldAngle = _map[i].Angle;
                    _map[i].Angle = EditorGUI.FloatField(new Rect(rowRect.x + totalWidth * 0.48f, rowRect.y + fieldHeight / 2 - 9, totalWidth * 0.12f, 18), _map[i].Angle, _style2);
                    if (_oldAngle != _map[i].Angle)
                    {
                        float _offset = _map[i].Angle - _oldAngle;
                        for (int j = i; j < _map.Count; j++)
                        {
                            Vector3 _tempIn, _tempOut;
                            PlatformBrain _brain = _map[j].GameObject.GetComponent<PlatformBrain>();

                            _tempOut = _brain.Out.position;
                            _map[j].GameObject.transform.position -= _brain.InDirection * _map[j].Distance;
                            _tempIn = _brain.In.position;
                            _map[j].GameObject.transform.Rotate(Vector3.up, _offset);
                            _map[j].GameObject.transform.position += _tempIn - _brain.In.position;
                            _map[j].GameObject.transform.position += _brain.InDirection * _map[j].Distance;

                            for (int k = j + 1; k < _map.Count; k++)
                            {
                                _map[k].GameObject.transform.position += _brain.Out.position - _tempOut;
                            }
                        }
                    }
                    GUI.backgroundColor = new Color(70f / 255f, 215f / 255f, 254f / 255f, 1f);
                    if (GUI.Button(new Rect(rowRect.x + totalWidth * 0.68f, rowRect.y + fieldHeight / 2 - 9, totalWidth * 0.1f, 18), "<", _style1))
                    {
                        if (Selection.activeGameObject != _map[i].GameObject)
                        {
                            Selection.activeGameObject = _map[i].GameObject;
                        }
                        else
                        {
                            SceneView.lastActiveSceneView.FrameSelected();
                        }
                    }
                    GUI.backgroundColor = Color.red;
                    if (GUI.Button(new Rect(rowRect.x + totalWidth * 0.8f, rowRect.y + fieldHeight / 2 - 9, totalWidth * 0.1f, 18), "-", _style1))
                    {
                        if (i < _map.Count - 1)
                        {
                            Vector3 _offset = _map[i].GameObject.GetComponent<PlatformBrain>().In.position - _map[i + 1].GameObject.GetComponent<PlatformBrain>().In.position;

                            _map[i + 1].Angle += _map[i].Angle;
                            for (int j = i; j < _map.Count; j++)
                            {
                                _map[j].GameObject.transform.position += _offset;
                            }
                        }
                        DestroyImmediate(_map[i].GameObject);
                        _map.Remove(_map[i]);
                    }
                    GUI.backgroundColor = originalColor;
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("List is empty or not found!", skin.GetStyle("header2"));
                GUILayout.FlexibleSpace();
            }

            GUILayout.EndArea();
            */
        }
        void DrawSpawnSettings()
        {
            /*
            GUILayout.BeginArea(spawnSection);

            GUILayout.Space(25);

            GUILayout.BeginHorizontal();
            GUILayout.Space(position.width / 15);
            GUILayout.Label("Name:", skin.GetStyle("text2"));
            GUILayout.FlexibleSpace();
            SerializedProperty platformNameProperty = serializedDesignerData.FindProperty("PlatformName");
            EditorGUILayout.PropertyField(platformNameProperty, GUIContent.none);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Space(position.width / 15);
            GUILayout.Label("Distance:", skin.GetStyle("text2"));
            GUILayout.FlexibleSpace();
            SerializedProperty distanceProperty = serializedDesignerData.FindProperty("Distance");
            EditorGUILayout.PropertyField(distanceProperty, GUIContent.none, GUILayout.Width(position.width / 1.5f));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Space(position.width / 15);
            GUILayout.Label("Angle:", skin.GetStyle("text2"));
            GUILayout.FlexibleSpace();
            SerializedProperty angleProperty = serializedDesignerData.FindProperty("Angle");
            EditorGUILayout.PropertyField(angleProperty, GUIContent.none, GUILayout.Width(position.width / 1.5f));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Space(position.width / 15);
            GUILayout.Label("Hard Direction:", skin.GetStyle("text2"));
            SerializedProperty hardDirectionBoolProperty = serializedDesignerData.FindProperty("HardDirectionBool");
            GUILayout.Space(4);
            EditorGUILayout.PropertyField(hardDirectionBoolProperty, GUIContent.none, GUILayout.Width(30));
            GUILayout.FlexibleSpace();
            SerializedProperty hardDirectionVectorProperty = serializedDesignerData.FindProperty("HardDirectionVector");
            if (designerData.HardDirectionBool) EditorGUILayout.PropertyField(hardDirectionVectorProperty, GUIContent.none);
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Space(position.width / 15);
            GUILayout.Label("StartPosition:", skin.GetStyle("text2"));
            GUILayout.FlexibleSpace();
            SerializedProperty startPositionProperty = serializedDesignerData.FindProperty("StartPosition");
            EditorGUILayout.PropertyField(startPositionProperty, GUIContent.none);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("+", GUILayout.Width(100)))
            {
                List<PlatformInfo> _prefabs = Resources.Load<GameObject>("Prefabs/PlatformInfos")?.GetComponent<PlatformInfos>().Prefabs;
                List<SpawnedPlatformInfo> _map = GameObject.Find("LevelMap")?.GetComponent<LevelMap>().Map;
                if (_prefabs == null)
                {
                    Debug.LogError("Not found 'PlatformInfos' GameObject in 'Resources/Prefabs '!");
                }

                if (_map == null)
                {
                    Debug.LogError("Not found 'LevelMap' GameObject in scene!");
                }
                SpawnNewPlatform(_prefabs, _map, designerData.PlatformName, designerData.Distance, designerData.Angle, designerData.Amount);
            }
            GUI.backgroundColor = originalColor;
            SerializedProperty amountProperty = serializedDesignerData.FindProperty("Amount");
            amountProperty.intValue = Mathf.Max(1, amountProperty.intValue);
            EditorGUILayout.PropertyField(amountProperty, GUIContent.none, GUILayout.Width(30));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
            serializedDesignerData.ApplyModifiedProperties();
            */
        }
    }
}
