//来源表公会捐献_GuildDonate.xlsm.xlsx -> sheet:DonateItemSet
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class DonateItemSetData
	{
		[Key(0)]
		public Dictionary<int, DonateItemSetDataCell> StaticDonateItemSetDatas = new Dictionary<int, DonateItemSetDataCell>();
	}
	[MessagePackObject]
	public class DonateItemSetDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//itemtype
		[Key(1)]
		public List<int> ItemType = new List<int>();
		//subtype
		[Key(2)]
		public List<int> SubType = new List<int>();
		//quality
		[Key(3)]
		public List<int> Quality = new List<int>();
		//jobtype
		[Key(4)]
		public List<int> JobType = new List<int>();
		//装备等级段
		[Key(5)]
		public List<int> Level = new List<int>();
		//护符等级
		[Key(6)]
		public List<int> EquipLevel = new List<int>();
		//显示名
		[Key(7)]
		public string ShowName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_ShowName); }
			set { _ShowName = value; }
        }
		[IgnoreMember]
		private string _ShowName;
		//显示icon
		[Key(8)]
		public string Icon;
		//显示品质
		[Key(9)]
		public int ShowQuality;
		public int GetShowQuality()
		{
			return ShowQuality;
		}
	}
}
