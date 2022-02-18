using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AutoPets
{
    public delegate void AbilityEventHandler(object sender, Ability ability, Card card, int index, string message);
    public delegate void CardHurtEventHandler(object sender, Card card, Card sourceCard);

    public class Game
    {
        readonly Random _random;
        readonly Player _player1;
        readonly Player _player2;
        int _round;
        int _shopSlots;
        List<Ability> _tierAbilities;
        bool _fighting;

        public const int PetCost = 3;
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

        public event AbilityEventHandler AbilityEvent;
        public event EventHandler FightEvent;
        public event CardHurtEventHandler CardHurtEvent;

        public void OnAbilityEvent(Ability ability, Card card, int index, string message)
        {
            AbilityEvent?.Invoke(this, ability, card, index, message);
        }

        public void OnFightEvent()
        {
            FightEvent?.Invoke(this, EventArgs.Empty);
        }

        public void OnCardHurtEvent(Card card, Card sourceCard)
        {
            CardHurtEvent?.Invoke(this, card, sourceCard);
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
                    break;
                case int i when i >= 3 && i <= 4:
                    _tierAbilities.AddRange(AbilityList.Instance.TierTwoAbilities);
                    break;
                case int i when i >= 5 && i <= 6:
                    _shopSlots = 4;
                    _tierAbilities.AddRange(AbilityList.Instance.TierThreeAbilities);
                    break;
                case int i when i >= 7 && i <= 8:
                    _tierAbilities.AddRange(AbilityList.Instance.TierFourAbilities);
                    break;
                case int i when i >= 9 && i <= 10:
                    _shopSlots = 5;
                    _tierAbilities.AddRange(AbilityList.Instance.TierFiveAbilities);
                    break;
                case int i when i >= 11:
                    _tierAbilities.AddRange(AbilityList.Instance.TierSixAbilities);
                    break;
                default:
                    break;
            }
        }

        public void NewRound()
        {
            _round++;
            CheckNewTier();
            _player1.Gold = GoldPerTurn + 1;
            _player2.Gold = GoldPerTurn + 1;
            Roll(_player1);
            Roll(_player2);
        }

        public void Roll(Player player)
        {
            if (player.Gold < RollCost)
                throw new Exception("Not enough gold for Roll.");
            player.Gold -= RollCost;
            player.ShopDeck.Clear();
            for (int i = 0; i < _shopSlots; i++)
            {
                int rand = Random.Next(_tierAbilities.Count);
                player.ShopDeck.SetCard(new Card(player.ShopDeck, _tierAbilities[rand]), i);
            }
        }

        public void BuyFromShop(int shopIndex, int buildIndex, Player player)
        {
            if (player.Gold < Game.PetCost)
                throw new Exception("Not enough gold to buy pet");
            else
            {
                Debug.Assert(player.BuildDeck[buildIndex] == null); //TODO
                var card = new Card(player.BuildDeck, player.ShopDeck[shopIndex]);
                card.Buy(buildIndex);
                player.ShopDeck.Remove(shopIndex);
                player.Gold -= Game.PetCost;
            }
        }

        public void NewBattle()
        {
            _player1.NewBattle();
            _player2.NewBattle();
        }

        public bool IsFightOver()
        {
            // fight is over if either player has run out of cards, and there are no cards left to sweep to invoke Faint()
            return (_player1.BattleDeck.GetCardCount() == 0 || _player2.BattleDeck.GetCardCount() == 0) && 
                !_player1.BattleDeck.Any((c) => c.HitPoints <= 0) && !_player2.BattleDeck.Any((c) => c.HitPoints <= 0);
        }

        public void FightCanceled()
        {
            _fighting = false;
            _player1.BattleCanceled();
            _player2.BattleCanceled();
        }

        public void FightOver()
        {
            _fighting = false;
            _player1.RoundOver(_player1.BattleDeck.GetCardCount() > 0, _player1.BattleDeck.GetCardCount() == 0 && _player2.BattleDeck.GetCardCount() > 0, _round);
            _player2.RoundOver(_player2.BattleDeck.GetCardCount() > 0, _player2.BattleDeck.GetCardCount() == 0 && _player1.BattleDeck.GetCardCount() > 0, _round);
            NewRound();
        }

        public void FightOne()
        {
            foreach (var card in _player1.BattleDeck)
                card.ResetState();
            foreach (var card in _player2.BattleDeck)
                card.ResetState();
            if (!_fighting)
            {
                _fighting = true;
                foreach (var card in _player1.BattleDeck)
                    card.BattleStarted();
                foreach (var card in _player2.BattleDeck)
                    card.BattleStarted();
                foreach (var card in _player1.BattleDeck)
                    if (card.HitPoints <= 0)
                        card.Faint();
                foreach (var card in _player2.BattleDeck)
                    if (card.HitPoints <= 0)
                        card.Faint();
            }
            if (_player1.BattleDeck.GetCardCount() > 0 && _player2.BattleDeck.GetCardCount() > 0)
                _player1.BattleDeck.GetLastCard().FightOne(_player2.BattleDeck.GetLastCard());
        }
    }
}
