namespace Core.VinylSelect
{
    /**
     * Contains the possible states of the vinyl selection and inspection workflow.
     */
    public enum VinylState
    {
        BrowsingBox,
        VinylSelected,
        VinylRotating,
        DiscRotating,
        VinylInfoOpen,
        DraggingVinylOut,
        VinylDraggedOutFocused,
        DraggingVinylIn,
    }
}
