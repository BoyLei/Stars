using UnityEngine;

public class BgPosFollower : MonoBehaviour
{
    public Vector2 orgSize;
    public Vector2 orgLocalPos;
    public RectTransform bgRect;

    void Start()
    {
        var rat = new Vector2(bgRect.rect.width / orgSize.x, bgRect.rect.height / orgSize.y);
        var newPos = orgLocalPos * rat;
        transform.localPosition = new Vector3(newPos.x, newPos.y, 0);
    }
}
