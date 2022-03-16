using System;
using Godot;
using AnimalHurtedLib;

public interface ICardSlotDeck
{
    Deck Deck { get; }
}

public class CardSlotNode2D : Node2D
{
    bool _selected;

    public CardArea2D CardArea2D { get { return GetNode<CardArea2D>("CardArea2D"); } }
    public Sprite HoverSprite { get { return GetNode<Sprite>("HoverSprite"); } }
    public Node2D AbilityHintNode2D { get { return GetNode<Node2D>("AbilityHintNode2D"); } }
    public Sprite SelectedSprite { get { return GetNode<Sprite>("SelectedSprite"); } }
    public ICardSlotDeck CardSlotDeck { get { return GetParent() as ICardSlotDeck; } }

    public void ClearSelected()
    {
        _selected = false;
        SelectedSprite.Hide();
    }

    public bool Selected 
    { 
        get 
        { 
            return _selected; 
        }

        set
        {
            if (GetParent() is ICardSelectHost)
            {
                if (_selected != value)
                {
                    _selected = value;
                    var host = GetParent() as ICardSelectHost;
                    host.SelectionChanged(this);
                    if (_selected)
                        SelectedSprite.Show();
                    else
                        SelectedSprite.Hide();
                }
            }
        }
    }
}
