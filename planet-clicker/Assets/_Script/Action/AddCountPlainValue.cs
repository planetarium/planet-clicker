using Libplanet.Store;

namespace _Script.Action
{
    /// <summary>
    /// <see cref="DataModel"/> for encoding <see cref="AddCount"/> action.
    /// </summary>
    public class AddCountPlainValue : DataModel
    {
        public long Count { get; private set; }

        public AddCountPlainValue(long count)
        {
            Count = count;
        }

        public AddCountPlainValue(Bencodex.Types.Dictionary encoded)
            : base(encoded)
        {
        }
    }
}
