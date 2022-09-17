using HarmonyLib;

namespace ReversibleTramAI
{
    [HarmonyPatch(typeof(Notification), "AddProblems")]
    public static class AddProblemsPatch
    {
        [HarmonyPriority(Priority.High)]
        public static bool Prefix(ref Notification.ProblemStruct problems1, ref Notification.ProblemStruct problems2)
        {
            if (problems2 == Notification.Problem1.TrackNotConnected) return false;
            return true;
        }
    }
}
