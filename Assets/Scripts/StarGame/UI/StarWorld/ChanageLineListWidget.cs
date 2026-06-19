using Google.Protobuf.Collections;
using ProtoMsg;
using SGF.Module.Framework;
using SGF.UI.Framework;
using Sirenix.Utilities;
using StarProject.Game;
using StarProject.Module;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarProject.UI.StarWorld
{
    /// <summary>
    /// TODO：
    /// 1，Tab之后做成通用组件。
    /// 2，外部脚本需要内部Window来实现，或通用，需识别其价值。
    /// 3，ScrollView需拓展，如各Tab功能相同TabContext需复用，ScrollViewEx需添加池化数据即Cell的复用。
    /// 4，可滑动窗口背景必须有不可滑动的部分，不然穿透，就需要处理很多按钮的重复点击逻辑。
    /// </summary>
	public class ChanageLineListWidget : UIWidget
    {
        private string LOG_TAG = "[ChanageLineListWidget]";

        private Button CloseBtn;
        private SuperScrollViewController ScrollView;    // 无限列表
        private Transform Content;
        private Transform ItemPrefab;

        private MapLinesRet mapLinesRetMsg;
        private RepeatedField<ServerMapLoadInfo> ServerLines;
        private List<ItemMapLine> ItemList = new();

        protected override void Awake()
        {
            CloseBtn = transform.Find("CloseBtn").GetComponent<Button>();
            CloseBtn.onClick.AddListener(OnClickCloseBtn);
            ScrollView = transform.Find("ScrollView").GetComponent<SuperScrollViewController>();
            ItemPrefab = transform.Find("ItemLine").transform;
            Content = transform.Find("ScrollView/Mask/List").transform;
            ScrollView.NewScroll(ItemPrefab.gameObject, 0, OnRefesh);
        }

        protected override void OnOpen(object arg)
        {
            base.OnOpen(arg);
            SetLines((MapLinesRet)arg);
        }

        protected override void OnClose(object arg = null)
        {
            base.OnClose(arg);
        }

        public void SetPos(UnityEngine.Vector3 worldPos)
        {
            transform.position = worldPos;
        }

        private void SetLines(MapLinesRet ret)
        {
            mapLinesRetMsg = ret;
            if (mapLinesRetMsg == null)
            {
                OnClickCloseBtn();
                return;
            }
            ServerLines = mapLinesRetMsg.ServerLines;
            ServerLines.Sort((a, b) =>
            {
                return a.LineID - b.LineID;
            });

            ScrollView.ContinueScroll(ServerLines.Count, false);
        }

        private void OnRefesh(Transform tr, int id)
        {
            ServerMapLoadInfo serverMapLoadInfo = ServerLines[id];
            for (int i = 0; i < ItemList.Count; i++)
            {
                ItemMapLine child = ItemList[i];
                if (child.transform == tr)
                {
                    child.SetLineItem(serverMapLoadInfo);
                    return;
                }
            }
            // 创建
            //Transform gob = Instantiate(ItemPrefab);
            //gob.transform.SetParent(Content);
            ItemMapLine item = tr.GetComponent<ItemMapLine>();
            item.InitClickCB(ChanageLine);
            item.SetLineItem(serverMapLoadInfo);
            ItemList.Add(item);
        }

        private void ChanageLine(ulong line, ulong spaceID)
        {
            ulong CurServerID = GameManager.Instance.GetCurServerID();
            ulong SpaceID = GameManager.Instance.GetSpaceID();
            if (line == CurServerID && spaceID == SpaceID)
            {
                SGF.Debuger.LogWarning($"重复选择分线 line={line}");
                OnClickCloseBtn();
                return;
            }
            StarWorldModule starWorld = (StarWorldModule)ModuleManager.Instance.GetModule(ModuleDef.Name.StarWorldModule);
            // 根据ID判断是那个副本类型
            int mmpID = GameManager.Instance.GetCurMapId();
            SpaceType spaceType = GameManager.Instance.GetCurMapType();
            starWorld.SendFBChangeReq(ChangeReason.ChangeLine, mmpID, spaceType, line, spaceID);
            //OnClickCloseBtn();
        }

        private void OnClickCloseBtn()
        {
            UIManager.Instance.CloseWidget(UIDef.ChanageLineListWidget, null, true);
        }

    }
}
