using SGF.UI.Framework;
using StarProject.Module;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.UI;

public class BenameWidget : UIWidget
{
    public InputFieldEx M_InputName;

    [SerializeField]
    Button m_RandomBtn;

    StringBuilder builder = new StringBuilder();
    private Action<string> m_SureAction = null;

    protected override void OnOpen(object arg)
    {
        base.OnOpen(arg);
        m_RandomBtn.onClick.RemoveAllListeners();
        m_RandomBtn.onClick.AddListener(SetRandomName);
        M_InputName.onEndEdit.RemoveAllListeners();
        SetRandomName();
        M_InputName.activeAction = OnActive;
        M_InputName.onEndEdit.AddListener((text) =>
        {
            M_InputName.LeaveEditor();
        });
        // M_InputName.text = "";
        {
            JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/Content/BtnCancel").GetComponent<JButton>();
            Btn.OnClick = (go) =>
            {
                Close(null);
            };
        }

        {
            JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/Content/BtnOk").GetComponent<JButton>();
            Btn.OnClick = (go) =>
            {
                OnBtnCreateAccount();
            };
        }
    }

    public void SetRandomName()
    {
        builder.Clear();
        var list1 = LocalDataManager.Instance.FirstName.StaticFirstNameDatas.KToArray();
        var list2 = LocalDataManager.Instance.LastName.StaticLastNameDatas.KToArray();
        var random1 = UnityEngine.Random.Range(0, list1.Length);
        var random2 = UnityEngine.Random.Range(0, list2.Length);
        var name = builder.Append(list1[random1].Value.Text).Append(list2[random2].Value.Text).ToString();
        M_InputName.text = name;
    }

    public void OnActive()
    {
        M_InputName.text = null;
    }

    public void SetSureCB(Action<string> cb)
    {
        m_SureAction = cb;
    }

    private void OnBtnCreateAccount()
    {
        string name = Convert.ToString(M_InputName.text);
        if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
        {
            Frame.Util.ShowMessageByCode(CRetMsgEnum.Tips_LogIn_01);
            return;
        }
        int length = System.Text.Encoding.UTF8.GetByteCount(name);
        if (length > 21)
        {
            //Frame.Util.ShowSystemMessage("昵称最多7个字");
            //Frame.Util.ShowSystemMessage(GameConfig.LocalStr["BenameTips"]);
            Frame.Util.ShowSystemMessage(LanguageManager.Instance.GetLanguageByKey("BenameTips"));

            return;
        }

        m_SureAction?.Invoke(M_InputName.text);
        //Close(M_InputName.text);
    }

}
