using Godot;
using System;
using System.Threading;
using AutoPets;

public class FoodArea2D : Area2D
{
    System.Threading.Thread _gameThread;

    Vector2 _defaultPosition;
    Vector2 _dragLocalMousePos;
    int _defaultZIndex;

    public Sprite Sprite { get { return GetNode<Sprite>("Sprite"); } }
    public IDragParent DragParent { get { return GetParent().GetParent() as IDragParent; } } 
    public BuildNode BuildNode { get { return GetParent().GetParent() as BuildNode; } }
    public int Index { get; set; }

    [Signal]
    public delegate void StartStopDragSignal();

    [Signal]
    public delegate void BuyFoodOverSignal();

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        // Dispose can be called from Godot editor so check if thread exists
        if (_gameThread != null)
            _gameThread.Abort();
    }

    public void _on_Area2D_mouse_entered()
    {
        GetParent().GetNode<Sprite>("HoverSprite").Show();
    }

    public void _on_Area2D_mouse_exited()
    {
        GetParent().GetNode<Sprite>("HoverSprite").Hide();
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
                Food food;
                if (Index == 1)
                    food = BuildNode.Player.ShopFood1;
                else
                    food = BuildNode.Player.ShopFood2;
                if (BuildNode.Player.Gold >= Game.FoodCost || 
                    food is SleepingPillFood && BuildNode.Player.Gold >= 1)
                    EmitSignal("StartStopDragSignal");
            }
            else
            {
                // mouse up
                if (Sprite.Visible && GameSingleton.Instance.DragSource == this &&
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

            if (GameSingleton.Instance.DragTarget is CardArea2D)
            {
                CardArea2D cardArea2D = GameSingleton.Instance.DragTarget as CardArea2D;
                if (cardArea2D.CardSlotNode2D.CardSlotDeck == BuildNode.DeckNode2D)
                {
                    var card = BuildNode.DeckNode2D.Deck[cardArea2D.CardIndex];
                    if (card != null)
                    {
                        _gameThread = new System.Threading.Thread(() =>
                        {
                            BuildNode.Player.BuyFood(card, Index);

                            this.EmitSignal("BuyFoodOverSignal");
                        });
                        _gameThread.Name = "Buy Food Thread";
                        _gameThread.Start();
                    }
                }
            }

            GameSingleton.Instance.DragTarget = null;
        }
    }

    public void _signal_BuyFoodOver()
    {
        BuildNode.RenderFood(1, BuildNode.Player.ShopFood1);
        BuildNode.RenderFood(2, BuildNode.Player.ShopFood2);
        BuildNode.DeckNode2D.RenderDeck(BuildNode.DeckNode2D.Deck);
        BuildNode.DeckNode2D.GulpPlayer.Play();
    }

    public override void _Ready()
    {
        Connect("StartStopDragSignal", this, "_signal_StartStopDrag");
        Connect("BuyFoodOverSignal", this, "_signal_BuyFoodOver", null, (int)ConnectFlags.Deferred);
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
