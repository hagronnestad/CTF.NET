using System;
using System.Collections.Generic;
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
            packet.InsertRange(sizeof(uint), BitConverter.GetBytes(packet.Count + sizeof(uint)));

            return packet.ToArray();
        }
    }
}
