#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using ProtoMsg;
using SGF.Module.Framework;
using SGF.Network;
using StarProject.Game;
using StarProject.Game.Player;
using StarProject.Module;
using StarProject.Module.StarWordGame;
using StarProjectDef;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SkillEditorGM : MonoBehaviour
{
    static SkillEditorGM _singleton;
    public static SkillEditorGM singleton
    {
        get
        {
            return _singleton;
        }
    }

    [SerializeField] Button genMonsterBtn;
    [SerializeField] Button genAreaMonsterBtn;
    [SerializeField] Button killAllMonsterBtn;
    [SerializeField] Button changeLevelBtn;
    [SerializeField] Button fillSkillBtn;
    [SerializeField] Button changeModelBtn;
    [SerializeField] Button useSkillBtn;

    [SerializeField] Toggle godModeTog;
    [SerializeField] Toggle noWaitTog;

    [SerializeField] InputField areaMonsterNumInput_I;
    [SerializeField] InputField areaMonsterNumInput_J;
    [SerializeField] InputField levelInput;
    [SerializeField] InputField roleIDInput;
    [SerializeField] InputField fillSkillInput;
    [SerializeField] InputField useSkillInput;

    [SerializeField] Transform skillRoot;

    private string skillCfgPath_srv = "";       //服务器配置路径

    public static int g_modelID = -1;       //职业id，切换模型用

    void Awake()
    {
        _singleton = this;

        genMonsterBtn.onClick.AddListener(onClick_GenMonsterBtn);
        genAreaMonsterBtn.onClick.AddListener(onClick_GenAreaMonsterBtn);
        killAllMonsterBtn.onClick.AddListener(onClick_KillAllMonsterBtn);
        changeLevelBtn.onClick.AddListener(onClick_ChangeLevelBtn);
        fillSkillBtn.onClick.AddListener(onClick_FillSkillBtn);

        godModeTog.isOn = false;
        godModeTog.onValueChanged.AddListener(onTog_GodMode);
        noWaitTog.isOn = false;
        noWaitTog.onValueChanged.AddListener(onTog_NoWait);

        changeModelBtn.onClick.AddListener(onClick_ChangeModelBtn);
        useSkillBtn.onClick.AddListener(onClick_UseSkillBtn);
    }

    //生成木桩
    void onClick_GenMonsterBtn()
    {
        SendGM("PlayAlong");
    }

    //生成范围怪
    void onClick_GenAreaMonsterBtn()
    {
        string msg = $"PlayAlongs {areaMonsterNumInput_I.text} {areaMonsterNumInput_J.text}";
        SendGM(msg);
    }

    //上帝模式
    void onTog_GodMode(bool isOn)
    {
        int num = isOn ? 1 : 0;
        SendGM($"setgod {num}");
    }

    //无CD模式
    void onTog_NoWait(bool isOn)
    {
        int num = isOn ? 1 : 0;
        SendGM($"nocd {num}");
    }

    //清除所有怪物
    void onClick_KillAllMonsterBtn()
    {
        SendGM("killallmon");
    }

    //修改等级
    void onClick_ChangeLevelBtn()
    {
        SendGM($"setlevel {levelInput.text}");
    }

    //填充技能
    void onClick_FillSkillBtn()
    {
        SendGM($"addskill {fillSkillInput.text}");

        var pcg = (GameManager.Instance.M_MainPlayerCtrlBase as PlayerCtrlGroup);
        pcg.GetSkillDispather()?.SkillUnitController?.UpdateSkill(int.Parse(fillSkillInput.text));
        pcg.RefreshSkill();

        LoadPrefab();

        // var generates = skillRoot.GetComponentsInChildren<SkillGenera>();
        // if (generates != null)
        // {
        //     foreach (var item in generates)
        //     {
        //         //导出json
        //         item.ExportJson();
        //         //存储预制体
        //         item.SavePrefab();
        //         //拷贝到服务器
        //         item.ExportServerSkill();
        //     }
        // }
    }

    public void LoadPrefab()
    {
        string PrefabPath = $"Assets/DevTools/SkillEditor/Export/SKill_{fillSkillInput.text}/Skill_{fillSkillInput.text}.prefab";
        string Name = System.IO.Path.GetFileNameWithoutExtension(PrefabPath);
        var old = SkillEditorGlobal.Instance.transform.Find(Name);
        if (old != null)
        {
            Debug.LogError($"场景已经存在{Name} 不能重复加载");
            return;
        }

        var baseGenera = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (baseGenera != null)
        {
            var go = PrefabUtility.InstantiatePrefab(baseGenera) as GameObject;
            if (go != null)
            {
                go.transform.SetParent(SkillEditorGlobal.Instance.transform);
                go.transform.localPosition = UnityEngine.Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = UnityEngine.Vector3.one;
                go.name = baseGenera.name;
            }
        }
    }

    //更换模型
    void onClick_ChangeModelBtn()
    {
        g_modelID = int.Parse(roleIDInput.text);
        GameCommand gameCommand = GameManager.Instance.gameCommand.Init(E_Command.Destroy, StarWorldGame.g_vsd.M_EntityID, StarWorldGame.g_vsd);
        gameCommand.isServerAOI = false;
        GameManager.Instance.EntityDataCommand(gameCommand);


        gameCommand = GameManager.Instance.gameCommand.Init(E_Command.Create, StarWorldGame.g_vsd.M_EntityID, StarWorldGame.g_vsd);
        gameCommand.isServerAOI = false;
        GameManager.Instance.EntityDataCommand(gameCommand);


        StarWorldModule starWorld = (StarWorldModule)ModuleManager.Instance.GetModule(ModuleDef.Name.StarWorldModule);
        starWorld?.ChangeModel();
    }

    //使用技能
    void onClick_UseSkillBtn()
    {
        SendGM($"useskill {useSkillInput.text}");
    }

    private void SendGM(string command)
    {
        SocketBase battleSocket = NetworkManager.Instance.gameSocket;
        GmCmdReq gmCmdReq = new GmCmdReq();
        gmCmdReq.Cmd = command;
        battleSocket.SendRPCMsg(ServerType.ServerTypeLobby, gmCmdReq, isAutoChangeMsgTarget: false);
    }
}
#endif