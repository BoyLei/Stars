//来源表公会捐献_GuildDonate.xlsm.xlsx -> sheet:DonateOrderList
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class DonateOrderListData
	{
		[Key(0)]
		public Dictionary<int, DonateOrderListDataCell> StaticDonateOrderListDatas = new Dictionary<int, DonateOrderListDataCell>();
	}
	[MessagePackObject]
	public class DonateOrderListDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//难度组
		[Key(1)]
		public int DiffcultyGroup;
		public int GetDiffcultyGroup()
		{
			return DiffcultyGroup;
		}
		//需求道具
		[Key(2)]
		public long Item;
		public long GetItem()
		{
			return Item;
		}
		//需求道具集
		[Key(3)]
		public int ItemSet;
		public int GetItemSet()
		{
			return ItemSet;
		}
		//需求数量
		[Key(4)]
		public long Num;
		public long GetNum()
		{
			return Num;
		}
		//权重
		[Key(5)]
		public long Weight;
		public long GetWeight()
		{
			return Weight;
		}
		//冒险等级下限
		[Key(6)]
		public int AdvantureLvLimit;
		public int GetAdvantureLvLimit()
		{
			return AdvantureLvLimit;
		}
		//委托人名字
		[Key(7)]
		public string NPCname
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_NPCname); }
			set { _NPCname = value; }
        }
		[IgnoreMember]
		private string _NPCname;
		//委托人形象
		[Key(8)]
		public string Spine_id;
		//势力名字
		[Key(9)]
		public string ForceName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_ForceName); }
			set { _ForceName = value; }
        }
		[IgnoreMember]
		private string _ForceName;
		//势力底纹
		[Key(10)]
		public string ForceIcon;
		//委托文本
		[Key(11)]
		public string Text
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Text); }
			set { _Text = value; }
        }
		[IgnoreMember]
		private string _Text;
		//角色等级下限
		[Key(12)]
		public int LvLimit;
		public int GetLvLimit()
		{
			return LvLimit;
		}
	}
}
