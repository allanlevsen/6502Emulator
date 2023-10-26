using System;
using System.Text;
using cpu6502.Interfaces;

namespace cpu6502
{

    public class Cpu : ICpu
	{
        public Bus bus;
        public Dictionary<byte, OpCode> ops;

        // CPU Core registers, exposed as public here for ease of access from external
        // examinors. This is all the 6502 has.
        //
        public byte a = 0x00;       // Accumulator Register
        public byte x = 0x00;       // X Register
        public byte y = 0x00;       // Y Register
        public byte stkp = 0x00;    // Stack Pointer (points to location on bus)
        public ushort pc = 0x0000;  // Program Counter
        public byte status = 0x00;  // Status Register


        // Produces a map of opcodes, with keys equivalent to instruction start locations
        // in memory, for the specified address range
        //
        public Dictionary<ushort, string> disassembledCode = new Dictionary<ushort, string>();


        // The status register stores 8 flags. Ive enumerated these here for ease
        // of access. You can access the status register directly since its public.
        // The bits have different interpretations depending upon the context and 
        // instruction being executed.
        //
        public enum Flag
        {
            C = (1 << 0),   // Carry Bit
            Z = (1 << 1),   // Zero
            I = (1 << 2),   // Disable Interrupts
            D = (1 << 3),   // Decimal Mode (unused in this implementation)
            B = (1 << 4),   // Break
            U = (1 << 5),   // Unused
            V = (1 << 6),   // Overflow
            N = (1 << 7),   // Negative
        };


        // Assisstive variables to facilitate emulation
        //
        byte fetched = 0x00;        // Represents the working input value to the ALU
        ushort temp = 0x0000;       // A convenience variable used everywhere
        ushort addr_abs = 0x0000;   // All used memory addresses end up in here
        ushort addr_rel = 0x00;     // Represents absolute address following a branch
        byte opcode = 0x00;         // Is the instruction byte
        byte cycles = 0;            // Counts how many cycles the instruction has remaining
        uint clock_count = 0;       // A global accumulation of the number of clocks

        public Cpu()
		{
			bus = new Bus();
            ops = new InstructionSet(this).Ops;
            
        }


        // This is the disassembly function. Its workings are not required for emulation.
        // It is merely a convenience function to turn the binary instruction code into
        // human readable form. Its included as part of the emulator because it can take
        // advantage of many of the CPUs internal operations to do this.
        //
        public Dictionary<ushort, string> Disassemble(ushort nStart, ushort nStop)
        {
            Dictionary<ushort, string> mapLines = new Dictionary<ushort, string>();

            ushort addr = nStart;
            byte value = 0x00, lo = 0x00, hi = 0x00;
            ushort line_addr = 0;

            // A convenient utility to convert variables into
            // hex strings because "modern C++"'s method with 
            // streams is atrocious
            Func<ushort, byte, string> hex = (n, d) =>
            {
                StringBuilder s = new StringBuilder(new string('0', d));
                for (int i = d - 1; i >= 0; i--, n >>= 4)
                    s[i] = "0123456789ABCDEF"[n & 0xF];
                return s.ToString();
            };

            // Starting at the specified address we read an instruction
            // byte, which in turn yields information from the lookup table
            // as to how many additional bytes we need to read and what the
            // addressing mode is. I need this info to assemble human readable
            // syntax, which is different depending upon the addressing mode

            // As the instruction is decoded, a std::string is assembled
            // with the readable output
            while (addr <= (uint)nStop)
            {
                line_addr = addr;

                // Prefix line with instruction address
                string sInst = "$" + hex(addr, 4) + ": ";

                // Read instruction, and get its readable name
                byte opcode = bus.Read(addr, true); addr++;
                sInst += ops[opcode].name + " ";

                // Get oprands from desired locations, and form the
                // instruction based upon its addressing mode. These
                // routines mimmick the actual fetch routine of the
                // 6502 in order to get accurate data as part of the
                // instruction

                if (ops[opcode].am == this.IMP)
                {
                    sInst += " {IMP}";
                }
                else if (ops[opcode].am == this.IMM)
                {
                    value = bus.Read(addr, true); addr++;
                    sInst += "#$" + hex(value, 2) + " {IMM}";
                }
                else if (ops[opcode].am == ZP0)
                {
                    lo = bus.Read(addr, true); addr++;
                    hi = 0x00;
                    sInst += "$" + hex(lo, 2) + " {ZP0}";
                }
                else if (ops[opcode].am == ZPX)
                {
                    lo = bus.Read(addr, true); addr++;
                    hi = 0x00;
                    sInst += "$" + hex(lo, 2) + ", X {ZPX}";
                }
                else if (ops[opcode].am == ZPY)
                {
                    lo = bus.Read(addr, true); addr++;
                    hi = 0x00;
                    sInst += "$" + hex(lo, 2) + ", Y {ZPY}";
                }
                else if (ops[opcode].am == IZX)
                {
                    lo = bus.Read(addr, true); addr++;
                    hi = 0x00;
                    sInst += "($" + hex(lo, 2) + ", X) {IZX}";
                }
                else if (ops[opcode].am == IZY)
                {
                    lo = bus.Read(addr, true); addr++;
                    hi = 0x00;
                    sInst += "($" + hex(lo, 2) + "), Y {IZY}";
                }
                else if (ops[opcode].am == ABS)
                {
                    lo = bus.Read(addr, true); addr++;
                    hi = bus.Read(addr, true); addr++;
                    sInst += "$" + hex((ushort)((hi << 8) | lo), 4) + " {ABS}";
                }
                else if (ops[opcode].am == ABX)
                {
                    lo = bus.Read(addr, true); addr++;
                    hi = bus.Read(addr, true); addr++;
                    sInst += "$" + hex((ushort)((hi << 8) | lo), 4) + ", X {ABX}";
                }
                else if (ops[opcode].am == ABY)
                {
                    lo = bus.Read(addr, true); addr++;
                    hi = bus.Read(addr, true); addr++;
                    sInst += "$" + hex((ushort)((hi << 8) | lo), 4) + ", Y {ABY}";
                }
                else if (ops[opcode].am == IND)
                {
                    lo = bus.Read(addr, true); addr++;
                    hi = bus.Read(addr, true); addr++;
                    sInst += "($" + hex((ushort)((hi << 8) | lo), 4) + ") {IND}";
                }
                else if (ops[opcode].am == REL)
                {
                    value = bus.Read(addr, true); addr++;
                    sInst += "$" + hex(value, 2) + " [$" + hex((ushort)(addr + value), 4) + "] {REL}";
                }

                // Add the formed string to a std::map, using the instruction's
                // address as the key. This makes it convenient to look for later
                // as the instructions are variable in length, so a straight up
                // incremental index is not sufficient.
                mapLines.Add(line_addr, sInst);
            }

            return mapLines;
        }

        ///////////////////////////////////////////////////////////////////////////////
        // EXTERNAL INPUTS

        // Forces the 6502 into a known state. This is hard-wired inside the CPU. The
        // registers are set to 0x00, the status register is cleared except for unused
        // bit which remains at 1. An absolute address is read from location 0xFFFC
        // which contains a second address that the program counter is set to. This 
        // allows the programmer to jump to a known and programmable location in the
        // memory to start executing from. Typically the programmer would set the value
        // at location 0xFFFC at compile time.
        //
        public void reset()
        {
            // Get address to set program counter to
            addr_abs = 0xFFFC;

            ushort hi_addr = (ushort)(addr_abs + 1);
            ushort hi = Read(hi_addr);

            ushort lo_addr = (ushort)(addr_abs + 0);
            ushort lo = Read(lo_addr);

            // Set it
            pc = (ushort)((hi << 8) | lo);

            // Reset internal registers
            a = 0;
            x = 0;
            y = 0;
            stkp = 0xFD;
            status = (byte)(0x00 | Flag.U);

            // Clear internal helper variables
            addr_rel = 0x0000;
            addr_abs = 0x0000;
            fetched = 0x00;

            // Reset takes time
            cycles = 8;
        }


        // Interrupt requests are a complex operation and only happen if the
        // "disable interrupt" flag is 0. IRQs can happen at any time, but
        // you dont want them to be destructive to the operation of the running 
        // program. Therefore the current instruction is allowed to finish
        // (which I facilitate by doing the whole thing when cycles == 0) and 
        // then the current program counter is stored on the stack. Then the
        // current status register is stored on the stack. When the routine
        // that services the interrupt has finished, the status register
        // and program counter can be restored to how they where before it 
        // occurred. This is impemented by the "RTI" instruction. Once the IRQ
        // has happened, in a similar way to a reset, a programmable address
        // is read form hard coded location 0xFFFE, which is subsequently
        // set to the program counter.
        //
        public void irq()
        {
            // If interrupts are allowed
            if (GetFlag(Flag.I) == 0)
            {
                // Push the program counter to the stack. It's 16-bits dont
                // forget so that takes two pushes
                Write((ushort)(0x0100 + stkp), (byte)((pc >> 8) & 0x00FF));
                stkp--;
                Write((ushort)(0x0100 + stkp), (byte)(pc & 0x00FF));
                stkp--;

                // Then Push the status register to the stack
                SetFlag(Flag.B, false);
                SetFlag(Flag.U, true);
                SetFlag(Flag.I, true);
                Write((ushort)(0x0100 + stkp), status);
                stkp--;

                // Read new program counter location from fixed address
                addr_abs = 0xFFFE;
                ushort lo = Read((ushort)(addr_abs + 0));
                ushort hi = Read((ushort)(addr_abs + 1));
                pc = (ushort)((hi << 8) | lo);

                // IRQs take time
                cycles = 7;
            }
        }


        // A Non-Maskable Interrupt cannot be ignored. It behaves in exactly the
        // same way as a regular IRQ, but reads the new program counter address
        // form location 0xFFFA.
        //
        public void nmi()
        {
            // Push the program counter to the stack. It's 16-bits dont
            // forget so that takes two pushes
            Write((ushort)(0x0100 + stkp), (byte)((pc >> 8) & 0x00FF));
            stkp--;
            Write((ushort)(0x0100 + stkp), (byte)(pc & 0x00FF));
            stkp--;

            // Then Push the status register to the stack
            SetFlag(Flag.B, false);
            SetFlag(Flag.U, true);
            SetFlag(Flag.I, true);
            Write((ushort)(0x0100 + stkp), status);
            stkp--;

            // Read new program counter location from fixed address
            addr_abs = 0xFFFA;
            ushort lo = Read((ushort)(addr_abs + 0));
            ushort hi = Read((ushort)(addr_abs + 1));
            pc = (ushort)((hi << 8) | lo);

            cycles = 8;
        }

        // Perform one clock cycles worth of emulation
        //
        public void clock()
        {
            // Each instruction requires a variable number of clock cycles to execute.
            // In my emulation, I only care about the final result and so I perform
            // the entire computation in one hit. In hardware, each clock cycle would
            // perform "microcode" style transformations of the CPUs state.
            //
            // To remain compliant with connected devices, it's important that the 
            // emulation also takes "time" in order to execute instructions, so I
            // implement that delay by simply counting down the cycles required by 
            // the instruction. When it reaches 0, the instruction is complete, and
            // the next one is ready to be executed.
            //
            if (cycles == 0)
            {
                // Read next instruction byte. This 8-bit value is used to index
                // the translation table to get the relevant information about
                // how to implement the instruction
                opcode = Read(pc);

                // Always set the unused status flag bit to 1
                SetFlag(Flag.U, true);

                // Increment program counter, we read the opcode byte
                pc++;

                // Get Starting number of cycles
                cycles = ops[opcode].cycles;

                // Perform fetch of intermmediate data using the
                // required addressing mode
                byte additional_cycle1 = ops[opcode].am();

                // Perform operation
                byte additional_cycle2 = ops[opcode].op();

                // The addressmode and opcode may have altered the number
                // of cycles this instruction requires before its completed
                cycles += (byte)(additional_cycle1 & additional_cycle2);

                // Always set the unused status flag bit to 1
                SetFlag(Flag.U, true);

            }

            // Increment global clock count - This is actually unused unless logging is enabled
            // but I've kept it in because its a handy watch variable for debugging
            clock_count++;

            // Decrement the number of cycles remaining for this instruction
            cycles--;
        }



        ///////////////////////////////////////////////////////////////////////////////
        // FLAG FUNCTIONS

        // Returns the value of a specific bit of the status register
        //
        public byte GetFlag(Flag f)
        {
            byte flag = (byte)f;
            return ((status & flag) > 0) ? (byte)1 : (byte)0;
        }

        // Sets or clears a specific bit of the status register
        //
        public void SetFlag(Flag f, bool v)
        {
            byte flag = (byte)f;
            if (v)
                status |= flag;
            else
                status &= (byte)~flag;
        }




        // Indicates the current instruction has completed by returning true. This is
        // a utility function to enable "step-by-step" execution, without manually 
        // clocking every cycle
        //
        public bool complete()
        {
            return cycles == 0;
        }





        ///////////////////////////////////////////////////////////////////////////////
        //
        // BUS CONNECTIVITY
        //

        // Reads an 8-bit byte from the bus, located at the specified 16-bit address
        //
        byte Read(ushort a)
        {
            // In normal operation "read only" is set to false. This may seem odd. Some
            // devices on the bus may change state when they are read from, and this 
            // is intentional under normal circumstances. However the disassembler will
            // want to read the data at an address without changing the state of the
            // devices on the bus
            return bus.Read(a, false);
        }

        // Writes a byte to the bus at the specified address
        //
        void Write(ushort a, byte d)
        {
            bus.Write(a, d);
        }



        ///////////////////////////////////////////////////////////////////////////////
        //
        // ADDRESSING MODES
        //

        // The 6502 can address between 0x0000 - 0xFFFF. The high byte is often referred
        // to as the "page", and the low byte is the offset into that page. This implies
        // there are 256 pages, each containing 256 bytes.
        //
        // Several addressing modes have the potential to require an additional clock
        // cycle if they cross a page boundary. This is combined with several instructions
        // that enable this additional clock cycle. So each addressing function returns
        // a flag saying it has potential, as does each instruction. If both instruction
        // and address function return 1, then an additional clock cycle is required.


        // Address Mode: Implied
        // There is no additional data required for this instruction. The instruction
        // does something very simple like like sets a status bit. However, we will
        // target the accumulator, for instructions like PHA
        //
        public byte IMP()
        {
            fetched = a;
            return 0;
        }


        // Address Mode: Immediate
        // The instruction expects the next byte to be used as a value, so we'll prep
        // the read address to point to the next byte
        //
        public byte IMM()
        {
            addr_abs = pc++;
            return 0;
        }



        // Address Mode: Zero Page
        // To save program bytes, zero page addressing allows you to absolutely address
        // a location in first 0xFF bytes of address range. Clearly this only requires
        // one byte instead of the usual two.
        //
        public byte ZP0()
        {
            addr_abs = Read(pc);
            pc++;
            addr_abs &= 0x00FF;
            return 0;
        }



        // Address Mode: Zero Page with X Offset
        // Fundamentally the same as Zero Page addressing, but the contents of the X Register
        // is added to the supplied single byte address. This is useful for iterating through
        // ranges within the first page.
        //
        public byte ZPX()
        {
            addr_abs = (ushort)(Read(pc) + x);
            pc++;
            addr_abs &= 0x00FF;
            return 0;
        }


        // Address Mode: Zero Page with Y Offset
        // Same as above but uses Y Register for offset
        //
        public byte ZPY()
        {
            addr_abs = (ushort)(Read(pc) + y);
            pc++;
            addr_abs &= 0x00FF;
            return 0;
        }


        // Address Mode: Relative
        // This address mode is exclusive to branch instructions. The address
        // must reside within -128 to +127 of the branch instruction, i.e.
        // you cant directly branch to any address in the addressable range.
        //
        public byte REL()
        {
            addr_rel = Read(pc);
            pc++;
            if ((addr_rel & 0x80) != 0)
                addr_rel |= 0xFF00;
            return 0;
        }


        // Address Mode: Absolute 
        // A full 16-bit address is loaded and used
        //
        public byte ABS()
        {
            ushort lo = Read(pc);
            pc++;
            ushort hi = Read(pc);
            pc++;
            addr_abs = (ushort)((hi << 8) | lo);
            return 0;
        }


        // Address Mode: Absolute with X Offset
        // Fundamentally the same as absolute addressing, but the contents of the X Register
        // is added to the supplied two byte address. If the resulting address changes
        // the page, an additional clock cycle is required
        //
        public byte ABX()
        {
            ushort lo = Read(pc);
            pc++;
            ushort hi = Read(pc);
            pc++;
            addr_abs = (ushort)((hi << 8) | lo);
            addr_abs += x;

            if ((addr_abs & 0xFF00) != (hi << 8))
                return 1;
            else
                return 0;
        }


        // Address Mode: Absolute with Y Offset
        // Fundamentally the same as absolute addressing, but the contents of the Y Register
        // is added to the supplied two byte address. If the resulting address changes
        // the page, an additional clock cycle is required
        //
        public byte ABY()
        {
            ushort lo = Read(pc);
            pc++;
            ushort hi = Read(pc);
            pc++;
            addr_abs = (ushort)((hi << 8) | lo);
            addr_abs += y;

            if ((addr_abs & 0xFF00) != (hi << 8))
                return 1;
            else
                return 0;
        }

        // Note: The next 3 address modes use indirection (aka Pointers!)

        // Address Mode: Indirect
        // The supplied 16-bit address is read to get the actual 16-bit address. This is
        // instruction is unusual in that it has a bug in the hardware! To emulate its
        // function accurately, we also need to emulate this bug. If the low byte of the
        // supplied address is 0xFF, then to read the high byte of the actual address
        // we need to cross a page boundary. This doesnt actually work on the chip as 
        // designed, instead it wraps back around in the same page, yielding an 
        // invalid actual address
        //
        public byte IND()
        {
            ushort ptr_lo = Read(pc);
            pc++;
            ushort ptr_hi = Read(pc);
            pc++;

            ushort ptr = (ushort)((ptr_hi << 8) | ptr_lo);

            if (ptr_lo == 0x00FF) // Simulate page boundary hardware bug
            {
                ushort hi_addr = (ushort)(ptr & 0xFF00);
                ptr_hi = Read(hi_addr);

                ushort lo_addr = (ushort)(ptr + 0);
                ptr_lo = Read(lo_addr);

                addr_abs = (ushort)((ptr_hi << 8) | ptr_lo);
            }
            else // Behave normally
            {
                ushort hi_addr = (ushort)(ptr + 1);
                ptr_hi = Read(hi_addr);

                ushort lo_addr = (ushort)(ptr + 0);
                ptr_lo = Read(lo_addr);

                addr_abs = (ushort)((ptr_hi << 8) | ptr_lo);
            }

            return 0;
        }


        // Address Mode: Indirect X
        // The supplied 8-bit address is offset by X Register to index
        // a location in page 0x00. The actual 16-bit address is read 
        // from this location
        //
        public byte IZX()
        {
            ushort t = Read(pc);
            pc++;

            ushort hi_addr = (ushort)((t + (ushort)x + 1) & 0x00FF);
            ushort hi = Read(hi_addr);

            ushort lo_addr = (ushort)((t + (ushort)x) & 0x00FF);
            ushort lo = Read(lo_addr);

            addr_abs = (ushort)((hi << 8) | lo);

            return 0;
        }


        // Address Mode: Indirect Y
        // The supplied 8-bit address indexes a location in page 0x00. From 
        // here the actual 16-bit address is read, and the contents of
        // Y Register is added to it to offset it. If the offset causes a
        // change in page then an additional clock cycle is required.
        //
        public byte IZY()
        {
            ushort t = Read(pc);
            pc++;

            ushort hi_addr = (ushort)((t + 1) & 0x00FF);
            ushort hi = Read(hi_addr);

            ushort lo_addr = (ushort)(t & 0x00FF);
            ushort lo = Read(lo_addr);

            addr_abs = (ushort)((hi << 8) | lo);
            addr_abs += y;
            if ((addr_abs & 0xFF00) != (hi << 8))
                return 1;
            else
                return 0;
        }


        // This function serves to extract data required for executing an
        // instruction and stores it in a numeric variable for ease of use.
        // Some instructions do not necessitate data retrieval because the
        // source is implicit within the instruction itself.For instance,
        // the "INX" instruction solely increments the X register and requires
        // no additional data.In all other addressing modes, the necessary data
        // is located at the memory address specified in the variable "addr_abs"
        // and it is retrieved from there.The immediate addressing mode takes
        // advantage of this by setting "addr_abs" to the program counter (pc)
        // plus one, which allows it to fetch the data from the next byte.
        // For example, "LDA $FF" simply loads the accumulator with the
        // value 256, eliminating the need for extensive memory retrieval.
        // The "fetched" variable is a global variable within the CPU and is
        // both set and returned by invoking this function, providing
        // convenience in accessing the fetched data.
        //

        public byte fetch()
        {
            if (!(ops[opcode].am == IMP))
                fetched = Read(addr_abs);
            return fetched;
        }


        ///////////////////////////////////////////////////////////////////////////////
        //
        // INSTRUCTION IMPLEMENTATIONS
        //



        // Note: Ive started with the two most complicated instructions to emulate, which
        // ironically is addition and subtraction! Ive tried to include a detailed 
        // explanation as to why they are so complex, yet so fundamental. Im also NOT
        // going to do this through the explanation of 1 and 2's complement.

        // Instruction: Add with Carry In
        // Function:    A = A + M + C
        // Flags Out:   C, V, N, Z
        //
        // Explanation:
        // The purpose of this function is to add a value to the accumulator and a carry bit. If
        // the result is > 255 there is an overflow setting the carry bit. Ths allows you to
        // chain together ADC instructions to add numbers larger than 8-bits. This in itself is
        // simple, however the 6502 supports the concepts of Negativity/Positivity and Signed Overflow.
        //
        // 10000100 = 128 + 4 = 132 in normal circumstances, we know this as unsigned and it allows
        // us to represent numbers between 0 and 255 (given 8 bits). The 6502 can also interpret 
        // this word as something else if we assume those 8 bits represent the range -128 to +127,
        // i.e. it has become signed.
        //
        // Since 132 > 127, it effectively wraps around, through -128, to -124. This wraparound is
        // called overflow, and this is a useful to know as it indicates that the calculation has
        // gone outside the permissable range, and therefore no longer makes numeric sense.
        //
        // Note the implementation of ADD is the same in binary, this is just about how the numbers
        // are represented, so the word 10000100 can be both -124 and 132 depending upon the 
        // context the programming is using it in. We can prove this!
        //
        //  10000100 =  132  or  -124
        // +00010001 = + 17      + 17
        //  ========    ===       ===     See, both are valid additions, but our interpretation of
        //  10010101 =  149  or  -107     the context changes the value, not the hardware!
        //
        // In principle under the -128 to 127 range:
        // 10000000 = -128, 11111111 = -1, 00000000 = 0, 00000000 = +1, 01111111 = +127
        // therefore negative numbers have the most significant set, positive numbers do not
        //
        // To assist us, the 6502 can set the overflow flag, if the result of the addition has
        // wrapped around. V <- ~(A^M) & A^(A+M+C) :D lol, let's work out why!
        //
        // Let's suppose we have A = 30, M = 10 and C = 0
        //          A = 30 = 00011110
        //          M = 10 = 00001010+
        //     RESULT = 40 = 00101000
        //
        // Here we have not gone out of range. The resulting significant bit has not changed.
        // So let's make a truth table to understand when overflow has occurred. Here I take
        // the MSB of each component, where R is RESULT.
        //
        // A  M  R | V | A^R | A^M |~(A^M) | 
        // 0  0  0 | 0 |  0  |  0  |   1   |
        // 0  0  1 | 1 |  1  |  0  |   1   |
        // 0  1  0 | 0 |  0  |  1  |   0   |
        // 0  1  1 | 0 |  1  |  1  |   0   |  so V = ~(A^M) & (A^R)
        // 1  0  0 | 0 |  1  |  1  |   0   |
        // 1  0  1 | 0 |  0  |  1  |   0   |
        // 1  1  0 | 1 |  1  |  0  |   1   |
        // 1  1  1 | 0 |  0  |  0  |   1   |
        //
        // We can see how the above equation calculates V, based on A, M and R. V was chosen
        // based on the following hypothesis:
        //       Positive Number + Positive Number = Negative Result -> Overflow
        //       Negative Number + Negative Number = Positive Result -> Overflow
        //       Positive Number + Negative Number = Either Result -> Cannot Overflow
        //       Positive Number + Positive Number = Positive Result -> OK! No Overflow
        //       Negative Number + Negative Number = Negative Result -> OK! NO Overflow
        //

        public byte ADC()
        {
            // Grab the data that we are adding to the accumulator
            fetch();

            // Add is performed in 16-bit domain for emulation to capture any
            // carry bit, which will exist in bit 8 of the 16-bit word
            temp = (ushort)((ushort)a + (ushort)fetched + (ushort)GetFlag(Flag.C));

            // The carry flag out exists in the high byte bit 0
            SetFlag(Flag.C, temp > 255);

            // The Zero flag is set if the result is 0
            SetFlag(Flag.Z, (temp & 0x00FF) == 0);

            // The signed Overflow flag is set based on all that up there! :D
            SetFlag(Flag.V, ((~((ushort)a ^ (ushort)fetched) & ((ushort)a ^ (ushort)temp)) & 0x0080) != 0);

            // The negative flag is set to the most significant bit of the result
            SetFlag(Flag.N, (temp & 0x80) != 0);

            // Load the result into the accumulator (it's 8-bit dont forget!)
            a = (byte)(temp & 0x00FF);

            // This instruction has the potential to require an additional clock cycle
            return 1;
        }


        // Instruction: Subtraction with Borrow In
        // Function:    A = A - M - (1 - C)
        // Flags Out:   C, V, N, Z
        //
        // Explanation:
        // Given the explanation for ADC above, we can reorganise our data
        // to use the same computation for addition, for subtraction by multiplying
        // the data by -1, i.e. make it negative
        //
        // A = A - M - (1 - C)  ->  A = A + -1 * (M - (1 - C))  ->  A = A + (-M + 1 + C)
        //
        // To make a signed positive number negative, we can invert the bits and add 1
        // (OK, I lied, a little bit of 1 and 2s complement :P)
        //
        //  5 = 00000101
        // -5 = 11111010 + 00000001 = 11111011 (or 251 in our 0 to 255 range)
        //
        // The range is actually unimportant, because if I take the value 15, and add 251
        // to it, given we wrap around at 256, the result is 10, so it has effectively 
        // subtracted 5, which was the original intention. (15 + 251) % 256 = 10
        //
        // Note that the equation above used (1-C), but this got converted to + 1 + C.
        // This means we already have the +1, so all we need to do is invert the bits
        // of M, the data(!) therfore we can simply add, exactly the same way we did 
        // before.
        //

        public byte SBC()
        {
            fetch();

            // Operating in 16-bit domain to capture carry out

            // We can invert the bottom 8 bits with bitwise xor
            ushort value = (ushort)(((ushort)fetched) ^ 0x00FF);

            // Notice this is exactly the same as addition from here!
            temp = (ushort)(a + value + (ushort)GetFlag(Flag.C));
            SetFlag(Flag.C, (temp & 0xFF00) != 0);
            SetFlag(Flag.Z, ((temp & 0x00FF) == 0));
            SetFlag(Flag.V, ((temp ^ (ushort)a) & (temp ^ value) & 0x0080) != 0);
            SetFlag(Flag.N, (temp & 0x0080) != 0);
            a = (byte)(temp & 0x00FF);
            return 1;
        }


        // Instruction: Bitwise Logic AND
        // Function:    A = A & M
        // Flags Out:   N, Z
        //
        public byte AND()
        {
            fetch();
            a = (byte)(a & fetched);
            SetFlag(Flag.Z, a == 0x00);
            SetFlag(Flag.N, (a & 0x80) != 0);
            return 1;
        }


        // Instruction: Arithmetic Shift Left
        // Function:    A = C <- (A << 1) <- 0
        // Flags Out:   N, Z, C
        //
        public byte ASL()
        {
            fetch();
            temp = (ushort)(fetched << 1);
            SetFlag(Flag.C, (temp & 0xFF00) > 0);
            SetFlag(Flag.Z, (temp & 0x00FF) == 0x00);
            SetFlag(Flag.N, (temp & 0x80)!=0);
            if (ops[opcode].am == IMP)
                a = (byte)(temp & 0x00FF);
            else
                Write(addr_abs, (byte)(temp & 0x00FF));
            return 0;
        }


        // Instruction: Branch if Carry Clear
        // Function:    if(C == 0) pc = address
        //
        public byte BCC()
        {
            if (GetFlag(Flag.C) == 0)
            {
                cycles++;
                addr_abs = (ushort)(pc + addr_rel);

                if ((addr_abs & 0xFF00) != (pc & 0xFF00))
                    cycles++;

                pc = addr_abs;
            }
            return 0;
        }


        // Instruction: Branch if Carry Set
        // Function:    if(C == 1) pc = address
        //
        public byte BCS()
        {
            if (GetFlag(Flag.C) == 1)
            {
                cycles++;
                addr_abs = (byte)(pc + addr_rel);

                if ((addr_abs & 0xFF00) != (pc & 0xFF00))
                    cycles++;

                pc = addr_abs;
            }
            return 0;
        }


        // Instruction: Branch if Equal
        // Function:    if(Z == 1) pc = address
        //
        public byte BEQ()
        {
            if (GetFlag(Flag.Z) == 1)
            {
                cycles++;
                addr_abs = (byte)(pc + addr_rel);

                if ((addr_abs & 0xFF00) != (pc & 0xFF00))
                    cycles++;

                pc = addr_abs;
            }
            return 0;
        }

        public byte BIT()
        {
            fetch();
            temp = (ushort)(a & fetched);
            SetFlag(Flag.Z, (temp & 0x00FF) == 0x00);
            SetFlag(Flag.N, (fetched & (1 << 7)) != 0);
            SetFlag(Flag.V, (fetched & (1 << 6)) != 0);
            return 0;
        }


        // Instruction: Branch if Negative
        // Function:    if(N == 1) pc = address
        //
        public byte BMI()
        {
            if (GetFlag(Flag.N) == 1)
            {
                cycles++;
                addr_abs = (ushort)(pc + addr_rel);

                if ((addr_abs & 0xFF00) != (pc & 0xFF00))
                    cycles++;

                pc = addr_abs;
            }
            return 0;
        }


        // Instruction: Branch if Not Equal
        // Function:    if(Z == 0) pc = address
        //
        public byte BNE()
        {
            if (GetFlag(Flag.Z) == 0)
            {
                cycles++;
                addr_abs = (ushort)(pc + addr_rel);

                if ((addr_abs & 0xFF00) != (pc & 0xFF00))
                    cycles++;

                pc = addr_abs;
            }
            return 0;
        }


        // Instruction: Branch if Positive
        // Function:    if(N == 0) pc = address
        //
        public byte BPL()
        {
            if (GetFlag(Flag.N) == 0)
            {
                cycles++;
                addr_abs = (ushort)(pc + addr_rel);

                if ((addr_abs & 0xFF00) != (pc & 0xFF00))
                    cycles++;

                pc = addr_abs;
            }
            return 0;
        }

        // Instruction: Break
        // Function:    Program Sourced Interrupt
        //
        public byte BRK()
        {
            pc++;

            SetFlag(Flag.I, true);
            Write((ushort)(0x0100 + stkp), (byte)((pc >> 8) & 0x00FF));
            stkp--;
            Write((ushort)(0x0100 + stkp), (byte)(pc & 0x00FF));
            stkp--;

            SetFlag(Flag.B, true);
            Write((ushort)(0x0100 + stkp), status);
            stkp--;
            SetFlag(Flag.B, false);

            ushort lo = Read((ushort)0xFFFE);
            ushort hi = Read((ushort)0xFFFF);
            pc = (ushort)((hi << 8) | lo);

            return 0;
        }


        // Instruction: Branch if Overflow Clear
        // Function:    if(V == 0) pc = address
        //
        public byte BVC()
        {
            if (GetFlag(Flag.V) == 0)
            {
                cycles++;
                addr_abs = (ushort)(pc + addr_rel);

                if ((addr_abs & 0xFF00) != (pc & 0xFF00))
                    cycles++;

                pc = addr_abs;
            }
            return 0;
        }


        // Instruction: Branch if Overflow Set
        // Function:    if(V == 1) pc = address
        //
        public byte BVS()
        {
            if (GetFlag(Flag.V) == 1)
            {
                cycles++;
                addr_abs = (ushort)(pc + addr_rel);

                if ((addr_abs & 0xFF00) != (pc & 0xFF00))
                    cycles++;

                pc = addr_abs;
            }
            return 0;
        }


        // Instruction: Clear Carry Flag
        // Function:    C = 0
        //
        public byte CLC()
        {
            SetFlag(Flag.C, false);
            return 0;
        }


        // Instruction: Clear Decimal Flag
        // Function:    D = 0
        //
        public byte CLD()
        {
            SetFlag(Flag.D, false);
            return 0;
        }


        // Instruction: Disable Interrupts / Clear Interrupt Flag
        // Function:    I = 0
        //
        public byte CLI()
        {
            SetFlag(Flag.I, false);
            return 0;
        }


        // Instruction: Clear Overflow Flag
        // Function:    V = 0
        //
        public byte CLV()
        {
            SetFlag(Flag.V, false);
            return 0;
        }

        // Instruction: Compare Accumulator
        // Function:    C <- A >= M      Z <- (A - M) == 0
        // Flags Out:   N, C, Z
        //
        public byte CMP()
        {
            fetch();
            temp = (ushort)((ushort)a - (ushort)fetched);
            SetFlag(Flag.C, a >= fetched);
            SetFlag(Flag.Z, (temp & 0x00FF) == 0x0000);
            SetFlag(Flag.N, (temp & 0x0080) != 0);
            return 1;
        }


        // Instruction: Compare X Register
        // Function:    C <- X >= M      Z <- (X - M) == 0
        // Flags Out:   N, C, Z
        //
        public byte CPX()
        {
            fetch();
            temp = (ushort)((ushort)x - (ushort)fetched);
            SetFlag(Flag.C, x >= fetched);
            SetFlag(Flag.Z, (temp & 0x00FF) == 0x0000);
            SetFlag(Flag.N, (temp & 0x0080) != 0);
            return 0;
        }


        // Instruction: Compare Y Register
        // Function:    C <- Y >= M      Z <- (Y - M) == 0
        // Flags Out:   N, C, Z
        //
        public byte CPY()
        {
            fetch();
            temp = (ushort)((ushort)y - (ushort)fetched);
            SetFlag(Flag.C, y >= fetched);
            SetFlag(Flag.Z, (temp & 0x00FF) == 0x0000);
            SetFlag(Flag.N, (temp & 0x0080) != 0);
            return 0;
        }


        // Instruction: Decrement Value at Memory Location
        // Function:    M = M - 1
        // Flags Out:   N, Z
        //
        public byte DEC()
        {
            fetch();
            temp = (ushort)(fetched - 1);
            Write(addr_abs, (byte)(temp & 0x00FF));
            SetFlag(Flag.Z, (temp & 0x00FF) == 0x0000);
            SetFlag(Flag.N, (temp & 0x0080) != 0);
            return 0;
        }


        // Instruction: Decrement X Register
        // Function:    X = X - 1
        // Flags Out:   N, Z
        //
        public byte DEX()
        {
            x--;
            SetFlag(Flag.Z, x == 0x00);
            SetFlag(Flag.N, (x & 0x80) != 0);
            return 0;
        }


        // Instruction: Decrement Y Register
        // Function:    Y = Y - 1
        // Flags Out:   N, Z
        //
        public byte DEY()
        {
            y--;
            SetFlag(Flag.Z, y == 0x00);
            SetFlag(Flag.N, (y & 0x80) != 0);
            return 0;
        }


        // Instruction: Bitwise Logic XOR
        // Function:    A = A xor M
        // Flags Out:   N, Z
        //
        public byte EOR()
        {
            fetch();
            a = (byte)(a ^ fetched);
            SetFlag(Flag.Z, a == 0x00);
            SetFlag(Flag.N, (a & 0x80) != 0);
            return 1;
        }


        // Instruction: Increment Value at Memory Location
        // Function:    M = M + 1
        // Flags Out:   N, Z
        //
        public byte INC()
        {
            fetch();
            temp = (byte)(fetched + 1);
            Write(addr_abs, (byte)(temp & 0x00FF));
            SetFlag(Flag.Z, (temp & 0x00FF) == 0x0000);
            SetFlag(Flag.N, (temp & 0x0080) != 0);
            return 0;
        }


        // Instruction: Increment X Register
        // Function:    X = X + 1
        // Flags Out:   N, Z
        //
        public byte INX()
        {
            x++;
            SetFlag(Flag.Z, x == 0x00);
            SetFlag(Flag.N, (x & 0x80) != 0);
            return 0;
        }


        // Instruction: Increment Y Register
        // Function:    Y = Y + 1
        // Flags Out:   N, Z
        //
        public byte INY()
        {
            y++;
            SetFlag(Flag.Z, y == 0x00);
            SetFlag(Flag.N, (y & 0x80) != 0);
            return 0;
        }


        // Instruction: Jump To Location
        // Function:    pc = address
        //
        public byte JMP()
        {
            pc = addr_abs;
            return 0;
        }


        // Instruction: Jump To Sub-Routine
        // Function:    Push current pc to stack, pc = address
        //
        public byte JSR()
        {
            pc--;

            Write((ushort)(0x0100 + stkp), (byte)((pc >> 8) & 0x00FF));
            stkp--;
            Write((ushort)(0x0100 + stkp), (byte)(pc & 0x00FF));
            stkp--;

            pc = addr_abs;
            return 0;
        }


        // Instruction: Load The Accumulator
        // Function:    A = M
        // Flags Out:   N, Z
        //
        public byte LDA()
        {
            fetch();
            a = fetched;
            SetFlag(Flag.Z, a == 0x00);
            SetFlag(Flag.N, (a & 0x80) != 0);
            return 1;
        }


        // Instruction: Load The X Register
        // Function:    X = M
        // Flags Out:   N, Z
        //
        public byte LDX()
        {
            fetch();
            x = fetched;
            SetFlag(Flag.Z, x == 0x00);
            SetFlag(Flag.N, (x & 0x80) != 0);
            return 1;
        }


        // Instruction: Load The Y Register
        // Function:    Y = M
        // Flags Out:   N, Z
        //
        public byte LDY()
        {
            fetch();
            y = fetched;
            SetFlag(Flag.Z, y == 0x00);
            SetFlag(Flag.N, (y & 0x80) != 0);
            return 1;
        }

        public byte LSR()
        {
            fetch();
            SetFlag(Flag.C, (fetched & 0x0001) != 0);
            temp = (ushort)(fetched >> 1);
            SetFlag(Flag.Z, (temp & 0x00FF) == 0x0000);
            SetFlag(Flag.N, (temp & 0x0080) != 0);
            if (ops[opcode].am == IMP)
                a = (byte)(temp & 0x00FF);
            else
                Write(addr_abs, (byte)(temp & 0x00FF));
            return 0;
        }

        public byte NOP()
        {
            // Sadly not all NOPs are equal, Ive added a few here
            // based on https://wiki.nesdev.com/w/index.php/CPU_unofficial_opcodes
            // and will add more based on game compatibility, and ultimately
            // I'd like to cover all illegal opcodes too
            //
            switch (opcode)
            {
                case 0x1C:
                case 0x3C:
                case 0x5C:
                case 0x7C:
                case 0xDC:
                case 0xFC:
                    return 1;
            }
            return 0;
        }


        // Instruction: Bitwise Logic OR
        // Function:    A = A | M
        // Flags Out:   N, Z
        //
        public byte ORA()
        {
            fetch();
            a = (byte)(a | fetched);
            SetFlag(Flag.Z, a == 0x00);
            SetFlag(Flag.N, (a & 0x80) != 0);
            return 1;
        }


        // Instruction: Push Accumulator to Stack
        // Function:    A -> stack
        //
        public byte PHA()
        {
            Write((ushort)(0x0100 + stkp), a);
            stkp--;
            return 0;
        }


        // Instruction: Push Status Register to Stack
        // Function:    status -> stack
        // Note:        Break flag is set to 1 before push
        //
        public byte PHP()
        {
            Write((ushort)(0x0100 + stkp), (byte)(status | (byte)Flag.B | (byte)Flag.U));
            SetFlag(Flag.B, false);
            SetFlag(Flag.U, false);
            stkp--;
            return 0;
        }


        // Instruction: Pop Accumulator off Stack
        // Function:    A <- stack
        // Flags Out:   N, Z
        //
        public byte PLA()
        {
            stkp++;
            a = Read((ushort)(0x0100 + stkp));
            SetFlag(Flag.Z, a == 0x00);
            SetFlag(Flag.N, (a & 0x80) != 0);
            return 0;
        }


        // Instruction: Pop Status Register off Stack
        // Function:    Status <- stack
        //
        public byte PLP()
        {
            stkp++;
            status = Read((ushort)(0x0100 + stkp));
            SetFlag(Flag.U, true);
            return 0;
        }

        public byte ROL()
        {
            fetch();
            temp = (ushort)((fetched << 1) | GetFlag(Flag.C));
            SetFlag(Flag.C, (temp & 0xFF00) != 0);
            SetFlag(Flag.Z, (temp & 0x00FF) == 0x0000);
            SetFlag(Flag.N, (temp & 0x0080) != 0);
            if (ops[opcode].am == IMP)
                a = (byte)(temp & 0x00FF);
            else
                Write(addr_abs, (byte)(temp & 0x00FF));
            return 0;
        }

        public byte ROR()
        {
            fetch();
            temp = (ushort)((GetFlag(Flag.C) << 7) | (fetched >> 1));
            SetFlag(Flag.C, (fetched & 0x01) != 0);
            SetFlag(Flag.Z, (temp & 0x00FF) == 0x00);
            SetFlag(Flag.N, (temp & 0x0080) != 0);
            if (ops[opcode].am == IMP)
                a = (byte)(temp & 0x00FF);
            else
                Write(addr_abs, (byte)(temp & 0x00FF));
            return 0;
        }

        public byte RTI()
        {
            stkp++;
            status = Read((ushort)(0x0100 + stkp));

            byte bFlag = (byte)Flag.B;
            status &= (byte)~bFlag;

            byte uFlag = (byte)Flag.U;
            status &= (byte)~uFlag;

            stkp++;
            pc = (ushort)Read((ushort)(0x0100 + stkp));
            stkp++;
            pc |= (ushort)(Read((ushort)(0x0100 + stkp)) << 8);
            return 0;
        }

        public byte RTS()
        {
            stkp++;
            pc = (ushort)Read((ushort)(0x0100 + stkp));
            stkp++;
            pc |= (ushort)(Read((ushort)(0x0100 + stkp)) << 8);

            pc++;
            return 0;
        }


        // Instruction: Set Carry Flag
        // Function:    C = 1
        //
        public byte SEC()
        {
            SetFlag(Flag.C, true);
            return 0;
        }


        // Instruction: Set Decimal Flag
        // Function:    D = 1
        //
        public byte SED()
        {
            SetFlag(Flag.D, true);
            return 0;
        }


        // Instruction: Set Interrupt Flag / Enable Interrupts
        // Function:    I = 1
        //
        public byte SEI()
        {
            SetFlag(Flag.I, true);
            return 0;
        }


        // Instruction: Store Accumulator at Address
        // Function:    M = A
        //
        public byte STA()
        {
            Write(addr_abs, a);
            return 0;
        }


        // Instruction: Store X Register at Address
        // Function:    M = X
        //
        public byte STX()
        {
            Write(addr_abs, x);
            return 0;
        }


        // Instruction: Store Y Register at Address
        // Function:    M = Y
        //
        public byte STY()
        {
            Write(addr_abs, y);
            return 0;
        }


        // Instruction: Transfer Accumulator to X Register
        // Function:    X = A
        // Flags Out:   N, Z
        //
        public byte TAX()
        {
            x = a;
            SetFlag(Flag.Z, x == 0x00);
            SetFlag(Flag.N, (x & 0x80) != 0);
            return 0;
        }


        // Instruction: Transfer Accumulator to Y Register
        // Function:    Y = A
        // Flags Out:   N, Z
        //
        public byte TAY()
        {
            y = a;
            SetFlag(Flag.Z, y == 0x00);
            SetFlag(Flag.N, (y & 0x80) != 0);
            return 0;
        }


        // Instruction: Transfer Stack Pointer to X Register
        // Function:    X = stack pointer
        // Flags Out:   N, Z
        //
        public byte TSX()
        {
            x = stkp;
            SetFlag(Flag.Z, x == 0x00);
            SetFlag(Flag.N, (x & 0x80) != 0);
            return 0;
        }


        // Instruction: Transfer X Register to Accumulator
        // Function:    A = X
        // Flags Out:   N, Z
        //
        public byte TXA()
        {
            a = x;
            SetFlag(Flag.Z, a == 0x00);
            SetFlag(Flag.N, (a & 0x80) != 0);
            return 0;
        }


        // Instruction: Transfer X Register to Stack Pointer
        // Function:    stack pointer = X
        //
        public byte TXS()
        {
            stkp = x;
            return 0;
        }


        // Instruction: Transfer Y Register to Accumulator
        // Function:    A = Y
        // Flags Out:   N, Z
        //
        public byte TYA()
        {
            a = y;
            SetFlag(Flag.Z, a == 0x00);
            SetFlag(Flag.N, (a & 0x80) > 0);
            return 0;
        }

    }
}

