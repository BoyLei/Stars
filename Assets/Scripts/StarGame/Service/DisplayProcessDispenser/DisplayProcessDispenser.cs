///--------------------------------------------------------------------
/// �ļ���   :   ��ʾ���̷ַ���
/// ��  ��   :   1��ϵͳ��Ϣ��ʾ
///              2��FlyItem
///              3��----û��Ҫ����Uimgr���棬UIMgr�ǿ����Page.Window��Weight�����Ǿ���Ĺ�����ui��������Ʒ���֣�û��Ҫ����UiMgr����
///              4����չ�Ƕ��е�OutPut
///--------------------------------------------------------------------
using SGF.Module.Framework;
using SGF.UI.Framework;
using SGF.Unity;
using StarProject.UI.Common;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Service.DisplayProcess
{
    //public class MessageObj
    //{=
    //    public ͼƬ ͼƬ;
    //    public string message;
    //    public ��ɫ ��ɫ;
    //    public λ�� λ��;
    //    public �ٶ� �ٶ�;
    //    public ͣ��ʱ�� ͣ��ʱ��;
    //    //public MessageObj(string _message, bool _isShowRight = false, bool _isOnly = false)
    //    //{
    //    //    message = _message;
    //    //    isShowRight = _isShowRight;
    //    //    isOnly = _isOnly;
    //    //}
    //}
    public class DisplayProcessDispenser : ServiceModule<DisplayProcessDispenser>
    {
        //ϵͳTips
        private int curMessageDelay = GameConfig.SPECIAL_MESSAGE_FIX_SPACE_DELAY;
        private int messageMaxDelay = GameConfig.SPECIAL_MESSAGE_FIX_SPACE_DELAY;       //ÿ5���߼�֡�����ʾһ����Ϣ
        private Queue<string> MessageQueue = new();
        private static Queue<GameObject> UnUseGameTips = new();
        //战斗消息
        private BattleUITips battleTip;
        private float curSystemMessageDelay = 0;
        private float curSystemMessageLeftSec = 0;
        private Queue<string> SystemMessageQueue = new();
        private List<SystemUITips> SystemMessageShowList = new();
        private static Queue<SystemUITips> UnUseSystemTips = new();
        //系统消息
        private SystemUITips systemTip;
        // 成就消息
        private float curAchievementMessageDelay = 0;
        private SystemUITips achievementTips;
        private Queue<string> AchievementMessageQueue = new();
        private static Queue<SystemUITips> UnUseAchievementTips = new();

        // 成就消息
        private float curPartnerLevelTeamMessageDelay = 0;
        private float curPartnerLevelMessageLeftSec = 0;
        private SystemUITips partnerLevelTeamTips;
        private Queue<string> PartnerLevelTeamMessageQueue = new();
        private static Queue<SystemUITips> UnUsePartnerLevelTeamTips = new();

        public void Init()
        {
            {
                GameObject _go1 = ResourceHelperMono.LoadSystemTips();
                _go1.transform.SetParent(DynamicUIRoot.SystemMsgRoot.transform, false);
                _go1.SetActive(false);
                systemTip = _go1.GetComponent<SystemUITips>();
            }

            {
                GameObject _go2 = ResourceHelperMono.LoadBattleTips();
                _go2.transform.SetParent(DynamicUIRoot.BattleMsgRoot.transform, false);
                _go2.SetActive(false);
                battleTip = _go2.GetComponent<BattleUITips>();
            }

            //------- 成就提示消息
            //{
            //    GameObject _go3 = ResourceHelperMono.LoadAchievementTips();
            //    _go3.transform.SetParent(DynamicUIRoot.SystemMsgRoot.transform, false);
            //    _go3.SetActive(false);
            //    achievementTips = _go3.GetComponent<SystemUITips>();
            //}

            //------- 伙伴离开提示消息
            {
                GameObject _go = ResourceHelperMono.AddressablesLoadPartnerLevelTips();
                _go.transform.SetParent(DynamicUIRoot.SystemMsgRoot.transform, false);
                _go.SetActive(false);
                partnerLevelTeamTips = _go.GetComponent<SystemUITips>();
            }

            CheckSingleton();
            MonoHelper.AddFixedUpdateListener(OnEnterFrame, MonoHelper.E_ModuleType.CommonService);
        }

        #region 系统提示Tips

        private string m_TempSystemMessage = "";
        public void ShowSystemMessage(string str)
        {
            AddSystemMessage(str);
        }

        public void AddSystemMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if (SystemMessageQueue.Count <= 0 && SystemMessageShowList.Count <= 0)
            {
                m_TempSystemMessage = string.Empty;
            }

            if (SystemMessageShowList.Count >= 3 && string.Equals(m_TempSystemMessage, message))
            {
                return;
            }

            m_TempSystemMessage = message;
            SystemMessageQueue.Enqueue(message);
        }

        private void DequeueSystemMessage()
        {
            string str = SystemMessageQueue.Dequeue();
            if (string.IsNullOrEmpty(str))
            {
                return;
            }

            SystemUITips systemUITips = null;
            if (UnUseSystemTips.Count == 0)
            {
                GameObject _go = ResourceHelperMono.LoadSystemTips();
                systemUITips = _go.GetComponent<SystemUITips>();
            }
            else
            {
                systemUITips = UnUseSystemTips.Dequeue();
                systemUITips.gameObject.SetActive(true);
            }

            if (systemUITips != null)
            {
                SystemMessageShowList.Add(systemUITips);
                systemUITips.transform.SetParent(DynamicUIRoot.SystemMsgRoot.transform, false);

                systemUITips.PlayTween(str, () =>
                {
                    SystemMessageShowList.Remove(systemUITips);
                    UnUseSystemTips.Enqueue(systemUITips);
                });

                for (int i = 0; i < SystemMessageShowList.Count; i++)
                {
                    SystemMessageShowList[i]?.SetOffsetY(i + 1, SystemMessageShowList.Count);
                }
            }
        }

        private static void ClearRemoveSystemTipsQueue()
        {
            while (UnUseSystemTips.Count != 0)
            {
                SystemUITips systemUITips = UnUseSystemTips.Dequeue();
                GameObject.Destroy(systemUITips.gameObject);
            }
        }


        #endregion

        #region 成就提示Tips

        private string m_TempAchievementMessage = "";
        public void ShowAchievementMessage(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return;
            }
            if (string.Equals(m_TempAchievementMessage, str))
            {
                return;
            }

            m_TempAchievementMessage = str;

            achievementTips.Play(str, () =>
            {
                achievementTips.gameObject.SetActive(false);
                m_TempAchievementMessage = "";
            });
        }

        public void AddAchievementMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            //if (AchievementMessageQueue.Count <= 0)
            //{
            //    curAchievementMessageDelay = 0;
            //}
            AchievementMessageQueue.Enqueue(message);
        }

        private void DequeueAchievementMessage()
        {
            string str = AchievementMessageQueue.Dequeue();
            if (string.IsNullOrEmpty(str))
            {
                return;
            }
            if (string.Equals(m_TempAchievementMessage, str))
            {
                return;
            }

            UIManager.Instance.OpenWidgetAsync(UIDef.AchievementTipsWidget, null, true, str, DynamicUIRoot.SystemMsgRoot.transform, MainPageCommond.HideNone);

            //SystemUITips systemUITips = null;
            //if (UnUseAchievementTips.Count == 0)
            //{
            //    GameObject _go = ResourceHelperMono.LoadAchievementTips();
            //    systemUITips = _go.GetComponent<SystemUITips>();
            //}
            //else
            //{
            //    systemUITips = UnUseAchievementTips.Dequeue();
            //    systemUITips.gameObject.SetActive(true);
            //}

            //if (systemUITips != null)
            //{
            //    systemUITips.transform.SetParent(DynamicUIRoot.SystemMsgRoot.transform, false);
            //    systemUITips.Play(str, () =>
            //    {
            //        systemUITips.gameObject.SetActive(false);
            //        UnUseAchievementTips.Enqueue(systemUITips);
            //    });
            //}
        }

        private static void ClearRemoveAchievementTipsQueue()
        {
            while (UnUseAchievementTips.Count != 0)
            {
                SystemUITips systemUITips = UnUseAchievementTips.Dequeue();
                GameObject.Destroy(systemUITips.gameObject);
            }
        }

        #endregion

        #region 战斗提示Tips

        private string m_TempBattleMessage = "";

        public void ShowBattleMessage(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return;
            }

            if (string.Equals(m_TempBattleMessage, str))
            {
                return;
            }

            m_TempBattleMessage = str;

            battleTip.Play(str, () =>
            {
                battleTip.gameObject.SetActive(false);
                m_TempBattleMessage = "";
            });
        }

        #endregion

        #region 提示Tips

        private static void ClearRemoveQueue()
        {
            while (UnUseGameTips.Count != 0)
            {
                GameObject gob = UnUseGameTips.Dequeue();
                GameObject.Destroy(gob);
            }
        }

        public void AddSpecialMessage(string message)
        {
            //暂时关闭

#if !UNITY_EDITOR
            return;
#endif

            MessageQueue.Enqueue(message);
        }

        private void DequeueSpecialMessage()
        {
            String str = MessageQueue.Dequeue();
            GameObject specMessage;
            if (UnUseGameTips.Count == 0)
            {
                specMessage = ResourceHelperMono.LoadSpecialTips();//ȫ�ֵ����
            }
            else
            {
                specMessage = UnUseGameTips.Dequeue();
                specMessage.SetActive(true);
            }

            if (specMessage != null)
            {
                specMessage.transform.SetParent(DynamicUIRoot.SpecMsgRoot.transform, false);
                specMessage.transform.GetComponent<UITips>().Play(str, () =>
                {
                    specMessage.SetActive(false);
                    UnUseGameTips.Enqueue(specMessage);
                });
            }
        }

        #endregion

        #region 伙伴离队提示Tips

        private string m_TempPartnerLevelMessage = "";

        public void AddPartnerLevelMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if (PartnerLevelTeamMessageQueue.Count <= 0)
            {
                m_TempPartnerLevelMessage = string.Empty;
            }

            if (string.Equals(m_TempPartnerLevelMessage, message))
            {
                return;
            }

            if (curPartnerLevelMessageLeftSec == 0)
            {
                curPartnerLevelTeamMessageDelay = 0;
            }
            curPartnerLevelMessageLeftSec += GameConfig.SYSTEM_MESSAGE_SPACE_DELAY;

            m_TempPartnerLevelMessage = message;
            PartnerLevelTeamMessageQueue.Enqueue(message);
        }

        private void DequeuePartnerLevelMessage()
        {
            string str = PartnerLevelTeamMessageQueue.Dequeue();
            if (string.IsNullOrEmpty(str))
            {
                return;
            }

            SystemUITips partnerLevelUITips = null;
            if (UnUsePartnerLevelTeamTips.Count == 0)
            {
                GameObject _go = ResourceHelperMono.LoadPartnerLevelTeamTips();
                partnerLevelUITips = _go.GetComponent<SystemUITips>();
            }
            else
            {
                partnerLevelUITips = UnUsePartnerLevelTeamTips.Dequeue();
                partnerLevelUITips.gameObject.SetActive(true);
            }

            if (partnerLevelUITips != null)
            {
                partnerLevelUITips.transform.SetParent(DynamicUIRoot.SystemMsgRoot.transform, false);
                partnerLevelUITips.Play(str, () =>
                {
                    partnerLevelUITips.gameObject.SetActive(false);
                    UnUsePartnerLevelTeamTips.Enqueue(partnerLevelUITips);
                });
            }
        }

        private static void ClearRemovePartnerLevelTupsQueue()
        {
            while (UnUsePartnerLevelTeamTips.Count != 0)
            {
                SystemUITips systemUITips = UnUsePartnerLevelTeamTips.Dequeue();
                GameObject.Destroy(systemUITips.gameObject);
            }
        }


        #endregion

        private void OnEnterFrame()
        {
            if (MessageQueue.Count != 0)
            {
                if (curMessageDelay < messageMaxDelay)
                {
                    curMessageDelay++;
                }
                else
                {
                    curMessageDelay = 0;
                    DequeueSpecialMessage();
                }
            }

            // 成就tips
            if (AchievementMessageQueue.Count != 0)
            {
                curAchievementMessageDelay -= UnityEngine.Time.fixedDeltaTime;
                if (curAchievementMessageDelay <= 0)
                {
                    //SGF.Debuger.LogWarning($"播放成就间隔 -----------");
                    curAchievementMessageDelay = GameConfig.ACHIEVEMENT_MESSAGE_SPACE_DELAY;
                    DequeueAchievementMessage();
                }
            }

            if (SystemMessageQueue.Count != 0)
            {
                //可以继续从未显示的队列取出做显示了
                if (SystemMessageShowList.Count <= 2)
                {
                    DequeueSystemMessage();
                }
            }

            // 伙伴离队消息
            if (PartnerLevelTeamMessageQueue.Count != 0)
            {
                curPartnerLevelTeamMessageDelay -= UnityEngine.Time.fixedDeltaTime;
                if (curPartnerLevelTeamMessageDelay <= 0)
                {
                    //SGF.Debuger.LogWarning($"播放成就间隔 -----------");
                    curPartnerLevelTeamMessageDelay = GameConfig.SYSTEM_MESSAGE_SPACE_DELAY;
                    DequeuePartnerLevelMessage();
                }
            }
        }

        public override void Release()
        {
            MonoHelper.AddFixedUpdateListener(OnEnterFrame, MonoHelper.E_ModuleType.CommonService);

            battleTip = null;
            systemTip = null;
            achievementTips = null;

            m_TempSystemMessage = string.Empty;
            m_TempAchievementMessage = string.Empty;
            m_TempBattleMessage = string.Empty;

            MessageQueue.Clear();
            ClearRemoveQueue();

            AchievementMessageQueue.Clear();
            ClearRemoveAchievementTipsQueue();

            base.Release();
        }

    }
}