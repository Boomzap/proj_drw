using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevDebug : MonoBehaviour
{
    public bool showDebug = true;

    public static bool showDebugLogs = true;
    public static void Log(object log)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD

        if (showDebugLogs == false) return;

        Debug.Log(log);
#endif
    }

    private void Awake()
    {
        showDebugLogs = showDebug;
    }

}
