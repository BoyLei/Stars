using AssetChecker.Define;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AssetCheckor.AssetBundleAssetCheck
{
    public class AssetBundleDepWindow : EditorWindow
    {
        private int _selGridInt = 0;
        private string[] _optionTexts = new string[] { "AssetBundle", "Memory Dependent" };
        private string _abName, _depName, _fullABName;

        private Vector2 _scrollPos;
        private SerializedObject _serialObject;
        private SerializedProperty _serialProp;
        private ReorderableList _reorderableList;

        [SerializeField]
        private int _selectedIndex = -1;
        [SerializeField]
        private List<DepABInfo> _listData = new List<DepABInfo>();

        [MenuItem("Tools/AssetChecker/AssetBundleCheckerWindow")]
        static void Init()
        {
            var x = (Screen.currentResolution.width / 2);
            var y = (Screen.currentResolution.height * 4);
            var wndAssetBundle = GetWindow<AssetBundleDepWindow>();
            wndAssetBundle.position = new Rect(x, y, 500, 800);
        }

        private void OnEnable()
        {
            _serialObject = new SerializedObject(this);
            _serialProp = _serialObject.FindProperty("_listData");
        }

        private void OnGUI()
        {
            DrawComponent();
            DrawReorderableList();
        }

        private void DrawComponent()
        {
            _abName = EditorGUILayout.TextField("AssetBundle Name:", _abName);
            _depName = EditorGUILayout.TextField("Dependencie Name:", _depName);

            EditorGUILayout.BeginHorizontal("Button");
            GUI.color = Color.red;
            if (GUILayout.Button("查找AB依赖"))
            {
                FindDepAssetBundle();
                UpdateSerialObject();
            }
            GUI.color = Color.white;
            if (GUILayout.Button("AB关系链"))
            {
                StartAssetCheck();
            }
            if (GUILayout.Button("循环依赖"))
            {
                FindDepAssetBundle(true);
                UpdateSerialObject();
            }
            GUI.color = Color.green;
            if (GUILayout.Button("清空列表"))
            {
                _listData.Clear();
                UpdateSerialObject();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("AB Path：" + _fullABName);
            GUI.color = Color.white;

            EditorGUILayout.BeginHorizontal("OptionButton"); 
            _selGridInt = GUILayout.SelectionGrid(_selGridInt, _optionTexts, 2, EditorStyles.miniButton);
            EditorGUILayout.EndHorizontal();
        }

        private void StartAssetCheck()
        {
            var _dataSourceType = (DataSourceType)_selGridInt;
            AssetBundleChecker.StartAssetCheck(_dataSourceType);
        }

        private void FindDepAssetBundle(bool isLoopDep = false)
        {
            if (string.IsNullOrEmpty(_abName))
            {
                return;
            }
            _listData.Clear();
            string bundle;
            string[] deps = null;

            var _dataSourceType = (DataSourceType)_selGridInt;
            if (isLoopDep)
            {
                (bundle, deps) = AssetBundleChecker.GetLoopDependency(_abName, _dataSourceType);
            }
            else
            {
                (bundle, deps) = AssetBundleChecker.GetDepAssetBundle(_abName, _dataSourceType);
            }
            if (deps == null)
            {
                return;
            }
            foreach (var dep in deps)
            {
                if (dep.Contains("shader_"))
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(_depName))
                {
                    if (dep.Contains(_depName))
                    {
                        _listData.Add(new DepABInfo { _name = dep });
                    }
                }
                else
                {
                    _listData.Add(new DepABInfo { _name = dep });
                }
            }
            _fullABName = bundle;
        }

        private void UpdateSerialObject()
        {
            _serialObject?.Update();
            _serialObject?.ApplyModifiedProperties();
        }

        private void DrawReorderableList()
        {
            _reorderableList = new ReorderableList(_serialObject, _serialProp, true, true, true, true);
            _reorderableList.elementHeight = 20;
            _reorderableList.displayAdd = _reorderableList.displayRemove = false;
            _reorderableList.index = _serialObject.FindProperty("_selectedIndex").intValue;
            _reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var e = _reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                rect.height -= 4;
                rect.y += 2;
                EditorGUI.HelpBox(rect, "", MessageType.None);
                EditorGUI.LabelField(rect, e.FindPropertyRelative("_name").stringValue);
            };

            _reorderableList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Dependencies:");
                Texture2D btnTexture = _reorderableList.elementHeight == 0f ? EditorGUIUtility.FindTexture("winbtn_win_max_h") : EditorGUIUtility.FindTexture("winbtn_win_min_h");
                if (GUI.Button(new Rect(rect.width + 4, rect.y + 2, rect.height, rect.height), btnTexture, EditorStyles.label))
                {
                    _reorderableList.elementHeight = _reorderableList.elementHeight == 0f ? 21f : 0f;
                    _reorderableList.draggable = _reorderableList.elementHeight > 0f;
                }
            };

            _reorderableList.onSelectCallback = delegate (ReorderableList list)
            {
                _serialObject.FindProperty("_selectedIndex").intValue = list.index;
                _serialObject.ApplyModifiedProperties();
                GUI.changed = true;
            };

            _reorderableList.onReorderCallback = delegate (ReorderableList list)
            {
                Repaint();
            };

            if (_reorderableList != null)
            {
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(position.width - 10), GUILayout.Height(position.height - 50));
                _reorderableList.DoLayoutList();
                EditorGUILayout.EndScrollView();
            }
        }

        private void OnInspectorUpdate()
        {
            if (EditorWindow.mouseOverWindow)
            {
                EditorWindow.mouseOverWindow.Focus();
            }
            this.Repaint();
        }
    }
}
