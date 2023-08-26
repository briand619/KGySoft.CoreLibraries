﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SerializationHelper.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Diagnostics.CodeAnalysis;
#if NETFRAMEWORK
using System.CodeDom.Compiler;
#endif
#if !NET35
using System.Collections;
#endif
using System.Globalization;
using System.Linq;
using System.Reflection;

using KGySoft.Collections;
using KGySoft.CoreLibraries;
using KGySoft.Reflection;

using TypeExtensions = KGySoft.CoreLibraries.TypeExtensions;

#endregion

#region Suppressions

#if NET8_0_OR_GREATER
#pragma warning disable SYSLIB0050 // Type.IsSerializable/FieldInfo.IsNotSerialized is obsolete - required by BinarySerializationFormatter, which still supports the original infrastructure
#endif

#endregion

namespace KGySoft.Serialization
{
    internal static class SerializationHelper
    {
        #region Fields

        private static readonly IThreadSafeCacheAccessor<Type, FieldInfo[]> serializableFieldsCache = ThreadSafeCacheFactory.Create<Type, FieldInfo[]>(t =>
            t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(f => !f.IsNotSerialized)
                .OrderBy(f => f.MetadataToken).ToArray(), LockFreeCacheOptions.Profile1K);

        private static readonly Type[] unsafeTypes =
        {
#if NETFRAMEWORK
            typeof(TempFileCollection),
#endif
#if !NET35
            StructuralComparisons.StructuralComparer.GetType(),
            StructuralComparisons.StructuralEqualityComparer.GetType(),
#endif
        };

        private static StringKeyedDictionary<Type>? nativelySupportedSimpleTypes;

        #endregion

        #region Properties

        private static StringKeyedDictionary<Type> NativelySupportedSimpleTypes
        {
            get
            {
                if (nativelySupportedSimpleTypes == null)
                {
                    var result = new StringKeyedDictionary<Type>();
                    foreach (Type type in TypeExtensions.GetNativelyParsedTypes())
                    {
                        result[type.AssemblyQualifiedName!] = type;
                        var legacyName = AssemblyResolver.GetForwardedAssemblyName(type);
                        if (legacyName.ForwardedAssemblyName != null)
                            result[$"{type.FullName}, {legacyName.ForwardedAssemblyName}"] = type;
                        if (legacyName.IsCoreIdentity || AssemblyResolver.IsCoreLibAssemblyName(type.Assembly.FullName))
                            result[type.FullName!] = type;
                    }

                    nativelySupportedSimpleTypes = result;
                }

                return nativelySupportedSimpleTypes;
            }
        }

        #endregion

        #region Methods

        internal static FieldInfo[] GetSerializableFields(Type t) => serializableFieldsCache[t];

        internal static StringKeyedDictionary<FieldInfo> GetFieldsWithUniqueNames(Type type, bool considerNonSerialized)
        {
            var result = new StringKeyedDictionary<(FieldInfo Field, int Count)>();

            // ReSharper disable once PossibleNullReferenceException
            for (Type t = type; t != Reflector.ObjectType; t = t.BaseType!)
            {
                // ReSharper disable once PossibleNullReferenceException
                FieldInfo[] fields = considerNonSerialized
                        ? GetSerializableFields(t)
                        : t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                foreach (FieldInfo field in fields)
                {
                    string name = field.Name;
                    if (!result.TryGetValue(name, out var entry))
                    {
                        result[name] = (field, 1);
                        continue;
                    }

                    // conflicting name 1st try: prefixing by type name
                    string prefixedName = field.DeclaringType!.Name + '+' + field.Name;
                    if (!result.ContainsKey(prefixedName))
                    {
                        result[prefixedName] = (field, 1);
                        continue;
                    }

                    // 1st try didn't work, using numeric postfix
                    entry.Count += 1;
                    result[name] = entry;
                    name += entry.Count.ToString(CultureInfo.InvariantCulture);
                    result[name] = (field, 1);
                }
            }

            return result.ToStringKeyedDictionary(e => e.Key, e => e.Value.Field);
        }

        /// <summary>
        /// Restores target from source. Can be used for read-only properties when source object is already fully serialized.
        /// </summary>
        internal static void CopyFields(object source, object target)
        {
            Debug.Assert(target != null! && source != null! && target.GetType() == source.GetType(), $"Same types are expected in {nameof(CopyFields)}.");
            Debug.Assert(!target!.GetType().IsArray, $"Arrays are not expected in {nameof(CopyFields)}.");

            for (Type? t = target.GetType(); t != null; t = t.BaseType)
            {
                foreach (FieldInfo field in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    field.Set(target, field.Get(source));
            }
        }

        internal static bool IsSafeType(Type type)
        {
#if NETFRAMEWORK
            // unsafeTypes are serializable in the .NET Framework but still we must not support them
            // in SafeMode because they can be used for known attacks
            return !type.In(unsafeTypes) && type.IsSerializable;
#else
            return type.IsSerializable || type.CanBeParsedNatively();
#endif
        }

        internal static bool IsUnsafeType(Type type) => type.In(unsafeTypes);

        internal static bool TryGetNativelySupportedSimpleType(string typeName, [NotNullWhen(true)]out Type? result)
            => NativelySupportedSimpleTypes.TryGetValue(typeName, out result);

        #endregion
    }
}
