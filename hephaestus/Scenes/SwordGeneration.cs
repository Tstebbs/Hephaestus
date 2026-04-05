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
    private HSlider edgesSlider;

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
	private float edges;

	private int numofPointsPerCs=4;
	private Button genButton;
	private Button clearButton;
	public Array<Vector2> SpinePositions;
	public Array<Vector2> crossSecPositions;
	
	public MeshInstance3D currMesh;
	public Material bladeMaterial;

    private PackedScene handguard1Scene;
    private PackedScene handguard2Scene;

    Node3D handguard1;
    Node3D handguard2;

    bool isSwordCurved = false;
	public int numCrossSec = 10;

	Godot.Collections.Array surfaceArray = [];   
	public enum SwordType
	{
		STRSWORD,
		GRTSWORD,
		KATANA,
		CUTLASS,
		RAPIER
	}

	public SwordType currSword;

	public override void _Ready()
	{
		handguard1Scene= ResourceLoader.Load<PackedScene>("res://Scenes/handguard1.tscn");
        handguard2Scene = ResourceLoader.Load<PackedScene>("res://Scenes/handguard2.tscn");
        bladeMaterial = GD.Load<Material>("res://Resources/materials/metal1/mat.tres");


        lengthSlider = GetNode<HSlider>("SwordBladeMesh/SwordGenUi2/sidebar/MarginContainer/HBoxContainer/sliderMenu/MarginContainer/VBoxContainer/LengthSlider");
		depthSlider = GetNode<HSlider>("SwordBladeMesh/SwordGenUi2/sidebar/MarginContainer/HBoxContainer/sliderMenu/MarginContainer/VBoxContainer/DepthSlider");
		widthSlider = GetNode<HSlider>("SwordBladeMesh/SwordGenUi2/sidebar/MarginContainer/HBoxContainer/sliderMenu/MarginContainer/VBoxContainer/WidthSlider");
		taperSlider = GetNode<HSlider>("SwordBladeMesh/SwordGenUi2/sidebar/MarginContainer/HBoxContainer/sliderMenu/MarginContainer/VBoxContainer/TaperSlider");

		fullerWSlider = GetNode<HSlider>("SwordBladeMesh/SwordGenUi2/sidebar/MarginContainer/HBoxContainer/sliderMenu/MarginContainer/VBoxContainer/fullerWSlider");
		fullerHSlider = GetNode<HSlider>("SwordBladeMesh/SwordGenUi2/sidebar/MarginContainer/HBoxContainer/sliderMenu/MarginContainer/VBoxContainer/fullerLSlider");
		fullerDSlider = GetNode<HSlider>("SwordBladeMesh/SwordGenUi2/sidebar/MarginContainer/HBoxContainer/sliderMenu/MarginContainer/VBoxContainer/fullerDSlider");

		edgesSlider = GetNode<HSlider>("SwordBladeMesh/SwordGenUi2/sidebar/MarginContainer/HBoxContainer/sliderMenu/MarginContainer/VBoxContainer/edgesSlider");

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

		edges=(float)edgesSlider.Value;

		if(edges==2 && bladeLength>1.4)
		{
			if (depthSlider.Value < 0.18)
			{
				depthSlider.Value = 0.18f;
			}

			if(widthSlider.Value <0.045)
			{
				widthSlider.Value = 0.045;
			}

			if (taperSlider.Value>0.5)
			{
				taperSlider.Value = 0.5;
			}
		}

	}

	private void generatePressed()
	{
		
		GD.Print("pressed!!");
		//takes Parameters and determine sword type
		setSwordType();

		//takes sword type to generate
		GenerateSwordType(currSword);
        GD.Print(currSword);
        generateMesh(numofPointsPerCs,currSword);
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
            addHandguard(sword, bladeWidth, bladeDepth);

        }

		if (sword == SwordType.GRTSWORD)
        {
            createBladeSpine(bladeLength);
            
            createStraightSword2DArray(numCrossSec, bladeWidth, bladeDepth, bladeTaper, fullerWidth, fullerLength, fullerDepth);
            addHandguard(sword, bladeWidth, bladeDepth);

        }

        if (sword == SwordType.RAPIER)
        {
            createBladeSpine(bladeLength);
            //createStraightSword2DArray(numCrossSec, bladeWidth, bladeDepth, bladeTaper, fullerWidth, fullerLength, fullerDepth);
            createThrustingSword(numCrossSec, bladeWidth, bladeDepth, bladeTaper);
			addHandguard(sword,bladeWidth,bladeDepth);
        }

    }

	private void setSwordType()
	{
		if(edges==0)
		{
			currSword=SwordType.RAPIER;
		}
		else if(edges==1) 
		{

            //if (bladeWidth <= 0.1f)
            //	//{
            //	//	currSword = SwordType.KATANA;
            //	//}
            //	//else
            //	//{
            //	//	//other curved sword
            //	//}
        }
        else
		{
			if(bladeLength<1.4)
			{
                currSword = SwordType.STRSWORD;
            }
			else
			{
				currSword = SwordType.GRTSWORD;
			}
			
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

        
       
		if(currSword==SwordType.STRSWORD)
		{
            GetNodeOrNull<Node3D>("Handguard1").QueueFree();
        }
		else if(currSword==SwordType.GRTSWORD)
		{
            GetNodeOrNull<Node3D>("Handguard2").QueueFree();
        }
       

        
    }

	private void curvedSpine(float length)
	{
		float spacing = length / numCrossSec;
		for (int i = 0; i < 11; i++)
		{
			SpinePositions.Add(new Vector2((float)(i * spacing), i));
		}
	}

	private void addHandguard(SwordType swordType, float bladewidth, float bladedepth)
	{
		if(swordType==SwordType.STRSWORD)
		{
            handguard1 = handguard1Scene.Instantiate<Node3D>();
            AddChild(handguard1);
			
        }
		else if(swordType == SwordType.GRTSWORD)
		{
            handguard2 = handguard2Scene.Instantiate<Node3D>();
            AddChild(handguard2);
			float bladewPercent = 0.2f*((float) ((bladewidth - 0.045) / (0.08 - 0.045)));
            float bladedPercent = 0.1f * ((float)((bladedepth - 0.18) / (0.3 - 0.18)));
           
            handguard2.Scale = new Vector3(0.8f+bladewPercent, 0.9f+bladedPercent, 1.0f);
        }
		else if(swordType==SwordType.RAPIER)
		{

		}
	}

    //creates diamond shape for cross section of sword for each spine point with paer towards tip
    //numCrossSec, bladeWidth, bladeDepth, bladeTaper, fullerWidth,fullerLength, fullerDepth
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
		float fWidth = currentHeight*fullerWidth;
		

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

            if (i >= 8)
            {
                shapeWidth = shapeWidth / 2f;
                shapeHeight = shapeHeight / 1.2f;
				fWidth = fWidth / 2;
            }


            if (fullerLength > 0)
			{
				GD.Print("fuller?");
                numofPointsPerCs = 6;
              
				crossSecPositions.Add(new Vector2(0 - (shapeWidth), 0 - currentHeight +fWidth));
				crossSecPositions.Add(new Vector2(0, 0 - currentHeight));
				crossSecPositions.Add(new Vector2(0 + (shapeWidth), 0 - currentHeight +fWidth));

				crossSecPositions.Add(new Vector2(0 + (shapeWidth), 0 + currentHeight - fWidth));
				crossSecPositions.Add(new Vector2(0, 0 + currentHeight));
				crossSecPositions.Add(new Vector2(0 - (shapeWidth), 0 + currentHeight - fWidth));


			}
            else
			{
                numofPointsPerCs = 6;
                crossSecPositions.Add(new Vector2(0 - shapeWidth, 0));
				crossSecPositions.Add(new Vector2(0, 0 - currentHeight));
				crossSecPositions.Add(new Vector2(0 + shapeWidth, 0));
				crossSecPositions.Add(new Vector2(0, 0 + currentHeight));
			}

				
			
			// tapers point
			
		}
	}

    private void createThrustingSword(int crossSections, float width, float height, float taperLength)
    {
        //start 4 points of blade to be adapted to 12 later to allow tunable sharpness and fuller
        float endTaper = height;

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

            if (i != 0)
            {
                currPoint = (float)i / crossSections;
            }

            //lerps for taper
            currentHeight = (height + (endTaper - height) * currPoint) / 2;

            //adds 2d cross section
            if (i == crossSections)
            {
                currentHeight = 0;
                shapeWidth = 0;
            }

            if (i >= 8)
            {
                shapeWidth = shapeWidth / 2f;
                shapeHeight = shapeHeight / 1.2f;
            }

            numofPointsPerCs = 12;

			for(int k=0;k< numofPointsPerCs;k++)
			{
				double angle = (k * 30) * Math.PI / 180;
                crossSecPositions.Add(new Vector2((float)(0 + (currentHeight*Math.Cos(angle))),(float)( 0+(currentHeight * Math.Sin(angle)))));
            }

           



        }
    }

	private void generateVerts(int crossSecPoints/*, godot_array bladeVertices, godot_array bladeIndices*/)
	{

	}

    private void generateMesh(int crossSecPoints, SwordType swordType/*, godot_array bladeVertices, godot_array bladeIndices*/)
	{
		int crossSections = 11;
		Godot.Collections.Array surfaceArray = [];
		surfaceArray.Resize((int)Mesh.ArrayType.Max);

		List<Vector3> verts = [];
		List<Vector2> uvs = [];
		List<Vector3> normals = [];
		List<int> indices = [];

		// Vertex indices.
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

                verts.Add(vert);
                normals.Add(Vector3.Zero);
                Vector3 center = new Vector3(SpinePositions[i].X, SpinePositions[i].Y, 0);


                uvs.Add(new Vector2(u, v));
                point += 1;

            }
        }

		
		// Loop over rings.
		for (int i = 0; i < crossSections; i++)
		{
            int prevRow = (i - 1) * crossSecPoints;
            int thisRow = i * crossSecPoints;

            // Loop over points per cross section
            for (int j = 0; j < crossSecPoints; j++)
			{
				
				//returns next i
				int nextRow = (j + 1) % crossSecPoints;
				// Create triangles in ring using indices.
				if (i > 0)
				{
                    indices.Add(prevRow + j);
					indices.Add(thisRow + j);
					indices.Add(prevRow + nextRow);
					
					Vector3 n1 = (verts[thisRow + j] - verts[prevRow + j]).Cross(verts[prevRow + nextRow] - verts[prevRow + j]);
					
					normals[prevRow + j] += n1; 
					normals[thisRow + j] += n1; 
					normals[prevRow + nextRow] += n1;

					indices.Add(prevRow + nextRow);
					indices.Add(thisRow + j);
					indices.Add(thisRow + nextRow);

					Vector3 n2 = (verts[thisRow + j] - verts[prevRow + nextRow]).Cross(verts[thisRow + nextRow] - verts[prevRow + nextRow]); 
					
					normals[prevRow + nextRow] += n2;
					normals[thisRow + j] += n2;
					normals[thisRow + nextRow] += n2;

                }
			  
			}
		
		}


        for (int n= 0; n < normals.Count; n++)
		{
			normals[n]= normals[n].Normalized();
		}
		// Convert Lists to arrays and assign to surface array
		surfaceArray[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
		surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
		//surfaceArray[(int)Mesh.ArrayType.Normal] = normals.ToArray();
		surfaceArray[(int)Mesh.ArrayType.Index] = indices.ToArray();

		ArrayMesh arrMesh = new ArrayMesh();
        
        if (arrMesh != null)
		{
			arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
			
		}

		
		MeshInstance3D meshInstance = new MeshInstance3D();
		meshInstance.Mesh = arrMesh;
		meshInstance.SetSurfaceOverrideMaterial(0,bladeMaterial);
		AddChild(meshInstance);
		currMesh =meshInstance;
		
		
	}

	
}
