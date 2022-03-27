using Godot;
using System;

public class WinnerNode : Node
{
    public Label WonLabel { get { return GetNode<Label>("WonLabel"); } }

    public override void _Ready()
    {
        GameSingleton.Instance.GameOverShown = true;
        if (GameSingleton.Instance.Game.Player1.Lives == 0)
            WonLabel.Text = "Player 2 won the game!";
        else
            WonLabel.Text = "Player 1 won the game!";
    }

    public void _on_ColorRect_gui_input(InputEvent @event)
    {
        if (@event is InputEventMouseButton)
        {
            var mouseEvent = @event as InputEventMouseButton;
            // mouse down
            if (mouseEvent.ButtonIndex == (int)ButtonList.Left && mouseEvent.Pressed)
            {
                GameSingleton.Instance.RestoreBattleDecks();
                GetTree().ChangeScene("res://Scenes/BattleNode.tscn");
            }
        }
    }
}
