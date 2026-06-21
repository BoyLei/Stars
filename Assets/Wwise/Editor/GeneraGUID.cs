///--------------------------------------------------------------------
/// 文件名   :   GeneraGUID.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/02/09 10:21:51
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public static class GeneraGUID
{
	[MenuItem("Tools/Generate WwiseAsset")]
    public static void Generate()
    {
		AkWwiseTreeView treeView = new AkWwiseTreeView();
		treeView.AssignDefaults();
		treeView.SetRootItem(System.IO.Path.GetFileNameWithoutExtension(AkWwiseEditorSettings.Instance.WwiseProjectPath), WwiseObjectType.Project);
		treeView.PopulateItem(treeView.RootItem, "Events", AkWwiseProjectInfo.GetData().EventWwu);
		treeView.PopulateItem(treeView.RootItem, "Switches", AkWwiseProjectInfo.GetData().SwitchWwu);
		treeView.PopulateItem(treeView.RootItem, "States", AkWwiseProjectInfo.GetData().StateWwu);
		treeView.PopulateItem(treeView.RootItem, "SoundBanks", AkWwiseProjectInfo.GetData().BankWwu);
		treeView.PopulateItem(treeView.RootItem, "Auxiliary Busses", AkWwiseProjectInfo.GetData().AuxBusWwu);
		treeView.PopulateItem(treeView.RootItem, "Virtual Acoustics", AkWwiseProjectInfo.GetData().AcousticTextureWwu);

		foreach (var item in treeView.RootItem.Items)
        {
			CreateItem(item);
		}
	}

	public static List<WwiseObjectType> CreateTypes = new List<WwiseObjectType>() { 
		
		WwiseObjectType.Event,
		WwiseObjectType.State,
		WwiseObjectType.Soundbank
	};


	private static void CreateItem(AK.Wwise.TreeView.TreeViewItem in_item)
    {
		SetGuid(in_item);
		if (in_item.Items.Count > 0)
		{
			foreach (var item in in_item.Items)
			{
				CreateItem(item);
			}
		}
	}

	private static  void SetGuid(AK.Wwise.TreeView.TreeViewItem in_item)
	{
		if(in_item==null || in_item.DataContext==null)
        {
			return;
        }
		var type = (in_item.DataContext as AkWwiseTreeView.AkTreeInfo).ObjectType;
		if(!CreateTypes.Contains(type))
        {
			return;
        }
		var obj = in_item.DataContext as AkWwiseTreeView.AkTreeInfo;

		var reference = WwiseObjectReference.FindOrCreateWwiseObject((in_item.DataContext as AkWwiseTreeView.AkTreeInfo).ObjectType, in_item.Header, obj.Guid);
		var groupReference = reference as WwiseGroupValueObjectReference;
		if (groupReference)
		{
			obj = in_item.Parent.DataContext as AkWwiseTreeView.AkTreeInfo;
			groupReference.SetupGroupObjectReference(in_item.Parent.Header, obj.Guid);
		}
	}

}
