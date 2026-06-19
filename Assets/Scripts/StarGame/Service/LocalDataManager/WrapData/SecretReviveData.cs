//来源表个人秘境_PersonSecret.xlsm.xlsx -> sheet:SecretRevive
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SecretReviveData
	{
		[Key(0)]
		public Dictionary<int, SecretReviveDataCell> StaticSecretReviveDatas = new Dictionary<int, SecretReviveDataCell>();
	}
	[MessagePackObject]
	public class SecretReviveDataCell
	{
		//复活次数
		[Key(0)]
		public int Times;
		public int GetTimes()
		{
			return Times;
		}
		//削减秒数
		[Key(1)]
		public int DecSecond;
		public int GetDecSecond()
		{
			return DecSecond;
		}
	}
}
