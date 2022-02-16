using Godot;
using AutoPets;

public class ShopNode2D : Node2D, IDragParent
{
    public BuildNode BuildNode { get { return GetParent() as BuildNode; } }

    public override void _Ready()
    {
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
                GameSingleton.Instance.Game.BuyFromShop(cardArea2D.CardIndex, targetCard.CardIndex, 
                    GameSingleton.Instance.BuildPlayer);
            }
        }
        RenderShop();
        BuildNode.Deck.RenderDeck(GameSingleton.Instance.BuildPlayer.BuildDeck);
        BuildNode.Deck.PlayThump();
    }

    public bool GetCanDrag()
    {
        return GameSingleton.Instance.BuildPlayer.Gold >= Game.PetCost;
    }
}
