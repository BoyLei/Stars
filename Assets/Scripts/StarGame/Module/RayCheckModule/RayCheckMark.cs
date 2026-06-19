using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCheckMark : MonoBehaviour
{
    public string GroupName;
    //相关的材质物体
    public List<MeshRenderer> LinkedMat;

    int StateNow = -1;

    public void SetMatState(bool isNeedOpenDither)
    {

        int toState = 0;
        if(isNeedOpenDither)
        {
            toState = 1;
        }

        if(StateNow!= toState)
        {
            StateNow = toState;
        }
        else
        {
            return;
        }


        for(int i=0;i<LinkedMat.Count;i++)
        {
            LinkedMat[i].material.SetFloat("_IsOpenDither", StateNow);
        }
    }
}
