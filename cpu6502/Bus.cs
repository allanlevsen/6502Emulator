using cpu6502.Interfaces;

namespace cpu6502
{
    using static Constants;

    public class Bus : IBus
	{

        public byte[] ram;

//////////////////////////////////////////
///
/// Devices connected to the bus
/// 

        // The 6502 processor
        // the cpu instantiates the Bus
        // only the cpu writes to the bus... but, we will 
        // we will store a reference back to the cpu to make things simpler

        public Cpu cpu;	

        // The 2C02 Picture Processing Unit
        public Ppu ppu;

        // The Cartridge or "GamePak"
        public Cartridge cart;
        // 2KB of RAM
        public byte[] cpuRam = new byte[2048]; // 2048 bytes
        // Controllers
        public byte[] controller; // 2 bytes

        // A count of how many clocks have passed
        uint nSystemClockCounter = 0;
        // Internal cache of controller state
        byte[] controller_state = new byte[2]; // 2 bytes;

        // A simple form of Direct Memory Access is used to swiftly
        // transfer data from CPU bus memory into the OAM memory. It would
        // take too long to sensibly do this manually using a CPU loop, so
        // the program prepares a page of memory with the sprite info required
        // for the next frame and initiates a DMA transfer. This suspends the
        // CPU momentarily while the PPU gets sent data at PPU clock speeds.
        // Note here, that dma_page and dma_addr form a 16-bit address in 
        // the CPU bus address space
        byte dma_page = 0x00;
        byte dma_addr = 0x00;
        byte dma_data = 0x00;

        // DMA transfers need to be timed accurately. In principle it takes
        // 512 cycles to read and write the 256 bytes of the OAM memory, a
        // read followed by a write. However, the CPU needs to be on an "even"
        // clock cycle, so a dummy cycle of idleness may be required
        bool dma_dummy = true;

        // Finally a flag to indicate that a DMA transfer is happening
        bool dma_transfer = false;

		public Bus(Cpu cpu, Ppu ppu)
		{
            this.cpu = cpu;
            this.ppu = ppu;
            
            // version 1 stuff
            ram = new byte[TOTAL_RAM+1];
            ClearMemory();      
        }

        private void ClearMemory()
        {
            for (int i = 0; i < TOTAL_RAM; i++) ram[i] = 0x00;
        }

        // public byte Read(ushort addr, bool readOnly = false)
        // {
        //     if (addr >= 0x0000 && addr <= 0xFFFF)
        //         return ram[addr];

        //     return 0x00;
        // }

        // public void Write(ushort addr, byte data)
        // {
        //     if (addr >= 0x0000 && addr <= 0xFFFF)
        //         ram[addr] = data;
        // }
        public void cpuWrite(ushort addr, byte data)
        {	
            if (cart.CpuWrite(addr, data))
            {
                // The cartridge "sees all" and has the facility to veto
                // the propagation of the bus transaction if it requires.
                // This allows the cartridge to map any address to some
                // other data, including the facility to divert transactions
                // with other physical devices. The NES does not do this
                // but I figured it might be quite a flexible way of adding
                // "custom" hardware to the NES in the future!
            }
            else if (addr >= 0x0000 && addr <= 0x1FFF)
            {
                // System RAM Address Range. The range covers 8KB, though
                // there is only 2KB available. That 2KB is "mirrored"
                // through this address range. Using bitwise AND to mask
                // the bottom 11 bits is the same as addr % 2048.
                cpuRam[addr & 0x07FF] = data;

            }
            else if (addr >= 0x2000 && addr <= 0x3FFF)
            {
                // PPU Address range. The PPU only has 8 primary registers
                // and these are repeated throughout this range. We can
                // use bitwise AND operation to mask the bottom 3 bits, 
                // which is the equivalent of addr % 8.
                ppu.CpuWrite((ushort)(addr & (ushort)0x0007), data);
            }	
            else if (addr == 0x4014)
            {
                // A write to this address initiates a DMA transfer
                dma_page = data;
                dma_addr = 0x00;
                dma_transfer = true;						
            }
            else if (addr >= 0x4016 && addr <= 0x4017)
            {
                // "Lock In" controller state at this time
                controller_state[addr & 0x0001] = controller[addr & 0x0001];
            }
            
        }

        public byte cpuRead(ushort addr, bool bReadOnly)
        {
            byte data = 0x00;	
            if (cart.CpuRead(addr, out data))
            {
                // Cartridge Address Range
            }
            else if (addr >= 0x0000 && addr <= 0x1FFF)
            {
                // System RAM Address Range, mirrored every 2048
                data = cpuRam[addr & 0x07FF];
            }
            else if (addr >= 0x2000 && addr <= 0x3FFF)
            {
                // PPU Address range, mirrored every 8
                data = ppu.CpuRead((ushort)(addr & (ushort)0x0007), bReadOnly);
            }
            else if (addr >= 0x4016 && addr <= 0x4017)
            {
                // Read out the MSB of the controller status word
                // data = (controller_state[addr & 0x0001] & 0x80) > 0;
                data = (byte)((controller_state[addr & 0x0001] & 0x80) > 0 ? 1 : 0);
                controller_state[addr & 0x0001] <<= 1;
            }

            return data;
        }

        public void insertCartridge(Cartridge cartridge)
        {
            // Connects cartridge to both Main Bus and CPU Bus
            cart = cartridge;
            ppu.ConnectCartridge(cartridge);

        }

        public void reset()
        {
            cart.Reset();
            cpu.reset();
            ppu.Reset();
            nSystemClockCounter = 0;
            dma_page = 0x00;
            dma_addr = 0x00;
            dma_data = 0x00;
            dma_dummy = true;
            dma_transfer = false;
        }

        public void clock()
        {
            // Clocking. The heart and soul of an emulator. The running
            // frequency is controlled by whatever calls this function.
            // So here we "divide" the clock as necessary and call
            // the peripheral devices clock() function at the correct
            // times.

            // The fastest clock frequency the digital system cares
            // about is equivalent to the PPU clock. So the PPU is clocked
            // each time this function is called.
            ppu.Clock();

            // The CPU runs 3 times slower than the PPU so we only call its
            // clock() function every 3 times this function is called. We
            // have a global counter to keep track of this.
            if (nSystemClockCounter % 3 == 0)
            {
                // Is the system performing a DMA transfer form CPU memory to 
                // OAM memory on PPU?...
                if (dma_transfer)
                {
                    // ...Yes! We need to wait until the next even CPU clock cycle
                    // before it starts...
                    if (dma_dummy)
                    {
                        // ...So hang around in here each clock until 1 or 2 cycles
                        // have elapsed...
                        if (nSystemClockCounter % 2 == 1)
                        {
                            // ...and finally allow DMA to start
                            dma_dummy = false;
                        }
                    }
                    else
                    {
                        // DMA can take place!
                        if (nSystemClockCounter % 2 == 0)
                        {
                            // On even clock cycles, read from CPU bus
                            //dma_data = cpuRead(dma_page << 8 | dma_addr);
                            dma_data = cpuRead((ushort)(((ushort)dma_page << (ushort)8) | (ushort)dma_addr), false);
                        }
                        else
                        {
                            // On odd clock cycles, write to PPU OAM
                            ppu.POAM[dma_addr] = dma_data;
                            // Increment the lo byte of the address
                            dma_addr++;
                            // If this wraps around, we know that 256
                            // bytes have been written, so end the DMA
                            // transfer, and proceed as normal
                            if (dma_addr == 0x00)
                            {
                                dma_transfer = false;
                                dma_dummy = true;
                            }
                        }
                    }
                }
                else
                {
                    // No DMA happening, the CPU is in control of its
                    // own destiny. Go forth my friend and calculate
                    // awesomeness for many generations to come...
                    cpu.clock();
                }		
            }

            // The PPU is capable of emitting an interrupt to indicate the
            // vertical blanking period has been entered. If it has, we need
            // to send that irq to the CPU.
            if (ppu.Nmi)
            {
                ppu.Nmi = false;
                cpu.nmi();
            }

            nSystemClockCounter++;
        }
    }
}

