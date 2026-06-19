using SGF.Module.Framework;
using SGF.Unity;
using StarProject;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using XLua;
using static StarProjectDef.ModuleDef;

namespace SGF.UI.Framework
{
    [LuaCallCSharp]
    public class UIQueueInfo
    {
        public UIQueueCfgData uIQueueCfgData = null;
        public WidgetInfo widgetInfo = null;
        public UIPanel uIPanel = null;
        public string specialUIKey = string.Empty;
        public Action openCB = null;

        public UIQueueInfo(UIQueueCfgData _uIQueueCfgData, WidgetInfo _widgetInfo)
        {
            uIQueueCfgData = _uIQueueCfgData;
            widgetInfo = _widgetInfo;
        }

        public UIQueueInfo(UIQueueCfgData _uIQueueCfgData, string _specialUIKey, Action _OpenCB)
        {
            uIQueueCfgData = _uIQueueCfgData;
            specialUIKey = _specialUIKey;
            openCB = _OpenCB;
        }
    }

    [LuaCallCSharp]
    public class UIQueueManager : ServiceModule<UIQueueManager>
    {
        public const string LOG_TAG = "UIQueueManager";

        private Dictionary<UISeatType, List<UIQueueInfo>> m_uiQueueDic = new();
        private Dictionary<UISeatType, string> m_SeatDesDic = new();

        public void Init()
        {
            CheckSingleton();

            m_uiQueueDic = new();
            m_uiQueueDic.Add(UISeatType.Top, new());
            m_uiQueueDic.Add(UISeatType.Down, new());
            m_uiQueueDic.Add(UISeatType.Left, new());
            m_uiQueueDic.Add(UISeatType.Right, new());
            m_uiQueueDic.Add(UISeatType.Full, new());

            m_SeatDesDic = new();
            m_SeatDesDic.Add(UISeatType.Top, "上");
            m_SeatDesDic.Add(UISeatType.Down, "下");
            m_SeatDesDic.Add(UISeatType.Left, "左");
            m_SeatDesDic.Add(UISeatType.Right, "右");
            m_SeatDesDic.Add(UISeatType.Full, "全屏");
        }

        private List<UIQueueInfo> GetUIQueueInfosBySeat(UISeatType uISeatType)
        {
            List<UIQueueInfo> uIQueueInfos = null;
            if (m_uiQueueDic.TryGetValue(uISeatType, out uIQueueInfos))
            {

            }
            return uIQueueInfos;
        }

        private bool GetIsFullUIPlaying()
        {
            bool isFullUIPlaying = false;
            List<UIQueueInfo> fullQueueInfos = GetUIQueueInfosBySeat(UISeatType.Full);
            if (fullQueueInfos != null)
            {
                isFullUIPlaying = fullQueueInfos.Count > 0;
            }
            return isFullUIPlaying;
        }

        public void AddUIQueue(UIQueueCfgData uIQueueCfgData, WidgetInfo widgetInfo)
        {
            List<UIQueueInfo> uIQueueInfos = GetUIQueueInfosBySeat(uIQueueCfgData.UISeatType);
            if (uIQueueInfos != null)
            {
                //if ("common/prefab/playtimelineblackwidget" == widgetInfo.name)
                //{
                //    SGF.Debuger.Log($"队列播放队列播放 UI配置 删除了timeline 队列111111");
                //}
                UIQueueInfo uIQueueInfo = new(uIQueueCfgData, widgetInfo);
                uIQueueInfos.Add(uIQueueInfo);
                if (uIQueueCfgData.UISeatType == UISeatType.Full)
                {
                    PauseCurQueueAndAddNull();
                }

                CheckUIQueue(uIQueueCfgData.UISeatType);
            }
        }

        public void AddUIQueuePanel(string path, UIPanel uIPanel)
        {
            SGF.Debuger.Log($"队列播放 添加界面 类型={uIPanel.UISeatType},name={uIPanel.Name}");
            List<UIQueueInfo> uIQueueInfos = GetUIQueueInfosBySeat(uIPanel.UISeatType);
            if (uIQueueInfos != null)
            {
                UIQueueInfo uIQueueInfo = null;
                foreach (var item in uIQueueInfos)
                {
                    if (item != null && item.widgetInfo != null && item.uIPanel == null && path == item.widgetInfo.name)
                    {
                        uIQueueInfo = item;
                        break;
                    }
                }
                if (uIQueueInfo != null)
                {
                    uIQueueInfo.uIPanel = uIPanel;
                    SGF.Debuger.LogWarning($"队列播放 添加界面 类型={uIPanel.UISeatType},name={uIPanel.Name}");
                    if (uIPanel.UISeatType != UISeatType.Full)
                    {
                        bool isFullUIPlaying = GetIsFullUIPlaying();
                        if (isFullUIPlaying)
                        {
                            uIQueueInfo.uIPanel.SetSelfCanvasGroup(false);
                        }
                    }
                }
            }
        }

        public void AddUIQueueGOB(GameObject gob)
        {
            if (gob == null)
            {
                SGF.Debuger.Log("UIQueueManager AddUIQueueGOB() gob=null,err!!!!");
                return;
            }
            LuaUIWidget uIPanel = gob.GetComponent<LuaUIWidget>();
            if (uIPanel == null)
            {
                SGF.Debuger.Log("UIQueueManager AddUIQueueGOB() uIPanel=null,err!!!!");
                return;
            }
            WidgetInfo widgetInfo = new();
            widgetInfo.name = uIPanel.Name.ToLower();
            widgetInfo.SetAsFirstSibling = false;
            widgetInfo.arg = null;
            widgetInfo.parent = null;
            widgetInfo._mainPageCommond = MainPageCommond.HideNone;
            widgetInfo.isInHudWhiteList = false;
            widgetInfo.isTopWidget = false;
            widgetInfo.isSceneWidget = false;
            widgetInfo.cb = null;
            UIQueueCfgData uIQueueCfgData = new(uIPanel.Name, uIPanel.UISeatType, (int)uIPanel.QueuePriorityType, uIPanel.IsLongTime);
            UIQueueInfo uIQueueInfo = new(uIQueueCfgData, widgetInfo);
            uIQueueInfo.uIPanel = uIPanel;

            List<UIQueueInfo> uIQueueInfos = GetUIQueueInfosBySeat(uIPanel.UISeatType);
            if (uIQueueInfos != null)
            {
                uIQueueInfos.Add(uIQueueInfo);
                CheckUIQueue(uIPanel.UISeatType);
            }
        }

        public void DelUIQueueGOB(GameObject gob)
        {
            if (gob == null)
            {
                SGF.Debuger.Log("UIQueueManager DelUIQueueGOB() gob=null,err!!!!");
                return;
            }
            LuaUIWidget uIPanel = gob.GetComponent<LuaUIWidget>();
            if (uIPanel == null)
            {
                SGF.Debuger.Log("UIQueueManager DelUIQueueGOB() uIPanel=null,err!!!!");
                return;
            }
            bool isDelSuccess = false;
            SGF.Debuger.Log($"队列播放 删除队列 类型={uIPanel.UISeatType},name={uIPanel.Name}");

            List<UIQueueInfo> uIQueueInfos = GetUIQueueInfosBySeat(uIPanel.UISeatType);
            if (uIQueueInfos != null)
            {
                isDelSuccess = DelUIQueueCache(uIPanel, uIQueueInfos);
                if (isDelSuccess)
                {
                    if (uIPanel.UISeatType == UISeatType.Full)
                    {
                        bool isFullUIPlaying = CheckOpen(uIPanel.UISeatType, uIQueueInfos, true);
                        if (!isFullUIPlaying)
                        {
                            ContinueAllQueue();
                        }
                    }
                    else
                    {
                        CheckOpen(uIPanel.UISeatType, uIQueueInfos, true);
                    }
                }
            }
            if (isDelSuccess)
            {
                SGF.Debuger.LogWarning($"队列播放 删除队列 类型={uIPanel.UISeatType},name={uIPanel.Name}");
            }
        }

        [XLua.BlackList]
        public void DelUIQueue(UIPanel uIPanel)
        {
            if (uIPanel == null)
            {
                SGF.Debuger.Log("UIQueueManager DelUIQueue() uIPanel=null,err!!!!");
                return;
            }
            bool isDelSuccess = false;
            SGF.Debuger.Log($"队列播放 删除队列 类型={uIPanel.UISeatType},name={uIPanel.Name}");

            List<UIQueueInfo> uIQueueInfos = GetUIQueueInfosBySeat(uIPanel.UISeatType);
            if (uIQueueInfos != null)
            {
                isDelSuccess = DelUIQueueCache(uIPanel, uIQueueInfos);
                if (isDelSuccess)
                {
                    if (uIPanel.UISeatType == UISeatType.Full)
                    {
                        bool isFullUIPlaying = CheckOpen(uIPanel.UISeatType, uIQueueInfos, true);
                        if (!isFullUIPlaying)
                        {
                            ContinueAllQueue();
                        }
                    }
                    else
                    {
                        CheckOpen(uIPanel.UISeatType, uIQueueInfos, true);
                    }
                }
            }
            if (isDelSuccess)
            {
                SGF.Debuger.LogWarning($"队列播放 删除队列 类型={uIPanel.UISeatType},name={uIPanel.Name}");
            }
        }

        private bool DelUIQueueCache(UIPanel uIPanel, List<UIQueueInfo> list)
        {
            int index = -1;
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (item != null && item.uIPanel != null && item.uIPanel.name == uIPanel.name)
                {
                    index = i;
                    break;
                }
            }
            if (index != -1)
            {
                list.RemoveAt(index);
            }
            return index != -1;
        }

        public void AddSpecialUIQueue(string key, int _queuePriority, UISeatType UISeatType, Action openCB)
        {
            List<UIQueueInfo> uIQueueInfos = GetUIQueueInfosBySeat(UISeatType);
            if (uIQueueInfos != null)
            {
                SGF.Debuger.Log($"队列播放 添加特殊队列 key={key},queuePriority={_queuePriority}");

                UIQueueCfgData uIQueueCfgData = new(key, UISeatType, _queuePriority, false);
                UIQueueInfo uIQueueInfo = new(uIQueueCfgData, key, openCB);
                uIQueueInfos.Add(uIQueueInfo);
                if (UISeatType == UISeatType.Full)
                {
                    PauseCurQueueAndAddNull();
                }
                CheckUIQueue(UISeatType);
            }
        }

        public void DelSpecialUIQueue(string key, UISeatType UISeatType)
        {
            bool isDelSuccess = false;
            SGF.Debuger.Log($"队列播放 删除队列特殊 类型={UISeatType},name={key}");

            List<UIQueueInfo> uIQueueInfos = GetUIQueueInfosBySeat(UISeatType);
            if (uIQueueInfos != null)
            {
                isDelSuccess = DelUIQueueCache(key, uIQueueInfos);
                if (isDelSuccess)
                {
                    if (UISeatType == UISeatType.Full)
                    {
                        bool isFullUIPlaying = CheckOpen(UISeatType, uIQueueInfos, true);
                        if (!isFullUIPlaying)
                        {
                            ContinueAllQueue();
                        }
                    }
                    else
                    {
                        CheckOpen(UISeatType, uIQueueInfos, true);
                    }
                }
            }
            if (isDelSuccess)
            {
                SGF.Debuger.LogWarning($"队列播放 删除队列特殊 类型={UISeatType},name={key}");
            }
        }

        public void DelAllSpecialUIQueue(string key, UISeatType UISeatType)
        {
            bool isDelSuccess = false;
            SGF.Debuger.Log($"队列播放 删除队列 类型={UISeatType},name={key}");

            List<UIQueueInfo> uIQueueInfos = GetUIQueueInfosBySeat(UISeatType);
            if (uIQueueInfos != null)
            {
                isDelSuccess = DelAllKeyUIQueueCache(key, uIQueueInfos);
                if (isDelSuccess)
                {
                    if (UISeatType == UISeatType.Full)
                    {
                        bool isFullUIPlaying = CheckOpen(UISeatType, uIQueueInfos, true);
                        if (!isFullUIPlaying)
                        {
                            ContinueAllQueue();
                        }
                    }
                    else
                    {
                        CheckOpen(UISeatType, uIQueueInfos, true);
                    }
                }
            }
            if (isDelSuccess)
            {
                SGF.Debuger.LogWarning($"队列播放 删除队列 类型={UISeatType},name={key}");
            }
        }

        private bool DelUIQueueCache(string key, List<UIQueueInfo> list)
        {
            int index = -1;
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (item != null && item.specialUIKey == key)
                {
                    index = i;
                    break;
                }
            }
            if (index != -1)
            {
                list.RemoveAt(index);
            }
            return index != -1;
        }

        private bool DelAllKeyUIQueueCache(string key, List<UIQueueInfo> list)
        {
            bool isDelSuccess = false;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                var item = list[i];
                if (item != null && item.specialUIKey != string.Empty && item.specialUIKey.Contains(key))
                {
                    list.RemoveAt(i);
                }
            }
            return isDelSuccess;
        }


        private void CheckUIQueue(UISeatType uiSeatType)
        {
            bool isPlaySucc = false;
            SGF.Debuger.Log($"队列播放 分类检查 uiSeatType={uiSeatType}");
            List<UIQueueInfo> uIQueueInfos = GetUIQueueInfosBySeat(uiSeatType);
            if (uIQueueInfos != null)
            {
                isPlaySucc = CheckOpen(uiSeatType, uIQueueInfos, false);
            }
            SGF.Debuger.LogWarning($"队列播放 分类检查 uiSeatType={uiSeatType},isPlaySucc={isPlaySucc}");
        }

        private bool CheckOpen(UISeatType uiSeatType, List<UIQueueInfo> list, bool isAnginStart = false)
        {
            bool isFullUIPlaying = false;
            if (uiSeatType != UISeatType.Full)
            {
                isFullUIPlaying = GetIsFullUIPlaying();
            }
            bool res = false;
            if (!isFullUIPlaying)
            {
                if (list.Count > 0)
                {
                    UIQueueInfo first = list[0];
                    if (isAnginStart)
                    {
                        if (first != null)
                        {
                            if (first.uIPanel != null)
                            {
                                first.uIPanel.SetSelfCanvasGroup(true);
                                if (first.uIPanel.gameObject.activeSelf)
                                {
                                    SGF.Debuger.LogWarning($"队列播放 检查 类型={uiSeatType},播放={first.uIPanel.Name},打开第一个");
                                    res = true;
                                }
                                else
                                {
                                    SGF.Debuger.LogWarning($"队列播放 检查打开 类型={uiSeatType},播放={first.uIQueueCfgData.UIPath}");
                                    OpenQueueWidget(first);
                                }
                            }
                            else
                            {
                                if (first.specialUIKey == string.Empty)
                                {
                                    SGF.Debuger.LogWarning($"队列播放 重新开始检查 类型={uiSeatType},播放={first.uIQueueCfgData.UIPath}");
                                    OpenQueueWidget(first);
                                    res = true;
                                }
                                else
                                {
                                    first.openCB?.Invoke();
                                    res = true;
                                }
                            }
                        }
                        else
                        {
                            SGF.Debuger.LogWarning($"队列播放 检查 类型={uiSeatType},数量={list.Count},删除第一个空,继续检查");
                            list.RemoveAt(0);
                            CheckOpen(uiSeatType, list, isAnginStart);
                        }
                    }
                    else
                    {
                        if (first == null)
                        {
                            SGF.Debuger.LogWarning($"队列播放 检查 类型={uiSeatType},数量={list.Count},删除第一个空,重新检查");
                            list.RemoveAt(0);
                            CheckOpen(uiSeatType, list, isAnginStart);
                        }
                        else
                        {
                            if (list.Count > 1)
                            {
                                UIQueueInfo last = list[list.Count - 1];
                                if (first != null && last != null && first.uIQueueCfgData.UIPath != last.uIQueueCfgData.UIPath)
                                {
                                    int firstQueuePriority = first.uIQueueCfgData.QueuePriority;
                                    int lastQueuePriority = last.uIQueueCfgData.QueuePriority;
                                    if (lastQueuePriority > firstQueuePriority)
                                    {
                                        SGF.Debuger.LogWarning($"队列播放 优先级检查 类型={uiSeatType},播放={first.uIQueueCfgData.UIPath}");
                                        if (last.uIPanel == null)
                                        {
                                            if (last.specialUIKey == string.Empty)
                                            {
                                                OpenQueueWidget(last);
                                            }
                                            else
                                            {
                                                last.openCB?.Invoke();
                                            }
                                        }
                                        else
                                        {
                                            SGF.Debuger.LogWarning($"队列播放 优先级检查 类型={uiSeatType},播放={first.uIQueueCfgData.UIPath},已经有界面,只修改下标");
                                        }
                                        if (first.uIPanel != null)
                                        {
                                            first.uIPanel.SetSelfCanvasGroup(false);
                                        }
                                        list.RemoveAt(list.Count - 1);
                                        list.Insert(0, last);
                                        res = true;
                                    }
                                    else
                                    {
                                        SGF.Debuger.Log($"队列播放 检查 类型={uiSeatType},last={last.uIQueueCfgData.UIPath},lastQueuePriority={lastQueuePriority},first={first.uIQueueCfgData.UIPath},firstQueuePriority={firstQueuePriority},无法播放优先级不足");
                                    }
                                }
                                else
                                {
                                    OpenQueueWidget(last);
                                    //list.RemoveAt(0);
                                    list.RemoveAt(list.Count - 1);

                                    SGF.Debuger.Log($"队列播放 检查 类型={uiSeatType},last={last.uIQueueCfgData.UIPath},用后面顶掉前面的，删除第一个");
                                }
                            }
                            else
                            {
                                if (first.uIPanel == null)
                                {
                                    SGF.Debuger.LogWarning($"队列播放 检查 类型={uiSeatType},播放={first.uIQueueCfgData.UIPath}");
                                    if (first.specialUIKey == string.Empty)
                                    {
                                        OpenQueueWidget(first);
                                    }
                                    else
                                    {
                                        first.openCB?.Invoke();
                                    }
                                    res = true;
                                }
                                else
                                {
                                    SGF.Debuger.LogWarning($"队列播放 检查 类型={uiSeatType},播放={first.uIQueueCfgData.UIPath},只记录,打开透明度");
                                    first.uIPanel.SetSelfCanvasGroup(true);
                                }
                            }
                        }
                    }
                }
                else
                {
                    SGF.Debuger.Log($"队列播放 检查 类型={uiSeatType},数量={list.Count},队列里没东西了");
                }
            }
            else
            {
                SGF.Debuger.Log($"队列播放 检查 类型={uiSeatType},无法播放,正在有全屏播放中");
            }
            return res;
        }

        private void OpenQueueWidget(UIQueueInfo uIQueueInfo)
        {
            if (uIQueueInfo == null || uIQueueInfo.widgetInfo == null)
            {
                return;
            }
            if (uIQueueInfo.uIQueueCfgData != null)
            {
                UIQueuePriorityType QueuePriority = (UIQueuePriorityType)uIQueueInfo.uIQueueCfgData.QueuePriority;
                if (uIQueueInfo.uIQueueCfgData.UISeatType == UISeatType.Full && (QueuePriority == UIQueuePriorityType.Full || QueuePriority == UIQueuePriorityType.BlockingPlot))
                {
                    UIManager.Instance.CloseAllLoadedNoUIQueueType();
                }
            }
            UIManager.Instance.OpenQueueWidget(
                uIQueueInfo.widgetInfo.name,
                uIQueueInfo.widgetInfo.cb,
                uIQueueInfo.widgetInfo.SetAsFirstSibling,
                uIQueueInfo.widgetInfo.arg,
                uIQueueInfo.widgetInfo.parent,
                uIQueueInfo.widgetInfo._mainPageCommond,
                uIQueueInfo.widgetInfo.isInHudWhiteList,
                uIQueueInfo.widgetInfo.isTopWidget,
                uIQueueInfo.widgetInfo.isSceneWidget
                );
        }

        private void PauseCurQueueAndAddNull()
        {
            foreach (var item in m_uiQueueDic)
            {
                if (item.Key != UISeatType.Full)
                {
                    if (item.Value.Count > 0)
                    {
                        UIQueueInfo first = item.Value[0];
                        if (first != null)
                        {
                            if (first.uIPanel != null)
                            {
                                first.uIPanel.SetSelfCanvasGroup(false);
                            }
                            item.Value.Insert(0, null);
                        }
                    }
                }
            }
        }

        private void ContinueAllQueue()
        {
            foreach (var item in m_uiQueueDic)
            {
                if (item.Key != UISeatType.Full)
                {
                    CheckOpen(item.Key, item.Value, true);
                }
            }
        }

        public void GMLogUIQueue()
        {
            foreach (var item in m_uiQueueDic)
            {
                string des = m_SeatDesDic[item.Key];
                SGF.Debuger.Log($"------------队列【{des}】-----------总数{item.Value.Count}-------------");
                foreach (var item2 in item.Value)
                {
                    if (item2 != null)
                    {
                        SGF.Debuger.Log($"队列【{des}】---path={item2.uIQueueCfgData.UIPath},uipanel={item2.uIPanel}");
                    }
                    else
                    {
                        SGF.Debuger.Log($"队列【{des}】---item=null");
                    }
                }
            }
        }

        public void ClearUIQueueCache()
        {
            foreach (var item in m_uiQueueDic)
            {
                item.Value.Clear();
            }
        }

        public override void Release()
        {
            ClearUIQueueCache();
            base.Release();

        }
    }
}
