using Cysharp.Threading.Tasks;
using DG.Tweening;
using ProtoMsg;
using SGF.Module.Framework;
using SGF.Network;
using SGF.Unity;
using StarProject;
using StarProject.ArtHelper;
using StarProject.Game;
using StarProject.OffLine;
using StarProject.Service.Cam;
using StarProject.Service.Cam.Data;
using StarProject.Service.Input;
using StarProject.Service.LocalData;
using StarProject.Service.SystemOpen;
using StarProject.Service.User;
using StarProjectDef;
using System;
using System.Collections;
/*using System.Collections.Concurrent;*/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using XLua;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// 1Page全局只有一个，系统控制开关
/// 2window可以有多个，自己控制开关
/// 3挂件可以有多个，
/// 4挂件在window，挂件也可以在Page
/// 5Window可以在page
/// 6主Page比其他page特殊
/// 7page没有开关按钮（一定没有），一定有上下步
/// 8window有开关按钮（一定有）
/// 9挂件开关按钮（很少有，通常逻辑卸载）
/// </summary>
namespace SGF.UI.Framework
{
    //预处理指令
    [LuaCallCSharp]
    public class PerCommondInfo
    {
        public string Name;
        public MainPageCommond Mpc;
        public bool NeedHide;
        public PerCommondInfo(string name, MainPageCommond mpc, bool isOpen)
        {
            Name = name;
            Mpc = mpc;
            NeedHide = isOpen;
        }
    }

    public class UILoadInfo
    {
        public string Name;
        public UIAsyncLoadState State = UIAsyncLoadState.None;
        public List<Action<UIPanel>> CbList = new();
        public UILoadInfo(string name, UIAsyncLoadState state, Action<UIPanel> cb)
        {
            Name = name;
            State = state;
            CbList.Clear();
            CbList.Add(cb);
        }

        public void AddCB(Action<UIPanel> cb)
        {
            CbList.Add(cb);
        }
    }

    [LuaCallCSharp]
    public class UIManager : ServiceModule<UIManager>
    {
        public const string LOG_TAG = "UIManager";

        public static string MainScene = "Main";

        //====================================================
        public static string MainPage = "UIMainPage";
        public static SpaceType MainPageSpaceType = SpaceType.SpaceDefault;
        //private MainPageCommond mainPageCommond = MainPageCommond.None;
        //====================================================
        ///*技能的也注册进来MobileController*/，动态注册一下？？？:操作流程：1，点击panel然后alt+shift+W保存，2，动态东西脚本调用下PanelOffLineData、
        ///根节点管理器
        public Dictionary<string, Transform> PlaneSingleNameTrans = new();
        public Transform GetPlaneNodeFromOfflineData(string path)
        {
            string[] parts = path.Split('/');

            string firstPart = parts[0];

            string remainingPath = string.Join("/", parts, 1, parts.Length - 1);

            Transform trs;

            //子节点离线数据查找器：可以做性能分离了
            if (PlaneSingleNameTrans.TryGetValue(firstPart, out Transform transform))
            {
                PanelOffLineData pfl = transform.GetComponent<PanelOffLineData>();

                if (pfl != null)
                {

                    trs = pfl.GetTransformByKey(remainingPath);

                    if (trs == null)
                    {
                        trs = transform.Find(remainingPath);
                    }

                    return trs;
                }
                else
                {
                    Debug.LogError("策划没配PanelOffLineData，离线数据不能在线生成");
                }
            }

            return null;
        }
        private Canvas m_Canvas;
        public Canvas M_Canvas
        {
            get
            {
                if (m_Canvas == null)
                {
                    m_Canvas = CameraManager.Instance.GetCamera(E_CameraType.UICam).transform.Find("Canvas").GetComponent<Canvas>();//
                }
                return m_Canvas;
            }
        }

        private CameraBase m_CameraBase;
        public CameraBase M_CameraBase
        {
            get
            {
                if (m_CameraBase == null)
                {
                    m_CameraBase = CameraManager.Instance.GetCamera(E_CameraType.StarWorldCam);
                }
                return m_CameraBase;
            }
        }
        [LuaCallCSharp]
        class UIPageTrack
        {
            public string name;
            public string scene;
        }

        private Stack<UIPageTrack> m_pageTrackStack;
        private UIPageTrack m_currentPage;
        private Action<string> sceneLoaded;
        /// <summary>
        /// Name ， 界面 对儿
        /// </summary>
        private /*ConcurrentDictionary*/Dictionary<string, UIPanel> m_listLoadedPanel = new();//表现和缓存数据一样所以0数据可以优先查找，1节省性能；2不用遍历的获取窗体，3不用限定界面开关了，6和后续处理
        // 所有打开的window计数器
        private Dictionary<string, bool> m_UIPanelTag = new();
        public ModuleEvent onServiceModuleEventTest = new();

        private Dictionary<string, Transform> transformDic = new();

        private Transform UI3DFront;// = UI3DWorld.UI_3D_Front.transform;
        private Transform UI3DBack;// = UI3DWorld.UI_3D_Back.transform;
        private Transform UIRaw;// = UI3DWorld.ra.transform;

        public UIPage M_Current_UIPage;
        private Action<UIPage> m_LoadUIPageAction;

        //预处理缓存：当系统还没启动的时候
        private List<PerCommondInfo> perNameCmdPairs = new();
        //实际情况一直融合一个肯定有问题，
        //本身有这个特性（Key不但计数了，而且还记录是谁）：本质来说：记录了是谁，控制九宫格，开关需求；（3如果基数和1是冲突的）
        //生命周期的完成预处理
        //[][]Excel
        //info[]
        private Dictionary<string, MainPageCommond> nameCmdPairs = new();
        /// <summary>
        /// 关闭主界面白名单
        /// </summary>
        private List<string> HudWidgetWhiteList = new();

        public UIManager()
        {
            m_pageTrackStack = new Stack<UIPageTrack>();
            m_listLoadedPanel = new Dictionary<string, UIPanel>();

        }

        public override void Release()
        {
            MonoHelper.RemoveLaterUpdateListener(MyLaterUpdate, MonoHelper.E_ModuleType.CommonService);
            SetModel2UIDefault();
            GlobalEvent.OnStarWorldSetUp.RemoveListener(OnStarWorldGameOpen);
            GlobalEvent.OnMapConfigLoaded.RemoveListener(OnMapLoaded);
            GlobalEvent.OnTimelinePlayStateChangeEvent.RemoveListener(OnTimelinePlayStateChangeEvent);
            base.Release();

        }

        /// <summary>
        /// /*世界转角，*//*直接转角通常是0度来常规查看如果是41度就是战斗俯视角，*/
        /// /*模型缩放*/，/*层级*/，/*动画*/，/*perfab*/
        /// 战斗模型拥有全部动画，展台拥有全部动画，并且相通顺
        /// 展台具有比战斗更多的动画，属于特殊制作的（也就是不需要考虑创建子弹，和子弹的Layer）
        /// 转换相机和转换世界是一样的
        /// 创建角色情况，battle的世界，battle角色，前面ui-----真实3D
        /// 通常前面还是有一个uiMask----用Raw
        /// 一个物件的显示-----用本方法
        /// 也有ui盖前面的模式，分Canvas麻烦，或者分距离导致重绘，不如Raw
        /// </summary>
        /// <param name="model"></param>
        /// <param name="modelDataCell">模型配置信息</param>
        /// <param name="threeDModelPos">基础分割位置</param>
        /// <param name="scale">模型缩放</param>
        /// <param name="WorleRotate">大世界是否旋转|如果是Raw就不必填写因为这个摄像机随便调整</param>
        /// <param name="localRotate">角色旋转，通常配合大世界旋转可以，通常只有旋转Y</param>
        /// <param name="localPos">位置位移</param>
        /// <param name="animName">动画播放名字</param>
        public void SetModel2UI2(GameObject model, AvatarDataCell avatarDataCell, ThreeDModelPos threeDModelPos, Vector3 scale, Vector3 WorleRotate, Vector3 localRotate, Vector3 localPos, string animName, bool isHighModel = true, E_LayerType e_LayerType = E_LayerType.ThreeDUIMessage)
        {
            switch (threeDModelPos)
            {
                case ThreeDModelPos.Front:
                    if (UI3DFront == null)
                    {
                        UI3DFront = UI3DWorld.UI_3D_Front.transform;
                    }
                    if (model != null)
                    {
                        model.transform.SetParent(UI3DFront);
                        UI3DWorld.Root.transform.localEulerAngles = WorleRotate;
                    }
                    break;
                case ThreeDModelPos.Back:
                    if (UI3DBack == null)
                    {
                        UI3DBack = UI3DWorld.UI_3D_Back.transform;
                    }
                    if (model != null)
                    {
                        model.transform.SetParent(UI3DBack);
                        UI3DWorld.Root.transform.localEulerAngles = WorleRotate;
                    }
                    break;
                case ThreeDModelPos.Raw:
                    if (UIRaw == null)
                    {
                        UIRaw = UI3DWorld.UIRawRoot.transform;
                    }
                    if (model != null)
                    {
                        model.transform.SetParent(UIRaw);
                        UI3DWorld.UIRawRoot.transform.localEulerAngles = WorleRotate;
                    }
                    break;
                default:
                    {
                        model.transform.SetParent(EntityRoot.Instance.RemoveRoot.transform);
                    }
                    break;
            }

            model.transform.localScale = scale;
            model.transform.localEulerAngles = localRotate;
            model.transform.localPosition = localPos;
            if (string.IsNullOrEmpty(animName))
            {
                animName = "Idle_01";
            }
            //UI上的所有展示的角色，角色模型本身用实体层级
            model.GetComponent<RoleViewDisplay>().InitData(avatarDataCell, animName, e_LayerType.ToString(), isHighModel);
        }

        public void SetModel2UI2(GameObject model, int avatarID, ThreeDModelPos threeDModelPos, Vector3 scale, Vector3 WorleRotate, Vector3 localRotate, Vector3 localPos, string animName, bool isHighModel = true, E_LayerType e_LayerType = E_LayerType.ThreeDUIMessage)
        {
            switch (threeDModelPos)
            {
                case ThreeDModelPos.Front:
                    if (UI3DFront == null)
                    {
                        UI3DFront = UI3DWorld.UI_3D_Front.transform;
                    }
                    if (model != null)
                    {
                        model.transform.SetParent(UI3DFront);
                        UI3DWorld.Root.transform.localEulerAngles = WorleRotate;
                    }
                    break;
                case ThreeDModelPos.Back:
                    if (UI3DBack == null)
                    {
                        UI3DBack = UI3DWorld.UI_3D_Back.transform;
                    }
                    if (model != null)
                    {
                        model.transform.SetParent(UI3DBack);
                        UI3DWorld.Root.transform.localEulerAngles = WorleRotate;
                    }
                    break;
                case ThreeDModelPos.Raw:
                    if (UIRaw == null)
                    {
                        UIRaw = UI3DWorld.UIRawRoot.transform;
                    }
                    if (model != null)
                    {
                        model.transform.SetParent(UIRaw);
                        UI3DWorld.UIRawRoot.transform.localEulerAngles = WorleRotate;
                    }
                    break;
                default:
                    {
                        model.transform.SetParent(EntityRoot.Instance.RemoveRoot.transform);
                    }
                    break;
            }

            model.transform.localScale = scale;
            model.transform.localEulerAngles = localRotate;
            model.transform.localPosition = localPos;
            if (string.IsNullOrEmpty(animName))
            {
                animName = "Idle_01";
            }
            //UI上的所有展示的角色，角色模型本身用实体层级
            AvatarDataCell avatarDataCell = LocalDataManager.Instance.GetAvatarDataCell(avatarID);
            model.GetComponent<RoleViewDisplay>().InitData(avatarDataCell, animName, e_LayerType.ToString(), isHighModel);
        }

        Tween tw1, tw2, tw3;

        //AvatarDataCell FillAvatarCell(LuaTable tb)
        //{
        //    AvatarDataCell avatarDataCell = new AvatarDataCell();

        //    Type avatarCellType = typeof(AvatarDataCell);

        //    FieldInfo[] fields = avatarCellType.GetFields();

        //    int index = 1;

        //    foreach (FieldInfo field in fields)
        //    {
        //        if (field.FieldType == typeof(Int32))
        //        {
        //            int value;
        //            tb.Get(index, out value);
        //            FieldInfo fieldinfo = avatarCellType.GetField(field.Name);
        //            fieldinfo.SetValue(avatarDataCell,value);
        //        }
        //        else if(field.FieldType == typeof(string))
        //        {
        //            string value;
        //            tb.Get(index, out value);
        //            FieldInfo fieldinfo = avatarCellType.GetField(field.Name);
        //            fieldinfo.SetValue(avatarDataCell, value);
        //        }
        //        else if(field.FieldType == typeof(List<int>))
        //        {
        //            List<int> value;
        //            tb.Get(index, out value);
        //            FieldInfo fieldinfo = avatarCellType.GetField(field.Name);
        //            fieldinfo.SetValue(avatarDataCell, value);
        //        }
        //        index++;
        //    }

        //    return avatarDataCell;
        //}

        public void SetDrawCardModel(GameObject model, int avatarID, string animName, string animName2, string animEnter, float time2, bool isHighModel = true)
        {
            AvatarDataCell avatarDataCell = LocalDataManager.Instance.GetAvatarDataCell(avatarID); //FillAvatarCell(luacelldata);
            CloseDrawCardModel();
            model.transform.localScale = Vector3.one;
            model.transform.localEulerAngles = Vector3.zero;
            model.transform.localPosition = Vector3.zero;
            //UI上的所有展示的角色，角色模型本身用实体层级
            //判断动画文件是否存在
            string path = $"{avatarDataCell.AnimsPath}/{animEnter}";
            path = path.Replace("Assets/Res/", string.Empty);
            path = path.Replace(".anim", string.Empty);
            string str = StarProject.Service.Resource.ResourceFormalManager.Instance.GetRecursionLoadAssetPath(path, E_AssetType.Animation);
            var showani = animName;
            if (!string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str))
            {
                showani = animEnter;
            }
            model.GetComponent<RoleViewDisplay>().InitData(avatarDataCell, showani, E_LayerType.Entity.ToString(), isHighModel, (time1) =>
            {
                tw1 = DOVirtual.DelayedCall(time1, () =>
                {
                    if (model != null)
                    {
                        SetDrawCardModelAni2(model, animName, animName2, time1, time2);
                    }
                });
            });
        }

        public void SetDrawCardModelAni1(GameObject model, string animName1, string animName2, float time1, float time2)
        {
            model.GetComponent<RoleViewDisplay>()?.PlayIAnimImmediateAsync(animName1);
            tw2 = DOVirtual.DelayedCall(time1, () => { if (model != null) { SetDrawCardModelAni2(model, animName1, animName2, time1, time2); } });
        }

        public void SetDrawCardModelAni2(GameObject model, string animName1, string animName2, float time1, float time2)
        {
            model.GetComponent<RoleViewDisplay>()?.PlayIAnimImmediateAsync(animName2);
            tw3 = DOVirtual.DelayedCall(time2, () => { if (model != null) { SetDrawCardModelAni1(model, animName1, animName2, time1, time2); } });
        }

        public void CloseDrawCardModel()
        {
            if (tw1 != null)
            {
                tw1.Kill();
                tw1 = null;
            }
            if (tw2 != null)
            {
                tw2.Kill();
                tw2 = null;
            }
            if (tw3 != null)
            {
                tw3.Kill();
                tw3 = null;
            }
        }

        public void SetBgModel(GameObject model, Vector3 pos, Vector3 rot, Vector3 scale)
        {
            if (UIRaw == null)
            {
                UIRaw = UI3DWorld.UIRawRoot.transform;
            }
            if (model != null)
            {
                model.transform.SetParent(UIRaw);
                UI3DWorld.UIRawRoot.transform.localEulerAngles = Vector3.zero;
            }

            model.transform.localScale = scale;
            model.transform.localEulerAngles = rot;
            model.transform.localPosition = pos;
        }

        public void SetRawCamrea(bool isShow)
        {
            GameManager.Instance.SetUIFog(isShow);
            CameraManager.Instance.SetRawCamrea(isShow);
            if (GameManager.Instance.M_Map != null && GameManager.Instance.M_Map.M_rootHelper != null && GameManager.Instance.M_Map.M_rootHelper.charAdditionLightController != null)
            {
                GameManager.Instance.M_Map.M_rootHelper.charAdditionLightController.enabled = !isShow;
            }
        }

        /// <summary>
        /// 切场景的时候用，或者某些时候用吧
        /// </summary>
        public void SetModel2UIDefault()
        {
            UI3DWorld.Root.GetComponent<UI3DWorld>().Clear();
        }

        private void OnTimelinePlayStateChangeEvent(int id, bool isPlay)
        {
            var scale = Vector3.one*(isPlay ? 0 : 1);
            UIRoot.SceneWidgetRoot.transform.localScale = scale;
            UIRoot.UiWindowRoot.transform.localScale = scale;
            UIRoot.UiWidgetRoot.transform.localScale = scale;
            UIRoot.UiGuideUIRoot.transform.localScale = scale;
        }

        private UIQueueToDefine uIQueueToDefine = null;
        public readonly string BinaryPath = "Config/UIQueue/UIQueueToDefine";

        /// <summary>
        /// 初始化操作
        /// </summary>
        /// <param name="uiResRoot">UI资源的根目录，默认为"ui/"</param>
        public void Init(string uiResRoot)
        {
            CheckSingleton();

            UIRes.UIResRoot = uiResRoot;

            GlobalEvent.OnStarWorldSetUp.AddListener(OnStarWorldGameOpen);
            GlobalEvent.OnMapConfigLoaded.AddListener(OnMapLoaded);
            GlobalEvent.OnTimelinePlayStateChangeEvent.AddListener(OnTimelinePlayStateChangeEvent);
            //监听UnityScene加载事件
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                //SGF.Debuger.LogError($"新手关新流程 时间={SGF.Time.TimeUtils.ServerNow} 切图 删除了啊 sceneLoaded={sceneLoaded} ");

                if (sceneLoaded != null)
                {
                    sceneLoaded(scene.name);
                }
            };

            var textasset = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadTxtAssetSync(BinaryPath);
            if (textasset != null)
            {
                uIQueueToDefine = MessagePack.MessagePackSerializer.Deserialize<UIQueueToDefine>(textasset.bytes);
                StarProject.Service.Resource.ResourceFormalManager.Instance.ReleaseTextAssetCache(BinaryPath);
            }

            MonoHelper.AddLaterUpdateListener(MyLaterUpdate, MonoHelper.E_ModuleType.CommonService);
        }


        bool takePhoto = false;
        bool _isShow = false;
        /// <summary>
        /// 设置UI模糊
        /// </summary>
        /// <param name="isShow"></param>
        public void SetBlurImage(bool isShow)
        {
            _isShow = isShow;
            takePhoto = true;

        }

        private void MyLaterUpdate()
        {
            if (takePhoto)
            {
                MonoHelper.StartCoroutine(TakeScreenshotLate());
                takePhoto = false;
            }
        }

        public IEnumerator TakeScreenshotLate()
        {
            yield return new WaitForEndOfFrame(); // 等待当前帧渲染完成
            if (_isShow)
            {
                Frame.UIUtil.ScreenshotToBlurImage(M_CameraBase.Camera, UIRoot.BlurImage);

                UIRoot.BlurImage.gameObject.SetActive(_isShow);

                CameraManager.Instance.SwitchBattleCameraShowState(false, true);
            }
            else
            {

                CameraManager.Instance.SwitchBattleCameraShowState(true, true);

                UIRoot.BlurImage.gameObject.SetActive(_isShow);
            }

        }

        public void CloseAllScreenUIEffect()
        {
            foreach (var it in screenUIEffect)
            {
                GameObject.Destroy(screenUIEffect[it.Key]);
            }
            screenUIEffect.Clear();
        }

        //ScreenUIEffects/Gve
        private Dictionary<string, GameObject> screenUIEffect = new();
        /// <summary>
        /// 设置屏幕效果
        /// </summary>
        /// <param name="prefabName"></param>
        /// <param name="isShow"></param>
        public void SetScreenUIEffect(string prefabName, bool isShow)
        {
            if (isShow)
            {
                if (!screenUIEffect.ContainsKey(prefabName))
                {
                    screenUIEffect.Add(prefabName, null);
                    //加载资源
                    Action<GameObject> action = (GameObject gob) =>
                    {
                        if (screenUIEffect.ContainsKey(prefabName))
                        {
                            gob.transform.SetParent(UIRoot.UIROOT.transform, false);
                            gob.transform.SetAsFirstSibling();
                            screenUIEffect[prefabName] = gob;
                        }
                    };
                    UIRes.LoadPrefabAsync(prefabName, action);

                    //var gob = UIRes.LoadPrefab(prefabName);
                    //gob.transform.SetParent(UIRoot.UIROOT.transform, false);
                    //gob.transform.SetAsFirstSibling();
                    //screenUIEffect[prefabName] = gob;
                }
            }
            else
            {
                if (screenUIEffect.ContainsKey(prefabName))
                {
                    if (screenUIEffect[prefabName] != null)
                    {
                        GameObject.Destroy(screenUIEffect[prefabName]);
                    }

                    screenUIEffect.Remove(prefabName);
                }
            }
        }

        /// <summary>
        /// 加载UI，如果UIRoot下已经有了，则直接取UIRoot下的
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        private T Load<T>(string name, Transform parent = null, bool isTopWidget = false, bool isSceneWidget = false) where T : UIPanel
        {
            T ui = null;
            //1，Load~Close对照;
            //2，设计上：目前具备挂载跟节点的挂件，局部page的挂件；（理论上活跃只有一个/通过增删-显隐）；主界面的挂载-全局的挂载-子界面的挂载；界面之间很少叠加；有重复的可能但是无所谓；
            //3,【核心是找到对应要处理的挂件（自己容器层）】
            //4，策略上：1,先找自己的，2，找局部的，3，找全局的；
            //5，细节上：从组件的空--->局部获取--->全局获取---->生成保底
            //6，Log：【有parents的情况下挂载挂件，没有开关逻辑：之前没发现】：知识点：Transform的Find会找“/”识别子节点的标识； UIRoot.Find是用Foreach方法所以避免了这个问题；也可以用DeepFind原理一样
            if (parent != null)
            {
                var go = UIRoot.Find(name, parent.gameObject);
                if (go != null)
                {
                    ui = go.GetComponent<T>();
                }
            }
            if (ui == null)
            {
                ui = UIRoot.Find<T>(name, isTopWidget, isSceneWidget);
            }
            if (ui == null)
            {
                GameObject original = UIRes.LoadPrefab(name);
                if (original != null)
                {
                    //GameObject go = GameObject.Instantiate(original);
                    ui = original.GetComponent<T>();
                    if (ui != null)
                    {
                        original.name = name;
                        UIRoot.AddChild<T>(ui, parent, isTopWidget, isSceneWidget);
                    }
                    else
                    {
                        this.LogError("Load() Prefab没有增加对应组件: " + name);
                    }
                }
                else
                {
                    this.LogError("Load() Res Not Found: " + name);
                }
            }

            if (ui != null)
            {
                if (!m_listLoadedPanel.ContainsKey(name))
                {
                    m_listLoadedPanel.Add(name, ui);
                }
                else
                {
                    if (m_listLoadedPanel[name] == null)
                    {
                        m_listLoadedPanel[name] = ui;
                    }
                }
                ui.Name = name;
                //ui.gameObject.SetActive(false);//卡顿先关闭后开启就变二次了
            }


            return ui;
        }

        public void OnUIPanelClose(string panelName)
        {
            //UIpanel自身调用的话一定是已经包含，但这种约束属于程序编写弱约束；很有可能别人调用
            //如果remove 外围，不可以被循环调用，互相控制也允许不然要延迟调用；
            if (m_listLoadedPanel.ContainsKey(panelName))
            {
                m_listLoadedPanel[panelName] = null;
            }
        }

        // 异步加载的UI状态
        private Dictionary<string, UILoadInfo> LoadAsyncUIState = new();

        private void LoadAsync<T>(string name, Action<T> cb, Transform parent = null, bool isTopWidget = false, bool isSceneWidget = false) where T : UIPanel
        {
            //if ("common/prefab/playtimelineblackwidget" == name)
            //{
            //    SGF.Debuger.LogError($"队列播放 加载timeline节点");
            //}
            T ui = null;
            // a、在传入的父节点下找
            if (parent != null)
            {
                var go = UIRoot.Find(name, parent.gameObject);
                if (go != null)
                {
                    ui = go.GetComponent<T>();
                }
            }
            // b、在泛型分类的父节点下找
            if (ui == null)
            {
                ui = UIRoot.Find<T>(name, isTopWidget, isSceneWidget);
            }
            // c、在路径下加载
            if (ui == null)
            {
                Action<UIPanel> locadUICB = (UIPanel localui) =>
                {
                    cb?.Invoke(localui as T);
                };
                UILoadInfo uILoadInfo = null;
                if (LoadAsyncUIState.TryGetValue(name, out uILoadInfo))
                {
                    // 如果存在那就是正在加载
                    // 只添加回调，不重复加载UI
                    if (uILoadInfo.State == UIAsyncLoadState.InLoaded)
                    {
                        uILoadInfo.AddCB(locadUICB);
                    }
                    return;
                }

                uILoadInfo = new UILoadInfo(name, UIAsyncLoadState.InLoaded, locadUICB);
                LoadAsyncUIState.Add(name, uILoadInfo);

                Action<GameObject> action = (GameObject gob) =>
                {
                    T loadUI = null;
                    if (gob != null)
                    {
                        loadUI = gob.GetComponent<T>();

                        // 修改缓存
                        UILoadInfo uILoadInfo = null;
                        {
                            if (LoadAsyncUIState.TryGetValue(name, out uILoadInfo))
                            {
                                //
                            }
                            else
                            {
                                // 已经调用删除了，那就不需要了，删除实例出来的预制吧
                                // uipanellist私有，外面循环，不是list迭代不会引发问题，所以单独卸载接口没问题
                                loadUI.Close();
                                return;
                            }
                        }

                        if (loadUI != null)
                        {
                            gob.name = name;
                            loadUI.Name = name;

                            UIRoot.AddChild<T>(loadUI, parent, isTopWidget, isSceneWidget);

                            if (!m_listLoadedPanel.ContainsKey(name))
                            {
                                m_listLoadedPanel.Add(name, loadUI);
                            }
                            else
                            {
                                if (m_listLoadedPanel[name] == null)
                                {
                                    m_listLoadedPanel[name] = loadUI;
                                }
                            }
                        }
                        else
                        {
                            this.LogError("Load() Prefab没有增加对应组件: " + name);
                        }

                        // 修改缓存
                        if (uILoadInfo != null)
                        {
                            for (int i = 0; i < uILoadInfo.CbList.Count; i++)
                            {
                                Action<UIPanel> itemcb = uILoadInfo.CbList[i];
                                if (itemcb != null)
                                {
                                    itemcb.Invoke(loadUI);
                                }
                            }
                            uILoadInfo.State = UIAsyncLoadState.LoadFinish;
                            uILoadInfo.CbList.Clear();
                        }
                        else
                        {
                            cb?.Invoke(loadUI);
                        }
                    }
                    else
                    {
                        this.LogError("Load() Res Not Found: " + name);
                        cb?.Invoke(loadUI);
                    }
                };
                UIRes.LoadPrefabAsync(name, action);
            }
            else
            {
                if (!m_listLoadedPanel.ContainsKey(name))
                {
                    m_listLoadedPanel.Add(name, ui);
                }
                else
                {
                    if (m_listLoadedPanel[name] == null)
                    {
                        m_listLoadedPanel[name] = ui;
                    }
                }
                UILoadInfo uILoadInfo = null;
                if (!LoadAsyncUIState.TryGetValue(name, out uILoadInfo))
                {
                    uILoadInfo = new UILoadInfo(name, UIAsyncLoadState.LoadFinish, null);
                    LoadAsyncUIState.Add(name, uILoadInfo);
                }
                ui.Name = name;
                cb?.Invoke(ui);
            }
        }

        //简单name就行了不用待“/”的
        //通常传递的都是【引导】需要传递的，配置策划的步骤的，也是需要挂载脚本的
        public bool HasPanele(string viewName)
        {
            //不是加载过的（不会删除同步表现策略），不是显示过的和当前显示的，不是全名称是简单名称，需要的时候要进行查询一下需求////
            //从启动开始，所有运行过（需要离线的）【注册的】【根节点】【缓存管理器】，【是挂载的节点本身】
            if (PlaneSingleNameTrans.TryGetValue(viewName, out Transform transform))
            {
                // 加个补丁, 处理 引导节点 在安卓机上 返回 登录，重新登录游戏没有
                if (transform == null)
                {
                    PlaneSingleNameTrans.Remove(viewName);
                    return false;
                }
                ///所有节点只要保证他能渲染就true
                return transform.gameObject.activeInHierarchy;
            }
            ///不然就没有
            return false;
        }

        /// <summary>
        /// Load是关闭，启动才打开合理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="arg"></param>
        /// <param name="_mainPageCommond"></param>
        /// <param name="isTopWidget"></param>
        /// <param name="isNeedBlurImage"></param>
        /// <returns></returns>
        private T Open<T>(string name, Transform parent = null, object arg = null, MainPageCommond _mainPageCommond = MainPageCommond.HideNone, bool isTopWidget = false, bool isSceneWidget = false, bool isNeedBlurImage = false) where T : UIPanel
        {
            T ui = Load<T>(name, parent, isTopWidget, isSceneWidget);
            if (ui != null)
            {
                //没有，还需要控制
                if (!nameCmdPairs.ContainsKey(name) && _mainPageCommond != MainPageCommond.HideNone && GameInput.Instance != null)
                {
                    nameCmdPairs.Add(name, _mainPageCommond);
                    RefleshMainPageByLeaveCmds(true);
                }
                else if (GameInput.Instance == null && _mainPageCommond != MainPageCommond.HideNone)
                {
                    //黑幕：是要隐藏；通常来说，默认都是显示，有特意发过来目的就是隐藏
                    perNameCmdPairs.Add(new PerCommondInfo(name, _mainPageCommond, true));
                }
                //ui.gameObject.SetActive(true);
                ui.Open(arg);

                if (isNeedBlurImage)
                {
                    SetBlurImage(true);
                }
            }
            else
            {
                this.LogError("Open() Failed! Name:{0}", name);
            }
            return ui;
        }

        private void OpenAsync<T>(string name, Action<T> cb, Transform parent = null, object arg = null, MainPageCommond _mainPageCommond = MainPageCommond.HideNone, bool isTopWidget = false, bool isSceneWidget = false, bool isNeedBlurImage = false) where T : UIPanel
        {
            Action<T> action = (T ui) =>
            {
                if (ui != null)
                {
                    //没有，还需要控制
                    if (!nameCmdPairs.ContainsKey(name) && _mainPageCommond != MainPageCommond.HideNone && GameInput.Instance != null)
                    {
                        nameCmdPairs.Add(name, _mainPageCommond);
                        RefleshMainPageByLeaveCmds(true);
                    }
                    //else if (GameInput.Instance == null && _mainPageCommond != MainPageCommond.HideNone)
                    //{
                    //    //黑幕：是要隐藏；通常来说，默认都是显示，有特意发过来目的就是隐藏
                    //    perNameCmdPairs.Add(new PerCommondInfo(name, _mainPageCommond, true));
                    //}
                    //ui.gameObject.SetActive(true);
                    UIQueueManager.Instance.AddUIQueuePanel(name.ToLower(), ui);
                    ui.Open(arg);
                    if (isNeedBlurImage)
                    {
                        SetBlurImage(true);
                    }
                }
                else
                {
                    this.LogError("Open() Failed! Name:{0}", name);
                }
                cb?.Invoke(ui);
            };
            LoadAsync<T>(name, action, parent, isTopWidget, isSceneWidget);
        }

        private bool starWorldSetup = false;

        /// <summary>
        /// 模块启动后刷一次
        /// StarWorldGame启动-->GameInput启动-->首次模块启动
        /// 转交预处理给正式
        /// </summary>
        /// <param name="arg0"></param>
        private void OnStarWorldGameOpen(bool arg0)
        {
            //首次Map地图刷
            //首次处理缓存信息，刷新剩余
            if (!starWorldSetup)
            {
                for (int i = 0; i < perNameCmdPairs.Count; i++)
                {
                    //缓存
                    if (nameCmdPairs != null)
                    {
                        if (!nameCmdPairs.ContainsKey(perNameCmdPairs[i].Name))
                        {
                            nameCmdPairs.Add(perNameCmdPairs[i].Name, perNameCmdPairs[i].Mpc);
                        }
                    }
                }
                //处理即可，不必记录是谁，这里
                RefleshMainPageByLeaveCmds(true);
                //清理
                perNameCmdPairs.Clear();
                starWorldSetup = true;
            }
        }

        private void OnMapLoaded(string arg0)
        {

            if (starWorldSetup)
            {
                //战斗地图刷新
                if (arg0.Contains("Map_"))
                {
                    //Map地图二次刷：可能换主界面，可能没换都刷
                    RefleshMainPageByLeaveCmds(true);
                }
                else
                {
                    //非Map地图没必要：虽然内部处理主界面了；但是所有【表世界/里世界】和【所有地图】都基于战斗不会换主界面所以不必刷
                    //如果换了也要刷
                    //要刷就加上呗
                }
            }
        }

        /// <summary>
        /// 不是处理任意一个指令，而是处理剩余指令
        /// 他是一种保底刷新策略
        /// </summary>
        /// <param name="mpc"></param>
        /// <param name="isHide"></param>
        private void RefleshMainPageByLeaveCmds(bool isHide)
        {
            //外层检测，是否本窗口关闭需要处理
            //里层根据所有剩余的指令竖向平铺  
            // 1 ，1，1，1，1
            // 1 ，0，0，1，1
            // 垂直检测，如果有一个是1，那就保持关闭，如果都是0就开启了（因为1是关闭指令：这是设定）
            //PS：“&”都是1，才是1；“|”有一个是1就是1
            MainPageCommond finalEveryPartNeedHide = MainPageCommond.HideNone;
            foreach (var item in nameCmdPairs)
            {
                finalEveryPartNeedHide |= item.Value;//用一个想全开启的指令，有一个(位）让他关闭他就关闭就好了
            }
            //所有节点并行都要处理的
            // 检测对应位 ：这一位两边都是1 == 这一位目标是1 Means 这一位是1 【说明这一位要隐藏】
            // 
            if (GameInput.Instance != null)
            {
                GameInput.Instance.PlayAnimByState(isHide, finalEveryPartNeedHide);
            }
            //1，永远处理剩余的本质逻辑，2，主界面不在（显示/控制）只是本次不控制（不刷），3，当主界面打开的时候要刷一次剩余（这样就永远不会忽略（剩余）指令）
            if (MainPage == UIDef.StarWorldPage && M_Current_UIPage is StarProject.UI.StarWorld.StarWorldPage)
            {
                //1,并行；2，结构相同，3，被控制组件不同
                StarProject.UI.StarWorld.StarWorldPage starWorldPage = (StarProject.UI.StarWorld.StarWorldPage)M_Current_UIPage;
                if (starWorldPage != null)
                {
                    starWorldPage.PlayAnimByState(!isHide, finalEveryPartNeedHide);
                }
                //我隐藏他就开启，我开启他就隐藏
                //【打开新界面】，隐藏主界面的时候，我传true ，他需发false
                //关闭界面，发true
                //【打开新界面】升级到->【我有任意一个要隐藏】，说明由于二级界面，或2级多个界面共同控制;有任意一个部分要隐藏
                ModuleManager.Instance.SendMessage(ModuleDef.Name.ObjectInteractiveModule, "OnShowHideObjectInteractive", new object[] { !(finalEveryPartNeedHide != MainPageCommond.HideNone) });
                ModuleManager.Instance.SendMessage(ModuleDef.Name.EventBossRankModule, "OnShowHideObjectInteractive", new object[] { !(finalEveryPartNeedHide != MainPageCommond.HideNone) });
                ModuleManager.Instance.SendMessage(ModuleDef.Name.QuickEquipModule, "OnShowHideQuickEquip", new object[] { !(finalEveryPartNeedHide != MainPageCommond.HideNone) });
                //ModuleManager.Instance.SendMessage(ModuleDef.Name.AvgLuaModule, "OnShowHideAvgPopBox", new object[] { !(finalEveryPartNeedHide != MainPageCommond.HideNone) });
            }
        }

        //-----------------------------------关闭就还原呗-----------------------------------------
        //Window 和 Widget 的关闭
        private bool ClosePanel<T>(string name, Transform parent = null, bool isTopWidget = false, bool isSceneWidget = false) where T : UIPanel
        {
            name = name.ToLower();
            //SGF.Debuger.Log($"队列播放 关闭UI name={name}");

            bool res = false;
            T ui = null;
            if (parent != null)
            {
                var go = UIRoot.Find(name, parent.gameObject);
                if (go != null)
                {
                    ui = go.GetComponent<T>();
                }
            }
            if (m_listLoadedPanel.ContainsKey(name))
            {
                ui = (T)m_listLoadedPanel[name];
            }
            if (ui == null)
            {
                ui = UIRoot.Find<T>(name, isTopWidget, isSceneWidget);
            }
            if (ui != null)
            {
                if (m_listLoadedPanel.ContainsKey(name) && m_listLoadedPanel[name])
                {
                    m_listLoadedPanel[name].Close();
                    //SGF.Debuger.LogError($"队列播放 关闭UI name={name}");

                    //有，要控制
                    if (nameCmdPairs.ContainsKey(name) && nameCmdPairs[name] != MainPageCommond.HideNone)
                    {
                        nameCmdPairs.Remove(name);
                        RefleshMainPageByLeaveCmds(false);
                    }
                    res = true;
                }
            }
            // 修改异步加载缓存
            RemoveLoadAsyncUI(name);
            return res;
        }

        //Page 的 关闭
        private void CloseAllLoadedPanels()
        {
            List<string> _keysToRemove = new();
            List<UIPanel> _keysToClose = new();
            //--------------
            // 先删掉已经删掉的 弹窗

            foreach (KeyValuePair<string, UIPanel> kvp in m_listLoadedPanel)
            {
                // 如果 value 是空，将 key 添加到待删除列表
                if (kvp.Value == null)
                {
                    _keysToRemove.Add(kvp.Key);
                }
            }

            // 遍历完后，统一执行删除操作
            foreach (string key in _keysToRemove)
            {
                m_listLoadedPanel.Remove(key);
            }
            _keysToRemove.Clear();
            //--------------

            //有的没key的早就没了，没value也处理了，所以都有了
            foreach (KeyValuePair<string, UIPanel> kvp in m_listLoadedPanel)
            {

                //1，原逻辑要清理主hud删除
                //2，这里要切场景不清理引导√，并且不清理hud对
                if (HudWidgetWhiteList.Contains(kvp.Key))
                {
                    //不关闭Hud,guildWidget
                    continue;
                }

                //保证add时稳妥，才可清理value；
                if (kvp.Value.IsOpen)
                {
                    string Name = kvp.Value.Name;
                    _keysToClose.Add(kvp.Value);

                    //有，要控制
                    if (nameCmdPairs.ContainsKey(Name) && nameCmdPairs[Name] != MainPageCommond.HideNone)
                    {
                        nameCmdPairs.Remove(Name);
                    }
                }
            }

            foreach (var item in _keysToClose)
            {
                try
                {
                    item.Close();//UIMgrCycleClose();//两次
                }
                catch (Exception ex)
                {
                    SGF.Debuger.LogError($"{LOG_TAG} CloseAllLoadedPanels() item={item},ex={ex}");
                }
            }
            _keysToClose.Clear();

            RefleshMainPageByLeaveCmds(false);
            ClearUIPanelTag();
            nameCmdPairs.Clear();
            LoadAsyncUIState.Clear();
            UIQueueManager.Instance.ClearUIQueueCache();
        }

        public void RemoveLoadAsyncUI(string name)
        {
            //if ("common/prefab/playtimelineblackwidget" == name)
            //{
            //    SGF.Debuger.LogError($"队列播放队列播放 UI配置 删除了timeline加载队列");
            //}
            //SGF.Debuger.Log($"队列播放队列播放 UI配置 删除了{name}的加载状态");
            if (LoadAsyncUIState.ContainsKey(name))
            {
                LoadAsyncUIState.Remove(name);
                //SGF.Debuger.LogError($"队列播放队列播放 UI配置 删除了{name}的加载状态");
            }
        }

        public UIPanel GetUIPanel(string name)
        {
            name = name.ToLower();
            UIPanel uIPanel;
            if (m_listLoadedPanel.TryGetValue(name, out uIPanel))
            {
                if (uIPanel == null)
                {
                    m_listLoadedPanel.Remove(name);
                    return null;
                }

                if (m_listLoadedPanel.ContainsKey(name))
                {
                    return m_listLoadedPanel[name];
                }
            }

            return null;
        }

        //1HudWidgetWhiteList
        //2    整理到Widget
        //这里会处理：除了主界面的是一定不关闭的
        //需要别人开启window的时候确认自己的Widget开启(OnOpen)，严格控制生命周期(本方法会关闭任意Window下的Widget）
        public void CloseAllLoadedWindowNWidget(bool isForce = false)
        {
            ModuleManager.Instance.ClearStackWindows();

            List<UIPanel> _keysToClose = new();
            if (isForce)
            {
                foreach (KeyValuePair<string, UIPanel> kvp in m_listLoadedPanel)
                {
                    if (kvp.Value is UIWidget || kvp.Value is UIWindow)
                    {
                        string Name = kvp.Key;
                        _keysToClose.Add(kvp.Value);
                        if (kvp.Value != null)
                        {
                            kvp.Value.onCloseDestroy = true;
                        }
                        //有，要控制
                        if (nameCmdPairs.ContainsKey(Name) && nameCmdPairs[Name] != MainPageCommond.HideNone)
                        {
                            nameCmdPairs.Remove(Name);
                        }
                    }
                }
            }
            else
            {
                List<string> _keysToRemove = new();
                foreach (KeyValuePair<string, UIPanel> kvp in m_listLoadedPanel)
                {
                    // 如果 value 是空，将 key 添加到待删除列表
                    if (kvp.Value == null)
                    {
                        _keysToRemove.Add(kvp.Key);
                    }
                }

                // 遍历完后，统一执行删除操作
                foreach (string key in _keysToRemove)
                {
                    m_listLoadedPanel.Remove(key);
                }
                _keysToRemove.Clear();


                foreach (KeyValuePair<string, UIPanel> kvp in m_listLoadedPanel)
                {
                    if (kvp.Value is UIWidget || kvp.Value is UIWindow)
                    {
                        if (HudWidgetWhiteList.Contains(kvp.Key))
                        {
                            //不关闭Hud
                            continue;
                        }
                        if (kvp.Value.IsOpen)
                        {
                            //Debug.LogError(m_listLoadedPanel[i].Name);
                            string Name = kvp.Key;
                            _keysToClose.Add(kvp.Value);
                            //有，要控制
                            if (nameCmdPairs.ContainsKey(Name) && nameCmdPairs[Name] != MainPageCommond.HideNone)
                            {
                                nameCmdPairs.Remove(Name);
                            }
                        }
                    }
                }
            }
            //foreach (var item in _keysToClose)
            //{
            //    SGF.Debuger.Log($"只是日志 关闭界面报错 111111111111111111界面名={item.name}");
            //}
            foreach (var item in _keysToClose)
            {
                //if (item != null)
                //{
                //    SGF.Debuger.LogWarning($"只是日志 关闭界面报错 111111111111111111界面名={item.name}");
                //}

                try
                {
                    item.Close();
                }
                catch (Exception ex)
                {
                    if (item != null)
                    {
                        SGF.Debuger.LogError($"只是日志 关闭界面报错 界面名={item.name},ex={ex}");
                    }
                    else
                    {
                        SGF.Debuger.LogError($"只是日志 关闭界面报错 item是个空,ex={ex} err!!!");
                    }
                }
            }
            _keysToClose.Clear();
            RefleshMainPageByLeaveCmds(false);
            ClearUIPanelTag();
            SetBlurImage(false);
        }

        public void CloseAllLoadedNoUIQueueType(bool isForce = false)
        {
            ModuleManager.Instance.ClearStackWindows();

            List<KeyValuePair<string, UIPanel>> list = m_listLoadedPanel.KToList();
            for (int i = list.Count - 1; i >= 0; i--)
            {
                UIPanel value = list[i].Value;
                string key = list[i].Key;
                if (value is UIWidget || value is UIWindow)
                {
                    if (HudWidgetWhiteList.Contains(key))
                    {
                        continue;
                    }
                    if (key != null)
                    {
                        if (value.IsOpen && value.QueuePriorityType == UIQueuePriorityType.None)
                        {
                            SGF.Debuger.Log($"只是日志 关闭界面报错 111111111111111111界面名={key}");
                            try
                            {
                                SetUIPanelTag(key, false);
                                value.Close();
                                //有，要控制
                                if (nameCmdPairs.ContainsKey(key) && nameCmdPairs[key] != MainPageCommond.HideNone)
                                {
                                    nameCmdPairs.Remove(key);
                                }
                            }
                            catch (Exception ex)
                            {
                                SGF.Debuger.LogError($"只是日志 关闭界面报错 item是个空,ex={ex} err!!!");
                            }
                        }
                    }
                }
            }
            //foreach (KeyValuePair<string, UIPanel> kvp in m_listLoadedPanel)
            //{
            //    if (kvp.Value is UIWidget || kvp.Value is UIWindow)
            //    {
            //        if (HudWidgetWhiteList.Contains(kvp.Key))
            //        {
            //            continue;
            //        }
            //        if (kvp.Value != null )
            //        {
            //            if (kvp.Value.IsOpen && kvp.Value.QueuePriorityType == UIQueuePriorityType.None)
            //            {
            //                SGF.Debuger.Log($"只是日志 关闭界面报错 111111111111111111界面名={kvp.Key}");
            //                try
            //                {
            //                    SetUIPanelTag(kvp.Key, false);
            //                    kvp.Value.Close();
            //                    //有，要控制
            //                    if (nameCmdPairs.ContainsKey(kvp.Key) && nameCmdPairs[kvp.Key] != MainPageCommond.HideNone)
            //                    {
            //                        nameCmdPairs.Remove(kvp.Key);
            //                    }
            //                }
            //                catch (Exception ex)
            //                {
            //                    SGF.Debuger.LogError($"只是日志 关闭界面报错 item是个空,ex={ex} err!!!");
            //                }
            //            }                        
            //        }
            //    }
            //}
            RefleshMainPageByLeaveCmds(false);
            //ClearUIPanelTag();
            SetBlurImage(false);
        }
        //-----------------------------------关闭就还原呗-----------------------------------------
        //=======================================================================

        /// <summary>
        /// 【进入主Page】
        /// 会清空Page堆栈
        /// 这个点击Dota2的Perv，不会回到上个场景
        /// 
        ///加载场景（不同场景）
        ///场景和界面绑定设计,所以是一组组合方式
        ///通常场景切换，主界面未必切换（副本，战场） |界面切换，场景未必切换（窗口点选） 
        /// </summary>
        public void EnterMainPage(ChangePageType type = ChangePageType.CommonForceChange)
        {
            switch (type)
            {
                case ChangePageType.CommonForceChange:
                    m_pageTrackStack.Clear();
                    //所谓强弱是针对是否强制，副本带动---UI强制切换。《通常来源于是否副本变化，是否UI变化，强制UI变化》
                    OpenPageWorker(MainScene, MainPage, null);
                    break;
                case ChangePageType.MeekFromFb:
                    m_pageTrackStack.Clear();
                    //这里UI和副本是平级，依赖相同不切换原则。
                    OpenPageWorkerMeek(MainScene, MainPage, null);
                    break;
                default:
                    break;
            }
        }


        //Page采用开关=======================================================================
        #region UIPage管理
        /// <summary>
        /// 【打开Page，去下一个】
        /// (可以其他场景，缓存栈）
        /// 这个点击Dota2的Perv，会回到上个场景
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="page"></param>
        /// <param name="arg"></param>
        public void OpenPage(string scene, string page, object arg = null, Action completeCb = null)
        {
            Debuger.Log(LOG_TAG, "OpenPage() scene:{0}, page:{1}, arg:{2} ", scene, page, arg);

            if (m_currentPage != null)
            {
                m_pageTrackStack.Push(m_currentPage);
            }

            OpenPageWorker(scene, page, arg, MainPageCommond.HideNone, completeCb);
        }

        /// <summary>
        ///  默认当前场景
        ///  Page也会默认关闭的OnClose
        ///  虽然Page是Panel的一个分支，但设计上他是所有widget，window的底层，会随着翻页一起清除
        /// </summary>
        /// <param name="page"></param>
        /// <param name="arg"></param>
        public void OpenPage(string page, object arg = null, Action completeCb = null)
        {
            this.OpenPage(MainScene, page, arg, completeCb);
        }

        /// <summary>
        /// 【返回上一个Page】
        /// 设计上同Scene才会用到，切换时需要EnterMainPage
        /// </summary>
        public void GoBackPage()
        {
            Debuger.Log(LOG_TAG, "GoBackPage()");
            if (m_pageTrackStack.Count > 0)
            {
                var track = m_pageTrackStack.Pop();
                OpenPageWorker(track.scene, track.name, null);
            }
            else if (m_pageTrackStack.Count == 0)
            {
                EnterMainPage();
            }
        }

        //所有service不会被释放，Business会释放，局部释放模式--即service不放，放business（完全释放不合适，都不释放性能浪费）【断线】
        public void ReLogin(AgainLoginType againLoginType)
        {
            CloseAllLoadedWindowNWidget(false);
            SystemOpenManager.Instance.ContainTipsInvoke();
            //againLoginType = AgainLoginType.BacktrackSelectRole;
            Action<bool> cb = (bool res) =>
            {
                GlobalEvent.OnBackLogin.Invoke(againLoginType);
                CloseAllScreenUIEffect();
                ModuleManager.Instance.ClearStackWindows();
                SetBlurImage(false);
                MainPage = UIDef.UILoginPage;
                MainScene = "MainTown";
                MainPageSpaceType = SpaceType.SpaceDefault;
                StarScenesManager.Instance.CreateRoleLoadMap = true;
                ModuleManager.Instance.CreateModule(ModuleDef.Name.LoginModule);
                ModuleManager.Instance.ShowModule(ModuleDef.Name.LoginModule, againLoginType);//启动界面
                GameManager.Instance.TriggerEvent("On_ReLogin", null);
                FixMessageManager.Instance.ClearAllDatas();
                GameManager.Instance.ClearEquipSlotDatas();
                PartnerManager.Instance.Reset();
                PreLoading.Instance.Reset();
                TaskHelper.ClearTask();
                // 清空玩家数据
                UserManager.Instance.Clear();
                CleanNullPanles();
                ModuleManager.Instance.ReleaseModule(ModuleDef.Name.StarWorldModule);
            };
            GameManager.Instance.Loading.OnPlayBlackDOFade(true, 0.2f, cb);
        }

        private void CleanNullPanles()
        {
            if (PlaneSingleNameTrans != null)
            {
                List<string> list = new();
                foreach (var item in PlaneSingleNameTrans)
                {
                    if (item.Value == null)
                    {
                        list.Add(item.Key);
                    }
                }

                foreach (var key in list)
                {
                    PlaneSingleNameTrans.Remove(key);
                }
            }
        }

        /// <summary>
        /// 打开新Page，默认会去掉挂件，window，虽然3个都是分支；但设计上Page是底层
        /// 场景相同不处理，UIPage相同也处理
        /// 进入场景顺序：先场景加载，然后加载MainPage（没填就默认），需当mainPage加载成功后执行后续UI逻辑     【场景接场景loaded，ui接UILoaded】
        /// </summary>
        /// <param name="toScene">场景名字</param>
        /// <param name="page">页面名</param>
        /// <param name="arg"></param>
        private void OpenPageWorker(string toScene, string page, object arg, MainPageCommond _mainPageCommond = MainPageCommond.HideNone, Action completeCb = null)
        {
            //111
            Debuger.Log(LOG_TAG, "OpenPageWorker() scene:{0}, page:{1}, arg:{2} ", toScene, page, arg);

            string currentScene = SceneManager.GetActiveScene().name;

            m_currentPage = new UIPageTrack();
            m_currentPage.scene = toScene;
            m_currentPage.name = page;

            //关闭当前Page时打开的所有UI
            CloseAllLoadedPanels();


            if (currentScene.Equals(toScene))
            {
                //M_Current_UIPage = Open<UIPage>(page, null, arg, _mainPageCommond);

                m_LoadUIPageAction = (UIPage ui) =>
                {
                    M_Current_UIPage = ui;
                    m_LoadUIPageAction = null;
                };
                OpenAsync<UIPage>(page, m_LoadUIPageAction, null, arg, _mainPageCommond);

                completeCb?.Invoke();
            }
            else
            {
                sceneLoaded = (sceneName) =>
                {
                    if (sceneName == toScene)
                    {
                        //SGF.Debuger.LogError($"新手关新流程 时间={SGF.Time.TimeUtils.ServerNow} 切图 删除了啊 111111 ");

                        sceneLoaded = null;
                        //M_Current_UIPage = Open<UIPage>(page, null, arg, _mainPageCommond);
                        m_LoadUIPageAction = (UIPage ui) =>
                        {
                            M_Current_UIPage = ui;
                            m_LoadUIPageAction = null;
                        };
                        OpenAsync<UIPage>(page, m_LoadUIPageAction, null, arg, _mainPageCommond);
                        //SceneManager（Unity）,loaded后会通知2
                        GlobalEvent.onSceneLoaded.Invoke(sceneName, false);
                        completeCb?.Invoke();
                    }
                };
                //异步加载，开始1
                //所以切场景，一定有Loading
                _ = StarScenesManager.Instance.LoadScene(toScene, MainPageSpaceType);
            }
            SetShowVirtualCameraToHideHud(false);
        }

        /// <summary>
        /// 柔情版
        /// 意识是不强制切换同名Page
        /// 不同才强制切
        /// </summary>
        /// <param name="toScene"></param>
        /// <param name="page"></param>
        /// <param name="arg"></param>
        private void OpenPageWorkerMeek(string toScene, string page, object arg, MainPageCommond _mainPageCommond = MainPageCommond.HideNone)
        {
            Debuger.Log(LOG_TAG, "OpenPageWorker() scene:{0}, page:{1}, arg:{2} ", toScene, page, arg);
            //表里世界是一个，并且只能开一个，有map规范，隐式加载默认的一起绑定加载，显示隐藏逻辑，不需要记录 //无论表里，到新切换都是一个类型的场景切换//理解成一个整体的话他不会因为整体判断相同而不切换，并且都属于maP_大类的
            string currentScene = SceneManager.GetActiveScene().name;//StarScenesManager.Instance.OldSceneName;
            bool isSamePage = string.Compare(m_currentPage.name, page) == 0;
            // 这里异步加载的，可能导致m_currentPage的值已经变了，但是M_Current_UIPage还没变
            bool isSamePage2 = true;
            if (M_Current_UIPage != null)
            {
                isSamePage2 = string.Compare(M_Current_UIPage.gameObject.name, page) == 0;
            }
            //Scene 和 UI 相机是分离的可以单独处理
            if (!isSamePage && !isSamePage2)
            {
                m_currentPage = new UIPageTrack();
                m_currentPage.scene = toScene;
                m_currentPage.name = page;

                //关闭当前Page时打开的所有UI
                CloseAllLoadedPanels();
            }


            //场景是否相同都会问page,很尊重，是平级
            //不确定是否换Scene，和UI，但这里全部是来源于
            //1，切换副本，2，切换场景，3，甚至一切同场景不同副本的切换
            if (currentScene.Equals(toScene) && StarScenesManager.Instance.loadSceneType == LoadSceneType.Default)
            {
                if (!isSamePage || !isSamePage2)
                {
                    //M_Current_UIPage = Open<UIPage>(page, null, arg, _mainPageCommond);
                    m_LoadUIPageAction = (UIPage ui) =>
                    {
                        M_Current_UIPage = ui;
                        m_LoadUIPageAction = null;
                    };
                    OpenAsync<UIPage>(page, m_LoadUIPageAction, null, arg, _mainPageCommond);
                    //场景相同，界面不同，默认加载
                    //不论场景是否相同，不论UI是否相同，这里一定是切换
                    //只有时机不同，切换就发就好，只不过切换类型不同
                }
                GlobalEvent.onSceneLoaded.Invoke(toScene, true);

                //===08+10庭凯临时要加的:loading
                //同场景切换很快，也要加入loading，就在这里同场景临时盖临时放上
                //不等于的，或者还是并且难以话语描述，通过&&还要运算 ||直接掉落程序运算现象本质来看更好
                if ((MainPageSpaceType != SpaceType.SpaceMirror) && (MainPageSpaceType != SpaceType.SpaceDefault) && (MainPageSpaceType != SpaceType.SpaceScene))
                {
                    ShowLoadingAsync();
                }
            }
            else
            {
                //SGF.Debuger.LogWarning($"新手关新流程 时间={SGF.Time.TimeUtils.ServerNow} 切图 11111111111111111111111111111 ");

                sceneLoaded = (sceneName) =>
                {
                    //SGF.Debuger.LogWarning($"新手关新流程 时间={SGF.Time.TimeUtils.ServerNow} 切图 2222222222222222 sceneName={sceneName} ");

                    if (sceneName == toScene)
                    {
                        //SGF.Debuger.LogError($"新手关新流程 时间={SGF.Time.TimeUtils.ServerNow} 切图 删除了啊 222222222222 ");

                        sceneLoaded = null;
                        if (!isSamePage || !isSamePage2)
                        {
                            //M_Current_UIPage = Open<UIPage>(page, null, arg, _mainPageCommond);
                            m_LoadUIPageAction = (UIPage ui) =>
                            {
                                M_Current_UIPage = ui;
                                m_LoadUIPageAction = null;
                            };
                            OpenAsync<UIPage>(page, m_LoadUIPageAction, null, arg, _mainPageCommond);
                        }
                        //不同Scene，场景发
                        GlobalEvent.onSceneLoaded.Invoke(sceneName, false);
                    }
                };

                _ = StarScenesManager.Instance.LoadScene(toScene, MainPageSpaceType);
            }
            SetShowVirtualCameraToHideHud(false);
        }

        #endregion

        //Window采用删除=======================================================================

        #region UIWindow管理

        public UIWindow OpenWindow(string name, object arg = null, MainPageCommond _mainPageCommond = MainPageCommond.HideNone, bool isInHudWhiteList = false, bool isNeedBlurImage = false)
        {
            name = name.ToLower();
            if (isInHudWhiteList && !HudWidgetWhiteList.Contains(name))
            {
                HudWidgetWhiteList.Add(name);
            }
            UIWindow ui = Open<UIWindow>(name, null, arg, _mainPageCommond, false, false, isNeedBlurImage);
            if (ui != null)
            {
                SetUIPanelTag(name, true);
                ui.transform.SetAsLastSibling();
                //SGF.Debuger.LogWarning($"移动是检查是否有窗口打开=========打开了窗口 name={name}");
            }

            return ui;
        }

        public void OpenWindowAsync(string name, Action<UIWindow> cb, object arg = null, MainPageCommond _mainPageCommond = MainPageCommond.HideNone, bool isInHudWhiteList = false, bool isNeedBlurImage = false)
        {
            name = name.ToLower();
            if (LoadAsyncUIState.TryGetValue(name, out var uILoadInfo))
            {
                // 如果存在那就是正在加载或则已经加载完了
                // 回调没有，那就不需要多次返回回去了
                if (uILoadInfo.State == UIAsyncLoadState.InLoaded || uILoadInfo.State == UIAsyncLoadState.LoadFinish)
                {
                    if (cb == null)
                    {
                        return;
                    }
                }
            }

            if (isInHudWhiteList && !HudWidgetWhiteList.Contains(name))
            {
                HudWidgetWhiteList.Add(name);
            }

            Action<UIWindow> action = (UIWindow ui) =>
            {
                if (ui != null)
                {
                    SetUIPanelTag(name, true);
                    ui.transform.SetAsLastSibling();
                    //SGF.Debuger.LogError($"创角界面11111=========打开了窗口 name={name}");
                }
                cb?.Invoke(ui);
            };

            OpenAsync<UIWindow>(name, action, null, arg, _mainPageCommond, false, false, isNeedBlurImage);
        }


        /// <summary>
        /// 关闭界面
        /// </summary>
        /// <param name="name"></param>
        public void CloseWindow(string name)
        {
            bool res = ClosePanel<UIWindow>(name);
            if (res)
            {
                SetUIPanelTag(name, false);
                SetBlurImage(false);
            }
        }

        #endregion

        //Widget采用删除=======================================================================

        #region UIWidget管理

        public UIWidget OpenWidget(WidgetInfo widgetInfo)
        {
            return OpenWidget(widgetInfo.name, widgetInfo.SetAsFirstSibling, widgetInfo.arg, widgetInfo.parent, widgetInfo._mainPageCommond, widgetInfo.isInHudWhiteList, widgetInfo.isTopWidget);
        }

        public void OpenWidgetAsync(WidgetInfo widgetInfo)
        {
            OpenWidgetAsync(widgetInfo.name, widgetInfo.cb, widgetInfo.SetAsFirstSibling, widgetInfo.arg, widgetInfo.parent, widgetInfo._mainPageCommond, widgetInfo.isInHudWhiteList, widgetInfo.isTopWidget);
        }

        public UIWidget OpenWidget(string name, bool SetAsFirstSibling = false, object arg = null, Transform parent = null, MainPageCommond _mainPageCommond = MainPageCommond.HideNone, bool isInHudWhiteList = false, bool isTopWidget = false, bool isSceneWidget = false)
        {
            name = name.ToLower();
            if (isInHudWhiteList && !HudWidgetWhiteList.Contains(name))
            {
                HudWidgetWhiteList.Add(name);
            }

            UIWidget ui = Open<UIWidget>(name, parent, arg, _mainPageCommond, isTopWidget, isSceneWidget);
            if (ui != null)
            {
                if (SetAsFirstSibling)
                {
                    ui.transform.SetAsFirstSibling();
                }
            }
            return ui;
        }

        /// <summary>
        /// 异步加载挂件
        /// </summary>
        /// <param name="name">UI路径</param>
        /// <param name="cb">加载完成回调</param>
        /// <param name="SetAsFirstSibling"></param>
        /// <param name="arg">参数</param>
        /// <param name="parent">父节点</param>
        /// <param name="_mainPageCommond">主界面显隐指令</param>
        /// <param name="isInHudWhiteList">是否加入白名单</param>
        /// <param name="isTopWidget">是否最上层</param>
        /// <param name="isSceneWidget">是否场景挂件</param>
        public void OpenWidgetAsync(string name, Action<UIWidget> cb, bool SetAsFirstSibling = false, object arg = null, Transform parent = null, MainPageCommond _mainPageCommond = MainPageCommond.HideNone, bool isInHudWhiteList = false, bool isTopWidget = false, bool isSceneWidget = false)
        {
            name = name.ToLower();
            //bool isAddQueue = true;
            //if ("common/prefab/playtimelineblackwidget" == name)
            //{
            //    SGF.Debuger.Log($"队列播放队列播放 UI配置 播放了timeline");
            //}
            if (LoadAsyncUIState.TryGetValue(name, out var uILoadInfo))
            {
                // 如果存在那就是正在加载或则已经加载完了
                // 回调没有，那就不需要多次返回回去了
                if (uILoadInfo.State == UIAsyncLoadState.InLoaded || uILoadInfo.State == UIAsyncLoadState.LoadFinish)
                {
                    //isAddQueue = false;
                    if (cb == null && arg == null)
                    {
                        return;
                    }
                }
            }
            if (uIQueueToDefine != null /*&& isAddQueue*/)
            {
                UIQueueCfgData uIQueueCfgData = null;
                if (uIQueueToDefine.UIQueueCfgDatas.TryGetValue(name.ToLower(), out uIQueueCfgData))
                {
                    //SGF.Debuger.Log($"队列播放队列播放 UI配置 name={uIQueueCfgData.UIPath},UISeatType={uIQueueCfgData.UISeatType},QueuePriority={uIQueueCfgData.QueuePriority},IsLongTime={uIQueueCfgData.IsLongTime}");
                    if (uIQueueCfgData.UISeatType != UISeatType.None)
                    {
                        WidgetInfo widgetInfo = new();
                        widgetInfo.name = name.ToLower();
                        widgetInfo.SetAsFirstSibling = SetAsFirstSibling;
                        widgetInfo.arg = arg;
                        widgetInfo.parent = parent;
                        widgetInfo._mainPageCommond = _mainPageCommond;
                        widgetInfo.isInHudWhiteList = isInHudWhiteList;
                        widgetInfo.isTopWidget = isTopWidget;
                        widgetInfo.isSceneWidget = isSceneWidget;
                        widgetInfo.cb = cb;
                        UIQueueManager.Instance.AddUIQueue(uIQueueCfgData, widgetInfo);
                        return;
                    }
                }
            }
            //else
            //{
            //    SGF.Debuger.LogWarning($"队列播放队列播放 UI配置 name={name},直接打开界面，不入队");
            //}

            OpenQueueWidget(name, cb, SetAsFirstSibling, arg, parent, _mainPageCommond, isInHudWhiteList, isTopWidget, isSceneWidget);
        }

        public void OpenQueueWidget(string name, Action<UIWidget> cb, bool SetAsFirstSibling = false, object arg = null, Transform parent = null, MainPageCommond _mainPageCommond = MainPageCommond.HideNone, bool isInHudWhiteList = false, bool isTopWidget = false, bool isSceneWidget = false)
        {
            //SGF.Debuger.LogError($"队列播放 打开界面 name={name}");
            name = name.ToLower();
            if (LoadAsyncUIState.TryGetValue(name, out var uILoadInfo))
            {
                // 如果存在那就是正在加载或则已经加载完了
                // 回调没有，那就不需要多次返回回去了
                if (uILoadInfo.State == UIAsyncLoadState.InLoaded || uILoadInfo.State == UIAsyncLoadState.LoadFinish)
                {
                    if (cb == null && arg == null)
                    {
                        return;
                    }
                }
            }

            if (isInHudWhiteList && !HudWidgetWhiteList.Contains(name))
            {
                HudWidgetWhiteList.Add(name);
            }
            Action<UIWidget> action = (UIWidget ui) =>
            {
                if (ui != null)
                {
                    if (SetAsFirstSibling)
                    {
                        ui.transform.SetAsFirstSibling();
                    }
                }
                cb?.Invoke(ui);
            };
            OpenAsync<UIWidget>(name, action, parent, arg, _mainPageCommond, isTopWidget, isSceneWidget);
        }

        public void CloseWidget(string name, Transform parent = null, bool isTopWidget = false)
        {
            ClosePanel<UIWidget>(name, parent, isTopWidget);
        }

        #endregion
        //=======================================================================

        #region 聊天界面的特殊逻辑

        // ------ 聊天界面，登录的时候会直接打开，
        // 点击主界面的按钮会显示聊天界面-此事才会关闭俩次遥感区域
        public void SetShowChatWindow(bool isShow)
        {
            MainPageCommond mainPageCommond = MainPageCommond.HideNone;
            if (isShow)
            {
                mainPageCommond = MainPageCommond.ChatHide;
            }
            if (!nameCmdPairs.ContainsKey(UIDef.ChatChannelWindow))
            {
                nameCmdPairs.Add(UIDef.ChatChannelWindow, mainPageCommond);
            }
            else
            {
                nameCmdPairs[UIDef.ChatChannelWindow] = mainPageCommond;
            }
            SetUIPanelTag(UIDef.ChatChannelWindow, isShow);
            RefleshMainPageByLeaveCmds(!isShow);
        }

        #endregion

        #region TimeLine特殊需求
        [XLua.BlackList]
        public Action ClosePlayTimelineBlackWidget = null;

        public void SetShowTimeLineToHideHud(bool isShow)
        {
            MainPageCommond mainPageCommond = MainPageCommond.HideNone;
            if (isShow)
            {
                mainPageCommond = MainPageCommond.HideBoth;
            }
            if (!nameCmdPairs.ContainsKey(UIDef.TimeLineToHideHud))
            {
                nameCmdPairs.Add(UIDef.TimeLineToHideHud, mainPageCommond);
            }
            else
            {
                nameCmdPairs[UIDef.TimeLineToHideHud] = mainPageCommond;
            }
            SetUIPanelTag(UIDef.TimeLineToHideHud, isShow);
            RefleshMainPageByLeaveCmds(!isShow);

            DynamicUIRoot.EntityUIRoot.GetComponent<CanvasGroup>().alpha = isShow ? 0f : 1f;
            DynamicUIRoot.DamageUIRoot.GetComponent<CanvasGroup>().alpha = isShow ? 0f : 1f;
        }

        #endregion

        #region VirtualCamera特殊需求

        public void SetShowVirtualCameraToHideHud(bool isShow)
        {
            MainPageCommond mainPageCommond = MainPageCommond.HideNone;
            if (isShow)
            {
                mainPageCommond = MainPageCommond.HideBoth;
            }
            if (!nameCmdPairs.ContainsKey(UIDef.VirtualCameraToHideHud))
            {
                nameCmdPairs.Add(UIDef.VirtualCameraToHideHud, mainPageCommond);
            }
            else
            {
                nameCmdPairs[UIDef.VirtualCameraToHideHud] = mainPageCommond;
            }
            SetUIPanelTag(UIDef.VirtualCameraToHideHud, isShow);
            RefleshMainPageByLeaveCmds(!isShow);

            DynamicUIRoot.EntityUIRoot.GetComponent<CanvasGroup>().alpha = isShow ? 0f : 1f;
            DynamicUIRoot.DamageUIRoot.GetComponent<CanvasGroup>().alpha = isShow ? 0f : 1f;
        }

        #endregion

        #region LoadingView

        public void SetShowLoadingViewToHideHud(bool isShow)
        {
            MainPageCommond mainPageCommond = MainPageCommond.HideNone;
            if (isShow)
            {
                mainPageCommond = MainPageCommond.HideBoth;
            }
            if (!nameCmdPairs.ContainsKey(UIDef.LoadingViewToHideHud))
            {
                nameCmdPairs.Add(UIDef.LoadingViewToHideHud, mainPageCommond);
            }
            else
            {
                nameCmdPairs[UIDef.LoadingViewToHideHud] = mainPageCommond;
            }
            SetUIPanelTag(UIDef.LoadingViewToHideHud, isShow);
            RefleshMainPageByLeaveCmds(!isShow);
        }

        #endregion

        #region GM面板

        public void SetShowGMPanelToHideHud(bool isShow)
        {
            MainPageCommond mainPageCommond = MainPageCommond.HideNone;
            if (isShow)
            {
                mainPageCommond = MainPageCommond.PageHide;
            }
            if (!nameCmdPairs.ContainsKey(UIDef.GMPanelToHideHud))
            {
                nameCmdPairs.Add(UIDef.GMPanelToHideHud, mainPageCommond);
            }
            else
            {
                nameCmdPairs[UIDef.GMPanelToHideHud] = mainPageCommond;
            }
            //SetUIPanelTag(UIDef.GMPanelToHideHud, isShow);
            RefleshMainPageByLeaveCmds(!isShow);
        }

        #endregion



        //=======================================================================

        #region 界面计数

        private void ClearUIPanelTag()
        {
            m_UIPanelTag.Clear();
        }

        private void SetUIPanelTag(string name, bool isOpen)
        {
            name = name.ToLower();
            if (!m_UIPanelTag.ContainsKey(name))
            {
                m_UIPanelTag.Add(name, isOpen);
            }
            else
            {
                m_UIPanelTag[name] = isOpen;
            }

            GetIsHasAnyWindowOpened();
        }

        public bool GetIsHasAnyWindowOpened()
        {
            foreach (var item in m_UIPanelTag)
            {
                if (item.Value)
                {
                    InputManager.Instance.ClearMoveCommand();
                    return true;
                }
            }
            return false;
        }

        #endregion
        /// <summary>
        // 1，同场景切换，loading要固定时间
        // 2，服务器有无敌状态
        //3，客户端此时不会自动寻路
        //4，除了波纹没有loading，其他策划全要loading
        //==============这些只是一部分，要案子
        /// </summary>
        /// <param name="time"></param>
        /// <param name="callBack"></param>
        public async System.Threading.Tasks.Task ShowLoadingAsync(float time = 1.0f, System.Action callBack = null)
        {
            //GameManager.Instance.Loading.OnProcessStart();
            SGF.Debuger.LogWarning("同场景切换，loading要固定时间  11111111111111");
            NetworkManager.Instance.LockMessage(true);
            GameManager.Instance.Loading.OnPlayBlackDOFade(true, time);
            await UniTask.Delay(TimeSpan.FromSeconds(time));
            NetworkManager.Instance.LockMessage(false);
            SGF.Debuger.LogWarning("同场景切换，loading要固定时间  22222222222");
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
            //GameManager.Instance.Loading.OnProcessEnd();
            GameManager.Instance.Loading.OnProcessBlackEnd();
            callBack?.Invoke();
            //Tweener tweener = DOTween.To(() => 0, (v) =>
            //{
            //    //GameManager.Instance.Loading.OnProcess(v); //Single
            //}, time, time);
            //tweener.onComplete = () =>
            //{
            //    SGF.Debuger.LogWarning("同场景切换，loading要固定时间  22222222222");
            //    NetworkManager.Instance.LockMessage(false);
            //    //GameManager.Instance.Loading.OnProcessEnd();
            //    GameManager.Instance.Loading.OnProcessBlackEnd();
            //    callBack?.Invoke();
            //};

        }
        //=======================================================================

        public LoadingView GetLoadingView()
        {
            return UIRoot.FindTinyUIRoot("LoadingView").GetComponent<LoadingView>();
            // return OpenWid
            // ("Loading/Prefab/LoadingView") as LoadingView;
        }
    }
}
