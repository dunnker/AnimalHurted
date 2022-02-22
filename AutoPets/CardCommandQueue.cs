using System;
using System.Collections;
using System.Collections.Generic;

namespace AutoPets
{
    public class CardCommandQueue
    {
        Queue<CardCommand> _queue = new Queue<CardCommand>();

        public void Enqueue(CardCommand cardCommand)
        {
            _queue.Enqueue(cardCommand);
        }

        public CardCommand Dequeue()
        {
            return _queue.Dequeue();
        }

        public CardCommand Peek()
        {
            if (_queue.Count > 0)
                return _queue.Peek(); 
            else
                return null;
        }

        public CardCommandQueue Clone()
        {
            var result = new CardCommandQueue();
            result._queue = new Queue<CardCommand>(_queue);
            return result;
        }
    }
}