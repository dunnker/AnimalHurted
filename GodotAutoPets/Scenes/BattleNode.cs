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

    [Signal]
    public delegate void CardSummonedSignal(DeckNode2D deck, int index);

    [Signal]
    public delegate void CardBuffedSignal(DeckNode2D deck, int index, int sourceIndex);

    [Signal]
    public delegate void CardHurtSignal();

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
        Connect("CardSummonedSignal", this, "_signal_CardSummoned", null, 
            (int)ConnectFlags.Deferred);
        Connect("CardBuffedSignal", this, "_signal_CardBuffed", null, 
            (int)ConnectFlags.Deferred);
        Connect("CardHurtSignal", this, "_signal_CardHurt", null, 
            (int)ConnectFlags.Deferred);

        GameSingleton.Instance.Game.NewBattle();
        Player1DeckNode2D.RenderDeck(GameSingleton.Instance.Game.Player1.BattleDeck);
        Player2DeckNode2D.ReverseCardAreaPositions();
        Player2DeckNode2D.RenderDeck(GameSingleton.Instance.Game.Player2.BattleDeck);
    }

    public void _on_BeginBattleTimer_timeout()
    {
        GameSingleton.Instance.Game.FightEvent += _game_FightEvent;
        GameSingleton.Instance.Game.Player1.CardFaintedEvent += _player_CardFaintedEvent;
        GameSingleton.Instance.Game.Player2.CardFaintedEvent += _player_CardFaintedEvent;
        GameSingleton.Instance.Game.Player1.CardSummonedEvent += _player_CardSummonedEvent;
        GameSingleton.Instance.Game.Player2.CardSummonedEvent += _player_CardSummonedEvent;
        GameSingleton.Instance.Game.Player1.CardBuffedEvent += _player_CardBuffedEvent;
        GameSingleton.Instance.Game.Player2.CardBuffedEvent += _player_CardBuffedEvent;
        GameSingleton.Instance.Game.CardHurtEvent += _game_CardHurtEvent;
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

    public void _game_CardHurtEvent(object sender, Card card, Card sourceCard)
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

    public void _player_CardSummonedEvent(object sender, Card card, int index)
    {
        DeckNode2D deck;
        if (card.Deck.Player == GameSingleton.Instance.Game.Player1)
            deck = Player1DeckNode2D;
        else
            deck = Player2DeckNode2D;
        EmitSignal("CardSummonedSignal", deck, card.Index);
        autoResetEvent.WaitOne();
    }

    public void _player_CardBuffedEvent(object sender, Card card, int sourceIndex)
    {
        DeckNode2D deck;
        if (card.Deck.Player == GameSingleton.Instance.Game.Player1)
            deck = Player1DeckNode2D;
        else
            deck = Player2DeckNode2D;
        EmitSignal("CardBuffedSignal", deck, card.Index, sourceIndex);
        autoResetEvent.WaitOne();
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

        // restore modulate, even though we're about to hide the sprite
        // next time something spawns we want modulate to have its restored value
        var color = cardSlot.CardArea2D.Sprite.Modulate;
        cardSlot.CardArea2D.Sprite.Modulate = new Color(color.r, color.g,
            color.b, 1);
        cardSlot.CardArea2D.RenderCard(null, index);

        autoResetEvent.Set();
    }

    public async void _signal_CardSummoned(DeckNode2D deck, int index)
    {
        var cardSlot = deck.GetCardSlotNode2D(index + 1);
        if (!cardSlot.Visible)
        {
            cardSlot.Show();
            await PositionDecks();
        }

        cardSlot.CardArea2D.RenderCard(deck.Deck[index], index);

        autoResetEvent.Set();
    }

    public async Task ThrowArea2D(Area2D area2D, Vector2 toPosition)
    {
        var tweenPosX = new Tween();
        AddChild(tweenPosX);
        var tweenPosY_Up = new Tween();
        AddChild(tweenPosY_Up);
        var tweenPosY_Down = new Tween();
        AddChild(tweenPosY_Down);
        var tweenRotate = new Tween();
        AddChild(tweenRotate);

        float buffSpeed = 0.6f;
        int arcY = 150;

        tweenPosX.InterpolateProperty(area2D, "position:x",
            area2D.GlobalPosition.x, toPosition.x, buffSpeed, 
            Tween.TransitionType.Linear, Tween.EaseType.In);
        tweenPosX.Start();

        tweenPosY_Up.InterpolateProperty(area2D, "position:y",
            area2D.GlobalPosition.y, area2D.GlobalPosition.y - arcY, buffSpeed / 2, 
            Tween.TransitionType.Quad, Tween.EaseType.Out);
        tweenPosY_Up.Start();

        tweenPosY_Down.InterpolateProperty(area2D, "position:y",
            area2D.GlobalPosition.y - arcY, area2D.GlobalPosition.y, buffSpeed / 2, 
            Tween.TransitionType.Quad, Tween.EaseType.In, 
            /* delay going back down! */ buffSpeed / 2);
        tweenPosY_Down.Start();

        tweenRotate.InterpolateProperty(area2D, "rotation",
            0, 6f, buffSpeed, Tween.TransitionType.Linear, Tween.EaseType.OutIn);
        tweenRotate.Start();

        await ToSignal(tweenPosX, "tween_all_completed");

        tweenPosX.QueueFree();
        tweenPosY_Up.QueueFree();
        tweenPosY_Down.QueueFree();
        tweenRotate.QueueFree();
    }

    public async void _signal_CardBuffed(DeckNode2D deck, int index, int sourceIndex)
    {
        var cardSlot = deck.GetCardSlotNode2D(index + 1);
        var sourceCardSlot = deck.GetCardSlotNode2D(sourceIndex + 1);

        var buffArea2DScene = (PackedScene)ResourceLoader.Load("res://Scenes/BuffArea2D.tscn");
        Area2D buffArea2D = buffArea2DScene.Instance() as Area2D;
        AddChild(buffArea2D);
        buffArea2D.GlobalPosition = sourceCardSlot.GlobalPosition;

        await ThrowArea2D(buffArea2D, cardSlot.GlobalPosition);

        buffArea2D.QueueFree();

        cardSlot.CardArea2D.RenderCard(deck.Deck[index], index);

        autoResetEvent.Set();
    }

    public async void _signal_CardHurt(DeckNode2D deck, int index, DeckNode2D sourceDeck, int sourceIndex)
    {
        var cardSlot = deck.GetCardSlotNode2D(index + 1);
        var sourceCardSlot = sourceDeck.GetCardSlotNode2D(sourceIndex + 1);

        var damageArea2DScene = (PackedScene)ResourceLoader.Load("res://Scenes/DamageArea2D.tscn");
        Area2D damageArea2D = damageArea2DScene.Instance() as Area2D;
        AddChild(damageArea2D);
        damageArea2D.GlobalPosition = sourceCardSlot.GlobalPosition;

        await ThrowArea2D(damageArea2D, cardSlot.GlobalPosition);

        damageArea2D.QueueFree();

        cardSlot.CardArea2D.RenderCard(deck.Deck[index], index);

        autoResetEvent.Set();
    }

    public async void _signal_FightOver()
    {
        await PositionDecks();

        GameSingleton.Instance.Game.FightEvent -= _game_FightEvent;
        GameSingleton.Instance.Game.Player1.CardFaintedEvent -= _player_CardFaintedEvent;
        GameSingleton.Instance.Game.Player2.CardFaintedEvent -= _player_CardFaintedEvent;
        GameSingleton.Instance.Game.Player1.CardSummonedEvent -= _player_CardSummonedEvent;
        GameSingleton.Instance.Game.Player2.CardSummonedEvent -= _player_CardSummonedEvent;
        GameSingleton.Instance.Game.Player1.CardBuffedEvent -= _player_CardBuffedEvent;
        GameSingleton.Instance.Game.Player2.CardBuffedEvent -= _player_CardBuffedEvent;
        GameSingleton.Instance.Game.CardHurtEvent -= _game_CardHurtEvent;
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
                Player2DeckNode2D.Position, destination, 0.5f, Tween.TransitionType.Expo, 
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
