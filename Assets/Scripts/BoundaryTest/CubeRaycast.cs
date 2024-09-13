using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class CubeRaycast : MonoBehaviour
{
    public LayerMask layerMask;
    public LayerMask planelayerMask;
    private Camera _cam;
    private bool _selectedState;
    private Transform _cubeTransform;

    // Start is called before the first frame update
    void Start()
    {
        _cam = FindObjectOfType<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (/*Mouse.current.leftButton.isPressed*/Inputs.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Inputs.MousePos());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, layerMask)) 
            {
                _cubeTransform = hit.transform;
                _selectedState = true;
            }
        }
        else
        {
            _selectedState = false;
        }

        if (_selectedState)
        {
            Ray planeRay = Camera.main.ScreenPointToRay(Inputs.MousePos());
            RaycastHit planeHit;
            if (Physics.Raycast(planeRay, out planeHit, planelayerMask))
            {
                _cubeTransform.position = new Vector3(planeHit.point.x, 0, planeHit.point.z);
            }
        }
    }
}
