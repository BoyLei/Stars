using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ui元素的渐变
/// </summary>
public class CustomGradient : BaseMeshEffect
{
    public enum DirectionType
    {
        Horizontal,
        Vertical,
    }

    [SerializeField]
    private DirectionType m_Direction = DirectionType.Vertical;
    [SerializeField]
    public Color32 m_Color1 = Color.white;
    [SerializeField]
    public Color32 m_Color2 = Color.white;

    [SerializeField]
    private float m_Range = 0f;
    [SerializeField]
    private bool m_Flip = false;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount <= 0)
        {
            return;
        }

        int count = vh.currentVertCount;
        List<UIVertex> vertices = new List<UIVertex>();
        for (int i = 0; i < count; i++)
        {
            UIVertex uIVertex = new UIVertex();
            vh.PopulateUIVertex(ref uIVertex, i);
            vertices.Add(uIVertex);
        }
        switch (m_Direction)
        {
            case DirectionType.Horizontal:
                DrawHorizontal(vh, vertices, count);
                break;
            case DirectionType.Vertical:
                DrawVertical(vh, vertices, count);
                break;
            default:
                break;
        }
    }

    private void DrawVertical(VertexHelper vh, List<UIVertex> vertices, int count)
    {
        float topY = vertices[0].position.y;
        float bottomY = vertices[0].position.y;
        for (int i = 0; i < count; i++)
        {
            float y = vertices[i].position.y;
            if (y > topY)
            {
                topY = y;
            }
            else if (y < bottomY)
            {
                bottomY = y;
            }
        }

        float height = topY - bottomY;
        for (int i = 0; i < count; i++)
        {
            UIVertex vertex = vertices[i];
            Color32 color = Color.white;
            if (m_Flip)
            {
                color = Color32.Lerp(m_Color2, m_Color1, 1 - (vertex.position.y - bottomY) / height * (1f - m_Range));
            }
            else
            {
                color = Color32.Lerp(m_Color2, m_Color1, (vertex.position.y - bottomY) / height * (1f - m_Range));
            }
            vertex.color = color;
            vh.SetUIVertex(vertex, i);
        }
    }
    private void DrawHorizontal(VertexHelper vh, List<UIVertex> vertices, int count)
    {
        float topX = vertices[0].position.x;
        float bottomX = vertices[0].position.x;
        for (int i = 0; i < count; i++)
        {
            float y = vertices[i].position.x;
            if (y > topX)
            {
                topX = y;
            }
            else if (y < bottomX)
            {
                bottomX = y;
            }
        }

        float height = topX - bottomX;
        for (int i = 0; i < count; i++)
        {
            UIVertex vertex = vertices[i];
            Color32 color = Color.white;
            if (m_Flip)
            {
                color = Color32.Lerp(m_Color1, m_Color2, 1 - (vertex.position.x - bottomX) / height * (1f - m_Range));
            }
            else
            {
                color = Color32.Lerp(m_Color1, m_Color2, (vertex.position.x - bottomX) / height * (1f - m_Range));
            }
            vertex.color = color;
            vh.SetUIVertex(vertex, i);
        }
    }
}
