using Godot;
using Godot.Collections;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using System.Transactions;
using static Godot.WebSocketPeer;

public partial class SwordGeneration : Node3D
{
    private HSlider lengthSlider;
    private HSlider depthSlider;
    private HSlider widthSlider;
    private HSlider taperSlider;
    private HSlider fullerWSlider;
    private HSlider fullerHSlider;
    private HSlider fullerDSlider;

    //blade variables
    private float bladeLength;
    private float bladeWidth;
    private float bladeDepth;
    private float bladeTaper;
    //point v
    private float pointLength;
    //fuller variables
    private float fullerWidth;
    private float fullerDepth;
    private float fullerLength;

    private Button genButton;
    private Button clearButton;
    public Array<Vector2> SpinePositions;
    public Array<Vector2> crossSecPositions;
    
    public MeshInstance3D currMesh;

    bool isSwordCurved = false;
    public int numCrossSec = 10;

    Godot.Collections.Array surfaceArray = [];
   
    public enum SwordType
    {
        STRSWORD,
        GRTSWORD,
        KATANA,
        CUTLASS
    }

    public SwordType currSword;

    public override void _Ready()
    {

        lengthSlider = GetNode<HSlider>("SwordBladeMesh/SwordGenUi2/sidebar/MarginContainer/HBoxContainer/sliderMenu/MarginContainer/VBoxContainer/LengthSlider");
        depthSlider = GetNode<HSlider>("SwordBladeMesh/SwordGenUi2/sidebar/MarginContainer/HBoxContainer/sliderMenu/MarginContainer/VBoxContainer/DepthSlider");
        widthSlider = GetNode<HSlider>("SwordBladeMesh/SwordGenUi2/sidebar/MarginContainer/HBoxContainer/sliderMenu/MarginContainer/VBoxContainer/WidthSlider");
        taperSlider = GetNode<HSlider>("SwordBladeMesh/SwordGenUi2/sidebar/MarginContainer/HBoxContainer/sliderMenu/MarginContainer/VBoxContainer/TaperSlider");

        fullerWSlider = GetNode<HSlider>("SwordBladeMesh/SwordGenUi2/sidebar/MarginContainer/HBoxContainer/sliderMenu/MarginContainer/VBoxContainer/fullerWSlider");
        fullerHSlider = GetNode<HSlider>("SwordBladeMesh/SwordGenUi2/sidebar/MarginContainer/HBoxContainer/sliderMenu/MarginContainer/VBoxContainer/fullerLSlider");
        fullerDSlider = GetNode<HSlider>("SwordBladeMesh/SwordGenUi2/sidebar/MarginContainer/HBoxContainer/sliderMenu/MarginContainer/VBoxContainer/fullerDSlider");

        //base parameters
        bladeLength = 2.0f;
        bladeDepth = 0.1f;
        bladeWidth = 0.2f;
        bladeTaper = 0;

        //fuller params 
        fullerWidth=0;
        fullerDepth=0;
        fullerLength = 0;

        surfaceArray.Resize((int)Mesh.ArrayType.Max);
        List<Vector3> verts = [];
        List<Vector2> uvs = [];
        List<Vector3> normals = [];
        List<int> indices = [];

        genButton = GetNode<Button>("./SwordBladeMesh/SwordGenUi2/sidebar/MarginContainer/HBoxContainer/sliderMenu/MarginContainer/buttonContainer/GenerateButton");
        clearButton = GetNode<Button>("./SwordBladeMesh/SwordGenUi2/sidebar/MarginContainer/HBoxContainer/sliderMenu/MarginContainer/buttonContainer/ClearButton");
        genButton.Pressed += generatePressed;
        clearButton.Pressed += clearMeshData;

        //sets 2D arrays for Spline and cross section shapes
        SpinePositions = new Array<Vector2>();
        crossSecPositions = new Array<Vector2>();

    }

    public override void _Process(double delta)
    {
        bladeLength = (float)lengthSlider.Value;
        bladeDepth = (float)depthSlider.Value;
        bladeWidth = (float)widthSlider.Value;
        bladeTaper = (float)taperSlider.Value;

        fullerWidth = (float)fullerWSlider.Value;
        fullerDepth = (float)fullerDSlider.Value;
        fullerLength= (float)fullerHSlider.Value;

    }

    private void generatePressed()
    {
        
        GD.Print("pressed!!");
        //takes Parameters and determine sword type
        setSwordType();

        //takes sword type to generate
        GenerateSwordType(currSword);
        generateMesh((4 /*SpinePositions, crossSecPositions*/));
    }

    private void GenerateSwordType(SwordType sword)
    {
        if (GetChild<MeshInstance3D>(2)!=null) 
        {
            clearMeshData();
        }

        //unimplmented function call for curved swords
        if (isSwordCurved == true)
        {
            curvedSpine(bladeLength);
        }

        //staight sword point generation
        if (sword == SwordType.STRSWORD)
        {
            createBladeSpine(bladeLength);
            createStraightSword2DArray(numCrossSec, bladeWidth, bladeDepth, bladeTaper, fullerWidth,fullerLength, fullerDepth);

        }

        //Output code to showcase cross section points for each spine point
        //int count = 0;
        //for (int i = 0; i < 11; i++)
        //{
        //    GD.Print(i, SpinePositions[i]);
        //    for (int j = 0; j < 4; j++)
        //    {

        //        GD.Print(" cross section 4 ", j, " X ", crossSecPositions[count].X, " Y ", crossSecPositions[count].Y);
        //        count++;
        //    }
        //}
    }

    private void setSwordType()
    {
        if (isSwordCurved == true)
        {
            if (bladeWidth <= 0.1f)
            {
                currSword = SwordType.KATANA;
            }
            else
            {
                //other curved sword
            }
        }
        else if (bladeLength > 20)
        {
            currSword = SwordType.GRTSWORD;
        }
        else
        {
            currSword = SwordType.STRSWORD;
        }

    }

    //spaces points equally along spine
    private void createBladeSpine(float length)
    {
        float spacing = length / numCrossSec;
        for (int i = 0; i < 11; i++)
        {
            SpinePositions.Add(new Vector2((float)(i * spacing), 0));
        }
    }

    private void clearMeshData()
    {
        SpinePositions.Clear();
        crossSecPositions.Clear();
        currMesh.QueueFree();
    }

    private void curvedSpine(float length)
    {
        float spacing = length / numCrossSec;
        for (int i = 0; i < 11; i++)
        {
            SpinePositions.Add(new Vector2((float)(i * spacing), i));
        }
    }

    //creates diamond shape for cross section of sword for each spine point with paer towards tip
    private void createStraightSword2DArray(int crossSections, float width, float height, float taperLength, float fullerWidth, float fullerLength, float fullerDepth)
    {
        //start 4 points of blade to be adapted to 12 later to allow tunable sharpness and fuller
        float endTaper=height;

        if (taperLength > 0)
        {
            endTaper = height * ((1 - taperLength));
        }
        
        float shapeWidth = width / 2;
        float shapeHeight = height / 2;
        float currPoint = 0;
        float currentHeight = shapeHeight;

        for (int i = 0; i < crossSections + 1; i++)
        {

            if(i!=0)
            {
                 currPoint = (float) i/crossSections;
            }
                
            //lerps for taper
            currentHeight = (height + (endTaper - height) * currPoint)/2;
                
            

            //adds 2d cross section
            if (i == crossSections)
            {
                currentHeight = 0;
                shapeWidth = 0;
            }


            if (fullerLength > 0)
            {
                GD.Print("fuller?");
                crossSecPositions.Add(new Vector2(0 - shapeWidth, 0));
                crossSecPositions.Add(new Vector2(0-shapeWidth+fullerWidth/2, 0 - currentHeight));
                crossSecPositions.Add(new Vector2(0+shapeWidth- fullerWidth / 2, 0 - currentHeight));
                crossSecPositions.Add(new Vector2(0 + shapeWidth, 0));
                crossSecPositions.Add(new Vector2(0 + shapeWidth - fullerWidth / 2, 0 + currentHeight));
                crossSecPositions.Add(new Vector2(0 - shapeWidth + fullerWidth / 2,0 + currentHeight));
            }
            else
            {
                crossSecPositions.Add(new Vector2(0 - shapeWidth, 0));
                crossSecPositions.Add(new Vector2(0, 0 - currentHeight));
                crossSecPositions.Add(new Vector2(0 + shapeWidth, 0));
                crossSecPositions.Add(new Vector2(0, 0 + currentHeight));
            }

                
            
            // tapers point
            if (i >= 8)
            {
                shapeWidth = shapeWidth / 2f;
                shapeHeight = shapeHeight / 1.2f;
            }
        }
    }

 
    private void generateMesh(int crossSecPoints/*, godot_array bladeVertices, godot_array bladeIndices*/)
    {
        int crossSections = 11;
        Godot.Collections.Array surfaceArray = [];
        surfaceArray.Resize((int)Mesh.ArrayType.Max);

        // C# arrays cannot be resized or expanded, so use Lists to create geometry.
        List<Vector3> verts = [];
        List<Vector2> uvs = [];
        List<Vector3> normals = [];
        List<int> indices = [];

        // Vertex indices.
        int thisRow = 0;
        int prevRow = 0;
        int point = 0;

        // Loop over rings.
        for (int i = 0; i < crossSections; i++)
        {
            float v = ((float)i) / crossSections;
        

            // Loop over points per cross section
            for (int j = 0; j < crossSecPoints; j++)
            {
                float u = ((float)j) / crossSecPoints;
        
                Vector3 vert = new Vector3(crossSecPositions[i * crossSecPoints + j].X, crossSecPositions[i * crossSecPoints + j].Y, SpinePositions[i].X);
                GD.Print(vert);
                verts.Add(vert);
                normals.Add(vert.Normalized());
                uvs.Add(new Vector2(u, v));
                point += 1;

                //returns next i
                int nextRow = (j + 1) % crossSecPoints;
                // Create triangles in ring using indices.
                if (i > 0)
                {
                    indices.Add(prevRow + j );
                    indices.Add(thisRow + j);
                    indices.Add(prevRow + nextRow);

                    indices.Add(prevRow + nextRow);
                    indices.Add(thisRow + j);
                    indices.Add(thisRow + nextRow);

                }
              
            }
            prevRow = thisRow;
            thisRow = point;
           
        }
        
        // Convert Lists to arrays and assign to surface array
        surfaceArray[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
        surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        surfaceArray[(int)Mesh.ArrayType.Normal] = normals.ToArray();
        surfaceArray[(int)Mesh.ArrayType.Index] = indices.ToArray();

        ArrayMesh arrMesh = new ArrayMesh();

        if (arrMesh != null)
        {
            arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
        }

        MeshInstance3D meshInstance = new MeshInstance3D();
        meshInstance.Mesh = arrMesh;
        
        AddChild(meshInstance);
        currMesh = GetChild<MeshInstance3D>(2);

        
    }

    
}




