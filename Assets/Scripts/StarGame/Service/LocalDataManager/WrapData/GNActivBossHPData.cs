//来源表公会午间活动表_GuildNoonActiv.xlsm.xlsx -> sheet:GNActivBossHP
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GNActivBossHPData
	{
		[Key(0)]
		public Dictionary<int, GNActivBossHPDataCell> StaticGNActivBossHPDatas = new Dictionary<int, GNActivBossHPDataCell>();
	}
	[MessagePackObject]
	public class GNActivBossHPDataCell
	{
		//开服天数
		[Key(0)]
		public int OpenDays;
		public int GetOpenDays()
		{
			return OpenDays;
		}
		//血条血量
		[Key(1)]
		public int HP;
		public int GetHP()
		{
			return HP;
		}
	}
}
