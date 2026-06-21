            using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG;
using DG.Tweening;

public class Testaaaaaaaa : MonoBehaviour
{
    public SkinnedMeshRenderer SkinnedMeshRenderer;
    // Start is called before the first frame update
    void Start()
    {
        /*SkinnedMeshRenderer =GetComponent<SkinnedMeshRenderer>();*/
    }

    // Update is called once per frame
/*    void Update()
    {
        
    }*/
    public void foo() 
    {
        DOTween.To(() => Color.black, color =>
        {
            SkinnedMeshRenderer.materials[0].SetColor("_MainColor", color);


        }, Color.white, 5).SetEase(Ease.Linear).SetUpdate(UpdateType.Fixed);
       
    }
}
