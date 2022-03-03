using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AutoPets
{
    public class NoAbility : Ability
    {
        public NoAbility() : base()
        {

        }
    }

    public class AntAbility : Ability
    {
        public AntAbility() : base()
        {
            DefaultHP = 1;
            DefaultAttack = 2;
        }

        public override string GetAbilityMessage(Card card)
        {
            return string.Format("Faint => Give a random friend +{0} attack and +{1} health.", 2 * card.Level, card.Level);
        }

        public override void Fainted(CardCommandQueue queue, Card card, int index)
        {
            base.Fainted(queue, card, index);
            var buffCard = card.Deck.GetRandomCard(new HashSet<int>() { index });
            if (buffCard != null)
                queue.Add(new BuffCardCommand(buffCard, index, card.Level, 2 * card.Level).Execute());
        }
    }

    public class CricketAbility : Ability
    {
        public CricketAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 1;
        }

        public override string GetAbilityMessage(Card card)
        {
            return string.Format("Faint => Summon a {0}/{1} cricket.", card.Level, card.Level);
        }

        public override void Fainted(CardCommandQueue queue, Card card, int index)
        {
            base.Fainted(queue, card, index);
            // cricket is no longer in the deck
            Debug.Assert(card.Index == -1);
            // ...so we have the empty slot to place the zombie cricket
            queue.Add(new SummonCardCommand(card, card.Deck, index, AbilityList.Instance.ZombieCricketAbility.GetType(), card.Level, card.Level).Execute());
        }
    }

    public class ZombieCricketAbility : NoAbility
    {
    }

    public class OtterAbility : Ability
    {
        public OtterAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 1;
        }

        public override string GetAbilityMessage(Card card)
        {
            return string.Format("Buy => Give a random friend +{0} attack and +{1} health.", card.Level, card.Level);
        }

        public override void Bought(CardCommandQueue queue, Card card)
        {
            base.Bought(queue, card);
            // find and buff a random card that is not the otter
            var buffCard = card.Deck.GetRandomCard(new HashSet<int>() { card.Index });
            if (buffCard != null)
                queue.Add(new BuffCardCommand(buffCard, card.Index, card.Level, card.Level).Execute());
        }
    }

    public class BeaverAbility : Ability
    {
        public BeaverAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 2;
        }

        public override string GetAbilityMessage(Card card)
        {
            return string.Format("Sell => Give two random friends +{0} health.", card.Level);
        }

        public override void Sold(CardCommandQueue queue, Card card, int index)
        {
            base.Sold(queue, card, index);
            var buffCard1 = card.Deck.GetRandomCard(new HashSet<int>() { index });
            if (buffCard1 != null)
            {
                queue.Add(new BuffCardCommand(buffCard1, index, card.Level, 0).Execute());
                // second card can't be the first card we found
                var buffCard2 = card.Deck.GetRandomCard(new HashSet<int>() { index, buffCard1.Index });
                if (buffCard2 != null)
                    queue.Add(new BuffCardCommand(buffCard2, index, card.Level, 0).Execute());
            }
        }
    }

    public class MosquitoAbility : Ability
    {
        public MosquitoAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 2;
        }

        public override string GetAbilityMessage(Card card)
        {
            return string.Format("Start of battle => Deal 1 damage to {0} random enemies.", card.Level);
        }

        public override void BattleStarted2(CardCommandQueue queue, Card card)
        {
            base.BattleStarted2(queue, card);
            var opponent = card.Deck.Player.GetOpponentPlayer();
            var excludingIndexes = new HashSet<int>();
            for (int i = 1; i <= card.Level; i++)
            {
                var randomCard = opponent.BattleDeck.GetRandomCard(excludingIndexes);
                if (randomCard != null)
                {
                    queue.Add(new HurtCardCommand(randomCard, 1, card.Deck, card.Index).Execute());
                    // ensures we won't pick the same pet more than once when getting the
                    // next random card
                    excludingIndexes.Add(randomCard.Index);
                }
            }
        }
    }

    public class PigAbility : Ability
    {
        public PigAbility() : base()
        {
            DefaultHP = 1;
            DefaultAttack = 3;
        }

        public override string GetAbilityMessage(Card card)
        {
            return string.Format("Sell => Gain an extra {0} gold.", card.Level);
        }

        public override void Sold(CardCommandQueue queue, Card card, int index)
        {
            base.Sold(queue, card, index);
            card.Deck.Player.Gold += card.Level;
        }
    }

    public class DuckAbility : Ability
    {
        public DuckAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 1;
        }

        public override string GetAbilityMessage(Card card)
        {
            return string.Format("Sell => Give shop pets {0} health.", card.Level);
        }

        public override void Sold(CardCommandQueue queue, Card card, int index)
        {
            base.Sold(queue, card, index);
            foreach (var shopCard in card.Deck.Player.ShopDeck)
                shopCard.Buff(-1, card.Level, 0);
        }
    }

    public class FishAbility : Ability
    {
        public FishAbility() : base()
        {
            DefaultHP = 3;
            DefaultAttack = 2;
        }

        public override string GetAbilityMessage(Card card)
        {
            return card.Level < 3 ? $"Level-up => Give all friends +{card.Level} health and +{card.Level} attack." : string.Empty;
        }

        public override void LeveledUp(CardCommandQueue queue, Card card)
        {
            base.LeveledUp(queue, card);
            foreach (var friendCard in card.Deck)
            {
                if (friendCard != card)
                    queue.Add(new BuffCardCommand(friendCard, card.Index, card.Level - 1, card.Level - 1).Execute());
            }
        }
    }

    public class HorseAbility : Ability
    {
        public HorseAbility() : base()
        {
            DefaultHP = 1;
            DefaultAttack = 2;
        }

        public override string GetAbilityMessage(Card card)
        {
            return string.Format("Friend summoned => Give it +{0} attack.", card.Level);
        }

        public override void FriendSummoned(CardCommandQueue queue, Card card, Card summonedCard)
        {
            base.FriendSummoned(queue, card, summonedCard);
            //TODO: SAP is worded "Give it +? attack until end of battle." and it underscores the attack points during build and battle
            // after battle, if the friend was summoned during build, the friend's attack points will revert for next build
            queue.Add(new BuffCardCommand(summonedCard, card.Index, 0, card.Level).Execute());
        }
    }

    // Tier 2
    public class CrabAbility : Ability
    {
        public CrabAbility() : base()
        {
            DefaultHP = 3;
            DefaultAttack = 3;
        }

        public override string GetAbilityMessage(Card card)
        {
            return "Buy => Copy health from the most healthy friend.";
        }

        public override void Bought(CardCommandQueue queue, Card card)
        {
            base.Bought(queue, card);
            var maxCard = card.Deck.OrderByDescending(c => c.TotalHitPoints).First();
            // if maxCard was buffed by a cupcake, then we're taking on those hit points
            // as well. not sure what SAP does in this case
            card.HitPoints = maxCard.TotalHitPoints;
        }
    }

    public class DodoAbility : Ability
    {
        public DodoAbility() : base()
        {
            DefaultHP = 3;
            DefaultAttack = 2;
        }

        void GetAttackPercent(Card card, out int attackPoints, out int attackPercent)
        {
            switch (card.Level)
            {
                // buff 50% of dodo's attack
                case 1:
                    // doing integer division, so adding +1 to card.AttackPoints to round up
                    attackPoints = (card.TotalAttackPoints + 1) / 2;
                    attackPercent = 50;
                    break;
                // buff 100% of dodo's attack
                case 2:
                    attackPoints = card.TotalAttackPoints;
                    attackPercent = 100;
                    break;
                // buff 150% of dodo's attack
                case 3:
                    attackPoints = card.TotalAttackPoints + ((card.TotalAttackPoints + 1) / 2);
                    attackPercent = 150;
                    break;
                default:
                    attackPercent = 0;
                    attackPoints = 0;
                    Debug.Assert(false);
                    break;
            }
        }

        public override string GetAbilityMessage(Card card)
        {
            GetAttackPercent(card, out int attackPoints, out int attackPercent);
            return string.Format("Start of battle => Give {0}% of Dodo's attack to friend ahead.", attackPercent);
        }

        public override void BattleStarted2(CardCommandQueue queue, Card card)
        {
            base.BattleStarted2(queue, card);
            // Note that this card might have been attacked by a mosquito and about to be fainted (see Game.FightOne, after calls to BattleStarted, there is a sweep to faint cards)
            // Same is true with the card ahead. It may have taken damage and about to be fainted
            // if a Dodo's ability were to buff the hitpoints of the card ahead, then that could bring the hitpoints of that card from negative to positive again --
            // e.g. bringing that card back to life again
            Card nextCard = null;
            Debug.Assert(card.Index != -1);
            if (card.Index < card.Deck.Size - 1)
                nextCard = card.Deck[card.Index + 1];
            if (nextCard != null)
            {
                GetAttackPercent(card, out int attackPoints, out int attackPercent);
                queue.Add(new BuffCardCommand(nextCard, card.Index, 0, attackPoints).Execute());
            }
        }
    }

    public class ElephantAbility : Ability
    {
        public ElephantAbility() : base()
        {
            DefaultHP = 5;
            DefaultAttack = 3;
        }

        public override string GetAbilityMessage(Card card)
        {
            return string.Format("Before attack => Deal {0} damage to friend behind.", card.Level);
        }

        public override void BeforeAttack(CardCommandQueue queue, Card card)
        {
            base.BeforeAttack(queue, card);
            Debug.Assert(card.Index != -1);
            Card priorCard = null;
            if (card.Index > 0)
                priorCard = card.Deck[card.Index - 1];
            if (priorCard != null)
                queue.Add(new HurtCardCommand(priorCard, card.Level, card.Deck, card.Index).Execute());
        }
    }

    public class FlamingoAbility : Ability
    {
        public FlamingoAbility() : base()
        {
            DefaultHP = 1;
            DefaultAttack = 3;
        }

        public override string GetAbilityMessage(Card card)
        {
            return string.Format("Faint => Give the two friends behind +{0} attack and +{1} health.", card.Level, card.Level);
        }

        public override void Fainted(CardCommandQueue queue, Card card, int index)
        {
            base.Fainted(queue, card, index);
            if (index > 0)
            {
                var priorCard = card.Deck[index - 1];
                if (priorCard != null)
                    queue.Add(new BuffCardCommand(priorCard, index, card.Level, card.Level).Execute());
            }
            if (index > 1)
            {
                var priorCard = card.Deck[index - 2];
                if (priorCard != null)
                    queue.Add(new BuffCardCommand(priorCard, index, card.Level, card.Level).Execute());
            }
        }
    }

    public class HedgehogAbility : Ability
    {
        public HedgehogAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 3;
        }

        public override string GetAbilityMessage(Card card)
        {
            return string.Format("Faint => Deal {0} damage to all.", 2 * card.Level);
        }

        public override void Fainted(CardCommandQueue queue, Card card, int index)
        {
            base.Fainted(queue, card, index);
            var opponent = card.Deck.Player.GetOpponentPlayer();
            foreach (var c in card.Deck)
				// checking TotalHitPoints > 0; see comments in HurtCommand
                if (c != card && c.TotalHitPoints > 0)
                    queue.Add(new HurtCardCommand(c, 2 * card.Level, card.Deck, index).Execute());
            if (card.Deck.Player.Game.Fighting)
                foreach (var c in opponent.BattleDeck)
					// checking TotalHitPoints > 0; see comments in HurtCommand
                    if (c.TotalHitPoints > 0)
                        queue.Add(new HurtCardCommand(c, 2 * card.Level, card.Deck, index).Execute());
        }
    }

    public class PeacockAbility : Ability
    {
        public PeacockAbility() : base()
        {
            DefaultHP = 5;
            DefaultAttack = 1;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"Hurt => Gain 50% of attack points, {card.Level} time(s).";
        }

        public override void Hurt(CardCommandQueue queue, Card card)
        {
            base.Hurt(queue, card);

            // not necessary to check card.TotalHitpoints > 0; see comments in HurtCommand

            // if not about to faint, then buff itself
            //if (card.TotalHitPoints > 0)

            int attackPoints = (int)Math.Round(((double)card.TotalAttackPoints / 2) * card.Level, 
                // if 0.5 then round up
                MidpointRounding.AwayFromZero);
            queue.Add(new BuffCardCommand(card, card.Index, 0, attackPoints).Execute());
        }
    }

    public class DirtyRatAbility : NoAbility
    {
        public DirtyRatAbility() : base()
        {
            DefaultHP = 1;
            DefaultAttack = 1;
        }
    }

    public class RatAbility : Ability
    {
        public RatAbility() : base()
        {
            DefaultHP = 5;
            DefaultAttack = 4;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"Faint => Summon {card.Level} dirty rat(s) for the opponent.";
        }

        public override void Fainted(CardCommandQueue queue, Card card, int index)
        {
            base.Fainted(queue, card, index);
            if (card.Deck.Player.Game.Fighting)
            {
                for (int i = 1; i <= card.Level; i++)
                {
                    var opponent = card.Deck.Player.GetOpponentPlayer();
                    if (opponent.BattleDeck[opponent.BattleDeck.Size - i] == null)
                    {
                        int summonIndex = Ability.GetSummonIndex(queue, opponent.BattleDeck, 
                            opponent.BattleDeck.Size - i);
                        if (summonIndex != -1)
                            queue.Add(new SummonCardCommand(card, opponent.BattleDeck, summonIndex, 
                                AbilityList.Instance.DirtyRatAbility.GetType(), 1, 1).Execute());
                    }
                }
            }
        }
    }

    public class ZombieBeeAbility : NoAbility
    {
        public ZombieBeeAbility()
        {
            DefaultAttack = 1;
            DefaultHP = 1;
        }
    }

    public class ShrimpAbility : Ability
    {
        public ShrimpAbility() : base()
        {
            DefaultHP = 3;
            DefaultAttack = 2;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"Friend sold => Give a random friend +{card.Level} health.";
        }

        public override void FriendSold(CardCommandQueue queue, Card card, Card soldCard)
        {
            base.FriendSold(queue, card, soldCard);
            var buffCard = card.Deck.GetRandomCard(new HashSet<int> { card.Index });
            if (buffCard != null)
                queue.Add(new BuffCardCommand(buffCard, card.Index, card.Level, 0).Execute());
        }
    }   

    public class SpiderAbility : Ability
    {
        public SpiderAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 2;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"Fainted => Summon a level {card.Level} tier 3 pet as 2/2.";
        }

        public override void Fainted(CardCommandQueue queue, Card card, int index)
        {
            base.Fainted(queue, card, index);
            int randIndex = card.Deck.Player.Game.Random.Next(0, AbilityList.Instance.TierThreeAbilities.Count);
            var ability = AbilityList.Instance.TierThreeAbilities[randIndex];
            // spider has fainted so we have the empty slot for the new card
            // otherwise would need to use Ability.GetSummonIndex()
            queue.Add(new SummonCardCommand(card, card.Deck, index, ability.GetType(), 2, 2, card.Level).Execute());
        }
    }

    public class SwanAbility : Ability
    {
        public SwanAbility() : base()
        {
            DefaultHP = 3;
            DefaultAttack = 3;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"Start of turn => Gain {card.Level} gold.";
        }

        public override void RoundStarted(Card card)
        {
            base.RoundStarted(card);
            card.Deck.Player.Gold += card.Level;
        }
    }

    // Tier 3
    public class BadgerAbility : Ability
    {
        public BadgerAbility() : base()
        {
            DefaultHP = 4;
            DefaultAttack = 5;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"Fainted => Deal damage - {card.Level} times attack - to adjacent pets.";
        }

        public override void Fainted(CardCommandQueue queue, Card card, int index)
        {
            base.Fainted(queue, card, index);
        	// checking TotalHitPoints > 0; see comments in HurtCommand
            // find card prior to the badger that is not fainting
            Card adjacentCard = card.Deck.LastOrDefault(c => c != null && c.Index < index && c.TotalHitPoints > 0);
            int damage = card.TotalAttackPoints * card.Level;
            if (adjacentCard != null)
                queue.Add(new HurtCardCommand(adjacentCard, damage, card.Deck, index).Execute());
            // find card after the badger that is not fainting
            adjacentCard = card.Deck.FirstOrDefault(c => c != null && c.Index > index && c.TotalHitPoints > 0);
            if (adjacentCard != null)
                queue.Add(new HurtCardCommand(adjacentCard, damage, card.Deck, index).Execute());
            else
            {
                var opponent = card.Deck.Player.GetOpponentPlayer();
                if (opponent.Game.Fighting)
                {
                    // last card for the opponent that is not fainting
                    adjacentCard = opponent.BattleDeck.LastOrDefault(c => c != null && c.TotalHitPoints > 0);
                    if (adjacentCard != null)
                        queue.Add(new HurtCardCommand(adjacentCard, damage, card.Deck, index).Execute());
                }
            }
        }
    }

    public class BlowfishAbility : Ability
    {
        public BlowfishAbility() : base()
        {
            DefaultHP = 5;
            DefaultAttack = 3;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"Hurt => Deal {card.Level * 2} damage to a random enemy.";
        }

        public override void Hurt(CardCommandQueue queue, Card card)
        {
            base.Hurt(queue, card);
            var opponent = card.Deck.Player.GetOpponentPlayer();
            if (opponent.Game.Fighting)
            {
                var opponentCard = opponent.BattleDeck.GetRandomCard();
                if (opponentCard != null)
                    queue.Add(new HurtCardCommand(opponentCard, card.Level * 2, card.Deck, card.Index).Execute());
            }
        }
    }

    public class CamelAbility : Ability
    {
        public CamelAbility() : base()
        {
            DefaultHP = 5;
            DefaultAttack = 2;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"Hurt => Give friend behind +{card.Level} attack and +{card.Level * 2} health.";
        }

        public override void Hurt(CardCommandQueue queue, Card card)
        {
            base.Hurt(queue, card);
            Card priorCard = card.Deck.LastOrDefault(c => c != null && c.Index < card.Index && c.TotalHitPoints > 0);
            if (priorCard != null)
                queue.Add(new BuffCardCommand(priorCard, card.Index, card.Level * 2, card.Level).Execute());
        }
    }

    public class DogAbility : Ability
    {
        public DogAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 2;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"Friend summoned => Gain +{card.Level} attack or health.";
        }

        public override void FriendSummoned(CardCommandQueue queue, Card card, Card summonedCard)
        {
            base.FriendSummoned(queue, card, summonedCard);
            int hitPoints = 0;
            int attackPoints = 0;
            // 50/50 chance either 0 or 1
            if (card.Deck.Player.Game.Random.Next(0, 2) == 0)
                hitPoints = card.Level;
            else
                attackPoints = card.Level;
            queue.Add(new BuffCardCommand(card, card.Index, hitPoints, attackPoints).Execute());
        }
    }

    public class GiraffeAbility : Ability
    {
        public GiraffeAbility() : base()
        {
            DefaultHP = 5;
            DefaultAttack = 2;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"End of turn => Give {card.Level} friend(s) ahead +1/+1.";
        }    

        public override void RoundEnded(CardCommandQueue queue, Card card)
        {
            base.RoundEnded(queue, card);
            for (int i = 1; i <= card.Level; i++)
            {
                if (i + card.Index >= card.Deck.Size)
                    break;
                // not checking buffCard.TotalHitPoints > 0 because we aren't in a battle
                var buffCard = card.Deck[card.Index + i];
                if (buffCard != null)
                    queue.Add(new BuffCardCommand(buffCard, card.Index, 1, 1).Execute());
            }
        }
    }

    public class KangarooAbility : Ability
    {
        public KangarooAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 1;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"Friend ahead attacks => Gain +{card.Level * 2} attack and +{card.Level * 2} health.";
        }    

        public override void FriendAheadAttacks(CardCommandQueue queue, Card card)
        {
            base.FriendAheadAttacks(queue, card);
            queue.Add(new BuffCardCommand(card, card.Index, card.Level * 2, card.Level * 2).Execute());
        }
    }

    public class OxAbility : Ability
    {
        public OxAbility() : base()
        {
            DefaultHP = 4;
            DefaultAttack = 1;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"Friend ahead faints => Gain melon armor and +{card.Level * 2} attack.";
        }    

        public override void FriendAheadFaints(CardCommandQueue queue, Card card, int faintedIndex)
        {
            base.FriendAheadFaints(queue, card, faintedIndex);
            queue.Add(new GainFoodAbilityCommand(card, new MelonArmorAbility()).Execute());
            queue.Add(new BuffCardCommand(card, card.Index, 0, card.Level * 2).Execute());
        }
    }

    public class RabbitAbility : Ability
    {
        public RabbitAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 3;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"Friend eats shop food => Give it +{card.Level} health.";
        }

        public override void FriendAteFood(CardCommandQueue queue, Card card, Card friendCard)
        {
            base.FriendAteFood(queue, card, friendCard);
            queue.Add(new BuffCardCommand(friendCard, card.Index, card.Level, 0).Execute());
        }
    }

    public class SheepAbility : Ability
    {
        public SheepAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 2;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"Faint => Summon two {card.Level * 2}/{card.Level * 2} rams.";
        }

        public override void Fainted(CardCommandQueue queue, Card card, int index)
        {
            base.Fainted(queue, card, index);
            // for the first ram we have the empty slot because the sheep has fainted
            queue.Add(new SummonCardCommand(card, card.Deck, index, AbilityList.Instance.ZombieRamAbility.GetType(), 
                card.Level * 2, card.Level * 2).Execute());
            //...but second ram we use GetSummonIndex to attempt to find a spot for it
            int summonIndex = Ability.GetSummonIndex(queue, card.Deck, index);
            if (summonIndex != -1)
                queue.Add(new SummonCardCommand(card, card.Deck, summonIndex, AbilityList.Instance.ZombieRamAbility.GetType(), 
                    card.Level * 2, card.Level * 2).Execute());
        }
    }

    public class ZombieRamAbility : NoAbility
    {
        public ZombieRamAbility()
        {
            DefaultAttack = 2;
            DefaultHP = 2;
        }
    }

    public class SnailAbility : Ability
    {
        public SnailAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 2;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"Buy => If you lost last battle, give all friends +{card.Level}/+{card.Level}.";
        }

        public override void Bought(CardCommandQueue queue, Card card)
        {
            base.Bought(queue, card);
            if (card.Deck.Player.LostLastBattle)
                foreach (var c in card.Deck)
                    if (c != card)
                        queue.Add(new BuffCardCommand(c, card.Index, card.Level, card.Level).Execute());
        }
    }

    public class TurtleAbility : Ability
    {
        public TurtleAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 1;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"Faint => Give {card.Level} friend(s) behind melon armor.";
        }

        public override void Fainted(CardCommandQueue queue, Card card, int index)
        {
            base.Fainted(queue, card, index);
            for (int i = 1; i <= card.Level; i++)
            {
                if (index - i < 0)
                    break;
                var priorCard = card.Deck[index - i];
                if (priorCard != null)
                    queue.Add(new GainFoodAbilityCommand(priorCard, new MelonArmorAbility()).Execute());
            }
        }
    }


    // Tier 4
    public class BisonAbility : Ability
    {
        public override string GetAbilityMessage(Card card)
        {
            return $"End turn => If there's at least one level 3 friend, gain +{card.Level * 2}/+{card.Level * 2})";
        }

        public BisonAbility() : base()
        {
            DefaultHP = 6;
            DefaultAttack = 6;
        }

        public override void RoundEnded(CardCommandQueue queue, Card card)
        {
            base.RoundEnded(queue, card);
            if (card.Deck.Any((c) => c.Level == 3))
                queue.Add(new BuffCardCommand(card, card.Index, card.Level * 2, card.Level * 2).Execute());
        }
    }

    public class ZombieBusAbility : NoAbility
    {
        public ZombieBusAbility()
        {
            DefaultHP = 5;
            DefaultAttack = 5;
        }
    }

    public class DeerAbility : Ability
    {
        public DeerAbility() : base()
        {
            DefaultHP = 1;
            DefaultAttack = 1;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"Fainted => Summon a {card.Level * 5}/{card.Level * 5} bus with splash attack.)";
        }

        public override void Fainted(CardCommandQueue queue, Card card, int index)
        {
            base.Fainted(queue, card, index);
            queue.Add(new SummonCardCommand(card, card.Deck, index, AbilityList.Instance.ZombieBusAbility.GetType(), 
                card.Level * 5, card.Level * 5, 1, typeof(SplashAttackAbility)).Execute());
        }
    }

    public class DolphinAbility : Ability
    {
        public DolphinAbility() : base()
        {
            DefaultHP = 6;
            DefaultAttack = 4;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"Start of battle => Deal {card.Level * 5} damage to the lowest health enemy.)";
        }

        public override void BattleStarted2(CardCommandQueue queue, Card card)
        {
            base.BattleStarted2(queue, card);
            var opponent = card.Deck.Player.GetOpponentPlayer();
            if (opponent.BattleDeck.GetCardCount() > 0)
            {
                var targetCard = opponent.BattleDeck.Aggregate((minCard, nextCard) => 
                    minCard.TotalHitPoints < nextCard.TotalHitPoints ? minCard : nextCard);
                queue.Add(new HurtCardCommand(targetCard, card.Level * 5, card.Deck, card.Index).Execute());
            }
        }
    }

    public class HippoAbility : Ability
    {
        public HippoAbility() : base()
        {
            DefaultHP = 7;
            DefaultAttack = 4;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"Knockout => Gain +{card.Level * 2}/+{card.Level * 2}.";
        }

        public override void Knockout(CardCommandQueue queue, Card card)
        {
            base.Knockout(queue, card);
            queue.Add(new BuffCardCommand(card, card.Index, card.Level * 2, card.Level * 2).Execute());
        }
    }

    public class ParrotAbility : Ability
    {
        public ParrotAbility() : base()
        {
            DefaultHP = 3;
            DefaultAttack = 5;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"End of turn => Copy ability from friend ahead as level {card.Level} until the end of battle.";
        }

        public override void NewBattleDeck(Card card)
        {
            base.NewBattleDeck(card);
            if (card.Index + 1 < card.Deck.Size)
            {
                var friendCard = card.Deck[card.Index + 1];
                if (friendCard != null)
                    // card.RenderAbility will still be Parrot
                    card.Ability = Activator.CreateInstance(friendCard.Ability.GetType()) as Ability;
            }
        }
    }

    public class PenguinAbility : Ability
    {
        public PenguinAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 1;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"End of turn => Give other level 2 and 3 friends +{card.Level}/+{card.Level}.";
        }    

        public override void RoundEnded(CardCommandQueue queue, Card card)
        {
            base.RoundEnded(queue, card);
            foreach (var buffCard in card.Deck.Where((c) => c.Level == 2 || c.Level == 3))
            {
                // not checking buffCard.TotalHitPoints > 0 because we aren't in a battle
                queue.Add(new BuffCardCommand(buffCard, card.Index, 1, 1).Execute());
            }
        }
    }

    public class ZombieChickAbility : NoAbility
    {
        public ZombieChickAbility()
        {
            DefaultAttack = 1;
            DefaultHP = 1;
        }
    }

    public class RoosterAbility : Ability
    {
        public RoosterAbility() : base()
        {
            DefaultHP = 3;
            DefaultAttack = 5;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"Faint => Summon {card.Level} chick(s) with 1 health and half of the attack.";
        }    

        public override void Fainted(CardCommandQueue queue, Card card, int index)
        {
            base.Fainted(queue, card, index);
            for (int i = 1; i <= card.Level; i++)
            {
                int summonIndex = GetSummonIndex(queue, card.Deck, index);
                // doing integer division, so adding +1 to card.TotalAttackPoints to round up
                int attackPoints = (card.TotalAttackPoints + 1) / 2;
                queue.Add(new SummonCardCommand(card, card.Deck, summonIndex, AbilityList.Instance.ZombieChickAbility.GetType(), 
                    1, attackPoints).Execute());
            }
        }
    }

    public class SkunkAbility : Ability
    {
        public SkunkAbility() : base()
        {
            DefaultHP = 6;
            DefaultAttack = 3;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"Start of battle => Reduce health of the highest health enemy by {card.Level * 33}%.";
        }

        public override void BattleStarted2(CardCommandQueue queue, Card card)
        {
            base.BattleStarted2(queue, card);
            var opponent = card.Deck.Player.GetOpponentPlayer();
            if (opponent.BattleDeck.GetCardCount() > 0)
            {
                var targetCard = opponent.BattleDeck.Aggregate((maxCard, nextCard) => 
                    maxCard.TotalHitPoints > nextCard.TotalHitPoints ? maxCard : nextCard);
                int damage = (int)Math.Round(targetCard.TotalHitPoints * (((double)card.Level * 33) / 100));
                if (damage >= targetCard.TotalHitPoints)
                    damage = targetCard.TotalHitPoints - 1;
                queue.Add(new HurtCardCommand(targetCard, damage, card.Deck, card.Index).Execute());
            }
        }
    }

    public class SquirrelAbility : Ability
    {
        public SquirrelAbility() : base()
        {
            DefaultHP = 5;
            DefaultAttack = 2;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"Start of turn => Discount shop food by {card.Level} gold.";
        }

        public override void RoundStarted(Card card)
        {
            base.RoundStarted(card);
            // shop food could have been frozen from earlier round, so we'd be discounting it a second time
            // but we're ensuring that the cost will never get below zero
            card.Deck.Player.ShopFood1.Cost -= Math.Min(card.Deck.Player.ShopFood1.Cost, card.Level);
            card.Deck.Player.ShopFood2.Cost -= Math.Min(card.Deck.Player.ShopFood2.Cost, card.Level);
        }
    }

    public class WhaleAbility : Ability
    {
        Card _friendAhead;

        public WhaleAbility() : base()
        {
            DefaultHP = 6;
            DefaultAttack = 2;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"Start of battle => Swallow friend ahead and release it as level {card.Level} after fainting.";
        }

        public override void BattleStarted1(CardCommandQueue queue, Card card)
        {
            base.BattleStarted1(queue, card);
            if (card.Index + 1 < card.Deck.Size)
            {
                _friendAhead = card.Deck[card.Index + 1];
                // we faint the card in BattleStarted1 because we don't want other ability methods
                // to target this card within the same queue.
                if (_friendAhead != null && _friendAhead.TotalHitPoints > 0)
                    queue.Add(new FaintCardCommand(_friendAhead).Execute());
            }
        }

        public override void Fainted(CardCommandQueue queue, Card card, int index)
        {
            base.Fainted(queue, card, index);
            if (_friendAhead != null)
            {
                queue.Add(new SummonCardCommand(card, card.Deck, index, _friendAhead.Ability.GetType(), 
                    _friendAhead.TotalHitPoints, _friendAhead.TotalAttackPoints, card.Level, 
                    //TODO: restore food ability on the summoned card?
                    null, 
                    // in case we swallowed a parrot
                    _friendAhead.RenderAbility.GetType()).Execute());
            }
        }
    }

    public class WormAbility : Ability
    {
        public WormAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 2;
        }

        public override string GetAbilityMessage(Card card)
        {
            return $"Eats shop food => Gain +{card.Level}/+{card.Level}.";
        }

        public override void AteFood(CardCommandQueue queue, Card card)
        {
            base.AteFood(queue, card);
            queue.Add(new BuffCardCommand(card, card.Index, card.Level, card.Level).Execute());
        }
    }


    // Tier 5
    public class CowAbility : Ability
    {
        public CowAbility() : base()
        {
            DefaultHP = 6;
            DefaultAttack = 4;
        }
    }

    public class CrocodileAbility : Ability
    {
        public CrocodileAbility() : base()
        {
            DefaultHP = 4;
            DefaultAttack = 8;
        }
    }

    public class MonkeyAbility : Ability
    {
        public MonkeyAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 1;
        }
    }

    public class RhinoAbility : Ability
    {
        public RhinoAbility() : base()
        {
            DefaultHP = 8;
            DefaultAttack = 5;
        }
    }

    public class ScorpionAbility : Ability
    {
        public ScorpionAbility() : base()
        {
            DefaultHP = 1;
            DefaultAttack = 1;
        }
    }

    public class SealAbility : Ability
    {
        public SealAbility() : base()
        {
            DefaultHP = 8;
            DefaultAttack = 3;
        }
    }

    public class SharkAbility : Ability
    {
        public SharkAbility() : base()
        {
            DefaultHP = 4;
            DefaultAttack = 4;
        }
    }

    public class TurkeyAbility : Ability
    {
        public TurkeyAbility() : base()
        {
            DefaultHP = 4;
            DefaultAttack = 3;
        }
    }


    // Tier 6
    public class BoarAbility : Ability
    {
        public BoarAbility() : base()
        {
            DefaultHP = 6;
            DefaultAttack = 8;
        }
    }

    public class CatAbility : Ability
    {
        public CatAbility() : base()
        {
            DefaultHP = 5;
            DefaultAttack = 4;
        }
    }

    public class DragonAbility : Ability
    {
        public DragonAbility() : base()
        {
            DefaultHP = 8;
            DefaultAttack = 6;
        }
    }

    public class FlyAbility : Ability
    {
        public FlyAbility() : base()
        {
            DefaultHP = 5;
            DefaultAttack = 5;
        }
    }

    public class GorillaAbility : Ability
    {
        public GorillaAbility() : base()
        {
            DefaultHP = 9;
            DefaultAttack = 6;
        }
    }

    public class LeopardAbility : Ability
    {
        public LeopardAbility() : base()
        {
            DefaultHP = 4;
            DefaultAttack = 10;
        }
    }

    public class MammothAbility : Ability
    {
        public MammothAbility() : base()
        {
            DefaultHP = 10;
            DefaultAttack = 3;
        }
    }

    public class SnakeAbility : Ability
    {
        public SnakeAbility() : base()
        {
            DefaultHP = 6;
            DefaultAttack = 6;
        }
    }

    public class TigerAbility : Ability
    {
        public TigerAbility() : base()
        {
            DefaultHP = 3;
            DefaultAttack = 4;
        }
    }
}

