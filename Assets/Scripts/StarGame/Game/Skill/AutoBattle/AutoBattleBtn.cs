using StarProject;
using StarProject.Service.Battle;
using StarProject.Service.Language;
using StarProjectDef;
using UnityEngine;
using UnityEngine.UI;

public class AutoBattleBtn : MonoBehaviour
{
    public Text text;

    public GameObject NoneActiveGo;
    public GameObject ActiveGo;
    public GameObject ForbiddenGo;

    public Transform activeRoteNode;

    public int rotationSpeed = 60;


    void Start()
    {
        RefreshIsOpen();

        GlobalEvent.AutoBattleEvent.AddListener(OnAutoBattleEvent);

        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(OnClick);
        }
    }

    void OnEnable()
    {
        // 如果没有缓存, 那就不刷新自动战斗的 缓存状态
        if (!LocalCacheManager.Instance.Has(BattleManager.AUTOBATTLE_OPEN))
        {
            // SGF.Debuger.Log($"[AutoSkill] , 没有缓存");

            return;
        }
        bool isOpen = (bool)LocalCacheManager.Instance.Get(BattleManager.AUTOBATTLE_OPEN);
        // SGF.Debuger.Log($"[AutoSkill] , 缓存 isOpen: {isOpen}");

        if (isOpen)
        {
            BattleManager.Instance.Start();
        }
        else
        {
            BattleManager.Instance.Stop();
        }
    }

    public void OnAutoBattleEvent(string eventType, object v)
    {
        if (eventType == BattleManager.AUTOBATTLEKEY.ToString())
        {
            RefreshIsOpen();
        }
    }

    public void RefreshIsOpen()
    {
        bool isActive = BattleManager.Instance.IsAutoBattling;
        bool isForbid = BattleManager.Instance.IsForbid;

        this.NoneActiveGo.SetActive(!isActive && !isForbid);
        this.ActiveGo.SetActive(isActive);
        this.ForbiddenGo.SetActive(isForbid);

        if (isActive)
        {
            //text.text = GameConfig.LocalStr["Open"];
            text.text = LanguageManager.Instance.GetLanguageByKey("Open");
        }
        else
        {
            if (isForbid)
            {
                //text.text = GameConfig.LocalStr["Stop"];
                text.text = LanguageManager.Instance.GetLanguageByKey("Stop");
            }
            else
            {
                //text.text = GameConfig.LocalStr["Close"];
                text.text = LanguageManager.Instance.GetLanguageByKey("Close");
            }
        }
    }

    public void Update()
    {
        if (!this.ActiveGo.activeInHierarchy)
        {
            return;
        }
        // 计算每帧需要旋转的角度
        float rotationAngle = rotationSpeed * Time.deltaTime;

        // 绕节点的y轴旋转
        activeRoteNode.Rotate(Vector3.forward, rotationAngle);
    }

    public void OnClick()
    {
        BattleManager.Instance.SwitchAutoBattle();

        // 如果是 关闭自动战斗, 那就啥都不提示
        if (!BattleManager.Instance.IsOpenAutoBattle)
        {
            return;
        }

        if (BattleManager.Instance.IsForbid)
        {
            return;
        }

        /// 如果伙伴自动战斗开启了
        /// 那就提示：伙伴自动战斗已开启
        /// 如果伙伴自动战斗没开
        /// 就提示：伙伴自动战斗未开启，前往设置界面中可手动开启
        if (!BattleManager.Instance.GetPartnerAutoBattleCache())
        {
            Frame.Util.ShowMessageByCode(70);
        }
        else
        {

        }
    }

}
