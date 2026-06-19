//来源表BossTypeNumData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class BossTypeNumData
	{
		public Dictionary<int, BossTypeNumDataCell> StaticBossTypeNumDatas = new Dictionary<int, BossTypeNumDataCell>();
	}
	public class BossTypeNumDataCell
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
		//是否取出不放回
		public string OnlyOnce;
		public bool GetOnlyOnce()
		{
			bool _OnlyOnce;
			if (bool.TryParse(OnlyOnce, out _OnlyOnce))
			{
			}
			return _OnlyOnce;
		}
		//刷出最大数量
		public string MaxNum;
		private int _MaxNum = -1;
		public int GetMaxNum()
		{
			if (_MaxNum == -1 && int.TryParse(MaxNum, out _MaxNum))
			{
			}
			return _MaxNum;
		}

	}
}
