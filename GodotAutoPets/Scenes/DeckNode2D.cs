using Godot;
using System;
using AutoPets;

public class DeckNode2D : Node2D, IDragParent
{
    public void RenderDeck(AutoPets.Deck deck)
    {
        for (int i = 0; i < deck.Size; i++)
        {
            var cardSlot = GetNode(string.Format("CardSlotNode2D_{0}", i + 1));
            var card = cardSlot.GetNode<global::CardArea2D>("CardAreaNode2D");
            card.RenderCard(deck[i], i);
        }
    }

    public override void _Ready()
    {
        
    }

    public void PlayThump()
    {
        GetNode<AudioStreamPlayer>("ThumpPlayer").Play();
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
        return true;
    }
}
