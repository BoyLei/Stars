//来源表个人秘境_PersonSecret.xlsm.xlsx -> sheet:SecretSettle
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SecretSettleData
	{
		[Key(0)]
		public Dictionary<int, SecretSettleDataCell> StaticSecretSettleDatas = new Dictionary<int, SecretSettleDataCell>();
	}
	[MessagePackObject]
	public class SecretSettleDataCell
	{
		//唯一键
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//剩余时间
		[Key(1)]
		public int RemainTime;
		public int GetRemainTime()
		{
			return RemainTime;
		}
		//宝箱数量
		[Key(2)]
		public long BoxNum;
		public long GetBoxNum()
		{
			return BoxNum;
		}
		//解锁层数
		[Key(3)]
		public int Unlock;
		public int GetUnlock()
		{
			return Unlock;
		}
	}
}
