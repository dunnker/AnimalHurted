using Godot;
using System;
using AutoPets;

public class Deck : Node2D
{
    public void RenderDeck(AutoPets.Deck deck)
    {
        for (int i = 0; i < deck.Size; i++)
        {
            var cardSlot = GetNode(string.Format("CardSlot{0}", i + 1));
            var card = cardSlot.GetNode<global::Card>("Card");
            card.RenderCard(deck[i]);
        }
    }

    public override void _Ready()
    {
        
    }
}
