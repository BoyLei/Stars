using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;




namespace StarProject.OffLine
{
    [ShowOdinSerializedPropertiesInInspector]
    public class ModelOffLineData : MonoBehaviour, ISerializationCallbackReceiver, ISupportsPrefabSerialization

    {

        [SerializeField, HideInInspector]
        private SerializationData serializationData;

        SerializationData ISupportsPrefabSerialization.SerializationData { get { return this.serializationData; } set { this.serializationData = value; } }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            UnitySerializationUtility.DeserializeUnityObject(this, ref this.serializationData);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            UnitySerializationUtility.SerializeUnityObject(this, ref this.serializationData);
        }

        public Dictionary<string, Transform> BindDummyPos = new Dictionary<string, Transform>();

        public float _ModelHeight = 2.0f;

        public Transform GetTransformByKey(string key)
        {
            Transform point = null;
            if (BindDummyPos.ContainsKey(key))
            {
                point = BindDummyPos[key];
            }
            return point;
        }

        public float GetModelHeight()
        {
            return _ModelHeight;
        }
    }
}