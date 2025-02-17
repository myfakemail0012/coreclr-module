using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;

namespace AltV.Net.Elements.Pools
{
    public abstract class BaseObjectPool<TBaseObject> : IBaseObjectPool<TBaseObject> where TBaseObject : IBaseObject
    {
        public static void SetEntityNoLongerExists(TBaseObject entity)
        {
            if (!(entity is IInternalBaseObject internalEntity)) return;
            internalEntity.Exists = false;
            internalEntity.ClearData();
        }

        private readonly Dictionary<IntPtr, TBaseObject> entities = new Dictionary<IntPtr, TBaseObject>();

        private readonly IBaseObjectFactory<TBaseObject> entityFactory;

        protected BaseObjectPool(IBaseObjectFactory<TBaseObject> entityFactory)
        {
            this.entityFactory = entityFactory;
        }

        public void Create(IServer server, IntPtr entityPointer)
        {
            Add(entityFactory.Create(server, entityPointer));
        }

        public void Create(IServer server, IntPtr entityPointer, out TBaseObject entity)
        {
            entity = entityFactory.Create(server, entityPointer);
            Add(entity);
        }

        public void Add(TBaseObject entity)
        {
            entities[entity.NativePointer] = entity;
            OnAdd(entity);
        }

        public bool Remove(TBaseObject entity)
        {
            return Remove(entity.NativePointer);
        }

        public bool Remove(IntPtr entityPointer)
        {
            if (!entities.Remove(entityPointer, out var entity) || !entity.Exists) return false;
            entity.OnRemove();
            SetEntityNoLongerExists(entity);
            OnRemove(entity);
            return true;
        }

        public bool Get(IntPtr entityPointer, out TBaseObject entity)
        {
            return entities.TryGetValue(entityPointer, out entity) && entity.Exists;
        }

        public bool GetOrCreate(IServer server, IntPtr entityPointer, out TBaseObject entity)
        {
            if (entityPointer == IntPtr.Zero)
            {
                entity = default;
                return false;
            }

            if (entities.TryGetValue(entityPointer, out entity)) return entity.Exists;

            Create(server, entityPointer, out entity);

            return entity.Exists;
        }

        public ICollection<TBaseObject> GetAllObjects()
        {
            return entities.Values;
        }
        
        public KeyValuePair<IntPtr, TBaseObject>[] GetObjectsArray()
        {
            var arr = new KeyValuePair<IntPtr, TBaseObject>[entities.Count];
            var i = 0;
            foreach (var (ptr, entity) in entities)
            {
                arr[i++] = new KeyValuePair<IntPtr, TBaseObject>(ptr, entity);
            }

            return arr;
        }
        
        public void ForEach(IBaseObjectCallback<TBaseObject> baseObjectCallback)
        {
            foreach (var entity in entities.Values)
            {
                baseObjectCallback.OnBaseObject(entity);
            }
        }

        public async Task ForEach(IAsyncBaseObjectCallback<TBaseObject> asyncBaseObjectCallback)
        {
            foreach (var entity in entities.Values)
            {
                await asyncBaseObjectCallback.OnBaseObject(entity);
            }
        }

        public virtual void OnAdd(TBaseObject entity)
        {
        }

        public virtual void OnRemove(TBaseObject entity)
        {
        }

        public void Dispose()
        {
            foreach (var entity in entities.Values)
            {
                if (!(entity is IInternalBaseObject internalEntity)) continue;
                internalEntity.ClearData();
                entity.OnRemove();
                OnRemove(entity);
            }
            entities.Clear();
        }
    }
}