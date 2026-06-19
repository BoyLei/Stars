//来源表玩法晚间GVE_PlayModeGVE.xlsx -> sheet:GVETime
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GVETimeData
	{
		[Key(0)]
		public Dictionary<int, GVETimeDataCell> StaticGVETimeDatas = new Dictionary<int, GVETimeDataCell>();
	}
	[MessagePackObject]
	public class GVETimeDataCell
	{
		//Key
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//玩法ID
		[Key(1)]
		public int PlayMode;
		public int GetPlayMode()
		{
			return PlayMode;
		}
		//生效开服时间
		[Key(2)]
		public int OpenDay;
		public int GetOpenDay()
		{
			return OpenDay;
		}
		//失效开服时间
		[Key(3)]
		public int FailureDay;
		public int GetFailureDay()
		{
			return FailureDay;
		}
		//玩法等级
		[Key(4)]
		public int EffectLevel;
		public int GetEffectLevel()
		{
			return EffectLevel;
		}
	}
}
