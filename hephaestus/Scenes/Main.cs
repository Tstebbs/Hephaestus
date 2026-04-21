using Godot;
using System;

public partial class Main : Control
{
    Button startButton;
    Button start2Button;
    Button quitButton;
    Button infoButton;
    Panel panel;
    public override void _Ready()
    {
        startButton = GetNode<Button>("TextureRect2/buttonvContainer/start");
        startButton.Pressed += onStartPressed;

        start2Button = GetNode<Button>("TextureRect2/buttonvContainer/start2");
        start2Button.Pressed += onStart2Pressed;
       
        quitButton = GetNode<Button>("TextureRect2/buttonvContainer/Quit");
        quitButton.Pressed += onQuitPressed;

        infoButton = GetNode<Button>("TextureRect2/buttonvContainer/Info");
        infoButton.Pressed += onInfoPressed;
        panel = GetNode<Panel>("Panel");
    }

    private void onStartPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/MainBlacksmith.tscn");
    }

    private void onStart2Pressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/testMainscene.tscn");
    }

    private void onQuitPressed()
    {
        GetTree().Quit();
    }

    private void onInfoPressed()
    {
        panel.Visible =! panel.Visible;
    }


}
