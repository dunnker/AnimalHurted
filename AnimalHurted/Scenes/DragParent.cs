public interface IDragParent
{
    void DragDropped();

    bool GetCanDrag();

    void DragReorder(CardArea2D cardArea2D);
}