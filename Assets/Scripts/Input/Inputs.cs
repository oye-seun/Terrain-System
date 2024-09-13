using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
//using UnityEngine.InputSystem;


public class Inputs : MonoBehaviour
{

    public static bool GetKeyDown(KeyCode key)
    {
        return Input.GetKeyDown(key);
    }

    public static bool GetMouseButton(int btn)
    {
        return Input.GetMouseButton(btn);
    }

    public static bool GetMouseDown(int btn)
    {
        return Input.GetMouseButtonDown(btn);
    }

    public static Vector3 MousePos()
    {
        return Input.mousePosition;
    }
}
