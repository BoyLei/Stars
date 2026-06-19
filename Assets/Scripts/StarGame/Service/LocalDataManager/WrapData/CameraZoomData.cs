//来源表战斗表现表_Camera.xlsx -> sheet:CameraZoom
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class CameraZoomData
	{
		[Key(0)]
		public Dictionary<int, CameraZoomDataCell> StaticCameraZoomDatas = new Dictionary<int, CameraZoomDataCell>();
	}
	[MessagePackObject]
	public class CameraZoomDataCell
	{
		//id
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
		//停留时间
		[Key(3)]
		public int Duration;
		public int GetDuration()
		{
			return Duration;
		}
		//摄像机视角
		[Key(4)]
		public int FieldView;
		public int GetFieldView()
		{
			return FieldView;
		}
		//摄像机视角转换过渡时间
		[Key(5)]
		public int TransitionDuration;
		public int GetTransitionDuration()
		{
			return TransitionDuration;
		}
	}
}
