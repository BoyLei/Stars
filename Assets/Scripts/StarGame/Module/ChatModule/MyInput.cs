using Google.Protobuf;
using ProtoMsg;
using SGF.Network;
using SGF.Time;
using StarProject.Game;
using StarProject.Service.Battle;
using StarProject.Service.Business;
using StarProject.Service.LocalData;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[XLua.LuaCallCSharp]
public class MyInput : InputField
{
    //正则表达式
    private static readonly Regex keywordReg = new(@"\[(.+?)\]", RegexOptions.Singleline);

    private const int TextLimitMax = 30;

    private TextMeshProUGUI m_TextMeshProUGUI;

    private int lastCaretPosition = 0;
    private int lastFocusPosition = 0;

    private bool isDragging = false;
    private int textOrgLength = 0;

    public ChatExtraInfos extrainfos = new();    // 聊天 表情、坐标、道具信息
    private string LastText = "";

    public UnityEngine.UI.Text numText = null;

    protected override void Awake()
    {
        base.Awake();
        var textMeshPro = transform.Find("TextMeshPro");
        if (textMeshPro != null)
        {
            m_TextMeshProUGUI = textMeshPro.GetComponent<TextMeshProUGUI>();
        }

        var numTrans = transform.parent.Find("Num");
        if (numTrans != null)
        {
            numText = numTrans.GetComponent<UnityEngine.UI.Text>();
        }
    }

    protected override void Start()
    {
        base.Start();
        //onValidateInput += delegate (string input, int charIndex, char addedChar) { return MyValidate(input, charIndex, addedChar); };
        onValueChanged.AddListener(OnValueChangedAction);
    }

    protected void Update()
    {
        if (isFocused)
        {
            if (isDragging)
            {
                int curFocusPosition = selectionFocusPosition;
                if (lastFocusPosition != curFocusPosition)
                {
                    foreach (Match match in keywordReg.Matches(text))
                    {
                        if (curFocusPosition > match.Index && curFocusPosition < match.Index + match.Length)
                        {
                            if (curFocusPosition > lastCaretPosition)
                            {
                                selectionFocusPosition = match.Index + match.Length;
                            }
                            else
                            {
                                selectionFocusPosition = match.Index;
                            }
                            break;
                        }
                    }
                    lastFocusPosition = curFocusPosition;
                }
            }
            else
            {
                int curCaretPosition = caretPosition;
                if (lastCaretPosition != curCaretPosition)
                {
                    foreach (Match match in keywordReg.Matches(text))
                    {
                        if (curCaretPosition > match.Index && curCaretPosition < match.Index + match.Length)
                        {
                            if (curCaretPosition > lastCaretPosition)
                            {
                                caretPosition = match.Index + match.Length;
                            }
                            else
                            {
                                caretPosition = match.Index;
                            }
                            break;
                        }
                    }
                    lastCaretPosition = curCaretPosition;
                }
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        onValueChanged.RemoveListener(OnValueChangedAction);
    }

    #region 系统

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        isDragging = true;
        //Debug.Log("OnBeginDrag");
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        //Debug.Log("OnDeselect");
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        //Debug.Log("OnDrag");
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        isDragging = false;
        //Debug.Log("OnEndDrag");
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        //Debug.Log("OnPointerClick");
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        //Debug.Log("OnPointerDown");
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        //Debug.Log("OnSelect");
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        base.OnSubmit(eventData);
        //Debug.Log("OnSubmit");
    }

    public override void OnUpdateSelected(BaseEventData eventData)
    {
        base.OnUpdateSelected(eventData);
        //Debug.Log("OnUpdateSelected");
    }
    #endregion

    ///////////----------------------------------------------
    ///////////----------------------------------------------
    ///////////----------------------------------------------
    ///////////----------------------------------------------
    ///////////----------------------------------------------
    ///////////----------------------------------------------

    private char MyValidate(string txt, int charIndex, char ch)
    {
        if (isFocused && charIndex == textOrgLength)
        {
            if (ch == '[' || ch == ']' || ch == '<' || ch == '>')
            {
                return '\0';
            }
        }
        return ch;
    }

    private void OnValueChangedAction(string str)
    {
        // 如果是+标签+道具+坐标这边就不处理了
        if (!isFocused)
        {
            return;
        }

        if (str.Length > 0)
        {
            // else
            // {
            // bool squareEqual = KeywordsEqual(str, '[', ']');
            // bool arrowEqual = KeywordsEqual(str, '<', '>');
            // if (!squareEqual)
            // {
            if (caretPosition > 0 && caretPosition <= str.Length)
            {
                //UnityEngine.Debug.Log($"聊天测试 111111111 caretPosition={caretPosition},leng={str.Length}");

                // 处理不能直接单个输入 "[","]","<",">" 这些字符，
                // 否者直接删除
                char ch = str[caretPosition - 1];
                if ((ch == '[' || ch == ']' || ch == '<' || ch == '>') && (!KeywordsEqual(str, '[', ']') || !KeywordsEqual(str, '<', '>')))
                {
                    text = str.Remove(caretPosition - 1, 1);
                    caretPosition = caretPosition - 1;
                    return;
                }

                //int length = System.Text.Encoding.UTF8.GetByteCount(str);
                //if (length > TextLimitMax * 3)
                //{
                //    text = str.Remove(caretPosition - 1, 1);
                //    caretPosition = caretPosition - 1;
                //    return;
                //}

                // 处理删除 "[","]" 
                if (!KeywordsEqual(str, '[', ']'))
                {
                    for (int i = caretPosition - 1; i >= 0; i--)
                    {
                        if (str[i] == ']')
                        {
                            break;
                        }
                        if (str[i] == '[')
                        {
                            int pos = caretPosition;

                            int index = GetKeyWordsIndex(str, pos);
                            RemoveItem(index);

                            caretPosition = i;
                            str = str.Remove(i, pos - i);
                        }
                    }
                }
            }
            else
            {
                // 从开头往后删除在这里判断 应为光标的下标==0
                UnityEngine.Debug.Log($"聊天测试 caretPosition={caretPosition},leng={str.Length}");
            }
            // }
            // if (!arrowEqual)
            // {
            //     var ch = str[str.Length - 1];
            //     if (ch == '<' || ch == '>')
            //     {
            //         text = str.Substring(0, str.Length - 1);
            //         return;
            //     }
            // }
            // var pos1 = str.LastIndexOf(']');
            // var pos2 = str.LastIndexOf('[');
            // if (pos2 > pos1)
            // {
            //     textOrgLength = pos2;
            //     text = str.Substring(0, pos2);
            // }
            //}
            //textOrgLength = text.Length;
        }
        //else
        //{
        //    // 全部删除了
        //    if (numText != null)
        //    {
        //        numText.text = $"0/{TextLimitMax}";
        //    }
        //}

        if (!LastText.Equals(str))
        {
            bool res = SetTextMeshProRes(str);
            text = LastText;
        }
    }

    private bool KeywordsEqual(string str, char lch, char rch)
    {
        int lCount = 0;
        int rCount = 0;
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == lch)
            {
                lCount++;
            }
            if (str[i] == rch)
            {
                rCount++;
            }
        }
        return lCount == rCount;
    }

    private int GetKeyWordsIndex(string str, int pos)
    {
        int index = 0;
        for (int i = 0; i < pos; i++)
        {
            if (str[i] == ']')
            {
                index++;
            }
        }
        return index;
    }

    //public void SetText(string _text, ChatExtraInfos chatExtraInfos)
    //{
    //    extrainfos = chatExtraInfos;
    //    text = _text;
    //    if (!LastText.Equals(text))
    //    {
    //        LastText = text;
    //        SetTextMeshPro(LastText);
    //    }
    //}

    public void SetText(string _text, string chatExtraInfosStr)
    {
        var protoInfo = ProtoDic.Instance.GetProtoInfoByCmd((int)MsgIDEnum.ChatExtraInfosID);
        if (!protoInfo.HasValue)
        {
            SGF.Debuger.LogError($"[MyInput] SetText 获取不到 {MsgIDEnum.ChatExtraInfosID} 对应的 protoInfo, 炸了！！！");
            return;
        }

        IMessage bbMessage = protoInfo.Value.parse.ParseJson(chatExtraInfosStr);
        if (!LastText.Equals(_text))
        {
            bool res = SetTextMeshProRes(_text);
            if (res)
            {
                text = _text;
                extrainfos = (ChatExtraInfos)bbMessage;
            }
        }
    }

    private const string LocationChar1 = "：";
    private const string LocationChar2 = ",";

    public void AddLocation()
    {
        var pos = GameManager.Instance.GetEntityProtoPosById(GameManager.Instance.mainPlayerId);
        var mapId = GameManager.Instance.GetCurMapId();
        string mapName = string.Empty;
        var mapCfgData = LocalDataManager.Instance.GetMapCfgData(mapId);
        if (mapCfgData != null)
        {
            mapName = mapCfgData.MapName;
        }
        string content = $"[{mapName}{LocationChar1}{pos.X:#0.0}{LocationChar2}{pos.Z:#0.0}]";
        string newText = $"{LastText}{content}";

        if (!LastText.Equals(newText))
        {
            bool res = SetTextMeshProRes(newText);
            if (res)
            {
                var spaceType = GameManager.Instance.GetCurMapType();
                var spaceID = GameManager.Instance.GetSpaceID();

                ChatLocationInfo chatLocationInfo = new();
                chatLocationInfo.Pos = pos;
                chatLocationInfo.MapId = mapId;
                chatLocationInfo.MapId = mapId;
                chatLocationInfo.SpaceID = spaceID;
                chatLocationInfo.SpType = spaceType;
                ChatExtraInfo chatExtraInfo = new();
                chatExtraInfo.EType = ProtoMsg.ChatExtraInfo.Types.EMsg.Location;
                chatExtraInfo.LocationInfo = chatLocationInfo;
                extrainfos.ChatExtraInfo.Add(chatExtraInfo);

                text = LastText;
            }
        }
        //AddTextMeshPro(content);
    }

    public void AddItem(ProtoMsg.ItemMD itemData)
    {
        var data = LocalDataManager.Instance.GetItemDataCell(itemData.BaseID);
        if (data == null)
        {
            return;
        }
        string content = $"[{data.Name}]";
        string newText = $"{LastText}{content}";

        if (!LastText.Equals(newText))
        {
            bool res = SetTextMeshProRes(newText);
            if (res)
            {
                ChatExtraInfo chatExtraInfo = new();
                chatExtraInfo.EType = ProtoMsg.ChatExtraInfo.Types.EMsg.Equip;
                chatExtraInfo.EquipData.Clear();
                chatExtraInfo.EquipData.Add(itemData);
                if (itemData.GemSlots != null)
                {
                    for (int i = 0; i < itemData.GemSlots.List.Count; i++)
                    {
                        var gem = BusinessManager.Instance.GetItemByItemEntityId(itemData.GemSlots.List[i].GemID);
                        if (gem != null)
                        {
                            chatExtraInfo.EquipData.Add(gem);
                        }
                    }
                }
                extrainfos.ChatExtraInfo.Add(chatExtraInfo);

                text = LastText;
            }
        }
        //AddTextMeshPro(content);
    }

    public void AddEmoji(int id)
    {
        string iconName = "Emoticon_nom_";
        if (id < 10)
        {
            iconName = "Emoticon_nom_0";
        }
        iconName += id;
        int index = BattleManager.Instance.GetCurrencySpriteSpriteIndexFromName(iconName);
        string newText = $"{LastText}[emoji:{index}]";
        if (!LastText.Equals(newText))
        {
            bool res = SetTextMeshProRes(newText);
            if (res)
            {
                ChatExtraInfo chatExtraInfo = new();
                chatExtraInfo.EType = ProtoMsg.ChatExtraInfo.Types.EMsg.Emoji;
                chatExtraInfo.EmojiID = id;
                extrainfos.ChatExtraInfo.Add(chatExtraInfo);

                text = LastText;
            }
        }
        //string content = $"<sprite={id}>";
        //AddTextMeshPro(content);
    }

    public void AddBigEmoji(int id)
    {
        ChatExtraInfo chatExtraInfo = new();
        chatExtraInfo.EType = ProtoMsg.ChatExtraInfo.Types.EMsg.BigEmoji;
        chatExtraInfo.BigEmojiID = id;
        //extrainfos.ChatExtraInfo.Clear();
        extrainfos.ChatExtraInfo.Add(chatExtraInfo);
    }

    public void RemoveItem(int index)
    {
        extrainfos.ChatExtraInfo.RemoveAt(index);
    }

    public void Clear()
    {
        text = "";
        LastText = "";
        extrainfos.ChatExtraInfo.Clear();
        if (numText != null)
        {
            numText.text = $"{0}/{TextLimitMax}";
        }
        ClearTextMeshPro();
    }

    public void Sync(MyInput ip)
    {
        if (ip == null)
        {
            return;
        }

        bool res = SetTextMeshProRes(ip.text);
        if (res)
        {
            text = ip.text;
            extrainfos = ip.extrainfos;
            caretPosition = text.Length;
        }
    }

    public void Sync(InputField ip)
    {
        if (ip == null)
        {
            return;
        }

        bool res = SetTextMeshProRes(ip.text);
        if (res)
        {
            text = ip.text;
            //extrainfos = new();
            caretPosition = text.Length;
        }
    }

    public void SendData(ProtoMsg.ChatChannel channel, ulong pid = 0)
    {
        string chatMsg = string.Empty;
        ChatExtraInfos _extrainfos = new();
        int isHaveBigEmojiIndex = GetIsHaveBigEmojiIndex();
        if (isHaveBigEmojiIndex == -1)
        {
            RecordManager.PushItem(text, extrainfos);
            chatMsg = ExtraData.ClientToSrv(text);
            _extrainfos = extrainfos;
        }
        else
        {
            _extrainfos.ChatExtraInfo.Add(extrainfos.ChatExtraInfo[isHaveBigEmojiIndex]);
            extrainfos.ChatExtraInfo.RemoveAt(isHaveBigEmojiIndex);
        }

        var message = new ProtoMsg.SendChatMsgReq();
        message.ChatMsg = chatMsg;
        message.Channel = channel;
        message.ChatTime = TimeUtils.ServerNowStampMilli;
        message.SendData.Add(_extrainfos.ToByteString());
        if (pid > 0)
        {
            message.PID = pid;
        }
        SGF.Network.NetworkManager.Instance.gameSocket.SendRPCMsg(SGF.Network.ServerType.ServerTypeLobby, message, false);

        if (isHaveBigEmojiIndex == -1)
        {
            Clear();
        }
    }

    private int GetIsHaveBigEmojiIndex()
    {
        if (extrainfos != null && extrainfos.ChatExtraInfo != null)
        {
            for (int i = 0; i < extrainfos.ChatExtraInfo.Count; i++)
            {
                var item = extrainfos.ChatExtraInfo[i];
                if (item.EType == ProtoMsg.ChatExtraInfo.Types.EMsg.BigEmoji)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    public void SendData2(ProtoMsg.ChatChannel channel, string str, ChatExtraInfos extrainfos, ulong pid = 0)
    {
        var message = new ProtoMsg.SendChatMsgReq();
        message.ChatMsg = ExtraData.ClientToSrv(str);
        message.Channel = channel;
        message.ChatTime = TimeUtils.ServerNowStampMilli;
        message.SendData.Add(extrainfos.ToByteString());
        if (pid > 0)
        {
            message.PID = pid;
        }
        SGF.Network.NetworkManager.Instance.gameSocket.SendRPCMsg(SGF.Network.ServerType.ServerTypeLobby, message, false);

        Clear();
    }

    #region 自定义显示的富文本逻辑

    private bool SetTextMeshProRes(string text)
    {
        if (m_TextMeshProUGUI == null)
        {
            return false;
        }

        int textLength = GetTextLength(text);
        if (textLength > TextLimitMax)
        {
            return false;
        }

        string str = ClientToSrv(text);

        m_TextMeshProUGUI.text = str;

        if (numText != null)
        {
            numText.text = $"{textLength}/{TextLimitMax}";
        }
        LastText = text;
        return true;
    }

    public string ClientToSrv(string str)
    {
        string srvStr = "";
        int curIndex = 0;
        foreach (Match match in keywordReg.Matches(str))
        {
            srvStr += str.Substring(curIndex, match.Index - curIndex);
            curIndex = match.Index + match.Length;
            string replaceStr = ReplaceStr(match.Value);
            srvStr += replaceStr;
        }
        srvStr += str.Substring(curIndex, str.Length - curIndex);

        return srvStr;
    }

    private int GetTextLength(string str)
    {
        string srvStr = "";
        int curIndex = 0;
        int length = 0;
        foreach (Match match in keywordReg.Matches(str))
        {
            srvStr += str.Substring(curIndex, match.Index - curIndex);
            curIndex = match.Index + match.Length;
            length++;
        }
        if (length > 0)
        {
            srvStr += str.Substring(curIndex, str.Length - curIndex);
            return length + srvStr.Length;
        }
        return str.Length;
    }

    private string ReplaceStr(string original)
    {
        string res = "";
        // 判断是不是【小表情】
        bool isEmoji = original.Contains("emoji");
        // 判断是不是【道具】
        //bool isItem = original.Contains("emoji");
        // 判断是不是【坐标】
        //bool isLocation = original.Contains(LocationChar1) && original.Contains(LocationChar2);
        if (isEmoji)
        {
            original = original.Replace("[", "<");
            original = original.Replace("emoji:", "sprite=");
            original = original.Replace("]", ">");
            res = original;
        }
        else
        {
            // 【道具】【坐标】不需要转换
            res = original;
        }

        return res;
    }


    private void AddTextMeshPro(string text)
    {
        if (m_TextMeshProUGUI == null)
        {
            return;
        }
        m_TextMeshProUGUI.text += text;
    }

    private void ClearTextMeshPro()
    {
        if (m_TextMeshProUGUI == null)
        {
            return;
        }
        m_TextMeshProUGUI.text = "";
    }

    #endregion

}
