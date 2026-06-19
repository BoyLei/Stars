using Google.Protobuf.Collections;
using ProtoMsg;
using SGF.UI.Framework;
using StarProject.Service.LocalData;
using UnityEngine;
using XLua;

namespace StarProjectDef
{
    public class CommonBoxWidgetArgs
    {
        public int Id;
        public string Title;
        public string Content;
        public string BtnText;
        public System.Action<string> CallBack;
        public string Path;
        public int ItemID;
        public bool IsShowItemCount;
        public object[] Params;
        public int delay = 0;
    }

    public enum MsgBoxRootType
    {
        Default = 1,
        System = 2,
    }

    [LuaCallCSharp]
    public static class UIAPI
    {
        public static void ShowSystemMsgBox(string title, string content, string btnName1, System.Action action1,
            string btnName2, System.Action action2)
        {
            var go = GameObject.Instantiate(Resources.Load(UIDef.SystemMsgBox)) as GameObject;
            if (go != null)
            {
                var parent = GameObject.Find("Canvas");
                go.transform.SetParent(parent.transform);
                go.transform.localPosition = UnityEngine.Vector3.zero;
                go.transform.localScale = UnityEngine.Vector3.one;
                go.transform.localRotation = Quaternion.identity;
                var rect = go.GetComponent<RectTransform>();
                rect.anchorMin = UnityEngine.Vector2.zero;
                rect.anchorMax = UnityEngine.Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                var msgBox = go.GetComponent<SystemMsgBox>();
                if (msgBox != null)
                {
                    msgBox.Show(title, content, btnName1, action1, btnName2, action2);
                }
            }
        }

        public static void DoFade(int fadeType, bool isShowSkip, float time = 1.5f, System.Action cb = null, System.Action SkipCallBack = null)
        {
            PlayTimelineBlackWidgetArgs playTimelineBlackWidgetArgs = new();
            playTimelineBlackWidgetArgs.FadeType = fadeType;
            playTimelineBlackWidgetArgs.IsNeedSkip = isShowSkip;
            playTimelineBlackWidgetArgs.FadeTime = time;
            playTimelineBlackWidgetArgs.CallBack = cb;
            playTimelineBlackWidgetArgs.SkipCallBack = SkipCallBack;

            SGF.UI.Framework.UIManager.Instance.OpenWidgetAsync(UIDef.PlayTimelineBlackWidget, null, true,
                playTimelineBlackWidgetArgs, null, MainPageCommond.HideNone, true, true);
            //UIManager.Instance.OpenWidget(UIDef.PlayTimelineBlackWidget, false, playTimelineBlackWidgetArgs, null, MainPageCommond.WidgetHide, false, true);
        }


        public static void ShowMsgBox(string title, string content, string btnText,
            System.Action<string> CallBack = null, MsgBoxRootType boxType = MsgBoxRootType.Default)
        {
            CommonBoxWidgetArgs common = new();
            common.Id = 0;
            common.Title = title;
            common.Content = content;
            common.BtnText = btnText;
            common.CallBack = CallBack;
            common.Path = UIDef.UICommonMsgBox + 1;
            Transform rootTrans = null;
            if (boxType == MsgBoxRootType.System)
            {
                rootTrans = UIRoot.SystemWidgetRoot.transform;
            }

            //UIManager.Instance.OpenWidget(common.Path, false, common, null, MainPageCommond.WidgetHide, false, true);
            UIManager.Instance.OpenWidgetAsync(common.Path, null, false, common, rootTrans, MainPageCommond.WidgetHide,
                false, true);
        }

        public static void ShowMsgBox(int id, System.Action<string> CallBack, params object[] prames)
        {
            var config = LocalDataManager.Instance.GetWindowDataCell(id);
            if (config == null)
            {
                StarDebug.LogError($"读取弹窗配置失败 ID{id}");
                return;
            }

            if (config.GetWarn())
            {
                if (!StarProject.Service.Business.BusinessManager.Instance.HasNotice(config.GetId().ToString()))
                {
                    //勾选了今日不在提醒
                    CallBack.Invoke("SURE");
                    return;
                }
            }

            CommonBoxWidgetArgs common = new();
            common.Id = id;
            common.CallBack = CallBack;
            common.Path = UIDef.UICommonMsgBox + config.GetType();
            if (prames.Length > 0)
            {
                common.Params = new object[prames.Length];
                for (int i = 0; i < prames.Length; i++)
                {
                    common.Params[i] = prames[i];
                }
            }

            Transform rootTrans = null;
            if (config.RootType == (int)MsgBoxRootType.System)
            {
                rootTrans = UIRoot.SystemWidgetRoot.transform;
            }

            //UIManager.Instance.OpenWidget(common.Path, false, common, null, MainPageCommond.WidgetHide, false, true);
            UIManager.Instance.OpenWidgetAsync(common.Path, null, false, common, rootTrans, MainPageCommond.WidgetHide,
                false, true);
        }

        public static void ShowTeamMsgBox(int id,int delay ,System.Action<string> CallBack, params object[] prames)
        {
            var config = LocalDataManager.Instance.GetWindowDataCell(id);
            if (config == null)
            {
                StarDebug.LogError($"读取弹窗配置失败 ID{id}");
                return;
            }

            if (config.GetWarn())
            {
                if (!StarProject.Service.Business.BusinessManager.Instance.HasNotice(config.GetId().ToString()))
                {
                    //勾选了今日不在提醒
                    CallBack.Invoke("SURE");
                    return;
                }
            }

            CommonBoxWidgetArgs common = new();
            common.Id = id;
            common.CallBack = CallBack;
            common.Path = UIDef.UICommonMsgBox + config.GetType();
            common.delay = delay;
            if (prames.Length > 0)
            {
                common.Params = new object[prames.Length];
                for (int i = 0; i < prames.Length; i++)
                {
                    common.Params[i] = prames[i];
                }
            }
            Transform rootTrans = null;
            if (config.RootType == (int)MsgBoxRootType.System)
            {
                rootTrans = UIRoot.SystemWidgetRoot.transform;
            }

            //UIManager.Instance.OpenWidget(common.Path, false, common, null, MainPageCommond.WidgetHide, false, true);
            UIManager.Instance.OpenWidgetAsync(common.Path, null, false, common, rootTrans, MainPageCommond.WidgetHide,
                false, true);
        }

        public static void ShowMsgBox(int id, int itemID, System.Action<string> CallBack, params object[] prames)
        {
            var config = LocalDataManager.Instance.GetWindowDataCell(id);
            if (config == null)
            {
                StarDebug.LogError($"读取弹窗配置失败 ID{id}");
                return;
            }

            if (config.GetWarn())
            {
                if (!StarProject.Service.Business.BusinessManager.Instance.HasNotice(config.GetId().ToString()))
                {
                    //勾选了今日不在提醒
                    CallBack.Invoke("SURE");
                    return;
                }
            }

            CommonBoxWidgetArgs common = new();
            common.Id = id;
            common.CallBack = CallBack;
            common.Path = UIDef.UICommonMsgBox + config.GetType();
            if (prames.Length > 0)
            {
                common.Params = new object[prames.Length];
                for (int i = 0; i < prames.Length; i++)
                {
                    common.Params[i] = prames[i];
                }
            }

            common.ItemID = itemID;
            common.IsShowItemCount = config.GetIsShowItemCount();

            Transform rootTrans = null;
            if (config.RootType == (int)MsgBoxRootType.System)
            {
                rootTrans = UIRoot.SystemWidgetRoot.transform;
            }

            //UIManager.Instance.OpenWidget(common.Path, false, common, null, MainPageCommond.WidgetHide, false, true);
            UIManager.Instance.OpenWidgetAsync(common.Path, null, false, common, rootTrans, MainPageCommond.WidgetHide,
                false, true);
        }


        public static void CloseMsgBoxNew(int id)
        {
            var config = LocalDataManager.Instance.GetWindowDataCell(id);
            if (config == null)
            {
                StarDebug.LogError($"读取弹窗配置失败 ID{id}");
                return;
            }


            string Path = UIDef.UICommonMsgBox + config.GetType();
            Transform rootTrans = null;
            if (config.RootType == (int)MsgBoxRootType.System)
            {
                rootTrans = UIRoot.SystemWidgetRoot.transform;
            }

            UIManager.Instance.CloseWidget(Path, rootTrans);
        }

        public static CommonBoxWidget ShowMsgBoxNew(int id, System.Action<string> CallBack, params object[] prames)
        {
            var config = LocalDataManager.Instance.GetWindowDataCell(id);
            if (config == null)
            {
                StarDebug.LogError($"读取弹窗配置失败 ID{id}");
                return null;
            }

            if (config.GetWarn())
            {
                if (!StarProject.Service.Business.BusinessManager.Instance.HasNotice(config.GetId().ToString()))
                {
                    //勾选了今日不在提醒
                    CallBack.Invoke("SURE");
                    return null;
                }
            }

            CommonBoxWidgetArgs common = new();
            common.Id = id;
            common.CallBack = CallBack;
            common.Path = UIDef.UICommonMsgBox + config.GetType();
            if (prames.Length > 0)
            {
                common.Params = new object[prames.Length];
                for (int i = 0; i < prames.Length; i++)
                {
                    common.Params[i] = prames[i];
                }
            }

            Transform rootTrans = null;
            if (config.RootType == (int)MsgBoxRootType.System)
            {
                rootTrans = UIRoot.SystemWidgetRoot.transform;
            }

            return UIManager.Instance.OpenWidget(common.Path, false, common, rootTrans, MainPageCommond.WidgetHide,
                false, true) as CommonBoxWidget;
        }

        public static void ShowMsgBoxNewAsync(int id, System.Action<string> CallBack, params object[] prames)
        {
            var config = LocalDataManager.Instance.GetWindowDataCell(id);
            if (config == null)
            {
                StarDebug.LogError($"读取弹窗配置失败 ID{id}");
                return;
            }

            if (config.GetWarn())
            {
                if (!StarProject.Service.Business.BusinessManager.Instance.HasNotice(config.GetId().ToString()))
                {
                    //勾选了今日不在提醒
                    CallBack.Invoke("SURE");
                }
            }

            CommonBoxWidgetArgs common = new();
            common.Id = id;
            common.CallBack = CallBack;
            common.Path = UIDef.UICommonMsgBox + config.GetType();
            if (prames.Length > 0)
            {
                common.Params = new object[prames.Length];
                for (int i = 0; i < prames.Length; i++)
                {
                    common.Params[i] = prames[i];
                }
            }

            Transform rootTrans = null;
            if (config.RootType == (int)MsgBoxRootType.System)
            {
                rootTrans = UIRoot.SystemWidgetRoot.transform;
            }

            UIManager.Instance.OpenWidgetAsync(common.Path, null, false, common, rootTrans, MainPageCommond.WidgetHide,
                false, true);
        }


        /// <summary>
        /// 玩法介绍界面 动态读配置或者需要自己手动注册调用该方法
        /// </summary>
        /// <param id="id">Intro表中的Id</param>
        public static void ShowGameplayInfo(int id)
        {
            //UIManager.Instance.OpenWindow(UIDef.GamePlayInfoWindow, id, MainPageCommond.HideNone);
            UIManager.Instance.OpenWindowAsync(UIDef.GamePlayInfoWindow, null, id, MainPageCommond.HideNone);
        }

        /// <summary>
        /// 玩法介绍界面 理论上逻辑业务层不需要调用该接口，全部由window和widget自己内部逻辑内调用
        /// </summary>
        /// <param name="name">Intro表中的Name</param>
        public static void ShowGameplayInfo(string name)
        {
            //UIManager.Instance.OpenWindow(UIDef.GamePlayInfoWindow, name, MainPageCommond.HideNone);
            UIManager.Instance.OpenWindowAsync(UIDef.GamePlayInfoWindow, null, name, MainPageCommond.HideNone);
        }

        /// <summary>
        /// 通用结算
        /// </summary>
        /// <param name="IsSuccess">成功、失败</param>
        /// <param name="second">X秒后自动退出</param>
        /// <param name="items">奖励数据</param>
        /// <param name="callback">点击退出回调</param>
        /// <param name="arg">额外参数</param>
        public static void ShowUniversalSettlement(bool isSuccess, int second, RepeatedField<Dropinfo> items,
            RepeatedField<Dropinfo> exitems,
            System.Action callback, object arg = null)
        {
            UniversalSettlementInfo info = new()
            {
                IsSuccess = isSuccess,
                Second = second,
                Awards = items,
                ExAwards = exitems,
                CallBack = callback,
                Arg = arg
            };
            //UIManager.Instance.OpenWindow(UIDef.UniversalSettlementWindow, info, MainPageCommond.WidgetHide);
            UIManager.Instance.OpenWindowAsync(UIDef.UniversalSettlementWindow, null, info, MainPageCommond.WidgetHide);
        }

        public static void ShowDailySettlement(bool isSuccess, int second, RepeatedField<DropNotify> drops,
            System.Action callback, BaseData firstDrop = null, object arg = null)
        {
            RepeatedField<Dropinfo> normalItems = new();
            foreach (var drop in drops)
            {
                foreach (var item in drop.Infos)
                {
                    normalItems.Add(item);
                }
            }

            RepeatedField<Dropinfo> firstItems = new();
            if (firstDrop != null)
            {
                var items = firstDrop.Data;
                foreach (var item in items)
                {
                    var dropInfo = new Dropinfo();
                    dropInfo.Itemid = item.Key;
                    dropInfo.ItemNum = item.Value;
                    firstItems.Add(dropInfo);
                }
            }

            ShowUniversalSettlement(isSuccess, second, normalItems, firstItems, callback, arg);
        }

        /// <summary>
        /// 等待菊花
        /// </summary>
        /// <param name="outTime">超时时间单位毫秒</param>
        /// <param name="onOutAction">超时回调</param>
        public static void ShowRequestLoading(int outTime = 0, System.Action onOutAction = null,
            LoadingWidgetTypeEnum _loadingWidgetType = LoadingWidgetTypeEnum.Default)
        {
            RequestInfo info = new()
            { outTime = outTime, callBack = onOutAction, loadingWidgetType = _loadingWidgetType };
            //UIManager.Instance.OpenWidget(UIDef.RequestLoadingWidget, false, info, null, MainPageCommond.HideNone, false, true);
            UIManager.Instance.OpenWidgetAsync(UIDef.RequestLoadingWidget, null, false, info, null,
                MainPageCommond.HideNone, false, true);
        }

        /// <summary>
        /// 关闭菊花
        /// </summary>
        public static void CloseRequestLoading()
        {
            UIManager.Instance.CloseWidget(UIDef.RequestLoadingWidget, null, true);
        }

        /// <summary>
        /// 等待菊花
        /// </summary>
        /// <param name="outTime">超时时间单位毫秒</param>
        /// <param name="onOutAction">超时回调</param>
        public static void ShowRequestLoading2(int outTime, System.Action _onOutAction, System.Action _btnCb,
            string _tips = "", LoadingWidgetTypeEnum _loadingWidgetType = LoadingWidgetTypeEnum.AwaitResponse)
        {
            RequestInfo2 info = new()
            {
                outTime = outTime,
                onOutAction = _onOutAction,
                btnCb = _btnCb,
                tips = _tips,
                loadingWidgetType = _loadingWidgetType
            };
            UIManager.Instance.OpenWidgetAsync(UIDef.RequestLoadingWidget2, null, false, info, null,
                MainPageCommond.HideNone, false, true);
        }

        /// <summary>
        /// 关闭菊花
        /// </summary>
        public static void CloseRequestLoading2()
        {
            UIManager.Instance.CloseWidget(UIDef.RequestLoadingWidget2, null, true);
        }

        /// <summary>
        /// 关闭指定 类型的 菊花
        /// </summary>
        /// <param name="_loadingWidgetType"></param>
        public static bool CloseRequestLoading(LoadingWidgetTypeEnum _loadingWidgetType)
        {
            var loadingWidget = UIManager.Instance.GetUIPanel(UIDef.RequestLoadingWidget) as RequestLoadingWidget;
            if (loadingWidget == null)
            {
                return false;
            }

            // 如果节点已经关闭，那就不管
            if (!loadingWidget.isActiveAndEnabled)
            {
                return false;
            }

            // 类型相同的时候，关闭这个loading
            if (loadingWidget.LoadingWidgetType != _loadingWidgetType)
            {
                return false;
            }

            UIManager.Instance.CloseWidget(UIDef.RequestLoadingWidget, null, true);
            return true;
        }
    }

    [XLua.LuaCallCSharp]
    public class UniversalSettlementInfo
    {
        public bool IsSuccess;
        public int Second;
        public RepeatedField<Dropinfo> Awards;
        public RepeatedField<Dropinfo> ExAwards;
        public System.Action CallBack;
        public object Arg;
    }

    [XLua.LuaCallCSharp]
    public class RequestInfo
    {
        public int outTime;
        public System.Action callBack;

        public LoadingWidgetTypeEnum loadingWidgetType;
    }

    [XLua.LuaCallCSharp]
    public class RequestInfo2
    {
        public int outTime; // 超时时间 秒
        public System.Action onOutAction; // 超时执行的回调
        public System.Action btnCb; // 按钮的回调
        public string tips; // 显示的提示文本
        public LoadingWidgetTypeEnum loadingWidgetType; // loading类型
    }
}