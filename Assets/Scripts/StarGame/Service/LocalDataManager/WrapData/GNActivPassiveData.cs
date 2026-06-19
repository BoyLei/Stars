//来源表公会午间活动表_GuildNoonActiv.xlsm.xlsx -> sheet:GNActivPassive
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GNActivPassiveData
	{
		[Key(0)]
		public Dictionary<int, GNActivPassiveDataCell> StaticGNActivPassiveDatas = new Dictionary<int, GNActivPassiveDataCell>();
	}
	[MessagePackObject]
	public class GNActivPassiveDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//对应被动ID
		[Key(1)]
		public int PassiveID;
		public int GetPassiveID()
		{
			return PassiveID;
		}
		//所属被动库
		[Key(2)]
		public int PassiveType;
		public int GetPassiveType()
		{
			return PassiveType;
		}
		//出现权重
		[Key(3)]
		public int Weight;
		public int GetWeight()
		{
			return Weight;
		}
		//被动名
		[Key(4)]
		public string PassiveName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_PassiveName); }
			set { _PassiveName = value; }
        }
		[IgnoreMember]
		private string _PassiveName;
		//被动文字描述
		[Key(5)]
		public string PassiveDesc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_PassiveDesc); }
			set { _PassiveDesc = value; }
        }
		[IgnoreMember]
		private string _PassiveDesc;
		//被动数值描述
		[Key(6)]
		public string PassiveNum;
		//被动icon
		[Key(7)]
		public string PassiveIcon;
		//被动底图
		[Key(8)]
		public string PassiveBigIcon;
	}
}
