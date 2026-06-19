///--------------------------------------------------------------------
/// 文件名   :   SystemMsgBox
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2024/09/24 12:19:11
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SystemMsgBox : MonoBehaviour
{
    public Text Title;
    public  Text Content;
    public  Text mSureText1;
    public JButton mSure1;
    private System.Action OnClose1;
    
    public  Text mSureText2;
    public JButton mSure2;
    private System.Action OnClose2;
    private void OnEnable()
    {
        mSure1.OnClick+=OnSureClick1;
        mSure2.OnClick+=OnSureClick2;
    }

    
    private void OnDisable()
    {
        mSure1.OnClick-=OnSureClick1;
        mSure2.OnClick+=OnSureClick2;
    }
    
    private void OnSureClick1(GameObject arg0)
    {

        if (OnClose1 != null)
        {
            OnClose1();
        }
        Destroy(gameObject);
    }

    private void OnSureClick2(GameObject arg0)
    {

        if (OnClose2 != null)
        {
            OnClose2();
        }
        Destroy(gameObject);
    }
    public void Show(string title, string content, string btnName1,System.Action action1, string btnName2,System.Action action2)
    {
        Title.text = title;
        Content.text = content;
        mSureText1.text = btnName1;
        OnClose1=action1;
        mSureText2.text = btnName2;
        OnClose2=action2;
    }

}
