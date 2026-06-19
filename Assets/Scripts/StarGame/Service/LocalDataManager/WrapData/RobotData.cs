//来源表机器人表_Robot.xlsm.xlsx -> sheet:Robot
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class RobotData
	{
		[Key(0)]
		public Dictionary<long, RobotDataCell> StaticRobotDatas = new Dictionary<long, RobotDataCell>();
	}
	[MessagePackObject]
	public class RobotDataCell
	{
		//序号
		[Key(0)]
		public long ID;
		public long GetID()
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
		//属性模板
		[Key(2)]
		public int AttrBase;
		public int GetAttrBase()
		{
			return AttrBase;
		}
		//职业
		[Key(3)]
		public int JobType;
		public int GetJobType()
		{
			return JobType;
		}
		//战斗起手技能
		[Key(4)]
		public int StartSkill;
		public int GetStartSkill()
		{
			return StartSkill;
		}
		//主动技能
		[Key(5)]
		public List<int> ActiveSkills = new List<int>();
		//主动技能等级
		[Key(6)]
		public List<int> ActiveSkillsLv = new List<int>();
		//被动技能
		[Key(7)]
		public List<int> PassiveSkills = new List<int>();
		//被动技能等级
		[Key(8)]
		public List<int> PassiveSkillsLv = new List<int>();
		//视野外是否休眠
		[Key(9)]
		public bool IsSleep;
		public bool GetIsSleep()
		{
			return IsSleep;
		}
		//AI类型
		[Key(10)]
		public int AIType;
		public int GetAIType()
		{
			return AIType;
		}
		//AI表ID
		[Key(11)]
		public int Aiindex;
		public int GetAiindex()
		{
			return Aiindex;
		}
		//外观
		[Key(12)]
		public int AvatarID;
		public int GetAvatarID()
		{
			return AvatarID;
		}
		//模型半径
		[Key(13)]
		public int ModelRadius;
		public int GetModelRadius()
		{
			return ModelRadius;
		}
		//机器人组ID
		[Key(14)]
		public int GroupID;
		public int GetGroupID()
		{
			return GroupID;
		}
	}
}
