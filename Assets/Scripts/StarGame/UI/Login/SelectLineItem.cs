using UnityEngine;
using UnityEngine.UI;

namespace StarProject.UI.Login
{
    public class SelectLineItem : MonoBehaviour
    {
        [SerializeField] public Text M_LineText;
        [SerializeField] public Toggle M_Toggle;


        public ulong M_lineId;  // ∑÷œﬂID
        private int index;

        public void InitSelectLineData(ulong lineId, int idx)
        {
            M_lineId = lineId;
            index = idx;
            if (M_lineId == 0 && idx != -1)
            {
                this.gameObject.SetActive(false);
            }
            else
            {
                M_LineText.text = M_lineId.ToString();
                this.gameObject.SetActive(true);
            }
        }
    }
}
