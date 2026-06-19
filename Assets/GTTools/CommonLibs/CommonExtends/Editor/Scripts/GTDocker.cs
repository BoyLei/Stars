/*
 * @Description: 窗口绑定，GameTech开发人员维护，各项目组谨慎修改
 */
#if UNITY_EDITOR
namespace GameTechTools.CommonLibs.CommonExtends
{
    using System;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    public static class GTDocker
    {

        #region Reflection Types
        private class _EditorWindow
        {
            private EditorWindow instance;
            private Type type;

            public _EditorWindow(EditorWindow instance)
            {
                this.instance = instance;
                type = instance.GetType();
            }

            public object m_Parent
            {
                get
                {
                    var field = type.GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic);
                    return field.GetValue(instance);
                }
            }
        }

        private class _DockArea
        {
            private object instance;
            private Type type;

            public _DockArea(object instance)
            {
                this.instance = instance;
                type = instance.GetType();
            }

            public object window
            {
                get
                {
                    var property = type.GetProperty("window", BindingFlags.Instance | BindingFlags.Public);
                    return property.GetValue(instance, null);
                }
            }

            public object s_OriginalDragSource
            {
                set
                {
                    var field = type.GetField("s_OriginalDragSource", BindingFlags.Static | BindingFlags.NonPublic);
                    field.SetValue(null, value);
                }
            }
        }

        private class _ContainerWindow
        {
            private object instance;
            private Type type;

            public _ContainerWindow(object instance)
            {
                this.instance = instance;
                type = instance.GetType();
            }


            public object rootSplitView
            {
                get
                {
                    var property = type.GetProperty("rootSplitView", BindingFlags.Instance | BindingFlags.Public);
                    return property.GetValue(instance, null);
                }
            }
        }

        private class _SplitView
        {
            private object instance;
            private Type type;

            public _SplitView(object instance)
            {
                this.instance = instance;
                type = instance.GetType();
            }

            public object DragOver(EditorWindow child, Vector2 screenPoint)
            {
                var method = type.GetMethod("DragOver", BindingFlags.Instance | BindingFlags.Public);
                return method.Invoke(instance, new object[] { child, screenPoint });
            }

            public void PerformDrop(EditorWindow child, object dropInfo, Vector2 screenPoint)
            {
                var method = type.GetMethod("PerformDrop", BindingFlags.Instance | BindingFlags.Public);
                method.Invoke(instance, new object[] { child, dropInfo, screenPoint });
            }

            public void SetupRectsFromSplitter(int[] realSizes, int[] minSizes, int[] maxSizes)
            {
                var t = System.Type.GetType("UnityEditor.SplitterState, UnityEditor");
                var c = t.GetConstructor(new System.Type[] { typeof(int[]), typeof(int[]), typeof(int[]) });
                object splitterState = c.Invoke(new object[] { realSizes, minSizes, maxSizes });
                splitState = splitterState;
                var method = type.GetMethod("SetupRectsFromSplitter", BindingFlags.Instance | BindingFlags.NonPublic);
                method.Invoke(instance, null);
            }

            private object splitState
            {
                set
                {
                    var field = type.GetField("splitState", BindingFlags.Instance | BindingFlags.NonPublic);
                    field.SetValue(instance, value);
                }
            }
        }
        #endregion

        public enum DockPosition
        {
            Left,
            Top,
            Right,
            Bottom
        }

        /// <summary>
        /// Docks the second window to the first window at the given position
        /// </summary>
        public static void Dock(this EditorWindow wnd, Vector2 parentPos, EditorWindow other, DockPosition position, int total, float percent)
        {
            var mousePosition = GetFakeMousePosition(wnd, parentPos, position);

            var parent = new _EditorWindow(wnd);
            var child = new _EditorWindow(other);
            var dockArea = new _DockArea(parent.m_Parent);
            var containerWindow = new _ContainerWindow(dockArea.window);
            var splitView = new _SplitView(containerWindow.rootSplitView);
            var dropInfo = splitView.DragOver(other, mousePosition);
            dockArea.s_OriginalDragSource = child.m_Parent;
            splitView.PerformDrop(other, dropInfo, mousePosition);
            if (percent <= 0 || percent >= 1)
                percent = 0.5f;
            if (total < 200 || total > 4000)
            {
                if (position == DockPosition.Left || position == DockPosition.Right)
                    total = Screen.currentResolution.width;
                else
                    total = Screen.currentResolution.height;
            }
            int[] realSizes = new int[2];
            int winSize = Mathf.RoundToInt(total * percent);
            if (winSize < 100)
                winSize = 100;
            if (position == DockPosition.Right || position == DockPosition.Bottom)
            {
                realSizes[0] = winSize;
                realSizes[1] = total - winSize;
            }
            else
            {
                realSizes[0] = total - winSize;
                realSizes[1] = winSize;
            }
            splitView.SetupRectsFromSplitter(realSizes, new int[] { 100, 100 }, new int[] { 4000, 4000 });
        }

        /// <summary>
        /// Docks the second window to the first window at the given position
        /// </summary>
        public static void Dock(this EditorWindow wnd, EditorWindow other, DockPosition position, int total, float percent)
        {
            var mousePosition = GetFakeMousePosition(wnd, position);

            var parent = new _EditorWindow(wnd);
            var child = new _EditorWindow(other);
            var dockArea = new _DockArea(parent.m_Parent);
            var containerWindow = new _ContainerWindow(dockArea.window);
            var splitView = new _SplitView(containerWindow.rootSplitView);
            var dropInfo = splitView.DragOver(other, mousePosition);
            dockArea.s_OriginalDragSource = child.m_Parent;
            splitView.PerformDrop(other, dropInfo, mousePosition);
            if (percent <= 0 || percent >= 1)
                percent = 0.5f;
            if (total < 200 || total > 4000)
            {
                if (position == DockPosition.Left || position == DockPosition.Right)
                    total = Screen.currentResolution.width;
                else
                    total = Screen.currentResolution.height;
            }
            int[] realSizes = new int[2];
            int winSize = Mathf.RoundToInt(total * percent);
            if (winSize < 100)
                winSize = 100;
            if (position == DockPosition.Right || position == DockPosition.Bottom)
            {
                realSizes[0] = winSize;
                realSizes[1] = total - winSize;
            }
            else
            {
                realSizes[0] = total - winSize;
                realSizes[1] = winSize;
            }
            splitView.SetupRectsFromSplitter(realSizes, new int[] { 100, 100 }, new int[] { 4000, 4000 });
        }

        private static Vector2 GetFakeMousePosition(EditorWindow wnd, Vector2 parentPos, DockPosition position)
        {
            Vector2 mousePosition = Vector2.zero;
            // The 20 is required to make the docking work.
            // Smaller values might not work when faking the mouse position.
            switch (position)
            {
                case DockPosition.Left:
                    mousePosition = new Vector2(20, wnd.position.size.y / 2);
                    break;
                case DockPosition.Top:
                    mousePosition = new Vector2(wnd.position.size.x / 2, 20);
                    break;
                case DockPosition.Right:
                    mousePosition = new Vector2(wnd.position.size.x - 20, wnd.position.size.y / 2);
                    break;
                case DockPosition.Bottom:
                    mousePosition = new Vector2(wnd.position.size.x / 2, wnd.position.size.y - 20);
                    break;
            }
            // return new Vector2(wnd.position.x + mousePosition.x, wnd.position.y + mousePosition.y);
            return new Vector2(parentPos.x + mousePosition.x, parentPos.y + mousePosition.y);
        }

        private static Vector2 GetFakeMousePosition(EditorWindow wnd, DockPosition position)
        {
            Vector2 mousePosition = Vector2.zero;

            // The 20 is required to make the docking work.
            // Smaller values might not work when faking the mouse position.
            switch (position)
            {
                case DockPosition.Left:
                    mousePosition = new Vector2(20, wnd.position.size.y / 2);
                    break;
                case DockPosition.Top:
                    mousePosition = new Vector2(wnd.position.size.x / 2, 20);
                    break;
                case DockPosition.Right:
                    mousePosition = new Vector2(wnd.position.size.x - 20, wnd.position.size.y / 2);
                    break;
                case DockPosition.Bottom:
                    mousePosition = new Vector2(wnd.position.size.x / 2, wnd.position.size.y - 20);
                    break;
            }

            return new Vector2(wnd.position.x + mousePosition.x, wnd.position.y + mousePosition.y);
        }
    }

}
#endif