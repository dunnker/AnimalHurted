using Godot;
using AutoPets;

public class ShopNode2D : Node2D, IDragParent
{
    public BuildNode Build { get { return GetParent() as BuildNode; } }

    public override void _Ready()
    {
        RenderShop();        
    }

    public void RenderShop()
    {
        for (int i = 0; i < GameSingleton.Instance.Game.Player1.ShopDeck.Size; i++)
        {
            var card = GameSingleton.Instance.Game.Player1.ShopDeck[i];
            var cardSlot = GetNode<Node2D>(string.Format("CardSlot{0}", i + 1));
            var gdCard = cardSlot.GetNode<global::CardArea2D>("Card");
            gdCard.RenderCard(card, i);
            if (i >= GameSingleton.Instance.Game.ShopSlots)
                cardSlot.Hide();
        }
    }

    public override void _Process(float delta)
    {
        
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
                GameSingleton.Instance.Game.BuyFromShop(card.CardIndex, targetCard.CardIndex, 
                    GameSingleton.Instance.Game.Player1);
            }
        }
        RenderShop();
        Build.Deck.RenderDeck(GameSingleton.Instance.Game.Player1.BuildDeck);
        Build.Deck.PlayThump();
    }

    public bool GetCanDrag()
    {
        return GameSingleton.Instance.Game.Player1.Gold >= Game.PetCost;
    }
}
