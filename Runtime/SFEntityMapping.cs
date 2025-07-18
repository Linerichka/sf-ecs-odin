using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Leopotam.EcsLite;
using SFramework.Core.Runtime;
using UnityEngine;

namespace SFramework.ECS.Runtime
{
    public static class SFEntityMapping
    {
        private static readonly Dictionary<int, EcsPackedEntityWithWorld> _packedEntities;

        static SFEntityMapping()
        {
            _packedEntities = new Dictionary<int, EcsPackedEntityWithWorld>();
        }

        static internal void AddMapping(GameObject gameObject, ref EcsWorld world, ref int entity)
        {
            _packedEntities[gameObject.GetInstanceID()] = world.PackEntityWithWorld(entity);
        }

        static internal void AddMapping(GameObject gameObject, ref EcsPackedEntityWithWorld entity)
        {
            _packedEntities[gameObject.GetInstanceID()] = entity;
        }

        static internal void RemoveMapping(GameObject gameObject)
        {
            var instanceId = gameObject.GetInstanceID();
            
            if (_packedEntities.ContainsKey(instanceId))
            {
                _packedEntities.Remove(instanceId);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FastGetEntity(GameObject gameObject)
        {
            return _packedEntities[gameObject.GetInstanceID()].Id;
        } 
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FastGetEntityPacked(GameObject gameObject, out EcsPackedEntityWithWorld entity)
        {
            entity = _packedEntities[gameObject.GetInstanceID()];
        }

        public static bool GetEntity(GameObject gameObject, out EcsWorld world, out int entity)
        {
            if (gameObject == null)
            {
                SFDebug.Log(LogType.Warning, "GameObject is null");
                entity = default;
                world = default;
                return false;
            }
            
            var instanceId = gameObject.GetInstanceID();
            
            if (_packedEntities.TryGetValue(instanceId, out var packedEntityWithWorld))
            {
                return packedEntityWithWorld.Unpack(out world, out entity);
            }
            
            entity = default;
            world = default;
            return false;
        }
        
        public static bool GetEntity(GameObject gameObject, out int entity)
        {
            if (gameObject == null)
            {
                SFDebug.Log(LogType.Warning, "GameObject is null");
                entity = default;
                return false;
            }
            
            var instanceId = gameObject.GetInstanceID();
            
            if (_packedEntities.TryGetValue(instanceId, out var packedEntityWithWorld))
            {
                return packedEntityWithWorld.Unpack(out _, out entity);
            }
            
            entity = default;
            return false;
        }

        public static bool GetEntityPacked(GameObject gameObject, out EcsPackedEntityWithWorld packedEntity)
        {
            if (gameObject == null)
            {
                SFDebug.Log(LogType.Warning, "GameObject is null");
                packedEntity = default;
                return false;
            }
            
            var instanceId = gameObject.GetInstanceID();
            
            if (_packedEntities.TryGetValue(instanceId, out var packedEntityWithWorld))
            {
                packedEntity = packedEntityWithWorld;
                return true;
            }

            packedEntity = default;
            return false;
        }

        public static void Clear()
        {
            _packedEntities.Clear();
        }
    }
}