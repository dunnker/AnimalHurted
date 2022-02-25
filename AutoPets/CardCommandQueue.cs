using System;
using System.Collections;
using System.Collections.Generic;

namespace AutoPets
{
    public class CardCommandQueue : IEnumerable<CardCommand>
    {
        List<CardCommand> _list = new List<CardCommand>();

        public int Count { get { return _list.Count; } }

        public void Add(CardCommand cardCommand)
        {
            _list.Add(cardCommand);
        }

        public void Execute()
        {
            var queue = this;
            while (queue.Count > 0)
            {
                var nextQueue = new CardCommandQueue(); 
                foreach (var command in queue)
                {
                    command.Execute().ExecuteAbility(nextQueue);
                }
                queue = nextQueue;
            }
        }        

        public IEnumerator<CardCommand> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }
}