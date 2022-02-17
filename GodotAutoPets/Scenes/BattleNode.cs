using Godot;
using AutoPets;
using System;
using System.Threading;
using System.Threading.Tasks;

public class BattleNode : Node
{
    Vector2 _player1DeckPosition;
    Vector2 _player2DeckPosition;

    static AutoResetEvent autoResetEvent = new AutoResetEvent(false);

    public DeckNode2D Player1DeckNode2D { get { return GetNode<DeckNode2D>("Player1DeckNode2D"); } }
    public DeckNode2D Player2DeckNode2D { get { return GetNode<DeckNode2D>("Player2DeckNode2D"); } }
    public AudioStreamPlayer FightPlayer { get { return GetNode<AudioStreamPlayer>("FightPlayer"); } }

    [Signal]
    public delegate void FightEventSignal();

    [Signal]
    public delegate void FightOverSignal();

    [Signal]
    public delegate void CardFaintedSignal(DeckNode2D deck, int index);

    public override void _Ready()
    {
        _player1DeckPosition = Player1DeckNode2D.Position;
        _player2DeckPosition = Player2DeckNode2D.Position;

        // using Deferred to ensure the event fires on the main thread
        Connect("FightEventSignal", this, "_signal_FightEvent", null, 
            (int)ConnectFlags.Deferred);
        Connect("FightOverSignal", this, "_signal_FightOver", null, 
            (int)ConnectFlags.Deferred);
        Connect("CardFaintedSignal", this, "_signal_CardFainted", null, 
            (int)ConnectFlags.Deferred);

        GameSingleton.Instance.Game.NewBattle();
        Player1DeckNode2D.RenderDeck(GameSingleton.Instance.Game.Player1.BattleDeck);
        Player2DeckNode2D.ReverseCardAreaPositions();
        Player2DeckNode2D.RenderDeck(GameSingleton.Instance.Game.Player2.BattleDeck);
    }

    public async void _on_BeginBattleTimer_timeout()
    {
        await PositionDecks();

        GameSingleton.Instance.Game.FightEvent += _game_FightEvent;
        GameSingleton.Instance.Game.Player1.CardFaintedEvent += _player_CardFaintedEvent;
        GameSingleton.Instance.Game.Player2.CardFaintedEvent += _player_CardFaintedEvent;
        var thread = new System.Threading.Thread(() => BattleNode.ThreadProc(this));
        thread.Start();
    }

    static void ThreadProc(BattleNode node) 
    {
        do
        {
            GameSingleton.Instance.Game.FightOne();
        } while (!GameSingleton.Instance.Game.IsFightOver());
        GameSingleton.Instance.Game.FightOver();

        //TODO assuming node is still valid, e.g. user hasn't exited the battle screen
        // before fight is finished. If we give the user a Close button or Cancel button
        // we'll have to terminate this thread before closing
        node.EmitSignal("FightOverSignal");
    }

    // thread events
    public void _game_FightEvent(object sender, EventArgs e)
    {
        EmitSignal("FightEventSignal");
        autoResetEvent.WaitOne();
    }

    public void _player_CardFaintedEvent(object sender, Card card, int index)
    {
        DeckNode2D deck;
        if (card.Deck.Player == GameSingleton.Instance.Game.Player1)
            deck = Player1DeckNode2D;
        else
            deck = Player2DeckNode2D;
        EmitSignal("CardFaintedSignal", deck, index);
        autoResetEvent.WaitOne();
    }

    // signal events on main thread
    public async void _signal_FightEvent()
    {
        var tween1 = new Tween();
        AddChild(tween1);
        var tween2 = new Tween();
        AddChild(tween2);

        var card1 = GameSingleton.Instance.Game.Player1.BattleDeck.GetLastCard();
        var cardSlot1 = Player1DeckNode2D.GetCardSlotNode2D(card1.Index + 1);
        tween1.InterpolateProperty(cardSlot1.CardArea2D.Sprite, "rotation",
            0, 0.5, 0.5f, Tween.TransitionType.Expo, Tween.EaseType.Out);
        tween1.Start();

        var card2 = GameSingleton.Instance.Game.Player2.BattleDeck.GetLastCard();
        var cardSlot2 = Player2DeckNode2D.GetCardSlotNode2D(card2.Index + 1);
        tween2.InterpolateProperty(cardSlot2.CardArea2D.Sprite, "rotation",
            0, -0.5, 0.5f, Tween.TransitionType.Expo, Tween.EaseType.Out);
        tween2.Start();

        await ToSignal(tween1, "tween_all_completed");

        tween1.QueueFree();
        tween2.QueueFree();

        cardSlot1.CardArea2D.Sprite.Rotation = 0;
        cardSlot2.CardArea2D.Sprite.Rotation = 0;

        cardSlot2.CardArea2D.RenderCard(card2, card2.Index);
        cardSlot1.CardArea2D.RenderCard(card1, card1.Index);

        //TODO: if a knockout, play additional knockout sound clip
        FightPlayer.Play();

        autoResetEvent.Set();
    }

    public async void _signal_CardFainted(DeckNode2D deck, int index)
    {
        var tween = new Tween();
        AddChild(tween);

        var cardSlot = deck.GetCardSlotNode2D(index + 1);
        tween.InterpolateProperty(cardSlot.CardArea2D.Sprite, "modulate:a",
            1.0, 0.0, 0.5f, Tween.TransitionType.Linear, Tween.EaseType.In);
        tween.Start();

        await ToSignal(tween, "tween_all_completed");

        tween.QueueFree();

        cardSlot.CardArea2D.RenderCard(null, index);

        await PositionDecks();

        autoResetEvent.Set();
    }

    public void _signal_FightOver()
    {
        GameSingleton.Instance.Game.FightEvent -= _game_FightEvent;
        GameSingleton.Instance.Game.Player1.CardFaintedEvent -= _player_CardFaintedEvent;
        GameSingleton.Instance.Game.Player2.CardFaintedEvent -= _player_CardFaintedEvent;
    }

    public async Task PositionDecks()
    {
        Player1DeckNode2D.HideEndingCardSlots();
        Player2DeckNode2D.HideEndingCardSlots();

        var tween1 = new Tween();
        AddChild(tween1);
        var tween2 = new Tween();
        AddChild(tween2);

        var lastVisibleCardSlot = Player1DeckNode2D.GetEndingVisibleCardSlot();
        Tween awaitTween = null;
        if (lastVisibleCardSlot != null)
        {
            var destination = _player1DeckPosition;
            var lastCardSlot = Player1DeckNode2D.GetCardSlotNode2D(5);
            destination.x += lastCardSlot.GlobalPosition.x - lastVisibleCardSlot.GlobalPosition.x;
            tween1.InterpolateProperty(Player1DeckNode2D, "position",
                Player1DeckNode2D.Position, destination, 0.05f, Tween.TransitionType.Expo, 
                Tween.EaseType.In);
            tween1.Start();
            awaitTween = tween1;
        }

        lastVisibleCardSlot = Player2DeckNode2D.GetEndingVisibleCardSlot();
        if (lastVisibleCardSlot != null)
        {
            var destination = _player2DeckPosition;
            var lastCardSlot = Player2DeckNode2D.GetCardSlotNode2D(5);
            destination.x -= lastVisibleCardSlot.GlobalPosition.x - lastCardSlot.GlobalPosition.x;
            tween2.InterpolateProperty(Player2DeckNode2D, "position",
                Player2DeckNode2D.Position, destination, 0.05f, Tween.TransitionType.Expo, 
                Tween.EaseType.In);
            tween2.Start();
            if (awaitTween == null)
                awaitTween = tween2;
        }

        if (awaitTween != null)
            await ToSignal(awaitTween, "tween_all_completed");

        tween1.QueueFree();
        tween2.QueueFree();
    }
}
