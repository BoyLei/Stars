using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SGF.UI.Framework
{
    [XLua.LuaCallCSharp]
    public class DynamicUIRoot : MonoBehaviour
    {
        //名称定义
        //费劲分离区
        private static string uniRootDef = "UniRoot";

        private static string sceneWidgetDef = "SceneWidgetRoot";

        private static string uniPageDef = "PageRoot";
        private static string uniWindowDef = "WindowRoot";

        private static string uniWidgetDef = "WidgetRoot";
        private static string uniTopWidgetDef = "TopWidgetRoot";
        private static string BlurImageDef = "BlurImage";

        //明确确定区---------------------------
        private static string EntityUIRootDef = "EntityUIRoot";//自己，相机，其他人动就会动分离单独拿出来合批不影响，而且静态里面绝对不会有
        private static string DamageUIRootDef = "DamageUIRoot";//而且他本身也有动画


        //比较好分离区域
        private static string systemMessageRootDef = "SystemMessageRoot";
        private static string specialMessageRootDef = "SpecialMessageRoot";
        private static string battleMessageRootDef = "BattleMessageRoot";
        //Tag
        public const string LOG_TAG = "DUIRoot";


        #region 定义
        //缓存
        private static GameObject _uniRoot;
        private static GameObject _sceneWidgetRoot;
        private static GameObject _specMsgRoot;
        private static GameObject _battleMsgRoot;
        private static GameObject _systemMsgRoot;
        private static GameObject _uiRoot;
        private static GameObject _uiPageRoot;

        private static GameObject _uiWidgetRoot;
        private static GameObject _uiTopWidgetRoot;

        private static Image _blurImage;
        private static GameObject _uiWindowRoot;

        private static GameObject _entityUIRoot;
        private static GameObject _damageUIRoot;


        //private static GameObject _gm;
        //public static GameObject GmRoot
        //{
        //    get
        //    {
        //        if (_gm == null)
        //        {
        //            _gm = FindTinyUIRoot("GmPanel");
        //        }
        //        return _gm;

        //    }
        //}

        public static GameObject UniRoot
        {
            get
            {
                if (_uniRoot == null)
                {
                    _uniRoot = FindTinyUIRoot(uniRootDef);
                }
                return _uniRoot;
            }
            set => _uniRoot = value;
        }

        public static GameObject SpecMsgRoot
        {
            get
            {
                if (_specMsgRoot == null)
                {
                    _specMsgRoot = FindTinyUIRoot(specialMessageRootDef);
                }
                return _specMsgRoot;
            }
            set => _specMsgRoot = value;
        }

        public static GameObject BattleMsgRoot
        {
            get
            {
                if (_battleMsgRoot == null)
                {
                    _battleMsgRoot = FindTinyUIRoot(battleMessageRootDef);
                }
                return _battleMsgRoot;
            }
            set => _battleMsgRoot = value;
        }

        public static GameObject SystemMsgRoot
        {
            get
            {
                if (_systemMsgRoot == null)
                {
                    _systemMsgRoot = FindTinyUIRoot(systemMessageRootDef);
                }
                return _systemMsgRoot;
            }
            set => _systemMsgRoot = value;
        }

        /// <summary>
        /// UIRoot根节点,UI的最Root节点
        /// </summary>
        /// <value></value>
        public static GameObject UIROOT
        {
            get
            {
                if (_uiRoot == null)
                {
                    _uiRoot = FindSubUIRoot();
                }
                return _uiRoot;
            }
        }
        public static GameObject SceneWidgetRoot
        {
            get
            {
                if (_sceneWidgetRoot == null)
                {
                    _sceneWidgetRoot = FindTinyUIRoot(sceneWidgetDef);
                }
                return _sceneWidgetRoot;
            }
        }
        public static GameObject UiPageRoot
        {
            get
            {
                if (_uiPageRoot == null)
                {
                    _uiPageRoot = FindTinyUIRoot(uniPageDef);
                }
                return _uiPageRoot;
            }
        }

        public static GameObject UiWidgetRoot
        {
            get
            {
                if (_uiWidgetRoot == null)
                {
                    _uiWidgetRoot = FindTinyUIRoot(uniWidgetDef);
                }
                return _uiWidgetRoot;
            }
        }

        public static GameObject UiTopWidgetRoot
        {
            get
            {
                if (_uiTopWidgetRoot == null)
                {
                    _uiTopWidgetRoot = FindTinyUIRoot(uniTopWidgetDef);
                }
                return _uiTopWidgetRoot;
            }
        }

        public static GameObject UiWindowRoot
        {
            get
            {
                if (_uiWindowRoot == null)
                {
                    _uiWindowRoot = FindTinyUIRoot(uniWindowDef);
                }
                return _uiWindowRoot;
            }
        }

        public static Image BlurImage
        {
            get
            {
                if (_blurImage == null)
                {
                    var obj = FindTinyUIRoot(BlurImageDef);
                    if (obj != null)
                    {
                        _blurImage = obj.GetComponent<Image>();
                    }
                }
                return _blurImage;
            }
        }

        public static GameObject EntityUIRoot
        {
            get
            {
                if (_entityUIRoot == null)
                {
                    _entityUIRoot = FindTinyUIRoot(EntityUIRootDef);
                }
                return _entityUIRoot;
            }
        }

        public static GameObject DamageUIRoot
        {
            get
            {
                if (_damageUIRoot == null)
                {
                    _damageUIRoot = FindTinyUIRoot(DamageUIRootDef);
                }
                return _damageUIRoot;
            }
        }

        #endregion

        public static Dictionary<int, Transform> DamageUILayerDic = new();

        /// <summary>
        /// 【通过人的远近，给一个人的伤害】？，通过rq对应parent分组，
        /// </summary>
        /// <param name="order"></param>
        /// <param name="tr"></param>
        public static void SetDamagePartnt(int order, Transform tr)
        {
            if (DamageUILayerDic.TryGetValue(order, out var parent))
            {
                tr.SetParent(parent);
            }
            else
            {
                GameObject layerRoot = new($"{order}");
                layerRoot.transform.SetParent(DamageUIRoot.transform, false);
                layerRoot.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                layerRoot.transform.SetSiblingIndex(order);
                UIUtilsFrameWork.ChangeLayer(layerRoot.transform, "UI");
                DamageUILayerDic.Add(order, layerRoot.transform);

                tr.SetParent(layerRoot.transform, false);
            }
            tr.SetAsLastSibling();
        }

        public static void ReleaseDamageRoot()
        {
            foreach (var item in DamageUILayerDic)
            {
                if (item.Value != null)
                {
                    GameObject.Destroy(item.Value.gameObject);
                }
            }
            DamageUILayerDic.Clear();
        }

        /// <summary>
        /// 从UIRoot下通过类型寻找一个组件对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Find<T>(bool isTopWidget = false) where T : UIPanel
        {
            GameObject root = GetRoot<T>(isTopWidget);

            string name = typeof(T).Name;
            GameObject obj = Find(name, root);
            if (obj != null)
            {
                return obj.GetComponent<T>();
            }


            return null;
        }

        /// <summary>
        /// 从UIRoot下通过名字&类型寻找一个组件对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T Find<T>(string name, bool isTopWidget = false) where T : UIPanel
        {
            GameObject root = GetRoot<T>(isTopWidget);
            GameObject obj = Find(name, root);
            if (obj != null)
            {
                return obj.GetComponent<T>();

            }

            return null;
        }

        /// <summary>
        /// 在root下通过名字寻找一个GameObject对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject Find(string name, GameObject root)
        {
            Transform obj = null;
            if (root != null)
            {
                //obj = root.transform.Find(name);
                //先，不用快找。先需要确定
                foreach (var item in root.transform)
                {
                    Transform trans = (Transform)item;
                    if (trans.name == name)
                    {
                        obj = trans.transform;
                    }
                }
            }

            if (obj != null)
            {
                return obj.gameObject;
            }

            //Debuger.LogError(LOG_TAG, "Find() UI:{0} 不存在！", name);
            return null;
        }


        public static GameObject FindSubUIRoot()
        {
            GameObject root = GameObject.Find("DUIRoot");
            if (root != null && root.GetComponent<DynamicUIRoot>() != null)
            {
                return root;
            }
            Debuger.LogError(LOG_TAG, "FindUIRoot() UIRoot Is Not Exist!!!");
            return null;
        }

        /// <summary>
        /// 当前场景中寻找UIRoot对象
        /// 运行时不关心节点，通过【静态函数】。查找【根节点】的【运行时脚本】
        /// </summary>
        /// <returns></returns>
        public static GameObject FindTinyUIRoot(string name)
        {
            GameObject tinyRoot = UIROOT.transform.Find(name).gameObject;//子节点
            if (tinyRoot != null)
            {
                return tinyRoot;
            }
            return null;
        }

        public static GameObject GetRoot<T>(bool isTopWidget = false) where T : UIPanel
        {
            GameObject gob;
            switch (typeof(T).Name)
            {
                case "UIWidget":
                    if (isTopWidget)
                    {
                        gob = UiTopWidgetRoot;
                    }
                    else
                    {
                        gob = UiWidgetRoot;
                    }
                    break;
                case "UIWindow":
                    gob = UiWindowRoot;
                    break;
                case "UIPage":
                    gob = UiPageRoot;
                    break;
                case "UIPanel":
                    gob = UniRoot;
                    break;
                default:
                    gob = UniRoot;
                    break;
            }
            return gob;
        }

        /// <summary>
        /// 当一个UIPage/UIWindow/UIWidget添加到UIRoot下面
        /// </summary>
        /// <param name="child"></param>
        public static void AddChild<T>(UIPanel child, Transform parent = null, bool isTopWidget = false) where T : UIPanel
        {
            GameObject root = parent == null ? null : parent.gameObject;
            if (root == null)
            {
                root = GetRoot<T>(isTopWidget);
            }

            if (root == null || child == null)
            {
                return;
            }


            child.transform.SetParent(root.transform, false);
            return;
        }

    }
}
