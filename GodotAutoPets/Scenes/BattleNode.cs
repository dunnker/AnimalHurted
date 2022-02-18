using Godot;
using AutoPets;
using System;
using System.Threading;
using System.Threading.Tasks;

public class BattleNode : Node
{
    System.Threading.Thread _gameThread;
    Vector2 _player1DeckPosition;
    Vector2 _player2DeckPosition;

    public DeckNode2D Player1DeckNode2D { get { return GetNode<DeckNode2D>("Player1DeckNode2D"); } }
    public DeckNode2D Player2DeckNode2D { get { return GetNode<DeckNode2D>("Player2DeckNode2D"); } }
    public AudioStreamPlayer FightPlayer { get { return GetNode<AudioStreamPlayer>("FightPlayer"); } }

    [Signal]
    public delegate void FightEventSignal();

    [Signal]
    public delegate void CardHurtSignal(int index, int sourceIndex);

    [Signal]
    public delegate void FightOverSignal();

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (_gameThread != null)
            _gameThread.Abort();
        // Dispose can be called from Godot editor, and our singleton
        // may not have a Game when designing
        if (GameSingleton.Instance.Game != null)
        {
            GameSingleton.Instance.Game.FightEvent -= _game_FightEvent;
            GameSingleton.Instance.Game.CardHurtEvent -= _game_CardHurtEvent;
        }
    }

    public override void _Ready()
    {
        _player1DeckPosition = Player1DeckNode2D.Position;
        _player2DeckPosition = Player2DeckNode2D.Position;

        GameSingleton.Instance.Game.FightEvent += _game_FightEvent;
        GameSingleton.Instance.Game.CardHurtEvent += _game_CardHurtEvent;

        // using Deferred to ensure the event fires on the main thread
        Connect("FightEventSignal", this, "_signal_FightEvent", null, 
            (int)ConnectFlags.Deferred);
        Connect("CardHurtSignal", this, "_signal_CardHurt", null, 
            (int)ConnectFlags.Deferred);
        Connect("FightOverSignal", this, "_signal_FightOver", null, 
            (int)ConnectFlags.Deferred);

        GameSingleton.Instance.Game.NewBattle();

        Player1DeckNode2D.RenderDeck(GameSingleton.Instance.Game.Player1.BattleDeck);
        Player2DeckNode2D.ReverseCardAreaPositions();
        Player2DeckNode2D.RenderDeck(GameSingleton.Instance.Game.Player2.BattleDeck);
    }

    public void _on_BeginBattleTimer_timeout()
    {
        _gameThread = new System.Threading.Thread(() => 
        {
            do
            {
                GameSingleton.Instance.Game.FightOne();
            } while (!GameSingleton.Instance.Game.IsFightOver());
            GameSingleton.Instance.Game.FightOver();

			// assuming "this" is still valid. See Dispose method where thread is aborted
            this.EmitSignal("FightOverSignal");
        });
        _gameThread.Name = "Battle Game Thread";
        _gameThread.Start();
    }

    // Game thread events happening within their own thread.
    // Other nodes may get the same event, but only one event should
    // call WaitOne(), and only one spawning signal should reset the thread
    // with GameSingleton.autoResetEvent.Reset()
    public void _game_FightEvent(object sender, EventArgs e)
    {
        EmitSignal("FightEventSignal");
        GameSingleton.autoResetEvent.WaitOne();
    }

    public void _game_CardHurtEvent(object sender, Card card, Card sourceCard)
    {
        // see also DeckNode2D where its _game_CardHurtEvent handles 
        // the case where source card deck is the same as the card
        if (sourceCard.Deck != card.Deck)
        {
            DeckNode2D deck;
            DeckNode2D sourceDeck;
            if (card.Deck.Player == GameSingleton.Instance.Game.Player1)
                deck = Player1DeckNode2D;
            else
                deck = Player2DeckNode2D;
            if (sourceCard.Deck.Player == GameSingleton.Instance.Game.Player1)
                sourceDeck = Player1DeckNode2D;
            else
                sourceDeck = Player2DeckNode2D;
            EmitSignal("CardHurtSignal", deck, card.Index, sourceDeck, sourceCard.Index);
            GameSingleton.autoResetEvent.WaitOne();
        }
    }

    // signal events on main thread
    public async void _signal_FightEvent()
    {
        await PositionDecks();

        var tween1 = new Tween();
        AddChild(tween1);
        var tween2 = new Tween();
        AddChild(tween2);

        var card1 = GameSingleton.Instance.Game.Player1.BattleDeck.GetLastCard();
        var cardSlot1 = Player1DeckNode2D.GetCardSlotNode2D(card1.Index + 1);
        tween1.InterpolateProperty(cardSlot1.CardArea2D.Sprite, "rotation",
            0, // radians
            0.5, // radians; about 30 degrees 
            0.5f, Tween.TransitionType.Expo, Tween.EaseType.Out);
        tween1.Start();

        var card2 = GameSingleton.Instance.Game.Player2.BattleDeck.GetLastCard();
        var cardSlot2 = Player2DeckNode2D.GetCardSlotNode2D(card2.Index + 1);
        tween2.InterpolateProperty(cardSlot2.CardArea2D.Sprite, "rotation",
            0, // radians 
            -0.5, // radians; about -30 degrees
            0.5f, Tween.TransitionType.Expo, Tween.EaseType.Out);
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

        GameSingleton.autoResetEvent.Set();
    }

    public async void _signal_CardHurt(DeckNode2D deck, int index, DeckNode2D sourceDeck, int sourceIndex)
    {
        var cardSlot = deck.GetCardSlotNode2D(index + 1);
        var sourceCardSlot = sourceDeck.GetCardSlotNode2D(sourceIndex + 1);

        deck.WhooshPlayer.Play();

        var damageArea2DScene = (PackedScene)ResourceLoader.Load("res://Scenes/DamageArea2D.tscn");
        Area2D damageArea2D = damageArea2DScene.Instance() as Area2D;
        AddChild(damageArea2D);
        damageArea2D.GlobalPosition = sourceCardSlot.GlobalPosition;

        await DeckNode2D.ThrowArea2D(this, damageArea2D, cardSlot.GlobalPosition);

        damageArea2D.QueueFree();

        cardSlot.CardArea2D.RenderCard(deck.Deck[index], index);

        GameSingleton.autoResetEvent.Set();
    }

    public async void _signal_FightOver()
    {
        await PositionDecks();
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
                Player1DeckNode2D.Position, destination, 0.5f, Tween.TransitionType.Expo, 
                Tween.EaseType.Out);
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
                Player2DeckNode2D.Position, destination, 0.5f, Tween.TransitionType.Expo, 
                Tween.EaseType.Out);
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
