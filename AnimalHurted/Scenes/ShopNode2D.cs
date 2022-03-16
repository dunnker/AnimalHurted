using System;
using Godot;
using AnimalHurtedLib;

public class ShopNode2D : Node2D, IDragParent, ICardSlotDeck, ICardSelectHost
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
            cardSlot.ClearSelected();
            cardSlot.CardArea2D.RenderCard(card, i, false);
            if (i >= GameSingleton.Instance.Game.GetShopSlotCount())
                cardSlot.Hide();
        }
    }

    public CardSlotNode2D GetSelectedCardSlotNode2D()
    {
        for (int i = 1; i <= 5; i++)
        {
            var cardSlot = GetCardSlotNode2D(i);
            if (cardSlot.Selected)
                return cardSlot;
        }
        return null;
    }

    // ICardSelectHost
    public void SelectionChanged(CardSlotNode2D cardSlot)
    {
        if (cardSlot.Selected)
        {
            for (int i = 1; i <= 5; i++)
            {
                var tempCardSlot = GetCardSlotNode2D(i);
                if (tempCardSlot != cardSlot)
                    tempCardSlot.Selected = false;
            }
        }
    }

    // ICardSelectHost
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
                sourceCardArea2D.CardSlotNode2D.Selected = false;

                var targetCard = targetDeck.Deck[targetCardArea2D.CardIndex];
                // are we dropping onto an empty slot, or leveling up a card with same ability
                if (targetCard == null ||
                    targetCard.Ability.GetType() == sourceDeck.Deck[sourceCardArea2D.CardIndex].Ability.GetType())
                {
                    // select immediately before animations
                    targetCardArea2D.CardSlotNode2D.Selected = true;

                    // hide immediately since it's being dropped and animations are about 
                    // to be shown (e.g. if the bought card is buffed by an ability)
                    // we don't want the card shown in the shop during animations
                    sourceCardArea2D.HideCard();

                    int saveGold = BuildNode.Player.Gold;
                    GameSingleton.Instance.Game.BeginUpdate();
                    // BuyFromShop makes direct changes to the deck (stored in saveDeck) and then
                    // invokes ability methods which make further changes
                    GameSingleton.Instance.Game.BuyFromShop(sourceCardArea2D.CardIndex, targetCardArea2D.CardIndex, 
                        BuildNode.Player, out CardCommandQueue queue, out Deck saveDeck);
                    GameSingleton.Instance.Game.EndUpdate();
                    // manually invoke gold changed event since we disabled events
                    BuildNode.Player.OnGoldChangedEvent(saveGold);
                    BuildNode.DeckNode2D.PlayThump();
                    // render the bought card as it existed before ability methods were invoked
                    targetCardArea2D.RenderCard(saveDeck[targetCardArea2D.CardIndex], targetCardArea2D.CardIndex);
                    // restore the deck to prior state and then execute ability methods which spawn 
                    // animations and renders changes
                    BuildNode.ExecuteQueue(queue, saveDeck);

                    RenderShop();
                    BuildNode.RenderPlayerFood(); // in case we bought a Cow
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
