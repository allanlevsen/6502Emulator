using cpu6502.Interfaces;

namespace cpu6502
{
   public abstract class Mapper
   {
      protected byte nPRGBanks = 0;
      protected byte nCHRBanks = 0;

      public Mapper(byte prgBanks, byte chrBanks)
      {
         nPRGBanks = prgBanks;
         nCHRBanks = chrBanks;

         Reset();
      }

      // Destructor in C# is represented by a Finalizer, but it's not common to use them unless you have unmanaged resources.
      // If you do need it, you can uncomment the below:
      //~Mapper()
      //{
      //    // Cleanup statements here
      //}

      // Transform CPU bus address into PRG ROM offset
      public abstract bool CpuMapRead(ushort addr, out uint mapped_addr);
      public abstract bool CpuMapWrite(ushort addr, out uint mapped_addr, byte data = 0);

      // Transform PPU bus address into CHR ROM offset
      public abstract bool PpuMapRead(ushort addr, out uint mapped_addr);
      public abstract bool PpuMapWrite(ushort addr, out uint mapped_addr);

      public abstract void Reset();
   }
}