///--------------------------------------------------------------------
/// 文件名   :   PrintEffect.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/11/01 21:29:35
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[XLua.LuaCallCSharp]
[RequireComponent(typeof(Text))]
public class PrintEffect : MonoBehaviour
{
    [XLua.LuaCallCSharp]
    public delegate void PlayComplete();

    private Text m_Text;
    private int index;
    private List<string> List=new List<string>();
    private bool print = false;
    private PlayComplete PlayEndCallBack = null;
    private void Awake()
    {
        m_Text = GetComponent<Text>();
        PlayEndCallBack = null;
    }
    public float Speed=0.1f;
    private float lasttime;

    public void Print(string effect, PlayComplete action)
    {
        m_Text.text = "";
        List.Clear();
        index = 0;
        lasttime = 0;
        PlayEndCallBack = action;
        char[] chars= effect.ToCharArray();
        foreach (var item in chars)
        {
            List.Add(item.ToString());
        }
        print = true;
    }

    public void Show(string text)
    {
        m_Text.text = text;
        List.Clear();
        index = 0;
        lasttime = 0;
        print = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(!print)
        {
            return;
        }

        if(index< List.Count)
        {
            if(Time.time- lasttime>=Speed)
            {
                lasttime = Time.time;
                m_Text.text += List[index];
                index += 1;
            }
 
        }

        if(index>=List.Count)
        {
            print = false;
            PlayEndCallBack?.Invoke();
        }
    }
}
