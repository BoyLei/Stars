using System;
using System.Collections;
using System.Collections.Generic;
using ProtoMsg;
using SGF.Module.Framework;
using SGF.Network;
using SGF.Unity;
using UnityEngine;

namespace StarProject.Module.QTE
{

    public class QTEModule : BusinessModule
    {
        /// <summary>
        /// 所有的QTE小游戏
        /// </summary>
        private Dictionary<int, Type> QTE_Games = new Dictionary<int, Type>()
        {
            //捕获诺诺球
            {1,typeof(QTE_CaptureNNQ)}
        };

        /// <summary>
        /// 当前运行的游戏
        /// </summary>
        private BaseQTEGame m_RunningGame;

        public override void Create(object args = null)
        {
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.LifeSkillQTEInfoNtfID,
                OnLifeSkillQTEInfoNtfHandler, this, false);
            MonoHelper.AddUpdateListener(OnUpdateHandler);
        }

        private void OnUpdateHandler()
        {
            if (m_RunningGame != null)
            {
                if (m_RunningGame.NeedTick && !m_RunningGame.IsWin)
                {
                    m_RunningGame.OnGameTick();
                }
            }
        }

        private void OnLifeSkillQTEInfoNtfHandler(MessageHandleData data)
        {
            LifeSkillQTEInfoNtf msg = data.data as LifeSkillQTEInfoNtf;
            if (msg != null && msg.QTEItem != null)
            {
                if (QTE_Games.ContainsKey(msg.QTEItem.QTEId))
                {
                    m_RunningGame = System.Activator.CreateInstance(QTE_Games[msg.QTEItem.QTEId]) as BaseQTEGame;
                    m_RunningGame.OnGameInit(msg.QTEItem);
                    m_RunningGame.OnGamePlay();
                }

            }
        }

        public override void Release()
        {
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.LifeSkillQTEInfoNtfID,
                OnLifeSkillQTEInfoNtfHandler, this, false);

            MonoHelper.RemoveUpdateListener(OnUpdateHandler);

            base.Release();
        }
    }
}