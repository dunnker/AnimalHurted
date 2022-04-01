using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using AnimalHurtedLib;
using AnimalHurtedLib.AI;
using MonteCarlo;

public delegate void AIProgressDelegate(object sender, int iterationCount, out bool abort);
public delegate void AIFinishedDelegate(object sender, IOrderedEnumerable<MonteCarloTreeSearch.Node<GameAIPlayer, Move>> result);

public class AISingleton
{
    AIProgressDelegate _aiProgressDelegate;
    AIFinishedDelegate _aiFinishedDelegate;
    bool _aiFinished;
    IOrderedEnumerable<MonteCarloTreeSearch.Node<GameAIPlayer, Move>> _aiResult;
    Thread _aiThread;

    static AISingleton _instance;

    public static AISingleton Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new AISingleton();
            }
            return _instance;
        }
    }

    public bool HardMode { get; set; }

    public const int AIMaxIterations = 30000;

    public void OnAIProgress(int iterationCount, out bool abort)
    {
        lock(aiLock)
        {
            abort = false;
            if (_aiProgressDelegate != null)
                _aiProgressDelegate(this, iterationCount, out abort);
        }
    }

    public void OnAIFinished(IOrderedEnumerable<MonteCarloTreeSearch.Node<GameAIPlayer, Move>> result)
    {
        lock(aiLock)
        {
            _aiFinished = true;
            if (_aiFinishedDelegate != null)
                _aiFinishedDelegate(this, result);
            _aiFinishedDelegate = null;
            _aiProgressDelegate = null;
        }
    }

    public void SetAIDelegates(out bool aiFinished, 
        out IOrderedEnumerable<MonteCarloTreeSearch.Node<GameAIPlayer, Move>> aiResult,
        AIProgressDelegate aiProgressDelegate, AIFinishedDelegate aiFinishedDelegate) 
    { 
        lock(aiLock)
        {
            aiFinished = _aiFinished;
            if (aiFinished)
                aiResult = _aiResult;
            else
            {
                aiResult = null;
                _aiFinishedDelegate = aiFinishedDelegate;
                _aiProgressDelegate = aiProgressDelegate;
            }
        }
    }

    private readonly object aiLock = new object();

    public void TerminateAIThread()
    {
        lock(aiLock)
        {
            if (!_aiFinished && _aiThread != null)
                _aiThread.Abort();
            _aiThread = null;
        }
    }

    public void StartAIThread()
    {
        _aiFinished = false;
        _aiResult = null;

        _aiThread = new System.Threading.Thread(() => {
            try
            {
                var game = new Game();
                GameSingleton.Instance.Game.CloneTo(game);

                //TODO: on first couple of rounds we pick some random moves to play for the human
                // but in later rounds, these random moves may not be an improvement to match up against
                // so for now only doing this in first couple of rounds until AI improves its move selection
                // with HardMode, we already see human player's deck so no need to add moves for the player1
                if (!HardMode && game.Round <= 3)
                {
                    var move = new Move(game.Player1);
                    move.ExecuteActions(game.Player1);
                }

                AnimalHurtedLib.AI.GameAIState rootState = new AnimalHurtedLib.AI.GameAIState(true, game, game.Player2);

                MonteCarloTreeSearch.Node<GameAIPlayer, Move> rootNode = new MonteCarloTreeSearch.Node<GameAIPlayer, Move>(rootState);

                rootNode.BuildTree(new Func<int, long, bool>((numIterations, elapsedMs) => { 
                    bool abort = false;
                    if (numIterations % 100 == 0)
                        OnAIProgress(numIterations, out abort);
                    return !abort && numIterations < AIMaxIterations;
                }));
                _aiResult = rootNode.Children.OrderByDescending(n => n.NumRuns);
                OnAIFinished(_aiResult);
            }
            catch //(Exception e)
            {
                // aborting thread can lead to "Exception has been thrown by the target of an invocation" errors in game library
                //if (!(e is ThreadAbortException))
                //    Debug.WriteLine(e.ToString());
            }
        });
        _aiThread.Start();
    }
}
