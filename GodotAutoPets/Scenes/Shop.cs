using Godot;
using AutoPets;

public class Shop : Node2D, IDragParent
{
    public Build Build { get { return GetParent() as Build; } }

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
            var gdCard = cardSlot.GetNode<global::Card>("Card");
            gdCard.RenderCard(card, i);
            if (i >= GameSingleton.Instance.Game.ShopSlots)
                cardSlot.Hide();
        }
    }

    public override void _Process(float delta)
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
                GameSingleton.Instance.Game.BuyFromShop(card.CardIndex, targetCard.CardIndex, 
                    GameSingleton.Instance.Game.Player1);
            }
        }
        RenderShop();
        Build.Deck.RenderDeck(GameSingleton.Instance.Game.Player1.BuildDeck);
    }
}
