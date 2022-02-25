using System;
using System.Threading;
using Godot;
using AutoPets;

public class BuildNode : Node
{
    Player _player;
    System.Threading.Thread _gameThread;
    bool _pauseBeforeContinue;

    public ShopNode2D ShopNode2D { get { return GetNode<ShopNode2D>("ShopNode2D"); } }

    public DeckNode2D DeckNode2D { get { return GetNode<global::DeckNode2D>("DeckNode2D"); } }

    public Player Player { get { return _player; } }

    public Label GoldLabel { get { return GetNode<Label>("PlayerAttrsNode2D/GoldLabel"); } }
    public Label LivesLabel { get { return GetNode<Label>("PlayerAttrsNode2D/LivesLabel"); } }
    public Label WinsLabel { get { return GetNode<Label>("PlayerAttrsNode2D/WinsLabel"); } }
    public Label RoundLabel { get { return GetNode<Label>("PlayerAttrsNode2D/RoundLabel"); } }
    public Label PlayerNameLabel { get { return GetNode<Label>("PlayerAttrsNode2D/PlayerNameLabel"); } } 

    [Signal]
    public delegate void SellOverSignal();

    [Signal]
    public delegate void RoundOverSignal();

    public void _on_QuitGameButton_pressed()
    {
        GetTree().ChangeScene("res://Scenes/MainNode.tscn");
    }

    public void _on_RollButton_pressed()
    {
        if (_player.Gold >= Game.RollCost)
            _player.Roll();
        ShopNode2D.RenderShop();
        RenderFood(1, _player.ShopFood1);
        RenderFood(2, _player.ShopFood2);
    }

    public void _on_ContinueButton_pressed()
    {
        GetNode<Button>("ContinueButton").Disabled = true;

        _gameThread = new System.Threading.Thread(() => 
        {
            // from here events can be invoked in DeckNode2D, which send
            // signals on main thread to render changes
            var queue = new CardCommandQueue();
            _player.BuildEnded(queue);
            if (queue.Count > 0)
            {
                _pauseBeforeContinue = true;
                queue.Execute();
            }

            this.EmitSignal("RoundOverSignal");
        });
        _gameThread.Start();
    }

    public async void _signal_RoundOver()
    {
        if (_pauseBeforeContinue)
            await ToSignal(GetTree().CreateTimer(1f), "timeout");
        _player.GoldChangedEvent -= _GoldChangedEvent;

        if (_player == GameSingleton.Instance.Game.Player1)
        {
            GameSingleton.Instance.BuildNodePlayer = GameSingleton.Instance.Game.Player2;
            GetTree().ChangeScene("res://Scenes/BuildNode.tscn");
        }
        else
            GetTree().ChangeScene("res://Scenes/BattleNode.tscn");
    }

    public void _on_SellButton_pressed()
    {
        var cardSlot = DeckNode2D.GetSelectedCardSlotNode2D();
        if (cardSlot != null)
        {
            var card = _player.BuildDeck[cardSlot.CardArea2D.CardIndex];
            cardSlot.CardArea2D.RenderCard(null, card.Index);
            cardSlot.Selected = false;
            _gameThread = new System.Threading.Thread(() => 
            {
                // from here events can be invoked in DeckNode2D, which send
                // signals on main thread to render changes
                card.Sell();

                this.EmitSignal("SellOverSignal");
            });
            _gameThread.Name = "Sell Game Thread";
            _gameThread.Start();
        }
    }

    public void _signal_SellOver()
    {
		// in case a Duck was sold, refresh the shop
        ShopNode2D.RenderShop();
    }

    public void _on_ShopPetOKButton_pressed()
    {
        var typeName = $"AutoPets.{GetNode<LineEdit>("ShopPetLineEdit").Text}Ability, AutoPets, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"; 
        var type = Type.GetType(typeName);
        if (type != null)
        {
            var ability = AbilityList.Instance.AllAbilities.Find((x) => x.GetType() == type);
            if (ability != null)
            {
                if (ShopNode2D.Deck[0] != null)
                    ShopNode2D.Deck.Remove(0);
                var card = new Card(ShopNode2D.Deck, ability);
                card.Summon(0);
            }
            ShopNode2D.RenderShop();
        }
        GetNode<Button>("ShopPetOKButton").Hide();
        GetNode<LineEdit>("ShopPetLineEdit").Hide();
    }

    public override void _Input(InputEvent @event)
    {
        #if CHEATS_ENABLED
        if (Input.IsActionPressed("give_gold"))
        {
            _player.Gold += 1;
        }
        if (Input.IsActionPressed("give_shop_pet"))
        {
            GetNode<Button>("ShopPetOKButton").Show();
            GetNode<LineEdit>("ShopPetLineEdit").Show();
        }
        #endif
    } 
    
    public override void _Ready()
    {
        // using singleton to init this scene
        // this is because GetTree().ChangeScene(...) is a deferred call and we can't
        // pass parameters to the new scene instance
        _player = GameSingleton.Instance.BuildNodePlayer;

        _player.GoldChangedEvent += _GoldChangedEvent;
        GoldLabel.Text = _player.Gold.ToString();
        LivesLabel.Text = _player.Lives.ToString();
        WinsLabel.Text = _player.Wins.ToString();
        RoundLabel.Text = GameSingleton.Instance.Game.Round.ToString();
        PlayerNameLabel.Text = _player.Name;
        DeckNode2D.RenderDeck(_player.BuildDeck);
        ShopNode2D.RenderShop();
        RenderFood(1, _player.ShopFood1);
        RenderFood(2, _player.ShopFood2);

        Connect("SellOverSignal", this, "_signal_SellOver", null, 
            (int)ConnectFlags.Deferred);
        Connect("RoundOverSignal", this, "_signal_RoundOver", null, 
            (int)ConnectFlags.Deferred);
    }

    public void RenderFood(int index, Food playerFood)
    {
        var foodSlot = GetNode($"FoodSlotNode2D{index}");
        var foodArea2D = foodSlot.GetNode<FoodArea2D>("Area2D");
        foodArea2D.Index = index;
        var sprite = foodSlot.GetNode<Sprite>("Area2D/Sprite");
        if (playerFood == null)
            sprite.Hide();
        else
        {
            var res = GD.Load($"res://Assets/Food/{playerFood.GetType().Name}.png");
            sprite.Texture = res as Godot.Texture;
            sprite.Show();
        }
    }

    public void _GoldChangedEvent(object sender, int oldValue)
    {
        GoldLabel.Text = _player.Gold.ToString();
    }
}
