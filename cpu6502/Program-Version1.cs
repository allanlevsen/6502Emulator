﻿// using System;
// using System.Globalization;
// using SFML.Graphics;
// using SFML.Window;
// using SFML.System;
// using System.ComponentModel.DataAnnotations;
// using System.Net.NetworkInformation;
// using System.Runtime;
// using System.Text;

// namespace cpu6502;

// class DosText {
//     public Text TextObject;
//     public RectangleShape Background;
// }

// class PageText {

//     public uint characterSize;
//     public uint rowSize;
    
//     // Assembly Code Section Constants
//     //
//     public int maxAssemblyLines;
//     public int midPoint;
//     public int maxMemoryLines;
//     public int startAssemblyColumn;
//     public int startAssemblyRow;
//     public int startRamColumn;
//     public int startRamRow;

//     public Font font;
//     public Text RegisterALabel;
//     public DosText RegisterXLabel;
//     public Text RegisterYLabel;
//     public DosText ToolBarAbove;
//     public DosText ToolBar;
//     public DosText ToolBarBelow;

//     // // temp testing code
//     // public Text TextObject;
//     // public RectangleShape Background;

//     public RenderWindow window;

//     public List<Text> assemblyCode;
//     public List<Text> activeAssemblyCode;
//     public Text statusLabel;
//     public List<Text> clearedFlags;
//     public List<Text> setFlags;
//     public Text PCLabel;
//     public Text PCValue;
//     public Text ARegLabel;
//     public Text ARegValue;
//     public Text XRegLabel;
//     public Text XRegValue;
//     public Text YRegLabel;
//     public Text YRegValue;
//     public Text SPLabel;
//     public Text SPValue;

//     // memory related output
//     public Text zeroPageRamLable;
//     public List<Text> zeroPageRam;
//     public Text programRamLable;
//     public List<Text> programRam;

//     public PageText(RenderWindow window, uint charSize, uint rowSize)
//     {
//         this.characterSize = charSize;
//         this.rowSize = rowSize;
//         this.window = window;

//         // Assembly Code Section Constants
//         //
//         this.startAssemblyColumn = 55;
//         this.startAssemblyRow = 11;
//         this.maxAssemblyLines = 27;
//         this.midPoint = 14;

//         this.startRamColumn = 3;
//         this.startRamRow = 4;
//         this.maxMemoryLines = 12;

//         this.font = new Font("fonts/JetBrainsMono-Bold.ttf");
//         this.RegisterALabel = CreateText("A", 1, 1);
//         this.RegisterXLabel = CreateText("Xnciowgsef", 1, 2, Color.Green, Color.Black);
//         this.RegisterYLabel = CreateText("Y", 1, 3);
//         this.ToolBarAbove = CreateText("", 1, 40, Color.Green, Color.Black, true);
//         this.ToolBar = CreateText("  SPACE = Step Instruction    R = RESET    I = IRQ    N = NMI", 1, 41, Color.Green, Color.Black, true);
//         this.ToolBarBelow = CreateText("", 1, 42, Color.Green, Color.Black, true);

//         // CPU related text
//         // flags index : N V - B D I Z C
//         // PC, A, X, Y, SP
//         //
//         this.statusLabel = CreateText("FLAGS:", this.startAssemblyColumn, 2);

//         this.clearedFlags = new List<Text>();
//         this.setFlags = new List<Text>();
//         this.clearedFlags.Add(CreateText("N", this.startAssemblyColumn+5, 2, new Color(141, 141, 141)));
//         this.setFlags.Add(CreateText("N", this.startAssemblyColumn+5, 2, Color.White));
//         this.clearedFlags.Add(CreateText("V", this.startAssemblyColumn+6, 2, new Color(141, 141, 141)));
//         this.setFlags.Add(CreateText("V", this.startAssemblyColumn+6, 2, Color.White));
//         this.clearedFlags.Add(CreateText("-", this.startAssemblyColumn+7, 2, new Color(141, 141, 141)));
//         this.setFlags.Add(CreateText("-", this.startAssemblyColumn+7, 2, Color.White));
//         this.clearedFlags.Add(CreateText("B", this.startAssemblyColumn+8, 2, new Color(141, 141, 141)));
//         this.setFlags.Add(CreateText("B", this.startAssemblyColumn+8, 2, Color.White));
//         this.clearedFlags.Add(CreateText("D", this.startAssemblyColumn+9, 2, new Color(141, 141, 141)));
//         this.setFlags.Add(CreateText("D", this.startAssemblyColumn+9, 2, Color.White));
//         this.clearedFlags.Add(CreateText("I", this.startAssemblyColumn+10, 2, new Color(141, 141, 141)));
//         this.setFlags.Add(CreateText("I", this.startAssemblyColumn+10, 2, Color.White));
//         this.clearedFlags.Add(CreateText("Z", this.startAssemblyColumn+11, 2, new Color(141, 141, 141)));
//         this.setFlags.Add(CreateText("Z", this.startAssemblyColumn+11, 2, Color.White));
//         this.clearedFlags.Add(CreateText("C", this.startAssemblyColumn+12, 2, new Color(141, 141, 141)));
//         this.setFlags.Add(CreateText("C", this.startAssemblyColumn+12, 2, Color.White));

//         this.PCLabel = CreateText("PC:", this.startAssemblyColumn+2, 4, Color.White);
//         this.PCValue = CreateText("$0000", this.startAssemblyColumn+5, 4, Color.White);
//         this.ARegLabel = CreateText(" A:", this.startAssemblyColumn+2, 5, Color.White);
//         this.ARegValue = CreateText("$00", this.startAssemblyColumn+5, 5, Color.White);
//         this.XRegLabel = CreateText(" X:", this.startAssemblyColumn+2, 6, Color.White);
//         this.XRegValue = CreateText("$00", this.startAssemblyColumn+5, 6, Color.White);
//         this.YRegLabel = CreateText(" Y:", this.startAssemblyColumn+2, 7, Color.White);
//         this.YRegValue = CreateText("$00", this.startAssemblyColumn+5, 7, Color.White);
//         this.SPLabel = CreateText("SP:", this.startAssemblyColumn+2, 8, Color.White);
//         this.SPValue = CreateText("$0000", this.startAssemblyColumn+5, 8, Color.White);

//         // Assembly Code Section Constants
//         //
//         assemblyCode = new List<Text>();
//         activeAssemblyCode = new List<Text>();
//         for(int i=0; i<maxAssemblyLines; i++) {
//             assemblyCode.Add(CreateText("", this.startAssemblyColumn, this.startAssemblyRow + i));
//             activeAssemblyCode.Add(CreateText("", this.startAssemblyColumn, this.startAssemblyRow + i, Color.Green));
//         }

//         this.zeroPageRamLable = CreateText("Zero Page Memory", this.startRamColumn, this.startRamRow++, Color.White);
//         this.programRamLable = CreateText("Program Memory", this.startRamColumn, this.startRamRow+15, Color.White);
//         zeroPageRam = new List<Text>();
//         programRam = new List<Text>();
//         for(int i=0; i<maxMemoryLines; i++) {
//             zeroPageRam.Add(CreateText("", this.startRamColumn, this.startRamRow + i, new Color(141, 141, 141)));
//             programRam.Add(CreateText("", this.startRamColumn, this.startRamRow + 16 + i, new Color(141, 141, 141)));
//         }
//     }

//     public DosText CreateText(string text, int x, int y, Color color, Color bgColor, bool fillEntireRow = false)
//     {
//         DosText dosText = new DosText();
//         dosText.TextObject = CreateText(text, x, y, color);
//         FloatRect temp = dosText.TextObject.GetLocalBounds();
//         FloatRect backgroundRect;
//         if (fillEntireRow)
//         {
//             backgroundRect = new FloatRect(
//                 0,
//                 (y-1) * this.rowSize,
//                 characterSize*80,
//                 rowSize); 
//         } else {
//             backgroundRect = new FloatRect(
//                 (x-1) * this.characterSize,
//                 (y-1) * this.rowSize,
//                 temp.Width,
//                 rowSize); 
//         }
//         dosText.Background = new RectangleShape(new Vector2f(backgroundRect.Width, backgroundRect.Height))
//         {
//             Position = new Vector2f((x-1) * this.characterSize, ((y-1) * this.rowSize)),
//             FillColor = bgColor
//         };

//         return dosText;
//     }

//     public Text CreateText(string text, int x, int y, Color color = default)
//     {
//         if (x>0) x--;
//         if (y>0) y--;
//         if (color == default)
//             color = Color.White;

//         var writeText = new Text(text, font, characterSize)
//         {
//             Position = new Vector2f(x * characterSize, y * rowSize),
//             FillColor = color,
//         };

//         return writeText;
//     }  


// }


// class Program
// {
//     static RenderWindow window = null;
//     static uint charWidth = 80;
//     static uint charHeight = 50;

//     static PageText pageText = null;

//     // 6502 CPU Related Members
//     static Cpu cpu = new Cpu();
//     static ushort programCodeStartingAddress;
//     static ushort programCodeEndingAddress;


//     // 6502 Program Related Members
//     static string progCode; 
//     static string[] opCodesArray;
//     static Dictionary<ushort, string> mapAsm;
 
//     static Func<ushort, byte, string> hex = (n, d) =>
//     {
//         StringBuilder s = new StringBuilder(new string('0', d));
//         for (int i = d - 1; i >= 0; i--, n >>= 4)
//             s[i] = "0123456789ABCDEF"[n & 0xF];
//         return s.ToString();
//     };
//     static void Main(string[] args)
//     {
//         Func<ushort, byte, string> hex = (n, d) =>
//         {
//             StringBuilder s = new StringBuilder(new string('0', d));
//             for (int i = d - 1; i >= 0; i--, n >>= 4)
//                 s[i] = "0123456789ABCDEF"[n & 0xF];
//             return s.ToString();
//         };
        
//         // Setup a test program 
//         progCode = "A2 0A 8E 00 00 A2 03 8E 01 00 AC 00 00 A9 00 18 6D 01 00 88 D0 FA 8D 02 00 EA"; // A2 0A 8E 00 00 A2 03 8E 01 00 AC 00 00 A9 00 18 6D 01 00 88 D0 FA 8D 02 00 EA A2 0A 8E 00 00 A2 03 8E 01 00 AC 00 00 A9 00 18 6D 01 00 88 D0 FA 8D 02 00 EA";
//         opCodesArray = progCode.Split(" ");
//         programCodeStartingAddress = 0x8000;
//         programCodeEndingAddress = 0x8000;
//         LoadProgram();

//         // Set Reset Vector
// 		cpu.bus.ram[0xFFFC] = 0x00;
// 		cpu.bus.ram[0xFFFD] = 0x80;

//         // Reset
// 		cpu.reset(); 

//         uint characterSize = 30;
//         uint rowSize = 36;
//         window = new RenderWindow(new VideoMode(charWidth * characterSize, charHeight * characterSize), "80x25 Character Simulator");
//         window.Closed += (sender, e) => window.Close();
//         window.KeyPressed += OnKeyPressed;

//         pageText = new PageText(window, characterSize, rowSize);

//         while (window.IsOpen)
//         {

//             window.Clear(Color.Blue);

//             //pageText.ToolBar.DrawText(window, pageText.ToolBar);
//             window.Draw(pageText.ToolBarAbove.Background);
//             window.Draw(pageText.ToolBarAbove.TextObject);
//             window.Draw(pageText.ToolBar.Background);
//             window.Draw(pageText.ToolBar.TextObject);
//             window.Draw(pageText.ToolBarBelow.Background);
//             window.Draw(pageText.ToolBarBelow.TextObject);

//             // CPU Related
//             window.Draw(pageText.statusLabel);
//             window.Draw(pageText.clearedFlags[0]);
//             if (cpu.GetFlag(Cpu.Flag.N) > 0)
//                 window.Draw(pageText.setFlags[0]);
//             else
//                 window.Draw(pageText.clearedFlags[0]);

//             if (cpu.GetFlag(Cpu.Flag.V) > 0)
//                 window.Draw(pageText.setFlags[1]);
//             else
//                 window.Draw(pageText.clearedFlags[1]);

//             if (cpu.GetFlag(Cpu.Flag.U) > 0)
//                 window.Draw(pageText.setFlags[2]);
//             else
//                 window.Draw(pageText.clearedFlags[2]);

//             if (cpu.GetFlag(Cpu.Flag.B) > 0)
//                 window.Draw(pageText.setFlags[3]);
//             else
//                 window.Draw(pageText.clearedFlags[3]);

//             if (cpu.GetFlag(Cpu.Flag.D) > 0)
//                 window.Draw(pageText.setFlags[4]);
//             else
//                 window.Draw(pageText.clearedFlags[4]);

//             if (cpu.GetFlag(Cpu.Flag.I) > 0)
//                 window.Draw(pageText.setFlags[5]);
//             else
//                 window.Draw(pageText.clearedFlags[5]);

//             if (cpu.GetFlag(Cpu.Flag.Z) > 0)
//                 window.Draw(pageText.setFlags[6]);
//             else
//                 window.Draw(pageText.clearedFlags[6]);

//             if (cpu.GetFlag(Cpu.Flag.C) > 0)
//                 window.Draw(pageText.setFlags[7]);
//             else
//                 window.Draw(pageText.clearedFlags[7]);

//             window.Draw(pageText.PCLabel);
//             pageText.PCValue.DisplayedString = "$"+ hex(cpu.pc,4);
//             window.Draw(pageText.PCValue);
//             window.Draw(pageText.ARegLabel);
//             pageText.ARegValue.DisplayedString = "$"+ hex(cpu.a,2);
//             window.Draw(pageText.ARegValue);
//             window.Draw(pageText.XRegLabel);
//             pageText.XRegValue.DisplayedString = "$"+ hex(cpu.x,2);
//             window.Draw(pageText.XRegValue);
//             window.Draw(pageText.YRegLabel);
//             pageText.YRegValue.DisplayedString = "$"+ hex(cpu.y,2);
//             window.Draw(pageText.YRegValue);
//             window.Draw(pageText.SPLabel);
//             pageText.SPValue.DisplayedString = "$"+ hex(cpu.stkp,4);
//             window.Draw(pageText.SPValue);

//             DrawAssemblyCode("$"+ hex(cpu.pc,4));

//             window.Draw(pageText.zeroPageRamLable);
//             window.Draw(pageText.programRamLable); 

//             DrawRam(0x0000, pageText.maxMemoryLines, 16, true);
// 		    DrawRam(0x8000, pageText.maxMemoryLines, 16, false);

//             window.DispatchEvents();

//             window.Display();
//         }
//     }

//     private static void LoadProgram()
//     {
//         ushort nOffset = programCodeStartingAddress;
//         foreach (var op in opCodesArray)
//         {
//             byte opByte = byte.Parse(op, NumberStyles.HexNumber);
//             cpu.bus.ram[nOffset++] = opByte;
//         }
//         programCodeEndingAddress = nOffset;
//     }

//     static void OnKeyPressed(object sender, SFML.Window.KeyEventArgs e)
//     {

//         if (e.Code == Keyboard.Key.Space) {
//             if (cpu.pc <= programCodeEndingAddress) {
//                 do
//                 {
//                     cpu.clock();
//                 } 
//                 while (!cpu.complete());
//             } else {
//                 cpu.reset();
//             }
//         }

//         if (e.Code == Keyboard.Key.R)
//         {
//             cpu.reset();
//         }

//         if (e.Code == Keyboard.Key.I)
//         {
//             cpu.irq();
//         }

//         if (e.Code == Keyboard.Key.N)
//         {
//             cpu.nmi();
//         }

//         if (e.Code == Keyboard.Key.Escape)
//         {
//             window.Close();
//         }
//     }

//     static void DrawCpu()
//     {

//     }

    
// 	static void DrawRam(ushort nAddr, int nRows, int nColumns, bool isZeroPage)
// 	{
// 		for (int row = 0; row < nRows; row++)
// 		{
// 			string sOffset = "$" + hex(nAddr, 4) + ":";
// 			for (int col = 0; col < nColumns; col++)
// 			{
// 				sOffset += " " + hex(cpu.bus.Read(nAddr, true), 2);
// 				nAddr += 1;
// 			}
//             if (isZeroPage)
//             {
//                 pageText.zeroPageRam[row].DisplayedString = sOffset;
//                 window.Draw(pageText.zeroPageRam[row]);
//             }
//             else
//             {
//                 pageText.programRam[row].DisplayedString = sOffset;
//                 window.Draw(pageText.programRam[row]);
//             }
// 		}
// 	}


// 	static void DrawAssemblyCode(string curPC)
// 	{
//         // Extract dissassembly
//         ushort stopAddr = (ushort)(programCodeStartingAddress + opCodesArray.Length-1);
//         mapAsm = cpu.Disassemble(programCodeStartingAddress, stopAddr);

//         // get the index value of the item in the dictionary we are looking for
//         int indexPc = -1;
//         int codeOffset = 0;
//         var foundItem = mapAsm.FirstOrDefault(kvp => kvp.Value.Contains(curPC));
//         if (!foundItem.Equals(default(KeyValuePair<ushort, string>)))
//         {
//             // Get the index of the item
//             indexPc = mapAsm.ToList().FindIndex(kvp => kvp.Key == foundItem.Key);
//             if (indexPc>pageText.midPoint && mapAsm.Count>27)
//             {
//                 // there are more lines that fits on the page and
//                 // the current PC > half of the list
//                 // then, we will scroll the list upwards
//                 codeOffset = indexPc - pageText.midPoint;
//             }
//         }


//         // populate the SFML Text Array and draw the assembly code
//         //
//         int offset = 0;
//         foreach (var asm in mapAsm)
//         {
//             if (codeOffset>0) {
//                 codeOffset--;
//                 continue;
//             }
//             if (asm.Value.Contains(curPC))
//             {
//                 pageText.activeAssemblyCode[offset].DisplayedString = asm.Value;
//                 window.Draw(pageText.activeAssemblyCode[offset]);
//             }
//             else  
//             {          
//                 pageText.assemblyCode[offset].DisplayedString = asm.Value;
//                 window.Draw(pageText.assemblyCode[offset]);
//             }
//             offset++;
//             if (offset>=pageText.maxAssemblyLines)
//                 break;
//         }
// 	}

// }

