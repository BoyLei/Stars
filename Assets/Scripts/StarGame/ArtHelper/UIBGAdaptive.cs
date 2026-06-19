using SGF.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 背景图片自适应
/// </summary>
public class UIBGAdaptive : MonoBehaviour
{

    static Canvas _Canvas;
    public Canvas Canvas
    {
        get
        {
            if (_Canvas == null)
            {
                _Canvas = transform.root.GetComponentInParent<Canvas>();
            }
            if (_Canvas == null)
            {
                var _canvasGob = GameObject.Find("Canvas");
                if (_canvasGob != null)
                {
                    _Canvas = _canvasGob.GetComponent<Canvas>();
                }
            }
            if (_Canvas == null && UIManager.Instance != null)
            {
                _Canvas = UIManager.Instance.M_Canvas;
            }
            return _Canvas;
        }
        set => _Canvas = value;
    }

    private void Awake()
    {
        Adaptive();
    }

    private void Adaptive()
    {
        Image sp = GetComponent<Image>();
        RawImage rawImage = GetComponent<RawImage>();
        float iconWidth = 0f;
        float iconHeight = 0f;
        var rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            iconWidth = rect.rect.width;
            iconHeight = rect.rect.height;
        }
        bool isFind = false;
        if (!isFind && sp != null && sp.sprite != null)
        {
            iconWidth = sp.sprite.rect.width;
            iconHeight = sp.sprite.rect.height;
            isFind = true;
            Debug.Log($"UIBGAdaptive Image sprite name={gameObject.name},width={iconWidth},height={iconHeight}");
        }
        if (!isFind && rawImage != null && rawImage.texture != null)
        {
            iconWidth = rawImage.texture.width;
            iconHeight = rawImage.texture.height;
            isFind = true;
            Debug.Log($"UIBGAdaptive RawImage texture name={gameObject.name},width={iconWidth},height={iconHeight}");
        }
        float canvasWidth = 0;
        float canvasHeight = 0;
        if (Canvas != null)
        {
            canvasWidth = Canvas.GetComponent<RectTransform>().rect.width;
            canvasHeight = Canvas.GetComponent<RectTransform>().rect.height;
        }
        Debug.Log($"UIBGAdaptive name={gameObject.name},currentResolution.width={Screen.currentResolution.width},currentResolution.height={Screen.currentResolution.height}");
        Debug.Log($"UIBGAdaptive name={gameObject.name},Screen.width={Screen.width},Screen.height={Screen.height}");
        Debug.Log($"UIBGAdaptive name={gameObject.name},canvas.width={canvasWidth},canvas.height={canvasHeight}");

        //if (isFind)
        {
            //bool widthIsExceed = iconWidth >= Screen.currentResolution.width;
            //bool heightIsExceed = iconHeight >= Screen.currentResolution.height;

            bool widthIsExceed = iconWidth >= canvasWidth;
            bool heightIsExceed = iconHeight >= canvasHeight;

            if (widthIsExceed && heightIsExceed)
            {
                // 没超过就不需要合并了
            }
            else
            {
                sp?.SetNativeSize();
                rawImage?.SetNativeSize();
                //float scaleWidth = (float)Screen.currentResolution.width / iconWidth;
                //float scaleHeight = (float)Screen.currentResolution.height / iconHeight;
                float scaleWidth = canvasWidth / iconWidth;
                float scaleHeight = canvasHeight / iconHeight;
                float scale = Mathf.Max(scaleWidth, scaleHeight);
                transform.SetLocalScale(Vector3.one * scale);
                Debug.Log($"UIBGAdaptive name={gameObject.name},scale={scale}");
            }
        }
    }


    [ContextMenu("刷新")]
    public void ForceExcute()
    {
        Adaptive();
    }

}
