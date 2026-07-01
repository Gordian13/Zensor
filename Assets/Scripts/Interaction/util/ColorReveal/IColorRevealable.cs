namespace Interaction.util.ColorReveal
{
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
