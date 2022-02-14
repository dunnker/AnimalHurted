using Godot;

public class Build : Node
{
    public void _on_QuitGameButton_pressed()
    {
        GetTree().ChangeScene("res://Scenes/Main.tscn");
    }

    public void _on_RollButton_pressed()
    {
        GameSingleton.Instance.Game.Roll(GameSingleton.Instance.Game.Player1);
        var shop = GetNode<Shop>("ShopNode2D");
        shop.RenderShop();
    }

    public override void _Ready()
    {
    }

    public override void _Process(float delta)
    {
    }
}
