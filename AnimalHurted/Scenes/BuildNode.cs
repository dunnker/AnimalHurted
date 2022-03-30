using System;
using System.IO;
using Godot;
using AnimalHurtedLib;

public class BuildNode : Node, IBattleNode
{
    Player _player;
    CardCommandQueueReader _reader;

    public ShopNode2D ShopNode2D { get { return GetNode<ShopNode2D>("ShopNode2D"); } }

    public DeckNode2D DeckNode2D { get { return GetNode<global::DeckNode2D>("DeckNode2D"); } }

    public Player Player { get { return _player; } }

    // IBattleNode
    public float MaxTimePerEvent { get; set; } = BattleNode.DefaultMaxTimePerEvent;
    public CardCommandQueueReader Reader { get { return _reader; } }
    // IBattleNode

    public Label GoldLabel { get { return GetNode<Label>("PlayerAttrsNode2D/GoldLabel"); } }
    public Label LivesLabel { get { return GetNode<Label>("PlayerAttrsNode2D/LivesLabel"); } }
    public Label WinsLabel { get { return GetNode<Label>("PlayerAttrsNode2D/WinsLabel"); } }
    public Label RoundLabel { get { return GetNode<Label>("PlayerAttrsNode2D/RoundLabel"); } }
    public Label PlayerNameLabel { get { return GetNode<Label>("PlayerAttrsNode2D/PlayerNameLabel"); } } 

    [Signal]
    public delegate void ExecuteQueueOverSignal();

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

    public async void _on_ContinueButton_pressed()
    {
        GetNode<Button>("ContinueButton").Disabled = true;

        var queue = new CardCommandQueue();
        var savedDeck = CreateSaveDeck();
        GameSingleton.Instance.Game.BeginUpdate();
        _player.BuildEnded(queue);
        GameSingleton.Instance.Game.EndUpdate();
        if (queue.Count > 0)
        {
            ExecuteQueue(queue, savedDeck);
            // if cards were buffed, give a short pause to let user be aware of animations
            await ToSignal(GetTree().CreateTimer(1f), "timeout");
        }

        if (GameSingleton.Instance.VersusAI)
        {
            GameSingleton.Instance.BuildNodePlayer = GameSingleton.Instance.Game.Player2;
            GetTree().ChangeScene("res://Scenes/AIProgressNode.tscn");
        }
        else if (_player == GameSingleton.Instance.Game.Player1)
        {
            GameSingleton.Instance.BuildNodePlayer = GameSingleton.Instance.Game.Player2;
            GetTree().ChangeScene("res://Scenes/BuildNode.tscn");
        }
        else
            BuildNode.StartBattle(this);
    }

    public static void StartBattle(Node node)
    {
        GameSingleton.Instance.Game.Player1.NewBattleDeck();
        GameSingleton.Instance.Game.Player2.NewBattleDeck();

        /*using (var fileStream = new FileStream(@".battles\animalhurted.ah", FileMode.Create))
        {
            using (var writer = new BinaryWriter(fileStream))
            {
                GameSingleton.Instance.Game.Player1.BattleDeck.SaveToStream(writer);
                GameSingleton.Instance.Game.Player2.BattleDeck.SaveToStream(writer);
            }
        }*/

        GameSingleton.Instance.SaveBattleDecks();
        GameSingleton.Instance.FightResult = GameSingleton.Instance.Game.CreateFightResult();

        // NewRound() calculates the winner, so do this now so winner can be displayed in battle screen
        GameSingleton.Instance.Game.NewRound();

        // restore for rendering in next scene
        GameSingleton.Instance.RestoreBattleDecks();

        node.GetTree().ChangeScene("res://Scenes/BattleNode.tscn");
    }

    public void _on_SellButton_pressed()
    {
        var cardSlot = DeckNode2D.GetSelectedCardSlotNode2D();
        if (cardSlot != null)
        {
            var card = _player.BuildDeck[cardSlot.CardArea2D.CardIndex];
            cardSlot.CardArea2D.RenderCard(null, card.Index);
            cardSlot.Selected = false;

            card.Sell();
            var savedDeck = CreateSaveDeck();
            var queue = new CardCommandQueue();
            GameSingleton.Instance.Game.BeginUpdate();
            card.Sold(queue, cardSlot.CardArea2D.CardIndex);
            GameSingleton.Instance.Game.EndUpdate();
            ExecuteQueue(queue, savedDeck);

            // in case a Duck was sold, refresh the shop
            ShopNode2D.RenderShop();
        }
    }

    public void _on_FreezeButton_pressed()
    {
        var cardSlot = ShopNode2D.GetSelectedCardSlotNode2D();
        if (cardSlot != null)
        {
            var card = _player.ShopDeck[cardSlot.CardArea2D.CardIndex];
            card.Frozen = !card.Frozen;
            cardSlot.CardArea2D.RenderCard(card, card.Index, false);
        }
    }    

    public Deck CreateSaveDeck()
    {
        var saveDeck = new Deck(_player, Game.BuildDeckSlots);
        _player.BuildDeck.CloneTo(saveDeck); 
        return saveDeck;       
    }

    public void ExecuteQueue(CardCommandQueue queue, Deck savedDeck)
    {
        if (queue.Count > 0)
        {
            var list = queue.CreateExecuteResult(GameSingleton.Instance.Game);
            savedDeck.CloneTo(_player.BuildDeck);
            if (list.Count > 0)
            {
                _reader = new CardCommandQueueReader(this, list, "ExecuteQueueOverSignal");
                _reader.Execute();
            }
        }
    }

    public void _signal_ExecuteQueueOver()
    {
        if (!_reader.Finished)
            _reader.Execute();
    }

    public void _on_ShopPetOKButton_pressed()
    {
        var typeName = $"AnimalHurtedLib.{GetNode<LineEdit>("ShopPetLineEdit").Text}Ability, AnimalHurted, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"; 
        var type = Type.GetType(typeName);
        if (type != null)
        {
            var ability = Activator.CreateInstance(type) as Ability;
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
    
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        // Dispose can be called from Godot editor, and our singleton
        // may not have a Game when designing
        if (GameSingleton.Instance.Game != null)
        {
            _player.GoldChangedEvent -= _GoldChangedEvent;
        }
    }

    public override void _Ready()
    {
        // using singleton to init this scene
        // this is because GetTree().ChangeScene(...) is a deferred call and we can't
        // pass parameters to the new scene instance
        if (GameSingleton.Instance.Game != null)
        {
            _player = GameSingleton.Instance.BuildNodePlayer;

            _player.GoldChangedEvent += _GoldChangedEvent;
            GoldLabel.Text = _player.Gold.ToString();
            LivesLabel.Text = _player.Lives.ToString();
            WinsLabel.Text = _player.Wins.ToString();
            RoundLabel.Text = GameSingleton.Instance.Game.Round.ToString();
            PlayerNameLabel.Text = _player.Name;
            DeckNode2D.RenderDeck(_player.BuildDeck);
            ShopNode2D.RenderShop();
            RenderPlayerFood();
        }

        Connect("ExecuteQueueOverSignal", this, "_signal_ExecuteQueueOver", null, 
            (int)ConnectFlags.Deferred);
    }

    public void RenderPlayerFood()
    {
        RenderFood(1, _player.ShopFood1);
        RenderFood(2, _player.ShopFood2);
    }

    void RenderFood(int index, Food playerFood)
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
