using Godot;
using Godot.Collections;
using Godot.NativeInterop;
using System;
using static Godot.WebSocketPeer;

public partial class SwordGeneration : Node3D
{
    private Button genButton;
    public Array<Vector2> SpinePositions;
    public Array<Vector2> crossSecPositions;
    private float bladeLength;
    private float bladeWidth;
    private float bladeHeight;
    bool isSwordCurved=false;
    public int numCrossSec = 10;

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
        //test parameters
        bladeLength = 2.0f;
        bladeHeight = 0.05f;
        bladeWidth = 0.2f;
        //genButton = GetChild<Button>(1);
        //genButton.Pressed += GeneratePressed;
        SpinePositions = new Array<Vector2>();
        generatePressed();
      
    }

    public override void _Process(double delta)
    {
       
    }

    private void generatePressed()
    {
        setSwordType();
        GenerateSwordType(currSword);
    }

    private void GenerateSwordType(SwordType sword)
    {
        //createBladeSpine(bladeLength);

        if(isSwordCurved==true)
        {
            curvedSpine();
        }

        if (sword == SwordType.STRSWORD)
        {
            createBladeSpine(bladeLength);
            createStraightSword2DArray(numCrossSec,bladeWidth,bladeHeight);

        }

        for (int i = 0; i < 11; i++)
        {
           GD.Print(i,SpinePositions[i]);
            GD.Print(crossSecPositions[i].X, crossSecPositions[i].Y);
        }
    }

    private void setSwordType()
    {
        if (isSwordCurved == true)
        {
            if(bladeWidth<=0.1f)
            {
                currSword = SwordType.KATANA; 
            }
            else
            {
                //other curved sword
            }
        }
        else if(bladeLength>2)
        {
            currSword = SwordType.GRTSWORD;
        }
        else
        {

        }

    }

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

    private void  createStraightSword2DArray(int crossSection, float width, float height)
    {
        for (int i = 0; i < crossSection+1; i++)
        {
            crossSecPositions.Add(new Vector2(width, height));
            if(i>=8)
            {
                width = width / 2f;
                height = height / 1.2f;
            }
        }
    }
}



