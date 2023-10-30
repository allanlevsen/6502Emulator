using cpu6502.Interfaces;

namespace cpu6502
{
   public class Mapper_000 : Mapper, IMapper
   {
      public Mapper_000(byte prgBanks, byte chrBanks) : base(prgBanks, chrBanks)
      {
      }

      public override void Reset()
      {
         // Implementation here if needed
      }

      public override bool CpuMapRead(ushort addr, out uint mapped_addr)
      {
         if (addr >= 0x8000 && addr <= 0xFFFF)
         {
               mapped_addr = addr & (nPRGBanks > 1 ? 0x7FFFU : 0x3FFFU);
               return true;
         }

         mapped_addr = 0;
         return false;
      }

      public override bool CpuMapWrite(ushort addr, out uint mapped_addr, byte data = 0)
      {
         if (addr >= 0x8000 && addr <= 0xFFFF)
         {
               mapped_addr = addr & (nPRGBanks > 1 ? 0x7FFFU : 0x3FFFU);
               return true;
         }

         mapped_addr = 0;
         return false;
      }

      public override bool PpuMapRead(ushort addr, out uint mapped_addr)
      {
         if (addr >= 0x0000 && addr <= 0x1FFF)
         {
               mapped_addr = addr;
               return true;
         }

         mapped_addr = 0;
         return false;
      }

      public override bool PpuMapWrite(ushort addr, out uint mapped_addr)
      {
         if (addr >= 0x0000 && addr <= 0x1FFF)
         {
               if (nCHRBanks == 0)
               {
                  mapped_addr = addr;
                  return true;
               }
         }

         mapped_addr = 0;
         return false;
      }
   }

}