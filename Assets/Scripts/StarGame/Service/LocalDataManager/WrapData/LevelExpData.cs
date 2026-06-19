//来源表角色升级表_CharacterAttr.xlsx -> sheet:LevelExp
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class LevelExpData
	{
		[Key(0)]
		public Dictionary<int, LevelExpDataCell> StaticLevelExpDatas = new Dictionary<int, LevelExpDataCell>();
	}
	[MessagePackObject]
	public class LevelExpDataCell
	{
		//角色等级
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//升级所需经验
		[Key(1)]
		public int Exp;
		public int GetExp()
		{
			return Exp;
		}
		//战力
		[Key(2)]
		public long Power;
		public long GetPower()
		{
			return Power;
		}
	}
}
