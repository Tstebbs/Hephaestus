using Godot;
using Godot.Collections;
using Godot.NativeInterop;
using System;
using static Godot.WebSocketPeer;

public partial class SwordGeneration : Node3D
{
    private Button genButton;
    public Array<Vector2> SpinePositions;
    private float bladeLength;
    private float bladeWidth;
    private float bladeHeight;
    bool isSwordCurved=false;
    public float numCrossSec = 10;

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
        bladeLength = 2.0f;
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
        createBladeSpine(bladeLength);
        if(isSwordCurved==true)
        {
            curvedSpine();
        }

        for (int i = 0; i < 11; i++)
        {
           GD.Print(i,SpinePositions[i]);
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

    private void CreateBladeType1()
    {

    }
}



