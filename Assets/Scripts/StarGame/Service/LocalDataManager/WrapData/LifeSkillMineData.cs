//来源表生活技能采集制造_LifeSkill.xlsm.xlsx -> sheet:LifeSkillMine
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class LifeSkillMineData
	{
		[Key(0)]
		public Dictionary<int, LifeSkillMineDataCell> StaticLifeSkillMineDatas = new Dictionary<int, LifeSkillMineDataCell>();
	}
	[MessagePackObject]
	public class LifeSkillMineDataCell
	{
		//矿ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//矿名多语言Key
		[Key(1)]
		public string Name
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Name); }
			set { _Name = value; }
        }
		[IgnoreMember]
		private string _Name;
		//单次基础产物1
		[Key(2)]
		public long BasicOutput;
		public long GetBasicOutput()
		{
			return BasicOutput;
		}
		//基础产物产出数量
		[Key(3)]
		public long BasicOutputNum;
		public long GetBasicOutputNum()
		{
			return BasicOutputNum;
		}
		//高级产物产出概率
		[Key(4)]
		public int RareOutputProb;
		public int GetRareOutputProb()
		{
			return RareOutputProb;
		}
		//高级产物1
		[Key(5)]
		public List<long> RareOutput = new List<long>();
		//高级产物产出数量
		[Key(6)]
		public List<long> RareOutputNum = new List<long>();
		//矿点等级
		[Key(7)]
		public int Level;
		public int GetLevel()
		{
			return Level;
		}
		//单次消耗活力
		[Key(8)]
		public int EnergyCost;
		public int GetEnergyCost()
		{
			return EnergyCost;
		}
		//对应职业ID
		[Key(9)]
		public int SkillID;
		public int GetSkillID()
		{
			return SkillID;
		}
		//对个人储量
		[Key(10)]
		public int PersonReserve;
		public int GetPersonReserve()
		{
			return PersonReserve;
		}
		//对个人重生时间 
		[Key(11)]
		public int RebornInterval;
		public int GetRebornInterval()
		{
			return RebornInterval;
		}
		//来源描述
		[Key(12)]
		public string CreatSource
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_CreatSource); }
			set { _CreatSource = value; }
        }
		[IgnoreMember]
		private string _CreatSource;
	}
}
