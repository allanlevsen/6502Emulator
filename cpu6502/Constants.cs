using System;
namespace cpu6502
{
    public delegate Byte AddrMode();
    public delegate Byte Operation();


    public struct OpCode
    {
        public byte opcode;
        public string name;
        public AddrMode am;
        public Operation op;
        public byte cycles;
    };

    public class Constants
	{
        public const ushort TOTAL_RAM = 64 * 1024 - 1;

    }
}

