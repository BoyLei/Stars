using SGF.Module.Framework;
using SGF.UI.Framework;
using StarProject.Game.Entity;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.WithOutLife;
using StarProjectDef;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//这里不是单单特效。而是全局触发器，特效只是表现形式【但是可以包含其中】
/// <summary>
/// 这里主要处理。远程动态
/// </summary>
namespace StarProject.Service.TriggerEntity
{
    #region[UI实体][Done]
    //UI层次【实体/Trigger/非生命体】：TODO：本地静态的我不管，动态的也不管
    //TODO：屏幕特效：点击出现点击按钮[本地，动态，只有View的ViewPoolLit]
    ///1【远程动态：这里】实体类：用于模拟战场实体的战斗【足球经理，小地图，中地图】：可能有池化，甚至数据项的，战场小地图UI类的实体
    ///2【远程静态】：UI的赋值项的，一次数据直接赋值：那你自己Business->resource功能加载
    ///3【本地静态】，你拼到你perfab里面
    //4,本地动态:LocalFxManager
    #endregion
    #region【场景的实体】【模式带来的陨石特效关卡】[Done]
    //场景特效  
    //*如非生命体征的创建就要放在FxMgr
    #endregion
    #region 【人的实体】[Done]
    //
    //【所有人创建的在人身体中：如人的子弹，人的技能，人技能触发的非生命体，人召唤的任意东西】
    //实体需要自己处理的-----------------
    //实体特效：那是View层的 :*被生命体征拥有的非生命体Fx，
    #endregion
    //##############################################
    //什么东西一定区分静态，动态|本地，服务器--不需要同步就别有逻辑实体池化器就可以
    /// 逻辑释放基于服务器
    /// 【工具项先不管】//1，特效如果是静态的那就不需要基于服务器【本地动态拼接，依赖美术，客户端顶多帮你加载上去】
    /// 
    /// 【TODO非逻辑实体的简单池特性：没必要同步逻辑】2，如果是动态的：客户端本地的压根不用实体直接特效加载器【本地，动态，可池化】
    /// 
    /// 【current】3，如果是动态：远端的就要基于服务器释放，就是实体【远端，动态可池化】
    public class TriggerEntityManager : ServiceModule<TriggerEntityManager>
    {
        Transform FxSceneRoot;//特效不基于场景：人身上能挂，【实体特效】；【特效身上不能挂实体特效】
        Transform UIRoot;
        //这个就是主体，不同于人就是人主体，删除自有ntt自己做
        private DictionaryEx<EnumEnityListKey, List<EntityObject>> entityListDic = new DictionaryEx<EnumEnityListKey, List<EntityObject>>();


        //找挂载节点
        public void Init(Transform fxSceneRoot/*, Transform uiRoot*/)
        {
            FxSceneRoot = fxSceneRoot;
            //UIRoot = uiRoot;

        }
        #region 实体类
        //[UI实体类型]：【主球经理,小地图】：不同于Buff，我说的是没有归属的哪种。buff归属于人可以在人身上
        //这里是副本陨石雨，烈火路径，就是环境的，模式的设定，我宏观给你保存了
        //这里要注意不是人身上的另外：什么才是实体：1强调生命，2反复创建，3不唯一的，4战斗的才是实体，5强调座位预留空间预留的，6循环机制的
        internal void AddUITriggerNtt(EnumEnityListKey entityFx)
        {
            EntityRemoteStatic ntt = EntityFactory.InstanceEntity<EntityRemoteStatic>();
            GetEntityList(entityFx).Add(ntt);
            //现在没有非实体数据

            switch (entityFx)
            {
                case EnumEnityListKey.Env_EntityFx:
                    break;
                case EnumEnityListKey.UI_Fx:
                    if (UIRoot == null)
                    {
                        UIRoot = UIFXRoot.UINttRoot.transform;
                    }
                    if (UIRoot == null)
                    {
                        SGF.Debuger.LogError("没找到ui节点，或者尚未启动");
                        break;
                    }
                    ntt.Create(E_WithOuLifeResType.UI, 0, UIRoot);//UIPerfab上挂载对应的，View实体脚本[UI类型的]，包含加载的TODO没写呢
                    break;
                default:
                    break;
            }
        }


        //场景实体
        //这个依赖游戏模式启动
        internal void AddEnvTriggerNtt(EnumEnityListKey entityFx)
        {
            EntityRemoteStatic ntt = EntityFactory.InstanceEntity<EntityRemoteStatic>();
            GetEntityList(entityFx).Add(ntt);
            //现在没有非实体数据
            switch (entityFx)
            {
                case EnumEnityListKey.Env_Buff:
                    break;
                case EnumEnityListKey.UI_Buff_MainPlayer:
                    break;
                case EnumEnityListKey.UI_Buff_FB_Team:
                    break;
                case EnumEnityListKey.Union_Buff_Boss:
                    break;
                case EnumEnityListKey.Env_EntityFx:
                    ntt.Create(E_WithOuLifeResType.FxUnit, 0, FxSceneRoot); //FxUnit
                    break;
                default:
                    break;
            }

        }
        //切换场景时候清理一下即可
        #endregion
        //public void ClearUI() 
        //{
        
        //}
        public void ReleaseAll() 
        {

            if (entityListDic != null && entityListDic.Count != 0)
            {
                foreach (var lists in entityListDic)
                {
                    if (lists.Value != null && lists.Value.Count != 0)
                    {
                        for (int i = 0; i < lists.Value.Count; i++)
                        {
                            EntityFactory.ReleaseEntity(lists.Value[i]);
                            //lists.Value[i].Release();
                        }
                        lists.Value.Clear();
                    }
                }
                entityListDic.Clear();

            }

        }
        public void OnEndGame()
        {
            //ClearUI();
            ReleaseAll();

        }

        protected List<EntityObject> GetEntityList(EnumEnityListKey key)
        {
            //取他就说明你肯定想用，
            //EnsureEntityListInit();
            //Get：Key，List
            if (entityListDic.ContainsKey(key))
            {
                return entityListDic[key];
            }
            else
            {
                entityListDic.Add(key, new List<EntityObject>());
            }
            return entityListDic[key];
        }
        //public init



        //通用特效的父级节点
        //[SerializeField] private Transform fxRoot;
        //public Transform UIFxRoot;
        //public Camera UI_Camera;
        //private List<GameObject> activatedFXList = new List<GameObject>();
        //private Queue<GameObject> pool = new Queue<GameObject>(5);

        //public Transform FxRoot
        //{
        //    get
        //    {
        //        if (fxRoot == null)
        //        {
        //            fxRoot = GameEventsManager.Instance.FXRoot;
        //        }

        //        return fxRoot;
        //    }

        //    set => fxRoot = value;
        //}

        //private void Awake()
        //{
        //    Instance = this;
        //    ModuleInit(this);
        //    UI_Camera = UIManager.Instance.UICamera.GetComponent<Camera>();
        //}

        //public void OnChangeSceneReAwake(SceneGameMode sceneGameMode)
        //{
        //    if (sceneGameMode == SceneGameMode.Explorer)
        //    {
        //        FxRoot = GameEventsManager.Instance.FXRoot;
        //    }
        //    else if (sceneGameMode == SceneGameMode.BuildingFarm)
        //    {
        //        FxRoot = GameBuildManager.Instance.BuildFxRoot;
        //    }
        //}


        //private Vector3 point = Vector3.zero;

        //void Update()

        //{
        //    // for (int i = activatedFXList.Count - 1; i >= 0; --i)
        //    // {
        //    //     GameObject fx = activatedFXList[i];
        //    //     float fxTime = float.Parse(fx.name);
        //    //     if(Time.time - fxTime > ParticleSystemLength(UIFxRoot))
        //    //     {
        //    //         RecycleFX(fx);
        //    //         activatedFXList.RemoveAt(i);
        //    //     }
        //    // }
        //    // if (Input.GetKeyDown(KeyCode.E))
        //    // {
        //    //     UIManager.Instance.CtrlWindow("OrderWindow", true, false);
        //    //
        //    // }
        //    // if (Input.GetKeyDown(KeyCode.M))
        //    // {
        //    //     UIManager.Instance.CtrlWindow("MailWindow", true, false);
        //    //
        //    // }
        //    if (Application.isMobilePlatform)
        //    {
        //        for (int i = 0; i < Input.touchCount; ++i)
        //        {
        //            // SGF.Debuger.Log("Input.touchCount"+Input.touchCount);
        //            Touch touch = Input.GetTouch(i);

        //            if (touch.phase == TouchPhase.Began)
        //            {
        //                SGF.Debuger.Log(touch.position);
        //                point.x = touch.position.x;
        //                point.y = touch.position.y;
        //                point.z = 10f;
        //                // SGF.Debuger.Log("beforeconversion----point"+point);
        //                point = UI_Camera.ScreenToWorldPoint(point);
        //                // SGF.Debuger.Log("----afterconversion--point---"+point);
        //                PlayFX(point);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (Input.GetMouseButtonDown(0))
        //        {
        //            point.x = Input.mousePosition.x;
        //            point.y = Input.mousePosition.y;
        //            point.z = 10f;
        //            // SGF.Debuger.Log("mousePosition"+point); 
        //            point = UI_Camera.ScreenToWorldPoint(point);
        //            // SGF.Debuger.Log("mousePositionToWorld"+point);
        //            // point = UI_Camera.WorldToScreenPoint(point);
        //            // SGF.Debuger.Log("mousePositionTorld"+point);
        //            // Ray ray = UI_Camera.ScreenPointToRay(Input.mousePosition);//拿到鼠标按下的点
        //            // RaycastHit hitInfo;
        //            // bool isCollider=Physics.Raycast(ray,out hitInfo);
        //            PlayFX(point);
        //        }
        //    }
        //}

        ////全萤幕特效
        //private void PlayFX(Vector3 tapPos)
        //{
        //    GameObject fx = ResourceManager.Instance.LoadEffect("FX_touch_01");
        //    fx = Instantiate(fx);
        //    fx.layer = 5;
        //    fx.transform.parent = UIFxRoot;
        //    fx.transform.position = tapPos;
        //    // fx.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        //    //SGF.Debuger.Log(fx.gameObject.name);
        //    fx.transform.rotation = Quaternion.identity;
        //    fx.AddComponent<GameDestroyItem>();
        //    fx.GetComponent<GameDestroyItem>().OnlyDestroyGameObjectDelay(ParticleSystemLength(UIFxRoot));
        //}

        //// private void RecycleFX(GameObject fx)
        //// {
        ////     fx.SetActive(false);
        ////     pool.Enqueue(fx);
        //// }
        //// private GameObject CreateFX()
        //// {
        ////     GameObject newFX = null;
        ////     if(pool.Count > 0)
        ////     {
        ////         newFX = pool.Dequeue();
        ////     }
        ////     else
        ////     {
        ////         newFX = ResourceManager.Instance.LoadEffect(name);
        ////     }
        ////     return newFX;
        //// }
        ///// Fun1
        ///// 通用特效加载放置，池化管理/和删除销毁管理
        ///// ArtHelper目录下查看
        ///// 特效直接在场景中指定位置用WorldPos
        ///// Name.pos，时间 inst perfab--inst--FxRoot
        //public void LoadEffect(Vector3 pos, string name, bool isAuotDestory)
        //{
        //    GameObject Effobj = ResourceManager.Instance.LoadEffect(name);
        //    Effobj = Instantiate(Effobj);
        //    Effobj.transform.parent = FxRoot;
        //    Effobj.transform.position = pos;
        //    // Effobj.GetComponentInChildren<ParticleSystem>().Play();
        //    Effobj.AddComponent<GameDestroyItem>();
        //    if (isAuotDestory)
        //    {
        //        Effobj.GetComponent<GameDestroyItem>().OnlyDestroyGameObjectDelay(ParticleSystemLength(FxRoot));
        //    }
        //}


        //public Action NeedInvokeDalay;

        ///// <summary>
        ///// 给定特效父级加载特效
        ///// </summary>
        ///// <param name="parenttf"></param>
        ///// <param name="name"></param>
        ///// <param isAuotDestory="isAuotDestory"></param> 是否自动销毁
        ///// <param Callback="Callback"></param> 需要在特效播放完之后调用的方法
        //public void LoadEffect(Transform parenttf, string name, bool isAuotDestory = false, Action Callback = null)
        //{
        //    GameObject Effobj = ResourceManager.Instance.LoadEffect(name);
        //    Effobj = Instantiate(Effobj);
        //    Effobj.transform.parent = parenttf;
        //    Effobj.transform.localPosition = Vector3.zero;
        //    Effobj.transform.localScale = Vector3.one;
        //    ;
        //    Effobj.GetComponentInChildren<ParticleSystem>().Play();
        //    Effobj.AddComponent<GameDestroyItem>();
        //    if (isAuotDestory)
        //    {
        //        Effobj.GetComponent<GameDestroyItem>().OnlyDestroyGameObjectDelay(ParticleSystemLength(parenttf));
        //        if (Callback != null)
        //        {
        //            NeedInvokeDalay = Callback;
        //            //不用反射-程序集指定;也没需要手写协程
        //            Invoke("InvokeDalayFunction", ParticleSystemLength(parenttf));
        //        }
        //    }
        //}
        ///// <summary>
        ///// 田地点击特效
        ///// </summary>
        ///// <returns></returns>
        //public GameObject LoadEffectPlanting(Vector3 pos, string name, bool isAuotDestory)
        //{
        //    GameObject Effobj = ResourceManager.Instance.LoadEffect(name);
        //    Effobj = Instantiate(Effobj);
        //    Effobj.transform.parent = FxRoot;
        //    Effobj.transform.position = pos;
        //    // Effobj.GetComponentInChildren<ParticleSystem>().Play();
        //    Effobj.AddComponent<GameDestroyItem>();
        //    if (isAuotDestory)
        //    {
        //        Effobj.GetComponent<GameDestroyItem>().OnlyDestroyGameObjectDelay(ParticleSystemLength(FxRoot));
        //    }
        //    return Effobj;
        //}

        //public void InvokeDalayFunction()
        //{
        //    if (NeedInvokeDalay != null)
        //    {
        //        NeedInvokeDalay.Invoke();
        //    }

        //    NeedInvokeDalay = null;
        //}


        //public GameObject LoadGuidEffect(Transform parenttf, string name)
        //{
        //    GameObject Effobj = ResourceManager.Instance.LoadEffect(name);
        //    Effobj = Instantiate(Effobj);
        //    Effobj.transform.parent = parenttf;
        //    Effobj.transform.localPosition = Vector3.zero;
        //    Effobj.transform.localScale = Vector3.one;
        //    return Effobj;
        //}

        ///// <summary>
        ///// 返回特效Gameobject加载特效
        ///// </summary>
        ///// <param name="pos"></param>
        ///// <param name="name"></param>
        ///// <returns></returns>
        //public GameObject LoadEffect(Vector3 pos, string name)
        //{
        //    GameObject Effobj = ResourceManager.Instance.LoadEffect(name);
        //    Effobj = Instantiate(Effobj);
        //    Effobj.transform.parent = FxRoot;
        //    Effobj.transform.position = pos;
        //    return Effobj;
        //}

        ///// <summary>
        ///// 计算特效播放一次的时间
        ///// </summary>
        ///// <param name="transform"></param>
        ///// <returns></returns>
        //public static float ParticleSystemLength(Transform transform)
        //{
        //    ParticleSystem[] particleSystems = transform.GetComponentsInChildren<ParticleSystem>();
        //    float maxDuration = 0;
        //    foreach (ParticleSystem ps in particleSystems)
        //    {
        //        if (ps.enableEmission)
        //        {
        //            if (ps.loop)
        //            {
        //                return -1f;
        //            }

        //            float dunration = 0f;
        //            if (ps.emissionRate <= 0)
        //            {
        //                dunration = ps.startDelay + ps.startLifetime;
        //            }
        //            else
        //            {
        //                dunration = ps.startDelay + Mathf.Max(ps.duration, ps.startLifetime);
        //            }

        //            if (dunration > maxDuration)
        //            {
        //                maxDuration = dunration;
        //            }
        //        }
        //    }

        //    return maxDuration;
        //}


        /// Fun2
        /// 父节点为某一个物体的防止
    }
}