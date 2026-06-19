using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR

namespace CokingNodeEditor
{
    public static class CokingNodeEditorHelper
    {
        public static ConnectionPoint CheckInConnectionPoint(List<ConnectionPoint> connectionPoints, Event e)
        {
            foreach (var connectPoint in connectionPoints)
            {
                if (connectPoint.rect.Contains(e.mousePosition))
                {
                    return connectPoint;
                }
            }
            return null;
        }
    }
}
#endif
