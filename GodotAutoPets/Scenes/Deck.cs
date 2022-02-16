using Godot;
using System;
using AutoPets;

public class Deck : Node2D, IDragParent
{
    public void RenderDeck(AutoPets.Deck deck)
    {
        for (int i = 0; i < deck.Size; i++)
        {
            var cardSlot = GetNode(string.Format("CardSlot{0}", i + 1));
            var card = cardSlot.GetNode<global::Card>("Card");
            card.RenderCard(deck[i], i);
        }
    }

    public override void _Ready()
    {
        
    }

    // IDragParent
    public void DragDropped(Card card)
    {
        if (GameSingleton.Instance.DragTarget != null)
        {
            var targetCard = GameSingleton.Instance.DragTarget as Card;
            var cardParent = targetCard.GetParent().GetParent();
            if (cardParent is Deck)
            {
                var sourceCard = GameSingleton.Instance.DragSource as Card;
                
                GameSingleton.Instance.Game.Player1.BuildDeck.MoveCard(
                    GameSingleton.Instance.Game.Player1.BuildDeck[sourceCard.CardIndex], 
                    targetCard.CardIndex);
            }
        }
        RenderDeck(GameSingleton.Instance.Game.Player1.BuildDeck);
    }
}
