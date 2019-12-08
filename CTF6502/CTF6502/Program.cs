/// ****************************************************************************
/// Created by Hein Andre Grønnestad (@heinandre) for CTFlearn Holiday Hack 2019
/// ****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace CTF6502 {
    class Program {

        static readonly byte[] ram = new byte[0xB0];

        static readonly byte[] flag = {
            0x4B,0x5C,0x4E,0x64,0x6D,0x69,0x7A,0x66,
            0x73,0x60,0x38,0x65,0x3B,0x57,0x6B,0x38,
            0x65,0x78,0x7D,0x7C,0x3B,0x7A,0x57,0x7A,
            0x3B,0x7E,0x38,0x64,0x7D,0x7C,0x39,0x38,
            0x66,0x75
        };

        static readonly byte[] program = {
            0xa2, 0x00,                 // LDX #$00
            0xb5, 0x00,                 // LDA $00,X
            0x00, 0x00,
            0x95, 0x40,                 // STA $40,X
            0xe8,                       // INX
            0xe0, (byte)flag.Length,    // CPX #${flag.Length}
            0xd0, 0xf5,                 // BNE $0604
            0xa9, 0xff                  // LDA #$ff
        };

        public static byte OPCODE_0x49(byte value, byte accumulator) {
            return 0xFF;
        }

        static void Main(string[] args) {
            Console.WriteLine("***********************************************************************");
            Console.WriteLine("* Welcome to the CTF 6502 challenge!                                  *");
            Console.WriteLine("*                                                                     *");
            Console.WriteLine("* This 6502 assembly program should process the flag data and make it *");
            Console.WriteLine("* readable, but the data in the memory dump looks corrupted.          *");
            Console.WriteLine("* I guess there's a bug in the assembly program, or maybe in the      *");
            Console.WriteLine("* emulator? This `OPCODE_0x49`-method looks strange...                *");
            Console.WriteLine("***********************************************************************\n");

            Console.WriteLine("- Loading flag data into memory location 0x0000 ...");
            Array.Copy(flag, 0, ram, 0, flag.Length);

            Console.WriteLine("- Loading program into memory location 0x0080 ...");
            Array.Copy(program, 0, ram, 0x0080, program.Length);

            Console.WriteLine("- Setting the program counter (PC) to 0x0080 and executing program ...\n");

            var cpu = new MOS6502(ram) {
                PC = 0x80
            };

            while (cpu.PC != (0x80 + program.Length)) {
                cpu.Step();
            }

            Console.WriteLine("- Dumping memory...\n");
            DumpMemory();
        }

        // No code below this comment needs to be modified to solve the challenge.

        static void DumpMemory() {
            for (int i = 0; i < 0xB0; i += 0x10) {
                Console.Write($"0x{i:X4}  ");

                for (int j = 0; j < 0x10; j++) {
                    if (j == 0x08) Console.Write(" ");
                    Console.Write($"{ram[i + j]:X2} ");
                }

                Console.Write(" ");

                for (int j = 0; j < 0x10; j++) {
                    var c = (char)ram[i + j];
                    if (c == 0) c = '.';
                    Console.Write($"{c}");
                }

                Console.WriteLine("");
            }
        }
    }


    public class MOS6502 {
        public byte[] Memory { get; private set; }

        public Dictionary<byte, OpCode> OpCodes { get; set; }

        public ushort PC;
        public byte SP;
        public byte AR;
        public byte XR;
        public byte YR;

        public StatusRegister SR = new StatusRegister();

        public OpCode OpCode { get; set; }

        public ushort Address;

        public byte Value {
            get {
                return OpCode.AddressingMode == AddressingMode.Accumulator ? AR : Memory[Address];
            }
            set {
                if (OpCode.AddressingMode == AddressingMode.Accumulator) {
                    AR = value;

                } else {
                    Memory[Address] = value;
                }
            }
        }

        public MOS6502(byte[] memory) {
            Memory = memory;

            var opCodesMethods = GetType()
                .GetMethods()
                .SelectMany(m => m.GetCustomAttributes(typeof(OpCodeDefinitionAttribute), true)
                .Select(a => new {
                    Attribute = a as OpCodeDefinitionAttribute,
                    Method = m
                }));

            var addressingMethods = GetType()
                .GetMethods()
                .SelectMany(m => m.GetCustomAttributes(typeof(AddressingModeAttribute), true)
                .Select(a => new {
                    Attribute = a as AddressingModeAttribute,
                    Method = m
                })).ToDictionary(x => x.Attribute.AddressingMode, x => x);

            var opCodes = opCodesMethods
                .Select(x => OpCode.FromOpCodeDefinitionAttribute(() => x.Method.Invoke(this, null), () => addressingMethods[x.Attribute.AddressingMode].Method.Invoke(this, null), x.Attribute))
                .ToList();

            OpCodes = opCodes.ToDictionary(x => x.Code, x => x);
        }

        public void Step() {
            OpCode = OpCodes[Memory[PC]];
            OpCode.OpCodeAddress = PC;

            PC++;

            if (OpCode.AddressingMode != AddressingMode.Implied && OpCode.AddressingMode != AddressingMode.Accumulator) {
                OpCode.GetAddress();
            }

            OpCode.Run();
        }


        [AddressingMode(AddressingMode = AddressingMode.Immediate)]
        public void Immediate() {
            Address = PC;
            PC++;
        }

        [AddressingMode(AddressingMode = AddressingMode.Relative)]
        public void Relative() {
            sbyte rel_addr = (sbyte)Memory[PC];
            PC++;
            Address = (ushort)(PC + rel_addr);
        }

        [AddressingMode(AddressingMode = AddressingMode.Zeropage)]
        public void Zeropage() {
            Address = Memory[PC];
            PC++;
        }

        [AddressingMode(AddressingMode = AddressingMode.ZeropageX)]
        public void ZeropageX() {
            ushort a = (ushort)(Memory[PC] + XR);
            Address = (a &= 0x00FF);
            PC++;
        }


        [OpCodeDefinition(Name = nameof(NOP), Code = 0x00, Length = 1, AddressingMode = AddressingMode.Implied, Description = "")]
        public void NOP() {

        }

        [OpCodeDefinition(Name = nameof(LDA), Code = 0xA9, Length = 2, AddressingMode = AddressingMode.Immediate, Description = "Load Accumulator")]
        [OpCodeDefinition(Name = nameof(LDA), Code = 0xB5, Length = 2, AddressingMode = AddressingMode.ZeropageX, Description = "Load Accumulator")]
        public void LDA() {
            AR = Value;
            SR.SetNegative(AR);
            SR.SetZero(AR);
        }

        [OpCodeDefinition(Name = nameof(LDX), Code = 0xA2, Length = 2, AddressingMode = AddressingMode.Immediate, Description = "Load X-register")]
        public void LDX() {
            XR = Value;
            SR.SetNegative(XR);
            SR.SetZero(XR);
        }

        [OpCodeDefinition(Name = nameof(STA), Code = 0x95, Length = 2, AddressingMode = AddressingMode.ZeropageX, Description = "Store Accumulator")]
        public void STA() {
            Value = AR;
        }


        [OpCodeDefinition(Name = nameof(OPCODE_0x49), Code = 0x49, Length = 2, AddressingMode = AddressingMode.Immediate, Description = "")]
        public void OPCODE_0x49() {
            AR = Program.OPCODE_0x49(Value, AR);

            SR.SetNegative(AR);
            SR.SetZero(AR);
        }

        [OpCodeDefinition(Name = nameof(INX), Code = 0xE8, Length = 1, AddressingMode = AddressingMode.Implied, Description = "Increment X-register")]
        public void INX() {
            XR++;
            SR.SetNegative(XR);
            SR.SetZero(XR);
        }

        [OpCodeDefinition(Name = nameof(BNE), Code = 0xD0, Length = 2, AddressingMode = AddressingMode.Relative, Description = "Branch on Not Equal")]
        public void BNE() {
            if (!SR.Zero) {
                PC = Address;
            }
        }

        [OpCodeDefinition(Name = nameof(CPX), Code = 0xE0, Length = 2, AddressingMode = AddressingMode.Immediate, Description = "Compare X-register")]
        public void CPX() {
            int r = XR - Value;

            SR.Carry = r >= 0;
            SR.Zero = r == 0;
            SR.Negative = ((r >> 7) & 1) == 1;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AddressingModeAttribute : Attribute {
        public AddressingMode AddressingMode { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OpCodeDefinitionAttribute : Attribute {
        public byte Code { get; set; }
        public string Name { get; set; }
        public ushort Length { get; set; }

        public AddressingMode AddressingMode { get; set; }

        public string Description { get; set; }
    }

    public enum AddressingMode {
        Accumulator,
        Immediate,
        Implied,
        Relative,
        Zeropage,
        ZeropageX
    }

    [Flags]
    public enum ProcessorStatusFlags : byte {
        Carry = 0b00000001,
        Zero = 0b00000010,
        IrqDisable = 0b00000100,
        DecimalMode = 0b00001000,
        BreakCommand = 0b00010000,
        Reserved = 0b00100000,
        Overflow = 0b01000000,
        Negative = 0b10000000,
    }

    public class OpCode : OpCodeDefinitionAttribute {
        public int OpCodeAddress { get; set; }

        public Action GetAddress { get; set; }
        public Action Run { get; set; }

        public static OpCode FromOpCodeDefinitionAttribute(Action action, Action getAddress, OpCodeDefinitionAttribute a) {
            return new OpCode {
                GetAddress = getAddress,
                Run = action,
                AddressingMode = a.AddressingMode,
                Code = a.Code,
                Description = a.Description,
                Length = a.Length,
                Name = a.Name
            };
        }
    }

    public class StatusRegister {
        public byte Register { get; set; }

        public bool Carry {
            get => Get(ProcessorStatusFlags.Carry);
            set => SetValue(ProcessorStatusFlags.Carry, value);
        }

        public bool Zero {
            get => Get(ProcessorStatusFlags.Zero);
            set => SetValue(ProcessorStatusFlags.Zero, value);
        }

        public bool Negative {
            get => Get(ProcessorStatusFlags.Negative);
            set => SetValue(ProcessorStatusFlags.Negative, value);
        }

        public bool Reserved {
            get => Get(ProcessorStatusFlags.Reserved);
            set => SetValue(ProcessorStatusFlags.Reserved, value);
        }

        public bool Get(ProcessorStatusFlags flag) {
            return (Register & (byte)flag) == (byte)flag;
        }

        public void SetValue(ProcessorStatusFlags flag, bool value) {
            Register = value ? Register |= (byte)flag : Register &= (byte)~flag;
        }

        public void SetNegative(byte operand) {
            Negative = (operand & 0b10000000) == 0b10000000;
        }

        public void SetZero(byte operand) {
            Zero = operand == 0;
        }
    }
}