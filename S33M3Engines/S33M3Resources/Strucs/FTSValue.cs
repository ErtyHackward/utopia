
namespace S33M3Resources.Structs
{
    public class FTSValue<T>
    {
        public T Value;
        public T ValuePrev;
        public T ValueInterp;

        public void BackUpValue()
        {
            ValuePrev = Value;
        }

        public void Initialize()
        {
            ValuePrev = Value;
        }
    }
}
