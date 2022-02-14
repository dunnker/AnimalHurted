using Godot;
using AutoPets;

public class Main : Node
{
    public void _on_QuitButton_pressed()
    {
        GetTree().Notification(NotificationWmQuitRequest);
    }
    public void _on_NewGameButton_pressed()
    {
        GameSingleton.Instance.Game = new Game();
        GetTree().ChangeScene("res://Scenes/Build.tscn");
    }

    public override void _Ready()
    {
        /*Game game = new Game();
        Card antCard = new Card(game.Player1.BuildDeck, AbilityList.Instance.AntAbility);
        antCard.Summon(0);
        Card cricketCard = new Card(game.Player2.BuildDeck, AbilityList.Instance.CricketAbility);
        cricketCard.Summon(0);
        game.NewBattle();*/
        //(FindNode("PopupPanel") as PopupPanel).Popup_();
    }

    public override void _Process(float delta)
    {
        
    }
}
