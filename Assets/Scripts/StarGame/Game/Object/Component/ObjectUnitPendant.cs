using SGF.UI.Framework;
using SkillEditor;
using StarProject.Game.Entity;
using StarProject.OffLine;
using StarProject.Service.WorldToUI;
using StarProjectDef;
///--------------------------------------------------------------------
/// 文件名   :   ObjectUnitPendant
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   #CREATETIME#
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using UnityEngine;
using static StarProject.Service.WorldToUI.WorldItemChecker;

namespace StarProject.Game.Player.Component
{
    public class ObjectUnitPendant : PlayerComponent
    {
        private string Log_Tag = "[ObjectUnitPendant]";
        private const string m_modelPath = "UI/StarWorld/Prefab/ObjectUnitPendantViewNew";

        private ObjectCtrlGroup m_object;//控制群组
        private EntityLocalDynamic m_entityLocalDynamic;
        private GameContext m_context;

        private ObjectUnitPendantViewNew m_unitPendantView;
        private bool m_isCheckTarget = false;

        public ObjectUnitPendant(ObjectCtrlGroup entity) : base(entity)
        {
            if (entity == null)
            {
                return;
            }
            m_object = entity;
            m_entityLocalDynamic = null;
            m_context = GameManager.Instance.Context;
            CreateAoi();
        }

        public ObjectUnitPendant(EntityLocalDynamic entity) : base(entity)
        {
            if (entity == null)
            {
                return;
            }
            m_entityLocalDynamic = entity;
            m_object = null;
            m_context = GameManager.Instance.Context;
            CreateLocal();
        }

        private void CreateAoi()
        {
            if (m_object == null || m_object.NoneVitalData == null)
            {
                return;
            }
            Log_Tag = $"[ObjectUnitPendant_{m_object.NoneVitalData.EntityType}_{m_object.NoneVitalData.M_EntityID}]";
            StarProject.Service.Resource.ResourceFormalManager.Instance.PopGameObject(m_modelPath,
            (go) =>
            {
                if (go == null)
                {
                    return;
                }
                if (m_object != null && m_object.NoneVitalData != null)
                {
                    InitUnitPendantViewNew(go, true);
                }
                else
                {
                    StarProject.Service.Resource.ResourceFormalManager.Instance.PushGameObject(m_modelPath, go);
                }
            });
        }

        private void CreateLocal()
        {
            if (m_entityLocalDynamic == null)
            {
                return;
            }
            Log_Tag = $"[ObjectUnitPendant_{m_entityLocalDynamic.Type}_{m_entityLocalDynamic.EntityKey}]";
            StarProject.Service.Resource.ResourceFormalManager.Instance.PopGameObject(m_modelPath,
            (go) =>
            {
                if (go == null)
                {
                    return;
                }
                if (m_entityLocalDynamic != null)
                {
                    InitUnitPendantViewNew(go, false);
                }
                else
                {
                    StarProject.Service.Resource.ResourceFormalManager.Instance.PushGameObject(m_modelPath, go);
                }
            });
        }

        private void InitUnitPendantViewNew(GameObject gob, bool isAoiEntity)
        {
            gob.GetComponent<CanvasGroup>().alpha = 1;
            float height = GetModelHeight();
            m_unitPendantView = gob.GetComponent<ObjectUnitPendantViewNew>();
            m_unitPendantView.transform.SetParent(DynamicUIRoot.EntityUIRoot.transform, false);
            m_unitPendantView.SetY(height);
            m_unitPendantView?.SetArrowShow(m_isCheckTarget);

            if (isAoiEntity)
            {
                gob.name = m_object.NoneVitalData.M_EntityID.ToString();
                m_unitPendantView.InitAoiObject(m_object);
                SetFlashHide(!m_object.IsShow);
            }
            else
            {
                m_unitPendantView.gameObject.name = m_entityLocalDynamic.EntityKey;
                m_unitPendantView.InitLocalObject(m_entityLocalDynamic);
            }
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

            m_object = null;
            m_entityLocalDynamic = null;
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
                //if (m_entityLocalDynamic != null)
                //{
                //    SGF.Debuger.LogWarning($"通缉实体 key={m_entityLocalDynamic.EntityKey},IsHide={isHide}");
                //}
                m_unitPendantView.SetFlashHide(isHide);

                //m_unitPendantView.gameObject.SetActive(!isHide);
            }
        }

        public override void InitRefreshState()
        {
            // 这个组件在异步后加载
            //if (m_object != null)
            //{
            //    SetFlashHide(m_object.IsHid);
            //}
            if (m_unitPendantView == null)
            {
                return;
            }
            float height = GetModelHeight();
            m_unitPendantView.SetY(height);
            m_unitPendantView.SetArrowShow(m_isCheckTarget);
            if (m_object != null)
            {
                SetFlashHide(!m_object.IsShow);
            }
            if (m_entityLocalDynamic != null)
            {
                //SGF.Debuger.LogWarning($"通缉实体 key={m_entityLocalDynamic.EntityKey},IsHide={m_entityLocalDynamic.IsHide}");
                bool isHide = m_entityLocalDynamic.IsHide;
                //if (m_entityLocalDynamic.Type == E_LocalEntityType.WantedEnity)
                //{
                //    isHide = !m_entityLocalDynamic.IsHide;
                //}
                SetFlashHide(isHide);
            }
        }

        private float GetModelHeight()
        {
            GameObject container = null;
            if (m_object != null)
            {
                container = m_object.Container;
            }
            else if (m_entityLocalDynamic != null)
            {
                container = m_entityLocalDynamic.Container;
            }
            float height = 2f;
            // 挂点位置
            ModelOffLineData modelOffLineData = container.GetComponentInChildren<ModelOffLineData>();
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
    }
}

