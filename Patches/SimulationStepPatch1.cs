using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System;
using ColossalFramework;


namespace ReversibleTramAI
{
    [HarmonyPatch]
    public static class SimulationStepPatch1
    {

        private delegate void TargetDelegate(ushort vehicleID, ref Vehicle data, Vector3 physicsLodRefPos);

        public static MethodBase TargetMethod() => Patcher.DeclaredMethod<TargetDelegate>(typeof(TramBaseAI), nameof(TramBaseAI.SimulationStep));

        public static bool Prefix(VehicleAI __instance, ref VehicleInfo ___m_info, ushort vehicleID, ref Vehicle data, Vector3 physicsLodRefPos)
        {
            if ((data.m_flags & Vehicle.Flags.WaitingPath) != 0)
            {
                byte pathFindFlags = Singleton<PathManager>.instance.m_pathUnits.m_buffer[data.m_path].m_pathFindFlags;
                if ((pathFindFlags & 4u) != 0)
                {
                    //PathfindSuccess(vehicleID, ref data);
                    PathfindSuccessReversePatch.PathfindSuccess(__instance, vehicleID, ref data);

                    //PathFindReady(vehicleID, ref data);
                    PathFindReadyReversePatch.PathFindReady(__instance, vehicleID, ref data);
                }
                else if ((pathFindFlags & 8u) != 0 || data.m_path == 0)
                {
                    data.m_flags &= ~Vehicle.Flags.WaitingPath;
                    Singleton<PathManager>.instance.ReleasePath(data.m_path);
                    data.m_path = 0u;
                    
                    //PathfindFailure(vehicleID, ref data);
                    PathfindFailureReversePatch.PathfindFailure(__instance, vehicleID, ref data);

                    return false;
                }
            }
            else if ((data.m_flags & Vehicle.Flags.WaitingSpace) != 0)
            {
                //TrySpawn(vehicleID, ref data);
                TrySpawnReversePatch.TrySpawn(__instance, vehicleID, ref data);
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
                return false;
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
                    return false;
                }
                flag2 = (data.m_flags & Vehicle.Flags.Reversed) != 0;
                if (flag2 != flag)
                {
                    Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleID);
                    return false;
                }
            }


            num = flag ? instance.m_vehicles.m_buffer[num].m_leadingVehicle : instance.m_vehicles.m_buffer[num].m_trailingVehicle;
            int num2 = 0;
            while (num != 0)
            {
                info = instance.m_vehicles.m_buffer[num].Info;
                info.m_vehicleAI.SimulationStep(num, ref instance.m_vehicles.m_buffer[num], vehicleID, ref data, 0);
                if ((data.m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Deleted)) != Vehicle.Flags.Created)
                {
                    return false;
                }
                num = flag ? instance.m_vehicles.m_buffer[num].m_leadingVehicle : instance.m_vehicles.m_buffer[num].m_trailingVehicle;
                if (++num2 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }

            // NON-STOCK CODE END

            if ((data.m_flags & (Vehicle.Flags.Spawned | Vehicle.Flags.WaitingPath | Vehicle.Flags.WaitingSpace | Vehicle.Flags.WaitingCargo)) == 0 || data.m_blockCounter == byte.MaxValue)
            {
                Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleID);
            }

            return false;
        }
    }
}
