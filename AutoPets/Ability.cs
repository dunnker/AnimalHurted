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

        public static bool SummonExistsAt(CardCommandQueue queue, int index)
        {
            return queue.Any((c) => {
                if (c is SummonCardCommand)
                    return (c as SummonCardCommand).AtIndex == index;
                else
                    return false;
            });
        }      

        public static int GetSummonIndex(CardCommandQueue queue, Deck deck, int index)
        {
            // First look towards the end of the deck. Look for a empty space, but also
            // no other prior summon command slated to happen at that space.
            for (int i = index; i < deck.Size; i++)
                if (deck[i] == null && !SummonExistsAt(queue, i))
                {
                    return i;
                }
            if (index > 0 && deck[index - 1] == null && !SummonExistsAt(queue, index - 1))
                return index - 1;
            /*
            else MoveCards?
                if we see there are prior gaps. and no existing SummonCommands exist there
                then it seems like we could move pets to make room.
                
                one problem with moving cards is that we might be coming from inside CreateFightResult
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
