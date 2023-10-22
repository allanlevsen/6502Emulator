using System;
using System.Globalization;

namespace cpu6502;

class Program
{
    static void Main(string[] args)
    {
        OutputAssembly();
    }

    static void OutputAssembly()
    {
        Cpu cpu = new Cpu();

        var progCode = "A2 0A 8E 00 00 A2 03 8E 01 00 AC 00 00 A9 00 18 6D 01 00 88 D0 FA 8D 02 00 EA";
        var opCodesArray = progCode.Split(" ");
        Dictionary<ushort, string> mapAsm;

        ushort nOffset = 0x8000;
        foreach(var op in opCodesArray)
        {
            byte opByte = byte.Parse(op, NumberStyles.HexNumber);
            cpu.bus.ram[nOffset++] = opByte;
        }

        // Extract dissassembly
        ushort stopAddr = (ushort)(0x8000 + opCodesArray.Length-1);
        mapAsm = cpu.Disassemble(0x8000, stopAddr);

        Console.WriteLine("---------------------------------------");
        Console.WriteLine();
        foreach (var asm in mapAsm)
        {
            Console.WriteLine(asm.Value);
        }
        Console.WriteLine();
        Console.WriteLine("---------------------------------------");


    }
}

