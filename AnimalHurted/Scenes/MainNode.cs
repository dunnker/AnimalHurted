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

    void NewGame()
    {
        GameSingleton.Instance.Game = new Game();
        GameSingleton.Instance.Game.NewGame();
        GameSingleton.Instance.BuildNodePlayer = GameSingleton.Instance.Game.Player1; 
        GetTree().ChangeScene("res://Scenes/BuildNode.tscn");
    }

    public void _on_NewGameButton_pressed()
    {
        GameSingleton.Instance.VersusAI = false;
        NewGame();
    }

    public void _on_NewAIGameButton_pressed()
    {
        GameSingleton.Instance.VersusAI = true;
        NewGame();
    }

    public void _on_ReplayButton_pressed()
    {
        OpenFileDialog.PopupCentered();
    }

    public void _on_SandboxButton_pressed()
    {
        GameSingleton.Instance.Game = new Game();
        GameSingleton.Instance.Game.NewGame();
        GetTree().ChangeScene("res://Scenes/SandboxNode.tscn");
    }

    public void _on_OpenFileDialog_file_selected(Godot.Path @string)
    {
        using (FileStream fileStream = new FileStream(ProjectSettings.GlobalizePath(OpenFileDialog.CurrentPath), FileMode.Open))
        {
            using (BinaryReader reader = new BinaryReader(fileStream))
            {
                GameSingleton.Instance.Game = new Game();
                GameSingleton.Instance.Game.NewGame();
                Deck deck = new Deck(GameSingleton.Instance.Game.Player1, Game.BuildDeckSlots);
                deck.LoadFromStream(reader);
                // load into build deck so sandbox will have it after replaying battle
                deck.CloneTo(GameSingleton.Instance.Game.Player1.BuildDeck);
                deck = new Deck(GameSingleton.Instance.Game.Player2, Game.BuildDeckSlots);
                deck.LoadFromStream(reader);
                deck.CloneTo(GameSingleton.Instance.Game.Player2.BuildDeck);
                GameSingleton.Instance.Game.Player1.NewBattleDeck();
                GameSingleton.Instance.Game.Player2.NewBattleDeck();
            }
        }
        GameSingleton.Instance.SaveBattleDecks();
        GameSingleton.Instance.FightResult = GameSingleton.Instance.Game.CreateFightResult();
        GameSingleton.Instance.RestoreBattleDecks();
        GameSingleton.Instance.Sandboxing = true;
        GetTree().ChangeScene("res://Scenes/BattleNode.tscn");
    }
}
