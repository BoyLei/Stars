using UnityEngine;

public class Footprints : MonoBehaviour
{
    public float delta = 1;
    public float gap = 0.5f;
    public bool ShowFootsteps = true;

    private ParticleSystem[] pss = new ParticleSystem[0];
    private Vector3 lastEmit;
    private int dir = 1;

    private void Awake()
    {
        pss = GetComponentsInChildren<ParticleSystem>();
    }


    private void Start()
    {
        lastEmit = transform.position;
    }

    private void Update()
    {
        if (!ShowFootsteps)
        {
            return;
        }

        if (Vector3.Distance(lastEmit, transform.position) > delta)
        {
            var pos = transform.position + (transform.right * gap * dir);
            dir *= -1;
            ParticleSystem.EmitParams ep = new();
            ep.position = pos;
            ep.rotation = transform.rotation.eulerAngles.y;

            foreach (var p in pss)
            {
                p.Emit(ep, 1);
            }

            //system.Emit(ep, 1);
            lastEmit = transform.position;
        }
    }
}
