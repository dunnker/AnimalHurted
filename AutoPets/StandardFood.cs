using System;
using System.Diagnostics;

namespace AutoPets
{
    public class AppleFood : Food
    {
        public override void Execute(Card card)
        {
            base.Execute(card);
            card.HitPoints += 1;
            card.AttackPoints += 1;
        }
    }

    public class HoneyFood : Food
    {
        public override void Execute(Card card)
        {
            base.Execute(card);
            card.FoodAbility = new HoneyBeeAbility();
        }
    }

    public class CupcakeFood : Food
    {
        
    }

    public class MeatBoneFood : Food
    {
        
    }

    public class SleepingPillFood : Food
    {
        
    }

    public class GarlicFood : Food
    {
        
    }

    public class SaladBowlFood : Food
    {
        
    }

    public class CannedFoodFood : Food
    {
        
    }

    public class PearFood : Food
    {
        
    }

    public class ChiliFood : Food
    {
        
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
        
    }
}