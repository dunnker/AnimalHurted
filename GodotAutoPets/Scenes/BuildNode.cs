//#define CHEATS_ENABLED

using System;
using System.Threading;
using Godot;
using AutoPets;

public class BuildNode : Node
{
    System.Threading.Thread _gameThread;

    public ShopNode2D Shop { get { return GetNode<ShopNode2D>("ShopNode2D"); } }

    public DeckNode2D Deck { get { return GetNode<global::DeckNode2D>("DeckNode2D"); } }

    public Label GoldLabel { get { return GetNode<Label>("PlayerAttrsNode2D/GoldLabel"); } }
    public Label LivesLabel { get { return GetNode<Label>("PlayerAttrsNode2D/LivesLabel"); } }
    public Label WinsLabel { get { return GetNode<Label>("PlayerAttrsNode2D/WinsLabel"); } }
    public Label RoundLabel { get { return GetNode<Label>("PlayerAttrsNode2D/RoundLabel"); } }
    public Label PlayerNameLabel { get { return GetNode<Label>("PlayerAttrsNode2D/PlayerNameLabel"); } } 

    public void _on_QuitGameButton_pressed()
    {
        GetTree().ChangeScene("res://Scenes/MainNode.tscn");
    }

    public void _on_RollButton_pressed()
    {
        if (GameSingleton.Instance.BuildPlayer.Gold >= Game.RollCost)
            GameSingleton.Instance.Game.Roll(GameSingleton.Instance.BuildPlayer);
        Shop.RenderShop();
    }

    public void _on_ContinueButton_pressed()
    {
        GameSingleton.Instance.BuildPlayer.GoldChangedEvent -= _GoldChangedEvent;
        if (GameSingleton.Instance.BuildPlayer == GameSingleton.Instance.Game.Player1)
        {
            GameSingleton.Instance.BuildPlayer = GameSingleton.Instance.Game.Player2;
            GetTree().ChangeScene("res://Scenes/BuildNode.tscn");
        }
        else
            GetTree().ChangeScene("res://Scenes/BattleNode.tscn");
    }

    public void _on_SellButton_pressed()
    {
        var cardSlot = Deck.GetSelectedCardSlotNode2D();
        if (cardSlot != null)
        {
            var card = GameSingleton.Instance.BuildPlayer.BuildDeck[cardSlot.CardArea2D.CardIndex];
            cardSlot.CardArea2D.RenderCard(null, card.Index);
            cardSlot.Selected = false;
            _gameThread = new System.Threading.Thread(() => 
            {
                // from here events can be invoked in DeckNode2D, which send
                // signals on main thread to render changes
                card.Sell();
            });
            _gameThread.Name = "Sell Game Thread";
            _gameThread.Start();
        }
    }

    public override void _Input(InputEvent @event)
    {
        #if CHEATS_ENABLED
        if (Input.IsActionPressed("give_gold"))
        {
            GameSingleton.Instance.BuildPlayer.Gold += 1;
        }
        #endif
    } 
    
    public override void _Ready()
    {
        GameSingleton.Instance.BuildPlayer.GoldChangedEvent += _GoldChangedEvent;
        Deck.RenderDeck(GameSingleton.Instance.BuildPlayer.BuildDeck);
        GoldLabel.Text = GameSingleton.Instance.BuildPlayer.Gold.ToString();
        LivesLabel.Text = GameSingleton.Instance.BuildPlayer.Lives.ToString();
        WinsLabel.Text = GameSingleton.Instance.BuildPlayer.Wins.ToString();
        RoundLabel.Text = GameSingleton.Instance.Game.Round.ToString();
        PlayerNameLabel.Text = GameSingleton.Instance.BuildPlayer.Name;
    }

    public void _GoldChangedEvent(object sender, int oldValue)
    {
        GoldLabel.Text = GameSingleton.Instance.BuildPlayer.Gold.ToString();
    }
}
