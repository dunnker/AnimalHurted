using Godot;
using System;
using AnimalHurtedLib;

public class SandboxItemNode : PanelContainer, IDragParent
{
    Sprite _dragSprite;

    public TextureRect TextureRect { get { return GetNode<TextureRect>("TextureRect"); } }

    public SandboxNode SandboxNode { get { return GetTree().Root.GetNode("Node") as SandboxNode; } }

    public string TypeName { get; set; }

    [Signal]
    public delegate void StartStopDragSignal();

    public override void _Ready()
    {
        Connect("StartStopDragSignal", this, "_signal_StartStopDrag", null, (int)ConnectFlags.Deferred);
    }

    public async void _on_PanelContainer_gui_input(InputEvent @event)
    {
        if (@event is InputEventMouseButton)
        {
            var mouseEvent = @event as InputEventMouseButton;
            // mouse down
            if (mouseEvent.ButtonIndex == (int)ButtonList.Left &&
                mouseEvent.Pressed)
            {
                EmitSignal("StartStopDragSignal");
            }
            else
            {
                // mouse up
                if (mouseEvent.ButtonIndex == (int)ButtonList.Left && !mouseEvent.Pressed)
                {
                    if (GameSingleton.Instance.Dragging && GameSingleton.Instance.DragSource == this)
                    {
						// wait a short time for CardArea2D to get its mouse entered event
						// when dragging from a TextureRect, the mouse events get captured and CardArea2D
						// won't get a mouse entered event until mouse button is unpressed and on
						// some computers the DragDropped code will be invoked before the mouse entered signal
						// this is my workaround for now
                        await ToSignal(GetTree().CreateTimer(0.05f), "timeout");

                        EmitSignal("StartStopDragSignal");
                    }
                }
            }
        }
    }

    public void _signal_StartStopDrag()
    {
        GameSingleton.Instance.Dragging = !GameSingleton.Instance.Dragging;
        if (GameSingleton.Instance.Dragging)
        {
            _dragSprite = new Sprite();
            _dragSprite.Texture = TextureRect.Texture;

            _dragSprite.ZIndex = 101; // so the sprite appears above everything else during drag
            SandboxNode.AddChild(_dragSprite);

            GameSingleton.Instance.DragSource = this;
            GameSingleton.Instance.DragTarget = null;
        }
        else
        {
            _dragSprite.QueueFree();
            _dragSprite = null;

            DragDropped();
            GameSingleton.Instance.DragTarget = null;
        }
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
            _dragSprite.GlobalPosition = globalMousePos;
        }
    }

    // IDragParent
    public void DragDropped()
    {
        if (GameSingleton.Instance.DragSource != null && GameSingleton.Instance.DragTarget != null)
        {
            var cardArea2D = GameSingleton.Instance.DragTarget as CardArea2D;
            var deck = cardArea2D.CardSlotNode2D.CardSlotDeck.Deck;
            var typeName = $"AnimalHurtedLib.{TypeName}, AnimalHurted, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"; 
            var type = Type.GetType(typeName);
            if (type != null)
            {
                if (typeof(Ability).IsAssignableFrom(type))
                {
                    if (deck[cardArea2D.CardIndex] != null)
                        deck.Remove(cardArea2D.CardIndex);
                    var ability = Activator.CreateInstance(type) as Ability; 
                    var card = new Card(deck, ability);
                    deck.SetCard(card, cardArea2D.CardIndex);
                    cardArea2D.RenderCard(card, cardArea2D.CardIndex);
                    SandboxNode.DeckNode2D.PlayThump();
                }
                else if (typeof(FoodAbility).IsAssignableFrom(type))
                {
                    var card = deck[cardArea2D.CardIndex];
                    if (card != null)
                    {
                        deck[cardArea2D.CardIndex].FoodAbility = Activator.CreateInstance(type) as FoodAbility;
                        cardArea2D.RenderCard(card, cardArea2D.CardIndex);
                        SandboxNode.DeckNode2D.PlayThump();
                    }
                }
            }
        }
    }

    public bool GetCanDrag()
    {
        throw new NotImplementedException();
    }

    public void DragReorder(CardArea2D cardArea2D)
    {
        throw new NotImplementedException();
    }
}
