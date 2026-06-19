using SkillEditor;
using StarProject.UI.SkillIndicator;
using UnityEngine;

namespace StarProject.Service.LocalDynamic.Fx
{
    /// <summary>
    /// 预警圈特效
    /// </summary>
    public class WarningRingEffect : MonoBehaviour
    {
        public bool Playing
        {
            get
            {
                // Playing 采用 activeInHierarchy
                return gameObject.activeSelf;
            }
            set
            {
                if (gameObject.activeSelf && value == false)
                {
                    m_SkillShapeIndicator = null;
                    m_SGAMESkillShapeIndicator = null;
                }
                gameObject.SetActive(value);
            }
        }

        // 都是是毫秒
        private float m_CurPlayTime;
        private float m_PlayMaxTime;

        private SkillShapeIndicator m_SkillShapeIndicator = null;
        private SGAMESkillShapeIndicator m_SGAMESkillShapeIndicator = null;


        private Vector3 m_localScale = new Vector3();
        //private float m_InsideRadius = 3f;
        private float m_CubeWidth = 2f;

        public void Init(ShapeSerialize shap, float startTime, float playMaxTime)
        {
            bool isPlaying = playMaxTime > startTime;

            if (!isPlaying)
            {
                return;
            }

            m_CurPlayTime = startTime;
            m_PlayMaxTime = playMaxTime;

            if (m_SkillShapeIndicator == null)
            {
                m_SkillShapeIndicator = transform.GetComponent<SkillShapeIndicator>();
            }

            if (m_SGAMESkillShapeIndicator == null)
            {
                m_SGAMESkillShapeIndicator = transform.GetComponent<SGAMESkillShapeIndicator>();
            }

            SetShap(shap);

            Playing = true;
        }

        private void SetShap(ShapeSerialize shape)
        {
            if (shape == null)
            {
                Playing = false;
                return;
            }
            switch (shape.ShapeType)
            {
                case Shape.Round:
                    {
                        //m_InsideRadius = (float)shape.Round.Radius / 100;

                        //m_localScale.x = m_InsideRadius * 2;
                        //m_localScale.y = m_InsideRadius * 2;
                        //m_localScale.z = m_InsideRadius * 2;
                        //transform.localScale = m_localScale;

                        m_SGAMESkillShapeIndicator.SetSectorRange((float)shape.Round.Radius / 100);

                        //m_SkillShapeIndicator.SetCircleRange((float)shape.Round.Radius / 100);
                    }
                    break;
                case Shape.HollowCircle:
                    {
                        //m_InsideRadius = (float)shape.HollowCircle.MinRadius / 100;

                        //m_localScale.x = m_InsideRadius * 2;
                        //m_localScale.y = m_InsideRadius * 2;
                        //m_localScale.z = m_InsideRadius * 2;
                        //transform.localScale = m_localScale;

                        m_SGAMESkillShapeIndicator.SetSectorRange((float)shape.HollowCircle.MaxRadius / 100);

                        //m_SkillShapeIndicator.SetCircleRange((float)shape.HollowCircle.MaxRadius / 100);
                    }
                    break;
                case Shape.Sector:
                    {
                        //m_InsideRadius = (float)shape.Sector.Radius / 100;

                        //m_localScale.x = m_InsideRadius * 2;
                        //m_localScale.y = m_InsideRadius * 2;
                        //m_localScale.z = m_InsideRadius * 2;
                        //transform.localScale = m_localScale;

                        m_SGAMESkillShapeIndicator.SetSectorRange((float)shape.Sector.Radius / 100);
                        m_SGAMESkillShapeIndicator.SetSectorAngle((float)shape.Sector.Angle * 2);

                        //m_SkillShapeIndicator.SetSectorRange((float)shape.Sector.Radius / 100);
                        //m_SkillShapeIndicator.SetSectorAngle((float)shape.Sector.Angle * 2);
                    }
                    break;
                case Shape.RingFan:
                    {
                        //m_InsideRadius = (float)shape.RingFan.MaxRadius / 100;
                        //m_localScale.x = m_InsideRadius * 2;
                        //m_localScale.y = m_InsideRadius * 2;
                        //m_localScale.z = m_InsideRadius * 2;
                        //transform.localScale = m_localScale;

                        float maxRadius = (float)shape.RingFan.MaxRadius / 100;
                        m_SGAMESkillShapeIndicator.SetSectorRange(maxRadius);
                        //m_SkillShapeIndicator.SetRingFanRange(maxRadius);
                        //m_SkillShapeIndicator.SetRingFanAngle((float)shape.RingFan.Angle * 2);
                        m_SGAMESkillShapeIndicator.SetSectorAngle((float)shape.RingFan.Angle * 2);

                        float minRadius = (float)shape.RingFan.MinRadius / 100;
                        float middleRing = minRadius / maxRadius;
                        //m_SkillShapeIndicator.SetRingFanMiddleRingSize(middleRing);
                        m_SGAMESkillShapeIndicator.SetRingFanMiddleRingSize(middleRing);
                    }
                    break;
                case Shape.Arrow:
                    {
                        //m_InsideRadius = (float)shape.Rect.Length / 100;
                        //m_CubeWidth = (float)shape.Rect.Width / 100;
                        //m_localScale.x = m_CubeWidth;
                        //m_localScale.y = 1;
                        //m_localScale.z = m_InsideRadius;
                        //transform.localScale = m_localScale;

                        m_SGAMESkillShapeIndicator.SetRectWidthAndLength((float)shape.Rect.Width / 100, (float)shape.Rect.Length / 100);

                        //m_SkillShapeIndicator.SetWarningRectRange((float)shape.Arrow.Width / 100);
                        //m_SkillShapeIndicator.SetWarningRectLength((float)shape.Arrow.Length / 100);
                    }
                    break;
                case Shape.Rect:
                    {
                        //m_InsideRadius = (float)shape.Rect.Length / 100;
                        //m_CubeWidth = (float)shape.Rect.Width / 100;
                        //m_localScale.x = m_CubeWidth;
                        //m_localScale.y = 1;
                        //m_localScale.z = m_InsideRadius;
                        //transform.localScale = m_localScale;

                        m_SGAMESkillShapeIndicator.SetRectWidthAndLength((float)shape.Rect.Width / 100, (float)shape.Rect.Length / 100);

                        //m_SkillShapeIndicator.SetWarningRectRange((float)shape.Rect.Width / 100);
                        //m_SkillShapeIndicator.SetWarningRectLength((float)shape.Rect.Length / 100);
                    }
                    break;
                case Shape.RotRoute:
                case Shape.InputTarget:
                    // 路径不知道要做成啥样
                    break;
                default:
                    break;
            }
            if (m_SkillShapeIndicator != null)
            {
                m_SkillShapeIndicator.SetShapeActive(true);
            }
            if (m_SGAMESkillShapeIndicator != null)
            {
                m_SGAMESkillShapeIndicator.SetShapeActive(true);
            }
        }

        private void OnDisable()
        {
            if (m_SkillShapeIndicator != null)
            {
                m_SkillShapeIndicator.SetGlowRange(0);
            }
            if (m_SGAMESkillShapeIndicator != null)
            {
                m_SGAMESkillShapeIndicator.SetGlowRange(0);
            }
        }

        void Update()
        {
            if (m_PlayMaxTime == -1)
            {
                return;
            }

            m_CurPlayTime += UnityEngine.Time.deltaTime * 1000;

            if (m_CurPlayTime > m_PlayMaxTime)
            {
                Playing = false;
            }
            if (m_SkillShapeIndicator != null)
            {
                float value = m_CurPlayTime / m_PlayMaxTime;
                m_SkillShapeIndicator.SetGlowRange(value);
            }

            if (m_SGAMESkillShapeIndicator != null)
            {
                float value = m_CurPlayTime / m_PlayMaxTime;
                m_SGAMESkillShapeIndicator.SetGlowRange(value);
            }
        }

        internal void DestroyRelease()
        {
            Playing = false;
            m_SkillShapeIndicator = null;
            m_SGAMESkillShapeIndicator = null;
            Destroy(gameObject);
        }
    }
}