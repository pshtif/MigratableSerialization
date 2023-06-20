/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;
using System.Linq;
using Nodemon;
using OdinSerializer;
using UnityEngine;

[assembly: RegisterFormatterLocator(typeof(MigrationFormatterLocator), -10)]

namespace Nodemon
{
    internal class MigrationFormatterLocator : IFormatterLocator
    {
        public bool TryGetFormatter(Type p_type, FormatterLocationStep step, ISerializationPolicy p_policy, bool p_allowWeakFallbackFormatters, out IFormatter p_formatter)
        {
            if (!typeof(IMigratable).IsAssignableFrom(p_type))
            {
                p_formatter = null;
                return false;
            }

            p_formatter =
                (IFormatter)Activator.CreateInstance(
                    typeof(MigrationFormatter<,>).MakeGenericType(typeof(IMigratable), p_type));
            Debug.Log(p_formatter);
            return true;
        }
    }
}