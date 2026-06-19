//来源表心灵追踪_Track.xlsm.xlsx -> sheet:TrackPlay
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class TrackPlayData
	{
		[Key(0)]
		public Dictionary<int, TrackPlayDataCell> StaticTrackPlayDatas = new Dictionary<int, TrackPlayDataCell>();
	}
	[MessagePackObject]
	public class TrackPlayDataCell
	{
		//功能ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//副本ID
		[Key(1)]
		public int MapID;
		public int GetMapID()
		{
			return MapID;
		}
		//玩法类型
		[Key(2)]
		public int ScenePlayType;
		public int GetScenePlayType()
		{
			return ScenePlayType;
		}
		//默认开关
		[Key(3)]
		public bool DefaultOpen;
		public bool GetDefaultOpen()
		{
			return DefaultOpen;
		}
		//功能图标
		[Key(4)]
		public string Icon;
		//说明项Name
		[Key(5)]
		public string Tips;
		//SpawnerID
		[Key(6)]
		public int SpawnerID;
		public int GetSpawnerID()
		{
			return SpawnerID;
		}
		//特效长度（cm）
		[Key(7)]
		public int EffectLen;
		public int GetEffectLen()
		{
			return EffectLen;
		}
		//特效清除时间
		[Key(8)]
		public int EffectCleanTime;
		public int GetEffectCleanTime()
		{
			return EffectCleanTime;
		}
		//持续时间
		[Key(9)]
		public int Duration;
		public int GetDuration()
		{
			return Duration;
		}
		//任务ID
		[Key(10)]
		public int TaskID;
		public int GetTaskID()
		{
			return TaskID;
		}
	}
}
