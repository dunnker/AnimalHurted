using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Rendering;
using System.Diagnostics;
using AutoPets;

namespace ConsoleAutoPets
{
    public class ConsoleGame
    {
        Game _game;
        string _lastMessage;
        string _player1Message;
        string _player2Message;
        int _battleSequence;

        public ConsoleGame()
        {

        }

        void Game_AbilityEvent(object sender, Ability ability, Card card, int index, string message)
        {
            if (card.Deck == _game.Player1.BattleDeck || card.Deck == _game.Player2.BattleDeck)
            {
                RenderBattleTables();
                AnsiConsole.WriteLine(string.Format("{0}. {1}", index + 1, ability.ToString()));
                AnsiConsole.WriteLine(message);
                AnsiConsole.Ask<Char?>(">");
            }
        }

        void BattleDeck_DeckEvent(object sender, Card card, string message)
        {
            _battleSequence++;
            if (card.Deck.Player == _game.Player1)
            {
                if (!string.IsNullOrEmpty(_player1Message))
                    _player1Message += Environment.NewLine;
                _player1Message += String.Format("({0}) {1}", _battleSequence, message);
            }
            else if (card.Deck.Player == _game.Player2)
            {
                if (!string.IsNullOrEmpty(_player2Message))
                    _player2Message += Environment.NewLine;
                _player2Message += String.Format("({0}) {1}", _battleSequence, message);
            }
            else
                Debug.Assert(false);
        }

        void BuildDeck_DeckEvent(object sender, Card card, string message)
        {
            if (!string.IsNullOrEmpty(_lastMessage))
                _lastMessage += Environment.NewLine;
            _lastMessage += message;
        }

        public void MainMenu()
        {
            do
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(new Panel(@"Welcome to [underline green]Super Auto Pets[/] Awesome Edition, version 0.02 super alpha

1. New Game (Human vs Human)
Q. Quit"));

                Char command = Char.ToUpper(AnsiConsole.Ask<Char>(">"));
                switch (command)
                {
                    case '1':
                        NewGame();
                        break;
                    case 'Q':
                        return;
                    default:
                        break;
                }
            } while (true);
        }

        Table CreateCardTable(Card card, bool includeLevel)
        {
            var cardTable = new Table();
            cardTable.Border(TableBorder.None);
            string abilityMarkup;
            if (includeLevel)
                abilityMarkup = string.Format("{0}  Lvl{1}{2}", card.Ability, card.Level, new string('+', card.XPRemainder));
            else
                abilityMarkup = card.Ability.ToString();
            if (card.State != CardState.Idle)
                abilityMarkup = string.Format("[underline green]{0}[/]", abilityMarkup);
            cardTable.AddColumn(abilityMarkup);
            var petTable = new Table();
            petTable.AddColumn(card.AttackPoints.ToString());
            petTable.AddColumn(string.Format("[underline red]{0}[/]", card.HitPoints.ToString()));
            petTable.Border(TableBorder.None);
            cardTable.AddRow(petTable);
            return cardTable;
        }

        string MarkupWarning(string message)
        {
            return string.Format("[underline red]{0}[/]", message);
        }

        void RenderBuildTables(Player player)
        {
            AnsiConsole.Clear();

            var table = new Table();
            table.AddColumn(string.Format("{0}. Prepare to Fight!", player.Name));

            var statsTable = new Table();
            statsTable.AddColumn("Gold");
            statsTable.AddColumn("Lives");
            statsTable.AddColumn("Wins");
            statsTable.AddColumn("Round");
            statsTable.AddRow(string.Format("[underline yellow]{0}[/]", player.Gold), player.Lives.ToString(), player.Wins.ToString(), _game.Round.ToString());

            var deckTable = new Table();
            deckTable.Title("Deck");
            var buildRenderable = new List<IRenderable>();
            for (int i = 0; i < player.BuildDeck.Size; i++)
            {
                deckTable.AddColumn((i + 1).ToString());
                if (player.BuildDeck[i] != null)
                    buildRenderable.Add(CreateCardTable(player.BuildDeck[i], true));
                else
                    buildRenderable.Add(new Text(string.Empty));
            }
            deckTable.AddRow(buildRenderable);

            table.AddRow(statsTable);
            table.AddRow(deckTable);

            var shopTable = new Table();
            shopTable.Title("Shop");
            shopTable.AddColumn("Pets");
            shopTable.AddColumn("Food");

            var shopPetsTable = new Table();
            var shopRenderable = new List<IRenderable>();
            for (int i = 0; i < player.ShopDeck.Size; i++)
            {
                shopPetsTable.AddColumn((i + 1).ToString());
                if (player.ShopDeck[i] != null)
                    shopRenderable.Add(CreateCardTable(player.ShopDeck[i], false));
                else
                    shopRenderable.Add(new Text(string.Empty));
            }
            shopPetsTable.AddRow(shopRenderable);

            var foodTable = new Table();
            for (int i = 1; i <= Game.ShopFoodSlots; i++)
            {
                foodTable.AddColumn(i.ToString());
            }
            foodTable.AddRow(string.Empty, string.Empty);

            shopTable.AddRow(shopPetsTable, foodTable);

            table.AddRow(shopTable);

            AnsiConsole.Write(table);
        }

        void BuildPlayer(Player player, out bool quit)
        {
            _lastMessage = string.Empty;
            quit = false;
            do
            {
                RenderBuildTables(player);

                if (!string.IsNullOrEmpty(_lastMessage))
                    AnsiConsole.MarkupLine(_lastMessage);

                _lastMessage = string.Empty;

                foreach (var card in player.BuildDeck)
                    card.ResetState();

                AnsiConsole.MarkupLine("[underline yellow]R[/]oll [underline yellow]B[/]uy [underline yellow]S[/]ell [underline yellow]M[/]ove [underline yellow]C[/]ontinue [underline yellow]Q[/]uit");
                Char command = Char.ToUpper(AnsiConsole.Ask<Char>(">"));
                switch (command)
                {
                    case 'C':
                        if (player.Gold <= 0 || Char.ToUpper(AnsiConsole.Ask<Char>("You have gold left. Are you sure? (Y/n)", 'n')) == 'Y')
                            return;
                        break;
                    case 'R':
                        if (player.Gold >= Game.RollCost)
                            _game.Roll(player);
                        else
                            _lastMessage = MarkupWarning("Not enough Gold");
                        break;
                    case 'B':
                        if (player.Gold >= Game.PetCost)
                        {
                            AnsiConsole.WriteLine("Enter shop pet number");
                            switch (AnsiConsole.Ask<int>(">"))
                            {
                                case int shopNumber when shopNumber >= 1 && shopNumber <= player.ShopDeck.Size && player.ShopDeck[shopNumber - 1] != null:
                                    AnsiConsole.WriteLine("Enter deck number where pet should be placed");
                                    switch (AnsiConsole.Ask<int>(">"))
                                    {
                                        case int deckNumber when deckNumber >= 1 && deckNumber <= player.BuildDeck.Size:
                                            if (player.BuildDeck[deckNumber - 1] != null)
                                                player.BuildDeck.MakeRoomAt(deckNumber - 1);
                                            if (player.BuildDeck[deckNumber - 1] != null)
                                            {
                                                if (player.BuildDeck[deckNumber - 1].Ability == player.ShopDeck[shopNumber - 1].Ability)
                                                    player.BuildDeck[deckNumber - 1].GainXP(player.ShopDeck[shopNumber - 1]);
                                                else
                                                    _lastMessage = MarkupWarning("No room for pet in the deck");
                                            }
                                            else
                                                _game.BuyFromShop(shopNumber - 1, deckNumber - 1, player);
                                            break;
                                        default:
                                            _lastMessage = MarkupWarning("Invalid selection from Shop");
                                            break;
                                    }
                                    break;
                                default:
                                    _lastMessage = MarkupWarning("Invalid selection from Shop");
                                    break;
                            }
                        }
                        else
                            _lastMessage = MarkupWarning("Not enough Gold");
                        break;
                    case 'S':
                        AnsiConsole.WriteLine("Enter pet number to sell");
                        switch (AnsiConsole.Ask<int>(">"))
                        {
                            case int i when i >= 1 && i <= player.BuildDeck.Size && player.BuildDeck[i - 1] != null:
                                player.BuildDeck[i - 1].Sell();
                                break;
                            default:
                                _lastMessage = MarkupWarning("Invalid selection from Deck");
                                break;
                        }
                        break;
                    case 'G':
                        AnsiConsole.WriteLine("Enter pet number");
                        switch (AnsiConsole.Ask<int>(">"))
                        {
                            case int abilityIndex when abilityIndex >= 0 && abilityIndex <= AbilityList.Instance.AllAbilities.Count - 1:
                                AnsiConsole.WriteLine("Enter deck number where pet should be placed");
                                switch (AnsiConsole.Ask<int>(">"))
                                {
                                    case int deckNumber when deckNumber >= 1 && deckNumber <= player.BuildDeck.Size:
                                        var card = new Card(player.BuildDeck, AbilityList.Instance.AllAbilities[abilityIndex]);
                                        card.Buy(deckNumber - 1);
                                        break;
                                    default:
                                        _lastMessage = MarkupWarning("Invalid selection");
                                        break;
                                }
                                break;
                            default:
                                _lastMessage = MarkupWarning("Invalid selection");
                                break;
                        }
                        break;
                    case 'M':
                        if (player.BuildDeck.GetCardCount() > 0)
                        {
                            AnsiConsole.WriteLine("Enter deck pet number to move");
                            switch (AnsiConsole.Ask<int>(">"))
                            {
                                case int fromCard when fromCard >= 1 && fromCard <= player.BuildDeck.Size && player.BuildDeck[fromCard - 1] != null:
                                    AnsiConsole.WriteLine("Move to");
                                    switch (AnsiConsole.Ask<int>(">"))
                                    {
                                        case int toCard when toCard >= 1 && toCard <= player.BuildDeck.Size:
                                            if (player.BuildDeck[toCard - 1] != null)
                                            {
                                                if ((player.BuildDeck[toCard - 1].Ability == player.BuildDeck[fromCard - 1].Ability) &&
                                                    AnsiConsole.Confirm("Combine these two cards?"))
                                                    player.BuildDeck[toCard - 1].GainXP(player.BuildDeck[fromCard - 1]);
                                                else
                                                {
                                                    // in case deck is full, remove the card we're moving first
                                                    var moveCard = player.BuildDeck[fromCard - 1];
                                                    player.BuildDeck.Remove(fromCard - 1);
                                                    player.BuildDeck.MakeRoomAt(toCard - 1);
                                                    player.BuildDeck.SetCard(moveCard, toCard - 1);
                                                }
                                            }
                                            else
                                                player.BuildDeck.MoveCard(player.BuildDeck[fromCard - 1], toCard - 1);
                                            break;
                                        default:
                                            _lastMessage = MarkupWarning("Invalid selection from deck");
                                            break;
                                    }
                                    break;
                                default:
                                    _lastMessage = MarkupWarning("Invalid selection from deck");
                                    break;
                            }
                        }
                        else
                            _lastMessage = MarkupWarning("No pets to move in deck");
                        break;
                    case 'Q':
                        if (Char.ToUpper(AnsiConsole.Ask<Char>("Are you sure? (Y/n)", 'n')) == 'Y')
                        {
                            quit = true;
                            return;
                        }
                        break;
                    default:
                        _lastMessage = MarkupWarning("Invalid command");
                        break;
                }
            } while (true);
        }

        void RenderBattleTables()
        {
            AnsiConsole.Clear();
            _battleSequence = 0;

            var table = new Table();
            table.AddColumn(_game.Player1.Name);
            table.AddColumn(_game.Player2.Name);

            IRenderable player1Table;
            if (_game.Player1.BattleDeck.GetCardCount() == 0)
            {
                player1Table = new Text(string.Empty);
            }
            else
            {
                player1Table = new Table();
                var player1Renderable = new List<IRenderable>();
                for (int i = 0; i <= _game.Player1.BattleDeck.Size - 1; i++)
                {
                    (player1Table as Table).AddColumn((i + 1).ToString());
                    if (_game.Player1.BattleDeck[i] != null)
                        player1Renderable.Add(CreateCardTable(_game.Player1.BattleDeck[i], true));
                    else
                        player1Renderable.Add(new Text(string.Empty));
                    if (_game.Player1.BattleDeck.GetNextCard(i) == null)
                        break;
                }
                (player1Table as Table).AddRow(player1Renderable);
            }

            IRenderable player2Table;
            if (_game.Player2.BattleDeck.GetCardCount() == 0)
            {
                player2Table = new Text(string.Empty);
            }
            else
            {
                player2Table = new Table();
                var player2Renderable = new List<IRenderable>();
                var lastCard = _game.Player2.BattleDeck.GetLastCard();
                for (int i = lastCard.Index; i >= 0; i--)
                {
                    (player2Table as Table).AddColumn((i + 1).ToString());
                    if (_game.Player2.BattleDeck[i] != null)
                        player2Renderable.Add(CreateCardTable(_game.Player2.BattleDeck[i], true));
                    else
                        player2Renderable.Add(new Text(string.Empty));
                }
                (player2Table as Table).AddRow(player2Renderable);
            }

            table.AddRow(player1Table, player2Table);
            table.AddRow(_player1Message, _player2Message);
            AnsiConsole.Write(table);
            _player1Message = string.Empty;
            _player2Message = string.Empty;
        }

        void Battle(out bool quit, out bool replay)
        {
            quit = false;
            replay = false;

            _player1Message = string.Empty;
            _player2Message = string.Empty;

            _game.NewBattle();
            _game.Player1.BattleDeck.DeckEvent += BattleDeck_DeckEvent;
            _game.Player2.BattleDeck.DeckEvent += BattleDeck_DeckEvent;

            _lastMessage = string.Empty;
            do
            {
                RenderBattleTables();

                if (!string.IsNullOrEmpty(_lastMessage))
                    AnsiConsole.MarkupLine(_lastMessage);
                _lastMessage = string.Empty;

                AnsiConsole.MarkupLine("[underline yellow]C[/]ontinue [underline yellow]Q[/]uit");
                Char command = Char.ToUpper(AnsiConsole.Ask<Char>(">"));
                switch (command)
                {
                    case 'C':
                        if (_game.IsFightOver())
                        {
                            _game.FightOver();
                            return;
                        }
                        else
                            _game.FightOne();

                        break;
                    case 'R':
                        _game.FightCanceled();
                        replay = true;
                        return;
                    case 'Q':
                        if (Char.ToUpper(AnsiConsole.Ask<Char>("Game will exit. Are you sure? (Y/n)", 'n')) == 'Y')
                        {
                            quit = true;
                            return;
                        }
                        break;
                    default:
                        _lastMessage = MarkupWarning("Invalid command");
                        break;
                }
            } while (true);
        }

        void NewGame()
        {
            bool quit;
            _game = new Game();
            _game.AbilityEvent += Game_AbilityEvent;
            _game.Player1.BuildDeck.DeckEvent += BuildDeck_DeckEvent;
            _game.Player2.BuildDeck.DeckEvent += BuildDeck_DeckEvent;
            do
            {
                BuildPlayer(_game.Player1, out quit);
                if (!quit)
                {
                    BuildPlayer(_game.Player2, out quit);
                    if (!quit)
                    {
                        bool replay;
                        do
                        {
                            Battle(out quit, out replay);
                        } while (replay);
                    }
                }
            } while (!quit);
        }
    }

    static class Program
    {
        static void Main(string[] args)
        {
            var consoleGame = new ConsoleGame();
            consoleGame.MainMenu();
        }
    }
}
