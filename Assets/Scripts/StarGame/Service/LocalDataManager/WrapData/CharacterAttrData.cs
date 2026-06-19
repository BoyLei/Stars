//来源表角色升级表_CharacterAttr.xlsx -> sheet:CharacterAttr
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class CharacterAttrData
	{
		[Key(0)]
		public Dictionary<int, CharacterAttrDataCell> StaticCharacterAttrDatas = new Dictionary<int, CharacterAttrDataCell>();
	}
	[MessagePackObject]
	public class CharacterAttrDataCell
	{
		//编号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//职业
		[Key(1)]
		public List<int> JobID = new List<int>();
		//角色等级
		[Key(2)]
		public int Level;
		public int GetLevel()
		{
			return Level;
		}
		//升级奖励
		[Key(3)]
		public long Award;
		public long GetAward()
		{
			return Award;
		}
		//累计战力
		[Key(4)]
		public int Fight;
		public int GetFight()
		{
			return Fight;
		}
		//属性ID
		[Key(5)]
		public List<int> AttrID = new List<int>();
		//属性值
		[Key(6)]
		public List<int> AttrValue = new List<int>();
	}
}
