using Godot;
using System;
using AutoPets;

public class CardArea2D : Area2D
{
    bool _dragging;
    Vector2 _savePosition;
    int _saveZIndex;
    int _cardIndex;

    public int CardIndex { get { return _cardIndex; } }

    public Sprite Sprite { get { return GetNode<Sprite>("Sprite"); } }

    public CardAttrsNode2D CardAttrsNode2D { get { return GetNode<CardAttrsNode2D>("CardAttrsNode2D"); } }

    public Label AttackPointsLabel { get { return GetNode<Label>("CardAttrsNode2D/AttackPointsLabel"); } }

    public Label HitPointsLabel { get { return GetNode<Label>("CardAttrsNode2D/HitPointsLabel"); } }

    public IDragParent DragParent { get { return GetParent().GetParent() as IDragParent; } } 

    [Signal]
    public delegate void DragSignal();

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
        if (GameSingleton.Instance.Dragging && GameSingleton.Instance.DragSource != this)
            GameSingleton.Instance.DragTarget = this;
        (GetParent().FindNode("SelectedSprite") as Sprite).Show();
    }

    public void _on_Area2D_mouse_exited()
    {
		// mouse exit event can be invoked AFTER the mouse enter event of another card
		// so we don't always want to set DragTarget to null; set it to null only when 
		// the DragTarget is "this"
        if (GameSingleton.Instance.DragTarget == this)
            GameSingleton.Instance.DragTarget = null;
        (GetParent().FindNode("SelectedSprite") as Sprite).Hide();
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
                if (DragParent.GetCanDrag())
                    EmitSignal("DragSignal");
            }
            else
            {
                // mouse up
                if (Sprite.Visible && _dragging && mouseEvent.ButtonIndex == (int)ButtonList.Left && 
                    !mouseEvent.Pressed)
                {
                    EmitSignal("DragSignal");
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
        _dragging = !_dragging;
        GameSingleton.Instance.Dragging = _dragging;
        if (_dragging)
        {
            GameSingleton.Instance.DragSource = this;
            _savePosition = Position;
            _saveZIndex = ZIndex;
            ZIndex = 101; // so the sprite appears above everything else during drag
        }
        else
        {
            Position = _savePosition;
            ZIndex = _saveZIndex;    
            // notify the parent of this card, either the Shop or BuildDeck that
            // a card has been dropped somewhere 
            DragParent.DragDropped(this);
            GameSingleton.Instance.DragTarget = null;
        }
    }

    public override void _Ready()
    {
        Connect("DragSignal", this, "_signal_StartStopDrag");
    }

    public override void _Process(float delta)
    {
        if (_dragging)
        {
            var mousepos = GetGlobalMousePosition();
            GlobalPosition = new Vector2(mousepos.x, mousepos.y);
        }
    }
}
