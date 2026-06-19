using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;

public class ScreenEffectDebug : MonoBehaviour
{

    public Text UITex;
    public UIParticle uiParticle;
    // Start is called before the first frame update
    void Start()
    {
        UITex.text = "(" + Screen.width + "," + Screen.height + ")("+ uiParticle.scale3D.x+","+ uiParticle.scale3D.y+")";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
