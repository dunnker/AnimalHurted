using Godot;
using System;

public class Card : Area2D
{
    public void _on_Area2D_mouse_entered()
    {
        (FindNode("SelectedSprite") as Sprite).Show();
    }

    public void _on_Area2D_mouse_exited()
    {
        (FindNode("SelectedSprite") as Sprite).Hide();
    }

    public override void _Ready()
    {
        
    }

    public override void _Process(float delta)
    {
        
    }
}
