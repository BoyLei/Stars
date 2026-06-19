using StarProject.Service.Cam;
using StarProject.Service.LocalData;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace StarProject.Service.LocalDynamic.Fx
{
    /// <summary>
    /// StarLife是单个例子的持续时间：一般不用管
    ///Durning是生命周期，发射器的生命周期
    ///Delay是延迟发射
    ///Delay=5，Durning=10 ：不Loop，5~15秒播放一次
    ///Delay=5，Durning=10 ：Loop，5~N一直播放，
    ///如有呼吸：需要代码控制
    ///但是除了常驻，必须销毁
    /// </summary>
    public class GameEffect : MonoBehaviour
    {
        public static int PS_MAX_LIFE_TIME = 60;
        private const string LOG_TAG = "GameEffect";

        public LocalEffectTags M_localEffectTags;
        //配置文件
        //public conf_effect conf { get; private set; }
        //粒子系统
        public ParticleSystem mainPs;
        // 子粒子系统
        public ParticleSystem[] particleSystems;

        //public Renderer[] renderParticleSystems;
        private List<ParticleSystemRenderer> psRenderers = new();
        // 粒子的默认播放速度
        public DictionaryEx<string, float> psDefaultSpeedDic = new();

        /// <summary>
        /// 特效的默认 scale
        /// </summary>
        /// <typeparam name="Transform"></typeparam>
        /// <typeparam name="Vector3"></typeparam>
        private Dictionary<Transform, Vector3> psDefaultScale = new();

        //播放时间
        private float playTime;
        //跟随物体，如果此物体不为空，特效会跟随父物体移动，直到父物体变为null
        //public GameObject followHeader;
        /// <summary>
        /// 设置的播放时间,如果是 -1 的时候, 会不停的的播放
        /// </summary>
        private float M_setPlayerTime;

        private bool isEmitCameraEvent = false;

        private int cameraEventID = 0;

        private CameraEvent emitCameraEvent = CameraEvent.ShakeCam;

        /// <summary>
        /// 默认播放的屏幕效果
        /// </summary>
        E_CameraEffectType e_CameraEffectType = E_CameraEffectType.Shake;

        public bool LookAtCamera = false;

        private System.Action OnClose;

        /// <summary>
        /// 用来做显示隐藏 的计数, 使用计数的原因是 隐藏可能是多个地方隐藏,那相应的，
        /// 也应该是 多个地方都显示后, 这个特效才会显示
        ///1 是一种，2所有人都同意才开启还是有一个同意就开启要分清楚；3游戏中以表现为常态
        ///4，隐身为特殊条件，5隐身可能有很多个，6有一个隐身就关闭
        ///7，只有全部不一隐身才开启，8（全部同意开启)才开启，9所有都是开启才开启的原则10，这种原则属于控制层，11通常是基数，bool，逻辑处理
        ///12，我想提个醒
        //        从bool进阶到int基数管理是一个上升的层次管理
        //        通常要多思考下
        //他是：“2所有人都同意才开启还是有一个同意就开启要分清楚”再需求上，和代码上的情况
        //通常如果不用int来管理可以用逻辑来处理，所以也就是逻辑层来处理的，或者位运算（当然要写注释）
        ///13,看似int高级，但是更耗空间需要看实际需求，理解也会加大难度，也要符合需求，也可以适当用逻辑层，或者位运算
        /// </summary>
        // private int hiddenCount = 0;

        private bool visible = true;

        /// <summary>
        /// 是否跟随隐藏
        /// </summary>
        public bool IsFollowBuilderHide = true;

        private string transformRootName = "";

        public bool Playing
        {
            get
            {
                // Playing 采用 activeInHierarchy
                return gameObject.activeSelf;
            }
            set
            {
                if (gameObject.activeSelf && value == false)//关闭情况，不缓存player
                {
                    SetDefaultSpeed();
                }
                gameObject.SetActive(value);
                if (value == false)
                {
                    // SGF.Debuger.LogWarning($"gameEffect[{this.transform.name}] 关闭:  : Playing {Playing}");
                    OnClose?.Invoke();
                    OnClose = null;
                }
            }
        }

        private void Awake()
        {
            mainPs = transform.GetComponent<ParticleSystem>();
            particleSystems = GetComponentsInChildren<ParticleSystem>();
            FillRsRenderers();
            InitDefaultScale();
            InitDefaultSpeed();
            transformRootName = $"{transform.name}_root";
        }

        void Update()
        {
            if (LookAtCamera)
            {
                transform.LookAt(Camera.main.transform);
            }
            if (M_setPlayerTime == -1)
            {
                return;
            }
            playTime += UnityEngine.Time.deltaTime;
            //if (followHeader != null)
            //{
            //transform.position = followHeader.transform.position;
            //return;
            //}

            if (playTime > M_setPlayerTime)
            {
                Playing = false;
            }
        }

        void OnDestroy()
        {
            // hiddenCount = 0;
            visible = true;
            cameraEventID = 0;
        }

        private void InitDefaultScale()
        {
            InitDefaultScale(transform);
        }

        private void InitDefaultScale(Transform node)
        {
            psDefaultScale[node] = node.localScale;

            for (int i = 0; i < node.childCount; i++)
            {
                InitDefaultScale(node.GetChild(i));
            }
        }

        private void InitDefaultSpeed()
        {
            psDefaultSpeedDic.Clear();
            if (mainPs != null)
            {
                psDefaultSpeedDic.Add(transformRootName, mainPs.main.simulationSpeed);

            }
            if (particleSystems != null)
            {
                for (int i = 0; i < particleSystems.Length; i++)
                {
                    ParticleSystem particleSystem = particleSystems[i];
                    psDefaultSpeedDic.Add($"{particleSystem.transform.name}_{i}", particleSystem.main.simulationSpeed);
                }
            }
        }

        public void Init(LocalEffectTags _localEffectTags, bool needSetPos, Vector3 confPos, float playerTime = 0, float startDelay = 0, bool isFollowBuilderHide = true, bool isFaceToCamera = false)
        {
            // SGF.Debuger.LogError($"[--x] init {transform.name}");
            visible = true;
            // hiddenCount = 0;
            RefreshPsRenderShow();

            M_localEffectTags = _localEffectTags;
            gameObject.SetActive(false);
            transform.localPosition = Vector3.zero;
            if (needSetPos)
            {
                transform.position = confPos;
            }
            switch (_localEffectTags)
            {
                case LocalEffectTags.UI:
                    gameObject.layer = LayerMask.NameToLayer("UI");
                    break;
                case LocalEffectTags.Env:
                    break;
                default:
                    break;
            }

            if (startDelay != 0 && mainPs != null)
            {
                //表中的500毫秒，转换unity秒
                mainPs.startDelay = startDelay;
            }

            if (mainPs != null)
            {
                if (playerTime == 0)
                {
                    ///Delay=5，Durning=10 ：不Loop，5~15秒播放一次
                    ///实现unity的默认方式，总时间
                    ///Unity和美术说的算
                    M_setPlayerTime = mainPs.main.duration + mainPs.startDelay;//bofang//播放//设置本发射器的总体时间长度
                }
                else
                {
                    //你填写的PlayTime，只填写特效表现的DurningTime即可
                    ///Delay=5，Durning=10 ：不Loop，5~15秒播放一次
                    ///这种情况：你就写10，但是你知道是15就好；因为如果让你填总时间的话你还需要看特效的DelayTime
                    ///    //表中的500毫秒，转换unity秒，生命时间长度39
                    M_setPlayerTime = playerTime + mainPs.startDelay;
                }
            }

            // 循环播放且 没设置最大时间, 那设置播放时间为 -1 , 无限循环播放
            if (playerTime == -1)
            {
                M_setPlayerTime = -1;//先这么写吧
            }

            // 还原默认参数
            LookAtCamera = isFaceToCamera;

            IsFollowBuilderHide = isFollowBuilderHide;
            // SGF.Debuger.LogWarning($"gameEffect[{this.transform.name}] init : M_setPlayerTime {M_setPlayerTime}");
        }

        public void SetLoop(bool isLoop, int liftTime = 5)
        {
            if (mainPs != null)
            {
                var main = mainPs.main;
                main.loop = isLoop;

                if (!isLoop)
                {
                    if (liftTime > 0)
                    {
                        main.startLifetime = liftTime;
                    }
                    else
                    {
                        // 配置中漏的特效, 默认设为 PS_MAX_LIFE_TIME
                        main.startLifetime = PS_MAX_LIFE_TIME;
                    }

                }
                else
                {
                    // // 循环特效 先默认 60s 
                    // 循环特效先不改
                    // main.startLifetime = PS_MAX_LIFE_TIME;
                }
            }

        }

        //设hi是播放的前提，还想代码，还想表现出来？
        public void Play(float startTime = 0)
        {
            //生命周期控制：重启播放必然入口，0开始
            playTime = 0;

            Playing = true;

            if (mainPs != null)
            {
                SetStartTime(startTime);
            }
            // SGF.Debuger.LogWarning($"gameEffect[{this.transform.name}] Play : startTime {startTime}");
        }

        public void SetScale(Vector3 scaleXYZ)
        {
            // for (int i = 0; i < transform.childCount; i++)
            // {
            //     SetScale(transform.GetChild(i), scaleXYZ);
            // }
            //1递归保证效果?不是参数？，2每一层1下可以。那你为啥要new
            transform.localScale = CalculateScale(psDefaultScale[transform], scaleXYZ);
        }

        /// <summary>
        /// 计算 scale 大小
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="scaleRatio">scale的缩放因子</param>
        /// <returns></returns>
        private Vector3 CalculateScale(Vector3 scale, Vector3 scaleRatio)
        {
            Vector3 resultScale = Vector3.zero;
            resultScale.x = scale.x * scaleRatio.x;
            resultScale.y = scale.y * scaleRatio.y;
            resultScale.z = scale.z * scaleRatio.z;

            return resultScale;
        }

        /// <summary>
        /// 修改特效的播放速度
        /// </summary>
        /// <param name="speed"></param>
        public void SetSpeed(float speed)
        {
            if (mainPs != null)
            {
                SetParticleSystemSpeed(mainPs.main, speed);
            }

            if (particleSystems != null)
            {
                foreach (ParticleSystem particle in particleSystems)
                {
                    SetParticleSystemSpeed(particle.main, speed);
                }
            }
        }

        private void SetDefaultSpeed()
        {
            if (mainPs != null)
            {
                float rootDefaultSpeed = GetDefaultSpeed(transformRootName);
                SetParticleSystemSpeed(mainPs.main, rootDefaultSpeed);
            }

            if (particleSystems == null || particleSystems.Length <= 0)
            {
                return;
            }

            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem particleSystem = particleSystems[i];
                float speed = GetDefaultSpeed($"{particleSystem.transform.name}_{i}");
                SetParticleSystemSpeed(particleSystem.main, speed);
            }
        }

        /// <summary>
        /// 设置粒子特效 开始播放的时间点
        /// </summary>
        /// <param name="startTime"></param>
        private void SetStartTime(float startTime)
        {
            if (mainPs != null)
            {
                MainModule main = mainPs.main;
                // main.prewarm = true;
                // ps.time = startTime;

                mainPs.Simulate(startTime, true, true, true);
                mainPs.Play();
            }

            if (particleSystems == null || particleSystems.Length <= 0)
            {
                return;
            }
            // TODO DL
            // 子节点 main.prewarm = true; 并且 Simulate(startTime, false) 后会出现特效出不来的情况
            // 目前不知道为啥

            // for (int i = 0; i < particleSystems.Length; i++)
            // {
            //     ParticleSystem particleSystem = particleSystems[i];
            //     MainModule main = particleSystem.main;
            //     // main.prewarm = true;
            //     // particleSystem.time = startTime;
            //     // particleSystem.Simulate(startTime, false);
            // }
        }

        private void SetParticleSystemSpeed(MainModule mainModule, float speed)
        {
            mainModule.simulationSpeed = speed;
        }

        private float GetDefaultSpeed(string key)
        {
            if (psDefaultSpeedDic.TryGetValue(key, out var defaultSpeed))
            {
                return defaultSpeed;
            }
            return 1;
        }

        public void SetCloseAction(System.Action closeAction)
        {
            if (closeAction != null)
            {
                OnClose = closeAction;
            }
        }

        //public void SetHeader(GameObject obj)
        //{
        //    this.followHeader = obj;
        //}

        /// <summary>
        /// 当特效被归还到 池的时候,一般是 foreach 返还到池里面, 所以此时一般也不需要 执行OnClose 才对
        /// </summary>
        public void OnReturnEffect()
        {
            OnClose = null;
            Playing = false;
            if (mainPs != null)
            {
                mainPs.Clear();
            }
        }

        internal void DestroyRelease()
        {
            if (mainPs != null)
            {
                mainPs.Stop();
            }
            //SetDefaultSpeed();//情况1，显示隐藏
            Destroy(gameObject);//情况2，添加删除
        }

        internal void DestroyRelease(LocalEffectTags condition)
        {
            if (condition == M_localEffectTags)
            {
                if (mainPs != null)
                {
                    mainPs.Stop();
                }
                Destroy(gameObject);
            }
        }

        public bool EqualTags(LocalEffectTags tag)
        {
            return tag == M_localEffectTags;
        }

        /// <summary>
        /// 1，通过Active的话会出现，Deactive的时候特效中断了（当策划的需求是需要继续播放），并且二次打开想显示的时候（冲头播放），这不符合需求
        //2，通过Scale会出现下面有的不会被缩放
        //3，位置（客户端有时候会需要碰撞感知）
        //4，通过层级可能出现穿墙不穿墙设置
        //5，通过特效里面的Render节点
        //做法：特效脚本初始化的时候 记录一个：所有【render开着的（本来就不开的记录他干啥！！）】 《ParticleSystem，Bool》的字典
        //1，记录所有子节点的 Renderer节点
        //2，渲染就都打开，不渲染就关闭
        /// </summary>
        /// <param name="isShow"></param>
        public void ShowHideParticleRender(bool isShow)
        {
            visible = isShow;
            // hiddenCount = isShow ? (++hiddenCount) : (--hiddenCount);
            // if (hiddenCount < 0)
            // {
            //     hiddenCount = 0;
            // }
            RefreshPsRenderShow();
        }

        private void RefreshPsRenderShow()
        {
            psRenderers.ForEach((ParticleSystemRenderer psRenderer) =>
            {
                // psRenderer.enabled = hiddenCount == 0;
                psRenderer.enabled = visible;
            });
        }

        private void FillPsRenderer(ParticleSystem particleSystem)
        {
            if (particleSystem == null)
            {
                return;
            }
            ParticleSystemRenderer psRender = particleSystem.GetComponent<ParticleSystemRenderer>();
            if (psRender != null && psRender.enabled)
            {
                psRenderers.Add(psRender);
            }
        }

        private void FillRsRenderers()
        {
            FillPsRenderer(mainPs);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                FillPsRenderer(particleSystems[i]);
            }
        }

        #region 屏幕特效

        void OnBecameInvisible()
        {
            //如果发送了屏幕效果事件,那就结束这个屏幕效果
            if (isEmitCameraEvent)
            {
                GlobalEvent.OnCameraStopEvent.Invoke(cameraEventID, emitCameraEvent, E_CameraType.StarWorldCam);
            }
        }
        //基于看见【特效就触发】
        //FxView 和 GameEffect 都是可能会挂载特效的脚本
        //        OnBecameVisible() : 当物体在/进入摄像机视野内会调用一次，类似触发器OnTriggerEnter();
        //        OnBecameInvisible() : 当物体离开摄像机视野会调用一次，类似触发器OnTriggerExit();
        //1.首先保证物体上具有MeshRenderer
        //2.触发这两个方法的类是添加给物体，不是摄像机
        //3.unity编辑器测试时注意：在scence界面中的射线机也被算进去了「传疯」
        //void OnBecameVisible()
        //{
        //    //段磊需要判断，别让每个特效都进去这个流程，没有配置的就return掉
        //    SGF.Debuger.LogError("当物体进入相机视野");
        //}
        //void OnBecameInvisible()
        //{
        //    SGF.Debuger.LogError("当物体离开相机视野");
        //}
        //基于特效出生时间才触发：CommonFunction

        /// <summary>
        /// 屏幕特效的处理接口
        /// </summary>
        /// <param name="ownerEntityId">特效的播放者</param>
        /// <param name="targetEnityId">由 target 产生,如 怪物对 我产生的效果, 那targetEnityId 就是怪物id </param>
        /// <param name="isMonster"></param>
        /// <param name="mainPlayerId"></param>
        /// <param name="cameraEffectID"></param>
        /// <param name="cameraEffctType"></param>
        public void HandleCameraEffect(ulong ownerEntityId, ulong targetEnityId, bool isMonster, ulong mainPlayerId, int cameraEffectID, int cameraEffctType)
        {
            ///震屏效果 会分为2种:
            /// 1.广播类型效果, 所有人(怪和其他人)播放广播特效,都会广播给所有人;
            /// 2.非广播类型效果,只针对 自己 播放的技能效果, 比如 砸地;
            if (cameraEffectID > 0)
            {
                SGF.Debuger.Log($"{LOG_TAG} HandleCameraEffect ownerEntityId {ownerEntityId} targetEnityId {targetEnityId} isMonster {isMonster} mainPlayerId {mainPlayerId} , cameraEffectID {cameraEffectID} cameraEffctType {cameraEffctType}");
                //1.是否是自己播放效果
                bool isSelfPlayEffect = targetEnityId == mainPlayerId;

                e_CameraEffectType = (E_CameraEffectType)cameraEffctType;
                E_CameraEffectBroadCastType broadCastType = E_CameraEffectBroadCastType.NotBroadCast;

                switch (e_CameraEffectType)
                {
                    case E_CameraEffectType.Shake:
                        {
                            CameraShakeDataCell cfg = LocalDataManager.Instance.GetCameraShakeDataCell(cameraEffectID);
                            broadCastType = (E_CameraEffectBroadCastType)cfg.GetBroadCastType();
                            emitCameraEvent = CameraEvent.ShakeCam;
                        }
                        break;
                    case E_CameraEffectType.Zoom:
                        {
                            CameraZoomDataCell cfg = LocalDataManager.Instance.GetCameraZoomDataCell(cameraEffectID);
                            broadCastType = (E_CameraEffectBroadCastType)cfg.GetBroadCastType();
                            emitCameraEvent = CameraEvent.CameraZoom;
                        }
                        break;
                    default:
                        {
                            SGF.Debuger.LogWarning($"{LOG_TAG} HandleCameraEffect targetEnityId {ownerEntityId} isMonster {isMonster} mainPlayerId {mainPlayerId} , cameraEffectID {cameraEffectID} cameraEffctType {cameraEffctType} no handle!!!");
                        }
                        break;
                }

                // //如果不是怪物,也不是自己
                // //说明时其他人,其他人的震屏不会对我自己产生效果
                // if (!isMonster && !isSelfPlayEffect)
                // {
                //     return;
                // }

                //接下来只考虑 自己 和 怪物 产生的特效的屏幕效果
                switch (broadCastType)
                {
                    case E_CameraEffectBroadCastType.NotBroadCast:
                        {
                            //不广播的特效 只 针对自己播放 技能效果: 比如 砸地
                            //只有在视野范围内才播放震屏
                            //视野判断放在 CamerManager中,HandleCameraEffect 执行的时候,特效还没显示  
                            if (isSelfPlayEffect)
                            {
                                isEmitCameraEvent = true;
                            }
                        }
                        break;
                    case E_CameraEffectBroadCastType.BroadCast:
                        {
                            //广播的 特效, 所有人(怪和其他人) 播放广播特效,都会广播给所有人
                            //不需要考虑谁播放,策划只要配了是广播类型,那就播
                            isEmitCameraEvent = true;
                        }
                        break;

                    default:
                        break;
                }

                // if (!isMonster)
                // {
                //     //2.如果不是怪物，并且也不是自己播放效果，那就不震屏了
                //     //  否则只要是自己播效果，就都得播
                //     isEmitEvent = isSelfPlayEffect;
                // }
                // else
                // {
                //     //如果是怪物，怪物的震屏分两种，一种是 广播式震屏，一种是 命中式震屏。
                //     //广播震屏 直接可以发送
                //     //命中式震屏，需要判断是否命中
                //     //3.如果是广播类型，则直接广播出去，由外面收到后，再去处理距离或者其它的条件
                //     if (broadCastType == E_CameraEffectBroadCastType.BroadCast)
                //     {
                //         isEmitEvent = true;
                //     }
                //     else
                //     {
                //         //4. 如果是命中类型的效果，则需要区分 是否是自己命中 了怪物
                //         //   所以需要区分 怪物播放的效果的 ownnerEntityID 是否是自己
                //         bool isSelfOwnner = ownerEntityId == mainPlayerId;
                //         isEmitEvent = isSelfOwnner;
                //     }
                // }

                if (isEmitCameraEvent)
                {
                    cameraEventID = CameraManager.Instance.GetCameraEventID();
                    GlobalEvent.OnCameraEvent.Invoke(cameraEventID, cameraEffectID, e_CameraEffectType, transform.position);
                }
            }
        }

        #endregion

    }
}