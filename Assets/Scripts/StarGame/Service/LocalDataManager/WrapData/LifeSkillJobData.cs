//来源表生活技能采集制造_LifeSkill.xlsm.xlsx -> sheet:LifeSkillJob
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class LifeSkillJobData
	{
		[Key(0)]
		public Dictionary<int, LifeSkillJobDataCell> StaticLifeSkillJobDatas = new Dictionary<int, LifeSkillJobDataCell>();
	}
	[MessagePackObject]
	public class LifeSkillJobDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//职业ID
		[Key(1)]
		public int SkillID;
		public int GetSkillID()
		{
			return SkillID;
		}
		//职业名多语言Key
		[Key(2)]
		public string SkillName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_SkillName); }
			set { _SkillName = value; }
        }
		[IgnoreMember]
		private string _SkillName;
		//等级
		[Key(3)]
		public int Level;
		public int GetLevel()
		{
			return Level;
		}
		//采集制造加速度
		[Key(4)]
		public int CollectSpeed;
		public int GetCollectSpeed()
		{
			return CollectSpeed;
		}
		//基础产出加成概率
		[Key(5)]
		public int BasicOutputAdditon;
		public int GetBasicOutputAdditon()
		{
			return BasicOutputAdditon;
		}
		//高级产出加成概率
		[Key(6)]
		public int RareOutputAdditon;
		public int GetRareOutputAdditon()
		{
			return RareOutputAdditon;
		}
		//加成生效等级
		[Key(7)]
		public int AdditionActiveLV;
		public int GetAdditionActiveLV()
		{
			return AdditionActiveLV;
		}
		//所需经验
		[Key(8)]
		public int NeedEX;
		public int GetNeedEX()
		{
			return NeedEX;
		}
		//突破任务
		[Key(9)]
		public long BreakQuest;
		public long GetBreakQuest()
		{
			return BreakQuest;
		}
		//QTE玩法
		[Key(10)]
		public int UseQTE;
		public int GetUseQTE()
		{
			return UseQTE;
		}
		//玩家交互动作
		[Key(11)]
		public string Action;
		//显示用分组
		[Key(12)]
		public int DisplayGroup;
		public int GetDisplayGroup()
		{
			return DisplayGroup;
		}
	}
}
