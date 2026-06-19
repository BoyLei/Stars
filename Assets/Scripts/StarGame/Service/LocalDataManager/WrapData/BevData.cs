using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class BevData
	{
		public Dictionary<int, BevDataCell> StaticBevDatas = new Dictionary<int, BevDataCell>();
	}
	public class BevDataCell
	{		//编号
		public string id;
		private int _id = -1;
		public int Getid()
		{
			if (_id == -1 && int.TryParse(id, out _id))
			{
			}
			return _id;
		}
		//事件行为树
		public List<string> EventBev = new List<string>();
		//状态位为树
		public List<string> StateBev = new List<string>();
		//初始状态
		public string InitBev;

	}
}
