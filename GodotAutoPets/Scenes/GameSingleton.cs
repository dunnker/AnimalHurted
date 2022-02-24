using System;
using System.Threading;
using AutoPets;

public class GameSingleton
{
    static GameSingleton _instance;

    public Game Game { get; set; }

    public static readonly AutoResetEvent autoResetEvent = new AutoResetEvent(false);

    public Player BuildNodePlayer { get; set; }
    
    public bool Dragging { get; set; }
    
    public CardArea2D DragTarget { get; set; }
    public object DragSource { get; set; }

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
}