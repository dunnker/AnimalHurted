using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimalHurtedLib
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

        protected void PerformTigerAbility(Card card, int index, Action<int> action)
        {
            action(card.Level);
            if (index > 0 && card.Deck.Player.Game.Fighting)
            {
                var priorCard = card.Deck[index - 1];
                if (priorCard != null && priorCard.Ability is TigerAbility)
                    action(priorCard.Level);
            }
        }

        public static bool CanMakeRoomAt(CardCommandQueue queue, Deck deck, int atIndex, out int summonIndex)
        {
            summonIndex = -1;
            // First look towards the end of the deck for an empty space
            for (int i = atIndex; i < deck.Size; i++)
                if (deck[i] == null)
                {
                    summonIndex = i;
                    return true;
                }
            // Empty space not found so move cards back if possible
            for (int i = atIndex - 1; i >= 0; i--)
            {
                if (deck[i] == null)
                {
                    var indexes = new List<(int from, int to)>();
                    for (int j = i; j <= atIndex - 1; j++)
                        indexes.Add((j + 1, j));
                        
                    // Perform the deck move operations and store in the queue so when the final result is read
                    // the same move operations can be performed -- just before the summon command is executed.
                    // queue._parentQueue typically contains one or more faint commands (because a card is only summoned
                    // from Ability.Fainted, or Ability.FriendFainted methods)
                    // When the final results are executed, ability methods are not executed, and that means that the
                    // faint commands in queue._parentQueue are simply executed and their associated cards are fainted
                    // So we don't need to worry about the commands in queue._parentQueue being associated with the wrong index
                    // values
                    queue.Add(new MoveCardsCommand(deck, indexes).Execute());

                    summonIndex = atIndex;
                    return true;
                }
            }
            return false;
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

        public virtual void FriendFaints(CardCommandQueue queue, Card card, int index)
        {

        }

        public virtual void AteFood(CardCommandQueue queue, Card card)
        {

        }

        // specifically for Cat, card is the one eating (which could be the cat itself if it's eating)
        public virtual void Eating(Card card, Card eatingCard, ref int hitPoints, ref int attackPoints)
        {

        }

        public virtual void FriendAteFood(CardCommandQueue queue, Card card, Card friendCard)
        {
            
        }

        public virtual void Fainted(CardCommandQueue queue, Card card, int index, bool attacking,
            Card opponentCard = null)
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

        public virtual void Hurted(CardCommandQueue queue, Card card)
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
