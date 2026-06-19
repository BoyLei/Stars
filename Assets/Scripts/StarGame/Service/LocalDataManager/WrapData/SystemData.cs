//来源表System.xlsx -> sheet:System
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SystemData
	{
		[Key(0)]
		public Dictionary<int, SystemDataCell> StaticSystemDatas = new Dictionary<int, SystemDataCell>();
	}
	[MessagePackObject]
	public class SystemDataCell
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
		public int Value;
		public int GetValue()
		{
			return Value;
		}
		//服务器所需备注
		[Key(2)]
		public string Note;
		//枚举类型
		[Key(3)]
		public string EnumName;
	}
}
