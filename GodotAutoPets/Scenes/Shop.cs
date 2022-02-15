using Godot;
using AutoPets;using System;

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
            var gdCard = cardSlot.GetNode("Card");
            var sprite = gdCard.GetNode<Sprite>("Sprite");
            if (card == null)
                sprite.Hide();
            else
            {
                int index = AbilityList.Instance.AllAbilities.IndexOf(card.Ability);
                var res = GD.Load(string.Format("res://Assets/Pets/{0}.png", index));
                sprite.Texture = res as Godot.Texture;
                sprite.Show();
            }
            if (i >= GameSingleton.Instance.Game.ShopSlots)
                cardSlot.Hide();
        }
    }

    public override void _Process(float delta)
    {
        
    }
}
