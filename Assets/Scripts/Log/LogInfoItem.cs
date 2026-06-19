using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace LogModule
{
    public class LogInfoItem : MonoBehaviour
    {
        public Text text;

        public void OnRefresh(LogViewInfo logViewInfo)
        {
            text.text = logViewInfo.LogString;
            switch (logViewInfo.LogInfoType)
            {
                case LogType.Error:
                    {
                        text.color = Color.red;
                    }
                    break;
                case LogType.Assert:
                    {
                        text.color = new Color(1f, 0.5f, 0f);
                    }
                    break;
                case LogType.Warning:
                    {
                        text.color = new Color(1f, 0.84f, 0.02f);
                    }
                    break;
                case LogType.Exception:
                    {
                        text.color = new Color(0.616f, 0f, 1f);
                    }
                    break;
                case LogType.Log:
                    {
                        text.color = Color.white;
                    }
                    break;

                default: break;
            }
        }

        public void OnRefresh(LogInfo logInfo)
        {
            text.text = logInfo.ToString();
            switch (logInfo.LogInfoType)
            {
                case LogType.Error:
                    {
                        text.color = Color.red;
                    }
                    break;
                case LogType.Assert:
                    {
                        text.color = new Color(1f, 0.5f, 0f);
                    }
                    break;
                case LogType.Warning:
                    {
                        text.color = new Color(1f, 0.84f, 0.02f);
                    }
                    break;
                case LogType.Exception:
                    {
                        text.color = new Color(0.616f, 0f, 1f);
                    }
                    break;
                case LogType.Log:
                    {
                        text.color = Color.white;
                    }
                    break;

                default: break;
            }

        }
    }
}
