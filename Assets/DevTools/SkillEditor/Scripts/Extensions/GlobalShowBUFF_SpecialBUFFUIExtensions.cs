using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
///--------------------------------------------------------------------
/// 文件名   :   GlobalShowBUFF_SpecialBUFFUIExtensions
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   
/// 创建人   :   Create By BaseDataConfig.xml
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
namespace SkillEditor
{
    [System.Flags]
    public enum BuffUIShowDetailsEnum
    {
        //None = 0,   // 都不显示
        ShowFloor = 1 << 0, //1 显示层数
        ShowCountdown = 1 << 1,  // 2 显示倒计时
        ShowIcon = 1 << 2,  // 4 显示图标
                            //ALL = ShowFloor | ShowCountdown | ShowIcon   // 全部显示
    }

    [System.Serializable]
    [MessagePack.MessagePackObject(keyAsPropertyName:true)]
    public class BUFFUIShow
    {
        [Newtonsoft.Json.JsonIgnore]
        public string Tile
        {
            get
            {
                string text = "显示情况：无";
                switch (BUFFUIPos)
                {
                    case BUFFUIShowPosEnum.HUDHpUp:
                        {
                            text = "显示情况：HUD血条上方";
                        }
                        break;
                    case BUFFUIShowPosEnum.ThreeDHPUp:
                        {
                            text = "显示情况：3D头顶上方";
                        }
                        break;
                    case BUFFUIShowPosEnum.SecretBUFF:
                        {
                            text = "显示情况：秘境BUFF";
                        }
                        break;
                    default:
                        break;
                }

                return text;
            }
        }
        /// <summary>
        /// 显示位置
        /// <summary>
        [LabelText("$Tile")]
        [SerializeField]
        [ValueDropdown("_specialbuffpos")]
        public BUFFUIShowPosEnum BUFFUIPos = BUFFUIShowPosEnum.HUDHpUp;

        [LabelText("Prefab具体显示详情")]
        [EnumPaging]
        public BuffUIShowDetailsEnum BuffUIShowDetails = BuffUIShowDetailsEnum.ShowFloor | BuffUIShowDetailsEnum.ShowCountdown | BuffUIShowDetailsEnum.ShowIcon;

        public IEnumerable _specialbuffpos()
        {
            return EnumDefineMap._buffuishowposenum;
        }
    }

    ///// <summary>
    ///// BUFFUI显示
    ///// </summary>
    //public partial class GlobalShowBUFF_BUFFUIDateils
    //{
    //    [LabelText("BUFFUI显示详情")]
    //    public List<BUFFUIShow> BUFFUIShowDetailsList = new List<BUFFUIShow>(0);
    //}
}