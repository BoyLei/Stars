using Sirenix.OdinInspector;
using StarProject.Service.Language;
using StarProjectDef;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarProject.UI.LanguageUI
{
    public class LocalizationActive : MonoBehaviour
    {
        private LanguageType _LanguageType;
        private GameObject _Go;
        private bool _ActiveState;


        private void Awake()
        {
            _LanguageType = LanguageManager.Instance.CurLanguageType;
            _Go = transform.gameObject;
            _ActiveState = _Go.activeSelf;
        }

        private void OnEnable()
        {
            _ActiveState = _Go.activeSelf;
            bool isActive = _LanguageType != null && _LanguageType != LanguageType.English;
            if (_Go != null && _ActiveState != isActive)
            {
                _Go.SetActive(isActive);
                _ActiveState = isActive;
            }
        }
    }
}