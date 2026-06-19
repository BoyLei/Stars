//来源表属性天赋树表_AttriTalentTree.xlsm.xlsx -> sheet:AttriTTCondition
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class AttriTTConditionData
	{
		[Key(0)]
		public Dictionary<int, AttriTTConditionDataCell> StaticAttriTTConditionDatas = new Dictionary<int, AttriTTConditionDataCell>();
	}
	[MessagePackObject]
	public class AttriTTConditionDataCell
	{
		//根节点id
		[Key(0)]
		public int RootNode;
		public int GetRootNode()
		{
			return RootNode;
		}
		//生效职业
		[Key(1)]
		public int JobID;
		public int GetJobID()
		{
			return JobID;
		}
		//解锁条件组
		[Key(2)]
		public int Condition;
		public int GetCondition()
		{
			return Condition;
		}
		//根节点显示用名称多语言Key
		[Key(3)]
		public string RootName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_RootName); }
			set { _RootName = value; }
        }
		[IgnoreMember]
		private string _RootName;
		//职业天赋树显示用名称多语言Key
		[Key(4)]
		public string TreeName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_TreeName); }
			set { _TreeName = value; }
        }
		[IgnoreMember]
		private string _TreeName;
	}
}
