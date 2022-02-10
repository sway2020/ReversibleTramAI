/*
 * Obsolete. Not used anymore
 */

using System;
using HarmonyLib;
using ColossalFramework;
using UnityEngine;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ReversibleTramAI
{
    //[HarmonyPatch]
    //internal static class InvalidPathReversePatch
    //{
    //    [HarmonyReversePatch]
    //    [HarmonyPatch((typeof(VehicleAI)), "InvalidPath")]
    //    [MethodImpl(MethodImplOptions.NoInlining)]
    //    internal static void InvalidPath(object instance, ushort vehicleID, ref Vehicle vehicleData, ushort leaderID, ref Vehicle leaderData)
    //    {
    //        throw new NotImplementedException("VehicleAI.InvalidPath - Harmony reverse patch not applied");
    //    }
    //}

    //[HarmonyPatch]
    //internal static class CalculateTargetSpeedReversePatch
    //{
    //    [HarmonyReversePatch]
    //    [HarmonyPatch((typeof(VehicleAI)), "CalculateTargetSpeed")]
    //    [MethodImpl(MethodImplOptions.NoInlining)]
    //    internal static float CalculateTargetSpeed(object instance, ushort vehicleID, ref Vehicle data, float speedLimit, float curve)
    //    {
    //        throw new NotImplementedException("VehicleAI.CalculateTargetSpeed - Harmony reverse patch not applied");
    //    }
    //}

    //[HarmonyPatch]
    //internal static class UpdateNodeTargetPosReversePatch
    //{
    //    [HarmonyReversePatch]
    //    [HarmonyPatch((typeof(VehicleAI)), "UpdateNodeTargetPos")]
    //    [MethodImpl(MethodImplOptions.NoInlining)]
    //    internal static void UpdateNodeTargetPos(object instance, ushort vehicleID, ref Vehicle vehicleData, ushort nodeID, ref NetNode nodeData, ref Vector4 targetPos, int index)
    //    {
    //        throw new NotImplementedException("VehicleAI.UpdateNodeTargetPos - Harmony reverse patch not applied");
    //    }
    //}

    //[HarmonyPatch]
    //internal static class ArrivingToDestinationReversePatch
    //{
    //    [HarmonyReversePatch]
    //    [HarmonyPatch((typeof(VehicleAI)), "ArrivingToDestination")]
    //    [MethodImpl(MethodImplOptions.NoInlining)]
    //    internal static void ArrivingToDestination(object instance, ushort vehicleID, ref Vehicle vehicleData)
    //    {
    //        throw new NotImplementedException("VehicleAI.ArrivingToDestination - Harmony reverse patch not applied");
    //    }
    //}

    //[HarmonyPatch]
    //internal static class CalculateSegmentPositionReversePatch1
    //{
    //    private delegate void TargetDelegate(ushort vehicleID, ref Vehicle vehicleData, PathUnit.Position position, uint laneID, byte offset, out Vector3 pos, out Vector3 dir, out float maxSpeed);

    //    public static MethodBase TargetMethod() => Patcher.DeclaredMethod<TargetDelegate>(typeof(TramBaseAI), "CalculateSegmentPosition");

    //    [HarmonyReversePatch]
    //    [MethodImpl(MethodImplOptions.NoInlining)]
    //    internal static void CalculateSegmentPosition(object instance, ushort vehicleID, ref Vehicle vehicleData, PathUnit.Position position, uint laneID, byte offset, out Vector3 pos, out Vector3 dir, out float maxSpeed)
    //    {
    //        throw new NotImplementedException("TramBaseAI.CalculateSegmentPosition1 - Harmony reverse patch not applied");
    //    }
    //}

    //[HarmonyPatch]
    //internal static class CalculateSegmentPositionReversePatch2
    //{
    //    private delegate void TargetDelegate(ushort vehicleID, ref Vehicle vehicleData, PathUnit.Position nextPosition, PathUnit.Position position, uint laneID, byte offset, PathUnit.Position prevPos, uint prevLaneID, byte prevOffset, int index, out Vector3 pos, out Vector3 dir, out float maxSpeed);

    //    public static MethodBase TargetMethod() => Patcher.DeclaredMethod<TargetDelegate>(typeof(TramBaseAI), "CalculateSegmentPosition");

    //    [HarmonyReversePatch]
    //    [MethodImpl(MethodImplOptions.NoInlining)]
    //    internal static void CalculateSegmentPosition(object instance, ushort vehicleID, ref Vehicle vehicleData, PathUnit.Position nextPosition, PathUnit.Position position, uint laneID, byte offset, PathUnit.Position prevPos, uint prevLaneID, byte prevOffset, int index, out Vector3 pos, out Vector3 dir, out float maxSpeed)
    //    {
    //        throw new NotImplementedException("TramBaseAI.CalculateSegmentPosition2 - Harmony reverse patch not applied");
    //    }
    //}

    //[HarmonyPatch]
    //internal static class PathfindSuccessReversePatch
    //{
    //    [HarmonyReversePatch]
    //    [HarmonyPatch((typeof(TramBaseAI)), "PathfindSuccess")]
    //    [MethodImpl(MethodImplOptions.NoInlining)]
    //    internal static void PathfindSuccess(object instance, ushort vehicleID, ref Vehicle data)
    //    {
    //        throw new NotImplementedException("TramBaseAI.PathfindSuccess - Harmony reverse patch not applied");
    //    }
    //}

    //[HarmonyPatch]
    //internal static class PathFindReadyReversePatch
    //{
    //    [HarmonyReversePatch]
    //    [HarmonyPatch((typeof(TramBaseAI)), "PathFindReady")]
    //    [MethodImpl(MethodImplOptions.NoInlining)]
    //    internal static bool PathFindReady(object instance, ushort vehicleID, ref Vehicle data)
    //    {
    //        throw new NotImplementedException("TramBaseAI.PathFindReady - Harmony reverse patch not applied");
    //    }
    //}


    //[HarmonyPatch]
    //internal static class PathfindFailureReversePatch
    //{
    //    [HarmonyReversePatch]
    //    [HarmonyPatch((typeof(TramBaseAI)), "PathfindFailure")]
    //    [MethodImpl(MethodImplOptions.NoInlining)]
    //    internal static void PathfindFailure(object instance, ushort vehicleID, ref Vehicle data)
    //    {
    //        throw new NotImplementedException("TramBaseAI.PathfindFailure - Harmony reverse patch not applied");
    //    }
    //}

    //[HarmonyPatch]
    //internal static class TrySpawnReversePatch
    //{
    //    [HarmonyReversePatch]
    //    [HarmonyPatch((typeof(TramBaseAI)), "TrySpawn")]
    //    [MethodImpl(MethodImplOptions.NoInlining)]
    //    internal static bool TrySpawn(object instance, ushort vehicleID, ref Vehicle data)
    //    {
    //        throw new NotImplementedException("TramBaseAI.TrySpawn - Harmony reverse patch not applied");
    //    }
    //}

    //[HarmonyPatch]
    //internal static class UpdatePathTargetPositionsReversePatch
    //{
    //    [HarmonyReversePatch]
    //    [HarmonyPatch((typeof(TramBaseAI)), "UpdatePathTargetPositions")]
    //    [MethodImpl(MethodImplOptions.NoInlining)]
    //    internal static void UpdatePathTargetPositions(object instance, ushort vehicleID, ref Vehicle vehicleData, Vector3 refPos1, Vector3 refPos2, ushort leaderID, ref Vehicle leaderData, ref int index, int max1, int max2, float minSqrDistanceA, float minSqrDistanceB)
    //    {
    //        throw new NotImplementedException("TramBaseAI.UpdatePathTargetPositions - Harmony reverse patch not applied");
    //    }
    //}

    //[HarmonyPatch]
    //internal static class GetMaxSpeedReversePatch
    //{
    //    [HarmonyReversePatch]
    //    [HarmonyPatch((typeof(TramBaseAI)), "GetMaxSpeed")]
    //    [MethodImpl(MethodImplOptions.NoInlining)]
    //    internal static float GetMaxSpeed(object instance, ushort leaderID, ref Vehicle leaderData)
    //    {
    //        throw new NotImplementedException("TramBaseAI.GetMaxSpeed - Harmony reverse patch not applied");
    //    }
    //}

    //[HarmonyPatch]
    //internal static class CalculateMaxSpeedReversePatch
    //{
    //    [HarmonyReversePatch]
    //    [HarmonyPatch((typeof(TramBaseAI)), "CalculateMaxSpeed")]
    //    [MethodImpl(MethodImplOptions.NoInlining)]
    //    internal static float CalculateMaxSpeed(object instance, float targetDistance, float targetSpeed, float maxBraking)
    //    {
    //        throw new NotImplementedException("TramBaseAI.CalculateMaxSpeed - Harmony reverse patch not applied");
    //    }
    //}


    //[HarmonyPatch]
    //internal static class ResetTargetsReversePatch
    //{
    //    [HarmonyReversePatch]
    //    [HarmonyPatch((typeof(TramBaseAI)), "ResetTargets")]
    //    //[MethodImpl(MethodImplOptions.NoInlining)]
    //    internal static void ResetTargets(object instance, ushort vehicleID, ref Vehicle vehicleData, ushort leaderID, ref Vehicle leaderData, bool pushPathPos)
    //    {
    //        throw new NotImplementedException("TramBaseAI.ResetTargets - Harmony reverse patch not applied");
    //    }
    //}

}