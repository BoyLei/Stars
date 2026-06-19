using System.Collections;
using System.Collections.Generic;
using Frame;
using StarProject.Service.Battle;
using UnityEngine;

public class SwitchEnemyBtn : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        var jButton = GetComponent<JButton>();
        jButton.OnClick = (go) =>
        {
            bool switchResult = BattleManager.Instance.SwitchCurSearchTarget();
            if (!switchResult)
            {
                Util.ShowMessageByCode(StarProjectDef.CRetMsgEnum.Tips_Skill_Cannot_Switch_Target);
            }
        };

    }


}
