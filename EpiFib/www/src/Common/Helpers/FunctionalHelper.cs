// <copyright file="FunctionalHelper.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Helpers
{
    using System;
    using System.Collections.Generic;

    public static class FunctionalHelper
    {
        public static Func<K, R> Memoize<K, R>(Func<K, R> getDataFunc)
        {
            EFGuard.NotNull(getDataFunc, nameof(getDataFunc));

            var index = new Dictionary<K, R>();

            return key =>
            {
                R result;

                if (object.ReferenceEquals(key, null))
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (!index.TryGetValue(key, out result))
                {
                    result = getDataFunc(key);
                    index.Add(key, result);
                }

                return result;
            };
        }
    }
}
