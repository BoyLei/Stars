using System.Collections;
using System.Collections.Generic;
using StarProject.Game;
using UnityEngine;

public class FogTrigger : MonoBehaviour
{


    // Start is called before the first frame update
    void Awake()
    {
        //把相关信息发送给RenderManager
        RenderManager.Instance.OnSetFog(gameObject);
    }


}
