using System;
using System.IO;
using Godot;
using AnimalHurtedLib;

public class MainNode : Node
{
    public FileDialog OpenFileDialog { get { return GetNode<FileDialog>("OpenFileDialog"); } } 
    public ConfirmationDialog ConfirmationDialog { get { return GetNode<ConfirmationDialog>("ConfirmationDialog"); } }
    public CheckBox FullScreenCheckBox { get { return GetNode<CheckBox>("ConfirmationDialog/VBoxContainer/FullScreenCheckBox"); } }
    public LineEdit PlayerName1Edit { get { return GetNode<LineEdit>("ConfirmationDialog/VBoxContainer/HBoxContainer/PlayerName1Edit"); } }
    public LineEdit PlayerName2Edit { get { return GetNode<LineEdit>("ConfirmationDialog/VBoxContainer/HBoxContainer2/PlayerName2Edit"); } }
    public LineEdit AINameEdit { get { return GetNode<LineEdit>("ConfirmationDialog/VBoxContainer/HBoxContainer3/AINameEdit"); } }

    public override void _Ready()
    {
        base._Ready();
        LoadConfigValues();
    }

    public void _on_QuitButton_pressed()
    {
        GetTree().Notification(NotificationWmQuitRequest);
    }

    void NewGame()
    {
        GameSingleton.Instance.NewGame();

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

    public void _on_SettingsButton_pressed()
    {
        var configFile = new ConfigFile();
        configFile.Load(@"user://main.cfg");
        FullScreenCheckBox.Pressed = (bool)configFile.GetValue("main", "full_screen", true);
        PlayerName1Edit.Text = GameSingleton.Instance.Player1Name;
        PlayerName2Edit.Text = GameSingleton.Instance.Player2Name;
        AINameEdit.Text = GameSingleton.Instance.AIName;
        ConfirmationDialog.Show();
    }

    void LoadConfigValues()
    {
        var configFile = new ConfigFile();
        var error = configFile.Load(@"user://main.cfg");
        var fullScreen = (bool)configFile.GetValue("main", "full_screen", true);
        OS.WindowFullscreen = fullScreen;
        GameSingleton.Instance.Player1Name = (string)configFile.GetValue("main", "player1_name", "Player 1");
        GameSingleton.Instance.Player2Name = (string)configFile.GetValue("main", "player2_name", "Player 2");
        GameSingleton.Instance.AIName = (string)configFile.GetValue("main", "ai_name", "AI");
    }

    public void _on_ConfirmationDialog_confirmed()
    {
        var configFile = new ConfigFile();
        configFile.SetValue("main", "full_screen", FullScreenCheckBox.Pressed);
        configFile.SetValue("main", "player1_name", string.IsNullOrEmpty(PlayerName1Edit.Text) ? "Player 1" : PlayerName1Edit.Text);
        configFile.SetValue("main", "player2_name", string.IsNullOrEmpty(PlayerName2Edit.Text) ? "Player 2" : PlayerName2Edit.Text);
        configFile.SetValue("main", "ai_name", string.IsNullOrEmpty(AINameEdit.Text) ? "AI" : AINameEdit.Text);
        configFile.Save(@"user://main.cfg");

        LoadConfigValues();
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
