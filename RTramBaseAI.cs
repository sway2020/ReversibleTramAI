using System;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace ReversibleTramAI
{
    /// <summary>
    /// Modified from vanilla TramBaseAI 
    /// </summary>
    public class RTramBaseAI : VehicleAI
    {
        public TransportInfo m_transportInfo;

        /// <summary>
        /// Modified from vanilla TramBaseAI
        /// </summary>
        public override void SimulationStep(ushort vehicleID, ref Vehicle data, Vector3 physicsLodRefPos)
        {
            if ((data.m_flags & Vehicle.Flags.WaitingPath) != 0)
            {
                byte pathFindFlags = Singleton<PathManager>.instance.m_pathUnits.m_buffer[data.m_path].m_pathFindFlags;
                if ((pathFindFlags & 4u) != 0)
                {
                    PathfindSuccess(vehicleID, ref data);
                    PathFindReady(vehicleID, ref data);
                }
                else if ((pathFindFlags & 8u) != 0 || data.m_path == 0)
                {
                    data.m_flags &= ~Vehicle.Flags.WaitingPath;
                    Singleton<PathManager>.instance.ReleasePath(data.m_path);
                    data.m_path = 0u;
                    PathfindFailure(vehicleID, ref data);
                    return;
                }
            }
            else if ((data.m_flags & Vehicle.Flags.WaitingSpace) != 0)
            {
                TrySpawn(vehicleID, ref data);
            }


            // NON-STOCK CODE START. Code in TrainAI but not TramBaseAI

            bool flag = (data.m_flags & Vehicle.Flags.Reversed) != 0;
            ushort num = ((!flag) ? vehicleID : data.GetLastVehicle(vehicleID));

            // NON-STOCK CODE END


            VehicleManager instance = Singleton<VehicleManager>.instance;
            VehicleInfo info = instance.m_vehicles.m_buffer[num].Info;
            info.m_vehicleAI.SimulationStep(num, ref instance.m_vehicles.m_buffer[num], vehicleID, ref data, 0);
            if ((data.m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Deleted)) != Vehicle.Flags.Created)
            {
                return;
            }


            // NON-STOCK CODE START. Code in TrainAI but not TramBaseAI


            bool flag2 = (data.m_flags & Vehicle.Flags.Reversed) != 0;
            if (flag2 != flag)
            {
                flag = flag2;
                num = ((!flag) ? vehicleID : data.GetLastVehicle(vehicleID));
                info = instance.m_vehicles.m_buffer[num].Info;
                info.m_vehicleAI.SimulationStep(num, ref instance.m_vehicles.m_buffer[num], vehicleID, ref data, 0);
                if ((data.m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Deleted)) != Vehicle.Flags.Created)
                {
                    return;
                }
                flag2 = (data.m_flags & Vehicle.Flags.Reversed) != 0;
                if (flag2 != flag)
                {
                    Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleID);
                    return;
                }
            }

            if (flag)
            {
                num = instance.m_vehicles.m_buffer[num].m_leadingVehicle;
                int num2 = 0;
                while (num != 0)
                {
                    info = instance.m_vehicles.m_buffer[num].Info;
                    info.m_vehicleAI.SimulationStep(num, ref instance.m_vehicles.m_buffer[num], vehicleID, ref data, 0);
                    if ((data.m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Deleted)) != Vehicle.Flags.Created)
                    {
                        return;
                    }
                    num = instance.m_vehicles.m_buffer[num].m_leadingVehicle;
                    if (++num2 > 16384)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                }
            }


            // NON-STOCK CODE END


            // Code in the following else block is from the original TramBaseAI. The original AI doesn't have the above if block
            else
            {
                num = instance.m_vehicles.m_buffer[num].m_trailingVehicle;
                int num2 = 0;
                while (num != 0)
                {
                    info = instance.m_vehicles.m_buffer[num].Info;
                    info.m_vehicleAI.SimulationStep(num, ref instance.m_vehicles.m_buffer[num], vehicleID, ref data, 0);
                    if ((data.m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Deleted)) != Vehicle.Flags.Created)
                    {
                        return;
                    }
                    num = instance.m_vehicles.m_buffer[num].m_trailingVehicle;
                    if (++num2 > 16384)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                }
            }
            if ((data.m_flags & (Vehicle.Flags.Spawned | Vehicle.Flags.WaitingPath | Vehicle.Flags.WaitingSpace | Vehicle.Flags.WaitingCargo)) == 0 || data.m_blockCounter == byte.MaxValue)
            {
                Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleID);
            }
        }

        /// <summary>
        /// Modified from vanilla trainAI
        /// </summary>
        public override void SimulationStep(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            bool reversedFlag = (leaderData.m_flags & Vehicle.Flags.Reversed) != 0;
            ushort leadingVehicle = ((!reversedFlag) ? vehicleData.m_leadingVehicle : vehicleData.m_trailingVehicle);
            VehicleInfo vehicleInfo = ((leaderID == vehicleID) ? m_info : leaderData.Info);
            RTramBaseAI tramBaseAI = vehicleInfo.m_vehicleAI as RTramBaseAI;
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
            Vector3 vector2 = frameData.m_rotation * new Vector3(0f, 0f, m_info.m_generatedInfo.m_wheelBase * 0.5f);
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
            float acceleration = m_info.m_acceleration;
            float braking = m_info.m_braking;
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
                float num5 = (((vehicleData.m_flags & Vehicle.Flags.Inverted) != 0 == reversedFlag) ? (m_info.m_attachOffsetFront - m_info.m_generatedInfo.m_size.z * 0.5f) : (m_info.m_attachOffsetBack - m_info.m_generatedInfo.m_size.z * 0.5f));
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
                    tramBaseAI.UpdatePathTargetPositions(vehicleID, ref vehicleData, vehicleData.m_targetPos0, position, 0, ref leaderData, ref index, 0, 0, Vector3.SqrMagnitude(position - (Vector3)vehicleData.m_targetPos0) + 1f, 1f);
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
                        tramBaseAI.UpdatePathTargetPositions(vehicleID, ref vehicleData, position, vector, 0, ref leaderData, ref i, 1, 2, num9, minSqrDistanceB);
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
                        instance2.m_segments.m_buffer[position5.m_segment].AddTraffic(Mathf.RoundToInt(m_info.m_generatedInfo.m_size.z * 3f), GetNoiseLevel());
                        if ((b & 1) == 0 || lastPathOffset == 0 || (leaderData.m_flags & Vehicle.Flags.WaitingPath) != 0)
                        {
                            uint laneID = PathManager.GetLaneID(position5);
                            if (laneID != 0)
                            {
                                instance2.m_lanes.m_buffer[laneID].ReserveSpace(m_info.m_generatedInfo.m_size.z);
                            }
                        }
                        else if (instance3.m_pathUnits.m_buffer[vehicleData.m_path].GetNextPosition(b >> 1, out position5))
                        {
                            uint laneID2 = PathManager.GetLaneID(position5);
                            if (laneID2 != 0)
                            {
                                instance2.m_lanes.m_buffer[laneID2].ReserveSpace(m_info.m_generatedInfo.m_size.z);
                            }
                        }
                    }
                }
                vector3 = quaternion * vector3;
                float num10 = (m_info.m_generatedInfo.m_wheelBase + info.m_generatedInfo.m_wheelBase) * -0.5f - num5 - num6;
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

                float num12 = (magnitude + acceleration) * (0.5f + 0.5f * (magnitude + acceleration) / braking) + (m_info.m_generatedInfo.m_size.z - m_info.m_generatedInfo.m_wheelBase) * 0.5f;
                // float num12 = (magnitude + acceleration) * (0.5f + 0.5f * (magnitude + acceleration) / braking);

                // Non-stock code END

                float num13 = Mathf.Max(magnitude + acceleration, 2f);
                float num14 = Mathf.Max((num12 - num13) / 2f, 1f);
                float num15 = num13 * num13;
                float minSqrDistanceB2 = num14 * num14;
                if (Vector3.Dot((Vector3)vehicleData.m_targetPos1 - (Vector3)vehicleData.m_targetPos0, (Vector3)vehicleData.m_targetPos0 - position) < 0f && vehicleData.m_path != 0 && (leaderData.m_flags & (Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped)) == 0)
                {
                    int index2 = -1;
                    tramBaseAI.UpdatePathTargetPositions(vehicleID, ref vehicleData, vehicleData.m_targetPos0, position, leaderID, ref leaderData, ref index2, 0, 0, Vector3.SqrMagnitude(position - (Vector3)vehicleData.m_targetPos0) + 1f, 1f);
                    num2 = 0f;
                }
                int j = 0;
                bool flag3 = false;
                if ((num2 < num15 || vehicleData.m_targetPos3.w < 0.01f) && (leaderData.m_flags & (Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped)) == 0)
                {
                    if (vehicleData.m_path != 0)
                    {
                        tramBaseAI.UpdatePathTargetPositions(vehicleID, ref vehicleData, position, vector, leaderID, ref leaderData, ref j, 1, 4, num15, minSqrDistanceB2);
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
                        instance4.m_segments.m_buffer[position6.m_segment].AddTraffic(Mathf.RoundToInt(m_info.m_generatedInfo.m_size.z * 3f), GetNoiseLevel());
                        if ((b2 & 1) == 0 || lastPathOffset2 == 0 || (leaderData.m_flags & Vehicle.Flags.WaitingPath) != 0)
                        {
                            uint laneID3 = PathManager.GetLaneID(position6);
                            if (laneID3 != 0)
                            {
                                instance4.m_lanes.m_buffer[laneID3].ReserveSpace(m_info.m_generatedInfo.m_size.z, vehicleID);
                            }
                        }
                        else if (instance5.m_pathUnits.m_buffer[vehicleData.m_path].GetNextPosition(b2 >> 1, out position6))
                        {
                            uint laneID4 = PathManager.GetLaneID(position6);
                            if (laneID4 != 0)
                            {
                                instance4.m_lanes.m_buffer[laneID4].ReserveSpace(m_info.m_generatedInfo.m_size.z, vehicleID);
                            }
                        }
                    }
                }
                float maxSpeed = (((leaderData.m_flags & Vehicle.Flags.Stopped) == 0) ? Mathf.Min(vehicleData.m_targetPos1.w, GetMaxSpeed(leaderID, ref leaderData)) : 0f);
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
                                            Reverse(leaderID, ref leaderData);
                                            return;
                                        }
                                        v.z = 0f;
                                        vector3 = Vector3.zero;
                                        maxSpeed = 0f;
                                    }
                                    else
                                    {
                                        vector = position + Vector3.Normalize(vehicleData.m_targetPos1 - vehicleData.m_targetPos0) * m_info.m_generatedInfo.m_wheelBase;
                                        j = -1;
                                        tramBaseAI.UpdatePathTargetPositions(vehicleID, ref vehicleData, vehicleData.m_targetPos0, vehicleData.m_targetPos1, leaderID, ref leaderData, ref j, 0, 0, Vector3.SqrMagnitude(vehicleData.m_targetPos1 - vehicleData.m_targetPos0) + 1f, 1f);
                                    }
                                }
                                else
                                {
                                    j = -1;
                                    tramBaseAI.UpdatePathTargetPositions(vehicleID, ref vehicleData, vehicleData.m_targetPos0, position, leaderID, ref leaderData, ref j, 0, 0, Vector3.SqrMagnitude(position - (Vector3)vehicleData.m_targetPos0) + 1f, 1f);
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
                        maxSpeed = Mathf.Min(maxSpeed, CalculateTargetSpeed(vehicleID, ref vehicleData, 1000f, num18));
                        float num19 = len;
                        maxSpeed = Mathf.Min(maxSpeed, CalculateMaxSpeed(num19, vehicleData.m_targetPos2.w, braking));
                        num19 += VectorUtils.LengthXZ(vehicleData.m_targetPos2 - vehicleData.m_targetPos1);
                        maxSpeed = Mathf.Min(maxSpeed, CalculateMaxSpeed(num19, vehicleData.m_targetPos3.w, braking));
                        num19 += VectorUtils.LengthXZ(vehicleData.m_targetPos3 - vehicleData.m_targetPos2);

                        // NON-STOCK CODE START. Code not in TrainAI but should be in TramBaseAI

                        if (vehicleData.m_targetPos3.w < 0.01f)
                        {
                            num19 = Mathf.Max(0f, num19 + (m_info.m_generatedInfo.m_wheelBase - m_info.m_generatedInfo.m_size.z) * 0.5f);
                        }
                        maxSpeed = Mathf.Min(maxSpeed, CalculateMaxSpeed(num19, 0f, braking));

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
                    return;
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
                vector12 = vector + frameData.m_rotation * new Vector3(0f, 0f, m_info.m_generatedInfo.m_wheelBase * 0.5f);
            }
            else
            {
                frameData.m_rotation = Quaternion.LookRotation(vector - position);
                vector12 = vector - frameData.m_rotation * new Vector3(0f, 0f, m_info.m_generatedInfo.m_wheelBase * 0.5f);
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
            frameData.m_swayVelocity = frameData.m_swayVelocity * (1f - m_info.m_dampers) - vector9 * (1f - m_info.m_springs) - frameData.m_swayPosition * m_info.m_springs;
            frameData.m_swayPosition += frameData.m_swayVelocity * 0.5f;
            frameData.m_steerAngle = 0f;
            frameData.m_travelDistance += vector6.z;
            frameData.m_lightIntensity.x = ((!reversedFlag) ? 5f : 0f);
            frameData.m_lightIntensity.y = ((!reversedFlag) ? 0f : 5f);
            frameData.m_lightIntensity.z = 0f;
            frameData.m_lightIntensity.w = 0f;
            frameData.m_underground = (vehicleData.m_flags & Vehicle.Flags.Underground) != 0;
            frameData.m_transition = (vehicleData.m_flags & Vehicle.Flags.Transition) != 0;
            base.SimulationStep(vehicleID, ref vehicleData, ref frameData, leaderID, ref leaderData, lodPhysics);
        }

        /// <summary>
        /// Slightly modified from vanilla TramBaseAI
        /// </summary>
        protected void UpdatePathTargetPositions(ushort vehicleID, ref Vehicle vehicleData, Vector3 refPos1, Vector3 refPos2, ushort leaderID, ref Vehicle leaderData, ref int index, int max1, int max2, float minSqrDistanceA, float minSqrDistanceB)
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
                    InvalidPath(vehicleID, ref vehicleData, leaderID, ref leaderData);
                    return;
                }
            }
            if (!instance.m_pathUnits.m_buffer[num3].GetPosition(b >> 1, out var position))
            {
                InvalidPath(vehicleID, ref vehicleData, leaderID, ref leaderData);
                return;
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
                        CalculateSegmentPosition(vehicleID, ref vehicleData, position, num4, offset, out var pos, out var _, out var maxSpeed);
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
                                return;
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
                    InvalidPath(vehicleID, ref vehicleData, leaderID, ref leaderData);
                    break;
                }
                NetInfo info = instance2.m_segments.m_buffer[position2.m_segment].Info;
                if (info.m_lanes.Length <= position2.m_lane)
                {
                    InvalidPath(vehicleID, ref vehicleData, leaderID, ref leaderData);
                    break;
                }
                uint laneID = PathManager.GetLaneID(position2);
                NetInfo.Lane lane = info.m_lanes[position2.m_lane];
                if (lane.m_laneType != NetInfo.LaneType.Vehicle)
                {
                    InvalidPath(vehicleID, ref vehicleData, leaderID, ref leaderData);
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
                    CalculateSegmentPosition(vehicleID, ref vehicleData, position, num4, position.m_offset, out bezier.a, out var dir2, out var _);


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
                        CalculateSegmentPosition(vehicleID, ref vehicleData, position3, position2, laneID, offset2, position, num4, position.m_offset, index, out bezier.d, out dir3, out maxSpeed3);
                    }
                    else
                    {
                        CalculateSegmentPosition(vehicleID, ref vehicleData, position2, laneID, offset2, out bezier.d, out dir3, out maxSpeed3);
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
                        CheckNextLane(vehicleID, ref vehicleData, ref maxSpeed3, position2, laneID, offset2, position, num4, position.m_offset, bezier);
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
                        maxSpeed3 = Mathf.Min(maxSpeed3, CalculateTargetSpeed(vehicleID, ref vehicleData, 1000f, num10));
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
                                    UpdateNodeTargetPos(vehicleID, ref vehicleData, num9, ref instance2.m_nodes.m_buffer[num9], ref targetPos, index);
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
                                    return;
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
                        ArrivingToDestination(leaderID, ref leaderData);
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
        }

        /// <summary>
        /// Taken from vanilla train AI. Unchanged
        /// </summary>
        private static void ResetTargets(ushort vehicleID, ref Vehicle vehicleData, ushort leaderID, ref Vehicle leaderData, bool pushPathPos)
        {
            Vehicle.Frame lastFrameData = vehicleData.GetLastFrameData();
            VehicleInfo info = vehicleData.Info;
            RTramBaseAI tramBaseAI = info.m_vehicleAI as RTramBaseAI;
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
                tramBaseAI.UpdatePathTargetPositions(vehicleID, ref vehicleData, position2, position, 0, ref leaderData, ref index, 1, 4, 4f, 1f);
            }
        }

        /// <summary>
        /// Taken from vanilla train AI, unchanged
        /// </summary>
        private static void Reverse(ushort leaderID, ref Vehicle leaderData)
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
                ResetTargets(num, ref instance.m_vehicles.m_buffer[num], leaderID, ref leaderData, pushPathPos: true);
                instance.m_vehicles.m_buffer[num].m_flags = (instance.m_vehicles.m_buffer[num].m_flags & ~Vehicle.Flags.Reversed) | (leaderData.m_flags & Vehicle.Flags.Reversed);
                num = instance.m_vehicles.m_buffer[num].m_trailingVehicle;
                if (++num2 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }

        /// <summary>
        /// Modified from vanilla train AI
        /// </summary>
        private void CheckNextLane(ushort vehicleID, ref Vehicle vehicleData, ref float maxSpeed, PathUnit.Position position, uint laneID, byte offset, PathUnit.Position prevPos, uint prevLaneID, byte prevOffset, Bezier3 bezier)
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

        /// <summary>
        /// Modified from vanilla tramBaseAI
        /// </summary>
        public override void FrameDataUpdated(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData)
        {
            Vector3 vector = frameData.m_position + frameData.m_velocity * 0.5f;
            Vector3 vector2 = frameData.m_rotation * new Vector3(0f, 0f, Mathf.Max(0.5f, m_info.m_generatedInfo.m_size.z * 0.5f - 1f));

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
        }



        /*                                                     */
        /* Everything below is the same as the vanilla tram AI */
        /*                                                     */


        public override Color GetColor(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode)
        {
            if (data.m_leadingVehicle != 0 && infoMode != 0)
            {
                VehicleManager instance = Singleton<VehicleManager>.instance;
                ushort firstVehicle = data.GetFirstVehicle(vehicleID);
                VehicleInfo info = instance.m_vehicles.m_buffer[firstVehicle].Info;
                return info.m_vehicleAI.GetColor(firstVehicle, ref instance.m_vehicles.m_buffer[firstVehicle], infoMode);
            }
            if (infoMode == InfoManager.InfoMode.NoisePollution)
            {
                int noiseLevel = GetNoiseLevel();
                return CommonBuildingAI.GetNoisePollutionColor((float)noiseLevel * 2.5f);
            }
            return base.GetColor(vehicleID, ref data, infoMode);
        }
        public override void LoadVehicle(ushort vehicleID, ref Vehicle data)
        {
            base.LoadVehicle(vehicleID, ref data);
            if (data.m_leadingVehicle != 0 || data.m_trailingVehicle == 0 || (data.m_flags & Vehicle.Flags.Reversed) == 0)
            {
                return;
            }
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort num = data.m_trailingVehicle;
            int num2 = 0;
            while (num != 0)
            {
                ushort trailingVehicle = instance.m_vehicles.m_buffer[num].m_trailingVehicle;
                instance.m_vehicles.m_buffer[num].m_flags |= Vehicle.Flags.Reversed;
                num = trailingVehicle;
                if (++num2 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }

        protected virtual void PathfindSuccess(ushort vehicleID, ref Vehicle data)
        {
        }

        protected virtual void PathfindFailure(ushort vehicleID, ref Vehicle data)
        {
            data.Unspawn(vehicleID);
        }

        private static bool CheckOverlap(ushort vehicleID, ref Vehicle vehicleData, Segment3 segment, ushort ignoreVehicle)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            Vector3 min = segment.Min();
            Vector3 max = segment.Max();
            int num = Mathf.Max((int)((min.x - 30f) / 32f + 270f), 0);
            int num2 = Mathf.Max((int)((min.z - 30f) / 32f + 270f), 0);
            int num3 = Mathf.Min((int)((max.x + 30f) / 32f + 270f), 539);
            int num4 = Mathf.Min((int)((max.z + 30f) / 32f + 270f), 539);
            bool overlap = false;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort num5 = instance.m_vehicleGrid[i * 540 + j];
                    int num6 = 0;
                    while (num5 != 0)
                    {
                        num5 = CheckOverlap(vehicleID, ref vehicleData, segment, ignoreVehicle, num5, ref instance.m_vehicles.m_buffer[num5], ref overlap, min, max);
                        if (++num6 > 16384)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return overlap;
        }

        private static ushort CheckOverlap(ushort vehicleID, ref Vehicle vehicleData, Segment3 segment, ushort ignoreVehicle, ushort otherID, ref Vehicle otherData, ref bool overlap, Vector3 min, Vector3 max)
        {
            if (ignoreVehicle == 0 || (otherID != ignoreVehicle && otherData.m_leadingVehicle != ignoreVehicle && otherData.m_trailingVehicle != ignoreVehicle))
            {
                VehicleInfo info = otherData.Info;
                if (info.m_vehicleType == VehicleInfo.VehicleType.Bicycle)
                {
                    return otherData.m_nextGridVehicle;
                }
                if (((vehicleData.m_flags | otherData.m_flags) & Vehicle.Flags.Transition) == 0 && (vehicleData.m_flags & Vehicle.Flags.Underground) != (otherData.m_flags & Vehicle.Flags.Underground))
                {
                    return otherData.m_nextGridVehicle;
                }
                Vector3 vector = Vector3.Min(otherData.m_segment.Min(), otherData.m_targetPos3);
                Vector3 vector2 = Vector3.Max(otherData.m_segment.Max(), otherData.m_targetPos3);
                if (min.x < vector2.x + 2f && min.y < vector2.y + 2f && min.z < vector2.z + 2f && vector.x < max.x + 2f && vector.y < max.y + 2f && vector.z < max.z + 2f)
                {
                    Vector3 rhs = Vector3.Normalize(segment.b - segment.a);
                    Vector3 lhs = otherData.m_segment.a - vehicleData.m_segment.b;
                    Vector3 lhs2 = otherData.m_segment.b - vehicleData.m_segment.b;
                    if (Vector3.Dot(lhs, rhs) >= 1f || Vector3.Dot(lhs2, rhs) >= 1f)
                    {
                        float num = segment.DistanceSqr(otherData.m_segment, out var u, out var v);
                        if (num < 4f)
                        {
                            overlap = true;
                        }
                        Vector3 a = otherData.m_segment.b;
                        segment.a.y *= 0.5f;
                        segment.b.y *= 0.5f;
                        for (int i = 0; i < 4; i++)
                        {
                            Vector3 vector3 = otherData.GetTargetPos(i);
                            Segment3 segment2 = new Segment3(a, vector3);
                            segment2.a.y *= 0.5f;
                            segment2.b.y *= 0.5f;
                            if (segment2.LengthSqr() > 0.01f)
                            {
                                num = segment.DistanceSqr(segment2, out u, out v);
                                if (num < 4f)
                                {
                                    overlap = true;
                                    break;
                                }
                            }
                            a = vector3;
                        }
                    }
                }
            }
            return otherData.m_nextGridVehicle;
        }

        protected override void CalculateSegmentPosition(ushort vehicleID, ref Vehicle vehicleData, PathUnit.Position nextPosition, PathUnit.Position position, uint laneID, byte offset, PathUnit.Position prevPos, uint prevLaneID, byte prevOffset, int index, out Vector3 pos, out Vector3 dir, out float maxSpeed)
        {
            NetManager instance = Singleton<NetManager>.instance;
            instance.m_lanes.m_buffer[laneID].CalculatePositionAndDirection((float)(int)offset * 0.003921569f, out pos, out dir);
            Vector3 b = instance.m_lanes.m_buffer[prevLaneID].CalculatePosition((float)(int)prevOffset * 0.003921569f);
            Vehicle.Frame lastFrameData = vehicleData.GetLastFrameData();
            Vector3 position2 = lastFrameData.m_position;
            Vector3 position3 = lastFrameData.m_position;
            Vector3 vector = lastFrameData.m_rotation * new Vector3(0f, 0f, m_info.m_generatedInfo.m_wheelBase * 0.5f);
            position2 += vector;
            position3 -= vector;
            float sqrMagnitude = lastFrameData.m_velocity.sqrMagnitude;
            float num = 0.5f * sqrMagnitude / m_info.m_braking;
            float a = Vector3.Distance(position2, b);
            float b2 = Vector3.Distance(position3, b);
            if (Mathf.Min(a, b2) >= num - 1f)
            {
                Segment3 segment = default(Segment3);
                segment.a = pos;
                ushort num2;
                ushort num3;
                if (offset < position.m_offset)
                {
                    segment.b = pos + dir.normalized * m_info.m_generatedInfo.m_size.z;
                    num2 = instance.m_segments.m_buffer[position.m_segment].m_startNode;
                    num3 = instance.m_segments.m_buffer[position.m_segment].m_endNode;
                }
                else
                {
                    segment.b = pos - dir.normalized * m_info.m_generatedInfo.m_size.z;
                    num2 = instance.m_segments.m_buffer[position.m_segment].m_endNode;
                    num3 = instance.m_segments.m_buffer[position.m_segment].m_startNode;
                }
                ushort num4 = ((prevOffset != 0) ? instance.m_segments.m_buffer[prevPos.m_segment].m_endNode : instance.m_segments.m_buffer[prevPos.m_segment].m_startNode);
                if (num2 == num4)
                {
                    NetNode.Flags flags = instance.m_nodes.m_buffer[num2].m_flags;
                    NetLane.Flags flags2 = (NetLane.Flags)instance.m_lanes.m_buffer[prevLaneID].m_flags;
                    bool flag = (flags & NetNode.Flags.TrafficLights) != 0;
                    bool flag2 = (flags & NetNode.Flags.LevelCrossing) != 0;
                    bool flag3 = (flags2 & NetLane.Flags.JoinedJunction) != 0;
                    if ((flags2 & (NetLane.Flags.YieldStart | NetLane.Flags.YieldEnd)) != 0 && (flags & (NetNode.Flags.Junction | NetNode.Flags.TrafficLights | NetNode.Flags.OneWayIn)) == NetNode.Flags.Junction && (vehicleData.m_flags2 & Vehicle.Flags2.Yielding) == 0)
                    {
                        if (sqrMagnitude < 0.01f)
                        {
                            vehicleData.m_flags2 |= Vehicle.Flags2.Yielding;
                        }
                        maxSpeed = 0f;
                        return;
                    }
                    if ((flags & (NetNode.Flags.Junction | NetNode.Flags.OneWayOut | NetNode.Flags.OneWayIn)) == NetNode.Flags.Junction && instance.m_nodes.m_buffer[num2].CountSegments() != 2)
                    {
                        float len = vehicleData.CalculateTotalLength(vehicleID) + 2f;
                        if (!instance.m_lanes.m_buffer[laneID].CheckSpace(len))
                        {
                            bool flag4 = false;
                            if (nextPosition.m_segment != 0 && instance.m_lanes.m_buffer[laneID].m_length < 30f)
                            {
                                NetNode.Flags flags3 = instance.m_nodes.m_buffer[num3].m_flags;
                                if ((flags3 & (NetNode.Flags.Junction | NetNode.Flags.OneWayOut | NetNode.Flags.OneWayIn)) != NetNode.Flags.Junction || instance.m_nodes.m_buffer[num3].CountSegments() == 2)
                                {
                                    uint laneID2 = PathManager.GetLaneID(nextPosition);
                                    if (laneID2 != 0)
                                    {
                                        flag4 = instance.m_lanes.m_buffer[laneID2].CheckSpace(len);
                                    }
                                }
                            }
                            if (!flag4)
                            {
                                vehicleData.m_flags2 |= Vehicle.Flags2.Yielding;
                                maxSpeed = 0f;
                                return;
                            }
                        }
                    }
                    if (flag && (!flag3 || flag2))
                    {
                        uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
                        uint num5 = (uint)(num4 << 8) / 32768u;
                        uint num6 = (currentFrameIndex - num5) & 0xFFu;
                        RoadBaseAI.GetTrafficLightState(num4, ref instance.m_segments.m_buffer[prevPos.m_segment], currentFrameIndex - num5, out var vehicleLightState, out var pedestrianLightState, out var vehicles, out var pedestrians);
                        if (!vehicles && num6 >= 196)
                        {
                            vehicles = true;
                            RoadBaseAI.SetTrafficLightState(num4, ref instance.m_segments.m_buffer[prevPos.m_segment], currentFrameIndex - num5, vehicleLightState, pedestrianLightState, vehicles, pedestrians);
                        }
                        switch (vehicleLightState)
                        {
                            case RoadBaseAI.TrafficLightState.RedToGreen:
                                if (num6 < 60)
                                {
                                    vehicleData.m_flags2 |= Vehicle.Flags2.Yielding;
                                    maxSpeed = 0f;
                                    return;
                                }
                                break;
                            case RoadBaseAI.TrafficLightState.GreenToRed:
                                if (num6 >= 30)
                                {
                                    vehicleData.m_flags2 |= Vehicle.Flags2.Yielding;
                                    maxSpeed = 0f;
                                    return;
                                }
                                break;
                            case RoadBaseAI.TrafficLightState.Red:
                                vehicleData.m_flags2 |= Vehicle.Flags2.Yielding;
                                maxSpeed = 0f;
                                return;
                        }
                    }
                }
            }
            NetInfo info = instance.m_segments.m_buffer[position.m_segment].Info;
            if (info.m_lanes != null && info.m_lanes.Length > position.m_lane)
            {
                maxSpeed = CalculateTargetSpeed(vehicleID, ref vehicleData, info.m_lanes[position.m_lane].m_speedLimit, instance.m_lanes.m_buffer[laneID].m_curve);
            }
            else
            {
                maxSpeed = CalculateTargetSpeed(vehicleID, ref vehicleData, 1f, 0f);
            }
        }

        protected override void CalculateSegmentPosition(ushort vehicleID, ref Vehicle vehicleData, PathUnit.Position position, uint laneID, byte offset, out Vector3 pos, out Vector3 dir, out float maxSpeed)
        {
            NetManager instance = Singleton<NetManager>.instance;
            instance.m_lanes.m_buffer[laneID].CalculatePositionAndDirection((float)(int)offset * 0.003921569f, out pos, out dir);
            NetInfo info = instance.m_segments.m_buffer[position.m_segment].Info;
            if (info.m_lanes != null && info.m_lanes.Length > position.m_lane)
            {
                maxSpeed = CalculateTargetSpeed(vehicleID, ref vehicleData, info.m_lanes[position.m_lane].m_speedLimit, instance.m_lanes.m_buffer[laneID].m_curve);
            }
            else
            {
                maxSpeed = CalculateTargetSpeed(vehicleID, ref vehicleData, 1f, 0f);
            }
        }

        private static float GetMaxSpeed(ushort leaderID, ref Vehicle leaderData)
        {
            float num = 1000000f;
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort num2 = leaderID;
            int num3 = 0;
            while (num2 != 0)
            {
                num = Mathf.Min(num, instance.m_vehicles.m_buffer[num2].m_targetPos0.w);
                num = Mathf.Min(num, instance.m_vehicles.m_buffer[num2].m_targetPos1.w);
                num2 = instance.m_vehicles.m_buffer[num2].m_trailingVehicle;
                if (++num3 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return num;
        }

        private static void InitializePath(ushort vehicleID, ref Vehicle vehicleData)
        {
            PathManager instance = Singleton<PathManager>.instance;
            VehicleManager instance2 = Singleton<VehicleManager>.instance;
            ushort trailingVehicle = vehicleData.m_trailingVehicle;
            int num = 0;
            while (trailingVehicle != 0)
            {
                if (instance2.m_vehicles.m_buffer[trailingVehicle].m_path != 0)
                {
                    instance.ReleasePath(instance2.m_vehicles.m_buffer[trailingVehicle].m_path);
                    instance2.m_vehicles.m_buffer[trailingVehicle].m_path = 0u;
                }
                if (instance.AddPathReference(vehicleData.m_path))
                {
                    instance2.m_vehicles.m_buffer[trailingVehicle].m_path = vehicleData.m_path;
                    instance2.m_vehicles.m_buffer[trailingVehicle].m_pathPositionIndex = 0;
                }
                ResetTargets(trailingVehicle, ref instance2.m_vehicles.m_buffer[trailingVehicle], vehicleID, ref vehicleData, pushPathPos: false);
                trailingVehicle = instance2.m_vehicles.m_buffer[trailingVehicle].m_trailingVehicle;
                if (++num > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            vehicleData.m_pathPositionIndex = 0;
            ResetTargets(vehicleID, ref vehicleData, vehicleID, ref vehicleData, pushPathPos: false);
        }

        private static float CalculateMaxSpeed(float targetDistance, float targetSpeed, float maxBraking)
        {
            float num = 0.5f * maxBraking;
            float num2 = num + targetSpeed;
            return Mathf.Sqrt(Mathf.Max(0f, num2 * num2 + 2f * targetDistance * maxBraking)) - num;
        }

        protected override bool StartPathFind(ushort vehicleID, ref Vehicle vehicleData, Vector3 startPos, Vector3 endPos)
        {
            return StartPathFind(vehicleID, ref vehicleData, startPos, endPos, startBothWays: true, endBothWays: true);
        }

        protected bool StartPathFind(ushort vehicleID, ref Vehicle vehicleData, Vector3 startPos, Vector3 endPos, bool startBothWays, bool endBothWays)
        {
            VehicleInfo info = m_info;
            bool allowUnderground;
            bool allowUnderground2;
            if (info.m_vehicleType == VehicleInfo.VehicleType.Metro)
            {
                allowUnderground = true;
                allowUnderground2 = true;
            }
            else
            {
                allowUnderground = (vehicleData.m_flags & (Vehicle.Flags.Underground | Vehicle.Flags.Transition)) != 0;
                allowUnderground2 = false;
            }
            if (PathManager.FindPathPosition(startPos, ItemClass.Service.Road, NetInfo.LaneType.Vehicle, info.m_vehicleType, allowUnderground, requireConnect: false, 32f, out var pathPosA, out var pathPosB, out var distanceSqrA, out var distanceSqrB) && PathManager.FindPathPosition(endPos, ItemClass.Service.Road, NetInfo.LaneType.Vehicle, info.m_vehicleType, allowUnderground2, requireConnect: false, 32f, out var pathPosA2, out var pathPosB2, out var distanceSqrA2, out var distanceSqrB2))
            {
                if (!startBothWays || distanceSqrB > distanceSqrA * 1.2f)
                {
                    pathPosB = default(PathUnit.Position);
                }
                if (!endBothWays || distanceSqrB2 > distanceSqrA2 * 1.2f)
                {
                    pathPosB2 = default(PathUnit.Position);
                }
                if (Singleton<PathManager>.instance.CreatePath(out var unit, ref Singleton<SimulationManager>.instance.m_randomizer, Singleton<SimulationManager>.instance.m_currentBuildIndex, pathPosA, pathPosB, pathPosA2, pathPosB2, NetInfo.LaneType.Vehicle, info.m_vehicleType, 20000f, isHeavyVehicle: false, ignoreBlocked: false, stablePath: true, skipQueue: false))
                {
                    if (vehicleData.m_path != 0)
                    {
                        Singleton<PathManager>.instance.ReleasePath(vehicleData.m_path);
                    }
                    vehicleData.m_path = unit;
                    vehicleData.m_flags |= Vehicle.Flags.WaitingPath;
                    return true;
                }
            }
            return false;
        }

        public override bool TrySpawn(ushort vehicleID, ref Vehicle vehicleData)
        {
            if ((vehicleData.m_flags & Vehicle.Flags.Spawned) != 0)
            {
                return true;
            }
            if (vehicleData.m_path != 0)
            {
                PathManager instance = Singleton<PathManager>.instance;
                if (instance.m_pathUnits.m_buffer[vehicleData.m_path].GetPosition(0, out var position))
                {
                    uint laneID = PathManager.GetLaneID(position);
                    if (laneID != 0 && !Singleton<NetManager>.instance.m_lanes.m_buffer[laneID].CheckSpace(1000f, vehicleID))
                    {
                        vehicleData.m_flags |= Vehicle.Flags.WaitingSpace;
                        return false;
                    }
                }
            }
            vehicleData.Spawn(vehicleID);
            vehicleData.m_flags &= ~Vehicle.Flags.WaitingSpace;
            InitializePath(vehicleID, ref vehicleData);
            return true;
        }

        protected virtual bool PathFindReady(ushort vehicleID, ref Vehicle vehicleData)
        {
            PathManager instance = Singleton<PathManager>.instance;
            NetManager instance2 = Singleton<NetManager>.instance;
            float num = vehicleData.CalculateTotalLength(vehicleID);
            float distance = (num + m_info.m_generatedInfo.m_wheelBase - m_info.m_generatedInfo.m_size.z) * 0.5f;
            Vector3 vector = vehicleData.GetLastFramePosition();
            if ((vehicleData.m_flags & Vehicle.Flags.Spawned) == 0 && instance.m_pathUnits.m_buffer[vehicleData.m_path].GetPosition(0, out var position))
            {
                uint laneID = PathManager.GetLaneID(position);
                vector = instance2.m_lanes.m_buffer[laneID].CalculatePosition((float)(int)position.m_offset * 0.003921569f);
            }
            vehicleData.m_flags &= ~Vehicle.Flags.WaitingPath;
            instance.m_pathUnits.m_buffer[vehicleData.m_path].MoveLastPosition(vehicleData.m_path, distance);
            if ((vehicleData.m_flags & Vehicle.Flags.Spawned) != 0)
            {
                InitializePath(vehicleID, ref vehicleData);
            }
            else
            {
                int index = Mathf.Min(1, instance.m_pathUnits.m_buffer[vehicleData.m_path].m_positionCount - 1);
                if (instance.m_pathUnits.m_buffer[vehicleData.m_path].GetPosition(index, out var position2))
                {
                    uint laneID2 = PathManager.GetLaneID(position2);
                    Vector3 vector2 = instance2.m_lanes.m_buffer[laneID2].CalculatePosition((float)(int)position2.m_offset * 0.003921569f);
                    Vector3 forward = vector2 - vector;
                    vehicleData.m_frame0.m_position = vector;
                    if (forward.sqrMagnitude > 1f)
                    {
                        float length = instance2.m_lanes.m_buffer[laneID2].m_length;
                        vehicleData.m_frame0.m_position += forward.normalized * Mathf.Min(length * 0.5f, (num - m_info.m_generatedInfo.m_size.z) * 0.5f);
                        vehicleData.m_frame0.m_rotation = Quaternion.LookRotation(forward);
                    }
                    vehicleData.m_frame1 = vehicleData.m_frame0;
                    vehicleData.m_frame2 = vehicleData.m_frame0;
                    vehicleData.m_frame3 = vehicleData.m_frame0;
                    FrameDataUpdated(vehicleID, ref vehicleData, ref vehicleData.m_frame0);
                }
                TrySpawn(vehicleID, ref vehicleData);
            }
            return true;
        }

        public override int GetNoiseLevel()
        {
            return 5;
        }

    }


}
