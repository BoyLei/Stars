//来源表DailyCopyConstData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class DailyCopyConstData
	{
		public Dictionary<int, DailyCopyConstDataCell> StaticDailyCopyConstDatas = new Dictionary<int, DailyCopyConstDataCell>();
	}
	public class DailyCopyConstDataCell
	{
		//初始默认次数
		public string DefaultTimes;
		private int _DefaultTimes = -1;
		public int GetDefaultTimes()
		{
			if (_DefaultTimes == -1 && int.TryParse(DefaultTimes, out _DefaultTimes))
			{
			}
			return _DefaultTimes;
		}
		//每日回复次数
		public string DailyReplyTimes;
		private int _DailyReplyTimes = -1;
		public int GetDailyReplyTimes()
		{
			if (_DailyReplyTimes == -1 && int.TryParse(DailyReplyTimes, out _DailyReplyTimes))
			{
			}
			return _DailyReplyTimes;
		}
		//挑战次数上限
		public string ChallengeLimits;
		private int _ChallengeLimits = -1;
		public int GetChallengeLimits()
		{
			if (_ChallengeLimits == -1 && int.TryParse(ChallengeLimits, out _ChallengeLimits))
			{
			}
			return _ChallengeLimits;
		}
		//宝箱自动开启倒计时
		public string BoxCountdown;
		private int _BoxCountdown = -1;
		public int GetBoxCountdown()
		{
			if (_BoxCountdown == -1 && int.TryParse(BoxCountdown, out _BoxCountdown))
			{
			}
			return _BoxCountdown;
		}

	}
}
