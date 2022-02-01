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

    [HarmonyPatch(typeof(CarAI), "CheckOtherVehicle")]
    public static class CheckOtherVehiclePatch
    {
        public static bool Prefix(ref ushort __result, ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ref float maxSpeed, ref bool blocked, ref Vector3 collisionPush, float maxBraking, ushort otherID, ref Vehicle otherData, Vector3 min, Vector3 max, int lodPhysics)
        {
            if (otherData.Info.m_vehicleType != VehicleInfo.VehicleType.Tram) return true;
            if (vehicleData.Info.m_vehicleType == VehicleInfo.VehicleType.Tram) return true;
            bool reversedFlag = (otherData.m_flags & Vehicle.Flags.Reversed) != 0;
            if (!reversedFlag) return true;

            if (!(otherID != vehicleID && vehicleData.m_leadingVehicle != otherID && vehicleData.m_trailingVehicle != otherID))
            {
                __result = otherData.m_nextGridVehicle;
                return false;
            }

            VehicleInfo info = otherData.Info;
            if (info.m_vehicleType == VehicleInfo.VehicleType.Bicycle)
            {
                __result = otherData.m_nextGridVehicle;
                return false;
            }

            Segment3 otherDataSegmentInverted = new Segment3(otherData.m_segment.b, otherData.m_segment.a);

            /*
            /* every thing below is the same as the vanilla method except using otherDataSegmentInverted instead of otherData.m_segment
            */

            if (((vehicleData.m_flags | otherData.m_flags) & Vehicle.Flags.Transition) == 0 && (vehicleData.m_flags & Vehicle.Flags.Underground) != (otherData.m_flags & Vehicle.Flags.Underground))
            {
                __result = otherData.m_nextGridVehicle;
                return false;
            }
            Vector3 vector;
            Vector3 vector2;
            if (lodPhysics >= 2)
            {
                vector = otherDataSegmentInverted.Min();
                vector2 = otherDataSegmentInverted.Max();
            }
            else
            {
                vector = Vector3.Min(otherDataSegmentInverted.Min(), otherData.m_targetPos3);
                vector2 = Vector3.Max(otherDataSegmentInverted.Max(), otherData.m_targetPos3);
            }
            if (min.x < vector2.x + 2f && min.y < vector2.y + 2f && min.z < vector2.z + 2f && vector.x < max.x + 2f && vector.y < max.y + 2f && vector.z < max.z + 2f)
            {
                Vehicle.Frame lastFrameData = otherData.GetLastFrameData();
                if (lodPhysics < 2)
                {
                    float u;
                    float v;
                    float num = vehicleData.m_segment.DistanceSqr(otherDataSegmentInverted, out u, out v);
                    if (num < 4f)
                    {
                        Vector3 vector3 = vehicleData.m_segment.Position(0.5f);
                        Vector3 vector4 = otherDataSegmentInverted.Position(0.5f);
                        Vector3 lhs = vehicleData.m_segment.b - vehicleData.m_segment.a;
                        if (Vector3.Dot(lhs, vector3 - vector4) < 0f)
                        {
                            collisionPush -= lhs.normalized * (0.1f - num * 0.025f);
                        }
                        else
                        {
                            collisionPush += lhs.normalized * (0.1f - num * 0.025f);
                        }
                        blocked = true;
                    }
                }
                float num2 = frameData.m_velocity.magnitude + 0.01f;
                float magnitude = lastFrameData.m_velocity.magnitude;
                float num3 = magnitude * (0.5f + 0.5f * magnitude / info.m_braking) + Mathf.Min(1f, magnitude);
                magnitude += 0.01f;
                float num4 = 0f;
                Vector3 vector5 = vehicleData.m_segment.b;
                Vector3 lhs2 = vehicleData.m_segment.b - vehicleData.m_segment.a;
                int num5 = ((vehicleData.Info.m_vehicleType == VehicleInfo.VehicleType.Tram) ? 1 : 0);
                for (int i = num5; i < 4; i++)
                {
                    Vector3 vector6 = vehicleData.GetTargetPos(i);
                    Vector3 vector7 = vector6 - vector5;
                    if (!(Vector3.Dot(lhs2, vector7) > 0f))
                    {
                        continue;
                    }
                    float magnitude2 = vector7.magnitude;
                    Segment3 segment = new Segment3(vector5, vector6);
                    min = segment.Min();
                    max = segment.Max();
                    segment.a.y *= 0.5f;
                    segment.b.y *= 0.5f;
                    if (magnitude2 > 0.01f && min.x < vector2.x + 2f && min.y < vector2.y + 2f && min.z < vector2.z + 2f && vector.x < max.x + 2f && vector.y < max.y + 2f && vector.z < max.z + 2f)
                    {
                        Vector3 a = otherDataSegmentInverted.a;
                        a.y *= 0.5f;
                        if (segment.DistanceSqr(a, out var u2) < 4f)
                        {
                            float num6 = Vector3.Dot(lastFrameData.m_velocity, vector7) / magnitude2;
                            float num7 = num4 + magnitude2 * u2;
                            if (num7 >= 0.01f)
                            {
                                num7 -= num6 + 3f;
                                float num8 = Mathf.Max(0f, CalculateMaxSpeed(num7, num6, maxBraking));
                                if (num8 < 0.01f)
                                {
                                    blocked = true;
                                }
                                Vector3 rhs = Vector3.Normalize((Vector3)otherData.m_targetPos0 - otherDataSegmentInverted.a);
                                float num9 = 1.2f - 1f / ((float)(int)vehicleData.m_blockCounter * 0.02f + 0.5f);
                                if (Vector3.Dot(vector7, rhs) > num9 * magnitude2)
                                {
                                    maxSpeed = Mathf.Min(maxSpeed, num8);
                                }
                            }
                            break;
                        }
                        if (lodPhysics < 2)
                        {
                            float num10 = 0f;
                            float num11 = num3;
                            Vector3 vector8 = otherDataSegmentInverted.b;
                            Vector3 lhs3 = otherDataSegmentInverted.b - otherDataSegmentInverted.a;
                            int num12 = ((info.m_vehicleType == VehicleInfo.VehicleType.Tram) ? 1 : 0);
                            bool flag = false;
                            for (int j = num12; j < 4; j++)
                            {
                                if (!(num11 > 0.1f))
                                {
                                    break;
                                }
                                Vector3 vector9;
                                if (otherData.m_leadingVehicle != 0)
                                {
                                    if (j != num12)
                                    {
                                        break;
                                    }
                                    vector9 = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[otherData.m_leadingVehicle].m_segment.b;
                                }
                                else
                                {
                                    vector9 = otherData.GetTargetPos(j);
                                }
                                Vector3 vector10 = Vector3.ClampMagnitude(vector9 - vector8, num11);
                                if (!(Vector3.Dot(lhs3, vector10) > 0f))
                                {
                                    continue;
                                }
                                vector9 = vector8 + vector10;
                                float magnitude3 = vector10.magnitude;
                                num11 -= magnitude3;
                                Segment3 segment2 = new Segment3(vector8, vector9);
                                segment2.a.y *= 0.5f;
                                segment2.b.y *= 0.5f;
                                if (magnitude3 > 0.01f)
                                {
                                    float u3;
                                    float v2;
                                    float num13 = ((otherID >= vehicleID) ? segment.DistanceSqr(segment2, out u3, out v2) : segment2.DistanceSqr(segment, out v2, out u3));
                                    if (num13 < 4f)
                                    {
                                        float num14 = num4 + magnitude2 * u3;
                                        float num15 = num10 + magnitude3 * v2 + 0.1f;
                                        if (num14 >= 0.01f && num14 * magnitude > num15 * num2)
                                        {
                                            float num16 = Vector3.Dot(lastFrameData.m_velocity, vector7) / magnitude2;
                                            if (num14 >= 0.01f)
                                            {
                                                num14 -= num16 + 1f + otherData.Info.m_generatedInfo.m_size.z;
                                                float num17 = Mathf.Max(0f, CalculateMaxSpeed(num14, num16, maxBraking));
                                                if (num17 < 0.01f)
                                                {
                                                    blocked = true;
                                                }
                                                maxSpeed = Mathf.Min(maxSpeed, num17);
                                            }
                                        }
                                        flag = true;
                                        break;
                                    }
                                }
                                lhs3 = vector10;
                                num10 += magnitude3;
                                vector8 = vector9;
                            }
                            if (flag)
                            {
                                break;
                            }
                        }
                    }
                    lhs2 = vector7;
                    num4 += magnitude2;
                    vector5 = vector6;
                }
            }
            __result = otherData.m_nextGridVehicle;
            return false;
        }

        private static float CalculateMaxSpeed(float targetDistance, float targetSpeed, float maxBraking)
        {
            float num = 0.5f * maxBraking;
            float num2 = num + targetSpeed;
            return Mathf.Sqrt(Mathf.Max(0f, num2 * num2 + 2f * targetDistance * maxBraking)) - num;
        }
    }
}
