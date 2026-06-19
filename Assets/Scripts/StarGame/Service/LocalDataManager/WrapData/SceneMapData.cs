//来源表SceneMap.xlsm.xlsx -> sheet:SceneMap
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SceneMapData
	{
		[Key(0)]
		public Dictionary<int, SceneMapDataCell> StaticSceneMapDatas = new Dictionary<int, SceneMapDataCell>();
	}
	[MessagePackObject]
	public class SceneMapDataCell
	{
		//地图ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//场景名称
		[Key(1)]
		public string SceneName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_SceneName); }
			set { _SceneName = value; }
        }
		[IgnoreMember]
		private string _SceneName;
		//地图的基础信息
		[Key(2)]
		public int BaseID;
		public int GetBaseID()
		{
			return BaseID;
		}
		//副本UI
		[Key(3)]
		public string LevelPage;
		//场景类型
		[Key(4)]
		public int SceneType;
		public int GetSceneType()
		{
			return SceneType;
		}
		//场景限制
		[Key(5)]
		public List<int> SceneLimit = new List<int>();
		//复活配置
		[Key(6)]
		public int ReviveTid;
		public int GetReviveTid()
		{
			return ReviveTid;
		}
		//进入场景提示资源
		[Key(7)]
		public string EnterTip;
		//开放角色等级
		[Key(8)]
		public int OpenRoleLv;
		public int GetOpenRoleLv()
		{
			return OpenRoleLv;
		}
		//开放需要完成任务
		[Key(9)]
		public int OpenComTask;
		public int GetOpenComTask()
		{
			return OpenComTask;
		}
		//是否隐藏伙伴
		[Key(10)]
		public bool IsHidePartner;
		public bool GetIsHidePartner()
		{
			return IsHidePartner;
		}
		//是否隐藏其他玩家
		[Key(11)]
		public bool IsHideOtherPlayer;
		public bool GetIsHideOtherPlayer()
		{
			return IsHideOtherPlayer;
		}
		//是否可旋转镜头
		[Key(12)]
		public bool IsCameraRotate;
		public bool GetIsCameraRotate()
		{
			return IsCameraRotate;
		}
		//是否可超链接传送
		[Key(13)]
		public bool IsCameraTeleport;
		public bool GetIsCameraTeleport()
		{
			return IsCameraTeleport;
		}
	}
}
