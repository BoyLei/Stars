//来源表Job.xlsm.xlsx -> sheet:Talent
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class TalentData
	{
		[Key(0)]
		public Dictionary<int, TalentDataCell> StaticTalentDatas = new Dictionary<int, TalentDataCell>();
	}
	[MessagePackObject]
	public class TalentDataCell
	{
		//天赋ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//天赋类别
		[Key(1)]
		public int TalentType;
		public int GetTalentType()
		{
			return TalentType;
		}
		//天赋名称
		[Key(2)]
		public string Name;
		//所属技能
		[Key(3)]
		public int BelongSkill;
		public int GetBelongSkill()
		{
			return BelongSkill;
		}
		//天赋对应SKILLID
		[Key(4)]
		public List<int> BattleActive = new List<int>();
		//天赋对应PassiveSkillId
		[Key(5)]
		public List<int> BattlePassive = new List<int>();
		//挂载激活被动
		[Key(6)]
		public bool IsDependPadding;
		public bool GetIsDependPadding()
		{
			return IsDependPadding;
		}
		//天赋可使用条件-仅等级限制
		[Key(7)]
		public int LimitLv;
		public int GetLimitLv()
		{
			return LimitLv;
		}
		//天赋标签
		[Key(8)]
		public string TalentLabel
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_TalentLabel); }
			set { _TalentLabel = value; }
        }
		[IgnoreMember]
		private string _TalentLabel;
		//天赋名称
		[Key(9)]
		public string TalentName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_TalentName); }
			set { _TalentName = value; }
        }
		[IgnoreMember]
		private string _TalentName;
		//天赋icon
		[Key(10)]
		public string TalentIcon;
		//天赋描述
		[Key(11)]
		public string TalentDesc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_TalentDesc); }
			set { _TalentDesc = value; }
        }
		[IgnoreMember]
		private string _TalentDesc;
	}
}
