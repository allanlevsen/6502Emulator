using System.IO;
using cpu6502.Interfaces;

namespace cpu6502
{
   public class Cartridge
   {
      public enum MIRROR
      {
         HORIZONTAL,
         VERTICAL,
         ONESCREEN_LO,
         ONESCREEN_HI,
      } 
      
      public MIRROR mirror = MIRROR.HORIZONTAL;

      private struct Header
      {
         public char[] name;
         public byte prg_rom_chunks;
         public byte chr_rom_chunks;
         public byte mapper1;
         public byte mapper2;
         public byte prg_ram_size;
         public byte tv_system1;
         public byte tv_system2;
         public char[] unused;
      }

      private bool bImageValid;
      private IMapper pMapper;
      private byte nPRGBanks;
      private byte nCHRBanks;
      private List<byte> vPRGMemory;
      private List<byte> vCHRMemory;
      private int nMapperID;

      public Cartridge(string sFileName)
      {
         bImageValid = false;

         using (FileStream fs = new FileStream(sFileName, FileMode.Open, FileAccess.Read))
         using (BinaryReader reader = new BinaryReader(fs))
         {
                Header header = new Header
                {
                    name = reader.ReadChars(4),
                    prg_rom_chunks = reader.ReadByte(),
                    chr_rom_chunks = reader.ReadByte(),
                    mapper1 = reader.ReadByte(),
                    mapper2 = reader.ReadByte(),
                    prg_ram_size = reader.ReadByte(),
                    tv_system1 = reader.ReadByte(),
                    tv_system2 = reader.ReadByte(),
                    unused = reader.ReadChars(5)
                };

                if ((header.mapper1 & 0x04) != 0)
                  reader.BaseStream.Seek(512, SeekOrigin.Current);

               nMapperID = ((header.mapper2 >> 4) << 4) | (header.mapper1 >> 4);

               byte nFileType = 1;

               if (nFileType == 1)
               {
                  nPRGBanks = header.prg_rom_chunks;
                  vPRGMemory = new List<byte>(nPRGBanks * 16384);
                  vPRGMemory.AddRange(reader.ReadBytes(nPRGBanks * 16384));

                  nCHRBanks = header.chr_rom_chunks;

                  if (nCHRBanks == 0)
                  {
                     vCHRMemory = new List<byte>(8192);
                  }
                  else
                  {
                     vCHRMemory = new List<byte>(nCHRBanks * 8192);
                     vCHRMemory.AddRange(reader.ReadBytes(nCHRBanks * 8192));
                  }
               }

               switch (nMapperID)
               {
                  case 0: pMapper = new Mapper_000(nPRGBanks, nCHRBanks); break;
                  // Add other cases for other mappers as needed
               }

               bImageValid = true;
         }
      }

      public bool ImageValid() => bImageValid;

      public bool CpuRead(ushort addr, out byte data)
      {
         uint mapped_addr = 0;
         if (pMapper.CpuMapRead(addr, out mapped_addr))
         {
               data = vPRGMemory[(int)mapped_addr];
               return true;
         }
         else
         {
               data = 0;
               return false;
         }
      }

      public bool CpuWrite(ushort addr, byte data)
      {
         uint mapped_addr = 0;
         if (pMapper.CpuMapWrite(addr, out mapped_addr, data))
         {
               vPRGMemory[(int)mapped_addr] = data;
               return true;
         }
         else
         {
               return false;
         }
      }

      public bool PpuRead(ushort addr, out byte data)
      {
         uint mapped_addr = 0;
         if (pMapper.PpuMapRead(addr, out mapped_addr))
         {
               data = vCHRMemory[(int)mapped_addr];
               return true;
         }
         else
         {
               data = 0;
               return false;
         }
      }

      public bool PpuWrite(ushort addr, byte data)
      {
         uint mapped_addr = 0;
         if (pMapper.PpuMapWrite(addr, out mapped_addr))
         {
               vCHRMemory[(int)mapped_addr] = data;
               return true;
         }
         else
         {
               return false;
         }
      }

      public void Reset()
      {
         pMapper?.Reset();
      }
   }
}