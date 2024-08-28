using System.Runtime.InteropServices;
using cpu6502.Interfaces;
using SFML.Graphics; 
namespace cpu6502
{
   public class Ppu
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
      public Texture sprNameTable0Texture;
      public Image sprNameTable1Image;
      public Texture sprNameTable1Texture;

      // pattern tables
      public Sprite[] sprPatternTable = new Sprite[2];
      public Image sprPatternTable0Image;
      public Texture sprPatternTable0Texture;
      public Image sprPatternTable1Image;
      public Texture sprPatternTable1Texture;

      public Sprite GetScreen() => sprScreen;
      public Sprite GetNameTable(byte i) => sprNameTable[i];
      public bool FrameComplete { get; set; } = false;

      [StructLayout(LayoutKind.Explicit)]
      public struct Status
      {
         [FieldOffset(0)] public byte reg;

         public byte SpriteOverflow
         {
            get { return (byte)(reg & (1 << 2)); }
            set
            {
                  if (value > 0)
                     reg |= (byte)(1 << 2);
                  else
                     reg &= (byte)(~(1 << 2) & 0xFF);
            }
         }

         public byte SpriteZeroHit
         {
            get { return (byte)(reg & (1 << 1)); }
            set
            {
                  if (value > 0)
                     reg |= (byte)(1 << 1);
                  else
                     reg &= (byte)(~(1 << 1) & 0xFF);
            }
         }

         public byte VerticalBlank
         {
            get { return (byte)(reg & 1); }
            set
            {
                  if (value > 0)
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
            get { 
               byte regResult = (byte)(reg & 0x08);
               return (reg & 0x08) != 0; 
            }
            set { 
               reg = (byte)(value ? (reg | 0x08) : (reg & ~0x08)); 
            }
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

         public byte NametableX
         {
            get { return (byte)(reg & (1 << 0)); }
            set
            {
                  if (value > 0)
                     reg |= (byte)(1 << 0);
                  else
                     reg &= (byte)(~(1 << 0) & 0xFF);
            }
         }

         public byte NametableY
         {
            get { return (byte)(reg & (1 << 1)); }
            set
            {
                  if (value > 0)
                     reg |= (byte)(1 << 1);
                  else
                     reg &= (byte)(~(1 << 1) & 0xFF);
            }
         }

         public byte IncrementMode
         {
            get { return (byte)(reg & (1 << 2)); }
            set
            {
                  if (value > 0)
                     reg |= (byte)(1 << 2);
                  else
                     reg &= (byte)(~(1 << 2) & 0xFF);
            }
         }

         public byte PatternSprite
         {
            get { return (byte)(reg & (1 << 3)); }
            set
            {
                  if (value > 0)
                     reg |= (byte)(1 << 3);
                  else
                     reg &= (byte)(~(1 << 3) & 0xFF);
            }
         }

         public byte PatternBackground
         {
            get { return (byte)(reg & (1 << 4)); }
            set
            {
                  if (value > 0)
                     reg |= (byte)(1 << 4);
                  else
                     reg &= (byte)(~(1 << 4) & 0xFF);
            }
         }

         public byte SpriteSize
         {
            get { return (byte)(reg & (1 << 5)); }
            set
            {
                  if (value > 0)
                     reg |= (byte)(1 << 5);
                  else
                     reg &= (byte)(~(1 << 5) & 0xFF);
            }
         }

         public byte SlaveMode // Note: This is marked as unused
         {
            get { return (byte)(reg & (1 << 6)); }
            set
            {
                  if (value > 0)
                     reg |= (byte)(1 << 6);
                  else
                     reg &= (byte)(~(1 << 6) & 0xFF);
            }
         }

         public byte EnableNMI
         {
            get { return (byte)(reg & (1 << 7)); }
            set
            {
                  if (value > 0)
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

         public ushort NametableX
         {
            get { return (ushort)(reg & (1 << 10)); }
            set
            {
                  if (value != 0)
                     reg |= (ushort)(1 << 10);
                  else
                     reg &= (ushort)(~(1 << 10) & 0xFFFF);

            }
         }


         public ushort NametableY
         {
            get { return (ushort)(reg & (1 << 11)); }
            set
            {
                  if (value != 0)
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

      public bool Nmi { get; set; } = false;

public ushort addrOffset = 0;
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
            sprPatternTable0Image = new Image(128, 128, Color.White);
            sprPatternTable0Texture = new Texture(sprPatternTable0Image);
            sprPatternTable[0] = new Sprite(sprPatternTable0Texture) {
               Scale = new SFML.System.Vector2f(2.0f, 2.0f)
            };
            sprPatternTable1Image = new Image(128, 128, Color.White);
            sprPatternTable1Texture = new Texture(sprPatternTable1Image);
            sprPatternTable[1] = new Sprite(sprPatternTable1Texture) {
               Scale = new SFML.System.Vector2f(2.0f, 2.0f)
            };

         sprImage = new Image(342,261, new Color(64,64,64));
         sprTexture = new Texture(sprImage);
         
         sprScreen = new Sprite(sprTexture, new IntRect(1,1,256,240)) {
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

      public byte CpuRead(ushort addr, bool rdonly)
      {
         byte data = 0x00;

         if (rdonly)
         {
            // Reading from PPU registers can affect their contents
            // so this read-only option is used for examining the
            // state of the PPU without changing its state. This is
            // really only used in debug mode.
            switch (addr)
            {
                  case 0x0000: // Control
                     data = control.reg;
                     break;
                  case 0x0001: // Mask
                     data = mask.reg;
                     break;
                  case 0x0002: // Status
                     data = status.reg;
                     break;
                  case 0x0003: // OAM Address
                     break;
                  case 0x0004: // OAM Data
                     break;
                  case 0x0005: // Scroll
                     break;
                  case 0x0006: // PPU Address
                     break;
                  case 0x0007: // PPU Data
                     break;
            }
         }
         else
         {
            // These are the live PPU registers that respond
            // to being read from in various ways. Note that not
            // all the registers are capable of being read from
            // so they just return 0x00
            switch (addr)
            {
                  // Control - Not readable
                  case 0x0000: break;
                  
                  // Mask - Not readable
                  case 0x0001: break;
                  
                  // Status
                  case 0x0002:
                     data = (byte)((status.reg & 0xE0) | (ppuDataBuffer & 0x1F));
                     status.VerticalBlank = 0;
                     addressLatch = 0;
                     break;

                  // OAM Address
                  case 0x0003: break;

                  // OAM Data
                  case 0x0004: break;

                  // Scroll - Not readable
                  case 0x0005: break;

                  // PPU Address - Not readable
                  case 0x0006: break;

                  // PPU Data
                  case 0x0007:
                     data = ppuDataBuffer;
                     ppuDataBuffer = PpuRead(vramAddr.reg);
                     if (vramAddr.reg >= 0x3F00) data = ppuDataBuffer;
                     vramAddr.reg += (ushort)(control.IncrementMode>0 ? 32 : 1);
                     break;
            }
         }

         return data;
      }

      public void CpuWrite(ushort addr, byte data)
      {
         switch (addr)
         { 
            case 0x0000: // Control
                  control.reg = data;
                  tramAddr.NametableX = (byte)control.NametableX;
                  tramAddr.NametableY = (byte)control.NametableY;
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
                     tramAddr.CoarseX = (byte)(data >> 3);
                     addressLatch = 1;
                  }
                  else
                  {
                     tramAddr.FineY = (byte)(data & 0x07);
                     tramAddr.CoarseY = (byte)(data >> 3);
                     addressLatch = 0;
                  }
                  break;
            case 0x0006: // PPU Address
                  if (addressLatch == 0)
                  {
                     // PPU address bus can be accessed by CPU via the ADDR and DATA
                     // registers. The fisrt write to this register latches the high byte
                     // of the address, the second is the low byte. Note the writes
                     // are stored in the tram register...

                     tramAddr.reg = (ushort)(((data & 0x3F) << 8) | (tramAddr.reg & 0x00FF));
                     addressLatch = 1;
                  }
                  else
                  {
                     // ...when a whole address has been written, the internal vram address
                     // buffer is updated. Writing to the PPU is unwise during rendering
                     // as the PPU will maintam the vram address automatically whilst
                     // rendering the scanline position.
                     tramAddr.reg = (ushort)((tramAddr.reg & 0xFF00) | data);
                     vramAddr = tramAddr;
                     addressLatch = 0;
                  }
                  break;
            case 0x0007: // PPU Data
                  // All writes from PPU data automatically increment the nametable
                  // address depending upon the mode set in the control register.
                  // If set to vertical mode, the increment is 32, so it skips
                  // one whole nametable row; in horizontal mode it just increments
                  // by 1, moving to the next column
                  PpuWrite(vramAddr.reg, data);
                  vramAddr.reg += (ushort)(control.IncrementMode>0 ? 32 : 1);
                  break;
         }
      }

      public byte PpuRead(ushort addr, bool rdonly = false)
      {
         byte data = 0x00;
         addr &= 0x3FFF;

         if (cart.PpuRead(addr, ref data))
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

      public Sprite GetPatternTable(byte i, byte palette)
      {
         // This function draws the CHR ROM for a given pattern table into
         // an olc::Sprite, using a specified palette. Pattern tables consist
         // of 16x16 "tiles or characters". It is independent of the running
         // emulation and using it does not change the systems state, though
         // it gets all the data it needs from the live system. Consequently,
         // if the game has not yet established palettes or mapped to relevant
         // CHR ROM banks, the sprite may look empty. This approach permits a 
         // "live" extraction of the pattern table exactly how the NES, and 
         // ultimately the player would see it.
         
         // A tile consists of 8x8 pixels. On the NES, pixels are 2 bits, which
         // gives an index into 4 different colours of a specific palette. There
         // are 8 palettes to choose from. Colour "0" in each palette is effectively
         // considered transparent, as those locations in memory "mirror" the global
         // background colour being used. The mechanics of this are shown in 
         // detail in ppuRead() & ppuWrite()

         // Characters on NES
         // ~~~~~~~~~~~~~~~~~
         // The NES stores characters using 2-bit pixels. These are not stored sequentially
         // but in singular bit planes. For example:
         //
         // 2-Bit Pixels       LSB Bit Plane     MSB Bit Plane
         // 0 0 0 0 0 0 0 0	  0 0 0 0 0 0 0 0   0 0 0 0 0 0 0 0
         // 0 1 1 0 0 1 1 0	  0 1 1 0 0 1 1 0   0 0 0 0 0 0 0 0
         // 0 1 2 0 0 2 1 0	  0 1 1 0 0 1 1 0   0 0 1 0 0 1 0 0
         // 0 0 0 0 0 0 0 0 =   0 0 0 0 0 0 0 0 + 0 0 0 0 0 0 0 0
         // 0 1 1 0 0 1 1 0	  0 1 1 0 0 1 1 0   0 0 0 0 0 0 0 0
         // 0 0 1 1 1 1 0 0	  0 0 1 1 1 1 0 0   0 0 0 0 0 0 0 0
         // 0 0 0 2 2 0 0 0	  0 0 0 1 1 0 0 0   0 0 0 1 1 0 0 0
         // 0 0 0 0 0 0 0 0	  0 0 0 0 0 0 0 0   0 0 0 0 0 0 0 0
         //
         // The planes are stored as 8 bytes of LSB, followed by 8 bytes of MSB

         for (ushort nTileY = 0; nTileY < 16; nTileY++)
         {
            for (ushort nTileX = 0; nTileX < 16; nTileX++)
            {
                  ushort nOffset = (ushort)(nTileY * 256 + nTileX * 16);

                  for (ushort row = 0; row < 8; row++)
                  {
                     byte tile_lsb = PpuRead((ushort)(i * 0x1000 + nOffset + row + 0x0000));
                     byte tile_msb = PpuRead((ushort)(i * 0x1000 + nOffset + row + 0x0008));

                     for (ushort col = 0; col < 8; col++)
                     {
                        byte pixel = (byte)((tile_lsb & 0x01) + (tile_msb & 0x01));

                        tile_lsb >>= 1;
                        tile_msb >>= 1;

                        if (i==0) {
                           sprPatternTable0Image.SetPixel((uint)(nTileX * 8 + (7 - col)), (uint)nTileY * 8 + row, GetColourFromPaletteRam(palette, pixel));
                        } else {
                           sprPatternTable1Image.SetPixel((uint)(nTileX * 8 + (7 - col)), (uint)nTileY * 8 + row, GetColourFromPaletteRam(palette, pixel));
                        }
                     }
                  }
            }
         }

         sprPatternTable0Texture.Update(sprPatternTable0Image);
         sprPatternTable1Texture.Update(sprPatternTable1Image);

         return sprPatternTable[i];
      }

      public Color GetColourFromPaletteRam(byte palette, byte pixel)
      {
         // This is a convenience function that takes a specified palette and pixel
         // index and returns the appropriate screen colour.
         // "0x3F00"       - Offset into PPU addressable range where palettes are stored
         // "palette << 2" - Each palette is 4 bytes in size
         // "pixel"        - Each pixel index is either 0, 1, 2 or 3
         // "& 0x3F"       - Stops us reading beyond the bounds of the palScreen array
         return palScreen[PpuRead((ushort)(0x3F00 + (palette << 2) + pixel)) & 0x3F];

         // Note: We don't access tblPalette directly here, instead we know that ppuRead()
         // will map the address onto the separate small RAM attached to the PPU bus.
      }

      // public void Clock()
      // {
      //    Random rnd = new Random();
      //    int number = rnd.Next(0, 10);
          
      //    sprImage.SetPixel((uint)cycle, (uint)scanline, palScreen[number % 2 == 0 ? 0x3F : 0x30]);

      //    cycle++;
      //    if (cycle >= 341)
      //    {
      //       cycle = 0;
      //       scanline++;
      //       if (scanline >= 261)
      //       {
      //          scanline = 0;
      //          sprTexture.Update(sprImage);
      //          FrameComplete = true;
      //       }
      //    }
      // }

      public void Clock()
      {
         Action IncrementScrollX = () =>
         {
            if (mask.RenderBackground || mask.RenderSprites)
            {
                  if (vramAddr.CoarseX == 31)
                  {
                     vramAddr.CoarseX = 0;
                     //vramAddr.NametableX =  !vramAddr.NametableX;
                     // todo the above, we need to do the following
                     bool isBitSet = (bool)((vramAddr.reg & (1 << 10)) != 0);
                     vramAddr.NametableX = (ushort)(isBitSet ? 0 : 1);
                  }
                  else
                  {
                     vramAddr.CoarseX++;
                  }
            }
         };

         Action IncrementScrollY = () =>
         {
            if (mask.RenderBackground || mask.RenderSprites)
            {
                  if (vramAddr.FineY < 7)
                  {
                     vramAddr.FineY++;
                  }
                  else
                  {
                     vramAddr.FineY = 0;

                     if (vramAddr.CoarseY == 29)
                     {
                        vramAddr.CoarseY = 0;
                        //vramAddr.NametableY = !vramAddr.NametableY;
                        bool isBitSet = (bool)((vramAddr.reg & (1 << 11)) != 0);
                        vramAddr.NametableY = (ushort)(isBitSet ? 0 : 1);
                     }
                     else if (vramAddr.CoarseY == 31)
                     {
                        vramAddr.CoarseY = 0;
                     }
                     else
                     {
                        vramAddr.CoarseY++;
                     }
                  }
            }
         };

         Action TransferAddressX = () =>
         {
            if (mask.RenderBackground || mask.RenderSprites)
            {
                  vramAddr.NametableX = tramAddr.NametableX;
                  vramAddr.CoarseX = tramAddr.CoarseX;
            }
         };

         Action TransferAddressY = () =>
         {
            if (mask.RenderBackground || mask.RenderSprites)
            {
                  vramAddr.FineY = tramAddr.FineY;
                  vramAddr.NametableY = tramAddr.NametableY;
                  vramAddr.CoarseY = tramAddr.CoarseY;
            }
         };

         Action LoadBackgroundShifters = () =>
         {
            bgShifterPatternLo = (ushort)((bgShifterPatternLo & 0xFF00) | (byte)bgNextTileLsb);
            bgShifterPatternHi = (ushort)((bgShifterPatternHi & 0xFF00) | (byte)bgNextTileMsb);
            bgShifterAttribLo = (ushort)((bgShifterAttribLo & 0xFF00) | (byte)((bgNextTileAttrib & 0b01) != 0 ? 0xFF : 0x00));
            bgShifterAttribHi = (ushort)((bgShifterAttribHi & 0xFF00) | (byte)((bgNextTileAttrib & 0b10) != 0 ? 0xFF : 0x00));
         };

         Action UpdateShifters = () =>
         {
            if (mask.RenderBackground)
            {
                  bgShifterPatternLo <<= 1;
                  bgShifterPatternHi <<= 1;
                  bgShifterAttribLo <<= 1;
                  bgShifterAttribHi <<= 1;
            }

            if (mask.RenderSprites && cycle >= 1 && cycle < 258)
            {
                  for (int i = 0; i < spriteCount; i++)
                  {
                     if (spriteScanline[i].x > 0)
                     {
                        spriteScanline[i].x--;
                     }
                     else
                     {
                        spriteShifterPatternLo[i] <<= 1;
                        spriteShifterPatternHi[i] <<= 1;
                     }
                  }
            }
         };

         if (scanline >= -1 && scanline < 240)
         {
            if (scanline == 0 && cycle == 0)
            {
               cycle = 1;
            }

            if (scanline == -1 && cycle == 1)
            {
               status.VerticalBlank = 0;
               status.SpriteOverflow = 0;
               status.SpriteZeroHit = 0;

               for (int i = 0; i < 8; i++)
               {
                     spriteShifterPatternLo[i] = 0;
                     spriteShifterPatternHi[i] = 0;
               }
            }

            if ((cycle >= 2 && cycle < 258) || (cycle >= 321 && cycle < 338))
            {
               UpdateShifters();

               // In these cycles we are collecting and working with visible data
               // The "shifters" have been preloaded by the end of the previous
               // scanline with the data for the start of this scanline. Once we
               // leave the visible region, we go dormant until the shifters are
               // preloaded for the next scanline.

               // Fortunately, for background rendering, we go through a fairly
               // repeatable sequence of events, every 2 clock cycles.
               switch ((cycle - 1) % 8)
               {
                  case 0:
                     // Load the current background tile pattern and attributes into the "shifter"
                     LoadBackgroundShifters();

                     // Fetch the next background tile ID
                     // "(vram_addr.reg & 0x0FFF)" : Mask to 12 bits that are relevant
                     // "| 0x2000"                 : Offset into nametable space on PPU address bus

                     bgNextTileId = PpuRead((ushort)(0x2000 | (vramAddr.reg & 0x0FFF)));

                     // Explanation:
                     // The bottom 12 bits of the loopy register provide an index into
                     // the 4 nametables, regardless of nametable mirroring configuration.
                     // nametable_y(1) nametable_x(1) coarse_y(5) coarse_x(5)
                     //
                     // Consider a single nametable is a 32x32 array, and we have four of them
                     //   0                1
                     // 0 +----------------+----------------+
                     //   |                |                |
                     //   |                |                |
                     //   |    (32x32)     |    (32x32)     |
                     //   |                |                |
                     //   |                |                |
                     // 1 +----------------+----------------+
                     //   |                |                |
                     //   |                |                |
                     //   |    (32x32)     |    (32x32)     |
                     //   |                |                |
                     //   |                |                |
                     //   +----------------+----------------+
                     //
                     // This means there are 4096 potential locations in this array, which 
                     // just so happens to be 2^12!
                     break;
                  case 2:
                     // Fetch the next background tile attribute. OK, so this one is a bit
                     // more involved :P

                     // Recall that each nametable has two rows of cells that are not tile 
                     // information, instead they represent the attribute information that
                     // indicates which palettes are applied to which area on the screen.
                     // Importantly (and frustratingly) there is not a 1 to 1 correspondance
                     // between background tile and palette. Two rows of tile data holds
                     // 64 attributes. Therfore we can assume that the attributes affect
                     // 8x8 zones on the screen for that nametable. Given a working resolution
                     // of 256x240, we can further assume that each zone is 32x32 pixels
                     // in screen space, or 4x4 tiles. Four system palettes are allocated
                     // to background rendering, so a palette can be specified using just
                     // 2 bits. The attribute byte therefore can specify 4 distinct palettes.
                     // Therefore we can even further assume that a single palette is
                     // applied to a 2x2 tile combination of the 4x4 tile zone. The very fact
                     // that background tiles "share" a palette locally is the reason why
                     // in some games you see distortion in the colours at screen edges.

                     // As before when choosing the tile ID, we can use the bottom 12 bits of
                     // the loopy register, but we need to make the implementation "coarser"
                     // because instead of a specific tile, we want the attribute byte for a 
                     // group of 4x4 tiles, or in other words, we divide our 32x32 address
                     // by 4 to give us an equivalent 8x8 address, and we offset this address
                     // into the attribute section of the target nametable.

                     // Reconstruct the 12 bit loopy address into an offset into the
                     // attribute memory

                     // "(vram_addr.coarse_x >> 2)"        : integer divide coarse x by 4, 
                     //                                      from 5 bits to 3 bits
                     // "((vram_addr.coarse_y >> 2) << 3)" : integer divide coarse y by 4, 
                     //                                      from 5 bits to 3 bits,
                     //                                      shift to make room for coarse x

                     // Result so far: YX00 00yy yxxx

                     // All attribute memory begins at 0x03C0 within a nametable, so OR with
                     // result to select target nametable, and attribute byte offset. Finally
                     // OR with 0x2000 to offset into nametable address space on PPU bus.				
                     bgNextTileAttrib = PpuRead((ushort)(0x23C0 | (vramAddr.NametableY << 11) 
                                                         | (vramAddr.NametableY << 10) 
                                                         | ((vramAddr.CoarseY >> 2) << 3) 
                                                         | (vramAddr.CoarseX >> 2)));

                     // Right we've read the correct attribute byte for a specified address,
                     // but the byte itself is broken down further into the 2x2 tile groups
                     // in the 4x4 attribute zone.

                     // The attribute byte is assembled thus: BR(76) BL(54) TR(32) TL(10)
                     //
                     // +----+----+			    +----+----+
                     // | TL | TR |			    | ID | ID |
                     // +----+----+ where TL =   +----+----+
                     // | BL | BR |			    | ID | ID |
                     // +----+----+			    +----+----+
                     //
                     // Since we know we can access a tile directly from the 12 bit address, we
                     // can analyse the bottom bits of the coarse coordinates to provide us with
                     // the correct offset into the 8-bit word, to yield the 2 bits we are
                     // actually interested in which specifies the palette for the 2x2 group of
                     // tiles. We know if "coarse y % 4" < 2 we are in the top half else bottom half.
                     // Likewise if "coarse x % 4" < 2 we are in the left half else right half.
                     // Ultimately we want the bottom two bits of our attribute word to be the
                     // palette selected. So shift as required...				
                     if ((vramAddr.CoarseY & 0x02) != 0) bgNextTileAttrib >>= 4;
                     if ((vramAddr.CoarseX & 0x02) != 0) bgNextTileAttrib >>= 2;
                     bgNextTileAttrib &= 0x03;
                     break;

                     // Compared to the last two, the next two are the easy ones... :P

                  case 4: 
                     // Fetch the next background tile LSB bit plane from the pattern memory
                     // The Tile ID has been read from the nametable. We will use this id to 
                     // index into the pattern memory to find the correct sprite (assuming
                     // the sprites lie on 8x8 pixel boundaries in that memory, which they do
                     // even though 8x16 sprites exist, as background tiles are always 8x8).
                     //
                     // Since the sprites are effectively 1 bit deep, but 8 pixels wide, we 
                     // can represent a whole sprite row as a single byte, so offsetting
                     // into the pattern memory is easy. In total there is 8KB so we need a 
                     // 13 bit address.

                     // "(control.pattern_background << 12)"  : the pattern memory selector 
                     //                                         from control register, either 0K
                     //                                         or 4K offset
                     // "((uint16_t)bg_next_tile_id << 4)"    : the tile id multiplied by 16, as
                     //                                         2 lots of 8 rows of 8 bit pixels
                     // "(vram_addr.fine_y)"                  : Offset into which row based on
                     //                                         vertical scroll offset
                     // "+ 0"                                 : Mental clarity for plane offset
                     // Note: No PPU address bus offset required as it starts at 0x0000
                     bgNextTileLsb = PpuRead((ushort)(control.reg << 12 + bgNextTileId << 4 + vramAddr.FineY + 0));

                     break;
                  case 6:
                     // Fetch the next background tile MSB bit plane from the pattern memory
                     // This is the same as above, but has a +8 offset to select the next bit plane
                     bgNextTileMsb = PpuRead((ushort)(control.reg << 12 + bgNextTileId << 4 + vramAddr.FineY + 8));
                     break;
                  case 7:
                     // Increment the background tile "pointer" to the next tile horizontally
                     // in the nametable memory. Note this may cross nametable boundaries which
                     // is a little complex, but essential to implement scrolling
                     IncrementScrollX();
                     break;
               }
            }

            // End of a visible scanline, so increment downwards...
            if (cycle == 256)
            {
               IncrementScrollY();
            }

            //...and reset the x position
            if (cycle == 257)
            {
               LoadBackgroundShifters();
               TransferAddressX();
            }

            // Superfluous reads of tile id at end of scanline
            if (cycle == 338 || cycle == 340)
            {
               bgNextTileId = PpuRead((ushort)(0x2000 | (vramAddr.reg & 0x0FFF)));
            }

            if (scanline == -1 && cycle >= 280 && cycle < 305)
            {
               // End of vertical blank period so reset the Y address ready for rendering
               TransferAddressY();
            }
         }

         if (scanline == 240)
         {
            // Post Render Scanline - Do Nothing!
         }        

         if (scanline >= 241 && scanline < 261)
         {
            if (scanline == 241 && cycle == 1)
            {
               // Effectively end of frame, so set vertical blank flag
               status.VerticalBlank = 1;

               // If the control register tells us to emit a NMI when
               // entering vertical blanking period, do it! The CPU
               // will be informed that rendering is complete so it can
               // perform operations with the PPU knowing it wont
               // produce visible artefacts
               if (control.EnableNMI>0) 
                  Nmi = true;
            }
         }

         byte bg_pixel = 0x00;
         byte bg_palette = 0x00;

         if (mask.RenderBackground)
         {
            ushort bit_mux = (ushort)(0x8000 >> fineX);
            byte p0_pixel = (byte)((bgShifterPatternLo & bit_mux) > 0 ? 1 : 0);
            byte p1_pixel = (byte)((bgShifterPatternHi & bit_mux) > 0 ? 1 : 0);
            bg_pixel = (byte)((p1_pixel << 1) | p0_pixel);

            byte bg_pal0 = (byte)((bgShifterAttribLo & bit_mux) > 0 ? 1 : 0);
            byte bg_pal1 = (byte)((bgShifterAttribHi & bit_mux) > 0 ? 1 : 0);
            bg_palette = (byte)((bg_pal1 << 1) | bg_pal0);
         }

         // we need to offset the co-ordinates by one because SFML 
         // images cannot be written to in negative x and y values
         //
         int imageOffset = 1;

         sprImage.SetPixel((uint)(cycle-1+imageOffset), (uint)(scanline+imageOffset), GetColourFromPaletteRam(bg_palette, bg_pixel));

         // Fake some noise for now
         //sprImage.SetPixel((uint)((cycle-1)+imageOffset), (uint)(scanline+imageOffset), new Color((byte)(cycle % 255), (byte)(cycle % 255), (byte)((cycle + scanline) % 255)));

         cycle++;
         if (cycle >= 341)
         {
            cycle = 0;
            scanline++;
            if (scanline >= 261)
            {
               scanline = -1;
               sprTexture.Update(sprImage);
               addrOffset = 0;
               FrameComplete = true;
            }
         }
      }
      

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

   }
}