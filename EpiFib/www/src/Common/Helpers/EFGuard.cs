// <copyright file="EFGuard.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Helpers
{
    using System;

    public class EFGuard
    {
        public static void NotNull<T>(T @object, string paramName)
        {
            if (@object == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        public static void StringNotNull(string str, string paramName)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}
