using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPets
{
    public class Ability
    {
        public int DefaultHP { get; set; }

        public int DefaultAttack { get; set; }

        public override string ToString()
        {
            return GetType().Name.Replace("Ability", string.Empty);
        }

        public Ability()
        {
        }

        virtual public void BattleStarted(Card card)
        {
            // unlike other ability methods, this gets called for each card in the deck

            //OnDeckEvent(...);
        }

        virtual public void FriendSummoned(Card card, Card summonedCard)
        {

        }

        virtual public void Summoned(Card card)
        {
            card.Deck.OnDeckEvent(card, string.Format(string.Format("{0}. {1} summoned", card.Index + 1, ToString())));
        }

        virtual public void Fainted(Card card, int index)
        {
            card.Deck.OnDeckEvent(card, string.Format(string.Format("{0}. {1} fainted", index + 1, ToString())));
        }

        virtual public void BeforeAttack(Card card)
        {

        }

        virtual public void Attacked(Card card)
        {
            card.Deck.OnDeckEvent(card, string.Format(string.Format("{0}. {1} attacked", card.Index + 1, ToString())));
        }

        virtual public void Hurt(Card card)
        {
            card.Deck.OnDeckEvent(card, string.Format(string.Format("{0}. {1} hurt", card.Index + 1, ToString())));
        }

        virtual public void Buffed(Card card, int sourceIndex, int hitPoints, int attackPoints)
        {
            card.Deck.OnDeckEvent(card, string.Format(string.Format("{0}. {1} buffed +{2} +{3} from {4}.", card.Index + 1, ToString(), attackPoints, hitPoints,
                sourceIndex + 1)));
        }

        virtual public void Bought(Card card)
        {
            card.Deck.OnDeckEvent(card, string.Format(string.Format("{0}. {1} bought", card.Index + 1, ToString())));
        }

        virtual public void Sold(Card card, int index)
        {
            card.Deck.OnDeckEvent(card, string.Format(string.Format("{0}. {1} sold. +{2} gold", index + 1, ToString(), card.Level)));
        }

        virtual public void LeveledUp(Card card)
        {
            card.Deck.OnDeckEvent(card, string.Format(string.Format("{0}. {1} leveled up", card.Index + 1, ToString())));
        }
    }
}
