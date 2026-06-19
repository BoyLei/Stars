//来源表GM.xlsx -> sheet:GM
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GMData
	{
		[Key(0)]
		public Dictionary<int, GMDataCell> StaticGMDatas = new Dictionary<int, GMDataCell>();
	}
	[MessagePackObject]
	public class GMDataCell
	{
		//编号
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//分组(1战斗/2关卡/3系统/4数值/5活动/6自定义/7自输入)
		[Key(1)]
		public int Tag;
		public int GetTag()
		{
			return Tag;
		}
		//GM描述
		[Key(2)]
		public string Name;
		//GM指令
		[Key(3)]
		public string GMCMD;
		//默认参数
		[Key(4)]
		public string Param;
		//参数类型(0直接发送/1输入框/2True/False按钮)
		[Key(5)]
		public string ParamType;
	}
}
