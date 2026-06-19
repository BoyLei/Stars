//来源表玩法开启表_PlayOpenConditions.xlsm.xlsx -> sheet:PlayOpenConditions
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class PlayOpenConditionsData
	{
		[Key(0)]
		public Dictionary<int, PlayOpenConditionsDataCell> StaticPlayOpenConditionsDatas = new Dictionary<int, PlayOpenConditionsDataCell>();
	}
	[MessagePackObject]
	public class PlayOpenConditionsDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//玩法名称多语言Key
		[Key(1)]
		public string PlayName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_PlayName); }
			set { _PlayName = value; }
        }
		[IgnoreMember]
		private string _PlayName;
		//人数
		[Key(2)]
		public int PeopleNum;
		public int GetPeopleNum()
		{
			return PeopleNum;
		}
		//等级
		[Key(3)]
		public int Level;
		public int GetLevel()
		{
			return Level;
		}
		//时间
		[Key(4)]
		public int Time;
		public int GetTime()
		{
			return Time;
		}
		//时间描述多语言Key
		[Key(5)]
		public string TimeDesc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_TimeDesc); }
			set { _TimeDesc = value; }
        }
		[IgnoreMember]
		private string _TimeDesc;
		//地图ID
		[Key(6)]
		public int MapID;
		public int GetMapID()
		{
			return MapID;
		}
		//开启NPCID
		[Key(7)]
		public int NPCID;
		public int GetNPCID()
		{
			return NPCID;
		}
		//地图ID
		[Key(8)]
		public int MapID2;
		public int GetMapID2()
		{
			return MapID2;
		}
		//开启NPCID
		[Key(9)]
		public int NPCID2;
		public int GetNPCID2()
		{
			return NPCID2;
		}
		//寻路索引变更条件
		[Key(10)]
		public int IndexChangeCondition;
		public int GetIndexChangeCondition()
		{
			return IndexChangeCondition;
		}
		//预览奖励
		[Key(11)]
		public long AwardID;
		public long GetAwardID()
		{
			return AwardID;
		}
		//玩法开启ID
		[Key(12)]
		public int PlayerGuidance;
		public int GetPlayerGuidance()
		{
			return PlayerGuidance;
		}
		//玩法tips描述多语言Key
		[Key(13)]
		public string TipsDec
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_TipsDec); }
			set { _TipsDec = value; }
        }
		[IgnoreMember]
		private string _TipsDec;
		//玩法任务标题多语言Key
		[Key(14)]
		public string TaskTitle
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_TaskTitle); }
			set { _TaskTitle = value; }
        }
		[IgnoreMember]
		private string _TaskTitle;
		//玩法任务目标多语言Key
		[Key(15)]
		public string TaskTarget
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_TaskTarget); }
			set { _TaskTarget = value; }
        }
		[IgnoreMember]
		private string _TaskTarget;
		//玩法任务描述多语言Key
		[Key(16)]
		public string TaskDesc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_TaskDesc); }
			set { _TaskDesc = value; }
        }
		[IgnoreMember]
		private string _TaskDesc;
		//X偏移量
		[Key(17)]
		public int OffsetX;
		public int GetOffsetX()
		{
			return OffsetX;
		}
		//Y偏移量
		[Key(18)]
		public int OffsetY;
		public int GetOffsetY()
		{
			return OffsetY;
		}
		//掉落标签色值
		[Key(19)]
		public string TagColor;
		//掉落标签
		[Key(20)]
		public string DropTag
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_DropTag); }
			set { _DropTag = value; }
        }
		[IgnoreMember]
		private string _DropTag;
	}
}
