using UnityEngine;
using UnityEngine.UI;

namespace StarProject.UI.Login
{
    public class ItemRoleLabel : MonoBehaviour
    {
        public Text M_Label;

        public void InitItemRoleLabelInfo(string label)
        {
            if (string.IsNullOrEmpty(label))
            {
                gameObject.SetActive(false);
            }
            else
            {
                M_Label.text = label;
                gameObject.SetActive(true);
            }
        }
    }
}

