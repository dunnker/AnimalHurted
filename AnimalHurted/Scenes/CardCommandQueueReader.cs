using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Godot;
using AnimalHurtedLib;

public class CardCommandQueueReader
{
    int _queueIndex = -1;
    List<CardCommandQueue> _list;
    string _signalName;
    Node _node;

    public SemaphoreSlim Signal = new SemaphoreSlim(0, 1);

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

    public async void Execute()
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

            CardCommand lastCommand = null;

            foreach (var command in queue)
            {
                // Each command Execute might inflict damage (HurtCommand), faint a pet (FaintCommand)
                // buff the health of a pet (BuffCommand) etc.
                // In turn, a corresonding event is fired off, OnHurt, OnFaint etc. and 
                // the DeckNode2D scene responds to the event and renders an animation 

                //GD.Print(command.ToString());

                // serialize animations for summon command
                if (command is SummonCardCommand)
                {
                    if (command != queue.First() && !(lastCommand is SummonCardCommand))
                        // wait for last animation(s) to finish
                        await _node.ToSignal(_node.GetTree().CreateTimer((_node as IBattleNode).MaxTimePerEvent), "timeout");

                    command.Execute();
                    // now wait for summon animation to finish
                    await Signal.WaitAsync();
                }
                else
                    command.Execute();

                // not executing abilities during replay, because abilities
                // have already been processed, for example in Game.CreateFightResult():
                //command.ExecuteAbility()

                lastCommand = command;
            }
        }
    }
}

