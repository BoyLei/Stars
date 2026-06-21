using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadDirectionController : MonoBehaviour
{
    /*public  GameObject baseT;
    public GameObject forwardT;
    public GameObject leftT;*/
    
    [Space(10)]
    [Header("用来让头阴影正确显示的脚本")]
    [Header("将预制体Prefab,挪动至头部骨骼下,设置为头部骨骼的子物体,且使得蓝色箭头朝向头的前方,红色箭头朝向头的右侧")]
    [Header("头部材质球拖入HeadMaterial,点击运行即可")]
    // [Header("用来让头阴影正确显示的脚本" +
    //         "将预制体Prefab,挪动至头部骨骼下,设置为头部骨骼的子物体,且使得蓝色箭头朝向头的前方,红色箭头朝向头的右侧\r\n" +
    //         "头部材质球拖入HeadMaterial,点击运行即可")]
    [Space(10)]
    //public GameObject HeadRig;

    public Material HeadMaterial;

    private Renderer HeadRender;

    private Vector3 frontDirWS;

    private Vector3 leftDirWS;
    
    private float timer = 10f;
    
    private float interval = 10f;
    
    private  MaterialPropertyBlock mpb;
    // Start is called before the first frame update
    void Start()
    {
        mpb = new MaterialPropertyBlock();
        FindHeadMat();
    }

    // Update is called once per frame
    void Awake()
    {
        FindHeadMat();
    }
    void Update()
    {
        //每interval秒刷新一下
        // timer += Time.deltaTime;
        //
        // if (timer >= interval)
        // {
        //     // 重置计时器
        //     timer = 0f;
        //     // 调用方法
        //     FindHeadMat();
        // }
        
        if (HeadMaterial != null/*&& HeadRig!=null*//*&& Front != null && Left != null*/)
        {
            
            /*Vector3 FrontDir = Vector3.Normalize(forwardT.transform.position - baseT.transform.position);
            Vector3 LeftDir =  Vector3.Normalize(leftT.transform.position - baseT.transform.position);*/
            //Vector3 FrontDir;
            //Vector3 LeftDir;
            frontDirWS =  this.GetComponent<Transform>().forward;
            leftDirWS = -1 * this.GetComponent<Transform>().right;
            /*FrontDir.x = FrontDir.x * -1;
            FrontDir.y = FrontDir.y * -1;
            FrontDir.z = FrontDir.z * -1;
            LeftDir.x = LeftDir.x * -1;
            LeftDir.y = LeftDir.y * -1;
            LeftDir.z = LeftDir.z * -1;*/
            /*FrontDir = HeadRig.GetComponent<Transform>().forward;
            LeftDir =-1f * HeadRig.GetComponent<Transform>().right;*/
            //LeftDir = leftDirWS ;
            //FrontDir = FrontDir * -1;
            if (HeadRender != null&&mpb!=null)
            {
                HeadRender.GetPropertyBlock(mpb);
                mpb.SetVector("_FrontDirHeadWS",frontDirWS);
                mpb.SetVector("_LeftDirHeadWS",leftDirWS);
                HeadRender.SetPropertyBlock(mpb);
            }
            
            /*Debug.Log("前方:"+LeftDir+"  左方:" +FrontDir);*/
        }
        
    }

    void FindHeadMat()
    {
        Transform parentTransform = this.transform.parent;
        int count = 1;
        while (parentTransform != null && parentTransform.name != "Root"&& count<=20)
        {
            parentTransform = parentTransform.parent;
            count++;
        }

        if (parentTransform != null && parentTransform.name == "Root")
        {
            Transform grandParentTransform = parentTransform.parent;
            if (grandParentTransform != null /*&& (grandParentTransform.parent !=null && !grandParentTransform.parent.name.Contains("ObjectPools")) */)
            {
                Renderer[] childRenders = grandParentTransform.GetComponentsInChildren<Renderer>();
                foreach (Renderer render in childRenders)
                {
                    if (render.transform.name == "Face")
                    {
                        //HeadMaterial = render.sharedMaterial;
                        HeadRender = render;
                    }
                }
            }
            else
            {
                //Debug.Log("HeadDirection找不到Root");
            }
        }
    }
}
