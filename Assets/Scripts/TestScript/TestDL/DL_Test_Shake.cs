using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DL_Test_Shake : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // 检测射线与物体的交点
            if (Physics.Raycast(ray, out hit, 150, LayerMask.GetMask("Ground", "Wall")))
            {
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red);

                // StarProject.Service.Battle.BattleManager.Instance.SetFindTargetPosFlag(true);
                // StarProject.Service.Battle.BattleManager.Instance.SetAutoBattleMovePoint(hit.point); ;

            }
        }

        if (Input.GetMouseButton(2))
        {
            // StarProject.Service.Battle.BattleManager.Instance.SetFindTargetPosFlag(false);
        }

    }
}
