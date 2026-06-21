///--------------------------------------------------------------------
/// 文件名   :   SoundButton.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/05/05 10:43:32
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using System;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundButton : MonoBehaviour
{
    private string SoundName = "UI_Click_On_Sound";

    public AK.Wwise.Event akEvent = new AK.Wwise.Event();

    private void Awake()
    {
        var btn= this.transform.GetComponent<Button>();
        if (btn!=null)
        {
            btn.onClick.AddListener(PlaySound);
        }
    }

    [Button("播放音效")]
    public void PlaySound()
    {
        if (!string.IsNullOrEmpty(akEvent.Name))
        {
            SoundName = akEvent.Name;
        }
        AkSoundEngine.PostEvent(SoundName, gameObject);
    }
}
