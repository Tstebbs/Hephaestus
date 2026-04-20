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
	public Array<Vector2> handlecrossSecPositions;

    public MeshInstance3D handleMesh;
    public MeshInstance3D currMesh;
	public Material bladeMaterial;
    public Material gripMaterial;

    private PackedScene handguard1Scene;
    private PackedScene handguard2Scene;
    private PackedScene handguard3Scene;

    Node3D handguard1;
    Node3D handguard2;
	Node3D handguard3;

    private PackedScene pommel1Scene;
    private PackedScene pommel2Scene;

    Node3D pommel1;
    Node3D pommel2;
    Node3D pommel3;

	Panel statMenu;
    Label seedLabel;
	Label weightLabel;
	Label slashLabel;
	Label pierceLabel;
	Label chopLabel;
	Label guardLabel;
	public float bladeWeightVal;
    float dmg1 = 100;
    float dmg2 = 100;
    float dmg3 = 100;

    bool isSwordCurved = false;
	public int numCrossSec = 10;

	Godot.Collections.Array surfaceArray = [];   
	public enum SwordType
	{
		STRSWORD,
		GRTSWORD,
		KATANA,
		RAPIER
	}

	public SwordType currSword;

	public override void _Ready()
	{
		handguard1Scene= ResourceLoader.Load<PackedScene>("res://Scenes/handguard1.tscn");
        handguard2Scene = ResourceLoader.Load<PackedScene>("res://Scenes/handguard2.tscn");
        handguard3Scene = ResourceLoader.Load<PackedScene>("res://Scenes/handguard3.tscn");

        pommel1Scene = ResourceLoader.Load<PackedScene>("res://Scenes/pommel1.tscn");
        pommel2Scene = ResourceLoader.Load<PackedScene>("res://Scenes/pommel2.tscn");

        bladeMaterial = GD.Load<Material>("res://Resources/materials/metal1/mat.tres");
        gripMaterial = GD.Load<Material>("res://Resources/materials/grip/leatherGrip.tres");

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
		handlecrossSecPositions = new Array<Vector2>();

        statMenu = GetNode<Panel>("SwordBladeMesh/SwordGenUi2/statMenu");
        //stats labels
       
		seedLabel = GetNode<Label>("SwordBladeMesh/SwordGenUi2/statMenu/MarginContainer/statshbox/statscontainer/seed");
		weightLabel= GetNode<Label>("SwordBladeMesh/SwordGenUi2/statMenu/MarginContainer/statshbox/statscontainer/weight");
        slashLabel = GetNode<Label>("SwordBladeMesh/SwordGenUi2/statMenu/MarginContainer/statshbox/statscontainer/slash");
        pierceLabel = GetNode<Label>("SwordBladeMesh/SwordGenUi2/statMenu/MarginContainer/statshbox/statscontainer/pierce");
        chopLabel = GetNode<Label>("SwordBladeMesh/SwordGenUi2/statMenu/MarginContainer/statshbox/statscontainer/chop");
        guardLabel = GetNode<Label>("SwordBladeMesh/SwordGenUi2/statMenu/MarginContainer/statshbox/statscontainer/guard");


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
		else if (edges==2)
		{
            if (depthSlider.Value > 0.2)
            {
                depthSlider.Value = 0.2f;
            }
        }
		else if(edges==0)
		{
			if(depthSlider.Value>0.14)
			{
				depthSlider.Value = 0.14f;
			}
			widthSlider.Value= 0;
		}

		
	}

	private void generatePressed()
	{
        if (GetChild<MeshInstance3D>(2) != null)
        {
            clearMeshData();
        }
        //clearMeshData();
		setSwordType();
		GenerateSwordType(currSword);

		generateMesh(numofPointsPerCs, false, currSword);

		string currseed=generateSeed(edges, bladeLength, bladeDepth, bladeWidth, bladeTaper, fullerWidth);

		updateDamageValues(currSword, bladeDepth, bladeWidth, bladeLength, bladeTaper, fullerWidth, bladeWeightVal);
		updateStats(currseed, bladeWeightVal, dmg1, dmg2, dmg3);
        statMenu.Visible = true;

	}

	private string generateSeed(float edges, float bladeLength, float bladeDepth, float bladeWidth, float bladeTaper,float fullerWidth)
	{
		string seed = edges.ToString()+bladeLength.ToString()+bladeDepth.ToString()+bladeWidth.ToString()+bladeTaper.ToString()+fullerWidth.ToString();
		return seed;
	}

	private void updateStats(string seed,float weight, float dmg1, float dmg2, float dmg3)
	{
		seedLabel.Text = seed;
		weightLabel.Text=weight.ToString();
		slashLabel.Text=dmg1.ToString();
		pierceLabel.Text=dmg2.ToString();
		chopLabel.Text=dmg3.ToString();
		guardLabel.Text = "qw";

		
	}

	private void updateDamageValues(SwordType sword, float bladedepth, float bladeWidth, float bladelength, float bladeTaper, float fullerWidth, float weight)
	{
		
        if (sword == SwordType.GRTSWORD)
		{
			
			dmg1 = 60;
			dmg2 = 40;
			dmg3 = 100;
		}
		else if (sword == SwordType.STRSWORD)
		{
            dmg1 = 100;
            dmg2 = 50;
            dmg3 = 40;
        }
		else
		{
            dmg1 = 20;
            dmg2 = 100;
            dmg3 = 10;
        }

        dmg1 =dmg1* ((float)( (fullerWidth / 0.8) + (bladelength / 2)) / 2);
		
		dmg2 = dmg2 * ((float)( (bladeTaper / 0.9)+(bladeWidth/0.02)) / 2);

		dmg3 = dmg3 * ((float)((weight / 90)+(bladedepth/0.25))/2);

    }


	private void GenerateSwordType(SwordType sword)
	{
		//staight sword point generation
				if (sword == SwordType.STRSWORD)
				{
					createBladeSpine(bladeLength);
					createStraightSword2DArray(numCrossSec, bladeWidth, bladeDepth, bladeTaper, fullerWidth, fullerLength, fullerDepth);
					bladeWeightVal = bladeWeightT(sword, bladeDepth, bladeWidth, bladeLength, bladeTaper);
					addHandleParts(sword, bladeWidth, bladeDepth);
					createHandle(10, sword, 10f);
					generateMesh(12, true, sword);


				}

				if (sword == SwordType.GRTSWORD)
				{
					createBladeSpine(bladeLength);

					createStraightSword2DArray(numCrossSec, bladeWidth, bladeDepth, bladeTaper, fullerWidth, fullerLength, fullerDepth);
					bladeWeightVal = bladeWeightT(sword, bladeDepth, bladeWidth, bladeLength, bladeTaper);
					addHandleParts(sword, bladeWidth, bladeDepth);
					createHandle(10, sword, 10);
					generateMesh(12, true, sword);

				}

				if (sword == SwordType.RAPIER)
				{
					createBladeSpine(bladeLength / 2);
					createThrustingSword(numCrossSec, bladeDepth / 2, bladeTaper);
					bladeWeightVal = bladeWeightT(sword, bladeDepth / 2, bladeDepth / 2, bladeLength / 2, bladeTaper);
					addHandleParts(sword, bladeWidth / 2, bladeDepth / 2);
					createHandle(10, sword, 10);
					generateMesh(12, true, sword);
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
		handlecrossSecPositions.Clear();
		currMesh.QueueFree();
		handleMesh.QueueFree();

		if(currSword==SwordType.STRSWORD)
		{
            GetNodeOrNull<Node3D>("Handguard1").QueueFree();
            GetNodeOrNull<Node3D>("Pommel2").QueueFree();
        }
		else if(currSword==SwordType.GRTSWORD)
		{
            GetNodeOrNull<Node3D>("Handguard2").QueueFree();
            GetNodeOrNull<Node3D>("Pommel2").QueueFree();
        }
		else if(currSword==SwordType.RAPIER)
		{
            GetNodeOrNull<Node3D>("Handguard3").QueueFree();
            GetNodeOrNull<Node3D>("Pommel1").QueueFree();
        }
        statMenu.Visible = false;
    }

	private void curvedSpine(float length)
	{
		float spacing = length / numCrossSec;
		for (int i = 0; i < 11; i++)
		{
			SpinePositions.Add(new Vector2((float)(i * spacing), i));
		}
	}

	private float bladeWeightT(SwordType swordType, float bladedepth ,float bladeWidth ,float bladelength, float bladeTaper)
	{
		float totalVolume = 0;

        if (swordType == SwordType.STRSWORD || swordType == SwordType.GRTSWORD)
		{
			float averageDepth = ((bladedepth + bladedepth * (1-bladeTaper))/ 2);
            totalVolume = averageDepth* bladeWidth * bladeLength;
	
        }
		else
		{
			double averageArea = (bladedepth * bladedepth * 3.14) + ((bladedepth * (1 - bladeTaper)) * (bladedepth * (1 - bladeTaper)) * 3.14);
			averageArea = averageArea / 2;
			totalVolume = (float)(averageArea * bladelength);

        }

		return (totalVolume * 7850);
	}


	private void addHandleParts(SwordType swordType, float bladewidth, float bladedepth)
	{
		if(swordType==SwordType.STRSWORD)
		{
            handguard1 = handguard1Scene.Instantiate<Node3D>();
			pommel3 = pommel2Scene.Instantiate<Node3D>();

            AddChild(handguard1);
			AddChild(pommel3);
            float bladewPercent = 0.6f * ((float)((bladewidth - 0.005) / (0.08 - 0.005)));
            float bladedPercent = 0.4f * ((float)((bladedepth - 0.1) / (0.2 - 0.1)));

            handguard1.Scale = new Vector3(0.4f + bladewPercent, 0.6f + bladedPercent, 1.0f);
			pommel3.Translate(new Vector3(0, -0.65f, 0));
			//pommel2.Scale = new Vector3(0.4f + bladewPercent, 0.6f + bladedPercent, 1.0f);

        }
		else if(swordType == SwordType.GRTSWORD)
		{
            handguard2 = handguard2Scene.Instantiate<Node3D>();
            pommel2 = pommel2Scene.Instantiate<Node3D>();

            handguard2.RotateX(-(float)1.571);
            AddChild(handguard2);
            AddChild(pommel2);
            float bladewPercent = 0.2f*((float) ((bladewidth - 0.045) / (0.08 - 0.045)));
            float bladedPercent = 0.1f * ((float)((bladedepth - 0.18) / (0.3 - 0.18)));
           
            handguard2.Scale = new Vector3(0.8f+bladewPercent, 0.9f+bladedPercent, 1.0f);
            pommel2.Translate(new Vector3(0, -0.95f, 0));
        }
		else if(swordType==SwordType.RAPIER)
		{
            handguard3 = handguard3Scene.Instantiate<Node3D>();
            pommel1 = pommel1Scene.Instantiate<Node3D>();
            handguard3.RotateX(-(float)1.571);
            AddChild(handguard3);
            AddChild(pommel1);
            float bladeDiamPercent = 0.35f * ((float)(((bladedepth-0.04) - 0.01) / (0.03 - 0.01)));
            

            handguard3.Scale = new Vector3(0.65f + bladeDiamPercent, 0.65f + bladeDiamPercent, 0.8f);
			pommel1.Translate(new Vector3(0, (float)-0.143, 0));
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


            if (fullerWidth > 0)
			{
				
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
                numofPointsPerCs = 4;
                crossSecPositions.Add(new Vector2(0 - shapeWidth, 0));
				crossSecPositions.Add(new Vector2(0, 0 - currentHeight));
				crossSecPositions.Add(new Vector2(0 + shapeWidth, 0));
				crossSecPositions.Add(new Vector2(0, 0 + currentHeight));
			}

		}
	}

    private void createThrustingSword(int crossSections,  float height, float taperLength)
    {
		
       
		//start 4 points of blade to be adapted to 12 later to allow tunable sharpness and fuller
		height = height - 0.04f;
        float endTaper = height;

        if (taperLength > 0)
        {
            endTaper = height * ((1 - taperLength));
        }

       // float shapeWidth = width / 2;
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
                //shapeWidth = 0;
            }

            if (i >= 8)
            {
                //shapeWidth = shapeWidth / 2f;
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


    private void createHandle(int crossSections, SwordType sword, float weight)
    {
		float height = 0.125f;
		float currPoint = 0;
		float currentHeight = height;

		for (int i = 0; i < crossSections + 1; i++)
		{

			if (i != 0)
			{
				currPoint = (float)i / crossSections;
			}

			if (SwordType.GRTSWORD == sword)
			{
                double scaleFactor = 0.004;
                if (i > 5)
                {
                    scaleFactor = scaleFactor * 0.8;
                }
                height = (float)(0.012f + (i *scaleFactor));
            }
			else if (SwordType.STRSWORD == sword)
			{
				float length = ((float)i / (crossSections - 1)) * 0.3f;
                height = (float)(0.5 * Math.Sin(1.55*length + 1.38) - 0.47);
			}
			else
			{
                float length = ((float)i / (crossSections - 1)) * 0.15f;
                height = (float)(0.5 * Math.Sin(2 * length + 1.47) - 0.465);
               
            }

			currentHeight = height;
			

			for (int l = 0; l < 12; l++)
			{
				double angle = (l * 30) * Math.PI / 180;
				handlecrossSecPositions.Add(new Vector2((float)(0 + (currentHeight * Math.Cos(angle))), (float)(0 + ((currentHeight/**0.8*/) * Math.Sin(angle)))));

			}


		}
	}



	private void generateMesh(int crossSecPoints,bool ishandle, SwordType sword)
	{
		int crossSections = 11;
		Godot.Collections.Array surfaceArray = [];
		surfaceArray.Resize((int)Mesh.ArrayType.Max);

		List<Vector3> verts = [];
		List<Vector2> uvs = [];
		List<Vector3> normals = [];
		List<int> indices = [];

		float handlelength=0;
		float handlePos=0.0f;

        // Vertex indices.
        int point = 0;

		// Loop over rings.
		if (ishandle)
		{
			if (sword == SwordType.GRTSWORD)
			{
				handlelength = 0.4f;
				handlePos = handlelength;
			}
			else if (sword == SwordType.STRSWORD)
			{
				handlelength = 0.25f;
				handlePos = handlelength;
			}
			else
			{
				handlelength = 0.15f;
				handlePos = handlelength-0.012f;
			}
				
            for (int i = 0; i < crossSections; i++)
            {
                float v = ((float)i) / crossSections;

                float z = ((float)i / (crossSections - 1)) * handlelength;
                // Loop over points per cross section
                for (int j = 0; j < crossSecPoints; j++)
                {
                    float u = ((float)j) / crossSecPoints;
                   
                    Vector3 vert = new Vector3(handlecrossSecPositions[i * crossSecPoints + j].X, handlecrossSecPositions[i * crossSecPoints + j].Y, z);

                    verts.Add(vert);
                    normals.Add(Vector3.Zero);
                    
                    uvs.Add(new Vector2(u, v));
                    point += 1;

                }
            }
        }
		else
		{
			for (int i = 0; i < crossSections; i++)
			{
				float v = ((float)i) / crossSections;
				float u;
                // Loop over points per cross section
                for (int j = 0; j < crossSecPoints; j++)
				{
					u = ((float)j) / crossSecPoints;

					Vector3 vert = new Vector3(crossSecPositions[i * crossSecPoints + j].X, crossSecPositions[i * crossSecPoints + j].Y, SpinePositions[i].X);

					verts.Add(vert);
					normals.Add(Vector3.Zero);

					uvs.Add(new Vector2(u, v));
					point += 1;

				}
				//u = 0.0f;
				//uvs.Add(new Vector2(u, v));

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
		
		meshInstance.RotateX(-(float)1.571);
		AddChild(meshInstance);
		if (ishandle)
		{
            meshInstance.SetSurfaceOverrideMaterial(0, gripMaterial);
            meshInstance.Translate(new Vector3(0, 0, -handlePos));
			handleMesh = meshInstance;
		}
		else
		{
            meshInstance.SetSurfaceOverrideMaterial(0, bladeMaterial);
            currMesh = meshInstance;
        }
	
		
	}

	
}
