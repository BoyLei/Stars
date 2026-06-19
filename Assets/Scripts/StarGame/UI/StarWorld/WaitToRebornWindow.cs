using ProtoMsg;
using SGF.Network;
using SGF.UI.Framework;
using StarProject.Game;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProjectDef;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace StarProject.UI.StarWorld
{
    public class WaitToRebornWindow : UIWindow
    {
        private const string LOG_TAG = "[WaitToRebornWindow]";

        public Text m_timeToRelive;
        public Text m_freeRelive;

        int reborn_point_time_limit;

        public Button btnRelive;
        public Button btnPerfectRelive;

        public GameObject go_leftRoot;
        public GameObject go_rightRoot;

        protected override void OnOpen(object arg = null)
        {
            int reviveId = 0;
            GlobalEvent.onMainPlayerDie.AddListener(OnMainPlayerDie);

            int mapid = GameManager.Instance.GetCurMapId();
            var mapCfgData = LocalDataManager.Instance.GetMapCfgData(mapid);
            if (mapCfgData != null)
            {
                reviveId = mapCfgData.ReviveTid;
            }
            //没有配置，默认五秒
            if (reviveId == 0)
            {
                go_leftRoot.SetActive(true);
                go_rightRoot.SetActive(false);

                reborn_point_time_limit = 5; //副本安全复活倒计时 

                btnRelive.interactable = false;
                StopCoroutine("CoroutineFunc");
                StartCoroutine("CoroutineFunc");
            }
            else
            {
                //拿到场景的复活ID
                var reviveCfg = LocalDataManager.Instance.GetLevelReviveDataCell(reviveId);

                bool isReborn = reviveCfg.GetIs_reborn();//是否可以安全复活
                bool isPerfectReborn = reviveCfg.GetIs_perfect_enable();//是否可以原地复活

                //检测是否在pvp场景中
                StarProject.Module.EctypeModule module = SGF.Module.Framework.ModuleManager.Instance.GetModule(ModuleDef.Name.EctypeModule) as StarProject.Module.EctypeModule;
                if(module != null)
                {
                    isPerfectReborn &= module.IsCanPerfectReborn();
                }

                go_leftRoot.SetActive(isReborn);
                go_rightRoot.SetActive(isPerfectReborn);

                if (isReborn)
                {
                    reborn_point_time_limit = reviveCfg.GetReborn_point_time_limit(); //副本安全复活倒计时
                                                                                      //第一个按钮变灰倒计时结束后可以点击
                    if (reborn_point_time_limit > 0)
                    {
                        btnRelive.interactable = false;
                        StopCoroutine("CoroutineFunc");
                        StartCoroutine("CoroutineFunc");
                    }
                    else
                    {
                        btnRelive.interactable = true;
                        m_timeToRelive.text = "";
                    }
                }

                if (isPerfectReborn)
                {
                    float freeLeft = GameManager.Instance.GetRemainFreeRelieveCount();
                    //m_freeRelive.text = $"{GameConfig.LocalStr["RemainingFreeRuns"]}：<color=#FFF726>" + freeLeft + "</color>";
                    m_freeRelive.text = $"{LanguageManager.Instance.GetLanguageByKey("RemainingFreeRuns")}：<color=#FFF726>" + freeLeft + "</color>";

                    btnPerfectRelive.interactable = (freeLeft > 0);
                }
            }

            base.OnOpen(arg);
            transform.SetAsLastSibling();
        }

        protected override void OnClose(object arg = null)
        {
            GlobalEvent.onMainPlayerDie.RemoveListener(OnMainPlayerDie);
            base.OnClose(arg);
        }

        private void OnMainPlayerDie(bool arg0)
        {
            if (!arg0)
            {
                UIManager.Instance.CloseWindow(UIDef.UIWaitToRebornWindow);
            }
        }

        IEnumerator CoroutineFunc()
        {
            while (reborn_point_time_limit>0)
            {
                if (reborn_point_time_limit<10)
                {
                    //m_timeToRelive.text = "<color=#FF5D1DFF>0" + reborn_point_time_limit + "</color>秒后安全复活";
                    m_timeToRelive.text = string.Format(LanguageManager.Instance.GetLanguageByKey("SafeResurrection"), $"0{reborn_point_time_limit}");
                }
                else
                {
                    //m_timeToRelive.text = "<color=#FF5D1DFF>" + reborn_point_time_limit + "</color>秒后安全复活";
                    m_timeToRelive.text = string.Format(LanguageManager.Instance.GetLanguageByKey("SafeResurrection"), reborn_point_time_limit);
                }


                yield return new WaitForSeconds(1.0f);

                reborn_point_time_limit -= 1;

            }

            btnRelive.interactable = true;
            m_timeToRelive.text = "";
        }

        /// <summary>
        /// 安全复活
        /// </summary>
        public void OnBtnRevive()
        {
            RoleReviveReq roleReviveReqMsg = new RoleReviveReq();
            roleReviveReqMsg.ReviveType = ReviveTypeEnum.ReviveTypeDefault;
            NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeScene, roleReviveReqMsg, false);
            SGF.Debuger.Log($"{LOG_TAG} OnBtnRevive Send Msg=RoleReviveReq");
            //this.Close();
            UIManager.Instance.CloseWindow(UIDef.UIWaitToRebornWindow);
        }

        /// <summary>
        /// 原地复活
        /// </summary>
        public void OnBtnReviveIns()
        {
            RoleReviveReq roleReviveReqMsg = new RoleReviveReq();
            roleReviveReqMsg.ReviveType = ReviveTypeEnum.ReviveTypeMapCommonRevive;
            NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeScene, roleReviveReqMsg, false);
            SGF.Debuger.Log($"{LOG_TAG} OnBtnRevive Send Msg=RoleReviveReq");
            //this.Close();
            UIManager.Instance.CloseWindow(UIDef.UIWaitToRebornWindow);
        }
    }
}
