using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

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

        // Invoke ExecuteAbility for commands in the queue. If an ability creates further state changes, 
        // e.g. adds additional commands into the next queue, then continue invoking ExecuteAbility on those
        // 'child' commands. The resulting list is represents all successive changes caused by the original 'parent'
        // queue.
        public List<CardCommandQueue> CreateExecuteResult(Game game)
        {
            // disables events 
            game.BeginUpdate();
            var result = new List<CardCommandQueue>();
            CardCommandQueue queue = this;
            while (queue.Count > 0)
            {
                result.Add(queue);
                var nextQueue = new CardCommandQueue();
                //Debug.WriteLine("Next queue");
                foreach (var command in queue)
                {
                    //Debug.WriteLine(command.ToString());
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