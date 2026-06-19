using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//不是单例，可以复用，已经复用
public class UICanvasInit : MonoBehaviour
{
    CanvasScaler canvasScaler;
    // Start is called before the first frame update
    private float MiniPixPhoneScalePara = 1;
    void Start()
    {
#if UNITY_STANDALONE && !UNITY_EDITOR

#else
    OnMyStart();

#endif

    }
    //【2340 配 1：0.25】4种类型都可以 === 【2120 配 1：0.125】4种类型都可以 === 【1920 配 1到 0 都不行】不行
    private Vector2 DesignRes = new Vector2(2120, 1080);//2340界面就小了 1920 2120

    [ContextMenu("刷新")]
    public void OnMyStart()
    {


        /*if (Screen.currentResolution.height <= 1080)
        {
            MiniPixPhoneScalePara = 1080f / (float)Screen.currentResolution.height;
        }
        else
        {
            MiniPixPhoneScalePara = 1;
        }*/

        

        Debug.Log(gameObject.name + "Screen.currentResolution.width:" + Screen.currentResolution.width + "_________" +
            "Screen.height:" + Screen.height);

        canvasScaler = GetComponent<CanvasScaler>();


        //canvasScaler.referenceResolution = new Vector2(1104,883);//new Vector2(Mathf.FloorToInt(Screen.currentResolution.width * MiniPixPhoneScalePara), Mathf.FloorToInt(Screen.currentResolution.height * MiniPixPhoneScalePara));

        canvasScaler.referenceResolution = DesignRes;

        //可以的
        canvasScaler.matchWidthOrHeight = ((float)Screen.currentResolution.width / (float)Screen.currentResolution.height) > (DesignRes.x / DesignRes.y)?1:0.125f;//1920 一般匹配是 0 这个设定，然后有些界面做的时候没按照1920 他的ui要改占20%的界面 0.25f

        //实际机器分辨率 和 手机分辨率比例导致的 左右值:        比我长的>1 结果之是1    比我宽的小于1 结果之 是0                  实际我们就关心 内部长宽比1~3的
        //((float)Screen.currentResolution.width / (float)Screen.currentResolution.height) > (DesignRes.x / DesignRes.y)
        //


        Debug.Log(gameObject.name + "Screen.currentResolution.width:" + Screen.currentResolution.width + "_________" +
          "Screen.width:" + Screen.width);
//#endif
    }
}
