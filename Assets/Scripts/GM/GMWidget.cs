using GPUInstancer;
using ProtoMsg;
using SGF.Network;
using StarProject;
using StarProject.Game;
using StarProject.Service.LocalData;
using StarProject.Service.User;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SGF.UI.Framework
{
    public class GMWidget : UIWidget
    {
        public Transform gmPanel;
        public GameObject gmTagItemCopy;
        public InputField inputNode;
        public Toggle toggleNode;
        public Transform gmPanelRoot;
        public Transform avatarPanelRoot;
        public Transform profilerPanelRoot;
        public Transform battleInfoPanelRoot;
        public Dropdown dp;
        public InputField row1;
        public InputField col1;
        public InputField row2;
        public InputField col2;
        public Toggle toggleFlower;
        public Toggle toggleProp;
        public Toggle toggleScene;
        public Toggle toggleRelics;
        public Toggle toggleFence;
        public Toggle toggleBuilding;
        public Toggle toggleStair;
        public Toggle toggleShrub;
        public Toggle toggleFloorNStone;
        public Toggle toggleWater;
        public Toggle toggleTree;
        public Toggle toggleStone;
        public Toggle toggleTerrain;
        public Toggle toggleGrass;
        public Toggle toggleCloud;
        public MeshRenderer terrMeshRender;
        GameObject curSelObj = null;

        private GMDataCell curSelectedGM;

        public Action<string> OnShowGmOtherPaner;

        protected override void Awake()
        {
            onCloseDestroy = true;
            base.Awake();
            toggleNode.onValueChanged.AddListener(onValueChanged);
            //Close.onClick.AddListener(OnCloseHandler);

            GMManager.Instance.ActionTriggerCusGM += ActionTriggerCusGM;

            {
                Button Btn = transform.Find("gmRoot/InputPanel/BtnClose").GetComponent<Button>();
                Btn.onClick.AddListener(() =>
                {
                    OnBtnClose();
                });
            }

#if OPEN_EFFECT_TEST
            var assets = EffectAssetsConfig.Instance;
            List<string> l = new List<string>();
            foreach (var item in assets.effects)
            {
                if(item != null)
                {
                    l.Add(item.name);
                }
            }
            dp.AddOptions(l);
            curSelObj = assets.effects[0];
            dp.onValueChanged.AddListener((a) => { curSelObj = assets.effects[a]; });
#endif

            toggleScene.onValueChanged.AddListener(onToggleScene);
            toggleWater.onValueChanged.AddListener(onToggleWater);
            toggleTree.onValueChanged.AddListener(onToggleTree);
            toggleStone.onValueChanged.AddListener(onToggleStone);
            toggleFlower.onValueChanged.AddListener(onToggleFlower);
            toggleProp.onValueChanged.AddListener(onToggleProp);
            toggleRelics.onValueChanged.AddListener(onToggleRelics);
            toggleFence.onValueChanged.AddListener(onToggleFence);
            toggleBuilding.onValueChanged.AddListener(onToggleBuilding);
            toggleStair.onValueChanged.AddListener(onToggleStair);
            toggleShrub.onValueChanged.AddListener(onToggleShrub);
            toggleFloorNStone.onValueChanged.AddListener(onToggleFloorNStone);
            toggleTerrain.onValueChanged.AddListener(onToggleTerrain);
            toggleGrass.onValueChanged.AddListener(onToggleGrass);
            toggleGrass.onValueChanged.AddListener(onToggleGrass);
            toggleCloud.onValueChanged.AddListener(onToggleCloud);
        }

        //手动删除，和自动删除都会走这里，自动走框架会调用destroy，手动直接走
        protected override void OnDestroy()
        {
            base.OnDestroy();
            toggleNode.onValueChanged.RemoveListener(onValueChanged);
            //Close.onClick.AddListener(OnCloseHandler);

            GMManager.Instance.ActionTriggerCusGM -= ActionTriggerCusGM;

            {
                Button Btn = transform.Find("gmRoot/InputPanel/BtnClose").GetComponent<Button>();
                Btn.onClick.RemoveAllListeners();
            }

#if OPEN_EFFECT_TEST
            var assets = EffectAssetsConfig.Instance;
            List<string> l = new List<string>();
            foreach (var item in assets.effects)
            {
                if(item != null)
                {
                    l.Add(item.name);
                }
            }
            dp.AddOptions(l);
            curSelObj = assets.effects[0];
            dp.onValueChanged.AddListener((a) => { curSelObj = assets.effects[a]; });
#endif

            toggleScene.onValueChanged.RemoveListener(onToggleScene);
            toggleWater.onValueChanged.RemoveListener(onToggleWater);
            toggleTree.onValueChanged.RemoveListener(onToggleTree);
            toggleStone.onValueChanged.RemoveListener(onToggleStone);
            toggleFlower.onValueChanged.RemoveListener(onToggleFlower);
            toggleProp.onValueChanged.RemoveListener(onToggleProp);
            toggleRelics.onValueChanged.RemoveListener(onToggleRelics);
            toggleFence.onValueChanged.RemoveListener(onToggleFence);
            toggleBuilding.onValueChanged.RemoveListener(onToggleBuilding);
            toggleStair.onValueChanged.RemoveListener(onToggleStair);
            toggleShrub.onValueChanged.RemoveListener(onToggleShrub);
            toggleFloorNStone.onValueChanged.RemoveListener(onToggleFloorNStone);
            toggleTerrain.onValueChanged.RemoveListener(onToggleTerrain);
            toggleGrass.onValueChanged.RemoveListener(onToggleGrass);
            toggleGrass.onValueChanged.RemoveListener(onToggleGrass);
            toggleCloud.onValueChanged.RemoveListener(onToggleCloud);
        }

        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);
            //1，永远处理剩
            RefreshGMPanel();

            //if (GameInput.Instance != null)
            //{
            //    GameInput.Instance.SetPanelAp(false);
            //}
        }

        protected override void OnClose(object arg = null)
        {
            base.OnClose(arg);

            //if (GameInput.Instance != null)
            //{
            //    GameInput.Instance.SetPanelAp(true);
            //}
        }

        private void onValueChanged(bool result)
        {
            Transform descNode = toggleNode.transform.Find("desc");
            if (descNode != null)
            {
                descNode.GetComponent<Text>().text = result.ToString();
            }
        }

        GameObject sceneObj = null;
        GameObject stoneObj1 = null;
        GameObject stoneObj2 = null;
        GameObject treeObj1 = null;
        GameObject treeObj2 = null;
        GameObject waterObj = null;
        GameObject flowerObj = null;
        GameObject propObj = null;
        GameObject relicsObj = null;
        GameObject fenceObj = null;
        GameObject buildingObj = null;
        GameObject stairObj = null;
        GameObject shrubObj = null;
        GameObject floorObj = null;
        GameObject cloudObj = null;
        void onToggleScene(bool result)
        {
            if (sceneObj == null)
            {
                sceneObj = GameObject.Find("Scene");
            }
            sceneObj.SetActive(result);
        }
        void onToggleStone(bool result)
        {
            if (stoneObj1 == null)
            {
                stoneObj1 = GameObject.Find("Rock-");
            }
            if (stoneObj2 == null)
            {
                stoneObj2 = GameObject.Find("Rock+");
            }
            stoneObj1.SetActive(result);
            stoneObj2.SetActive(result);
        }

        void onToggleWater(bool result)
        {
            if (waterObj == null)
            {
                waterObj = GameObject.Find("Water");
            }
            waterObj.SetActive(result);
        }

        void onToggleTree(bool result)
        {
            if (treeObj1 == null)
            {
                treeObj1 = GameObject.Find("Tree-");
            }
            if (treeObj2 == null)
            {
                treeObj2 = GameObject.Find("Tree+");
            }
            treeObj1.SetActive(result);
            treeObj2.SetActive(result);
        }

        void onToggleFlower(bool result)
        {
            if (flowerObj == null)
            {
                flowerObj = GameObject.Find("Flower");
            }
            flowerObj.SetActive(result);
        }
        void onToggleProp(bool result)
        {
            if (propObj == null)
            {
                propObj = GameObject.Find("Prop");
            }
            propObj.SetActive(result);
        }
        void onToggleRelics(bool result)
        {
            if (relicsObj == null)
            {
                relicsObj = GameObject.Find("Relics");
            }
            relicsObj.SetActive(result);
        }
        void onToggleFence(bool result)
        {
            if (fenceObj == null)
            {
                fenceObj = GameObject.Find("Fence");
            }
            fenceObj.SetActive(result);
        }
        void onToggleBuilding(bool result)
        {
            if (buildingObj == null)
            {
                buildingObj = GameObject.Find("Building");
            }
            buildingObj.SetActive(result);
        }
        void onToggleStair(bool result)
        {
            if (stairObj == null)
            {
                stairObj = GameObject.Find("Stair");
            }
            stairObj.SetActive(result);
        }
        void onToggleShrub(bool result)
        {
            if (shrubObj == null)
            {
                shrubObj = GameObject.Find("Shrub");
            }
            shrubObj.SetActive(result);
        }
        void onToggleFloorNStone(bool result)
        {
            if (floorObj == null)
            {
                floorObj = GameObject.Find("FloorNStone");
            }
            floorObj.SetActive(result);
        }

        void onToggleTerrain(bool result)
        {
            if (terrMeshRender == null)
            {
                terrMeshRender = GameObject.Find("Map_ChenXGD_Terrain").GetComponent<MeshRenderer>();
            }
            terrMeshRender.enabled = result;
        }
        GPUInstancerTreeManager instanceManager = null;
        void onToggleGrass(bool result)
        {
            if (instanceManager == null)
            {
                instanceManager = FindObjectOfType<GPUInstancerTreeManager>();
            }
            if (result)
            {
                foreach (var item in instanceManager.runtimeDataList)
                {
                    item.prototype.maxDistance = 40;
                }
            }
            else
            {
                foreach (var item in instanceManager.runtimeDataList)
                {
                    item.prototype.maxDistance = 0;
                }
            }
        }

        void onToggleCloud(bool result)
        {
            if (cloudObj == null)
            {
                cloudObj = GameObject.Find("AirShadow");
            }
            cloudObj.SetActive(result);
        }

        private void RefreshGMPanel()
        {
            gmPanel.DestroyChildren();

            if (gmTagItemCopy == null)
            {
                return;
            }

            Dictionary<int, GMDataCell> gmDatas = LocalDataManager.Instance.M_GMData.StaticGMDatas;

            Dictionary<int, List<GMDataCell>> tagGMDatas = new();

            foreach (KeyValuePair<int, GMDataCell> item in gmDatas)
            {
                GMDataCell gMDataCell = item.Value;
                int tag = gMDataCell.GetTag();
                List<GMDataCell> datas;
                if (!tagGMDatas.TryGetValue(tag, out datas))
                {
                    datas = new List<GMDataCell>();
                }

                datas.Add(item.Value);
                tagGMDatas[tag] = datas;
            }

            List<GMDataCell> datas1;
            if (!tagGMDatas.TryGetValue(6, out datas1))
            {
                datas1 = new List<GMDataCell>();
            }

            // 添加一个显示 模型 资源的命令
            {
                var dataCell = new GMDataCell();
                dataCell.ID = 10000111;
                dataCell.Tag = 6;
                dataCell.Name = "显示avatarIcon面板";
                dataCell.GMCMD = "ShowAvatarIcon";
                dataCell.Param = "";
                dataCell.ParamType = "1";

                datas1.Add(dataCell);
            }


            for (int i = 1; i <= 7; i++)
            {
                List<GMDataCell> datas;
                if (!tagGMDatas.TryGetValue(i, out datas))
                {
                    datas = new List<GMDataCell>();
                }
                GameObject gmItemGob = Instantiate(gmTagItemCopy, gmPanel);
                gmItemGob.SetActive(true);

                GMTagItem gmTagItem = gmItemGob.GetComponent<GMTagItem>();

                gmTagItem.ActionOnSelectGM = OnActionGmSelect;
                gmTagItem.InitDatas(i, datas);
            }
        }

        private void ResetGMParamsPanel()
        {
            if (inputNode)
            {
                inputNode.gameObject.SetActive(false);
            }
            if (toggleNode)
            {
                toggleNode.gameObject.SetActive(false);
            }
        }

        private void RefreshGMParamsPanel(GMDataCell gMDataCell)
        {
            curSelectedGM = gMDataCell;
            ResetGMParamsPanel();
            switch (gMDataCell.ParamType.ToLower())
            {
                case "1":
                    {
                        RefreshInputShow();
                    }
                    break;
                case "2":
                case "false":
                case "true":
                    {
                        RefreshToggleShow();
                    }
                    break;
                default: break;
            }
        }

        private string GetRecordGMData(string key)
        {
            string recordGmData = "";
            if (AppConfig.GMData.TryGetValue(key, out recordGmData))
            {
                return recordGmData;
            }
            return "";
        }

        private void RefreshInputShow()
        {
            if (inputNode == null)
            {
                return;
            }
            inputNode.gameObject.SetActive(true);

            string key = $"{curSelectedGM.GMCMD}_{curSelectedGM.Param}";
            string recordGmData = GetRecordGMData(key);

            string param = recordGmData.Length > 0 ? recordGmData : curSelectedGM.Param;

            inputNode.placeholder.GetComponent<Text>().text = param;
            inputNode.text = recordGmData.Length > 0 ? recordGmData : "";
        }

        private void RefreshToggleShow()
        {
            if (toggleNode == null)
            {
                return;
            }
            toggleNode.gameObject.SetActive(true);
            string key = $"{curSelectedGM.GMCMD}_{curSelectedGM.Param}";
            string recordGmData = GetRecordGMData(key);

            string param = recordGmData.Length > 0 ? recordGmData : curSelectedGM.Param;

            bool result = false;
            if (param == "" || param.ToLower() == "0" || param.ToLower() == "false")
            {
                result = false;
            }
            else
            {
                result = true;
            }
            Transform descNode = toggleNode.transform.Find("desc");
            if (descNode != null)
            {
                descNode.GetComponent<Text>().text = result.ToString();
            }

            toggleNode.isOn = result;
        }

        public void OnActionGmSelect(GMDataCell gMDataCell)
        {
            RefreshGMParamsPanel(gMDataCell);
        }

        private string GetCMDParam()
        {
            string gmParam = "";
            switch (curSelectedGM.ParamType.ToLower())
            {
                case "1":
                    {
                        if (inputNode == null)
                        {
                            return "";
                        }
                        if (inputNode.text.Length > 0)
                        {
                            gmParam = inputNode.text;
                        }
                        else
                        {
                            gmParam = inputNode.placeholder.GetComponent<Text>().text;
                        }
                    }
                    break;
                case "2":
                case "false":
                case "true":
                    {
                        if (toggleNode == null)
                        {
                            return "";
                        }
                        gmParam = toggleNode.isOn ? "1" : "0";
                    }
                    break;
                default: break;
            }
            return gmParam;
        }

        private string GetGMCMD()
        {
            string gmParam = GetCMDParam();
            if (string.IsNullOrEmpty(curSelectedGM.GMCMD))
            {
                return $"{gmParam}";
            }
            else
            {
                return $"{curSelectedGM.GMCMD} {gmParam}";
            }
        }

        private int GetGMTag()
        {
            return curSelectedGM.GetTag();
        }

        private void UpdateGMData()
        {
            string key = $"{curSelectedGM.GMCMD}_{curSelectedGM.Param}";
            string gmParam = GetCMDParam();
            AppConfig.GMData[key] = gmParam;
        }

        public void ClickSendGM()
        {
            string GMCMD = GetGMCMD();
            UpdateGMData();
            // 6 目前定下来的是 自定义的 gm指令, 不需要给服务器发送gm
            if (GetGMTag() != 6)
            {
                SendGM(GMCMD);
            }
            else
            {
                SendCustomGm();
            }
        }

        private void SendGM(string command)
        {
            SGF.Debuger.Log($"MG命令 = {command}");
            string[] sp = command.Split("|");
            for (int i = 0; i < sp.Length; i++)
            {
                string cmd = sp[i];
                SocketBase battleSocket = NetworkManager.Instance.gameSocket;
                GmCmdReq gmCmdReq = new();
                gmCmdReq.Cmd = cmd;
                SGF.Debuger.Log($"MG命令 i={i},cmd={gmCmdReq}");
                battleSocket.SendRPCMsg(ServerType.ServerTypeLobby, gmCmdReq, isAutoChangeMsgTarget: false);
            }
        }

        private void SendCustomGm()
        {
            GMManager.Instance?.TriggleCusCmd(curSelectedGM.GMCMD, GetCMDParam());
        }

        private void ShowAvatarIcon()
        {
            gmPanelRoot.gameObject.SetActive(false);
            avatarPanelRoot.gameObject.SetActive(true);
            profilerPanelRoot.gameObject.SetActive(false);
            battleInfoPanelRoot.gameObject.SetActive(false);
        }

        public void CloseAvatarIcon()
        {
            gmPanelRoot.gameObject.SetActive(true);
            avatarPanelRoot.gameObject.SetActive(false);
            profilerPanelRoot.gameObject.SetActive(false);
            battleInfoPanelRoot.gameObject.SetActive(false);
        }

        //挂载事件：没有调试时的性能隐患
        public void ShowProfiler()
        {
            gmPanelRoot.gameObject.SetActive(false);
            avatarPanelRoot.gameObject.SetActive(false);
            profilerPanelRoot.gameObject.SetActive(true);
            battleInfoPanelRoot.gameObject.SetActive(false);
            //关闭所有石头，点击的时候，先找到挂载的父节点然后遍历子节点，进行activeFalse
        }
        //挂载事件：没有调试时的性能隐患
        public void ShowBattleInfo()
        {
            gmPanelRoot.gameObject.SetActive(false);
            avatarPanelRoot.gameObject.SetActive(false);
            profilerPanelRoot.gameObject.SetActive(false);
            battleInfoPanelRoot.gameObject.SetActive(true);
        }

        public void ClosePorfiler()
        {
            gmPanelRoot.gameObject.SetActive(true);
            avatarPanelRoot.gameObject.SetActive(false);
            profilerPanelRoot.gameObject.SetActive(false);
            battleInfoPanelRoot.gameObject.SetActive(false);
        }

        public void CloseBattleInfo()
        {
            gmPanelRoot.gameObject.SetActive(true);
            avatarPanelRoot.gameObject.SetActive(false);
            profilerPanelRoot.gameObject.SetActive(false);
            battleInfoPanelRoot.gameObject.SetActive(false);
        }


        private void ActionTriggerCusGM(string triggerCusGM)
        {
            if (triggerCusGM == "ShowAvatarIcon")
            {
                ShowAvatarIcon();
            }
        }

        public void OnBtnClose()
        {
            UIManager.Instance.CloseWidget(UIDef.GmWidget, UIRoot.UIROOT.transform, true);
        }

        public void OnOpenPostProcess()
        {
            StarProject.Service.UniversalRenderPipeline.UniRenderPipline.Instance.OpenAllRenderPassFeature();
        }

        public void OnClosePostProcess()
        {
            StarProject.Service.UniversalRenderPipeline.UniRenderPipline.Instance.CloseAllRenderPassFeature();
        }

        public void CreateNpc()
        {
            int row = int.Parse(row1.text);
            int col = int.Parse(col1.text);
            StarProject.Game.Player.PlayerCtrlGroup m_MainPlayerCtrl = (StarProject.Game.Player.PlayerCtrlGroup)GameManager.Instance.M_MainPlayerCtrlBase;
            var pos = GameManager.Instance.GetEntityPosById(GameManager.Instance.mainPlayerId);
            for (int i = -row; i <= row; i++)
            {
                for (int j = -col; j <= col; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        continue;
                    }
                    /*                    GameCommand gameCommand = GameManager.Instance.gameCommand.Init(E_Command.Destroy, StarWorldGame.g_vsd.M_EntityID, StarWorldGame.g_vsd);
                                        gameCommand.isServerAOI = false;
                                        GameManager.Instance.EntityDataCommand(gameCommand);*/
                    m_MainPlayerCtrl?.AddPlayerViewNew(pos + new UnityEngine.Vector3(j, 0, i));
                }
            }

            /*for (int i = -row; i < row; i++)
            {
                for (int j = -col; j < col; j++)
                {
                    if(i==0 && j==0)
                    {
                        continue;
                    }
                    ProtoMsg.PropSyncList propSyncList = new ProtoMsg.PropSyncList();
                    propSyncList.EntityID = 99;
                    propSyncList.EntityType = "Player";
                    PropBaseSyncList propBaseSyncList = new PropBaseSyncList();
                    // 坐标
                    {
                        SyncBaseInfo syncBaseInfo = new SyncBaseInfo();
                        syncBaseInfo.Index = 1050;
                        ProtoMsg.Vector3 vector3 = new ProtoMsg.Vector3();
                        syncBaseInfo.MsgValue = vector3.ToByteString();
                        propBaseSyncList.Prop.Add(syncBaseInfo);
                    }
                    // 职业
                    {

                    }
                    propSyncList.Prop = propBaseSyncList;
                    GameManager.Instance.CreateEntityData(propSyncList);
                }
            }*/
        }




        public void ClearNpc()
        {
            StarProject.Game.Player.PlayerCtrlGroup m_MainPlayerCtrl = (StarProject.Game.Player.PlayerCtrlGroup)GameManager.Instance.M_MainPlayerCtrlBase;
            m_MainPlayerCtrl?.ClearPlayerView();
        }

        List<GameObject> cacheEffects = new();
        public void CreateEffect()
        {
            if (curSelObj == null)
            {
                return;
            }
            cacheEffects.Clear();
            var pos = GameManager.Instance.GetEntityPosById(GameManager.Instance.mainPlayerId);
            int row = int.Parse(row2.text);
            int col = int.Parse(col2.text);
            for (int i = -row; i <= row; i++)
            {
                for (int j = -col; j <= col; j++)
                {
                    var effPos = pos + new UnityEngine.Vector3(j, i, 0);
                    var obj = GameObject.Instantiate(curSelObj);
                    obj.transform.position = effPos;
                    cacheEffects.Add(obj);
                }
            }
        }

        public void ClearEffect()
        {
            foreach (var obj in cacheEffects)
            {
                GameObject.Destroy(obj);
            }
            cacheEffects.Clear();
        }
    }
}

