
using UnityEngine;

namespace SGF.UI.Framework
{
    public class DUIFXRoot : MonoBehaviour
    {
        public const string LOG_TAG = "DUIFXRoot";
        private static GameObject _root;
        private static string _UIFXRoot = "DUIFXRoot";
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

        public static GameObject FindUIFxRoot()
        {
            GameObject root = GameObject.Find(_UIFXRoot);//.Find("UIRoot");
            if (root != null && root.GetComponent<DUIFXRoot>() != null)
            {
                return root;
            }
            return null;
        }


    }
}
