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

        ProgressBar.MaxValue = AISingleton.AIMaxIterations;

        // with HardMode, we don't start thread until player 1 has finished building their deck
        if (AISingleton.Instance.HardMode)
            AISingleton.Instance.StartAIThread();

        AISingleton.Instance.SetAIDelegates(out bool aiFinished, out _result, AIProgress, AIFinished);
        if (aiFinished)
            EmitSignal("ProgressFinishedSignal");
    }

    // thread event
    void AIProgress(object sender, int iterationCount, out bool abort)
    {
        abort = _abort;
        // signal to main thread
        EmitSignal("ProgressSignal", iterationCount);
    }

    // thread event
    void AIFinished(object sender, IOrderedEnumerable<MonteCarloTreeSearch.Node<GameAIPlayer, Move>> result)
    {
        _result = result;
        EmitSignal("ProgressFinishedSignal");
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
