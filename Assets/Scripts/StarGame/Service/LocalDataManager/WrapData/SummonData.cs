//来源表Monster.xlsm.xlsx -> sheet:Summon
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SummonData
	{
		[Key(0)]
		public Dictionary<int, SummonDataCell> StaticSummonDatas = new Dictionary<int, SummonDataCell>();
	}
	[MessagePackObject]
	public class SummonDataCell
	{
		//序号
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//名称
		[Key(1)]
		public string Name
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Name); }
			set { _Name = value; }
        }
		[IgnoreMember]
		private string _Name;
		//数量
		[Key(2)]
		public int Count;
		public int GetCount()
		{
			return Count;
		}
		//主动技能
		[Key(3)]
		public List<int> ActiveSkills = new List<int>();
		//主动技能等级
		[Key(4)]
		public List<int> ActiveSkillsLv = new List<int>();
		//被动技能
		[Key(5)]
		public List<int> PassiveSkills = new List<int>();
		//被动技能等级
		[Key(6)]
		public List<int> PassiveSkillsLv = new List<int>();
		//视野外是否休眠
		[Key(7)]
		public bool IsSleep;
		public bool GetIsSleep()
		{
			return IsSleep;
		}
		//AI类型
		[Key(8)]
		public int AIType;
		public int GetAIType()
		{
			return AIType;
		}
		//AI表ID
		[Key(9)]
		public int Aiindex;
		public int GetAiindex()
		{
			return Aiindex;
		}
		//外观
		[Key(10)]
		public int AvatarID;
		public int GetAvatarID()
		{
			return AvatarID;
		}
		//模型半径
		[Key(11)]
		public int ModelRadius;
		public int GetModelRadius()
		{
			return ModelRadius;
		}
	}
}
