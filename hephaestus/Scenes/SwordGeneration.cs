using Godot;
using Godot.Collections;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using static Godot.WebSocketPeer;

public partial class SwordGeneration : Node3D
{
    private Button genButton;
    public Array<Vector2> SpinePositions;
    public Array<Vector2> crossSecPositions;
    private float bladeLength;
    private float bladeWidth;
    private float bladeHeight;
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
        //test parameters will be set using sliders in the future
        bladeLength = 2.0f;
        bladeHeight = 0.05f;
        bladeWidth = 0.2f;

        surfaceArray.Resize((int)Mesh.ArrayType.Max);
        List<Vector3> verts = [];
        List<Vector2> uvs = [];
        List<Vector3> normals = [];
        List<int> indices = [];

        genButton = GetNode<Button>("./SwordBladeMesh/SwordGenUi/MarginContainer/NinePatchRect/MarginContainer/buttonContainer/Button");
        genButton.Pressed += generatePressed;

        //sets 2D arrays for Spline and cross section shapes
        SpinePositions = new Array<Vector2>();
        crossSecPositions = new Array<Vector2>();

        //function called currently on applicattion start but will be set to button press
        //generatePressed();
        //generateMesh((4 /*SpinePositions, crossSecPositions*/));
        
    }

    public override void _Process(double delta)
    {
        
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
        //unimplmented function call for curved swords
        if (isSwordCurved == true)
        {
            curvedSpine();
        }

        //staight sword point generation
        if (sword == SwordType.STRSWORD)
        {
            createBladeSpine(bladeLength);
            createStraightSword2DArray(numCrossSec, bladeWidth, bladeHeight);

        }

        //Output code to showcase cross section points for each spine point
        int count = 0;
        for (int i = 0; i < 11; i++)
        {
            GD.Print(i, SpinePositions[i]);
            for (int j = 0; j < 4; j++)
            {

                GD.Print(" cross section 4 ", j, " X ", crossSecPositions[count].X, " Y ", crossSecPositions[count].Y);
                count++;
            }
        }
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

    private void curvedSpine()
    {

    }

    //creates diamond shape for cross section of sword for each spine point with paer towards tip
    private void createStraightSword2DArray(int crossSections, float width, float height)
    {
        //start 4 points of blade to be adapted to 12 later to allow tunable sharpness and fuller
        float shapeWidth = width / 2;
        float shapeHeight = width / 2;

        for (int i = 0; i < crossSections + 1; i++)
        {
            //adds 2d cross section
            if (i == crossSections)
            {
                shapeHeight = 0;
                shapeHeight = 0;
            }
            
            
            
                crossSecPositions.Add(new Vector2(0 - shapeWidth, 0));
                crossSecPositions.Add(new Vector2(0, 0 - shapeHeight));
                crossSecPositions.Add(new Vector2(0 + shapeWidth, 0));
                crossSecPositions.Add(new Vector2(0, 0 + shapeHeight));
            
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
        var thisRow = 0;
        var prevRow = 0;
        var point = 0;

        // Loop over rings.
        for (var i = 0; i < crossSections; i++)
        {
            var v = ((float)i) / crossSections;
        

            // Loop over points per cross section
            for (var j = 0; j < crossSecPoints; j++)
            {
                var u = ((float)j) / crossSecPoints;
        
                var vert = new Vector3(crossSecPositions[i * crossSecPoints + j].X, crossSecPositions[i * crossSecPoints + j].Y, SpinePositions[i].X);
                GD.Print(vert);
                verts.Add(vert);
                normals.Add(vert.Normalized());
                uvs.Add(new Vector2(u, v));
                point += 1;

                // Create triangles in ring using indices.
                if (i > 0 && j > 0)
                {
                    indices.Add(prevRow + j - 1);
                    indices.Add(prevRow + j);
                    indices.Add(thisRow + j - 1);

                    indices.Add(prevRow + j);
                    indices.Add(thisRow + j);
                    indices.Add(thisRow + j - 1);
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
            // Create mesh surface from mesh array
            // No blendshapes, lods, or compression used.
            arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
        }

        MeshInstance3D meshInstance = new MeshInstance3D();
        meshInstance.Mesh = arrMesh;
        
        AddChild(meshInstance);
    }
}   




