using Godot;
using AutoPets;

public class Shop : Node2D
{
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
            gdCard.RenderCard(card);
            if (i >= GameSingleton.Instance.Game.ShopSlots)
                cardSlot.Hide();
        }
    }

    public override void _Process(float delta)
    {
        
    }
}
