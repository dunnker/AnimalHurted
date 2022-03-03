using System;
using System.Diagnostics;

namespace AnimalHurtedLib
{
    public class Food
    {
        public int Cost { get; set; } = Game.FoodCost;

        public virtual string GetMessage()
        {
            return string.Empty;
        }

        public override string ToString()
        {
            return GetType().Name.Replace("Food", string.Empty);
        }

        // specifically for Cat to modify the effects of eating
        public void Eating(Card card, ref int hitPoints, ref int attackPoints)
        {
            foreach (var c in card.Deck)
                c.Ability.Eating(c, card, ref hitPoints, ref attackPoints);
        }

        public virtual void Execute(Card card)
        {

        }

		// this is not FoodAbility, but rather, if a Food needs to invoke some ability method, e.g. Faint
		// then it can do so with this method and add to the queue
        public virtual void ExecuteAbility(CardCommandQueue queue, Card card)
        {

        }
    }
}