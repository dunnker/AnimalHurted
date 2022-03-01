using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using AutoPets;

public class CardCommandQueueReader
{
    int _queueIndex = -1;
    List<CardCommandQueue> _list;
    string _signalName;
    Node _node;

    public CardCommandQueueReader(Node node, List<CardCommandQueue> list, string signalName)
    {
        _list = list;
        _signalName = signalName;
        _node = node;
    }

    public void Reset()
    {
        _queueIndex = -1;
    }

    public bool Finished { get { return _queueIndex >= _list.Count - 1; } }

    public void Execute()
    {
        _queueIndex++;
        if (_queueIndex < _list.Count)
        {
            var queue = _list[_queueIndex];
            // Get notification of when the last command is finished animation
            // Any event handler associated with the last command invokes UserEvent when finished playing
            // its animation to notify when done.
            if (queue.Last().UserEvent != null)
                throw new Exception("UserEvent assigned");
            queue.Last().UserEvent += (sender, e) => {
                queue.Last().UserEvent = null; // garbage collect   
                _node.EmitSignal(_signalName);
            };    

            foreach (var command in queue)
            {
                // Each command Execute might inflict damage (HurtCommand), faint a pet (FaintCommand)
                // buff the health of a pet (BuffCommand) etc.
                // In turn, a corresonding event is fired off, OnHurt, OnFaint etc. and 
                // the DeckNode2D scene responds to the event and renders an animation 

                command.Execute();

                // not executing abilities during replay, because abilities
                // have already been processed, for example in Game.CreateFightResult():
                //command.ExecuteAbility()
            }
        }
    }
}

