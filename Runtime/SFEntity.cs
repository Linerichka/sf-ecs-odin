﻿using System;
using System.Collections.Generic;
using Leopotam.EcsLite;
using SFramework.Core.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SFramework.ECS.Runtime
{
    [HideMonoScript]
    [DisallowMultipleComponent]
    public sealed class SFEntity : SFView, ISFEntity
    {
        public ref EcsPackedEntityWithWorld EcsPackedEntity => ref _ecsPackedEntity;

        [SFInject] private readonly ISFWorldsService _worldsService;

        [SFWorld] [SerializeField] private string _world;

        public bool removeEntityOnDisable;

        private EcsPackedEntityWithWorld _ecsPackedEntity;
        private bool _injected;
        private ISFEntitySetup[] _components;
        private EcsPool<GameObjectRef> _gameObjectRefPool;
        private EcsPool<TransformRef> _transformRefPool;
        private EcsPool<RootEntity> _rootEntityPool;
        private EcsWorld _ecsWorld;

        [SerializeField] [HideInInspector] private bool _isRootEntity;
        [SerializeField] [HideInInspector] private bool _checkedRootEntity;

        private IReadOnlyList<SFEntity> _childrenEntities;

        private bool _createdEntity;

        protected override void Init()
        {
            if (_injected) return;
            _components = GetComponents<ISFEntitySetup>();
            _injected = true;
            _ecsWorld = _worldsService.GetWorld(_world);
            _gameObjectRefPool = _ecsWorld.GetPool<GameObjectRef>();
            _transformRefPool = _ecsWorld.GetPool<TransformRef>();
            _rootEntityPool = _ecsWorld.GetPool<RootEntity>();

            if (_checkedRootEntity == false)
            {
                OnValidate();
            }
        }

        private void OnEnable()
        {
            CreateEntity();
        }

        private void OnDisable()
        {
            if (removeEntityOnDisable == false) return;
            DestroyEntity();
            _createdEntity = false;
        }

        private void OnDestroy()
        {
            if (_createdEntity == false) return;
            DestroyEntity();
        }

        private void OnValidate()
        {
            _isRootEntity = transform.parent == null || transform.parent.GetComponentInParent<SFEntity>(true) == null;
            _checkedRootEntity = true;
        }

        [Button("Recreate Entity")]
        public void RecreateEntity()
        {
            DestroyEntity();
            CreateEntity();
        }

        public void CreateEntity()
        {
            if (_createdEntity) return;
            var entity = _ecsWorld.NewEntity();
            _ecsPackedEntity = _ecsWorld.PackEntityWithWorld(entity);

            SFEntityMapping.AddMapping(gameObject, ref _ecsPackedEntity);

            _gameObjectRefPool.Add(entity) = new GameObjectRef
            {
                value = gameObject
            };

            if (_isRootEntity)
            {
                _rootEntityPool.Add(entity) = new RootEntity();
            }

            _transformRefPool.Add(entity) = new TransformRef
            {
                value = transform
            };

            foreach (var entitySetup in _components)
            {
                entitySetup.Setup(ref _ecsPackedEntity);
            }

            _createdEntity = true;
        }

        public void DestroyEntity()
        {
            SFEntityMapping.RemoveMapping(gameObject);

            if (_ecsPackedEntity.Unpack(out var world, out var entity))
            {
                world.DelEntity(entity);
            }
        }
    }
}