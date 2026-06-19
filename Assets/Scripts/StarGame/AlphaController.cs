using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class AlphaController : MonoBehaviour
{
    [Range(0, 1)]
    [SerializeField] float alpha = 1f;
    [SerializeField] SpriteRenderer[] sps;
    [SerializeField] TextMeshPro[] txs;
    void Awake()
    {
        sps = GetComponentsInChildren<SpriteRenderer>();
        txs = GetComponentsInChildren<TextMeshPro>();
    }

    void ChangeAlpha()
    {
        if (sps == null || txs == null || sps.Length == 0 || txs.Length == 0)
        {
            sps = GetComponentsInChildren<SpriteRenderer>();
            txs = GetComponentsInChildren<TextMeshPro>();
        }

        if (sps != null)
        {
            foreach (SpriteRenderer sp in sps)
            {
                sp.color = new Color(sp.color.r, sp.color.g, sp.color.b, alpha);
            }
        }
        if (txs != null)
        {
            foreach (TextMeshPro tx in txs)
            {
                tx.color = new Color(tx.color.r, tx.color.g, tx.color.b, alpha);
            }
        }
    }

    void Update()
    {
        ChangeAlpha();
    }

#if UNITY_EDITOR

    void OnValidate()
    {
        ChangeAlpha();
    }
#endif
}
