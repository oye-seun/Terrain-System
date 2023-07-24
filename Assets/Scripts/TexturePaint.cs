using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class TexturePaint : MonoBehaviour
{
    public Texture2D Tex;
    [Range(0, 255f)]
    public int Red;
    [Range(0, 255f)]
    public int Green;
    [Range(0, 255f)]
    public int Blue;
    [Range(0, 100f)]
    public float Size;
    [Range(0, 1f)]
    public float Opacity;
    public string path;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    //void Update()
    //{
    //    //if (Mouse.current.leftButton.isPressed && Tex != null) 
    //    if (Keyboard.current.bKey.isPressed && Tex != null) 
    //    {
    //        Debug.Log("B pressed");
    //        RaycastHit hit;
    //        if (UnityEditor.SceneView.lastActiveSceneView.camera != null && Physics.Raycast(UnityEditor.SceneView.lastActiveSceneView.camera.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit))
    //        {

    //            //Renderer rend = hit.transform.GetComponent<Renderer>();
    //            //MeshCollider meshCollider = hit.collider as MeshCollider;

    //            //if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
    //            //    return;

    //            //Texture2D tex = rend.material.mainTexture as Texture2D;
    //            Vector2 pixelUV = hit.textureCoord;
    //            pixelUV.x *= Tex.width;
    //            pixelUV.y *= Tex.height;

    //            Tex.SetPixel((int)pixelUV.x, (int)pixelUV.y, Color.green);
    //            Tex.Apply();
    //            Debug.Log("texture drawn");
    //        }
    //    }

    //    //Debug.Log("no key pressed");
    //}


    public void Paint(RaycastHit hit)
    {
        //Debug.Log("paint");

        //Renderer rend = hit.transform.GetComponent<Renderer>();
        //MeshCollider meshCollider = hit.collider as MeshCollider;

        //if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
        //    return;

        //Debug.Log("hit texture coord: " + hit.textureCoord);

        //Texture2D tex = rend.material.mainTexture as Texture2D;
        Vector2Int pixelUV = new Vector2Int( (int)(hit.textureCoord.x * Tex.width), (int)(hit.textureCoord.y * Tex.height));
        //Debug.Log("calculated texture coord: " + pixelUV);

        int minx = (int)(pixelUV.x - Size);
        int miny = (int)(pixelUV.y - Size);
        int diff = pixelUV.x - minx;
        diff = (diff * 2) + 1;


        Vector2Int[] affPixels = new Vector2Int[diff * diff];
        for(int j = 0; j < diff; j++)
        {
            for(int i = 0; i < diff; i++)
            {
                affPixels[(j * diff) + i] = new Vector2Int(minx + i, miny + j);
                //Debug.Log("uv: " + affPixels[(j * diff) + i]);
            }
        }

        Color col = new Color(Red / 255f, Green / 255f, Blue / 255f);
        foreach(Vector2Int uv in affPixels)
        {
            float dist = Vector2Int.Distance(uv, pixelUV);
            //float pow = Mathf.InverseLerp(Size, 0, dist);
            float pow = 1 - Mathf.InverseLerp(0, Size, dist);
            Color newcol = Color.Lerp(Tex.GetPixel(uv.x, uv.y), col, col.a * pow * Opacity);
            Tex.SetPixel(uv.x, uv.y, newcol);
        }


        //Tex.SetPixel(pixelUV.x, pixelUV.y, new Color((float)Red/255, (float)Green/255, (float)Blue/255));
        Tex.Apply();
    }

    public void SaveTexture()
    {
        byte[] bytes = Tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
    }


    //public override bool RequiresConstantRepaint()
    //{
    //    return true;
    //}

    //void OnDrawGizmos()
    //{
    //    // Your gizmo drawing thing goes here if required...

    //#if UNITY_EDITOR
    //    // Ensure continuous Update calls.
    //    if (!Application.isPlaying)
    //    {
    //        UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
    //        UnityEditor.SceneView.RepaintAll();
    //    }
    //#endif
    //}
}



#if UNITY_EDITOR
[CustomEditor(typeof(TexturePaint))]
public class TexturePaintEditor : Editor
{
    bool hotControlReturned;
    void OnSceneGUI()
    {
        //Event e = Event.current;

        // We use hotControl to lock focus onto the editor (to prevent deselection)
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        //Debug.Log("control ID: " + controlID);
        //Debug.Log("hotControl: " + GUIUtility.hotControl);

        bool bpressed = Inputs.GetKeyDown(KeyCode.B);
        if (bpressed)
        {
            hotControlReturned = false;
            Debug.Log("B pressed");
            GUIUtility.hotControl = controlID;
        }
        else if(!hotControlReturned)
        {
            GUIUtility.hotControl = 0;
            hotControlReturned = true;
        }

        if (Event.current.type == EventType.MouseDrag && bpressed)
        {
            //Debug.Log("Mouse Dragging");
            //Debug.Log("Mouse Pos: " + Event.current.mousePosition);
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            //Debug.Log("ray origin: " + ray.origin + " ray dir: " + ray.direction);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                //Debug.Log("hit object");
                TexturePaint TexPaint = (TexturePaint)target;
                TexPaint.Paint(hit);
            }

            Event.current.Use();
        }
        else if(Event.current.type == EventType.MouseUp)
        {
            //Debug.Log("Mouse up");
            GUIUtility.hotControl = 0;
            Event.current.Use();
        }

        //switch (Event.current.type)
        //{
        //    //case EventType.MouseDown:
        //    //    canPaint = true;
        //    //    Debug.Log("Mouse Down!");
        //    //    GUIUtility.hotControl = controlID;
        //    //    Event.current.Use();
        //    //    break;

        //    case EventType.MouseDrag:
                
        //        Debug.Log("Mouse Dragging");
        //        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        //        RaycastHit hit;

        //        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        //        {
        //            TexturePaint TexPaint = (TexturePaint)target;
        //            TexPaint.Paint(hit);
        //        }

        //        Event.current.Use();
        //        break;

        //    case EventType.MouseUp:
        //        Debug.Log("Mouse up");
        //        canPaint = false;
        //        GUIUtility.hotControl = 0;
        //        Event.current.Use();
        //        break;
            

        //}
    }

    public override void OnInspectorGUI()
    {
        TexturePaint TP = (TexturePaint)target;
        base.OnInspectorGUI();


        if (GUILayout.Button("Save Texture"))
        {
            TP.SaveTexture();
        }
    }
}
#endif