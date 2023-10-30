namespace cpu6502.Interfaces
{
   public interface IPpu
   {
      void ConnectCartridge(Cartridge cartridge);
      void Clock();
      void Reset();
   }
}