/*
 * @Description: 通用件预览界面
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameTechTools.CommonLibs.CommonExtends;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GameTechTools.UIHelpsTool
{
    internal class GeneralCompTypeConfig : UIHelpsCustomObject
    {
        //分类名称
        [ReadOnly]
        public string typeName;

        //预览列表
        [HideInInspector]
        public List<GeneralCompPreviewItem> previewItems = new List<GeneralCompPreviewItem>();

        //当前选中的预览
        [NonSerialized]
        public GameObject curSelectGo = null;

        private float mSizePercent = 2f;

        public void OnSizePercentChange(float percent)
        {
            mSizePercent = percent;
            mCellSizeX = Mathf.FloorToInt(Mathf.FloorToInt(60 * UIHelpsToolGlobalConfig.generalSetting.eScreenStdWidth / UIHelpsToolGlobalConfig.generalSetting.eScreenStdHeight) * mSizePercent);
            mCellSizeY = Mathf.FloorToInt(60 * mSizePercent);
        }

        //反向绑定
        protected OdinMenuItem menuItem = null;
        public void RegisterMenuItem(OdinMenuItem item)
        {
            menuItem = item;
            // EditorUtility.SetDirty(this);
        }

        public void UpdateName(string itemName)
        {

            if (menuItem != null)
            {
                menuItem.SearchString = itemName;
                menuItem.Name = itemName;
            }
            EditorUtility.SetDirty(this);

            if (GeneralCompWindow.mainWindow != null)
            {
                // GeneralCompWindow.mainWindow.UpdateMenuTreeItemName(GeneralCompWindow.GetWindow().mTree);
            }
        }

        /*-----------------------------------------------------------绘制-----------------------------------------------------------*/

        List<Item> mItems = new List<Item>();
        class Item
        {
            public GameObject prefab;
            public string guid;
            public Texture tex;
        }

        const int cellPaddingX = 4;
        const int cellPaddingY = 20;
        int mCellSizeX = Mathf.FloorToInt(60f * 1920 / 1080 * 2f);
        int cellSizeX { get { return mCellSizeX; } }

        int mCellSizeY = Mathf.FloorToInt(60f * 2f);
        int cellSizeY { get { return mCellSizeY; } }

        Vector2 mPos = Vector2.zero;
        bool mMouseIsInside = false;
        GUIContent mContent;
        GUIStyle mStyle;
        GUIStyle lableStyle;
        Color bgNormal = new Color(1f, 1f, 1f, 0.5f);
        Color prefabNameColor = new Color(1f, 1f, 1f, 0.8f);

        private List<Item> _selections = new List<Item>();
        private static int _labelDefaultFontSize = 10;

        GameObject[] draggedObjects
        {
            get
            {
                if (DragAndDrop.objectReferences == null || DragAndDrop.objectReferences.Length == 0)
                    return null;

                return DragAndDrop.objectReferences.Where(x => x as GameObject).Cast<GameObject>().ToArray();
            }
            set
            {
                if (value != null)
                {
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = value;
                    draggedObjectIsOurs = true;
                }
                else DragAndDrop.AcceptDrag();
            }
        }

        bool draggedObjectIsOurs
        {
            get
            {
                object obj = DragAndDrop.GetGenericData("GeneralCompWindow");
                if (obj == null) return false;
                return (bool)obj;
            }
            set
            {
                DragAndDrop.SetGenericData("GeneralCompWindow", value);
            }
        }

        //根据存储的数据load出来通用件预览图
        void OnEnable()
        {
            menutype = EnumMenuItemType.eGeneralCompPreview;
            if (GTHelper.LoadWithBatchmode)
            {
                return;
            }

            mCellSizeX = Mathf.FloorToInt(60f * UIHelpsToolGlobalConfig.generalSetting.eScreenStdWidth / UIHelpsToolGlobalConfig.generalSetting.eScreenStdHeight * 2f);
            mSizePercent = 2f;
            mContent = new GUIContent();
            mStyle = new GUIStyle();
            mStyle.alignment = TextAnchor.MiddleCenter;
            mStyle.padding = new RectOffset(2, 2, 2, 2);
            mStyle.clipping = TextClipping.Clip;
            mStyle.wordWrap = true;
            mStyle.stretchWidth = false;
            mStyle.stretchHeight = false;
            mStyle.normal.textColor = Color.red;// //EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.5f) : new Color(0f, 0f, 0f, 0.5f);
            mStyle.normal.background = null;

            //prefab文本
            lableStyle = new GUIStyle();
            lableStyle.fontSize = 10;
            Load();
        }

        public void Load()
        {
            mSizePercent = 2f;
            mItems.Clear();
            previewItems.Sort((x, y) =>
                {
                    return x.prefabGo.name.CompareTo(y.prefabGo.name);
                });
            foreach (var item in previewItems)
            {
                AddItem(item.prefabGo);
            }
        }

        private void AddItem(GameObject go, bool isInsert = false)
        {
            string guid = GTHelper.ObjectToGUID(go);

            if (isInsert)
            {
                if (string.IsNullOrEmpty(guid))
                {
                    if (string.IsNullOrEmpty(guid)) return;
                }

                foreach (var item in previewItems)
                {
                    if (GTHelper.ObjectToGUID(go) == GTHelper.ObjectToGUID(item.prefabGo))
                    {
                        // EditorUtility.DisplayDialog("提示", "当前prefab预览已在列表中", "确定");
                        return;
                    }
                }
            }

            Item ent = new Item();
            ent.prefab = go;
            ent.guid = guid;
            GeneratePreview(ent);
            mItems.Add(ent);

            if (isInsert)
            {
                GeneralCompPreviewItem previewItem = new GeneralCompPreviewItem();
                previewItem.prefabGo = go;
                previewItems.Add(previewItem);

                previewItems.Sort((x, y) =>
                {
                    return x.prefabGo.name.CompareTo(y.prefabGo.name);
                });
                EditorUtility.SetDirty(this);
            }
        }

        private void RemoveItem(Item item)
        {
            if (item == null)
            {
                return;
            }

            mItems.Remove(item);
            foreach (var preview in previewItems)
            {
                if (GTHelper.ObjectToGUID(preview.prefabGo) == GTHelper.ObjectToGUID(item.prefab))
                {
                    previewItems.Remove(preview);
                    break;
                }
            }
            EditorUtility.SetDirty(this);
            UIHelpsToolUtils.DeletePreviewData(GTHelper.ObjectToGUID(item.prefab));
            //删除的时候停掉GUI，避免绘制的迭代器出问题
            GUIUtility.ExitGUI();
            GeneralCompWindow.mainWindow.Repaint();
        }

        public void Disable()
        {
            foreach (var item in this.mItems)
            {
                if (item.prefab != null && item.tex != null)
                {
                    item.tex = null;
                }
            }
        }

        Item FindItem(GameObject go)
        {
            for (int i = 0; i < mItems.Count; ++i)
                if (mItems[i].prefab == go)
                    return mItems[i];
            return null;
        }

        void UpdateVisual()
        {
            if (draggedObjects == null) DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            else if (draggedObjectIsOurs) DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            else DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        }

        //构建预览效果
        void GeneratePreview(Item item)
        {
            if (item == null || item.prefab == null) return;
            {
                string preview_path = UIHelpsToolConfigure.PreviewPath + "/" + item.guid + ".png";
                if (File.Exists(preview_path))
                {
                    Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(preview_path);//UIHelpsToolUtils.LoadTextureInLocal(preview_path);
                    item.tex = texture;
                }
                else
                {
                    Texture Tex = UIHelpsToolUtils.ExportBasicCompImg(item.prefab);
                    if (Tex != null)
                    {
                        item.tex = Tex;
                        // UIHelpsToolUtils.SaveTextureToPNG(Tex, preview_path);
                        AssetDatabase.ImportAsset(preview_path, ImportAssetOptions.ForceUpdate);
                    }
                }
                return;
            }
        }

        static Transform FindChild(Transform t, string startsWith)
        {
            if (t.name.StartsWith(startsWith)) return t;

            for (int i = 0, imax = t.childCount; i < imax; ++i)
            {
                Transform ch = FindChild(t.GetChild(i), startsWith);
                if (ch != null) return ch;
            }
            return null;
        }

        int GetCellUnderMouse(int spacingX, int spacingY)
        {
            Vector2 pos = Event.current.mousePosition + mPos;

            int topPadding = 2;
            int rightPadding = 4;
            int x = cellPaddingX + GeneralCompWindow.mainWindow.mMenuWidth, y = cellPaddingY + topPadding + GeneralCompWindow.mainWindow.mTopBarHeight;
            if (pos.y < y) return -1;

            float width = Screen.width - cellPaddingX + mPos.x - rightPadding;
            // width -= (int)GeneralCompWindow.mainWindow.mMenuWidth;
            float height = Screen.height - cellPaddingY + mPos.y;
            int index = 0;
            for (; ; index++)
            {
                Rect rect = new Rect(x, y, spacingX, spacingY);
                if (rect.Contains(pos)) break;

                x += spacingX;

                if (x + spacingX > width)
                {
                    if (pos.x > x) return -1;
                    y += spacingY;
                    x = cellPaddingX + GeneralCompWindow.mainWindow.mMenuWidth;
                    if (y + spacingY > height - 30)
                        return -1;
                }
            }

            return index;
        }

        private float clickTime = 0;
        private Texture deleteIcon;
        // [OnInspectorGUI]
        public void Draw()
        {
            if (GTHelper.LoadWithBatchmode)
            {
                return;
            }
            if (GeneralCompWindow.mainWindow == null)
            {
                return;
            }
            if (deleteIcon == null)
            {
                deleteIcon = AssetDatabase.LoadAssetAtPath<Texture>(UIHelpsToolConfigure.UIHelpsAssetPath + "/Res/SketchImg/UIHelpsTool_delete-icon.png");
            }
            Event currentEvent = Event.current;
            EventType type = currentEvent.type;


            int x = cellPaddingX, y = cellPaddingY;
            int panddingRight = 4;
            int width = Screen.width - cellPaddingX - panddingRight;
            width -= (int)GeneralCompWindow.mainWindow.mMenuWidth;
            int spacingX = cellSizeX + cellPaddingX;
            int spacingY = cellSizeY + cellPaddingY;

            GameObject[] draggeds = draggedObjects;
            bool isDragging = (draggeds != null);
            int indexUnderMouse = GetCellUnderMouse(spacingX, spacingY);
            if (isDragging)
            {
                foreach (var gameObject in draggeds)
                {
                    var result = FindItem(gameObject);

                    if (result != null && _selections.Contains(result) == false)
                    {
                        _selections.Add(result);
                    }
                }
            }

            bool eligibleToDrag = (currentEvent.mousePosition.y < Screen.height - 50);

            if (type == EventType.MouseDown)
            {
                mMouseIsInside = true;
                var nowTime = Time.realtimeSinceStartup;
                if (currentEvent.button == 0)
                {
                    //实现双击事件
                    if (nowTime - clickTime < 0.3f && curSelectGo != null)
                    {
                        AssetDatabase.OpenAsset(curSelectGo);
                    }
                }

                clickTime = nowTime;
            }
            else if (type == EventType.MouseDrag)
            {
                mMouseIsInside = true;

                if (indexUnderMouse != -1 && eligibleToDrag)
                {
                    if (draggedObjectIsOurs) DragAndDrop.StartDrag("GeneralCompWindow");
                    currentEvent.Use();
                }
            }
            else if (type == EventType.MouseUp)
            {
                DragAndDrop.PrepareStartDrag();
                mMouseIsInside = false;
                if (GeneralCompWindow.mainWindow)
                {
                    GeneralCompWindow.mainWindow.Repaint();
                }
            }
            else if (type == EventType.DragUpdated)
            {
                mMouseIsInside = true;
                UpdateVisual();
                currentEvent.Use();
            }
            else if (type == EventType.DragPerform)
            {
                if (draggeds != null)
                {
                    foreach (var dragged in draggeds)
                    {
                        AddItem(dragged, true);
                        ++indexUnderMouse;
                    }

                    draggeds = null;
                }
                mMouseIsInside = false;
                currentEvent.Use();
            }
            else if (type == EventType.DragExited || type == EventType.Ignore)
            {
                mMouseIsInside = false;
            }

            if (!mMouseIsInside)
            {
                _selections.Clear();
                draggeds = null;
                curSelectGo = null;
            }

            // if (indexUnderMouse == -1)
            // {
            //     curSelectGo = null;
            // }

            List<int> indices = new List<int>();
            string searchTrem = GeneralCompWindow.mainWindow.searchFile;
            for (int i = 0; i < this.mItems.Count; i++)
            {
                if (string.IsNullOrEmpty(searchTrem) || mItems[i].prefab.name.ToLower().IndexOf(searchTrem.ToLower()) > -1)
                {
                    indices.Add(i);
                }
            }


            //60是最顶上的距离
            mPos = EditorGUILayout.BeginScrollView(mPos, GUILayout.Width(Screen.width - GeneralCompWindow.mainWindow.mMenuWidth - panddingRight), GUILayout.Height(Screen.height - 60 - GeneralCompWindow.mainWindow.mBottomBarHeight));
            {
                if (indices.Count == 0)
                {
                    GUILayout.Space(10);
                    GUILayout.Label("拖拽Prefab到面板空白区域添加");
                    GUILayout.Space(5);
                    GUILayout.Label("可以一次性拖拽多个哦");
                }
                for (int i = 0; i < indices.Count; ++i)
                {
                    int index = indices[i];
                    Item ent = (index != -1) ? mItems[index] : null;
                    if (ent != null && ent.prefab == null)
                    {
                        continue;
                    }

                    Rect rect = new Rect(x, y, cellSizeX, cellSizeY);
                    Rect inner = rect;
                    inner.xMin += 2f;
                    inner.xMax -= 2f;
                    inner.yMin += 2f;
                    inner.yMax -= 2f;
                    rect.yMax -= 1f;

                    if (ent != null && ent.tex != null)
                        mContent.tooltip = ent.prefab.name;
                    else mContent.tooltip = "";

                    // if (ent == selection)
                    {
                        GUI.color = bgNormal;
                        UIHelpsToolUtils.DrawTiledTexture(inner, UIHelpsToolUtils.backdropTexture);
                    }

                    GUI.color = Color.white;
                    if (indexUnderMouse > -1 && indexUnderMouse == index)
                    {
                        if (ent != null)
                        {
                            curSelectGo = ent.prefab;
                        }
                    }
                    GUI.backgroundColor = bgNormal;

                    string caption = (ent == null) ? "" : ent.prefab.name.Replace("Control - ", "");
                    if (ent != null)
                    {
                        if (ent.tex == null)
                        {
                            GeneratePreview(ent);
                        }
                        if (ent.tex != null)
                        {
                            GUI.DrawTexture(inner, ent.tex);
                            var labelPos = new Rect(inner.x + 2, inner.y, inner.width - 4, inner.height);
                            labelPos.height = (mSizePercent > 1.8 ? lableStyle.lineHeight + 5 : lableStyle.lineHeight);
                            //控件名称位置
                            labelPos.y = inner.y + inner.height;
                            lableStyle.fontSize = (int)(_labelDefaultFontSize * Math.Min(mSizePercent, mSizePercent > 1.8 ? 1.3f : 1.0f));
                            lableStyle.alignment = TextAnchor.MiddleCenter;
                            lableStyle.fixedWidth = inner.width - 4;
                            lableStyle.fixedHeight = labelPos.height;
                            lableStyle.stretchWidth = false;
                            lableStyle.stretchHeight = false;
                            lableStyle.clipping = TextClipping.Clip;
                            lableStyle.normal.textColor = prefabNameColor;
                            {
                                GUI.Label(labelPos, ent.prefab.name, lableStyle);
                                // GUI.Label(labelPos, x + "|" + y, lableStyle);
                            }

                            if (GUI.Button(new Rect(x + 5, y + 5, 22, 22), new GUIContent(deleteIcon, "点击删除当前预览")))
                            {
                                if (EditorUtility.DisplayDialog("提示", "确定移除当前资源的预览？", "确定"))
                                {
                                    RemoveItem(ent);
                                }
                            }
                        }
                        else
                        {
                            GUI.Label(inner, caption, mStyle);
                            caption = "";
                        }

                    }

                    x += spacingX;
                    if (x + spacingX > width)
                    {
                        y += spacingY;
                        x = cellPaddingX;
                    }
                }

                GUILayout.Space(y + spacingY + 5);
            }
            EditorGUILayout.EndScrollView();

            if (eligibleToDrag && type == EventType.MouseDown && indexUnderMouse > -1)
            {
                GUIUtility.keyboardControl = 0;

                if (currentEvent.button == 0 && indexUnderMouse < indices.Count)
                {
                    int index = indices[indexUnderMouse];

                    if (index != -1 && index < mItems.Count)
                    {
                        // _selections.Add(mItems[index]);
                        //还是不开放多选吧
                        if (_selections.Count != 0)
                        {
                            _selections.Clear();
                        }
                        _selections.Add(mItems[index]);
                        Selection.activeGameObject = mItems[index].prefab;
                        curSelectGo = mItems[index].prefab;
                        // if (GeneralCompWindow.mainWindow)
                        // {
                        //     GeneralCompWindow.mainWindow.Repaint();
                        // }

                        draggedObjects = _selections.Select(item => item.prefab).ToArray();
                        draggeds = _selections.Select(item => item.prefab).ToArray();
                        currentEvent.Use();
                    }
                }
            }

        }
    }

    //预览资源细节，当前只有一个Prefab信息，还是包装成一个Item，方便将来直接扩展
    [Serializable]
    public class GeneralCompPreviewItem
    {
        //预览引用的prefab
        public GameObject prefabGo;

        //效果图
        [NonSerialized]
        public Texture tex;
    }

}