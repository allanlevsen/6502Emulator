using System;
namespace cpu6502.Interfaces
{
	public interface ICpu
	{

        // The read location of data can come from two sources, a memory address, or
        // its immediately available as part of the instruction. This function decides
        // depending on address mode of instruction byte
        //
        byte fetch();


    }
}

