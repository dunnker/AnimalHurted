using Godot;
using System;
using AutoPets;

public class CardArea2D : Area2D
{
    Vector2 _defaultPosition;
    int _defaultZIndex;
    int _cardIndex;
    bool _cancelCardReorder = true;

    public CardSlotNode2D CardSlotNode2D { get { return GetParent() as CardSlotNode2D; } }

    public Vector2 DefaultPosition { get { return _defaultPosition; } }
    public int DefaultZIndex { get { return _defaultZIndex; } }

    public int CardIndex { get { return _cardIndex; } }

    public Sprite Sprite { get { return GetNode<Sprite>("Sprite"); } }

    public CardAttrsNode2D CardAttrsNode2D { get { return GetNode<CardAttrsNode2D>("CardAttrsNode2D"); } }

    public Label AttackPointsLabel { get { return GetNode<Label>("CardAttrsNode2D/AttackPointsLabel"); } }

    public Label HitPointsLabel { get { return GetNode<Label>("CardAttrsNode2D/HitPointsLabel"); } }

    public IDragParent DragParent { get { return GetParent().GetParent() as IDragParent; } } 

    public Timer CardReorderTimer { get { return GetNode<Timer>("CardReorderTimer"); } }

    [Signal]
    public delegate void StartStopDragSignal();

    public void HideCard()
    {
        Sprite.Hide();
        CardAttrsNode2D.Hide();
    }

    public void ShowCard()
    {
        Sprite.Show();
        CardAttrsNode2D.Show();
    }

    public void RenderCard(AutoPets.Card card, int index)
    {
        _cardIndex = index;
        if (card == null)
            HideCard();
        else
        {
            int abilityIndex = AbilityList.Instance.AllAbilities.IndexOf(card.Ability);
            var res = GD.Load(string.Format("res://Assets/Pets/{0}.png", abilityIndex));
            Sprite.Texture = res as Godot.Texture;
            AttackPointsLabel.Text = card.AttackPoints.ToString();
            HitPointsLabel.Text = card.HitPoints.ToString();
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
        if ((GameSingleton.Instance.StartingDrag || GameSingleton.Instance.Dragging) && 
            GameSingleton.Instance.DragSource != this)
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
                    GameSingleton.Instance.StartingDrag && GameSingleton.Instance.DragSource == this && 
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

    public async void _signal_StartStopDrag()
    {
        GameSingleton.Instance.StartingDrag = !GameSingleton.Instance.StartingDrag;
        if (GameSingleton.Instance.StartingDrag)
        {
            // must set immediately because of mouse enter/exit events checking DragSource
            GameSingleton.Instance.DragSource = this;
            GameSingleton.Instance.DragTarget = null;

            // defer starting drag in case user is just clicking on the card
            await ToSignal(GetTree().CreateTimer(0.1f, false), "timeout");

            // StartingDrag might be false if there was a mouse up event during the "await"
            GameSingleton.Instance.Dragging = GameSingleton.Instance.StartingDrag;
            if (GameSingleton.Instance.Dragging)
            {
                ZIndex = 101; // so the sprite appears above everything else during drag
            }
        }
        else
        {
            // otherwise end drag immediately
            Position = _defaultPosition;
            ZIndex = _defaultZIndex;;

            // if we ever started the drag
            if (GameSingleton.Instance.Dragging)
            {    
                GameSingleton.Instance.Dragging = false;
                // notify the parent of this card, either the Shop or BuildDeck that
                // a card has been dropped somewhere 
                DragParent.DragDropped(this);
                GameSingleton.Instance.DragTarget = null;
            }
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
            var mousePos = GetGlobalMousePosition();
            GlobalPosition = new Vector2(mousePos.x, mousePos.y);
        }
    }
}
