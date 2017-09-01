using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondSquareMapGeneratorV2 : MonoBehaviour {

    //State variables;
    public float mapSize;     //For now we will force a square map. This size is how many 'unity units' wise it is.
    public float resolutionGap;     //How many units in between each height vertex. Lower value means more diamond square iterations.
                                    //I'm doing it this way so we can expand the map and keep the same 'resolution'.

    public float xPosOffset;        //Position offsets for the mesh that is ultimately generated. This way we don't always have to generate at origin.
    public float yPosOffset;
    public float zPosOffset;

    public float xAngleOffset;      //Roation offsets for resulting mesh
    public float yAngleOffset;
    public float zAngleOffset;

    public float topLeftCornerHeight;      //Allow user to manually enter in starting positions to generate overall sloped terrain!
    public float topRightCornerHeight;
    public float botLeftCornerHeight;
    public float botRightCornerHeight;

    public float baseRandomComponentValue;  //The base value for adding randomness. How up and down it can be per unit across. 
                                            //For a rand coefficient of 1, the maximum gradient will be base/1 units (rise over run)

    public int seed;                //Seed for the random generation of the map.
    public float randomCoefficient;     //Multiplier for how big the random component of height values are, between 0-1. larger number
                                        //will result in more randomness and bumpyness. 0 will be completely smooth.
    public float randomDecayCoeff;  //Coefficient to determine how the 'randomness' will decrease for each iteration, to smoothen and become
                                    //less random as the points get closer together. (this means early iterations will determine the overall
                                    //structure of the land, and later iterations will determine local, micro bumpiness).

    private int arrayLength;    //How many entries across and deep the height value matrix will have to be. Determined from map size and resolution.
    private float[,] heights;  //Will stor a 2d height value matrix, which will be used to map into the mesh later.

    //SHADER AND LIGHT FROM GUI INTERFACE
    public Shader shader;
    public DirLight sun;
    public DirLight moon;

    // Use this for initialization
    void Start () {
        Random.InitState(seed);
        //We need to calcualte how many matrix values we need. Derive from map size and desired resolution.
        //Since the user entered values may not divide evenly we will just round to the nearest whole division.
        if (mapSize <= 0) {
            mapSize = 100;      //Reset to a testing default if the user enters impossible size.
        }

        if (randomCoefficient < 0) {
            randomCoefficient = 0.8f;
        }
        if (randomDecayCoeff < 0) {
            randomDecayCoeff = 0.95f;
        }

        int temp = Mathf.RoundToInt(mapSize/resolutionGap);

        //Determine the array length. We need it to be a power of 2, +1, so that it algins properly for diamond sqaure iterations!
        arrayLength = 2; //Default power, will result in minimum resolution.
        while (2*arrayLength+1 <= temp) {
            //We can do more resolution!
            arrayLength *= 2;
        }
        arrayLength++;  //Need to increment now in order to get odd number

        //Okay, let's perform the diamond square algorithm generation now.
        generateHeightsDiamondSquare();

        //DEBUG
        debugPrintArray();

        //The heights field should be populated now. Generate the mesh itself, and then we're done!
        generateLandscapeMesh();

        //Add renderer and shader to the renderer for this mesh
        MeshRenderer renderer = this.gameObject.AddComponent<MeshRenderer>();
        renderer.material.shader = shader;
    }
	
    private void generateHeightsDiamondSquare() {
        heights = new float[arrayLength,arrayLength];

        //Set corner values.
        heights[0,0] = topLeftCornerHeight;
        heights[0, arrayLength - 1] = topRightCornerHeight;
        heights[arrayLength - 1, 0] = botLeftCornerHeight;
        heights[arrayLength - 1, arrayLength - 1] = botRightCornerHeight;

        //Okay, lets loop the diamond square strategy until we have completely filled the array!
        int span = arrayLength - 1;
        int i, j, mid;
        float randCoeff = randomCoefficient;

        while (span > 1) {
            mid = span / 2;     //SHOULD DIVIDE EVENLY SINCE SPAN IS A POWER OF TWO
            //Span has not reached zero so there is still processing to do!
            //DIAG
            for (i = 0; i + span < arrayLength; i += span) {
                for (j = 0; j + span < arrayLength; j += span) {
                    heights[i + mid, j + mid] = diagValue(i, j, span, randCoeff);
                }
            }

            //SQUARE
            //For square, i'll go line by line rather than span by span.
            for (i=0; i*mid<arrayLength; i++) {
                //If we are on a even numbered line, the empty values start one 'mid' value offset.
                //If we are on an odd numbered line, the empty values start at the edge.
                if (i%2 == 0) { j = mid; }
                else { j = 0; }

                //Now, scan across the current line and calculate the averages from adjacent values.
                //Each 'empty' square value is a spanning distance apart (horizontally) and the values to draw from are
                //one mid step up, down, left and right. Note that on the edges, the values might not exist.
                for (; j < arrayLength; j+= span) {
                    heights[i*mid, j] = squareValue(i*mid, j, span, mid, randCoeff);
                }
            }

            //Update values.
            span = mid;
            randCoeff *= randomDecayCoeff;
            //randCoeff -= randomLinearDecay;
        }
    }

    private float squareValue(int i, int j, int span, int mid, float randCoeff) {
        float val, sum = 0f;
        
        //Okay, this is inefficient since we're doing too many index bounds checks, but its the easiest way to code it for now.
        if (i+mid >= arrayLength) {
            sum += heights[i, j + mid];
            sum += heights[i, j - mid];
            sum += heights[i - mid, j];
            val = sum / 3;
        }
        else if (i - mid < 0) {
            sum += heights[i, j + mid];
            sum += heights[i, j - mid];
            sum += heights[i + mid, j];
            val = sum / 3;
        }
        else if (j + mid >= arrayLength) {
            sum += heights[i + mid, j];
            sum += heights[i, j - mid];
            sum += heights[i - mid, j];
            val = sum / 3;
        }
        else if (j - mid < 0) {
            sum += heights[i, j + mid];
            sum += heights[i + mid, j];
            sum += heights[i - mid, j];
            val = sum / 3;
        }
        else {
            sum += heights[i, j - mid];
            sum += heights[i, j + mid];
            sum += heights[i + mid, j];
            sum += heights[i - mid, j];
            val = sum / 4;
        }

        //Okay, we have calculated the average value using the sqaure source values. Now lets add the random component!
        val += Random.Range(-1f, 1f) * baseRandomComponentValue * span * randCoeff;

        return val;
    }

    private float diagValue(int i, int j, int span, float randCoeff) {
        //This function is responsible for averaging the diagonal corners and adding a random component.
        //the random cmponent will be relative to the spanning distance between points.
        //We will also multiply by the given coefficient value!
        float val, sum = 0f;

        //For diagonals, we know there will always be four availbe points to average, so no border checking is required.
        sum += heights[i, j];
        sum += heights[i, j+span];
        sum += heights[i+span, j];
        sum += heights[i+span, j+span];

        val = (sum / 4);

        //Add the random component. Should be proportional to absolute value of average and the random coeff.
        //Also proportional to the span, so that we can have more drastic change over larger spans.
        //the base variance will be 100% of the base rand value per single span step.
        //since this is the diagonal step, the 'span' multiplier is root2*span since the spanning distance is hypotenuse.
        float spacing = mapSize / (arrayLength - 1);
        val += Random.Range(-1f, 1f) * baseRandomComponentValue * Mathf.Sqrt(2) * span * spacing * randCoeff;

        return val;
    }

    private void generateLandscapeMesh() {
        //Setup mesh factors
        //        MeshFilter cubeMesh = this.gameObject.AddComponent<MeshFilter>();
        //        cubeMesh.mesh = this.createCubeMesh();

        createCubeMesh();

        // Add a MeshRenderer component. This component actually renders the mesh that
        // is defined by the MeshFilter component.
        //MeshRenderer renderer = this.gameObject.AddComponent<MeshRenderer>();
        //renderer.material.shader = Shader.Find("Unlit/SolidColourShader");
    }

    private Mesh createCubeMesh() {
        Mesh m = new Mesh();
        m.name = "LandScapeSection1";
        List<int> triangles;
        MeshFilter cubeMesh = this.gameObject.AddComponent<MeshFilter>();

        //First, let's convert our matrix of height values into a matrix of Vertex positions (Vector3's)
        Vector3[,] points = new Vector3[arrayLength,arrayLength];
        float spacing = mapSize / (arrayLength-1);
        for (int i=0; i < arrayLength; i++) {
            for (int j=0; j < arrayLength; j++) {
                //TODO: Implement rotation offsets
                points[i, j] = new Vector3(xPosOffset + (i * spacing), yPosOffset + heights[i,j], zPosOffset + (j * spacing));
            }
        }

        //Okay, now we have a list of points. Let's turn them into a list of clock wise (when viewed from above) triangles!!
        List<Vector3> vertList = new List<Vector3>();
        List<Color> vertColourList = new List<Color>();
        int[,] triangleOrdering = new int[arrayLength, arrayLength];

        //Go line by line and add each grid square as two triangles
        for (int i=0; i < arrayLength; i++) {
            for (int j = 0; j < arrayLength; j++) {
                //Triangle 1 Positions.
                vertList.Add(points[i, j]);

                //Store 2d array of indexes for triangle building later
                triangleOrdering[i, j] = vertList.Count - 1;

                //Triangle 1 Colours.
                vertColourList.Add(getColourBasedOnHeight(points[i, j]));
            }
        }
        //Convert list into an array and set it into mesh structure
        m.vertices = vertList.ToArray();
        m.colors = vertColourList.ToArray();

        triangles = new List<int>();
        for (int i = 0; i < arrayLength-1; i++) {
            for (int j=0; j < arrayLength-1; j++) {

                //Triangle 1 Positions.
                triangles.Add(triangleOrdering[i, j]);
                triangles.Add(triangleOrdering[i, j + 1]);
                triangles.Add(triangleOrdering[i + 1, j]);

                //Triangle 2 Positions.
                triangles.Add(triangleOrdering[i, j + 1]);
                triangles.Add(triangleOrdering[i + 1, j + 1]);
                triangles.Add(triangleOrdering[i + 1, j]);
            }
        }

        m.triangles = triangles.ToArray();
        cubeMesh.mesh = m;

        cubeMesh.mesh.RecalculateNormals();     //Automatically calculate the normals

        return m;
    }

    private Color getColourBasedOnHeight(Vector3 pt) {
        //If the vertex is 'below' the origin line, let's make it blue for water (??)
        if (pt.y < -25) { return new Color(0.845f,0.612f,0.349f,1.0f); }
        else if (pt.y < 30) { return new Color(0.486f,0.988f,0.0f,1.0f); }
        else if (pt.y < 110) { return Color.green; }
        else { return Color.grey; }
    }


    private void debugPrintArray() {
        for (int i = 0; i < arrayLength; i++) {
            string toLog = "";
            for (int j = 0; j < arrayLength; j++) {
                toLog += (heights[i, j] + ", ");
            }
            Debug.Log(toLog + "\n");
        }
    }

	// Update is called once per frame - we will need to update light positions for our shader's sake every frame!!
	void Update () {
        // Get renderer component (in order to pass params to shader)
        MeshRenderer renderer = this.gameObject.GetComponent<MeshRenderer>();

        // Pass updated light positions to shader
        renderer.material.SetColor("_SunLightColor", this.sun.color);
        renderer.material.SetVector("_SunLightPosition", this.sun.GetWorldPosition());
        renderer.material.SetColor("_MoonLightColor", this.moon.color);
        renderer.material.SetVector("_MoonLightPosition", this.moon.GetWorldPosition());
    }
}
