//来源表BossSelectData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class BossSelectData
	{
		public Dictionary<int, BossSelectDataCell> StaticBossSelectDatas = new Dictionary<int, BossSelectDataCell>();
	}
	public class BossSelectDataCell
	{
		//主Key
		public string Id;
		private int _Id = -1;
		public int GetId()
		{
			if (_Id == -1 && int.TryParse(Id, out _Id))
			{
			}
			return _Id;
		}
		//生效玩法ID
		public string PlayMode;
		private int _PlayMode = -1;
		public int GetPlayMode()
		{
			if (_PlayMode == -1 && int.TryParse(PlayMode, out _PlayMode))
			{
			}
			return _PlayMode;
		}
		//使用BOSS
		public string UseBoss;
		private long _UseBoss = -1;
		public long GetUseBoss()
		{
			if (_UseBoss == -1 && long.TryParse(UseBoss, out _UseBoss))
			{
			}
			return _UseBoss;
		}
		//BOSS类别
		public string BossType;
		private int _BossType = -1;
		public int GetBossType()
		{
			if (_BossType == -1 && int.TryParse(BossType, out _BossType))
			{
			}
			return _BossType;
		}
		//BOSS随机权重
		public string Num;
		private int _Num = -1;
		public int GetNum()
		{
			if (_Num == -1 && int.TryParse(Num, out _Num))
			{
			}
			return _Num;
		}

	}
}
