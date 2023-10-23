using System;
using System.Globalization;
using SFML.Graphics;
using SFML.Window;
using SFML.System;
using System.ComponentModel.DataAnnotations;

namespace cpu6502;

class Program
{
    static RenderWindow window = null;
    static uint charSize = 30;
    static uint charWidth = 80;
    static uint charHeight = 50;

    static void Main(string[] args)
    {
        window = new RenderWindow(new VideoMode(charWidth*charSize, charHeight*charSize), "80x25 Character Simulator");
        window.Closed += (sender, e) => window.Close();

        RunAsync().GetAwaiter().GetResult();
    }

    static async Task RunAsync()
    {
        while (window.IsOpen)
        {
            window.DispatchEvents();
            window.Clear(Color.Blue);

            WriteText("Hello World", 1, 1);
            WriteText("This is more test", 1, 2);

            window.Display();
        }
        OutputAssembly();
    }

    static void WriteText(string text, int x, int y, Boolean display = false)
    {
        if (x>0) x--;
        if (y>0) y--;

        // Draw your characters/text here
        // PTM55FT.ttf
        // JetBrainsMono-Bold.ttf
        var font = new Font("JetBrainsMono-Bold.ttf"); // Use any monospace font
        var writeText = new Text(text, font, charSize); // The number 10 is the character size
        writeText.Position = new Vector2f(x*charSize, y*charSize); // Position on the window
        window.Draw(writeText);
        if (display)
            window.Display();
    }
    static Task<Keyboard.Key> WaitForKeypress(RenderWindow window)
    {
        var tcs = new TaskCompletionSource<Keyboard.Key>();

        void OnKeyPressed(object? sender, KeyEventArgs e)
        {
            window.KeyPressed -= OnKeyPressed; // Unsubscribe after keypress
            tcs.SetResult(e.Code);
        }

        window.KeyPressed += OnKeyPressed;

        return tcs.Task;
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

