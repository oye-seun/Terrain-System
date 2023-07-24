using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

[ExecuteInEditMode]
public class PlanarReflectionCamControl : MonoBehaviour
{
    public Transform plane;
    public Camera thisCam;
    public Camera targetCam;
    public RenderTexture RenderTexture;
    public Vector3 scale = new Vector3(1,1,1);
    // Start is called before the first frame update
    //void OnEnable()
    //{
    //    thisCam.ResetWorldToCameraMatrix();
    //    thisCam.ResetProjectionMatrix();
    //    thisCam.projectionMatrix = thisCam.projectionMatrix * Matrix4x4.Scale(scale);
    //}

    //// Update is called once per frame
    //void Update()
    //{
    //    Vector3 probeFwd = Vector3.Reflect(targetCam.transform.forward, plane.forward);
    //    transform.position = new Vector3(targetCam.transform.position.x, -targetCam.transform.position.y, targetCam.transform.position.z);
    //    transform.forward = probeFwd;
    //}

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += UpdateCamera;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= UpdateCamera;
    }

    void UpdateCamera(ScriptableRenderContext Src, Camera cam)
    {
        if ((cam.cameraType == CameraType.Game || cam.cameraType == CameraType.SceneView) && cam.tag != "ReflectionCam")
        {
            //thisCam.targetTexture.width = cam.targetTexture.width;
            //thisCam.targetTexture.height = cam.targetTexture.height;

            //Matrix4x4 mat = cam.projectionMatrix;
            //mat *= Matrix4x4.Scale(new Vector3(1, -1, 1));
            //thisCam.projectionMatrix = mat;

            thisCam.ResetWorldToCameraMatrix();
            thisCam.ResetProjectionMatrix();
            thisCam.projectionMatrix = thisCam.projectionMatrix * Matrix4x4.Scale(scale);
            thisCam.aspect = cam.aspect;
            //thisCam.projectionMatrix = cam.projectionMatrix * Matrix4x4.Scale(new Vector3(1, -1, 1));

            //Vector4 clipPlane = new Vector4(0, 1, 0, plane.position.y);
            //clipPlane = thisCam.worldToCameraMatrix * clipPlane;

            Plane p = new Plane(Vector3.up, new Vector3(0, plane.position.y, 0));
            Vector4 clipPlane = new Vector4(p.normal.x, p.normal.y, p.normal.z, p.distance);
            clipPlane = Matrix4x4.Transpose(Matrix4x4.Inverse(thisCam.worldToCameraMatrix)) * clipPlane;
            //clipPlane = Matrix4x4.Transpose(thisCam.cameraToWorldMatrix) * clipPlane;

            //Vector4 clipPlane = CameraSpacePlane(thisCam, plane.position, Vector3.up, 1.0f);

            Matrix4x4 ClipMatrix = thisCam.CalculateObliqueMatrix(clipPlane);
            thisCam.projectionMatrix = ClipMatrix;

            //Vector3 probeFwd = Vector3.Reflect(cam.transform.forward, plane.forward);
            Vector3 probeFwd = Vector3.Reflect(cam.transform.forward, Vector3.up);
            float camDist = Vector3.Distance(cam.transform.position, plane.position);

            //Vector3 probePos = cam.transform.position;
            //probePos.y = -probePos.y;
            //transform.position = plane.position + (-probeFwd * camDist);
            transform.position = new Vector3(cam.transform.position.x, plane.position.y + (plane.position.y - cam.transform.position.y), cam.transform.position.z);
            transform.forward = probeFwd;
            //transform.up = -cam.transform.up;
            //transform.rotation = Quaternion.LookRotation(probeFwd, -cam.transform.up);
        }
    }

    static Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        Vector3 offsetPos = pos + normal * 0.07f;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 point = m.inverse.MultiplyPoint(new Vector3(0.0f, 0.0f, 0.0f));
        cpos -= new Vector3(0.0f, point.y, 0.0f);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

}
