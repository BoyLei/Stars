using StarProject.Service.Input;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.UI.SkillBtn
{
    public class SkillCanceller : MonoBehaviour
    {
        public GameObject Pressed;
        public GameObject Active;

        private E_SkillBtnState M_State = E_SkillBtnState.Active;
        private int m_ShowCount = 0;

        private Dictionary<int, bool> skillBtnState = new();

        private void Start()
        {
            skillBtnState.Clear();
            SetState(E_SkillBtnState.Active);
        }

        public void SetState(E_SkillBtnState state)
        {
            M_State = state;
            Active.SetActive(M_State == E_SkillBtnState.Active);
            Pressed.SetActive(M_State == E_SkillBtnState.Pressed);
        }

        public void SetVisiableState(int skillBtnPos, bool state)
        {
            if (skillBtnState.ContainsKey(skillBtnPos))
            {
                skillBtnState[skillBtnPos] = state;
            }
            else
            {
                skillBtnState.Add(skillBtnPos, state);
            }

            SetShow();
        }

        private void SetShow()
        {
            bool isShow = GetIsHasAnySkillBtnDown();
            gameObject.SetActive(isShow);
        }

        private bool GetIsHasAnySkillBtnDown()
        {
            foreach (var item in skillBtnState)
            {
                if (item.Value)
                {
                    return true;
                }
            }
            return false;
        }

        public void SetVisiable(bool isShow)
        {
            if (isShow)
            {
                Show();
                //Debug.Log($"技能按钮 取消按钮 isShow={isShow},m_ShowCount={m_ShowCount}");
            }
            else
            {
                Hide();
                //Debug.LogWarning($"技能按钮 取消按钮 isShow={isShow},m_ShowCount={m_ShowCount}");
            }
        }

        private void Hide()
        {
            m_ShowCount--;
            if (m_ShowCount <= 0)
            {
                gameObject.SetActive(false);
            }
        }

        private void Show()
        {
            m_ShowCount++;
            if (m_ShowCount == 1)
            {
                gameObject.SetActive(true);
            }
        }
    }
}