using System.Collections;
using System.Collections.Generic;
using SGF.Module.Framework;
using UnityEngine;
using System;
using StarProject.Game;
using StarProject.Game.Player;

namespace StarProject.Service.User
{
    public delegate void CusCmdHandle(string cusCmd, string param);

    public class GMManager : ServiceModule<GMManager>
    {
        public Dictionary<string, List<CusCmdHandle>> cusCmdHandles = new Dictionary<string, List<CusCmdHandle>>();

        public Action<string> ActionTriggerCusGM;
        public void Init()
        {
            CheckSingleton();
            RegCusCmdHandle("ShowMapMesh", ShowMapMesh);
            RegCusCmdHandle("CloseSkillLog", CloseSkillLog);
            RegCusCmdHandle("ShowAvatarIcon", ShowAvatarIcon);
            RegCusCmdHandle("ClientCareerChange", ClientCareerChange);
            RegCusCmdHandle("OpenQASDK", OnOpenSDK);
            RegCusCmdHandle("TestReConnect", OnTestReConnect);
        }

        /// <summary>
        /// 注册一些 自定义的 gm 指令
        /// </summary>
        /// <param name="cusCmd"></param>
        /// <param name="handle"></param>
        public void RegCusCmdHandle(string cusCmd, CusCmdHandle handle)
        {
            List<CusCmdHandle> handles;
            if (cusCmdHandles.ContainsKey(cusCmd))
            {
                handles = cusCmdHandles[cusCmd];
            }
            else
            {
                handles = new List<CusCmdHandle>();
                cusCmdHandles.Add(cusCmd, handles);
            }
            handles.Add(handle);

        }

        public void UnRegCusCmdHandle(string cusCmd, CusCmdHandle handle)
        {
            List<CusCmdHandle> handles;
            if (!cusCmdHandles.ContainsKey(cusCmd))
            {
                return;
            }

            handles = cusCmdHandles[cusCmd];
            if (handles.Count == 0)
            {
                return;
            }

            int idx = handles.FindIndex((item) =>
            {
                return handle == item;
            });

            if (-1 == idx)
            {
                return;
            }

            handles.RemoveAt(idx);
        }

        public void TriggleCusCmd(string cusCmd, string param)
        {
            if (!cusCmdHandles.ContainsKey(cusCmd))
            {
                return;
            }

            switch (cusCmd)
            {
                case "ShowMapMesh":
                case "ClosePrePlaySkill":
                case "CloseSkillLog":
                case "ShowAvatarIcon":
                case "ClientCareerChange":
                case "OpenQASDK":
                case "TestReConnect":
                    {
                        List<CusCmdHandle> handles = cusCmdHandles[cusCmd];
                        handles.ForEach((CusCmdHandle handle) =>
                        {
                            handle.Invoke(cusCmd, param);
                        });
                    }
                    break;
                default: { SGF.Debuger.LogError($"No handle with cusCmd : {cusCmd} on TriggleCusCmd !!!"); } break;
            }
        }

        private Transform _mapMesh;
        public void ShowMapMesh(string cusCmd, string param)
        {
            if (_mapMesh == null)
            {
                GameObject prefab = Resources.Load<GameObject>("Perfab/TestMesh/200_200M");
                GameObject go = HideGameObjectOnInput.Instantiate(prefab);
                _mapMesh = go.transform;

                var gos = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

                for (int i = 0; i < gos.Length; i++)
                {
                    // 临时代码, map_4 地图的 y = 0 
                    if (gos[i].name.ToLower() == "map_4")
                    {
                        _mapMesh.SetY(0.1f);
                        break;
                    }
                }

            }
            bool active = param == "1";
            _mapMesh.gameObject.SetActive(active);
        }

        /// <summary>
        /// 关闭 客户端 预播放 技能
        /// </summary>
        private void ClosePrePlaySkill(string cusCmd, string param)
        {
            HandleCusCMDIsActive(cusCmd, param);
        }

        /// <summary>
        /// 关闭 客户端 预播放 技能
        /// </summary>
        private void CloseSkillLog(string cusCmd, string param)
        {
            HandleCusCMDIsActive(cusCmd, param);
        }

        private void HandleCusCMDIsActive(string cusCmd, string param)
        {
            bool active = param == "1";
            if (!GMTestData.GMDataMap.ContainsKey(cusCmd))
            {
                GMTestData.GMDataMap.Add(cusCmd, active);
            }
            else
            {
                GMTestData.GMDataMap[cusCmd] = active;
            }
        }

        /// <summary>
        /// 显示 avatar 的 模型 icon
        /// </summary>
        /// <param name="cusCmd"></param>
        /// <param name="param"></param>
        private void ShowAvatarIcon(string cusCmd, string param)
        {
            ActionTriggerCusGM?.Invoke("ShowAvatarIcon");
        }

        /// <summary>
        /// 客户端转职，切换主角模型
        /// </summary>
        /// <param name="cusCmd"></param>
        /// <param name="param"></param>
        private void ClientCareerChange(string cusCmd, string param)
        {
            var mainPlayer = GameManager.Instance.M_MainPlayerCtrlBase;
            if (mainPlayer != null)
            {
                var mainPlayerCtrl = mainPlayer as PlayerCtrlGroup;
                if (mainPlayerCtrl != null)
                {
                    mainPlayerCtrl.ChanageJob(Convert.ToUInt32(param));
                }
            }
        }

        private void OnOpenSDK(string cusCmd, string param)
        {
            HandleCusCMDIsActive(cusCmd, param);
        }

        private void OnTestReConnect(string cusCmd, string param)
        {
            SGF.Network.SocketBase battleSocket = SGF.Network.NetworkManager.Instance.gameSocket;
            battleSocket.ReStart();
        }

    }
}