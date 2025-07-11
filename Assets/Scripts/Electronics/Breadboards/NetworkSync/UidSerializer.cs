using Mirror;

namespace Reconnect.Electronics.Breadboards.NetworkSync
{
    public static class UidSerializer
    {
        public static void WriteUid(this NetworkWriter writer, Uid uid)
        {
            writer.WriteUInt(uid.Value);
        }

        public static Uid ReadUid(this NetworkReader reader)
        {
            return new Uid(reader.ReadUInt());
        }
    }
}