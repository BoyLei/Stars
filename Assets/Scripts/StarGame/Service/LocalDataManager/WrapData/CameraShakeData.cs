//来源表战斗表现表_Camera.xlsx -> sheet:CameraShake
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class CameraShakeData
	{
		[Key(0)]
		public Dictionary<int, CameraShakeDataCell> StaticCameraShakeDatas = new Dictionary<int, CameraShakeDataCell>();
	}
	[MessagePackObject]
	public class CameraShakeDataCell
	{
		//震屏id
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//优先级
		[Key(1)]
		public int Priority;
		public int GetPriority()
		{
			return Priority;
		}
		//广播类型
		[Key(2)]
		public int BroadCastType;
		public int GetBroadCastType()
		{
			return BroadCastType;
		}
		//持续时间
		[Key(3)]
		public int Duration;
		public int GetDuration()
		{
			return Duration;
		}
		//频率次数
		[Key(4)]
		public int Frequency;
		public int GetFrequency()
		{
			return Frequency;
		}
		//震屏方向
		[Key(5)]
		public int Type;
		public int GetType()
		{
			return Type;
		}
		//幅度
		[Key(6)]
		public int Amplitude;
		public int GetAmplitude()
		{
			return Amplitude;
		}
	}
}
