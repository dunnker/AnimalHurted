using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using AnimalHurtedLib;

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
}