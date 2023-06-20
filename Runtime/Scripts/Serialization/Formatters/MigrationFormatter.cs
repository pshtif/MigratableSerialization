/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using OdinSerializer;
using OdinSerializer.Utilities;
using UnityEngine;
using IFormatter = OdinSerializer.IFormatter;

namespace Nodemon
{
    public class MigrationFormatter<T,K> : ReflectionOrEmittedBaseFormatter<T> where T:IMigratable where K:IMigratable
    {
        private Dictionary<string, List<Type>> _versionStackCache; 
        
        public MigrationFormatter()
        {
        }

        protected List<Type> GetVersionStackCache(Type p_type)
        {
            _versionStackCache ??= new Dictionary<string, List<Type>>();

            var id = p_type.GetAttribute<SerializedIdAttribute>().Id;
            List<Type> cache;
            if (_versionStackCache.TryGetValue(id, out cache))
            {
                return cache;
            }

            cache = MigrationUtils.GetAllMigratableTypes().FindAll(t => t.GetAttribute<SerializedIdAttribute>().Id == id).OrderBy(t => t.GetAttribute<SerializedIdAttribute>().Version)
                .ToList();

            return cache;
        }

        protected override void DeserializeImplementation(ref T p_value, IDataReader p_reader)
        {
            Debug.Log("MigratableFormatter.Deserialize: " + typeof(K));
            bool skipCurrentDeserialization = false;
            if (p_reader.PeekEntry(out string name) == EntryType.Integer)
            {
                p_reader.ReadInt32(out int version);

                var versionStack = GetVersionStackCache(typeof(K));
                var currentVersionType =
                    versionStack.Find(t => t.GetAttribute<SerializedIdAttribute>().Version == version);
                var currentVersionIndex = versionStack.IndexOf(currentVersionType);

                if (currentVersionIndex < versionStack.Count - 1)
                {
                    var currentVersion = DeserializeOldVersion(currentVersionType, p_reader);
                    
                    while (currentVersionIndex<versionStack.Count-1)
                    {
                        currentVersionIndex++;
                        var nextVersion = (IMigratable)FormatterServices.GetUninitializedObject(versionStack[currentVersionIndex]);
                        Debug.Log(nextVersion);
                        nextVersion.Migrate(currentVersion);
                        currentVersion = nextVersion;
                    }

                    p_value = (T)currentVersion;
                    skipCurrentDeserialization = true;
                }
            }

            if (!skipCurrentDeserialization)
            {
                var formatter =
                    (IFormatter)Activator.CreateInstance(typeof(MigrationFormatter<>).MakeGenericType(typeof(K)));
                p_value = (T)formatter.Deserialize(p_reader);
                //base.DeserializeImplementation(ref p_value, p_reader);
            }
        }

        protected override void SerializeImplementation(ref T p_value, IDataWriter p_writer)
        {
            if (p_value != null && p_value.GetType().IsDefined(typeof(SerializedIdAttribute), true))
            {
                p_writer.WriteInt32("v",p_value.GetType().GetAttribute<SerializedIdAttribute>().Version);
            }

            var formatter =
                (IFormatter)Activator.CreateInstance(typeof(MigrationFormatter<>).MakeGenericType(typeof(K)));
            formatter.Serialize(p_value, p_writer);
            //base.SerializeImplementation(ref p_value, p_writer);
        }

        protected IMigratable DeserializeOldVersion(Type p_type, IDataReader p_reader)
        {
            var formatter =
                (IFormatter)Activator.CreateInstance(
                    typeof(MigrationFormatter<,>).MakeGenericType(typeof(IMigratable), p_type));
            var migratable = (IMigratable)formatter.Deserialize(p_reader);
            return migratable;
        }
        
        protected override T GetUninitializedObject()
        {
            return (T)FormatterServices.GetUninitializedObject(typeof(K));
        }
    }

    public class MigrationFormatter<T> : ReflectionOrEmittedBaseFormatter<T> where T : IMigratable
    {
        public MigrationFormatter()
        {
        }
    }
}