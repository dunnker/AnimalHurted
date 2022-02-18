using System;
using System.Threading;
using Godot;
using AutoPets;

public class ShopNode2D : Node2D, IDragParent
{
    System.Threading.Thread _gameThread;

    public BuildNode BuildNode { get { return GetParent() as BuildNode; } }

    [Signal]
    public delegate void CardBoughtSignal();

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        // Dispose can be called from Godot editor so check if thread exists
        if (_gameThread != null)
            _gameThread.Abort();
    }

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
            // did we drop onto the build deck?
            if (cardParent is DeckNode2D)
            {
                // hide immediately since it's being dropped and animations are about 
                // to be shown (e.g. if the bought card is buffed by an ability)
				// we don't want the card shown in the shop during animations
                cardArea2D.HideCard();

                _gameThread = new System.Threading.Thread(() => 
                {
                    // from here events can be invoked in DeckNode2D, which send
                    // signals on main thread to render changes
                    GameSingleton.Instance.Game.BuyFromShop(cardArea2D.CardIndex, targetCard.CardIndex, 
                        GameSingleton.Instance.BuildPlayer);
                    // notify the scene that the thread is finished
					// assuming "this" is still valid. See Dispose method where thread is aborted
                    this.EmitSignal("CardBoughtSignal");
                });
                _gameThread.Start();
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
        // summoned event and the deck scene has rendered it
        //BuildNode.Deck.RenderDeck(GameSingleton.Instance.BuildPlayer.BuildDeck);
        BuildNode.Deck.PlayThump();
    }
}
