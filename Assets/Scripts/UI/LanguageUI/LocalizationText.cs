using Sirenix.OdinInspector;
using StarProject.Service.Language;
using StarProjectDef;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarProject.UI.LanguageUI
{
    public class LocalizationText : MonoBehaviour
    {
        [LabelText("多语言表key")]
        public string key = string.Empty;


        private IEnumerable _languageType = new ValueDropdownList<LanguageType>()
        {
            { "中文", LanguageType.Chinese},
            { "英文", LanguageType.English},
        };

        [LabelText("默认语言")]
        [ValueDropdown("_languageType")]
        [OnValueChanged("OnLanguageTypeChange")]
        public LanguageType languageType;

        public void OnLanguageTypeChange()
        {
            if (languageType != LanguageManager.Instance.CurLanguageType)
            {
                Refresh();
            }
        }

        private Text text;
        private TextMeshPro textMeshPro;
        private TextMeshProUGUI textMeshProText;

        private void Awake()
        {
            text = GetComponent<Text>();
            textMeshPro = GetComponent<TextMeshPro>();
            textMeshProText = GetComponent<TextMeshProUGUI>();

            LanguageManager.Instance.RegisterAction(Refresh);

            languageType = LanguageManager.Instance.CurLanguageType;

            Refresh();
        }

        private void OnEnable()
        {
            var _languageType = LanguageManager.Instance.CurLanguageType;
            if (languageType != _languageType)
            {
                languageType = _languageType;
                Refresh();
            }
        }

        private void Start()
        {

        }

        private void Refresh()
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            if (languageType == LanguageType.English)
            {
                if (text != null && !text.resizeTextForBestFit)
                {
                    text.resizeTextForBestFit = true;
                    text.resizeTextMinSize = text.fontSize - 15;
                    text.resizeTextMaxSize = text.fontSize;
                    text.horizontalOverflow = HorizontalWrapMode.Wrap;//受限于拼接框
                    text.verticalOverflow = VerticalWrapMode.Truncate;//overflow是全显示出来
                }
            }


            string val = LanguageManager.Instance.GetLanguageByKey(key, languageType);

            if (text != null)
            {
                text.text = val;
            }

            if (textMeshPro != null)
            {
                textMeshPro.text = val;
            }

            if (textMeshProText != null)
            {
                textMeshProText.text = val;
            }
        }

        private void OnDisable()
        {
            LanguageManager.Instance.UnRegisterAction(Refresh);
        }

        private void OnDestroy()
        {
            LanguageManager.Instance.UnRegisterAction(Refresh);
        }

    }
}