///--------------------------------------------------------------------
/// 文件名   :   ViewObjectNormal
/// 内  容   :   物件显示层基类
/// 说  明   :  
/// 创建日期 :   2022/07/20 14:52:14
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Animancer;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using SGF.Unity;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.VitalSigns;
using StarProject.OffLine;
using StarProject.Service.LocalDynamic;
using StarProject.Service.LocalDynamic.Fx;
using StarProjectDef;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace StarProject.Game.Entity.View.VitalSign
{
    public class ViewObjectNormal : ViewModel
    {
        private string TagFlag = "[ViewAOI]";
        public static StringBuilder sb = new();

        public override string ModelPath
        {
            get
            {
                if (m_entity.modelDataCell != null)
                {
                    return m_entity.modelDataCell.ModelsPath;
                }
                return "";
            }
        }

        [SerializeField]
        protected Vector3 m_EntityPosition;

        protected ObstacleBase m_entity;

        /// <summary>
        /// 物体身上 特效记录
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="GameEffect"></typeparam>
        public DictionaryEx<string, FxGeParam> gameEffctDic = new();

        /// <summary>
        /// 播放特效时,生产的绑点的记录
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="Transform"></typeparam>
        public DictionaryEx<string, Transform> gamePointDic = new();


        protected Transform m_ModelNoRotationRoot;
        protected Vector3 m_EntityRotate = Vector3.zero;
        /// <summary>
        /// 模型跟随移动且跟随旋转节点
        /// </summary>

        public TweenerCore<float, float, FloatOptions> LerpRotate1;

        private GameObject go_interactiveEff;

        private Vector3 ModelPosOffsetConf = Vector3.zero;  // 模型配置的偏移
        protected Vector3 ModelRotOffsetConf = Vector3.zero;

        private bool m_IsShowInteractiveEff = false;

        protected override void Create(EntityObject entity)
        {
            m_entity = entity as ObstacleBase;
            if (m_entity == null)
            {
                return;
            }

            m_ModelOffset = transform.Find("ModelOffset");

            CreateModelNoRotationRoot();
            OnEventListener();
            InitAvatarModelPosOffset();
            InitAvatarModelRotOffset();
            //CreateModel();
            //SetModelScale();

            Action<bool> modelFinish = (res) =>
            {
                if (!res)
                {
                    SGF.Debuger.LogWarning($"{TagFlag} Create() entityid={m_entity.EntityId},entitytype={m_entity.EntityType},ModelPath={ModelPath},模型加载失败 err!!!");
                    return;
                }
                SetModelScale();
                SGF.UI.Framework.UIUtilsFrameWork.ChangeLayer(transform, E_LayerType.Entity.ToString());

                m_entity.ActionOnViewCreateFinish?.Invoke();

                OnAngelChangeMove(m_entity.EulerAngles.y, false, 0, false);
                OnForceMove(m_entity.ServerPosition);
            };
            CreateModelAsync(modelFinish);

            tag = E_TagType.Collections.ToString();// "InterActionObject";
        }

        protected virtual void OnEventListener()
        {
            if (m_entity != null)
            {
                m_entity.DoForceMove += OnForceMove;
                m_entity.DoAngelChange += OnAngelChangeMove;
                m_entity.ActionModelVisiable += OnActionModelVisiable;
                m_entity.ActionModelFesnel += OnActionModelFesnel;
                m_entity.ActionModelInteractiveEff += ActionModelInteractiveEff;
            }
        }

        protected virtual void OffEventListener()
        {
            if (m_entity != null)
            {
                m_entity.DoForceMove -= OnForceMove;
                m_entity.DoAngelChange -= OnAngelChangeMove;
                m_entity.ActionModelVisiable -= OnActionModelVisiable;
                m_entity.ActionModelFesnel -= OnActionModelFesnel;
                m_entity.ActionModelInteractiveEff -= ActionModelInteractiveEff;
            }
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

        [Obsolete("请使用 CreateModelAsync", false)]//标记该方法已弃用
        private void CreateModel()
        {
            if (m_entity != null)
            {
                GameObject gob = null;
                if (m_entity.modelDataCell != null)
                {
                    gob = Service.Resource.ResourceFormalManager.Instance.RecursionLoadGameObject(m_entity.modelDataCell.ModelsPath, E_AssetType.Roles);
                }
                else
                {
                    SGF.Debuger.LogWarning($"[ViewObjectNormal] Create PlayerEnityId=>{m_entity.EntityId},modelId={m_entity.ConfigIndex},type={m_entity.EntityType},modelDataCell=null");
                }
                if (gob == null)
                {
                    gob = Service.Resource.ResourceFormalManager.Instance.RecursionLoadGameObject("InteractObject/Cm/Interact_Temp", E_AssetType.Roles);
                }
                gob.transform.parent = m_ModelOffset;

                #region 防错添加组件

                Animator animator = gob.GetComponent<Animator>();
                if (animator == null)
                {
                    gob.AddComponent<Animator>();
                }
                // _Animancer = GameObjectUtils.EnsureComponent<AnimancerComponent>(gob);
                _Animancer = GameObjectUtils.EnsureComponent<AnimancerExtend>(gob);
                _Animancer.Animator = animator;
                (_Animancer as AnimancerExtend).ActionOnPlayWwise = OnActionPlayWwise;

                ModelOffLineData modelOffLineData = gob.GetComponent<ModelOffLineData>();
                if (modelOffLineData == null)
                {
                    M_ModelOffLineData = gob.AddComponent<ModelOffLineData>();
                    M_ModelOffLineData.BindDummyPos.Clear();
                    M_ModelOffLineData.BindDummyPos.Add("Root", gob.transform);
                }
                if (M_ModelOffLineData != null && M_ModelOffLineData.BindDummyPos.Count <= 0)
                {
                    modelOffLineData.BindDummyPos.Add("Root", gob.transform);
                }
                #endregion

                #region 动态修改材质的renderqueue值

                //int count = 0;
                //foreach (Transform child in gob.transform)
                //{
                //    Renderer childRenderer = child.GetComponent<Renderer>();
                //    if (childRenderer != null)
                //    {
                //        count++;
                //    }
                //}

                //Renderer[] rendererArr = new Renderer[count];
                //int index = 0;
                //foreach (Transform child in gob.transform)
                //{
                //    Renderer childRenderer = child.GetComponent<Renderer>();
                //    if (childRenderer != null)
                //    {
                //        rendererArr[index] = childRenderer;
                //        index++;
                //    }
                //}
                //DynamicRenderQueueManager.Instance.AddNewRender(rendererArr);

                List<Renderer> rendererList = new();
                foreach (Transform child in gob.transform)
                {
                    Renderer childRenderer = child.GetComponent<Renderer>();
                    if (childRenderer != null)
                    {
                        rendererList.Add(childRenderer);
                    }
                }
                //交互物件：石头水晶
                OutlineActiveCount = DynamicRenderQueueManager.Instance.AddNewRender(rendererList.ToArray(), E_OutlineEntityType.DynamicItem);

                #endregion
            }
        }

        private void CreateModelAsync(Action<bool> callback)
        {
            string tag = "";
            sb.Clear();
            if (m_entity.modelDataCell != null)
            {
                tag = sb.Append(m_entity.modelDataCell.ModelsPath).Replace(".prefab", string.Empty).Replace("Assets/Res/", string.Empty).ToString();
            }
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

            ModelRecord.Add(tag, gob);

            #region 动态修改材质的renderqueue值

            //int count = 0;
            //foreach (Transform child in gob.transform)
            //{
            //    Renderer childRenderer = child.GetComponent<Renderer>();
            //    if (childRenderer != null)
            //    {
            //        count++;
            //    }
            //}

            //Renderer[] rendererArr = new Renderer[count];
            //int index = 0;
            //foreach (Transform child in gob.transform)
            //{
            //    Renderer childRenderer = child.GetComponent<Renderer>();
            //    if (childRenderer != null)
            //    {
            //        rendererArr[index] = childRenderer;
            //        index++;
            //    }
            //}
            //DynamicRenderQueueManager.Instance.AddNewRender(rendererArr);

            List<Renderer> rendererList = new();
            foreach (Transform child in gob.transform)
            {
                Renderer childRenderer = child.GetComponent<Renderer>();
                if (childRenderer != null)
                {
                    rendererList.Add(childRenderer);
                }
            }
            //交互物件：石头水晶
            OutlineActiveCount = DynamicRenderQueueManager.Instance.AddNewRender(rendererList.ToArray(), E_OutlineEntityType.DynamicItem);

            #endregion

            SelfParticleSystem = gob.GetComponent<ParticleSystem>();

            gob.SetActive(true);
        }


        private void SetModelScale()
        {
            Vector3 scale = Vector3.one * m_entity.ModleScale;
            // 设置模型的缩放
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

        private float lastScale = 1;

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
            //ModelRotOffsetConf = Vector3.zero;
            /*if (m_entity != null && m_entity.avatarDataCell != null)
            {
                int rotOffset = m_entity.avatarDataCell.GetModelRotOffset();
                ModelRotOffsetConf.y = rotOffset;
            }*/
            //SetEntityAngle(ModelRotOffsetConf.y);
        }

        protected override void Reset()
        {
            base.Reset();

            ShowModel(true);

            // 物体 reset时,将 人身上的 特效 都要移除
            foreach (KeyValuePair<string, FxGeParam> item in gameEffctDic)
            {
                var fxGeParam = item.Value;
                fxGeParam.Reset();
            }
            gameEffctDic.Clear();

            // 删除身上的子节点
            foreach (KeyValuePair<string, Transform> item in gamePointDic)
            {
                NodePool.Put(item.Value, NodePool.NodePoolType.FxRoot);
            }
            gamePointDic.Clear();

            ModelPosOffsetConf = Vector3.zero;
            ModelRotOffsetConf = Vector3.zero;

            SetLerpAngelKill();
            LerpRotate1 = null;
            m_IsShowInteractiveEff = false;

            m_entity = null;
        }

        protected override void Release()
        {
            OffEventListener();

            Reset();

            base.Release();
        }

        #region 设置模型显示

        private int m_HideCount = 0;

        private void OnActionModelVisiable(bool isShow, bool isForce)
        {
            if (isShow)
            {
                ShowModel(isForce);
            }
            else
            {
                HideModel(isForce);
            }
        }

        private void HideModel(bool isForceShow = false)
        {
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
                OnActionVisible(false);
            }
        }

        private void ShowModel(bool isForceShow = false)
        {
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
                OnActionVisible(true);
            }
        }

        private void OnActionVisible(bool visible)
        {
            // 发送事件隐藏组件
            m_entity?.CompoentShowAction?.Invoke(!visible);

            float scale = visible ? 1 : 0;
            // SGF.Debuger.LogError($"[=x=] {transform.name} scale : {m_ModelOffset.transform.localScale} , try setScale {scale}");
            m_ModelOffset.transform.SetScaleXYZ(scale, scale, scale);

            if (SelfParticleSystem != null)
            {
                SelfParticleSystem.gameObject.SetActive(visible);
            }
            lastScale = scale;

            ControllFxShowHiden(visible);
        }

        #endregion

        #region 设置移动

        private void OnForceMove(Vector3 nextPoint)
        {
            m_EntityPosition = transform.localPosition = nextPoint;
            m_entity.SetCurrentPos(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);//过程【中】同步逻辑层
        }

        #endregion

        #region 旋转接口

        private float tmep_interpolation = 0;
        private float temp_timeSec = 0;

        /// <summary>
        /// 角度 改变的接口
        /// </summary>
        /// <param name="y"></param>
        /// <param name="neepLerp"></param>
        /// <param name="maxLerpTime">最大的lerp时间，单位 秒/s </param>
        /// <param name="useMaxLerpTime"> 之间 使用 maxLerpTime 做角度变化的插值</param>
        private void OnAngelChangeMove(float y, bool neepLerp, float maxLerpTime, bool useMaxLerpTime)
        {
            tmep_interpolation = y - m_ModelOffset.localEulerAngles.y;
            tmep_interpolation %= 360; //-360 ~ 360
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
            else if (Mathf.Abs(tmep_interpolation) <= 15)//15度直接设置
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

                    temp_timeSec = tmep_interpolation / GameConfig.PLAYER_ROTATE_SPEED;

                    // 直接使用 maxLerpTime 作为插值
                    if (useMaxLerpTime)
                    {
                        temp_timeSec = maxLerpTime;
                    }
                    else
                    {
                        // 如果 maxLerpTime 设置了 时间,并且 timeSec 超过了最大时间, 那么就采用 maxLerpTime ;
                        temp_timeSec = (maxLerpTime != 0 && temp_timeSec > maxLerpTime) ? maxLerpTime : temp_timeSec;
                    }

                    LerpRotate1 = DOTween.To(() => m_ModelOffset.localEulerAngles.y, changing =>
                    {
                        //float changeAngel = Vector3.Angle(defLook, destVec); //转换后：env向量与当前面向向量夹角;
                        //Vector3 normal = Vector3.Cross(defLook, destVec);//叉乘求出法线向量
                        //changeAngel *= Mathf.Sign(Vector3.Dot(normal, Vector3.up));
                        //tempVec.y = changeAngel;
                        SetEntityAngle(changing);
                    }, m_ModelOffset.localEulerAngles.y + tmep_interpolation, temp_timeSec).SetEase(Ease.Linear).SetAutoKill(false);
                    LerpRotate1.SetTarget(gameObject);

                    LerpRotate1.onComplete = () =>
                    {
                        LerpRotate1.Kill();
                    };
                    //m_ModelOffset.localEulerAngles = m_entity.EulerAngles;
                }
                else
                {
                    SetAngelNoLerp(tmep_interpolation);
                }
            }
        }

        private void SetLerpAngelKill()
        {
            if (LerpRotate1 != null && LerpRotate1.IsActive() && LerpRotate1.IsPlaying())
            {
                LerpRotate1.onComplete = null;
                LerpRotate1.Kill(false);
            }
        }

        private void SetAngelNoLerp(float interpolation)
        {
            if (LerpRotate1 != null && LerpRotate1.IsActive() && LerpRotate1.IsPlaying())
            {
                //策略1：立刻中断模式，只依据网络最新协议，抛弃过程，即刻更新，当前位置不完成
                //策略2：还原快照模式，依据缓存客户端加速执行过程，执行过程，如被中断立刻结束
                //计算取得最大推出时间，移动和转角
                //转角变成第二优先级【意识是如被中断立刻准确完成同步】
                LerpRotate1.Kill(/*false*/GameConfig.RotSyncMode);
            }

            SetEntityAngle(m_ModelOffset.localEulerAngles.y + interpolation);
        }

        private void SetEntityAngle(float angle)
        {
            m_EntityRotate.y = angle;
            m_ModelOffset.localEulerAngles = m_EntityRotate;
        }

        #endregion

        /// <summary>
        /// 交互提示特效回调
        /// </summary>
        /// <param name="isShow"></param>
        private void ActionModelInteractiveEff(bool isShow)
        {
            m_IsShowInteractiveEff = isShow;
            if (m_IsShowInteractiveEff)
            {
                if (go_interactiveEff == null)
                {
                    //加载特效
                    //添加贴图
                    //加载交互特效
                    ObjectCtrlGroup ocg = GameManager.Instance.GetEntityCtr(m_entity.EntityId) as ObjectCtrlGroup;
                    if (ocg != null && !string.IsNullOrEmpty(ocg.EffectAddress))
                    {
                        Action<GameObject> cb = (go) =>
                        {
                            if (go == null)
                            {
                                SGF.Debuger.LogWarning($"[ViewObjectNormal] ActionModelInteractiveEff() EffectAddress={ocg.EffectAddress},isShow={isShow},err!!!");
                                return;
                            }
                            go_interactiveEff = GameObject.Instantiate(go);
                            go_interactiveEff.transform.SetParent(transform);
                            go_interactiveEff.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                            go_interactiveEff.transform.localPosition = new Vector3(0, 0.3f, 0);
                            go_interactiveEff.SetActive(m_IsShowInteractiveEff);
                        };
                        Service.Resource.ResourceFormalManager.Instance.RecursionLoadGameObject(ocg.EffectAddress, E_AssetType.Effects, false, cb);

                        //go_interactiveEff = Service.Resource.ResourceFormalManager.Instance.RecursionLoadGameObject(ocg.EffectAddress, E_AssetType.Effects);
                        //if (go_interactiveEff != null)
                        //{
                        //    go_interactiveEff.transform.SetParent(transform);
                        //    go_interactiveEff.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                        //    go_interactiveEff.transform.localPosition = new Vector3(0, 0.3f, 0);
                        //    go_interactiveEff.SetActive(true);
                        //}
                        //else
                        //{
                        //    SGF.Debuger.LogError($"[ViewObjectNormal] ActionModelInteractiveEff() EffectAddress={ocg.EffectAddress},isShow={isShow},err!!!");
                        //}
                    }
                }
                else
                {
                    go_interactiveEff.SetActive(true);
                }
            }
            else
            {
                if (go_interactiveEff != null)
                {
                    go_interactiveEff.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 打开关闭模型菲涅尔
        /// </summary>
        /// <param name="isOpen"></param>
        private void OnActionModelFesnel(bool isOpen)
        {
            if (isOpen)
            {
                //Debug.LogError("打开菲涅尔");
                //m_ModelOffset.transform.gameObject.SetActive(true);

                Transform[] childrenList = m_ModelOffset.transform.GetComponentsInChildren<Transform>();
                foreach (Transform child in childrenList)
                {
                    ObjectCtrlGroup ocg = GameManager.Instance.GetEntityCtr(m_entity.EntityId) as ObjectCtrlGroup;
                    if (ocg.SpecialEffect == 1)
                    {
                        MeshRenderer mr = child.GetComponent<MeshRenderer>();
                        if (mr != null)
                        {
                            mr.material.SetFloat("_FesColorFlag", 1);
                        }
                    }
                    else if (ocg.SpecialEffect == 2)
                    {

                    }
                }
            }
            else
            {
                //Debug.LogError("关闭菲涅尔");
                //m_ModelOffset.transform.gameObject.SetActive(false);

                Transform[] childrenList = m_ModelOffset.transform.GetComponentsInChildren<Transform>();
                foreach (Transform child in childrenList)
                {
                    ObjectCtrlGroup ocg = GameManager.Instance.GetEntityCtr(m_entity.EntityId) as ObjectCtrlGroup;

                    if (ocg.SpecialEffect == 1)
                    {
                        MeshRenderer mr = child.GetComponent<MeshRenderer>();
                        if (mr != null)
                        {
                            mr.material.SetFloat("_FesColorFlag", 0);
                        }
                    }
                    else
                    {
                        //删除贴图
                    }
                }
            }
        }

        #region 特效接口

        /// <summary>
        /// 模型 半径 总的 缩放尺寸 = 缩放因子(BoxScaleRatio) * 配置缩放半径(BoxCfgScale)
        /// </summary>
        private Vector3 BoxScale => m_entity.GetBoxScale();

        private string GetGameEffectKey(int effectId, string key = "")
        {
            return $"{effectId}_{key}";
        }

        public void BasePlayEfxAsync(I_FxParam fxParam, string key, string effectPathPrefix)
        {
            string effectName = $"{effectPathPrefix}/{fxParam.EffectPath}";
            // 防报错
            effectName = effectName.Replace("Assets/Res/", string.Empty);
            effectName = effectName.Replace(".prefab", string.Empty);
            bool checkEffectNeedPlay = LocalFxManager.Instance.CheckEffectNeedPlay(effectName, fxParam.StartTime);
            if (!checkEffectNeedPlay)
            {
                // 开始时间已经大于特效最大时间了，不创建了
                return;
            }

            // 根据 配置的 IsAll 确定特效是否需要播放
            bool isShow = fxParam.IsAll ? true : fxParam.BuilderID == fxParam.OwnerID;

            UnityEngine.Vector3 pos = m_entity.Position();
            Vector3 rotate = m_entity.EulerAngles;
            // 特效是否，面向施法者
            if (fxParam.IsFaceToBuilder)
            {
                float y = Skill.Utils.SkillUtils.GetOrientationBuilderRotate(fxParam.BuilderID, m_entity.EntityId);
                rotate = new Vector3(rotate.x, y, rotate.z);
            }
            else
            {
                // 如果不是面向施法者, 那就用挂点的 角度, 此处就不需要传入额朝向
                rotate = Vector3.zero;
            }

            // 每次显示 特效前，都去清理一下特效的 root节点
            ReturnEffectRootNode();

            string fxMainKey = $"{fxParam.Key}_{key}";

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
            // 特效是否是 玩家的 骨骼之下(只要是 模型节点之下既可以 认为是在骨骼节点之下)
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
                    // 如果 跟随旋转 且跟随 移动,那就是 采用人物挂点,  不需要设置挂点父节点
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
                        SGF.Debuger.LogWarning($"[ViewAOI] BasePlayEfx  effectName : {effectName} , Key : {fxParam.Key} dont support FollowRotate when fx is FollowMove , error!!!  ");
                    }
                }
            }

            FxGeParam fxGeParam = new FxGeParam(rootTransform: rootScaleTransform, isUnderViewBone: isUnderViewBone, e_fxScaleType: fxParam.EFxScale, cfgScale: fxParam.ScaleXYZ).UpdateFxScale(fxBoxScale: BoxScale, true);
            fxGeParam.SetCloseAction(() =>
            {
                CloseFx(fxMainKey);
            });
            Vector3 scaleRatio = fxGeParam.CurScale;
            Vector3 fxRotate = rotate + fxParam.DirectionOffset;
            Vector3 moveOffset = fxParam.MoveOffset;

            Action<GameEffect> cb = (ge) =>
            {
                if (ge == null)
                {
                    SGF.Debuger.LogWarning($"[ViewAOI] BasePlayEfx  effectName : {effectName} , Key : {fxParam.Key} not find error!!!  ");
                    return;
                }

                // 将 ge 设置进入 fxGeParam 中
                fxGeParam.SetGameEffect(ge);
            };

            ResoruceReleaseType releaseType = ResoruceReleaseType.MapSceneAndServerID;

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
            //将播放的动画存储下来
            gameEffctDic[fxMainKey] = fxGeParam;
        }

        public void BaseStopFx(I_FxParam fxParam, string key = "")
        {
            string fxKey = $"{fxParam.Key}_{key}";
            CloseFx(fxKey);
        }

        private void CloseFx(string fxKey)
        {
            if (gameEffctDic.ContainsKey(fxKey))
            {
                var fxGeParam = gameEffctDic[fxKey];
                if (fxGeParam != null)
                {
                    fxGeParam.Reset();
                    gameEffctDic.Remove(fxKey);
                }
            }

            if (gamePointDic.ContainsKey(fxKey))
            {
                NodePool.Put(gamePointDic[fxKey], NodePool.NodePoolType.FxRoot);
                gamePointDic.Remove(fxKey);
            }
        }

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

        private void UpdateGeEffectScale()
        {
            foreach (KeyValuePair<string, FxGeParam> item in gameEffctDic)
            {
                var fxGeParam = item.Value;
                fxGeParam.UpdateFxScale(fxBoxScale: BoxScale, false);
            }
        }

        /// <summary>
        /// 控制特效的显隐
        /// </summary>
        /// <param name="isShow"></param>
        private void ControllFxShowHiden(bool isShow)
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

        #region 模型偏移

        /// <summary>
        /// 更新 模型偏移的坐标, 目前模型的 偏移坐标 = 模型自己设置的偏移坐标 modelOffset + 客户端模拟的偏移坐标 SimulateOffset
        /// </summary>
        private void UpdateModelOffsetPos()
        {
            m_ModelOffset.localPosition = ModelPosOffsetConf;
        }

        #endregion

    }


}
