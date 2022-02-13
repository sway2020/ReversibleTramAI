using ICities;
using CitiesHarmony.API;

namespace ReversibleTramAI
{
    public class Mod : IUserMod
    {
        public const string version = "0.5.0";
        public string Name => "Reversible Tram AI " + version;
        public string Description
        {
            get { return "Modifies tram AI to allow reversible trams"; }
        }

        public void OnEnabled()
        {
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
        }

        public void OnDisabled()
        {
            if (HarmonyHelper.IsHarmonyInstalled)
            {
                Patcher.UnpatchAll();
            }
        }

    }

    /// This mod uses harmony prefix to skip the following methods in the original TramBaseAI:
    /// 
    /// public override void SimulationStep(ushort vehicleID, ref Vehicle data, Vector3 physicsLodRefPos)
    /// 
    /// public override void SimulationStep(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
    /// 
    /// protected void UpdatePathTargetPositions(ushort vehicleID, ref Vehicle vehicleData, Vector3 refPos1, Vector3 refPos2, ushort leaderID, ref Vehicle leaderData, ref int index, int max1, int max2, float minSqrDistanceA, float minSqrDistanceB)
    /// 
    /// public override void FrameDataUpdated(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData)
    /// 
    /// private static void ResetTargets(ushort vehicleID, ref Vehicle vehicleData, ushort leaderID, ref Vehicle leaderData, bool pushPathPos)
}
