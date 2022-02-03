using HarmonyLib;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace ReversibleTramAI
{
    internal static class Patcher
    {
        private const string HarmonyId = "com.github.sway2020.ReversibleTramAI";

        private static bool patched = false;

        public static void PatchAll()
        {
            if (patched) return;

            UnityEngine.Debug.Log("ReversibleTramAIMod: Patching...");

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

            UnityEngine.Debug.Log("ReversibleTramAIMod: Reverted...");
        }
    }

    [HarmonyPatch(typeof(Notification), "AddProblems")]
    public static class AddProblemsPatch
    {
        public static bool Prefix(ref Notification.Problem problems1, ref Notification.Problem problems2)
        {
            if (problems2 == Notification.Problem.TrackNotConnected) return false;
            return true;
        }
    }

}
