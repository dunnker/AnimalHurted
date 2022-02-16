using Godot;
using AutoPets;

public class BuildNode : Node
{
    public ShopNode2D Shop { get { return GetNode<ShopNode2D>("ShopNode2D"); } }

    public DeckNode2D Deck { get { return GetNode<global::DeckNode2D>("DeckNode2D"); } }

    public Label GoldLabel { get { return GetNode<Label>("GoldLabel"); } }
    public Label LivesLabel { get { return GetNode<Label>("LivesLabel"); } }
    public Label WinsLabel { get { return GetNode<Label>("WinsLabel"); } }
    public Label RoundLabel { get { return GetNode<Label>("RoundLabel"); } }

    public void _on_QuitGameButton_pressed()
    {
        GetTree().ChangeScene("res://Scenes/MainNode.tscn");
    }

    public void _on_RollButton_pressed()
    {
        if (GameSingleton.Instance.Game.Player1.Gold >= Game.RollCost)
            GameSingleton.Instance.Game.Roll(GameSingleton.Instance.Game.Player1);
        Shop.RenderShop();
    }

    public override void _Ready()
    {
        GameSingleton.Instance.Game.Player1.GoldChangedEvent += _GoldChangedEvent;
        Deck.RenderDeck(GameSingleton.Instance.Game.Player1.BuildDeck);
        GoldLabel.Text = GameSingleton.Instance.Game.Player1.Gold.ToString();
        LivesLabel.Text = GameSingleton.Instance.Game.Player1.Lives.ToString();
        WinsLabel.Text = GameSingleton.Instance.Game.Player1.Wins.ToString();
        RoundLabel.Text = GameSingleton.Instance.Game.Round.ToString();
    }

    public void _GoldChangedEvent(object sender, int oldValue)
    {
        GoldLabel.Text = GameSingleton.Instance.Game.Player1.Gold.ToString();
    }

    public override void _Process(float delta)
    {
    }
}
