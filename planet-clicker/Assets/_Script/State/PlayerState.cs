using Libplanet;

namespace _Script.State
{
    public class PlayerState
    {
        public Address Address;
        public long Count;

        public PlayerState(Address address, long count)
        {
            Address = address;
            Count = count;
        }
    }
}