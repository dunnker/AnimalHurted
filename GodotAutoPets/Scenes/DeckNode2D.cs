using Godot;
using System;
using AutoPets;

public class DeckNode2D : Node2D, IDragParent
{
    Deck _deck;

    public Deck Deck { get { return _deck; } }

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
        }
    }

    public override void _Ready()
    {
    }

    public void PlayThump()
    {
        GetNode<AudioStreamPlayer>("ThumpPlayer").Play();
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

    // IDragParent
    public void DragDropped(CardArea2D card)
    {
        if (GameSingleton.Instance.DragTarget != null)
        {
            var targetCard = GameSingleton.Instance.DragTarget as CardArea2D;
            var cardParent = targetCard.GetParent().GetParent();
            if (cardParent is DeckNode2D)
            {
                var sourceCard = GameSingleton.Instance.DragSource as CardArea2D;
                
                GameSingleton.Instance.BuildPlayer.BuildDeck.MoveCard(
                    GameSingleton.Instance.BuildPlayer.BuildDeck[sourceCard.CardIndex], 
                    targetCard.CardIndex);
                PlayThump();   
            }
        }
        RenderDeck(GameSingleton.Instance.BuildPlayer.BuildDeck);
    }

    public bool GetCanDrag()
    {
        return GetParent() is BuildNode;
    }
}
