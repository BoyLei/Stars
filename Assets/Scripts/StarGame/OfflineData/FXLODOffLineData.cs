

/*
        有两个方案, 一个是用工具把特效Prefab导出成低中高3个Prefab.另一个就是在游戏里实时开关1，2，3这三个子物体.
            动态的
            工具是第二条路因为目前我们高中低是能切换的 目前做的合并式属于源文件了
            另一种逆向到这一种很麻烦。就动态
            并且相对内存目前特效属于拆分不会影响现有的情况下。相对渲染压力比较大。我认为默认应该都关闭然后请求高中低陪得到我应该开什么。而不是都开然后关闭
            特效lod的这么做
美术先划分一个特效根节点下面的所有子节点
            然后点击根节点下按一个快捷键 我帮他挂载脚本并且默认关闭
其他管理逻辑都我来写*/

 using Sirenix.OdinInspector;
using Sirenix.Serialization;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;




namespace StarProject.OffLine
{
    [ShowOdinSerializedPropertiesInInspector]
    public class FXLODOffLineData : MonoBehaviour, ISerializationCallbackReceiver, ISupportsPrefabSerialization

    {

        [SerializeField, HideInInspector]
        private SerializationData serializationData;

        SerializationData ISupportsPrefabSerialization.SerializationData { get { return this.serializationData; } set { this.serializationData = value; } }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            UnitySerializationUtility.DeserializeUnityObject(this, ref this.serializationData);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            UnitySerializationUtility.SerializeUnityObject(this, ref this.serializationData);
        }

        [LabelText("低级--只显示1, 中级--显示1+2, 高级--显示1+2+3")]
        public Dictionary<string, GameObject> BindDummyPos = new Dictionary<string, GameObject>();

    public void GenerateNodesData()
    {
        if (transform.Find("1") != null && !BindDummyPos.ContainsKey("1"))
        {
            GameObject t1 = transform.Find("1").gameObject;
            BindDummyPos.Add("1", t1);
            t1.gameObject.SetActive(false);
        }
        if (transform.Find("2") != null && !BindDummyPos.ContainsKey("2"))
        {
            GameObject t2 = transform.Find("2").gameObject;
            BindDummyPos.Add("2", t2);
            t2.gameObject.SetActive(false);
        }
        if (transform.Find("3") != null && !BindDummyPos.ContainsKey("3"))
        {
            GameObject t3 = transform.Find("3").gameObject;
            BindDummyPos.Add("3", t3);
            t3.gameObject.SetActive(false);
        }
    }

    private void Awake()
    {
        OnReflesh();//系统代码默认，或编辑器，或者玩家设定
        GlobalEvent.OnQualChangeTinyModuleReflesh.AddListener(OnReflesh);
    }
    private void OnDestroy()
    {
        GlobalEvent.OnQualChangeTinyModuleReflesh.RemoveListener(OnReflesh);
    }

    private void OnReflesh(int level = 0)
    {
        // Debug.Log("GraphQualityLevel: " + AppMain.Instance.GraphQualityLevel  + "!!!!!!!!!!");
        if (BindDummyPos.ContainsKey("1") != false && BindDummyPos.ContainsKey("2") != false &&
            BindDummyPos.ContainsKey("3") != false 
            )
        {
            if ( BindDummyPos["1"] !=null && BindDummyPos["2"] !=null && BindDummyPos["3"] !=null)
            {
                if (GameConfig.MachineQualityLevel == MachineQualityLevel.LowerLevel)
                {
                    BindDummyPos["1"].SetActive(true);
                    BindDummyPos["2"].SetActive(false);//都协商的原因是因为可能有缓存，然后开启的特效要给他关闭 
                    BindDummyPos["3"].SetActive(false);
                }
                else if (GameConfig.MachineQualityLevel == MachineQualityLevel.MiddleLevel)
                {
                    BindDummyPos["1"].SetActive(true);
                    BindDummyPos["2"].SetActive(true);
                    BindDummyPos["3"].SetActive(false);
                }
                //0不是启动级（！最低初始），是最高级
                else if (GameConfig.MachineQualityLevel == MachineQualityLevel.TopLevel)
                {
                    BindDummyPos["1"].SetActive(true);
                    BindDummyPos["2"].SetActive(true);
                    BindDummyPos["3"].SetActive(true);
                }//=================1+2+3=整体=============================
                else if (GameConfig.MachineQualityLevel == MachineQualityLevel.TopestLevel)
                {
                    BindDummyPos["1"].SetActive(true);
                    BindDummyPos["2"].SetActive(true);
                    BindDummyPos["3"].SetActive(true);
                }
                else if (GameConfig.MachineQualityLevel == MachineQualityLevel.LowestLevel)
                {
                    BindDummyPos["1"].SetActive(true);
                    BindDummyPos["2"].SetActive(false);//都协商的原因是因为可能有缓存，然后开启的特效要给他关闭 
                    BindDummyPos["3"].SetActive(false);
                }
            }
            
        }
        //有些特效的key写成01 02 03了, 现在封板 特效上传不了,先兼容一下
        else if (BindDummyPos.ContainsKey("01") != false && BindDummyPos.ContainsKey("02") != false &&
                 BindDummyPos.ContainsKey("03") != false &&
                 BindDummyPos["01"] !=null && BindDummyPos["02"] !=null && BindDummyPos["03"] !=null)
        {
            if (BindDummyPos["01"] != null && BindDummyPos["02"] != null && BindDummyPos["03"] != null)
            {
                if (GameConfig.MachineQualityLevel == MachineQualityLevel.LowerLevel)
                {
                    BindDummyPos["01"].SetActive(true);
                    BindDummyPos["02"].SetActive(false);//都协商的原因是因为可能有缓存，然后开启的特效要给他关闭 
                    BindDummyPos["03"].SetActive(false);
                }
                else if (GameConfig.MachineQualityLevel == MachineQualityLevel.MiddleLevel)
                {
                    BindDummyPos["01"].SetActive(true);
                    BindDummyPos["02"].SetActive(true);
                    BindDummyPos["03"].SetActive(false);
                }
                //0不是启动级（！最低初始），是最高级
                else if (GameConfig.MachineQualityLevel == MachineQualityLevel.TopLevel)
                {
                    BindDummyPos["01"].SetActive(true);
                    BindDummyPos["02"].SetActive(true);
                    BindDummyPos["03"].SetActive(true);
                }//=================1+2+3=整体=============================
                else if (GameConfig.MachineQualityLevel == MachineQualityLevel.TopestLevel)
                {
                    BindDummyPos["01"].SetActive(true);
                    BindDummyPos["02"].SetActive(true);
                    BindDummyPos["03"].SetActive(true);
                }
                else if (GameConfig.MachineQualityLevel == MachineQualityLevel.LowestLevel)
                {
                    BindDummyPos["01"].SetActive(true);
                    BindDummyPos["02"].SetActive(false);//都协商的原因是因为可能有缓存，然后开启的特效要给他关闭 
                    BindDummyPos["03"].SetActive(false);
                }
            }
            
        }
        
    }
    }
}


//美术放在FX上了，所以可以删除父节点应该没问题
//为什么美术根节点要放一个FX，主要是因为他放了方便预览，既然有这个性能损耗，美术就把这个时间配置在根节点上，程序可以来关闭，