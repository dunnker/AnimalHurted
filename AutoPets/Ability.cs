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

        virtual public void BattleStarted(CardCommandQueue queue, Card card)
        {

        }

        virtual public void FriendSummoned(CardCommandQueue queue, Card card, Card summonedCard)
        {

        }

        virtual public void Fainted(CardCommandQueue queue, Card card, int index)
        {

        }

        virtual public void BeforeAttack(CardCommandQueue queue, Card card)
        {

        }

        virtual public void Hurt(CardCommandQueue queue, Card card)
        {

        }

        virtual public void Bought(Card card)
        {

        }

        virtual public void Sold(Card card, int index)
        {

        }

        virtual public void LeveledUp(Card card)
        {

        }
    }
}
