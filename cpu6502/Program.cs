using System;
using System.Globalization;
using SFML.Graphics;
using SFML.Window;
using SFML.System;

namespace cpu6502;

class Program
{
    static void Main(string[] args)
    {

            var window = new RenderWindow(new VideoMode(2400, 1500), "80x25 Character Simulator");
            window.Closed += (sender, e) => window.Close();

            while (window.IsOpen)
            {
                window.DispatchEvents();
                window.Clear(Color.Blue);

                // Draw your characters/text here
                var font = new Font("JetBrainsMono-Bold.ttf"); // Use any monospace font
                var text = new Text("Hello, World!", font, 40); // The number 10 is the character size
                text.Position = new Vector2f(0, 0); // Position on the window
                window.Draw(text);

                window.Display();
            }
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

