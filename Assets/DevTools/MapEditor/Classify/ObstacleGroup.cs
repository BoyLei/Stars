///--------------------------------------------------------------------
/// 文件名   :   ObstacleGroup.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/02/03 15:04:05
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
#if UNITY_EDITOR
using MapEditor;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class EffectTransform
{
    [LabelText("特效场景位置")]
    public Transform EffectTransfom;

    [LabelText("特效路径")]
    public string EffectPath;
}

[HideMonoScript]
public class ObstacleGroup : MonoBehaviour
{
    [FoldoutGroup("动态阻挡", 1)]
    [LabelText("组ID")]
    public int ID;
    
    [FoldoutGroup("动态阻挡", 1)]
    [LabelText("是否默认开启")]
    public bool IsOpen;
    
    [FoldoutGroup("动态阻挡", 1)]
    [ReadOnly]
    [LabelText("当前组最大索引")]
    public int Index = 0;

    [FoldoutGroup("动态阻挡", 1)]
    [LabelText("特效组")]
    public List<EffectTransform> Effects;

    [FoldoutGroup("动态阻挡", 1)]
    [Button("创建动态阻挡")]
    public void CreateObstacle()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name=$"Obstacle_{Index++}";
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        go.AddComponent<NavMeshObstacle>();
        go.layer = LayerMask.NameToLayer("Entity");
    }


    [FoldoutGroup("贴地相关", 3)]
    [Button("一键贴地")]
    public void OnGround()
    {
        Vector3 position = transform.position;
        transform.position = MapEditorUtils.GetGroundPoint(position);

        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            transform.GetChild(i).position = MapEditorUtils.GetGroundPoint(transform.GetChild(i).position);
        }
    }
}
#endif