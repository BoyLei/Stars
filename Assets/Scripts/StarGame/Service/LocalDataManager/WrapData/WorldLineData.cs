//来源表剧情配置表_Plot.xlsm.xlsx -> sheet:WorldLine
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class WorldLineData
	{
		[Key(0)]
		public Dictionary<int, WorldLineDataCell> StaticWorldLineDatas = new Dictionary<int, WorldLineDataCell>();
	}
	[MessagePackObject]
	public class WorldLineDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//节点类型
		[Key(1)]
		public int NodeType;
		public int GetNodeType()
		{
			return NodeType;
		}
		//节点标题
		[Key(2)]
		public string NodeTitle
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_NodeTitle); }
			set { _NodeTitle = value; }
        }
		[IgnoreMember]
		private string _NodeTitle;
		//预览文本
		[Key(3)]
		public string UnfinishedText
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_UnfinishedText); }
			set { _UnfinishedText = value; }
        }
		[IgnoreMember]
		private string _UnfinishedText;
		//完成文本
		[Key(4)]
		public string FinishText
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_FinishText); }
			set { _FinishText = value; }
        }
		[IgnoreMember]
		private string _FinishText;
		//前置节点id
		[Key(5)]
		public int PreNode;
		public int GetPreNode()
		{
			return PreNode;
		}
		//接取任务
		[Key(6)]
		public int AccessTask;
		public int GetAccessTask()
		{
			return AccessTask;
		}
		//完成任务
		[Key(7)]
		public int FinishTask;
		public int GetFinishTask()
		{
			return FinishTask;
		}
		//配图
		[Key(8)]
		public string Image;
		//未完成时是否显示配图
		[Key(9)]
		public bool UnfinishImageShow;
		public bool GetUnfinishImageShow()
		{
			return UnfinishImageShow;
		}
		//未接取时是否显示标题
		[Key(10)]
		public bool UnaccessTitleShow;
		public bool GetUnaccessTitleShow()
		{
			return UnaccessTitleShow;
		}
		//未完成前置节点时否显示节点
		[Key(11)]
		public bool UnfinishPreNodeShow;
		public bool GetUnfinishPreNodeShow()
		{
			return UnfinishPreNodeShow;
		}
		//节点奖励
		[Key(12)]
		public List<long> Reward = new List<long>();
		//奖励数量
		[Key(13)]
		public List<long> RewardNum = new List<long>();
	}
}
