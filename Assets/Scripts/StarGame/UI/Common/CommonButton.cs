///--------------------------------------------------------------------
/// 文件名   :   CommonButton.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/12/06 16:06:55
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CommonButton : MonoBehaviour
{
    private Button m_button;
    private Image m_image;
    private Text m_text;
    public int Index;
    public UnityAction<int> ClickAction;
    public bool IsSureBtn=false;
    private void Awake()
    {
        m_button = GetComponent<Button>();
        m_image = GetComponent<Image>();
        m_text = transform.Find("Text").GetComponent<Text>();
    }

    private void OnEnable()
    {
        if (m_button != null)
        {
            m_button.onClick.AddListener(OnClickButtonHandler);
        }
    }


    private void OnClickButtonHandler()
    {
        PlaySound(IsSureBtn ? "UI_Click_On_Sound" : "UI_Click_Off_Sound");
        ClickAction?.Invoke(Index);
    }

    private void OnDisable()
    {
        if (m_button != null)
        {
            m_button.onClick.RemoveListener(OnClickButtonHandler);
        }

    }

    public string Text
    {
        get
        {
            return m_text.text;
        }
    }

    public void SetImage(Sprite sprite)
    {
        if (m_image != null)
        {
            m_image.sprite = sprite;
        }
    }

    public void SetText(string text)
    {
        if (m_text != null)
        {
            m_text.text = text;
        }
    }

    public void SetTextColor(Color color)
    {
        if (m_text != null)
        {
            m_text.color = color;
        }
    }

    public void PlaySound(string soundName)
    {
         AkSoundEngine.PostEvent(soundName, gameObject);
    }
}
