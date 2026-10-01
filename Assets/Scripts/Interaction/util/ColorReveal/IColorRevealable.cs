namespace Interaction.util.ColorReveal
{
    /**
     * Interface for objects that can be revealed in color when hovered over or selected.
     * 
     * Implement this interface on any object that should change its color state based on user interaction.
     */
    public interface IColorRevealable
    {
        /**
         * Toggles the Color of the Object that has this Interface
         */
        void SetColorReveal(bool revealed);

        /**
         * Sets whether the object should be shown in color.
         */
        void SetColor(bool showColor);

        /**
         * Locks or unlocks the object in its colored state.
         */
        void SetStayColored(bool stayColored);
    }
}
