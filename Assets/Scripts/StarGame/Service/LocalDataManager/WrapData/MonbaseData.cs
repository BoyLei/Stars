//来源表Monster.xlsm.xlsx -> sheet:Monbase
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class MonbaseData
	{
		[Key(0)]
		public Dictionary<int, MonbaseDataCell> StaticMonbaseDatas = new Dictionary<int, MonbaseDataCell>();
	}
	[MessagePackObject]
	public class MonbaseDataCell
	{
		//属性模板
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//属性ID
		[Key(1)]
		public List<int> AttrID = new List<int>();
		//属性值
		[Key(2)]
		public List<long> AttrValue = new List<long>();
	}
}
