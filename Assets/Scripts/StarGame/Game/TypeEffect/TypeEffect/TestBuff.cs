using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SGF.Network;
using ProtoMsg;

public class TestBuff : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void TestSkillUseReq()
    {
        ProtoMsg.Vector3 v3 = new ProtoMsg.Vector3();
        v3.X = 0;
        v3.Y = 0;
        v3.Z = 0;

        SkillUseReq skillUseReq = new SkillUseReq();
        skillUseReq.SkillID = 1;
        skillUseReq.Pos = v3;
        skillUseReq.Rot = 120;


        NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeScene, skillUseReq, false);
        SGF.Debuger.Log("TestSkillUseReq --------->");
    }

    public void TestSkillQuitReq(ulong runtimeID)
    {
        SkillQuitReq skillQuitReq = new SkillQuitReq();
        skillQuitReq.RuntimeID = runtimeID;
        NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeScene, skillQuitReq, false);
        SGF.Debuger.Log("TestSkillUseReq --------->");
    }
}
