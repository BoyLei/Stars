//来源表掉落配置表_Drops.xlsx -> sheet:DropDisplay
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class DropDisplayData
	{
		[Key(0)]
		public Dictionary<int, DropDisplayDataCell> StaticDropDisplayDatas = new Dictionary<int, DropDisplayDataCell>();
	}
	[MessagePackObject]
	public class DropDisplayDataCell
	{
		//id
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//唯一识别
		[Key(1)]
		public string UniqueKey;
		//掉落显示id
		[Key(2)]
		public int Display;
		public int GetDisplay()
		{
			return Display;
		}
	}
}
