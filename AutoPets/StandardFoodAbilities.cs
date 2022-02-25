using System;
using System.Diagnostics;

namespace AutoPets
{
    public class HoneyBeeAbility : FoodAbility
    {
        public override void Fainted(CardCommandQueue queue, Card card, int index)
        {
            base.Fainted(queue, card, index);
            queue.Add(new SummonCardCommand(card, card.Deck, index, AbilityList.Instance.ZombieBeeAbility, 1, 1));
        }
    }

    public class BoneAttackAbility : FoodAbility
    {

    }

    public class GarlicAbility : FoodAbility
    {
        
    }
}