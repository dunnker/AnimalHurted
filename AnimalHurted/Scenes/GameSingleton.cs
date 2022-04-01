using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AnimalHurtedLib;
using AnimalHurtedLib.AI;
using MonteCarlo;

public delegate void AIProgressDelegate(object sender, int iterationCount, out bool abort);
public delegate void AIFinishedDelegate(object sender, IOrderedEnumerable<MonteCarloTreeSearch.Node<GameAIPlayer, Move>> result);

public class GameSingleton
{
    static GameSingleton _instance;

    Deck _saveBattleDeck1;
    Deck _saveBattleDeck2;

    public Game Game { get; set; }

    public Player BuildNodePlayer { get; set; }
    
    public bool Dragging { get; set; }
    
    public CardArea2D DragTarget { get; set; }
    public object DragSource { get; set; }

    public List<CardCommandQueue> FightResult { get; set; }

    public bool Sandboxing { get; set; }

    public int BattleSpeed { get; set; } = 3;

    public bool VersusAI { get; set; }

    public bool GameOverShown { get; set; }

    public static GameSingleton Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameSingleton();
            }
            return _instance;
        }
    }

    public void SaveBattleDecks()
    {
        _saveBattleDeck1 = new Deck(Game.Player1, Game.BuildDeckSlots);
        Game.Player1.BattleDeck.CloneTo(_saveBattleDeck1);
        _saveBattleDeck2 = new Deck(Game.Player2, Game.BuildDeckSlots);
        Game.Player2.BattleDeck.CloneTo(_saveBattleDeck2);
    }

    public void RestoreBattleDecks()
    {
        _saveBattleDeck1.CloneTo(Game.Player1.BattleDeck);
        _saveBattleDeck2.CloneTo(Game.Player2.BattleDeck);
    }

    public const int AIMaxIterations = 30000;

    AIProgressDelegate _aiProgressDelegate;
    AIFinishedDelegate _aiFinishedDelegate;
    bool _aiFinished;
    IOrderedEnumerable<MonteCarloTreeSearch.Node<GameAIPlayer, Move>> _aiResult;
    Thread _aiThread;

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
            var game = new Game();
            GameSingleton.Instance.Game.CloneTo(game);

            //TODO: on first couple of rounds we pick some random moves to play for the human
            // but in later rounds, these random moves may not be an improvement to match up against
            // so for now only doing this in first couple of rounds until AI improves its move selection
            if (game.Round <= 3)
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
        });
        _aiThread.Start();
    }
}