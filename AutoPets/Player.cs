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
        Deck _battleDeck;
        int _updateCount;

        public Game Game { get { return _game; } }

        public Deck BuildDeck { get { return _buildDeck; } }

        public Deck BattleDeck { get { return _battleDeck; } }

        public Deck ShopDeck { get { return _shopDeck; } }

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

        public void RoundOver(bool won, bool lost, int round)
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
        }
    }
}
