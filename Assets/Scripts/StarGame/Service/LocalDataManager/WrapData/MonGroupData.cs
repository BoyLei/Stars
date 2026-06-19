//来源表怪物组表_MonGroup.xlsx -> sheet:MonGroup
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class MonGroupData
	{
		[Key(0)]
		public Dictionary<int, MonGroupDataCell> StaticMonGroupDatas = new Dictionary<int, MonGroupDataCell>();
	}
	[MessagePackObject]
	public class MonGroupDataCell
	{
		//唯一键
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//怪物组id
		[Key(1)]
		public int Group;
		public int GetGroup()
		{
			return Group;
		}
		//怪物id
		[Key(2)]
		public long AttrID;
		public long GetAttrID()
		{
			return AttrID;
		}
		//数量
		[Key(3)]
		public int Num;
		public int GetNum()
		{
			return Num;
		}
		//附加进度值
		[Key(4)]
		public int Progress;
		public int GetProgress()
		{
			return Progress;
		}
	}
}
