//来源表关卡词缀表_LevelAffix.xlsm.xlsx -> sheet:LevelAffix
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class LevelAffixData
	{
		[Key(0)]
		public Dictionary<int, LevelAffixDataCell> StaticLevelAffixDatas = new Dictionary<int, LevelAffixDataCell>();
	}
	[MessagePackObject]
	public class LevelAffixDataCell
	{
		//机制ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//机制名称
		[Key(1)]
		public string Name
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Name); }
			set { _Name = value; }
        }
		[IgnoreMember]
		private string _Name;
		//小图标
		[Key(2)]
		public string LittleIcon;
		//机制Icon
		[Key(3)]
		public string Icon;
		//备注
		[Key(4)]
		public string Desc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Desc); }
			set { _Desc = value; }
        }
		[IgnoreMember]
		private string _Desc;
		//触发阶段
		[Key(5)]
		public List<int> Stages = new List<int>();
		//限定次数
		[Key(6)]
		public int TriggerCount;
		public int GetTriggerCount()
		{
			return TriggerCount;
		}
		//机制类型
		[Key(7)]
		public int Affix;
		public int GetAffix()
		{
			return Affix;
		}
		//效果参数1
		[Key(8)]
		public int Param1;
		public int GetParam1()
		{
			return Param1;
		}
		//效果参数2
		[Key(9)]
		public int Param2;
		public int GetParam2()
		{
			return Param2;
		}
		//效果参数3
		[Key(10)]
		public int Param3;
		public int GetParam3()
		{
			return Param3;
		}
		//附加机制
		[Key(11)]
		public List<int> Attachs = new List<int>();
	}
}
