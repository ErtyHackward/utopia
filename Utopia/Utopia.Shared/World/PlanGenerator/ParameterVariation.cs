namespace Utopia.Shared.World.PlanGenerator
{
    public struct ParameterVariation
    {
        private int _maximum;
        private int _minimum;

        public int Minimum
        {
            get { return _minimum; }
            set { _minimum = value; }
        }

        public int Maximum
        {
            get { return _maximum; }
            set { _maximum = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minimum">Inclusive minimum</param>
        /// <param name="maximum">Inclusive maximum</param>
        public ParameterVariation(int minimum, int maximum)
        {
            _minimum = minimum;
            _maximum = maximum;
        }
        
        public bool Contains(int value)
        {
            return Minimum <= value && value <= Maximum;
        }

        public int GetPercent(int elevation)
        {
            return (int) (100 * ((double)elevation - Minimum) / (Maximum - Minimum));
        }
    }
}