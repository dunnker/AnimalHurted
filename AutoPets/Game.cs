using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AutoPets
{
    public delegate void CardEventHandler(object sender, Card card, int index);
    public delegate void CardBuffedEventHandler(object sender, Card card, int sourceIndex);
    public delegate void CardHurtEventHandler(object sender, Card card, Deck sourceDeck, int sourceIndex);

    public class Game
    {
        readonly Random _random;
        readonly Player _player1;
        readonly Player _player2;
        int _round;
        int _shopSlots;
        List<Ability> _tierAbilities;
        List<Food> _tierFood;
        int _updateCount;
        bool _fighting;

        public const int PetCost = 3;
        public const int FoodCost = 3;
        public const int RollCost = 1;
        public const int GoldPerTurn = 10;
        public const int BuildDeckSlots = 5;
        public const int ShopMaxPetSlots = 5;
        public const int ShopFoodSlots = 2;

        public int ShopSlots { get { return _shopSlots; } }

        public Random Random { get { return _random; } }

        public int Round { get { return _round; } }

        public Player Player1 { get { return _player1; } }
        public Player Player2 { get { return _player2; } }
        public bool Fighting { get { return _fighting; } }
        public List<Ability> TierAbilities { get { return _tierAbilities; } }
        public List<Food> TierFood { get { return _tierFood; } }

        public event EventHandler AttackEvent;
        public event CardEventHandler CardFaintedEvent;
        public event CardEventHandler CardSummonedEvent;
        public event CardBuffedEventHandler CardBuffedEvent;
        public event CardHurtEventHandler CardHurtEvent;

        public void OnAttackEvent()
        {
            if (_updateCount == 0)
                AttackEvent?.Invoke(this, EventArgs.Empty);
        }

        public void OnCardFaintedEvent(Card card, int index)
        {
            if (_updateCount == 0)
                CardFaintedEvent?.Invoke(this, card, index);
        }

        public void OnCardSummonedEvent(Card card, int index)
        {
            if (_updateCount == 0)
                CardSummonedEvent?.Invoke(this, card, index);
        }

        public void OnCardBuffedEvent(Card card, int sourceIndex)
        {
            if (_updateCount == 0)
                CardBuffedEvent?.Invoke(this, card, sourceIndex);
        }

        public void OnCardHurtEvent(Card card, Deck sourceDeck, int sourceIndex)
        {
            if (_updateCount == 0)
                CardHurtEvent?.Invoke(this, card, sourceDeck, sourceIndex);
        }

        public Game()
        {
            _random = new Random();
            _player1 = new Player(this, "Player 1");
            _player2 = new Player(this, "Player 2");
            NewGame();
        }

        public void NewGame()
        {
            _tierAbilities = new List<Ability>();
            _tierFood = new List<Food>();
            _player1.NewGame();
            _player2.NewGame();
            NewRound();
        }

        public void CheckNewTier()
        {
            switch (_round)
            {
                case int i when i >= 1 && i <= 2:
                    _shopSlots = 3;
                    _tierAbilities.AddRange(AbilityList.Instance.TierOneAbilities);
                    _tierFood.AddRange(FoodList.Instance.TierOneFood);
                    break;
                case int i when i >= 3 && i <= 4:
                    _tierAbilities.AddRange(AbilityList.Instance.TierTwoAbilities);
                    _tierFood.AddRange(FoodList.Instance.TierTwoFood);
                    break;
                case int i when i >= 5 && i <= 6:
                    _shopSlots = 4;
                    _tierAbilities.AddRange(AbilityList.Instance.TierThreeAbilities);
                    _tierFood.AddRange(FoodList.Instance.TierThreeFood);
                    break;
                case int i when i >= 7 && i <= 8:
                    _tierAbilities.AddRange(AbilityList.Instance.TierFourAbilities);
                    _tierFood.AddRange(FoodList.Instance.TierFourFood);
                    break;
                case int i when i >= 9 && i <= 10:
                    _shopSlots = 5;
                    _tierAbilities.AddRange(AbilityList.Instance.TierFiveAbilities);
                    _tierFood.AddRange(FoodList.Instance.TierFiveFood);
                    break;
                case int i when i >= 11:
                    _tierAbilities.AddRange(AbilityList.Instance.TierSixAbilities);
                    _tierFood.AddRange(FoodList.Instance.TierSixFood);
                    break;
                default:
                    break;
            }
        }

        public void NewRound()
        {
            _round++;
            CheckNewTier();
            // assign Gold before calling Player.NewRound() because card abilities
            // can be invoked in Player.NewRound() which can buff Gold
            _player1.Gold = GoldPerTurn;
            _player2.Gold = GoldPerTurn;
            _player1.NewRound(_player1.BattleDeck.GetCardCount() > 0, 
                _player1.BattleDeck.GetCardCount() == 0 && _player2.BattleDeck.GetCardCount() > 0, _round);
            _player2.NewRound(_player2.BattleDeck.GetCardCount() > 0, 
                _player2.BattleDeck.GetCardCount() == 0 && _player1.BattleDeck.GetCardCount() > 0, _round);
            _player1.Roll(deductGold: false);
            _player2.Roll(deductGold: false);
        }

        public void BuyFromShop(int shopIndex, int buildIndex, Player player)
        {
            if (player.Gold < Game.PetCost)
                throw new Exception("Not enough gold to buy pet");
            else
            {
                if (player.BuildDeck[buildIndex] == null)
                {
                    var card = new Card(player.BuildDeck, player.ShopDeck[shopIndex]);
                    card.Buy(buildIndex);
                    player.ShopDeck.Remove(shopIndex);

                    var queue = new CardCommandQueue();
                    foreach (var c in player.BuildDeck)
                        if (c != card)
                            c.Ability.FriendSummoned(queue, c, card);
                    var nextQueue = new CardCommandQueue();
                    foreach (var command in queue)
                    {
                        command.Execute().ExecuteAbility(nextQueue);
                    }
					// nextQueue should be empty because there should be no abilities invoked
					// from buying a card; an exception here would mean we need to continue
                    // processing abilities similar to Game.CreateFightResult()
					if (nextQueue.Count > 0)
						throw new Exception("Unprocessed abilities were found.");
                }
                else
                {
                    if (player.ShopDeck[shopIndex].Ability == player.BuildDeck[buildIndex].Ability)
                        player.BuildDeck[buildIndex].GainXP(player.ShopDeck[shopIndex]);
                }
                player.Gold -= Game.PetCost;
            }
        }

        public void BeginUpdate()
        {
            _player1.BeginUpdate();
            _player2.BeginUpdate();
            _updateCount++;
        }

        public void EndUpdate()
        {
            _player1.EndUpdate();
            _player2.EndUpdate();
            _updateCount--;
        }

        public bool IsFightOver()
        {
            // fight is over if either player has run out of cards
            return _player1.BattleDeck.GetCardCount() == 0 || _player2.BattleDeck.GetCardCount() == 0;
        }

        public void FightOver()
        {
            _fighting = false;
        }

        public CardCommandQueue CreateAttackResult(CardCommandQueue lastQueue = null)
        {
            var resultQueue = new CardCommandQueue();
            if (!_fighting)
            {
                _fighting = true;
                foreach (var card in _player1.BattleDeck)
                    card.Ability.BattleStarted(resultQueue, card);
                foreach (var card in _player2.BattleDeck)
                    card.Ability.BattleStarted(resultQueue, card);
            }
            else
            {
                if (lastQueue != null)
                {
                    foreach (var command in lastQueue)
                    {
                        command.Execute();
                        command.ExecuteAbility(resultQueue);
                    }
                }
            }
            // if there were no ability methods invoked then start a new card attack
            if (resultQueue.Count == 0)
                if (_player1.BattleDeck.GetCardCount() > 0 && _player2.BattleDeck.GetCardCount() > 0)
                    _player1.BattleDeck.GetLastCard().Attack(resultQueue, _player2.BattleDeck.GetLastCard());
            return resultQueue;
        }

        public List<CardCommandQueue> CreateFightResult()
        {
            // disable events
            BeginUpdate();
            _player1.NewBattleDeck();
            _player2.NewBattleDeck();
            var fightResult = new List<CardCommandQueue>();
            CardCommandQueue lastQueue = null;
            do
            {
                lastQueue = CreateAttackResult(lastQueue);
                if (lastQueue.Count > 0)
                    fightResult.Add(lastQueue);
            } while (lastQueue.Count > 0 || !IsFightOver());
            FightOver();
            // restore battle decks
            _player1.NewBattleDeck();
            _player2.NewBattleDeck();
            EndUpdate();
            return fightResult;
        }
    }
}
