//来源表Job.xlsm.xlsx -> sheet:Job
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class JobData
	{
		[Key(0)]
		public Dictionary<int, JobDataCell> StaticJobDatas = new Dictionary<int, JobDataCell>();
	}
	[MessagePackObject]
	public class JobDataCell
	{
		//编号
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//职业名
		[Key(1)]
		public string Name
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Name); }
			set { _Name = value; }
        }
		[IgnoreMember]
		private string _Name;
		//职业阶层
		[Key(2)]
		public int TransLevel;
		public int GetTransLevel()
		{
			return TransLevel;
		}
		//基础职业
		[Key(3)]
		public int BaseJob;
		public int GetBaseJob()
		{
			return BaseJob;
		}
		//可进阶职业
		[Key(4)]
		public List<int> TransferJob = new List<int>();
		//职业冲刺技能
		[Key(5)]
		public int DashJobSkills;
		public int GetDashJobSkills()
		{
			return DashJobSkills;
		}
		//职业普攻技能
		[Key(6)]
		public int AttackJobSkill;
		public int GetAttackJobSkill()
		{
			return AttackJobSkill;
		}
		//初始职业技能
		[Key(7)]
		public List<int> BornJobSkills = new List<int>();
		//进阶职业技能
		[Key(8)]
		public List<int> TransferJobSkills = new List<int>();
		//特性职业技能
		[Key(9)]
		public int UltimateJobSkills;
		public int GetUltimateJobSkills()
		{
			return UltimateJobSkills;
		}
		//挂载被动技能
		[Key(10)]
		public List<int> PassiveSkills = new List<int>();
		//性别
		[Key(11)]
		public int Sex;
		public int GetSex()
		{
			return Sex;
		}
		//化身id
		[Key(12)]
		public int AvatarID;
		public int GetAvatarID()
		{
			return AvatarID;
		}
		//高模化身id
		[Key(13)]
		public int HighAvatarID;
		public int GetHighAvatarID()
		{
			return HighAvatarID;
		}
		//模型半径
		[Key(14)]
		public int ModelRadius;
		public int GetModelRadius()
		{
			return ModelRadius;
		}
		//转职任务
		[Key(15)]
		public int TransferTask;
		public int GetTransferTask()
		{
			return TransferTask;
		}
		//增加属性
		[Key(16)]
		public List<int> AttrID = new List<int>();
		//增加属性数值
		[Key(17)]
		public List<int> AttrValue = new List<int>();
		//基础职业icon
		[Key(18)]
		public string JobIcon;
		//进阶职业icon
		[Key(19)]
		public string JobIcon2;
		//职业大图标
		[Key(20)]
		public string JobIcon3;
		//转职文本描述
		[Key(21)]
		public string TransferDesc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_TransferDesc); }
			set { _TransferDesc = value; }
        }
		[IgnoreMember]
		private string _TransferDesc;
		//量谱1
		[Key(22)]
		public string Spectral1
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Spectral1); }
			set { _Spectral1 = value; }
        }
		[IgnoreMember]
		private string _Spectral1;
		//量谱2
		[Key(23)]
		public string Spectral2;
		//量谱3
		[Key(24)]
		public string Spectral3;
	}
}
