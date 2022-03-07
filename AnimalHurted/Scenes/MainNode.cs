using System;
using System.IO;
using Godot;
using AnimalHurtedLib;

public class MainNode : Node
{
    public FileDialog OpenFileDialog { get { return GetNode<FileDialog>("OpenFileDialog"); } } 

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

    public void _on_ReplayButton_pressed()
    {
        OpenFileDialog.PopupCentered();
    }

    public void _on_SandboxButton_pressed()
    {
        GameSingleton.Instance.Game = new Game();
        GetTree().ChangeScene("res://Scenes/SandboxNode.tscn");
    }

    public void _on_OpenFileDialog_file_selected(Godot.Path @string)
    {
        using (FileStream fileStream = new FileStream(ProjectSettings.GlobalizePath(OpenFileDialog.CurrentPath), FileMode.Open))
        {
            using (BinaryReader reader = new BinaryReader(fileStream))
            {
                GameSingleton.Instance.Game = new Game();
                Deck deck = new Deck(GameSingleton.Instance.Game.Player1, Game.BuildDeckSlots);
                deck.LoadFromStream(reader);
                deck.CloneTo(GameSingleton.Instance.Game.Player1.BattleDeck);
                deck = new Deck(GameSingleton.Instance.Game.Player2, Game.BuildDeckSlots);
                deck.LoadFromStream(reader);
                deck.CloneTo(GameSingleton.Instance.Game.Player2.BattleDeck);
            }
        }
        GameSingleton.Instance.SaveBattleDecks();
        GameSingleton.Instance.FightResult = GameSingleton.Instance.Game.CreateFightResult();
        GameSingleton.Instance.RestoreBattleDecks();
        GetTree().ChangeScene("res://Scenes/BattleNode.tscn");
    }

    public override void _Ready()
    {
        
    }

    public override void _Process(float delta)
    {
        
    }
}
