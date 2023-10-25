using System;
using System.Globalization;
using SFML.Graphics;
using SFML.Window;
using SFML.System;
using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;

namespace cpu6502;

class DosText {
    public Text TextObject;
    public RectangleShape Background;
}

class PageText {

    public uint characterSize;
    public uint rowSize;
    
    // Assembly Code Section Constants
    //
    public int maxAssemblyLines;
    public int startAssemblyColumn;
    public int startAssemblyRow;

    public Font font;
    public Text RegisterALabel;
    public DosText RegisterXLabel;
    public Text RegisterYLabel;
    public DosText ToolBarAbove;
    public DosText ToolBar;
    public DosText ToolBarBelow;

    // temp testing code
    public Text TextObject;
    public RectangleShape Background;

    public RenderWindow window;

    public List<Text> assemblyCode;
    public PageText(RenderWindow window, uint charSize, uint rowSize)
    {
        this.characterSize = charSize;
        this.rowSize = rowSize;
        this.window = window;

        // Assembly Code Section Constants
        //
        this.startAssemblyColumn = 60;
        this.startAssemblyRow = 15;
        this.maxAssemblyLines = 20;

        this.font = new Font("JetBrainsMono-Bold.ttf");
        this.RegisterALabel = CreateText("A", 1, 1);
        this.RegisterXLabel = CreateText("Xnciowgsef", 1, 2, Color.Green, Color.Black);
        this.RegisterYLabel = CreateText("Y", 1, 3);
        this.ToolBarAbove = CreateText("", 1, 40, Color.Green, Color.Black, true);
        this.ToolBar = CreateText("  SPACE = Step Instruction    R = RESET    I = IRQ    N = NMI", 1, 41, Color.Green, Color.Black, true);
        this.ToolBarBelow = CreateText("", 1, 42, Color.Green, Color.Black, true);

        // Assembly Code Section Constants
        //
        assemblyCode = new List<Text>();
        for(int i=0; i<maxAssemblyLines; i++)
            assemblyCode.Add(CreateText("", this.startAssemblyColumn, this.startAssemblyRow + i));

    }

    public DosText CreateText(string text, int x, int y, Color color, Color bgColor, bool fillEntireRow = false)
    {
        DosText dosText = new DosText();
        dosText.TextObject = CreateText(text, x, y, color);
        FloatRect temp = dosText.TextObject.GetLocalBounds();
        FloatRect backgroundRect;
        if (fillEntireRow)
        {
            backgroundRect = new FloatRect(
                0,
                (y-1) * this.rowSize,
                characterSize*80,
                rowSize); 
        } else {
            backgroundRect = new FloatRect(
                (x-1) * this.characterSize,
                (y-1) * this.rowSize,
                temp.Width,
                rowSize); 
        }
        dosText.Background = new RectangleShape(new Vector2f(backgroundRect.Width, backgroundRect.Height))
        {
            Position = new Vector2f((x-1) * this.characterSize, ((y-1) * this.rowSize)),
            FillColor = bgColor
        };

        return dosText;
    }

    public Text CreateText(string text, int x, int y, Color color = default)
    {
        if (x>0) x--;
        if (y>0) y--;
        if (color == default)
            color = Color.White;

        var writeText = new Text(text, font, characterSize)
        {
            Position = new Vector2f(x * characterSize, y * rowSize),
            FillColor = color,
        };

        return writeText;
    }  


}


class Program
{
    static RenderWindow window = null;
    static uint charWidth = 80;
    static uint charHeight = 50;

    static PageText pageText = null;

    // 6502 CPU Related Members
    static Cpu cpu = new Cpu();
    static ushort programCodeStartingAddress;


    // 6502 Program Related Members
    static string progCode; 
    static string[] opCodesArray;
    static Dictionary<ushort, string> mapAsm;
 
    static void Main(string[] args)
    {
        // Setup a test program 
        progCode = "A2 0A 8E 00 00 A2 03 8E 01 00 AC 00 00 A9 00 18 6D 01 00 88 D0 FA 8D 02 00 EA";
        opCodesArray = progCode.Split(" ");
        programCodeStartingAddress = 0x8000;
        LoadProgram();

        // Set Reset Vector
		cpu.bus.ram[0xFFFC] = 0x00;
		cpu.bus.ram[0xFFFD] = 0x80;

        // Reset
        // todo: implement the reset
		// cpu.reset(); 

        uint characterSize = 30;
        uint rowSize = 36;
        window = new RenderWindow(new VideoMode(charWidth * characterSize, charHeight * characterSize), "80x25 Character Simulator");
        window.Closed += (sender, e) => window.Close();
        window.KeyPressed += OnKeyPressed;

        pageText = new PageText(window, characterSize, rowSize);

        while (window.IsOpen)
        {

            window.Clear(Color.Blue);

            //window.Draw(pageText.RegisterALabel);
            window.Draw(pageText.RegisterXLabel.Background);
            window.Draw(pageText.RegisterXLabel.TextObject);
            window.Draw(pageText.RegisterYLabel);

            //pageText.ToolBar.DrawText(window, pageText.ToolBar);
            window.Draw(pageText.ToolBarAbove.Background);
            window.Draw(pageText.ToolBarAbove.TextObject);
            window.Draw(pageText.ToolBar.Background);
            window.Draw(pageText.ToolBar.TextObject);
            window.Draw(pageText.ToolBarBelow.Background);
            window.Draw(pageText.ToolBarBelow.TextObject);

            DrawAssemblyCode();
            window.DispatchEvents();

            window.Display();
        }
    }

    private static void LoadProgram()
    {
        ushort nOffset = programCodeStartingAddress;
        foreach (var op in opCodesArray)
        {
            byte opByte = byte.Parse(op, NumberStyles.HexNumber);
            cpu.bus.ram[nOffset++] = opByte;
        }
    }

    static void OnKeyPressed(object sender, SFML.Window.KeyEventArgs e)
    {

        //if (e.Code == Keyboard.Key.Space) {
        //     do
		// 	{
		// 		cpu.Clock();
		// 	} 
		// 	while (!cpu.complete());
        //}

        // if (e.Code == Keyboard.Key.R)
        // {
        //     cpu.reset();
        // }

        // if (e.Code == Keyboard.Key.I)
        // {
        //     cpu.irq();
        // }

        // if (e.Code == Keyboard.Key.N)
        // {
        //     cpu.nmi();
        // }
    }

	static void DrawAssemblyCode()
	{

        // Extract dissassembly
        ushort stopAddr = (ushort)(programCodeStartingAddress + opCodesArray.Length-1);
        mapAsm = cpu.Disassemble(programCodeStartingAddress, stopAddr);

        // populate the SFML Text Array and draw the assembly code
        //
        int offset = 0;
        foreach (var asm in mapAsm)
        {
            pageText.assemblyCode[offset].DisplayedString = asm.Value;
            window.Draw(pageText.assemblyCode[offset]);
            offset++;
            if (offset>=pageText.maxAssemblyLines)
                break;
        }
	}

}

