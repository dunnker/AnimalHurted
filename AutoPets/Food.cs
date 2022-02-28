using System;
using System.Diagnostics;

namespace AutoPets
{
    public class Food
    {
        public virtual string GetMessage()
        {
            return string.Empty;
        }

        public override string ToString()
        {
            return GetType().Name.Replace("Food", string.Empty);
        }

        public virtual void Execute(CardCommandQueue queue, Card card)
        {

        }
    }
}