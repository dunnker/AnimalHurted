public interface IDragParent
{
    void DragDropped(CardArea2D cardArea2D);

    bool GetCanDrag();
}