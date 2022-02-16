using Godot;
using System;
using AutoPets;

public class Card : Area2D
{
    bool _dragging;
    Vector2 _savePosition;
    int _saveZIndex;
    int _cardIndex;

    public int CardIndex { get { return _cardIndex; } }

    public Sprite Sprite { get { return GetNode<Sprite>("Sprite"); } }

    [Signal]
    public delegate void DragSignal();

    public void RenderCard(AutoPets.Card card, int index)
    {
        _cardIndex = index;
        if (card == null)
            Sprite.Hide();
        else
        {
            int abilityIndex = AbilityList.Instance.AllAbilities.IndexOf(card.Ability);
            var res = GD.Load(string.Format("res://Assets/Pets/{0}.png", abilityIndex));
            Sprite.Texture = res as Godot.Texture;
            Sprite.Show();
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
        // Note: can't set DragTarget to null on exit event because an exit event might happen
        // after an enter event of the actual drag target

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
            (GetParent().GetParent() as IDragParent).DragDropped(this);
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
