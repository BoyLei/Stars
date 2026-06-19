//来源表Sound.xlsx -> sheet:Audio
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class AudioData
	{
		[Key(0)]
		public Dictionary<int, AudioDataCell> StaticAudioDatas = new Dictionary<int, AudioDataCell>();
	}
	[MessagePackObject]
	public class AudioDataCell
	{
		//声音id
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//资源名
		[Key(1)]
		public string ResName;
		//优先级
		[Key(2)]
		public string Priority;
		//区块地图区分
		[Key(3)]
		public string PartId;
		//功能区分模块
		[Key(4)]
		public string ModuleId;
		//触发器枚举
		[Key(5)]
		public string TriMethod;
	}
}
