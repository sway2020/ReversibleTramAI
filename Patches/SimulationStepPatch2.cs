using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System;
using ColossalFramework;
using ColossalFramework.Math;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ReversibleTramAI
{
    [HarmonyPatch]
    public static class SimulationStepPatch2
    {
        private delegate void TargetDelegate(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics);

        public static MethodBase TargetMethod() => Patcher.DeclaredMethod<TargetDelegate>(typeof(TramBaseAI), nameof(TramBaseAI.SimulationStep));

        public static bool Prefix(VehicleAI __instance, ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics, ref VehicleInfo ___m_info)
        {
            bool reversedFlag = (leaderData.m_flags & Vehicle.Flags.Reversed) != 0;
            ushort leadingVehicle = ((!reversedFlag) ? vehicleData.m_leadingVehicle : vehicleData.m_trailingVehicle);
            VehicleInfo vehicleInfo = ((leaderID == vehicleID) ? ___m_info : leaderData.Info);
            TramBaseAI tramBaseAI = vehicleInfo.m_vehicleAI as TramBaseAI;
            if (leadingVehicle != 0)
            {
                frameData.m_position += frameData.m_velocity * 0.4f;
            }
            else
            {
                frameData.m_position += frameData.m_velocity * 0.5f;
            }
            frameData.m_swayPosition += frameData.m_swayVelocity * 0.5f;
            Vector3 vector = frameData.m_position;
            Vector3 position = frameData.m_position;
            Vector3 vector2 = frameData.m_rotation * new Vector3(0f, 0f, ___m_info.m_generatedInfo.m_wheelBase * 0.5f);
            if (reversedFlag)
            {
                vector -= vector2;
                position += vector2;
            }
            else
            {
                vector += vector2;
                position -= vector2;
            }
            float acceleration = ___m_info.m_acceleration;

            // NON STOCK CODE START

            float braking = ___m_info.m_braking * 0.3f;
            //float braking = m_info.m_braking;

            // NON STOCK CODE END

            float magnitude = frameData.m_velocity.magnitude;
            Vector3 vector3 = (Vector3)vehicleData.m_targetPos1 - vector;
            float num2 = vector3.sqrMagnitude;
            Quaternion quaternion = Quaternion.Inverse(frameData.m_rotation);
            Vector3 vector4 = quaternion * frameData.m_velocity;
            Vector3 vector5 = Vector3.forward;
            Vector3 vector6 = Vector3.zero;
            float num3 = 0f;
            float num4 = 0.5f;
            if (leadingVehicle != 0)
            {
                VehicleManager instance = Singleton<VehicleManager>.instance;
                Vehicle.Frame lastFrameData = instance.m_vehicles.m_buffer[leadingVehicle].GetLastFrameData();
                VehicleInfo info = instance.m_vehicles.m_buffer[leadingVehicle].Info;
                float num5 = (((vehicleData.m_flags & Vehicle.Flags.Inverted) != 0 == reversedFlag) ? (___m_info.m_attachOffsetFront - ___m_info.m_generatedInfo.m_size.z * 0.5f) : (___m_info.m_attachOffsetBack - ___m_info.m_generatedInfo.m_size.z * 0.5f));
                float num6 = (((instance.m_vehicles.m_buffer[leadingVehicle].m_flags & Vehicle.Flags.Inverted) != 0 == reversedFlag) ? (info.m_attachOffsetBack - info.m_generatedInfo.m_size.z * 0.5f) : (info.m_attachOffsetFront - info.m_generatedInfo.m_size.z * 0.5f));
                Vector3 position2 = frameData.m_position;
                if (reversedFlag)
                {
                    position2 += frameData.m_rotation * new Vector3(0f, 0f, num5);
                }
                else
                {
                    position2 -= frameData.m_rotation * new Vector3(0f, 0f, num5);
                }
                Vector3 position3 = lastFrameData.m_position;
                if (reversedFlag)
                {
                    position3 -= lastFrameData.m_rotation * new Vector3(0f, 0f, num6);
                }
                else
                {
                    position3 += lastFrameData.m_rotation * new Vector3(0f, 0f, num6);
                }
                Vector3 position4 = lastFrameData.m_position;
                vector2 = lastFrameData.m_rotation * new Vector3(0f, 0f, info.m_generatedInfo.m_wheelBase * 0.5f);
                if (reversedFlag)
                {
                    position4 += vector2;
                }
                else
                {
                    position4 -= vector2;
                }
                if (Vector3.Dot((Vector3)vehicleData.m_targetPos1 - (Vector3)vehicleData.m_targetPos0, (Vector3)vehicleData.m_targetPos0 - position) < 0f && vehicleData.m_path != 0 && (leaderData.m_flags & Vehicle.Flags.WaitingPath) == 0)
                {
                    int index = -1;

                    //tramBaseAI.UpdatePathTargetPositions(vehicleID, ref vehicleData, vehicleData.m_targetPos0, position, 0, ref leaderData, ref index, 0, 0, Vector3.SqrMagnitude(position - (Vector3)vehicleData.m_targetPos0) + 1f, 1f);
                    UpdatePathTargetPositions(tramBaseAI, vehicleID, ref vehicleData, vehicleData.m_targetPos0, position, 0, ref leaderData, ref index, 0, 0, Vector3.SqrMagnitude(position - (Vector3)vehicleData.m_targetPos0) + 1f, 1f, ref ___m_info);

                    num2 = 0f;
                }
                float num7 = Mathf.Max(Vector3.Distance(position2, position3), 2f);
                float num8 = 1f;
                float num9 = num7 * num7;
                float minSqrDistanceB = num8 * num8;
                int i = 0;
                if (num2 < num9)
                {
                    if (vehicleData.m_path != 0 && (leaderData.m_flags & Vehicle.Flags.WaitingPath) == 0)
                    {
                        //tramBaseAI.UpdatePathTargetPositions(vehicleID, ref vehicleData, position, vector, 0, ref leaderData, ref i, 1, 2, num9, minSqrDistanceB);
                        UpdatePathTargetPositions(tramBaseAI, vehicleID, ref vehicleData, position, vector, 0, ref leaderData, ref i, 1, 2, num9, minSqrDistanceB, ref ___m_info);
                    }
                    for (; i < 4; i++)
                    {
                        vehicleData.SetTargetPos(i, vehicleData.GetTargetPos(i - 1));
                    }
                    vector3 = (Vector3)vehicleData.m_targetPos1 - vector;
                    num2 = vector3.sqrMagnitude;
                }

                // NON-STOCK CODE START. Code in TrainAI but should not be in TramBaseAI

                //if ((leaderData.m_flags & (Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped)) == 0 && m_info.m_vehicleType != VehicleInfo.VehicleType.Monorail)
                //{
                //	ForceTrafficLights(vehicleID, ref vehicleData, magnitude > 0.1f);
                //}

                // NON-STOCK CODE END

                if (vehicleData.m_path != 0)
                {
                    NetManager instance2 = Singleton<NetManager>.instance;
                    byte b = vehicleData.m_pathPositionIndex;
                    byte lastPathOffset = vehicleData.m_lastPathOffset;
                    if (b == byte.MaxValue)
                    {
                        b = 0;
                    }
                    PathManager instance3 = Singleton<PathManager>.instance;
                    if (instance3.m_pathUnits.m_buffer[vehicleData.m_path].GetPosition(b >> 1, out var position5))
                    {
                        instance2.m_segments.m_buffer[position5.m_segment].AddTraffic(Mathf.RoundToInt(___m_info.m_generatedInfo.m_size.z * 3f), 5);// GetNoiseLevel());
                        if ((b & 1) == 0 || lastPathOffset == 0 || (leaderData.m_flags & Vehicle.Flags.WaitingPath) != 0)
                        {
                            uint laneID = PathManager.GetLaneID(position5);
                            if (laneID != 0)
                            {
                                instance2.m_lanes.m_buffer[laneID].ReserveSpace(___m_info.m_generatedInfo.m_size.z);
                            }
                        }
                        else if (instance3.m_pathUnits.m_buffer[vehicleData.m_path].GetNextPosition(b >> 1, out position5))
                        {
                            uint laneID2 = PathManager.GetLaneID(position5);
                            if (laneID2 != 0)
                            {
                                instance2.m_lanes.m_buffer[laneID2].ReserveSpace(___m_info.m_generatedInfo.m_size.z);
                            }
                        }
                    }
                }
                vector3 = quaternion * vector3;
                float num10 = (___m_info.m_generatedInfo.m_wheelBase + info.m_generatedInfo.m_wheelBase) * -0.5f - num5 - num6;
                bool flag2 = false;
                if (vehicleData.m_path != 0 && (leaderData.m_flags & Vehicle.Flags.WaitingPath) == 0)
                {
                    if (Line3.Intersect(vector, vehicleData.m_targetPos1, position4, num10, out var u, out var u2))
                    {
                        vector6 = vector3 * Mathf.Clamp(Mathf.Min(u, u2) / 0.6f, 0f, 2f);
                    }
                    else
                    {
                        Line3.DistanceSqr(vector, vehicleData.m_targetPos1, position4, out u);
                        vector6 = vector3 * Mathf.Clamp(u / 0.6f, 0f, 2f);
                    }
                    flag2 = true;
                }
                if (flag2)
                {
                    if (Vector3.Dot(position4 - vector, vector - position) < 0f)
                    {
                        num4 = 0f;
                    }
                }
                else
                {
                    float num11 = Vector3.Distance(position4, vector);
                    num4 = 0f;
                    vector6 = quaternion * ((position4 - vector) * (Mathf.Max(0f, num11 - num10) / Mathf.Max(1f, num11 * 0.6f)));
                }
            }
            else
            {
                // Non-stock code START

                float num12 = (magnitude + acceleration) * (0.5f + 0.5f * (magnitude + acceleration) / braking) + (___m_info.m_generatedInfo.m_size.z - ___m_info.m_generatedInfo.m_wheelBase) * 0.5f;
                // float num12 = (magnitude + acceleration) * (0.5f + 0.5f * (magnitude + acceleration) / braking);

                // Non-stock code END

                float num13 = Mathf.Max(magnitude + acceleration, 2f);
                float num14 = Mathf.Max((num12 - num13) / 2f, 1f);
                float num15 = num13 * num13;
                float minSqrDistanceB2 = num14 * num14;
                if (Vector3.Dot((Vector3)vehicleData.m_targetPos1 - (Vector3)vehicleData.m_targetPos0, (Vector3)vehicleData.m_targetPos0 - position) < 0f && vehicleData.m_path != 0 && (leaderData.m_flags & (Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped)) == 0)
                {
                    int index2 = -1;
                    
                    //tramBaseAI.UpdatePathTargetPositions(vehicleID, ref vehicleData, vehicleData.m_targetPos0, position, leaderID, ref leaderData, ref index2, 0, 0, Vector3.SqrMagnitude(position - (Vector3)vehicleData.m_targetPos0) + 1f, 1f);
                    UpdatePathTargetPositions(tramBaseAI, vehicleID, ref vehicleData, vehicleData.m_targetPos0, position, leaderID, ref leaderData, ref index2, 0, 0, Vector3.SqrMagnitude(position - (Vector3)vehicleData.m_targetPos0) + 1f, 1f, ref ___m_info);

                    num2 = 0f;
                }
                int j = 0;
                bool flag3 = false;
                if ((num2 < num15 || vehicleData.m_targetPos3.w < 0.01f) && (leaderData.m_flags & (Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped)) == 0)
                {
                    if (vehicleData.m_path != 0)
                    {
                        //tramBaseAI.UpdatePathTargetPositions(vehicleID, ref vehicleData, position, vector, leaderID, ref leaderData, ref j, 1, 4, num15, minSqrDistanceB2);
                        UpdatePathTargetPositions(tramBaseAI, vehicleID, ref vehicleData, position, vector, leaderID, ref leaderData, ref j, 1, 4, num15, minSqrDistanceB2, ref ___m_info);
                    }
                    if (j < 4)
                    {
                        flag3 = true;
                        for (; j < 4; j++)
                        {
                            vehicleData.SetTargetPos(j, vehicleData.GetTargetPos(j - 1));
                        }
                    }
                    vector3 = (Vector3)vehicleData.m_targetPos1 - vector;
                    num2 = vector3.sqrMagnitude;
                }

                if (vehicleData.m_path != 0)
                {
                    NetManager instance4 = Singleton<NetManager>.instance;
                    byte b2 = vehicleData.m_pathPositionIndex;
                    byte lastPathOffset2 = vehicleData.m_lastPathOffset;
                    if (b2 == byte.MaxValue)
                    {
                        b2 = 0;
                    }
                    PathManager instance5 = Singleton<PathManager>.instance;
                    if (instance5.m_pathUnits.m_buffer[vehicleData.m_path].GetPosition(b2 >> 1, out var position6))
                    {
                        instance4.m_segments.m_buffer[position6.m_segment].AddTraffic(Mathf.RoundToInt(___m_info.m_generatedInfo.m_size.z * 3f), 5); // GetNoiseLevel());
                        if ((b2 & 1) == 0 || lastPathOffset2 == 0 || (leaderData.m_flags & Vehicle.Flags.WaitingPath) != 0)
                        {
                            uint laneID3 = PathManager.GetLaneID(position6);
                            if (laneID3 != 0)
                            {
                                instance4.m_lanes.m_buffer[laneID3].ReserveSpace(___m_info.m_generatedInfo.m_size.z, vehicleID);
                            }
                        }
                        else if (instance5.m_pathUnits.m_buffer[vehicleData.m_path].GetNextPosition(b2 >> 1, out position6))
                        {
                            uint laneID4 = PathManager.GetLaneID(position6);
                            if (laneID4 != 0)
                            {
                                instance4.m_lanes.m_buffer[laneID4].ReserveSpace(___m_info.m_generatedInfo.m_size.z, vehicleID);
                            }
                        }
                    }
                }
                //float maxSpeed = (((leaderData.m_flags & Vehicle.Flags.Stopped) == 0) ? Mathf.Min(vehicleData.m_targetPos1.w, GetMaxSpeed(leaderID, ref leaderData)) : 0f);
                float maxSpeed = (((leaderData.m_flags & Vehicle.Flags.Stopped) == 0) ? Mathf.Min(vehicleData.m_targetPos1.w, GetMaxSpeedStub(leaderID, ref leaderData)) : 0f);

                vector3 = quaternion * vector3;
                if (reversedFlag)
                {
                    vector3 = -vector3;
                }

                // NON-STOCK CODE START. Code not in TrainAI but should be in TramBaseAI

                Vector3 collisionPush = Vector3.zero;

                // NON-STOCK CODE END


                bool blocked = false;
                float len = 0f;
                if (num2 > 1f)
                {
                    vector5 = VectorUtils.NormalizeXZ(vector3, out len);
                    if (len > 1f)
                    {
                        Vector3 v = vector3;
                        num13 = Mathf.Max(magnitude, 2f);
                        num15 = num13 * num13;
                        if (num2 > num15)
                        {
                            float num17 = num13 / Mathf.Sqrt(num2);
                            v.x *= num17;
                            v.y *= num17;
                        }
                        if (v.z < -1f)
                        {
                            if (vehicleData.m_path != 0 && (leaderData.m_flags & Vehicle.Flags.WaitingPath) == 0)
                            {
                                Vector3 vector7 = vehicleData.m_targetPos1 - vehicleData.m_targetPos0;
                                vector7 = quaternion * vector7;
                                if (reversedFlag)
                                {
                                    vector7 = -vector7;
                                }
                                if (vector7.z < -0.01f)
                                {
                                    if (vector3.z < Mathf.Abs(vector3.x) * -10f)
                                    {
                                        if (magnitude < 0.01f)
                                        {
                                            Reverse(__instance, ref ___m_info, leaderID, ref leaderData);
                                            return false;
                                        }
                                        v.z = 0f;
                                        vector3 = Vector3.zero;
                                        maxSpeed = 0f;
                                    }
                                    else
                                    {
                                        vector = position + Vector3.Normalize(vehicleData.m_targetPos1 - vehicleData.m_targetPos0) * ___m_info.m_generatedInfo.m_wheelBase;
                                        j = -1;

                                        //tramBaseAI.UpdatePathTargetPositions(vehicleID, ref vehicleData, vehicleData.m_targetPos0, vehicleData.m_targetPos1, leaderID, ref leaderData, ref j, 0, 0, Vector3.SqrMagnitude(vehicleData.m_targetPos1 - vehicleData.m_targetPos0) + 1f, 1f);
                                        UpdatePathTargetPositions(tramBaseAI, vehicleID, ref vehicleData, vehicleData.m_targetPos0, vehicleData.m_targetPos1, leaderID, ref leaderData, ref j, 0, 0, Vector3.SqrMagnitude(vehicleData.m_targetPos1 - vehicleData.m_targetPos0) + 1f, 1f, ref ___m_info);
                                    }
                                }
                                else
                                {
                                    j = -1;

                                    //tramBaseAI.UpdatePathTargetPositions(vehicleID, ref vehicleData, vehicleData.m_targetPos0, position, leaderID, ref leaderData, ref j, 0, 0, Vector3.SqrMagnitude(position - (Vector3)vehicleData.m_targetPos0) + 1f, 1f);
                                    UpdatePathTargetPositions(tramBaseAI, vehicleID, ref vehicleData, vehicleData.m_targetPos0, position, leaderID, ref leaderData, ref j, 0, 0, Vector3.SqrMagnitude(position - (Vector3)vehicleData.m_targetPos0) + 1f, 1f, ref ___m_info);

                                    vehicleData.m_targetPos1 = vector;
                                    v.z = 0f;
                                    vector3 = Vector3.zero;
                                    maxSpeed = 0f;
                                }
                            }
                            num4 = 0f;
                        }
                        vector5 = VectorUtils.NormalizeXZ(v, out len);
                        float num18 = (float)Math.PI / 2f * (1f - vector5.z);
                        if (len > 1f)
                        {
                            num18 /= len;
                        }
                        
                        //maxSpeed = Mathf.Min(maxSpeed, CalculateTargetSpeed(vehicleID, ref vehicleData, 1000f, num18));
                        //maxSpeed = Mathf.Min(maxSpeed, CalculateTargetSpeedReversePatch.CalculateTargetSpeed(__instance, vehicleID, ref vehicleData, 1000f, num18));
                        maxSpeed = Mathf.Min(maxSpeed, CalculateTargetSpeedStub(__instance, vehicleID, ref vehicleData, 1000f, num18));


                        float num19 = len;

                        //maxSpeed = Mathf.Min(maxSpeed, CalculateMaxSpeed(num19, vehicleData.m_targetPos2.w, braking));
                        maxSpeed = Mathf.Min(maxSpeed, CalculateMaxSpeedStub(num19, vehicleData.m_targetPos2.w, braking));

                        num19 += VectorUtils.LengthXZ(vehicleData.m_targetPos2 - vehicleData.m_targetPos1);

                        //maxSpeed = Mathf.Min(maxSpeed, CalculateMaxSpeed(num19, vehicleData.m_targetPos3.w, braking));
                        maxSpeed = Mathf.Min(maxSpeed, CalculateMaxSpeedStub(num19, vehicleData.m_targetPos3.w, braking));


                        num19 += VectorUtils.LengthXZ(vehicleData.m_targetPos3 - vehicleData.m_targetPos2);

                        // NON-STOCK CODE START. Code not in TrainAI but should be in TramBaseAI

                        if (vehicleData.m_targetPos3.w < 0.01f)
                        {
                            num19 = Mathf.Max(0f, num19 + (___m_info.m_generatedInfo.m_wheelBase - ___m_info.m_generatedInfo.m_size.z) * 0.5f);
                        }

                        //maxSpeed = Mathf.Min(maxSpeed, CalculateMaxSpeed(num19, 0f, braking));
                        maxSpeed = Mathf.Min(maxSpeed, CalculateMaxSpeedStub(num19, 0f, braking));

                        CarAI.CheckOtherVehicles(vehicleID, ref vehicleData, ref frameData, ref maxSpeed, ref blocked, ref collisionPush, num12, braking * 0.9f, lodPhysics);

                        // NON-STOCK CODE END

                        if (maxSpeed < magnitude)
                        {
                            float num20 = Mathf.Max(acceleration, Mathf.Min(braking, magnitude));
                            num3 = Mathf.Max(maxSpeed, magnitude - num20);
                        }
                        else
                        {
                            float num21 = Mathf.Max(acceleration, Mathf.Min(braking, 0f - magnitude));
                            num3 = Mathf.Min(maxSpeed, magnitude + num21);
                        }
                    }
                }
                else if (magnitude < 0.1f && flag3 && vehicleInfo.m_vehicleAI.ArriveAtDestination(leaderID, ref leaderData))
                {
                    leaderData.Unspawn(leaderID);
                    return false;
                }
                if ((leaderData.m_flags & Vehicle.Flags.Stopped) == 0 && maxSpeed < 0.1f)
                {
                    blocked = true;
                }
                if (blocked)
                {
                    leaderData.m_blockCounter = (byte)Mathf.Min(leaderData.m_blockCounter + 1, 255);
                }
                else
                {
                    leaderData.m_blockCounter = 0;
                }
                if (len > 1f)
                {
                    if (reversedFlag)
                    {
                        vector5 = -vector5;
                    }
                    vector6 = vector5 * num3;
                }
                else
                {
                    if (reversedFlag)
                    {
                        vector3 = -vector3;
                    }
                    num3 = 0f;
                    Vector3 vector8 = Vector3.ClampMagnitude(vector3 * 0.5f - vector4, braking);
                    vector6 = vector4 + vector8;
                }
            }
            Vector3 vector9 = vector6 - vector4;
            Vector3 vector10 = frameData.m_rotation * vector6;
            Vector3 vector11 = Vector3.Normalize((Vector3)vehicleData.m_targetPos0 - position) * (vector6.magnitude * num4);
            vector += vector10;
            position += vector11;
            Vector3 vector12;
            if (reversedFlag)
            {
                frameData.m_rotation = Quaternion.LookRotation(position - vector);
                vector12 = vector + frameData.m_rotation * new Vector3(0f, 0f, ___m_info.m_generatedInfo.m_wheelBase * 0.5f);
            }
            else
            {
                frameData.m_rotation = Quaternion.LookRotation(vector - position);
                vector12 = vector - frameData.m_rotation * new Vector3(0f, 0f, ___m_info.m_generatedInfo.m_wheelBase * 0.5f);
            }
            frameData.m_velocity = vector12 - frameData.m_position;
            if (leadingVehicle != 0)
            {
                frameData.m_position += frameData.m_velocity * 0.6f;
            }
            else
            {
                frameData.m_position += frameData.m_velocity * 0.5f;
            }
            frameData.m_swayVelocity = frameData.m_swayVelocity * (1f - ___m_info.m_dampers) - vector9 * (1f - ___m_info.m_springs) - frameData.m_swayPosition * ___m_info.m_springs;
            frameData.m_swayPosition += frameData.m_swayVelocity * 0.5f;
            frameData.m_steerAngle = 0f;
            frameData.m_travelDistance += vector6.z;
            frameData.m_lightIntensity.x = ((!reversedFlag) ? 5f : 0f);
            frameData.m_lightIntensity.y = ((!reversedFlag) ? 0f : 5f);
            frameData.m_lightIntensity.z = 0f;
            frameData.m_lightIntensity.w = 0f;
            frameData.m_underground = (vehicleData.m_flags & Vehicle.Flags.Underground) != 0;
            frameData.m_transition = (vehicleData.m_flags & Vehicle.Flags.Transition) != 0;
            //base.SimulationStep(vehicleID, ref vehicleData, ref frameData, leaderID, ref leaderData, lodPhysics);

            return false;
        }

        /// <summary>
        /// Taken from vanilla train AI, unchanged
        /// </summary>
        private static void Reverse(VehicleAI __instance, ref VehicleInfo ___m_info, ushort leaderID, ref Vehicle leaderData)
        {
            if ((leaderData.m_flags & Vehicle.Flags.Reversed) != 0)
            {
                leaderData.m_flags &= ~Vehicle.Flags.Reversed;
            }
            else
            {
                leaderData.m_flags |= Vehicle.Flags.Reversed;
            }
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort num = leaderID;
            int num2 = 0;
            while (num != 0)
            {
                ResetTargets(num, ref ___m_info, ref instance.m_vehicles.m_buffer[num], leaderID, ref leaderData, pushPathPos: true);

                instance.m_vehicles.m_buffer[num].m_flags = (instance.m_vehicles.m_buffer[num].m_flags & ~Vehicle.Flags.Reversed) | (leaderData.m_flags & Vehicle.Flags.Reversed);
                num = instance.m_vehicles.m_buffer[num].m_trailingVehicle;
                if (++num2 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }

        ///// <summary>
        ///// Taken from vanilla tramBaseAI, unchanged
        ///// </summary>
        //private static float GetMaxSpeed(ushort leaderID, ref Vehicle leaderData)
        //{
        //    float num = 1000000f;
        //    VehicleManager instance = Singleton<VehicleManager>.instance;
        //    ushort num2 = leaderID;
        //    int num3 = 0;
        //    while (num2 != 0)
        //    {
        //        num = Mathf.Min(num, instance.m_vehicles.m_buffer[num2].m_targetPos0.w);
        //        num = Mathf.Min(num, instance.m_vehicles.m_buffer[num2].m_targetPos1.w);
        //        num2 = instance.m_vehicles.m_buffer[num2].m_trailingVehicle;
        //        if (++num3 > 16384)
        //        {
        //            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
        //            break;
        //        }
        //    }
        //    return num;
        //}

        /// <summary>
        /// Taken from vanilla tramBaseAI, slightly unchanged
        /// </summary>
        private static void ResetTargets(ushort vehicleID, ref VehicleInfo ___m_info, ref Vehicle vehicleData, ushort leaderID, ref Vehicle leaderData, bool pushPathPos)
        {
            Vehicle.Frame lastFrameData = vehicleData.GetLastFrameData();
            VehicleInfo info = vehicleData.Info;
            TramBaseAI tramBaseAI = info.m_vehicleAI as TramBaseAI;
            Vector3 position = lastFrameData.m_position;
            Vector3 position2 = lastFrameData.m_position;
            Vector3 vector = lastFrameData.m_rotation * new Vector3(0f, 0f, info.m_generatedInfo.m_wheelBase * 0.5f);
            if ((leaderData.m_flags & Vehicle.Flags.Reversed) != 0)
            {
                position -= vector;
                position2 += vector;
            }
            else
            {
                position += vector;
                position2 -= vector;
            }
            vehicleData.m_targetPos0 = position2;
            vehicleData.m_targetPos0.w = 2f;
            vehicleData.m_targetPos1 = position;
            vehicleData.m_targetPos1.w = 2f;
            vehicleData.m_targetPos2 = vehicleData.m_targetPos1;
            vehicleData.m_targetPos3 = vehicleData.m_targetPos1;
            if (vehicleData.m_path != 0)
            {
                PathManager instance = Singleton<PathManager>.instance;
                int num = (vehicleData.m_pathPositionIndex >> 1) + 1;
                uint num2 = vehicleData.m_path;
                if (num >= instance.m_pathUnits.m_buffer[num2].m_positionCount)
                {
                    num = 0;
                    num2 = instance.m_pathUnits.m_buffer[num2].m_nextPathUnit;
                }
                if (instance.m_pathUnits.m_buffer[vehicleData.m_path].GetPosition(vehicleData.m_pathPositionIndex >> 1, out var position3))
                {
                    uint laneID = PathManager.GetLaneID(position3);
                    if (num2 != 0 && instance.m_pathUnits.m_buffer[num2].GetPosition(num, out var position4))
                    {
                        uint laneID2 = PathManager.GetLaneID(position4);
                        if (laneID2 == laneID)
                        {
                            if (num2 != vehicleData.m_path)
                            {
                                instance.ReleaseFirstUnit(ref vehicleData.m_path);
                            }
                            vehicleData.m_pathPositionIndex = (byte)(num << 1);
                        }
                    }
                    PathUnit.CalculatePathPositionOffset(laneID, position2, out vehicleData.m_lastPathOffset);
                }
            }
            if (vehicleData.m_path != 0)
            {
                int index = 0;

                //tramBaseAI.UpdatePathTargetPositions(vehicleID, ref vehicleData, position2, position, 0, ref leaderData, ref index, 1, 4, 4f, 1f);
                UpdatePathTargetPositions(tramBaseAI, vehicleID, ref vehicleData, position2, position, 0, ref leaderData, ref index, 1, 4, 4f, 1f, ref ___m_info);
            }
        }

        public static bool UpdatePathTargetPositions(VehicleAI __instance, ushort vehicleID, ref Vehicle vehicleData, Vector3 refPos1, Vector3 refPos2, ushort leaderID, ref Vehicle leaderData, ref int index, int max1, int max2, float minSqrDistanceA, float minSqrDistanceB, ref VehicleInfo ___m_info)
        {
            PathManager instance = Singleton<PathManager>.instance;
            NetManager instance2 = Singleton<NetManager>.instance;
            Vector4 targetPos = vehicleData.m_targetPos0;
            targetPos.w = 1000f;
            float num = minSqrDistanceA;
            float num2 = 0f;
            uint num3 = vehicleData.m_path;
            byte b = vehicleData.m_pathPositionIndex;
            byte offset = vehicleData.m_lastPathOffset;
            if (b == byte.MaxValue)
            {
                b = 0;
                if (index <= 0)
                {
                    vehicleData.m_pathPositionIndex = 0;
                }
                if (!Singleton<PathManager>.instance.m_pathUnits.m_buffer[num3].CalculatePathPositionOffset(b >> 1, targetPos, out offset))
                {
                    //InvalidPath(vehicleID, ref vehicleData, leaderID, ref leaderData);
                    //InvalidPathReversePatch.InvalidPath(__instance, vehicleID, ref vehicleData, leaderID, ref leaderData);
                    InvalidPathStub(__instance, vehicleID, ref vehicleData, leaderID, ref leaderData);
                    return false;
                }
            }
            if (!instance.m_pathUnits.m_buffer[num3].GetPosition(b >> 1, out var position))
            {
                //InvalidPath(vehicleID, ref vehicleData, leaderID, ref leaderData);
                //InvalidPathReversePatch.InvalidPath(__instance, vehicleID, ref vehicleData, leaderID, ref leaderData);
                InvalidPathStub(__instance, vehicleID, ref vehicleData, leaderID, ref leaderData);

                return false;
            }
            uint num4 = PathManager.GetLaneID(position);
            while (true)
            {
                if ((b & 1) == 0)
                {
                    bool flag = true;
                    while (offset != position.m_offset)
                    {
                        if (flag)
                        {
                            flag = false;
                        }
                        else
                        {
                            float num5 = Mathf.Max(Mathf.Sqrt(num) - Vector3.Distance(targetPos, refPos1), Mathf.Sqrt(num2) - Vector3.Distance(targetPos, refPos2));
                            int num6 = ((!(num5 < 0f)) ? (4 + Mathf.CeilToInt(num5 * 256f / (instance2.m_lanes.m_buffer[num4].m_length + 1f))) : 4);
                            if (offset > position.m_offset)
                            {
                                offset = (byte)Mathf.Max(offset - num6, position.m_offset);
                            }
                            else if (offset < position.m_offset)
                            {
                                offset = (byte)Mathf.Min(offset + num6, position.m_offset);
                            }
                        }
                        //CalculateSegmentPosition(vehicleID, ref vehicleData, position, num4, offset, out var pos, out var _, out var maxSpeed);
                        //CalculateSegmentPositionReversePatch1.CalculateSegmentPosition(__instance, vehicleID, ref vehicleData, position, num4, offset, out var pos, out var _, out var maxSpeed);
                        CalculateSegmentPositionStub1(__instance, vehicleID, ref vehicleData, position, num4, offset, out var pos, out var _, out var maxSpeed);

                        targetPos.Set(pos.x, pos.y, pos.z, Mathf.Min(targetPos.w, maxSpeed));
                        float sqrMagnitude = (pos - refPos1).sqrMagnitude;
                        float sqrMagnitude2 = (pos - refPos2).sqrMagnitude;
                        if (sqrMagnitude >= num && sqrMagnitude2 >= num2)
                        {
                            if (index <= 0)
                            {
                                vehicleData.m_lastPathOffset = offset;
                            }
                            vehicleData.SetTargetPos(index++, targetPos);
                            if (index < max1)
                            {
                                num = minSqrDistanceB;
                                refPos1 = targetPos;
                            }
                            else if (index == max1)
                            {
                                num = (refPos2 - refPos1).sqrMagnitude;
                                num2 = minSqrDistanceA;
                            }
                            else
                            {
                                num2 = minSqrDistanceB;
                                refPos2 = targetPos;
                            }
                            targetPos.w = 1000f;
                            if (index == max2)
                            {
                                return false;
                            }
                        }
                    }
                    b = (byte)(b + 1);
                    offset = 0;
                    if (index <= 0)
                    {
                        vehicleData.m_pathPositionIndex = b;
                        vehicleData.m_lastPathOffset = offset;
                    }
                }
                int num7 = (b >> 1) + 1;
                uint num8 = num3;
                if (num7 >= instance.m_pathUnits.m_buffer[num3].m_positionCount)
                {
                    num7 = 0;
                    num8 = instance.m_pathUnits.m_buffer[num3].m_nextPathUnit;
                    if (num8 == 0)
                    {
                        if (index <= 0)
                        {
                            Singleton<PathManager>.instance.ReleasePath(vehicleData.m_path);
                            vehicleData.m_path = 0u;
                        }
                        targetPos.w = 1f;
                        vehicleData.SetTargetPos(index++, targetPos);
                        break;
                    }
                }
                if (!instance.m_pathUnits.m_buffer[num8].GetPosition(num7, out var position2))
                {
                    //InvalidPath(vehicleID, ref vehicleData, leaderID, ref leaderData);
                    //InvalidPathReversePatch.InvalidPath(__instance, vehicleID, ref vehicleData, leaderID, ref leaderData);
                    InvalidPathStub(__instance, vehicleID, ref vehicleData, leaderID, ref leaderData);

                    break;
                }
                NetInfo info = instance2.m_segments.m_buffer[position2.m_segment].Info;
                if (info.m_lanes.Length <= position2.m_lane)
                {
                    //InvalidPath(vehicleID, ref vehicleData, leaderID, ref leaderData);
                    //InvalidPathReversePatch.InvalidPath(__instance, vehicleID, ref vehicleData, leaderID, ref leaderData);
                    InvalidPathStub(__instance, vehicleID, ref vehicleData, leaderID, ref leaderData);

                    break;
                }
                uint laneID = PathManager.GetLaneID(position2);
                NetInfo.Lane lane = info.m_lanes[position2.m_lane];
                if (lane.m_laneType != NetInfo.LaneType.Vehicle)
                {
                    //InvalidPath(vehicleID, ref vehicleData, leaderID, ref leaderData);
                    //InvalidPathReversePatch.InvalidPath(__instance, vehicleID, ref vehicleData, leaderID, ref leaderData);
                    InvalidPathStub(__instance, vehicleID, ref vehicleData, leaderID, ref leaderData);

                    break;
                }
                if (position2.m_segment != position.m_segment && leaderID != 0)
                {
                    leaderData.m_flags &= ~Vehicle.Flags.Leaving;
                }
                byte offset2 = 0;
                if (num4 != laneID)
                {
                    PathUnit.CalculatePathPositionOffset(laneID, targetPos, out offset2);
                    Bezier3 bezier = default(Bezier3);

                    //CalculateSegmentPosition(vehicleID, ref vehicleData, position, num4, position.m_offset, out bezier.a, out var dir2, out var _);
                    //CalculateSegmentPositionReversePatch1.CalculateSegmentPosition(__instance, vehicleID, ref vehicleData, position, num4, position.m_offset, out bezier.a, out var dir2, out var _);
                    CalculateSegmentPositionStub1(__instance, vehicleID, ref vehicleData, position, num4, position.m_offset, out bezier.a, out var dir2, out var _);

                    // NON-STOCK CODE START. Code in TrainAI but not in TramBaseAI

                    bool flag2 = (((leaderData.m_flags & Vehicle.Flags.Reversed) == 0) ? (vehicleData.m_leadingVehicle == 0) : (vehicleData.m_trailingVehicle == 0));
                    bool flag3 = flag2 && offset == 0;

                    // NON-STOCK CODE END


                    Vector3 dir3;
                    float maxSpeed3;
                    if (flag2 && offset == 0)
                    {
                        if (!instance.m_pathUnits.m_buffer[num8].GetNextPosition(num7, out var position3))
                        {
                            position3 = default(PathUnit.Position);
                        }
                        //CalculateSegmentPosition(vehicleID, ref vehicleData, position3, position2, laneID, offset2, position, num4, position.m_offset, index, out bezier.d, out dir3, out maxSpeed3);
                        //CalculateSegmentPositionReversePatch2.CalculateSegmentPosition(__instance, vehicleID, ref vehicleData, position3, position2, laneID, offset2, position, num4, position.m_offset, index, out bezier.d, out dir3, out maxSpeed3);
                        CalculateSegmentPositionStub2(__instance, vehicleID, ref vehicleData, position3, position2, laneID, offset2, position, num4, position.m_offset, index, out bezier.d, out dir3, out maxSpeed3);
                    }
                    else
                    {
                        //CalculateSegmentPosition(vehicleID, ref vehicleData, position2, laneID, offset2, out bezier.d, out dir3, out maxSpeed3);
                        //CalculateSegmentPositionReversePatch1.CalculateSegmentPosition(__instance, vehicleID, ref vehicleData, position2, laneID, offset2, out bezier.d, out dir3, out maxSpeed3);
                        CalculateSegmentPositionStub1(__instance, vehicleID, ref vehicleData, position2, laneID, offset2, out bezier.d, out dir3, out maxSpeed3);
                    }
                    if (position.m_offset == 0)
                    {
                        dir2 = -dir2;
                    }
                    if (offset2 < position2.m_offset)
                    {
                        dir3 = -dir3;
                    }
                    dir2.Normalize();
                    dir3.Normalize();
                    NetSegment.CalculateMiddlePoints(bezier.a, dir2, bezier.d, dir3, smoothStart: true, smoothEnd: true, out bezier.b, out bezier.c, out var distance);

                    // NON-STOCK CODE START. Code in TrainAI but not in TramBaseAI

                    // check and yield to the tram in an ending segment waiting to be reversed)
                    if (flag3 && (lane.m_direction == NetInfo.Direction.Both))
                    {
                        CheckNextLane(___m_info, vehicleID, ref vehicleData, ref maxSpeed3, position2, laneID, offset2, position, num4, position.m_offset, bezier);
                    }

                    // NON-STOCK CODE END

                    if (flag2 && (maxSpeed3 < 0.01f || (instance2.m_segments.m_buffer[position2.m_segment].m_flags & (NetSegment.Flags.Collapsed | NetSegment.Flags.Flooded)) != 0))
                    {
                        if (index <= 0)
                        {
                            vehicleData.m_lastPathOffset = offset;
                        }
                        targetPos = bezier.a;
                        targetPos.w = 0f;
                        while (index < max2)
                        {
                            vehicleData.SetTargetPos(index++, targetPos);
                        }
                        break;
                    }
                    if (distance > 1f)
                    {
                        ushort num9;
                        switch (offset2)
                        {
                            case 0:
                                num9 = instance2.m_segments.m_buffer[position2.m_segment].m_startNode;
                                break;
                            case byte.MaxValue:
                                num9 = instance2.m_segments.m_buffer[position2.m_segment].m_endNode;
                                break;
                            default:
                                num9 = 0;
                                break;
                        }
                        float num10 = (float)Math.PI / 2f * (1f + Vector3.Dot(dir2, dir3));
                        if (distance > 1f)
                        {
                            num10 /= distance;
                        }
                        //maxSpeed3 = Mathf.Min(maxSpeed3, CalculateTargetSpeed(vehicleID, ref vehicleData, 1000f, num10));
                        //maxSpeed3 = Mathf.Min(maxSpeed3, CalculateTargetSpeedReversePatch.CalculateTargetSpeed(__instance, vehicleID, ref vehicleData, 1000f, num10));
                        maxSpeed3 = Mathf.Min(maxSpeed3, CalculateTargetSpeedStub(__instance, vehicleID, ref vehicleData, 1000f, num10));

                        while (offset < byte.MaxValue)
                        {
                            float num11 = Mathf.Max(Mathf.Sqrt(num) - Vector3.Distance(targetPos, refPos1), Mathf.Sqrt(num2) - Vector3.Distance(targetPos, refPos2));
                            int num12 = ((!(num11 < 0f)) ? (8 + Mathf.CeilToInt(num11 * 256f / (distance + 1f))) : 8);
                            offset = (byte)Mathf.Min(offset + num12, 255);
                            Vector3 vector = bezier.Position((float)(int)offset * 0.003921569f);
                            targetPos.Set(vector.x, vector.y, vector.z, Mathf.Min(targetPos.w, maxSpeed3));
                            float sqrMagnitude3 = (vector - refPos1).sqrMagnitude;
                            float sqrMagnitude4 = (vector - refPos2).sqrMagnitude;
                            if (sqrMagnitude3 >= num && sqrMagnitude4 >= num2)
                            {
                                if (index <= 0)
                                {
                                    vehicleData.m_lastPathOffset = offset;
                                }
                                if (num9 != 0)
                                {
                                    //UpdateNodeTargetPos(vehicleID, ref vehicleData, num9, ref instance2.m_nodes.m_buffer[num9], ref targetPos, index);
                                    //UpdateNodeTargetPosReversePatch.UpdateNodeTargetPos(__instance, vehicleID, ref vehicleData, num9, ref instance2.m_nodes.m_buffer[num9], ref targetPos, index);
                                    UpdateNodeTargetPosStub(__instance, vehicleID, ref vehicleData, num9, ref instance2.m_nodes.m_buffer[num9], ref targetPos, index);
                                }
                                vehicleData.SetTargetPos(index++, targetPos);
                                if (index < max1)
                                {
                                    num = minSqrDistanceB;
                                    refPos1 = targetPos;
                                }
                                else if (index == max1)
                                {
                                    num = (refPos2 - refPos1).sqrMagnitude;
                                    num2 = minSqrDistanceA;
                                }
                                else
                                {
                                    num2 = minSqrDistanceB;
                                    refPos2 = targetPos;
                                }
                                targetPos.w = 1000f;
                                if (index == max2)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
                else
                {
                    PathUnit.CalculatePathPositionOffset(laneID, targetPos, out offset2);
                }
                if (index <= 0)
                {
                    if (num7 == 0)
                    {
                        Singleton<PathManager>.instance.ReleaseFirstUnit(ref vehicleData.m_path);
                    }
                    if (num7 >= instance.m_pathUnits.m_buffer[num8].m_positionCount - 1 && instance.m_pathUnits.m_buffer[num8].m_nextPathUnit == 0 && leaderID != 0)
                    {
                        //ArrivingToDestination(leaderID, ref leaderData);
                        //ArrivingToDestinationReversePatch.ArrivingToDestination(__instance, leaderID, ref leaderData);
                        ArrivingToDestinationStub(__instance, leaderID, ref leaderData);
                    }
                }
                num3 = num8;
                b = (byte)(num7 << 1);
                offset = offset2;
                if (index <= 0)
                {
                    vehicleData.m_pathPositionIndex = b;
                    vehicleData.m_lastPathOffset = offset;
                    vehicleData.m_flags = (vehicleData.m_flags & ~(Vehicle.Flags.OnGravel | Vehicle.Flags.Underground | Vehicle.Flags.Transition)) | info.m_setVehicleFlags;
                    vehicleData.m_flags2 &= ~Vehicle.Flags2.Yielding;
                }
                position = position2;
                num4 = laneID;
            }

            return false;
        }

        private static void CheckNextLane(VehicleInfo m_info, ushort vehicleID, ref Vehicle vehicleData, ref float maxSpeed, PathUnit.Position position, uint laneID, byte offset, PathUnit.Position prevPos, uint prevLaneID, byte prevOffset, Bezier3 bezier)
        {
            NetManager instance = Singleton<NetManager>.instance;
            Vehicle.Frame lastFrameData = vehicleData.GetLastFrameData();
            Vector3 position2 = lastFrameData.m_position;
            Vector3 position3 = lastFrameData.m_position;
            Vector3 vector = lastFrameData.m_rotation * new Vector3(0f, 0f, m_info.m_generatedInfo.m_wheelBase * 0.5f);
            position2 += vector;
            position3 -= vector;
            float num = 0.5f * lastFrameData.m_velocity.sqrMagnitude / m_info.m_braking;
            float a = Vector3.Distance(position2, bezier.a);
            float b = Vector3.Distance(position3, bezier.a);
            if (!(Mathf.Min(a, b) >= num - 5f))
            {
                return;
            }
            if (!instance.m_lanes.m_buffer[laneID].CheckSpace(1000f, vehicleID))
            {
                vehicleData.m_flags2 |= Vehicle.Flags2.Yielding;
                vehicleData.m_waitCounter = 0;
                maxSpeed = 0f;
                return;
            }

            // 

            // unneeded STOCK code removed

            //
        }

        public static void InvalidPathStub(VehicleAI __instance, ushort vehicleID, ref Vehicle vehicleData, ushort leaderID, ref Vehicle leaderData)
        {
            throw new NotImplementedException("SimulationStepPatch2.InvalidPathStub - Harmony transpiler not applied");
        }

        public static void ArrivingToDestinationStub(VehicleAI __instance, ushort vehicleID, ref Vehicle vehicleData)
        {
            throw new NotImplementedException("SimulationStepPatch2.ArrivingToDestinationStub - Harmony transpiler not applied");
        }

        public static float CalculateTargetSpeedStub(VehicleAI __instance, ushort vehicleID, ref Vehicle data, float speedLimit, float curve)
        {
            throw new NotImplementedException("SimulationStepPatch2.CalculateTargetSpeedStub - Harmony transpiler not applied");
        }

        public static void UpdateNodeTargetPosStub(VehicleAI __instance, ushort vehicleID, ref Vehicle vehicleData, ushort nodeID, ref NetNode nodeData, ref Vector4 targetPos, int index)
        {
            throw new NotImplementedException("SimulationStepPatch2.UpdateNodeTargetPosStub - Harmony transpiler not applied");
        }

        public static void CalculateSegmentPositionStub1(VehicleAI __instance, ushort vehicleID, ref Vehicle vehicleData, PathUnit.Position position, uint laneID, byte offset, out Vector3 pos, out Vector3 dir, out float maxSpeed)
        {
            throw new NotImplementedException("SimulationStepPatch2.CalculateSegmentPositionStub1 - Harmony transpiler not applied");
        }

        public static void CalculateSegmentPositionStub2(VehicleAI __instance, ushort vehicleID, ref Vehicle vehicleData, PathUnit.Position nextPosition, PathUnit.Position position, uint laneID, byte offset, PathUnit.Position prevPos, uint prevLaneID, byte prevOffset, int index, out Vector3 pos, out Vector3 dir, out float maxSpeed)
        {
            throw new NotImplementedException("SimulationStepPatch2.CalculateSegmentPositionStub2 - Harmony transpiler not applied");
        }

        public static float CalculateMaxSpeedStub(float targetDistance, float targetSpeed, float maxBraking)
        {
            throw new NotImplementedException("SimulationStepPatch2.CalculateMaxSpeedStub - Harmony transpiler not applied");
        }

        public static float GetMaxSpeedStub(ushort leaderID, ref Vehicle leaderData)
        {
            throw new NotImplementedException("SimulationStepPatch2.GetMaxSpeedStub - Harmony transpiler not applied");
        }
    }

    [HarmonyPatch(typeof(SimulationStepPatch2), nameof(SimulationStepPatch2.UpdatePathTargetPositions))]
    public static class UpdatePathTargetPositionsTranspiler
    {
        private delegate void InvalidPathStubDelegate(VehicleAI __instance, ushort vehicleID, ref Vehicle vehicleData, ushort leaderID, ref Vehicle leaderData);
        private static MethodBase InvalidPathStubMethod() => Patcher.DeclaredMethod<InvalidPathStubDelegate>(typeof(SimulationStepPatch2), "InvalidPathStub");

        private delegate void InvalidPathDelegate(ushort vehicleID, ref Vehicle vehicleData, ushort leaderID, ref Vehicle leaderData);
        private static MethodBase InvalidPathMethod() => Patcher.DeclaredMethod<InvalidPathDelegate>(typeof(VehicleAI), "InvalidPath");


        private delegate void ArrivingToDestinationStubDelegate(VehicleAI __instance, ushort vehicleID, ref Vehicle vehicleData);
        private static MethodBase ArrivingToDestinationStubMethod() => Patcher.DeclaredMethod<ArrivingToDestinationStubDelegate>(typeof(SimulationStepPatch2), "ArrivingToDestinationStub");

        private delegate void ArrivingToDestinationDelegate(ushort vehicleID, ref Vehicle vehicleData);
        private static MethodBase ArrivingToDestinationMethod() => Patcher.DeclaredMethod<ArrivingToDestinationDelegate>(typeof(VehicleAI), "ArrivingToDestination");


        private delegate void CalculateTargetSpeedStubDelegate(VehicleAI __instance, ushort vehicleID, ref Vehicle data, float speedLimit, float curve);
        private static MethodBase CalculateTargetSpeedStubMethod() => Patcher.DeclaredMethod<CalculateTargetSpeedStubDelegate>(typeof(SimulationStepPatch2), "CalculateTargetSpeedStub");

        private delegate void CalculateTargetSpeedDelegate(ushort vehicleID, ref Vehicle data, float speedLimit, float curve);
        private static MethodBase CalculateTargetSpeedMethod() => Patcher.DeclaredMethod<CalculateTargetSpeedDelegate>(typeof(VehicleAI), "CalculateTargetSpeed");


        private delegate void UpdateNodeTargetPosStubDelegate(VehicleAI __instance, ushort vehicleID, ref Vehicle vehicleData, ushort nodeID, ref NetNode nodeData, ref Vector4 targetPos, int index);
        private static MethodBase UpdateNodeTargetPosStubMethod() => Patcher.DeclaredMethod<UpdateNodeTargetPosStubDelegate>(typeof(SimulationStepPatch2), "UpdateNodeTargetPosStub");

        private delegate void UpdateNodeTargetPosDelegate(ushort vehicleID, ref Vehicle vehicleData, ushort nodeID, ref NetNode nodeData, ref Vector4 targetPos, int index);
        private static MethodBase UpdateNodeTargetPosMethod() => Patcher.DeclaredMethod<UpdateNodeTargetPosDelegate>(typeof(VehicleAI), "UpdateNodeTargetPos");


        private delegate void CalculateSegmentPositionStub1Delegate(VehicleAI __instance, ushort vehicleID, ref Vehicle vehicleData, PathUnit.Position position, uint laneID, byte offset, out Vector3 pos, out Vector3 dir, out float maxSpeed);
        private static MethodBase CalculateSegmentPositionStub1Method() => Patcher.DeclaredMethod<CalculateSegmentPositionStub1Delegate>(typeof(SimulationStepPatch2), "CalculateSegmentPositionStub1");

        private delegate void CalculateSegmentPosition1Delegate(ushort vehicleID, ref Vehicle vehicleData, PathUnit.Position position, uint laneID, byte offset, out Vector3 pos, out Vector3 dir, out float maxSpeed);
        private static MethodBase CalculateSegmentPosition1Method() => Patcher.DeclaredMethod<CalculateSegmentPosition1Delegate>(typeof(TramBaseAI), "CalculateSegmentPosition");


        private delegate void CalculateSegmentPositionStub2Delegate(VehicleAI __instance, ushort vehicleID, ref Vehicle vehicleData, PathUnit.Position nextPosition, PathUnit.Position position, uint laneID, byte offset, PathUnit.Position prevPos, uint prevLaneID, byte prevOffset, int index, out Vector3 pos, out Vector3 dir, out float maxSpeed);
        private static MethodBase CalculateSegmentPositionStub2Method() => Patcher.DeclaredMethod<CalculateSegmentPositionStub2Delegate>(typeof(SimulationStepPatch2), "CalculateSegmentPositionStub2");

        private delegate void CalculateSegmentPosition2Delegate(ushort vehicleID, ref Vehicle vehicleData, PathUnit.Position nextPosition, PathUnit.Position position, uint laneID, byte offset, PathUnit.Position prevPos, uint prevLaneID, byte prevOffset, int index, out Vector3 pos, out Vector3 dir, out float maxSpeed);
        private static MethodBase CalculateSegmentPosition2Method() => Patcher.DeclaredMethod<CalculateSegmentPosition2Delegate>(typeof(TramBaseAI), "CalculateSegmentPosition");

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator il, IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = Patcher.ToCodeList(instructions);

            CodeInstruction invalidPathStubInstruction = new CodeInstruction(OpCodes.Call, InvalidPathStubMethod());
            CodeInstruction invalidPathInstruction = new CodeInstruction(OpCodes.Callvirt, InvalidPathMethod());

            CodeInstruction arrivingToDestinationStubInstruction = new CodeInstruction(OpCodes.Call, ArrivingToDestinationStubMethod());
            CodeInstruction arrivingToDestinationInstruction = new CodeInstruction(OpCodes.Callvirt, ArrivingToDestinationMethod());

            CodeInstruction calculateTargetSpeedStubInstruction = new CodeInstruction(OpCodes.Call, CalculateTargetSpeedStubMethod());
            CodeInstruction calculateTargetSpeedInstruction = new CodeInstruction(OpCodes.Callvirt, CalculateTargetSpeedMethod());

            CodeInstruction updateNodeTargetPosStubInstruction = new CodeInstruction(OpCodes.Call, UpdateNodeTargetPosStubMethod());
            CodeInstruction updateNodeTargetPosInstruction = new CodeInstruction(OpCodes.Callvirt, UpdateNodeTargetPosMethod());

            CodeInstruction calculateSegmentPositionStub1Instruction = new CodeInstruction(OpCodes.Call, CalculateSegmentPositionStub1Method());
            CodeInstruction calculateSegmentPosition1Instruction = new CodeInstruction(OpCodes.Callvirt, CalculateSegmentPosition1Method());

            CodeInstruction calculateSegmentPositionStub2Instruction = new CodeInstruction(OpCodes.Call, CalculateSegmentPositionStub2Method());
            CodeInstruction calculateSegmentPosition2Instruction = new CodeInstruction(OpCodes.Callvirt, CalculateSegmentPosition2Method());
            

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Call) continue;

                if (codes[i].opcode == invalidPathStubInstruction.opcode && codes[i].operand == invalidPathStubInstruction.operand)
                {
                    codes[i].opcode = invalidPathInstruction.opcode;
                    codes[i].operand = invalidPathInstruction.operand;
                }
                else if (codes[i].opcode == arrivingToDestinationStubInstruction.opcode && codes[i].operand == arrivingToDestinationStubInstruction.operand)
                {
                    codes[i].opcode = arrivingToDestinationInstruction.opcode;
                    codes[i].operand = arrivingToDestinationInstruction.operand;
                }
                else if (codes[i].opcode == calculateTargetSpeedStubInstruction.opcode && codes[i].operand == calculateTargetSpeedStubInstruction.operand)
                {
                    codes[i].opcode = calculateTargetSpeedInstruction.opcode;
                    codes[i].operand = calculateTargetSpeedInstruction.operand;
                }
                else if (codes[i].opcode == updateNodeTargetPosStubInstruction.opcode && codes[i].operand == updateNodeTargetPosStubInstruction.operand)
                {
                    codes[i].opcode = updateNodeTargetPosInstruction.opcode;
                    codes[i].operand = updateNodeTargetPosInstruction.operand;
                }
                else if (codes[i].opcode == calculateSegmentPositionStub1Instruction.opcode && codes[i].operand == calculateSegmentPositionStub1Instruction.operand)
                {
                    codes[i].opcode = calculateSegmentPosition1Instruction.opcode;
                    codes[i].operand = calculateSegmentPosition1Instruction.operand;
                }
                else if (codes[i].opcode == calculateSegmentPositionStub2Instruction.opcode && codes[i].operand == calculateSegmentPositionStub2Instruction.operand)
                {
                    codes[i].opcode = calculateSegmentPosition2Instruction.opcode;
                    codes[i].operand = calculateSegmentPosition2Instruction.operand;
                }
                
            }

            return codes;
        }

    }

    [HarmonyPatch(typeof(SimulationStepPatch2), nameof(SimulationStepPatch2.Prefix))]
    public static class SimulationStepPatch2Transpiler
    {
        private delegate void CalculateTargetSpeedStubDelegate(VehicleAI __instance, ushort vehicleID, ref Vehicle data, float speedLimit, float curve);
        private static MethodBase CalculateTargetSpeedStubMethod() => Patcher.DeclaredMethod<CalculateTargetSpeedStubDelegate>(typeof(SimulationStepPatch2), "CalculateTargetSpeedStub");

        private delegate void CalculateTargetSpeedDelegate(ushort vehicleID, ref Vehicle data, float speedLimit, float curve);
        private static MethodBase CalculateTargetSpeedMethod() => Patcher.DeclaredMethod<CalculateTargetSpeedDelegate>(typeof(VehicleAI), "CalculateTargetSpeed");


        private delegate void CalculateMaxSpeedStubDelegate(float targetDistance, float targetSpeed, float maxBraking);
        private static MethodBase CalculateMaxSpeedStubMethod() => Patcher.DeclaredMethod<CalculateMaxSpeedStubDelegate>(typeof(SimulationStepPatch2), "CalculateMaxSpeedStub");

        private delegate void CalculateMaxSpeedDelegate(float targetDistance, float targetSpeed, float maxBraking);
        private static MethodBase CalculateMaxSpeedMethod() => Patcher.DeclaredMethod<CalculateMaxSpeedDelegate>(typeof(TramBaseAI), "CalculateMaxSpeed");


        private delegate void GetMaxSpeedStubDelegate(ushort leaderID, ref Vehicle leaderData);
        private static MethodBase GetMaxSpeedStubMethod() => Patcher.DeclaredMethod<GetMaxSpeedStubDelegate>(typeof(SimulationStepPatch2), "GetMaxSpeedStub");

        private delegate void GetMaxSpeedDelegate(ushort leaderID, ref Vehicle leaderData);
        private static MethodBase GetMaxSpeedMethod() => Patcher.DeclaredMethod<GetMaxSpeedDelegate>(typeof(TramBaseAI), "GetMaxSpeed");


        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator il, IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = Patcher.ToCodeList(instructions);

            CodeInstruction calculateTargetSpeedStubInstruction = new CodeInstruction(OpCodes.Call, CalculateTargetSpeedStubMethod());
            CodeInstruction calculateTargetSpeedInstruction = new CodeInstruction(OpCodes.Callvirt, CalculateTargetSpeedMethod());

            CodeInstruction calculateMaxSpeedStubInstruction = new CodeInstruction(OpCodes.Call, CalculateMaxSpeedStubMethod());
            CodeInstruction calculateMaxSpeedInstruction = new CodeInstruction(OpCodes.Call, CalculateMaxSpeedMethod());

            CodeInstruction getMaxSpeedStubInstruction = new CodeInstruction(OpCodes.Call, GetMaxSpeedStubMethod());
            CodeInstruction getMaxSpeedInstruction = new CodeInstruction(OpCodes.Call, GetMaxSpeedMethod());

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Call) continue;

                if (codes[i].opcode == calculateTargetSpeedStubInstruction.opcode && codes[i].operand == calculateTargetSpeedStubInstruction.operand)
                {
                    codes[i].opcode = calculateTargetSpeedInstruction.opcode;
                    codes[i].operand = calculateTargetSpeedInstruction.operand;
                }
                else if (codes[i].opcode == calculateMaxSpeedStubInstruction.opcode && codes[i].operand == calculateMaxSpeedStubInstruction.operand)
                {
                    codes[i].opcode = calculateMaxSpeedInstruction.opcode;
                    codes[i].operand = calculateMaxSpeedInstruction.operand;
                }
                else if (codes[i].opcode == getMaxSpeedStubInstruction.opcode && codes[i].operand == getMaxSpeedStubInstruction.operand)
                {
                    codes[i].opcode = getMaxSpeedInstruction.opcode;
                    codes[i].operand = getMaxSpeedInstruction.operand;
                }
            }

            return codes;
        }

    }
}
