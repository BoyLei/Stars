//来源表BattleFieldBUFFData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class BattleFieldBUFFData
	{
		public Dictionary<int, BattleFieldBUFFDataCell> StaticBattleFieldBUFFDatas = new Dictionary<int, BattleFieldBUFFDataCell>();
	}
	public class BattleFieldBUFFDataCell
	{
		//ID
		public string ID;
		private int _ID = -1;
		public int GetID()
		{
			if (_ID == -1 && int.TryParse(ID, out _ID))
			{
			}
			return _ID;
		}
		//BUFFID
		public string BUFFID;
		private int _BUFFID = -1;
		public int GetBUFFID()
		{
			if (_BUFFID == -1 && int.TryParse(BUFFID, out _BUFFID))
			{
			}
			return _BUFFID;
		}
		//备注
		public string Comment;
		//生成区域
		public string BUFFArea;
		private int _BUFFArea = -1;
		public int GetBUFFArea()
		{
			if (_BUFFArea == -1 && int.TryParse(BUFFArea, out _BUFFArea))
			{
			}
			return _BUFFArea;
		}
		//生成数量
		public string BUFFNum;
		private int _BUFFNum = -1;
		public int GetBUFFNum()
		{
			if (_BUFFNum == -1 && int.TryParse(BUFFNum, out _BUFFNum))
			{
			}
			return _BUFFNum;
		}
		//生成时间间隔
		public string BUFFRefreshCD;
		private int _BUFFRefreshCD = -1;
		public int GetBUFFRefreshCD()
		{
			if (_BUFFRefreshCD == -1 && int.TryParse(BUFFRefreshCD, out _BUFFRefreshCD))
			{
			}
			return _BUFFRefreshCD;
		}
		//BUFF表现
		public string BUFFAvatar;
		private int _BUFFAvatar = -1;
		public int GetBUFFAvatar()
		{
			if (_BUFFAvatar == -1 && int.TryParse(BUFFAvatar, out _BUFFAvatar))
			{
			}
			return _BUFFAvatar;
		}

	}
}
