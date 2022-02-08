using HarmonyLib;
using UnityEngine;


namespace ReversibleTramAI
{
    [HarmonyPatch(typeof(TramBaseAI), "FrameDataUpdated")]
    public static class FrameDataUpdatedPatch
    {
        public static bool Prefix(ref VehicleInfo ___m_info, ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData)
        {
            Vector3 vector = frameData.m_position + frameData.m_velocity * 0.5f;
            Vector3 vector2 = frameData.m_rotation * new Vector3(0f, 0f, Mathf.Max(0.5f, ___m_info.m_generatedInfo.m_size.z * 0.5f - 1f));

            bool reversedFlag = (vehicleData.m_flags & Vehicle.Flags.Reversed) != 0;

            if (!reversedFlag)
            {
                vehicleData.m_segment.a = vector - vector2;
                vehicleData.m_segment.b = vector + vector2;
            }

            // NON-STOCK CODE START
            else
            {
                vehicleData.m_segment.a = vector + vector2;
                vehicleData.m_segment.b = vector - vector2;
            }
            // NON-STOCK CODE END

            return false; // skip original
        }
    }
}
