
namespace S33M3Resources.Structs
{
    public class FTSValue<T>
    {
        public T Value;
        public T ValuePrev;
        public T ValueInterp;

        public FTSValue()
        {
        }

        public FTSValue(T value)
        {
            Initialize(value);
        }

        public void BackUpValue()
        {
            ValuePrev = Value;
        }

        public void Initialize()
        {
            ValuePrev = Value;
        }

        public void Initialize(T value)
        {
            ValuePrev = Value = ValueInterp = value;
        }
    }
}
