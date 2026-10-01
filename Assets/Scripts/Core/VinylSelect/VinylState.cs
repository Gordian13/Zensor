namespace Core.VinylSelect
{
    /**
     * @brief Defines the states shared by vinyl selection, inspection, UI and playback.
     * @see VinylSelectController
     */
    public enum VinylState
    {
        BrowsingBox,                ///< Records can be hovered and selected at the active spot.
        VinylSelected,              ///< The selected cover can rotate; its disc peeks out.
        VinylInfoOpen,              ///< Inspection metadata is open; closing restores VinylSelected.
        DraggingVinylOut,           ///< The disc is being pulled out of its cover.
        VinylDraggedOutFocused,    ///< The extracted disc can rotate or be sent to the player.
        DraggingVinylIn,            ///< The disc is being pushed back into its cover.
        vinylPlayer,               ///< Player mode without the metadata overlay.
        VinylPlayerInfoOpen        ///< Player metadata is open; closing restores vinylPlayer.
    }
}
