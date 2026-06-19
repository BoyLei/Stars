///--------------------------------------------------------------------
/// 文件名   :   CommonBoxWidget
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   "2022/12/06 15:42:49"
/// 创建人   :   "zhaoerdong"
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using SGF.UI.Framework;
using SGF.Unity;
using StarProject.Service.Business;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProjectDef;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XLua;

[LuaCallCSharp]
public class CommonBoxWidget : UIWidget
{
    private Text m_Tile;                     // 标题
    private TextMeshProUGUI m_Content;       // 提示内容
    private RectTransform m_Button;          // 按钮item
    private HorizontalLayoutGroup m_Buttons; // 按钮父节点
    private Toggle m_Toggle;                 // 今日不在提示复选康
    private TextMeshProUGUI ItemIcon;       // 道具数量
    private TextMeshProUGUI ItemCount;       // 道具数量


    private CommonBoxWidgetArgs commonBox;

    public TextMeshProLink textMeshProLink;
    public List<string> SureBttns = new() {
     "SURE",
    };

    private string pattern = @"\[image:(.*?)\]";

    protected override void Awake()
    {
        m_Content = transform.Find("Cut4Cam90/Content").GetComponent<TextMeshProUGUI>();
        m_Button = transform.Find("Cut4Cam90/Button").GetComponent<RectTransform>();
        m_Buttons = transform.Find("Cut4Cam90/Buttons").GetComponent<HorizontalLayoutGroup>();
        m_Tile = transform.Find("Cut4Cam90/Tile").GetComponent<Text>();
        m_Toggle = transform.Find("Cut4Cam90/Toggle").GetComponent<Toggle>();
        if (transform.Find("Cut4Cam90/Image/ItemIcon") != null)
        {

            ItemIcon = transform.Find("Cut4Cam90/Image/ItemIcon").GetComponent<TextMeshProUGUI>();
        }
        ItemCount = transform.Find("Cut4Cam90/Image/ItemCount").GetComponent<TextMeshProUGUI>();
    }

    protected override void OnOpen(object arg = null)
    {
        commonBox = arg as CommonBoxWidgetArgs;
        Show(commonBox);
    }

    public void Show(CommonBoxWidgetArgs commonBox)
    {
        if (commonBox != null)
        {
            if (!string.IsNullOrEmpty(commonBox.Title))
            {
                m_Tile.text = commonBox.Title;
                m_Content.text = commonBox.Content;
                ClearButtons();

                var rectTransform = GameObject.Instantiate(m_Button);
                rectTransform.transform.SetParent(m_Buttons.transform);
                rectTransform.transform.localPosition = Vector3.zero;
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localScale = Vector3.one;

                rectTransform.gameObject.SetActive(true);
                m_Toggle.gameObject.SetActive(false);
                var button = rectTransform.GetComponent<CommonButton>();
                button.IsSureBtn = IsSureBtn("SURE");
                button.SetText(commonBox.BtnText);
                // Color color = Color.white;
                // ColorUtility.TryParseHtmlString(texColor, out color);
                //  button.SetTextColor(color);
                button.Index = 0;
                button.ClickAction = onClickButtonHandler;
            }
            else
            {
                var config = LocalDataManager.Instance.GetWindowDataCell(commonBox.Id);
                if (config == null)
                {
                    StarDebug.LogError($"读取弹窗配置失败 ID{commonBox.Id}");
                    return;
                }
                // 标题
                m_Tile.text = config.Ttile;
                // 内容描述
                string desc = config.Desc;
                var boxParams = commonBox.Params;
                if (boxParams != null && boxParams.Length > 0)
                {
                    // 第一个参数 表明需要连接 url
                    string params0 = boxParams[0] as string;
                    if (params0 != null && params0.Equals("LinkUrl"))
                    {
                        textMeshProLink = GameObjectUtils.EnsureComponent<TextMeshProLink>(m_Content.gameObject);
                        List<string> urls = new();

                        for (int i = 1; i < boxParams.Length; i++)
                        {
                            urls.Add((string)boxParams[i]);
                        }
                        textMeshProLink.SetUrl(urls);
                        textMeshProLink.enabled = true;
                        desc = config.Desc;
                    }
                    else
                    {
                        if (textMeshProLink != null)
                        {
                            textMeshProLink.enabled = false;
                        }
                        desc = string.Format(config.Desc, boxParams);
                    }
                    //第二个参数表示延迟多久确定
                     
                    //if (commonBox.Params.Length > 1 )
                    //{
                    //    if(int.TryParse(commonBox.Params[1].ToString(),out delaytime))
                    //    {
                    //        MonoHelper.RemoveSecTimeUpdateListener(ConfirmCheckFunc);
                    //        MonoHelper.AddSecTimeUpdateListener(ConfirmCheckFunc);
                    //    } 
                    //}

                }
                //延迟
                if( commonBox.delay > 0)
                {
                    delaytime = commonBox.delay;
                    MonoHelper.RemoveSecTimeUpdateListener(ConfirmCheckFunc);
                    MonoHelper.AddSecTimeUpdateListener(ConfirmCheckFunc);
                }


                // 道具名字
                string itemIconName = "";
                if (commonBox.ItemID != 0)
                {
                    // 道具名字
                    var itemCfg = LocalDataManager.Instance.GetItemDataCell(commonBox.ItemID);
                    if (itemCfg != null)
                    {
                        var iconPath = itemCfg.Icon.Split("/");
                        if (iconPath.Length > 0)
                        {
                            itemIconName = iconPath[iconPath.Length - 1];
                        }
                    }
                    // 内容描述
                    MatchCollection matches = Regex.Matches(desc, pattern);
                    foreach (Match match in matches)
                    {
                        string itemOldStr = match.Groups[1].Value;
                        int index = m_Content.spriteAsset.GetSpriteIndexFromName(itemIconName);
                        string image = $"<sprite={index}>";
                        string extractedString = match.Groups[1].Value;
                        string old = $"[image:{extractedString}]";
                        desc = desc.Replace(old, image);
                    }
                    m_Content.text = desc.Replace("\\n", "\n");
                }
                else
                {
                    // 内容描述
                    MatchCollection matches = Regex.Matches(desc, pattern);
                    foreach (Match match in matches)
                    {
                        string extractedString = match.Groups[1].Value;
                      //  Debug.LogError(extractedString);
                        string old = $"[image:{extractedString}]";
                        int index = m_Content.spriteAsset.GetSpriteIndexFromName(extractedString);
                        string image = $"<sprite={index}>";
                        desc = desc.Replace(old, image);
                    }

                    m_Content.text = desc.Replace("\\n", "\n");
                }
                // 今日提醒
                {
                    if (config.GetWarn())
                    {
                        m_Toggle.gameObject.SetActive(true);
                        m_Toggle.isOn = false;
                    }
                    else
                    {
                        m_Toggle.gameObject.SetActive(false);
                    }
                }
                // 按钮
                {
                    ClearButtons();
                    string[] buttonStrs = config.Buttons.Split(",");
                    int count = buttonStrs.Length;

                    for (int i = 0; i < count; i++)
                    {
                        var rectTransform = GameObject.Instantiate(m_Button);
                        rectTransform.transform.SetParent(m_Buttons.transform);
                        rectTransform.transform.localPosition = Vector3.zero;
                        rectTransform.localRotation = Quaternion.identity;
                        rectTransform.localScale = Vector3.one;

                        rectTransform.gameObject.SetActive(true);

                        var button = rectTransform.GetComponent<CommonButton>();
                        button.IsSureBtn = IsSureBtn(config.Events[i]);
                        string path = "";
                        string texColor = "";
                        if (button.IsSureBtn)
                        {
                            path = "Common_Btn_NormalL3";
                            texColor = "# DCD2B4";
                        }
                        else
                        {
                            path = "Common_Btn_NormalL1";
                            texColor = "#69605A";
                        }

                        GetSprite(path, (sp) =>
                        {
                            button.SetImage(sp);
                        });
                       
                        string str = buttonStrs[i];
                        button.SetText(str);
                        
                         Color color = Color.white;
                         ColorUtility.TryParseHtmlString(texColor, out color);
                         button.SetTextColor(color);
                         
                        button.Index = i;
                        button.ClickAction = onClickButtonHandler;
                    }
                }
                // 显示道具图标
                {
                    if (ItemCount != null)
                    {
                        string itemCountStr = "";
                        if (commonBox.IsShowItemCount)
                        {
                            int index = m_Content.spriteAsset.GetSpriteIndexFromName(itemIconName);
                            string image = $"<sprite={index}>";
                            if (ItemIcon != null)
                            {

                                ItemIcon.text = image;
                            }

                            var itemCount = BusinessManager.Instance.GetCurrencyNum(commonBox.ItemID);
                            itemCountStr = string.Format(LanguageManager.Instance.GetLanguageByKey("CurHaveCount"), itemCount);
                        }
                        ItemCount.text = itemCountStr;
                        ItemCount.transform.parent.gameObject.SetActive(commonBox.IsShowItemCount);
                    }
                }

              
                transform.SetAsLastSibling();
            }
        }
    }

    float waittime = 0;
    int delaytime = 0;
    string confirmtext;
    void ConfirmCheckFunc()
    {
        waittime += 1;
        if (waittime > delaytime)
        {
            onClickButtonHandler(1);
            waittime = 0;
        }
        else
        {
            //刷新按钮倒计时显示
            UpdateTimeText(1);
        }
    }

    void UpdateTimeText(int index)
    {
        var button = m_Buttons.transform.GetChild(index).GetComponent<CommonButton>();
        if (confirmtext == null)
        {
            confirmtext = button.Text;
        }
        button.SetText(confirmtext + "("+(int)(delaytime - waittime) + ")");
    }

    public void UpdateContent(string str)
    {
        if (m_Content != null)
        {
            m_Content.text = str;
        }
    }

    private bool IsSureBtn(string eventName)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            return false;
        }
        if (SureBttns == null || SureBttns.Count < 1)
        {
            return false;
        }
        if (SureBttns.Contains(eventName))
        {
            return true;
        }
        return false;
    }

    private void onClickButtonHandler(int index)
    {
        MonoHelper.RemoveSecTimeUpdateListener(ConfirmCheckFunc);
        if (commonBox != null)
        {
            if (commonBox.Id > 0)
            {
                string eventName = "None";
                var config = LocalDataManager.Instance.GetWindowDataCell(commonBox.Id);
                if (config == null)
                {
                    StarDebug.LogError($"读取弹窗配置失败 ID{commonBox.Id}");
                }
                if (index > -1 && index < config.Events.Count)
                {
                    eventName = config.Events[index];
                }
                if (m_Toggle.IsActive() && m_Toggle.isOn)
                {
                    StarProject.Service.Business.BusinessManager.Instance.SaveNotice(commonBox.Id.ToString());
                }
                UIManager.Instance.CloseWidget(commonBox.Path, null, true);
                commonBox.CallBack?.Invoke(eventName);
            }
            else
            {
                UIManager.Instance.CloseWidget(commonBox.Path, null, true);
                string eventName = "SURE";
                commonBox.CallBack?.Invoke(eventName);
            }
        }
    }

    private void ClearButtons()
    {
        int count = m_Buttons.transform.childCount;
        for (int i = count - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(m_Buttons.transform.GetChild(i).gameObject);
        }
    }
    protected override void OnClose(object arg = null)
    {
        ClearButtons();
        base.OnClose(arg);
    }

    protected override void OnDestroy()
    {
        MonoHelper.RemoveSecTimeUpdateListener(ConfirmCheckFunc);
        base.OnDestroy();
    }
}
