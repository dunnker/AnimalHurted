public interface IDragParent
{
    void DragDropped(CardArea2D card);

    bool GetCanDrag();
}