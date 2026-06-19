using UnityEngine;
using UnityEngine.UI;

namespace StarProject.UI.Login
{
    public class ItemRoleCapacity : MonoBehaviour
    {
        public Image M_CapacityIcon;
        public Text M_Capacity;
        public RectTransform M_CapacityBar;

        public void InitItemRoleCapacity(string job, int power)
        {
            if (string.IsNullOrEmpty(job))
            {
                gameObject.SetActive(false);
            }
            else
            {
                //M_CapacityIcon
                M_Capacity.text = job;
                M_CapacityBar.sizeDelta =new Vector2( (float)power / 100.0f*320.0f,8.0f);
                gameObject.SetActive(true);
            }
        }
    }
}





