//来源表玩法晚间GVE_PlayModeGVE.xlsx -> sheet:GVEBossTypeNum
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GVEBossTypeNumData
	{
		[Key(0)]
		public Dictionary<int, GVEBossTypeNumDataCell> StaticGVEBossTypeNumDatas = new Dictionary<int, GVEBossTypeNumDataCell>();
	}
	[MessagePackObject]
	public class GVEBossTypeNumDataCell
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
		//BOSS类别
		[Key(2)]
		public int BossType;
		public int GetBossType()
		{
			return BossType;
		}
		//是否取出不放回
		[Key(3)]
		public bool OnlyOnce;
		public bool GetOnlyOnce()
		{
			return OnlyOnce;
		}
		//刷出最大数量
		[Key(4)]
		public int MaxNum;
		public int GetMaxNum()
		{
			return MaxNum;
		}
	}
}
