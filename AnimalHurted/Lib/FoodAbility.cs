using System;
using System.Diagnostics;

namespace AnimalHurtedLib
{
    public class FoodAbility
    {
        public virtual void Fainted(CardCommandQueue queue, Card card, int index)
        {

        }

        public virtual void Attacking(CardCommandQueue queue, Card card, Card opponentCard = null)
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