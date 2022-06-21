using Libplanet.Store;

namespace Scripts.Actions
{
    /// <summary>
    /// <see cref="DataModel"/> for encoding <see cref="AddCount"/> action.
    /// </summary>
    public class AddCountPlainValue : DataModel
    {
        public long Count { get; private set; }

        public AddCountPlainValue(long count)
            : base()
        {
            Count = count;
        }

        public AddCountPlainValue(Bencodex.Types.Dictionary encoded)
            : base(encoded)
        {
        }
    }
}
