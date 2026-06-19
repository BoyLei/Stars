//来源表派对时刻_PartyTime.xlsm.xlsx -> sheet:PartyTimeAct
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class PartyTimeActData
	{
		[Key(0)]
		public Dictionary<long, PartyTimeActDataCell> StaticPartyTimeActDatas = new Dictionary<long, PartyTimeActDataCell>();
	}
	[MessagePackObject]
	public class PartyTimeActDataCell
	{
		//对应活动玩法表的主Key
		[Key(0)]
		public long ID;
		public long GetID()
		{
			return ID;
		}
		//小游戏类型
		[Key(1)]
		public int GameType;
		public int GetGameType()
		{
			return GameType;
		}
	}
}
