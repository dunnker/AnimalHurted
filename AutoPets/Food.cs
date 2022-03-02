using System;
using System.Diagnostics;

namespace AutoPets
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

        public virtual void Execute(Card card)
        {

        }

		// this is not FoodAbility, but rather, if a Food needs to invoke some ability method
		// then it can do so with this method and add to the queue
        public virtual void ExecuteAbility(CardCommandQueue queue, Card card)
        {

        }
    }
}