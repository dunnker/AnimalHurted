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

        public override void Fainted(Card card, int index)
        {
            base.Fainted(card, index);
            var buffCard = card.Deck.GetRandomCard(index);
            if (buffCard != null)
            {
                card.Deck.Player.Game.OnAbilityEvent(this, card, index, string.Format("Faint => Give a random friend +{0} attack and +{1} health.", 2 * card.Level, card.Level));
                buffCard.Buff(index, card.Level, 2 * card.Level);
            }
        }
    }

    public class CricketAbility : Ability
    {
        public CricketAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 1;
        }

        public override void Fainted(Card card, int index)
        {
            base.Fainted(card, index);
            // cricket is no longer in the deck
            Debug.Assert(card.Index == -1);
            // ...so we have the empty slot to place the zombie cricket
            Card newCard = new Card(card.Deck, AbilityList.Instance.ZombieCricketAbility)
            {
                HitPoints = card.Level,
                AttackPoints = card.Level
            };
            card.Deck.Player.Game.OnAbilityEvent(this, card, index, string.Format("Faint => Summon a {0}/{1} cricket.", card.Level, card.Level));
            newCard.Summon(index);
        }
    }

    public class ZombieCricketAbility : NoAbility
    {
        public override string ToString()
        {
            return "* Cricket";
        }
    }

    public class OtterAbility : Ability
    {
        public OtterAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 1;
        }

        public override void Bought(Card card)
        {
            base.Bought(card);
            // find and buff a random card that is not the otter
            var buffCard = card.Deck.GetRandomCard(card.Index);
            if (buffCard != null)
            {
                card.Deck.Player.Game.OnAbilityEvent(this, card, card.Index, string.Format("Buy => Give a random friend +{0} attack and +{1} health.", card.Level, card.Level));
                buffCard.Buff(card.Index, card.Level, card.Level);
            }
        }
    }

    public class BeaverAbility : Ability
    {
        public BeaverAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 2;
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
            var buffCard1 = card.Deck.GetRandomCard(index);
            var buffCard2 = card.Deck.GetRandomCard(index);
            if (buffCard1 != null && buffCard2 != null)
            {
                card.Deck.Player.Game.OnAbilityEvent(this, card, index, string.Format("Sell => Give two random friends +{0} health.", card.Level));
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

        public override void BattleStarted(Card card)
        {
            var opponent = card.Deck.Player.GetOpponentPlayer();
            //TODO: are the random enemies found all unique?
            card.Deck.Player.Game.OnAbilityEvent(this, card, card.Index, string.Format("Start of battle => Deal 1 damage to {0} random enemies.", card.Level));
            for (int i = 1; i <= card.Level; i++)
            {
                var randomCard = opponent.BattleDeck.GetRandomCard();
                if (randomCard != null)
                    randomCard.Hurt(1, card);
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

        public override void Sold(Card card, int index)
        {
            base.Sold(card, index);
            card.Deck.Player.Game.OnAbilityEvent(this, card, card.Index, string.Format("Sell => Gain an extra {0} gold.", card.Level));
            card.Deck.Player.Gold += card.Level;
            card.Deck.OnDeckEvent(card, string.Format(string.Format("{0}. {1} sold. +{2} bonus gold", index + 1, ToString(), card.Level)));
        }
    }

    public class DuckAbility : Ability
    {
        public DuckAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 1;
        }

        public override void Sold(Card card, int index)
        {
            base.Sold(card, index);
            card.Deck.Player.Game.OnAbilityEvent(this, card, index, string.Format("Sell => Give shop pets {0} health.", card.Level));
            foreach (var shopCard in card.Deck.Player.ShopDeck)
                shopCard.Buff(-1, card.Level, 0);
            card.Deck.OnDeckEvent(card, string.Format(string.Format("{0}. {1} sold. shop pets +{2} hitpoints", index + 1, ToString(), card.Level)));
        }
    }

    public class FishAbility : Ability
    {
        public FishAbility() : base()
        {
            DefaultHP = 3;
            DefaultAttack = 2;
        }

        public override void LeveledUp(Card card)
        {
            base.LeveledUp(card);
            if (card.Deck.GetCardCount() > 0)
            {
                card.Deck.Player.Game.OnAbilityEvent(this, card, card.Index, string.Format("Level-up => Give all friends {0} health.", card.Level));
                foreach (var tempCard in card.Deck)
                {
                    if (tempCard != card)
                        tempCard.Buff(card.Index, card.Level - 1, card.Level - 1);
                }
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

        public override void FriendSummoned(Card card, Card summonedCard)
        {
            base.FriendSummoned(card, summonedCard);
            //TODO: SAP is worded "Give it +? attack until end of battle." and it underscores the attack points during build and battle
            // after battle, if the friend was summoned during build, the friend's attack points will revert for next build
            card.Deck.Player.Game.OnAbilityEvent(this, card, card.Index, string.Format("Friend summoned => Give it +{0} attack.", card.Level));
            summonedCard.Buff(card.Index, 0, card.Level);
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

        public override void Bought(Card card)
        {
            base.Bought(card);
            var maxCard = card.Deck.OrderByDescending(c => c.HitPoints).First();
            card.Deck.Player.Game.OnAbilityEvent(this, card, card.Index, "Buy => Copy health from the most healthy friend.");
            card.HitPoints = maxCard.HitPoints;
            card.Deck.OnDeckEvent(card, string.Format(string.Format("{0}. {1} copied hit points from {2}. {3}", card.Index + 1, ToString(), maxCard.Index + 1, maxCard.Ability.ToString())));
        }
    }

    public class DodoAbility : Ability
    {
        public DodoAbility() : base()
        {
            DefaultHP = 3;
            DefaultAttack = 2;
        }

        public override void BattleStarted(Card card)
        {
            base.BattleStarted(card);
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
                int attackPoints = 0;
                int attackPercent = 0;
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
                        Debug.Assert(false);
                        break;
                }
                card.Deck.Player.Game.OnAbilityEvent(this, card, card.Index, string.Format("Start of battle => Give {0}% of Dodo's attack to friend ahead.", attackPercent));
                nextCard.Buff(card.Index, 0, attackPoints);
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

        public override void BeforeAttack(Card card)
        {
            base.BeforeAttack(card);
            Debug.Assert(card.Index != -1);
            Card priorCard = null;
            if (card.Index > 0)
                priorCard = card.Deck[card.Index - 1];
            if (priorCard != null)
            {
                // not invoking attack as that would recursively call BeforeAttack in an infinite loop!
                //card.Attack(priorCard, card.Index, card.Level);

                card.Deck.Player.Game.OnAbilityEvent(this, card, card.Index, string.Format("Before attack => Deal {0} damage to friend behind.", card.Level));
                card.Deck.OnDeckEvent(card, string.Format(string.Format("{0}. {1} attacked {2}. {3}", card.Index + 1, ToString(), priorCard.Index + 1, priorCard.Ability.ToString())));
                priorCard.Hurt(card.Level, card);
            }
        }
    }

    public class FlamingoAbility : Ability
    {
        public FlamingoAbility() : base()
        {
            DefaultHP = 1;
            DefaultAttack = 3;
        }
		
        public override void Fainted(Card card, int index)
        {
            base.Fainted(card, index);
            Card priorCard1 = null;
            if (index > 0)
                priorCard1 = card.Deck[index - 1];
            Card priorCard2 = null;
            if (index > 1)
                priorCard2 = card.Deck[index - 2];
            if (priorCard1 != null || priorCard2 != null)
            {
                card.Deck.Player.Game.OnAbilityEvent(this, card, index, string.Format("Faint => Give the two friends behind +{0} attack and +{1} health.", card.Level, card.Level));
                if (priorCard1 != null)
                    priorCard1.Buff(index, card.Level, card.Level);
                if (priorCard2 != null)
                    priorCard2.Buff(index, card.Level, card.Level);
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

        public override void Fainted(Card card, int index)
        {
            base.Fainted(card, index);
            var opponent = card.Deck.Player.GetOpponentPlayer();
            if (card.Deck.GetCardCount() > 0 || 
                (card.Deck.Player.Game.Fighting && opponent.BattleDeck.GetCardCount() > 0))
            {
                card.Deck.Player.Game.OnAbilityEvent(this, card, index, 
                    string.Format("Faint => Deal {0} damage to all.", 2 * card.Level));
                foreach (var c in card.Deck)
                    c.Hurt(2 * card.Level, card);
                if (card.Deck.Player.Game.Fighting)
                    foreach (var c in opponent.BattleDeck)
                        c.Hurt(2 * card.Level, card);
            }
        }
    }

    public class PeacockAbility : Ability
    {
        public PeacockAbility() : base()
        {
            DefaultHP = 5;
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
    }

    public class ShrimpAbility : Ability
    {
        public ShrimpAbility() : base()
        {
            DefaultHP = 3;
            DefaultAttack = 2;
        }
    }

    public class SpiderAbility : Ability
    {
        public SpiderAbility() : base()
        {
            DefaultHP = 2;
            DefaultAttack = 2;
        }
    }

    public class SwanAbility : Ability
    {
        public SwanAbility() : base()
        {
            DefaultHP = 3;
            DefaultAttack = 3;
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

