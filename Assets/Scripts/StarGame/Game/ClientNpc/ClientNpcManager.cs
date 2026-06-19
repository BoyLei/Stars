///--------------------------------------------------------------------
/// 文件名   :   ClientNpcManager.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/05/04 13:32:43
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using SGF.Module.Framework;
using StarProject;
using StarProject.Game.Map;
using StarProject.Game.Player;
using System.Collections;
using System.Collections.Generic;
using StarProject.Game;
using UnityEngine;

namespace ClientNpc
{
    public class ClientNpcManager : ServiceModule<ClientNpcManager>
    {
        public Dictionary<int, ClientNpc> ClientNpcs = null;

        public void Init()
        {
            ClientNpcs = new Dictionary<int, ClientNpc>();
            GlobalEvent.OnSceneMapConfigLoad.AddListener(OnSceneMapLoadComplete);
        }

        private void ClearClientNpc()
        {
            if (ClientNpcs != null)
            {
                foreach (var item in ClientNpcs)
                {
                    item.Value.OnRelease();
                }

                ClientNpcs.Clear();
            }
        }

        private void OnSceneMapLoadComplete(int mapID)
        {
            ClearClientNpc();
            if (GameMap.sceneJsonData != null && GameMap.sceneJsonData.Npcs != null)
            {
                foreach (var item in GameMap.sceneJsonData.Npcs)
                {
                    ClientNpcs.Add(item.Value.Index,
                        ClientNpc.Create(item.Value.Index, item.Value.NpcID, item.Value.TriggerGroups));
                }
            }
        }

        public void EnterFrame(int frameIndx)
        {
            if (ClientNpcs != null)
            {
                foreach (var item in ClientNpcs)
                {
                    item.Value.EnterFrame(frameIndx);
                }
            }
        }

        public void OnEnterAOI(EntityCtrlBase ctrlBase)
        {
            if (ctrlBase != null)
            {
                if (ClientNpcs.TryGetValue(ctrlBase.M_Curr.SpaceIndex, out var npc) && npc != null)
                {
                    npc.OnEnterAOI(ctrlBase);
                }
            }
        }

        public void OnLeaveAOI(int Index)
        {
            if (ClientNpcs.TryGetValue(Index, out var npc) && npc != null)
            {
                npc.OnLeaveAOI();
            }
        }

        public void ResumeLast(int Index)
        {
            if (ClientNpcs.TryGetValue(Index, out var npc) && npc != null)
            {
                npc.ResumeLast();
            }
        }

        public void TranslateState(int Index, StateEnum state)
        {
            if (ClientNpcs.TryGetValue(Index, out var npc) && npc != null)
            {
                npc.TranslateState(state);
            }
        }

        public Vector3 GetPosition(int Index)
        {
            if (ClientNpcs.TryGetValue(Index, out var npc) && npc != null)
            {
                return npc.Position;
            }

            return Vector3.zero;
        }

        public override void Release()
        {
            GlobalEvent.OnSceneMapConfigLoad.RemoveListener(OnSceneMapLoadComplete);
            base.Release();
        }


        /// <summary>
        /// Npc 移动
        /// </summary>
        /// <param name="mapID">地图ID</param>
        /// <param name="Index">Npc SpaceID</param>
        /// <param name="movetype">移动类型 0：按Spanwder 移动 1 按路径移动</param>
        /// <param name="argID"> sp : spid/ path :pathid</param>
        /// <param name="effectID">效果id</param>
        public void NpcMove(int mapID, int Index, int movetype, int argID, int effectID)
        {
            if (mapID == GameManager.Instance.GetCurMapId())
            {
                if (ClientNpcs.TryGetValue(Index, out var npc) && npc != null)
                {
                    switch (movetype)
                    {
                        case 0:
                            npc.MoveByPosition(mapID, argID, effectID);
                            break;
                        case 1:
                            npc.MoveByPath(mapID, argID, effectID);
                            break;
                    }
                }
            }
        }
    }
}