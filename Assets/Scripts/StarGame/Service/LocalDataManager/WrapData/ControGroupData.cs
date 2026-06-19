//来源表关卡控制器_Controller.xlsx -> sheet:ControGroup
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ControGroupData
	{
		[Key(0)]
		public Dictionary<int, ControGroupDataCell> StaticControGroupDatas = new Dictionary<int, ControGroupDataCell>();
	}
	[MessagePackObject]
	public class ControGroupDataCell
	{
		//控制器组id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//开启时间
		[Key(1)]
		public List<int> OpenTime = new List<int>();
		//开启控制器1
		[Key(2)]
		public List<int> OpenContro = new List<int>();
		//关闭时间
		[Key(3)]
		public List<int> CloseTime = new List<int>();
		//关闭控制器1
		[Key(4)]
		public List<int> CloseContro = new List<int>();
	}
}
