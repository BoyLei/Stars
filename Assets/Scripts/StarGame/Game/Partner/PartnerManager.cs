using ProtoMsg;
using SGF.Network;
using SGF.UI.Framework;
using StarProject;
using StarProject.Game.Player;
using StarProjectDef;
using System.Collections.Generic;
using static SGF.Network.FixMessageManager;

namespace SGF.Module.Framework
{
    public class PartnerManager : ServiceModule<PartnerManager>
    {
        protected string LOG_TAG = "PartnerManager";

        public Dictionary<long, PartnerMD> PartnerDic = new();

        private List<PartnerMD> InBattlePartners = new();

        public PartnerCtrlGroup CurConcretizationPartner = null;  // 记录一个当前出战的伙伴

        public void Init()
        {
            CheckSingleton();
            AddEventListener();
        }

        public void Reset()
        {
            PartnerDic.Clear();
            InBattlePartners.Clear();
            CurConcretizationPartner = null;
        }

        public override void Release()
        {
            base.Release();
            OffEventListener();
        }

        private void AddEventListener()
        {
            //NetworkManager.Instance.OnMessageEnum(MsgIDEnum.PartnerBattleRetID, OnPartnerBattleRet, this);
            //NetworkManager.Instance.OnMessageEnum(MsgIDEnum.PartnerAssistRetID, OnPartnerAssistRet, this);
            //NetworkManager.Instance.OnMessageEnum(MsgIDEnum.PartnerFallRetID, OnPartnerFallRet, this);
            //NetworkManager.Instance.OnMessageEnum(MsgIDEnum.AllPartnerListID, OnAllPartnerList, this);
            //NetworkManager.Instance.OnMessageEnum(MsgIDEnum.NoticePartnerListID, OnNoticePartnerList, this);
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.PartnerConcretizeRetID, OnPartnerConcretizeRet, this);
            //NetworkManager.Instance.OnMessageEnum(MsgIDEnum.PartnerSelectCDRetID, OnPartnerSelectCDRet, this);
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.TempPartnerCreateNtfID, OnTempPartnerCreateNtf, this);
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.PartnerSwitchEndTimeNtfID, OnPartnerSwitchEndTimeNtf, this);

            FixMessageManager.Instance.OnMessage(FixUpdateDef.Partner, OnPartnerRefesh);

            GlobalEvent.OnPartnerCreate.AddListener(OnPartnerCtrlBaseCreate);
            GlobalEvent.OnPartnerLeave.AddListener(OnPartnerLeave);
        }

        private void OffEventListener()
        {
            //NetworkManager.Instance.OffMessageEnum(MsgIDEnum.PartnerBattleRetID, OnPartnerBattleRet, this);
            //NetworkManager.Instance.OffMessageEnum(MsgIDEnum.PartnerAssistRetID, OnPartnerAssistRet, this);
            //NetworkManager.Instance.OffMessageEnum(MsgIDEnum.PartnerFallRetID, OnPartnerFallRet, this);
            //NetworkManager.Instance.OffMessageEnum(MsgIDEnum.AllPartnerListID, OnAllPartnerList, this);
            //NetworkManager.Instance.OffMessageEnum(MsgIDEnum.NoticePartnerListID, OnNoticePartnerList, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.PartnerConcretizeRetID, OnPartnerConcretizeRet, this);//
                                                                                                                   //NetworkManager.Instance.OffMessageEnum(MsgIDEnum.PartnerSelectCDRetID, OnPartnerSelectCDRet, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.TempPartnerCreateNtfID, OnTempPartnerCreateNtf, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.PartnerSwitchEndTimeNtfID, OnPartnerSwitchEndTimeNtf, this);

            FixMessageManager.Instance.OffMessage(FixUpdateDef.Partner, OnPartnerRefesh);

            GlobalEvent.OnPartnerCreate.RemoveListener(OnPartnerCtrlBaseCreate);
            GlobalEvent.OnPartnerLeave.RemoveListener(OnPartnerLeave);
        }

        #region  伙伴相关协议 (日, kl 说伙伴的协议不走 rpc,它们是 系统层协议 )

        private void OnPartnerRefesh(FixMessageNotifyData data)
        {
            bool isChanage = false;
            //修改的
            for (int i = 0; i < data.Changes.Count; i++)
            {
                var item = (PartnerMD)data.Changes[i];
                bool isPlayed = item.State == PartnerState.InBattle || item.State == PartnerState.Concretization;
                if (isPlayed)
                {
                    if (!PartnerDic.ContainsKey(item.Index))
                    {
                        PartnerDic.Add(item.Index, item);
                    }
                    else
                    {
                        PartnerDic[item.Index] = item;
                    }
                    isChanage = true;
                }
                else
                {
                    if (PartnerDic.ContainsKey(item.Index))
                    {
                        PartnerDic.Remove(item.Index);
                        isChanage = true;
                    }
                }
            }
            // 删
            for (int i = 0; i < data.Delets.Count; i++)
            {
                var item = (PartnerMD)data.Delets[i];
                if (PartnerDic.ContainsKey(item.Index))
                {
                    PartnerDic.Remove(item.Index);
                    isChanage = true;
                }
            }

            if (isChanage)
            {
                GlobalEvent.OnPartnerListUpdate.Invoke(null);
            }
            else
            {
                //SGF.Debuger.LogError($"伙伴血量 没有改变");
            }
        }

        /// <summary>
        /// 伙伴出战 s回复
        /// </summary>
        //private void OnPartnerBattleRet(MessageHandleData data)
        //{
        //    PartnerBattleRet partnerBattleRet = (PartnerBattleRet)data.data;

        //    //SGF.Debuger.LogError($"{LOG_TAG}  OnPartnerBattleRet  ID {partnerBattleRet.ID} ");

        //    if (partnerBattleRet.Ret != PartnerOperate.OpOk)
        //    {
        //        Frame.Util.ShowBattleMessage($"PartnerBattleRet result : {partnerBattleRet.Ret}");
        //        return;
        //    }
        //    //// 伙伴出战的 更新状态为 Concretization
        //    //int id = partnerBattleRet.ID;
        //    //if (PartnerDic.ContainsKey(id))
        //    //{
        //    //    PartnerDic[id].State = PartnerState.Concretization;
        //    //    PartnerDic[id].CurSeat = partnerBattleRet.Seat;
        //    //}

        //    //// 被替换的伙伴, 更新状态为 默认状态
        //    //int replaceId = partnerBattleRet.ReplaceSeat;
        //    //if (PartnerDic.ContainsKey(replaceId))
        //    //{
        //    //    PartnerDic[replaceId].State = PartnerState.DefaultState;
        //    //    PartnerDic[replaceId].CurSeat = partnerBattleRet.ReplaceSeat;
        //    //}

        //    //GlobalEvent.OnPartnerListUpdate.Invoke(null);
        //}

        ///// <summary>
        ///// 伙伴助战 s回复
        ///// </summary>
        //private void OnPartnerAssistRet(MessageHandleData data)
        //{
        //    PartnerAssistRet partnerAssistRet = (PartnerAssistRet)data.data;

        //    //SGF.Debuger.LogError($"{LOG_TAG}  OnPartnerAssistRet  ID {partnerAssistRet.ID} ");

        //    if (partnerAssistRet.Ret != PartnerOperate.OpOk)
        //    {
        //        Frame.Util.ShowBattleMessage($"PartnerAssistRet result : {partnerAssistRet.Ret}");
        //        return;
        //    }
        //    //// 伙伴助战的 更新状态为 Assist
        //    //int id = partnerAssistRet.ID;
        //    //if (PartnerDic.ContainsKey(id))
        //    //{
        //    //    PartnerDic[id].State = PartnerState.Assist;
        //    //    PartnerDic[id].CurSeat = partnerAssistRet.Seat;
        //    //}

        //    //// 被替换的伙伴, 更新状态为 默认状态
        //    //int replaceId = partnerAssistRet.Seat; // ReplaceSeat
        //    //if (PartnerDic.ContainsKey(replaceId))
        //    //{
        //    //    PartnerDic[replaceId].State = PartnerState.DefaultState;
        //    //    PartnerDic[replaceId].CurSeat = partnerAssistRet.Seat; // ReplaceSeat
        //    //}

        //    //GlobalEvent.OnMsgPartnerAssistRet.Invoke(partnerAssistRet);
        //}

        ///// <summary>
        ///// 伙伴下阵 s回复
        ///// </summary>
        //private void OnPartnerFallRet(MessageHandleData data)
        //{
        //    PartnerFallRet partnerFallRet = (PartnerFallRet)data.data;

        //    //SGF.Debuger.LogError($"{LOG_TAG}  OnPartnerFallRet  ID {partnerFallRet.ID} ");

        //    if (partnerFallRet.Ret != PartnerOperate.OpOk)
        //    {
        //        Frame.Util.ShowBattleMessage($"PartnerFallRet result : {partnerFallRet.Ret}");
        //        return;
        //    }
        //    //// 伙伴下阵的 更新状态为 Assist
        //    //int id = partnerFallRet.ID;
        //    //if (PartnerDic.ContainsKey(id))
        //    //{
        //    //    PartnerDic[id].State = PartnerState.DefaultState;
        //    //}

        //    //GlobalEvent.OnMsgPartnerFallRet.Invoke(partnerFallRet);
        //    //GlobalEvent.OnPartnerListUpdate.Invoke(null);
        //}

        ///// <summary>
        ///// 伙伴全同步时的消息
        ///// </summary>
        //private void OnAllPartnerList(MessageHandleData data)
        //{
        //    AllPartnerList allPartnerList = (AllPartnerList)data.data;

        //    SGF.Debuger.Log($"{LOG_TAG}  OnAllPartnerList  {allPartnerList.Pars.Count} ");

        //    PartnerDic.Clear();

        //    RepeatedField<PartnerMD> pars = allPartnerList.Pars;
        //    for (int i = 0; i < pars.Count; i++)
        //    {
        //        PartnerDic.Add(pars[i].Index, pars[i]);
        //    }

        //    GlobalEvent.OnPartnerListUpdate.Invoke(null);
        //}

        /// <summary>
        /// 伙伴脏数据
        /// </summary>
        //private void OnNoticePartnerList(MessageHandleData data)
        //{
        //    NoticePartnerList noticePartnerList = (NoticePartnerList)data.data;

        //    SGF.Debuger.Log($"{LOG_TAG}  OnNoticePartnerList  {noticePartnerList.Pars.Count} ");
        //    //PartnerMD concretizationPartner = null;

        //    RepeatedField<PartnerMD> pars = noticePartnerList.Pars;
        //    for (int i = 0; i < pars.Count; i++)
        //    {
        //        PartnerMD partner = pars[i];
        //        long index = partner.Index;
        //        if (PartnerDic.ContainsKey(index))
        //        {
        //            // 同步当前出战伙伴数据 如果是星级发生了变化
        //            // 通知实体层 获取最新的技能id
        //            //if (partner.State == PartnerState.Concretization)
        //            //{
        //            //    PartnerMD last = PartnerDic[index];
        //            //    if (partner.CurStar > last.CurStar)
        //            //    {
        //            //        concretizationPartner = partner;
        //            //    }
        //            //}
        //            PartnerDic[index] = partner;
        //        }
        //        else
        //        {
        //            PartnerDic.Add(index, partner);
        //        }
        //    }

        //    GlobalEvent.OnPartnerListUpdate.Invoke(null);
        //    // 如果有当前出战的伙伴属性更新的话，就通知伙伴实体更新伙伴技能
        //    //if (concretizationPartner != null && CurConcretizationPartner != null && CurConcretizationPartner.M_Curr != null)
        //    //{
        //    //    int partnerId = (int)CurConcretizationPartner.M_Curr.ConfigIndex;
        //    //    if (partnerId == concretizationPartner.Index)
        //    //    {
        //    //        UpdatePartnerSkill(CurConcretizationPartner, concretizationPartner.CurStar);
        //    //    }
        //    //}
        //}

        private void OnTempPartnerCreateNtf(MessageHandleData data)
        {
            TempPartnerCreateNtf ntf = (TempPartnerCreateNtf)data.data;
            if (ntf != null)
            {
                GlobalEvent.OnPartnerListUpdate.Invoke(ntf);
            }
        }

        private void OnPartnerSwitchEndTimeNtf(MessageHandleData data)
        {
            PartnerSwitchEndTimeNtf ntf = (PartnerSwitchEndTimeNtf)data.data;
            if (ntf == null)
            {
                return;
            }

            GlobalEvent.OnPartnerSwitchEndTimeNtf.Invoke(ntf);
        }
        /// <summary>
        /// 请求切换上阵回复 - 只有上阵的才能释放伙伴技能
        /// </summary>
        private void OnPartnerConcretizeRet(MessageHandleData data)
        {
            PartnerConcretizeRet partnerConcretizeRet = (PartnerConcretizeRet)data.data;

            //SGF.Debuger.LogError($"{LOG_TAG}  OnPartnerConcretizeRet  {partnerConcretizeRet.Index} , result : {partnerConcretizeRet.Ret} ");

            if (partnerConcretizeRet.Ret != PartnerOperate.OpOk)
            {
                //Frame.Util.ShowBattleMessage($"PartnerConcretizeRet result : {partnerConcretizeRet.Ret}");
                return;
            }
            //PartnerMD downPartnerMD = null;
            //// 下阵的 伙伴  先恢复状态
            //foreach (KeyValuePair<int, PartnerMD> item in PartnerDic)
            //{
            //    if (item.Value.State == PartnerState.Concretization)
            //    {
            //        item.Value.State = PartnerState.InBattle;
            //        downPartnerMD = item.Value;
            //    }
            //}
            //foreach (KeyValuePair<int, PartnerMD> item in PartnerDic)
            //{
            //    if (partnerConcretizeRet.Index == item.Key)
            //    {
            //        item.Value.State = PartnerState.Concretization;
            //        downPartnerMD = item.Value;
            //    }
            //}
            //GlobalEvent.OnPartnerListUpdate.Invoke(null);
            GlobalEvent.OnMsgPartnerConcretizeRet.Invoke(partnerConcretizeRet, partnerConcretizeRet.Index);
            //UIManager.Instance.OpenWidget(UIDef.PartnerAppearedWidget, true, partnerConcretizeRet.Index, null, MainPageCommond.HideNone, false, true);
            UIManager.Instance.OpenWidgetAsync(UIDef.PartnerAppearedWidget, null, true, partnerConcretizeRet.Index, null, MainPageCommond.HideNone, false, true);

            //EntityCtrlBase entityCtrlBase = GameManager.Instance.GetMainPlayerParterByIndex((int)partnerConcretizeRet.Index);
            //if (entityCtrlBase == null)
            //{
            //    return;
            //}

            //UpdatePartnerSkill(entityCtrlBase, (int)partnerConcretizeRet.Start);
        }

        ///// <summary>
        ///// 伙伴切换cd
        ///// </summary>
        //private void OnPartnerSelectCDRet(MessageHandleData data)
        //{
        //    PartnerSelectCDRet partnerSelectCDRet = (PartnerSelectCDRet)data.data;

        //    //SGF.Debuger.LogError($"{LOG_TAG}  OnPartnerSelectCDRet start: {partnerSelectCDRet.Start} , end: {partnerSelectCDRet.End} , CD : {partnerSelectCDRet.End - partnerSelectCDRet.Start} ms");

        //    GlobalEvent.OnMsgPartnerSelectCDRet.Invoke(partnerSelectCDRet);
        //}

        /////////////////----------------------------------------------------
        /////////////////----------------------------------------------------

        /// <summary>
        /// 收到伙伴 创建的时候, 更新技能槽按钮
        /// note:
        ///     目前此处的逻辑处理 有点奇怪, 是因为 技能槽的按钮绑定了 EntityBaseCtrlGroup。
        ///     而伙伴的 创建, 走的是单独的 AOI实体 创建, 在主角伙伴数据创建的时候, 伙伴实体并不一定创建.
        ///     所以此处只能 绕一圈, 转而监听 伙伴实体的创建, 去刷新 技能按钮的显示. 
        /// note1:
        ///     后续  如果曲 能够做到将 技能按钮和 EntityBaseCtrlGroup 解耦, 那伙伴的技能按钮
        ///     的创建只需要依赖 技能数据即可
        /// </summary>
        /// <param name="partnerCtrlGroup"></param>
        private void OnPartnerCtrlBaseCreate(EntityCtrlBase entityCtrlBase)
        {
            int index = (int)entityCtrlBase.M_Curr.ConfigIndex;
            if (!PartnerDic.ContainsKey(index))
            {
                return;
            }
            PartnerMD partner = PartnerDic[index];
            // 如果不是 出战状态
            if (partner.State != PartnerState.Concretization)
            {
                return;
            }
            CurConcretizationPartner = entityCtrlBase as PartnerCtrlGroup;
            //UpdatePartnerSkill(entityCtrlBase, partner.CurStar);
        }

        private void OnPartnerLeave(EntityCtrlBase entityCtrlBase)
        {
            //int index = (int)entityCtrlBase.M_Curr.ConfigIndex;
            //if (!PartnerDic.ContainsKey(index))
            //{
            //    return;
            //}
            //PartnerMD partner = PartnerDic[index];
            //// 如果不是 出战状态
            //if (partner.State != PartnerState.Concretization)
            //{
            //    return;
            //}
            // 伙伴删除服务器通知的顺序是
            // 1.脏数据-》伙伴AOI离开 （a伙伴的离开和b伙伴的进入【有可能会是一帧内的网络数据】）
            if (entityCtrlBase == null)
            {
                return;
            }
            if (entityCtrlBase.Data == null)
            {
                return;
            }
            if (CurConcretizationPartner == null)
            {
                return;
            }
            if (CurConcretizationPartner.Data == null)
            {
                return;
            }
            if (entityCtrlBase.Data.M_EntityID == CurConcretizationPartner.Data.M_EntityID)
            {
                CurConcretizationPartner = null;
            }
        }

        //private void UpdatePartnerSkill(EntityCtrlBase entityCtrlBase, int starNum)
        //{
        //    //SGF.Debuger.LogError($"伙伴数据  更新伙伴的技能");

        //    //GlobalEvent.OnPartnerUpdate.Invoke(entityCtrlBase, starNum);
        //}

        #endregion

        /// <summary>
        /// 1.玩家在任何状态下，主动撤下已经在场上战斗的伙伴，系统都不会默认将未上阵伙伴自动加入战斗
        ///2.若玩家在无佣兵状态下，获得了新的佣兵，则默认为“待上阵”状态，即在右侧“待上阵”栏，
        ///玩家至多3名佣兵，都会处于待上阵状态，都需要玩家点击后上阵
        ///此时，若玩家撤下任何佣兵，则在客户端表现上，会向下掉落填充
        ///3.若玩家无已经在战场上的佣兵，且此时，玩家在“待上阵”列表中的佣兵，为小于等于2，则此时，客户端会锁定“待上阵”列表的位置，不会进行掉落填充懂抓
        ///  获得出战的 伙伴
        /// </summary>
        public List<PartnerMD> GetInPlayedPartners()
        {
            List<PartnerMD> newInPlayedPartners = new();    // 新出战的伙伴
            PartnerMD concretizationData = null;    // 新具象化出站的伙伴
            foreach (KeyValuePair<long, PartnerMD> item in PartnerDic)
            {
                PartnerMD partner = item.Value;
                // 【0】0号位置一定是具象化出站的
                // 伙伴列表排序，根据具象化出站》出站
                if (partner.State == PartnerState.Concretization)
                {
                    concretizationData = item.Value;
                    newInPlayedPartners.Insert(0, item.Value);
                }
                else if (partner.State == PartnerState.InBattle)
                {
                    newInPlayedPartners.Add(item.Value);
                }
            }
            // 如果没有具象化出战的伙伴，那就把第一空出来，给个null
            if (concretizationData == null && newInPlayedPartners.Count < 3)
            {
                newInPlayedPartners.Insert(0, null);
            }
            //InBattlePartners.Sort((PartnerMD partner1, PartnerMD partner2) =>
            //{
            //    return partner1.CurSeat - partner2.CurSeat;
            //});
            bool isSame = GetBattleListIsSame(newInPlayedPartners, InBattlePartners);
            if (isSame && concretizationData != null)
            {
                // 相同，只是换了位置
                // 查找元素所在的索引
                int oldIndex = InBattlePartners.FindIndex((item) =>
                {
                    return item.Index == concretizationData.Index;
                });
                foreach (var item in newInPlayedPartners)
                {
                    int index = InBattlePartners.FindIndex((child) =>
                   {
                       return child.Index == item.Index;
                   });
                    if (index != -1)
                    {
                        InBattlePartners[index] = item;
                    }
                }
                InBattlePartners = RotateList(InBattlePartners, oldIndex);
            }
            else
            {
                // 不相同
                InBattlePartners = newInPlayedPartners;
            }

            return InBattlePartners;
        }

        private List<PartnerMD> RotateList(List<PartnerMD> data, int count)
        {
            int actualCount = count % data.Count;

            // 创建一个新的列表，将滚动后的元素依次添加进去
            List<PartnerMD> rotatedData = new();
            for (int i = actualCount; i < data.Count; i++)
            {
                rotatedData.Add(data[i]);
            }
            for (int i = 0; i < actualCount; i++)
            {
                rotatedData.Add(data[i]);
            }

            return rotatedData;
        }

        // 获得俩个列表是否相同
        private bool GetBattleListIsSame(List<PartnerMD> newList, List<PartnerMD> oldList)
        {
            bool isSame = newList.Count == oldList.Count;
            if (!isSame)
            {
                return isSame;
            }

            foreach (var item in newList)
            {
                PartnerMD partnerMD = oldList.Find((item1) =>
                {
                    return item?.Index == item1?.Index;
                });
                if (partnerMD == null)
                {
                    isSame = false;
                    break;
                }
            }

            return isSame;
        }

        public void SendPartnerSelectCDReq()
        {
            PartnerSelectCDReq partnerSelectCDReq = new();
            NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeLobby, partnerSelectCDReq, false);
        }

        public int GetPartnerStarById(int parId)
        {
            int star = 0;

            if (PartnerDic != null)
            {
                if (PartnerDic.ContainsKey(parId))
                {
                    star = PartnerDic[parId].CurStar;
                }
            }

            return star;
        }

    }
}
