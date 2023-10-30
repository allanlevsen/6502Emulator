namespace cpu6502.Interfaces
{
    public interface IBus
	{
        void cpuWrite(ushort addr, byte data);
        byte cpuRead(ushort addr, bool bReadOnly);
        void insertCartridge(Cartridge cartridge);
        void reset();
        void clock();
	}
}

