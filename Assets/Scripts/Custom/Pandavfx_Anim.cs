using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[XLua.LuaCallCSharp]
public class Pandavfx_Anim : MonoBehaviour
{
    public Image img;

    public Vector4 MaskPlusTex_ST;



    public void UpdateW(float w)
    {
        MaskPlusTex_ST.w = w;
    }
    // Update is called once per frame
    void Update()
    {
        img.material.SetVector("_MaskPlusTex_ST", MaskPlusTex_ST);
    }
}
