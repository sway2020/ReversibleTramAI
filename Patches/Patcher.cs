using HarmonyLib;
using System.Reflection;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReversibleTramAI
{
    internal static class Patcher
    {
        private const string HarmonyId = "com.github.sway2020.ReversibleTramAI";

        private static bool patched = false;

        public static void PatchAll()
        {
            if (patched) return;

            Debug.Log("ReversibleTramAIMod: Appying Harmony Patches...");

            patched = true;

            // Harmony.DEBUG = true;
            var harmony = new Harmony(HarmonyId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static void UnpatchAll()
        {
            if (!patched) return;

            var harmony = new Harmony(HarmonyId);
            harmony.UnpatchAll(HarmonyId);

            patched = false;

            Debug.Log("ReversibleTramAIMod: Reverting Harmony Patches...");
        }


        /* Taken from TMPE */

        /// <summary>
        /// Gets parameter types from delegate
        /// </summary>
        /// <typeparam name="TDelegate">delegate type</typeparam>
        /// <param name="instance">skip first parameter. Default value is false.</param>
        /// <returns>Type[] representing arguments of the delegate.</returns>
        internal static Type[] GetParameterTypes<TDelegate>(bool instance = false) where TDelegate : Delegate
        {
            IEnumerable<ParameterInfo> parameters = typeof(TDelegate).GetMethod("Invoke").GetParameters();
            if (instance)
            {
                parameters = parameters.Skip(1);
            }

            return parameters.Select(p => p.ParameterType).ToArray();
        }

        /// <summary>
        /// Gets directly declared method.
        /// </summary>
        /// <typeparam name="TDelegate">delegate that has the same argument types as the intented overloaded method</typeparam>
        /// <param name="type">the class/type where the method is delcared</param>
        /// <param name="name">the name of the method</param>
        /// <param name="instance">is instance delegate (require skip if the first param)</param>
        /// <returns>a method or null when type is null or when a method is not found</returns>
        internal static MethodInfo DeclaredMethod<TDelegate>(Type type, string name, bool instance = false)
            where TDelegate : Delegate
        {
            var args = GetParameterTypes<TDelegate>(instance);
            var ret = AccessTools.DeclaredMethod(type, name, args);
            if (ret == null)
                Debug.Log($"ReversibleTramAIMod: failed to retrieve method {type}.{name}({args.ToString()})");
            return ret;
        }

        internal static List<CodeInstruction> ToCodeList(IEnumerable<CodeInstruction> instructions)
        {
            var originalCodes = new List<CodeInstruction>(instructions);
            var codes = new List<CodeInstruction>(originalCodes);
            return codes;
        }

        internal static bool IsSameInstruction(CodeInstruction a, CodeInstruction b, bool debug = false)
        {
            if (a.opcode == b.opcode)
            {
                if (a.operand == b.operand)
                {
                    return true;
                }

                // This special code is needed for some reason because the == operator doesn't work on System.Byte
                return (a.operand is byte aByte && b.operand is byte bByte && aByte == bByte)
                       || (a.operand is int aInt && b.operand is int bInt && aInt == bInt);
            }
            else
            {
                return false;
            }
        }

    }
}
