using SGF.UI.Framework;
using StarProject;
using StarProject.Game.Entity.VitalSigns;
using UnityEngine;
using UnityEngine.UI;

public class ArrowLockAtWidget : UIWidget
{
    //private string Log_Tag = "[ArrowLockAtWidget]";

    public Text M_nickName;                     // 昵称
    public Transform M_arrow;                   // 箭头
    public Transform M_bg;                   // 背景


    //private Vector3 prePos = Vector3.zero;
    public float devValue = 100f;

    private NPCEntityBase m_entityBase;    //目标

    private Camera m_uiCamera;  // ui摄像机

    //private bool m_isCheck = true;

    private RectTransform m_arrowRect;

    protected override void Awake()
    {
        GlobalEvent.onTargetArrowShow.AddListener(SetChildAction);
        GlobalEvent.onTargetArrowPos.AddListener(SetArrowData);
        //m_uiCamera = CameraManager.Instance.GetCamera(E_CameraType.UICam).Camera;//--
        m_arrowRect = M_arrow.GetComponent<RectTransform>();
        base.Awake();
    }

    protected override void OnDestroy()
    {
        GlobalEvent.onTargetArrowShow.RemoveListener(SetChildAction);
        GlobalEvent.onTargetArrowPos.RemoveListener(SetArrowData);
    }

    protected override void OnOpen(object arg)
    {
        base.OnOpen(arg);
        SetEnemyInfo((NPCEntityBase)arg);
    }

    public void SetEnemyInfo(NPCEntityBase entityBase)
    {
        // 如果是空的
        if (entityBase == null)
        {
            m_entityBase = null;
            SetChildAction(false);
            return;
        }
        // 如果相同了
        if (m_entityBase == entityBase)
        {
            return;
        }
        m_entityBase = entityBase;
        M_nickName.text = m_entityBase.M_Name;

        //m_isCheck = true;
    }

    public void CloseWidhet()
    {
        m_entityBase = null;

        base.Close();
    }

    /*private void LateUpdate()
    {
        if (m_entityBase != null)
        {
            var pos = m_battleCamera.WorldToScreenPoint(m_entityBase.Position());
            var distance = Vector3.Distance(prePos, pos);
            if (distance < 1) {*//* SGF.Debuger.Log("沒動")*//*; return; };
            prePos = pos;
            if (0 < pos.x && pos.x < Screen.width && pos.y > 0 && pos.y < Screen.height && pos.z > 0)
            {
                // 在屏幕内了
                SetChildAction(false);
                return;
            }
            else if (!m_isCheck)
            {
                SetChildAction(true);
            }
            if (pos.z < 0)
                pos = pos * -1;
            var startpos = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            var dir = pos - startpos;

            //通过反余弦函数获取 向量 a、b 夹角（默认为 弧度）wwwwww
            float radians = Mathf.Atan2(dir.y, dir.x);
            //将弧度转换为 角度
            float angle = (radians * Mathf.Rad2Deg) - 90;
            transform.localEulerAngles = new Vector3(0, 0, angle);

            float sereenangle = (float)(Screen.height) / (float)(Screen.width);
            var va = Mathf.Abs(dir.y / dir.x);
            var length = m_arrowRect.sizeDelta.x;
            if (va <= sereenangle)
            {
                if (pos.x < 0)
                {
                    //SGF.Debuger.Log($"方向 左");
                    transform.position = GetNode(pos, startpos, length * 0.5f, true);
                }
                else
                {
                    //SGF.Debuger.Log($"方向 右");
                    transform.position = GetNode(pos, startpos, Screen.width - length * 0.5f, false);
                }
            }
            else
            {
                if (pos.y < 0)
                {
                    //SGF.Debuger.Log($"方向 下");
                    transform.position = GetNode2(pos, startpos, length * 0.5f, false);
                }
                else
                {
                    //SGF.Debuger.Log($"方向 上");
                    transform.position = GetNode2(pos, startpos, Screen.height - length * 0.5f, true);
                }
            }
        }
    }*/

    private void SetArrowData(Vector3 pos)
    {
        var startpos = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        var dir = pos - startpos;

        //通过反余弦函数获取 向量 a、b 夹角（默认为 弧度）wwwwww
        float radians = Mathf.Atan2(dir.y, dir.x);
        //将弧度转换为 角度
        float angle = (radians * Mathf.Rad2Deg) - 90;
        transform.localEulerAngles = new Vector3(0, 0, angle);
        M_bg.localEulerAngles = new Vector3(0, 0, -angle);
        float sereenangle = (float)Screen.height / (float)Screen.width;
        var va = Mathf.Abs(dir.y / dir.x);
        var length = m_arrowRect.sizeDelta.x;
        if (va <= sereenangle)
        {
            if (pos.x < 0)
            {
                //SGF.Debuger.Log($"箭头问题 方向 左");
                transform.position = GetNode(pos, startpos, length * 0.5f, true);
            }
            else
            {
                //SGF.Debuger.Log($"箭头问题 方向 右");
                transform.position = GetNode(pos, startpos, Screen.width - (length * 0.5f), false);
            }
        }
        else
        {
            if (pos.y < 0)
            {
                //SGF.Debuger.Log($"箭头问题 方向 下");
                transform.position = GetNode2(pos, startpos, length * 0.5f, false);
            }
            else
            {
                //SGF.Debuger.Log($"箭头问题 方向 上");
                transform.position = GetNode2(pos, startpos, Screen.height - (length * 0.5f), true);
            }
        }
    }

    private Vector3 GetNode2(Vector3 pos, Vector3 startpos, float v, bool isUp)
    {
        pos = new Vector3(pos.x, pos.y, 0);
        Vector3 ab = pos - startpos;
        float amx = isUp ? startpos.y - v + devValue : startpos.y - v - devValue;
        float amy = isUp ? pos.y - startpos.y + devValue : pos.y - startpos.y + devValue;
        Vector3 am = ab * (Mathf.Abs(amx) / Mathf.Abs(amy));
        Vector3 om = startpos + am;
        // 屏幕坐标转换为 UGUI 坐标
        return ScreenPointToUIPoint(m_arrowRect, om);
    }

    private Vector3 GetNode(Vector3 pos, Vector3 startpos, float v, bool isLeft)
    {
        pos = new Vector3(pos.x, pos.y, 0);
        Vector3 ab = pos - startpos;
        float amx = isLeft ? startpos.x - v - devValue : startpos.x - v + devValue;
        float amy = isLeft ? pos.x - startpos.x - devValue : pos.x - startpos.x + devValue;
        Vector3 am = ab * (Mathf.Abs(amx) / Mathf.Abs(amy));
        Vector3 om = startpos + am;

        // 屏幕坐标转换为 UGUI 坐标
        return ScreenPointToUIPoint(m_arrowRect, om);
    }

    // 屏幕坐标转换为 UGUI 坐标
    private Vector3 ScreenPointToUIPoint(RectTransform rt, Vector2 screenPoint)
    {
        Vector3 globalMousePos;
        //UI屏幕坐标转换为世界坐标
        // 当 Canvas renderMode 为 RenderMode.ScreenSpaceCamera、RenderMode.WorldSpace 时 uiCamera 不能为空
        // 当 Canvas renderMode 为 RenderMode.ScreenSpaceOverlay 时 uiCamera 可以为空

        // 计算屏幕点对应的世界坐标
        // 这个是转换在默认canvas下的坐标内
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, screenPoint, null, out globalMousePos))
        {
            // 将世界坐标转换到 BattleCamera 的坐标系下；不要变成3D
            //globalMousePos = M_battleCamera.ScreenToWorldPoint(globalMousePos);
        }

        // 下面是转换在相机渲染的坐标下
        //RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, screenPoint, M_battleCamera, out globalMousePos);


        // 转换后的 globalMousePos 使用下面方法赋值
        // target 为需要使用的 UI RectTransform
        // rt 可以是 target.GetComponent<RectTransform>(), 也可以是 target.parent.GetComponent<RectTransform>()
        // target.transform.position = globalMousePos;

        // 矩形限制区域方案
        // 屏幕宽高
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // 计算中心区域的边界
        float centerX = screenWidth / 2;
        float centerY = screenHeight / 2;
        float halfWidth = screenWidth / 4;  // 50% 区域的半宽
        float halfHeight = screenHeight / 4; // 50% 区域的半高

        float minX = centerX - halfWidth;
        float maxX = centerX + halfWidth;
        float minY = centerY - halfHeight;
        float maxY = centerY + halfHeight;

        // 限制X坐标
        globalMousePos.x = Mathf.Clamp(globalMousePos.x, minX, maxX);
        // 限制Y坐标
        globalMousePos.y = Mathf.Clamp(globalMousePos.y, minY, maxY);

        //// 圆形限制区域方案
        //// 屏幕中心坐标
        //Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        //// 设定圆形区域的半径 (这里设置为屏幕宽度或高度的一半)
        //float radius = Mathf.Min(Screen.width, Screen.height) / 4;  // 屏幕中心50%的区域

        //// 计算UI元素与屏幕中心的距离
        //Vector2 offset = globalMousePos - (Vector3)screenCenter;
        //float distance = offset.magnitude;

        //// 如果距离大于半径，将其限制在圆形区域内
        //if (distance > radius)
        //{
        //    offset = offset.normalized * radius;
        //    globalMousePos = screenCenter + offset;
        //}

        return globalMousePos;
    }

    private void SetChildAction(bool show)
    {
        if (show && m_entityBase == null)
        {
            return;
        }
        //M_nickName.gameObject.SetActive(show);
        M_arrow.gameObject.SetActive(show);
        M_bg.gameObject.SetActive(show);
        //m_isCheck = show;
    }
}
