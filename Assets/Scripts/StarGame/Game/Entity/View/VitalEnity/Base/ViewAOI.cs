using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Reign;
using SGF.Time;
using SGF.Unity;
using SkillEditor;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.RemoteDynamic;
using StarProject.Game.Skill;
using StarProject.Service.LocalData;
using StarProject.Service.LocalDynamic;
using StarProject.Service.LocalDynamic.Fx;
using StarProject.Service.WorldToUI;
using StarProjectDef;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StarProject.Game.Entity.View.VitalSign
{
    public abstract class ViewAOI : ViewModel
    {
        private string TagFlag = "[ViewAOI]";
        public static StringBuilder sb = new();

        protected AOIEntityObject m_entity;

        public override string ModelPath
        {
            get
            {
                if (M_EntityBase.modelDataCell != null)
                {
                    return M_EntityBase.modelDataCell.ModelsPath;
                }
                return "";
            }
        }


        // 抽象到每个角色理论上都能播放声音（虽然不是所有Entity都能播放动画）
        // 基础通知到wwise
        // 提前获取所有Event（如果有动画的话）  vitalstate通知我动画切换，我获取所有事件帧，我自己内化通过时间 去转发给wwise了
        // 配置 动画解扣 配置和状态驱动解耦合 我 和 wwise也解耦
        // 声音是表现，这么些更方便
        // animationClip.events数组通过viaalstate 给我就好了 其它我来处理

        /// <summary>
        /// 获取实体ID
        /// </summary>
        public ulong EntityID
        {
            get
            {
                if (m_entity == null)//new 和 override 都不行，检测空和非空都是他，说明内存中存了两份，分别检测了不同的东西
                {
                    return 0;
                }
                return m_entity.EntityId;
            }
        }

        /// <summary>
        /// 获取实体
        /// </summary>
        public AOIEntityObject M_EntityBase
        {
            get
            {
                return m_entity;
            }
        }

        [SerializeField]
        protected Vector3 m_EntityPosition;

        /// <summary>
        /// 动态帧率 60~70
        /// </summary>

        public Vector3 moveMotion;



        /// <summary>
        /// 模型跟随移动但不跟随旋转节点. 
        /// note:
        ///     目前 人身上的特效, 跟 gl的 约定都是跟随人， 但可以 不随人物旋转,
        ///     所以,会在 人物 create的时候 创建一个 m_ModelNoRotationRoot
        /// </summary>
        protected Transform m_ModelNoRotationRoot;

        protected Vector3 m_EntityRotate = Vector3.zero;
        protected Vector3 ModelRotOffsetConf = Vector3.zero;


        //?????[·??/???] -> ????????? -> ???? ??| ????????? | ??????
        public TweenerCore<Vector3, Vector3, VectorOptions> LerpMover1;

        public TweenerCore<float, float, FloatOptions> LerpRotate1;

        private float _verticalVelocity = 0f;
        protected E_AirState e_AirState = E_AirState.GriDownSpeedOnGround;
        protected virtual E_AirState GetE_AirState(bool isGround) { return e_AirState; }
        protected bool M_isSkillMove = false;   // 技能位移的Tag标签
        private bool isBornGroundUp = true;//出生服务器给的是地面上还是地面下的两种情况划分
        //protected bool isReleased = false;

        #region 重力相关
        /*  public float Speed = 12f;
          public float Gravity = 20f;
          private Vector3 m_Velocity;

          //public Transform GroundCheck;//需要检测和是否和地面接触的物体
          public float GroundDistance;
          public LayerMask GroundMask = LayerMask.NameToLayer("Ground");*/

        protected bool M_IsStayInAir = false;	// 是否忽略重力


        private Vector3 downVector3 = Vector3.zero;
        private float oldCacheVerticalVelocity;
        private float newCacheVerticalVelocity;//这个缓存的意义是调用速度的get会浪费性能，错误更新
        protected bool ForceSetPosState = false;//强拉不等于位移，媚光的标变成人是位移

        /*    protected NativeArray<RaycastCommand> raycastRaycastCommandNativeArray;
            protected NativeArray<RaycastHit> raycastHitNativeArray;
            protected JobHandle jobHandle;*/

        Vector3 recordPos = Vector3.back;
        protected E_EntityGState keepFirstToGround = E_EntityGState.Default;
        //设置坐标后需要初始化状态
        protected void InitStatus()
        {
            recordPos = Vector3.back;
            keepFirstToGround = E_EntityGState.Default;
        }
        /// <summary>

        /// Every Fix Frame
        /// </summary>
        /// <returns>true 允许位移</returns>
        bool AllowYChange(bool isGround)
        {
            bool AllowYChange = false;

            //空不允许位移
            if (transform == null)
            {
                return AllowYChange;
            }



            if ((keepFirstToGround == E_EntityGState.nonTouchGroundDrop || keepFirstToGround == E_EntityGState.Default) && isGround)
            {
                keepFirstToGround = E_EntityGState.readyTouchGroundOnceStop;
            }

            /// 0到0.15f之间 地面不允许位移。

            if (isGround)
            {
                AllowYChange = false;
                //【记录标记1】，首次在地面会记录位置！
                recordPos = transform.localPosition;
            }
            else  /// 0.15f以上 允许位移
            {
                // 首次可以位移 1 次
                if (recordPos == Vector3.back)
                {
                    //首次非地面一定会进入到这里
                    keepFirstToGround = E_EntityGState.nonTouchGroundDrop;

                    recordPos = transform.localPosition;
                    //以前考虑的都是地面上的问题现在有出生就在地面下的情况
                    if (isBornGroundUp)
                    {
                        AllowYChange = true;
                    }
                    else
                    {
                        AllowYChange = false;
                        isBornGroundUp = true;
                    }

                }
                // X Z 有变化
                else if (recordPos.x != transform.localPosition.x || recordPos.z != transform.localPosition.z)
                {
                    recordPos = transform.localPosition;
                    AllowYChange = true;
                }
                else //没变化不能位移
                {

                    //如果在空中，执行到第二次了，ZX没变化 就不会掉落了。为了解决这个问题，首次让他掉落

                    switch (keepFirstToGround)
                    {
                        //首次在地面，记录了数据。被别人Y轴踢飞这时候就到这里了
                        case E_EntityGState.Default:
                            AllowYChange = false; //这时候设定是不管他掉落了
                            break;
                        case E_EntityGState.nonTouchGroundDrop:
                            AllowYChange = true;
                            break;
                        case E_EntityGState.readyTouchGroundOnceStop:
                            if (M_EntityBase.Data.isMainPlayer)
                            {
                                AllowYChange = true;
                            }
                            else
                            {
                                AllowYChange = false;
                            }

                            break;
                        default:
                            break;
                    }
                }
            }






            return AllowYChange;
        }

        /// <summary>
        /// 技能设定不会用服务器强拉的
        ///  不强制设置位置，正常行走。
        /*隐藏时候可能移动位置，移动就需要重力判断
                【隐身漫步】怎么走，隐身过程走 1，buff = shader半透明；2，隐藏meshrender；3，模型替换 + buff
                ，还是隐身闪烁是 = 强制设置位置*/
        /// </summary>
        protected bool LogicSignShow = true;

        /// <summary>
        /// 还是隐身闪烁是 = 强制设置位置
        /// 人变成飞镖进行移动
        /// </summary>
        protected bool BattleDesignShow = true;

        private int newCreateCount = 0;

        private List<Action> regWaitLoadCompleteCB = new();
        /// <summary>
        /// 30fps
        /// </summary>
        public void OnFixUpdate()
        {
            if (ForceSetPosState == false)//强制设置pos的时候要暂停重力模拟
            {
                if (BattleDesignShow)
                {
                    //默认值
                    if (LogicSignShow == false)
                    {
                        return;
                    }
                }
                else
                {
                    //战斗隐藏
                    //逻辑显示不参与控制
                    //所以走到下面需要被重力控制
                }

                // 如果是模拟运动的话, 就不需要收到重力的影响
                if (M_EntityBase != null && (M_EntityBase.IsSimulateMove || M_EntityBase.EntityType == E_EntityType.BulletEntity))
                {
                    return;
                }



                if (M_IsStayInAir == false)
                {
                    //并行，前置逻辑，每帧必然执行一次
                    //如已再地面状态就不必执行，那么再空中也不知道


                    IsUnderGround();
                    bool isGround = IsGrounded();
                    float newSpeed = UpdateVerticalVelocity(isGround);
                    //0.05f 检测地面
                    //loading不进行任何移动 速度是 0 
                    //首次地面速度是 -0.01
                    if (isOnloading >= 1)
                    {
                        //恢复初始化
                        InitStatus();
                    }
                    else
                    {

                        if (AllowYChange(isGround))
                        {
                            //一个是速度越来越大，一个是场景没加载好
                            newCacheVerticalVelocity = newSpeed;
                            if (newCacheVerticalVelocity != 0)
                            {
                                if (oldCacheVerticalVelocity > newCacheVerticalVelocity)
                                {
                                    //V2是长时间的绝对值是大的是后来的，V1是短时间的绝对值是小的是缓存的，
                                    //S差量向量 = (V2^2 - V1^2)/20   *  -1   说明  [1/2 * 1/g]
                                    downVector3.y = (Mathf.Pow(newCacheVerticalVelocity, 2) - Mathf.Pow(oldCacheVerticalVelocity, 2)) * -0.05f;
                                    OnAOIObjectMoveing(downVector3);
                                    //是不是应该给属性赋值
                                }
                                else if (oldCacheVerticalVelocity == newCacheVerticalVelocity && newCacheVerticalVelocity == -0.1f)
                                {
                                    downVector3.y = -0.03f;
                                    OnAOIObjectMoveing(downVector3);


                                    //transform.localPosition += downVector3;
                                }
                            }
                            oldCacheVerticalVelocity = newCacheVerticalVelocity;
                        }
                    }
                }



            }
        }

        private Vector3 tempVectorChecker;
        Ray ray = new(Vector3.zero, Vector3.down);
        float rayLength = 400f;
        float height = 0f;
        /// <summary>
        /// 检测角色高度合法性：
        /// //地下不合法，地上合法，地中正常
        /// terrain + item  = 碰撞
        /// terrain = gpui
        /// terrain = 还可以用在合法性判断上 
        /// terrain 需要配置 不差这份内存
        /// </summary>
        private void IsUnderGround()
        {
            if (GameManager.Instance != null && GameManager.Instance.M_Map != null && GameManager.Instance.M_Map.M_rootHelper != null)
            {
                height = 0f;
                if (GameManager.Instance.M_Map.M_rootHelper.terrain != null)
                {
                    Vector3 targetPos = transform.position;
                    height = GameManager.Instance.M_Map.M_rootHelper.terrain.SampleHeight(targetPos);
                    if (GameManager.Instance.M_Map.M_rootHelper.terrain.transform.position != Vector3.zero)
                    {
                        height = height + GameManager.Instance.M_Map.M_rootHelper.terrain.transform.position.y;
                    }
                }
                else
                {
                    tempVectorChecker.y = 200f;
                    tempVectorChecker.z = transform.position.z;
                    tempVectorChecker.x = transform.position.x;
                    ray.origin = tempVectorChecker;
                    RaycastHit hitInfo;
                    if (Physics.Raycast(ray, out hitInfo, rayLength, LayerMask.GetMask("Ground")))
                    {
                        // 如果射线检测到"Ground"层次的碰撞点，则将其 Y 坐标赋给 height
                        height = hitInfo.point.y;
                    }
                    else
                    {
                        //0;
                    }
                }

                if (transform.localPosition.y - height < -0.5f)
                {
                    //不合法情况
                    //初始化
                    //Y速度 = 0
                    InitStatus();
                    transform.localPosition = new Vector3(transform.localPosition.x, height + 0.5f, transform.localPosition.z);
                    isBornGroundUp = false;
                }

            }
        }

        //调用掉落速度，自动先更新自己状态，自动先检测碰撞地面
        //只是被调用一次给重力，
        //谨慎使用会调用一堆甚至物理，不要update调用甚至别调用，只给物理要用就找我
        //[[[反正现在都客户端进行计算，NPC，主角，thd，物件，怪物无所谓，我自己电脑的thd是按照我的重力来算的]]]
        //垂直速度和水平分开计算
        protected float UpdateVerticalVelocity(bool isGround)
        {

            if (isOnloading >= 1)
            {
                _verticalVelocity = 0;
            }
            else
            {
                //外层控制是，不等于0就会一直掉落，除了StayInAir不会处理
                switch (GetE_AirState(isGround))
                {
                    case E_AirState.VerySmallDownSpeed://默认无重力
                        _verticalVelocity = -10 * Time.fixedDeltaTime;//说是没重力，其实要少有一点，防止检测过长或者上坡的时候，但是不会递增
                        break;
                    case E_AirState.GriDownSpeedOnGround://地面有重力
                        _verticalVelocity += Physics.gravity.y * Time.fixedDeltaTime; //但是这个就很大
                        break;
                    case E_AirState.StayInAir://默认浮空，之后可以策划配置，但是目前物件都设定为悬空
                        _verticalVelocity = 0;
                        break;
                    case E_AirState.GriDownSpeed:
                        //重力加速度 Vt = 0 + at
                        //通常离散时间，调用点（递增点为）1秒
                        //每次1/30秒，增加1/30份向下速度，虽离散但更平滑的处理即刻下降速度，累加确定向下即时速度Vt
                        //每次间隔1/N秒，共调用N次，总系数为1，调用时总距离就是最后【1秒的Vt】，每次调用就是【此帧刻的Vt】
                        _verticalVelocity += Physics.gravity.y * Time.fixedDeltaTime;
                        //-9.8f
                        //0.033f
                        break;
                    case E_AirState.InitSpeed://默认无重力
                        _verticalVelocity = -0.1f;//说是没重力，其实要少有一点，防止检测过长或者上坡的时候，但是不会递增
                        break;
                    default:
                        break;
                }
            }
            return _verticalVelocity;

        }
        #endregion

        public Action<Vector3> OnAngelChange; // 角度改变委托-》用于影子和倒影

        /// <summary>
        /// 物体身上 正在播放的 特效记录
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="FxGeParam "></typeparam>
        public DictionaryEx<string, FxGeParam> gameEffctDic = new();

        /// <summary>
        /// 播放特效时,生产的绑点的记录
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="Transform"></typeparam>
        public DictionaryEx<string, Transform> gamePointDic = new();


        protected int isOnloading = 0;
        /// <summary>
        /// 需要替换的 meshRender
        /// </summary>
        protected List<SkinnedMeshRenderer> m_SkinMeshRender = new();

        protected Dictionary<string, BeAttackFlashColor> m_PlayBeAttackFlashColorDic = new();

        protected List<CharacterFadeOutController> m_CharacterFadeOuts = new();

        protected CharStateController charStateController;

        private Vector3 ModelPosOffsetConf = Vector3.zero;  // 模型配置的偏移

        protected override void Create(EntityObject entity)
        {
            base.Create(entity);
            m_entity = (AOIEntityObject)entity;

            m_ModelOffset = transform.Find("ModelOffset");
            M_IsStayInAir = m_entity.IgnoreGravity;
            m_InitLayer = gameObject.layer;
            RefreshDefaultAnimCfg();

            CreateModelNoRotationRoot();
            OnEventListener();
            InitAvatarModelPosOffset();
            InitAvatarModelRotOffset();

            // 先同步一次 服务器出生点坐标
            // m_entity.ActionOnSyncBorthPos?.Invoke();

            //CreateModel();
            OnAngelChangeMove(M_EntityBase.EulerAngles.y, false, 0, false);

            OnBirthPos(M_EntityBase.ServerPosition);
            Action<bool> modelFinish = (res) =>
            {
                if (!res)
                {
                    if (m_entity != null)
                    {
                        SGF.Debuger.LogWarning($"{TagFlag} Create() entityid={m_entity.EntityId},entitytype={m_entity.EntityType},ModelPath={ModelPath},模型加载失败 err!!!");
                        m_entity.ActionOnViewCreateFinish?.Invoke();
                        CheckCreateFootprint();
                    }
                    else
                    {
                        SGF.Debuger.LogError($"{TagFlag} Create() 实体都没了模型加载失败 err!!!");
                    }
                    return;
                }
                SetModelScale();
                InitSkinMeshRender();
                if (m_entity.EntityType == E_EntityType.BulletEntity)
                {
                    SGF.UI.Framework.UIUtilsFrameWork.ChangeLayer(transform, E_LayerType.Bullet.ToString());
                }
                else
                {
                    SGF.UI.Framework.UIUtilsFrameWork.ChangeLayer(transform, E_LayerType.Entity.ToString());
                }

                m_entity.ActionOnViewCreateFinish?.Invoke();



                CheckCreateFootprint();
            };
            CreateModelAsync(modelFinish);
            /*     if (!raycastRaycastCommandNativeArray.IsCreated)
                 {
                     raycastRaycastCommandNativeArray = new NativeArray<RaycastCommand>(1, Allocator.Persistent);
                 }
                 if (!raycastHitNativeArray.IsCreated)
                 {
                     raycastHitNativeArray = new NativeArray<RaycastHit>(1, Allocator.Persistent);
                 }
     */

            //isReleased = false;
        }

        protected virtual void OnEventListener()
        {
            if (m_entity != null)
            {
                m_entity.ActionOnBirthPos += OnBirthPos;
                m_entity.DoPathMove += OnDoPathMove;
                m_entity.DoAngelChange += OnAngelChangeMove;
                m_entity.DoForceMove += OnForceMove;
                m_entity.DoThdPsnMove += OnThdPersionMove;
                m_entity.ActionStopMoveDotween += OnActionStopMoveDotween;

                m_entity.ActionOnStartHidden += HideModel;
                m_entity.ActionOnStopHidden += ShowModel;
                m_entity.ActionOnUpdateBoxScale += UpdateBoxScaleRatio;

                m_entity.ActionOnViewOffectY += OnActionOnViewOffectY;

                m_entity.ActionOnSwitchModel += OnActionSwitchModel;
            }

            //本帧active == false 逻辑还会走一次物理检测，所以用late会再次空执行一次    
            //报错是数据为空，所以先进行完物理检测，然后模型删除就没问题
            MonoHelper.AddFixedUpdateListener(OnFixUpdate, MonoHelper.E_ModuleType.CommonService);//碰撞也是逻辑层，不算表现要30fps

            GlobalEvent.IsBeginLoadingOrEnd.AddListener(isOnLoading);
        }

        protected virtual void OffEventListener()
        {
            if (m_entity != null)
            {
                m_entity.ActionOnBirthPos -= OnBirthPos;
                m_entity.DoPathMove -= OnDoPathMove;
                m_entity.DoAngelChange -= OnAngelChangeMove;
                m_entity.DoForceMove -= OnForceMove;
                m_entity.DoThdPsnMove -= OnThdPersionMove;

                m_entity.ActionStopMoveDotween -= OnActionStopMoveDotween;

                m_entity.ActionOnStartHidden -= HideModel;
                m_entity.ActionOnStopHidden -= ShowModel;
                m_entity.ActionOnUpdateBoxScale -= UpdateBoxScaleRatio;


                m_entity.ActionOnViewOffectY -= OnActionOnViewOffectY;

                m_entity.ActionOnSwitchModel -= OnActionSwitchModel;
            }

            MonoHelper.RemoveFixedUpdateListener(OnFixUpdate, MonoHelper.E_ModuleType.CommonService);//碰撞也是逻辑层，不算表现要30fps

            GlobalEvent.IsBeginLoadingOrEnd.RemoveListener(isOnLoading);
        }

        private void isOnLoading(bool arg0)
        {
            //掉落问题测试简述
            //关闭loading都收到了，全局没有true都是false,正常应该成对儿出现,但是开启loading的时候没有实体所以看不出来又true的时候，所以全部状态正确false，结果正确，所有人都正确
            //true和false 出现的 现象只有一次，确实不会反复，（找不到别的办法了要不然就不用loading状态，因为顺序可能有问题）
            //策略：不关心先后只关心结果 - 代码

            /*第一个场景====================
             * 其他false
            主角有false
            （其他都是删除）离开aoi，
            ========================================
            主角 - true
            ========================================
            主角 - false
            其他又是新增 false - 结果对，id无重复（服务器设定不会重复）。多次调用只是顺序有问题主角状态对就行了，false控制并没有顺序问题（结果上来看）
             ========================================
             黑盒现象：主角可以掉落，npc能踩在地面上
              ========================================
             */

            if (arg0)
            {
                isOnloading++;//1~n loading
            }
            else
            {
                isOnloading--;//0~-n 非loading
            }
            //Debug.Log(EntityID + "IsLoading" + arg0);
            //isOnloading = false;//备选方案
        }

        private void CreateModelNoRotationRoot()
        {
            // 创建不旋转的特效绑点
            if (m_ModelNoRotationRoot == null)
            {
                GameObject ModelNoRotationRoot = new("m_ModelNoRotationRoot");
                m_ModelNoRotationRoot = ModelNoRotationRoot.transform;
                m_ModelNoRotationRoot.parent = transform;

            }
        }


        private void CreateModelAsync(Action<bool> createFinish = null)
        {
            // 数据层都没了, 那就不创建 了
            if (M_EntityBase == null)
            {
                createFinish?.Invoke(false);
                return;
            }

            Action<bool> acFinish = (bool result) =>
                {
                    createFinish?.Invoke(result);

                    // 刷新完模型后, 执行所有需要等待 模型刷新完成的回调
                    ExecuteWaitLoadCompleteCB();
                };
            // 子弹没有模型，只有壳子
            if (M_EntityBase.Data.hasModel)
            {
                RefreshModelAsync(acFinish);
            }
            else
            {
                /// 2024/8/2
                /// fixed 子弹移动和朝向不对的bug
                ///     狗曲删除 viewModel.cs 中 m_ModelOffLineData 为null 会从子节点中获取 的逻辑，导致 M_ModelOffLineData 为null
                /// 
                {
                    if (M_ModelOffLineData == null)
                    {
                        M_ModelOffLineData = transform.GetComponentInChildren<OffLine.ModelOffLineData>();
                    }
                }

                acFinish?.Invoke(false);
            }
        }

        private void RefreshModelAsync(Action<bool> callback)
        {
            string tag = "";
            // 模型的标签 tag 规则:
            // 先检查 是否有 modelDataCell, 如果有, 那不管这个资源 能不能通过这个路径加载到, tag 都是配置的路径;
            // 如果 modelDataCell 没有, 就按 实体类型判定。
            // 由上, 不管 资源有没有, 其实 tag 是完全确定的. 那么在切换模型的时候, 可以不销毁模型, 根据tag 查找之前是否缓存了这个 model
            sb.Clear();
            if (M_EntityBase.modelDataCell != null)
            {
                tag = sb.Append(M_EntityBase.modelDataCell.ModelsPath).Replace(".prefab", string.Empty).Replace("Assets/Res/", string.Empty).ToString();
            }
            else
            {
                // 如果没有配置,那就是 走通用模型
                if (M_EntityBase.EntityType == E_EntityType.Player)
                {
                    tag = "1";
                }
                else
                {
                    tag = "0";
                }
            }
            //如果是中配以下（包含中配优先加载低模）这里先去拿_LOD1 拿不到再拿中模
            if ((GameConfig.modelType == ModelQualityLevel.Low || GameConfig.modelType == ModelQualityLevel.Middle) &&
                (M_EntityBase.EntityType == E_EntityType.Player || M_EntityBase.EntityType == E_EntityType.Partner))
            {
                //美术不会做的情况下，防止浪费O1的性能
                if (GameConfig.modelType == ModelQualityLevel.Middle)
                {
                    if (M_EntityBase.IsMainPlayer)
                    {
                        string lodPath = tag + "_LOD1";
                        if (Service.Resource.ResourceFormalManager.Instance.IsPathContains(lodPath, E_AssetType.Roles))
                        {
                            tag = lodPath;
                        }
                    }
                }
                else
                {
                    string lodPath = tag + "_LOD1";
                    if (Service.Resource.ResourceFormalManager.Instance.IsPathContains(lodPath, E_AssetType.Roles))
                    {
                        tag = lodPath;
                    }
                }


            }

            // 首先 关闭当前的 模型节点
            if (M_ModelOffLineData != null && M_ModelOffLineData.transform != null)
            {
                M_ModelOffLineData.transform.gameObject.SetActive(false);
            }

            GameObject gob = null;
            // 如果找到了 之前隐藏的模型,那就直接显示之前隐藏的模型就可以了
            if (ModelRecord.TryGetValue(tag, out GameObject gob2))
            {
                if (gob2 != null)
                {
                    gob = gob2;
                    gob.SetActive(true);
                }
                else
                {
                    // 如果节点被销毁了(之前切换模型 直接执行的 destroy), 就移除之前的记录
                    ModelRecord.Remove(tag);
                }
            }
            // 如果 找不到 相应的 模型子节点, 那就 新创建对应的模型
            if (gob == null)
            {
                Action<GameObject> cb = (go) =>
                {
                    if (go != null)
                    {
                        LoadModelCallBack(go, tag);
                    }
                    callback?.Invoke(go != null);
                };
                CreateModelAsync(tag, cb);
            }
            else
            {
                LoadModelCallBack(gob, tag);
                callback?.Invoke(true);
            }
        }

        private void LoadModelCallBack(GameObject gob, string tag)
        {
            if (gob == null)
            {
                SGF.Debuger.LogWarning($"{TagFlag} LoadModelCallBack() tag={tag},gob={gob},err!!!");
                return;
            }

            gob.transform.localEulerAngles = Vector3.zero;

            RefreshModelAnimancer(gob);

            RefreshModelOfflineData(gob);

            if (ModelRecord.ContainsKey(tag))
            {
                SGF.Debuger.LogWarning($"{TagFlag} LoadModelCallBack() ModelRecord 重复的key,tag={tag},entityid={m_entity.EntityId},entitytype={m_entity.EntityType},ModelPath={ModelPath}");
                //Debug.Break();
            }
            else
            {
                ModelRecord.Add(tag, gob);
            }

            #region 动态修改材质的renderqueue值
            List<Renderer> rendererList = new();
            foreach (Transform child in gob.transform)
            {
                Renderer childRenderer = child.GetComponent<Renderer>();
                if (childRenderer != null)
                {
                    rendererList.Add(childRenderer);
                }
            }
            E_OutlineEntityType istype = E_OutlineEntityType.StaticItem;//伙伴是怪物或者子弹
            if (this is ViewVitalHeroNormal) //当前对象是 B 类或其子类的实例,C#也为了你能优雅
            {
                istype = E_OutlineEntityType.Hero;
                if (M_EntityBase.Data.isMainPlayer)
                {
                    istype = E_OutlineEntityType.MainPlayer;
                }
            }
            else if (this is ViewVitalMonsterNormal)
            {
                istype = E_OutlineEntityType.Monster;
            }
            else if (this is ViewVitalSummonNormal)
            {
                istype = E_OutlineEntityType.Summon;
            }
            else if (this is ViewVitalGameNPCNormal)
            {
                istype = E_OutlineEntityType.NPC;
            }
            OutlineActiveCount = DynamicRenderQueueManager.Instance.AddNewRender(rendererList.ToArray(), istype);

            #endregion

            // 头顶信息移动创建坑位
            //if (m_entity.Data != null && m_entity.Data.isMainPlayer)
            //{
            //    SGF.Debuger.LogError($"名字测试 主角创建 2坑位3D填充");
            //}

            WorldItemChecker.Instance.RegToRoleTrans(EntityID, gob.transform);

            SelfParticleSystem = gob.GetComponent<ParticleSystem>();

            gob.SetActive(true);

            // 刷新完模型后, 执行所有需要等待 模型刷新完成的回调
            // ExecuteWaitLoadCompleteCB();
        }

        private void SetModelScale()
        {
            Vector3 scale = Vector3.one * m_entity.ModleScale;
            if (Animancer != null)
            {
                //float scaleCur = 1;
                //if (scaleCfg != 1)
                //{
                //    scaleCur = 1 - ((scaleCfg - 1) / scaleCfg);
                //}
                //Animancer.transform.SetLocalScale(new Vector3(scaleCur, scaleCur, scaleCur));

                Animancer.transform.SetLocalScale(scale);
                // 模型scale 发生变化的时候， 更新 特效的scale 大小
                UpdateGeEffectScale();
            }

            if (SelfParticleSystem != null)
            {
                foreach (var item in SelfParticleSystem.gameObject.GetComponentsInChildren<ParticleSystem>())
                {
                    item.transform.SetLocalScale(scale);
                }
            }
        }

        private void InitSkinMeshRender()
        {
            m_SkinMeshRender.Clear();
            m_SkinMeshRender = m_ModelOffset.GetComponentsInChildren<SkinnedMeshRenderer>().KToList<SkinnedMeshRenderer>();

            m_CharacterFadeOuts.Clear();
            m_CharacterFadeOuts = GetComponentsInChildren<CharacterFadeOutController>().KToList<CharacterFadeOutController>();

            // 如果之前的 模型存在， 那就先还原之前的 模型shader 状态
            if (charStateController != null)
            {
                charStateController?.ReturnNormal();
            }
            charStateController = GameObjectUtils.EnsureComponent<CharStateController>(m_ModelOffset.gameObject);
            charStateController.SetMeshRenderer();
            charStateController.SetNormalMat();
            // 先把一些 shader 的默认参数存储下来
            {
                InitEdgeLightDefaultValues();

                InitTranslucentDefaultValues();
            }

        }

        private void ClearSkinMeshRender()
        {
            m_SkinMeshRender.Clear();
            m_CharacterFadeOuts.Clear();
            StopTranslucentTween();
        }

        /// <summary>
        /// 存储的 材质中默认 的 参数值
        /// 由于 shader 改变的 是 模型下面 所有的 SkinnedMeshRenderer 的相关参数, 所以 会存入 相应 render 对应的 hashcode 和 与它相关的 参数值
        /// </summary>
        protected Dictionary<GlobalShowType, Dictionary<SkinnedMeshRenderer, object>> shaderDefaultValues = new();

        public void HandleRenders(Action<SkinnedMeshRenderer> action)
        {
            foreach (var render in m_SkinMeshRender)
            {
                if (render != null && render.material != null)
                {
                    action?.Invoke(render);
                }
            }
        }

        /// <summary>
        /// 得到 GlobalShowType 相关联的 SkinnedMeshRenderer 数据 和它的 默认值
        /// </summary>
        /// <param name="globalShowType"></param>
        public Dictionary<SkinnedMeshRenderer, object> GetGlobalShowTypeMeshRenders(GlobalShowType globalShowType)
        {
            if (shaderDefaultValues.TryGetValue(globalShowType, out var values))
            {
                return values;
            }

            return null;
        }
        // 半透的 效果tween
        public DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions> translucentTween;
        /// <summary>
        /// 初始化 半头效果的默认 值
        /// </summary>
        private void InitTranslucentDefaultValues()
        {
            var globalShowType = GlobalShowType.BUFF_TransChange;

            // 先清理 旧值
            shaderDefaultValues.Remove(globalShowType);

            // 先初始化 半透明的默认值
            var values = new Dictionary<SkinnedMeshRenderer, object>();

            HandleRenders((SkinnedMeshRenderer render) =>
            {
                Dictionary<Material, object> renderParams = new();

                // 半透效果 需要操作 身上所有材质的特效
                for (int i = 0; i < render.materials.Length; i++)
                {
                    Material material = render.materials[i];
                    if (!material.shader.name.Contains("CartoonChar_4"))
                    {
                        return;
                    }
                    List<object> materialParam = new();

                    if (material.shader.name.Contains("CartoonChar_4_Line"))
                    {
                        // 角色的 alpha
                        materialParam.Add(material.GetFloat("_OutlineAlpha"));
                    }
                    else
                    {
                        // 角色的 alpha
                        materialParam.Add(material.GetFloat("_Alpha"));
                    }

                    // 角色的渲染队列
                    materialParam.Add(material.renderQueue);

                    renderParams.Add(material, materialParam);
                }

                values.Add(render, renderParams);
            });

            shaderDefaultValues.Add(globalShowType, values);
        }

        public void StopTranslucentTween()
        {
            if (translucentTween != null)
            {
                translucentTween.Kill();
            }
        }
        public void ResetDefaultTranslucent()
        {
            // 还原之前 先停止 tween
            StopTranslucentTween();

            var globalShowType = GlobalShowType.BUFF_TransChange;
            // 获取最小的值
            if (shaderDefaultValues.TryGetValue(globalShowType, out var values))
            {
                foreach (var item in values)
                {
                    // 一个 材质 对应的 它的默认参数
                    var material2Values = item.Value as Dictionary<Material, object>;

                    foreach (KeyValuePair<Material, object> itemValue in material2Values)
                    {
                        List<object> materialParam = itemValue.Value as List<object>;
                        if (itemValue.Key.shader.name.Contains("CartoonChar_4_Line"))
                        {
                            itemValue.Key.SetFloat("_OutlineAlpha", (float)materialParam[0]);
                        }
                        else
                        {
                            itemValue.Key.SetFloat("_Alpha", (float)materialParam[0]);
                        }

                        // SGF.Debuger.LogError($"[viewAoi] 修改 renderQueue: {transform.name} , {(int)materialParam[1]}");
                        itemValue.Key.renderQueue = (int)materialParam[1];
                    }

                }
            }
        }


        private void InitEdgeLightDefaultValues()
        {
            var globalShowType = GlobalShowType.BUFF_ChangeColorShader;

            // 先清理 旧值
            shaderDefaultValues.Remove(globalShowType);

            // 先初始化 默认的外发光的值
            Dictionary<SkinnedMeshRenderer, object> values = new();

            HandleRenders((render) =>
            {
                Dictionary<string, object> renderParams = new();

                var material = render.material;
                if (!material.shader.name.Contains("CartoonChar_4"))
                {
                    return;
                }
                renderParams.Add("_RimWidth", material.GetFloat("_RimWidth"));
                renderParams.Add("_RimColor", material.GetColor("_RimColor"));
                renderParams.Add("_MainColor", material.GetColor("_MainColor"));

                values.Add(render, renderParams);
            });

            shaderDefaultValues.Add(globalShowType, values);
        }

        public void ResetDefaultEdgeLight()
        {
            // 获取最小的值
            if (shaderDefaultValues.TryGetValue(GlobalShowType.BUFF_ChangeColorShader, out var values))
            {
                HandleRenders((render) =>
                {
                    // 还原默认材质的 值
                    values.TryGetValue(render, out var renderParams);

                    Dictionary<string, object> paras = renderParams as Dictionary<string, object>;
                    if (!render.material.shader.name.Contains("CartoonChar_4"))
                    {
                        return;
                    }
                    if (paras.TryGetValue("_RimWidth", out var v))
                    {
                        render.material.SetFloat("_RimWidth", (float)v);
                    }
                    if (paras.TryGetValue("_RimColor", out var v1))
                    {
                        render.material.SetColor("_RimColor", (Color)v1);
                    }
                    if (paras.TryGetValue("_MainColor", out var v2))
                    {
                        render.material.SetColor("_MainColor", (Color)v2);
                    }

                });
            }
        }

        private void InitAvatarModelPosOffset()
        {
            ModelPosOffsetConf = Vector3.zero;
            if (m_entity != null && m_entity.avatarDataCell != null)
            {
                var posOffset = m_entity.avatarDataCell.ModelPosOffset;
                if (posOffset != null && posOffset.Count >= 3)
                {
                    ModelPosOffsetConf.x = (float)posOffset[0] / 100;
                    ModelPosOffsetConf.y = (float)posOffset[1] / 100;
                    ModelPosOffsetConf.z = (float)posOffset[2] / 100;
                }
            }
            UpdateModelOffsetPos();
        }

        private void InitAvatarModelRotOffset()
        {
            ModelRotOffsetConf = Vector3.zero;
            if (m_entity != null && m_entity.avatarDataCell != null)
            {
                int rotOffset = m_entity.avatarDataCell.GetModelRotOffset();
                ModelRotOffsetConf.y = rotOffset;
            }
            SetEntityAngle(ModelRotOffsetConf.y);
        }

        /// <summary>
        /// 不用这个判断，之后利用自己写的box判断，修改枚举
        /// 通过枚举来决策
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsGrounded()
        {
            return false;
        }

        protected override void Reset()
        {
            ShowModel(true);
            SetLayer(m_InitLayer);

            base.Reset();

            CloseAllFx();

            ClearSkinMeshRender();

            modelOffset = Vector3.zero;
            SimulateWorldOffset = Vector3.zero;
            SimulateLocalOffset = Vector3.zero;
            ModelPosOffsetConf = Vector3.zero;

            m_EntityRotate = Vector3.zero;
            ModelRotOffsetConf = Vector3.zero;
            isOnloading = 0;
            // 清理晚上身上的 lineRender
            ReleaseLineRender();

            if (simulatFloatTween != null)
            {
                simulatFloatTween.Kill(false);
            }
            if (simulatMoveTween != null)
            {
                simulatMoveTween.Kill(false);
            }

            regWaitLoadCompleteCB.Clear();
        }

        public void ResetOnSwitchModule()
        {
            ShowModel(true);
            SetLayer(m_InitLayer);


            CloseAllFx();

            ClearSkinMeshRender();

            modelOffset = Vector3.zero;
            SimulateWorldOffset = Vector3.zero;
            SimulateLocalOffset = Vector3.zero;
            ModelPosOffsetConf = Vector3.zero;

            m_EntityRotate = Vector3.zero;
            ModelRotOffsetConf = Vector3.zero;
            isOnloading = 0;
            // 清理晚上身上的 lineRender
            ReleaseLineRender();

            if (simulatFloatTween != null)
            {
                simulatFloatTween.Kill(false);
            }
            if (simulatMoveTween != null)
            {
                simulatMoveTween.Kill(false);
            }

            regWaitLoadCompleteCB.Clear();
        }

        /// <summary>
        /// 切换模型接口
        /// </summary>
        public void SwitchModel(bool resetModel)
        {
            // 如果需要重置模型, 那应该是 角色创角的模型刷新,此时可以先 reset
            if (resetModel)
            {
                Reset();
            }
            else
            {
                // 如果不是创角模型这种, 那只需要 清除之前的模型就可以
                ResetOnSwitchModule();
            }
            RefreshDefaultAnimCfg();

            Action<bool> modelFinish = (res) =>
            {
                if (!res)
                {
                    SGF.Debuger.LogWarning($"{TagFlag} SwitchModel() ModelPath={ModelPath},模型加载失败 err!!!");
                    return;
                }

                SetModelScale();
                InitSkinMeshRender();
                InitAvatarModelPosOffset();
                InitAvatarModelRotOffset();
                if (m_entity.EntityType == E_EntityType.BulletEntity)
                {
                    SGF.UI.Framework.UIUtilsFrameWork.ChangeLayer(transform, E_LayerType.Bullet.ToString());
                }
                else
                {
                    SGF.UI.Framework.UIUtilsFrameWork.ChangeLayer(transform, E_LayerType.Entity.ToString());
                }
                m_entity.Data.ActionRefreshCurAnims?.Invoke();

            };
            CreateModelAsync(modelFinish);
        }

        public void RegisterAnimancerLoadComleteCB(Action ac)
        {
            // 如果 此时模型已经加载完毕, 那就不需要等待，立即执行
            if (_Animancer != null)
            {
                ac.Invoke();
            }
            else
            {
                // 如果模型还没有加载好, 那就等到模型加载完成, 再去触发执行
                regWaitLoadCompleteCB.Add(ac);
            }
        }

        private void ExecuteWaitLoadCompleteCB()
        {
            for (int i = 0; i < regWaitLoadCompleteCB.Count; i++)
            {
                var ac = regWaitLoadCompleteCB[i];
                try
                {
                    ac.Invoke();
                }
                catch (System.Exception e)
                {
                    SGF.Debuger.LogError($" ExecuteWaitLoadCompleteCB: {e.Message} , {e.StackTrace} ");
                }
            }

            // 执行完成后 清楚所有注册的 事件回调
            regWaitLoadCompleteCB.Clear();
        }

        private void OnActionSwitchModel(int avatarID, bool resetModel)
        {
            SwitchModel(resetModel);
        }

        protected override void Release()
        {
            OffEventListener();

            /*        this.jobHandle.Complete();
                    if (raycastRaycastCommandNativeArray.IsCreated && raycastRaycastCommandNativeArray.Length != 0)
                    {
                        this.raycastRaycastCommandNativeArray.Dispose();
                    }
                    if (raycastHitNativeArray.IsCreated && raycastHitNativeArray.Length != 0)
                    {
                        this.raycastHitNativeArray.Dispose();
                    }*/
            Reset();
            isOnloading = 0;
            base.Release();
            //isReleased = true;
        }

        #region 移动接口

        private void OnActionStopMoveDotween(string info)
        {
            //Debug.LogError(info);
            KillLerpMover_StopInPlace(); //手柄 和 客户端Nav寻路
        }

        /// <summary>
        /// 原地暂停
        /// </summary>
        /// <param name="isFroce"></param>
        public virtual void KillLerpMover_StopInPlace(bool isFroce = false)
        {

        }

        /// <summary>
        /// 表现出 物体移动的接口
        /// 如果人物的移动,那就是调用人物身上的 CharacterController.Move
        /// </summary>
        /// <param name="movingMotion"></param>
        public virtual void OnAOIObjectMoveing(Vector3 movingMotion)
        {

        }

        public Vector3 pathStartPos = Vector3.zero;

        /// <summary>
        /// 通用的 设置 view 节点坐标的 接口
        /// </summary>
        /// <param name="v3"></param>
        public void SetPosition(Vector3 v3)
        {
            transform.localPosition = v3;
        }

        /// <summary>
        /// 实际处理：目论如何都会逻辑停一下；上层处理决定[zero当前无目标/保持之前目标/Dequeue请求新的目标/Clear如不同步永远无目标]
        /// 方法意义：循环请求最新点坐标
        /// </summary>
        /// <param name="nextPoint"></param>
        public void OnDoPathMove(Vector3 nextPoint)
        {
            if (!M_EntityBase.M_IsAlive)
            {
                return;
            }

            if (nextPoint != Vector3.zero)
            {
                KillLerpMover_StopInPlace();
                M_isSkillMove = false;

                //// GameObject nextPointBox = UnityEngine.Object.Instantiate(Resources.Load("Perfab/TestPoint/1") as GameObject);
                // GameObject nextPointBox = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadGameObject("Prefabs/GMPoint/1");

                // nextPointBox.name = "NextPoint";
                // nextPointBox.transform.position = nextPoint;

                // 如果是子弹的话, 忽视 服务器发过来的 y , 采用客户端创建子弹默认的坐标 y
                UpdateType updateType = UpdateType.Fixed;
                if (M_EntityBase.EntityType == E_EntityType.BulletEntity)
                {
                    nextPoint.y = transform.localPosition.y;
                    // UpdateType.Normal 高帧率，因为子弹fix模式不丝滑
                    updateType = UpdateType.Normal;
                }

                //特殊情况，中间值-目标：存储===没有就停下，有就继续了===如再次插入，再断开，中间到下一个目标
                Vector3 distXZ = nextPoint - transform.localPosition; //此时并未更新到逻辑层：【特殊形况unity层数据还在为主】
                distXZ.y = 0;

                // if (M_EntityBase.IsMainPlayer)
                // {
                //     SGF.Debuger.Log($"[Move] {M_EntityBase.EntityType}_{M_EntityBase.EntityId}  OnDoPathMove : {nextPoint} , offset: {distXZ} , {distXZ.magnitude} , moveTime: {distXZ.magnitude / m_entity.Speed} s");
                // }


                if (m_entity.Speed == 0)
                {
                    m_EntityPosition = nextPoint;
                    SetPosition(nextPoint);

                    m_entity.SetCurrentPos(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);//过程【中】同步逻辑层,比上面更全
                    M_EntityBase.ClientExecuteWayPoint(true, true);       //速度为0异常，路店更新
                    return;
                }

                //没有缓存组信息了，不重要的怪物电脑ai，怎么都能走出来的怪物
                float clientMovePara = m_entity.Speed /** Mathf.Pow(GameConfig.THD_PERSION_CLIENT_SIM_SPEED_PARA, (float)ThdPersionDataCache.Count) *   Mathf.Pow(GameConfig.G_CLIENT_SIM_SPEED_PARA, (float)DynamicDataFactory.GetCount<SerNttSnapData>(EntityID));*/;
                float moveTimeSec = distXZ.magnitude / clientMovePara; //可能提前释放锁
                pathStartPos = transform.localPosition;
                if (moveTimeSec > 0)
                {
                    LerpMover1 = DOTween.To(() => pathStartPos, changeVector =>
                    {
                        moveMotion = changeVector - transform.localPosition;
                        //移动只考虑xz，重力考虑Y 
                        //直接掉落无法测试，因为不包含xz，只有Y
                        //水平移动只有xz，重力可以合并
                        //DG 服务器给的，或者NavMesh有Y，就要让这里只考虑XZ，重力考虑Y ：143行
                        //所有人，所有的移动方式都走这里，含主角（寻路），自己走也走不出Y轴，含有（Nav，服务器实体移动（怪物，召唤物，子弹服务器控制））
                        moveMotion.y = 0;
                        // 磊子说:
                        //------- 2020年以前的子弹都不受重力影响，直飞就行
                        //SGF.Debuger.LogError($"SetUpdate 移动调试 [OnDoPathMove] type={M_EntityBase.Data.EntityType},id={M_EntityBase.Data.M_EntityID},pos={transform.localPosition},moveMotion={moveMotion}");
                        /// 子弹的重力需要加配置，判断是否需要重力影响Y轴
                        OnAOIObjectMoveing(moveMotion);                             //基础向量移动
                        m_EntityPosition = changeVector;                            //过程中同步显示层
                                                                                    //SGF.Debuger.Log($"移动调试 OnDoPathMove2222 changeVector={changeVector} id={m_entity.EnityId},pos={transform.localPosition},Speed={m_entity.Speed},sec={moveTimeSec}");
                        m_entity.SetCurrentPos(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z, true);//过程【中】同步逻辑层,比上面更全
                                                                                                                                      //changeVector目标  transform.localPosition.x当前
                    }, nextPoint, moveTimeSec).SetEase(Ease.Linear).SetUpdate(updateType).OnUpdate(() =>
                    {
                        if (M_EntityBase.EntityType == E_EntityType.BulletEntity)
                        {
                            //moveMotion.y = 0;
                            e_AirState = E_AirState.StayInAir;
                        }
                        else
                        {
                            //moveMotion.y = this.VerticalVelocity;
                            e_AirState = E_AirState.GriDownSpeed;
                        }
                    });//一定用fix，物理，下落速度要fix，所以dtween要用fix
                    Vector3 pos = transform.localPosition;
                    //  SGF.Debuger.LogError($"[pos-x-{m_entity.EnityId}] , [OnDoPathMove] nextPoint [{nextPoint.x},{nextPoint.y},{nextPoint.z}] , curPos [{pos.x},{pos.y},{pos.z}] , speed : {m_entity.Speed} , magnitude {(distXZ).magnitude} , moveTimeSec {moveTimeSec} ms");
                    //这里不可以删
                    LerpMover1.onComplete = () =>
                    {
                        //SGF.Debuger.LogWarning($"寻路看看 id={m_entity.EntityId},nextPoint={nextPoint}");

                        LerpMover1.onComplete = null;
                        OnPathMoveLerpComplete(nextPoint);
                    };

                    //主角监听路径更新
                    if (M_EntityBase.IsNeedListenPathUpdate)
                    {
                        LerpMover1.onUpdate = () =>
                        {
                            // CheckClientMainPlayerFindingPath 这个按理说只有主角有
                            if (M_EntityBase.CheckClientMainPlayerFindingPath())
                            {
                                //SGF.Debuger.LogError($"[pos-x-{m_entity.EntityId}] , [OnDoPathMove] onComplete :  [{transform.localPosition.x},{transform.localPosition.y},{transform.localPosition.z}]");
                                LerpMover1.Kill(false);     //主角结束处理
                                LerpMover1.onComplete = null;
                                //m_entity.SetCurrentPos(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z, true);         //过程【结束后】同步逻辑层
                                M_EntityBase.ClientExecuteWayPoint(true, false);       //主角寻路请求新的目标
                            }
                        };
                    }
                }
                else
                {
                    OnPathMoveLerpComplete(nextPoint);
                }
            }
            else //零值就是无目标
            {
                KillLerpMover_StopInPlace(false); //异常情况
                /// 2024/2/1
                /// fixed issue: 修复 寻路停止后, 站在原地 空跑的bug;
                /// 
                /// DL:
                ///     目前没有稳定复现, 通过代码分析 在 ViewAOI.OnDoPathMove 中, 如果正常移动 结束, 都会 ResetToIdleState();
                ///     但如果 是 KillLerpMover_StopInPlace, 则 会停止位移,但是战斗状态 没有 还原成 idle
                {
                    State.I_AnimParam animParam = M_EntityBase.GetAnimParamByState(E_ULayerSubState.Idle);
                    M_EntityBase.ChangeState((GameKeyCommand)E_ULayerSubState.Idle, animParam, false);
                }

            }
        }

        private void OnPathMoveLerpComplete(Vector3 targetPos)
        {
            //SGF.Debuger.LogError($"[pos-x-{m_entity.EnityId}] , [OnDoPathMove] onComplete :  [{transform.localPosition.x},{transform.localPosition.y},{transform.localPosition.z}]");
            //SGF.Debuger.LogError($"onComplete 移动调试 [OnDoPathMove] type={M_EntityBase.Data.EntityType},id={M_EntityBase.Data.M_EntityID},pos={transform.localPosition},nextPoint={nextPoint}");
            //LerpMover1.Kill(false); //   正常结束处理
            //m_EntityPosition = nextPoint;//显示层
            //循环请求：没有则程序出口，保证逻辑对，再到测试表现对
            m_entity.SetCurrentPos(targetPos.x, transform.localPosition.y, targetPos.z, true);//过程【结束后】同步逻辑层
                                                                                              //移动结束请求新的目标
            M_EntityBase.ClientExecuteWayPoint(true, true);
            //LerpMover1.onComplete = null;
            //if (M_EntityBase.EntityType == E_EntityType.BulletEntity)
            //{
            //    SGF.Debuger.LogError($"子弹移动调试 [OnDoPathMove] 222222222 id={M_EntityBase.Data.M_EntityID},pos={transform.localPosition},nextPoint={nextPoint},Speed={m_entity.Speed},magnitude={(distXZ).magnitude},sec={moveTimeSec}");
            //}
        }

        //1【我先不管-999的客户端自己刷/服务器刷/频繁和位置】，也不管服务器没事就刷我时间/频次/挤压/距离，，就是物理系统被频繁更新刷坏了

        /// <summary>
        /// 【验证移动位移：不中断lerp】
        /// 首次是强制移动
        /// </summary>
        /// <param name="nextPoint"></param>
        public virtual void OnForceMove(Vector3 nextPoint)
        {

        }


        public virtual void OnThdPersionMove(Vector3 nextPoint)
        {

        }

        /// <summary>
        /// 设置出生坐标
        /// </summary>
        public virtual void OnBirthPos(Vector3 nextPoint)
        {

        }

        #endregion

        #region 旋转接口

        private float tmep_interpolation = 0;
        private float temp_interpolationABS = 0;
        private float temp_timeSec = 0;

        /// <summary>
        /// jiao
        /// </summary>
        /// <param name="y"></param>
        /// <param name="neepLerp"></param>
        /// <param name="maxLerpTime"></param>
        /// <param name="useMaxLerpTime"></param>
        protected void OnAngelChangeMove(float y, bool neepLerp, float maxLerpTime, bool useMaxLerpTime)
        {
            //if (m_entity.EntityType == E_EntityType.BulletEntity)
            //{
            //    SGF.Debuger.LogWarning($"实体角度调试 OnAngelChangeMove id={m_entity.EntityId},y={y},localPos={transform.localPosition},LocalEulerAngles={m_ModelOffset.localEulerAngles.y},EulerAngles={m_entity.EulerAngles.y},Speed={m_entity.Speed},neepLerp={neepLerp}");
            //}
            //Debug.LogError("OnAngelChangeMove__________________" + objNoUse);

            //if (m_entity.EntityId == GameManager.Instance.mainPlayerId)
            //{
            //    SGF.Debuger.LogError($"实体角度调试 角度tag 显示层11111 原来的={m_ModelOffset.localEulerAngles.y},新的={m_entity.EulerAngles.y}");
            //}
            //角度通常理解是，左右相对关系，有左就有右目标一样只不过有费劲不费劲的事情
            //客户端是和水平X的角度，服务器有bug和我们客户端XY象限是相反的
            //客户端0是向前的Z的，（max改成-90到unity）
            tmep_interpolation = y - m_ModelOffset.localEulerAngles.y;
            tmep_interpolation %= 360; //-360 ~ 360【对于360的操作只是忽略周期，但是意义不变】
                                       // SGF.Debuger.LogWarning($"[Rotate] viewAoi OnAngelChangeMove 设置 cur:{m_ModelOffset.localEulerAngles.y} angle: {y} interpolation:{interpolation}");

            if (tmep_interpolation < 0)
            {
                tmep_interpolation = 360 + tmep_interpolation;//0~360【意义不变】
            }

            if (tmep_interpolation > 180f)//check180~360  ，傻子模式转角（转角时间更长）
            {
                tmep_interpolation = -360 + tmep_interpolation;
                //目标需求： -180~180,达成
            }


            //此时是两边摆动角度
            if (Mathf.Abs(tmep_interpolation) <= 1)// <=1 度不管，精度问题
            {
                return;
            }
            if (Mathf.Abs(tmep_interpolation) < 2)//1 ~ 2度直接设置，分成 360 份了已经
            {
                SetAngelNoLerp(tmep_interpolation);
            }
            else//大于则过度
            {
                if (neepLerp)
                {
                    SetLerpAngelKill();

                    /*if (interpolation > 0 && lastAngel != m_entity.EulerAngles.y && !m_entity.Data.isMainPlayer)
                    {
                        SGF.Debuger.Log($"移动调试 OnAngelChangeMove id={m_entity.EnityId},localPos={transform.localPosition},LocalEulerAngles={m_ModelOffset.localEulerAngles.y},EulerAngles={m_entity.EulerAngles.y},Speed={m_entity.Speed},interpolation={interpolation}");
                    }*/
                    //设计是应该是，转角和移动取得最大时间值作为移动的限制值【前置】 或 必须约束最后要移动的方向扇形【+-15度就可以前行】
                    //不过目前设置是角速度是720度1秒

                    temp_interpolationABS = Mathf.Abs(tmep_interpolation);//目标-当前 = 插值；插值+过程（当前的拆分）= 到目标的过程
                    float turnAroundSpeed = GameConfig.PLAYER_ROTATE_SPEED;
                    if (m_entity.avatarDataCell != null)
                    {
                        turnAroundSpeed = (float)m_entity.avatarDataCell.GetTurnAroundSpeed();
                    }
                    temp_timeSec = temp_interpolationABS / turnAroundSpeed; //GameConfig.PLAYER_ROTATE_SPEED;

                    if (useMaxLerpTime)
                    {
                        temp_timeSec = maxLerpTime;
                    }
                    else
                    {
                        // 如果 maxLerpTime 设置了 时间,并且 timeSec 超过了最大时间, 那么就采用 maxLerpTime ;
                        temp_timeSec = (maxLerpTime != 0 && temp_timeSec > maxLerpTime) ? maxLerpTime : temp_timeSec;
                    }                                                                                          // Debug.Log($"arrow start : {m_ModelOffset.localEulerAngles.y} ,  interpolation {interpolation} ,targe : {m_ModelOffset.localEulerAngles.y + interpolation}");


                    LerpRotate1 = DOTween.To(() => m_ModelOffset.localEulerAngles.y, changing =>
                    {
                        //float changeAngel = Vector3.Angle(defLook, destVec); //转换后：env向量与当前面向向量夹角;
                        //Vector3 normal = Vector3.Cross(defLook, destVec);//叉乘求出法线向量
                        //changeAngel *= Mathf.Sign(Vector3.Dot(normal, Vector3.up));
                        //tempVec.y = changeAngel;

                        SetEntityAngle(changing);
                        //if (m_entity.EntityType == E_EntityType.BulletEntity)
                        //{
                        //    SGF.Debuger.Log($"实体角度调试 OnAngelChangeMove id={m_entity.EntityId},y={y},localPos={transform.localPosition},LocalEulerAngles={m_ModelOffset.localEulerAngles.y},EulerAngles={m_entity.EulerAngles.y},Speed={m_entity.Speed},neepLerp={neepLerp}");
                        //}

                    }, m_ModelOffset.localEulerAngles.y + tmep_interpolation, temp_timeSec).SetEase(Ease.Linear);//原 + 差 = 目标

                    LerpRotate1.onComplete = () =>
                    {
                        OnAngelChange?.Invoke(m_EntityRotate);
                        //1，完成就拉倒，2，释放为空，3，不需要回调
                        //if (m_entity.EntityId == GameManager.Instance.mainPlayerId)
                        //{
                        //    SGF.Debuger.LogError($"实体角度调试 角度tag 显示层11111 原来的={m_ModelOffset.localEulerAngles.y},新的={m_entity.EulerAngles.y}");
                        //}
                    };
                    //m_ModelOffset.localEulerAngles = m_entity.EulerAngles;
                }
                else
                {
                    SetAngelNoLerp(tmep_interpolation);
                }
            }
            //if (m_entity.EntityType == E_EntityType.BulletEntity)
            //{
            //    SGF.Debuger.LogError($"实体角度调试 OnAngelChangeMove id={m_entity.EntityId},y={y},localPos={transform.localPosition},LocalEulerAngles={m_ModelOffset.localEulerAngles.y},EulerAngles={m_entity.EulerAngles.y},Speed={m_entity.Speed},neepLerp={neepLerp}");
            //}
        }

        private void SetEntityAngle(float angle)
        {
            m_EntityRotate.y = angle;                                //缓存Y
            UpdateModelAngle();                //同步unity层
            OnAngelChange?.Invoke(m_EntityRotate);
            // SGF.Debuger.LogError($"[Rotate] viewAoi lerp 设置 angle: {angle}");
        }

        private void SetAngelNoLerp(float interpolation)
        {
            SetLerpAngelKill();

            SetEntityAngle(m_ModelOffset.localEulerAngles.y + interpolation);
        }

        protected void SetLerpAngelKill()
        {
            if (LerpRotate1 != null && LerpRotate1.IsActive() && LerpRotate1.IsPlaying())
            {
                LerpRotate1.onComplete = null;
                LerpRotate1.Kill(false);
            }
        }

        public Vector3 GetRotation()
        {
            return m_ModelOffset.localEulerAngles;
        }

        #endregion

        #region 特效接口

        private string GetGameEffectKey(int effectId, string key = "")
        {
            return $"{effectId}_{key}";
        }

        /// <summary>
        /// 2023/8/24
        /// 碰撞盒 缩放因子.
        /// gl 要求 通过改变碰撞盒参数, 从而 改变 模型大小.同时,  改变 相应的 特效 大小.
        /// 
        /// note:
        ///     模型大小 = 模型配置大小cfgScale * 碰撞盒 缩放因子;
        ///     特效大小 = 碰撞盒配置cfgScale * 碰撞盒 缩放因子;
        /// </summary>
        private Vector3 BoxScaleRatio => M_EntityBase.BoxScaleRatio;


        /// <summary>
        /// 模型 半径 总的 缩放尺寸 = 缩放因子(BoxScaleRatio) * 配置缩放半径(BoxCfgScale)
        /// </summary>
        private Vector3 BoxScale => M_EntityBase.GetBoxScale();

        /// <summary>
        /// 更新 碰撞盒 scale.
        /// </summary>
        /// <param name="boxScale"></param>
        private void UpdateBoxScaleRatio(Vector3 boxScaleRatio)
        {
            // 设置 模型 scale
            SetModelScale();
        }

        private void UpdateGeEffectScale()
        {
            foreach (KeyValuePair<string, FxGeParam> item in gameEffctDic)
            {
                var fxGeParam = item.Value;
                fxGeParam.UpdateFxScale(fxBoxScale: BoxScale, false);
            }
        }

        /// <summary>
        /// 特效的 播放接口
        /// </summary>
        /// <param name="fxParam"></param>
        /// <param name="actionOnPlayerEffect"></param>
        //public void BasePlayEfx(I_FxParam fxParam, string key, string effectPathPrefix)
        //{
        //    string effectName = $"{effectPathPrefix}/{fxParam.EffectPath}";
        //    // 防报错
        //    effectName = effectName.Replace("Assets/Res/", string.Empty);
        //    effectName = effectName.Replace(".prefab", string.Empty);

        //    HangPoint roleBindPoint = fxParam.HangPoint;
        //    float playTime = fxParam.PlayTime;
        //    int startDelay = fxParam.StartDelay;
        //    bool isFollowMove = fxParam.IsFollowMove;
        //    bool isFollowRotate = fxParam.IsFollowRot;
        //    bool isFaceToBuilder = fxParam.IsFaceToBuilder;
        //    bool isLoop = fxParam.Loop;
        //    float speedMultiplier = fxParam.SpeedMultiplier;
        //    // 特效 缩放 参数
        //    E_FxScale eFxScale = fxParam.EFxScale;

        //    // 根据 配置的 IsAll 确定特效是否需要播放
        //    bool isShow = fxParam.IsAll ? true : fxParam.BuilderID == fxParam.OwnerID;

        //    UnityEngine.Vector3 pos = m_entity.Position();
        //    Vector3 rotate = m_entity.EulerAngles;
        //    // 特效是否，面向施法者
        //    if (isFaceToBuilder)
        //    {
        //        float y = Skill.Utils.SkillUtils.GetOrientationBuilderRotate(fxParam.BuilderID, M_EntityBase.EntityId);
        //        rotate = new Vector3(rotate.x, y, rotate.z);
        //    }
        //    else
        //    {
        //        // 如果不是面向施法者, 那就用挂点的 角度, 此处就不需要传入额朝向
        //        rotate = Vector3.zero;
        //    }

        //    // 每次显示 特效前，都去清理一下特效的 root节点
        //    ReturnEffectRootNode();

        //    string fxMainKey = $"{fxParam.Key}_{key}";

        //    Transform point = null;
        //    bool hasModelPoint = false;

        //    // 先取人身上对应的绑点节点
        //    if (M_ModelOffLineData != null)
        //    {
        //        point = M_ModelOffLineData.GetTransformByKey(roleBindPoint.ToString());
        //        hasModelPoint = point != null;
        //    }

        //    // 特效根部 transform
        //    Transform rootScaleTransform = transform;
        //    // 特效是否是 玩家的 骨骼之下(只要是 模型节点之下既可以 认为是在骨骼节点之下)
        //    bool isUnderViewBone = false;

        //    // 如果还是找不到挂点,那就用自身
        //    if (!hasModelPoint)
        //    {
        //        point = transform;
        //    }
        //    else
        //    {
        //        // 挂点的父节点, 如果是直接挂在 人物绑点上,那挂点的parent = null；
        //        // 如果 是 在人身上，但是不跟随旋转, 那 pointParent = m_ModelNoRotationRoot；
        //        // 如果 不在人身上, 那 pointParent =  fxRoot；
        //        if (!isFollowRotate)
        //        {
        //            // 如果 不跟随 旋转, 但是 跟随 移动, 那说明 生成的挂点 父节点在人身上,且是 m_ModelNoRotationRoot；
        //            // 如果 不跟随 旋转 且不跟随 移动, 那说明 是在 空间中 生成一个 挂点, 那 父节点就是 空间 特效节点 FxSceneRoot;
        //            Transform pointParent = isFollowMove ? m_ModelNoRotationRoot : LocalFxManager.Instance.FxSceneRoot;
        //            point = CopyNoRoattionPoint(point, fxMainKey);
        //            point.name = $"{point.name}_[{fxMainKey}]";
        //            point.parent = pointParent;
        //            rootScaleTransform = point;
        //        }
        //        else
        //        {
        //            // 如果 跟随旋转 且跟随 移动,那就是 采用人物挂点,  不需要设置挂点父节点
        //            if (isFollowMove)
        //            {
        //                // 如果跟随玩家移动和旋转, 特效的 根节点就认为是 模型节点.
        //                rootScaleTransform = Animancer.transform;
        //                // 设置为 在根节点骨骼之下
        //                isUnderViewBone = true;
        //                // dont do anything
        //            }
        //            else
        //            {
        //                // 如果跟旋转,但是不跟随移动, 这种情况不允许, 采用 人物挂点本身, 不需要设置挂点父节点
        //                // dont do anything
        //                SGF.Debuger.LogWarning($"[ViewAOI] BasePlayEfx  effectName : {effectName} , Key : {fxParam.Key} dont support FollowRotate when fx is FollowMove , error!!!  ");
        //            }
        //        }
        //    }

        //    FxGeParam fxGeParam = new FxGeParam(rootTransform: rootScaleTransform, isUnderViewBone: isUnderViewBone, fxScale: eFxScale).UpdateFxScale(fxBoxScale: BoxScale, true);

        //    Vector3 scaleRatio = fxGeParam.CurScale;
        //    Vector3 fxRotate = rotate + fxParam.DirectionOffset;
        //    Vector3 moveOffset = fxParam.MoveOffset;

        //    //  gl 确认 在人身上,不会播放 感觉特效,如果不跟随 人,那也是用 配置的挂点数据,生成一份新的挂点在环境中
        //    GameEffect ge = LocalFxManager.Instance.AddPlayerEffect(
        //          effectName: effectName,
        //          root: point,
        //          scale: scaleRatio,
        //          playTime: playTime,
        //          startDelay: startDelay,
        //          isLoop: isLoop,
        //          speedMultiplier: speedMultiplier,
        //          startTime: fxParam.StartTime,
        //          baseRotate: fxRotate,
        //          isFollowBuilderHide: fxParam.IsFollowBuilderHide,
        //          moveOffset: moveOffset
        //       );

        //    if (ge == null)
        //    {
        //        SGF.Debuger.LogWarning($"[ViewAOI] BasePlayEfx  effectName : {effectName} , Key : {fxParam.Key} not find error!!!  ");
        //        return;
        //    }

        //    if (m_entity.EntityType == E_EntityType.BulletEntity)
        //    {
        //        SGF.UI.Framework.UIUtils.ChangeLayer(ge.transform, E_LayerType.Bullet.ToString());
        //    }
        //    else
        //    {
        //        SGF.UI.Framework.UIUtils.ChangeLayer(ge.transform, E_LayerType.Entity.ToString());
        //    }

        //    // 注册一个 特效关闭时 移除 此处 特效引用的逻辑
        //    ge.SetCloseAction(() =>
        //    {
        //        CloseFx(fxMainKey);
        //    });

        //    // 特效面向摄像机
        //    ge.LookAtCamera = fxParam.IsFaceToCamera;

        //    // 将 ge 设置进入 fxGeParam 中
        //    fxGeParam.SetGameEffect(ge);

        //    //将播放的动画存储下来
        //    gameEffctDic[fxMainKey] = fxGeParam;
        //}



        private void BasePlayEfxAsync(I_FxParam fxParam, string key, string effectPathPrefix, Action playCb)
        {

            // 每次显示 特效前，都去清理一下特效的 root节点
            ReturnEffectRootNode();

            string effectName = $"{effectPathPrefix}/{fxParam.EffectPath}";
            // 防报错
            effectName = effectName.Replace("Assets/Res/", string.Empty);
            effectName = effectName.Replace(".prefab", string.Empty);
            bool checkEffectNeedPlay = LocalFxManager.Instance.CheckEffectNeedPlay(effectName, fxParam.StartTime);
            if (!checkEffectNeedPlay)
            {
                // 开始时间已经大于特效最大时间了，不创建了
                playCb.Invoke();
                return;
            }

            UnityEngine.Vector3 pos = m_entity.Position();
            Vector3 rotate = m_entity.EulerAngles;
            // 特效是否，面向施法者
            if (fxParam.IsFaceToBuilder)
            {
                float y = Skill.Utils.SkillUtils.GetOrientationBuilderRotate(fxParam.BuilderID, M_EntityBase.EntityId);
                rotate = new Vector3(rotate.x, y, rotate.z);
            }
            else
            {
                // 如果不是面向施法者, 那就用挂点的 角度, 此处就不需要传入额朝向
                rotate = Vector3.zero;
            }
            string fxKey = fxParam.Key;

            string fxMainKey = $"{fxKey}_{key}";

            Transform point = null;
            bool hasModelPoint = false;
            // 先取人身上对应的绑点节点
            if (M_ModelOffLineData != null)
            {
                point = M_ModelOffLineData.GetTransformByKey(fxParam.HangPoint.ToString());
                hasModelPoint = point != null;
            }

            // 特效根部 transform
            Transform rootScaleTransform = transform;

            // 特效是否是 玩家的 骨骼之下(只要是 模型节点之下即可以 认为是在骨骼节点之下)
            bool isUnderViewBone = false;
            // 如果还是找不到挂点,那就用自身
            if (!hasModelPoint)
            {
                point = transform;
            }
            else
            {
                // 挂点的父节点, 如果是直接挂在 人物绑点上,那挂点的parent = null；
                // 如果 是 在人身上，但是不跟随旋转, 那 pointParent = m_ModelNoRotationRoot；
                // 如果 不在人身上, 那 pointParent =  fxRoot；
                if (!fxParam.IsFollowRot)
                {
                    // 如果 不跟随 旋转, 但是 跟随 移动, 那说明 生成的挂点 父节点在人身上,且是 m_ModelNoRotationRoot；
                    // 如果 不跟随 旋转 且不跟随 移动, 那说明 是在 空间中 生成一个 挂点, 那 父节点就是 空间 特效节点 FxSceneRoot;
                    Transform pointParent = fxParam.IsFollowMove ? m_ModelNoRotationRoot : LocalFxManager.Instance.FxSceneRoot;
                    point = CopyNoRoattionPoint(point, fxMainKey);
                    point.name = $"{point.name}_[{fxMainKey}]";
                    point.parent = pointParent;
                    rootScaleTransform = point;
                }
                else
                {
                    // 如果 跟随旋转 且跟随 移动,那就是 采用人物身上的挂点， point不需要设置它的父节点
                    if (fxParam.IsFollowMove)
                    {
                        // 如果跟随玩家移动和旋转, 特效的 根节点就认为是 模型节点.
                        rootScaleTransform = Animancer.transform;

                        // 设置为 在根节点骨骼之下
                        isUnderViewBone = true;
                        // dont do anything
                    }
                    else
                    {
                        // 如果跟旋转,但是不跟随移动, 这种情况不允许, 采用 人物挂点本身, 不需要设置挂点父节点
                        // dont do anything
                        SGF.Debuger.LogWarning($"[ViewAOI] BasePlayEfx  effectName : {effectName} , Key : {fxKey} dont support FollowRotate when fx is FollowMove , error!!!  ");
                    }
                }
            }

            FxGeParam fxGeParam = new FxGeParam(rootTransform: rootScaleTransform, isUnderViewBone: isUnderViewBone, e_fxScaleType: fxParam.EFxScale, cfgScale: fxParam.ScaleXYZ).UpdateFxScale(fxBoxScale: BoxScale, true);
            fxGeParam.SetCloseAction(() =>
            {
                // SGF.Debuger.LogError($"[Fx] close fx: {fxParam.EffectPath} , type: {fxGeParam.FxRenderType}  ");

                CloseFx(fxMainKey);
            });
            fxGeParam.SetFxType(FxParam.GetFxRenderType(fxParam, m_entity));
            // SGF.Debuger.LogError($"[Fx] play fx: {fxParam.EffectPath} , type: {fxGeParam.FxRenderType}  ");

            fxGeParam.Path = effectName;
            // fxGeParam.EffectName = fxParam.EffectName;

            Vector3 moveOffset = fxParam.MoveOffset;

            // 特效的 生成坐标 暂定为 父节点的坐标 + 特效的偏移量 moveOffset (如果是绑点, 那影响也不大)
            fxGeParam.SetPosition(point.transform.position + moveOffset);



            bool isCanAdd = LocalFxManager.Instance.CheckCanAdd(fxGeParam);
            if (!isCanAdd)
            {
                ReturnFxRootNode(fxMainKey);
                // SGF.Debuger.LogError($"[Fx] add fx: {fxGeParam.EffectName} , type: {fxGeParam.FxRenderType}  超过最大数且优先级不足");

                return;
            }

            Vector3 scaleRatio = fxGeParam.CurScale;
            Vector3 fxRotate = rotate + fxParam.DirectionOffset;


            Action<GameEffect> cb = (ge) =>
            {
                if (ge == null)
                {
                    CloseFx(fxMainKey);
                    SGF.Debuger.LogWarning($"[ViewAOI] BasePlayEfx  effectName : {effectName} , Key : {fxKey} not find error!!!  ");
                    return;
                }

                // 将 ge 设置进入 fxGeParam 中
                fxGeParam.SetGameEffect(ge);
            };

            ResoruceReleaseType releaseType = ResoruceReleaseType.MapSceneAndServerID;


            if (m_entity.Data.isMainPlayer)
            {
                releaseType = ResoruceReleaseType.Force;
            }
            else
            {
                bool M_IsMainPlayerSummon = GameManager.Instance.mainPlayerId == m_entity.SummonHostID;
                // 如果 是主角的召唤物
                if (M_IsMainPlayerSummon)
                {
                    releaseType = ResoruceReleaseType.Force;
                }
            }

            //将播放的动画存储下来
            gameEffctDic[fxMainKey] = fxGeParam;

            // 播放一个特效后, 将特效记录在 LocalFxManager中
            LocalFxManager.Instance.AddFxGeRecord(fxGeParam);


            //  gl 确认 在人身上,不会播放 感觉特效,如果不跟随 人,那也是用 配置的挂点数据,生成一份新的挂点在环境中
            LocalFxManager.Instance.AddPlayerEffectAsync(
                  effectName: effectName,
                  root: point,
                  scale: scaleRatio,
                  playTime: fxParam.PlayTime,
                  startDelay: fxParam.StartDelay,
                  isLoop: fxParam.Loop,
                  speedMultiplier: fxParam.SpeedMultiplier,
                  startTime: fxParam.StartTime,
                  baseRotate: fxRotate,
                  isFollowBuilderHide: fxParam.IsFollowBuilderHide,
                  isFaceToCamera: fxParam.IsFaceToCamera,
                  moveOffset: moveOffset,
                  entityType: m_entity.EntityType,
                  fxGeParam: fxGeParam,
                  callBack: cb,
                  releaseType: releaseType
               );

            playCb.Invoke();

        }

        public void SafePlayEffect(I_FxParam fxParam, string key, string effectPathPrefix)
        {
            // BasePlayEfxAsync(fxParam, key, effectPathPrefix, () => { });
            // return;
            // FxParam fxParamCopy = SimpleDataFactory.InstanceData<FxParam>();
            FxParam fxParamCopy = new FxParam();
            FxParam.Clone(fxParam, fxParamCopy);

            // SGF.Debuger.Log($"[fx] fxParamCopy: {fxParamCopy.GetHashCode()} create, path: {fxParam.EffectName}");

            // 播放特效的时候 要先等模型加载完成
            RegisterAnimancerLoadComleteCB(() =>
            {
                BasePlayEfxAsync(fxParamCopy, key, effectPathPrefix, () =>
                {
                    // SGF.Debuger.Log($"[fx] fxParamCopy: {fxParamCopy.GetHashCode()} release, path: {fxParam.EffectName}");

                    //SimpleDataFactory.ReleaseData(fxParamCopy);
                });
            });

        }

        /// <summary>
        /// 返回特效根节点
        /// note:
        ///     2023/12/25
        ///     在特效异步加载的情况下,特效并不会立即添加到特效根节点上. 
        ///     所以此时采用特效根节点的 子节点数量==0 并不能判定特效是否应该被回收
        /// </summary>
        private void ReturnEffectRootNode()
        {
            return;
            //List<string> readyRetrunRoots = new();
            //foreach (KeyValuePair<string, Transform> item in gamePointDic)
            //{
            //    if (item.Value.childCount == 0)
            //    {
            //        readyRetrunRoots.Add(item.Key);
            //    }
            //}

            //readyRetrunRoots.ForEach((string key) =>
            //{
            //    NodePool.Put(gamePointDic[key], NodePool.NodePoolType.FxRoot);
            //    gamePointDic.Remove(key);
            //});
        }

        /// <summary>
        /// 拷贝一个 节点到 m_ModelNoRotationRoot 下
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private Transform CopyNoRoattionPoint(Transform point, string key, bool isWorld = false)
        {
            // 特效删除的时候 去掉root
            // 然后特效还原位置在NotRemove
            GameObject newPoint = NodePool.Get(NodePool.NodePoolType.FxRoot);
            newPoint.name = point.name;

            Transform newpointTransform = newPoint.transform;
            Transform pointTransform = point.transform;

            newpointTransform.parent = m_ModelNoRotationRoot;
            newpointTransform.SetPositionAndRotation(pointTransform.position, pointTransform.rotation);
            if (isWorld)
            {
                newpointTransform.eulerAngles = Vector3.zero;
            }

            gamePointDic[key] = newpointTransform;
            return newpointTransform;
        }

        private void CloseFx(string fxKey)
        {
            if (gameEffctDic.ContainsKey(fxKey))
            {
                var fxGeParam = gameEffctDic[fxKey];
                if (fxGeParam != null)
                {
                    // 特效关闭的时候,移除 特效
                    LocalFxManager.Instance.RemoveFxGeRecord(fxGeParam);

                    fxGeParam.Reset();
                    gameEffctDic.Remove(fxKey);
                }
            }

            ReturnFxRootNode(fxKey);
        }

        private void ReturnFxRootNode(string fxKey)
        {
            if (gamePointDic.ContainsKey(fxKey))
            {
                NodePool.Put(gamePointDic[fxKey], NodePool.NodePoolType.FxRoot);
                gamePointDic.Remove(fxKey);
            }
        }

        private void CloseAllFx()
        {
            foreach (KeyValuePair<string, FxGeParam> item in gameEffctDic)
            {
                var fxGeParam = item.Value;
                if (fxGeParam != null)
                {
                    // 特效关闭的时候,移除 特效
                    LocalFxManager.Instance.RemoveFxGeRecord(fxGeParam);

                    fxGeParam.Reset();

                }

                if (gamePointDic.ContainsKey(item.Key))
                {
                    NodePool.Put(gamePointDic[item.Key], NodePool.NodePoolType.FxRoot);
                }
            }

            gameEffctDic.Clear();
            gamePointDic.Clear();
        }

        public void BaseStopFx(I_FxParam fxParam, string key = "")
        {
            string fxKey = $"{fxParam.Key}_{key}";
            CloseFx(fxKey);
        }

        /// <summary>
        /// 关闭玩家身上特效的接口
        /// </summary>
        /// <param name="effects"></param>
        /// <param name="key">特殊的key</param>
        public void BaseStopGameEffects(List<int> effects, string key = "")
        {
            for (int i = 0; i < effects.Count; i++)
            {
                int effectId = effects[i];
                string effectKey = GetGameEffectKey(effectId, key);

                CloseFx(effectKey);
            }
        }

        /// <summary>
        /// 控制特效的显隐
        /// </summary>
        /// <param name="isShow"></param>
        public void ControllFxShowHiden(bool isShow)
        {
            int count = gameEffctDic.Count;
            if (count == 0)
            {
                return;
            }
            foreach (KeyValuePair<string, FxGeParam> item in gameEffctDic)
            {
                FxGeParam fxGeParam = item.Value;
                var ge = fxGeParam.ge;
                if (ge != null && ge.IsFollowBuilderHide)
                {
                    ge.ShowHideParticleRender(isShow);
                }
            }
        }


        #endregion

        #region 设置模型显示

        private int m_HideCount = 0;

        /// <summary>
        /// 开启一段时间的 隐身
        /// </summary>
        /// <param name="time"></param>
        /// <param name="key">开启定时器时的 标识tag, 取消的时候 需要用到</param>
        public void StartTimeHidden(float time, string key)
        {
            if (time <= 0)
            {
                return;
            }
            HideModel(false);
            StartDelayShow(time, key);
        }

        /// <summary>
        /// 根据 key 停止 key 对应的 隐身
        /// </summary>
        /// <param name="key"></param>
        public void StopTimeHidden(string key)
        {
            // 如果定时器中 已经不存在 这个 唯一 key 的 延时显示,说明这个 持续一段时间隐藏 已经结束了,那就不需要去 终止了
            if (!DelayInvoker.ContainInvoke(key))
            {
                return;
            }
            DelayInvoker.CancelInvoke(key);
            ShowModel(false);
        }

        public void StartDelayShow(float time, string key)
        {
            DelayInvoker.DelayInvoke(key, time, DelayShowModel, new object[] { });
        }

        public void DelayShowModel(object[] args)
        {
            bool isForce = args.Length > 0 ? (bool)args[0] : false;
            ShowModel(isForce);
        }

        private void HideModel(bool isForceShow = false)
        {
            if (M_EntityBase != null)
            {
                //if (M_EntityBase.EntityType == E_EntityType.Npc)
                //{
                //    SGF.Debuger.LogWarning($"NPC显隐 隐藏 11111111 id={M_EntityBase.EntityId},m_HideCount={m_HideCount}");
                //}

                if (isForceShow)
                {
                    m_HideCount = 1;
                }
                else
                {
                    m_HideCount++;
                }
                if (m_HideCount == 1)
                {
                    ControllShowHide(false);
                }
                //if (M_EntityBase.EntityType == E_EntityType.Npc)
                //    SGF.Debuger.LogError($"NPC显隐 隐藏 222222222 id={M_EntityBase.EntityId},m_HideCount={m_HideCount}");
            }
        }

        private void ShowModel(bool isForceShow = false)
        {
            if (M_EntityBase != null)
            {
                //if (M_EntityBase.EntityType == E_EntityType.Npc)
                //{
                //    SGF.Debuger.LogWarning($"NPC显隐 显示 11111111 id={M_EntityBase.EntityId},m_HideCount={m_HideCount}");
                //}
                if (isForceShow)
                {
                    m_HideCount = 0;
                }
                else
                {
                    m_HideCount--;
                }
                if (m_HideCount <= 0)
                {
                    m_HideCount = 0;
                    ControllShowHide(true);
                }
                //if (M_EntityBase.EntityType == E_EntityType.Npc)
                //SGF.Debuger.LogError($"NPC显隐 显示 222222222 id={M_EntityBase.EntityId},m_HideCount={m_HideCount}");
            }
        }

        public bool GetViewIsShow()
        {
            return m_HideCount < 1;
        }

        private void ControllShowHide(bool isShow)
        {
            M_EntityBase.ControlShowHide?.Invoke(EntityShowHidenTag.Self, isShow);
            ControllFxShowHiden(isShow);
            if (SelfParticleSystem != null)
            {
                SelfParticleSystem.gameObject.SetActive(isShow);
            }
        }

        #endregion

        #region 修改layer层级

        // 初始层级
        private int m_InitLayer;

        public void IsOpenCollision(bool isOpen)
        {
            if (isOpen)
            {
                SetLayer(m_InitLayer);
            }
            else
            {
                SetLayer(LayerMask.NameToLayer("CrossEntity"));// 无碰撞层
            }
        }

        public void SetLayer(int layer)
        {
            gameObject.layer = layer;
        }

        #endregion

        #region 模型动作配置

        protected ModelAnimancerDataCell animCfg;
        protected void RefreshDefaultAnimCfg()
        {
            if (M_EntityBase.avatarDataCell == null)
            {
                animCfg = LocalDataManager.Instance.GetModelAnimancerDataCell(1);
                SGF.Debuger.LogWarning($"{TagFlag} RefreshDefaultAnimCfg() M_EntityBase.avatarDataCell == null");
                return;
            }
            int animID = System.Convert.ToInt32(M_EntityBase.avatarDataCell.BaseAnims);
            animCfg = LocalDataManager.Instance.GetModelAnimancerDataCell(animID);
        }

        protected string GetAnimPath(string animName)
        {
            if (M_EntityBase.avatarDataCell == null)
            {
                SGF.Debuger.LogWarning($"{TagFlag} GetAnimPath() M_EntityBase.avatarDataCell == null");
                return animName;
            }
            string path = $"{M_EntityBase.avatarDataCell.AnimsPath}/{animName}";
            path = path.Replace("Assets/Res/", string.Empty);
            path = path.Replace(".anim", string.Empty);
            return path;
        }

        protected void RefreshStateAnim(string animName, Action<AnimationClip> callBack)
        {
            if (animName == "")
            {
                if (M_EntityBase != null)
                {
                    SGF.Debuger.LogWarning($"{TagFlag} RefreshStateAnim type={M_EntityBase.EntityType},entityid={M_EntityBase.EntityId},animName={animName},animName=null");
                }
                else
                {
                    SGF.Debuger.LogWarning($"{TagFlag} RefreshStateAnim animName={animName},animName=null");
                }
                return;
            }
            if (M_EntityBase == null)
            {
                SGF.Debuger.LogWarning($"{TagFlag} RefreshStateAnim state animName={animName},M_EntityBase=null");
                return;
            }

            string path = GetAnimPath(animName);

            Service.Resource.ResourceFormalManager.Instance.RecursionLoadAsset<AnimationClip>(path, E_AssetType.Animation, callBack);
        }

        #endregion

        #region 修改模型的Y轴偏移量

        /// <summary>
        /// 模型 节点的 偏移量, 策划 配置的 一开始初始化的 模型偏移 或者 后续代码 修改设置的偏移量
        /// </summary>
        private Vector3 modelOffset = Vector3.zero;

        private void OnActionOnViewOffectY(float obj)
        {
            modelOffset.y = obj;

            UpdateModelOffsetPos();
        }

        /// <summary>
        /// 更新 模型偏移的坐标, 目前模型的 偏移坐标 = 模型自己设置的偏移坐标 modelOffset + 客户端模拟的偏移坐标 SimulateOffset
        /// </summary>
        private void UpdateModelOffsetPos()
        {
            m_ModelOffset.localPosition = modelOffset + SimulateLocalOffset + SimulateWorldOffset + ModelPosOffsetConf;
        }

        #endregion

        #region 角度模拟偏移

        /// <summary>
        /// 模型角度自身的偏移
        /// </summary>
        private Vector3 modelAngleOffset = Vector3.zero;

        /// <summary>
        /// 模拟模型父节点的 角度偏移
        /// </summary>
        private Vector3 SimulateModelParentAngleOffset = Vector3.zero;


        /// <summary>
        /// 更新模拟下朝向. 
        /// note1:
        ///     模型的朝向 = 原始朝向 + 模拟朝向
        /// note2:
        ///     模型的朝向自己朝向 和 模型根节点朝向共同影响;
        ///     modelParentAngleOffset 就是模型的父节点 的朝向模拟.
        ///     目前主要用来做 贝塞尔曲线 朝向旋转的 模拟
        /// </summary>
        private void UpdateModelAngle()
        {
            // 模型角度设置
            m_ModelOffset.localEulerAngles = m_EntityRotate + modelAngleOffset;

            // 模型父节点 模拟角度的设置
            // m_ModelOffset.parent.localEulerAngles = SimulateModelParentAngleOffset;
        }

        #endregion

        #region  此处放置一些表现层 模拟移动的 逻辑

        /// 2023/7/19
        /// 客户端模拟的表现， 本来的想法是 直接 在逻辑层驱动 节点的坐标. 但是考虑到玩家 可能走在斜坡上, 直接在逻辑层上驱动表现层模拟运动,
        /// 那就需要 记录之前节点的坐标 , 同时 因为 策划有需求做 在配置的一个高度 做抛物线类似的需求, 那还需要 在逻辑层获取表现层的 配置高度等.
        /// 这个 逻辑层驱动表现层的 模拟 在目前的 逻辑/表现分离 的设计中 很麻烦.
        /// 
        /// 基于上， 对于抛物线 这种纯客户端的模拟, 放在 客户端 做纯表现模拟 .  同时, 由于服务器路点、坐标属性同步等 数据 还是会不停的 给客户端发送, 
        /// 此时 如果客户端 处于 客户端表现模拟的过程中, 上面的 服务器同步 均不设置坐标. 等 客户端模拟结束后, 再同步一次服务器的坐标. 同时, 将 表现层
        /// 的 模拟运动 XZ 平面的 增量 还原，保留 Y 轴上的 增量
        /// 
        /// eg:
        ///     如 抛物线 从 (0,2,0)----->(1,0,0), 模拟运动结束后, XZ 屏幕的 模拟运动增量 还原为 0， y轴的移动增量保留, 最终 剩余的 模拟增量 为 (0,-2,0)
        ///     进一步 这个模拟增量 增加到    m_ModelOffset.localPosition 中, 模拟增量 重新还原为 0,0,0     
        ///     
        ///     然后，在逻辑层 同步 玩家当前的服务器坐标，模拟流程结束.

        public TweenerCore<Vector3, DG.Tweening.Plugins.Core.PathCore.Path, PathOptions> simulatMoveTween;
        public TweenerCore<float, float, FloatOptions> simulatFloatTween;

        /// <summary>
        /// 模拟 本地坐标位移的增量
        /// </summary>
        public Vector3 SimulateLocalOffset = Vector3.zero;

        /// <summary>
        /// 模拟 世界坐标位移的增量
        /// </summary>
        public Vector3 SimulateWorldOffset = Vector3.zero;

        /// <summary>
        /// 模拟运动 在时间轴上 的开始点
        /// </summary>
        Vector2 tStart = Vector2.zero;
        /// <summary>
        /// 模拟运动 在时间轴上 的结束点
        /// </summary>
        Vector2 tEnd = Vector2.zero;
        /// <summary>
        /// 模拟运动 在时间轴上 的中间点
        /// </summary>
        Vector2 tMid = Vector2.zero;

        /// <summary>
        /// 临时的 v2
        /// </summary>
        Vector2 tempV2 = Vector2.zero;

        /// <summary>
        /// 抛物线的三个变量
        /// </summary>
        //double a = 0;
        //double b = 0;
        //double c = 0;

        private List<float> processList = new();

        private List<Vector3> pathPoints = new();

        /// <summary>
        /// 模拟一个 抛物线 运动, 按玩家的朝向和速度抛射 time 秒. 落点在 y=0 的点.
        /// </summary>
        public void StartSimulateThrowBullet(float totalTime, float maxHight, float startTime, Vector3 directionXZ, Action onComplete = null)
        {
            // 位移从 任意的恢复时间 t 开始, 需要 startTime < totalTime
            if (startTime > totalTime)
            {
                onComplete?.Invoke();
                return;
            }
            totalTime = totalTime < 0.15f ? 0.15f : totalTime;

            // 抛物线的 起点 采用 这个节点 当前的 y 
            tStart.Set(0, m_ModelOffset.localPosition.y);
            // 抛物线的 落点 默认 落在 时间轴t 的 (time,0)
            tEnd.Set(totalTime, 0);
            tempV2 = tStart + tEnd;
            // 抛物线的最高点 落在 中间位置
            tMid.Set(tempV2.x / 2, maxHight);
            // 去除 恢复时间剩余需要的时间
            float leastTime = totalTime - startTime;

            // 采用路点 doPathMove 的方式 移动
            {
                processList.Clear();

                // 1.先要生成 一些列的 process , 先用 剩余移动的时间计算 对应的点. 比如 总运行时长 1s, 剩余 需要运行时间 0.5s.  
                //   那 需要的 点数 = 剩余运行时间 / 总运行时间 * 单位时间(1s)的点数
                int pointCounts = Mathf.CeilToInt(leastTime / totalTime * 15);
                int timeCount = Mathf.CeilToInt(leastTime / 0.0167f);

                pointCounts = pointCounts <= timeCount ? pointCounts : timeCount;

                // 3. 每一次 process 增加的 量
                float processSubValue = leastTime / pointCounts;

                // 4. 生成所有的 processList + 1 个点 (+1 是为了 包含 最后的一个 终点)   
                for (int i = 0; i < pointCounts + 1; i++)
                {
                    //从开始，到结束分配片段，分配的process百分比占比
                    var process = (startTime + (i * processSubValue)) / totalTime;
                    processList.Add(process);
                }

                // 先情况 路点 数据
                pathPoints.Clear();

                //通过process取得3d坐标位置
                pathPoints = Reign.MathUtilities.GetTimeParacurvePoints(tStart, tMid, tEnd, directionXZ, processList);

                GameObject doPathGOB = new("PathMoveGOB");

                // 开始路点位移前的一些 设置
                {
                    // 如果是走的恢复逻辑, 那可能在一帧之内 执行好几次 模拟运动, 所以就直接 kill(true). 将结果设置为最终结果
                    simulatMoveTween.Kill(true);

                    // 设置 doPathGOB 的起始点就在 第一个 路点
                    doPathGOB.transform.position = pathPoints[0];
                }

                Vector3 curPathPos = pathPoints[0];

                // 7. 生成 一份 临时的 节点, 将节点 执行 DOPath , 然后 这个临时节点的 y 的值 就是 这个抛物线 真正 需要的 y 的值
                simulatMoveTween = doPathGOB.transform.DOPath(pathPoints.ToArray(), leastTime, PathType.Linear, PathMode.Full3D).SetEase(Ease.Linear).OnUpdate(() =>
                {
                    SimulateLocalOffset = doPathGOB.transform.position;
                    SimulateLocalOffset.y = doPathGOB.transform.position.y - modelOffset.y;

                    UpdateModelOffsetPos();

                    // 得到当前的tween的进度
                    // float progress = simulatMoveTween.ElapsedPercentage();
                    Vector3 direction = (doPathGOB.transform.position - curPathPos).normalized;

                    modelAngleOffset = MathUtilities.CalculateDirection2EulerAngles(direction) - m_EntityRotate;

                    curPathPos = doPathGOB.transform.position;

                    UpdateModelAngle();

                }).OnComplete(() =>
                {
                    // 模拟运动结束后, 清除 模拟变量的值, 同时 将 节点的 模型偏移量 y设置为 0
                    {
                        ///  2024/06/17
                        ///  战斗策划要求模拟运动后保持原状,后续不用交给服务器控制;
                        /// 所以此处先注释掉
                        {
                            // SimulateLocalOffset = Vector3.zero;
                            // modelOffset.y = 0;
                            // UpdateModelOffsetPos();

                            // modelAngleOffset = Vector3.zero;
                            // UpdateModelAngle();
                        }



                    }
                    Destroy(doPathGOB);
                    onComplete?.Invoke();
                });

            }
        }

        /// <summary>
        /// 模拟追踪弹
        /// note:
        ///     追踪弹 由服务器实体创建决定起始位置, 根据黑板中 的 目标, 找到指定 目标
        /// </summary>
        //
        /// <param name="targetEntityID"></param>
        /// <param name="hangPoint"></param>
        /// <param name="speed">速度 m/s</param>
        /// <param name="onComplete"></param>
        public void StartSimulateTrackingBullet(ulong targetEntityID, HangPoint hangPoint, float speed, bool isTargetDie2Stop, Action onComplete = null)
        {
            // note: 此处 计算增加了 modelOffset, 相当于 模拟位移的点 是从 modelOffset处开始的. 
            // 比如子弹 生成的 世界坐标 (0,0,0) , 偏移量 (0.5,0.5,0.5)
            // 那么 此处 SimulateWorldOffset 相当于是 从 (0.5,0.5,0.5) 开始模拟运动
            {
                // 先设置 本地坐标系增量, 相当于 将模拟节点 先设置在世界坐标系原点
                SimulateLocalOffset = -transform.position - modelOffset;

                // 再设置 世界坐标系增量, 相当于 通过 SimulateWorldOffset 模拟 原点在 世界坐标系的 位移运动
                SimulateWorldOffset = transform.position + modelOffset;

                UpdateModelOffsetPos();
            }

            // 每帧的 位移
            float itemFrameMove = speed * TimeUtils.FixedDeltaTime;

            /// <summary>
            /// 检查 是否移动到目标节点或者 是否因为子弹销毁移动结束
            /// </summary>
            Func<ulong, HangPoint, bool, float, bool> checkComplete = (targetEntityID, hangPoint, targetDieStop, frameSpeed) =>
            {
                VitalSigns.NPCEntityBase targetEntity = GameManager.Instance.GetEntityByEntityID(targetEntityID);

                if (targetEntity == null)
                {
                    // 如果 怪物死亡了 并且 子弹跟随怪物 死亡销毁, 那就 结束子弹运动
                    if (targetDieStop)
                    {
                        return true;
                    }
                    return false;
                }
                Vector3 targetPos = GameManager.Instance.GetEntityHangPointPos(targetEntityID, hangPoint, true, out var findResult);

                // 如果 怪物存在, 那就检查 怪物的绑点坐标 到 当前玩家的距离, 小于一定范围的时候 结束(这个范围 可以用 速度*1帧的距离 计算) .
                Vector3 curFramPos = SimulateWorldOffset + ((targetPos - SimulateWorldOffset).normalized * frameSpeed);

                float sqrMag = (targetPos - curFramPos).sqrMagnitude;

                // 如果 距离目标的 位置 <=0.2f 或者 距离 < 1帧的距离
                if (sqrMag <= 0.04f || sqrMag <= frameSpeed * frameSpeed)
                {
                    return true;
                }
                return false;
            };

            /// <summary>
            /// 每一帧的 移动逻辑
            /// </summary>
            Func<ulong, HangPoint, bool, float, Vector3, Vector3> move2Target = (targetEntityID, hangPoint, targetDieStop, frameSpeed, lastPos) =>
            {
                VitalSigns.NPCEntityBase targetEntity = GameManager.Instance.GetEntityByEntityID(targetEntityID);

                Vector3 moveTargetPos = Vector3.zero;
                if (targetEntity == null)
                {
                    // 如果 怪物死亡了 并且 子弹跟随怪物 死亡销毁, 那就 结束子弹运动
                    if (targetDieStop)
                    {
                        return moveTargetPos;
                    }
                    // 如果怪物死亡了, 同时 子弹不跟随 目标怪物而销毁, 那就继续上一次的 目标节点.
                    moveTargetPos = lastPos;
                }
                else
                {
                    // 如果找到了怪物, 那就继续用怪物的 绑点坐标
                    moveTargetPos = GameManager.Instance.GetEntityHangPointPos(targetEntityID, hangPoint, true, out var findResult);
                }

                // 如果 怪物存在, 那就检查 怪物的绑点坐标 到 当前玩家的距离, 小于一定范围的时候 结束(这个范围 可以用 速度*1帧的距离 计算) .
                SimulateWorldOffset += (moveTargetPos - SimulateWorldOffset).normalized * frameSpeed;

                UpdateModelOffsetPos();

                return moveTargetPos;
            };


            float startValue = 0f;
            // 上一次 目标节点的坐标, 如果 玩家
            Vector3 lastTargetPos = Vector3.zero;

            simulatFloatTween.Kill(true);

            // 追踪弹模拟时间 先设置为最大20s
            simulatFloatTween = DOTween.To(() => startValue, x => startValue = x, 1f, 20).OnUpdate(() =>
            {
                bool runComplete = checkComplete(targetEntityID, hangPoint, isTargetDie2Stop, itemFrameMove);
                if (runComplete)
                {
                    // run OnComplete
                    simulatFloatTween.Kill(true);
                    return;
                }

                // 否则的话, 移动到对应目标
                lastTargetPos = move2Target(targetEntityID, hangPoint, isTargetDie2Stop, itemFrameMove, lastTargetPos);

            }).OnComplete(() =>
            {
                // 结束的时候, 销毁

                ///  2024/06/17
                ///  战斗策划要求模拟运动后保持原状,后续不用交给服务器控制;
                /// 所以此处先注释掉
                {
                    // 还原 对应的 模拟坐标
                    {
                        // SimulateLocalOffset = Vector3.zero;
                        // SimulateWorldOffset = Vector3.zero;

                        // UpdateModelOffsetPos();
                    }
                }


                // 执行结束回调
                onComplete?.Invoke();
            });
        }


        /// <summary>
        /// 开始模拟 一个贝塞尔曲线
        /// </summary>
        public void StartSimulateBezierBullet(ulong targetEntityID, Vector3 targetPos, EffectTypeBezierBullet effectTypeBezier, Action onComplete)
        {

            {
                // 先设置本地坐标. 后续开启模拟的时候, 只需要改动 SimulateWorldOffset .
                SimulateLocalOffset = -transform.position - modelOffset;

                // 由于上面已经将 本地坐标设置 设置为 负的原始坐标值。那么此处模拟坐标可以直接按 世界坐标系进行模拟设置.
                // 也就不需要再模拟的时候， 还要将目标节点改为 基于玩家的本地坐标.
                SimulateWorldOffset = transform.position + modelOffset;

                UpdateModelOffsetPos();
            }

            // 每帧的 水平位移
            float speed = m_entity.Speed;

            // 起始坐标，按世界坐标系开始的起始点
            Vector3 startPos = SimulateWorldOffset;

            Vector3 endPos = targetPos;

            Vector3 totalMove = endPos - startPos;
            totalMove.y = 0;
            float totalTime = totalMove.magnitude / speed;

            Vector2 c1 = EffectUtils.GetRandomVector2(effectTypeBezier.RandomPoint1, 100f);
            Vector2 c2 = EffectUtils.GetRandomVector2(effectTypeBezier.RandomPoint2, 100f);

            // 控制的需要根据朝向做偏转

            // c1 = new Vector2(3, -1);
            // c2 = new Vector2(1, 2);

            // 贝塞尔曲线的方向
            var moveDirection = totalMove.normalized;

            // 计算向量夹角
            float moveAngle = Vector3.Angle(Vector3.forward, moveDirection);
            if (Vector3.Cross(Vector3.forward, moveDirection).y < 0)
            {
                moveAngle = 360 - moveAngle;
            }

            Vector2 randomAngle = EffectUtils.GetRandomVector2(effectTypeBezier.RandomAngle, 1f);

            Quaternion quaternion = Quaternion.Euler(0, moveAngle, 0);

            // 人物朝着怪物释放技能, 所以控制点需要先按玩家朝向 做角度偏转
            Vector3 newCPoint1 = quaternion * new Vector3(c1.x, 0, c1.y);
            Vector3 newCPoint2 = quaternion * new Vector3(c2.x, 0, c2.y);


            // 计算出 水平面上的 随机角度, 如果角度大于0, 就需要对贝塞尔曲线的 Z 轴 做角度angle 的偏转
            float angle = randomAngle.x;

            // angle = 30 * GameManager.Instance.testInt;
            // 由于目标点不一定完全落在玩家的 正前方z轴点上。 所以需要把玩家沿 z轴旋转的角度 angle 转换成 移动方向的朝向

            // 定义一个按 moveDirection 旋转的角度四元素
            Quaternion rotaion = Quaternion.AngleAxis(angle, moveDirection);

            // 将控制点 按照 子弹朝向 旋转 angle 角度
            {
                newCPoint1 = rotaion * newCPoint1;
                newCPoint2 = rotaion * newCPoint2;
            }

            Vector3 controlPoint1 = SimulateWorldOffset + newCPoint1;
            Vector3 controlPoint2 = SimulateWorldOffset + newCPoint2;

            // 设置贝塞尔曲线的角度
            // UpdateModelAngle();

            Action CompleteCb = () =>
            {
                ///  2024/06/17
                ///  战斗策划要求模拟运动后保持原状,后续不用交给服务器控制;
                /// 所以此处先注释掉
                {
                    // 还原模拟的偏移
                    // SimulateLocalOffset = Vector3.zero;
                    // SimulateWorldOffset = Vector3.zero;

                    // UpdateModelOffsetPos();

                    // modelAngleOffset = Vector3.zero;

                    // UpdateModelAngle();
                }


                onComplete?.Invoke();
            };

            // 如果目标id存在, 那就是一个跟随的贝塞尔曲线子弹
            if (targetEntityID != 0)
            {
                SimulateBezierMove2Target(targetID: targetEntityID, effectTypeBezier.ToTargetHangPoint, controlPoint1: controlPoint1, controlPoint2: controlPoint2, CompleteCb);
                return;
            }

            // 如果没有目标id,但是有目标点坐标
            if (targetPos != Vector3.zero)
            {
                SimulateBezierMove2Point(totalTime, startPos, controlPoint1, controlPoint2, endPos, CompleteCb);
                return;
            }

            // 如果既没有目标怪,也没有目标节点, 那就 直接 执行 结束回调
            CompleteCb.Invoke();

        }

        /// <summary>
        /// 模拟贝塞尔曲线 移动到固定点的 位移。
        /// note:
        ///     如果是 固定点,时间 是固定的. 那在一开始就可以确定这个 besizer 曲线
        /// </summary>
        public void SimulateBezierMove2Point(float totalTime, Vector3 startPoint, Vector3 controlPoint1, Vector3 controlPoint2, Vector3 endPoint, Action onComplete)
        {

            StartSimulateTween((float progress) =>
                {
                    // 每一帧的位移.
                    SimulateWorldOffset = MathUtilities.CalculateBezier3Point(startPoint, controlPoint1, controlPoint2, endPoint, progress);
                    //SGF.Debuger.Log($"[ViewAoi] 模拟运动到指定点: {endPoint} , progress: {progress} ,当前点: {SimulateWorldOffset}");
                    UpdateModelOffsetPos();

                    SimulateBezierRotateFrameUpdate(startPoint, controlPoint1, controlPoint2, endPoint, progress);

                },
                (float progress) =>
                {
                    // 移动到指定的点, 就不需要是否到钟点的检查, 到时间结束就可以了
                    return false;
                }
            , onComplete, totalTime);
        }

        public void SimulateBezierMove2Target(ulong targetID, HangPoint hangPoint, Vector3 controlPoint1, Vector3 controlPoint2, Action onComplete)
        {

            // 每帧的 水平位移
            float speed = m_entity.Speed;
            float frameSpeed = speed * 0.0167f;

            // 起始点坐标就是当前的 模拟世界坐标
            Vector3 startPoint = SimulateWorldOffset;
            Vector3 loginStartPoint = SimulateWorldOffset;
            var now = TimeUtils.ClientNowStampMilli;

            StartSimulateTween((float progress) =>
                {
                    var targePos = GameManager.Instance.GetEntityHangPointPos(targetID, hangPoint, true, out var findResult);

                    // 如果找不到实体挂点,就返回(目前 没有实体才会返回false)
                    if (!findResult)
                    {
                        return;
                    }
                    // 每一帧目标怪物当前的 坐标

                    Vector3 endPoint = targePos;


                    var moveDir = endPoint - startPoint;
                    moveDir.y = 0;

                    // 当前的坐标就是 上一帧的水平坐标+ 每帧位移
                    startPoint = startPoint + (moveDir * speed * (TimeUtils.ClientNowStampMilli - now) / 1000f);
                    now = TimeUtils.ClientNowStampMilli;

                    Vector3 leastMove = endPoint - startPoint;
                    leastMove.y = 0;

                    // 剩余位移需要的时间
                    float leastTime = leastMove.magnitude / speed;

                    // 当前贝塞尔曲线点的时间 就是 当前的运行时间/(已经运行的时间+剩余还需要运行的时间)
                    float curProgress = progress / (progress + leastTime);

                    // 每一帧的位移.
                    SimulateWorldOffset = MathUtilities.CalculateBezier3Point(loginStartPoint, controlPoint1, controlPoint2, endPoint, curProgress);
                    // SGF.Debuger.Log($"[ViewAoi] 模拟运动到指定点: {endPoint} , progress: {progress} , lasetTime: {leastTime}, curProgress: {curProgress} ,当前点: {SimulateWorldOffset}");
                    UpdateModelOffsetPos();

                    // 模拟贝塞尔曲线的 每帧的朝向更新
                    SimulateBezierRotateFrameUpdate(loginStartPoint, controlPoint1, controlPoint2, endPoint, curProgress);

                },
                (float progress) =>
                {
                    // 只做点位是否到达的检查
                    return CheckSimulateMove2TargetEntity(SimulateWorldOffset, targetID, hangPoint, true, frameSpeed);
                }
            , onComplete, 20);
        }

        /// <summary>
        /// 模拟贝塞尔曲线的的每帧的朝向更新
        /// </summary>
        private void SimulateBezierRotateFrameUpdate(Vector3 startPoint, Vector3 controlPoint, Vector3 controlPoint2, Vector3 endPoint, float curProgress)
        {
            Vector3 direction = MathUtilities.CalculateBezier3CurvePoint(startPoint, controlPoint, controlPoint2, endPoint, curProgress);

            modelAngleOffset = MathUtilities.CalculateDirection2EulerAngles(direction) - m_EntityRotate;

            UpdateModelAngle();
        }
        /// <summary>
        /// 开启一段时间的 模拟运动的 tween
        /// </summary>
        /// <param name="frameAction"></param>
        /// <param name="checkComplete"></param>
        /// <param name="onComplete"></param>
        /// <param name="simulateTime"></param>
        private void StartSimulateTween(Action<float> frameAction, Func<float, bool> checkComplete, Action onComplete, float simulateTime = 20)
        {
            // 上一次 目标节点的坐标, 如果 玩家

            simulatFloatTween.Kill(true);

            float startValue = 0f;
            // 追踪弹模拟时间 先设置为最大20s
            simulatFloatTween = DOTween.To(() => startValue, x => startValue = x, 1f, simulateTime).OnUpdate(() =>
            {
                if (checkComplete.Invoke(startValue))
                {
                    // run OnComplete
                    simulatFloatTween.Kill(true);
                    return;
                }

                // 否则的话, 移动到对应目标
                frameAction.Invoke(startValue);

            }).OnComplete(() =>
            {
                // 执行结束回调
                onComplete?.Invoke();
            });
        }

        /// <summary>
        /// 检查是否移动到 目标实体挂点
        /// </summary>
        /// <param name="curPosition">当前所在的 坐标</param>
        /// <param name="targetEntityID"></param>
        /// <param name="hangPoint"></param>
        /// <param name="targetDieStop">怪物死亡是否终止</param>
        /// <returns></returns>
        private bool CheckSimulateMove2TargetEntity(Vector3 curPosition, ulong targetEntityID, HangPoint hangPoint, bool targetDieStop, float frameSpeed)
        {
            VitalSigns.NPCEntityBase targetEntity = GameManager.Instance.GetEntityByEntityID(targetEntityID);

            if (targetEntity == null)
            {
                // 如果 怪物死亡了 并且 子弹跟随怪物 死亡销毁, 那就 结束子弹运动
                if (targetDieStop)
                {
                    return true;
                }
                return false;
            }
            Vector3 targetPos = GameManager.Instance.GetEntityHangPointPos(targetEntityID, hangPoint, true, out var findResult);

            float sqrMag = (targetPos - curPosition).sqrMagnitude;

            // 如果 距离目标的 位置 <=0.2f 或者 距离 < 1帧的距离
            if (sqrMag <= 0.04f || sqrMag <= frameSpeed * frameSpeed)
            {
                return true;
            }
            return false;
        }




        #endregion

        #region 播放 lineRender 的接口

        #region lineRender 的管理， 此处以后 如果存在 LineRenderHelper 这种全局管理的脚本 就可以全部移植过去
        /// <summary>
        /// 由 tagKey 关联的 LineRenderParamsShow 组件. 增加 这个 映射的目的是为了 在播放的途中 能够追踪到具体的 谁与谁的连线。
        /// 这样 ， 当连线途中如果 因为距离等原因 需要关闭这些连线，就可以直接 定位到关闭 那一个.
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="LineRenderParamsShow"></typeparam>
        private DictionaryEx<string, LineRenderParamsShow> tagLineRenderShows = new();

        /// <summary>
        /// LineRenderParamsShow 的缓存队列. 当一个 LineRenderParamsShow 被关闭后 都会存入 lineRenderMapQueue 中
        /// 由于 不同的 LineRenderParamsShow 类型不一样, 所以 采用 它prefab 的path 作为 map的 key
        /// </summary>
        private Dictionary<string, Queue<LineRenderParamsShow>> lineRenderMapQueue = new();

        private LineRenderParams lineRenderParams = new();

        private LineRenderParamsShow GetRenderFromLineRenderMapQueue(string path)
        {
            if (!lineRenderMapQueue.ContainsKey(path))
            {
                return null;
            }

            var queue = lineRenderMapQueue[path];
            if (queue.Count == 0)
            {
                return null;
            }

            return queue.Dequeue();
        }

        private void EnqueueLineRender(string path, LineRenderParamsShow lineRender)
        {
            if (!lineRenderMapQueue.TryGetValue(path, out var queue))
            {
                queue = new Queue<LineRenderParamsShow>();
                lineRenderMapQueue.Add(path, queue);
            }

            queue.Enqueue(lineRender);
        }

        private void CreateLineRender(string path, Action<LineRenderParamsShow> callback)
        {
            var lineRender = GetRenderFromLineRenderMapQueue(path);
            if (lineRender != null)
            {
                callback?.Invoke(lineRender);
            }
            else
            {
                path = path.Replace("Assets/Res/", "");
                path = path.Replace(".prefab", "");

                StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(path,
               (GameObject go) =>
               {
                   if (go == null)
                   {
                       callback?.Invoke(null);
                       return;
                   }

                   var gob = GameObject.Instantiate<GameObject>(go);
                   if (gob != null)
                   {
                       lineRender = gob.GetComponent<LineRenderParamsShow>();
                       callback?.Invoke(lineRender);
                   }
               });
            }
        }

        private LineRenderParamsShow GetLineRender(string tagKey)
        {
            return tagLineRenderShows[tagKey];
        }

        private List<KeyValuePair<string, LineRenderParamsShow>> GetTagMatchLineRender(string tagKey)
        {
            return tagLineRenderShows.KFilter((keyValue) =>
            {
                return keyValue.Key.Contains(tagKey);
            });
        }

        private Dictionary<string, LineRenderParamsShow> GetTagMatchLineRenderDic(string tagKey)
        {
            return tagLineRenderShows.KFilter((key, lineRender) =>
            {
                return key.Contains(tagKey);
            });
        }

        private bool CheckHasTagLineRender(string tagKey)
        {
            foreach (var item in tagLineRenderShows)
            {
                if (item.Key.Contains(tagKey))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 每一个 lineRender 链接 points 所有节点的接口。
        /// note:
        ///     LineRendererConfig 中 策划配置的 发散/全连接 或者 什么其它的 在此处均不关心。 对于 这个接口而言, 它就是根据
        ///     tagKey 唯一确定 一个 LineRenderParamsShow 。 然后根据 传入的 所有点points ，连成对应的线。
        ///     而全连接还是 发散 等等 都是 外部调用的时候自己去 区分.
        /// </summary>
        /// <param name="lineRendererConfig"></param>
        /// <param name="points"></param>
        /// <param name="startTime"></param>
        /// <param name="tagKey"></param>
        /// <param name="playCb">播放的cb</param>
        private void PlayLineRender(LineRendererConfig lineRendererConfig, List<Vector3> points, int startTime, string tagKey, Action<bool> playCb)
        {
            var lineRender = GetLineRender(tagKey);

            if (lineRender != null)
            {
                PlayTagKeyLineRender(tagKey, lineRender, lineRendererConfig, points, startTime);
                playCb?.Invoke(true);
            }
            else
            {
                CreateLineRender(lineRendererConfig.LinePrefab, (lineRender) =>
                {
                    if (lineRender == null)
                    {
                        playCb?.Invoke(false);
                        return;
                    }
                    PlayTagKeyLineRender(tagKey, lineRender, lineRendererConfig, points, startTime);
                    playCb?.Invoke(true);
                });
            }
        }

        private void PlayTagKeyLineRender(string tagKey, LineRenderParamsShow lineRender, LineRendererConfig lineRendererConfig, List<Vector3> points, int startTime)
        {
            tagLineRenderShows[tagKey] = lineRender;
            string str = points.KJoin(",");
            // SGF.Debuger.LogError($"[viewAOI] PlayLineRender tagKey: [{tagKey}] , points: {str}");
            lineRender.PlayLineRender(lineRenderParams.Update(lineRendererConfig, points, startTime), () =>
            {
                FinishLineRender(lineRendererConfig, tagKey);
            });
        }

        /// <summary>
        /// 更新 tagKey 对应 lineRender 的 points / 进度 等
        /// </summary>
        /// <param name="lineRendererConfig"></param>
        /// <param name="points"></param>
        /// <param name="startTime">效果的开始时间,  -1 表示  当前时间</param>
        /// <param name="tagKey"></param>
        private bool UpdateLineRender(LineRendererConfig lineRendererConfig, List<Vector3> points, int startTime, string tagKey)
        {
            var lineRender = GetLineRender(tagKey);
            if (lineRender == null)
            {
                return false;
            }
            // string str = points.KJoin(",");
            // SGF.Debuger.LogError($"[ViewAOI] UpdateLineRender 更新线: {tagKey} , points: {str} , startTime: {startTime}");

            lineRender.UpdateLineRender(lineRenderParams.Update(lineRendererConfig, points, startTime));
            return true;
        }

        private void StopLineRenders(LineRendererConfig lineRendererConfig, Dictionary<string, LineRenderParamsShow> lineRendersDic)
        {
            foreach (var item in lineRendersDic)
            {
                StopLineRender(lineRendererConfig, item.Key);
            }
        }

        /// <summary>
        /// 停止 tagKey 关联的 所有 线
        /// </summary>
        /// <param name="lineRendererConfig"></param>
        /// <param name="tagKey"></param>
        public void StopTagMatchLineRenders(LineRendererConfig lineRendererConfig, string tagKey)
        {
            // SGF.Debuger.LogError($"[ViewAOI] StopTagMatchLineRenders 停止线: {tagKey}");

            var lineRenders = GetTagMatchLineRender(tagKey);

            lineRenders.ForEach((item) =>
            {
                StopLineRender(lineRendererConfig, item.Key);
            });
        }

        /// <summary>
        /// 停止 tagKey 链接的 线
        /// </summary>
        /// <param name="lineRendererConfig"></param>
        /// <param name="tagKey"></param>
        private void StopLineRender(LineRendererConfig lineRendererConfig, string tagKey)
        {
            // SGF.Debuger.LogError($"[ViewAOI] StopLineRender 停止线: {tagKey}");

            var lineRender = FinishLineRender(lineRendererConfig, tagKey);
            if (lineRender == null)
            {
                return;
            }
            lineRender.Stop();
            // SGF.Debuger.LogError($"[ViewAOI] StopLineRender 停止线: {tagKey} success!!");

        }

        private LineRenderParamsShow FinishLineRender(LineRendererConfig lineRendererConfig, string tagKey)
        {
            var lineRender = GetLineRender(tagKey);
            if (lineRender == null)
            {
                return null;
            }
            tagLineRenderShows.Remove(tagKey);

            EnqueueLineRender(lineRendererConfig.LinePrefab, lineRender);

            return lineRender;
        }
        #endregion
        /// <summary>
        /// 播放 lineRender 的接口.  
        /// note:
        ///     lineRender 的播放 有两种实现方式:
        ///     一种新增一个 LineRenderHelper, 由 它来 控制 游戏内所有的 lineRender 显示 和隐藏。 生成的节点 存在于 节点树上，脱离玩家的 实体节点;
        ///     另一种 是 目前游戏 内的方式， 节点挂在 玩家的实体上, 由玩家自己控制 lineRender 的显隐.但是 LineRender节点没办法公用。
        ///         目前游戏内没有节点池的概念，后续如果增加节点池 可以公用. 当前这种方式 会麻烦一点.
        /// </summary>
        /// <param name="lineRendererConfig"></param>
        /// <param name="targets"></param>
        /// <param name="startTime"></param>
        /// <param name="tagKey"></param>
        public void StartPlayLineRenders(LineRendererConfig lineRendererConfig, List<ulong> targets, int startTime, string tagKey)
        {
            HandleLineRenderPoints(lineRendererConfig, targets, tagKey, (List<Vector3> points, string distributionTagKey) =>
            {
                //PlayLineRender(lineRendererConfig, points, startTime, distributionTagKey);
                PlayLineRender(lineRendererConfig, points, startTime, distributionTagKey, (bool result) =>
                {
                    bool isFollwTargets = lineRendererConfig.IsChangeWithTime;
                    if (isFollwTargets)
                    {
                        StartUpdateLineRenders(lineRendererConfig, targets, -1, tagKey);
                    }
                });
            });


        }

        private string GetUpdateKey(string tagKey)
        {
            return $"LineRender_{tagKey}";
        }

        /// <summary>
        /// 开启 一个定时刷新的 lineRender. 处理的是 跟随 的 lineRender
        /// </summary>
        /// <param name="lineRendererConfig"></param>
        /// <param name="targets"></param>
        /// <param name="startTime"></param>
        /// <param name="tagKey"></param>
        public void StartUpdateLineRenders(LineRendererConfig lineRendererConfig, List<ulong> targets, int startTime, string tagKey)
        {
            string updateKey = GetUpdateKey(tagKey);

            DelayInvoker.CancelInvoke(updateKey);

            // 如果 更新的时候 检查发现 所有的 线 都被关闭了, 那就不刷了
            if (!CheckHasTagLineRender(tagKey))
            {
                //SGF.Debuger.LogError($"[ViewAOI] StartUpdateLineRenders 已经没有所有 {tagKey} 相关的 lineRender, 结束更新");
                return;
            }

            DelayInvoker.DelayInvoke(updateKey, 0.05f, (args) =>
            {
                UpdateLineRenders(lineRendererConfig, targets, startTime, tagKey);
                // 重新 开启 下一次的 updateLineRender
                StartUpdateLineRenders(lineRendererConfig, targets, startTime, tagKey);
            });
        }

        public void UpdateLineRenders(LineRendererConfig lineRendererConfig, List<ulong> targets, int startTime, string tagKey)
        {

            var lineRenders = GetTagMatchLineRenderDic(tagKey);

            // 如果一个相关的 lineRender 都没有, 那就不走后续逻辑
            if (lineRenders.Count == 0)
            {
                return;
            }

            HandleLineRenderPoints(lineRendererConfig, targets, tagKey, (List<Vector3> points, string distributionTagKey) =>
            {
                bool updateResult = UpdateLineRender(lineRendererConfig, points, startTime, distributionTagKey);
                // 如果更新失败了, 说明产生了 新的 lineRender 对应的 线, 这个时候, 就需要采用 PlayLineRender 接口, 新创建这条线
                if (!updateResult)
                {
                    // SGF.Debuger.LogError($"[ViewAOI] UpdateLineRenders 缺少 {distributionTagKey} 的线, 新创建一根在 : {startTime}");
                    //PlayLineRender(lineRendererConfig, points, startTime, distributionTagKey);
                    PlayLineRender(lineRendererConfig, points, startTime, distributionTagKey, null);
                }

                lineRenders.Remove(distributionTagKey);
            });

            if (lineRenders.Count > 0)
            {
                // SGF.Debuger.LogError($"[ViewAOI] UpdateLineRenders 节点销毁, 需要删除 线");
                StopLineRenders(lineRendererConfig, lineRenders);
            }

        }

        private void HandleLineRenderPoints(LineRendererConfig lineRendererConfig, List<ulong> targets, string tagKey, Action<List<Vector3>, string> handleAc)
        {
            GetLineRenderHangPointsPos(lineRendererConfig, targets, out Vector3 startPos, out List<KeyValuePair<ulong, Vector3>> toPoints);
            if (startPos == Vector3.zero)
            {
                return;
            }

            if (toPoints.Count == 0)
            {
                return;
            }

            // 如果确定了 起始点 和 目标点, 那就要根据 LineRendererConfig 的配置的 类型, 具体的 去创建对应的lineRender.

            switch (lineRendererConfig.LinkType)
            {
                case LinkTypeEnum.Distribution:
                    {
                        // 如果是 分发类型, 那就是 从 自己为 出发点， 分发连线到 所有目标, 会创建 目标数量的 lineRender
                        toPoints.ForEach((toPoint) =>
                        {
                            List<Vector3> points = new()
                            {
                                startPos,
                                toPoint.Value
                            };
                            string distributionTagKey = $"{tagKey}_[{EntityID}->{toPoint.Key}]";
                            handleAc.Invoke(points, distributionTagKey);

                        });
                    }
                    break;
                case LinkTypeEnum.Sequence:
                    {


                        // 先屏蔽
                        {
                            // 如果是 链式类型, 那是从 自己 为出发点, 链接 所有目标, 只需要创建 一个lineRender, 然后连接所有的点
                            // toPoints.ForEach((toPoint) =>
                            // {
                            //     points.Add(toPoint.Value);
                            // });
                            // PlayLineRender(lineRendererConfig, points, startTime, tagKey);
                        }

                        {
                            /// 2023/8/18
                            /// gl 确定 链式 结构 只是两个点 之间的连线.
                            /// 所以 链式结构 ，也是多个 lineRender 依次相连
                            toPoints.ForEach((toPoint) =>
                            {
                                List<Vector3> points = new()
                             {
                            startPos,
                        };
                                points.Add(toPoint.Value);

                                string distributionTagKey = $"{tagKey}_[{EntityID}->{toPoint.Key}]";
                                var str = points.KJoin("|");
                                //SGF.Debuger.LogError($"[LineRender]: key: {toPoint.Key}  , point: {str}");

                                handleAc.Invoke(points, distributionTagKey);
                            });
                        }

                    }
                    break;
                case LinkTypeEnum.NoFromSequence:
                    {
                        // 如果是 链式排除自己类型, 那是从 第一个目标点 为出发点, 链接 所有目标, 只需要创建 一个lineRender, 然后连接所有的点

                        toPoints.ForEach((toPoint) =>
                        {
                            List<Vector3> points = new();
                            points.Add(toPoint.Value);
                            if (points.Count >= 2)
                            {
                                string distributionTagKey = $"{tagKey}_[{EntityID}->{toPoint.Key}]";
                                handleAc.Invoke(points, distributionTagKey);
                            }
                        });
                    }
                    break;

                default:
                    {
                        SGF.Debuger.LogWarning($"[ViewAOI] 缺少 lineRender 的linkType: {lineRendererConfig.LinkType} 处理方式");
                    }
                    break;
            }
        }

        private void GetLineRenderHangPointsPos(LineRendererConfig lineRendererConfig, List<ulong> targets, out Vector3 startPos, out List<KeyValuePair<ulong, Vector3>> toPoints)
        {
            var fromHangPoint = lineRendererConfig.FromTargetHangPoint;
            startPos = GetHangPointPos(fromHangPoint);

            HangPoint toHangPoint = lineRendererConfig.ToTargetHangPoint;

            toPoints = new();
            foreach (var targetEttid in targets)
            {
                var targePos = GameManager.Instance.GetEntityHangPointPos(targetEttid, toHangPoint, false, out var findResult);
                if (findResult)
                {
                    toPoints.Add(new KeyValuePair<ulong, Vector3>(targetEttid, targePos));
                }
            }
        }

        private Vector3 GetHangPointPos(HangPoint hangPoint)
        {
            if (M_ModelOffLineData == null)
            {
                return transform.position;
            }

            var point = M_ModelOffLineData.GetTransformByKey(hangPoint.ToString());
            if (point == null)
            {
                SGF.Debuger.LogWarning($"挂点: [{hangPoint.ToString()}] 找不到!!!");
                return Vector3.zero;
            }
            return point.position;
        }


        private void ReleaseLineRender()
        {
            foreach (var item in tagLineRenderShows)
            {
                item.Value.Stop();
                Destroy(item.Value);
            }
            tagLineRenderShows.Clear();

            foreach (var item in lineRenderMapQueue)
            {
                var queue = item.Value;
                while (queue.Count > 0)
                {
                    var lineRender = queue.Dequeue();
                    Destroy(lineRender);
                }
            }
            lineRenderMapQueue.Clear();
        }
        #endregion

        #region 主角脚印效果

        protected virtual void CheckCreateFootprint()
        {

        }


        #endregion

    }



    public class BeAttackFlashColor
    {
        public Material mat;
        public TweenerCore<float, float, FloatOptions> tweener;
        public BeAttackFlashColor(Material _mat, TweenerCore<float, float, FloatOptions> _tweener)
        {
            mat = _mat;
            tweener = _tweener;
        }
    }

}

