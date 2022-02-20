using Godot;
using AutoPets;

public class MainNode : Node
{
    public void _on_QuitButton_pressed()
    {
        GetTree().Notification(NotificationWmQuitRequest);
    }
    public void _on_NewGameButton_pressed()
    {
        GameSingleton.Instance.Game = new Game();
        GameSingleton.Instance.BuildNodePlayer = GameSingleton.Instance.Game.Player1; 
        GetTree().ChangeScene("res://Scenes/BuildNode.tscn");
    }

    public override void _Ready()
    {
        
    }

    public override void _Process(float delta)
    {
        
    }
}
