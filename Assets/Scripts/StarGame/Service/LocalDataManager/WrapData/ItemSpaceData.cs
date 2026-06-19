//来源表背包配置表_ItemSpace.xlsm.xlsx -> sheet:ItemSpace
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ItemSpaceData
	{
		[Key(0)]
		public Dictionary<int, ItemSpaceDataCell> StaticItemSpaceDatas = new Dictionary<int, ItemSpaceDataCell>();
	}
	[MessagePackObject]
	public class ItemSpaceDataCell
	{
		//编号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//类型
		[Key(1)]
		public int BagType;
		public int GetBagType()
		{
			return BagType;
		}
		//描述
		[Key(2)]
		public string Desc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Desc); }
			set { _Desc = value; }
        }
		[IgnoreMember]
		private string _Desc;
		//背包格数
		[Key(3)]
		public int BagNum;
		public int GetBagNum()
		{
			return BagNum;
		}
		//是否需要保存
		[Key(4)]
		public bool IsNeedSave;
		public bool GetIsNeedSave()
		{
			return IsNeedSave;
		}
	}
}
