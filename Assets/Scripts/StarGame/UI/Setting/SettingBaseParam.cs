using UnityEngine;
namespace StarProject.UI.StarWorld
{
    public class SettingBaseParam : MonoBehaviour
    {
        protected CanvasGroup canvasGroup;
        protected Animator anim;

        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            anim = GetComponent<Animator>();
        }

        public virtual void Reset()
        {

        }

        public virtual void Init()
        {
            Reset();
        }

        public void SetShow(bool isShow)
        {
            canvasGroup.alpha = isShow ? 1 : 0;
            canvasGroup.interactable = isShow;
            canvasGroup.blocksRaycasts = isShow;
            if (isShow)
            {
                anim.Play("Go");
            }
        }

        public virtual void SaveLocalData()
        {

        }

        public virtual void SaveServerData()
        {

        }

    }
}

