using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/* 
 * Code by Ed F
 * www.github.com/edf1101
 */

public class cSharpSide : MonoBehaviour
{
    [Header("Image Settings")]
    // Image Settings
    [SerializeField] private int xDivisions = 10; // divisions in x axis for spawn points
    [SerializeField] private int steps = 10; // how many times it runs the perlin field iteration

    [Header("Other Settings")]
    // Raw image Variables
    // Reference to the RenderTexture we want to send to the UI
    [SerializeField] private RenderTexture rawImageTexture;
    private RawImage rawImageReference; // refernce to UI Image


    // Screen variables
    private Vector2Int screenSize; // screen size
    private Vector2 adjustedScreenSize;
    [SerializeField] private float screenScale=1; // ratio of screen size to resolution


    // Compute shader variables
    [SerializeField] private ComputeShader myShader; // reference to shader we use
    private const int groupSize = 1; // how big the shader groups will be


   

    private void Start() // called before first frame
    {
        rawImageReference = GetComponent<RawImage>(); // get component reference

        screenSize = new Vector2Int(Screen.width, Screen.height); // get screen size
        adjustedScreenSize = new Vector2((int)(screenScale * screenSize.x), (int)(screenScale * screenSize.y));

        makeFrame();
    }


    private void makeFrame()
    {


        rawImageTexture = computeTexture(); // compute new texture 
        
        rawImageReference.texture = rawImageTexture;   // assign it  
    }

    private RenderTexture computeTexture()
    {
        // Create blank texture
        RenderTexture generateTexture = new RenderTexture((int)adjustedScreenSize.x,(int)adjustedScreenSize.y, 24);
        generateTexture.enableRandomWrite = true;
        generateTexture.Create();
        myShader.SetTexture(0, "screenTex", generateTexture); // assign it

        // Set other variables to compute shader
        myShader.SetVector("screenSize", adjustedScreenSize);
        myShader.SetInt("maxSteps", steps);

        // Create spawn points buffer
        Vector2Int[] spawnPoints = pointGeneration(xDivisions);
        ComputeBuffer spawnPointsBuffer = new ComputeBuffer(spawnPoints.Length, sizeof(int) * 2);
        spawnPointsBuffer.SetData(spawnPoints); // put data into buffer
        // put buffer in shader
        myShader.SetBuffer(0, "pointsBuffer", spawnPointsBuffer);


        // Dispatch the shader to kernel 0
        myShader.Dispatch(0,spawnPoints.Length ,1, 1);

        spawnPointsBuffer.Release(); // free the buffer from the GPU


        return generateTexture; // return generated texture
    }

    // Generates all the points evenly
    private Vector2Int[] pointGeneration(int _divisionsX)
    {
        // get adjusted size of grid
        int xSize = (int)adjustedScreenSize.x;
        int ySize = (int)adjustedScreenSize.y;

        int interval = xSize / _divisionsX; // calucalte interval

        List<Vector2Int> spawnPoints = new List<Vector2Int>();

        for(int y = 0; y < ySize; y += interval)
        {

            for(int x = 0; x < xSize; x += interval)
            {
                // Go through each spawn postion and add to the list
                spawnPoints.Add(new Vector2Int(x, y));

            }
        }


        return spawnPoints.ToArray(); // return array version
    }

}
