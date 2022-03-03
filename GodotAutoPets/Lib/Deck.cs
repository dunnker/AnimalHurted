using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

namespace AutoPets
{
    public class Deck : IEnumerable<Card>
    {
        readonly Player _player;

        Card[] _cards;

        public Card this[int index] { get { return _cards[index]; } }

        public Player Player { get { return _player; } }

        public int Size { get { return _cards.Length; } }

        public Deck(Player player, int size)
        {
            _cards = new Card[size];
            _player = player;
        }

        public void SaveToStream(StreamWriter writer)
        {
            // version number of this stream; used to support backward compatibility
            // if stream format changes later
            writer.WriteLine(1);
            writer.WriteLine(_cards.Length);
            foreach (var c in _cards)
            {
                writer.WriteLine(c != null);
                if (c != null)
                    c.SaveToStream(writer);
            }
        }

        public void LoadFromStream(StreamReader reader)
        {
            int version = Int32.Parse(reader.ReadLine());
            if (version != 1)
                throw new Exception("Invalid stream version");
            int count = Int32.Parse(reader.ReadLine());
            _cards = new Card[count];
            for (int i = 0; i < _cards.Length; i++)
            {
                var assigned = Boolean.Parse(reader.ReadLine());
                if (assigned)
                {
                    Card card = new Card(this);
                    card.LoadFromStream(reader);
                    SetCard(card, i);
                }
            }
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
            return _cards.LastOrDefault(c => c != null);
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

        public Card GetRandomCard(HashSet<int> excludingIndexes = null)
        {
            // Make sure there is at least one card that matches the criteria of what
            // we're looking for in order to avoid infinite loop below.
            // We're excluding cards that are TotalHitPoints <= 0 because we should not be Buffing or Hurting
            // cards that are about to faint
            int count = _cards.Count(c => c != null && c.TotalHitPoints > 0 && 
                (excludingIndexes == null || !excludingIndexes.Contains(c.Index)));
            if (count > 0)
            {
                do
                {
                    int i = _player.Game.Random.Next(0, _cards.Length);
                    var c = _cards[i];
                    if (c != null && c.TotalHitPoints > 0 && 
                        (excludingIndexes == null || !excludingIndexes.Contains(c.Index)))
                        return _cards[i];
                } while (true);
            }
            else
                return null;
        }

        public void CloneTo(Deck deck)
        {
            if (deck._cards.Length != _cards.Length)
                throw new Exception("Invalid deck size.");
            Array.Clear(deck._cards, 0, deck._cards.Length);
            foreach (var card in _cards)
            {
                if (card != null)
                    deck.SetCard(new Card(deck, card), card.Index);
            }
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