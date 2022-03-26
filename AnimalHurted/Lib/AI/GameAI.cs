using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MonteCarlo;

namespace AnimalHurtedLib.AI
{
    public enum MoveActionEnum { Buy, BuyFood, Reorder, Roll }

    public class MoveAction
    {
        public virtual void Execute(Player player, List<CardCommandQueue> result)
        {

        }
    }

    public class BuyAction : MoveAction
    {
        public int ShopIndex { get; set; }
        public int TargetIndex { get; set; }

        public override void Execute(Player player, List<CardCommandQueue> result)
        {
            var shopCard = player.ShopDeck[ShopIndex];
            // AI picked a shop card that doesn't exist, so find next card in ShopDeck
            if (shopCard == null)
                shopCard = player.ShopDeck.SkipWhile((card) => card != null && card.Index <= ShopIndex).FirstOrDefault();
            if (shopCard == null)
                shopCard = player.ShopDeck.Reverse().SkipWhile((card) => card != null && card.Index >= ShopIndex).FirstOrDefault();
            if (shopCard != null)
            {
                var buildCard = player.BuildDeck[TargetIndex];
                // if a card is at target location, and we can't level it up, then sell it
                if (buildCard != null && shopCard.Ability.GetType() != buildCard.Ability.GetType() &&
                // after getting gold from selling will we have enough to buy new card
                    player.Gold + buildCard.Level >= Game.PetCost)
                {
                    var queue = new CardCommandQueue();
                    buildCard.Sell();
                    buildCard.Sold(queue, TargetIndex);
                    result.AddRange(queue.CreateExecuteResult(player.Game));
                }
                if (player.Gold >= Game.PetCost)
                {
                    var queue = new CardCommandQueue();
                    player.Game.BuyFromShop(shopCard.Index, TargetIndex, player, queue);   
                    result.AddRange(queue.CreateExecuteResult(player.Game));
                }
            }
        }
    }

    public class BuyFoodAction : MoveAction
    {
        public int FoodIndex { get; set; }
        public int TargetIndex { get; set; }

        public override void Execute(Player player, List<CardCommandQueue> result)
        {
            var food = player.GetShopFoodFromIndex(FoodIndex);
            var buildCard = player.BuildDeck[TargetIndex];
            if (buildCard != null && food != null && player.Gold >= food.Cost)
            {
                player.BuyFood(buildCard, FoodIndex);
                var queue = new CardCommandQueue();
                buildCard.Ate(queue, food);
                result.AddRange(queue.CreateExecuteResult(player.Game));
            }
        }
    }

    public class ReorderAction : MoveAction
    {
        public int FromIndex { get; set; }
        public int ToIndex { get; set; }

        public override void Execute(Player player, List<CardCommandQueue> result)
        {
            var fromCard = player.BuildDeck[FromIndex];
            if (fromCard != null)
            {
                var toCard = player.BuildDeck[ToIndex];
                if (toCard == null)
                    player.BuildDeck.MoveCard(fromCard, ToIndex);
                else if (fromCard.Ability.GetType() == toCard.Ability.GetType())
                {
                    // level up
                    var saveLevel = fromCard.Level;
                    fromCard.GainXP(toCard);
                    var queue = new CardCommandQueue();
                    fromCard.GainedXP(queue, saveLevel);
                    result.AddRange(queue.CreateExecuteResult(player.Game));
                }
                else
                {
                    // swap cards
                    player.BuildDeck.Remove(ToIndex);
                    player.BuildDeck.MoveCard(fromCard, ToIndex);
                    player.BuildDeck.SetCard(toCard, FromIndex);
                }
            }
        }
    }

    public class RollAction : MoveAction
    {
        public override void Execute(Player player, List<CardCommandQueue> result)
        {
            if (player.Gold >= Game.RollCost)
                player.Roll();
        }
    }

    /// A Move represents one permutation of actions the AI can perform in the build phase before the battle
    public class Move : MonteCarlo.IAction
    {
        List<MoveAction> _actions = new List<MoveAction>();
        List<CardCommandQueue> _result;
        static Array _enums = Enum.GetValues(typeof(MoveActionEnum));

        public void ExecuteActions(Player player)
        {
            // if _result == null
            _result = new List<CardCommandQueue>();
            foreach (var action in _actions)
                action.Execute(player, _result);
            //TODO
            // should be able to just execute commands again? will investigate later
            //else
            //    foreach (var queue in _result)
            //        foreach (var command in queue)
            //            command.Execute();
        }

        // constructor
        public Move(Player player)
        {
            AddActions(player);
        }

        void AddActions(Player player)
        {
            int gold = Game.GoldPerTurn;
            while (gold >= 1)
            {
                double[] probabilities = new double[] 
                { 
                    0.6, // 0.5 chance of Buy 
                    0.8, // 0.2 chance of BuyFood
                    0.9, // 0.1 chance of Reorder
                    1.0  // 0.1 chance of Roll
                };

                MoveActionEnum foundEnum = MoveActionEnum.Buy;
                double rand = GameAIState.Random.NextDouble();
                foreach (var @enum in _enums)
                    if ((MoveActionEnum)@enum == MoveActionEnum.Buy)
                    {
                        if (rand <= probabilities[0])
                        {
                            foundEnum = MoveActionEnum.Buy;
                            break;
                        }
                    }
                    else if (rand >= probabilities[(int)(MoveActionEnum)@enum - 1] && 
                        rand <= probabilities[(int)(MoveActionEnum)@enum])
                    {
                        foundEnum = (MoveActionEnum)@enum;
                        break;
                    }

                switch (foundEnum)
                {
                    case MoveActionEnum.Buy:
                        if (gold >= Game.PetCost)
                        {
                            gold -= Game.PetCost;
                            var buildIndex = GameAIState.Random.Next(player.BuildDeck.Size);
                            var shopIndex = GameAIState.Random.Next(player.Game.GetShopSlotCount());
                            _actions.Add(new BuyAction() { ShopIndex = shopIndex, TargetIndex = buildIndex });
                        }                        
                        break;
                    case MoveActionEnum.BuyFood:
                        if (gold >= Game.FoodCost)
                        {
                            gold -= Game.FoodCost;
                            var buildIndex = GameAIState.Random.Next(player.BuildDeck.Size);
                            var foodIndex = GameAIState.Random.Next(2);
                            _actions.Add(new BuyFoodAction() { FoodIndex = foodIndex, TargetIndex = buildIndex });
                        }
                        break;
                    case MoveActionEnum.Reorder:
                        var buildCard = player.BuildDeck.GetRandomCard();
                        if (buildCard != null)
                        {
                            var moveTo = GameAIState.Random.Next(player.BuildDeck.Size);
                            _actions.Add(new ReorderAction() { FromIndex = buildCard.Index, ToIndex = moveTo });
                        }
                        break;
                    case MoveActionEnum.Roll:
                        gold--;
                        _actions.Add(new RollAction());
                        break;
                    default:
                        throw new Exception("Invalid enum");
                }
            }
        }
    }

    // Helper class to support IPlayer interface from the MCTS library
    public class GameAIPlayer : MonteCarlo.IPlayer
    {
        Player _player;

        public GameAIPlayer(Player player)
        {
            _player = player;
        }

        public Player Player { get { return _player; } }
    }

    // not used currently, since UI wants events for AI progress
    // may remove later
    public static class GameAI
    {
        public static void ExecuteBestMove(GameAIState gameAIState, Player player)
        {
            var moves = MonteCarloTreeSearch.GetTopActions<GameAIPlayer, Move>(gameAIState, 50000);
            var move = moves.FirstOrDefault();
            move?.Action.ExecuteActions(player);
        }
    }

    // state class is attached to each node in the tree
    public class GameAIState : MonteCarlo.IState<GameAIPlayer, Move>
    {
        Game _game;
        GameAIPlayer _player1;
        GameAIPlayer _player2;
        GameAIPlayer _currentPlayer;
        GameAIPlayer _opponentPlayer;

        public static Random Random = new Random();

        void NewCurrentPlayer()
        {
            if (_currentPlayer == _player1)
            {
                _currentPlayer = _player2;
                _opponentPlayer = _player1;
            }
            else
            {
                _currentPlayer = _player1;
                _opponentPlayer = _player2;
            }
        }

        public GameAIState(Game game, Player currentPlayer)
        {
            _game = game;
            _player1 = new GameAIPlayer(_game.Player1);
            _player2 = new GameAIPlayer(_game.Player2);
            if (currentPlayer == _player1.Player)
            {
                _currentPlayer = _player1;
                _opponentPlayer = _player2;
            }
            else
            {
                _currentPlayer = _player2;
                _opponentPlayer = _player1;
            }
        }

        // MonteCarlo.IState interfaces
        public IList<Move> Actions
        { 
            get
            {
                var result = new List<Move>();
                // pick an arbitrary number of permuations of random actions to perform during build
                for (int i = 1; i <= 50; i++)
                {
                    var move = new Move(_currentPlayer.Player);
                    result.Add(move);
                }
                return result;
            }
        }

        public void Rollout()
        {
            do
            {
                _currentPlayer.Player.NewBattleDeck();
                _opponentPlayer.Player.NewBattleDeck();
                _game.CreateFightResult();
                if (_game.IsGameOver())
                    break;
                else 
                {
                    _game.NewRound();
                    var move1 = new Move(_currentPlayer.Player);
                    move1.ExecuteActions(_currentPlayer.Player);
                    var move2 = new Move(_opponentPlayer.Player);
                    move2.ExecuteActions(_opponentPlayer.Player);
                }
            }
            while (true);
        }

        public void ApplyAction(Move action)
        { 
            action.ExecuteActions(_currentPlayer.Player);
            NewCurrentPlayer();
        }

        public GameAIPlayer CurrentPlayer { get { return _currentPlayer; } }

        public double GetResult(IState<GameAIPlayer, Move> state, GameAIPlayer player)
        { 
            var currentPlayer = ((GameAIState)state)._player2.Player;
            Player opponentPlayer;
            opponentPlayer = currentPlayer.GetOpponentPlayer();
            if (currentPlayer.Lives > 0 && opponentPlayer.Lives == 0)
                return 1.0; 
            else if (opponentPlayer.Lives > 0 && currentPlayer.Lives == 0)
                return 0;
            else
                return 0.5;
        }

        public IState<GameAIPlayer, Move> Clone()
        { 
            Game game = new Game();
            _game.CloneTo(game);
            return new GameAIState(game, CurrentPlayer.Player); 
        }
    }
}
