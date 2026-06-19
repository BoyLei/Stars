using StarProject.Service.Language;
using StarProjectDef;
using UnityEngine;
using UnityEngine.UI;

namespace StarProject.UI.Login
{
    public class ItemArea : MonoBehaviour
    {
        public JButton M_Btn;
        // 正常
        public GameObject M_NormalGob;
        public Text M_NormalAreaName;
        // 选中
        public GameObject M_SelectedGob;
        public Text M_SelectedAreaName;

        public AreaInfo M_AreaInfo;
        public E_AreaTabType M_AreaTabType;


        public void InitItemAreaInfo(AreaInfo areaInfo, E_AreaTabType areaTabType)
        {
            M_AreaTabType = areaTabType;
            RefreshAreaData(areaInfo);
            gameObject.SetActive(true);
            //M_NormalGob.SetActive(true);
            //M_SelectedGob.SetActive(false);
        }

        public void RefreshAreaData(AreaInfo areaInfo)
        {
            M_AreaInfo = areaInfo;
            string areaName = M_AreaInfo.AreaName == "Recommond" ? LanguageManager.Instance.GetLanguageByKey("RecommondGroup") : M_AreaInfo.AreaName;
            M_NormalAreaName.text = areaName;
            M_SelectedAreaName.text = areaName;
        }

        public void SetSelected(bool isSelect)
        {
            M_NormalGob.SetActive(!isSelect);
            M_SelectedGob.SetActive(isSelect);
        }

    }
}
