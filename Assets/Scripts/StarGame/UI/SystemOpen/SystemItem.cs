using Sirenix.OdinInspector;
using Sirenix.Utilities;
using StarProjectDef;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace StarProject.Service.SystemOpen
{
    [XLua.LuaCallCSharp]
    public class SystemItem : MonoBehaviour
    {
        /// <summary>
        /// 系统类型
        /// </summary>
        [ReadOnly]
        [OnInspectorInit("RefreshSystemType")]
        public SystemOpenType systemType;

        [LabelText("系统类型枚举的名字")]
        [ValueDropdown("GetSystemTypeName")]
        [OnValueChanged("RefreshSystemType")]
        public string systemTypeName;

        public void RefreshSystemType()
        {
            systemType = SystemOpenTypeUtils.GetSystemOpenType(systemTypeName);
        }
        /// <summary>
        /// 系统按钮, 系统开启后，开启这个系统按钮
        /// </summary>
        public GameObject SystemNode;

        /// <summary>
        /// 高亮 节点
        /// </summary>
        public GameObject HighLightNode;

        /// <summary>
        /// 反转节点的显示
        /// </summary>
        [LabelText("反转节点的显示")]
        public bool Reverse = false;

        /// <summary>
        /// 是否刷新 systemNode 的 active 属性. 默认都会去 刷新节点的 active
        /// </summary>
        [LabelText("是否开关节点")]
        public bool IsRefreshSystemNodeActive = true;
        [LabelText("节点IsOpen")]
        public bool IsOpen = true;


        public Action<bool> ActionOnRefreshSystemNodeActive;
        private bool TrueFlag => Reverse ^ true;
        private bool FalseFlag => Reverse ^ false;

        /// <summary>
        /// 是否 置灰, 如果置灰, 不active SystemNode 节点, 只做 灰度变化
        /// </summary>
        [LabelText("置灰[不再显隐节点]")]
        public bool UseGray = false;

        /// <summary>
        /// 其他 跟随的 需要显示的节点。 有时候 节点 需要置灰 加 锁。 那就 勾选 UseGray 同时 拖入 锁这个节点.
        /// </summary>
        [LabelText("跟随显示的节点")]
        public List<GameObject> FollowShowNodes;

        /// <summary>
        /// 跟随的 需要反转显示的节点. 比如这个系统入口节点需要显示的时候, FollowShowNodes 会跟随显示. FollowReverseShowNodes 会跟随关闭
        /// </summary>
        [LabelText("反转显示的节点")]
        public List<GameObject> FollowReverseShowNodes;

        [LabelText("是否tips未开启")]
        public bool ShowTips = true;

        private Action OnSystemClick;

        private Dictionary<MaskableGraphic, Color> MaskableGraphics;

        private JButton jButton;


#if UNITY_EDITOR
        [SerializeField]
        private bool Trigger;

        private void OnValidate()
        {
            // 测试 系统 item 开关的逻辑
            if (Trigger)
            {
                // ActionOnRefreshSystemNodeActive?.Invoke(true);
                SystemOpenManager.Instance.TriggerRefresh();
            }
        }
#endif

        private void Awake()
        {
            /// 2024/6/27
            /// fixed by DL: 修复prefab上系统类型与实际枚举不一致的问题
            /// 
            /// note:
            ///     现在的枚举改为导表工具自动导，所以枚举id 可能会变，但是 id 存在于已经序列化的prefab中.
            ///     所以 在Awake 的时候, 先刷一次， 不再依赖于 prefab 上的id
            RefreshSystemType();

            SystemOpenManager.Instance.RegSystemItemNode(this);

            GlobalEvent.OnSystemOpen.AddListener(OnSystemOpen);
            GlobalEvent.OnRefreshSystemOpen.AddListener(OnRefreshSystemOpen);
            GlobalEvent.OnSystemOpenTipFly.AddListener(OnSystemOpenTipFly);

            if (SystemNode != null)
            {

                Button btn = SystemNode.GetComponent<Button>();
                // 如果 这个节点 是个按钮，那就 自动挂这个按钮的点击事件
                if (btn)
                {
                    btn.onClick.AddListener(OnBtnClick);
                }

                jButton = SystemNode.GetComponent<JButton>();



                if (jButton != null)
                {
                    jButton.OnClick += OnJBtnClick;
                }
                else
                {
                    // jbt 有自己的置灰会逻辑
                    // 如果 需要置灰 并且没有 jbutton的时候，就自己去置灰(jbtn 有置灰逻辑)
                    if (UseGray)
                    {
                        MaskableGraphics = new Dictionary<MaskableGraphic, Color>();
                        MaskableGraphics.Clear();
                        var graphics = SystemNode.GetComponentsInChildren<MaskableGraphic>();
                        if (graphics != null && graphics.Length > 0)
                        {
                            foreach (var graphic in graphics)
                            {
                                MaskableGraphics.Add(graphic, graphic.color);
                            }
                        }
                    }
                }
            }

            MarkDirty();


        }

        private void OnEnable()
        {
            MarkDirty();
        }
        private void OnDestroy()
        {
            SystemOpenManager.Instance.UnRegSystemItemNode(this);
            GlobalEvent.OnSystemOpen.RemoveListener(OnSystemOpen);
        }

        public void InitWithType(SystemOpenType systemOpenType)
        {
            SystemOpenManager.Instance.UnRegSystemItemNode(this);
            // 先设置类型 , 再注册这个 类型 的节点
            systemTypeName = systemOpenType.ToString();
            RefreshSystemType();
            SystemOpenManager.Instance.RegSystemItemNode(this);

            MarkDirty();
        }

        public void OnSystemOpen(SystemOpenType systemOpenType, bool isOpen)
        {
            if (systemType != systemOpenType)
            {
                return;
            }
            if (systemType == SystemOpenType.None)
            {
                return;
            }

            MarkDirty();
        }

        public void OnRefreshSystemOpen(int v)
        {

            MarkDirty();
        }

        private bool dirtyFlag = false;

        public void MarkDirty()
        {
            if (dirtyFlag)
            {
                return;
            }
            dirtyFlag = true;
            RefreshUI();
            dirtyFlag = false;
        }

        private void RefreshUI()
        {
            if (GetComponent<LayoutElement>() != null)
            {
                GetComponent<LayoutElement>().minWidth += 0.000001f;
            }



            if (systemType == SystemOpenType.None)
            {
                return;
            }
            // if (systemType == SystemOpenType.Playmode_Daily)
            // {
            //     Debug.Log("[system] 刷新每日玩法");
            // }


            SystemItemData systemItemData = SystemOpenManager.Instance.GetOpenedSystemItemData(systemType);

            if (systemItemData == null)
            {

                RefreshSystemNode(FalseFlag);
                return;
            }


            // if (systemType == SystemOpenType.Playmode_Daily)
            // {
            //     Debug.Log($"[system] 刷新每日玩法 : {FalseFlag} , isOpen: {systemItemData.IsOpen}");
            // }

            RefreshSystemNode(TrueFlag);


            bool needHightLight = systemItemData.GetNeedHightLight();

            RefreshHightLight(needHightLight);
        }

        public int GetUnlockRoleLevel()
        {
            if (systemType == SystemOpenType.None)
            {
                return 0;
            }

            SystemItemData systemItemData = SystemOpenManager.Instance.GetSystemItemData(systemType);
            if (systemItemData == null)
            {
                return 0;
            }

            return systemItemData.RoleLevel;
        }

        private void RefreshSystemNode(bool isShow)
        {
            // 按钮都没有 ，那就不刷
            if (SystemNode == null)
            {
                return;
            }

            IsOpen = isShow;


            // 如果 勾选了 不刷新节点的 active, 那就 不处理 ui刷新逻辑
            if (!IsRefreshSystemNodeActive)
            {
                // 节点不刷新
            }
            else
            {
                // 如果有开启 数据, 那就显示这个 功能入口
                SystemNode.SetActive(isShow || UseGray);
            }


            // 开启 跟随显示的
            FollowShowNodes.ForEach((node) =>
            {
                node.SetActive(isShow);
            });

            FollowReverseShowNodes.ForEach((node) =>
            {
                node.SetActive(!isShow);
            });

            RefreshGrey(!isShow);

            // 不管刷不刷新 节点的active, 都会通过 action 通知外面
            ActionOnRefreshSystemNodeActive?.Invoke(isShow);
        }

        private void RefreshHightLight(bool needHightLight)
        {
            if (HighLightNode == null)
            {
                return;
            }
            HighLightNode.SetActive(needHightLight);
        }


        private void RefreshGrey(bool isGrey)
        {
            if (!UseGray)
            {
                return;
            }

            if (jButton != null)
            {
                // jButton.ForbidClick = isGrey;
                jButton.UnClick = isGrey;
                return;
            }

            if (MaskableGraphics.Count > 0)
            {
                foreach (var item in MaskableGraphics)
                {
                    float a = isGrey ? 0.3f : item.Value.a;
                    item.Key.color = new Color(item.Value.r, item.Value.g, item.Value.b, a);
                }
            }

        }

        private void OnJBtnClick(GameObject go)
        {
            OnBtnClick();
        }

        public void OnBtnClick()
        {
            // 如果没有开放并且 勾选了 弹tips 未开放提示,就弹出 tips 弹窗
            if (ShowTips && !IsOpen)
            {
                Frame.Util.ShowMessage(SystemOpenManager.Instance.GetSystemNoOpenTips(systemType));
                return;
            }

            var systemItemData = SystemOpenManager.Instance.GetOpenedSystemItemData(systemType);
            if (systemItemData == null)
            {
                return;
            }



            // 系统开放 按钮 被点击的 action
            OnSystemClick?.Invoke();

            bool needHightLight = systemItemData.GetNeedHightLight();
            if (!needHightLight)
            {
                return;
            }

            systemItemData.MarkHighLight();

            RefreshHightLight(false);
        }

        private void OnSystemOpenTipFly(SystemOpenType systemOpenType, Vector3 startPos)
        {
            if (systemType != systemOpenType)
            {
                return;
            }
            if (systemType == SystemOpenType.None)
            {
                return;
            }

            SGF.Debuger.Log($"[SystemItem] open : {systemOpenType} , startPos: {startPos} ---> {SystemNode.transform.position}");

            MarkDirty();

        }

        public void RegisterSystemClick(Action cb)
        {
            if (cb == null)
            {
                return;
            }
            OnSystemClick += cb;
        }

        public void UnRegisterSystemClick(Action cb)
        {
            if (cb == null)
            {
                return;
            }
            OnSystemClick -= cb;
        }


        private static ValueDropdownList<string> SystemOpenTypeKeys = new() { };
        private static bool _initSystemOpenTypeKeys = false;
        public static IEnumerable GetSystemTypeName()
        {
            if (!_initSystemOpenTypeKeys)
            {
                _initSystemOpenTypeKeys = true;
                Enum.GetNames(typeof(SystemOpenType)).ForEach((item) =>
                {
                    SystemOpenTypeKeys.Add(item, item);
                });
            }

            return SystemOpenTypeKeys;
        }

    }

    public static class SystemOpenTypeUtils
    {


        private static Dictionary<string, SystemOpenType> systemOpenTypes = new();

        public static SystemOpenType GetSystemOpenType(string key)
        {
            if (systemOpenTypes.ContainsKey(key))
            {
                return systemOpenTypes[key];
            }

            if (Enum.TryParse<SystemOpenType>(key, out SystemOpenType v))
            {
                systemOpenTypes.Add(key, v);
            }
            else
            {
                systemOpenTypes.Add(key, SystemOpenType.None);
            }
            return systemOpenTypes[key];
        }
    }
}


