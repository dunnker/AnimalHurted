using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

namespace AnimalHurtedLib
{
    public class AppleFood : Food
    {
        public override string GetMessage()
        {
            return "Give a pet +1/+1.";
        }

        public override void Execute(Card card)
        {
            base.Execute(card);
            int hitPoints = 1;
            int attackPoints = 1;
            Eating(card, ref hitPoints, ref attackPoints);
            card.HitPoints += hitPoints;
            card.AttackPoints += attackPoints;
        }
    }

    public class HoneyFood : Food
    {
        public override string GetMessage()
        {
            return "Give a pet a Honey Bee.";
        }

        public override void Execute(Card card)
        {
            base.Execute(card);
            card.FoodAbility = new HoneyBeeAbility();
        }
    }

    public class CupcakeFood : Food
    {
        public override string GetMessage()
        {
            return "Give a pet +3/+3 until end of battle.";
        }

        public override void Execute(Card card)
        {
            base.Execute(card);
            int hitPoints = 3;
            int attackPoints = 3;
            Eating(card, ref hitPoints, ref attackPoints);
            card.BuildHitPoints += hitPoints;
            card.BuildAttackPoints += attackPoints;
        }
    }

    public class MeatBoneFood : Food
    {
        public override string ToString()
        {
            return "Meat Bone";
        }

        public override string GetMessage()
        {
            return "Give a pet Bone Attack.";
        }

        public override void Execute(Card card)
        {
            base.Execute(card);
            card.FoodAbility = new BoneAttackAbility();
        }
    }

    public class SleepingPillFood : Food
    {
        public SleepingPillFood()
        {
            Cost = 1;
        }

        public override string ToString()
        {
            return "Sleeping Pill";
        }

        public override string GetMessage()
        {
            return "Make a friendly pet faint.";
        }

        public override void ExecuteAbility(CardCommandQueue queue, Card card)
        {
            base.ExecuteAbility(queue, card);
            queue.Add(new FaintCardCommand(card).Execute());
        }
    }

    public class GarlicFood : Food
    {
        public override string GetMessage()
        {
            return "Give a pet garlic armor.";
        }

        public override void Execute(Card card)
        {
            base.Execute(card);
            card.FoodAbility = new GarlicArmorAbility();
        }
    }

    public class SaladBowlFood : Food
    {
        public override string ToString()
        {
            return "Salad Bowl";
        }
        
        public override string GetMessage()
        {
            return "Give 2 random pets +1/+1.";
        }

        public override void Execute(Card card)
        {
            base.Execute(card);
            var hashSet = new HashSet<int>();
            for (int i = 1; i <= 2; i++)
            {
                var buffCard = card.Deck.GetRandomCard(hashSet);
                if (buffCard != null)
                {
                    int hitPoints = 1;
                    int attackPoints = 1;
                    Eating(buffCard, ref hitPoints, ref attackPoints);
                    buffCard.HitPoints += hitPoints;
                    buffCard.AttackPoints += attackPoints;

                    // exclude this buffCard for the search for the next random card
                    hashSet.Add(buffCard.Index);
                }
            }
        }
    }

    public class CannedFoodFood : Food
    {
        public override string ToString()
        {
            return "Canned Food";
        }
        
        public override string GetMessage()
        {
            return "Give all current and future shop pets +2 attack and +1 health.";
        }

        public override void Execute(Card card)
        {
            card.Deck.Player.BuffHitPoints += 1;
            card.Deck.Player.BuffAttackPoints += 2;
            card.Deck.Player.BuffShopDeck();
        }
    }

    public class PearFood : Food
    {
        public override string GetMessage()
        {
            return "Give a pet +2/+2.";;
        }
        
        public override void Execute(Card card)
        {
            base.Execute(card);
            int hitPoints = 2;
            int attackPoints = 2;
            Eating(card, ref hitPoints, ref attackPoints);
            card.HitPoints += hitPoints;
            card.AttackPoints += attackPoints;
        }
    }

    public class ChiliFood : Food
    {
        public override string GetMessage()
        {
            return "Give a pet Splash Attack.";
        }

        public override void Execute(Card card)
        {
            base.Execute(card);
            card.FoodAbility = new SplashAttackAbility();
        }
    }

    public class ChocolateFood : Food
    {
        int _oldLevel;

        public override string GetMessage()
        {
            return "Give a pet +1 experience.";
        }
        
        public override void Execute(Card card)
        {
            base.Execute(card);
            _oldLevel = card.Level;
            card.XP += 1;
        }

        public override void ExecuteAbility(CardCommandQueue queue, Card card)
        {
            base.ExecuteAbility(queue, card);
            if (card.Level > _oldLevel)
                card.GainedXP(queue, _oldLevel);
        }
    }

    public class SushiFood : Food
    {
        public override string GetMessage()
        {
            return "Give 3 random pets +1/+1.";
        }

        public override void Execute(Card card)
        {
            base.Execute(card);
            var hashSet = new HashSet<int>();
            for (int i = 1; i <= 3; i++)
            {
                var randomCard = card.Deck.GetRandomCard(hashSet);
                if (randomCard != null)
                {
                    int hitPoints = 1;
                    int attackPoints = 1;
                    Eating(randomCard, ref hitPoints, ref attackPoints);
                    randomCard.HitPoints += hitPoints;
                    randomCard.AttackPoints += attackPoints;
                    hashSet.Add(randomCard.Index);
                }
            }
        }
    }

    public class MelonFood : Food
    {
        public override string GetMessage()
        {
            return "Give a pet Melon Armor.";
        }

        public override void Execute(Card card)
        {
            base.Execute(card);
            card.FoodAbility = new MelonArmorAbility();
        }
    }

    public class MushroomFood : Food
    {
        
    }

    public class PizzaFood : Food
    {
        public override string GetMessage()
        {
            return "Give 2 random pets +2/+2.";
        }

        public override void Execute(Card card)
        {
            base.Execute(card);
            var hashSet = new HashSet<int>();
            for (int i = 1; i <= 2; i++)
            {
                var randomCard = card.Deck.GetRandomCard(hashSet);
                if (randomCard != null)
                {
                    int hitPoints = 2;
                    int attackPoints = 2;
                    Eating(randomCard, ref hitPoints, ref attackPoints);
                    randomCard.HitPoints += hitPoints;
                    randomCard.AttackPoints += attackPoints;
                    hashSet.Add(randomCard.Index);
                }
            }
        }
    }

    public class SteakFood : Food
    {
        public override string GetMessage()
        {
            return "Give a pet Steak Attack.";
        }

        public override void Execute(Card card)
        {
            base.Execute(card);
            card.FoodAbility = new SteakAttackAbility();
        }
    }

    public class MilkFood : Food
    {
        public int HitPoints { get; set; }
        public int AttackPoints { get; set; }

        public override string GetMessage()
        {
             return $"Give a pet +{AttackPoints} attack and +{HitPoints} health.";
        }

        public override void Execute(Card card)
        {
            base.Execute(card);
            int hitPoints = HitPoints;
            int attackPoints = AttackPoints;
            Eating(card, ref hitPoints, ref attackPoints);
            card.HitPoints += hitPoints;
            card.AttackPoints += attackPoints;
       }
    }
}