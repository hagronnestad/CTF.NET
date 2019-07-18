using FlagProtocol;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FlagCapture {
    class Program {
        private static byte[] _flag = null;
        private static UdpClient _uc = new UdpClient(6969);

        static async Task Main(string[] args) {
            Console.WriteLine("Capturing flag...\n");

            while (true) {
                await ReceiveAsync();

                Console.WriteLine(Encoding.ASCII.GetString(_flag));

                if (_flag.All(x => x != 0)) {
                    break;
                }
            }

            Console.WriteLine("\nThe complete flag has been captured, good job!");
            Console.ReadLine();
        }

        public static async Task ReceiveAsync() {
            var dg = await _uc.ReceiveAsync();

            var p = FlagPacket.FromBytes(dg.Buffer);
            if (p == null) return;

            if (_flag == null) {
                _flag = new byte[p.SequenceSize];
            }

            if (_flag.Length < p.SequenceSize) return;

            _flag[p.SequenceNumber] = p.FlagByte;
        }
    }
}
