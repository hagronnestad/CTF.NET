using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlagProtocol {
    public class FlagPacket {
        public const string MagicNumbers = "flag";
        public byte SequenceSize { get; set; }
        public byte SequenceNumber { get; set; }
        public byte FlagByte { get; set; }

        public byte[] ToBytes() {
            var packet = new List<byte>();

            packet.AddRange(Encoding.ASCII.GetBytes(MagicNumbers));
            packet.Add(SequenceSize);
            packet.Add(SequenceNumber);
            packet.Add(FlagByte);

            // Insert packet size after the magic numbers
            packet.Insert(MagicNumbers.Length, (byte) packet.Count);

            return packet.ToArray();
        }

        public static FlagPacket FromBytes(byte[] bytes) {
            if (bytes.Length < 7) return null;

            var magicNumbers = Encoding.ASCII.GetString(bytes.Take(4).ToArray());
            if (magicNumbers != "flag") return null;

            var p = new FlagPacket() {
                SequenceSize = bytes[5],
                SequenceNumber = bytes[6],
                FlagByte = bytes[7]
            };

            return p;
        }
    }
}
