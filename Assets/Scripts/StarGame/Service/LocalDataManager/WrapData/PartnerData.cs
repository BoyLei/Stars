//来源表伙伴配置表_Partner.xlsm.xlsx -> sheet:Partner
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class PartnerData
	{
		[Key(0)]
		public Dictionary<long, PartnerDataCell> StaticPartnerDatas = new Dictionary<long, PartnerDataCell>();
	}
	[MessagePackObject]
	public class PartnerDataCell
	{
		//伙伴id
		[Key(0)]
		public long Id;
		public long GetId()
		{
			return Id;
		}
		//伙伴名称
		[Key(1)]
		public string Name
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Name); }
			set { _Name = value; }
        }
		[IgnoreMember]
		private string _Name;
		//化身id
		[Key(2)]
		public int AvatarID;
		public int GetAvatarID()
		{
			return AvatarID;
		}
		//伙伴品质
		[Key(3)]
		public int Quality;
		public int GetQuality()
		{
			return Quality;
		}
		//伙伴类型
		[Key(4)]
		public int PartnerType;
		public int GetPartnerType()
		{
			return PartnerType;
		}
		//属性组ID
		[Key(5)]
		public int Nature;
		public int GetNature()
		{
			return Nature;
		}
		//初始星级
		[Key(6)]
		public int Star;
		public int GetStar()
		{
			return Star;
		}
		//是否为临时伙伴
		[Key(7)]
		public int TemporaryPar;
		public int GetTemporaryPar()
		{
			return TemporaryPar;
		}
		//资质组ID
		[Key(8)]
		public int QualificationID;
		public int GetQualificationID()
		{
			return QualificationID;
		}
		//消耗组ID
		[Key(9)]
		public int QualificationsExpendID;
		public int GetQualificationsExpendID()
		{
			return QualificationsExpendID;
		}
		//升星道具
		[Key(10)]
		public long UpStarItem;
		public long GetUpStarItem()
		{
			return UpStarItem;
		}
		//AI类型
		[Key(11)]
		public int AIType;
		public int GetAIType()
		{
			return AIType;
		}
		//AI表ID
		[Key(12)]
		public int Aiindex;
		public int GetAiindex()
		{
			return Aiindex;
		}
		//引导用登场技弹出判定
		[Key(13)]
		public bool GuidUseSkiOp;
		public bool GetGuidUseSkiOp()
		{
			return GuidUseSkiOp;
		}
		//伙伴模型
		[Key(14)]
		public int ModelRadius;
		public int GetModelRadius()
		{
			return ModelRadius;
		}
		//复活时间
		[Key(15)]
		public int RebornTime;
		public int GetRebornTime()
		{
			return RebornTime;
		}
		//伙伴模型位置
		[Key(16)]
		public string ModelPos;
		//伙伴模型旋转
		[Key(17)]
		public string ModelRot;
		//伙伴模型缩放
		[Key(18)]
		public string ModelScale;
		//动画名称
		[Key(19)]
		public string ModelAnima;
		//上阵动画
		[Key(20)]
		public string AppearAnima;
		//上阵台词
		[Key(21)]
		public List<string> AppearDialogue = new List<string>();
		//抽卡格言
		[Key(22)]
		public string GachaSlogen
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_GachaSlogen); }
			set { _GachaSlogen = value; }
        }
		[IgnoreMember]
		private string _GachaSlogen;
	}
}
