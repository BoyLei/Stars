//来源表玩法晚间GVE_PlayModeGVE.xlsx -> sheet:GVEMonRefresh
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GVEMonRefreshData
	{
		[Key(0)]
		public Dictionary<int, GVEMonRefreshDataCell> StaticGVEMonRefreshDatas = new Dictionary<int, GVEMonRefreshDataCell>();
	}
	[MessagePackObject]
	public class GVEMonRefreshDataCell
	{
		//主Key
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//生效玩法ID
		[Key(1)]
		public int PlayMode;
		public int GetPlayMode()
		{
			return PlayMode;
		}
		//支持地图
		[Key(2)]
		public List<int> Maps = new List<int>();
		//小怪群落
		[Key(3)]
		public List<int> Lackey = new List<int>();
		//精英群落
		[Key(4)]
		public List<int> Elite = new List<int>();
		//小怪积分
		[Key(5)]
		public int LackeyScore;
		public int GetLackeyScore()
		{
			return LackeyScore;
		}
		//精英积分
		[Key(6)]
		public int EliteScore;
		public int GetEliteScore()
		{
			return EliteScore;
		}
		//boss积分
		[Key(7)]
		public int BossScore;
		public int GetBossScore()
		{
			return BossScore;
		}
	}
}
