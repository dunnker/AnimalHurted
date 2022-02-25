using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AutoPets
{
    public delegate void GoldChangedEventHandler(object sender, int oldValue);

    public class Player
    {
        readonly Game _game;
        int _lives;
        int _wins;
        int _gold;
        readonly Deck _shopDeck;
        readonly Deck _buildDeck;
        Food _shopFood1;
        Food _shopFood2;
        Deck _battleDeck;
        int _updateCount;

        public Game Game { get { return _game; } }

        public Deck BuildDeck { get { return _buildDeck; } }

        public Deck BattleDeck { get { return _battleDeck; } }

        public Deck ShopDeck { get { return _shopDeck; } }

        public Food ShopFood1 { get { return _shopFood1; } }
        public Food ShopFood2 { get { return _shopFood2; } }

        public int Gold { 
            get
            {
                return _gold;
            } 
            
            set
            {
                int oldValue = _gold;
                _gold = value;
                OnGoldChangedEvent(oldValue);
            } 
        }

        public int Lives { get { return _lives; } }

        public int Wins { get { return _wins; } }

        public string Name { get; set; }

        public event GoldChangedEventHandler GoldChangedEvent;

        public void OnGoldChangedEvent(int oldValue)
        {
            if (_updateCount == 0)
                GoldChangedEvent?.Invoke(this, oldValue);
        }

        public Player(Game game, string name)
        {
            Name = name;
            _game = game;
            _shopDeck = new Deck(this, Game.ShopMaxPetSlots);
            _buildDeck = new Deck(this, Game.BuildDeckSlots);
            _battleDeck = new Deck(this, Game.BuildDeckSlots);
        }

        public void BeginUpdate()
        {
            _updateCount++;
        }

        public void EndUpdate()
        {
            _updateCount--;
        }

        public Player GetOpponentPlayer()
        {
            if (Game.Player1 == this)
                return Game.Player2;
            else if (Game.Player2 == this)
                return Game.Player1;
            else
            {
                Debug.Assert(false);
                return null;
            }
        }

        public void NewGame()
        {
            _wins = 0;
            _lives = 10;
        }

        public void NewBattleDeck()
        {
            // other classes may keep a reference to the battle deck, e.g. CardCommand
            // so CloneTo creates new card instances for the deck, but the deck reference
            // itself remains
            _buildDeck.CloneTo(_battleDeck);
        }

        public void Roll(bool deductGold = true)
        {
            if (Gold < Game.RollCost)
                throw new Exception("Not enough gold for Roll.");
            if (deductGold)
                Gold -= Game.RollCost;
            _shopDeck.Clear();
            for (int i = 0; i < Game.ShopSlots; i++)
            {
                int rand = Game.Random.Next(Game.TierAbilities.Count);
                _shopDeck.SetCard(new Card(_shopDeck, Game.TierAbilities[rand]), i);
            }
            NewShopFood();
        }

        public void BuyFood(Card card, int index)
        {
            Food food;
            if (index == 1)
                food = _shopFood1;
            else
                food = _shopFood2;
            int foodCost = Game.FoodCost;
            if (food is SleepingPillFood)
                foodCost = 1;
            if (Gold < foodCost)
                throw new Exception("Not enough gold to buy food.");
            var queue = new CardCommandQueue();
            card.Eat(queue, food);
            queue.Execute();
            if (index == 1)
                _shopFood1 = null;
            else
                _shopFood2 = null;
            Gold -= foodCost;
        }

        void NewShopFood()
        {
            _shopFood1 = _game.TierFood[_game.Random.Next(0, _game.TierFood.Count)];
            _shopFood2 = _game.TierFood[_game.Random.Next(0, _game.TierFood.Count)];
        }

        public void BuildEnded(CardCommandQueue queue)
        {
            foreach (var c in _buildDeck)
                c.BuildEnded(queue);
        }

        public void NewRound(bool won, bool lost, int round)
        {
            if (won)
                _wins += 1;
            else if (lost)
            {
                if (round == 1 || round == 2)
                    _lives -= 1;
                else if (round == 3 || round == 4)
                    _lives -= 2;
                else
                    _lives -= 3;
                if (_lives < 0)
                    _lives = 0;
            };
            foreach (var c in _buildDeck)
                c.NewRound();
        }
    }
}
