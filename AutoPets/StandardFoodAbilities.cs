using System;
using System.Diagnostics;
using System.Linq;

namespace AutoPets
{
    public class HoneyBeeAbility : FoodAbility
    {
        public override void Fainted(CardCommandQueue queue, Card card, int index)
        {
            base.Fainted(queue, card, index);
            int summonIndex = Ability.GetSummonIndex(queue, card.Deck, index);
            if (summonIndex != -1)
                queue.Add(new SummonCardCommand(card, card.Deck, summonIndex, AbilityList.Instance.ZombieBeeAbility, 1, 1));
        }
    }

    public class BoneAttackAbility : FoodAbility
    {
        public override void Attacking(Card card, ref int damage)
        {
            base.Attacking(card, ref damage);
            damage += 5;
        }
    }

    public class GarlicAbility : FoodAbility
    {
        
    }

    public class MelonArmorAbility : FoodAbility
    {
        public override void Hurting(Card card, ref int damage)
        {
            base.Hurting(card, ref damage);
            damage = Math.Max(0, damage - 20);
            card.FoodAbility = null; // remove the melon armor after first damage
        }
    }
}