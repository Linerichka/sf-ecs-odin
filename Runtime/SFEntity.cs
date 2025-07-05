using Leopotam.EcsLite;
using SFramework.Core.Runtime;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SFramework.ECS.Runtime
{
    [HideMonoScript]
    [DisallowMultipleComponent]
    public sealed class SFEntity : SFView, ISFEntity
    {
        public ref EcsPackedEntityWithWorld EcsPackedEntity => ref _ecsPackedEntity;

        [SFInject]
        private readonly ISFWorldsService _worldsService;
        [SFWorld]
        private string _world;

        private EcsPackedEntityWithWorld _ecsPackedEntity;
        
        private ISFEntitySetup[] _components;
        private EcsPool<GameObjectRef> _gameObjectRefPool;
        private EcsPool<TransformRef> _transformRefPool;
        private EcsWorld _ecsWorld;

        protected override void Init()
        {
            _components = GetComponents<ISFEntitySetup>();
            
            _ecsWorld = _worldsService.GetWorld(_world);
            
            _gameObjectRefPool = _ecsWorld.GetPool<GameObjectRef>();
            _transformRefPool = _ecsWorld.GetPool<TransformRef>();
        }

        #if UNITY_EDITOR
        [Button(ButtonSizes.Medium), EnableIf("@UnityEngine.Application.isPlaying")]
        public void ReInit()
        {
            OnDisable();
            OnEnable();
        }
        #endif
        
        public void OnEnable()
        {
            var entity = _ecsWorld.NewEntity();
            _ecsPackedEntity = _ecsWorld.PackEntityWithWorld(entity);

            SFEntityMapping.AddMapping(gameObject, ref _ecsPackedEntity);

            _gameObjectRefPool.Add(entity) = new GameObjectRef
            {
                value = gameObject
            };

            _transformRefPool.Add(entity) = new TransformRef
            {
                value = transform
            };

            foreach (var entitySetup in _components)
            {
                entitySetup.Setup(ref _ecsPackedEntity);
            }
        }

        public void OnDisable()
        {
            SFEntityMapping.RemoveMapping(gameObject);

            if (_ecsPackedEntity.Unpack(out var world, out var entity))
            {
                world.DelEntity(entity);
            }
        }
    }
}