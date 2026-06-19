//来源表生活技能采集制造_LifeSkill.xlsm.xlsx -> sheet:LifeSkillCreate
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class LifeSkillCreateData
	{
		[Key(0)]
		public Dictionary<int, LifeSkillCreateDataCell> StaticLifeSkillCreateDatas = new Dictionary<int, LifeSkillCreateDataCell>();
	}
	[MessagePackObject]
	public class LifeSkillCreateDataCell
	{
		//配方ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//解锁道具ID
		[Key(1)]
		public long UnlockItem;
		public long GetUnlockItem()
		{
			return UnlockItem;
		}
		//基础产出道具
		[Key(2)]
		public long BasicOutput;
		public long GetBasicOutput()
		{
			return BasicOutput;
		}
		//基础产物数量
		[Key(3)]
		public long BasicOutputNum;
		public long GetBasicOutputNum()
		{
			return BasicOutputNum;
		}
		//消耗道具ID
		[Key(4)]
		public List<long> CostItem = new List<long>();
		//消耗数量
		[Key(5)]
		public List<long> CostItemNum = new List<long>();
		//高级产物产出概率
		[Key(6)]
		public int RareOutputProb;
		public int GetRareOutputProb()
		{
			return RareOutputProb;
		}
		//高级产物1
		[Key(7)]
		public long RareOutput;
		public long GetRareOutput()
		{
			return RareOutput;
		}
		//高级产物数量
		[Key(8)]
		public long RareOutputNum;
		public long GetRareOutputNum()
		{
			return RareOutputNum;
		}
		//配方等级
		[Key(9)]
		public int Level;
		public int GetLevel()
		{
			return Level;
		}
		//对应职业ID
		[Key(10)]
		public int SkillID;
		public int GetSkillID()
		{
			return SkillID;
		}
		//消耗活力
		[Key(11)]
		public int EnergyCost;
		public int GetEnergyCost()
		{
			return EnergyCost;
		}
		//单次最大制作数
		[Key(12)]
		public int OnceMaxCreateNum;
		public int GetOnceMaxCreateNum()
		{
			return OnceMaxCreateNum;
		}
		//来源描述
		[Key(13)]
		public string CreatSource
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_CreatSource); }
			set { _CreatSource = value; }
        }
		[IgnoreMember]
		private string _CreatSource;
	}
}
