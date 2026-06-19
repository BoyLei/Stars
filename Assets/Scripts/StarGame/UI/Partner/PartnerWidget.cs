using ProtoMsg;
using SGF.Module.Framework;
using SGF.Unity;
using StarProject;
using StarProject.Game;
using StarProject.Service.Business;
using StarProject.Service.LocalData;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;

namespace SGF.UI.Framework
{
    public class PartnerWidget : UIWidget
    {
        public List<PartnerItem> partnerItems = new();
        public Animation m_Animation;
        public GameObject buffItem;
        private Transform m_objDialog;
        private Text m_txtDialog;
        private CanvasGroup m_Root;

        //public Transform parent;
        //private string prefabPath = "UI/StarWorld/PartnerItem";

        private Dictionary<long, int> m_LastPartnerSeat = new();
        private E_PlayerStateForMusic e_PlayerStateForMusic = E_PlayerStateForMusic.Normal;

        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);
            m_objDialog = transform.Find("ObjDialogTips");
            m_txtDialog = transform.Find("ObjDialogTips/LevelName").GetComponent<Text>();
            m_Root = transform.Find("parent").GetComponent<CanvasGroup>();
            e_PlayerStateForMusic = E_PlayerStateForMusic.Normal;
            AddEventListener();
            // 创建的时候，拿一次伙伴数据
            OnPartnerListUpdate(null);

            OnSceneMapConfigLoad(0);
        }

        protected override void OnClose(object arg = null)
        {
            base.OnClose(arg);
            OnCloseDialog();
            OffEventListener();
            RemoveAllChildren();
            e_PlayerStateForMusic = E_PlayerStateForMusic.Normal;
        }

        public void AddEventListener()
        {

            GlobalEvent.OnPartnerListUpdate.AddListener(OnPartnerListUpdate);
            GlobalEvent.OnMsgPartnerConcretizeRet.AddListener(OnMsgPartnerConcretizeRet);
            GlobalEvent.OnPartnerSwitchEndTimeNtf.AddListener(OnPartnerSwitchEndTimeNtf);
            //GlobalEvent.OnMsgPartnerSelectCDRet.AddListener(OnMsgPartnerSelectCDRet);
            GlobalEvent.OnPartnerDialog.AddListener(OnPartnerDialog);

            GlobalEvent.OnSceneMapConfigLoad.AddListener(OnSceneMapConfigLoad);
            GlobalEvent.onMainPlayerChanageBattleState.AddListener(onMainPlayerChanageBattleState);
        }

        public void OffEventListener()
        {
            GlobalEvent.OnPartnerListUpdate.RemoveListener(OnPartnerListUpdate);
            GlobalEvent.OnMsgPartnerConcretizeRet.RemoveListener(OnMsgPartnerConcretizeRet);
            GlobalEvent.OnPartnerSwitchEndTimeNtf.RemoveListener(OnPartnerSwitchEndTimeNtf);

            //GlobalEvent.OnMsgPartnerSelectCDRet.RemoveListener(OnMsgPartnerSelectCDRet);
            GlobalEvent.OnPartnerDialog.RemoveListener(OnPartnerDialog);

            GlobalEvent.OnSceneMapConfigLoad.RemoveListener(OnSceneMapConfigLoad);
            GlobalEvent.onMainPlayerChanageBattleState.RemoveListener(onMainPlayerChanageBattleState);
        }

        private void OnSceneMapConfigLoad(int arg0)
        {
            bool isHidePartner = GameManager.Instance.GetMapIsHidePartner();
            m_Root.alpha = isHidePartner ? 0 : 1;
            m_Root.interactable = !isHidePartner;
            m_Root.blocksRaycasts = !isHidePartner;
        }

        private void onMainPlayerChanageBattleState(E_PlayerStateForMusic arg0)
        {
            if (e_PlayerStateForMusic == arg0)
            {
                return;
            }
            e_PlayerStateForMusic = arg0;
            foreach (var item in partnerItems)
            {
                item.MainPlayerBattleShowCDAnim(e_PlayerStateForMusic == E_PlayerStateForMusic.Battle);
            }
        }

        public void OnPartnerListUpdate(object ob)
        {
            List<PartnerMD> partnerMDs = new();

            if (ob == null)
            {
                partnerMDs = PartnerManager.Instance.GetInPlayedPartners();

                //if (m_PartnerList != null)
                //{
                //    //if (m_PartnerList.Count == partnerMDs.Count && m_PartnerList.Except(partnerMDs).Count() == 0)
                //    if (m_PartnerList.Count == partnerMDs.Count && m_PartnerList.SequenceEqual(partnerMDs))
                //    {
                //        Debuger.LogWarning("PartnerManager 数据相同");
                //        return;
                //    }
                //}

                //CopyData(m_PartnerList, partnerMDs);
            }
            else
            {
                TempPartnerCreateNtf ntf = (TempPartnerCreateNtf)ob;
                foreach (var item in ntf.List)
                {
                    partnerMDs.Add(item);
                }
                //CopyData(m_PartnerList, partnerMDs);
            }

            UpdatePartners(partnerMDs);
        }

        private void CopyData(List<PartnerMD> sourceList, List<PartnerMD> copyList)
        {
            if (sourceList.Count > 0)
            {
                m_LastPartnerSeat.Clear();
                for (int i = 0; i < sourceList.Count; i++)
                {
                    var item = sourceList[i];
                    if (item != null)
                    {
                        m_LastPartnerSeat.Add(item.Index, i);
                    }
                }
            }
            sourceList.Clear();
            for (int i = 0; i < copyList.Count; i++)
            {
                sourceList.Add(copyList[i]);
            }
        }

        public void UpdatePartners(List<PartnerMD> m_PartnerList)
        {
            int count = m_PartnerList.Count;
            int childCount = partnerItems.Count;
            int maxLength = Mathf.Max(childCount, count);
            for (int i = 0; i < maxLength; i++)
            {
                PartnerMD partnerMD = count > i ? m_PartnerList[i] : null;
                if (childCount > i)
                {
                    //SGF.Debuger.LogError($"伙伴的位置 i={i},id={partnerMD.Index}");
                    UpdatePartnerItem(partnerMD, partnerItems[i], i);
                }
                //else
                //{
                //    AddPartnerItem(partnerMD);
                //}
            }
        }

        private void AddPartnerItem(PartnerMD partnerMD)
        {
            //StarProject.Service.Resource.ResourceManager.Instance.PopGameObject(prefabPath,
            //    (GameObject gob) =>
            //    {
            //        if (gob != null)
            //        {
            //            gob.transform.SetParent(parent);
            //            gob.transform.localPosition = UnityEngine.Vector3.zero;
            //            gob.transform.SetLocalScale(UnityEngine.Vector3.one);
            //            UpdatePartnerItem(partnerMD, gob);
            //            // 创建后，问一下，伙伴，有没有公共CD
            //            PartnerManager.Instance.SendPartnerSelectCDReq();
            //        }
            //    });
        }

        private void UpdatePartnerItem(PartnerMD partnerMD, PartnerItem partnerItem, int curSeat)
        {
            if (partnerItem == null)
            {
                return;
            }
            PartnerDataCell partnerDataCell = null;
            int lastSeat = -1;
            if (partnerMD != null)
            {
                if (m_LastPartnerSeat.TryGetValue(partnerMD.Index, out lastSeat))
                {
                }
                partnerDataCell = LocalDataManager.Instance.GetPartnerDataCell(partnerMD.Index);
            }
            partnerItem.UpdateData(partnerDataCell, partnerMD, AtlasPath, buffItem);
            partnerItem.SetSeat(curSeat, lastSeat);
        }

        private void RemoveAllChildren()
        {
            foreach (var item in partnerItems)
            {
                item.UpdateData(null, null, AtlasPath, buffItem);
            }
        }

        private void OnMsgPartnerConcretizeRet(PartnerConcretizeRet partnerConcretizeRet, long downPartnerIndex)
        {
            long start = partnerConcretizeRet.Start;
            long end = partnerConcretizeRet.End;
            // 现在不显示公共CD了
            /// 2023/4/20
            /// 策划 伙伴验收模块中要求:
            /// 更换伙伴CD时，所有伙伴伙伴头像出现公共CD（更换伙伴CD）
            foreach (var item in partnerItems)
            {
                item.StartSeatCD(start, end);
            }

            // 播放切换动画
            //m_Animation.Stop();
            //m_Animation.Play("PartnerChange"); 

            // var updateNode = chidldren.Find((Transform child) =>
            // {
            //     return child.GetComponent<PartnerItem>().M_PartnerID == downPartnerIndex;
            // });
            // if (updateNode == null)
            // {
            //     return;
            // }
            // long start = partnerConcretizeRet.Start;
            // long end = partnerConcretizeRet.End;
            // updateNode.GetComponent<PartnerItem>().StartSeatCD(start, end);
        }

        private void OnPartnerSwitchEndTimeNtf(PartnerSwitchEndTimeNtf partnerSwitchEndTimeNtf)
        {
            long start = 0;
            long end = partnerSwitchEndTimeNtf.EndTime;
            // SGF.Debuger.Log($"[CD_Refresh]: PartnerSwitchEndTimeNtf: {partnerSwitchEndTimeNtf}");
            foreach (var item in partnerItems)
            {
                item.StartSeatCD(start, end);
            }
        }
        private void OnPartnerDialog(int index, int dialogID, int partnerID)
        {
            OnCloseDialog();
            if (m_objDialog == null || m_txtDialog == null)
            {
                return;
            }
            if (index >= 3)
            {
                SGF.Debuger.LogError($"伙伴喊话参数index不合法 index: {index}");
                return;
            }
            CommonDialogDataCell config = LocalDataManager.Instance.GetCommonDialogDataCell(dialogID);
            if (config != null)
            {

                m_txtDialog.text = BusinessManager.Instance.ReplacePlayerName(config.Dialog_desc);
                if (index <= partnerItems.Count)
                {
                    PartnerItem curItem;
                    if (partnerID != 0)
                    {
                        curItem = FindPartnerItem(partnerID);
                    }
                    else
                    {
                        curItem = partnerItems[index - 1];
                    }
                    if (curItem == null)
                    {
                        SGF.Debuger.LogWarning($"伙伴喊话参数 index={index},dialogID={dialogID},partnerID={partnerID},partnerID没找到");
                        return;
                    }
                    //m_objDialog.SetParent(curItem.transform);
                    m_objDialog.gameObject.SetActive(true);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(m_objDialog.GetComponent<RectTransform>());
                    m_objDialog.position = curItem.ObjDialogRoot.position;

                    DelayInvoker.DelayInvoke(this, config.Show_time / 1000, OnCloseDialog);
                }
            }
            else
            {
                SGF.Debuger.LogError($"伙伴喊话参数dialogID不合法 index: {index}");
            }
        }
        private PartnerItem FindPartnerItem(int partnerID)
        {
            if (partnerItems != null)
            {
                foreach (var item in partnerItems)
                {
                    if (item.M_PartnerID == partnerID)
                    {
                        return item;
                    }
                }
            }
            return null;
        }
        private void OnCloseDialog(object[] args = null)
        {
            if (m_objDialog == null || m_txtDialog == null)
            {
                return;
            }
            if (DelayInvoker.ContainInvoke(this))
            {
                DelayInvoker.CancelInvoke(this);
            }
            //m_objDialog.SetParent(gameObject.transform);
            m_objDialog.localPosition = Vector3.zero;
            m_objDialog.gameObject.SetActive(false);

        }
    }
}
