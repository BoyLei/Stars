//来源表单人日常本表_DailyCopy.xlsm.xlsx -> sheet:DailyCopy
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class DailyCopyData
	{
		[Key(0)]
		public Dictionary<int, DailyCopyDataCell> StaticDailyCopyDatas = new Dictionary<int, DailyCopyDataCell>();
	}
	[MessagePackObject]
	public class DailyCopyDataCell
	{
		//副本ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//解锁条件
		[Key(1)]
		public int UnlockConditions;
		public int GetUnlockConditions()
		{
			return UnlockConditions;
		}
		//副本名
		[Key(2)]
		public string Name
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Name); }
			set { _Name = value; }
        }
		[IgnoreMember]
		private string _Name;
		//副本描述
		[Key(3)]
		public string Decs
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Decs); }
			set { _Decs = value; }
        }
		[IgnoreMember]
		private string _Decs;
		//副本背景图资源
		[Key(4)]
		public string BG;
	}
}
