//来源表战场表_BattleField.xlsm.xlsx -> sheet:BattleFieldSession
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class BattleFieldSessionData
	{
		[Key(0)]
		public Dictionary<int, BattleFieldSessionDataCell> StaticBattleFieldSessionDatas = new Dictionary<int, BattleFieldSessionDataCell>();
	}
	[MessagePackObject]
	public class BattleFieldSessionDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//场次
		[Key(1)]
		public int Session;
		public int GetSession()
		{
			return Session;
		}
		//阶段类型
		[Key(2)]
		public int StageType;
		public int GetStageType()
		{
			return StageType;
		}
		//开始时间
		[Key(3)]
		public long Start;
		public long GetStart()
		{
			return Start;
		}
		//结束时间
		[Key(4)]
		public long End;
		public long GetEnd()
		{
			return End;
		}
	}
}
