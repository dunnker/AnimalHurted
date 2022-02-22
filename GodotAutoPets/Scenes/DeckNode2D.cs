using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using AutoPets;

public interface ICardSelectHost
{
    void SelectionChanged(CardSlotNode2D cardSlot);
}

public class DeckNode2D : Node2D, IDragParent, ICardSlotDeck, ICardSelectHost
{
    System.Threading.Thread _gameThread;
    Deck _deck;

    public AudioStreamPlayer ThumpPlayer { get { return GetNode<AudioStreamPlayer>("ThumpPlayer"); } }
    public AudioStreamPlayer GulpPlayer { get { return GetNode<AudioStreamPlayer>("GulpPlayer"); } }
    public AudioStreamPlayer WhooshPlayer { get { return GetNode<AudioStreamPlayer>("WhooshPlayer"); } }

    public Deck Deck { get { return _deck; } }

    [Signal]
    public delegate void CardFaintedSignal(int index);

    [Signal]
    public delegate void CardSummonedSignal(int index);

    [Signal]
    public delegate void CardBuffedSignal(int index, int sourceIndex);

    [Signal]
    public delegate void CardHurtSignal(int index, int sourceIndex);

    [Signal]
    public delegate void CardGainedXPDragDropSignal();

    public CardSlotNode2D GetCardSlotNode2D(int index)
    {
        return GetNode<CardSlotNode2D>(string.Format("CardSlotNode2D_{0}", index));
    }

    public void RenderDeck(AutoPets.Deck deck)
    {
        _deck = deck;
        for (int i = 0; i < deck.Size; i++)
        {
            var cardSlot = GetCardSlotNode2D(i + 1);
            cardSlot.CardArea2D.RenderCard(deck[i], i);
            // during battle cardSlot can be hidden; so restoring to visible
            cardSlot.Show();
        }
    }

    // ICardSelectHost
    public void SelectionChanged(CardSlotNode2D cardSlot)
    {
        if (cardSlot.Selected)
        {
            for (int i = 1; i <= 5; i++)
            {
                var tempCardSlot = GetCardSlotNode2D(i);
                if (tempCardSlot != cardSlot)
                    tempCardSlot.Selected = false;
            }
        }
    }
    // ICardSelectHost

    public CardSlotNode2D GetSelectedCardSlotNode2D()
    {
        for (int i = 1; i <= 5; i++)
        {
            var cardSlot = GetCardSlotNode2D(i);
            if (cardSlot.Selected)
                return cardSlot;
        }
        return null;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (_gameThread != null)
            _gameThread.Abort();
        // Dispose can be called from Godot editor, and our singleton
        // may not have a Game when designing
        if (GameSingleton.Instance.Game != null)
        {
            GameSingleton.Instance.Game.CardFaintedEvent -= _game_CardFaintedEvent;
            GameSingleton.Instance.Game.CardSummonedEvent -= _game_CardSummonedEvent;
            GameSingleton.Instance.Game.CardBuffedEvent -= _game_CardBuffedEvent;
            GameSingleton.Instance.Game.CardHurtEvent -= _game_CardHurtEvent;
        }
    }

    public override void _Ready()
    {
        GameSingleton.Instance.Game.CardFaintedEvent += _game_CardFaintedEvent;
        GameSingleton.Instance.Game.CardSummonedEvent += _game_CardSummonedEvent;
        GameSingleton.Instance.Game.CardBuffedEvent += _game_CardBuffedEvent;
        GameSingleton.Instance.Game.CardHurtEvent += _game_CardHurtEvent;

        Connect("CardFaintedSignal", this, "_signal_CardFainted", null, 
            (int)ConnectFlags.Deferred);
        Connect("CardSummonedSignal", this, "_signal_CardSummoned", null, 
            (int)ConnectFlags.Deferred);
        Connect("CardBuffedSignal", this, "_signal_CardBuffed", null, 
            (int)ConnectFlags.Deferred);
        Connect("CardHurtSignal", this, "_signal_CardHurt", null, 
            (int)ConnectFlags.Deferred);
        Connect("CardGainedXPDragDropSignal", this, "_signal_CardGainedXPDragDrop", null,
            (int)ConnectFlags.Deferred);
    }

    public void PlayThump()
    {
        ThumpPlayer.Play();
    }

    public void ReverseCardAreaPositions()
    {
        var cardSlot = GetCardSlotNode2D(1);
        var savePosition = cardSlot.Position;
        cardSlot.Position = GetCardSlotNode2D(5).Position;
        GetCardSlotNode2D(5).Position = savePosition;

        cardSlot = GetCardSlotNode2D(2);
        savePosition = cardSlot.Position;
        cardSlot.Position = GetCardSlotNode2D(4).Position;
        GetCardSlotNode2D(4).Position = savePosition;

        // flip the sprite to face other direction
        for (int i = 1; i <= 5; i++)
            GetCardSlotNode2D(i).CardArea2D.Sprite.FlipH = true;
    }

    public void HideEndingCardSlots()
    {
        for (int i = 5; i >= 1; i--)
        {
            var cardSlot = GetCardSlotNode2D(i);
            if (cardSlot.CardArea2D.Sprite.Visible)
                break;
            else
                cardSlot.Hide();
        }
    }

    public CardSlotNode2D GetEndingVisibleCardSlot()
    {
        for (int i = 5; i >= 1; i--)
        {
            var cardSlot = GetCardSlotNode2D(i);
            if (cardSlot.Visible)
                return cardSlot;
        }
        return null;
    }

    public static async Task ThrowArea2D(Node parent, Area2D area2D, Vector2 toPosition)
    {
        var tweenPosX = new Tween();
        parent.AddChild(tweenPosX);
        var tweenPosY_Up = new Tween();
        parent.AddChild(tweenPosY_Up);
        var tweenPosY_Down = new Tween();
        parent.AddChild(tweenPosY_Down);
        var tweenRotate = new Tween();
        parent.AddChild(tweenRotate);

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

        await parent.ToSignal(tweenPosX, "tween_all_completed");

        tweenPosX.QueueFree();
        tweenPosY_Up.QueueFree();
        tweenPosY_Down.QueueFree();
        tweenRotate.QueueFree();
    }

    // IDragParent
    public void DragDropped()
    {
        if (GameSingleton.Instance.DragTarget != null)
        {
            var sourceCardArea2D = GameSingleton.Instance.DragSource;
            var sourceDeck = sourceCardArea2D.CardSlotNode2D.CardSlotDeck;
            var targetCardArea2D = GameSingleton.Instance.DragTarget;
            var targetDeck = targetCardArea2D.CardSlotNode2D.CardSlotDeck;
            if (targetDeck == this && sourceDeck == this)
            {
                // if dropping on an empty slot
                if (_deck[targetCardArea2D.CardIndex] == null)
                {
                    // moving a card does not invoke any game events otherwise this
                    // would need to be done in a thread
                    _deck.MoveCard(_deck[sourceCardArea2D.CardIndex], targetCardArea2D.CardIndex);
                }
                else
                {
                    if ((targetCardArea2D.CardIndex != sourceCardArea2D.CardIndex) && 
                        _deck[targetCardArea2D.CardIndex].Ability == sourceDeck.Deck[sourceCardArea2D.CardIndex].Ability)
                    {
                        _gameThread = new System.Threading.Thread(() =>
                        {
                            // leveling up can trigger abilities, so doing this in a thread
                            // events will update UI
                            _deck[targetCardArea2D.CardIndex].GainXP(sourceDeck.Deck[sourceCardArea2D.CardIndex]);

                            this.EmitSignal("CardGainedXPDragDropSignal");
                        });
                        _gameThread.Name = "DragDropped Game Thread";
                        _gameThread.Start();
                    }
                }

                targetCardArea2D.CardSlotNode2D.Selected = true;
                PlayThump();   
            }
        }
        RenderDeck(_deck);
    }

    public void _signal_CardGainedXPDragDrop()
    {
        RenderDeck(_deck);
    }

    public void DragReorder(CardArea2D atCardArea2D)
    {
        // we're either drag/dropping from the Shop scene or we are
        // drag/dropping in the build deck -- reordering cards in the same deck
        Card sourceCard = null;
        CardArea2D sourceCardArea2D = GameSingleton.Instance.DragSource;
        // if reordering cards within the same deck 
        if (sourceCardArea2D.CardSlotNode2D.CardSlotDeck == this)
        {
            //.. remove source card immediately
            sourceCard = _deck[sourceCardArea2D.CardIndex];
            _deck.Remove(sourceCardArea2D.CardIndex);
        }
        if (_deck.MakeRoomAt(atCardArea2D.CardIndex))
        {
            if (sourceCardArea2D.CardSlotNode2D.CardSlotDeck == this)
                // ...place in its new position
                _deck.SetCard(sourceCard, atCardArea2D.CardIndex);
            // redisplay cards that have been moved
            RenderDeck(_deck);
            if (sourceCardArea2D.CardSlotNode2D.CardSlotDeck == this)
                // sourceCardArea2D is now associated with a different card
                // so restore its drag position and assign a new drag source card
                sourceCardArea2D.ReplaceDragSource(atCardArea2D);
            atCardArea2D.CardSlotNode2D.Selected = false;
        }
    }

    public bool GetCanDrag()
    {
        return GetParent() is BuildNode;
    }

    // Game thread events happening within their own thread.
    // Other nodes may get the same event, but only one event should
    // call WaitOne(), and only one spawning signal should reset the thread
    // with GameSingleton.autoResetEvent.Reset()
    public void _game_CardFaintedEvent(object sender, Card card, int index)
    {
        if (card.Deck == this._deck)
        {
            EmitSignal("CardFaintedSignal", index);
            GameSingleton.autoResetEvent.WaitOne();
        }
    }

    public void _game_CardSummonedEvent(object sender, Card card, int index)
    {
        if (card.Deck == this._deck)
        {
            EmitSignal("CardSummonedSignal", card.Index);
            GameSingleton.autoResetEvent.WaitOne();
        }
    }

    public void _game_CardBuffedEvent(object sender, Card card, int sourceIndex)
    {
        if (card.Deck == this._deck)
        {
            EmitSignal("CardBuffedSignal", card.Index, sourceIndex);
            GameSingleton.autoResetEvent.WaitOne();
        }
    }

    public void _game_CardHurtEvent(object sender, Card card, Card sourceCard)
    {
        // see also BattleNode where its _game_CardHurtEvent handles 
        // the case where source card deck is an opponent
        if (card.Deck == this._deck && sourceCard.Deck == card.Deck)
        {
            EmitSignal("CardHurtSignal", card.Index, sourceCard.Index);
            GameSingleton.autoResetEvent.WaitOne();
        }
    }

    // main thread signals
    public async void _signal_CardFainted(int index)
    {
        var tween = new Tween();
        AddChild(tween);

        var cardSlot = GetCardSlotNode2D(index + 1);
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

        GameSingleton.autoResetEvent.Set();
    }

    public void _signal_CardSummoned(int index)
    {
        var cardSlot = GetCardSlotNode2D(index + 1);
        if (!cardSlot.Visible)
        {
            cardSlot.Show();

            //TODO: for Sheep we may need to call this method...
            //await PositionDecks();
        }

        cardSlot.CardArea2D.RenderCard(_deck[index], index);

        GameSingleton.autoResetEvent.Set();
    }

    public async void _signal_CardBuffed(int index, int sourceIndex)
    {
        var cardSlot = GetCardSlotNode2D(index + 1);
        var sourceCardSlot = GetCardSlotNode2D(sourceIndex + 1);

        var buffArea2DScene = (PackedScene)ResourceLoader.Load("res://Scenes/BuffArea2D.tscn");
        Area2D buffArea2D = buffArea2DScene.Instance() as Area2D;
        GetParent().AddChild(buffArea2D);
        buffArea2D.GlobalPosition = sourceCardSlot.GlobalPosition;

        await DeckNode2D.ThrowArea2D(GetParent(), buffArea2D, cardSlot.GlobalPosition);

        buffArea2D.QueueFree();

        GulpPlayer.Play();
        cardSlot.CardArea2D.RenderCard(_deck[index], index);

        GameSingleton.autoResetEvent.Set();
    }

    public async void _signal_CardHurt(int index, int sourceIndex)
    {
        var cardSlot = GetCardSlotNode2D(index + 1);
        var sourceCardSlot = GetCardSlotNode2D(sourceIndex + 1);

        WhooshPlayer.Play();

        var damageArea2DScene = (PackedScene)ResourceLoader.Load("res://Scenes/DamageArea2D.tscn");
        Area2D damageArea2D = damageArea2DScene.Instance() as Area2D;
        AddChild(damageArea2D);
        damageArea2D.GlobalPosition = sourceCardSlot.GlobalPosition;

        await DeckNode2D.ThrowArea2D(this, damageArea2D, cardSlot.GlobalPosition);

        damageArea2D.QueueFree();

        cardSlot.CardArea2D.RenderCard(_deck[index], index);

        GameSingleton.autoResetEvent.Set();
    }
}
