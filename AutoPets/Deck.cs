using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;

namespace AutoPets
{
    public delegate void DeckEventHandler(object sender, Card card, string message);

    public class Deck : IEnumerable<Card>
    {
        readonly Player _player;

        readonly Card[] _cards;

        public Card this[int index] { get { return _cards[index]; } }

        public Player Player { get { return _player; } }

        public int Size { get { return _cards.Length; } }

        public event DeckEventHandler DeckEvent;

        public void OnDeckEvent(Card card, string message)
        {
            DeckEvent?.Invoke(this, card, message);
        }

        public Deck(Player player, int size)
        {
            _cards = new Card[size];
            _player = player;
        }

        public void SetCard(Card card, int index)
        {
            Debug.Assert(index >= 0 && index < _cards.Length);
            Debug.Assert(_cards[index] == null);
            _cards[index] = card;
            card.SetIndex(index);
        }

        public void MoveCard(Card card, int toIndex)
        {
            Remove(card.Index);
            SetCard(card, toIndex);
        }

        public void Remove(int index)
        {
            Debug.Assert(index >= 0 && index < _cards.Length);
            Debug.Assert(_cards[index] != null);
            _cards[index].SetIndex(-1);
            _cards[index] = null;
        }

        public void Clear()
        {
            foreach (var card in _cards)
                if (card != null)
                    Remove(card.Index);
        }

        public int GetCardCount()
        {
            return _cards.Count(c => c != null);
        }

        public Card GetLastCard()
        {
            return _cards.Last(c => c != null);
        }

        public Card GetPriorCard(int cardIndex)
        {
            Debug.Assert(cardIndex < _cards.Length);
            for (int i = cardIndex - 1; i >= 0; i--)
                if (_cards[i] != null)
                    return _cards[i];
            return null;
        }

        public Card GetNextCard(int cardIndex)
        {
            Debug.Assert(cardIndex >= 0);
            for (int i = cardIndex + 1; i < _cards.Length; i++)
                if (_cards[i] != null)
                    return _cards[i];
            return null;
        }

        public bool MakeRoomAt(int atIndex)
        {
            Debug.Assert(atIndex >= 0 && atIndex < _cards.Length);
            bool moved = false;
            for (int i = atIndex; i < _cards.Length; i++)
            {
                // find the first empty slot past atIndex
                if (_cards[i] == null)
                {
                    // work backwards to move cards to empty slots
                    for (int j = i; j >= atIndex + 1; j--)
                    {
                        MoveCard(_cards[j - 1], j);
                        moved = true;
                    }
                    break;
                }
            }
            if (!moved)
                for (int i = atIndex; i >= 0; i--)
                {
                    if (_cards[i] == null)
                    {
                        for (int j = i; j <= atIndex - 1; j++)
                        {
                            MoveCard(_cards[j + 1], j);
                            moved = true;
                        }
                        break;
                    }
                }
            return moved;
        }

        public Card GetRandomCard(int excludingIndex = -1)
        {
            int count = GetCardCount();
            // if there are no cards
            if (count == 0 ||
                // ...or we are excluding a position and there is no card prior or after this position
                (excludingIndex >= 0 && GetNextCard(excludingIndex) == null && GetPriorCard(excludingIndex) == null))
                return null;
            else
            {
                do
                {
                    int i = _player.Game.Random.Next(0, _cards.Length);
                    if ((excludingIndex == -1 || i != excludingIndex) && _cards[i] != null)
                        return _cards[i];
                } while (true);
            }
        }

        public static Deck Clone(Deck deck)
        {
            var result = new Deck(deck._player, deck.Size);
            foreach (var card in deck._cards)
            {
                if (card != null)
                    result.SetCard(new Card(result, card), card.Index);
            }
            return result;
        }

        public IEnumerator<Card> GetEnumerator()
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                if (_cards[i] != null)
                    yield return _cards[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}