using UnityEngine;
using UnityEngine.UI;

namespace StarProject.UI.Login
{
    public class ItemRoleSwitch : MonoBehaviour
    {
        public Image M_SwitchIcon;
        public Text M_Switch;

        public void InitItemRoleSwitch(string jobName, string iconPath)
        {
            if (string.IsNullOrEmpty(jobName))
            {
                gameObject.SetActive(false);
            }
            else
            {
                //Service.Resource.ResourceManager.Instance.LoadAssetAsync<Sprite>(iconPath,
                //(Sprite img, int index, object obj) =>
                //{
                //    if (img != null && M_SwitchIcon!= null)
                //    {
                //        M_SwitchIcon.sprite = img;
                //    }
                //});

                M_Switch.text = jobName;
                gameObject.SetActive(true);
            }
        }
    }
}




