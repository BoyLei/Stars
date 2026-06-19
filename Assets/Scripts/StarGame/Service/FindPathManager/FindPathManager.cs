///--------------------------------------------------------------------
/// 文件名   :   FindPathManager
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/08/04 09:25:02
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------


using SGF.Module.Framework;
using SGF.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace StarProject.Service.FindPath
{
    public class FindPathManager : ServiceModule<FindPathManager>
    {
        private int playerAgentId;

        private int playerFatAgentId;

        private const int FIND_PATH_LAYER = 16;

        private const int PLAYER_FATAREA_DIS_DEF = 10;//玩家和肥距离检测
        private int PlayerFatAreaDis = PLAYER_FATAREA_DIS_DEF;//玩家和肥距离检测

        private const int NPC_FATAREA_DIS_DEF = 10;//玩家和肥距离检测
        private int NpcFatAreaDis = NPC_FATAREA_DIS_DEF;//玩家和肥距离检测


        private const int MIN_CHECKER_DIS = 5;//最小检测距离，小于这个距离直接走3

        private NavMeshAgent m_NavMeshAgent;

        private NavMeshPath path;

        private LineRenderer lineRenderer;

        private GameObject lineGo;

        private string RenderLinePath = "Prefabs/VirtualCamera/FindPathLine";

        public const float MaxSearchRange = 10;
        public void Init()
        {



            playerAgentId = NavMesh.GetSettingsByIndex(0).agentTypeID;
            playerFatAgentId = NavMesh.GetSettingsByIndex(3).agentTypeID;

            GameObject go = GameObject.Find("NavmeshAgent");

            if (go != null)
            {
                m_NavMeshAgent = go.GetComponent<NavMeshAgent>();
                if (m_NavMeshAgent == null)
                {
                    m_NavMeshAgent = go.AddComponent<NavMeshAgent>();
                }
            }
            else
            {
                go = new GameObject("NavmeshAgent");
                //   GameObject.DontDestroyOnLoad(go);
                m_NavMeshAgent = go.AddComponent<NavMeshAgent>();

            }
            /*NavMesh.SetAreaCost(NavMesh.GetAreaFromName("FindPathArea"), 500);*/
            m_NavMeshAgent.agentTypeID = playerFatAgentId;
            m_NavMeshAgent.speed = 6.9f;

            go.transform.SetParent(EntityRoot.Instance.DotRemoveRoot.transform);

            path = new NavMeshPath();
            //StarProject.Service.Resource.ResourceManager.Instance.PopGameObject(RenderLinePath, (gameobject) =>
            //{
            //    if (gameobject != null)
            //    {
            //        lineGo = gameobject;
            //        lineGo.SetActive(false);
            //        lineGo.transform.SetParent(EntityRoot.Instance.DotRemoveRoot.transform);
            //        lineRenderer = lineGo.GetComponent<LineRenderer>();
            //    }
            //});

            MonoHelper.AddUpdateListener(OnUpdateHandler, MonoHelper.E_ModuleType.PathFinding);
        }

        private void OnUpdateHandler()
        {

            if (lineGo == null)
            {
                return;
            }
            if (path == null)
            {
                return;
            }
            if (m_NavMeshAgent == null)
            {
                return;
            }

            lineRenderer.positionCount = m_NavMeshAgent.path.corners.Length;
            lineRenderer.SetPositions(m_NavMeshAgent.path.corners);

            lineGo.SetActive(m_NavMeshAgent.path.corners.Length > 0);
        }

        // List<GameObject> list = new List<GameObject>();
        /// <summary>
        /// 第一段路：简称1 是起始点路到肥子Start
        /// 第二段路：简称2 是肥子Start路到肥子End
        /// 第三段路：简称3 是肥子End路到目标
        /// 
        /// 第一种情况：很近距离直接走“3”
        /// 其他情况中
        /// 第二种情况：如果再肥子空间，直接走“2”-“3”
        /// 第三种情况：如果再正常空间，直接走“1”-“2”-“3”
        /// </summary>
        /// <param name="start"></param>
        /// <param name="target"></param>
        /// <param name="distance">到目标点 距离停止</param>
        /// <param name="corners"></param>
        /// <param name="stopMinDistance">导航到 目标点停止的最小距离</param>
        /// <returns></returns> <summary>
        public bool FindPath(Vector3 start, Vector3 target, float distance, out Vector3[] corners, float stopMinDistance = 1f)
        {
            #region 数据初始化流程
            m_NavMeshAgent.stoppingDistance = Mathf.Max(distance, stopMinDistance);
            corners = new Vector3[1];
            List<Vector3> pointList = new();
            float startToTargetDis = Vector3.Distance(start, target);
            bool _____isNearest = startToTargetDis < MIN_CHECKER_DIS;
            #endregion
            /*
               He渲染正式
               找不到附近等于3的情况 不要中断到别处*/
            Debug.Log(startToTargetDis + "就是距离");
            path.ClearCorners();
            if (!_____isNearest)
            {
                #region 搜寻器薪资准备
                //如果目标点不在Navmesh 上则修正到navmesh
                m_NavMeshAgent.agentTypeID = playerFatAgentId;
                //寻路指引器初始点设定

                var fatStart = m_NavMeshAgent.transform.position;
                NavMeshQueryFilter navMeshQueryFilter = new();
                navMeshQueryFilter.agentTypeID = playerFatAgentId;
                navMeshQueryFilter.areaMask = FIND_PATH_LAYER;// 16;
                                                              //navMeshQueryFilter.areaMask = 1 << NavMesh.GetAreaFromName("FindPathArea");
                #endregion
                #region 准备流程
                /*出生点
                 到肥点
                 终点
                 策略上 三角形通常要走费劲的路，所以一定有现在是第一种123还是第三种3 边界是否选择情况

                  */



                //正常流程提前确定肥START
                bool isStarInFatArea = IsInFatArea(start);
                if (isStarInFatArea)
                {




                    fatStart = start;
                }
                else
                {
                    //正常流程，在肥圈内寻找初始点
                    if (NavMesh.SamplePosition(start, out NavMeshHit hit, PlayerFatAreaDis, navMeshQueryFilter))
                    {
                        fatStart = hit.position;
                        PlayerFatAreaDis = PLAYER_FATAREA_DIS_DEF;//还原init
                    }
                    else
                    {
                        //深度是信心是思路，广度是效率，都重要
                        //理论上，设定和自增加都不会存在问题，但是万一出现问题
                        // 开始 （肥S） （ 肥E） 结尾：
                        //
                        //如果Start和Target在一边的情况：【两个都没有表现正常直接过去】，其中一个没有就很怪 要跑回去fat 
                        //
                        //如果Start和Target在两边的情况：两个都没有 表现永远不贴边，【其中一个没有起码保证在路径入或者路径出】
                        ///但是在两边的情况不好拿（用射线也有包裹的形式其实还是要走肥路）：两个都没有表现永远不贴边，就没办法；往回跑一下也是没办法？？？
                        ///都处理不贴边有点太粗了 4种 情况中 毕竟50% 是好得,就这么设定了，不处理，毕竟不好识别
                        ///另外是因为player 和 npc 距离设置不对才跑出来的，正常不会不对，我还会增加的；不管了

                        //异常情况-------------没在肥区域，但是还找不到附近的肥起始点，策划问题下次会找到但是现在也不能有问题
                        fatStart = start;
                        //m_NavMeshAgent.Warp(start);

                        //玩家在边界不能让他找不到
                        PlayerFatAreaDis++;
                    }
                }

                //正常流畅时提前确定肥END
                var fatEnd = m_NavMeshAgent.transform.position;
                if (NavMesh.SamplePosition(target, out NavMeshHit hit1, NpcFatAreaDis, navMeshQueryFilter))
                {
                    fatEnd = hit1.position;
                    NpcFatAreaDis = NPC_FATAREA_DIS_DEF;
                }
                else
                {
                    //异常情况-------------没在肥区域，但是还找不到附近的肥最后点，策划问题下次会找到但是现在也不能有问题

                    fatEnd = target;
                    //尾巴走的时候自然会设置
                    //m_NavMeshAgent.Warp(fatEnd);
                    //如果两种异常情况都存在的时候或者其中一种存在，那就是直接起点到终点（本次）

                    NpcFatAreaDis++;
                    Debug.Log("找不到肥End"); //NPC距离摆放
                }
                #endregion
                #region 第一段路
                m_NavMeshAgent.agentTypeID = playerAgentId;

                if (isStarInFatArea)
                {

                }
                else
                {
                    m_NavMeshAgent.Warp(start);
                    if (m_NavMeshAgent.isOnNavMesh && IsWalkable(fatStart))
                    {
                        if (path.corners.Length != 0)
                        {
                            for (int i = 0; i < path.corners.Length; i++)
                            {
                                pointList.Add(path.corners[i]);
                            }
                        }

                    }
                }

                #endregion
                #region 第三段路
                List<Vector3> lastPoints = new();

                m_NavMeshAgent.Warp(fatEnd);
                if (m_NavMeshAgent.isOnNavMesh && IsWalkable(target))
                {
                    if (path.corners.Length != 0)
                    {
                        for (int i = 0; i < path.corners.Length; i++)
                        {
                            lastPoints.Add(path.corners[i]);
                        }
                    }
                }
                #endregion
                #region 第二段路
                m_NavMeshAgent.Warp(fatStart);
                m_NavMeshAgent.agentTypeID = playerFatAgentId;
                //m_NavMeshAgent.agentTypeID = playerFatAgentId;
                //指引器是否在地面上，并且路径清理，用指引器寻路
                if (m_NavMeshAgent.isOnNavMesh && IsWalkable(fatEnd))
                {
                    for (int i = 0; i < path.corners.Length; i++)
                    {
                        pointList.Add(path.corners[i]);
                    }


                    for (int i = 0; i < lastPoints.Count; i++)
                    {
                        pointList.Add(lastPoints[i]);
                    }
                    corners = pointList.ToArray();
                    //for (int i = 0; i < corners.Length; i++)
                    //{
                    //    GameObject trans = GameObject.Find("Sphere");
                    //    GameObject.Instantiate(trans, corners[i], Quaternion.identity);
                    //}

                    //不近时只有两种情况2，3 和 1，2，3
                    //第二段路是必须的，所以必须走这里才是true
                    return true;
                }
                else
                {
                    Debug.Log("第二段路径不能走");
                    return false;
                }
                #endregion
            }
            else
            {
                #region 直接走第三段路直接shin寻路
                m_NavMeshAgent.agentTypeID = playerAgentId;
                m_NavMeshAgent.Warp(start);
                if (m_NavMeshAgent.isOnNavMesh && IsWalkable(target))
                {
                    if (path.corners.Length != 0)
                    {
                        for (int i = 0; i < path.corners.Length; i++)
                        {
                            pointList.Add(path.corners[i]);
                        }
                        corners = pointList.ToArray();
                        return true;
                    }
                    else
                    {
                        Debug.Log("直接走第三段路没有路店可以走");
                    }

                }
                else
                {
                    Debug.Log("直接走第三段路不能走");
                }
                #endregion
            }




            return false;

        }

        private bool IsInFatArea(Vector3 start)
        {
            //通常设定就是2
            if (NavMesh.SamplePosition(start, out NavMeshHit hit999, 2, 16))
            {

                //我猜测unity就是球体检测
                //假设就是平面x he Z 一定一样
                if (hit999.position.x == start.x && hit999.position.z == start.z)
                {
                    Debug.Log("是在肥圈内");
                    return true;
                }
            }
            else
            {
                Debug.Log("不在肥圈内");
                return false;

            }
            return false;
        }

        private void FatToShinSetDest(Vector3 target)
        {
            //m_NavMeshAgent.agentTypeID = playerFatAgentId;
            if (!m_NavMeshAgent.SetDestination(target))
            {
                /*m_NavMeshAgent.agentTypeID = playerAgentId;
                if (!m_NavMeshAgent.SetDestination(target))
                {
                    Debuger.LogError("基础寻路都有问题");
                }*/
            }

        }

        public bool IsWalkable(Vector3 target)
        {
            path.ClearCorners();

            return m_NavMeshAgent.CalculatePath(target, path);
        }

        public bool CanArrive(Vector3 start, Vector3 target)
        {
            path.ClearCorners();
            m_NavMeshAgent.Warp(start);
            m_NavMeshAgent.CalculatePath(target, path);

            if (path.status == NavMeshPathStatus.PathComplete)
            {
                return true;
            }

            return false;
        }


        public bool CheckWalkable(Vector3 start, Vector3 target)
        {
            path.ClearCorners();
            m_NavMeshAgent.Warp(start);
            m_NavMeshAgent.stoppingDistance = 1;
            return m_NavMeshAgent.CalculatePath(target, path);
        }

        public bool GetWalkable(Vector3 sourcePoint, out Vector3 result)
        {
            result = sourcePoint;
            if (NavMesh.SamplePosition(sourcePoint, out NavMeshHit hit, 1, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
            return false;
        }

        public bool FindValidPointNearby(Vector3 begin, out Vector3 TargetPosition, int filter = -1)
        {
            TargetPosition = begin;
            NavMeshHit hit;
            NavMeshQueryFilter navMeshQueryFilter = new();
            navMeshQueryFilter.agentTypeID = filter;
            // 在目标层里面，基础点，固定范围，找到一个点相近的
            /*bool foundValidPoint = NavMesh.SamplePosition(begin, out hit, MaxSearchRange, navMeshQueryFilter);
            if (foundValidPoint)
            {
                //找到合法点
                TargetPosition = hit.position;
                return true;
            }
            else
            {
                Debug.Log("无法找到合法点");
            }*/

            // 在给定点附近查找最近的NavMesh边缘
            if (NavMesh.FindClosestEdge(begin, out hit, navMeshQueryFilter))
            {
                //找到合法点
                TargetPosition = hit.position;
                return true;
            }
            else
            {
                Debug.Log("无法找到最近的NavMesh边缘");
            }
            return false;
        }

        public bool FindValidPoint(Vector3 start, out Vector3 TargetPosition)
        {
            TargetPosition = start;

            NavMeshQueryFilter navMeshQueryFilter = new();
            navMeshQueryFilter.agentTypeID = -1;
            navMeshQueryFilter.areaMask = FIND_PATH_LAYER;
            if (NavMesh.SamplePosition(start, out NavMeshHit hit, PlayerFatAreaDis, navMeshQueryFilter))
            {
                TargetPosition = hit.position;
                PlayerFatAreaDis = PLAYER_FATAREA_DIS_DEF;//还原init
                return true;
            }
            else
            {
                Debug.Log("无法找到最近的NavMesh边缘");
            }
            return false;
        }

        /// <summary>
        /// 战斗用的，找点用的，所以不用分3步走肥瘦
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <param name="TargetPosition"></param>
        /// <returns></returns>
        public bool FindDistanceValidPoint(Vector3 start, float distance, out Vector3 TargetPosition)
        {

            TargetPosition = start;
            NavMeshQueryFilter navMeshQueryFilter = new();
            navMeshQueryFilter.agentTypeID = -1;
            navMeshQueryFilter.areaMask = FIND_PATH_LAYER;
            if (NavMesh.SamplePosition(start, out NavMeshHit hit, distance, navMeshQueryFilter))
            {
                TargetPosition = hit.position;
                return true;
            }
            else
            {
                Debug.Log("无法找到 {start} 附近 {distance} 范围内的有效点");
            }
            return false;
        }


        public bool FindPath(Vector3 startPos, Vector3 targetPos, out Vector3[] corners, out float distance)
        {
            distance = 99999;
            bool findResult = FindPathManager.Instance.FindPath(startPos, targetPos, 0, out corners);

            if (findResult)
            {
                distance = GetSqrDistance(startPos, corners);
            }

            return findResult;
        }

        /// <summary>
        /// 路径长度的 平方
        /// </summary>
        /// <param name="start"></param>
        /// <param name="pathPoint"></param>
        /// <returns></returns>
        public float GetSqrDistance(Vector3 start, Vector3[] pathPoint)
        {
            float distance = 0;
            var startPoint = start;
            var offsetMove = Vector3.zero;
            for (int i = 0; i < pathPoint.Length; i++)
            {
                var point = pathPoint[i];
                offsetMove = point - startPoint;
                offsetMove.y = 0;
                distance += offsetMove.sqrMagnitude;
                startPoint = point;
            }

            return distance;
        }

        public override void Release()
        {
            MonoHelper.RemoveUpdateListener(OnUpdateHandler, MonoHelper.E_ModuleType.PathFinding);
        }
    }
}