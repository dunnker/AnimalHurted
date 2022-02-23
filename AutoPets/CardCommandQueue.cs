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