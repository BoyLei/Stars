using ProtoMsg;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ItemMapLine : MonoBehaviour
{
    private Text LineText;
    private Image LineStateIcon;
    private JButton SelfJBtn;

    private Action<ulong, ulong> clickAction;
    private ulong m_Line = 0;
    private ulong m_SpaceID = 0;

    private ServerMapLoadInfo m_serverMapLoadInfo;

    private void Awake()
    {
        LineText = transform.Find("Text").GetComponent<Text>();
        LineStateIcon = transform.Find("State").GetComponent<Image>();
        SelfJBtn = transform.GetComponent<JButton>();

        SelfJBtn.OnClick += OnClickJBtn;
    }

    public void InitClickCB(Action<ulong, ulong> clickCB)
    {
        clickAction = clickCB;
    }

    public void SetLineItem(ServerMapLoadInfo serverMapLoadInfo)
    {
        m_serverMapLoadInfo = serverMapLoadInfo;
        m_Line = m_serverMapLoadInfo.ServerID;
        m_SpaceID = m_serverMapLoadInfo.SpaceID;

        //LineText.text = $"{m_serverMapLoadInfo.LineID + 1}线:{m_serverMapLoadInfo.Num}人:{m_serverMapLoadInfo.Load}";
        LineText.text = string.Format(StarProject.Service.Language.LanguageManager.Instance.GetLanguageByKey("LineStr"), m_serverMapLoadInfo.LineID + 1);
    }

    private void OnClickJBtn(GameObject arg0)
    {
        clickAction?.Invoke(m_Line, m_SpaceID);
    }

}
