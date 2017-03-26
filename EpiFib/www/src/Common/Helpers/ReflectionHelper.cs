// <copyright file="ReflectionHelper.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>

namespace Common.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Dynamic;
    using System.Linq;
    using System.Reflection;
    using Dynamitey;
    using Microsoft.CSharp.RuntimeBinder;

    public static class ReflectionHelper
    {
        public static object[] EmptyArray { get; } = new object[0];

        public static object GetNamedPropertyValue(
            object item,
            string propertyName,
            bool usesCaseSensitivePropertyNameMatch,
            bool exceptionThrownIfNoMatch)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (string.IsNullOrEmpty("propertyName"))
            {
                throw new ArgumentException("propertyName is a null reference or empty string.", nameof(propertyName));
            }

            ICustomTypeDescriptor customProvider;
            IDynamicMetaObjectProvider dynamicProvider;
            if ((dynamicProvider = item as IDynamicMetaObjectProvider) != null)
            {
                return GetNamedPropertyValue(
                    dynamicProvider,
                    propertyName,
                    usesCaseSensitivePropertyNameMatch,
                    exceptionThrownIfNoMatch);
            }

            if ((customProvider = item as ICustomTypeDescriptor) != null)
            {
                return GetNamedPropertyValue(
                    customProvider,
                    propertyName,
                    usesCaseSensitivePropertyNameMatch,
                    exceptionThrownIfNoMatch);
            }

            StringComparison comparisonType = StringComparison.CurrentCultureIgnoreCase;
            if (usesCaseSensitivePropertyNameMatch)
            {
                comparisonType = StringComparison.CurrentCulture;
            }

            IEnumerable<PropertyInfo> matchingProps = item.GetType().GetProperties().Where(
                    t => string.Equals(t.Name, propertyName, comparisonType));

            MethodInfo methodInfo = matchingProps.Select(t => t.GetGetMethod()).FirstOrDefault(u => u != null);

            if (methodInfo == default(MethodInfo))
            {
                if (exceptionThrownIfNoMatch)
                {
                    throw new ArgumentException("propertyName does not name a property on item", nameof(propertyName));
                }

                return null;
            }

            return methodInfo.Invoke(item, EmptyArray);
        }

        public static Func<dynamic, dynamic> ProducePropertyValueExtractor(
            string propertyName,
            bool usesCaseSensitivePropertyNameMatch,
            bool exceptionThrownIfNoMatch)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("propertyName is a null reference or empty string.", nameof(propertyName));
            }

            Func<Type, MethodInfo> getMethodInfo =
                FunctionalHelper.Memoize<Type, MethodInfo>(ProduceGetMethodExtractor(propertyName, usesCaseSensitivePropertyNameMatch));

            return (item) =>
            {
                if (object.ReferenceEquals(item, null))
                {
                    throw new ArgumentNullException(nameof(item));
                }

                ICustomTypeDescriptor customTypeDescriptor;
                IDynamicMetaObjectProvider dynamicMetaObjectProvider;
                if ((dynamicMetaObjectProvider = item as IDynamicMetaObjectProvider) != null)
                {
                    return GetNamedPropertyValue(
                        dynamicMetaObjectProvider,
                        propertyName,
                        usesCaseSensitivePropertyNameMatch,
                        exceptionThrownIfNoMatch);
                }
                else if ((customTypeDescriptor = item as ICustomTypeDescriptor) != null)
                {
                    return GetNamedPropertyValue(
                        customTypeDescriptor,
                        propertyName,
                        usesCaseSensitivePropertyNameMatch,
                        exceptionThrownIfNoMatch);
                }
                else
                {
                    MethodInfo methodInfo = getMethodInfo(item.GetType());

                    if (methodInfo == null)
                    {
                        if (exceptionThrownIfNoMatch)
                        {
                            throw new ArgumentException("propertyName does not name a property on item", nameof(propertyName));
                        }
                    }
                    else
                    {
                        return methodInfo.Invoke(item, EmptyArray);
                    }
                }

                return null;
            };
        }

       public static void SetNamedPropertyValue(
            object item,
            object newValue,
            string propertyName,
            bool usesCaseSensitivePropertyNameMatch,
            bool exceptionThrownIfNoMatch)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (string.IsNullOrEmpty("propertyName"))
            {
                throw new ArgumentException("propertyName is a null reference or empty string.", nameof(propertyName));
            }

            ICustomTypeDescriptor customProvider;
            if ((customProvider = item as ICustomTypeDescriptor) != null)
            {
                SetNamedPropertyValue(
                    customProvider,
                    newValue,
                    propertyName,
                    usesCaseSensitivePropertyNameMatch,
                    exceptionThrownIfNoMatch);
            }

            StringComparison comparisonType = StringComparison.CurrentCultureIgnoreCase;
            if (usesCaseSensitivePropertyNameMatch)
            {
                comparisonType = StringComparison.CurrentCulture;
            }

            IEnumerable<PropertyInfo> matchingProps = item.GetType().GetProperties().Where(
                    t => string.Equals(t.Name, propertyName, comparisonType));

            MethodInfo methodInfo = matchingProps.Select(t => t.GetSetMethod()).FirstOrDefault(u => u != null);

            if (methodInfo == default(MethodInfo))
            {
                if (exceptionThrownIfNoMatch)
                {
                    throw new ArgumentException("propertyName does not name a property on item", nameof(propertyName));
                }
            }

           methodInfo?.Invoke(item, new object[] { newValue });
        }

        public static void SetNamedPropertyValue(
            ICustomTypeDescriptor item,
            object newValue,
            string propertyName,
            bool usesCaseSensitivePropertyNameMatch,
            bool exceptionThrownIfNoMatch)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (string.IsNullOrEmpty("propertyName"))
            {
                throw new ArgumentException("propertyName is a null reference or empty string.", nameof(propertyName));
            }

            StringComparison comparisonType = StringComparison.CurrentCultureIgnoreCase;
            if (usesCaseSensitivePropertyNameMatch)
            {
                comparisonType = StringComparison.CurrentCulture;
            }

            PropertyDescriptor descriptor = default(PropertyDescriptor);
            foreach (PropertyDescriptor pd in item.GetProperties())
            {
                if (string.Equals(propertyName, pd.Name, comparisonType))
                {
                    descriptor = pd;
                }
            }

            if (descriptor == default(PropertyDescriptor))
            {
                if (exceptionThrownIfNoMatch)
                {
                    throw new ArgumentException("propertyName does not name a property on item.", nameof(propertyName));
                }
            }
            else
            {
                descriptor.SetValue(item, newValue);
            }
        }

        public static IEnumerable<KeyValuePair<string, object>> ToKeyValuePairs(this IDynamicMetaObjectProvider item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            foreach (string memberName in Dynamic.GetMemberNames(item, true))
            {
                yield return new KeyValuePair<string, object>(
                    memberName,
                    Dynamic.InvokeGet(item, memberName));
            }
        }

        public static IEnumerable<KeyValuePair<string, object>> ToKeyValuePairs(this ICustomTypeDescriptor item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            foreach (PropertyDescriptor prop in item.GetProperties())
            {
                yield return new KeyValuePair<string, object>(
                    prop.Name,
                    prop.GetValue(item));
            }
        }

        public static IEnumerable<KeyValuePair<string, object>> ToKeyValuePairs(this object item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            IDynamicMetaObjectProvider metaObjectProvider;
            ICustomTypeDescriptor typeDescriptor;
            if ((metaObjectProvider = item as IDynamicMetaObjectProvider) != null)
            {
                return metaObjectProvider.ToKeyValuePairs();
            }
            else if ((typeDescriptor = item as ICustomTypeDescriptor) != null)
            {
                return typeDescriptor.ToKeyValuePairs();
            }

            return item.ToKeyValuePairImpl();
        }

        private static object GetNamedPropertyValue(
            ICustomTypeDescriptor item,
            string propertyName,
            bool usesCaseSensitivePropertyNameMatch,
            bool exceptionThrownIfNoMatch)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (string.IsNullOrEmpty("propertyName"))
            {
                throw new ArgumentException("propertyName is a null reference or empty string.", nameof(propertyName));
            }

            StringComparison comparisonType = StringComparison.CurrentCultureIgnoreCase;
            if (usesCaseSensitivePropertyNameMatch)
            {
                comparisonType = StringComparison.CurrentCulture;
            }

            PropertyDescriptor descriptor = default(PropertyDescriptor);
            foreach (PropertyDescriptor pd in item.GetProperties())
            {
                if (string.Equals(propertyName, pd.Name, comparisonType))
                {
                    descriptor = pd;
                    break;
                }
            }

            if (descriptor == default(PropertyDescriptor))
            {
                if (exceptionThrownIfNoMatch)
                {
                    throw new ArgumentException("propertyName does not name a property on item.", nameof(propertyName));
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return descriptor.GetValue(item);
            }
        }

        private static object GetNamedPropertyValue(
            IDynamicMetaObjectProvider item,
            string propertyName,
            bool usesCaseSensitivePropertyNameMatch,
            bool exceptionThrownIfNoMatch)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("propertyName is a null reference or empty string.", nameof(propertyName));
            }

            if (!usesCaseSensitivePropertyNameMatch || exceptionThrownIfNoMatch)
            {
                StringComparison comparisonType = StringComparison.CurrentCultureIgnoreCase;
                if (usesCaseSensitivePropertyNameMatch)
                {
                    comparisonType = StringComparison.CurrentCulture;
                }

                propertyName = Dynamic.GetMemberNames(item, true).FirstOrDefault(
                        t => string.Equals(t, propertyName, comparisonType));

                if (string.IsNullOrEmpty(propertyName))
                {
                    if (exceptionThrownIfNoMatch)
                    {
                        throw new ArgumentException("propertyName does not name a property on item", nameof(propertyName));
                    }

                    return null;
                }
            }

            try
            {
                return Dynamic.InvokeGet(item, propertyName);
            }
            catch (RuntimeBinderException)
            {
                return null;
            }
        }

        private static Func<Type, MethodInfo> ProduceGetMethodExtractor(
            string propertyName,
            bool usesCaseSensitivePropertyNameMatch)
        {
            Debug.Assert(!string.IsNullOrEmpty(propertyName), "propertyName is a null reference or empty string.");

            StringComparison comparisonType = StringComparison.CurrentCultureIgnoreCase;
            if (usesCaseSensitivePropertyNameMatch)
            {
                comparisonType = StringComparison.CurrentCulture;
            }

            return (type) =>
            {
                if (type == null)
                {
                    throw new ArgumentNullException(nameof(type));
                }

                IEnumerable<PropertyInfo> properties =
                    type.GetProperties().Where(t => string.Equals(propertyName, t.Name, comparisonType));

                return properties.Select(t => t.GetGetMethod()).FirstOrDefault(u => u != null);
            };
        }

        private static IEnumerable<KeyValuePair<string, object>> ToKeyValuePairImpl(this object item)
        {
            MethodInfo getter;

            Debug.Assert(item != null, "item is a null reference.");

            foreach (PropertyInfo prop in item.GetType().GetProperties())
            {
                if ((getter = prop.GetGetMethod()) != null)
                {
                    yield return new KeyValuePair<string, object>(
                        prop.Name,
                        getter.Invoke(item, EmptyArray));
                }
            }
        }
    }
}
