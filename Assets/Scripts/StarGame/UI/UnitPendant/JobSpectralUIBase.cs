using SkillEditor;
using StarProject.Game.Player;
using UnityEngine;

public class JobSpectralUI : MonoBehaviour
{
    protected EntityCtrlBase m_entityCtrl;

    protected bool m_isShow = false;

    public virtual void Init(EntityCtrlBase entityCtrlBase)
    {
        m_entityCtrl = entityCtrlBase;
        if (m_entityCtrl == null)
        {
            return;
        }

        RegisterAttribute();
    }

    protected virtual void Reset()
    {
        m_isShow = false;
    }

    public virtual void Release()
    {
        if (m_entityCtrl == null || m_entityCtrl.Data == null)
        {
            return;
        }
        UnRegisterAttribute();
        m_entityCtrl = null;
    }


    protected virtual void RegisterAttribute()
    {

    }

    protected virtual void UnRegisterAttribute()
    {

    }

    public virtual void SetCountdown(SpTimeShowTypeEnum spTimeShowTypeEnum, float countdown, float startCountdown)
    {

    }

    public virtual void SetOpenCountdown(SpTimeShowTypeEnum spTimeShowTypeEnum, bool isOpen)
    {

    }

    public virtual void SetShow(bool isShow)
    {
        m_isShow = isShow;
    }

}
