using Harmony;
using System.Collections.Generic;

namespace ModListBackup.Utils {

    public static class HarmonyUtils {

        public static void PrintAllInstructions(IEnumerable<CodeInstruction> instructions, bool ignoreNull = false) {
            foreach (var instr in instructions) {
                if (ignoreNull && instr.operand == null) {
                    continue;
                }
                PrintInstruction(instr);
            }
        }

        public static void PrintInstruction(CodeInstruction instruction) {
            Main.Log.Message("Opcode:{0} Operand:{1}", instruction.opcode.Name, (instruction.operand == null) ? "null" : instruction.operand.ToString());
        }
    }
}