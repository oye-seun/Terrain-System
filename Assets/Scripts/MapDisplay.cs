using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MapDisplay : MonoBehaviour
{
    [SerializeField] private Renderer _textureRender;

    public void DrawNoiseMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        Texture2D texture = new Texture2D(width, height);

        Color[] colourmap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colourmap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            } 
        }
        texture.SetPixels(colourmap);
        texture.Apply();

        //_textureRender.sharedMaterial.mainTexture = texture;
        _textureRender.sharedMaterial.SetTexture("_MainTex", texture);
        //_textureRender.transform.localScale = new Vector3(width, 1, height);   
    }

}
