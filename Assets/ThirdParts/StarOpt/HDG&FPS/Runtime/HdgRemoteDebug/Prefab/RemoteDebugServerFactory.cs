using UnityEngine;
using System;
using System.Reflection;

public class RemoteDebugServerFactory : MonoBehaviour
{
    public void Awake()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        Type type = null;

        for (var i = 0; i < assemblies.Length; i++)
        {
            var asm = assemblies[i];
            type = asm.GetType("Hdg.RemoteDebugServer");
            if (type != null)
                break;
        }

        if (type == null)
            return;

        var server = FindObjectOfType(type);
        if (server == null)
        {
            // If there is no server in the scene, then create one.
            if (type != null)
                gameObject.AddComponent(type);
        }
        else
        {
            // Otherwise destroy ourselves because we aren't needed.
            Destroy(gameObject);
        }
    }
}
