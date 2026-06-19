using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CharAlphaController : MonoBehaviour
{
  //  [serializeable]
    [SerializeField]
    public  List<Material> materialList;

    public bool StartAlpha;

    [Range(0.0f, 1.0f)] public float Alpha = 1f;

    private bool ifInitial = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (StartAlpha == true)
        {
            foreach (Material VARIABLE in materialList)
            {
                VARIABLE.renderQueue = 3000;
                VARIABLE.SetFloat("_Alpha",Alpha);
                VARIABLE.SetFloat("_OutlineAlpha",Alpha);
                ifInitial = false;
            }
        }
        else
        {
            if (ifInitial == false)
            {
               foreach (Material VARIABLE in materialList)
               {
                   VARIABLE.renderQueue = 2015;
                   VARIABLE.SetFloat("_Alpha",1);
                   VARIABLE.SetFloat("_OutlineAlpha",1);
               }

               ifInitial = true;
            }
            
        }
        
    }
}
