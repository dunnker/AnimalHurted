using Godot;
using System;

public class Card : Area2D
{
    bool _dragging;
    Vector2 _savePosition;
    int _saveZIndex;

    [Signal]
    public delegate void DragSignal();

    public void _on_Area2D_mouse_entered()
    {
        if (GameSingleton.Instance.Dragging)
            GameSingleton.Instance.DragTarget = this;
        (GetParent().FindNode("SelectedSprite") as Sprite).Show();
    }

    public void _on_Area2D_mouse_exited()
    {
        if (GameSingleton.Instance.Dragging)
            GameSingleton.Instance.DragTarget = null;
        (GetParent().FindNode("SelectedSprite") as Sprite).Hide();
    }

    public void _on_Area2D_input_event(Node viewport, InputEvent @event, int shape_idx)
    {
        if (@event is InputEventMouseButton)
        {
            var mouseEvent = @event as InputEventMouseButton;
            // mouse down
            if (mouseEvent.ButtonIndex == (int)ButtonList.Left && 
                mouseEvent.Pressed)
                EmitSignal("DragSignal");
            else
            {
                // mouse up
                if (_dragging && mouseEvent.ButtonIndex == (int)ButtonList.Left && !mouseEvent.Pressed)
                    EmitSignal("DragSignal");
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
            _savePosition = Position;
            _saveZIndex = ZIndex;
            ZIndex = 101;
        }
        else
        {
            Position = _savePosition;
            ZIndex = _saveZIndex;            
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
