/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OdinSerializer.Utilities;
using UnityEngine;

namespace Nodemon
{
    public static class MigrationUtils
    {
        private static bool _isValid = false;
        
        public static bool CheckValidity()
        {
            if (_migratableTypeCache == null)
            {
                _migratableTypeCache = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
                    .Where(t => typeof(IMigratable).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract).ToList();
                
                if (_migratableTypeCache.Exists(t => t.GetAttribute<SerializedIdAttribute>() == null))
                {
                    Debug.LogError("SerializationIdAttribute missing on IMigratable type.");
                    _isValid = false;
                }
                else
                {
                    _isValid = true;
                }
            }

            return _isValid;
        }            
        
        private static List<Type> _migratableTypeCache;

        public static List<Type> GetAllMigratableTypes()
        {
            if (_migratableTypeCache == null)
            {
                _migratableTypeCache = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
                    .Where(t => typeof(IMigratable).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract).ToList();

                CreateVersionStacksCache();
            }

            return _migratableTypeCache;
        }

        private static Dictionary<string, List<Type>> _versionStacksCache;

        private static void CreateVersionStacksCache()
        {
            _versionStacksCache = new Dictionary<string, List<Type>>();

            foreach (var type in _migratableTypeCache)
            {
                var id = type.GetAttribute<SerializedIdAttribute>().Id;
                if (_versionStacksCache.ContainsKey(id))
                    continue;
                
                var versionStack = GetAllMigratableTypes()
                    .FindAll(t => t.GetAttribute<SerializedIdAttribute>().Id == id)
                    .OrderBy(t => t.GetAttribute<SerializedIdAttribute>().Version)
                    .ToList();
                
                _versionStacksCache.Add(id, versionStack);
            }
        }

        static public List<Type> GetVersionStackForType(Type p_type)
        {
            var id = p_type.GetAttribute<SerializedIdAttribute>().Id;
            _versionStacksCache.TryGetValue(id, out List<Type> versionStack);
            return versionStack;
        }
        
        private static Dictionary<Type, string> _typeToNameCache;
        
        public static string GetTypeName(Type p_type) 
        {
            _typeToNameCache ??= new Dictionary<Type, string>();

            if (_typeToNameCache.TryGetValue(p_type, out string cachedName))
            {
                return cachedName;
            }

            string name = p_type.IsDefined(typeof(SerializedIdAttribute))
                ? p_type.GetAttribute<SerializedIdAttribute>().Id + "." +
                  p_type.GetAttribute<SerializedIdAttribute>().Version
                : p_type.Namespace + "." + p_type.Name;
			
            if (p_type.GenericTypeArguments.Length > 0)
            {
                name += "<";
                for (int i = 0; i < p_type.GenericTypeArguments.Length; ++i)
                {
                    if (i > 0)
                    {
                        name += ",";
                    }

                    name += GetTypeName(p_type.GenericTypeArguments[i]);
                }

                name += ">";
            }

            _typeToNameCache.Add(p_type, name);
            return name;
        }
    }
}