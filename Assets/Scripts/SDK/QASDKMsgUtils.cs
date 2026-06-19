using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SGF.Network;
using System;
using Fire;
using ProtoMsg;
using StarProject.Service.LocalData;
using StarProjectDef;
using System.Reflection;
using Google.Protobuf.Collections;

public static class QASDKMsgUtils
{
    public static string PROP = "prop";

    /// <summary>
    /// 将 pb 的数据 解析 为 QA 需要的 解开的 json 结构
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static string ConvertPB2QAJson(IMessage message)
    {

        string jsonStr = JsonFormatter.Default.Format(message);

        JObject jOb = ((JObject)JsonConvert.DeserializeObject(jsonStr));

        string tag = message.Descriptor.Name;
        // Debug.Log($"message: {tag} ,1 parse  --> json: {jOb} ");
        Serialize_BlackListJson(jOb, tag);

        try
        {
            Serialize_SyncBaseInfoJson(message, jOb, tag);
        }
        catch (System.Exception e)
        {
            Debug.Log($"ConvertPB2QAJson message: {tag} ,1 parse  --> json: {jOb},e={e} error ， 解析失败");
        }


        // Debug.Log($"message: {tag} ,2 --> json: {jOb}");
        return jOb.ToString();
    }

    public static JObject ConvertPB2QAJsonObject(IMessage message)
    {

        string jsonStr = JsonFormatter.Default.Format(message);

        JObject jOb = ((JObject)JsonConvert.DeserializeObject(jsonStr));

        string tag = message.Descriptor.Name;
        // Debug.Log($"message: {tag} ,1 parse  --> json: {jOb} ");
        Serialize_BlackListJson(jOb, tag);


        Serialize_SyncBaseInfoJson(message, jOb, tag);

        // Debug.Log($"message: {tag} ,2 --> json: {jOb}");
        return jOb;
    }

    public static QAProto ConvertQAJson2PB(string qAJson)
    {
        QAProto qAProto = JsonConvert.DeserializeObject<QAProto>(qAJson);

        // 首先找出 pb 消息的 json 字段
        string message = qAProto.messageJson.ToString();

        // 然后找出 message 用什么pb 结构来解
        int cmd = qAProto.CMD;

        ProtoInfo? protoInfo = ProtoDic.Instance.GetProtoInfoByCmd(cmd);
        if (!protoInfo.HasValue)
        {
            return null;
        }

        var messageName = protoInfo.Value.Name;

        try
        {
            // 将 message json字符串 转成 json 的 jobject 结构
            JObject jOb = ((JObject)JsonConvert.DeserializeObject(message));

            DeSerializeQAMessage(jOb, messageName);
            // qAProto.message = jOb.ToString();
            qAProto.messageJson = jOb;
        }
        catch (System.Exception e)
        {
            SGF.Debuger.LogError($"[QASDK] message 解析 qaSDK json 报错: messageName {messageName} , json: {qAJson}");
            SGF.Debuger.LogError($"[QASDK] message 解析 qaSDK json 报错: e : {e.Message}");
        }

        return qAProto;
    }

    private static void DeSerializeQAMessage(JObject jOb, string pbName)
    {
        if (jOb == null)
        {
            return;
        }

        DeSerialize_BlackListJson(jOb, pbName);

        Deserialize_SyncBaseInfoJson(jOb, pbName);

    }

    public static void Send2QAMsg(QAProto qAProto)
    {
        // return;
        string msgType = qAProto.IsClientSend ? "m" : "n";
        int opcode = qAProto.CMD;
        string msg = qAProto.ToJsonString();
        string protoName = qAProto.protoName;
        // Debug.Log($"message:  发送到 QASDK qAProto: {qAProto}");
        var now = SGF.Time.TimeUtils.ClientNowStampMilli;
        QASDK.QASDK_StarsClient.GetInstance().PushQAProtoData(msgType, opcode, msg, protoName);
        // Debug.Log($"message:  发送到 QASDK qAProto: {qAProto} cost: {SGF.Time.TimeUtils.ClientNowStampMilli - now} ms");

    }

    public static void SendQAMsg2Server(string json)
    {
        // #if QA_DEBUG
        if (!GMTestData.CheckGmIsOpen("OpenQASDK"))
        {
            return;
        }

        try
        {
            QAProto qaProto = QASDKMsgUtils.ConvertQAJson2PB(json);
            // Debug.Log($"message:  收到 QASDK json: {json}");
            // Debug.Log($"message:  转换为 qaProto: {qaProto.ToJsonString()}");

            IMessage message = qaProto.GetMessage();
            if (message == null)
            {
                return;
            }
            NetworkManager.Instance.gameSocket.SendRPCMsg(qaProto.SendServerType, message, qaProto.IsEncrypt, qaProto.OneOfCtrlEntityId, qaProto.IsAutoChangeMsgTarget, false);

        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.Log($"[QASDK] 发送 转换 json {json} 转换失败  ， 解析失败 , e: {e.Message}");
        }

        // Debug.Log($"message: 发送 转换 qaProto 的 message: {message.Descriptor.Name} , messageJson: {JsonFormatter.Default.Format(message)}");
        // #endif

    }

    #region 将pb 的 Serialize 序列化 为 json 结构的部分 
    /// <summary>
    /// 解析 json 中 BlackList 字段 的 包含了 RawValue的二进制 json , 将它 转换成 实际解出 后的结构.
    /// note:
    ///     找的 是 BlackList.  如果改命名  就会失效.
    /// </summary>
    /// <param name="jObject"></param>
    /// <param name="tag"></param>
    public static void Serialize_BlackListJson(JObject jObject, string tag)
    {
        JArray jBlackList = (JArray)jObject.GetValue("BlackList");
        if (jBlackList == null || jBlackList.Count == 0)
        {
            return;
        }

        // Debug.Log($"message: {tag} ,1 parse [BlackList]  --> json: {jObject} ");

        foreach (JObject item in jBlackList)
        {
            // 先检查 这个黑板中 是否包含 二进制 属性的 json 数据, 如果不包含, 这个黑板数据就不需要继续解
            JObject rawValue = item.GetValue("RawValue") as JObject;
            if (rawValue == null)
            {
                continue;
            }

            JValue msgValue = rawValue.GetValue("MsgValue") as JValue;
            // 如果 msgValue中 没有任何 二进制数据, 那就不需要继续解
            if (msgValue == null || msgValue.ToString() == "")
            {
                continue;
            }

            // 日, 直接通过 msgValue 的字符串 无法转换 成 跟 pb结构一致的 byteSting. 
            //  Google.Protobuf.ByteString.CopyFromUtf8(msgValue.ToString());
            // JValue msgValue = (JValue)rawValue.GetValue("MsgValue");

            // pb 转换出来的 json 字符 还是需要 先 通过 ParseJson 的方式转换为 原始的pb 结构才行.
            IMessage bbMessage = ProtoMsg.BlackBoardNode.Descriptor.Parser.ParseJson(item.ToString());

            if (bbMessage == null)
            {
                continue;
            }

            BlackBoardNode bbNode = (ProtoMsg.BlackBoardNode)bbMessage;

            // 将这个黑板数据 的 二进制 结构 再次 解开,注意 此处 如果bbNode的二进制数据为 空,接出来的也是默认的 {}
            IMessage pbMessage = ProtoUtils.DeserializeBlackBoardCommon<IMessage>(bbNode);

            if (pbMessage == null)
            {
                continue;
            }

            // 将 rawMsg 序列化 后的 结构 转成对应的 json 字符串, 替换 一开始的 MsgValue 结构数据
            string msgStr = ConvertPB2QAJson(pbMessage);

            JObject replaceJ = (JObject)JsonConvert.DeserializeObject(msgStr);
            rawValue["MsgValue"] = replaceJ;

        }


    }




    /// <summary>
    /// 将 SyncBaseInfo 相关的 pb 结构 序列化 为 json 结构
    /// </summary>
    /// <param name="message"></param>
    /// <param name="jObject"></param>
    /// <param name="tag"></param>
    public static void Serialize_SyncBaseInfoJson(IMessage message, JObject jObject, string tag)
    {
        // 如果传入的message 就是 SyncBaseInfo 的结构, 那直接解就可以
        if (tag == ProtoMsg.SyncBaseInfo.Descriptor.Name)
        {
            Serialize_SyncBaseInfo(message, jObject);
            return;
        }
        // 如果 是 PropBaseSyncList 结构,那就先 按 P
        if (tag == ProtoMsg.PropBaseSyncList.Descriptor.Name)
        {
            Serialize_PropBaseSyncList(message, jObject);
            return;
        }

        if (tag == ProtoMsg.PropSyncList.Descriptor.Name)
        {
            Serialize_PropSyncList(message, jObject);
            return;
        }



        if (tag == ProtoMsg.PropPanelRet.Descriptor.Name)
        {
            Serialize_PropPanelRet(message, jObject);
            return;
        }

        //if (tag == ProtoMsg.ItemPropSyncList.Descriptor.Name)
        //{
        //    Serialize_ItemPropSyncList(message, jObject);
        //    return;
        //}

        //if (tag == ProtoMsg.ItemSpaceSyncList.Descriptor.Name)
        //{
        //    Serialize_ItemSpaceSyncList(message, jObject);
        //    return;
        //}


        if (tag == ProtoMsg.RepeatedPropSyncList.Descriptor.Name)
        {
            Serialize_RepeatedPropSyncList(message, jObject);
            return;
        }

        if (tag == ProtoMsg.UserMainDataNotify.Descriptor.Name)
        {
            Serialize_UserMainDataNotify(message, jObject);
            return;
        }

        if (tag == ProtoMsg.EnterAOI.Descriptor.Name)
        {
            Serialize_EnterAOI(message, jObject);
            return;
        }

        if (tag == ProtoMsg.UpdateAOI.Descriptor.Name)
        {
            Serialize_UpdateAOI(message, jObject);
            return;
        }

        if (tag == ProtoMsg.AOIMsg.Descriptor.Name)
        {
            Serialize_AOIMsg(message, jObject);
            return;
        }

        if (jObject.ToString().Contains(PROP))
        {
            SGF.Debuger.LogError($"message: 属性 解析 漏了 : {tag} !!!!");
#if UNITY_EDITOR
            //Debug.Break();
#endif
        }
        return;

        // 因为不确定以后是否还会有新的 proto 结构 包含 prop 结构,所以 此处先用 字符串查找的方式
        // 好处是 可以一劳永逸， 可以处理各种 包含 PROP 字段的 proto结构;
        // 缺点是 需要 服务器保证 字符串大小写 一致
        //if (jObject.ToString().Contains(PROP))
        //{
        //    foreach (KeyValuePair<string, JToken?> item in jObject)
        //    {
        //        // 先判断 key 是不是就是 PROP, 如果是的话,就需要 判断 是单个的 信息还是 List信息
        //        if (item.Key == PROP)
        //        {

        //        }
        //        else
        //        {
        //            // item.Value
        //        }


        //    }
        //}


    }

    /// <summary>
    /// 解析 单个 的 SyncBaseInfo
    /// </summary>
    /// <param name="message"></param>
    /// <param name="jObject"></param>
    public static void Serialize_SyncBaseInfo(IMessage message, JObject jObject)
    {
        var syncBaseInfo = (SyncBaseInfo)message;

        VitalSignAOIClientAttrs vitalSignAOIClientAttrs = LocalDataManager.Instance.GetAttrPropByAttrIdx((ushort)syncBaseInfo.Index);

        if (vitalSignAOIClientAttrs == null)
        {
            return;
        }
        string key = vitalSignAOIClientAttrs.Name;

        // 如果 包含了二进制的数据,就对这个 做二进制的解析
        if (syncBaseInfo.PropValueCase != SyncBaseInfo.PropValueOneofCase.MsgValue)
        {
            return;
        }

        // TODO: 曲
        // 需要跟服务器沟通 是否需要将 属性 二进制的结构 转变为 统一的 RawMsg 格式
        // note:
        //     属性的二进制 数据中并不包含 msgID 的信息, 而是根据约定的方式 单独定义 不同的属性用不同的结构来解
        //     这样在 此处 多层解的 时候, 就需要特殊处理, 
        // PropertyInfo v = syncBaseInfo.GetType().GetProperty(syncBaseInfo.PropValueCase.ToString());

        ByteString value = syncBaseInfo.MsgValue;
        byte[] msgData = value.ToByteArray();

        if (msgData.Length == 0)
        {
            return;
        }
        IMessage pbMessage = null;

        switch (key)
        {
            case AOIAttrDefine.Position:
                {
                    pbMessage = ProtoUtils.Deserialize((int)MsgIDEnum.Vector3ID, msgData);
                }
                break;
            case AOIAttrDefine.StorageDrugs:
                {
                    pbMessage = ProtoUtils.Deserialize((int)MsgIDEnum.StorageDrugsMdID, msgData);
                }
                break;
            case AOIAttrDefine.ExAmuletInfo:
                {
                    pbMessage = ProtoUtils.Deserialize((int)MsgIDEnum.ExtractaAmuletMDID, msgData);
                }
                break;
            /*
        case AOIAttrDefine.GVEBonus:
            {
                pbMessage = ProtoUtils.Deserialize( (int) MsgIDEnum.GVEBonusID, msgData);
            }
            break;
            */
            default: break;
        }
        if (pbMessage == null)
        {
            return;
        }
        // 将 rawMsg 序列化 后的 结构 转成对应的 json 字符串, 替换 一开始的 MsgValue 结构数据
        string msgStr = ConvertPB2QAJson(pbMessage);

        JObject replaceJ = (JObject)JsonConvert.DeserializeObject(msgStr);
        jObject["MsgValue"] = replaceJ;
    }



    public static void Serialize_SyncBaseInfoList(RepeatedField<SyncBaseInfo> prop, JArray jProp)
    {
        if (prop.Count == 0)
        {
            return;
        }

        for (int i = 0; i < prop.Count; i++)
        {
            JToken pJObject = jProp[i];
            SyncBaseInfo pItem = prop[i];
            Serialize_SyncBaseInfo(pItem, (JObject)pJObject);
        }
    }

    public static void Serialize_PropBaseSyncList(IMessage message, JObject jObject)
    {
        PropBaseSyncList propBaseSyncList = (PropBaseSyncList)message;
        RepeatedField<SyncBaseInfo> prop = propBaseSyncList.Prop;
        if (prop == null || prop.Count == 0)
        {
            return;
        }

        JArray jProp = jObject.GetValue(PROP) as JArray;
        if (jProp == null || jProp.Count == 0)
        {
            return;
        }

        Serialize_SyncBaseInfoList(prop, jProp);
    }

    public static void Serialize_PropSyncList(IMessage message, JObject jObject)
    {
        PropSyncList propSyncList = (PropSyncList)message;
        PropBaseSyncList prop = propSyncList.Prop;
        if (prop == null)
        {
            return;
        }

        JObject jProp = jObject.GetValue(PROP) as JObject;
        if (jProp == null)
        {
            return;
        }

        Serialize_PropBaseSyncList(prop, jProp);
    }

    public static void Serialize_PropPanelRet(IMessage message, JObject jObject)
    {
        PropPanelRet propPanelRet = (PropPanelRet)message;
        RepeatedField<SyncBaseInfo> prop = propPanelRet.Prop;
        if (prop == null || prop.Count == 0)
        {
            return;
        }

        JArray jProp = jObject.GetValue(PROP) as JArray;

        if (jProp == null || jProp.Count == 0)
        {
            return;
        }

        Serialize_SyncBaseInfoList(prop, jProp);
    }

    //public static void Serialize_ItemPropSyncList(IMessage message, JObject jObject)
    //{
    //    ItemPropSyncList data = (ItemPropSyncList)message;
    //    PropBaseSyncList prop = data.Prop;
    //    if (prop == null)
    //    {
    //        return;
    //    }

    //    var jProp = jObject.GetValue(PROP) as JObject;

    //    if (jProp == null)
    //    {
    //        return;
    //    }
    //    Serialize_PropBaseSyncList(prop, jProp);
    //}


    //public static void Serialize_ItemSpaceSyncList(IMessage message, JObject jObject)
    //{
    //    //ItemSpaceSyncList data = (ItemSpaceSyncList)message;
    //    var prop = data.ItemInfo;

    //    if (prop.Count == 0)
    //    {
    //        return;
    //    }

    //    JArray jProp = jObject.GetValue("ItemInfo") as JArray;
    //    if (jProp == null || jProp.Count == 0)
    //    {
    //        return;
    //    }


    //    for (int i = 0; i < prop.Count; i++)
    //    {
    //        JToken pJObject = jProp[i];
    //        var pItem = prop[i];
    //        //Serialize_ItemPropSyncList(pItem, (JObject)pJObject);
    //    }

    //}


    public static void Serialize_RepeatedPropSyncList(IMessage message, JObject jObject)
    {
        RepeatedPropSyncList data = (RepeatedPropSyncList)message;
        var prop = data.InfoList;

        if (prop.Count == 0)
        {
            return;
        }

        JArray jProp = jObject.GetValue("infoList") as JArray;
        if (jProp == null || jProp.Count == 0)
        {
            return;
        }

        for (int i = 0; i < prop.Count; i++)
        {
            JToken pJObject = jProp[i];
            var pItem = prop[i];
            Serialize_PropSyncList(pItem, (JObject)pJObject);
        }
    }

    public static void Serialize_UserMainDataNotify(IMessage message, JObject jObject)
    {
        UserMainDataNotify data = (UserMainDataNotify)message;
        PropBaseSyncList prop = data.Prop;
        if (prop == null)
        {
            return;
        }

        var jProp = jObject.GetValue(PROP) as JObject;
        if (jProp == null)
        {
            return;
        }

        Serialize_PropBaseSyncList(prop, jProp);
    }

    public static void Serialize_EnterAOI(IMessage message, JObject jObject)
    {
        EnterAOI data = (EnterAOI)message;
        PropSyncList prop = data.Prop;
        if (prop == null)
        {
            return;
        }

        JObject jProp = jObject.GetValue(PROP) as JObject;
        if (jProp == null)
        {
            return;
        }

        Serialize_PropSyncList(prop, jProp);
    }

    public static void Serialize_UpdateAOI(IMessage message, JObject jObject)
    {
        UpdateAOI data = (UpdateAOI)message;
        PropSyncList prop = data.Prop;

        if (prop == null)
        {
            return;
        }

        JObject jProp = jObject.GetValue(PROP) as JObject;
        if (jProp == null)
        {
            return;
        }

        Serialize_PropSyncList(prop, jProp);
    }

    public static void Serialize_AOIMsg(IMessage message, JObject jObject)
    {
        AOIMsg data = (AOIMsg)message;
        RepeatedField<EnterAOI> enterAOIs = data.EnterAOIs;
        RepeatedField<UpdateAOI> updateAOIs = data.UpdateAOIs;

        if (enterAOIs.Count > 0)
        {
            JArray jEnterAOIs = jObject.GetValue("enterAOIs") as JArray;
            if (jEnterAOIs != null)
            {
                for (int i = 0; i < enterAOIs.Count; i++)
                {
                    JToken pJObject = jEnterAOIs[i];
                    EnterAOI pItem = enterAOIs[i];
                    Serialize_EnterAOI(pItem, (JObject)pJObject);
                }

            }
        }

        if (updateAOIs.Count > 0)
        {
            JArray jUpdateAOIs = jObject.GetValue("updateAOIs") as JArray;
            if (jUpdateAOIs != null)
            {
                for (int i = 0; i < jUpdateAOIs.Count; i++)
                {
                    JToken pJObject = jUpdateAOIs[i];
                    UpdateAOI pItem = updateAOIs[i];
                    Serialize_UpdateAOI(pItem, (JObject)pJObject);
                }
            }

        }

    }


    #endregion

    #region 将 json DeSerialize 反序列化 为 pb 结构 的部分

    public static void DeSerialize_BlackListJson(JObject jObject, string tag)
    {
        JArray jBlackList = (JArray)jObject.GetValue("BlackList");
        if (jBlackList == null || jBlackList.Count == 0)
        {
            return;
        }

        // Debug.Log($"message: {tag} ,1 parse [BlackList]  --> json: {jObject} ");

        foreach (JObject item in jBlackList)
        {
            // 先检查 这个黑板中 是否包含 二进制 属性的 json 数据, 如果不包含, 这个黑板数据就不需要继续解
            JObject rawValue = item.GetValue("RawValue") as JObject;
            if (rawValue == null)
            {
                continue;
            }

            JToken? msg_value = rawValue.GetValue("MsgValue");
            if (msg_value == null)
            {
                continue;
            }

            JObject msgValue = msg_value as JObject;
            // 如果 msgValue中 没有任何 二进制数据, 那就不需要继续解
            if (msgValue == null)
            {
                continue;
            }

            // 如果找不到对应的 消息 cmd , 那就不需要继续 解
            JValue msgID = rawValue.GetValue("MsgID") as JValue;
            if (msgID == null)
            {
                continue;
            }

            // 此处 如果存在 被解开了 MsgValue json 结构, 那就需要先将 msgValue 转换成 对应的 pb 结构, 然后 将pb 转换为 pb 的 toString 结构
            int cmd = (int)msgID;

            ProtoInfo? protoInfo = ProtoDic.Instance.GetProtoInfoByCmd(cmd);

            if (!protoInfo.HasValue)
            {
                continue;
            }



            // 此处已经被解开为 json 了, 所以就存在一种 黑板的 rawMsg 也包含着  拥有黑板的结构
            DeSerializeQAMessage(msgValue, protoInfo.Value.Name);




            // note: 注意此处的 黑板 中的 二进制结构 反序列化只解了一层, 如果出现了 多层的 嵌套, 就需要单个 proto结构都去单独处理。
            // 此处 只是针对 黑板的 结构取巧而已.
            IMessage pbMessage = ProtoUtils.Deserialize(cmd, msgValue.ToString());
            if (pbMessage == null)
            {
                continue;
            }

            // 目前通过实验发现 pb 二进制转化为 string 是采用 base64 的方式
            var pbStr = pbMessage.ToByteString().ToBase64();
            //string rawMsgJson1 = JsonFormatter.Default.Format(pbbyte);

            //JObject replaceJ = (JObject)JsonConvert.DeserializeObject(rawMsgJson1);
            rawValue["MsgValue"] = new JValue(pbStr);

        }
    }


    public static void Deserialize_SyncBaseInfoJson(JObject jOb, string pbName)
    {
        // 如果传入的message 就是 SyncBaseInfo 的结构, 那直接解就可以
        if (pbName == ProtoMsg.SyncBaseInfo.Descriptor.Name)
        {
            DeSerialize_SyncBaseInfo(jOb);
            return;
        }
        // 如果 是 PropBaseSyncList 结构,那就先 按 P
        if (pbName == ProtoMsg.PropBaseSyncList.Descriptor.Name)
        {
            DeSerialize_PropBaseSyncList(jOb, pbName);
            return;
        }

        if (pbName == ProtoMsg.PropSyncList.Descriptor.Name)
        {
            DeSerialize_PropSyncList(jOb, pbName);
            return;
        }

        if (pbName == ProtoMsg.PropPanelRet.Descriptor.Name)
        {
            DeSerialize_PropPanelRet(jOb, pbName);
            return;
        }

        //if (pbName == ProtoMsg.ItemPropSyncList.Descriptor.Name)
        //{
        //    DeSerialize_ItemPropSyncList(jOb, pbName);
        //    return;
        //}

        //if (pbName == ProtoMsg.ItemSpaceSyncList.Descriptor.Name)
        //{
        //    DeSerialize_ItemSpaceSyncList(jOb, pbName);
        //    return;
        //}


        if (pbName == ProtoMsg.RepeatedPropSyncList.Descriptor.Name)
        {
            DeSerialize_RepeatedPropSyncList(jOb, pbName);
            return;
        }

        if (pbName == ProtoMsg.UserMainDataNotify.Descriptor.Name)
        {
            DeSerialize_UserMainDataNotify(jOb, pbName);
            return;
        }

        if (pbName == ProtoMsg.EnterAOI.Descriptor.Name)
        {
            DeSerialize_EnterAOI(jOb, pbName);
            return;
        }

        if (pbName == ProtoMsg.UpdateAOI.Descriptor.Name)
        {
            DeSerialize_UpdateAOI(jOb, pbName);
            return;
        }

        if (pbName == ProtoMsg.AOIMsg.Descriptor.Name)
        {
            DeSerialize_AOIMsg(jOb, pbName);
            return;
        }
    }

    public static void DeSerialize_SyncBaseInfo(JObject job)
    {
        JToken msg_value = job.SelectToken("MsgValue");
        if (msg_value == null)
        {
            return;
        }
        // note:
        // msgValue 可能为 "" 或者 存在 pb 转换后的 json , 那么就需要 判断 是否为 空
        // ""  的话, 用 as 得到的 的 JObject 类型为 null
        JObject msgValue = msg_value as JObject;
        if (msgValue == null)
        {
            return;
        }

        JValue indexValue = job.SelectToken("index") as JValue;
        if (indexValue == null)
        {
            return;
        }


        VitalSignAOIClientAttrs vitalSignAOIClientAttrs = LocalDataManager.Instance.GetAttrPropByAttrIdx((ushort)indexValue);

        if (vitalSignAOIClientAttrs == null)
        {
            return;
        }
        string key = vitalSignAOIClientAttrs.Name;


        int cmd = 0;

        switch (key)
        {
            case AOIAttrDefine.Position:
                {
                    cmd = (int)MsgIDEnum.Vector3ID;
                }
                break;
            case AOIAttrDefine.StorageDrugs:
                {
                    cmd = (int)MsgIDEnum.StorageDrugsMdID;
                }
                break;
            case AOIAttrDefine.ExAmuletInfo:
                {
                    cmd = (int)MsgIDEnum.ExtractaAmuletMDID;
                }
                break;
            /*
        case AOIAttrDefine.GVEBonus:
            {
                cmd =  (int) MsgIDEnum.GVEBonusID;
            }
            break;
            */
            default: break;
        }
        // 找不到对应的消息号, 那也就不用解了
        if (cmd == 0)
        {
            return;
        }

        // 不确定  msgValue 中否 存在多层 pb 解开的结构, 所以 此处嵌套 做一次 DeSerializeQAMessage
        {
            ProtoInfo? protoInfo = ProtoDic.Instance.GetProtoInfoByCmd(cmd);
            if (!protoInfo.HasValue)
            {
                return;
            }

            string messageName = protoInfo.Value.Name;

            DeSerializeQAMessage(msgValue, messageName);
        }


        string msgValueStr = msgValue.ToString();

        IMessage pbMessage = null;
        pbMessage = ProtoUtils.Deserialize(cmd, msgValueStr);

        if (pbMessage == null)
        {
            return;
        }

        // 将 pbMessage 转换为 原始的 base64 字符串
        string pbStr = pbMessage.ToByteString().ToBase64();

        // 替换掉 json 中的 msgValue 字段
        job["MsgValue"] = new JValue(pbStr);
    }

    public static void Handle_Deserialize_PropBaseSyncList(JObject jOb, string propName)
    {

        HandleXXXJArray<JObject>(jOb, propName, (item) =>
        {
            DeSerialize_SyncBaseInfo(item);
        });
    }
    public static void DeSerialize_PropBaseSyncList(JObject jOb, string pbName)
    {

        Handle_Deserialize_PropBaseSyncList(jOb, "prop");
    }

    public static void DeSerialize_PropSyncList(JObject jOb, string pbName)
    {
        HandleXXXJObject<JObject>(jOb, "prop", (item) =>
        {
            DeSerialize_PropBaseSyncList(item, pbName);
        });
    }

    public static void DeSerialize_PropPanelRet(JObject jOb, string pbName)
    {
        Handle_Deserialize_PropBaseSyncList(jOb, "prop");
    }

    public static void DeSerialize_ItemPropSyncList(JObject jOb, string pbName)
    {
        HandleXXXJObject<JObject>(jOb, "prop", (item) =>
        {
            DeSerialize_PropBaseSyncList(item, pbName);
        });
    }

    public static void DeSerialize_ItemSpaceSyncList(JObject jOb, string pbName)
    {
        HandleXXXJArray<JObject>(jOb, "ItemInfo", (item) =>
        {
            DeSerialize_ItemPropSyncList(item, pbName);
        });
    }

    public static void DeSerialize_RepeatedPropSyncList(JObject jOb, string pbName)
    {
        HandleXXXJArray<JObject>(jOb, "infoList", (item) =>
        {
            DeSerialize_PropSyncList(item, pbName);
        });

    }

    public static void DeSerialize_UserMainDataNotify(JObject jOb, string pbName)
    {
        HandleXXXJObject<JObject>(jOb, "prop", (item) =>
        {
            DeSerialize_PropBaseSyncList(item, pbName);
        });

    }

    public static void DeSerialize_EnterAOI(JObject jOb, string pbName)
    {
        HandleXXXJObject<JObject>(jOb, "prop", (item) =>
        {
            DeSerialize_PropSyncList(item, pbName);
        });

    }

    public static void DeSerialize_UpdateAOI(JObject jOb, string pbName)
    {
        HandleXXXJObject<JObject>(jOb, "prop", (item) =>
        {
            DeSerialize_PropSyncList(item, pbName);
        });

    }

    private static void HandleXXXJObject<T>(JObject jOb, string jTokenName, Action<T> cb) where T : JToken
    {
        JToken prop = jOb.SelectToken(jTokenName);

        if (prop == null)
        {
            return;
        }

        T propV = prop as T;
        if (propV == null)
        {
            return;
        }
        cb.Invoke(propV);
    }

    private static void HandleXXXJArray<T>(JObject jOb, string jTokenName, Action<T> cb) where T : JToken
    {
        JToken prop = jOb.SelectToken(jTokenName);

        if (prop == null)
        {
            return;
        }

        JArray propV = prop as JArray;
        if (propV == null)
        {
            return;
        }
        for (int i = 0; i < propV.Count; i++)
        {
            T pItem = (T)propV[i];
            cb.Invoke(pItem);
        }
    }

    public static void DeSerialize_AOIMsg(JObject jOb, string pbName)
    {
        HandleXXXJArray<JObject>(jOb, "enterAOIs", (item) =>
        {
            DeSerialize_EnterAOI(item, pbName);
        });

        HandleXXXJArray<JObject>(jOb, "updateAOIs", (item) =>
        {
            DeSerialize_UpdateAOI(item, pbName);
        });
    }


    #endregion

    private static QAProto tempQAProto = new();

    /// <summary>
    /// 将 客户端发送给 服务器的 pb 数据发送给 qasdk
    /// </summary>
    public static void SendClientRpcPb2QASDK(ServerType serverType, int cmd, IMessage message, Boolean isEncrypt, ulong oneOfCtrlEntityId, bool isAutoChangeMsgTarget)
    {
        // #if QA_DEBUG
        if (!GMTestData.CheckGmIsOpen("OpenQASDK"))
        {
            return;
        }
        try
        {
            tempQAProto.Init(true, true, true, cmd, message);
            tempQAProto.InitExtraClientSendData(serverType, isEncrypt, oneOfCtrlEntityId, isAutoChangeMsgTarget);
            QASDKMsgUtils.Send2QAMsg(tempQAProto);
        }
        catch (System.Exception e)
        {
            //Debug.Log($"[QASDK]  message: {protoInfo.Value.cmd}  ， 解析失败 , e: {e.Message}");

        }
        // #endif
    }

    public static void SendServerMessage2QASDK(bool isRpc, bool isPB, bool isClientSend, int cmd, IMessage pbMessage)
    {
        // #if QA_DEBUG
        if (!GMTestData.CheckGmIsOpen("OpenQASDK"))
        {
            return;
        }
        try
        {
            tempQAProto.Init(isRpc, isPB, isClientSend, cmd, pbMessage);
            QASDKMsgUtils.Send2QAMsg(tempQAProto);
        }
        catch (System.Exception e)
        {
            Debug.Log($"[QASDK] SendServerMessage2QASDK  message: {cmd}  ， 解析失败 , e: {e.Message}");

        }
        // #endif
    }

}

/// <summary>
/// QA 的 proto 通用 基础类
/// </summary>
public class QAProto
{
    public bool IsRpc;
    public bool IsPB;

    public int CMD;

    public string protoName;

    public bool IsClientSend;

    // public string message;
    public JObject messageJson;


    public ServerType SendServerType;
    public Boolean IsEncrypt = false;
    public ulong OneOfCtrlEntityId = 0;

    public bool IsAutoChangeMsgTarget = true;

    public void Reset()
    {
        IsRpc = false;
        IsPB = false;
        IsClientSend = false;
        CMD = 0;
        protoName = "";
        // message = "";

        SendServerType = ServerType.ServerTypeScene;
        IsEncrypt = false;
        OneOfCtrlEntityId = 0;
        IsAutoChangeMsgTarget = true;
    }

    public void Init(bool isRpc, bool isPB, bool isClientSend, int cmd, IMessage pbMessage)
    {
        Reset();
        IsRpc = isRpc;
        IsPB = isPB;
        IsClientSend = isClientSend;
        CMD = cmd;
        protoName = pbMessage.Descriptor.Name;

        // message = QASDKMsgUtils.ConvertPB2QAJson(pbMessage);
        messageJson = QASDKMsgUtils.ConvertPB2QAJsonObject(pbMessage);
    }



    public void InitExtraClientSendData(ServerType serverType, Boolean isEncrypt = false, ulong oneOfCtrlEntityId = 0, bool isAutoChangeMsgTarget = true)
    {
        SendServerType = serverType;
        IsEncrypt = isEncrypt;
        OneOfCtrlEntityId = oneOfCtrlEntityId;
        IsAutoChangeMsgTarget = isAutoChangeMsgTarget;
    }

    public string ToJsonString()
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(this);
    }

    public IMessage GetMessage()
    {
        return ProtoUtils.Deserialize(CMD, messageJson.ToString());
        // return ProtoUtils.Deserialize(CMD, message);
    }

    public static QAProto GetQAProtoBase(string jsonStr)
    {
        return Newtonsoft.Json.JsonConvert.DeserializeObject<QAProto>(jsonStr);
    }
}
