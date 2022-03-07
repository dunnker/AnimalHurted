using Godot;
using System;
using AnimalHurtedLib;

public class SandboxNode : Node
{
    public DeckNode2D DeckNode2D { get { return GetNode<DeckNode2D>("DeckNode2D"); } }
    public PanelContainer CardContainer { get { return GetNode<PanelContainer>("CardContainer"); } }
    public ScrollContainer CardScrollContainer { get { return GetNode<ScrollContainer>("CardContainer/CardScrollContainer"); } }
    public LineEdit FindEdit { get { return GetNode<LineEdit>("CardContainer/FindEdit"); } }
    public VBoxContainer CardVBoxContainer { get { return GetNode<VBoxContainer>("CardContainer/CardScrollContainer/CardVBoxContainer"); } }
    public HBoxContainer FoodAbilityHBoxContainer { get { return GetNode<HBoxContainer>("FoodAbilityHBoxContainer"); } }
    public Node2D CardAttrsNode2D { get { return GetNode<Node2D>("CardAttrsNode2D"); } }
    public LineEdit HealthEdit { get { return GetNode<LineEdit>("CardAttrsNode2D/HealthEdit"); } }
    public LineEdit AttackEdit { get { return GetNode<LineEdit>("CardAttrsNode2D/AttackEdit"); } }
    public LineEdit LevelEdit { get { return GetNode<LineEdit>("CardAttrsNode2D/LevelEdit"); } }

    [Signal]
    public delegate void CardSelectionChangedSignal();

    public override void _Ready()
    {
        Connect("CardSelectionChangedSignal", this, "_signal_CardSelectionChanged", null, (int)ConnectFlags.Deferred);
        RenderAbilities();
        RenderFoodAbilities();
        DeckNode2D.RenderDeck(GameSingleton.Instance.Game.Player1.BuildDeck);
        DeckNode2D.CanDragDropLevelUp = false;
    }

    public void _signal_CardSelectionChanged(int cardIndex)
    {
        if (DeckNode2D.GetCardSlotNode2D(cardIndex + 1).Selected && DeckNode2D.Deck[cardIndex] != null)
        {
            var card = DeckNode2D.Deck[cardIndex];
            HealthEdit.Text = card.HitPoints.ToString();
            AttackEdit.Text = card.AttackPoints.ToString();
            LevelEdit.Text = card.Level.ToString();
        }
        UpdateEdits();
    }

    public void UpdateEdits()
    {
        if (DeckNode2D.GetSelectedCardSlotNode2D() == null)
        {
            HealthEdit.Text = string.Empty;
            AttackEdit.Text = string.Empty;
            LevelEdit.Text = string.Empty;
            CardAttrsNode2D.Hide();
        }
        else
            CardAttrsNode2D.Show();
    }

    void EditTextChanged(string newText, Action<Card, int> action)
    {
        var cardSlot = DeckNode2D.GetSelectedCardSlotNode2D();
        if (cardSlot != null)
        {
            var card = DeckNode2D.Deck[cardSlot.CardArea2D.CardIndex];
            if (card != null)
            {
                Int32.TryParse(newText, out int value);
                action(card, value);
                cardSlot.CardArea2D.RenderCard(card, card.Index);
            }
        }
    }

    public void _on_HealthEdit_text_changed(string newText)
    {
        EditTextChanged(newText, (card, hitPoints) => { card.HitPoints = hitPoints; });
    }

    public void _on_AttackEdit_text_changed(string newText)
    {
        EditTextChanged(newText, (card, attackPoints) => { card.AttackPoints = attackPoints; });
    }

    public void _on_LevelEdit_text_changed(string newText)
    {
        EditTextChanged(newText, (card, level) => { card.XP = Card.GetXPFromLevel(level); });
    }

    public void RenderAbilities()
    {
        foreach (var child in CardVBoxContainer.GetChildren())
		    (child as Node).QueueFree();
        foreach (var type in AbilityList.Instance.AllAbilities)
        {
            if (string.IsNullOrEmpty(FindEdit.Text) || type.Name.ToUpper().IndexOf(FindEdit.Text.ToUpper()) == 0)
            {
                var res = GD.Load($"res://Assets/Pets/{type.Name}.png");
                var node = GD.Load<PackedScene>("res://Scenes/SandboxItemNode.tscn").Instance<SandboxItemNode>();
                node.TypeName = type.Name;
                node.TextureRect.Texture = res as Godot.Texture;
                CardVBoxContainer.AddChild(node);
            }
        }
    }

    public void RenderFoodAbilities()
    {
        foreach (var type in FoodAbilityList.Instance.AllAbilities)
        {
            var res = GD.Load($"res://Assets/FoodAbilities/{type.Name}.png");
            var node = GD.Load<PackedScene>("res://Scenes/SandboxItemNode.tscn").Instance<SandboxItemNode>();
            node.TypeName = type.Name;
            node.TextureRect.Texture = res as Godot.Texture;
            FoodAbilityHBoxContainer.AddChild(node);
        }
    }

    public void _on_Player1Button_toggled(bool buttonPressed)
    {
        if (buttonPressed)
            DeckNode2D.RenderDeck(GameSingleton.Instance.Game.Player1.BuildDeck);
        CardAttrsNode2D.Hide();
    }

    public void _on_Player2Button_toggled(bool buttonPressed)
    {
        if (buttonPressed)
            DeckNode2D.RenderDeck(GameSingleton.Instance.Game.Player2.BuildDeck);
        CardAttrsNode2D.Hide();
    }

    public void _on_BackButton_pressed()
    {
        GameSingleton.Instance.Sandboxing = false;

        GetTree().ChangeScene("res://Scenes/MainNode.tscn");
    }

    public void _on_BattleButton_pressed()
    {
        GameSingleton.Instance.Sandboxing = true;

        GameSingleton.Instance.Game.Player1.NewBattleDeck();
        GameSingleton.Instance.Game.Player2.NewBattleDeck();

        GameSingleton.Instance.SaveBattleDecks();
        GameSingleton.Instance.FightResult = GameSingleton.Instance.Game.CreateFightResult();
        // restore for rendering in next scene
        GameSingleton.Instance.RestoreBattleDecks();

        GetTree().ChangeScene("res://Scenes/BattleNode.tscn");
    }

    public void _on_FindEdit_text_changed(string newText)
    {
        RenderAbilities();
    }

    public void _on_RemoveButton_pressed()
    {
        for (int i = 1; i <= Game.BuildDeckSlots; i++)
        {
            var cardSlot = DeckNode2D.GetCardSlotNode2D(i);
            if (cardSlot.Selected && DeckNode2D.Deck[i - 1] != null)
            {
                DeckNode2D.Deck.Remove(i - 1);
                cardSlot.CardArea2D.RenderCard(null, i - 1);
                cardSlot.Selected = false;
            }
        }
    }
}
