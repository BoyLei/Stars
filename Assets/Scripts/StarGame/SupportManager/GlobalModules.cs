using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalModules : SGF.Unity.MonoSingletonEx<GlobalModules>
{
    public Transform ModuleRoot;
    public Transform GlobalListener;
}
