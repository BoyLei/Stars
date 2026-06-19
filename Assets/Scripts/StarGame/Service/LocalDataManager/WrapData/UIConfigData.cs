//来源表UIConfig.xlsx -> sheet:UIConfig
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class UIConfigData
	{
		[Key(0)]
		public Dictionary<int, UIConfigDataCell> StaticUIConfigDatas = new Dictionary<int, UIConfigDataCell>();
	}
	[MessagePackObject]
	public class UIConfigDataCell
	{
		//技能类型枚举
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//标签
		[Key(1)]
		public List<int> Labels = new List<int>();
	}
}
