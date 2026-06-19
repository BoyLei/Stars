//来源表背包配置表_ItemSpace.xlsm.xlsx -> sheet:InitSpace
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class InitSpaceData
	{
		[Key(0)]
		public Dictionary<int, InitSpaceDataCell> StaticInitSpaceDatas = new Dictionary<int, InitSpaceDataCell>();
	}
	[MessagePackObject]
	public class InitSpaceDataCell
	{
		//编号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//服务器名
		[Key(1)]
		public string ServerName;
		//对应的背包初始化id集合
		[Key(2)]
		public List<int> Initspace = new List<int>();
	}
}
