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
                queue.Add(new BuffCardCommand(buffCard, index, card.Level, 2 * card.Level));
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
            queue.Add(new SummonCardCommand(card, card.Deck, index, AbilityList.Instance.ZombieCricketAbility, card.Level, card.Level));
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

        public override void Bought(Card card)
        {
            base.Bought(card);
            // find and buff a random card that is not the otter
            var buffCard = card.Deck.GetRandomCard(new HashSet<int>() { card.Index });
            if (buffCard != null)
                buffCard.Buff(card.Index, card.Level, card.Level);
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

        public override void Sold(Card card, int index)
        {
            //TODO the second card found might be the same as the first one we found
            // but SAP doesn't work that way, it finds two unique cards to buff
            // we could keep our game the way it is, or change it to be more like SAP
            // e.g. we could change GetRandomCard to allow more than one index to exclude in its search
            // probably best way to implement that would be to dynamically create a 
            // new array of pets that are exclusive to the otter and the first pet we found
            // and then get a random pet from that array
            var buffCard1 = card.Deck.GetRandomCard(new HashSet<int>() { index });
            var buffCard2 = card.Deck.GetRandomCard(new HashSet<int>() { index });
            if (buffCard1 != null && buffCard2 != null)
            {
                buffCard1.Buff(index, card.Level, 0);
                buffCard2.Buff(index, card.Level, 0);
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

        public override void BattleStarted(CardCommandQueue queue, Card card)
        {
            var opponent = card.Deck.Player.GetOpponentPlayer();
            var excludingIndexes = new HashSet<int>();
            for (int i = 1; i <= card.Level; i++)
            {
                var randomCard = opponent.BattleDeck.GetRandomCard(excludingIndexes);
                if (randomCard != null)
                {
                    queue.Add(new HurtCardCommand(randomCard, 1, card.Deck, card.Index));
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

        public override void Sold(Card card, int index)
        {
            base.Sold(card, index);
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

        public override void Sold(Card card, int index)
        {
            base.Sold(card, index);
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
            return string.Format("Level-up => Give all friends {0} health.", card.Level);
        }

        public override void LeveledUp(Card card)
        {
            base.LeveledUp(card);
            foreach (var tempCard in card.Deck)
            {
                if (tempCard != card)
                    tempCard.Buff(card.Index, card.Level - 1, card.Level - 1);
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
            queue.Add(new BuffCardCommand(summonedCard, card.Index, 0, card.Level));
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

        public override void Bought(Card card)
        {
            base.Bought(card);
            var maxCard = card.Deck.OrderByDescending(c => c.HitPoints).First();
            card.HitPoints = maxCard.HitPoints;
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
                    attackPoints = (card.AttackPoints + 1) / 2;
                    attackPercent = 50;
                    break;
                // buff 100% of dodo's attack
                case 2:
                    attackPoints = card.AttackPoints;
                    attackPercent = 100;
                    break;
                // buff 150% of dodo's attack
                case 3:
                    attackPoints = card.AttackPoints + ((card.AttackPoints + 1) / 2);
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

        public override void BattleStarted(CardCommandQueue queue, Card card)
        {
            base.BattleStarted(queue, card);
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
                queue.Add(new BuffCardCommand(nextCard, card.Index, 0, attackPoints));
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
                queue.Add(new HurtCardCommand(priorCard, card.Level, card.Deck, card.Index));
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
                    queue.Add(new BuffCardCommand(priorCard, index, card.Level, card.Level));
            }
            if (index > 1)
            {
                var priorCard = card.Deck[index - 2];
                if (priorCard != null)
                    queue.Add(new BuffCardCommand(priorCard, index, card.Level, card.Level));
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
				// checking HitPoints > 0; see comments in HurtCommand
                if (c != card && c.HitPoints > 0)
                    queue.Add(new HurtCardCommand(c, 2 * card.Level, card.Deck, index));
            if (card.Deck.Player.Game.Fighting)
                foreach (var c in opponent.BattleDeck)
					// checking HitPoints > 0; see comments in HurtCommand
                    if (c.HitPoints > 0)
                        queue.Add(new HurtCardCommand(c, 2 * card.Level, card.Deck, index));
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
            // if not about to faint, then buff itself
            if (card.HitPoints > 0)
                queue.Add(new BuffCardCommand(card, card.Index, 0, (card.AttackPoints / 2) * card.Level));
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
                        queue.Add(new SummonCardCommand(card, opponent.BattleDeck, opponent.BattleDeck.Size - i, 
                            AbilityList.Instance.DirtyRatAbility, 1, 1));
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

        public override void FriendSold(Card card, Card soldCard)
        {
            base.FriendSold(card, soldCard);
            var buffCard = card.Deck.GetRandomCard(new HashSet<int> { card.Index });
            if (buffCard != null)
                buffCard.Buff(card.Index, card.Level, 0);
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
            queue.Add(new SummonCardCommand(card, card.Deck, index, ability, 2, 2, card.Level));
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

        public override void NewRoundStarted(Card card)
        {
            base.NewRoundStarted(card);
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
    }

    public class BlowfishAbility : Ability
    {
        public BlowfishAbility() : base()
        {
            DefaultHP = 5;
            DefaultAttack = 3;
        }
    }

    public class CamelAbility : Ability
    {
        public CamelAbility() : base()
        {
            DefaultHP = 5;
            DefaultAttack = 2;
        }
    }

    public class DogAbility : Ability
    {
        public DogAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 2;
        }
    }

    public class GiraffeAbility : Ability
    {
        public GiraffeAbility() : base()
        {
            DefaultHP = 5;
            DefaultAttack = 2;
        }
    }

    public class KangarooAbility : Ability
    {
        public KangarooAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 1;
        }
    }

    public class OxAbility : Ability
    {
        public OxAbility() : base()
        {
            DefaultHP = 4;
            DefaultAttack = 1;
        }
    }

    public class RabbitAbility : Ability
    {
        public RabbitAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 3;
        }
    }

    public class SheepAbility : Ability
    {
        public SheepAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 2;
        }
    }

    public class SnailAbility : Ability
    {
        public SnailAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 2;
        }
    }

    public class TurtleAbility : Ability
    {
        public TurtleAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 1;
        }
    }


    // Tier 4
    public class BisonAbility : Ability
    {
        public BisonAbility() : base()
        {
            DefaultHP = 6;
            DefaultAttack = 6;
        }
    }

    public class DeerAbility : Ability
    {
        public DeerAbility() : base()
        {
            DefaultHP = 1;
            DefaultAttack = 1;
        }
    }

    public class DolphinAbility : Ability
    {
        public DolphinAbility() : base()
        {
            DefaultHP = 6;
            DefaultAttack = 4;
        }
    }

    public class HippoAbility : Ability
    {
        public HippoAbility() : base()
        {
            DefaultHP = 7;
            DefaultAttack = 4;
        }
    }

    public class ParrotAbility : Ability
    {
        public ParrotAbility() : base()
        {
            DefaultHP = 3;
            DefaultAttack = 5;
        }
    }

    public class PenguinAbility : Ability
    {
        public PenguinAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 1;
        }
    }

    public class RoosterAbility : Ability
    {
        public RoosterAbility() : base()
        {
            DefaultHP = 3;
            DefaultAttack = 5;
        }
    }

    public class SkunkAbility : Ability
    {
        public SkunkAbility() : base()
        {
            DefaultHP = 6;
            DefaultAttack = 3;
        }
    }

    public class SquirrelAbility : Ability
    {
        public SquirrelAbility() : base()
        {
            DefaultHP = 5;
            DefaultAttack = 2;
        }
    }

    public class WhaleAbility : Ability
    {
        public WhaleAbility() : base()
        {
            DefaultHP = 6;
            DefaultAttack = 2;
        }
    }

    public class WormAbility : Ability
    {
        public WormAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 2;
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

