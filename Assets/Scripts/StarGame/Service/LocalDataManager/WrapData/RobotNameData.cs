//来源表机器人表_Robot.xlsm.xlsx -> sheet:RobotName
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class RobotNameData
	{
		[Key(0)]
		public Dictionary<int, RobotNameDataCell> StaticRobotNameDatas = new Dictionary<int, RobotNameDataCell>();
	}
	[MessagePackObject]
	public class RobotNameDataCell
	{
		//序号
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//名称
		[Key(1)]
		public string Name;
		//组别
		[Key(2)]
		public int Group;
		public int GetGroup()
		{
			return Group;
		}
	}
}
