using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EditorModeTest
{
    public class EditorMode
    {
        public static bool IsEditorMode
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorPrefs.GetBool("LocalPlay");
#else
        return false;
#endif
            }

        }
        private static EditorMode m_instance = null;
        public static EditorMode Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new EditorMode();

                }

                return m_instance;
            }
        }

        public LocalServer localServer = null;

        public EditorMode()
        {
            // 创建 一个本地服务器
            localServer = new LocalServer();
        }

    }
}

