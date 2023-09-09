using HarmonyLib;
using Interactables.Interobjects;
using PluginAPI.Core.Attributes;
using PluginAPI.Core;
using PluginAPI.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HarmonyLib.AccessTools;
using System.Reflection.Emit;
using NorthwoodLib.Pools;
using System.Text.RegularExpressions;
using System.Reflection;

namespace NameFix
{
    public class Plugin
    {
        public static Harmony Harmony { get; private set; }

        [PluginEntryPoint("NameFix", "1.0.0", "Fixes players putting colors and sizes in their names", "Steven4547466")]
        void LoadPlugin()
        {
            Harmony = new Harmony($"namefix-{DateTime.Now.Ticks}");
            Harmony.PatchAll();
        }

        [PluginUnload]
        void UnloadPlugin()
        {
            Harmony.UnpatchAll(Harmony.Id);
            Harmony = null;
        }
    }

    [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.SetNick))]
    public class NicknameSyncSetNickPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            int index = newInstructions.FindIndex(i => i.opcode == OpCodes.Ldarg_1) + 1;
            newInstructions.Insert(index, new CodeInstruction(OpCodes.Call, Method(typeof(Regex), nameof(Regex.Unescape))).MoveLabelsFrom(newInstructions[index]));

            index = newInstructions.FindIndex(i => i.opcode == OpCodes.Br_S) + 1;
            List<Label> labels = newInstructions[index].ExtractLabels();
            newInstructions.RemoveAt(index);
            newInstructions.InsertRange(index, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_0).WithLabels(labels),
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(NicknameSync), nameof(NicknameSync.MyNick)))
            });

            index = newInstructions.FindIndex(i => i.opcode == OpCodes.Call && (MethodInfo)i.operand == Method(typeof(Regex), nameof(Regex.Replace), new Type[] { typeof(string), typeof(string) })) + 4;
            labels = newInstructions[index].ExtractLabels();
            newInstructions.RemoveAt(index);
            newInstructions.InsertRange(index, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_0).WithLabels(labels),
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(NicknameSync), nameof(NicknameSync.MyNick)))
            });

            foreach (CodeInstruction instruction in newInstructions)
                yield return instruction;

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }

    [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.MyNick), MethodType.Setter)]
    public class NicknameSyncMyNickPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            newInstructions.InsertRange(0, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, Method(typeof(Regex), nameof(Regex.Unescape))),
                new CodeInstruction(OpCodes.Starg_S, 1)
            });

            foreach (CodeInstruction instruction in newInstructions)
                yield return instruction;

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }

    [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.DisplayName), MethodType.Setter)]
    public class NicknameSyncDisplayNamePatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            newInstructions.InsertRange(0, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, Method(typeof(Regex), nameof(Regex.Unescape))),
                new CodeInstruction(OpCodes.Starg_S, 1)
            });

            foreach (CodeInstruction instruction in newInstructions)
                yield return instruction;

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}
