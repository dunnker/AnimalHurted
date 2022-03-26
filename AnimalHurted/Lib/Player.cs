using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AnimalHurtedLib
{
    public delegate void GoldChangedEventHandler(object sender, int oldValue);

    public class Player
    {
        readonly Game _game;
        int _lives;
        int _wins;
        int _gold;
        Deck _shopDeck;
        Deck _buildDeck;
        Food _shopFood1;
        Food _shopFood2;
        Deck _battleDeck;
        int _updateCount;
        bool _lostLastBattle;

        public Game Game { get { return _game; } }

        public Deck BuildDeck { get { return _buildDeck; } }

        public Deck BattleDeck { get { return _battleDeck; } }

        public Deck ShopDeck { get { return _shopDeck; } }

        public Food ShopFood1 { get { return _shopFood1; } set { _shopFood1 = value; } }
        public Food ShopFood2 { get { return _shopFood2; } set { _shopFood2 = value; } }

        public int BuffHitPoints { get; set; }
        public int BuffAttackPoints { get; set; }

        public bool LostLastBattle { get { return _lostLastBattle; } }

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

        public void CloneTo(Player player)
        {
            // Name and Game should already be set
            player._wins = _wins;
            player._lives = _lives;
            player._gold = _gold;
            player._lostLastBattle = _lostLastBattle;
            _buildDeck.CloneTo(player._buildDeck);
            _shopDeck.CloneTo(player._shopDeck);
            // food doesn't keep any state except Cost
            if (_shopFood1 != null)
            {
                player._shopFood1 = Activator.CreateInstance(_shopFood1.GetType()) as Food;
                player._shopFood1.Cost = _shopFood1.Cost;
            }
            if (_shopFood2 != null)
            {
                player._shopFood2 = Activator.CreateInstance(_shopFood2.GetType()) as Food;
                player._shopFood2.Cost = _shopFood2.Cost;
            }
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
            BuffAttackPoints = 0;
            BuffHitPoints = 0;
        }

        public void NewBattleDeck()
        {
            // other classes may keep a reference to the battle deck, e.g. CardCommand
            // so CloneTo creates new card instances for the deck, but the deck reference
            // itself remains
            _buildDeck.CloneTo(_battleDeck);
            foreach (var card in _battleDeck)
                card.Ability.NewBattleDeck(card);
        }

        public void BattleStarted1(CardCommandQueue queue)
        {
            foreach (var card in _battleDeck)
                card.Ability.BattleStarted1(queue, card);
        }

        public void BattleStarted2(CardCommandQueue queue)
        {
            foreach (var card in _battleDeck)
                card.Ability.BattleStarted2(queue, card);
        }

        public void BuffShopDeck()
        {
            foreach (var card in _shopDeck)
            {
                card.HitPoints += BuffHitPoints;
                card.AttackPoints += BuffAttackPoints;
            }
        }

        public void Roll(bool deductGold = true)
        {
            if (Gold < Game.RollCost)
                throw new Exception("Not enough gold for Roll.");
            if (deductGold)
                Gold -= Game.RollCost;
            foreach (var card in _shopDeck)
                if (!card.Frozen)
                    card.Deck.Remove(card.Index);
            for (int i = 0; i < Game.GetShopSlotCount(); i++)
            {
                if (_shopDeck[i] == null)
                {
                    var abilityList = AbilityList.Instance.GetAbilityListForRound(Game.Round);
                    int rand = Game.Random.Next(abilityList.Count);
                    var card = new Card(_shopDeck, Activator.CreateInstance(abilityList[rand]) as Ability);
                    card.HitPoints += BuffHitPoints;
                    card.AttackPoints += BuffAttackPoints;
                    _shopDeck.SetCard(card, i);
                }
            }
            NewShopFood();
        }

        public Food GetShopFoodFromIndex(int foodIndex)
        {
            if (foodIndex == 1)
                return _shopFood1;
            else
                return _shopFood2;
        }

        public void BuyFood(Card card, int foodIndex)
        {
            Food food = GetShopFoodFromIndex(foodIndex);
            if (Gold < food.Cost)
                throw new Exception("Not enough gold to buy food.");
            card.Eat(food);
            if (foodIndex == 1)
                _shopFood1 = null;
            else
                _shopFood2 = null;
            Gold -= food.Cost;
        }

        void NewShopFood()
        {
            var foodList = FoodList.Instance.GetFoodListForRound(_game.Round);
            int randIndex = _game.Random.Next(foodList.Count);
            _shopFood1 = Activator.CreateInstance(foodList[randIndex]) as Food;
            randIndex = _game.Random.Next(foodList.Count);
            _shopFood2 = Activator.CreateInstance(foodList[randIndex]) as Food;
        }

        public void BuildEnded(CardCommandQueue queue)
        {
            foreach (var c in _buildDeck)
                c.Ability.RoundEnded(queue, c);
        }

        public FightResultEnum GetFightResultEnum(Player opponentPlayer)
        {
            if (_battleDeck.GetCardCount() > 0)
                return FightResultEnum.Won;
            else if (opponentPlayer.BattleDeck.GetCardCount() > 0)
                return FightResultEnum.Lost;
            else
                return FightResultEnum.Draw;
        }

        public void NewRound(FightResultEnum fightResultEnum, int round)
        {
            Gold = Game.GoldPerTurn;
            Roll(deductGold: false);
            _lostLastBattle = false;
            if (fightResultEnum == FightResultEnum.Won)
                _wins += 1;
            else if (fightResultEnum == FightResultEnum.Lost)
            {
                _lostLastBattle = true;
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
