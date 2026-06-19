using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;


public class UIParticleScreenMatch : MonoBehaviour
{

    Canvas cvs;
    CanvasScaler canvasScaler;
    RectTransform rect;

    public UIParticle uiParticle;
    // Start is called before the first frame update
    void Start()
    {

        if(cvs==null)
        {
            cvs = SGF.UI.Framework.UIManager.Instance.M_Canvas;
            canvasScaler = cvs.GetComponent<CanvasScaler>();
            rect= cvs.GetComponent<RectTransform>();
        }
           
        

        float scaleW = rect.sizeDelta.x / canvasScaler.referenceResolution.x;
        float scaleH = rect.sizeDelta.y / canvasScaler.referenceResolution.y;

        uiParticle.scale3D = new Vector3(scaleW, scaleH,1.0f);
    }

}
