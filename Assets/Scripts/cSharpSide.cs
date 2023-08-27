using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/* 
 * Code by Ed F
 * www.github.com/edf1101
 */

public class cSharpSide : MonoBehaviour
{
    [Header("Algorithm Settings")]
    // Image Settings
    [SerializeField] private int xDivisions = 10; // divisions in x axis for spawn points
    [SerializeField] private int steps = 10; // how many times it runs the perlin field iteration
    [SerializeField] private int stepDistance = 1; // how far each step moves
    [SerializeField] private float colourMomentum = 0.7f; // how much of the colour remains each iteration
    [SerializeField] private bool renderFirst = false; // do we render initial pixel
    private int lastXDiv;
    private Gradient lastGrad;


    [Header("Noise Settings")]
    //Noise Settings
    [SerializeField] private float noiseFreq = 0.01f;
    [SerializeField] private float zValue = 0;
    [SerializeField] private int octaves = 2;
    [SerializeField] private float persistance = 0.5f;
    [SerializeField] private float Lacunarity = 2f;

    [Header("Other Settings")]
    // Raw image Variables
    // Reference to the RenderTexture we want to send to the UI
    [SerializeField] private RenderTexture rawImageTexture;
    private RawImage rawImageReference; // refernce to UI Image
    [SerializeField] private Gradient colourGradient;
    private Vector2Int[] spawnPoints;
    private Color[] spawnColours;

    // Screen variables
    private Vector2Int screenSize; // screen size
    private Vector2 adjustedScreenSize;
    [SerializeField] private float screenScale=1; // ratio of screen size to resolution


    // Compute shader variables
    [SerializeField] private ComputeShader myShader; // reference to shader we use



   

    private void Start() // called before first frame
    {
        rawImageReference = GetComponent<RawImage>(); // get component reference

        screenSize = new Vector2Int(Screen.width, Screen.height); // get screen size
        adjustedScreenSize = new Vector2((int)(screenScale * screenSize.x), (int)(screenScale * screenSize.y));
        spawnColours = colourGeneration(xDivisions);
        spawnPoints = pointGeneration(xDivisions);


        makeFrame();
      
    }

    private void Update()
    {
       
        zValue += Time.deltaTime * 0.1f;
        makeFrame();
    }


    private void makeFrame()
    {
        // check to see if these changed
        if (!gradientsEqual(lastGrad,colourGradient))
        {
            lastGrad = copyGrad(colourGradient);
            spawnColours = colourGeneration(xDivisions);
        }

        if (lastXDiv != xDivisions)
        {
            lastXDiv = xDivisions;
            spawnPoints = pointGeneration(xDivisions);
        }

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
        myShader.SetInt("stepWeight", stepDistance);
        myShader.SetFloat("colourMomentum", colourMomentum);
        myShader.SetBool("renderFirst", renderFirst);

        // set noise settings
        myShader.SetFloat("noiseFreq", noiseFreq);
        myShader.SetFloat("zVal", zValue);
        myShader.SetFloat("persistence", persistance);
        myShader.SetFloat("lacunarity", Lacunarity);
        myShader.SetInt("octaves", octaves);

        // Create spawn points buffer
        ComputeBuffer spawnPointsBuffer = new ComputeBuffer(spawnPoints.Length, sizeof(int) * 2);
        spawnPointsBuffer.SetData(spawnPoints); // put data into buffer
        // put buffer in shader
        myShader.SetBuffer(0, "pointsBuffer", spawnPointsBuffer);

        // Create spawn colours buffer
        ComputeBuffer spawnColoursBuffer = new ComputeBuffer(spawnColours.Length, sizeof(float) * 4);
        spawnColoursBuffer.SetData(spawnColours); // put data into buffer
        // put buffer in shader
        myShader.SetBuffer(0, "coloursBuffer", spawnColoursBuffer);



        // Dispatch the shader to kernel 0
        myShader.Dispatch(0,spawnPoints.Length ,1, 1);

        spawnPointsBuffer.Release(); // free the buffer from the GPU
        spawnColoursBuffer.Release();

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

    private Color[] colourGeneration(int _divisionsX)
    {
        // get adjusted size of grid
        int xSize = (int)adjustedScreenSize.x;
        int ySize = (int)adjustedScreenSize.y;

        int interval = xSize / _divisionsX; // calucalte interval

        List<Color> spawnColours = new List<Color>();

        for (int y = 0; y < ySize; y += interval)
        {

            for (int x = 0; x < xSize; x += interval)
            {
                // Go through each spawn postion and add to the list
                spawnColours.Add(colourGradient.Evaluate(Random.value));

            }
        }


        return spawnColours.ToArray(); // return array version
    }

    // Returns if 2 gradients are equal or not bc we cant do that with == 
    private bool gradientsEqual(Gradient grad1, Gradient grad2)
    {
        // means we have to redo if a gradient is null
        if (grad2 == null || grad1 == null)
            return false;
        // simply check if their even same length
        if (grad1.colorKeys.Length != grad2.colorKeys.Length)
            return false;

        // Then check if each colour is the same
        for(int i = 0; i < grad1.colorKeys.Length; i++)
        {
            if (grad1.colorKeys[i].color != grad2.colorKeys[i].color)
                return false;
        }

        // this will get it wrong if they have same colours just diff positions
        // but not too worried about that

        return true;
    }

    // deep copy for gradients 
    private Gradient copyGrad(Gradient _inp)
    {
        Gradient retGradient = new Gradient();
        retGradient.colorKeys = _inp.colorKeys;
        retGradient.alphaKeys = _inp.alphaKeys;

        return retGradient;
    }

#if UNITY_EDITOR
    private void OnValidate() // this means if you change a slider in editor it updates
    {
        if(Application.isPlaying &&Time.time>1f)
        makeFrame();
        
    }
#endif
}
