using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent (typeof(Camera))]

public class DepthTest : MonoBehaviour
{
    public Camera cam;
    public Material mat;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(cam == null)
        {
            cam = this.GetComponent<Camera>();
            cam.depthTextureMode = DepthTextureMode.Depth;
        }

        if (mat == null)
        {
            // Assign shader to Material
            mat = new Material(Shader.Find("Hidden/Depth"));
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // Render source to screen space
        Graphics.Blit(source, destination, mat);

    }
}
