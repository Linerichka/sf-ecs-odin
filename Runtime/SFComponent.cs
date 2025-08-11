using System;
using Leopotam.EcsLite;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.PlayerLoop;
#endif

namespace SFramework.ECS.Runtime
{
    [Serializable]
    public abstract class SFComponent<T> : MonoBehaviour, ISFComponentInspector where T : struct
    {
        [HideLabel]
        [InlineProperty]
        [SerializeField]
        protected T _value;

        protected EcsPackedEntityWithWorld _packedEntityWithWorld;
        protected EcsPool<T> _pool;

        public void Setup(ref EcsPackedEntityWithWorld packedEntity)
        {
            if (!packedEntity.Unpack(out var world, out var entity)) return;

            _pool = world.GetPool<T>();
            _packedEntityWithWorld = packedEntity;

            if (_value is IEcsAutoInit<T> autoInit)
            {
                autoInit.AutoInit(ref _value);
            }

            _pool.Add(entity) = _value;
        }


        protected virtual void OnDrawGizmos()
        {
            if (!_packedEntityWithWorld.Unpack(out _, out var entity) || !_pool.Has(entity)) return;
            ref var value = ref _pool.Get(entity);

            if (value is ISFDrawGizmos<T> drawGizmos)
            {
                drawGizmos.DrawGizmos(transform);
            }
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (!_packedEntityWithWorld.Unpack(out _, out var entity) || !_pool.Has(entity)) return;
            ref var value = ref _pool.Get(entity);

            if (value is ISFDrawGizmosSelected<T> drawGizmosSelected)
            {
                drawGizmosSelected.DrawGizmosSelected(transform);
            }
        }
        
        #if UNITY_EDITOR
        private bool _isUpdateSubscribed;
        private double _nextUpdateTime;
        private double _delay = 0.25; 
        private bool _isSubscribed;
        private bool _isCalledAfterSubscribed;
        protected void OnValidate()
        {
            if (_value is IOnValidate onValidate)
            {
                onValidate.OnValidate();
            }

            if (!_isUpdateSubscribed)
            {
                _nextUpdateTime = EditorApplication.timeSinceStartup + _delay;
                EditorApplication.update += UpdateComponent;
                _isUpdateSubscribed = true;
                _isCalledAfterSubscribed = false;
            }
            else
            {
                _isCalledAfterSubscribed = true;
            }
        }

        private void UpdateComponent()
        {
            if (_nextUpdateTime > EditorApplication.timeSinceStartup) return;
            
            if (_packedEntityWithWorld.Unpack(out _, out var entity))
            {
                if (_pool.Has(entity))
                {
                    _pool.Get(entity) = _value;
                }
            }

            if (!_isCalledAfterSubscribed)
            {
                EditorApplication.update -= UpdateComponent;
                _isUpdateSubscribed = false;
            }
            else
            {
                _nextUpdateTime = EditorApplication.timeSinceStartup + _delay;
            }
        }
        #endif
    }
}