using Godot;
using System;
using AutoPets;

public class Main : Node
{
    Game _game;

    public override void _Ready()
    {
        _game = new Game();

        var card = new Card(_game.Player1.BuildDeck, new AntAbility());
        card.Summon(4);
        card = new Card(_game.Player1.BuildDeck, new CricketAbility());
        card.Summon(3);
        card = new Card(_game.Player1.BuildDeck, new HorseAbility());
        card.Summon(0);

        card = new Card(_game.Player2.BuildDeck, new MosquitoAbility());
        card.Summon(0);
        card = new Card(_game.Player2.BuildDeck, new FishAbility());
        card.Summon(1);
        card = new Card(_game.Player2.BuildDeck, new AntAbility());
        card.Summon(3);

        _game.Player1.NewBattleDeck();
        _game.Player2.NewBattleDeck();

        RenderDeck(GetNode<Node2D>("Player1"), _game.Player1.BattleDeck);
        RenderDeck(GetNode<Node2D>("Player2"), _game.Player2.BattleDeck);
    }

    void RenderDeck(Node2D godotDeck, Deck deck)
    {
        for (int i = 1; i <= 5; i++)
        {
            var card = deck[i - 1];
            var sprite = godotDeck.GetNode<Sprite>(string.Format("Sprite{0}", i));
            if (card == null)
                sprite.Hide();
            else
            {
                sprite.Show();
                Resource res = null;
                if (card.Ability is AntAbility)
                    res = GD.Load("res://ant.png");
                else if (card.Ability is HorseAbility)
                    res = GD.Load("res://horse.png");
                else if (card.Ability is CricketAbility)
                    res = GD.Load("res://cricket.png");
                else if (card.Ability is ZombieCricketAbility)
                    res = GD.Load("res://zombie.png");
                else if (card.Ability is FishAbility)
                    res = GD.Load("res://fish.png");
                sprite.Texture = res as Godot.Texture;
            }
        }
    }
}
