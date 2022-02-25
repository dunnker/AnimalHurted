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

        public virtual string GetAbilityMessage(Card card)
        {
            return string.Empty;
        }

        // "Start of turn"
        public virtual void RoundStarted(Card card)
        {

        }

        // "End of turn"
        public virtual void RoundEnded(CardCommandQueue queue, Card card)
        {

        }

        public virtual void BattleStarted(CardCommandQueue queue, Card card)
        {

        }

        public virtual void FriendSummoned(CardCommandQueue queue, Card card, Card summonedCard)
        {

        }

        public virtual void FriendAheadAttacks(CardCommandQueue queue, Card card)
        {

        }

        public virtual void FriendAheadFaints(CardCommandQueue queue, Card card, int faintedIndex)
        {

        }

        public virtual void AteFood(Card card)
        {

        }

        public virtual void FriendAteFood(Card card, Card friendCard)
        {
            
        }

        public virtual void Fainted(CardCommandQueue queue, Card card, int index)
        {

        }

        public virtual void BeforeAttack(CardCommandQueue queue, Card card)
        {

        }

        public virtual void Hurt(CardCommandQueue queue, Card card)
        {

        }

        public virtual void Bought(Card card)
        {

        }

        public virtual void FriendBought(Card card, Card friendCard)
        {

        }

        public virtual void Sold(Card card, int index)
        {

        }

        public virtual void FriendSold(Card card, Card soldCard)
        {

        }

        public virtual void LeveledUp(Card card)
        {

        }
    }
}
