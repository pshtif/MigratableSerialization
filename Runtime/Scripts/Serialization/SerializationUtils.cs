/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System.Collections.Generic;
using System.Linq;
using OdinSerializer;
using OdinSerializer.Utilities;
using UnityEngine;

namespace Nodemon
{
    public class SerializationUtils
    {
        static protected Dictionary<Object,List<Object>> _cachedReferences = new Dictionary<Object, List<Object>>();
        
        static public byte[] SerializeUnityObjectToBytes(Object p_object, DataFormat p_format = DataFormat.Binary)
        {
            byte[] bytes = null;
            List<Object> references = new Object[] {p_object}.ToList();

            using (var cachedContext = Cache<SerializationContext>.Claim())
            {
                cachedContext.Value.Config.SerializationPolicy = SerializationPolicies.Everything;
                UnitySerializationUtility.SerializeUnityObject(p_object, ref bytes, ref references, DataFormat.JSON, true,
                    cachedContext.Value);
            }

            _cachedReferences.Add(p_object, references);

            return bytes;
        }

        static public void DeserializeUnityObjectFromBytes(ref Object p_object, byte[] p_bytes)
        {
            List<Object> p_references;
            if (_cachedReferences.ContainsKey(p_object))
            {
                p_references = _cachedReferences[p_object];
            }
            else
            {
                Debug.Log("No cached references for deserialization potentional loss in correct data serialization.");
                p_references = new List<Object>(); 
            }

            using (var cachedContext = Cache<DeserializationContext>.Claim())
            {
                cachedContext.Value.Config.SerializationPolicy = SerializationPolicies.Everything;
                UnitySerializationUtility.DeserializeUnityObject(p_object, ref p_bytes, ref p_references, DataFormat.JSON,
                    cachedContext.Value);
            }
        }
    }
}