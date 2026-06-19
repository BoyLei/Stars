//来源表RotationToVector.xlsx -> sheet:RotationToVector
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class RotationToVectorData
	{
		[Key(0)]
		public Dictionary<int, RotationToVectorDataCell> StaticRotationToVectorDatas = new Dictionary<int, RotationToVectorDataCell>();
	}
	[MessagePackObject]
	public class RotationToVectorDataCell
	{
		//编号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//消息类型
		[Key(1)]
		public float X;
		public float GetX()
		{
			return X;
		}
		//消息字符串
		[Key(2)]
		public float Z;
		public float GetZ()
		{
			return Z;
		}
	}
}
