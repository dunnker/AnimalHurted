using Godot;
using System;

public class BattleNode : Node
{
    public Tween Player1Tween { get { return GetNode<Tween>("Player1Tween"); } }
    public Tween Player2Tween { get { return GetNode<Tween>("Player2Tween"); } }

    public DeckNode2D Player1DeckNode2D { get { return GetNode<DeckNode2D>("Player1DeckNode2D"); } }
    public DeckNode2D Player2DeckNode2D { get { return GetNode<DeckNode2D>("Player2DeckNode2D"); } }

    public override void _Ready()
    {
        GameSingleton.Instance.Game.Player1.NewBattle();
        Player1DeckNode2D.RenderDeck(GameSingleton.Instance.Game.Player1.BattleDeck);
        GameSingleton.Instance.Game.Player2.NewBattle();
        Player2DeckNode2D.ReverseCardAreaPositions();
        Player2DeckNode2D.RenderDeck(GameSingleton.Instance.Game.Player2.BattleDeck);
    }

    public void _on_BeginBattleTimer_timeout()
    {
        Player1DeckNode2D.HideEndingCardSlots();
        Player2DeckNode2D.HideEndingCardSlots();
        PositionDecks();
    }

    public void PositionDecks()
    {
        var destination = Player1DeckNode2D.Position;
        var lastVisibleCardSlot = Player1DeckNode2D.GetEndingVisibleCardSlot();
        var lastCardSlot = Player1DeckNode2D.GetNode<CardSlotNode2D>("CardSlotNode2D_5");
        destination.x += lastCardSlot.GlobalPosition.x - lastVisibleCardSlot.GlobalPosition.x;
        Player1Tween.InterpolateProperty(Player1DeckNode2D, "position",
            Player1DeckNode2D.Position, destination, 1, Tween.TransitionType.Expo, 
            Tween.EaseType.In);
        Player1Tween.Start();

        destination = Player2DeckNode2D.Position;
        lastVisibleCardSlot = Player2DeckNode2D.GetEndingVisibleCardSlot();
        lastCardSlot = Player2DeckNode2D.GetNode<CardSlotNode2D>("CardSlotNode2D_5");
        destination.x -= lastVisibleCardSlot.GlobalPosition.x - lastCardSlot.GlobalPosition.x;
        Player2Tween.InterpolateProperty(Player2DeckNode2D, "position",
            Player2DeckNode2D.Position, destination, 1, Tween.TransitionType.Expo, 
            Tween.EaseType.In);
        Player2Tween.Start();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
    }
}
