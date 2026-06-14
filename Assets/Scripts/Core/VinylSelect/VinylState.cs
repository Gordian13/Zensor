namespace Core.VinylSelect
{
    /**
     * Contains the possible states of the vinyl selection and inspection workflow.
     */
    public enum VinylState
    {
        BrowsingBox,
        VinylSelected,
        VinylInfoOpen,
        DraggingVinylOut,
        VinylDraggedOutFocused,
        DraggingVinylIn,
    }
}
