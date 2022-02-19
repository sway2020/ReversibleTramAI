using HarmonyLib;

namespace ReversibleTramAI
{
    [HarmonyPatch(typeof(Notification), "AddProblems")]
    public static class AddProblemsPatch
    {
        [HarmonyPriority(Priority.High)]
        public static bool Prefix(ref Notification.Problem problems1, ref Notification.Problem problems2)
        {
            if (problems2 == Notification.Problem.TrackNotConnected) return false;
            return true;
        }
    }
}
