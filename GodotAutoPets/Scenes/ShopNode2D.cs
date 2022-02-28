using System;
using Godot;
using AutoPets;

public class ShopNode2D : Node2D, IDragParent, ICardSlotDeck
{
    public BuildNode BuildNode { get { return GetParent() as BuildNode; } }

    public Deck Deck { get { return BuildNode.Player.ShopDeck; } }

    public CardSlotNode2D GetCardSlotNode2D(int index)
    {
        return GetNode<CardSlotNode2D>(string.Format("CardSlotNode2D_{0}", index));
    }

    public void RenderShop()
    {
        for (int i = 0; i < Deck.Size; i++)
        {
            var card = Deck[i];
            var cardSlot = GetCardSlotNode2D(i + 1);
            cardSlot.CardArea2D.RenderCard(card, i, false);
            if (i >= GameSingleton.Instance.Game.ShopSlots)
                cardSlot.Hide();
        }
    }

    // IDragParent
    public void DragDropped()
    {
        if (GameSingleton.Instance.DragTarget != null && GameSingleton.Instance.DragSource is CardArea2D)
        {
            var sourceCardArea2D = GameSingleton.Instance.DragSource as CardArea2D;
            var targetCardArea2D = GameSingleton.Instance.DragTarget;
            var sourceDeck = sourceCardArea2D.CardSlotNode2D.CardSlotDeck;
            var targetDeck = targetCardArea2D.CardSlotNode2D.CardSlotDeck;
            // did we drop onto the build deck?
            if (targetDeck.Deck == BuildNode.Player.BuildDeck)
            {
                // are we dropping onto an empty slot, or leveling up a card with same ability
                if (targetDeck.Deck[targetCardArea2D.CardIndex] == null ||
                    targetDeck.Deck[targetCardArea2D.CardIndex].Ability == sourceDeck.Deck[sourceCardArea2D.CardIndex].Ability)
                {
                    // select immediately before animations
                    targetCardArea2D.CardSlotNode2D.Selected = true;

                    // hide immediately since it's being dropped and animations are about 
                    // to be shown (e.g. if the bought card is buffed by an ability)
                    // we don't want the card shown in the shop during animations
                    sourceCardArea2D.HideCard();

                    var queue = new CardCommandQueue();
                    GameSingleton.Instance.Game.BuyFromShop(queue, sourceCardArea2D.CardIndex, targetCardArea2D.CardIndex, 
                        BuildNode.Player);
                    RenderShop();
                    BuildNode.DeckNode2D.RenderDeck(BuildNode.DeckNode2D.Deck);
                    BuildNode.DeckNode2D.PlayThump();
                    BuildNode.ExecuteQueue(queue);
                }
            }
        }
    }

    public void DragReorder(CardArea2D cardArea2D)
    {

    }

    public bool GetCanDrag()
    {
        return BuildNode.Player.Gold >= Game.PetCost;
    }
}
