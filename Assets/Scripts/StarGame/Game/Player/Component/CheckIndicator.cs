using SGF.Utlis;
using UnityEngine;
namespace StarProject.Game.Player.Component
{
    /// <summary>
    /// 选中指示器
    /// </summary>
    public class CheckIndicator : PlayerComponent
    {
        //private string Log_Tag = "[CheckIndicator]";
        private const string m_modelPath = "UI/StarWorld/Prefab/CheckIndicatorView";

        private EntityCtrlBase m_entityCtrl;
        private GameContext m_context;

        private CheckIndicatorView m_checkIndicatorView;
        private bool m_isCheckTarget = false;
        private bool m_isLoadView = false;

        public CheckIndicator(EntityCtrlBase entityCtrl) : base(entityCtrl)
        {
            m_entityCtrl = entityCtrl;
            m_context = GameManager.Instance.Context;
        }

        public override void EnterFrame(int frameIndex)
        {
            //if (m_checkIndicatorView != null)
            //{
            //    m_checkIndicatorView.EnterFrame(frameIndex);
            //}
        }

        private void Reset()
        {
            if (m_checkIndicatorView != null)
            {
                m_checkIndicatorView.Release();
            }
            m_checkIndicatorView = null;
            m_isLoadView = false;
            m_isCheckTarget = false;
            m_entityCtrl = null;
        }

        public override void Release()
        {
            Reset();
        }
        public override MonoBehaviour GetView()
        {
            if (m_checkIndicatorView != null)
            {
                return m_checkIndicatorView;
            }
            return null;
        }

        public override void SetFlashHide(bool isHide)
        {
            if (m_checkIndicatorView != null)
            {
                m_checkIndicatorView.SetFlashHide(isHide);
            }
        }

        public override void InitRefreshState()
        {
            SetCheckIndicatorStatus();
        }


        /// <summary>
        /// 选中指示器回调
        /// </summary>
        /// <param name="isShow">是否显示</param>
        public void OnActionOnCheckTarget(bool isShow)
        {
            //SGF.Debuger.LogError($"目标选中 entityid={m_entityCtrl.Data.M_EntityID},isShow={isShow}");
            m_isCheckTarget = isShow;
            SetCheckIndicatorStatus();
        }

        /// <summary>
        /// 设置选择指示器状态
        /// </summary>
        public void SetCheckIndicatorStatus()
        {
            // NPC先判断是不是空模型
            // 如果是就不显示
            if (m_entityCtrl.M_Curr.modelDataCell != null && m_entityCtrl.M_Curr.EntityType == StarProjectDef.E_EntityType.Npc && m_entityCtrl.M_Curr.modelDataCell.GetModleID() == 1)
            {
                return;
            }
            if (m_checkIndicatorView != null)
            {
                m_checkIndicatorView.SetCheckActive(m_isCheckTarget);
            }
            else if (m_isCheckTarget)
            {
                if (m_entityCtrl.Container != null && !m_isLoadView)
                {
                    Transform m_container = UnityExtension.FindFunc(m_entityCtrl.Container.transform, "ModelOffset");
                    if (m_container != null)
                    {
                        //GameObject go = Service.Resource.ResourceManager.Instance.LoadGameObject(m_modelPath);
                        ////GameObject go = ResourceManager.LoadPrefab("CheckIndicator/CheckIndicatorView");
                        //if (go != null)
                        //{
                        //    m_checkIndicatorView = go.GetComponent<CheckIndicatorView>();
                        //    if (m_checkIndicatorView != null)
                        //    {
                        //        //CharacterController m_characterController = m_entityCtrl.Container.transform.GetChildren()[0].GetComponent<CharacterController>();
                        //        m_checkIndicatorView.transform.SetParent(m_container, false);
                        //        m_checkIndicatorView.Init(m_entityCtrl);
                        //    }
                        //}
                        //SGF.Debuger.LogError($"目标选中 entityid={m_entityCtrl.Data.M_EntityID},isShow={m_isCheckTarget}");
                        m_isLoadView = true;
                        StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(m_modelPath,
                        (GameObject go) =>
                        {
                            if (go == null)
                            {
                                return;
                            }
                            if (m_entityCtrl != null && m_entityCtrl.Data != null)
                            {
                                var gob = GameObject.Instantiate<GameObject>(go);
                                if (gob != null)
                                {
                                    InitCheckIndicatorView(gob, m_container);
                                }
                            }
                        });
                    }
                }
            }
        }

        private void InitCheckIndicatorView(GameObject gob, Transform m_container)
        {
            m_checkIndicatorView = gob.GetComponent<CheckIndicatorView>();
            if (m_checkIndicatorView != null)
            {
                //CharacterController m_characterController = m_entityCtrl.Container.transform.GetChildren()[0].GetComponent<CharacterController>();
                m_checkIndicatorView.transform.SetParent(m_container, false);
                m_checkIndicatorView.Init(m_entityCtrl);
            }
            SetFlashHide(m_entityCtrl.IsHid);
            OnActionOnCheckTarget(m_isCheckTarget);
        }

    }
}
