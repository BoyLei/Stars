//来源表Model.xlsx -> sheet:Model
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ModelData
	{
		[Key(0)]
		public Dictionary<int, ModelDataCell> StaticModelDatas = new Dictionary<int, ModelDataCell>();
	}
	[MessagePackObject]
	public class ModelDataCell
	{
		//模型ID
		[Key(0)]
		public int ModleID;
		public int GetModleID()
		{
			return ModleID;
		}
		//策划备注
		[Key(1)]
		public string Desc;
		//模型路径
		[Key(2)]
		public string ModelsPath;
	}
}
