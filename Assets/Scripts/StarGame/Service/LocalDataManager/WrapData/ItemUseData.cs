//来源表道具配置表_Item.xlsm.xlsx -> sheet:ItemUse
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ItemUseData
	{
		[Key(0)]
		public Dictionary<long, ItemUseDataCell> StaticItemUseDatas = new Dictionary<long, ItemUseDataCell>();
	}
	[MessagePackObject]
	public class ItemUseDataCell
	{
		//编号
		[Key(0)]
		public long Id;
		public long GetId()
		{
			return Id;
		}
		//效果类型
		[Key(1)]
		public int EffectType;
		public int GetEffectType()
		{
			return EffectType;
		}
		//使用类型
		[Key(2)]
		public int TypeID;
		public int GetTypeID()
		{
			return TypeID;
		}
		//效果ID
		[Key(3)]
		public long EffectID;
		public long GetEffectID()
		{
			return EffectID;
		}
		//效果值
		[Key(4)]
		public long EffectValue;
		public long GetEffectValue()
		{
			return EffectValue;
		}
		//CD组
		[Key(5)]
		public int Group;
		public int GetGroup()
		{
			return Group;
		}
		//使用CD
		[Key(6)]
		public int CDTime;
		public int GetCDTime()
		{
			return CDTime;
		}
	}
}
