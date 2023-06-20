/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using System;

namespace Nodemon
{
    public class SerializedIdAttribute : Attribute
    {
        public readonly string Id;
        public readonly int Version;

        public SerializedIdAttribute(string p_id, int p_version)
        {
            Id = p_id;
            Version = p_version;
        }

    }
}