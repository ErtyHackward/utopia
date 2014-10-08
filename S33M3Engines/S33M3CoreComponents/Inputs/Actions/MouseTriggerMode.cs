namespace S33M3CoreComponents.Inputs.Actions
{
    public enum MouseTriggerMode
    {
        /// <summary>
        /// Happens each update if button is pressed
        /// </summary>
        ButtonDown,
        /// <summary>
        /// Happens once when the button is released
        /// </summary>
        ButtonReleased,
        /// <summary>
        /// Happens once on each button press pressed
        /// </summary>
        ButtonPressed,
        /// <summary>
        /// The Mouse scrollWheel has been rolled Forward
        /// </summary>
        ScrollWheelForward,
        /// <summary>
        /// The Mouse scrollWheel has been rolled Backward
        /// </summary>
        ScrollWheelBackWard
    }
}
