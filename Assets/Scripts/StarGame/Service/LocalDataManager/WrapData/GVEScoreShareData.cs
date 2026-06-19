//来源表玩法晚间GVE_PlayModeGVE.xlsx -> sheet:GVEScoreShare
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GVEScoreShareData
	{
		[Key(0)]
		public Dictionary<int, GVEScoreShareDataCell> StaticGVEScoreShareDatas = new Dictionary<int, GVEScoreShareDataCell>();
	}
	[MessagePackObject]
	public class GVEScoreShareDataCell
	{
		//生效玩法id
		[Key(0)]
		public int PlayMode;
		public int GetPlayMode()
		{
			return PlayMode;
		}
		//队内1人积分比例
		[Key(1)]
		public int ScoOneP;
		public int GetScoOneP()
		{
			return ScoOneP;
		}
		//队内2人积分比例
		[Key(2)]
		public int ScoTwoP;
		public int GetScoTwoP()
		{
			return ScoTwoP;
		}
		//队内3人积分比例
		[Key(3)]
		public int ScoThrP;
		public int GetScoThrP()
		{
			return ScoThrP;
		}
		//队内4人积分比例
		[Key(4)]
		public int ScoFouP;
		public int GetScoFouP()
		{
			return ScoFouP;
		}
		//队内5人积分比例
		[Key(5)]
		public int ScoFiveP;
		public int GetScoFiveP()
		{
			return ScoFiveP;
		}
	}
}
