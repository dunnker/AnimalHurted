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

        public static int GetSummonIndex(CardCommandQueue queue, Deck deck, int index)
        {
            // First look towards the end of the deck. Look for a empty space
            for (int i = index; i < deck.Size; i++)
                if (deck[i] == null)
                {
                    return i;
                }
            //TODO should be a loop to find prior empty slot
            if (index > 0 && deck[index - 1] == null)
                return index - 1;
            /*
            else MoveCards?
                if we see there are prior gaps. and no existing SummonCommands exist there
                then it seems like we could move pets to make room.
                
                one problem with moving cards is that we might be coming from inside CardCommandQueue.CreateExecuteResult
                and it's in the process of iterating through a "parent" queue to this current one.
                the commands in the parent queue will have their index's get out of sync if we move
                cards here
            */
            else
                return -1;
        }

        public virtual string GetAbilityMessage(Card card)
        {
            return string.Empty;
        }

        // "Start of turn"
        public virtual void RoundStarted(Card card)
        {

        }

        // also a "Start of turn" ability specifically for Parrot
        public virtual void NewBattleDeck(Card card)
        {

        }

        // "End of turn"
        public virtual void RoundEnded(CardCommandQueue queue, Card card)
        {

        }

        // BattleStarted has two phases; phase 1 is for Whale, to faint the card ahead
        public virtual void BattleStarted1(CardCommandQueue queue, Card card)
        {

        }

        public virtual void BattleStarted2(CardCommandQueue queue, Card card)
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

        public virtual void FriendFaints(CardCommandQueue queue, Card card)
        {

        }

        public virtual void AteFood(CardCommandQueue queue, Card card)
        {

        }

        public virtual void FriendAteFood(CardCommandQueue queue, Card card, Card friendCard)
        {
            
        }

        public virtual void Fainted(CardCommandQueue queue, Card card, int index)
        {

        }

        public virtual void BeforeAttack(CardCommandQueue queue, Card card)
        {

        }

        public virtual void Attacked(CardCommandQueue queue, Card card, int damage, Card opponentCard = null)
        {

        }

        public virtual void Knockout(CardCommandQueue queue, Card card)
        {

        }

        public virtual void Hurt(CardCommandQueue queue, Card card)
        {

        }

        public virtual void Bought(CardCommandQueue queue, Card card)
        {

        }

        public virtual void FriendBought(CardCommandQueue queue, Card card, Card friendCard)
        {

        }

        public virtual void Sold(CardCommandQueue queue, Card card, int index)
        {

        }

        public virtual void FriendSold(CardCommandQueue queue, Card card, Card soldCard)
        {

        }

        public virtual void LeveledUp(CardCommandQueue queue, Card card)
        {

        }
    }
}
