using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AnimalHurtedLib
{
    public class CardCommandQueue : IEnumerable<CardCommand>
    {
        List<CardCommand> _list = new List<CardCommand>();

        public int Count { get { return _list.Count; } }

        public void Add(CardCommand cardCommand)
        {
            _list.Add(cardCommand);
        }

        public List<CardCommandQueue> CreateExecuteResult(Game game)
        {
            game.BeginUpdate();
            var result = new List<CardCommandQueue>();
            CardCommandQueue queue = this;
            while (queue.Count > 0)
            {
                result.Add(queue);
                var nextQueue = new CardCommandQueue();
                foreach (var command in queue)
                {
                    command.ExecuteAbility(nextQueue);
                }
                queue = nextQueue;
            }
            game.EndUpdate();
            return result;
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