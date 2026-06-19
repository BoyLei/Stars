using SGF.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

public class UIAutoFitable : MonoBehaviour
{
    //A-E=S
    // Start is called before the first frame update
    RectTransform P_rt_cut;
    Vector2 p_rt_size;
    RectTransform ancleBG;
    AspectRatioFitter m_arf;
    Vector2 Vector2Scale = new Vector2();
    private const float pix4CutMin = 30;//系统不准我加50，
    private const float pix4CutMin1 = 30;//系统不准我加50，
    ////防止美术透明的10个像素////防止美术透明的10个像素////防止美术透明的10个像素///防止美术透明的10个像素///防止美术透明的10个像素///防止美术透明的10个像素///防止美术透明的10个像素///防止美术透明的10个像素///防止美术透明的10个像素
    private int extendBGPix = 10;//防止美术透明的10个像素

    int widthPercent = 0;

    private float MiniPixPhoneScalePara = 1;
    float parent_ratio;
    void Start()
    {
        widthPercent = (int)((float)Screen.currentResolution.width * 0.035f);//100 = 3.5 1000 = 35
        if (widthPercent >= 50)
        {
            widthPercent = 50;
        }
        else if(widthPercent <= 35)
        {
            widthPercent = 35;
        }

        /* //让低分辨率也拥有向上拉的权力，不论多少分辨率，首先降低其高（最小值）缩放作为基础，然后向上拉
         //    也不能说向下适配就准寻另一种排布，不至于像素小的时候有另一种布局而只是减少分辨率即可
         //    他也享受上下拉的共同特点
         //默认标尺1920 * 1080 比例尺后续已经处理了，就是没处理像素，高像素就是上下拉，左右拉
         //低像素就是缩放基础
         if (Screen.currentResolution.height <= 1080)
         {
             MiniPixPhoneScalePara = (float)Screen.currentResolution.height / 1080f;
         }
         else
         {
             MiniPixPhoneScalePara = 1;
         }*/
        //写在UICanvasInit，放大去了，确实
        //适配问题缩放就不全屏了，应该要放大，放大在canvasInit上
        //然后根据这个适配unity再次调整显示全，缩小，再根据我的细致分类

        /*Screen.currentResolution //就是Game下的设定。再[sim模拟模式下][手机真实分辨率 = Game下设定 = 分辨率]所以是一致的
            Screen.width //是屏幕手机的*/

        /*    // 1.图片的Top设置为100（偏移的Max是负数）对应Right、Top
            GetComponent<RectTransform>().offsetMax = new Vector2(GetComponent<RectTransform>().offsetMax.x, -100);

            // 2.图片的Bottom设置为120   对应Left、Bottom
            GetComponent<RectTransform>().offsetMin = new Vector2(GetComponent<RectTransform>().offsetMin.x, 120);

            // 3.改变RectTransform的宽和高（注：测试的时候锚点中不要选择带蓝色线的适配方式，那样会被拉伸的）
            GetComponent<RectTransform>().sizeDelta = new Vector2(100, 200);

            // 4.改变RectTransform的postion（x,y,z）
            GetComponent<RectTransform>().anchoredPosition3D = new Vector3(70, 80, 90);

            // 5.改变锚点的位置
            GetComponent<RectTransform>().anchoredPosition = new Vector2(posx, posy);*/



#if UNITY_STANDALONE && !UNITY_EDITOR

#else
        //适配背景
        SetBgFullOfScreen();
        //确认上级4边裁剪
        ConfirmFitting4SideCutting();
        //通过取新分辨率差设置适配层次（绿层）的适配用缩放和拉伸的手段
        SetRatioLayerFittingByRatioNScale();
#endif


    }

    [ContextMenu("刷新")]

    public void ForceExcute()
    {




#if UNITY_STANDALONE && !UNITY_EDITOR

#else
        UIManager.Instance.M_Canvas.GetComponent<UICanvasInit>().OnMyStart();
        //适配背景
        //SetBgFullOfScreen();
        //确认上级4边裁剪
        ConfirmFitting4SideCutting();
        //通过取新分辨率差设置适配层次（绿层）的适配用缩放和拉伸的手段
        SetRatioLayerFittingByRatioNScale();
#endif


    }



    private void SetRatioLayerFittingByRatioNScale()
    {
        //刷新现在safe分辨率
        p_rt_size = P_rt_cut.rect.size;//裁剪后剩余的//.sizeDelta;用Screen.safeArea也一样
        if (p_rt_size.y == 0 && p_rt_size.x == 0)
        {
            parent_ratio = 1;
        }
        else
        {
            parent_ratio = p_rt_size.x / p_rt_size.y;//更新以下
        }





        m_arf = GetComponent<AspectRatioFitter>();
        //【【【【【【【【【【【【【【2.35以上是横向Scale；【2.35新（2老）~1.766是规划的横向拉伸；|||||||||||||】】】】】】】】】】
        //fitInP，一定是放大 大于1.766//【默认系统】
        if (m_arf.aspectRatio < parent_ratio)
        {
            //他值比我大
            //长比我长，我的长按照他的比例尺进行缩放
            //他长度/我长  等价于 他比例处理我比例 等价于 内部计算  ；但是有一个机制别忘了他长于我的时候我们的高一定相等
            //内部逻辑，1不要缩放倍数太大所以一定情况下碎屏会拉伸（请注意内部适配因为横向拉会默认修改大小），2美术习惯1920验收所以2340是拓展出来的；3，所以再比例尺到2（2160/1080）的时候也是允许设置为2的
            if (parent_ratio <= 3f) //2.35f
            {
                m_arf.aspectRatio = parent_ratio;//1.778之上 直接调整到[1.778~2.35新（2老）]
            }
            else //>=2.35新（2老） 那就要缩放Scale了，所以1920*1080两边适配都可能有变数，太剑就Scale，高度一直拉伸100%（比例在变小注意）:内部高度是开放的
            {
                //保证2.35新（2老）之内的是拉伸，2.35新（2老）之外的是缩放
                //1.778之上 直接调整到[2.35新（2老）]
                m_arf.aspectRatio = 3f;//2.35f;
                //2.05部分，后续缩放不是2.05/1.778，而是2.05/2
                transform.localScale = new Vector3(MiniPixPhoneScalePara * parent_ratio / m_arf.aspectRatio, MiniPixPhoneScalePara, 1f);
            }


        }
        else//【【【【【【||||||||||| 1、7666~1.333(1.01)以上是纵向拉升扩大空间】；1.333(1.01)以下进行横向缩放】】】】】】】】】】】
        //小于1.766
        {
            //他比我短，p他比我m高，我们必然长度相同，
            //他高/我高 = x/他  /  X/我 = 倒过来 我/他
            //transform.localScale = new Vector3(1f, m_arf.aspectRatio /p_p ,1f);
            //高度缩放字不好看
            //一定是变小的；他比我高；他分母比我大；他值比我小；我取他的值，我在缩小


            //UI在互相接近的趋势，值在小于16：9，在小于1.77778
            //元神上下是没有Scale的一直是拉，基于空间变大，（那尽量是上对其，和左上对齐，和不会出现由于上下拉伸倒是内部等比变宽的情况）
            //在子集没有按照百分比缩放，是绝对值的情况下，实际是上下空间放大（但确实有变小的情况比如横的比较小，高度还比较大）
            //所以没有缩放概念，就是放大空间，也不必阀值比如小于1.67778（少0.1）



            //不会出现盖不住的情况；缩放不好看；有些如果因为程序设计尽量采用不会上下变大导致内容变多的情况（itemView动态算的），当然美术可以提醒(哪些会变化），但是绝对不会由于高度到宽度的变化在1.776以下
            if (parent_ratio >= 1.01f)//1.333f
            {
                m_arf.aspectRatio = parent_ratio;//1.778之下 直接调整到[1.778~1.333(1.01)]
            }
            else
            {

                m_arf.aspectRatio = 1.01f;//1.333f;//1.778 和 1.333(1.01)之下 直接调整到[1.333(1.01)]
                //1.333(1.01)/1.2几了，小额倍数缩放了，就不是1.777/1.2几了
                transform.localScale = new Vector3(MiniPixPhoneScalePara * m_arf.aspectRatio / parent_ratio, MiniPixPhoneScalePara, 1f);
                //但是这里你注意，变成纵屏我就不管了
            }



        }
    }

    private void ConfirmFitting4SideCutting()
    {
        P_rt_cut = transform.parent.GetComponent<RectTransform>();
        //2.35新（2老）以上是横向Scale；2~1.766是规划的横向拉伸；1、7666~1.333(1.01)以上是纵向拉升扩大空间；1.333(1.01)以下进行横向缩放
        p_rt_size = P_rt_cut.rect.size;//.sizeDelta;
 
        Debug.Log(p_rt_size);
        parent_ratio = p_rt_size.x / p_rt_size.y;

        //p_p == window = canvas
        //Android手机设置
        //周申概念是，如果如果手机分辨率大于1920一定挖孔，取分辨率，手机型号
        //这里就可以设置上级
        //当前屏幕窗口像素宽约等于  【分辨率 = canvas = 像素宽度屏幕】  Screen.width 小于90就90，大于90归纳给系统,

        float eachOffsetleft;
        float eachOffsetDown;
        float eachOffsetRight;
        float eachOffsetTop;
        if (Screen.safeArea.x == 0)//右侧不必判断，左右对称，全面屏左右完全没有安全-我不必给他安全=给他全面屏
        {
            eachOffsetleft = Screen.safeArea.x;//Screen.width - Screen.safeArea.width;/*(Screen.width - Screen.safeArea.width) / 2f;*///Mathf.Max((Screen.width - Screen.safeArea.width) / 2f, pix4CutMin);
            eachOffsetDown = Screen.safeArea.y;//还是下面0用xs max
            eachOffsetRight = Screen.currentResolution.width - Screen.safeArea.x - Screen.safeArea.width; //Screen.width - Screen.safeArea.x - Screen.safeArea.width;//全局宽 - 左侧非安全区 - 安全宽度
            eachOffsetTop = Screen.currentResolution.height - Screen.safeArea.y - Screen.safeArea.height;//Screen.height - Screen.safeArea.y - Screen.safeArea.height;//高度 - 下方 - 安全高度
        }
        else
        {
            
            eachOffsetleft = Screen.safeArea.x + widthPercent;//如果刘海挖空，就左右对称再宽一点，拓展当前分辨率的0.035f
            eachOffsetDown = Screen.safeArea.y;//还是下面0用xs max
            eachOffsetRight = Screen.currentResolution.width - Screen.safeArea.x - Screen.safeArea.width + widthPercent; //Screen.width - Screen.safeArea.x - Screen.safeArea.width;//全局宽 - 左侧非安全区 - 安全宽度
            eachOffsetTop = Screen.currentResolution.height - Screen.safeArea.y - Screen.safeArea.height;//Screen.height - Screen.safeArea.y - Screen.safeArea.height;//高度 - 下方 - 安全高度
        }

     
        //Screen.cutouts[]//什么点挖了多大的东西
        //机器配置模拟看以下Screen.currentResolution  Screen.width * Screen.height都可以
        //Screen.safeArea
        //左，下
        P_rt_cut.offsetMin = new Vector2(eachOffsetleft, eachOffsetDown);//挖也是左侧挖
        //右，上                                                //不对呀
        P_rt_cut.offsetMax = new Vector2(-1 * eachOffsetRight, -1 * eachOffsetTop);//这里两个都要传递负数
    }

    private void SetBgFullOfScreen()
    {
        if (transform.parent.parent.Find("BackGround") != null)
        {
            ancleBG = transform.parent.parent.Find("BackGround").GetComponent<RectTransform>();

            //我防止美术留两个像素，所以我外面拓展【10个像素】，这个可是性能问题
            //注意注意！！！
            //ancleBG.sizeDelta = new Vector2(Screen.width + extendBGPix, Screen.height + extendBGPix);//宽等于高在于：是内部放的最大Ext的比例尺保证显示并且是像素绝对尺寸，所以要用最大的来调整长度进行1的等比放大，取得长宽最大还不如明确细化一定长是最长的因为我们适配横屏
            ancleBG.sizeDelta = new Vector2(Screen.currentResolution.width + extendBGPix, Screen.currentResolution.height + extendBGPix);//宽等于高在于：是内部放的最大Ext的比例尺保证显示并且是像素绝对尺寸，所以要用最大的来调整长度进行1的等比放大，取得长宽最大还不如明确细化一定长是最长的因为我们适配横屏
                                                                                                                                         //永远填不满的模式，FitInP
        }

    }


}
