using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace StarProject.StarFramework.UI.Extend
{
    public class ScrollViewMapExtend : ScrollRect
    {
        [HideInInspector]
        public bool isDrag = false;
        [HideInInspector]
        public bool isScaleing = false;

        public Action<float> FingerScaleAction = null;

        private bool State = true;

        private int touchNum = 0;

        private float DragDistance = 0.1f;
        public void SetDragDistance(float dis)
        {
            DragDistance = dis;
        }

        private float minScale = 1.0f;
        private float maxScale = 1.0f;

        public void SetScale(float min, float max)
        {
            minScale = min;
            maxScale = max;
        }

        private RectTransform ScaleTarget;

        public void SetScaleTarget(RectTransform target)
        {
            ScaleTarget = target;
        }

        public void SetState(bool state)
        {
            State = state;
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (Input.touchCount > 1)
            {
                return;
            }

            base.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (Input.touchCount > 1)
            {
                touchNum = Input.touchCount;
                return;
            }
            else if (Input.touchCount == 1 && touchNum > 1)
            {
                touchNum = Input.touchCount;
                base.OnBeginDrag(eventData);
                return;
            }

            base.OnDrag(eventData);
            if (normalizedPosition.magnitude >= DragDistance && !isDrag)
            {
                SetDrag(true);
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            SetDrag(false);
        }

        private void SetDrag(bool drag)
        {
            isDrag = drag;
        }

        private void SetScaleing(bool scaleing)
        {
            isScaleing = scaleing;
        }

        private float preX;
        private float preY;

        private void Update()
        {
            if (!State)
            {
                return;
            }
            if (Input.touchCount > 1)
            {
                Touch t1 = Input.GetTouch(0);
                Touch t2 = Input.GetTouch(1);

                Vector3 p1 = t1.position;
                Vector3 p2 = t2.position;

                float newX = Mathf.Abs(p1.x - p2.x);
                float newY = Mathf.Abs(p1.y - p2.y);

                if (t1.phase == TouchPhase.Began || t2.phase == TouchPhase.Began)
                {
                    preX = newX;
                    preY = newY;
                }
                else if (t1.phase == TouchPhase.Moved && t2.phase == TouchPhase.Moved)
                {
                    //RectTransform rt = base.content;
                    RectTransform rt = ScaleTarget;
                    float scale = (newX + newY - preX - preY) / (rt.rect.width * 0.25f) + rt.localScale.x;

                    if (scale > minScale && scale < maxScale)
                    {
                        float ratio = scale / rt.localScale.x;

                        //rt.localScale = new Vector3(scale, scale, 0);

                        FingerScaleAction?.Invoke(scale);

                        //float maxX = base.content.rect.width  * scale / 2 - this.viewRect.rect.width  / 2;
                        //float minX = -maxX;

                        //float maxY = base.content.rect.height * scale / 2 - this.viewRect.rect.height / 2;
                        //float minY = -maxY;

                        //Vector3 pos = rt.position * ratio;

                        //if (pos.x > maxX)
                        //{
                        //    pos.x = maxX;
                        //}
                        //else if (pos.x < minX)
                        //{
                        //    pos.x = minX;
                        //}

                        //if (pos.y > maxY)
                        //{
                        //    pos.y = maxY;
                        //}
                        //else if (pos.y < minY)
                        //{
                        //    pos.y = minY;
                        //}

                        //rt.position = pos;
                    }
                }

                preX = newX;
                preY = newY;
            }
            SetScaleing(Input.touchCount > 1);
        }

    }
}
