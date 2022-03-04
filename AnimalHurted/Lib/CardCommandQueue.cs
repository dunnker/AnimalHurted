using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AnimalHurtedLib
{
    public class CardCommandQueue : IEnumerable<CardCommand>
    {
        CardCommandQueue _parentQueue;
        CardCommand _parentCommand;

        public CardCommandQueue(CardCommandQueue parentQueue = null)
        {
            _parentQueue = parentQueue;
        }

        List<CardCommand> _list = new List<CardCommand>();

        public int Count { get { return _list.Count; } }

        public void Add(CardCommand cardCommand)
        {
            _list.Add(cardCommand);
        }

        public void CardMoving(Deck deck, int from, int to)
        {
            foreach (var command in _parentQueue.SkipWhile((c) => c != _parentCommand).Skip(1))
                command.CardMoving(deck, from, to);
        }

        public List<CardCommandQueue> CreateExecuteResult(Game game)
        {
            game.BeginUpdate();
            var result = new List<CardCommandQueue>();
            CardCommandQueue queue = this;
            while (queue.Count > 0)
            {
                result.Add(queue);
                // queue is the "parent" to nextQueue
                var nextQueue = new CardCommandQueue(queue);
                foreach (var command in queue)
                {
                    // store the current location that is being read in _parentCommand
                    nextQueue._parentCommand = command;

                    // if this ability method adds a summon command to nextQueue, and that command moves cards
                    // then CardMoving will be responsible for updating the new index locations from this
                    // command (the _parentCommand) forward
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