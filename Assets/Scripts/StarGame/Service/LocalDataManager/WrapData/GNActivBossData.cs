//来源表公会午间活动表_GuildNoonActiv.xlsm.xlsx -> sheet:GNActivBoss
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GNActivBossData
	{
		[Key(0)]
		public Dictionary<long, GNActivBossDataCell> StaticGNActivBossDatas = new Dictionary<long, GNActivBossDataCell>();
	}
	[MessagePackObject]
	public class GNActivBossDataCell
	{
		//ID
		[Key(0)]
		public long ID;
		public long GetID()
		{
			return ID;
		}
		//关联关卡
		[Key(1)]
		public int LevelID;
		public int GetLevelID()
		{
			return LevelID;
		}
		//BOSS出现日期
		[Key(2)]
		public List<int> Schedule = new List<int>();
		//BOSS名
		[Key(3)]
		public int BOSSName;
		public int GetBOSSName()
		{
			return BOSSName;
		}
		//BOSS形象
		[Key(4)]
		public int AvatarID;
		public int GetAvatarID()
		{
			return AvatarID;
		}
	}
}
