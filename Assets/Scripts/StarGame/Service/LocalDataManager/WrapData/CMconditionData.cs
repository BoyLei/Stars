//来源表通用条件表_CommonCondition.xlsm.xlsx -> sheet:CMcondition
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class CMconditionData
	{
		[Key(0)]
		public Dictionary<int, CMconditionDataCell> StaticCMconditionDatas = new Dictionary<int, CMconditionDataCell>();
	}
	[MessagePackObject]
	public class CMconditionDataCell
	{
		//条件ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//条件组ID
		[Key(1)]
		public int GroupID;
		public int GetGroupID()
		{
			return GroupID;
		}
		//条件组子ID
		[Key(2)]
		public int SubGroupID;
		public int GetSubGroupID()
		{
			return SubGroupID;
		}
		//提示描述多语言Key
		[Key(3)]
		public string Desc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Desc); }
			set { _Desc = value; }
        }
		[IgnoreMember]
		private string _Desc;
		//条件类型
		[Key(4)]
		public int ConditionType;
		public int GetConditionType()
		{
			return ConditionType;
		}
		//不满足时是否显示
		[Key(5)]
		public bool IsShow;
		public bool GetIsShow()
		{
			return IsShow;
		}
		//是否取反
		[Key(6)]
		public bool Flag;
		public bool GetFlag()
		{
			return Flag;
		}
		//参数1
		[Key(7)]
		public string Args1;
		//参数2
		[Key(8)]
		public string Args2;
		//参数3
		[Key(9)]
		public string Args3;
		//参数4
		[Key(10)]
		public string Args4;
		//参数5
		[Key(11)]
		public string Args5;
		//参数6
		[Key(12)]
		public string Args6;
		//参数7
		[Key(13)]
		public string Args7;
		//参数8
		[Key(14)]
		public string Args8;
		//参数9
		[Key(15)]
		public string Args9;
		//参数10
		[Key(16)]
		public string Args10;
	}
}
