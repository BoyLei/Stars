using SGF.UI.Framework;
using SGF.Unity;
using StarProjectDef;
using System;
using UnityEngine.UI;

public class AchievementTipsWidget : UIWidget
{
    private string TagFlag = "[AchievementTipsWidget]";

    public Text M_text;

    private bool useAnimKeyFrame = true;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnOpen(object arg = null)
    {
        base.OnOpen(arg);
        string str = (string)arg;
        M_text.text = str;
        PlayAnimation("AchievementTips_Open", OnAnimFinish);

        // 保底5秒一定关闭
        DelayInvoker.DelayInvoke(TagFlag, 3.2f, DelayClose, null);
    }

    protected override void OnClose(object arg = null)
    {
        base.OnClose(arg);
        if (DelayInvoker.ContainInvoke(TagFlag))
        {
            DelayInvoker.CancelInvoke(TagFlag);
        }
    }

    private void DelayClose(object[] args)
    {
        OnAnimFinish();
    }

    public void OnAnimFinish()
    {
        UIManager.Instance.CloseWidget(UIDef.AchievementTipsWidget, DynamicUIRoot.SystemMsgRoot.transform, false);
    }
}
