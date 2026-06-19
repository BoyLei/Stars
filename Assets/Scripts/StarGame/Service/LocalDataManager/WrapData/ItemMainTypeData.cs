//来源表道具配置表_Item.xlsm.xlsx -> sheet:ItemMainType
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ItemMainTypeData
	{
		[Key(0)]
		public Dictionary<int, ItemMainTypeDataCell> StaticItemMainTypeDatas = new Dictionary<int, ItemMainTypeDataCell>();
	}
	[MessagePackObject]
	public class ItemMainTypeDataCell
	{
		//编号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//道具类型
		[Key(1)]
		public int ItemType;
		public int GetItemType()
		{
			return ItemType;
		}
		//类型名称
		[Key(2)]
		public string Name
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Name); }
			set { _Name = value; }
        }
		[IgnoreMember]
		private string _Name;
		//生效筛选器
		[Key(3)]
		public List<int> OperaFilter = new List<int>();
	}
}
