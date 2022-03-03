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
        
    }

    public class SushiFood : Food
    {
        
    }

    public class MelonFood : Food
    {
        
    }

    public class MushroomFood : Food
    {
        
    }

    public class PizzaFood : Food
    {
        
    }

    public class SteakFood : Food
    {
        
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