using Godot;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MonteCarlo;
using AnimalHurtedLib;
using AnimalHurtedLib.AI;

public class AIProgressNode : Node
{
    const int MaxIterations = 50000;
    bool _abort;

    IOrderedEnumerable<MonteCarloTreeSearch.Node<GameAIPlayer, Move>> _result;
    
    public ProgressBar ProgressBar { get { return GetNode<ProgressBar>("ProgressBar"); } }

    [Signal]
    public delegate void ProgressSignal();

    [Signal]
    public delegate void ProgressFinishedSignal();

    public override void _Ready()
    {
        Connect("ProgressSignal", this, "_signal_Progress", null, (int)ConnectFlags.Deferred);
        Connect("ProgressFinishedSignal", this, "_signal_ProgressFinished", null, (int)ConnectFlags.Deferred);

        ProgressBar.MaxValue = MaxIterations;

        new System.Threading.Thread(() => {
            AnimalHurtedLib.AI.GameAIState rootState = new AnimalHurtedLib.AI.GameAIState(true, GameSingleton.Instance.Game,
                GameSingleton.Instance.Game.Player2);

            MonteCarloTreeSearch.Node<GameAIPlayer, Move> rootNode = new MonteCarloTreeSearch.Node<GameAIPlayer, Move>(rootState);

            rootNode.BuildTree(new Func<int, long, bool>((numIterations, elapsedMs) => { 
                if (numIterations % 100 == 0)
                    EmitSignal("ProgressSignal", numIterations);
                return !_abort && numIterations < MaxIterations;
            }));

            _result = rootNode.Children.OrderByDescending(n => n.NumRuns);
            
            EmitSignal("ProgressFinishedSignal");
        }).Start();
    }

    public void _signal_Progress(int numIterations)
    {
        ProgressBar.Value = numIterations;
    }

    public void _signal_ProgressFinished()
    {
        var move = _result.FirstOrDefault();
        move?.Action.ExecuteActions(GameSingleton.Instance.Game.Player2);
        BuildNode.StartBattle(this);
    }

    public void _on_ContinueButton_pressed()
    {
        _abort = true;
        GetNode<Button>("ContinueButton").Disabled = true;
    }
}
