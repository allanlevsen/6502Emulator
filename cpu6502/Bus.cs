using cpu6502.Interfaces;

namespace cpu6502
{
    using static Constants;

    public class Bus : IBus
	{

        public byte[] ram;

		public Bus()
		{
            // todo : associate the bus to the CPU in the CPU constructor
            //
            // Connect CPU to communication bus
            // cpu.ConnectBus(this);
            //
            
            ram = new byte[TOTAL_RAM+1];
            ClearMemory();      
        }

        private void ClearMemory()
        {
            for (int i = 0; i < TOTAL_RAM; i++) ram[i] = 0x00;
        }

        public byte Read(ushort addr, bool readOnly = false)
        {
            if (addr >= 0x0000 && addr <= 0xFFFF)
                return ram[addr];

            return 0x00;
        }

        public void Write(ushort addr, byte data)
        {
            if (addr >= 0x0000 && addr <= 0xFFFF)
                ram[addr] = data;
        }
    }

}

