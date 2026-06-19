
using UnityEngine;

namespace SGF.UI.Framework
{
    public class UI3DWorld : MonoBehaviour
    {
        public const string LOG_TAG = "UI3DWorld";
        private static GameObject _root;
        private static string _UI3DWorld = "UI_3D_World";
        public static GameObject Root
        {
            get
            {
                if (_root == null)
                {
                    _root = FindUI3DRoot();
                }
                return _root;
            }
            set => _root = value;
        }


        private static GameObject _UI_3D_Front;
        public static GameObject UI_3D_Front
        {
            get
            {
                if (_UI_3D_Front == null)
                {
                    if (Root != null)
                    {
                        _UI_3D_Front = Root.transform.Find("UI_3D_Front").gameObject;
                    }
                }
                return _UI_3D_Front;
            }
            set => _UI_3D_Front = value;
        }


        private static GameObject _UI_3D_Back;
        public static GameObject UI_3D_Back
        {
            get
            {
                if (_UI_3D_Back == null)
                {
                    if (Root != null)
                    {
                        _UI_3D_Back = Root.transform.Find("UI_3D_Back").gameObject;
                    }
                }
                return _UI_3D_Back;
            }
            set => _UI_3D_Back = value;
        }

        private static Transform _UIRawRoot;
        public static Transform UIRawRoot
        {
            get
            {
                if (_UIRawRoot == null)
                {
                    if (Root != null)
                    {
                        _UIRawRoot = Root.transform.parent.parent.parent.Find("DynamicCamRoot/BCameraRoot/SpecRenderCamera/RenderRoot");
                    }
                }
                return _UIRawRoot;
            }
            set => _UIRawRoot = value;
        }

        //缺少一个功能就是ui盖前面就再来一个canvas或者renderqueue
        //private void Awake()
        //{
        //    _root = gameObject;
        //}

        public void Clear() 
        {
            if (UI_3D_Front != null)
            {
                for (int i = UI_3D_Front.transform.childCount; i >= 0; i--)
                {
                    Destroy(UI_3D_Front.transform.GetChild(i).gameObject);
                }
            }
            if (UI_3D_Back != null)
            {
                for (int i = UI_3D_Back.transform.childCount; i >= 0; i--)
                {
                    Destroy(UI_3D_Back.transform.GetChild(i).gameObject);
                }
            }
            if (UIRawRoot != null)
            {
                for (int i = UIRawRoot.transform.childCount; i >= 0; i--)
                {
                    Destroy(UIRawRoot.transform.GetChild(i).gameObject);
                }
            }

            UI3DWorld.Root.transform.localEulerAngles = Vector3.zero;
            UI3DWorld.UIRawRoot.transform.localEulerAngles = Vector3.zero;

        }

        /// <summary>
        /// 当前场景中寻找UIRoot对象
        /// 运行时不关心节点，通过【静态函数】。查找【根节点】的【运行时脚本】
        /// </summary>
        /// <returns></returns>
        public static GameObject FindUI3DRoot()
        {
            GameObject root = GameObject.Find(_UI3DWorld);//.Find("UIRoot");
            if (root != null && root.GetComponent<UI3DWorld>() != null)
            {
                return root;
            }
            return null;
        }



        
    }
}
