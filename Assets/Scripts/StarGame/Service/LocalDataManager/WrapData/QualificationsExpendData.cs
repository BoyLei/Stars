//来源表伙伴配置表_Partner.xlsm.xlsx -> sheet:QualificationsExpend
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class QualificationsExpendData
	{
		[Key(0)]
		public Dictionary<int, QualificationsExpendDataCell> StaticQualificationsExpendDatas = new Dictionary<int, QualificationsExpendDataCell>();
	}
	[MessagePackObject]
	public class QualificationsExpendDataCell
	{
		//编号id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//消耗组ID
		[Key(1)]
		public int QualificationsExpendID;
		public int GetQualificationsExpendID()
		{
			return QualificationsExpendID;
		}
		//突破阶段
		[Key(2)]
		public int BreakStep;
		public int GetBreakStep()
		{
			return BreakStep;
		}
		//资质id
		[Key(3)]
		public List<int> QualiFications = new List<int>();
		//货币消耗
		[Key(4)]
		public List<long> Money = new List<long>();
		//道具消耗
		[Key(5)]
		public List<long> Item_id = new List<long>();
		//道具数量
		[Key(6)]
		public List<long> Item_num = new List<long>();
		//基础下限（/次）
		[Key(7)]
		public List<int> NumMin_base = new List<int>();
		//基础上限（/次）
		[Key(8)]
		public List<int> NumMax_base = new List<int>();
	}
}
