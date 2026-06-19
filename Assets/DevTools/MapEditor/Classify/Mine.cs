///--------------------------------------------------------------------
/// 文件名   :   Mine
/// 内  容   :   编辑器布矿
/// 说  明   :  
/// 创建日期 :   2022/05/26 11:30:24
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
# if UNITY_EDITOR
using OfficeOpenXml;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapEditor
{
    [DisallowMultipleComponent]
    [HideMonoScript]
    [SelectionBase]
    [ExecuteInEditMode]
    public class Mine : FreshRule
    {
        private IEnumerable _mineTypes = new ValueDropdownList<MineType>()
        {
            { "基础物件", MineType.BaseObject },
            { "接任务", MineType.ReciveTask },
            { "传送", MineType.Transfer },
            { "跨场景传送", MineType.SceneTransfer },
            { "攀爬", MineType.Clamber },
            { "加buff", MineType.AddBuff },
        };

        [ReadOnly]
        public int Index;

        [FoldoutGroup("基础信息", 0)]
        [LabelText("矿物ID")]
        [OnValueChanged("OnMonsterIDChange")]
        public long MineID;



        [FoldoutGroup("基础信息", 0)]
        [LabelText("矿物类型")]
        [ReadOnly]
        [ValueDropdown("_mineTypes")]
        public MineType mineType = MineType.BaseObject;

        [FoldoutGroup("基础信息", 0)]
        [LabelText("是否显示范围")]
        public bool IsShowRange;


        [FoldoutGroup("基础信息", 0)]
        [LabelText("矿物组ID")]
        public int MineGroup;

        [FoldoutGroup("基础信息", 0)]
        [LabelText("默认显隐")][SuffixLabel("true 显示  false 隐藏")]
        public bool DefaultVisible = true;
        
        [FoldoutGroup("基础信息", 0)]
        [LabelText("范围颜色")]
        [ColorUsage(true, true)]
        public Color InterRangeColor = new Color(0.2f, 0.5f, 0.7f, 0.3f);

        [FoldoutGroup("基础信息", 0)]
        [LabelText("是否显示模型")]
        [OnValueChanged("ChangeHide")]
        public bool ShowModle;

        
        private Color color;

        private float InterRange;

        public void ReBuildMineIndex()
        {
            if (MapEditorUtils.SceneConfig != null)
            {
                int isRepeat = 0;
                Mine[] mines = MapEditorUtils.SceneConfig.gameObject.GetComponentsInChildren<Mine>();
                if (mines != null && mines.Length > 0)
                {
                    foreach (var item in mines)
                    {
                        if (item.Index == Index)
                        {
                            isRepeat++;
                        }
                    }
                }
                if (isRepeat > 1)
                {
                    Index = MapEditorUtils.SceneConfig.GetMineIndex();
                }
            }
        }

        [FoldoutGroup("基础信息", 0)]
        [Button("刷新模型")]
        public void FreshModel() {
            ReshModel();
        }

        private GameObject mModel;
        protected override void Awake()
        {
            ReshModel();
        }

        private void OnMonsterIDChange()
        {
            if (EditorConfigUtils.MineCfgs.Mines.ContainsKey(MineID))
            {
                mineType = (MineType)EditorConfigUtils.MineCfgs.Mines[MineID].Type;
                InterRange = EditorConfigUtils.MineCfgs.Mines[MineID].Range;
            }
            else
            {
                mineType = MineType.BaseObject;
            }
        }
        private void ClearChild()
        {
            int count = transform.childCount;
            for (int i = count - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(transform.GetChild(i).gameObject);

            }
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            if(IsShowRange)
            {
                color = UnityEditor.Handles.color;
                UnityEditor.Handles.color = InterRangeColor;
                UnityEditor.Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360, InterRange);
                UnityEditor.Handles.color = color;
            }
        }
        private void ChangeHide()
        {
            if (mModel != null)
            {
                if (ShowModle)
                {
                    mModel.hideFlags = HideFlags.DontSave;
                }
                else
                {
                    mModel.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
                }
            }
        }
        
        public void ReshModel()
        {
            ClearChild();
            OnMonsterIDChange();
            ReBuildMineIndex();
            int modelID = 0;
            if (EditorConfigUtils.MineCfgs.Mines.TryGetValue(MineID, out var Mine))
            {
                modelID = Mine.ModelID;
                string path = string.Empty;
                if (EditorConfigUtils.ModelCfgs.Models.TryGetValue(modelID, out path))
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        if(asset!=null)
                        {
                            if(mModel!=null)
                            {
                                GameObject.DestroyImmediate(mModel);
                            }

                            mModel = GameObject.Instantiate(asset) as GameObject;
                            mModel.transform.SetParent(transform);
                            mModel.transform.localPosition = Vector3.zero;
                            mModel.transform.localRotation = Quaternion.identity;
                            mModel.transform.localScale = Vector3.one;
                            if (ShowModle)
                            {
                                mModel.hideFlags = HideFlags.DontSave;
                            }
                            else
                            {
                                mModel.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
                            }
                        }
                        else
                        {
                            StarDebug.Log(StarDebug.Purple, $"加载模型失败:{path}");
                        }
                    }
                }
            }
        }

        public override void ExportExcel(int row, ExcelRange excel, int index)
        {
            excel[row, index++].Value = MineID;
            excel[row, index++].Value = (int)mineType;
            excel[row, index++].Value = IsShowRange;
            excel[row, index++].Value = EditorConfigUtils.Vector3ToString(transform.position);
            excel[row, index++].Value = EditorConfigUtils.Vector3ToString(transform.rotation.eulerAngles);
            base.ExportExcel(row, excel, index);
        }
    }
}
#endif