using System.Collections;
using System.Collections.Generic;
using SGF.Module.Framework;
using StarProject.Game;
using StarProject.Game.Entity;
using StarProject.Game.Entity.Factory;
using UnityEngine;

namespace StarProject.Service.ServerService
{
    /// <summary>
    /// 服务类型,此处用的是 客户端自定义的服务类型,可能客户端不同的模块功能定义不同的服务,
    /// 目前先与 服务器的 MapServiceSourceTypeEnum 类型分离
    /// </summary>
    public enum ServerServiceType
    {
        Default,
        NPC,
        InterAction,
    }

    /// <summary>
    /// 注册的服务子类型. 比如NPC的服务，一条任务,可以有 注册服务/领取服务/ 和事件任务服务
    /// </summary>
    public enum RegServiceSubType
    {
        /// <summary>
        /// 承接任务类型
        /// </summary>
        Task_Pick,
        /// <summary>
        /// 交付任务
        /// </summary>
        Task_Finish,
        /// <summary>
        /// 事件任务
        /// </summary>
        Task_Event,

    }

    /// <summary>
    /// 服务的状态,如领取任务, 在未达成任务的状态时, 服务状态为 Default; 达到领取条件,但是未 领取时, 服务状态 Open; 领取之后,服务状态 Close
    /// </summary>
    public enum ServerServiceState
    {
        Default,
        Open,
        Close,
    }

    [XLua.LuaCallCSharp]
    public class ServerService : EntityRemoteStatic
    {
        /// <summary>
        /// 服务名称，只有动态注册的服务才会填写，通用效果服务读取配置
        /// </summary>
        public string ServiceName;

        /// <summary>
        /// 通用的条件组 ID
        /// </summary>
        public int ConditionGroupID;
        /// <summary>
        /// 通用的效果ID
        /// </summary>
        public int EffectID;

        /// <summary>
        /// 服务的来源ID, 如果是承接/交付Npc任务,服务器的来源ID就是任务的ID
        /// </summary>
        public int ServiceSourceID;

        /// <summary>
        /// 服务来源的子ID, 如果是 事件任务, 那这个服务器需要给服务器发送 ServiceSourceID 任务ID + ServiceSourceSubID 事件任务ID
        /// </summary>
        public int ServiceSourceSubID;

        //交互对象存在的配置ID,如果是NPC,那就是NPC配置表中的npcID
        public uint ObjID;

        /// <summary>
        /// 注册的服务子类型. 比如NPC的服务，一条任务,可以有 注册服务/领取服务/ 和事件任务服务
        /// </summary>
        public RegServiceSubType regServiceSubType;

        /// <summary>
        /// 服务的 主key
        /// </summary>
        public string MainKey => $"{ConditionGroupID}_{EffectID}_{regServiceSubType}";

        /// <summary>
        /// 服务的状态,每次 get的时候,都会检查这个服务的状态
        /// </summary>
        public ServerServiceState state;

        public bool IsTaskServer = false;

        public void UpdateState()
        {

            // 服务都已经关闭了,就不需要再去刷新状态了
            //if (state == ServerServiceState.Close)
            //{
            //    return;
            //}

            // TODO: DL
            // 东哥给个检查服务状态的 接口
            bool isMete = Function.GlobalFunctionManager.Instance.ConditionGroupMete(GameManager.Instance.mainPlayerId, ConditionGroupID);//判断是否满足
            if(isMete)
            {
                state = ServerServiceState.Open;
            }
            else
            {
                state = ServerServiceState.Close;
            }

        }

        public bool CheckIsOpenState()
        {
            //UpdateState();
            return state == ServerServiceState.Open;
        }
    }

    public class ServerServiceManager : ServiceModule<ServerServiceManager>
    {
        private DictionaryEx<ServerServiceType, DictionaryEx<uint, List<ServerService>>> serverServices = new DictionaryEx<ServerServiceType, DictionaryEx<uint, List<ServerService>>>();

        public void Init()
        {
            CheckSingleton();
        }

        public void ClearAllService()
        {
            foreach(var v in serverServices)
            {
                foreach (var v2 in v.Value)
                {
                    v2.Value.Clear();
                }
                v.Value.Clear();
            }
        }

        public void RegisterService(ServerServiceType serviceType, ServerService serverService)
        {
            if (!serverServices.ContainsKey(serviceType))
            {
                serverServices.Add(serviceType, new DictionaryEx<uint, List<ServerService>>());
            }

            DictionaryEx<uint, List<ServerService>> services = serverServices[serviceType];

            uint cfgID = serverService.ObjID;
            if (!services.ContainsKey(cfgID))
            {
                services.Add(cfgID, new List<ServerService>());
            }

            List<ServerService> listServices = services[cfgID];

            int idx = listServices.FindIndex((ServerService service) =>
            {
                return service.MainKey == serverService.MainKey;
            });

            if (-1 != idx)
            {
                return;
            }
            listServices.Add(serverService);
        }


        public List<ServerService> GetRegisterServices(ServerServiceType serviceType, uint configID)
        {
            List<ServerService> getServices = new List<ServerService>();

            if (!serverServices.ContainsKey(serviceType))
            {
                return getServices;
            }
            DictionaryEx<uint, List<ServerService>> services = serverServices[serviceType];
            if (!services.ContainsKey(configID))
            {
                return getServices;
            }

            List<ServerService> serverList = services[configID];

            for (int i = 0; i < serverList.Count; i++)
            {
                ServerService server = serverList[i];
                getServices.Add(server);
            }

            return getServices;
        }

        /// <summary>
        /// 获取服务服务list
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="configID">根据服务类型,传入的配置id, 如果时npc,那就是npc的 configID</param>
        public List<ServerService> GetServices(ServerServiceType serviceType, uint configID)
        {
            List<ServerService> getServices = new List<ServerService>();

            if (!serverServices.ContainsKey(serviceType))
            {
                return getServices;
            }

            DictionaryEx<uint, List<ServerService>> services = serverServices[serviceType];


            if (!services.ContainsKey(configID))
            {
                return getServices;
            }

            List<ServerService> serverList = services[configID];

            for (int i = 0; i < serverList.Count; i++)
            {
                ServerService server = serverList[i];
                if (server.CheckIsOpenState())
                {
                    getServices.Add(server);
                }
            }

            return getServices;
        }


        /// <summary>
        /// 刷新所有NPC的显隐
        /// </summary>
        /// <param name="configIDs"></param>
        public void UpdateAllNpcServiceState()
        {
            if (serverServices.ContainsKey(ServerServiceType.NPC))
            {
                foreach (var s in serverServices[ServerServiceType.NPC])
                {
                    for (int i = 0; i < s.Value.Count; i++)
                    {
                        s.Value[i].UpdateState();
                    }
                }
            }



            if (serverServices.ContainsKey(ServerServiceType.InterAction))
            {
                foreach (var s in serverServices[ServerServiceType.InterAction])
                {
                    for (int i = 0; i < s.Value.Count; i++)
                    {
                        s.Value[i].UpdateState();
                    }
                }
            }

        }

        /// <summary>
        /// 刷新指定NPC的显隐
        /// </summary>
        /// <param name="configIDs"></param>
        public void UpdateNpcServiceState(ServerServiceType serviceType, List<uint> configIDs)
        {
            if (!serverServices.ContainsKey(serviceType))
            {
                return ;
            }

            for(int i=0;i< configIDs.Count;i++)
            {
                uint configID = configIDs[i];
                if (!serverServices[serviceType].ContainsKey(configID))
                {
                    return;
                }
                List<ServerService> serverList = serverServices[serviceType][configID];
                for (int j = 0; j < serverList.Count; j++)
                {
                    serverList[j].UpdateState();
                }
            }

            //刷新地图NPC显影


        }


        /// <summary>
        /// 重新登录时的重置
        /// </summary>
        public void Reset()
        {
            foreach (KeyValuePair<ServerServiceType, DictionaryEx<uint, List<ServerService>>> item in serverServices)
            {
                foreach (KeyValuePair<uint, List<ServerService>> itemServiceList in item.Value)
                {
                    List<ServerService> serviceList = itemServiceList.Value;
                    for (int i = 0; i < serviceList.Count; i++)
                    {
                        EntityFactory.ReleaseEntity(serviceList[i]);
                    }
                    serviceList.Clear();
                }
                item.Value.Clear();
            }
            serverServices.Clear();
        }
    }
}
