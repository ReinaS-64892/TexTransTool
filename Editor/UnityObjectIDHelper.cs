using System;
using UnityEditor;
using UnityEngine;
namespace net.rs64.TexTransTool
{

    public static class UnityObjectIDHelper
    {

#if !UNITY_6000_2_OR_NEWER
        public static EntityId GetEntityId(this UnityEngine.Object unityObject)
        {
            return new(unityObject.GetInstanceID());
        }
#endif

        public static EntityId GetEntityId(this ChangeAssetObjectPropertiesEventArgs data)
        {
#if UNITY_6000_5_OR_NEWER
            return data.entityId;
#elif UNITY_6000_2_OR_NEWER
            return data.instanceId;
#else
            return new(data.instanceId);
#endif
        }
        public static EntityId GetEntityId(this ChangeGameObjectOrComponentPropertiesEventArgs data)
        {
#if UNITY_6000_5_OR_NEWER
            return data.entityId;
#elif UNITY_6000_2_OR_NEWER
            return data.instanceId;
#else
            return new(data.instanceId);
#endif
        }


    }
#if !UNITY_6000_2_OR_NEWER
    public struct EntityId : IEquatable<EntityId>
    {
        public int InstanceID;

        public EntityId(int id)
        {
            InstanceID = id;
        }

        public bool Equals(EntityId other)
        {
            return InstanceID == other.InstanceID;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityId eid && Equals(eid);
        }
        public override int GetHashCode() => InstanceID;
        public static EntityId None => default(EntityId);


        public static bool operator ==(EntityId left, EntityId right) => left.Equals(right);
        public static bool operator !=(EntityId left, EntityId right) => !left.Equals(right);
    }
#endif
}
