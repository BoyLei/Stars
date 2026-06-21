
using DG.Tweening;
using StarProject.Service.Sound;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[XLua.LuaCallCSharp]
public class JButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler,
IDragHandler, IBeginDragHandler, IEndDragHandler
{

    public bool NeedRest = true;
    
    public enum ButtonState
    {
        NORMAL = 1,
        /// <summary>
        /// 按下时
        /// </summary>
        PRESS = 2,
        /// <summary>
        /// 禁用时
        /// </summary>
        DISABLE = 3,
    }

    private ButtonState state;
    public ButtonState State
    {
        get { return state; }
        set
        {
            state = value;
        }
    }

    /// <summary>
    /// 点击显示图片
    /// </summary>
    public Image PressChangeImage;
    public CanvasGroup PressChangeAlpha;
    private RectTransform targetRectTransform;
    public bool DoScale = true;
    public float ScaleDur = .1f;

    public float LongPressThreshold = .3f;


    private float pTick;
    public float PressThreshold = .03f;

    public float AlphaHitTestThreshold = 0f;

    private Vector3 mouseStartPosition;

    private float pressTick;
    private bool pressing;
    public bool Pressing { get { return pressing; } set { pressing = value; } }
    private bool dragStarted;
    private bool longPressStarted;
    private bool isInside;//PoinerUp手指是否在显示区域内

    [Sirenix.OdinInspector.LabelText("是否需要按钮点击CD")]
    [SerializeField]
    private bool hasInputCd = false;
    private bool isInInputCD = false;
    public bool IsInInputCD
    {
        get => isInInputCD;
        set
        {
            isInInputCD = value;
            OnSetInputCdClick();
        }
    }
    [SerializeField]
    [Sirenix.OdinInspector.ShowIf("hasInputCd")]
    [Sirenix.OdinInspector.LabelText("按钮CD时间 单位毫秒")]
    private float inputCDTimeMS = 500f;
    [SerializeField]
    [Sirenix.OdinInspector.ShowIf("hasInputCd")]
    [Sirenix.OdinInspector.LabelText("按钮CD中 转cd")]
    private Image cdImage;
    //[SerializeField]
    private float inputCDTimePassedMs = 0f;


    private const float SQR_THRESHOLD = 1000f;

    private static readonly Vector3 Scale1 = Vector3.one;
    private static readonly Vector3 Scale2 = new Vector3(.9f, .9f);


    public Vector3 offset;

   
    public UnityAction<GameObject> AddtionAction { get; set; }
    public UnityAction<GameObject> AddtionPressDown { get; set; }
    public UnityAction<GameObject> ReplaceClickAction { get; set; }
    public UnityAction<GameObject> OnClick;
    public UnityAction<GameObject> OnDragStart;
    public UnityAction<GameObject> OnDraging;
    public UnityAction<GameObject> OnDragEnd;
    public UnityAction<GameObject> OnPressDown;
    public UnityAction<GameObject> OnLongPressDown;
    public UnityAction<GameObject> OnLongPressUp;
    public UnityAction<GameObject> OnTouchDownAction;
    public UnityAction<GameObject> OnTouchUpAction;

    private Tweener tweener;

    private Image image;


    private string click_se = "UI_Click_On_Sound";
    public string PlaySoundName = "";

    private ScrollRect ScrollRectParent;

    bool canMakeLongPress = true;


    Vector3 oldPos;

    //是否关闭tips
    public bool DontCloseTips = false;

    public Image Image
    {
        get
        {
            if (image == null)
            {
                image = gameObject.GetComponent<Image>();
            }

            return image;
        }
    }

    private bool _unClick = false;
    public bool UnClick
    {
        get
        {
            return _unClick;
        }
        set
        {
            if (_unClick!= value)
            {
                _unClick = value;
                OnSetClick();
            }
        }
    }

    /// <summary>
    /// 禁止点击
    /// </summary>
    public bool ForbidClick = false;

    private Dictionary<MaskableGraphic, Color> MaskableGraphics;
    private void OnSetClick()
    {
        if (MaskableGraphics!=null && MaskableGraphics.Count > 0)
        {
            foreach (var item in MaskableGraphics)
            {
                float a = _unClick ? 0.3f : item.Value.a;
                item.Key.color = new Color(item.Value.r, item.Value.g, item.Value.b, a);
            }
        }
    }

    private void OnSetInputCdClick() 
    {
        if (MaskableGraphics!=null && MaskableGraphics.Count > 0)
        {
            foreach (var item in MaskableGraphics)
            {
                float a = !CanInput() ? 0.3f : item.Value.a;
                item.Key.color = new Color(item.Value.r, item.Value.g, item.Value.b, a);
            }
        }
    }

    /// <summary>
    /// 清除InputCD，复用列表中的JButton会需要
    /// </summary>
    public void ClearInputCD() 
    {
        if (hasInputCd)
        {
            IsInInputCD = false;
            inputCDTimePassedMs = 0;
        }
    }

    private bool CanInput() 
    {
        if (!hasInputCd) return true;

        return !IsInInputCD;
    }

    public AK.Wwise.Event akEvent = new AK.Wwise.Event();

    private void Awake()
    {
        targetRectTransform = transform.GetComponent<RectTransform>();
        if (targetRectTransform == null)
        {
            targetRectTransform.AddComp<RectTransform>();
        }

        oldPos = transform.localPosition;

        if (GetComponentInParent<ScrollRect>() != null)
        {
            ScrollRectParent = GetComponentInParent<ScrollRect>();
        }

        if (PressChangeImage != null)
        {
            PressChangeImage.color = new Color(PressChangeImage.color.r, PressChangeImage.color.g, PressChangeImage.color.b, 0);
        }

        if (PressChangeAlpha != null)
        {
            PressChangeAlpha.alpha = 0;
        }

        if (Image != null)
        {
            image.alphaHitTestMinimumThreshold = AlphaHitTestThreshold;
        }

        SoundManager.Instance.EnsureWwiseEventBank(akEvent);
        MaskableGraphics = new Dictionary<MaskableGraphic, Color>();
        MaskableGraphics.Clear();
        var graphics = transform.GetComponentsInChildren<MaskableGraphic>();
        if (graphics != null && graphics.Length > 0)
        {
            foreach (var graphic in graphics)
            {
                MaskableGraphics.Add(graphic, graphic.color);
            }
        }

        if (cdImage != null)
        {
            cdImage.type = Image.Type.Filled;
            cdImage.fillMethod = Image.FillMethod.Radial360;
            cdImage.fillAmount = 0;
        }

        OnSetClick();
        OnSetInputCdClick();
    }

    private void ValidClick()
    {
        if (OnPressDown == null && OnLongPressDown == null && OnLongPressUp == null)
        {
            // 如果没有添加长按的回调
            // 就不检测长按的逻辑
            // 长按后响应down点击的回调
        }
        else
        {
            if (pressTick > LongPressThreshold)
            {
                return;
            }
        }

        if (longPressStarted)
        {
            return;
        }

        if (dragStarted)
        {
            return;
        }

        //TODO 有些界面Button按钮显示区域和点击区域不同需要一起修改掉，但暂时版署版本很重要时间紧，先临时屏蔽检测，回头再打开检测统一时间所有界面处理干净 @caojie
        /*if (!isInside)
        {
            return;
        }*/

        var call = ReplaceClickAction;
        ReplaceClickAction = null;
        if (call != null)
        {
            call(gameObject);
            return;
        }

        InvokeOnclick();
    }

    public void InvokeOnclick()
    {
        if (OnClick != null)
        {
            if (AddtionAction != null)
            {
                AddtionAction(gameObject);
                AddtionAction = null;
            }
            PlaySound();
            OnClick.Invoke(gameObject);

            if (!DontCloseTips)
            {
                //ModuleManager.Instance.SendMessage(ModuleDef.ItemTipsModule, "OnCloseTips", new object[] { });
            }
        }
    }

    public void PlaySound()
    {
        if (!string.IsNullOrEmpty(PlaySoundName))
        {
            click_se = PlaySoundName;
        }
        else if (!string.IsNullOrEmpty(akEvent.Name))
        {
            click_se = akEvent.Name;
        }
        SoundManager.Instance.PlayEventName(click_se, gameObject, gameObject);
        // AkSoundEngine.PostEvent(click_se, gameObject);
    }

    void OnDisable() 
    {
        if (NeedRest)
        {
            pressing = false;
        }

        state = ButtonState.NORMAL;
        longPressStarted = false;
        ClearInputCD();
    }

    protected void OnDestroy()
    {
        if (tweener != null)
        {
            tweener.Kill();
        }
        //SoundManager.Instance.UnLoadWwiseEventBank(akEvent);
    }

    private void Down()
    {
        //播放音效
        if (!string.IsNullOrEmpty(click_se))
        {
            //if(SeManager.Instance!=null)
            //SeManager.Instance.PlaySe(click_se);
        }

        if (!CanInput()) return;
        if (hasInputCd)
        {
            IsInInputCD = true;
            inputCDTimePassedMs = 0;
        }

        pressing = true;
        pressTick = 0f;
        pTick = 0f;
        canMakeLongPress = true;

        mouseStartPosition = Input.mousePosition;
        State = ButtonState.PRESS;

        if (OnTouchDownAction != null)
        {
            OnTouchDownAction(gameObject);
        }

        if (PressChangeImage != null)
        {
            PressChangeImage.color = new Color(PressChangeImage.color.r, PressChangeImage.color.g, PressChangeImage.color.b, 1);
        }

        if (PressChangeAlpha != null)
        {
            PressChangeAlpha.alpha = 1;
        }

        if (offset != new Vector3(0, 0, 0))
        {
            transform.localPosition = oldPos + offset;
        }

        if (DoScale)
        {
            if (tweener != null)
            {
                tweener.Kill(true);
            }
            tweener = transform.DOScale(Scale2, ScaleDur);
        }
    }

    private void Up()
    {
        if (!pressing)
        {
            dragStarted = false;
            return;
        }

        pressing = false;

        if (OnTouchUpAction != null)
        {
            OnTouchUpAction(gameObject);
        }

        if (PressChangeImage != null)
        {
            PressChangeImage.color = new Color(PressChangeImage.color.r, PressChangeImage.color.g, PressChangeImage.color.b, 0);
        }

        if (PressChangeAlpha != null)
        {
            PressChangeAlpha.alpha = 0;
        }

        if (tweener != null)
        {
            tweener.Kill(true);
        }

        if (offset != new Vector3(0, 0, 0))
        {
            transform.localPosition = oldPos;
        }

        if (longPressStarted)
        {
            longPressStarted = false;
            if (OnLongPressUp != null)
            {
                OnLongPressUp(gameObject);
            }

            if (DoScale)
            {
                transform.localScale = Scale1;
            }
        }
        else
        {
            if (DoScale)
            {
                transform.localScale = Scale1;
            }

            ValidClick();
        }

        dragStarted = false;
    }

    public void CancleLongPress()
    {
        if (longPressStarted)
        {
            transform.localScale = Vector3.one;
            Up();
        }
    }

    public void OnPointerDown(PointerEventData ed)
    {
        if (ForbidClick)
        {
            return;
        }
        if (ed.button == PointerEventData.InputButton.Left)
        {
            //PassEvent(ed, ExecuteEvents.pointerDownHandler);
            Down();
        }
    }

    public void OnPointerUp(PointerEventData ed)
    {
        if (ForbidClick)
        {
            return;
        }
        if (ed.button == PointerEventData.InputButton.Left)
        {
            isInside = RectTransformUtility.RectangleContainsScreenPoint(targetRectTransform, ed.position, ed.pressEventCamera);
            Up();
        }
    }

    void SetCdImageFillAmount(float per) 
    {
        if (cdImage != null)
        {
            cdImage.fillAmount = per;
        }
    }

    protected void Update()
    {
        if (hasInputCd && IsInInputCD)
        {
            inputCDTimePassedMs += Time.deltaTime * 1000;
            if (inputCDTimePassedMs >= inputCDTimeMS)
            {
                IsInInputCD = false;
                inputCDTimePassedMs = 0f;
            }

            //大于2秒才显示cd转圈图
            if (inputCDTimeMS > 2000f)
            {
                SetCdImageFillAmount(inputCDTimePassedMs / inputCDTimeMS);
            }
        }

        if (!pressing)
        {
            return;
        }

        pressTick += Time.deltaTime;
        pTick += Time.deltaTime;

        if (pTick > PressThreshold)
        {
            pTick = 0;
            if (OnPressDown != null)
            {
                OnPressDown(gameObject);
            }
        }

        if ((Input.mousePosition - mouseStartPosition).sqrMagnitude > SQR_THRESHOLD)
        {
            canMakeLongPress = false;
        }

        if (pressTick > LongPressThreshold && (Input.mousePosition - mouseStartPosition).sqrMagnitude < SQR_THRESHOLD)
        {
            if (!longPressStarted && canMakeLongPress)
            {

                if (OnLongPressDown != null)
                {
                    longPressStarted = true;
                    OnLongPressDown(gameObject);
                }
                if (AddtionPressDown != null)
                {
                    AddtionPressDown(gameObject);
                    AddtionPressDown = null;
                }
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ScrollRectParent != null)
        {
            ScrollRectParent.OnDrag(eventData);
        }

        if (OnDraging != null)
        {
            OnDraging(gameObject);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (ScrollRectParent != null)
        {
            ScrollRectParent.OnEndDrag(eventData);
        }

        if (OnDragEnd != null)
        {
            OnDragEnd(gameObject);
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (GetComponentInParent<ScrollRect>() != null)
        {
            ScrollRectParent = GetComponentInParent<ScrollRect>();
        }

        if (ScrollRectParent != null)
        {
            ScrollRectParent.OnBeginDrag(eventData);
            dragStarted = true;
        }

        if (OnDragStart != null)
        {
            OnDragStart(gameObject);
        }
    }

    public static GameObject PickUIObjAt(Vector3 pos)
    {
        var ed = new PointerEventData(EventSystem.current);
        ed.position = pos;
        var list = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ed, list);
        if (list.Count > 0)
        {
            var hit = list[0];
            return hit.gameObject;
        }
        return null;
    }

    /*
    //把事件透下去
    public void PassEvent<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function)
        where T : IEventSystemHandler
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);
        GameObject current = data.pointerCurrentRaycast.gameObject;
        for (int i = 0; i < results.Count; i++)
        {
            if (current != results[i].gameObject) 
            {
                ExecuteEvents.Execute(results[i].gameObject, data, function);
                return;
                //RaycastAll后ugui会自己排序，如果你只想响应透下去的最近的一个响应，这里ExecuteEvents.Execute后直接break就行。
            }
        }
    }
    */

}

