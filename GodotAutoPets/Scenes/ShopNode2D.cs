using System;
using System.Threading;
using Godot;
using AutoPets;

public class ShopNode2D : Node2D, IDragParent
{
    public BuildNode BuildNode { get { return GetParent() as BuildNode; } }

    [Signal]
    public delegate void CardBoughtSignal();

    public override void _Ready()
    {
        Connect("CardBoughtSignal", this, "_signal_CardBought", null, 
            (int)ConnectFlags.Deferred);
        RenderShop();        
    }

    public void RenderShop()
    {
        for (int i = 0; i < GameSingleton.Instance.BuildPlayer.ShopDeck.Size; i++)
        {
            var card = GameSingleton.Instance.BuildPlayer.ShopDeck[i];
            var cardSlot = GetNode<CardSlotNode2D>(string.Format("CardSlotNode2D_{0}", i + 1));
            cardSlot.CardArea2D.RenderCard(card, i);
            if (i >= GameSingleton.Instance.Game.ShopSlots)
                cardSlot.Hide();
        }
    }

    public override void _Process(float delta)
    {
        
    }

    // IDragParent
    public void DragDropped(CardArea2D cardArea2D)
    {
        if (GameSingleton.Instance.DragTarget != null)
        {
            var targetCard = GameSingleton.Instance.DragTarget as CardArea2D;
            var cardParent = targetCard.GetParent().GetParent();
            // did we drop onto the build deck
            if (cardParent is DeckNode2D)
            {
                new System.Threading.Thread(() => 
                {
                    GameSingleton.Instance.Game.BuyFromShop(cardArea2D.CardIndex, targetCard.CardIndex, 
                        GameSingleton.Instance.BuildPlayer);

                    //TODO: assuming "this" is still valid; e.g. user hasn't closed
                    // the scene immediately after dropping card from shop
                    this.EmitSignal("CardBoughtSignal");
                }).Start();
            }
        }
    }

    public bool GetCanDrag()
    {
        return GameSingleton.Instance.BuildPlayer.Gold >= Game.PetCost;
    }

    public void _signal_CardBought()
    {
        RenderShop();
        // not necessary to render deck because the bought card gets a 
        // summoned event and the deck renders it
        //BuildNode.Deck.RenderDeck(GameSingleton.Instance.BuildPlayer.BuildDeck);
        BuildNode.Deck.PlayThump();
    }
}
