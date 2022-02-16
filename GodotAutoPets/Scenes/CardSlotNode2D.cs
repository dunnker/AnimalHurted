using Godot;
using System;

public class CardSlotNode2D : Node2D
{
    public CardArea2D CardArea2D { get { return GetNode<CardArea2D>("CardArea2D"); } }
}
