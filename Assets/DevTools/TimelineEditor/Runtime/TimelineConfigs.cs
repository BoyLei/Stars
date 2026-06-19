///--------------------------------------------------------------------
/// 文件名   :   TimelineConfigs.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2024/04/18 17:07:25
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
[CreateAssetMenu(menuName = "Assets/Create TimelineConfigs")]
public class TimelineConfigs:ScriptableObject, ISerializationCallbackReceiver, ISupportsPrefabSerialization
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
    
    [FilePath(AbsolutePath = true, Extensions = ".xlsx")]
    [LabelText("Timeline Excel 路径")]
    public string TimelineExcelPath;
    
    [LabelText("一级目录")]
    [TableList]
    [OnValueChanged("OnGroupTypeChange",true)]
    public  List<DicGroupType> TimelineDicGroupTypeDic = new List<DicGroupType>();
    
    [LabelText("主线目录")]
    [TableList]
    [OnValueChanged("OnGroupNameChange",true)]
    public  List<DicInfo> TimelineDicNameDic = new List<DicInfo>();


    private void OnGroupTypeChange()
    {
        TimelineConfigUtils.InitGroupType();
    }

    private void OnGroupNameChange()
    {
        TimelineConfigUtils.InitGroupName();
    }
}


[System.Serializable]
public class DicInfo
{
    [LabelText("描述")]
    public string Desc;
    
    [LabelText("目录名称")]
    public string DicName;
}

[System.Serializable]
public class DicGroupType
{
    [LabelText("描述")]
    public string Desc;
    
    [LabelText("目录名称")]
    [FolderPath]
    public string FolderPath;
}

#endif