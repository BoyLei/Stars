//来源表个人秘境_PersonSecret.xlsm.xlsx -> sheet:SecretBuff
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SecretBuffData
	{
		[Key(0)]
		public Dictionary<int, SecretBuffDataCell> StaticSecretBuffDatas = new Dictionary<int, SecretBuffDataCell>();
	}
	[MessagePackObject]
	public class SecretBuffDataCell
	{
		//唯一键
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//Buff
		[Key(1)]
		public long Buff;
		public long GetBuff()
		{
			return Buff;
		}
		//库
		[Key(2)]
		public int Group;
		public int GetGroup()
		{
			return Group;
		}
		//权重
		[Key(3)]
		public int Weight;
		public int GetWeight()
		{
			return Weight;
		}
		//所属Buff库
		[Key(4)]
		public int SeasonGroup;
		public int GetSeasonGroup()
		{
			return SeasonGroup;
		}
	}
}
