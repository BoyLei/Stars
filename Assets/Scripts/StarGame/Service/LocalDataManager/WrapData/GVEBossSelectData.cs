//来源表玩法晚间GVE_PlayModeGVE.xlsx -> sheet:GVEBossSelect
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GVEBossSelectData
	{
		[Key(0)]
		public Dictionary<int, GVEBossSelectDataCell> StaticGVEBossSelectDatas = new Dictionary<int, GVEBossSelectDataCell>();
	}
	[MessagePackObject]
	public class GVEBossSelectDataCell
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
		//使用BOSS
		[Key(2)]
		public long UseBoss;
		public long GetUseBoss()
		{
			return UseBoss;
		}
		//BOSS类别
		[Key(3)]
		public int BossType;
		public int GetBossType()
		{
			return BossType;
		}
		//BOSS随机权重
		[Key(4)]
		public int Num;
		public int GetNum()
		{
			return Num;
		}
	}
}
