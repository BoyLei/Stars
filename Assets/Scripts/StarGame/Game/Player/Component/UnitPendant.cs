using SGF.UI.Framework;
using StarProject.OffLine;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Game.Player.Component
{
    /// <summary>
    /// 角色头顶信息面板：还需要头顶信息和2d，3d位置对齐管理
    /// TxeMeshPro-中文图集
    /// 1，等级
    /// 2，名字，名字颜色
    /// 3，等等状态
    /// 包含血条，
    /// </summary>
    public class UnitPendant : PlayerComponent
    {
        private string Log_Tag = "[UnitPendant]";
        private const string m_modelPath = "UI/StarWorld/Prefab/UnitPendantViewNew";

        private EntityCtrlBase m_entityCtrl;
        private GameContext m_context;

        private UnitPendantViewNew m_unitPendantView;
        private bool m_isCheckTarget = false;

        public UnitPendant(EntityCtrlBase entityCtrl) : base(entityCtrl)
        {
            m_entityCtrl = entityCtrl;
            m_context = GameManager.Instance.Context;

            Create();
        }

        private void Create()
        {
            if (m_entityCtrl == null || m_entityCtrl.Data == null)
            {
                return;
            }

            Log_Tag = $"[UnitPendant_{m_entityCtrl.Data.EntityType}_{m_entityCtrl.Data.M_EntityID}]";

            // TODO：曲  注册消息通知
            // 部分玩法/地图消息
            // 目标姓名需要根据目标类型和目标状态等进行不同的颜色区分显示

            if (m_entityCtrl.EntityType == E_EntityType.BulletEntity)
            {
                return;
            }

            //// 创建UnitPendantView预制体
            //GameObject go = Service.Resource.ResourceManager.Instance.LoadGameObject(m_modelPath);
            //if (go != null)
            //{
            //    m_unitPendantView = go.GetComponent<UnitPendantView>();
            //    m_unitPendantView.Init(m_entityCtrl);
            //}

            StarProject.Service.Resource.ResourceFormalManager.Instance.PopGameObject(m_modelPath,
            (go) =>
            {
                if (go == null)
                {
                    return;
                }
                if (m_entityCtrl != null && m_entityCtrl.Data != null)
                {
                    InitUnitPendantViewNew(go);
                }
                else
                {
                    StarProject.Service.Resource.ResourceFormalManager.Instance.PushGameObject(m_modelPath, go);
                }
            });

            RegisterAttribute();
        }

        //private void InitUnitPendantView(GameObject gob)
        //{
        //    m_unitPendantView = gob.GetComponent<UnitPendantView>();
        //    m_unitPendantView.Init(m_entityCtrl);

        //    bool isFindParent = false;
        //    ModelOffLineData modelOffLineData = m_entityCtrl.Container.GetComponentInChildren<ModelOffLineData>();
        //    if (modelOffLineData != null)
        //    {
        //        Transform pointGod = modelOffLineData.GetTransformByKey(HangPoint.UnitInfo_D.ToString());
        //        if (pointGod != null)
        //        {
        //            m_unitPendantView.transform.SetParent(pointGod, false);
        //            isFindParent = true;
        //        }
        //        else
        //        {
        //            SGF.Debuger.LogWarning($"{Log_Tag} Create HangPoint.UnitInfo_D=null,,not find HangPoint.UnitInfo_D error!!!");
        //        }
        //    }
        //    else
        //    {
        //        SGF.Debuger.LogWarning($"{Log_Tag} Create modelOffLineData=null,,not find ModelOffLineData error!!!");
        //    }
        //    if (!isFindParent)
        //    {
        //        // 下面是迫不得已给给父节点
        //        Transform m_container = UnityExtension.FindFunc(m_entityCtrl.Container.transform, "ModelOffset");
        //        if (m_container != null)
        //        {
        //            m_unitPendantView.transform.SetParent(m_container, false);
        //            m_unitPendantView.transform.localPosition = new Vector3(0, 2, 0);
        //        }
        //        else
        //        {
        //            m_unitPendantView.transform.SetParent(m_entityCtrl.Container.transform, false);
        //        }
        //    }
        //    SetFlashHide(m_entityCtrl.IsHid);
        //    m_unitPendantView?.SetCheckActive(m_isCheckTarget);
        //}

        private void InitUnitPendantViewNew(GameObject gob)
        {
            gob.GetComponent<CanvasGroup>().alpha = 1;
            float height = GetModelHeight();
            m_unitPendantView = gob.GetComponent<UnitPendantViewNew>();
            m_unitPendantView.transform.SetParent(DynamicUIRoot.EntityUIRoot.transform, false);
            //if (m_entityCtrl != null && m_entityCtrl.M_Curr != null && m_entityCtrl.M_Curr.ModleScale > 1)
            //{
            //    height *= (4 * m_entityCtrl.M_Curr.ModleScale);
            //}
            m_unitPendantView.SetY(height);
            m_unitPendantView.Init(m_entityCtrl);
            SetFlashHide(m_entityCtrl.IsHid);
            m_unitPendantView?.SetCheckActive(m_isCheckTarget);
        }

        public override void EnterFrame(int frameIndex)
        {
            if (m_unitPendantView != null)
            {
                m_unitPendantView.EnterFrame(frameIndex);
            }
        }

        private void Reset()
        {
            m_isCheckTarget = false;

            if (m_entityCtrl != null)
            {
                UnRegisterAttribute();
            }
            m_entityCtrl = null;
        }

        public override void Release()
        {
            if (m_unitPendantView != null)
            {
                m_unitPendantView.Release();
                StarProject.Service.Resource.ResourceFormalManager.Instance.PushGameObject(m_modelPath, m_unitPendantView.gameObject, GameObjectPoolType.CanvasGroupType);
            }
            m_unitPendantView = null;
            Reset();
        }

        public override MonoBehaviour GetView()
        {
            if (m_unitPendantView != null)
            {
                return m_unitPendantView;
            }
            return null;
        }

        public override void SetFlashHide(bool isHide)
        {
            if (m_unitPendantView != null)
            {
                m_unitPendantView.SetFlashHide(isHide);
            }
        }

        public override void InitRefreshState()
        {
            if (m_unitPendantView != null)
            {
                float height = GetModelHeight();
                m_unitPendantView.SetY(height, true);
                if (m_entityCtrl != null)
                {
                    SetFlashHide(m_entityCtrl.IsHid);
                }
                m_unitPendantView?.SetCheckActive(m_isCheckTarget);
            }
        }

        private float GetModelHeight()
        {
            float height = 2f;
            // 挂点位置
            ModelOffLineData modelOffLineData = m_entityCtrl.Container.GetComponentInChildren<ModelOffLineData>();
            if (modelOffLineData != null)
            {
                //Transform UnitInfoPoint = modelOffLineData.GetTransformByKey(HangPoint.UnitInfo_D.ToString());
                //if (UnitInfoPoint != null)
                //{
                //    Vector3 v3 = WorldItemChecker.Instance.PlayerAndNpcPos(UnitInfoPoint, E_AnchorPresets.Center, UiOr3D.D3, PivotPos.Center);
                //    height = v3.y;
                //    Transform RootPoint = modelOffLineData.GetTransformByKey(HangPoint.Root.ToString());
                //    if (RootPoint != null)
                //    {
                //        Vector3 v3root = WorldItemChecker.Instance.PlayerAndNpcPos(RootPoint, E_AnchorPresets.Center, UiOr3D.D3, PivotPos.Center);
                //        height -= v3root.y;
                //    }
                //}
                height = modelOffLineData.GetModelHeight();
            }
            if (height < 0.2f)
            {
                height = 2f;
            }
            return height;
        }

        // 通知显示层更新
        #region 设置文本的显示状态

        public void OnActionMainPlayerHurt()
        {
            SetHealthValShow(true);
        }

        public void OnActionSetNameAndCor(string str, int headNameColorID)
        {
            m_unitPendantView?.SetNameTextColor(str, headNameColorID);
        }

        public void OnActionSetNameOrgCor()
        {
            m_unitPendantView?.SetNameOrgColor();
        }

        public void SetTitleShow(bool isShow)
        {
            m_unitPendantView?.SetTitleShow(isShow);
        }
        public void SetLaborUnionShow(bool isShow)
        {
            m_unitPendantView?.SetLaborUnionShow(isShow);
        }
        public void SetHealthValShow(bool isShow)
        {
            m_unitPendantView?.SetHealthValShow(isShow, true);
        }
        public void SetNameShow(bool isShow)
        {
            m_unitPendantView?.SetNameShow(isShow);
        }

        /// <summary>
        /// 选中指示器回调
        /// </summary>
        /// <param name="isShow">是否显示</param>
        public void OnActionOnCheckTarget(bool isShow)
        {
            m_isCheckTarget = isShow;
            m_unitPendantView?.SetCheckActive(isShow);
        }

        /// <summary>
        /// 删除显示层
        /// </summary>
        public void OnActionOnViewDel()
        {
            //if (m_unitPendantView != null)
            //{
            //    Transform m_container = UnityExtension.FindFunc(m_entityCtrl.Container.transform, "ModelOffset");
            //    if (m_container != null)
            //    {
            //        m_unitPendantView.transform.SetParent(m_container, false);
            //    }
            //    else
            //    {
            //        m_unitPendantView.transform.SetParent(m_entityCtrl.Container.transform, false);
            //    }
            //}
        }

        /// <summary>
        /// 切换父节点
        /// </summary>
        public void OnActionOnViewCreateFinifh()
        {
            //if (m_unitPendantView == null || m_unitPendantView.transform == null)
            //{
            //    return;
            //}
            //// 重新查找父节点
            //// 创建UnitPendantView预制体
            //ModelOffLineData modelOffLineData = m_entityCtrl.Container.GetComponentInChildren<ModelOffLineData>();
            //if (modelOffLineData != null)
            //{
            //    Transform pointGod = modelOffLineData.GetTransformByKey(HangPoint.UnitInfo_D.ToString());
            //    if (pointGod != null)
            //    {
            //        m_unitPendantView.transform.SetParent(pointGod, false);
            //        m_unitPendantView.InitTransformData();
            //    }
            //    else
            //    {
            //        SGF.Debuger.LogError($"{Log_Tag} Create HangPoint.UnitInfo_D=null,,not find HangPoint.UnitInfo_D error!!!");
            //    }
            //}
            //else
            //{
            //    SGF.Debuger.LogError($"{Log_Tag} Create modelOffLineData=null,,not find ModelOffLineData error!!!");
            //}
        }

        public void SetMvpAnimShow()
        {
            m_unitPendantView?.SetMvpAnim();
        }

        #endregion

        #region 属性变更监听

        private void RegisterAttribute()
        {
            m_entityCtrl.Data.RegisterAttribute(AOIAttrDefine.curHp, OnAOICurHpChange);
            m_entityCtrl.Data.RegisterAttribute(AOIAttrDefine.TruthHp, OnAOITruthHpChange);
            m_entityCtrl.Data.RegisterAttribute(AOIAttrDefine.State, OnAOIStateChange);
            string attrLevelName = m_entityCtrl.M_Curr.EntityType == E_EntityType.Player ? AOIAttrDefine.PlayerLevel : AOIAttrDefine.EntityLevel;
            m_entityCtrl.Data.RegisterAttribute(attrLevelName, OnAOILevelChange);
            m_entityCtrl.Data.RegisterAttribute(AOIAttrDefine.PVPState, OnAOIPvpStateChange);
        }

        private void UnRegisterAttribute()
        {
            m_entityCtrl.Data.UnRegisterAttribute(AOIAttrDefine.curHp, OnAOICurHpChange);
            m_entityCtrl.Data.UnRegisterAttribute(AOIAttrDefine.TruthHp, OnAOITruthHpChange);
            m_entityCtrl.Data.UnRegisterAttribute(AOIAttrDefine.State, OnAOIStateChange);
            string attrLevelName = m_entityCtrl.M_Curr.EntityType == E_EntityType.Player ? AOIAttrDefine.PlayerLevel : AOIAttrDefine.EntityLevel;
            m_entityCtrl.Data.UnRegisterAttribute(attrLevelName, OnAOILevelChange);
            m_entityCtrl.Data.UnRegisterAttribute(attrLevelName, OnAOIPvpStateChange);
        }
        #region 监听属性改变的回调
        /// <summary> AOI [当前血量] 变化 </summary>
        private void OnAOICurHpChange(string key, object val)
        {
            long value = m_entityCtrl.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, key);
            UpdateHealth(value);
        }
        /// <summary> AOI [实际血量] 变化 </summary>
        private void OnAOITruthHpChange(string key, object val)
        {
            long value = m_entityCtrl.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, key);
            UpdateTruthHp(value);
        }
        /// <summary> AOI [状态] 变化 </summary>
        private void OnAOIStateChange(string key, object val)
        {
            uint value = m_entityCtrl.Data.Attrs.GetAoiValue<uint>(EnumAOIType.Uint, key);
            UpdateState(value);
        }
        /// <summary> AOI [状态] 变化 </summary>
        private void OnAOILevelChange(string key, object val)
        {
            int value = m_entityCtrl.Data.Attrs.GetAoiValue<int>(EnumAOIType.Int, key);
            UpdateLevel(value);
        }
        /// <summary> AOI [状态] 变化 </summary>
        private void OnAOIPvpStateChange(string key, object val)
        {
            int value = m_entityCtrl.Data.Attrs.GetAoiValue<int>(EnumAOIType.Int, key);
            UpdatePvpState(value);
        }
        #endregion

        #endregion

        #region 更新属性信息
        /// <summary>
        /// 更新血量
        /// </summary>
        /// <param name="curHp">当前血量</param>
        public void UpdateHealth(long curHp)
        {
            if (m_unitPendantView != null)
            {
                m_unitPendantView.UpdateHealth(curHp);
            }
        }
        public void UpdateTruthHp(long hp)
        {
            if (m_unitPendantView != null)
            {
                m_unitPendantView.UpdateTruthHp(hp);
            }
        }

        public void UpdateState(uint value)
        {
            if (m_unitPendantView != null)
            {
                m_unitPendantView.UpdateState(value);
            }
        }

        public void UpdateLevel(int level)
        {
            if (m_unitPendantView != null)
            {
                m_unitPendantView.UpdateLevel(level);
            }
        }
        public void UpdatePvpState(int pvpstate)
        {
            if (m_unitPendantView != null)
            {
                m_unitPendantView.UpdatePvpIcon(pvpstate);
            }
        }
        #endregion
    }
}
