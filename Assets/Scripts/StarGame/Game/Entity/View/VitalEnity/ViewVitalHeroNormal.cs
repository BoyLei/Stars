using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Service.Cam;
using StarProject.Service.Sound;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 人的显示层
/// </summary>
namespace StarProject.Game.Entity.View.VitalSign
{
    public class ViewVitalHeroNormal : ViewVitalNPCNormal
    {
        //private string LOG_TAG = "VVitalNormal";

        //别人的重力来源于位置，位置来源于服务器，来源于我的信息中转
        //我来源于碰撞，来源于重力，所以最终来源于我驱动的Fix
        //主角有就好，其他人都不用，因为服务器转达加存储，但是不算
        protected Vector3 m_TempPos;


        //确认了，杨睿涵确认，其他生物都没脚步声音，其他人也没有脚步声音，只有主角有脚步声音
        public AK.Wwise.Switch m_Switches;//AKSwitch，需要AddComp，然后GetComp，因为是Mono，没必要就算了
        public new HeroEntityBase M_EntityBase
        {
            get
            {
                return m_entity as HeroEntityBase;
            }
        }

        protected override void Create(EntityObject entity)
        {
            m_entity = entity as HeroEntityBase;

            base.Create(entity);

            // 主角绑定技能指示器
            if (M_EntityBase != null)
            {
                if (M_EntityBase.Data.isMainPlayer && !HeroEntityBase.g_testModel)
                {
                    InitMlayerDefaultFootSound();

                    // 修改人节点的tag
                    gameObject.tag = E_TagType.MainPlayer.ToString();
                    SoundManager.Instance.RegMainPlayer(gameObject);
                    // 主角自己，设置虚拟相机的跟随节点
                    float localRotateX = GameConfig.MAX_ANGLE_OF_PITCH;
                    string mapSubType = GameManager.Instance.GetMapSubType();
                    if (mapSubType == GameConfig.INSTANCE_PLOT_FIRST)
                    {
                        localRotateX = 10.0f;
                    }
                    UnityEngine.Vector3 localRotate = new(localRotateX, 0, 0);
                    UnityEngine.Vector3 localPos = M_EntityBase.Data.Pos;
                    CameraManager.Instance.SetBattlePos(localRotate, localPos);
                    StarProject.Service.Cam.CameraManager.Instance.SetPlayerFlowTarget(gameObject.transform);
                    StarProject.Service.CameraShake.CameraShakeManager.Instance.SetMainPlayerTransfom(gameObject.transform);
                }
                else
                {
                    gameObject.tag = E_TagType.Player.ToString();
                }
            }
            //1,逻辑层分别通知,3d和2d信息面板,3d不通知3D因为这两个是逻辑分离的!!!
            //2,现在是出生通知注册,通常是AOI或者特殊情况服务器告诉我注册
            RegUIPendentToWordChecker();
        }

        private void RegUIPendentToWordChecker()
        {
            /* WorldItemChecker.Instance.RegToRectTransformPend(transform,);*/
            //rect init 自己找了
        }
        #region 声音材质部分
        public Dictionary<GameObject, AK.Wwise.Switch> switches = new();
        private void InitMlayerDefaultFootSound()
        {
            if (M_EntityBase.Data.isMainPlayer)
            {
                if (GameManager.Instance.M_Map != null && GameManager.Instance.M_Map.M_rootHelper != null && GameManager.Instance.M_Map.M_rootHelper.@switch != null)
                {
                    //AkSwitch是对switch 使用的 封装，是基于触发这的设计理念

                    m_Switches = GameManager.Instance.M_Map.M_rootHelper.@switch;
                    //初始设置


                    /*Switch是切换我知道，他要的是事件，gameobject有配置refGob上面有事件事件做切换
                    也就是地面物件要事件。也就是他是基于地面发声的设计原则
                    但是美术那样就要拆分地块terrain就不对，我是基于人的*/
                    //他的设计原则是，地面有材质，谁用【哪个声音】，【在材质上进行变化】，【耳朵谁在附近】
                    //MMo是【一个人在什么材质容器环境中】【发送自己脚步声音】【目前发送给自己】
                    m_Switches.SetValue(gameObject);//其实没什么用我上面也没挂载什么脚本


                    //发送一个 Gameobject 和id 给AKengine 哪个有就用哪个 基于Editor编辑的设计模式
                    /*AK.Wwise.Event akEvent;
                    akEvent.Post*/
                }

            }
        }

        public override void PlayerWWise(string soundBank, string eventName)
        {
            SoundManager.Instance.PlayWwiseAudio(eventName, false, E_SoundNTFtype.Target, soundMaker: gameObject);
            //DL
        }
        /// <summary>
        /// 碰撞注册  理解到底是地面出的声音还是人|两边都有多样性|考虑维护列表么不用
        /// 进入isin == true/离开都需要处理
        /// </summary>
        /// <param name="_switch"></param>
        private void RegFootSound(AK.Wwise.Switch _switch, GameObject triggerGob, bool isIn)
        {
            if (M_EntityBase.Data.isMainPlayer)
            {
                if (isIn)
                {
                    if (!switches.ContainsKey(triggerGob))
                    {
                        switches.Add(triggerGob, _switch);

                        //1因为脚下 的东西触发器东西碰撞的，保底逻辑
                        //为什么重复触发？不是stay
                        //但是没必要相同的东西也触发，给自己相同的，重复处理没问题但是消耗
                        //应该时刻换，但是地图是空没必要换，容易出错
                        if (GameManager.Instance.M_Map != null)
                        {
                            //AkSwitch是对switch 使用的 封装，是基于触发这的设计理念
                            //以人为主，设置新的声音
                            m_Switches = _switch;
                            //初始设置
                            //设置自己
                            m_Switches.SetValue(gameObject);
                        }
                        //进入就把新的给他
                    }

                }
                else
                {
                    if (switches.ContainsKey(triggerGob))
                    {
                        switches.Remove(triggerGob);
                    }

                    if (switches.Count == 0)
                    {
                        m_Switches = GameManager.Instance.M_Map.M_rootHelper.@switch;
                        m_Switches.SetValue(gameObject);
                    }
                    else
                    {
                        //碰撞这种东西，肯定不是先进先出，先进后出，只能随便给一个，用不着用Stay来检测（浪费），
                        //同时也用不着把上一个给他
                        //压根就不允许两个碰撞盒子有交集
                        //尽量我给你最后一个（兼容类似完全包裹形式），不过一定会出错，策划不允许有交集
                        foreach (var item in switches)
                        {
                            //取得第一个，直到最后一个
                            if (item.Key != null)
                            {
                                m_Switches = item.Value;
                                m_Switches.SetValue(gameObject);
                            }
                        }
                    }
                    //缓存那个gob
                    //通过他设置我为草地之脚
                    //我设置我自己为结果
                    //触发我自己
                }
            }

        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<AkSwitchMaterial>() != null)
            {
                RegFootSound(other.GetComponent<AkSwitchMaterial>().Switch, other.gameObject, true);
            }

        }
        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<AkSwitchMaterial>() != null)
            {
                RegFootSound(other.GetComponent<AkSwitchMaterial>().Switch, other.gameObject, false);
            }
        }
        #endregion


        /// <summary>
        /// FixUpdate去更新
        /// 主角物理放在fix
        /// </summary>
        /// <param name="obj"></param>
        protected override void OnEnterFixFrame(Vector3 skillmove)
        {
            if (M_EntityBase != null)
            {
                //主角 和 0 缺一不可，因为非主角，并不会因为初始是0就会被碰撞抬起
                //动态帧率：0~max60
                //这里是任意实体：是主角儿，或者非
                if (M_EntityBase.Data.isMainPlayer)
                {
                    //主角1次收，全部发
                    MainPlayerControl(skillmove);
                }
                else
                {
                    // OtherPlayerByControl();
                    OnFrameMove();
                }
            }
        }

        protected override void Release()
        {
            //WorldItemChecker.Instance.UnRegToRectTransformPend(transform);

            if (M_EntityBase.Data.isMainPlayer && !HeroEntityBase.g_testModel)
            {
                CameraManager.Instance.ReleasePlayerFlowTarget();
                StarProject.Service.CameraShake.CameraShakeManager.Instance.SetMainPlayerTransfom(null);
            }


            base.Release();
            m_entity = null;
            m_context = null;
        }
        //NavMeshPath path = new NavMeshPath();
        //主角：客户端通知服务器
        //其他人，怪物，技能：服务器通知客户端
        //C => S 分批处理 + 地面高度模拟
        //S => C 只有固定点 + 地面高度模拟
        private void MainPlayerControl(Vector3 SkillMove)
        {
            if (!m_IsSyncPos)
            {
                return;
            }
            //SGF.Debuger.LogError($"[状态切换] MainPlayerControl() 摇杆状态={M_EntityBase.Is___JoySitckStop},禁移动={M_EntityBase.Is___ForbidMove},禁朝向={M_EntityBase.Is___ForbidDir},向量={M_EntityBase.M_EntityMoveDir}");

            //动态追赶逻辑
            //if (M_EntityBase.M_CtrlByServer == false)
            {
                //技能移动
                if (SkillMove != Vector3.zero)
                {

                }
                else if (!M_EntityBase.Is___JoySitckStop || M_EntityBase.Data.IsBattleState_BlackHole)    //普通移动事件【v】 或者 [黑洞] 效果下, 服务器发送的坐标 作为 输入指令让主角移动
                {
                    #region 提醒
                    //这里需要拼出Y在给服务器Fix通知
                    {   //方法一:Update中更新，【这是错】！！！的Update帧率不稳，移动轮盘也不固定，技能更不确定
                        //1秒时 = 动态 * 30次Fix
                        //float detalTime = Time.deltaTime * GameConfig.FIX_TIME_PER_SEC;
                        //m_CharacterController.Move(M_EntityBase.GetChangeVector() * detalTime);// * 30/* * moveSpeed * Time.deltaTime*/);//一帧就过去；Time.deltaTime是60帧1秒过去0~1的过程；虽然不是帧数同步不过效果没错的原因是后续差距过大也会lerp
                    }
                    {
                        //方法2：直接设置，碰撞不要了奥？
                        //this.transform.localPosition = M_EntityBase.Position();//只更新XZ
                    }
                    {
                        //每次间隔1/N秒，共调用N次，总系数为1，调用时总距离就是最后【1秒的Vt】，每次调用就是【此帧刻的Vt】，公式嘛
                        //此时需要向下移动的距离，就是本次时间，调用时单位时间的，帧移动速度就是距离
                        //方法3：逻辑帧数Fix推，View执行，稳定30，不需要你给我144同样你也别给我2，物理就是Fix，只不过是View层而已
                        //我不需要显示的过度，我本质就是加等，我要的是插值，你给的是插值
                        //我需要物理，我需要逻辑驱动推送，我不需要update弥补fix的显示和逻辑
                        //每秒30帧，稳定推送插值，做物理；逻辑计算->表现处理->利用unity更新逻辑->同步客户端逻辑->同步服务器
                    }
                    #endregion
                    if (!M_EntityBase.Data.Is___ForbidMove) //滑动过程中，未禁原子状态
                    {
                        {
                            // Vector3 needChangeMotion = M_EntityBase.M_EntityMoveDir;//【1调用逻辑层】


                            /// 2023/9/26
                            /// 如果是黑洞效果， 在每次服务器属性同步黑洞坐标后, 客户端都会将将属性坐标的偏移量设置为 M_EntityMoveDir.
                            /// 在 玩家 每次 执行完 这个偏移量后, 都需要将 这个偏移量清空才行.
                            // 每次移动完成后,清除移动偏移量
                            Vector3 needChangeMotion = Vector3.zero;
                            if (M_EntityBase.Data.IsBattleState_BlackHole)
                            {
                                needChangeMotion = M_EntityBase.M_EntitySmoothMoveV3.UpdateMoveData(M_EntityBase.m_EntityServerFrameMoveDir).CurMoveData;
                                SGF.Debuger.LogWarning($" [黑洞] [平滑] M_EntityServerFrameMoveDir: {M_EntityBase.m_EntityServerFrameMoveDir} ---> {needChangeMotion}  ");
                                M_EntityBase.m_EntityServerFrameMoveDir = Vector3.zero;
                            }
                            else
                            {
                                needChangeMotion = M_EntityBase.M_EntityMoveDir;
                                // SGF.Debuger.LogWarning($" [黑洞] [平滑] moveDir: {M_EntityBase.M_EntityMoveDir} ---> {needChangeMotion}  ");
                                M_EntityBase.M_EntityMoveDir = Vector3.zero;
                            }

                            //needChangeMotion.y = this.VerticalVelocity;


                            bool isOnNavMesh = GameManager.Instance.IsPointOnNavMesh(m_CharacterController.transform.position + needChangeMotion);
                            if (isOnNavMesh)
                            {
                                // 目标点在NavMesh上，可以继续移动
                                //nma.SetDestination(targetPosition);
                                m_CharacterController.Move(needChangeMotion);//fix，//【主角自己控制】
                            }
                            else
                            {
                                // 目标点不在NavMesh上，暂停移动
                                //nma.ResetPath();
                            }

                            //m_CharacterController.Move(needChangeMotion);//fix，//【主角自己控制】




                            //Vector3 testPoint = transform.position; // 将此点替换为您要测试的点





                            //不用清理，要最后一帧的方向缓存，不发即可
                        }
                        m_TempPos = m_context.EntityToViewPoint(this.transform.localPosition);
                        //【2完成Set逻辑层】
                        M_EntityBase.SetCollY_FilterSSPData(m_TempPos);//标记和推送逻辑层，和信息再次拦截过滤到服务器

                        //GCamera.UpdateCameraPos(m_TempPos);

                        m_EntityPosition = transform.localPosition;
                    }
                    // 在逻辑层通知角度改变了
                    //if (!M_EntityBase.Is___ForbidDir)
                    //{
                    //    OnAngelChangeMove(Vector3.one, true);//这个值用不着
                    //    //this.transform.localEulerAngles = M_EntityBase.EulerAngles;
                    //}
                }
                else if (M_EntityBase.Is___SkillJoyStick) //技能移动事件
                {
                    // 技能遥感发生的角色转向
                    if (!M_EntityBase.Data.Is___ForbidDir)
                    {
                        OnAngelChangeMove(M_EntityBase.EulerAngles.y, true, 0, false);//这个值用不着
                        //this.transform.localEulerAngles = M_EntityBase.EulerAngles;
                    }
                }
                else if (M_EntityBase.Is_MainPlayer_FindingPath)
                {
                    if (!M_EntityBase.Data.Is___ForbidMove) //滑动过程中，未禁原子状态
                    {
                        m_TempPos = m_context.EntityToViewPoint(this.transform.localPosition);
                        //【2完成Set逻辑层】
                        M_EntityBase.SetCollY_FilterSSPData(m_TempPos);//标记和推送逻辑层，和信息再次拦截过滤到服务器
                        //GCamera.UpdateCameraPos(m_TempPos);
                    }

                    // 这里不需要每次通知，主角寻路的时候在逻辑层会通知
                    //if (!M_EntityBase.Is___ForbidDir)
                    //{
                    //    OnAngelChangeMove(Vector3.one, true);//这个值用不着
                    //    // this.transform.localEulerAngles = Vector3.zero;
                    //}
                }
                else
                {
                    /// 2024-6-28
                    /// 
                    /// fixed by DL:
                    ///     定位到 DoPath 寻路还没有结束, 但是 Is_MainPlayer_FindingPath 寻路状态被设置为false.
                    ///     导致主角移动了,但是没有给 服务器同步坐标.  从而导致 使用技能时， 被服务器强拉.
                    /// 
                    /// 此处增加一个补丁， 在这种情况下也给服务器同步坐标. 
                    /// 
                    /// note:
                    ///     1.强同步拉回目前并没有打断 客户端 DoTween，这是个显式存在的逻辑bug;
                    ///     2.寻路状态 为何在 DoTween 过程中被打断，需要寻路系统那边细查一下;
                    ///     3.此 补丁很临时，移动代码 最好重构一下.
                    if (!M_EntityBase.Data.Is___ForbidMove) //滑动过程中，未禁原子状态
                    {
                        m_TempPos = m_context.EntityToViewPoint(this.transform.localPosition);
                        //【2完成Set逻辑层】
                        M_EntityBase.SetCollY_FilterSSPData(m_TempPos);//标记和推送逻辑层，和信息再次拦截过滤到服务器
                        //GCamera.UpdateCameraPos(m_TempPos);
                    }

                }
            }


        }



        #region 过时，可删除的

        #region Update
        /// <summary>
        /// 动态帧率 60~70
        /// </summary>
        //protected virtual void Update()
        //{
        //    #region 机制复习
        //    ///1,Lock了Fix；
        //    ///2,但Update其实不必锁定如需锁定参考脚本拓展在MonoHelper中；
        //    ///3,既然不锁定他们之间的配合关系是Logic驱动动态View
        //    //SGF.Debuger.Log("UnLock:" + TimeUtils.GetTimeSpanSince1970().Seconds + "0~40~50~60~140");折叠出每秒动态帧率都是不同的，但本质是每次甚至都不稳定

        //    //把V Sync Count 设置为不限制（Don`t Sync）（我们用脚本限制，不然unity自己控制不了它自己，亲测真的） +  Lod Bias设置为2（默认是1，不能用默认）
        //    //UpdateFrame:脚本|虽然而已锁但不允许|update提供了更高的容纳度属于弱流程逻辑，Fix提供准确但容纳度低阻断强属于强流程逻辑

        //    //这里只能动态补数据，不可以有逻辑控制，逻辑分离的核心意义是逻辑层运算是必要的同步的极简的；表现层是玩家动态性能的不重要的允许卡顿而且应该可动态适配甚至可忽略每次流程的

        //    //Update极其不稳定，每一帧每一秒都不稳定

        //    //SGF.Debuger.Log(Time.deltaTime + "__" + TimeUtils.GetTimeSpanSince1970().Seconds);//【每次打印与上次的插值】，可能0.01到2秒不等 || 动态帧无法保证关键信息 || 
        //    //如1秒144次，那144的时间差之和是1秒||因为打印是过程值所以要配合累加类型的逻辑，如Translate，MoveTo 这种本质是+=的操作
        //    #endregion
        //    #region 控制
        //    //Time.deltaTime 动态帧处理 如144帧数 = 144帧次对应1秒（只是和是1秒但根本不平均）
        //    //| 当加等逻辑配合的时！Translate MoveTo！就相当于每次Update变化量是：
        //    //Time.deltaTime【一次Update用时】 * Speed = 1次Update的距离
        //    //数学上：N次递增时间和为1时，N次调用=B-A + C -B ... = Z - A=1 时间为1；当多次时间堆积为1秒时，（update中1秒）中累加距离也是【+= 速度 * DTime】
        //    //T * V = 1秒的Pos变化【速度值 = 距离值】

        //    //Vector3.up * 0.5f 动态帧执行变化量 = 1显示帧时间（差） * 速度 = 1显示帧的插值位移 = 1显示帧的路程
        //    //m_CharacterController.Move【+=】(Vector3.up  * Time.deltaTime);//每显示帧内插值(速度 或 距离) 在累加的情况下 ，【每秒的累加】变化值恰好是Vector3.up
        //    //当然了这里会因为显示帧的卡顿，（效果上是）减少次数，增加步长，导致判定非逐步，进而引发几何和物理的判定失常

        //    //m_CharacterController.Move(Vector3.up);//每帧1(速度 或 距离)：每一帧增加1，但由于显示帧压根不稳定，按照显示帧做【同步客户端】就是耍流氓
        //    //SGF.Debuger.Log(m_CharacterController.transform.position + "__"+ TimeUtils.GetTimeSpanSince1970().Seconds);

        //    #endregion
        //    #region 测试
        //    //--测试代码---
        //    //m_entityPosition = M_EntityBase.Position();
        //    //Vector3 pos2 = m_context.EntityToViewPoint(m_entityPosition);
        //    //this.transform.localPosition = pos2;
        //    //SGF.Debuger.Log("ALL：" + "m_entityID" + M_EntityBase.EnityId + "_" + TimeUtils.GetTimeSpanSince1970().Seconds);
        //    #endregion

        //    //别人也不用我碰撞，服务器都知道NavMesh，属于非重要信息，【这种中转信息就属于单向RPC本质，虽然没调用人家方法】
        //    //1,碰撞我主导，移动我主导，主导属于重要，同步属于重要，发起者属于重要
        //    //2,别人属于非重要，我重要
        //    //3,别人不能放太多到fix，别人需要平滑但视情况而定,我必须保持驱动力
        //    //if (M_EntityBase != null)
        //    //{
        //    //    //m_renderer.enabled = M_EntityBase.ShowData.bodyVisible;
        //    //    //主角 和 0 缺一不可，因为非主角，并不会因为初始是0就会被碰撞抬起
        //    //    //动态帧率：0~max60
        //    //    //这里是任意实体：是主角儿，或者非
        //    //    if (M_EntityBase.Data.isMainPlayer)
        //    //    {
        //    //        //主角1次收，全部发
        //    //        //MainPlayerControl();
        //    //    }
        //    //    else
        //    //    {
        //    //        //OtherPlayerByControl();//原来这里处理，现在用Tweener,MMLERP更好
        //    //    }
        //    //}
        //}
        #endregion

        #region ifItIsSingLetonPanelUpdate
        ///// <summary>
        ///// 融合1对多，多对1 || 最差可以用global，看设计是1对多还是多对1还是1对1 || view通过事件拥有logic弱弱的耦合，因为事件是逻辑层到逻辑层，逻辑层到界面层
        /// <summary>
        /// logic不会拥有view的list因为要数据到logic可能通知其他logic||而且防止相互持有||通知主体限制在logic上
        /// </summary>
        /// <param name="obj"></param>
        ///// </summary>
        ///// <param name="entity"></param>
        //private void ifItIsSingLetonPanelUpdate(EntityObject entity) 
        //{
        //    if (M_EntityBase.RefreshActions != null)
        //    {
        //        M_EntityBase.RefreshActions -= OnRefreshActions;


        //        M_EntityBase = entity as VitalSignsEntityBase;

        //        M_EntityBase.RefreshActions += OnRefreshActions;
        //    }
        //}
        #endregion

        #region OtherPlayerByControl
        //private void OtherPlayerByControl()
        //{
        //    //TODO：角度提取出来另算
        //    //有就更的拦截，检测频次根据Update动态帧率算
        //    //直接设置没有差池，不暂时需要过度，且不要影响性能

        //    m_EntityPosition = M_EntityBase.Position();
        //    // 第一次 第三方玩家是直接设置坐标
        //    if (tempV3Pos == Vector3.zero)
        //    {
        //        this.transform.localPosition = m_EntityPosition;//只更新XZ
        //        this.transform.localEulerAngles = M_EntityBase.EulerAngles;
        //        tempV3Pos = m_EntityPosition;
        //        return;
        //    }
        //    //我这里的：其他人都在收，不会发
        //    //SGF.Debuger.Log("收：" + pos + "m_entityID" + M_EntityBase.EnityId + "_" + TimeUtils.GetTimeSpanSince1970().Seconds);
        //    //换算
        //    pos = m_context.EntityToViewPoint(m_EntityPosition);

        //    float step = GameConfig.PLAYER_MOVE_SPEED_PER_FIXFRAME;
        //    //if (M_EntityBase.M_CtrlByServer)
        //    {
        //        step = 1;
        //    }
        //    //m_CharacterController.Move(pos);
        //    Vector3 t = Vector3.Lerp(this.transform.localPosition, pos, step);//只更新XZ
        //    tempV3Pos.x = t.x;
        //    tempV3Pos.y = pos.y;
        //    tempV3Pos.z = t.z;
        //    m_CharacterController.Move(tempV3Pos - this.transform.localPosition);
        //    //this.transform.localPosition = tempV3Pos;
        //    this.transform.localEulerAngles = M_EntityBase.EulerAngles;
        //    if (Vector3.Distance(transform.localPosition, pos) < 0.1f)
        //    {
        //        //M_EntityBase.IsDriveByServerStop = true;
        //        M_EntityBase.IsJoySitckStop = true;
        //    }
        //    else
        //    {
        //        //M_EntityBase.IsDriveByServerStop = false;
        //        M_EntityBase.IsJoySitckStop = false;
        //    }



        //    //this.transform.localPosition = pos;//只更新XZ
        //    //this.transform.localEulerAngles = M_EntityBase.EulerAngles;

        //    // M_EntityBase.IsDriveByServerStop = true;//既然呗服务器控制，闲置状态就是停

        //    //if (m_PosData.Count == 0)
        //    //{
        //    //    M_EntityBase.IsDriveByServerStop = true;
        //    //    M_EntityBase.IsJoySitckStop = true;
        //    //}
        //    //else
        //    //{
        //    //    Vector3 dir = m_PosData[0] - transform.localPosition;
        //    //    float angles = (float)(Math.Atan2(dir.x, dir.z) * Mathf.Rad2Deg);
        //    //    this.transform.localEulerAngles = new Vector3(0, angles, 0);
        //    //    this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, m_PosData[0], (70 * Time.deltaTime));
        //    //    if (Vector3.Distance(transform.localPosition, m_PosData[0]) < 0.1f)
        //    //    {
        //    //        m_PosData.RemoveAt(0);
        //    //    }
        //    //    M_EntityBase.IsDriveByServerStop = false;
        //    //    M_EntityBase.IsJoySitckStop = false;
        //    //}

        //    // this.transform.localEulerAngles = M_EntityBase.EulerAngles;

        //}
        #endregion


        #endregion

    }
}
