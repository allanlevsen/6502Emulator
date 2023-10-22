namespace cpu6502.Interfaces
{
    public interface IBus
	{
        byte Read(ushort addr, bool readOnly = false);
        void Write(ushort addr, byte data);
	}
}

