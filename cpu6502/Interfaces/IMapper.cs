namespace cpu6502.Interfaces
{
   public interface IMapper
   {
      bool CpuMapRead(ushort addr, out uint mapped_addr);
      bool CpuMapWrite(ushort addr, out uint mapped_addr, byte data = 0);
      bool PpuMapRead(ushort addr, out uint mapped_addr);
      bool PpuMapWrite(ushort addr, out uint mapped_addr);
      void Reset();
   }
}