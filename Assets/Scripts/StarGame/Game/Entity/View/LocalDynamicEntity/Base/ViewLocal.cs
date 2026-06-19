using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Service.Business;
using StarProject.Service.LocalDynamic;
using StarProject.Service.LocalDynamic.Fx;
using StarProjectDef;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StarProject.Game.Entity.View.LocalDynamic
{
    public abstract class ViewLocal : ViewModel
    {
        private string TagFlag = "[ViewLocal]";
        public static StringBuilder sb = new();

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

        private int groundLayer = -1;
        public int GroundLayer //这里是根据unity实际转化int值
        {
            get
            {
                if (groundLayer == -1)
                {
                    groundLayer = LayerMask.NameToLayer(E_LayerType.Ground.ToString());
                }
                return groundLayer;
            }
        }

        protected Vector3 m_EntityRotate = Vector3.zero;

        protected List<SkinnedMeshRenderer> m_SkinMeshRender = new();

        private Vector3 ModelPosOffsetConf = Vector3.zero;  // 模型配置的偏移
        protected Vector3 ModelRotOffsetConf = Vector3.zero;

        private EntityObject entityObject;


        protected override void Create(EntityObject entity)
        {
            base.Create(entity);

            entityObject = entity;
            if (entity == null)
            {
                return;
            }

            m_ModelOffset = transform.Find("ModelOffset");
            CreateModelNoRotationRoot();
            OnEventListener();
            InitAvatarModelPosOffset();
            InitAvatarModelRotOffset();

            Action<bool> modelFinish = (res) =>
            {
                if (!res)
                {
                    OnModleViewCreateFinish(false);
                    return;
                }
                SetModelScale();
                //SetEntityAngle(m_entity.GetEulerAngles());
                SetBornPosition();
                InitSkinMeshRender();
                OnModleViewCreateFinish(true);
            };
            CreateModelAsync(modelFinish);
        }

        /// <summary>
        /// 抽象的 modleView 创建结束的 接口
        /// </summary>
        abstract protected void OnModleViewCreateFinish(bool result);
        abstract protected void OnEventListener();
        abstract protected void OffEventListener();
        abstract protected void SetEntityPosition(Vector3 pos);

        abstract protected AvatarDataCell AvatarData { get; }


        protected override void Reset()
        {
            base.Reset();
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

            m_SkinMeshRender.Clear();

            ModelPosOffsetConf = Vector3.zero;
            ModelRotOffsetConf = Vector3.zero;

            entityObject = null;
        }

        protected override void Release()
        {
            OffEventListener();

            Reset();

            base.Release();
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

        private void CreateModelAsync(Action<bool> callback)
        {
            string tag = "";
            sb.Clear();
            if (ModelPath != "")
            {
                tag = sb.Append(ModelPath).Replace(".prefab", string.Empty).Replace("Assets/Res/", string.Empty).ToString();
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
                    cb = null;
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
                SGF.Debuger.LogWarning($"{TagFlag} LoadModelCallBack() An item with the same key has already been added. Key:tag={tag},gob={gob},err!!!");
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
            //交互物件：石头水晶
            OutlineActiveCount = DynamicRenderQueueManager.Instance.AddNewRender(rendererList.ToArray(), E_OutlineEntityType.DynamicItem);

            #endregion

            SelfParticleSystem = gob.GetComponent<ParticleSystem>();

            gob.SetActive(true);
        }

        private void SetModelScale()
        {
            Vector3 scale = Vector3.one * ModleScale;
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

        protected virtual void SetBornPosition()
        {
            //SGF.Debuger.LogWarning($"设置宝箱坐标 ----------id={m_entity.EntityKey}---- 走这里就不对了");
            Vector3 pos = GetBirthPos();

            SetPosition(pos);
        }

        protected void SetPosition(Vector3 pos)
        {
            transform.localPosition = pos;
            SetEntityPosition(pos);
        }

        protected virtual Vector3 GetBirthPos()
        {
            Vector3 pos = entityObject.Position();
            float y = BusinessManager.Instance.GetGroundHeight(pos.x, pos.z);
            if (y > 0)
            {
                pos.y = y;
            }

            //RaycastHit hit;
            //bool sd = Physics.Raycast(pos + Vector3.up, Vector3.down, out hit, 10.0f, GroundLayer);
            ////bool sd = Physics.Raycast(birthPos + (Vector3.up * 200), Vector3.down, out hit, 250f, LayerMask.GetMask("Ground"));
            //if (sd)
            //{
            //    pos.y = hit.point.y + 0.08f;
            //}
            return pos;
        }

        protected void SetEntityAngle(float angle)
        {
            m_EntityRotate.y = angle;
            m_ModelOffset.localEulerAngles = m_EntityRotate;
        }

        private void InitSkinMeshRender()
        {
            m_SkinMeshRender.Clear();
            m_SkinMeshRender = GetComponentsInChildren<SkinnedMeshRenderer>().KToList<SkinnedMeshRenderer>();
        }

        private void InitAvatarModelPosOffset()
        {
            ModelPosOffsetConf = Vector3.zero;

            if (AvatarData != null)
            {
                var posOffset = AvatarData.ModelPosOffset;
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
            if (AvatarData != null)
            {
                int rotOffset = AvatarData.GetModelRotOffset();
                ModelRotOffsetConf.y = rotOffset;
            }
            SetEntityAngle(ModelRotOffsetConf.y);
        }

        #region 显隐和模型特效

        private int m_HideCount = 0;

        protected void OnActionOnHidden(bool hide)
        {
            if (hide)
            {
                HideModel();
            }
            else
            {
                ShowModel();
            }
        }

        protected void HideModel(bool isForceShow = false)
        {
            if (entityObject == null)
            {
                return;
            }

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
        }

        private void ShowModel(bool isForceShow = false)
        {
            if (entityObject == null)
            {
                return;
            }

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
                ControllShowHide(true);
            }
        }

        private void ControllShowHide(bool isShow)
        {
            // 本地实体，特效和骨骼是混合用的，只能设置显隐了
            M_ModelOffLineData.gameObject.SetActive(isShow);

            ControllFxShowHiden(isShow);
        }

        #endregion

        #region 播放特效

        /// <summary>
        /// 模型 半径 总的 缩放尺寸 = 缩放因子(BoxScaleRatio) * 配置缩放半径(BoxCfgScale)
        /// </summary>
        private Vector3 BoxScale => Vector3.one;

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

            UnityEngine.Vector3 pos = entityObject.Position();
            Vector3 rotate = m_ModelOffset.localEulerAngles;
            // 特效是否，面向施法者
            if (fxParam.IsFaceToBuilder)
            {
                //float y = Skill.Utils.SkillUtils.GetOrientationBuilderRotate(fxParam.BuilderID, m_entity.EntityId);
                //rotate = new Vector3(rotate.x, y, rotate.z);
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
                  entityType: E_EntityType.Interact,
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

        #region 始终面向一个目标
        private ulong lookAtTarget = 0;
        // 转角速度
        private float angleSpeed = 0.01f;

        private float lookAtTargetTotalTime = 0;

        private float curLookAtTargetTime = 0;
        public void OnActionTurn2Target(ulong targetEntityID, float trunLerpTime, float totalTime)
        {
            // m_ModelOffset.LookAt()

            NPCEntityBase targetEntity = GameManager.Instance.GetEntityByEntityID(targetEntityID);
            if (targetEntity == null)
            {
                return;
            }

            lookAtTarget = targetEntityID;

            // 最大转交速度
            angleSpeed = trunLerpTime > 0 ? 180 / trunLerpTime : 180;

            lookAtTargetTotalTime = totalTime;

            curLookAtTargetTime = 0;
        }

        private void LookAtTarget()
        {
            if (lookAtTarget == 0)
            {
                return;
            }

            NPCEntityBase targetEntity = GameManager.Instance.GetEntityByEntityID(lookAtTarget);
            if (targetEntity == null)
            {
                return;
            }
            curLookAtTargetTime += Time.deltaTime;



            Vector3 vec = targetEntity.Position() - m_ModelOffset.position;
            Quaternion rotate = Quaternion.LookRotation(vec);

            float angle = Vector3.Angle(m_ModelOffset.forward, vec);


            m_ModelOffset.rotation = Quaternion.Lerp(m_ModelOffset.localRotation, rotate, angleSpeed * Time.deltaTime);


            if (curLookAtTargetTime >= lookAtTargetTotalTime)
            {
                lookAtTarget = 0;
                curLookAtTargetTime = 0;
                lookAtTargetTotalTime = 0;
            }

            // m_ModelOffset.LookAt(targetEntity.Position());
        }


        /// <summary>
        /// 跟随的目标的朝向 id
        /// </summary>
        private ulong lookTargetForwardID = 0;
        /// <summary>
        /// 看向某个目标的 朝向. 永远跟随目标朝向
        /// </summary>
        /// <param name="targetID"></param>
        protected void LookAtTargetForward(ulong targetID)
        {
            lookTargetForwardID = targetID;
        }

        private void LookAtTargetIDForward()
        {
            // 如果已经有 看向的目标, 那就不需要跟 lookTargetForwardID 保持朝向的一致
            if (lookAtTarget != 0)
            {
                return;
            }

            if (lookTargetForwardID == 0)
            {
                return;
            }

            NPCEntityBase targetEntity = GameManager.Instance.GetEntityByEntityID(lookTargetForwardID);
            if (targetEntity == null)
            {
                return;
            }

            m_ModelOffset.eulerAngles = targetEntity.EulerAngles;
        }

        #endregion

        public virtual void Update()
        {
            LookAtTarget();

            LookAtTargetIDForward();
        }
    }
}
