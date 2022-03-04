using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AnimalHurtedLib
{
    public delegate void CardCommandEventHandler(object sender, CardCommand command);

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

        public event CardCommandEventHandler AttackEvent;
        public event CardCommandEventHandler CardFaintedEvent;
        public event CardCommandEventHandler CardSummonedEvent;
        public event CardCommandEventHandler CardBuffedEvent;
        public event CardCommandEventHandler CardHurtEvent;
        public event CardCommandEventHandler CardGainedFoodAbilityEvent;
        public event CardCommandEventHandler CardsMovedEvent;

        public void OnAttackEvent(CardCommand command)
        {
            if (_updateCount == 0)
                AttackEvent?.Invoke(this, command);
        }

        public void OnCardsMovedEvent(CardCommand command)
        {
            if (_updateCount == 0)
                CardsMovedEvent?.Invoke(this, command);
        }

        public void OnCardFaintedEvent(CardCommand command, Deck deck, int index)
        {
            if (_updateCount == 0)
                CardFaintedEvent?.Invoke(this, command);
        }

        public void OnCardSummonedEvent(CardCommand command)
        {
            if (_updateCount == 0)
                CardSummonedEvent?.Invoke(this, command);
        }

        public void OnCardBuffedEvent(CardCommand command)
        {
            if (_updateCount == 0)
                CardBuffedEvent?.Invoke(this, command);
        }

        public void OnCardHurtEvent(CardCommand command, Card card)
        {
            if (_updateCount == 0)
                CardHurtEvent?.Invoke(this, command);
        }

        public void OnCardGainedFoodAbilityEvent(CardCommand command)
        {
            if (_updateCount == 0)
                CardGainedFoodAbilityEvent?.Invoke(this, command);
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
            _player1.Roll(deductGold: false);
            _player2.Roll(deductGold: false);
            _player1.NewRound(_player1.BattleDeck.GetCardCount() > 0, 
                _player1.BattleDeck.GetCardCount() == 0 && _player2.BattleDeck.GetCardCount() > 0, _round);
            _player2.NewRound(_player2.BattleDeck.GetCardCount() > 0, 
                _player2.BattleDeck.GetCardCount() == 0 && _player1.BattleDeck.GetCardCount() > 0, _round);
        }

        public void BuyFromShop(int shopIndex, int buildIndex, Player player, 
            out CardCommandQueue queue, out Deck saveDeck)
        {
            if (player.Gold < Game.PetCost)
            {
                queue = null;
                saveDeck = null;
                throw new Exception("Not enough gold to buy pet");
            }
            else
            {
                queue = new CardCommandQueue();
                saveDeck = new Deck(player, Game.BuildDeckSlots);

                var shopCard = player.ShopDeck[shopIndex]; 
                var buildCard = player.BuildDeck[buildIndex];

                if (buildCard == null)
                {
                    var card = new Card(player.BuildDeck, shopCard);
                    card.Buy(buildIndex);
                    player.ShopDeck.Remove(shopIndex);

                    player.BuildDeck.CloneTo(saveDeck);
                    card.Bought(queue);
                }
                else
                {
                    if (shopCard.Ability.GetType() == buildCard.Ability.GetType())
                    {
                        int oldLevel = buildCard.Level; 
                        buildCard.GainXP(shopCard);
                        player.BuildDeck.CloneTo(saveDeck);
                        buildCard.GainedXP(queue, oldLevel);
                    }
                    else
                        throw new Exception("Shop card cannot be bought or leveled up.");
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

        public List<CardCommandQueue> CreateFightResult()
        {
            // disable events
            BeginUpdate();
            _fighting = true;
            var fightResult = new List<CardCommandQueue>();
            var queue = new CardCommandQueue();
            _player1.BattleStarted1(queue);
            _player2.BattleStarted1(queue);
            if (queue.Count > 0)
                fightResult.AddRange(queue.CreateExecuteResult(this));
            queue = new CardCommandQueue();
            _player1.BattleStarted2(queue);
            _player2.BattleStarted2(queue);
            if (queue.Count > 0)
                fightResult.AddRange(queue.CreateExecuteResult(this));
            while (!IsFightOver())
            {
                queue = new CardCommandQueue();

                var card = _player1.BattleDeck.GetLastCard();
                var opponentCard = _player2.BattleDeck.GetLastCard();

                card.Attacking(queue);
                opponentCard.Attacking(queue);
                if (queue.Count > 0)
                {
                    fightResult.AddRange(queue.CreateExecuteResult(this));
                    queue = new CardCommandQueue();
                }

                queue.Add(new AttackCardCommand(card, opponentCard).Execute());

                fightResult.AddRange(queue.CreateExecuteResult(this));
            }
            _fighting = false;
            EndUpdate();
            return fightResult;
        }
    }
}
