using AutoPets;

public class GameSingleton
{
    static GameSingleton _instance;

    public Game Game { get; set; }

    public Player BuildPlayer { get; set; }
    
    public bool Dragging { get; set; }
    
    public object DragTarget { get; set; }
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