using System;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace ReversibleTramAI
{
    /// <summary>
    /// Taken from vanilla TramAI, unchanged
    /// </summary>
    public class RTramAI : RTramBaseAI
    {
        public EffectInfo m_arriveEffect;

        [CustomizableProperty("PassengerCapacity")]
        public int m_passengerCapacity = 20;

        public override void SimulationStep(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            if (vehicleData.m_leadingVehicle == 0 && (vehicleData.m_flags & Vehicle.Flags.Stopped) != 0)
            {
                vehicleData.m_waitCounter++;
                if (CanLeave(vehicleID, ref vehicleData))
                {
                    VehicleManager instance = Singleton<VehicleManager>.instance;
                    ushort trailingVehicle = vehicleData.m_trailingVehicle;
                    bool flag = true;
                    int num = 0;
                    while (trailingVehicle != 0)
                    {
                        VehicleInfo info = instance.m_vehicles.m_buffer[trailingVehicle].Info;
                        if (!info.m_vehicleAI.CanLeave(trailingVehicle, ref instance.m_vehicles.m_buffer[trailingVehicle]))
                        {
                            flag = false;
                            break;
                        }
                        trailingVehicle = instance.m_vehicles.m_buffer[trailingVehicle].m_trailingVehicle;
                        if (++num > 16384)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                    if (flag)
                    {
                        vehicleData.m_flags &= ~Vehicle.Flags.Stopped;
                        vehicleData.m_flags |= Vehicle.Flags.Leaving;
                        vehicleData.m_waitCounter = 0;
                        if (vehicleData.m_transportLine != 0)
                        {
                            Singleton<TransportManager>.instance.m_lines.m_buffer[vehicleData.m_transportLine].LeaveStop(vehicleData.m_targetBuilding);
                        }
                    }
                }
            }
            base.SimulationStep(vehicleID, ref vehicleData, ref frameData, leaderID, ref leaderData, lodPhysics);
            if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) == 0 && ShouldReturnToSource(vehicleID, ref vehicleData))
            {
                SetTransportLine(vehicleID, ref vehicleData, 0);
            }
        }

        public override void InitializeAI()
        {
            base.InitializeAI();
            if (m_arriveEffect != null)
            {
                m_arriveEffect.InitializeEffect();
            }
        }

        public override void ReleaseAI()
        {
            if (m_arriveEffect != null)
            {
                m_arriveEffect.ReleaseEffect();
            }
            base.ReleaseAI();
        }

        public override Color GetColor(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode)
        {
            if (data.m_leadingVehicle != 0)
            {
                VehicleManager instance = Singleton<VehicleManager>.instance;
                ushort firstVehicle = data.GetFirstVehicle(vehicleID);
                VehicleInfo info = instance.m_vehicles.m_buffer[firstVehicle].Info;
                return info.m_vehicleAI.GetColor(firstVehicle, ref instance.m_vehicles.m_buffer[firstVehicle], infoMode);
            }
            switch (infoMode)
            {
                case InfoManager.InfoMode.None:
                case InfoManager.InfoMode.Transport:
                    {
                        ushort transportLine = data.m_transportLine;
                        if (transportLine != 0)
                        {
                            return Singleton<TransportManager>.instance.m_lines.m_buffer[transportLine].GetColor();
                        }
                        return Singleton<TransportManager>.instance.m_properties.m_transportColors[(int)m_transportInfo.m_transportType];
                    }
                case InfoManager.InfoMode.Tourism:
                    if (Singleton<InfoManager>.instance.CurrentSubMode == InfoManager.SubInfoMode.Default)
                    {
                        int num = data.m_touristCount;
                        VehicleManager instance2 = Singleton<VehicleManager>.instance;
                        ushort trailingVehicle = data.m_trailingVehicle;
                        int num2 = 0;
                        while (trailingVehicle != 0)
                        {
                            num += instance2.m_vehicles.m_buffer[trailingVehicle].m_touristCount;
                            trailingVehicle = instance2.m_vehicles.m_buffer[trailingVehicle].m_trailingVehicle;
                            if (++num2 > 16384)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                break;
                            }
                        }
                        if (num != 0)
                        {
                            return CommonBuildingAI.GetTourismColor(num);
                        }
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                case InfoManager.InfoMode.TrafficRoutes:
                    if (Singleton<InfoManager>.instance.CurrentSubMode == InfoManager.SubInfoMode.Default)
                    {
                        InstanceID empty = InstanceID.Empty;
                        empty.Vehicle = vehicleID;
                        if (Singleton<NetManager>.instance.PathVisualizer.IsPathVisible(empty))
                        {
                            return Singleton<InfoManager>.instance.m_properties.m_routeColors[3];
                        }
                        return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                default:
                    return base.GetColor(vehicleID, ref data, infoMode);
            }
        }

        public override string GetLocalizedStatus(ushort vehicleID, ref Vehicle data, out InstanceID target)
        {
            if ((data.m_flags & Vehicle.Flags.Stopped) != 0)
            {
                target = InstanceID.Empty;
                return ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_TRAM_STOPPED");
            }
            if ((data.m_flags & Vehicle.Flags.GoingBack) != 0)
            {
                target = InstanceID.Empty;
                return ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_TRAM_RETURN");
            }
            if (data.m_transportLine != 0)
            {
                target = InstanceID.Empty;
                return ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_TRAM_ROUTE");
            }
            target = InstanceID.Empty;
            return ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_CONFUSED");
        }

        public override string GetLocalizedStatus(ushort parkedVehicleID, ref VehicleParked data, out InstanceID target)
        {
            target = InstanceID.Empty;
            return ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_TRAM_STOPPED");
        }

        public override void GetBufferStatus(ushort vehicleID, ref Vehicle data, out string localeKey, out int current, out int max)
        {
            localeKey = "Default";
            current = data.m_transferSize;
            max = m_passengerCapacity;
            if (data.m_leadingVehicle != 0)
            {
                return;
            }
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort trailingVehicle = data.m_trailingVehicle;
            int num = 0;
            while (trailingVehicle != 0)
            {
                VehicleInfo info = instance.m_vehicles.m_buffer[trailingVehicle].Info;
                if (instance.m_vehicles.m_buffer[trailingVehicle].m_leadingVehicle != 0)
                {
                    info.m_vehicleAI.GetBufferStatus(trailingVehicle, ref instance.m_vehicles.m_buffer[trailingVehicle], out localeKey, out var current2, out var max2);
                    current += current2;
                    max += max2;
                }
                trailingVehicle = instance.m_vehicles.m_buffer[trailingVehicle].m_trailingVehicle;
                if (++num > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }
        public override bool GetProgressStatus(ushort vehicleID, ref Vehicle data, out float current, out float max)
        {
            ushort transportLine = data.m_transportLine;
            ushort targetBuilding = data.m_targetBuilding;
            if (transportLine != 0 && targetBuilding != 0)
            {
                Singleton<TransportManager>.instance.m_lines.m_buffer[transportLine].GetStopProgress(targetBuilding, out var min, out var max2, out var total);
                uint path = data.m_path;
                bool valid;
                if (path == 0 || (data.m_flags & Vehicle.Flags.WaitingPath) != 0)
                {
                    current = min;
                    valid = false;
                }
                else
                {
                    current = BusAI.GetPathProgress(path, data.m_pathPositionIndex, min, max2, out valid);
                }
                max = total;
                return valid;
            }
            current = 0f;
            max = 0f;
            return true;
        }

        public override void CreateVehicle(ushort vehicleID, ref Vehicle data)
        {
            base.CreateVehicle(vehicleID, ref data);
            Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, 0, vehicleID, 0, 0, 0, m_passengerCapacity, 0);
        }

        public override void ReleaseVehicle(ushort vehicleID, ref Vehicle data)
        {
            RemoveSource(vehicleID, ref data);
            RemoveLine(vehicleID, ref data);
            base.ReleaseVehicle(vehicleID, ref data);
        }

        public override void LoadVehicle(ushort vehicleID, ref Vehicle data)
        {
            base.LoadVehicle(vehicleID, ref data);
            if (data.m_sourceBuilding != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].AddOwnVehicle(vehicleID, ref data);
            }
            if (data.m_transportLine != 0)
            {
                Singleton<TransportManager>.instance.m_lines.m_buffer[data.m_transportLine].AddVehicle(vehicleID, ref data, findTargetStop: false);
            }
        }

        protected override void PathfindSuccess(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_sourceBuilding != 0)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                BuildingAI.PathFindType pathFindType = (((data.m_flags & Vehicle.Flags.GoingBack) == 0) ? BuildingAI.PathFindType.LeavingTransport : BuildingAI.PathFindType.EnteringTransport);
                if (pathFindType == BuildingAI.PathFindType.EnteringTransport || (data.m_flags & Vehicle.Flags.Spawned) == 0)
                {
                    BuildingInfo info = instance.m_buildings.m_buffer[data.m_sourceBuilding].Info;
                    info.m_buildingAI.PathfindSuccess(data.m_sourceBuilding, ref instance.m_buildings.m_buffer[data.m_sourceBuilding], pathFindType);
                }
            }
            base.PathfindSuccess(vehicleID, ref data);
        }

        protected override void PathfindFailure(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_sourceBuilding != 0)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                BuildingAI.PathFindType pathFindType = (((data.m_flags & Vehicle.Flags.GoingBack) == 0) ? BuildingAI.PathFindType.LeavingTransport : BuildingAI.PathFindType.EnteringTransport);
                if (pathFindType == BuildingAI.PathFindType.EnteringTransport || (data.m_flags & Vehicle.Flags.Spawned) == 0)
                {
                    BuildingInfo info = instance.m_buildings.m_buffer[data.m_sourceBuilding].Info;
                    info.m_buildingAI.PathfindFailure(data.m_sourceBuilding, ref instance.m_buildings.m_buffer[data.m_sourceBuilding], pathFindType);
                }
            }
            base.PathfindFailure(vehicleID, ref data);
        }

        public override void SetSource(ushort vehicleID, ref Vehicle data, ushort sourceBuilding)
        {
            bool flag = data.m_sourceBuilding != 0;
            if (flag)
            {
                RemoveSource(vehicleID, ref data);
            }
            data.m_sourceBuilding = sourceBuilding;
            if (sourceBuilding != 0)
            {
                if (!flag)
                {
                    data.Unspawn(vehicleID);
                    data.m_targetPos0 = data.GetLastFramePosition();
                    data.m_targetPos0.w = 2f;
                    data.m_targetPos1 = data.m_targetPos0;
                    data.m_targetPos2 = data.m_targetPos0;
                    data.m_targetPos3 = data.m_targetPos0;
                }
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[sourceBuilding].AddOwnVehicle(vehicleID, ref data);
            }
        }

        public override void SetTarget(ushort vehicleID, ref Vehicle data, ushort targetBuilding)
        {
            data.m_targetBuilding = targetBuilding;
            if (!StartPathFind(vehicleID, ref data))
            {
                data.Unspawn(vehicleID);
            }
        }

        public override void BuildingRelocated(ushort vehicleID, ref Vehicle data, ushort building)
        {
            base.BuildingRelocated(vehicleID, ref data, building);
            if ((data.m_flags & Vehicle.Flags.GoingBack) != 0 && building == data.m_sourceBuilding)
            {
                InvalidPath(vehicleID, ref data, vehicleID, ref data);
            }
        }

        public override void SetTransportLine(ushort vehicleID, ref Vehicle data, ushort transportLine)
        {
            RemoveLine(vehicleID, ref data);
            data.m_transportLine = transportLine;
            if (transportLine != 0)
            {
                Singleton<TransportManager>.instance.m_lines.m_buffer[transportLine].AddVehicle(vehicleID, ref data, findTargetStop: true);
            }
            else
            {
                data.m_flags |= Vehicle.Flags.GoingBack;
            }
            if (!StartPathFind(vehicleID, ref data))
            {
                data.Unspawn(vehicleID);
            }
        }

        public override void StartTransfer(ushort vehicleID, ref Vehicle data, TransferManager.TransferReason reason, TransferManager.TransferOffer offer)
        {
            if (reason == (TransferManager.TransferReason)data.m_transferType)
            {
                SetTransportLine(vehicleID, ref data, offer.TransportLine);
            }
            else
            {
                base.StartTransfer(vehicleID, ref data, reason, offer);
            }
        }

        public override void GetSize(ushort vehicleID, ref Vehicle data, out int size, out int max)
        {
            size = 0;
            max = m_passengerCapacity;
        }

        private bool ShouldReturnToSource(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_sourceBuilding != 0)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                if ((instance.m_buildings.m_buffer[data.m_sourceBuilding].m_flags & Building.Flags.Active) == 0 && instance.m_buildings.m_buffer[data.m_sourceBuilding].m_fireIntensity == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private void RemoveSource(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_sourceBuilding != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].RemoveOwnVehicle(vehicleID, ref data);
                data.m_sourceBuilding = 0;
            }
        }

        private void RemoveLine(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_transportLine != 0)
            {
                Singleton<TransportManager>.instance.m_lines.m_buffer[data.m_transportLine].RemoveVehicle(vehicleID, ref data);
                data.m_transportLine = 0;
            }
        }

        private bool ArriveAtTarget(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_targetBuilding == 0)
            {
                Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleID);
                return true;
            }
            ushort targetBuilding = data.m_targetBuilding;
            ushort num = 0;
            if (data.m_transportLine != 0)
            {
                num = TransportLine.GetNextStop(data.m_targetBuilding);
            }
            else if ((data.m_flags & (Vehicle.Flags.Importing | Vehicle.Flags.Exporting)) != 0)
            {
                num = TransportLine.GetNextStop(data.m_targetBuilding);
                Vector3 lastFramePosition = data.GetLastFramePosition();
                if (Mathf.Max(Mathf.Abs(lastFramePosition.x), Mathf.Abs(lastFramePosition.z)) > 4800f && CheckPassengers(vehicleID, ref data, targetBuilding, num) == 0)
                {
                    num = 0;
                }
            }
            UnloadPassengers(vehicleID, ref data, targetBuilding, num);
            if (num == 0)
            {
                data.m_flags |= Vehicle.Flags.GoingBack;
                if (!StartPathFind(vehicleID, ref data))
                {
                    return true;
                }
                data.m_flags &= ~Vehicle.Flags.Arriving;
                data.m_flags |= Vehicle.Flags.Stopped;
                data.m_waitCounter = 0;
            }
            else
            {
                data.m_targetBuilding = num;
                if (!StartPathFind(vehicleID, ref data))
                {
                    return true;
                }
                LoadPassengers(vehicleID, ref data, targetBuilding, num);
                data.m_flags &= ~Vehicle.Flags.Arriving;
                data.m_flags |= Vehicle.Flags.Stopped;
                data.m_waitCounter = 0;
            }
            return false;
        }

        private void UnloadPassengers(ushort vehicleID, ref Vehicle data, ushort currentStop, ushort nextStop)
        {
            if (currentStop == 0)
            {
                return;
            }
            VehicleManager instance = Singleton<VehicleManager>.instance;
            NetManager instance2 = Singleton<NetManager>.instance;
            TransportManager instance3 = Singleton<TransportManager>.instance;
            Vector3 position = instance2.m_nodes.m_buffer[currentStop].m_position;
            Vector3 targetPos = Vector3.zero;
            if (nextStop != 0)
            {
                targetPos = instance2.m_nodes.m_buffer[nextStop].m_position;
            }
            int serviceCounter = 0;
            int num = 0;
            while (vehicleID != 0)
            {
                if (data.m_transportLine != 0)
                {
                    BusAI.TransportArriveAtTarget(vehicleID, ref instance.m_vehicles.m_buffer[vehicleID], position, targetPos, ref serviceCounter, ref instance3.m_lines.m_buffer[data.m_transportLine].m_passengers, nextStop == 0);
                }
                else
                {
                    BusAI.TransportArriveAtTarget(vehicleID, ref instance.m_vehicles.m_buffer[vehicleID], position, targetPos, ref serviceCounter, ref instance3.m_passengers[(int)m_transportInfo.m_transportType], nextStop == 0);
                }
                vehicleID = instance.m_vehicles.m_buffer[vehicleID].m_trailingVehicle;
                if (++num > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            StatisticBase statisticBase = Singleton<StatisticsManager>.instance.Acquire<StatisticArray>(StatisticType.PassengerCount);
            statisticBase.Acquire<StatisticInt32>((int)m_transportInfo.m_transportType, 17).Add(serviceCounter);
            serviceCounter += instance2.m_nodes.m_buffer[currentStop].m_tempCounter;
            instance2.m_nodes.m_buffer[currentStop].m_tempCounter = (ushort)Mathf.Min(serviceCounter, 65535);
        }

        private int CheckPassengers(ushort vehicleID, ref Vehicle data, ushort currentStop, ushort nextStop)
        {
            if (currentStop == 0 || nextStop == 0)
            {
                return 0;
            }
            CitizenManager instance = Singleton<CitizenManager>.instance;
            NetManager instance2 = Singleton<NetManager>.instance;
            if (instance2.m_nodes.m_buffer[currentStop].m_maxWaitTime != byte.MaxValue)
            {
                return 0;
            }
            Vector3 position = instance2.m_nodes.m_buffer[currentStop].m_position;
            Vector3 position2 = instance2.m_nodes.m_buffer[nextStop].m_position;
            int num = Mathf.Max((int)((position.x - 64f) / 8f + 1080f), 0);
            int num2 = Mathf.Max((int)((position.z - 64f) / 8f + 1080f), 0);
            int num3 = Mathf.Min((int)((position.x + 64f) / 8f + 1080f), 2159);
            int num4 = Mathf.Min((int)((position.z + 64f) / 8f + 1080f), 2159);
            int num5 = 0;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort num6 = instance.m_citizenGrid[i * 2160 + j];
                    int num7 = 0;
                    while (num6 != 0)
                    {
                        ushort nextGridInstance = instance.m_instances.m_buffer[num6].m_nextGridInstance;
                        if ((instance.m_instances.m_buffer[num6].m_flags & CitizenInstance.Flags.WaitingTransport) != 0)
                        {
                            Vector3 vector = instance.m_instances.m_buffer[num6].m_targetPos;
                            float num8 = Vector3.SqrMagnitude(vector - position);
                            if (num8 < 4096f)
                            {
                                CitizenInfo info = instance.m_instances.m_buffer[num6].Info;
                                if (info.m_citizenAI.TransportArriveAtSource(num6, ref instance.m_instances.m_buffer[num6], position, position2))
                                {
                                    num5++;
                                }
                            }
                        }
                        num6 = nextGridInstance;
                        if (++num7 > 65536)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return num5;
        }

        private void LoadPassengers(ushort vehicleID, ref Vehicle data, ushort currentStop, ushort nextStop)
        {
            if (currentStop == 0 || nextStop == 0)
            {
                return;
            }
            CitizenManager instance = Singleton<CitizenManager>.instance;
            VehicleManager instance2 = Singleton<VehicleManager>.instance;
            NetManager instance3 = Singleton<NetManager>.instance;
            Vector3 position = instance3.m_nodes.m_buffer[currentStop].m_position;
            Vector3 position2 = instance3.m_nodes.m_buffer[nextStop].m_position;
            instance3.m_nodes.m_buffer[currentStop].m_maxWaitTime = 0;
            int num = instance3.m_nodes.m_buffer[currentStop].m_tempCounter;
            bool flag = false;
            int num2 = Mathf.Max((int)((position.x - 64f) / 8f + 1080f), 0);
            int num3 = Mathf.Max((int)((position.z - 64f) / 8f + 1080f), 0);
            int num4 = Mathf.Min((int)((position.x + 64f) / 8f + 1080f), 2159);
            int num5 = Mathf.Min((int)((position.z + 64f) / 8f + 1080f), 2159);
            for (int i = num3; i <= num5; i++)
            {
                if (flag)
                {
                    break;
                }
                for (int j = num2; j <= num4; j++)
                {
                    if (flag)
                    {
                        break;
                    }
                    ushort num6 = instance.m_citizenGrid[i * 2160 + j];
                    int num7 = 0;
                    while (num6 != 0 && !flag)
                    {
                        ushort nextGridInstance = instance.m_instances.m_buffer[num6].m_nextGridInstance;
                        if ((instance.m_instances.m_buffer[num6].m_flags & CitizenInstance.Flags.WaitingTransport) != 0)
                        {
                            Vector3 vector = instance.m_instances.m_buffer[num6].m_targetPos;
                            float num8 = Vector3.SqrMagnitude(vector - position);
                            if (num8 < 4096f)
                            {
                                CitizenInfo info = instance.m_instances.m_buffer[num6].Info;
                                if (info.m_citizenAI.TransportArriveAtSource(num6, ref instance.m_instances.m_buffer[num6], position, position2))
                                {
                                    if (Vehicle.GetClosestFreeTrailer(vehicleID, vector, out var trailerID, out var unitID))
                                    {
                                        if (info.m_citizenAI.SetCurrentVehicle(num6, ref instance.m_instances.m_buffer[num6], trailerID, unitID, position))
                                        {
                                            num++;
                                            instance2.m_vehicles.m_buffer[trailerID].m_transferSize++;
                                        }
                                        else
                                        {
                                            flag = true;
                                        }
                                    }
                                    else
                                    {
                                        flag = true;
                                    }
                                }
                            }
                        }
                        num6 = nextGridInstance;
                        if (++num7 > 65536)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            instance3.m_nodes.m_buffer[currentStop].m_tempCounter = (ushort)Mathf.Min(num, 65535);
        }

        private bool ArriveAtSource(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_sourceBuilding == 0)
            {
                Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleID);
                return true;
            }
            RemoveSource(vehicleID, ref data);
            Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleID);
            return true;
        }

        public override bool ArriveAtDestination(ushort vehicleID, ref Vehicle vehicleData)
        {
            if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) != 0)
            {
                return ArriveAtSource(vehicleID, ref vehicleData);
            }
            if ((vehicleData.m_flags & Vehicle.Flags.WaitingLoading) != 0)
            {
                vehicleData.m_waitCounter = (byte)Mathf.Min(vehicleData.m_waitCounter + 1, 255);
                if (vehicleData.m_waitCounter >= 16)
                {
                    Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleID);
                    return true;
                }
                return false;
            }
            return ArriveAtTarget(vehicleID, ref vehicleData);
        }

        protected override void ArrivingToDestination(ushort vehicleID, ref Vehicle vehicleData)
        {
            base.ArrivingToDestination(vehicleID, ref vehicleData);
            if ((object)m_arriveEffect == null)
            {
                return;
            }
            if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) != 0)
            {
                if (vehicleData.m_sourceBuilding != 0)
                {
                    Vector3 position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[vehicleData.m_sourceBuilding].m_position;
                    InstanceID instance = default(InstanceID);
                    instance.Building = vehicleData.m_sourceBuilding;
                    EffectInfo.SpawnArea spawnArea = new EffectInfo.SpawnArea(position, Vector3.up, 0f);
                    Singleton<EffectManager>.instance.DispatchEffect(m_arriveEffect, instance, spawnArea, Vector3.zero, 0f, 1f, Singleton<BuildingManager>.instance.m_audioGroup);
                }
            }
            else if (vehicleData.m_targetBuilding != 0)
            {
                Vector3 position2 = Singleton<NetManager>.instance.m_nodes.m_buffer[vehicleData.m_targetBuilding].m_position;
                InstanceID instance2 = default(InstanceID);
                instance2.NetNode = vehicleData.m_targetBuilding;
                EffectInfo.SpawnArea spawnArea2 = new EffectInfo.SpawnArea(position2, Vector3.up, 0f);
                Singleton<EffectManager>.instance.DispatchEffect(m_arriveEffect, instance2, spawnArea2, Vector3.zero, 0f, 1f, Singleton<BuildingManager>.instance.m_audioGroup);
            }
        }

        protected override bool StartPathFind(ushort vehicleID, ref Vehicle vehicleData)
        {
            if (vehicleData.m_leadingVehicle == 0)
            {
                Vector3 startPos;
                if ((vehicleData.m_flags & Vehicle.Flags.Reversed) != 0)
                {
                    ushort lastVehicle = vehicleData.GetLastVehicle(vehicleID);
                    startPos = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[lastVehicle].m_targetPos0;
                }
                else
                {
                    startPos = vehicleData.m_targetPos0;
                }
                if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) != 0)
                {
                    if (vehicleData.m_sourceBuilding != 0)
                    {
                        BuildingManager instance = Singleton<BuildingManager>.instance;
                        BuildingInfo info = instance.m_buildings.m_buffer[vehicleData.m_sourceBuilding].Info;
                        Randomizer randomizer = new Randomizer(vehicleID);
                        info.m_buildingAI.CalculateUnspawnPosition(vehicleData.m_sourceBuilding, ref instance.m_buildings.m_buffer[vehicleData.m_sourceBuilding], ref randomizer, m_info, out var position, out var _);
                        return StartPathFind(vehicleID, ref vehicleData, startPos, position);
                    }
                }
                else if (vehicleData.m_targetBuilding != 0)
                {
                    Vector3 position2 = Singleton<NetManager>.instance.m_nodes.m_buffer[vehicleData.m_targetBuilding].m_position;
                    return StartPathFind(vehicleID, ref vehicleData, startPos, position2);
                }
            }
            return false;
        }

        public override bool CanLeave(ushort vehicleID, ref Vehicle vehicleData)
        {
            if (vehicleData.m_leadingVehicle == 0 && vehicleData.m_waitCounter < 12)
            {
                return false;
            }
            if (!base.CanLeave(vehicleID, ref vehicleData))
            {
                return false;
            }
            if (vehicleData.m_leadingVehicle == 0 && vehicleData.m_transportLine != 0)
            {
                return Singleton<TransportManager>.instance.m_lines.m_buffer[vehicleData.m_transportLine].CanLeaveStop(vehicleData.m_targetBuilding, vehicleData.m_waitCounter >> 4);
            }
            return true;
        }

        public override int GetTicketPrice(ushort vehicleID, ref Vehicle vehicleData)
        {
            if (vehicleData.m_transportLine == 0)
            {
                return m_transportInfo.m_ticketPrice;
            }
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(vehicleData.m_targetPos3);
            DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[district].m_servicePolicies;
            DistrictPolicies.Event @event = instance.m_districts.m_buffer[district].m_eventPolicies & Singleton<EventManager>.instance.GetEventPolicyMask();
            if ((servicePolicies & DistrictPolicies.Services.FreeTransport) != 0)
            {
                instance.m_districts.m_buffer[district].m_servicePoliciesEffect |= DistrictPolicies.Services.FreeTransport;
                return 0;
            }
            if ((@event & DistrictPolicies.Event.ComeOneComeAll) != 0)
            {
                instance.m_districts.m_buffer[district].m_eventPoliciesEffect |= DistrictPolicies.Event.ComeOneComeAll;
                return 0;
            }
            if ((servicePolicies & DistrictPolicies.Services.HighTicketPrices) != 0)
            {
                instance.m_districts.m_buffer[district].m_servicePoliciesEffect |= DistrictPolicies.Services.HighTicketPrices;
                return Singleton<TransportManager>.instance.m_lines.m_buffer[vehicleData.m_transportLine].m_ticketPrice * 5 / 4;
            }
            return Singleton<TransportManager>.instance.m_lines.m_buffer[vehicleData.m_transportLine].m_ticketPrice;
        }

        public override InstanceID GetOwnerID(ushort vehicleID, ref Vehicle vehicleData)
        {
            return default(InstanceID);
        }

        public override InstanceID GetTargetID(ushort vehicleID, ref Vehicle vehicleData)
        {
            InstanceID result = default(InstanceID);
            if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) != 0)
            {
                result.Building = vehicleData.m_sourceBuilding;
            }
            else
            {
                result.NetNode = vehicleData.m_targetBuilding;
            }
            return result;
        }
    }
}
