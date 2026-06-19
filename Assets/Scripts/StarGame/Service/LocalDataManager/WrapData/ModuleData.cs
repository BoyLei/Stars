//来源表战力表_Power.xlsx -> sheet:Module
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ModuleData
	{
		[Key(0)]
		public Dictionary<int, ModuleDataCell> StaticModuleDatas = new Dictionary<int, ModuleDataCell>();
	}
	[MessagePackObject]
	public class ModuleDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//模块/养成线名称
		[Key(1)]
		public string ModuleName;
		//所属评分模块ID
		[Key(2)]
		public int FromModule;
		public int GetFromModule()
		{
			return FromModule;
		}
		//养成线跳转
		[Key(3)]
		public int SystemJump;
		public int GetSystemJump()
		{
			return SystemJump;
		}
	}
}
