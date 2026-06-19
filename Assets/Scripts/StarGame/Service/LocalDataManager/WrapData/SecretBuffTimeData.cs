//来源表个人秘境_PersonSecret.xlsm.xlsx -> sheet:SecretBuffTime
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SecretBuffTimeData
	{
		[Key(0)]
		public Dictionary<int, SecretBuffTimeDataCell> StaticSecretBuffTimeDatas = new Dictionary<int, SecretBuffTimeDataCell>();
	}
	[MessagePackObject]
	public class SecretBuffTimeDataCell
	{
		//时间类型
		[Key(0)]
		public int TimeType;
		public int GetTimeType()
		{
			return TimeType;
		}
		//时间
		[Key(1)]
		public int Time;
		public int GetTime()
		{
			return Time;
		}
	}
}
