using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System;
using ColossalFramework;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ReversibleTramAI
{
    [HarmonyPatch]
    public static class SimulationStepPatch1
    {

        private delegate void TargetDelegate(ushort vehicleID, ref Vehicle data, Vector3 physicsLodRefPos);

        public static MethodBase TargetMethod() => Patcher.DeclaredMethod<TargetDelegate>(typeof(TramBaseAI), nameof(TramBaseAI.SimulationStep));

        /// <summary>
        /// 
        /// This prefix is also Transpiled by TMPE !!!
        /// 
        /// </summary>
        public static bool Prefix(VehicleAI __instance, ushort vehicleID, ref Vehicle data, Vector3 physicsLodRefPos, ref VehicleInfo ___m_info)
        {
            if ((data.m_flags & Vehicle.Flags.WaitingPath) != 0)
            {
                byte pathFindFlags = Singleton<PathManager>.instance.m_pathUnits.m_buffer[data.m_path].m_pathFindFlags;
                if ((pathFindFlags & 4u) != 0)
                {
                    //PathfindSuccess(vehicleID, ref data);
                    //PathfindSuccessReversePatch.PathfindSuccess(__instance, vehicleID, ref data);
                    PathfindSuccessStub(__instance, vehicleID, ref data);

                    //PathFindReady(vehicleID, ref data);
                    //PathFindReadyReversePatch.PathFindReady(__instance, vehicleID, ref data);
                    PathFindReadyStub(__instance, vehicleID, ref data);

                }
                else if ((pathFindFlags & 8u) != 0 || data.m_path == 0)
                {
                    data.m_flags &= ~Vehicle.Flags.WaitingPath;
                    Singleton<PathManager>.instance.ReleasePath(data.m_path);
                    data.m_path = 0u;

                    //PathfindFailure(vehicleID, ref data);
                    //PathfindFailureReversePatch.PathfindFailure(__instance, vehicleID, ref data);
                    PathfindFailureStub(__instance, vehicleID, ref data);

                    return false;
                }
            }
            else if ((data.m_flags & Vehicle.Flags.WaitingSpace) != 0)
            {
                //TrySpawn(vehicleID, ref data);
                //TrySpawnReversePatch.TrySpawn(__instance, vehicleID, ref data);
                TrySpawnStub(__instance, vehicleID, ref data);
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

            uint maxVehicleCount = Singleton<VehicleManager>.instance.m_vehicles.m_size;
            
            while (num != 0)
            {
                info = instance.m_vehicles.m_buffer[num].Info;
                info.m_vehicleAI.SimulationStep(num, ref instance.m_vehicles.m_buffer[num], vehicleID, ref data, 0);
                if ((data.m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Deleted)) != Vehicle.Flags.Created)
                {
                    return false;
                }
                num = flag ? instance.m_vehicles.m_buffer[num].m_leadingVehicle : instance.m_vehicles.m_buffer[num].m_trailingVehicle;
                if (++num2 > maxVehicleCount) //16384)
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

        public static void PathfindSuccessStub(VehicleAI __instance, ushort vehicleID, ref Vehicle data)
        {
            throw new NotImplementedException("SimulationStepPatch1.PathfindSuccessStub - Harmony transpiler not applied");
        }

        public static void PathfindFailureStub(VehicleAI __instance, ushort vehicleID, ref Vehicle data)
        {
            throw new NotImplementedException("SimulationStepPatch1.PathfindFaiureStub - Harmony transpiler not applied");
        }

        public static bool PathFindReadyStub(VehicleAI __instance, ushort vehicleID, ref Vehicle data)
        {
            throw new NotImplementedException("SimulationStepPatch1.PathFindReadyStub - Harmony transpiler not applied");
        }

        public static bool TrySpawnStub(VehicleAI __instance, ushort vehicleID, ref Vehicle data)
        {
            throw new NotImplementedException("SimulationStepPatch1.TrySpawnStub - Harmony transpiler not applied");
        }
    }


    [HarmonyPatch(typeof(SimulationStepPatch1), nameof(SimulationStepPatch1.Prefix))]
    public static class SimulationStepPatch1Transpiler
    {
        private delegate void PathfindSuccessStubDelegate(VehicleAI __instance, ushort vehicleID, ref Vehicle data);
        private static MethodBase PathfindSuccessStubMethod() => Patcher.DeclaredMethod<PathfindSuccessStubDelegate>(typeof(SimulationStepPatch1), "PathfindSuccessStub");
        
        private delegate void PathfindSuccessDelegate(ushort vehicleID, ref Vehicle data);
        private static MethodBase PathfindSuccessMethod() => Patcher.DeclaredMethod<PathfindSuccessDelegate>(typeof(TramBaseAI), "PathfindSuccess");


        private delegate void PathfindFailureStubDelegate(VehicleAI __instance, ushort vehicleID, ref Vehicle data);
        private static MethodBase PathfindFailureStubMethod() => Patcher.DeclaredMethod<PathfindFailureStubDelegate>(typeof(SimulationStepPatch1), "PathfindFailureStub");

        private delegate void PathfindFailureDelegate(ushort vehicleID, ref Vehicle data);
        private static MethodBase PathfindFailureMethod() => Patcher.DeclaredMethod<PathfindFailureDelegate>(typeof(TramBaseAI), "PathfindFailure");


        private delegate void PathFindReadyStubDelegate(VehicleAI __instance, ushort vehicleID, ref Vehicle data);
        private static MethodBase PathFindReadyStubMethod() => Patcher.DeclaredMethod<PathFindReadyStubDelegate>(typeof(SimulationStepPatch1), "PathFindReadyStub");

        private delegate void PathFindReadyDelegate(ushort vehicleID, ref Vehicle data);
        private static MethodBase PathFindReadyMethod() => Patcher.DeclaredMethod<PathFindReadyDelegate>(typeof(TramBaseAI), "PathFindReady");


        private delegate void TrySpawnStubDelegate(VehicleAI __instance, ushort vehicleID, ref Vehicle data);
        private static MethodBase TrySpawnStubMethod() => Patcher.DeclaredMethod<TrySpawnStubDelegate>(typeof(SimulationStepPatch1), "TrySpawnStub");

        private delegate void TrySpawnDelegate(ushort vehicleID, ref Vehicle data);
        private static MethodBase TrySpawnMethod() => Patcher.DeclaredMethod<TrySpawnDelegate>(typeof(TramBaseAI), "TrySpawn");

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator il, IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = Patcher.ToCodeList(instructions);

            CodeInstruction pathfindSuccessStubInstruction = new CodeInstruction(OpCodes.Call, PathfindSuccessStubMethod());
            CodeInstruction pathfindSuccessInstruction = new CodeInstruction(OpCodes.Callvirt, PathfindSuccessMethod());

            CodeInstruction pathfindFailureStubInstruction = new CodeInstruction(OpCodes.Call, PathfindFailureStubMethod());
            CodeInstruction pathfindFailureInstruction = new CodeInstruction(OpCodes.Callvirt, PathfindFailureMethod());

            CodeInstruction PathFindReadyStubInstruction = new CodeInstruction(OpCodes.Call, PathFindReadyStubMethod());
            CodeInstruction PathFindReadyInstruction = new CodeInstruction(OpCodes.Callvirt, PathFindReadyMethod());

            CodeInstruction TrySpawnStubInstruction = new CodeInstruction(OpCodes.Call, TrySpawnStubMethod());
            CodeInstruction TryspawnInstruction = new CodeInstruction(OpCodes.Callvirt, TrySpawnMethod());

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Call) continue;

                if (codes[i].opcode == pathfindSuccessStubInstruction.opcode && codes[i].operand == pathfindSuccessStubInstruction.operand)
                {
                    codes[i].opcode = pathfindSuccessInstruction.opcode;
                    codes[i].operand = pathfindSuccessInstruction.operand;
                }
                else if (codes[i].opcode == pathfindFailureStubInstruction.opcode && codes[i].operand == pathfindFailureStubInstruction.operand)
                {
                    codes[i].opcode = pathfindFailureInstruction.opcode;
                    codes[i].operand = pathfindFailureInstruction.operand;
                }
                else if (codes[i].opcode == PathFindReadyStubInstruction.opcode && codes[i].operand == PathFindReadyStubInstruction.operand)
                {
                    codes[i].opcode = PathFindReadyInstruction.opcode;
                    codes[i].operand = PathFindReadyInstruction.operand;
                }
                else if (codes[i].opcode == TrySpawnStubInstruction.opcode && codes[i].operand == TrySpawnStubInstruction.operand)
                {
                    codes[i].opcode = TryspawnInstruction.opcode;
                    codes[i].operand = TryspawnInstruction.operand;
                }
            }

            return codes;
        }

    }
}
