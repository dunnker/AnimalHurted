using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using AnimalHurtedLib;

public interface ICardSelectHost
{
    void SelectionChanged(CardSlotNode2D cardSlot);
}

public class DeckNode2D : Node2D, IDragParent, ICardSlotDeck, ICardSelectHost
{
    Deck _deck;

    public AudioStreamPlayer ThumpPlayer { get { return GetNode<AudioStreamPlayer>("ThumpPlayer"); } }
    public AudioStreamPlayer GulpPlayer { get { return GetNode<AudioStreamPlayer>("GulpPlayer"); } }
    public AudioStreamPlayer WhooshPlayer { get { return GetNode<AudioStreamPlayer>("WhooshPlayer"); } }

    public Deck Deck { get { return _deck; } }

    public bool CanDragDropLevelUp { get; set; } = true;

    public CardSlotNode2D GetCardSlotNode2D(int index)
    {
        return GetNode<CardSlotNode2D>(string.Format("CardSlotNode2D_{0}", index));
    }

    public void RenderDeck(Deck deck)
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
        GetParent().EmitSignal("CardSelectionChangedSignal", cardSlot.CardArea2D.CardIndex);

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
        // Dispose can be called from Godot editor, and our singleton
        // may not have a Game when designing
        if (GameSingleton.Instance.Game != null)
        {
            GameSingleton.Instance.Game.CardFaintedEvent -= _game_CardFaintedEvent;
            GameSingleton.Instance.Game.CardSummonedEvent -= _game_CardSummonedEvent;
            GameSingleton.Instance.Game.CardBuffedEvent -= _game_CardBuffedEvent;
            GameSingleton.Instance.Game.CardHurtEvent -= _game_CardHurtEvent;
            GameSingleton.Instance.Game.CardGainedFoodAbilityEvent -= _game_CardGainedFoodAbilityEvent;
            GameSingleton.Instance.Game.CardsMovedEvent -= _game_CardsMoved;
        }
    }

    public override void _Ready()
    {
        if (GameSingleton.Instance.Game != null)
        {
            GameSingleton.Instance.Game.CardFaintedEvent += _game_CardFaintedEvent;
            GameSingleton.Instance.Game.CardSummonedEvent += _game_CardSummonedEvent;
            GameSingleton.Instance.Game.CardBuffedEvent += _game_CardBuffedEvent;
            GameSingleton.Instance.Game.CardHurtEvent += _game_CardHurtEvent;
            GameSingleton.Instance.Game.CardGainedFoodAbilityEvent += _game_CardGainedFoodAbilityEvent;
            GameSingleton.Instance.Game.CardsMovedEvent += _game_CardsMoved;
        }
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

        float buffSpeed = BattleNode.MaxTimePerEvent;

        // pick a somewhat random height to throw, to minimize other objects from having
        // the same trajectory
        int yDelta = GameSingleton.Instance.Game.Random.Next(0, 100);
        if (GameSingleton.Instance.Game.Random.Next(0, 2) == 1)
            yDelta *= -1;
        int arcY = 200 + yDelta;

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
        if (GameSingleton.Instance.DragTarget != null && GameSingleton.Instance.DragSource is CardArea2D)
        {
            var sourceCardArea2D = GameSingleton.Instance.DragSource as CardArea2D;
            var sourceDeck = sourceCardArea2D.CardSlotNode2D.CardSlotDeck;
            var targetCardArea2D = GameSingleton.Instance.DragTarget;
            var targetDeck = targetCardArea2D.CardSlotNode2D.CardSlotDeck;
            if (targetDeck == this && sourceDeck == this)
            {
                // if dropping on an empty slot
                if (_deck[targetCardArea2D.CardIndex] == null)
                {
                    // moving a card does not invoke any abilities otherwise this
                    // would need to be done with a queue
                    _deck.MoveCard(_deck[sourceCardArea2D.CardIndex], targetCardArea2D.CardIndex);
                }
                else if (CanDragDropLevelUp)
                {
                    var targetCard = _deck[targetCardArea2D.CardIndex];
                    var sourceCard = sourceDeck.Deck[sourceCardArea2D.CardIndex];
                    if ((targetCardArea2D.CardIndex != sourceCardArea2D.CardIndex) && 
                        targetCard.Ability.GetType() == sourceCard.Ability.GetType())
                    {
                        int oldLevel = targetCard.Level;
                        targetCard.GainXP(sourceCard);
                        targetCardArea2D.RenderCard(targetCard, targetCard.Index);
                        var queue = new CardCommandQueue();
                        var savedDeck = (GetParent() as BuildNode).CreateSaveDeck();
                        GameSingleton.Instance.Game.BeginUpdate();
                        targetCard.GainedXP(queue, oldLevel);
                        GameSingleton.Instance.Game.EndUpdate();
                        // show animations from abilities, like Fish
                        (GetParent() as BuildNode).ExecuteQueue(queue, savedDeck);
                    }
                }

                targetCardArea2D.CardSlotNode2D.Selected = true;
                PlayThump();   
            }
        }
        RenderDeck(_deck);
    }

    public void DragReorder(CardArea2D atCardArea2D)
    {
        // we're either drag/dropping from the Shop scene or we are
        // drag/dropping in the build deck -- reordering cards in the same deck
        Card sourceCard = null;
        CardArea2D sourceCardArea2D = GameSingleton.Instance.DragSource as CardArea2D;
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
        return GetParent() is BuildNode || GetParent() is SandboxNode;
    }

    public async void _game_CardFaintedEvent(object sender, CardCommand command)
    {
        if (command.Deck == this._deck)
        {
            var tween = new Tween();
            AddChild(tween);

            float faintTime = BattleNode.MaxTimePerEvent;

            var cardSlot = GetCardSlotNode2D(command.Index + 1);
            tween.InterpolateProperty(cardSlot.CardArea2D.Sprite, "modulate:a",
                1.0, 0.0, faintTime, Tween.TransitionType.Linear, Tween.EaseType.In);
            tween.Start();

            await ToSignal(tween, "tween_all_completed");

            tween.QueueFree();

            // restore modulate, even though we're about to hide the sprite
            // next time something spawns we want modulate to have its restored value
            var color = cardSlot.CardArea2D.Sprite.Modulate;
            cardSlot.CardArea2D.Sprite.Modulate = new Color(color.r, color.g,
                color.b, 1);
            cardSlot.CardArea2D.RenderCard(null, command.Index);

            command.UserEvent?.Invoke(this, EventArgs.Empty);
        }
    }

    public void _game_CardsMoved(object sender, CardCommand command)
    {
        if (command.Deck == _deck)
        {
            RenderDeck(_deck);  

            // don't invoke because we're expecting a summon event, which is likely the last command
            // in the current queue
            //command.UserEvent?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void _game_CardSummonedEvent(object sender, CardCommand command)
    {
        var summonedCommand = command as SummonCardCommand;
        if (summonedCommand.SummonedCard.Deck == this._deck)
        {
            var cardSlot = GetCardSlotNode2D(summonedCommand.AtIndex + 1);
            if (!cardSlot.Visible)
            {
                // multiple slots prior to cardSlot might be hidden, so restore
                for (int i = summonedCommand.AtIndex + 1; i >= 0; i--)
                {
                    var hiddenSlot = GetCardSlotNode2D(i);
                    if (!hiddenSlot.Visible)
                        hiddenSlot.Show();
                    else
                        break;
                }
                if (GetParent() is BattleNode)
                {
                    await (GetParent() as BattleNode).PositionDecks(false);
                }
            }

            cardSlot.CardArea2D.RenderCard(_deck[summonedCommand.AtIndex], summonedCommand.AtIndex);
            
            command.UserEvent?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void _game_CardBuffedEvent(object sender, CardCommand command)
    {
        if (command.Deck == this._deck)
        {
            var cardSlot = GetCardSlotNode2D(command.Index + 1);
            var sourceCardSlot = GetCardSlotNode2D((command as BuffCardCommand).SourceIndex + 1);

            var buffArea2DScene = (PackedScene)ResourceLoader.Load("res://Scenes/BuffArea2D.tscn");
            Area2D buffArea2D = buffArea2DScene.Instance() as Area2D;
            GetParent().AddChild(buffArea2D);
            buffArea2D.GlobalPosition = sourceCardSlot.GlobalPosition;

            await DeckNode2D.ThrowArea2D(GetParent(), buffArea2D, cardSlot.GlobalPosition);

            buffArea2D.QueueFree();

            GulpPlayer.Play();
            cardSlot.CardArea2D.RenderCard(_deck[command.Index], command.Index);

            command.UserEvent?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void _game_CardHurtEvent(object sender, CardCommand command)
    {
        var hurtCommand = command as HurtCardCommand;
        // see also BattleNode where its _game_CardHurtEvent handles 
        // the case where source card deck is an opponent
        // in which case we have to animate from the opponent's DeckNodeScene
        if (hurtCommand.Deck == this._deck && hurtCommand.SourceDeck == hurtCommand.Deck)
        {
            var cardSlot = GetCardSlotNode2D(hurtCommand.Index + 1);
            var sourceCardSlot = GetCardSlotNode2D(hurtCommand.SourceIndex + 1);

            WhooshPlayer.Play();

            var damageArea2DScene = (PackedScene)ResourceLoader.Load("res://Scenes/DamageArea2D.tscn");
            Area2D damageArea2D = damageArea2DScene.Instance() as Area2D;
            GetParent().AddChild(damageArea2D);
            damageArea2D.GlobalPosition = sourceCardSlot.GlobalPosition;

            await DeckNode2D.ThrowArea2D(this, damageArea2D, cardSlot.GlobalPosition);

            damageArea2D.QueueFree();

            cardSlot.CardArea2D.RenderCard(_deck[hurtCommand.Index], hurtCommand.Index);

            command.UserEvent?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void _game_CardGainedFoodAbilityEvent(object sender, CardCommand command)
    {
        if (command.Deck == this._deck)
        {
            var cardSlot = GetCardSlotNode2D(command.Index + 1);
            /*var sourceCardSlot = GetCardSlotNode2D(sourceIndex + 1);

            var buffArea2DScene = (PackedScene)ResourceLoader.Load("res://Scenes/BuffArea2D.tscn");
            Area2D buffArea2D = buffArea2DScene.Instance() as Area2D;
            GetParent().AddChild(buffArea2D);
            buffArea2D.GlobalPosition = sourceCardSlot.GlobalPosition;

            await DeckNode2D.ThrowArea2D(GetParent(), buffArea2D, cardSlot.GlobalPosition);

            buffArea2D.QueueFree();*/

            await ToSignal(GetTree().CreateTimer(BattleNode.MaxTimePerEvent), "timeout"); //TODO remove

            GulpPlayer.Play();
            cardSlot.CardArea2D.RenderCard(_deck[command.Index], command.Index);

            command.UserEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
