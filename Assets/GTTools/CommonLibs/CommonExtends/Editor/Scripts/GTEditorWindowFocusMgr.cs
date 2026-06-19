/*
 * @Description: Editor 界面 Focus焦点控制管理器，GameTech开发人员维护，各项目组谨慎修改
 */
#if UNITY_EDITOR
namespace GameTechTools.CommonLibs.CommonExtends
{
    using System.Collections.Generic;
    using UnityEditor;
    /// <summary>
    /// Editor 界面 Focus焦点控制管理器
    /// </summary>
    public class GTEditorWindowFocusMgr : GTSingleBase<GTEditorWindowFocusMgr>
    {
        private Dictionary<int, Stack<EditorWindow>> windowList = new Dictionary<int, Stack<EditorWindow>>();
        //private Stack<EditorWindow> windowList = new Stack<EditorWindow>();

        public void AddExpressionWindow(EditorWindow window)
        {
            PushEditorWindow(window);
        }

        public void PushEditorWindow(EditorWindow window, int groupId = 0)
        {
            if (!windowList.ContainsKey(groupId))
            {
                windowList.Add(groupId, new Stack<EditorWindow>());
            }
            if (!windowList[groupId].Contains(window))
            {
                windowList[groupId].Push(window);
                FoucusWindow(null, groupId);
            }
        }

        public void PopEditorWindow(EditorWindow window, int groupId = 0)
        {
            if (windowList.ContainsKey(groupId))
            {
                if (windowList[groupId].Contains(window))
                {
                    windowList[groupId].Pop();
                    FoucusWindow(null, groupId);
                }
            }
        }

        public void FoucusWindow(EditorWindow window = null, int groupId = 0)
        {
            if (window != null && windowList.ContainsKey(groupId) && windowList[groupId].Count > 0 && windowList[groupId].Peek() == window)
            {
                //D.Log("FoucusWindow return!");
                return;
            }
            if (windowList.ContainsKey(groupId) && windowList[groupId].Count > 0)
            {
                windowList[groupId].Peek().Focus();
            }
        }

        public void ClearAllWindow()
        {
            foreach (var item in windowList)
            {
                item.Value.Clear();
            }
        }

        /// <summary>
        /// 关闭所有界面，并清理WindowList缓存
        /// </summary>
        public void DestoryAllWindow()
        {
            foreach (var item in windowList)
            {
                if (item.Value != null)
                {
                    foreach (var window in item.Value)
                    {
                        if (window != null)
                            window.Close();
                    }
                }
            }
            windowList.Clear();
        }
    }
}
#endif