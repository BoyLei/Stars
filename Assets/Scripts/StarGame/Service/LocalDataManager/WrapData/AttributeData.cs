//来源表属性显示表_Attribute.xlsm.xlsx -> sheet:Attribute
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class AttributeData
	{
		[Key(0)]
		public Dictionary<int, AttributeDataCell> StaticAttributeDatas = new Dictionary<int, AttributeDataCell>();
	}
	[MessagePackObject]
	public class AttributeDataCell
	{
		//ID
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//属性id
		[Key(1)]
		public int AttrId;
		public int GetAttrId()
		{
			return AttrId;
		}
		//属性名称多语言Key
		[Key(2)]
		public string Name
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Name); }
			set { _Name = value; }
        }
		[IgnoreMember]
		private string _Name;
		//数值类型
		[Key(3)]
		public int AttType;
		public int GetAttType()
		{
			return AttType;
		}
		//页签
		[Key(4)]
		public int Tab;
		public int GetTab()
		{
			return Tab;
		}
		//属于模块
		[Key(5)]
		public int OwnModule;
		public int GetOwnModule()
		{
			return OwnModule;
		}
		//属性说明多语言Key
		[Key(6)]
		public string Des
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Des); }
			set { _Des = value; }
        }
		[IgnoreMember]
		private string _Des;
		//值计算
		[Key(7)]
		public List<string> ComputationalFormula = new List<string>();
		//属性图标
		[Key(8)]
		public string Icon;
	}
}
