
using UnityEngine;

namespace SGF.UI.Framework
{
    public class UIFXRoot : MonoBehaviour
    {
        public const string LOG_TAG = "UIFXRoot";
        private static GameObject _root;
        private static string _UIFXRoot = "UIFXRoot";
        public static GameObject Root
        {
            get
            {
                if (_root == null)
                {
                    _root = FindUIFxRoot();
                }
                return _root;
            }
            set => _root = value;
        }


        private static GameObject _uiNttroot;
        public static GameObject UINttRoot
        {
            get
            {
                if (_uiNttroot == null)
                {
                    if (Root != null)
                    {
                        _uiNttroot = Root.transform.Find("NttFx").gameObject;
                    }
                }
                return _uiNttroot;
            }
            set => _uiNttroot = value;
        }


        private static GameObject _uiNormalFxRoot;
        public static GameObject UINormalFxRoot
        {
            get
            {
                if (_uiNormalFxRoot == null)
                {
                    if (Root != null)
                    {
                        _uiNormalFxRoot = Root.transform.Find("NormalFx").gameObject;
                    }
                }
                return _uiNormalFxRoot;
            }
            set => _uiNormalFxRoot = value;
        }
        //private void Awake()
        //{
        //    _root = gameObject;
        //}



        /// <summary>
        /// 当前场景中寻找UIRoot对象
        /// 运行时不关心节点，通过【静态函数】。查找【根节点】的【运行时脚本】
        /// </summary>
        /// <returns></returns>
        public static GameObject FindUIFxRoot()
        {
            GameObject root = GameObject.Find(_UIFXRoot);//.Find("UIRoot");
            if (root != null && root.GetComponent<UIFXRoot>() != null)
            {
                return root;
            }
            return null;
        }



        ///// <summary>
        ///// 从UIRoot下通过类型寻找一个组件对象
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <returns></returns>
        //public static T Find<T>() where T : MonoBehaviour
        //{
        //    string name = typeof(T).Name;
        //    GameObject obj = Find(name);
        //    if (obj != null)
        //    {
        //        return obj.GetComponent<T>();
        //    }


        //    return null;
        //}


        ///// <summary>
        ///// 从UIRoot下通过名字&类型寻找一个组件对象
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="name"></param>
        ///// <returns></returns>
        //public static T Find<T>(string name) where T : MonoBehaviour
        //{
        //    GameObject obj = Find(name);
        //    if (obj != null)
        //    {
        //        return obj.GetComponent<T>();

        //    }

        //    return null;
        //}


        ///// <summary>
        ///// 当一个UIPage/UIWindow/UIWidget添加到UIRoot下面
        ///// </summary>
        ///// <param name="child"></param>
        //public static void AddChild(UIPanel child)
        //{
        //    GameObject root = Root;
        //    if (root == null || child == null)
        //    {
        //        return;
        //    }


        //    child.transform.SetParent(root.transform, false);
        //    return;
        //}


        ///// <summary>
        ///// 在UIRoot下通过名字寻找一个GameObject对象
        ///// </summary>
        ///// <param name="name"></param>
        ///// <returns></returns>
        //public static GameObject Find(string name)
        //{
        //    Transform obj = null;
        //    GameObject root = Root;
        //    if (root != null)
        //    {
        //        //obj = root.transform.Find(name);
        //        //先，不用快找。先需要确定
        //        foreach (var item in root.transform)
        //        {
        //            Transform trans = (Transform)item;
        //            if (trans.name == name)
        //            {
        //                obj = trans.transform;
        //            }
        //        }
        //    }

        //    if (obj != null)
        //    {
        //        return obj.gameObject;
        //    }

        //    //Debuger.LogError(LOG_TAG, "Find() UI:{0} 不存在！", name);
        //    return null;
        //}
    }
}
