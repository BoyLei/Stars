using DG.Tweening;
using UnityEngine;

public class TestQU : MonoBehaviour
{
    public GameObject _obj;
    public Vector3 beginPos, midPos, endPos;
    // Start is called before the first frame update


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            midPos = (beginPos + endPos) / 2;
            midPos.y += 10;
            //Vector3 midPos = Vector3.Lerp(beginPos, endPos, 0.5f) + Vector3.up * 2;
            //Vector3[] posArr = new Vector3[] { beginPos, pos, endPos };
            Vector3[] posArr = Reign.MathUtilities.SampleBezierCurve(beginPos, midPos, endPos, 4);

            _obj.transform.DOPath(posArr, 2.0f);

            Debug.Log(posArr);
            //DOTween.To((t) =>
            //{
            //    _obj.transform.position = BesselCurve(posArr, t);
            //}, 0, 1, 3f);
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            //Vector3 midPos = Vector3.Lerp(beginPos, endPos, 0.5f) + Vector3.up * 2;
            //Vector3[] posArr = new Vector3[] { beginPos, pos, endPos };
            Vector3[] posArr = Reign.MathUtilities.CalculateArcVertices(beginPos, midPos, endPos, 10);


            _obj.transform.DOPath(posArr, 2.0f);

            Debug.Log(posArr);
            //DOTween.To((t) =>
            //{
            //    _obj.transform.position = BesselCurve(posArr, t);
            //}, 0, 1, 3f);
        }
    }

    //public Vector3 BesselCurve(Vector3[] pos, float t)
    //{
    //    Vector3[] arr = new Vector3[pos.Length - 1];
    //    for (int i = 0; i < arr.Length; i++)
    //    {
    //        arr[i] = pos[i] * (1 - t) + pos[i + 1] * t;
    //        Debug.DrawLine(pos[i], pos[i + 1], Color.red);
    //    }
    //    if (arr.Length == 1)
    //    {
    //        return arr[0];
    //    }
    //    else
    //    {
    //        return BesselCurve(arr, t);
    //    }
    //}


    //public Transform startPoint;   // 起点
    //public Transform endPoint;     // 终点
    //public Transform[] controlPoints;  // 控制点
    //public float duration = 1f;     // 动画时长
    //public float controlPointHeight = 2f; // 控制点高度

    //private float t = 0f;           // 插值参数

    //private void Update()
    //{
    //    t += Time.deltaTime / duration;

    //    // 使用贝塞尔曲线计算当前节点的位置
    //    transform.position = CalculateBezierPoint(t);

    //    // 动画结束后销毁节点脚本
    //    if (t >= 1f)
    //    {
    //        //Destroy(this);
    //    }
    //}

    //// 计算贝塞尔曲线上某一时间t的点
    //private Vector3 CalculateBezierPoint(float t)
    //{
    //    float u = 1f - t;
    //    float tt = t * t;
    //    float uu = u * u;
    //    float uuu = uu * u;
    //    float ttt = tt * t;

    //    Vector3 p = uuu * startPoint.position;
    //    p += 3f * uu * t * (controlPoints[0].position + Vector3.up * controlPointHeight);
    //    p += 3f * u * tt * (controlPoints[1].position + Vector3.up * controlPointHeight);
    //    p += ttt * endPoint.position;

    //    return p;
    //}





    //private float _times = 3f;
    //private float _pointCount = 5f;
    //public GameObject obj;
    //public GameObject startObj;
    //public GameObject controlObj;
    //public GameObject endObj;

    //// Update is called once per frame
    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.A))
    //    {
    //        DoAnim();
    //    }
    //}
    //private void DoAnim()
    //{
    //    obj.transform.position = startObj.transform.position;
    //    Vector3[] pathvec = Bezier2Path(startObj.transform.position, controlObj.transform.position, endObj.transform.position);
    //    obj.transform.DOPath(pathvec, _times);
    //}
    ////获取二阶贝塞尔曲线路径数组
    //private Vector3[] Bezier2Path(Vector3 startPos, Vector3 controlPos, Vector3 endPos)
    //{
    //    Vector3[] path = new Vector3[(int)_pointCount];
    //    for (int i = 1; i <= _pointCount; i++)
    //    {
    //        float t = i / _pointCount;
    //        path[i - 1] = Bezier2(startPos, controlPos, endPos, t);
    //    }
    //    return path;
    //}
    //// 2阶贝塞尔曲线
    //public static Vector3 Bezier2(Vector3 startPos, Vector3 controlPos, Vector3 endPos, float t)
    //{
    //    return (1 - t) * (1 - t) * startPos + 2 * t * (1 - t) * controlPos + t * t * endPos;
    //}

    //// 3阶贝塞尔曲线
    //public static Vector3 Bezier3(Vector3 startPos, Vector3 controlPos1, Vector3 controlPos2, Vector3 endPos, float t)
    //{
    //    float t2 = 1 - t;
    //    return t2 * t2 * t2 * startPos
    //        + 3 * t * t2 * t2 * controlPos1
    //        + 3 * t * t * t2 * controlPos2
    //        + t * t * t * endPos;
    //}



}
