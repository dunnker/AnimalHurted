using Godot;
using System;
using AutoPets;

public class CardArea2D : Area2D
{
    Vector2 _defaultPosition;
    Vector2 _dragLocalMousePos;
    int _defaultZIndex;
    int _cardIndex;
    bool _cancelCardReorder = true;
    bool _showLevelLabel;

    public CardSlotNode2D CardSlotNode2D { get { return GetParent() as CardSlotNode2D; } }

    public int CardIndex { get { return _cardIndex; } }

    public Sprite Sprite { get { return GetNode<Sprite>("Sprite"); } }
    public CollisionShape2D CollisionShape2D { get { return GetNode<CollisionShape2D>("CollisionShape2D"); } }

    public CardAttrsNode2D CardAttrsNode2D { get { return GetNode<CardAttrsNode2D>("CardAttrsNode2D"); } }

    public Label AttackPointsLabel { get { return GetNode<Label>("CardAttrsNode2D/AttackPointsLabel"); } }

    public Label HitPointsLabel { get { return GetNode<Label>("CardAttrsNode2D/HitPointsLabel"); } }

    public Label LevelLabel { get { return GetNode<Label>("LevelLabel"); } }

    public IDragParent DragParent { get { return GetParent().GetParent() as IDragParent; } } 

    public Timer CardReorderTimer { get { return GetNode<Timer>("CardReorderTimer"); } }

    [Signal]
    public delegate void StartStopDragSignal();

    public void HideCard()
    {
        Sprite.Hide();
        CardAttrsNode2D.Hide();
        LevelLabel.Hide();
    }

    public void ShowCard()
    {
        Sprite.Show();
        CardAttrsNode2D.Show();
        if (_showLevelLabel)
            LevelLabel.Show();
    }

    public void RenderCard(AutoPets.Card card, int index, bool showLevelLabel = true)
    {
        _cardIndex = index;
        _showLevelLabel = showLevelLabel;
        if (!_showLevelLabel)
            LevelLabel.Hide();
        if (card == null)
            HideCard();
        else
        {
            int abilityIndex = AbilityList.Instance.AllAbilities.IndexOf(card.Ability);
            var res = GD.Load(string.Format("res://Assets/Pets/{0}.png", abilityIndex));
            Sprite.Texture = res as Godot.Texture;
            AttackPointsLabel.Text = card.AttackPoints.ToString();
            HitPointsLabel.Text = card.HitPoints.ToString();
            LevelLabel.Text = string.Format("Lvl{0}{1}", card.Level, new string('+', card.XPRemainder));
            ShowCard();
        }
    }

    public void _on_Area2D_mouse_entered()
    {
        CardSlotNode2D.HoverSprite.Show();

        // if dragging from one card to another adjacent card
        // sometimes the mouse_entered event will fire for the adjacent card
        // and then fire for the card we're dragging. So checking that the
        // DragSource != this
        if (GameSingleton.Instance.Dragging && GameSingleton.Instance.DragSource != this)
        {
            if (!Sprite.Visible)
            {
                GameSingleton.Instance.DragTarget = this;
            }
            else
            {
                // every card has their own timer which can start from a mouse entered event
                // and is stopped from a card exit event
                _cancelCardReorder = false;
                CardReorderTimer.Start();
            }
        }
    }

    public void _on_CardReorderTimer_timeout()
    {
        if (GameSingleton.Instance.Dragging && !_cancelCardReorder)
        {
            DragParent.DragReorder(this);
            // if after re-order, the sprite is now not visible (e.g. empty slot was created), set the DragTarget
			// to this empty slot
            if (!Sprite.Visible)
                GameSingleton.Instance.DragTarget = this;
        }
        _cancelCardReorder = true;
    }

    public void ReplaceDragSource(CardArea2D cardArea2D)
    {
        GameSingleton.Instance.DragSource = cardArea2D;
        GameSingleton.Instance.DragTarget = cardArea2D;
        cardArea2D.GlobalPosition = GlobalPosition;
        cardArea2D.ZIndex = ZIndex;
        cardArea2D._dragLocalMousePos = _dragLocalMousePos;
        // restore old DragSource's position
        Position = _defaultPosition;
        ZIndex = _defaultZIndex;
        CardSlotNode2D.Selected = false;
    }

    public void _on_Area2D_mouse_exited()
    {
        CardSlotNode2D.HoverSprite.Hide();

        if (GameSingleton.Instance.Dragging)
        {
            _cancelCardReorder = true;
            CardReorderTimer.Stop();
        }

		// mouse exit event can be invoked AFTER the mouse enter event of another card
		// so we don't always want to set DragTarget to null; set it to null only when 
		// the DragTarget is "this"
        if (GameSingleton.Instance.DragTarget == this)
        {
            GameSingleton.Instance.DragTarget = null;
        }
    }

    public void _on_Area2D_input_event(Node viewport, InputEvent @event, int shape_idx)
    {
        if (@event is InputEventMouseButton)
        {
            var mouseEvent = @event as InputEventMouseButton;
            // mouse down
            if (Sprite.Visible && mouseEvent.ButtonIndex == (int)ButtonList.Left && 
                mouseEvent.Pressed)
            {
                CardSlotNode2D.Selected = !CardSlotNode2D.Selected;
                if (DragParent.GetCanDrag())
                {
                    EmitSignal("StartStopDragSignal");
                }
            }
            else
            {
                // mouse up
                if (Sprite.Visible && 
                    GameSingleton.Instance.Dragging && GameSingleton.Instance.DragSource == this && 
                    mouseEvent.ButtonIndex == (int)ButtonList.Left && 
                    !mouseEvent.Pressed)
                {
                    EmitSignal("StartStopDragSignal");
                }
            }
        }
        else
        {
            if (@event is InputEventScreenTouch)
            {
                var screenTouchEvent = @event as InputEventScreenTouch;
                if (screenTouchEvent.Pressed && screenTouchEvent.Index == 0)
                    this.Position = screenTouchEvent.Position;
            }
        } 
    }

    public void _signal_StartStopDrag()
    {
        GameSingleton.Instance.Dragging = !GameSingleton.Instance.Dragging;
        if (GameSingleton.Instance.Dragging)
        {
            GameSingleton.Instance.DragSource = this;
            GameSingleton.Instance.DragTarget = null;
            _dragLocalMousePos = GetLocalMousePosition();
            ZIndex = 101; // so the sprite appears above everything else during drag
        }
        else
        {
            // may have dropped somewhere, but restore its position no matter what
            Position = _defaultPosition;
            ZIndex = _defaultZIndex;;

            // notify the parent of this card, either the Shop or BuildDeck that
            // a card has been dropped somewhere 
            DragParent.DragDropped();
            GameSingleton.Instance.DragTarget = null;
        }
    }

    public override void _Ready()
    {
        Connect("StartStopDragSignal", this, "_signal_StartStopDrag");
        _defaultPosition = Position;
        _defaultZIndex = ZIndex;
    }

    public override void _Process(float delta)
    {
        if (GameSingleton.Instance.Dragging && GameSingleton.Instance.DragSource == this)
        {
            var globalMousePos = GetGlobalMousePosition();
			// offset by original local mouse coords because if user is just clicking on the sprite,
			// dragging starts immediately, and we don't want the sprite to snap to the new position
			// for instance, if user mouse down is in the corner of the sprite and not exactly in 
			// the middle of the sprite
            GlobalPosition = globalMousePos - _dragLocalMousePos;
        }
    }
}
