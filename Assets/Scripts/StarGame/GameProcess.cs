///--------------------------------------------------------------------
/// 文件名   :   GameProcess.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/10/12 14:02:41
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using SGF.Module.Framework;
using UnityEngine;
using UnityEngine.UI;

public class GameProcess : MonoBehaviour
{

    private EventTable m_tblEvent;
    private Slider ProcessSlider;
    private Text ProcessText;
    private Text ContentText;

    void Awake()
    {
        ProcessSlider = transform.Find("Slider").GetComponent<Slider>();
        ProcessText = transform.Find("Slider/Fill Area/Fill/Text").GetComponent<Text>();
        ContentText = transform.Find("Text").GetComponent<Text>();
    }

    // Start is called before the first frame update
    void Start()
    {
        ProcessSlider.value = 0;
        ProcessText.text = string.Empty;
        ContentText.text = string.Empty;
    }

    public void OnShowText(string content)
    {
        if (ContentText != null)
        {
            ContentText.text = content;
        }
    }

    public void OnProcess(float process)
    {
        if (ProcessSlider != null)
        {
            ProcessSlider.value = process;
        }

        if (ProcessText != null)
        {
            ProcessText.text = $"{(int)(process * 100)}%";
        }
    }

    private void OnDestroy()
    {
        ProcessSlider.value = 0;
        ProcessText.text = string.Empty;
        ContentText.text = string.Empty;
    }

}
