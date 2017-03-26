// <copyright file="CommandTypes.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace IoTInfrastructure.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class CommandTypes
    {
        public static ReadOnlyDictionary<string, Type> Types { get; } = new ReadOnlyDictionary<string, Type>(
            new Dictionary<string, Type>
            {
                { "int16", typeof(short) },
                { "int", typeof(int) },
                { "int32", typeof(int) },
                { "int64", typeof(long) },
                { "sbyte", typeof(sbyte) },
                { "byte", typeof(byte) },
                { "double", typeof(double) },
                { "boolean", typeof(bool) },
                { "_bool", typeof(bool) },
                { "decimal", typeof(decimal) },
                { "single", typeof(float) },
                { "guid", typeof(Guid) },
                { "binary", typeof(string) },
                { "string", typeof(string) },
                { "date", typeof(DateTime) },
                { "datetimeoffset", typeof(DateTime) }
            });
    }
}
