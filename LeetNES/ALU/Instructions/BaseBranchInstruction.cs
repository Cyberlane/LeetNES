using System.Collections.Generic;

namespace LeetNES.ALU.Instructions
{
    /// <summary>
    /// Most branching instructions are a single addressing mode, with only the conditions for the branch changing.
    /// This will keep the commonalitites in one place.
    /// </summary>
    public abstract class BaseBranchInstruction : IInstruction
    {
        public abstract string Mnemonic { get; }
        protected abstract byte OpCode { get; }

        public IDictionary<byte, AddressingMode> Variants {
            get
            {
                return new Dictionary<byte, AddressingMode>
                {
                    { OpCode, AddressingMode.Relative }
                };
            }
        }

        public int Execute(Cpu.State cpuState, IMemory memory)
        {
            if (ShouldBranch(cpuState, memory))
            {
                var offset = memory[cpuState.Pc + 1];
                ushort newPc;
                if ((offset & 0x80) != 0)
                {
                    newPc = (ushort) (cpuState.Pc - (0x100 - offset));
                }
                else
                {
                    newPc = (ushort)(cpuState.Pc + offset);
                }

                // Addresses are relative to the beginning of the next instruction not
                // the beginning of this one, so we'll need to advance the program counter.
                newPc += 2;
                
                int cycles = 3;
                if ((newPc & 0xFF0) != (cpuState.Pc & 0xFF00))
                {
                    // Extra cycle if the relative branch occurs to cross a page boundary
                    ++cycles;
                }
                cpuState.Pc = newPc;
                return cycles;
            }
            cpuState.Pc += 2;
            return 2;
        }

        protected abstract bool ShouldBranch(Cpu.State cpuState, IMemory memory);
    }
}