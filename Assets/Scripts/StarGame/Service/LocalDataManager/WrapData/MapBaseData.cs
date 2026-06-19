//来源表SceneMap.xlsm.xlsx -> sheet:MapBase
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class MapBaseData
	{
		[Key(0)]
		public Dictionary<int, MapBaseDataCell> StaticMapBaseDatas = new Dictionary<int, MapBaseDataCell>();
	}
	[MessagePackObject]
	public class MapBaseDataCell
	{
		//索引
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//背景音乐
		[Key(1)]
		public string BGMName;
		//地图名
		[Key(2)]
		public string MapName;
		//小地图路径
		[Key(3)]
		public string MiniMap;
		//地图宽
		[Key(4)]
		public int MapWidth;
		public int GetMapWidth()
		{
			return MapWidth;
		}
		//地图高
		[Key(5)]
		public int MapHeight;
		public int GetMapHeight()
		{
			return MapHeight;
		}
		//小地图缩放
		[Key(6)]
		public int MiniMapScale;
		public int GetMiniMapScale()
		{
			return MiniMapScale;
		}
	}
}
