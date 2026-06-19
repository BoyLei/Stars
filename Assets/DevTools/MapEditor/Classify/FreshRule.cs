///--------------------------------------------------------------------
/// 文件名   :   FreshRule
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/05/26 13:19:49
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
#if UNITY_EDITOR
using OfficeOpenXml;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace MapEditor
{
    abstract public class FreshRule : ControlRelated
    {
        private IEnumerable _freshTypes = new ValueDropdownList<FreshType>()
        {
            { "死亡间隔刷新", FreshType.OnDead },
            { "开服间隔刷新", FreshType.FirstOpenServer },
            { "固定时刻刷新", FreshType.FixTime },
        };

        [FoldoutGroup("Spawner", 1)]
        [LabelText("SpawnerID")]
        [ReadOnly]
        public int SpawnerID;

        [FoldoutGroup("Spawner", 1)]
        [LabelText("SpawnerTransform")]
        [OnValueChanged("OnSetSpawner")]
        public Spawner Spawner;

        [FoldoutGroup("追击区域", 1)]
        [LabelText("AreaID")]
        [ReadOnly]
        public int ChaseAreaID;

        [FoldoutGroup("追击区域", 1)]
        [LabelText("AreaTransform")]
        [OnValueChanged("OnSetChaseArea")]
        public Area ChaseArea;

        [FoldoutGroup("重力", 1)]
        [LabelText("忽略重力")]
        public bool IgnoreGravity = false;
        
        [FoldoutGroup("刷新规则", 1)]
        [LabelText("范围颜色")]
        [ColorUsage(true, true)]
        public Color RangeColor = new Color(0.2f, 0.5f, 0.7f, 0.3f);

        [FoldoutGroup("刷新规则", 1)]
        [LabelText("刷新数量")]
        public int Num;

        [FoldoutGroup("刷新规则", 1)]
        [LabelText("刷新范围")]
        public int Range;

        [FoldoutGroup("刷新规则", 1)]
        [LabelText("刷新次数")]
        public int Count;

        [FoldoutGroup("刷新规则", 1)]
        [LabelText("刷新类型")]
        [ValueDropdown("_freshTypes")]
        public FreshType freshType = FreshType.OnDead;

        [FoldoutGroup("刷新规则", 1)]
        [LabelText("死亡间隔刷新时间")]
        [ShowIf("_IsShowDead")]
        public int DeadFreshInterval;

        [FoldoutGroup("刷新规则", 1)]
        [LabelText("开服间隔刷新时间")]
        [ShowIf("_IsShowServer")]
        public int OpenServerInterval;

        [FoldoutGroup("刷新规则", 1)]
        [LabelText("固定时刻刷新时间")]
        [ShowIf("_IsShowFix")]
        [TableList]
        public List<DataTime> FixFreshTimes = new List<DataTime>();


        [FoldoutGroup("刷新规则", 1)]
        [LabelText("是否显示刷新范围")]
        public bool IsShowFreshRange;

        [FoldoutGroup("刷新规则", 1)]
        [LabelText("是否在不可达区域创建")]
        public bool CanCreateUnreachableArea;

        private Color color;
        private void OnSetSpawner()
        {
            if (Spawner != null)
            {
                SpawnerID = Spawner.SpawnerID;
            }
            else
            {
                SpawnerID = 0;
            }
        }

        private void OnSetChaseArea()
        {
            if (ChaseArea != null)
            {
                ChaseAreaID = ChaseArea.AreaID;
            }
            else
            {
                ChaseAreaID = 0;
            }
        }

        private bool _IsShowDead()
        {
            return freshType == FreshType.OnDead;
        }

        private bool _IsShowServer()
        {
            return freshType == FreshType.FirstOpenServer;
        }

        private bool _IsShowFix()
        {
            return freshType == FreshType.FixTime;
        }
        public string GetFixFreshTimes()
        {
            if (FixFreshTimes != null && FixFreshTimes.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                foreach (var item in FixFreshTimes)
                {
                    if (item.Year > 0)
                    {
                        builder.Append(item.DataString());
                        builder.Append(";");
                    }
                }
                return builder.ToString();
            }
            return string.Empty;
        }

        public void ResolveFixFreshTimes(string times)
        {
            FixFreshTimes.Clear();
            string[] timeArray = times.Split(';');
            if (timeArray != null)
            {
                foreach (var time in timeArray)
                {
                    DataTime t = new DataTime(time);
                    if (t.Year > 0)
                    {
                        FixFreshTimes.Add(t); 
                    }
                }
            }
        }
        
        public void InitFixFreshTime(string cfgs)
        {
            FixFreshTimes.Clear();

            string[] splits = cfgs.Split(';');
            if (splits != null && splits.Length > 0)
            {
                foreach (var item in splits)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        DataTime time = new DataTime(item);
                        FixFreshTimes.Add(time);
                    }
                }
            }


        }
        public override void ExportExcel(int row, ExcelRange excel, int index)
        {
            excel[row, index++].Value = Num;
            excel[row, index++].Value = Range;
            excel[row, index++].Value = Count;
            excel[row, index++].Value = (int)freshType;
            excel[row, index++].Value = DeadFreshInterval;
            excel[row, index++].Value = OpenServerInterval;
            excel[row, index++].Value = GetFixFreshTimes();
            base.ExportExcel(row, excel, index);
        }

        public override void Load()
        {
            base.Load();
        }


        protected override void OnDrawGizmos()
        {      
            if(IsShowFreshRange)
            {
                color = UnityEditor.Handles.color;
                UnityEditor.Handles.color = RangeColor;
                UnityEditor.Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360, Range * 0.01f);
                UnityEditor.Handles.color = color;
            }
        }
    }

}
#endif