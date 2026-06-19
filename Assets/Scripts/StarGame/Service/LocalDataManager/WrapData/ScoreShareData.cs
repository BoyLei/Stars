//来源表ScoreShareData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class ScoreShareData
	{
		public Dictionary<int, ScoreShareDataCell> StaticScoreShareDatas = new Dictionary<int, ScoreShareDataCell>();
	}
	public class ScoreShareDataCell
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
		public List<int> PlayMode = new List<int>();
		//队内1人积分比例
		public string ScoOneP;
		private int _ScoOneP = -1;
		public int GetScoOneP()
		{
			if (_ScoOneP == -1 && int.TryParse(ScoOneP, out _ScoOneP))
			{
			}
			return _ScoOneP;
		}
		//队内2人积分比例
		public string ScoTwoP;
		private int _ScoTwoP = -1;
		public int GetScoTwoP()
		{
			if (_ScoTwoP == -1 && int.TryParse(ScoTwoP, out _ScoTwoP))
			{
			}
			return _ScoTwoP;
		}
		//队内3人积分比例
		public string ScoThrP;
		private int _ScoThrP = -1;
		public int GetScoThrP()
		{
			if (_ScoThrP == -1 && int.TryParse(ScoThrP, out _ScoThrP))
			{
			}
			return _ScoThrP;
		}
		//队内4人积分比例
		public string ScoFouP;
		private int _ScoFouP = -1;
		public int GetScoFouP()
		{
			if (_ScoFouP == -1 && int.TryParse(ScoFouP, out _ScoFouP))
			{
			}
			return _ScoFouP;
		}
		//队内5人积分比例
		public string ScoFiveP;
		private int _ScoFiveP = -1;
		public int GetScoFiveP()
		{
			if (_ScoFiveP == -1 && int.TryParse(ScoFiveP, out _ScoFiveP))
			{
			}
			return _ScoFiveP;
		}

	}
}
