namespace cpu6502.Interfaces
{
    public interface IBus
	{
        void insertCartridge(Cartridge cartridge);
        void reset();
        void clock();
	}
}

