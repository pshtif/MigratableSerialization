using System;
using System.Collections.Generic;
using System.Reflection;
using OdinSerializer;
using OdinSerializer.Utilities;
using UnityEngine;

namespace Nodemon
{
	public class MigrationSerializationBinder : TwoWaySerializationBinder
	{
		private static MigrationSerializationBinder _instance;
		
		private Dictionary<Type, string> _bindTypeToName;
		private Dictionary<string, Type> _bindNameToType;

		public static MigrationSerializationBinder Init()
		{
			if (_instance == null)
			{
				_instance = new MigrationSerializationBinder();
			}

			return _instance;
		}

		private MigrationSerializationBinder()
		{
			_bindTypeToName = new Dictionary<Type, string>();
			_bindNameToType = new Dictionary<string, Type>();

			var types = MigrationUtils.GetAllMigratableTypes();
			
			foreach (var type in types)
			{
				string name = MigrationUtils.GetTypeName(type);
				_bindTypeToName.Add(type, name);
				_bindNameToType.Add(name, type);
			}
		}

		public override string BindToName(Type p_type, DebugContext p_debugContext = null)
		{
			if (_bindTypeToName.TryGetValue(p_type, out string name))
			{
				//Debug.Log("BindToName: "+p_type+" - "+name);
				return name;
			}

			return Default.BindToName(p_type, p_debugContext);
		}

		public override Type BindToType(string p_name, DebugContext p_debugContext = null)
		{
			if (_bindNameToType.TryGetValue(p_name, out Type type))
			{
				//Debug.Log("BindToType: "+p_name+" - "+type);
				return type;
			}

			return Default.BindToType(p_name, p_debugContext);
		}

		public override bool ContainsType(string p_name)
		{
			return _bindNameToType.ContainsKey(p_name) || Default.ContainsType(p_name); 
		}
	}
}