///--------------------------------------------------------------------
/// 文件名   :   MapSceneConfig
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/07/15 10:15:11
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

#if UNITY_EDITOR
using Koenigz.PerfectCulling.SamplingProviders;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MapEditor
{
    [HideMonoScript]
    public class MapSceneConfig : MonoBehaviour
    {
        [FoldoutGroup("区域颜色", 0)] [LabelText("出生点")]
        //[ColorUsageAttribute(true, true)]
        public Color bronColor = new Color(1, 0.84f, 0, 0.5f);


        [FoldoutGroup("区域颜色", 0)] [LabelText("复活点")]
        //[ColorUsageAttribute(true, true)]
        public Color reliveColor = new Color(0, 0.74f, 1, 0.5f);

        [FoldoutGroup("区域颜色", 0)] [LabelText("安全区")]
        // [ColorUsageAttribute(true, true)]
        public Color safeColor = new Color(0, 1, 0, 0.5f);

        [LabelText("格子大小")] public int GridSize = 2000;

        [LabelText("是否为固定阻挡")] public bool IsFixedBlock;
        [LabelText("是否隐藏伙伴")] public bool IsHidePartner;
        [LabelText("是否隐藏其他玩家")] public bool IsHideOtherPlayer;

        [LabelText("Tag")] public string Tag;

        [ReadOnly] public int NPCIndex = 0;

        [ReadOnly] public int MineIndex = 0;

        public int GetNpcIndex()
        {
            NPCIndex++;
            return NPCIndex;
        }

        public int GetMineIndex()
        {
            MineIndex++;
            return MineIndex;
        }


        [Button("重建NPC索引")]
        public void ResetNpcIndex()
        {
            NPCIndex = 0;
            NPC[] mines = GetComponentsInChildren<NPC>();
            if (mines != null && mines.Length > 0)
            {
                foreach (var item in mines)
                {
                    NPCIndex++;
                    item.Index = NPCIndex;
                }
            }
        }

        [Button("重建矿物索引")]
        public void ResetMineIndex()
        {
            MineIndex = 0;
            Mine[] mines = GetComponentsInChildren<Mine>();
            if (mines != null && mines.Length > 0)
            {
                foreach (var item in mines)
                {
                    MineIndex++;
                    item.Index = MineIndex;
                }
            }
        }

        [Button("重置层级")]
        public void ResetLayer()
        {
            SetLayer(gameObject, LayerMask.NameToLayer("Entity"));
        }

        [Button("开启场景Culling")]
        public void OpenClulling()
        {
            var cullings=GameObject.FindObjectsByType<PerfectCullingRebounder>(FindObjectsSortMode.None);
            foreach (var cul in cullings)
            {
                cul.IsDraw = true;
            }
        }
        [Button("关闭场景Culling")]
        public void CloseClulling()
        {
            var cullings=GameObject.FindObjectsByType<PerfectCullingRebounder>(FindObjectsSortMode.None);
            foreach (var cul in cullings)
            {
                cul.IsDraw = false;
            }
        }
        public void SetLayer(GameObject go, int layer)
        {
            if (go != null)
            {
                int childcount = go.transform.childCount;
                for (int i = 0; i < childcount; i++)
                {
                    SetLayer(go.gameObject.transform.GetChild(i).gameObject, layer);
                }

                go.layer = layer;
            }
        }
    }
}
#endif