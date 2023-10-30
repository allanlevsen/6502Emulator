using System.Runtime.InteropServices;
using cpu6502.Interfaces;
using SFML.Graphics; 
namespace cpu6502
{
   public class Ppu //: IPpu
   {

      public byte[,] tblName = new byte[2, 1024];
      public byte[,] tblPattern = new byte[2, 4096]; // not required, custom
      private byte[] tblPalette = new byte[32];

      public Color[] palScreen = new Color[0x40];

      // SFML screen/image/texture/sprite
      public Sprite sprScreen;
      public Image sprImage;
      public Texture sprTexture;


// name tables
      public Sprite[] sprNameTable = new Sprite[2];
      public Image sprNameTable0Image;
      public Image sprNameTable1Image;
      public Texture sprNameTable0Texture;
      public Texture sprNameTable1Texture;

// pattern tables
      public Sprite[] sprPatternTable = new Sprite[2];
      public Image sprPatternTable0Image;
      public Image sprPatternTable1Image;
      public Texture sprPatternTable0Texture;
      public Texture sprPatternTable1Texture;

      public Sprite GetScreen() => sprScreen;
      public Sprite GetNameTable(byte i) => sprNameTable[i];
      public Sprite GetPatternTable(byte i, byte palette) => sprPatternTable[i];
      public Color GetColourFromPaletteRam(byte palette, byte pixel) => palScreen[palette * 4 + pixel];
      public bool FrameComplete { get; set; } = false;

      [StructLayout(LayoutKind.Explicit)]
      public struct Status
      {
         [FieldOffset(0)] public byte reg;

         public bool SpriteOverflow
         {
            get { return (reg & (1 << 2)) != 0; }
            set
            {
                  if (value)
                     reg |= (byte)(1 << 2);
                  else
                     reg &= (byte)(~(1 << 2) & 0xFF);
            }
         }

         public bool SpriteZeroHit
         {
            get { return (reg & (1 << 1)) != 0; }
            set
            {
                  if (value)
                     reg |= (byte)(1 << 1);
                  else
                     reg &= (byte)(~(1 << 1) & 0xFF);
            }
         }

         public bool VerticalBlank
         {
            get { return (reg & 1) != 0; }
            set
            {
                  if (value)
                     reg |= 1;
                  else
                     reg &= 0xFE;
            }
         }
      }

      public Status status;

      [StructLayout(LayoutKind.Explicit)]
      public struct Mask
      {
         [FieldOffset(0)]
         public byte reg;

         public bool Grayscale
         {
            get { return (reg & 0x01) != 0; }
            set { reg = (byte)(value ? (reg | 0x01) : (reg & ~0x01)); }
         }

         public bool RenderBackgroundLeft
         {
            get { return (reg & 0x02) != 0; }
            set { reg = (byte)(value ? (reg | 0x02) : (reg & ~0x02)); }
         }

         public bool RenderSpritesLeft
         {
            get { return (reg & 0x04) != 0; }
            set { reg = (byte)(value ? (reg | 0x04) : (reg & ~0x04)); }
         }

         public bool RenderBackground
         {
            get { return (reg & 0x08) != 0; }
            set { reg = (byte)(value ? (reg | 0x08) : (reg & ~0x08)); }
         }

         public bool RenderSprites
         {
            get { return (reg & 0x10) != 0; }
            set { reg = (byte)(value ? (reg | 0x10) : (reg & ~0x10)); }
         }

         public bool EnhanceRed
         {
            get { return (reg & 0x20) != 0; }
            set { reg = (byte)(value ? (reg | 0x20) : (reg & ~0x20)); }
         }

         public bool EnhanceGreen
         {
            get { return (reg & 0x40) != 0; }
            set { reg = (byte)(value ? (reg | 0x40) : (reg & ~0x40)); }
         }

         public bool EnhanceBlue
         {
            get { return (reg & 0x80) != 0; }
            set { reg = (byte)(value ? (reg | 0x80) : (reg & ~0x80)); }
         }
      }

      public Mask mask;


      [StructLayout(LayoutKind.Explicit)]
      public struct PPUCTRL
      {
         [FieldOffset(0)] public byte reg;

         public bool NametableX
         {
            get { return (reg & (1 << 0)) != 0; }
            set
            {
                  if (value)
                     reg |= (byte)(1 << 0);
                  else
                     reg &= (byte)(~(1 << 0) & 0xFF);
            }
         }

         public bool NametableY
         {
            get { return (reg & (1 << 1)) != 0; }
            set
            {
                  if (value)
                     reg |= (byte)(1 << 1);
                  else
                     reg &= (byte)(~(1 << 1) & 0xFF);
            }
         }

         public bool IncrementMode
         {
            get { return (reg & (1 << 2)) != 0; }
            set
            {
                  if (value)
                     reg |= (byte)(1 << 2);
                  else
                     reg &= (byte)(~(1 << 2) & 0xFF);
            }
         }

         public bool PatternSprite
         {
            get { return (reg & (1 << 3)) != 0; }
            set
            {
                  if (value)
                     reg |= (byte)(1 << 3);
                  else
                     reg &= (byte)(~(1 << 3) & 0xFF);
            }
         }

         public bool PatternBackground
         {
            get { return (reg & (1 << 4)) != 0; }
            set
            {
                  if (value)
                     reg |= (byte)(1 << 4);
                  else
                     reg &= (byte)(~(1 << 4) & 0xFF);
            }
         }

         public bool SpriteSize
         {
            get { return (reg & (1 << 5)) != 0; }
            set
            {
                  if (value)
                     reg |= (byte)(1 << 5);
                  else
                     reg &= (byte)(~(1 << 5) & 0xFF);
            }
         }

         public bool SlaveMode // Note: This is marked as unused
         {
            get { return (reg & (1 << 6)) != 0; }
            set
            {
                  if (value)
                     reg |= (byte)(1 << 6);
                  else
                     reg &= (byte)(~(1 << 6) & 0xFF);
            }
         }

         public bool EnableNMI
         {
            get { return (reg & (1 << 7)) != 0; }
            set
            {
                  if (value)
                     reg |= (byte)(1 << 7);
                  else
                     reg &= (byte)(~(1 << 7) & 0xFF);
            }
         }
      }

      public PPUCTRL control;

      [StructLayout(LayoutKind.Explicit)]
      public struct LoopyRegister
      {
         [FieldOffset(0)] public ushort reg;

         public ushort CoarseX
         {
            get { return (ushort)(reg & 0x001F); }
            set { reg = (ushort)((reg & ~0x001F) | (value & 0x001F)); }
         }

         public ushort CoarseY
         {
            get { return (ushort)((reg >> 5) & 0x001F); }
            set { reg = (ushort)((reg & ~(0x001F << 5)) | ((value & 0x001F) << 5)); }
         }

         public bool NametableX
         {
            get { return (reg & (1 << 10)) != 0; }
            set
            {
                  if (value)
                     reg |= (ushort)(1 << 10);
                  else
                     reg &= (ushort)(~(1 << 10) & 0xFFFF);

            }
         }

         public bool NametableY
         {
            get { return (reg & (1 << 11)) != 0; }
            set
            {
                  if (value)
                     reg |= (ushort)(1 << 11);
                  else
                     reg &= (ushort)(~(1 << 11) & 0xFFFF);

            }
         }

         public ushort FineY
         {
            get { return (ushort)((reg >> 12) & 0x0007); }
            set { reg = (ushort)((reg & ~(0x0007 << 12)) | ((value & 0x0007) << 12)); }
         }

         public bool Unused
         {
            get { return (reg & (1 << 15)) != 0; }
            set
            {
                  if (value)
                     reg |= (ushort)(1 << 15);
                  else
                     reg &= (ushort)(~(1 << 15) & 0xFFFF);
            }
         }

         public LoopyRegister(ushort initialValue)
         {
            reg = initialValue;
         }
      }

		LoopyRegister vramAddr; // Active "pointer" address into nametable to extract background tile info
	   LoopyRegister tramAddr; // Temporary store of information to be "transferred" into "pointer" at various times
 

      private byte fineX;
      private byte addressLatch;
      private byte ppuDataBuffer;
      private short scanline;
      private short cycle;

      private byte bgNextTileId;
      private byte bgNextTileAttrib;
      private byte bgNextTileLsb;
      private byte bgNextTileMsb;
      private ushort bgShifterPatternLo;
      private ushort bgShifterPatternHi;
      private ushort bgShifterAttribLo;
      private ushort bgShifterAttribHi;

      public struct ObjectAttributeEntry
      {
         public byte y;
         public byte id;
         public byte attribute;
         public byte x;
      }
      private ObjectAttributeEntry[] OAM = new ObjectAttributeEntry[64];
      private byte oamAddr;

      private ObjectAttributeEntry[] spriteScanline = new ObjectAttributeEntry[8];
      private byte spriteCount;
      private byte[] spriteShifterPatternLo = new byte[8];
      private byte[] spriteShifterPatternHi = new byte[8];

      private bool bSpriteZeroHitPossible;
      private bool bSpriteZeroBeingRendered;
      // The following isn't supported in C
      //public byte[] POAM => (byte[])OAM;
      //
      // the following two methods will similate this
      //
      byte[] ObjectAttributeEntryToByteArray(ObjectAttributeEntry entry)
      {
         return new byte[]
         {
            entry.y,
            entry.id,
            entry.attribute,
            entry.x
         };
      }

      public byte[] POAM
      {
         get
         {
            List<byte> byteArray = new List<byte>();

            foreach (var entry in OAM)
            {
                  byteArray.AddRange(ObjectAttributeEntryToByteArray(entry));
            }

            return byteArray.ToArray();
         }
      }

      public Cartridge cart;
      public Ppu()
      {
         // name table
            sprNameTable0Image = new Image(342, 262, Color.Black);
            sprNameTable0Texture = new Texture(sprNameTable0Image);
            sprNameTable[0] = new Sprite(sprNameTable0Texture);
            sprNameTable1Image = new Image(342, 262, Color.Black);
            sprNameTable1Texture = new Texture(sprNameTable1Image);
            sprNameTable[1] = new Sprite(sprNameTable1Texture);

         // pattern table
            sprPatternTable[0] = new Sprite();
            sprPatternTable0Image = new Image(128, 128, Color.Black);
            sprPatternTable0Texture = new Texture(sprPatternTable0Image);
            sprPatternTable[0] = new Sprite(sprPatternTable0Texture);
            sprPatternTable1Image = new Image(128, 128, Color.Black);
            sprPatternTable1Texture = new Texture(sprPatternTable1Image);
            sprPatternTable[1] = new Sprite(sprPatternTable1Texture);

         //sprImage = new Image(256,240, new Color(64,64,64));
         sprImage = new Image(342,261, new Color(64,64,64));
         sprTexture = new Texture(sprImage);
         
         sprScreen = new Sprite(sprTexture, new IntRect(0,0,256,240)) {
            Scale = new SFML.System.Vector2f(6.0f, 6.0f)
         };

         InitializePalScreen();
      }

      private void InitializePalScreen()
      {
         palScreen[0x00] = new Color(84, 84, 84);
         palScreen[0x01] = new Color(0, 30, 116);
         palScreen[0x02] = new Color(8, 16, 144);
         palScreen[0x03] = new Color(48, 0, 136);
         palScreen[0x04] = new Color(68, 0, 100);
         palScreen[0x05] = new Color(92, 0, 48);
         palScreen[0x06] = new Color(84, 4, 0);
         palScreen[0x07] = new Color(60, 24, 0);
         palScreen[0x08] = new Color(32, 42, 0);
         palScreen[0x09] = new Color(8, 58, 0);
         palScreen[0x0A] = new Color(0, 64, 0);
         palScreen[0x0B] = new Color(0, 60, 0);
         palScreen[0x0C] = new Color(0, 50, 60);
         palScreen[0x0D] = new Color(0, 0, 0);
         palScreen[0x0E] = new Color(0, 0, 0);
         palScreen[0x0F] = new Color(0, 0, 0);

         palScreen[0x10] = new Color(152, 150, 152);
         palScreen[0x11] = new Color(8, 76, 196);
         palScreen[0x12] = new Color(48, 50, 236);
         palScreen[0x13] = new Color(92, 30, 228);
         palScreen[0x14] = new Color(136, 20, 176);
         palScreen[0x15] = new Color(160, 20, 100);
         palScreen[0x16] = new Color(152, 34, 32);
         palScreen[0x17] = new Color(120, 60, 0);
         palScreen[0x18] = new Color(84, 90, 0);
         palScreen[0x19] = new Color(40, 114, 0);
         palScreen[0x1A] = new Color(8, 124, 0);
         palScreen[0x1B] = new Color(0, 118, 40);
         palScreen[0x1C] = new Color(0, 102, 120);
         palScreen[0x1D] = new Color(0, 0, 0);
         palScreen[0x1E] = new Color(0, 0, 0);
         palScreen[0x1F] = new Color(0, 0, 0);

         palScreen[0x20] = new Color(236, 238, 236);
         palScreen[0x21] = new Color(76, 154, 236);
         palScreen[0x22] = new Color(120, 124, 236);
         palScreen[0x23] = new Color(176, 98, 236);
         palScreen[0x24] = new Color(228, 84, 236);
         palScreen[0x25] = new Color(236, 88, 180);
         palScreen[0x26] = new Color(236, 106, 100);
         palScreen[0x27] = new Color(212, 136, 32);
         palScreen[0x28] = new Color(160, 170, 0);
         palScreen[0x29] = new Color(116, 196, 0);
         palScreen[0x2A] = new Color(76, 208, 32);
         palScreen[0x2B] = new Color(56, 204, 108);
         palScreen[0x2C] = new Color(56, 180, 204);
         palScreen[0x2D] = new Color(60, 60, 60);
         palScreen[0x2E] = new Color(0, 0, 0);
         palScreen[0x2F] = new Color(0, 0, 0);

         palScreen[0x30] = new Color(236, 238, 236);
         palScreen[0x31] = new Color(168, 204, 236);
         palScreen[0x32] = new Color(188, 188, 236);
         palScreen[0x33] = new Color(212, 178, 236);
         palScreen[0x34] = new Color(236, 174, 236);
         palScreen[0x35] = new Color(236, 174, 212);
         palScreen[0x36] = new Color(236, 180, 176);
         palScreen[0x37] = new Color(228, 196, 144);
         palScreen[0x38] = new Color(204, 210, 120);
         palScreen[0x39] = new Color(180, 222, 120);
         palScreen[0x3A] = new Color(168, 226, 144);
         palScreen[0x3B] = new Color(152, 226, 180);
         palScreen[0x3C] = new Color(160, 214, 228);
         palScreen[0x3D] = new Color(160, 162, 160);
         palScreen[0x3E] = new Color(0, 0, 0);
         palScreen[0x3F] = new Color(0, 0, 0);
      }

      public void ConnectCartridge(Cartridge cartridge)
      {
         cart = cartridge;
      }

      public byte CpuRead(ushort addr, bool rdonly = false)
      {
         byte data = 0x00;
         uint mapped_addr = 0;
         //if (cart.CpuRead(addr, ref data))
         if (cart.CpuRead(addr, out data))
         {
            // Cartridge Address Range
         }
         else if (addr >= 0x2000 && addr <= 0x3FFF)
         {
            addr &= 0x0007;
            switch (addr)
            {
                  case 0x0000: // Control
                     break;
                  case 0x0001: // Mask
                     break;
                  case 0x0002: // Status
                     data = (byte)((status.reg & 0xE0) | (ppuDataBuffer & 0x1F));
                     status.VerticalBlank = false;
                     addressLatch = 0;
                     break;
                  case 0x0003: // OAM Address
                     break;
                  case 0x0004: // OAM Data
                     data = POAM[oamAddr];
                     break;
                  case 0x0005: // Scroll
                     break;
                  case 0x0006: // PPU Address
                     break;
                  case 0x0007: // PPU Data
                     data = ppuDataBuffer;
                     ppuDataBuffer = PpuRead(vramAddr.reg, rdonly);
                     if (addr > 0x3F00) data = ppuDataBuffer;
                     break;
            }
         }

         return data;
      }

      public void CpuWrite(ushort addr, byte data)
      {
         uint mapped_addr = 0;
         if (cart.CpuWrite(addr, data))
         {
            // Cartridge Address Range
         }
         else if (addr >= 0x2000 && addr <= 0x3FFF)
         {
            addr &= 0x0007;
            switch (addr)
            {
                  case 0x0000: // Control
                     control.reg = data;
                     tramAddr.NametableX = control.NametableX;
                     tramAddr.NametableY = control.NametableY;
                     break;
                  case 0x0001: // Mask
                     mask.reg = data;
                     break;
                  case 0x0002: // Status
                     break;
                  case 0x0003: // OAM Address
                     oamAddr = data;
                     break;
                  case 0x0004: // OAM Data
                     POAM[oamAddr] = data;
                     break;
                  case 0x0005: // Scroll
                     if (addressLatch == 0)
                     {
                        fineX = (byte)(data & 0x07);
                        tramAddr.CoarseX = (ushort)(data >> 3);
                        addressLatch = 1;
                     }
                     else
                     {
                        tramAddr.FineY = (ushort)(data & 0x07);
                        tramAddr.CoarseY = (ushort)(data >> 3);
                        addressLatch = 0;
                     }
                     break;
                  case 0x0006: // PPU Address
                     if (addressLatch == 0)
                     {
                        tramAddr.reg = (ushort)(((data & 0x3F) << 8) | (tramAddr.reg & 0x00FF));
                        addressLatch = 1;
                     }
                     else
                     {
                        tramAddr.reg = (ushort)((tramAddr.reg & 0xFF00) | data);
                        vramAddr.reg = tramAddr.reg;
                        addressLatch = 0;
                     }
                     break;
                  case 0x0007: // PPU Data
                     PpuWrite(vramAddr.reg, data);
                     break;
            }
         }
      }

      public byte PpuRead(ushort addr, bool rdonly = false)
      {
         byte data = 0x00;
         addr &= 0x3FFF;

         if (cart.PpuRead(addr, out data))
         {
            // Cartridge Address Range
         }
         else if (addr >= 0x0000 && addr <= 0x1FFF)
         {
            data = tblPattern[(addr & 0x1000) >> 12, addr & 0x0FFF];
         }
         else if (addr >= 0x2000 && addr <= 0x3EFF)
         {
            addr &= 0x0FFF;
            if (cart.mirror == Cartridge.MIRROR.VERTICAL)
            {
                  // Vertical
                  if (addr >= 0x0000 && addr <= 0x03FF)
                     data = tblName[0,addr & 0x03FF];
                  if (addr >= 0x0400 && addr <= 0x07FF)
                     data = tblName[1,addr & 0x03FF];
                  if (addr >= 0x0800 && addr <= 0x0BFF)
                     data = tblName[0,addr & 0x03FF];
                  if (addr >= 0x0C00 && addr <= 0x0FFF)
                     data = tblName[1,addr & 0x03FF];
            }
            else if (cart.mirror == Cartridge.MIRROR.HORIZONTAL)
            {
                  // Horizontal
                  if (addr >= 0x0000 && addr <= 0x03FF)
                     data = tblName[0,addr & 0x03FF];
                  if (addr >= 0x0400 && addr <= 0x07FF)
                     data = tblName[0,addr & 0x03FF];
                  if (addr >= 0x0800 && addr <= 0x0BFF)
                     data = tblName[1,addr & 0x03FF];
                  if (addr >= 0x0C00 && addr <= 0x0FFF)
                     data = tblName[1,addr & 0x03FF];
            }
         }
         else if (addr >= 0x3F00 && addr <= 0x3FFF)
         {
            addr &= 0x001F;
            if (addr == 0x0010) addr = 0x0000;
            if (addr == 0x0014) addr = 0x0004;
            if (addr == 0x0018) addr = 0x0008;
            if (addr == 0x001C) addr = 0x000C;
            data = (byte)(tblPalette[addr] & (mask.Grayscale ? 0x30 : 0x3F));
         }

         return data;
      }

      public void PpuWrite(ushort addr, byte data)
      {
         addr &= 0x3FFF;

         if (cart.PpuWrite(addr, data))
         {
            // Cartridge Address Range
         }
         else if (addr >= 0x0000 && addr <= 0x1FFF)
         {
            tblPattern[(addr & 0x1000) >> 12,addr & 0x0FFF] = data;
         }
         else if (addr >= 0x2000 && addr <= 0x3EFF)
         {
            addr &= 0x0FFF;
            if (cart.mirror == Cartridge.MIRROR.VERTICAL)
            {
                  // Vertical
                  if (addr >= 0x0000 && addr <= 0x03FF)
                     tblName[0,addr & 0x03FF] = data;
                  if (addr >= 0x0400 && addr <= 0x07FF)
                     tblName[1,addr & 0x03FF] = data;
                  if (addr >= 0x0800 && addr <= 0x0BFF)
                     tblName[0,addr & 0x03FF] = data;
                  if (addr >= 0x0C00 && addr <= 0x0FFF)
                     tblName[1,addr & 0x03FF] = data;
            }
            else if (cart.mirror == Cartridge.MIRROR.HORIZONTAL)
            {
                  // Horizontal
                  if (addr >= 0x0000 && addr <= 0x03FF)
                     tblName[0,addr & 0x03FF] = data;
                  if (addr >= 0x0400 && addr <= 0x07FF)
                     tblName[0,addr & 0x03FF] = data;
                  if (addr >= 0x0800 && addr <= 0x0BFF)
                     tblName[1,addr & 0x03FF] = data;
                  if (addr >= 0x0C00 && addr <= 0x0FFF)
                     tblName[1,addr & 0x03FF] = data;
            }
         }
         else if (addr >= 0x3F00 && addr <= 0x3FFF)
         {
            addr &= 0x001F;
            if (addr == 0x0010) addr = 0x0000;
            if (addr == 0x0014) addr = 0x0004;
            if (addr == 0x0018) addr = 0x0008;
            if (addr == 0x001C) addr = 0x000C;
            tblPalette[addr] = data;
         }
      }

      public void Clock()
      {
         Random rnd = new Random();
         int number = rnd.Next(0, 10);
          
         sprImage.SetPixel((uint)cycle, (uint)scanline, palScreen[number % 2 == 0 ? 0x3F : 0x30]);

         cycle++;
         if (cycle >= 341)
         {
            cycle = 0;
            scanline++;
            if (scanline >= 261)
            {
               scanline = 0;
               sprTexture.Update(sprImage);
               FrameComplete = true;
            }
         }

      }

      // public void Clock()
      // {
      //    Action IncrementScrollX = () =>
      //    {
      //       if (mask.RenderBackground || mask.RenderSprites)
      //       {
      //             if (vramAddr.CoarseX == 31)
      //             {
      //                vramAddr.CoarseX = 0;
      //                vramAddr.NametableX = !vramAddr.NametableX;
      //             }
      //             else
      //             {
      //                vramAddr.CoarseX++;
      //             }
      //       }
      //    };

      //    Action IncrementScrollY = () =>
      //    {
      //       if (mask.RenderBackground || mask.RenderSprites)
      //       {
      //             if (vramAddr.FineY < 7)
      //             {
      //                vramAddr.FineY++;
      //             }
      //             else
      //             {
      //                vramAddr.FineY = 0;

      //                if (vramAddr.CoarseY == 29)
      //                {
      //                   vramAddr.CoarseY = 0;
      //                   vramAddr.NametableY = !vramAddr.NametableY;
      //                }
      //                else if (vramAddr.CoarseY == 31)
      //                {
      //                   vramAddr.CoarseY = 0;
      //                }
      //                else
      //                {
      //                   vramAddr.CoarseY++;
      //                }
      //             }
      //       }
      //    };

      //    Action TransferAddressX = () =>
      //    {
      //       if (mask.RenderBackground || mask.RenderSprites)
      //       {
      //             vramAddr.NametableX = tramAddr.NametableX;
      //             vramAddr.CoarseX = tramAddr.CoarseX;
      //       }
      //    };

      //    Action TransferAddressY = () =>
      //    {
      //       if (mask.RenderBackground || mask.RenderSprites)
      //       {
      //             vramAddr.FineY = tramAddr.FineY;
      //             vramAddr.NametableY = tramAddr.NametableY;
      //             vramAddr.CoarseY = tramAddr.CoarseY;
      //       }
      //    };

      //    Action LoadBackgroundShifters = () =>
      //    {
      //       bgShifterPatternLo = (ushort)((bgShifterPatternLo & 0xFF00) | bgNextTileLsb);
      //       bgShifterPatternHi = (ushort)((bgShifterPatternHi & 0xFF00) | bgNextTileMsb);
      //       bgShifterAttribLo = (ushort)((bgShifterAttribLo & 0xFF00) | ((bgNextTileAttrib & 0x01) == 0x01 ? (ushort)0xFF : (ushort)0x00));
      //       bgShifterAttribHi = (ushort)((bgShifterAttribHi & 0xFF00) | ((bgNextTileAttrib & 0x02) == 0x02 ? (ushort)0xFF : (ushort)0x00));
      //    };

      //    Action UpdateShifters = () =>
      //    {
      //       if (mask.RenderBackground)
      //       {
      //             bgShifterPatternLo <<= 1;
      //             bgShifterPatternHi <<= 1;
      //             bgShifterAttribLo <<= 1;
      //             bgShifterAttribHi <<= 1;
      //       }

      //       if (mask.RenderSprites && cycle >= 1 && cycle < 258)
      //       {
      //             for (int i = 0; i < spriteCount; i++)
      //             {
      //                if (spriteScanline[i].x > 0)
      //                {
      //                   spriteScanline[i].x--;
      //                }
      //                else
      //                {
      //                   spriteShifterPatternLo[i] <<= 1;
      //                   spriteShifterPatternHi[i] <<= 1;
      //                }
      //             }
      //       }
      //    };

      //    if (scanline >= -1 && scanline < 240)
      //    {
      //       if (scanline == 0 && cycle == 0)
      //       {
      //          cycle = 1;
      //       }

      //       if (scanline == -1 && cycle == 1)
      //       {
      //          status.VerticalBlank = false;
      //          status.SpriteOverflow = false;
      //          status.SpriteZeroHit = false;
               
      //          for (int i = 0; i < 8; i++)
      //          {
      //                spriteShifterPatternLo[i] = 0;
      //                spriteShifterPatternHi[i] = 0;
      //          }
      //       }

      //       if ((cycle >= 2 && cycle < 258) || (cycle >= 321 && cycle < 338))
      //       {
      //          UpdateShifters();
      //       }
      //    }

      //    // ... [remainder of the Clock method] ...

      //    byte bg_pixel = 0x00;
      //    byte bg_palette = 0x00;

      //    if (mask.RenderBackground)
      //    {
      //       ushort bit_mux = (ushort)(0x8000 >> fineX);
      //       byte p0_pixel = (byte)((bgShifterPatternLo & bit_mux) > 0 ? 1 : 0);
      //       byte p1_pixel = (byte)((bgShifterPatternHi & bit_mux) > 0 ? 1 : 0);
      //       bg_pixel = (byte)((p1_pixel << 1) | p0_pixel);

      //       byte bg_pal0 = (byte)((bgShifterAttribLo & bit_mux) > 0 ? 1 : 0);
      //       byte bg_pal1 = (byte)((bgShifterAttribHi & bit_mux) > 0 ? 1 : 0);
      //       bg_palette = (byte)((bg_pal1 << 1) | bg_pal0);
      //    }

      //    byte fg_pixel = 0x00;
      //    byte fg_palette = 0x00;
      //    byte fg_priority = 0x00;

      //    if (mask.RenderSprites)
      //    {
      //       bSpriteZeroBeingRendered = false;

      //       for (int i = 0; i < spriteCount; i++)
      //       {
      //          if (spriteScanline[i].x == 0)
      //          {
      //                byte fg_pixel_lo = (byte)((spriteShifterPatternLo[i] & 0x80) > 0 ? 1 : 0);
      //                byte fg_pixel_hi = (byte)((spriteShifterPatternHi[i] & 0x80) > 0 ? 1 : 0);
      //                fg_pixel = (byte)((fg_pixel_hi << 1) | fg_pixel_lo);

      //                fg_palette = (byte)((spriteScanline[i].attribute & 0x03) + 0x04);
      //                fg_priority = (byte)((spriteScanline[i].attribute & 0x20) == 0 ? 1 : 0);

      //                if (fg_pixel != 0)
      //                {
      //                   if (i == 0)
      //                   {
      //                      bSpriteZeroBeingRendered = true;
      //                   }

      //                   break;
      //                }
      //          }
      //       }
      //    }

      //    byte pixel = 0x00;
      //    byte palette = 0x00;

      //    if (bg_pixel == 0 && fg_pixel == 0)
      //    {
      //       pixel = 0x00;
      //       palette = 0x00;
      //    }
      //    else if (bg_pixel == 0 && fg_pixel > 0)
      //    {
      //       pixel = fg_pixel;
      //       palette = fg_palette;
      //    }
      //    else if (bg_pixel > 0 && fg_pixel == 0)
      //    {
      //       pixel = bg_pixel;
      //       palette = bg_palette;
      //    }
      //    else if (bg_pixel > 0 && fg_pixel > 0)
      //    {
      //       if (fg_priority != 0)
      //       {
      //          pixel = fg_pixel;
      //          palette = fg_palette;
      //       }
      //       else
      //       {
      //          pixel = bg_pixel;
      //          palette = bg_palette;
      //       }

      //       if (bSpriteZeroHitPossible && bSpriteZeroBeingRendered)
      //       {
      //          if (mask.RenderBackground != false && mask.RenderSprites != false)
      //          {
      //                if (cycle >= 9 && cycle < 258)
      //                {
      //                   status.SpriteZeroHit = true;
      //                }
      //                else if (cycle >= 1 && cycle < 258)
      //                {
      //                   status.SpriteZeroHit = true;
      //                }
      //          }
      //       }
      //    }

      //    if (cycle-1>=0 && scanline>=0)
      //       sprImage.SetPixel((uint)cycle - 1, (uint)scanline, GetColourFromPaletteRam(palette, pixel));


      //    cycle++;
      //    if (cycle >= 341)
      //    {
      //       cycle = 0;
      //       scanline++;
      //       if (scanline >= 261)
      //       {
      //          scanline = -1;
      //          sprTexture.Update(sprImage);
               
      //          FrameComplete = true;
      //       }
      //    }
      // }

      public void Reset()
      {
         fineX = 0x00;
         addressLatch = 0x00;
         ppuDataBuffer = 0x00;
         scanline = 0;
         cycle = 0;
         bgNextTileId = 0x00;
         bgNextTileAttrib = 0x00;
         bgNextTileLsb = 0x00;
         bgNextTileMsb = 0x00;
         bgShifterPatternLo = 0x0000;
         bgShifterPatternHi = 0x0000;
         bgShifterAttribLo = 0x0000;
         bgShifterAttribHi = 0x0000;
         status.reg = 0x00;
         mask.reg = 0x00;
         control.reg = 0x00;
         vramAddr.reg = 0x0000;
         tramAddr.reg = 0x0000;
      }

      public bool Nmi { get; set; } = false;
   }
}