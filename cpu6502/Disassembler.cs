using System;
using System.Text;

namespace cpu6502
{
	public class Disassembler
	{
        Dictionary<ushort, string> result = new Dictionary<ushort, string>();

        Func<ushort, byte, string> hex = (n, d) =>
        {
            StringBuilder s = new StringBuilder(new string('0', d));
            for (int i = d - 1; i >= 0; i--, n >>= 4)
                s[i] = "0123456789ABCDEF"[n & 0xF];
            return s.ToString();
        };

        public Disassembler()
		{
		}

        public Dictionary<ushort, string> Disassemble(ushort nStart, ushort nStop)
        {

            // Add your disassembly logic here and populate the 'result' dictionary
            // For example:
            for (ushort address = nStart; address <= nStop; address++)
            {
                string instruction = DisassembleInstruction(address); // Replace with your disassembly logic
                result[address] = instruction;
            }

            return result;
        }

        // Replace this with your actual disassembly logic
        public string DisassembleInstruction(ushort address)
        {
            // Implement your disassembly logic here
            // Return the disassembled instruction as a string
            return $"Instruction at address {address}";
        }

        public void OutputCode()
        {
            ushort startAddress = 0x1000;
            ushort stopAddress = 0x1010;

            Dictionary<ushort, string> disassembledInstructions = Disassemble(startAddress, stopAddress);

            foreach (var kvp in disassembledInstructions)
            {
                Console.WriteLine($"Address: 0x{kvp.Key:X4}, Instruction: {kvp.Value}");
            }
        }

    }

}

