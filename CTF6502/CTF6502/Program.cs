using System;

namespace CTF6502 {
    class Program {

        static byte[] ram = new byte[0x100];

        static byte[] data = {
            0x4B,0x5C,0x4E,0x44,0x6D,0x69,0x7A,0x66,
            0x73,0x60,0x38,0x65,0x3B,0x57,0x6B,0x38,
            0x65,0x78,0x7D,0x7C,0x3B,0x7A,0x57,0x7A,
            0x3B,0x7E,0x38,0x64,0x7D,0x7C,0x39,0x38,
            0x66,0x75
        };

        static byte[] program = {

        };

        static void Main(string[] args) {
            Console.WriteLine("Welcome to the CTF6502 challenge!");
            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine("I know there is a flag hidden inside the 6502 assembly somewhere,");
            Console.WriteLine("but the data written to memory looks corrupted. I guess there's a");
            Console.WriteLine("bug in the assembly program....... or could it be in my emulator?");
            Console.WriteLine("");

            Console.WriteLine("Loading data into memory location 0x0000...");
            Array.Copy(data, 0, ram, 0, data.Length);

            Console.WriteLine("Loading program into memory location 0x0040...");
            Array.Copy(program, 0, ram, 0x0040, program.Length);

            Console.WriteLine("Setting the program counter (PC) to 0x0040 and executing program...\n");

            Console.WriteLine("Dumping memory...\n");
            DumpMemory();
        }


        static void DumpMemory() {
            for (int i = 0; i < 0x100; i += 0x10) {
                Console.Write($"{i:X4}   ");

                for (int j = 0; j < 0x10; j++) {
                    if (j == 0x08) Console.Write(" ");
                    Console.Write($"{ram[i + j]:X2} ");
                }

                Console.Write("   ");

                for (int j = 0; j < 0x10; j++) {
                    var c = (char)ram[i + j];
                    if (c == 0) c = '.';
                    Console.Write($"{c}");
                }

                Console.WriteLine("");
            }
        }

    }
}
