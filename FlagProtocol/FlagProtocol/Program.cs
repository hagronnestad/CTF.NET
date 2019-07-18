using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace FlagProtocol {
    class Program {

        private static byte[] _indexes = new byte[] { 0x14, 0x0, 0x18, 0x13, 0x5, 0xb, 0x9, 0x8, 0x10, 0x4, 0xd, 0x15, 0x2, 0x3, 0x17, 0xf, 0xa, 0x11, 0x6, 0x1, 0xe, 0x7, 0x12, 0x16, 0xc };
        private static byte[] _pieces = new byte[] { 0x64, 0x43, 0x7d, 0x33, 0x61, 0x75, 0x79, 0x7b, 0x74, 0x65, 0x63, 0x5f, 0x46, 0x6c, 0x33, 0x70, 0x30, 0x75, 0x72, 0x54, 0x34, 0x6e, 0x72, 0x6d, 0x5f };

        static bool _TheFlagCanBeCapturedWithoutReversing() {
            return true;
        }

        [TheFlagIs("CTFlearn{aHR0cHM6Ly93d3cueW91dHViZS5jb20vd2F0Y2g/dj1kUXc0dzlXZ1hjUQ==}")]
        static void Main(string[] args) {
            if (!_TheFlagCanBeCapturedWithoutReversing()) return;

            Console.WriteLine("Transmitting flag...");

            var ipe = new IPEndPoint(IPAddress.Parse("224.0.0.252"), 6969);
            var c = new UdpClient();

            while (true) {

                for (int i = 0; i < _indexes.Length; i++) {
                    var p = new FlagPacket() {
                        SequenceSize = (byte)_indexes.Length,
                        SequenceNumber = _indexes[i],
                        FlagByte = _pieces[i]
                    };

                    var b = p.ToBytes();

                    c.Send(b, b.Length, ipe);

                    Thread.Sleep(500);
                }

            }
        }
    }

    internal class TheFlagIsAttribute : Attribute {
        public TheFlagIsAttribute(string s) {

        }
    }
}
