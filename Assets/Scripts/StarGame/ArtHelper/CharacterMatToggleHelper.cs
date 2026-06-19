using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;

[ExecuteAlways]
public class CharacterMatToggleHelper : MonoBehaviour
{
    public List<Material> CharMats;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void CheckDissolve()
    {
        if (CharMats.Count > 0)
        {
            //Debug.Log("123332112333");
            foreach (var VARIABLE in CharMats)
            {
                if (VARIABLE.GetFloat("_Dissolve") == 0)
                {
                    VARIABLE.EnableKeyword("_DISSOLVE_ON");
                    VARIABLE.DisableKeyword("_DISSOLVE_OFF");
                }
                else
                {
                    VARIABLE.EnableKeyword("_DISSOLVE_OFF");
                    VARIABLE.DisableKeyword("_DISSOLVE_ON");
                }
            }
        }
    }
    
    public void DissolveON()
    {
        Debug.Log("DissolveON");
        if (CharMats.Count > 0)
        {
            //Debug.Log("123332112333");
            foreach (var VARIABLE in CharMats)
            {
                VARIABLE.EnableKeyword("_DISSOLVE_ON");
                VARIABLE.DisableKeyword("_DISSOLVE_OFF");
            }
        }
    }

    public void DissolveOFF()
    {
        if (CharMats.Count > 0)
        {
            foreach (var VARIABLE in CharMats)
            {
                VARIABLE.EnableKeyword("_DISSOLVE_OFF");
                VARIABLE.DisableKeyword("_DISSOLVE_ON");
            }
        }
    }

    public void ZWriteInOn()
    {
        if (CharMats.Count > 0)
        {
            foreach (var VARIABLE in CharMats)
            {
                VARIABLE.SetFloat("_ZwriteOp_Char",1);
            }
        }
    }
    
    public void ZWriteInOff()
    {
        if (CharMats.Count > 0)
        {
            foreach (var VARIABLE in CharMats)
            {
                VARIABLE.SetFloat("_ZwriteOp_Char",0);
            }
        }
    }
}
