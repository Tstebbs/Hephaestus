using Godot;
using System;

public partial class Main : Control
{
    Button startButton;
    Button quitButton;
    public override void _Ready()
    {
        startButton = GetNode<Button>("MarginContainer/VBoxContainer/buttonvContainer/start");
        startButton.Pressed += onStartPressed;
        quitButton = GetNode<Button>("MarginContainer/VBoxContainer/buttonvContainer/Quit");
        quitButton.Pressed += onQuitPressed;
    }

    private void onStartPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/testMainscene.tscn");
    }

    private void onQuitPressed()
    {
        GetTree().Quit();
    }

}
