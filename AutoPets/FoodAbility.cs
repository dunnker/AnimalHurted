using System;
using System.Diagnostics;

namespace AutoPets
{
    public class FoodAbility
    {
        public virtual void Fainted(CardCommandQueue queue, Card card, int index)
        {

        }

        public virtual void Attacking(CardCommandQueue queue, Card card)
        {

        }

        public virtual void CalculatingDamage(Card card, ref int damage)
        {

        }

        public virtual void Hurting(Card card, ref int damage)
        {

        }
    }
}