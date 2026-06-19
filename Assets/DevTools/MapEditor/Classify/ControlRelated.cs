///--------------------------------------------------------------------
/// 文件名   :   ControlRelated
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/05/26 13:11:56
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
#if UNITY_EDITOR
using OfficeOpenXml;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace MapEditor
{
    abstract public class ControlRelated : MonoBehaviour, IMapElement
    {
        [FoldoutGroup("控制相关", 2)]
        [LabelText("波次ID")]
        public int WaveID;

        [FoldoutGroup("控制相关", 2)]
        [LabelText("控制器ID")]
        public int ControlID;


        [FoldoutGroup("贴地相关", 3)]
        [Button("一键贴地")]
        public virtual void OnGround()
        {
            Vector3 position = transform.position;
            transform.position = MapEditorUtils.GetGroundPoint(position);
        }

        virtual public void ExportExcel(int row, ExcelRange excel, int index)
        {
            excel[row, index++].Value = WaveID;
            excel[row, index++].Value = ControlID;
        }

        virtual public void Load()
        {

        }

        virtual protected void Awake()
        {

        }

        virtual protected void Start()
        {


        }


        virtual protected void OnDrawGizmos()
        {

        }
    }
}
#endif
