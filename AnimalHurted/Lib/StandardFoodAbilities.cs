using System;
using System.Diagnostics;
using System.Linq;

namespace AnimalHurtedLib
{
    public class HoneyBeeAbility : FoodAbility
    {
        public override void Fainted(CardCommandQueue queue, Card card, int index)
        {
            base.Fainted(queue, card, index);
            int summonIndex = Ability.GetSummonIndex(queue, card.Deck, index);
            if (summonIndex != -1)
                queue.Add(new SummonCardCommand(card, card.Deck, summonIndex, 
                    typeof(ZombieBeeAbility), 1, 1).Execute());
        }
    }

    public class BoneAttackAbility : FoodAbility
    {
        public override void CalculatingDamage(Card card, ref int damage)
        {
            base.CalculatingDamage(card, ref damage);
            damage += 5;
        }
    }

    public class GarlicArmorAbility : FoodAbility
    {
        public override void Hurting(Card card, ref int damage)
        {
            base.Hurting(card, ref damage);
            damage = Math.Max(1, damage - 2);
        }
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

    public class SplashAttackAbility : FoodAbility
    {
        public override void Attacking(CardCommandQueue queue, Card card)
        {
            base.Attacking(queue, card);
            var opponent = card.Deck.Player.GetOpponentPlayer();
            var lastCard = opponent.BattleDeck.GetLastCard();
            if (lastCard != null && lastCard.Index > 0)
            {
                var targetCard = opponent.BattleDeck[lastCard.Index - 1];
                if (targetCard != null && targetCard.TotalHitPoints > 0)
                    queue.Add(new HurtCardCommand(targetCard, 5, card.Deck, card.Index).Execute());
            }
        }
    }
}