using SGF.Utlis;
using StarProject.Game.Skill;
using StarProject.UI.SkillIndicator;
using UnityEngine;

namespace StarProject.Game.Player.Component
{
    /// <summary>
    /// 技能指示器
    /// </summary>
    public class SkillIndicator : PlayerComponent
    {
        private string Log_Tag = "[SkillIndicator]";
        private const string m_modelPath = "UI/StarWorld/Prefab/SkillIndicator/SkillIndicatorView";

        private EntityCtrlBase m_entityCtrl;
        private GameContext m_context;

        private SkillIndicatorView m_skillIndicatorView;

        public SkillIndicator(EntityCtrlBase entityCtrl) : base(entityCtrl)
        {
            m_entityCtrl = entityCtrl;
            m_context = GameManager.Instance.Context;

            Create();
        }

        private void Create()
        {
            if (m_entityCtrl == null && m_entityCtrl.Data != null)
            {
                return;
            }
            if (m_entityCtrl != null && m_entityCtrl.Data != null)
            {
                Log_Tag = $"[SkillIndicator_{m_entityCtrl.Data.EntityType}_{m_entityCtrl.Data.M_EntityID}]";
            }
            // 创建UnitPendantView预制体
            Transform m_container = UnityExtension.FindFunc(m_entityCtrl.Container.transform, "ModelOffset");
            if (m_container != null)
            {
                //GameObject go = Service.Resource.ResourceManager.Instance.LoadGameObject(m_modelPath);
                ////GameObject go = ResourceManager.LoadPrefab("Skill/SkillIndicator/SkillIndicatorView");
                //if (go != null)
                //{
                //    m_skillIndicatorView = go.GetComponent<SkillIndicatorView>();
                //    if (m_skillIndicatorView != null)
                //    {
                //        //CharacterController m_characterController = m_entityCtrl.Container.transform.GetChildren()[0].GetComponent<CharacterController>();
                //        m_skillIndicatorView.transform.SetParent(m_container.parent, false);
                //        m_skillIndicatorView.Init(m_entityCtrl);
                //    }
                //}
                StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(m_modelPath,
                (GameObject go) =>
                {
                    if (go == null)
                    {
                        return;
                    }

                    if (m_container != null && m_entityCtrl != null && m_entityCtrl.Data != null)
                    {
                        var gob = GameObject.Instantiate<GameObject>(go);
                        if (gob != null)
                        {
                            InitSkillIndicatorView(gob, m_container);
                        }
                    }
                });
            }
            else
            {
                SGF.Debuger.LogWarning($"{Log_Tag} Create m_container=null,,not find ModelOffset error!!!");
            }
        }

        private void InitSkillIndicatorView(GameObject gob, Transform m_container)
        {
            m_skillIndicatorView = gob.GetComponent<SkillIndicatorView>();
            if (m_skillIndicatorView != null)
            {
                //CharacterController m_characterController = m_entityCtrl.Container.transform.GetChildren()[0].GetComponent<CharacterController>();
                m_skillIndicatorView.transform.SetParent(m_container.parent, false);
                m_skillIndicatorView.Init(m_entityCtrl);
            }
            SetFlashHide(m_entityCtrl.IsHid);
        }

        public override void EnterFrame(int frameIndex)
        {
            if (m_skillIndicatorView != null)
            {
                m_skillIndicatorView.EnterFrame(frameIndex);
            }
        }

        private void Reset()
        {
            if (m_skillIndicatorView != null)
            {
                m_skillIndicatorView.Release();
            }
            m_skillIndicatorView = null;

            m_entityCtrl = null;
        }

        public override void Release()
        {
            Reset();
        }

        public override MonoBehaviour GetView()
        {
            if (m_skillIndicatorView != null)
            {
                return m_skillIndicatorView;
            }
            return null;
        }

        public override void SetFlashHide(bool isHide)
        {
            //if (m_skillIndicatorView != null)
            //{
            //    m_skillIndicatorView.SetFlashHide(isHide);
            //}
        }

        public override void InitRefreshState()
        {
            if (m_entityCtrl != null)
            {
                SetFlashHide(m_entityCtrl.IsHid);
            }
        }

        #region 技能委托回调

        /// <summary>
        /// 按下
        /// </summary>
        /// <param name="skillUnit">技能槽</param>
        public void OnPointerDown(SkillContainer skillUnit)
        {
            m_skillIndicatorView?.ShowlcontrolById(skillUnit);
        }
        /// <summary>
        /// 滑动
        /// </summary>
        /// <param name="dir"></param>
        public void OnSkillDirChange(SkillContainer skillUnit, Vector3 dir)
        {
            m_skillIndicatorView?.Movelcontrol(skillUnit, dir, Vector3.zero);
        }
        /// <summary>
        /// 抬起
        /// </summary>
        public void OnPointerUp(SkillContainer skillUnit)
        {
            m_skillIndicatorView?.HideSkilllcontrol(skillUnit);
        }
        /// <summary>
        /// 结束触摸
        /// </summary>
        public void OnEndDrag(SkillContainer skillUnit)
        {
            m_skillIndicatorView?.HideSkilllcontrol(skillUnit);
        }
        /// <summary>
        /// 取消
        /// </summary>
        public void OnCancel(SkillContainer skillUnit)
        {
            m_skillIndicatorView?.HideSkilllcontrol(skillUnit);
        }
        /// <summary>
        /// 死亡关闭预警圈
        /// </summary>
        public void OnDeadDisPlay()
        {
            m_skillIndicatorView?.HideSkilllcontrol();
        }
        #endregion

    }
}
