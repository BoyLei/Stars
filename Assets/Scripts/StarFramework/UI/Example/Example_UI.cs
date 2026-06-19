using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SGF.UI.Framework;
using UnityEngine;

namespace SGF.UI.Example
{
    public class Example_UI : MonoBehaviour
    {
        void Start()
        {
            Debuger.EnableLog = true;
            SGF.UI.Framework.UIManager.Instance.Init("ui/Example/");
            SGF.UI.Framework.UIManager.MainPage = "UIPage1";
            SGF.UI.Framework.UIManager.Instance.EnterMainPage();

        }


    }
}
