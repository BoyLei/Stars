//来源表EtchUPMatData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class EtchUPMatData
	{
		public Dictionary<int, EtchUPMatDataCell> StaticEtchUPMatDatas = new Dictionary<int, EtchUPMatDataCell>();
	}
	public class EtchUPMatDataCell
	{
		//鸣器ID
		public string EtchID;
		private int _EtchID = -1;
		public int GetEtchID()
		{
			if (_EtchID == -1 && int.TryParse(EtchID, out _EtchID))
			{
			}
			return _EtchID;
		}
		//强化道具ID
		public List<int> ItemID = new List<int>();

	}
}
