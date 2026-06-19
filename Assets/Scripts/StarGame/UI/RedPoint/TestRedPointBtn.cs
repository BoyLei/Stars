using System.Collections;
using System.Collections.Generic;
using StarProjectDef;
using UnityEngine;
using UnityEngine.UI;

public class TestRedPointBtn : MonoBehaviour
{

    public RedPointType redPointType;

    public bool add = true;

    public RedPointConditionType redPointConditionType;
    public bool testCount = true;

    public bool openTestWidget = false;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        if (openTestWidget)
        {
            //SGF.UI.Framework.UIManager.Instance.OpenWidget(UIDef.TestRedPointWidget, true, null, SGF.UI.Framework.UIRoot.UIROOT.transform, MainPageCommond.HideNone, true, true);
            SGF.UI.Framework.UIManager.Instance.OpenWidgetAsync(UIDef.TestRedPointWidget, null, true, null, SGF.UI.Framework.UIRoot.UIROOT.transform, MainPageCommond.HideNone, true, true);
            return;
        }
        if (testCount)
        {
            RedPointManager.Instance.TestCount(redPointType, add);
            return;
        }

        bool result = !RedPointManager.Instance.recordBool[redPointConditionType];
        RedPointManager.Instance.TestBool(redPointConditionType, result);

    }
}
