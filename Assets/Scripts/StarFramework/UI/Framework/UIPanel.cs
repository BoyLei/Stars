using Animancer;
using Sirenix.OdinInspector;
using StarProject;
using StarProject.OffLine;
using StarProject.Service.AtlasManager;
using StarProjectDef;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XLua;

namespace SGF.UI.Framework
{
    [LuaCallCSharp]
    public enum E_UI_TYPE
    {
        Panel,
        Widget,
        Window,

        Page,
    }

    [LuaCallCSharp]
    public abstract class UIPanel : MonoBehaviour
    {
        [XLua.BlackList]
        public IEnumerable uiSeatType = new ValueDropdownList<UISeatType>()
        {
            {"空",UISeatType.None},
            {"上",UISeatType.Top},
            {"下",UISeatType.Down},
            {"左",UISeatType.Left},
            {"右",UISeatType.Right},
            {"全屏",UISeatType.Full},
        };

        [XLua.BlackList]
        public IEnumerable _uiSeatType()
        {
            return uiSeatType;
        }

        [XLua.BlackList]
        public IEnumerable uiQueuePriorityType = new ValueDropdownList<UIQueuePriorityType>()
        {
            {"空",UIQueuePriorityType.None},
            {"阻断剧情",UIQueuePriorityType.BlockingPlot},
            {"全屏功能",UIQueuePriorityType.Full},
            {"按钮",UIQueuePriorityType.Button},
            {"玩法信息",UIQueuePriorityType.PlayGmae},
            {"短期信息",UIQueuePriorityType.ShortMsg},
            {"获取荣耀提示",UIQueuePriorityType.GetTips},
            {"常驻常规信息",UIQueuePriorityType.PermanentTips},
            {"非阻断剧情",UIQueuePriorityType.NotBlockingPlot},
        };

        [XLua.BlackList]
        public IEnumerable _uiQueuePriorityType()
        {
            return uiQueuePriorityType;
        }

        public List<AnimationClip> Clips = new();

        private Dictionary<string, AnimationClip> AnimationClips = new();

        private Animancer.AnimancerComponent _Animancer;
        public Animancer.AnimancerComponent Animancer => _Animancer;

        //=======================================================================
        //Lua内存可以释放掉？Lua虚拟机上面的内存是否可以放掉老的会一直存在么？要测试???
        //并且 逻辑层次不应该持有表现层次，适用于动画表现，不常用的，比较耗性能的
        //原则是module之间是msg，module到界面是直接调用可以重新绑定（例子），界面到module上通过action
        //例子：Module也不是持有自己的page 是通过UImgrfind的 所以弱关联的同时保证这个东西一定可以创建和找到
        //window 3D游戏Window只占使用率50%，内存很大不必加快速度，widget共有特性不会单独删除，会随着window删除并且能防错 :/3D游戏Window只占使用率50% page不删除特殊设置删除，内存很大不必加快速度，widget共有特性不会单独删除，会随着window删除并且能防错
        //[NonSerialized]

        public virtual bool onCloseDestroy { get { return false; } set { } }

        //public Action<UIPanel> PopStackListener { get; set; }
        //public bool PopStack { get; set; }//是否是通过堆栈系统弹出的弹窗

        public string Name;
        //public string Name { get; set; }//一个未必命名都按照规范了，可能出现1对多，反射那么不如存储

        public System.Action<string, System.Action> PlayAnimationAction;

        public System.Action<string, System.Action> ForcePlayAnimationAction;

        [LabelText("图集")]
        [OnValueChanged("OnSetAtlas")]
        public Texture2D Atlas;

        [ReadOnly]
        public string AtlasPath;

        public void OnSetAtlas()
        {
#if UNITY_EDITOR
            if (Atlas != null)
            {
                AtlasPath = AssetDatabase.GetAssetPath(Atlas);
                AtlasPath = AtlasPath.Replace("Assets/Res/", "").Replace(".png", "").ToLower();
                Atlas = null;
            }
            else
            {
                AtlasPath = "";
            }
#endif
        }

        public void GetSprite(string name, System.Action<Sprite> cb)
        {
            GetSpriteByAtlas(AtlasPath, name, cb);
        }

        public void GetSpriteByAtlas(string atlasPath, string spriteName, System.Action<Sprite> cb)
        {
            if (string.IsNullOrEmpty(atlasPath) || string.IsNullOrEmpty(name))
            {
                cb?.Invoke(null);
                return;
            }
            AtlasManager.Instance.GetSpriteAsync(atlasPath, spriteName, cb);
        }

        [SerializeField]
        [LabelText("分区")]
        [ValueDropdown("_uiSeatType")]
        public UISeatType UISeatType = new();
        [SerializeField]
        [LabelText("优先级")]
        [ValueDropdown("_uiQueuePriorityType")]
        public UIQueuePriorityType QueuePriorityType = new();
        [SerializeField]
        [LabelText("是否长期")]
        public bool IsLongTime = false;

        private CanvasGroup canvasGroup = null;

        public virtual E_UI_TYPE UIType
        {
            get => E_UI_TYPE.Panel;
        }
        public virtual void Open(object arg = null)
        {
            string PanelName = "";
            var pod = gameObject.GetComponent<PanelOffLineData>();
            if (pod != null)
            {
                PanelName = pod.SPECIALNAME;
            }

            if (!string.IsNullOrEmpty(PanelName))
            {
                GlobalEvent.OnOpenUI.Invoke(PanelName);
            }
            else
            {
                string realName = Name;
                var t = name.Split("/");
                if (t != null && t.Length > 0)
                {
                    realName = t[t.Length - 1];
                }
                GlobalEvent.OnOpenUI.Invoke(realName);
            }

            this.Log("Open() arg:{0}", arg);
        }

        /// <summary>
        /// 允许单独被调用，允许循环调用，唯独不允许uimgr循环调用
        /// </summary>
        /// <param name="arg"></param>
        public virtual void Close(object arg = null)
        {
            this.Log("Close() arg:{0}", arg);
            if (onCloseDestroy)
            {
                UIManager.Instance.OnUIPanelClose(Name);
            }
        }

        /* /// <summary>
         /// UIMgr循环删除的特殊接口
         /// 如果缓存一个list就可以解决，那么提取这个接口的必要性有么？：之前的思路是一定知道uimgr清理不然无法特殊处理，现在就算特殊处理依然无法本质解决问题；之前不想要修改close默认函数没问题；
         /// 核心问题是 延迟调用 走 uimgr里面的list的修改有问题没
         /// </summary>
         /// <param name="arg"></param>
         public virtual void UIMgrCycleClose(object arg = null)
         {

         }*/

        /// <summary>
        /// 当UI关闭时，会响应这个函数
        /// 该函数在重写时，需要支持可重复调用
        /// </summary>
        protected virtual void OnClose(object arg = null)
        {
            this.Log("OnClose()");

            UIManager.Instance.RemoveLoadAsyncUI(Name);

            //释放UI
            AtlasManager.Instance.ReleaseAtlas(AtlasPath);

            //if ("common/prefab/playtimelineblackwidget" == Name)
            //{
            //    SGF.Debuger.Log($"队列播放队列播放 UI配置 删除了timeline 队列111111");
            //}
            UIQueueManager.Instance.DelUIQueue(this);
        }
        /// <summary>
        /// 当前UI是否打开
        /// </summary>
        public bool IsOpen
        {
            get
            {
                if (this.gameObject == null)
                {
                    return false;
                }
                return this.gameObject.activeSelf;
            }
        }




        /// <summary>
        /// 当UI打开时，会响应这个函数
        /// </summary>
        /// <param name="arg"></param>
        protected virtual void OnOpen(object arg = null)
        {
            this.Log("OnOpen() ");
        }

        protected virtual void Awake()
        {

            PlayAnimationAction = PlayAnimation;
            ForcePlayAnimationAction = ForcePlayAnimation;
            AnimationClips.Clear();
            if (Clips != null && Clips.Count > 0)
            {
                foreach (var item in Clips)
                {
                    if (item != null)
                    {
                        AnimationClips.Add(item.name, item);
                    }
                    else
                    {
                        SGF.Debuger.LogError($"Panel {this.transform.name} , 缺少动画");
                    }
                }
            }
            _Animancer = transform.GetComponent<Animancer.AnimancerComponent>();

            canvasGroup = transform.GetComponent<CanvasGroup>();
        }

        public void PlayAnimation(string AimationName, System.Action action = null)
        {
            if (_Animancer == null)
            {
                action?.Invoke();
                return;
            }
            if (AnimationClips.TryGetValue(AimationName, out AnimationClip animationClip) && animationClip != null)
            {
                Animancer.Animator.enabled = true;  // todo 下面
                var state = Animancer.Play(animationClip, 0.05f, FadeMode.FixedDuration);
                state.Events.OnEnd = () =>
                {
                    state.IsPlaying = false;
                    // TODO: 播放完毕就吧动画组件关闭了，不然收到动画k帧的节点属性，就代码无法操作了。节点属性会一直受到最后一帧动画的值
                    //state.Stop();
                    //Animancer.Animator.enabled = false;


                    /* 回滚
                     * //Revision: 22028
                    Author: maliangbo@DOBEST
                    Date: 2023年7月14日 15:48:37
                    Message:
                        【新增】：删除动画接口
                        ----
                    Modified : /StarsProject_Client/trunk/Stars/Assets/Scripts/StarFramework/UI/Framework/UIPanel.cs*/

                    //调用DestroyGraph 会导致断点或者阻塞情况下 动画播不完全就结束
                    //Animancer.Playable.DestroyGraph();
                    // Animancer.Playable = null;

                    action?.Invoke();
                };
            }
        }

        public void ForcePlayAnimation(string AimationName, System.Action action = null)
        {
            if (_Animancer == null)
            {
                action?.Invoke();
                return;
            }

            if (AnimationClips.TryGetValue(AimationName, out AnimationClip animationClip) && animationClip != null)
            {
                //這個是時間，理論上最少動畫要大於20fps
                Animancer.Animator.enabled = true;
                var state = Animancer.Play(animationClip, 0.05f, FadeMode.FixedDuration);
                state.Events.OnEnd = () =>
                {
                    state.IsPlaying = false;
                    // TODO: 播放完毕就吧动画组件关闭了，不然收到动画k帧的节点属性，就代码无法操作了。节点属性会一直受到最后一帧动画的值
                    state.Stop();
                    Animancer.Animator.enabled = false;
                    //Animancer.Playable.DestroyGraph();
                    //Animancer.Playable = null;
                    action?.Invoke();
                };
            }
        }

        protected virtual void Start()
        {

        }

        protected virtual void OnDestroy()
        {

        }

        /// <summary>
        /// 当UI可用时调用
        /// </summary>
        protected virtual void OnEnable()
        {

        }

        /// <summary>
        /// 当UI不可用时调用
        /// </summary>
        protected virtual void OnDisable()
        {

        }



        //----------------------工具类-----------------------
        public void Find(string path, out Transform result)
        {
            result = null;
            if (transform != null)
            {
                result = transform.Find(path);
            }
        }

        public void Find(string path, Transform parent, out Transform result)
        {
            result = null;
            if (parent != null)
            {
                result = parent.Find(path);
            }
        }

        public void SetSelfCanvasGroup(bool isShow)
        {
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = isShow ? 1 : 0;
            canvasGroup.interactable = isShow;
            canvasGroup.blocksRaycasts = isShow;
        }


#if UNITY_EDITOR
        [ContextMenu("添加")]
        public void AddCanvasGroup()
        {
            CanvasGroup canvasGroup = transform.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                gameObject.AddComponent<CanvasGroup>();
                AssetDatabase.SaveAssets();
            }
        }
#endif
    }
}
