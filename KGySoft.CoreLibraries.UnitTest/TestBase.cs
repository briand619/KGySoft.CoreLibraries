﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TestBase.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

#region Used Namespaces

using System;
using System.Collections;
#if !NET35
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
#endif
using System.Collections.Generic;
#if NETCOREAPP
using System.Collections.Immutable;
#endif
using System.Collections.Specialized;
#if NETFRAMEWORK
using System.ComponentModel.Design; 
#endif
using System.Drawing;
using System.Drawing.Imaging; 
using System.IO;
using System.Linq;
using System.Reflection;
#if NET5_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif
#if NETFRAMEWORK
using System.Security;
using System.Security.Permissions;
using System.Security.Policy; 
#endif
using System.Text;
#if NETFRAMEWORK
using System.Windows.Forms;
#endif

using KGySoft.Collections;
using KGySoft.Reflection;
using KGySoft.Resources;
using KGySoft.Serialization.Binary;

using NUnit.Framework;

#endregion

#region Used Aliases

using Assert = NUnit.Framework.Assert;
#if NETFRAMEWORK
using SystemDataNode = System.Resources.ResXDataNode;
using SystemFileRef = System.Resources.ResXFileRef; 
#endif

#endregion

#endregion

namespace KGySoft.CoreLibraries
{
    public class TestBase
    {
        #region Enumerations

        protected enum TargetFramework
        {
            NetStandard20,
            NetStandard21,
            Other
        }

        #endregion

        #region Properties

        protected static TargetFramework TestedFramework =>
#if NETCOREAPP
            typeof(PublicResources).Assembly.GetReferencedAssemblies().FirstOrDefault(asm => asm.Name == "netstandard") is AssemblyName an
                ? an.Version == new Version(2, 0, 0, 0) ? TargetFramework.NetStandard20
                    : an.Version == new Version(2, 1, 0, 0) ? TargetFramework.NetStandard21
                    : TargetFramework.Other
                : TargetFramework.Other;
#else
            TargetFramework.Other;
#endif

        #endregion

        #region Methods

        #region Protected Methods

        /// <summary>
        /// Asserts whether <paramref name="check"/> and <paramref name="reference"/> (can also be simple objects) are equal in depth.
        /// If <paramref name="forceEqualityByMembers"/> is <see langword="true"/>,
        /// then comparing by public fields/properties is forced for non-primitive types also when Equals is overridden.
        /// By default, Equals is called for types with overridden Equals.
        /// </summary>
        protected static void AssertDeepEquals(object reference, object check, bool forceEqualityByMembers = false)
        {
            var errors = new List<string>();
            AssertResult(CheckDeepEquals(reference, check, forceEqualityByMembers, errors, new HashSet<object>(ReferenceEqualityComparer.Comparer)), errors);
        }

        /// <summary>
        /// Asserts whether reference and target collections are equal in depth. If <paramref name="forceEqualityByMembers"/> is <see langword="true"/>,
        /// then comparing by public fields/properties is forced for non-primitive types also when Equals is overridden.
        /// </summary>
        protected static void AssertItemsEqual(IEnumerable referenceObjects, IEnumerable targetObjects, bool forceEqualityByMembers = false)
        {
            var errors = new List<string>();
            AssertResult(CheckItemsEqual(referenceObjects, targetObjects, forceEqualityByMembers, errors, new HashSet<object>(ReferenceEqualityComparer.Comparer)), errors);
        }

        /// <summary>
        /// Asserts whether <paramref name="check"/> and <paramref name="reference"/> are equal in depth by fields/public properties recursively.
        /// If the root objects can be simple objects use the <see cref="AssertDeepEquals"/> instead.
        /// </summary>
        protected static void AssertMembersEqual(object reference, object check, bool forceNestedEqualityByMembers = false)
        {
            var errors = new List<string>();
            AssertResult(CheckMembersEqual(reference, check, forceNestedEqualityByMembers, errors, new HashSet<object>(ReferenceEqualityComparer.Comparer)), errors);
        }

        /// <summary>
        /// Asserts whether <paramref name="check"/> and <paramref name="reference"/> are equal in depth by fields/public properties recursively.
        /// If the root objects can be simple objects use the <see cref="AssertDeepEquals"/> instead.
        /// </summary>
        protected static void AssertMembersAndItemsEqual(IEnumerable reference, IEnumerable check, bool forceNestedEqualityByMembers = false)
        {
            var errors = new List<string>();
            AssertResult(CheckMembersAndItemsEqual(reference, check, forceNestedEqualityByMembers, errors, new HashSet<object>(ReferenceEqualityComparer.Comparer)), errors);
        }

        /// <summary>
        /// Gets whether <paramref name="check"/> and <paramref name="reference"/> (can also be simple objects) are equal in depth. If <paramref name="forceEqualityByMembers"/> is <see langword="true"/>,
        /// then comparing by public fields/properties is forced for non-primitive types also when Equals is overridden.
        /// </summary>
        protected static bool DeepEquals(object reference, object check, bool forceEqualityByMembers = false)
            => CheckDeepEquals(reference, check, forceEqualityByMembers, null, new HashSet<object>(ReferenceEqualityComparer.Comparer));

        /// <summary>
        /// Gets whether <paramref name="check"/> and <paramref name="reference"/> (can also be simple objects) are equal by their public members.
        /// </summary>
        protected static bool MembersEqual(object reference, object check, bool forceNestedEqualityByMembers = false)
            => CheckMembersEqual(reference, check, forceNestedEqualityByMembers, null, new HashSet<object>(ReferenceEqualityComparer.Comparer));

        /// <summary>
        /// Gets whether reference and target collections are equal in depth. If <paramref name="forceEqualityByMembers"/> is <see langword="true"/>,
        /// then comparing by public fields/properties is forced for non-primitive types also when Equals is overridden.
        /// </summary>
        protected static bool ItemsEqual(IEnumerable referenceObjects, IEnumerable targetObjects, bool forceEqualityByMembers = false)
            => CheckItemsEqual(referenceObjects, targetObjects, forceEqualityByMembers, null, new HashSet<object>(ReferenceEqualityComparer.Comparer));

        /// <summary>
        /// Gets whether <paramref name="check"/> and <paramref name="reference"/> are equal in depth by fields/public properties recursively.
        /// If the root objects can be simple objects use the <see cref="CheckDeepEquals"/> instead.
        /// </summary>
        protected static bool MembersAndItemsEqual(IEnumerable reference, IEnumerable check, bool forceNestedEqualityByMembers = false)
            => CheckMembersAndItemsEqual(reference, check, forceNestedEqualityByMembers, null, new HashSet<object>(ReferenceEqualityComparer.Comparer));

        protected static void Throws<T>(TestDelegate code, string expectedMessageContent = null)
            where T : Exception
        {
            var e = Assert.Throws<T>(code);
            Assert.IsInstanceOf(typeof(T), e);
            Assert.IsTrue(expectedMessageContent == null || e.Message.Contains(expectedMessageContent, StringComparison.Ordinal), $"Expected message: {expectedMessageContent}{Environment.NewLine}Actual message: {e.Message}");
            Console.WriteLine($"Expected exception {typeof(T)} has been thrown: {e.Message}");
        }

        protected static bool ThrowsOnFramework<T>(TestDelegate code, params TargetFramework[] targets)
            where T : Exception
        {
            if (TestedFramework.In(targets))
            {
                Throws<T>(code);
                return true;
            }

            Assert.DoesNotThrow(code);
            return false;
        }

        protected static bool ThrowsOnFramework<T>(TestDelegate code, string expectedMessageContent, params TargetFramework[] targets)
            where T : Exception
        {
            if (TestedFramework.In(targets))
            {
                Throws<T>(code, expectedMessageContent);
                return true;
            }

            Assert.DoesNotThrow(code);
            return false;
        }

        protected static void CopyContent(object target, object source)
        {
            if (target == null || source == null)
                return;

            if (target is Array arrayTarget)
            {
                var arraySource = (Array)source;
                Array.Copy(arraySource, arrayTarget, arrayTarget.Length);
                return;
            }

            for (Type t = target.GetType(); t != null; t = t.BaseType)
            {
                foreach (FieldInfo field in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    Reflector.SetField(target, field, Reflector.GetField(source, field));
            }
        }

#if NETFRAMEWORK
        protected static AppDomain CreateSandboxDomain(params IPermission[] permissions)
        {
            try
            {
                var evidence = new Evidence(AppDomain.CurrentDomain.Evidence);
                var permissionSet = GetPermissionSet(permissions);
                var setup = new AppDomainSetup
                {
                    ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                };

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var strongNames = new List<StrongName>();
                foreach (Assembly asm in assemblies)
                {
                    AssemblyName asmName = asm.GetName();
                    strongNames.Add(new StrongName(new StrongNamePublicKeyBlob(asmName.GetPublicKey()), asmName.Name, asmName.Version));
                }

                return AppDomain.CreateDomain("SandboxDomain", evidence, setup, permissionSet, strongNames.ToArray());
            }
            catch (MissingMethodException e) when (EnvironmentHelper.IsMono)
            {
                Assert.Inconclusive($"Not supported on Mono: {e}");
                throw;
            }
        }
#endif

        protected static string Combine(string p1, string p2, string p3) =>
#if NET35
            Path.Combine(p1, Path.Combine(p2, p3));
#else
            Path.Combine(p1, p2, p3);
#endif

        #endregion

        #region Private Methods

        private static bool CheckDeepEquals(object reference, object check, bool forceEqualityByMembers, List<string> errors, HashSet<object> checkedObjects)
        {
            if (ReferenceEquals(reference, check))
                return true;

            Type typeRef = reference?.GetType();
            Type typeChk = check?.GetType();
            if (!Check(typeRef != null && typeChk != null, $"'{reference ?? "<null>"}' compared to '{check ?? "<null>"}'", errors))
                return false;

#pragma warning disable CS0618 // Type or member is obsolete
            if (typeRef == typeof(AnyObjectSerializerWrapper))
                return CheckDeepEquals(Reflector.GetField(reference, "obj"), check, forceEqualityByMembers, errors, checkedObjects);
#pragma warning restore CS0618

            if (typeRef != typeChk)
            {
                if (typeRef == typeof(ResXFileRef))
                    return Check(CheckDeepEquals(Reflector.ResolveType(((ResXFileRef)reference).TypeName), typeChk, forceEqualityByMembers, errors, checkedObjects), $"File reference type error. Expected type: {typeChk}", errors);

                if (typeRef == typeof(ResXDataNode))
                    return CheckDeepEquals(((ResXDataNode)reference).GetValue(), check, forceEqualityByMembers, errors, checkedObjects);

#if NETFRAMEWORK
            if (typeRef == typeof(SystemFileRef))
                return Check(CheckDeepEquals(Reflector.ResolveType(((SystemFileRef)reference).TypeName), typeChk, forceEqualityByMembers, errors, checkedObjects), $"File reference type error. Expected type: {typeChk}", errors);

            if (typeRef == typeof(SystemDataNode))
                return CheckDeepEquals(((SystemDataNode)reference).GetValue((ITypeResolutionService)null), check, forceEqualityByMembers, errors, checkedObjects);
#endif

                Fail($"Types are different. {typeRef} <-> {typeChk}", errors);
                return false;
            }

            if (typeRef == typeof(object))
                return true;

            // checking circular reference
            if (checkedObjects.Contains(reference))
                return true;
            checkedObjects.Add(reference);
            try
            {
                // ignoring items and checking members only because of the backing array that can be larger than the segment/buffer
                if (typeRef.IsGenericTypeOf(typeof(ArraySegment<>)) || typeRef.IsGenericTypeOf(typeof(ArraySection<>)) || typeRef.IsGenericTypeOf(typeof(Array2D<>)) || typeRef.IsGenericTypeOf(typeof(Array3D<>)))
                    return CheckMembersEqual(reference, check, forceEqualityByMembers, errors, checkedObjects);

#if NETCOREAPP2_0_OR_GREATER
                // getting the enumerator throws an exception for default immutable arrays so performing some pre-checks before examining the items
                if (typeRef.IsGenericTypeOf(typeof(ImmutableArray<>)))
                {
                    bool isDefaultRef = (bool)Accessors.GetPropertyValue(reference, nameof(ImmutableArray<_>.IsDefault))!;
                    bool isDefaultCheck = (bool)Accessors.GetPropertyValue(check, nameof(ImmutableArray<_>.IsDefault))!;
                    if (isDefaultRef && isDefaultCheck)
                        return true;
                    if (isDefaultRef || isDefaultCheck)
                    {
                        Fail($"{typeRef.GetName(TypeNameKind.ShortName)}.{nameof(ImmutableArray<_>.IsDefault)} {isDefaultRef} <-> {isDefaultCheck}", errors);
                        return false;
                    }
                }
#endif

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                if (typeRef.IsGenericTypeOf(typeof(Memory<>)) || typeRef.IsGenericTypeOf(typeof(ReadOnlyMemory<>)))
                {
                    // Length, IsEmpty
                    if (!CheckMembersEqual(reference, check, forceEqualityByMembers, errors, checkedObjects))
                        return false;

                    // Items - cannot use SequenceEqual because it only accepts spans
                    return CheckItemsEqual((IEnumerable)Reflector.InvokeMethod(reference, "ToArray"), (IEnumerable)Reflector.InvokeMethod(check, "ToArray"), forceEqualityByMembers, errors, checkedObjects);
                }
#endif

                if (reference is not (string or StringSegment) && reference is IEnumerable enumerable)
                    return forceEqualityByMembers
                        ? CheckMembersAndItemsEqual(enumerable, (IEnumerable)check, true, errors, checkedObjects)
                        : CheckItemsEqual(enumerable, (IEnumerable)check, false, errors, checkedObjects);

                if (reference is float floatRef && check is float floatCheck)
                    return Check(BitConverter.ToInt32(BitConverter.GetBytes(floatRef), 0) == BitConverter.ToInt32(BitConverter.GetBytes((float)check), 0), $"Float equality failed: {floatRef.ToRoundtripString()} <-> {floatCheck.ToRoundtripString()}. Binary representation: 0x{BitConverter.GetBytes(floatRef).ToHexValuesString()} <-> 0x{BitConverter.GetBytes(floatCheck).ToHexValuesString()}", errors);

                if (reference is double doubleRef && check is double doubleCheck)
                    return Check(BitConverter.DoubleToInt64Bits(doubleRef) == BitConverter.DoubleToInt64Bits(doubleCheck), $"Double equality failed: {doubleRef.ToRoundtripString()} <-> {doubleCheck.ToRoundtripString()}. Binary representation: 0x{BitConverter.GetBytes(doubleRef).ToHexValuesString()} <-> 0x{BitConverter.GetBytes(doubleCheck).ToHexValuesString()}", errors);

#if NET5_0_OR_GREATER
                if (reference is Half halfRef && check is Half halfCheck)
                    return Check(Unsafe.As<Half, ushort>(ref halfRef)  == Unsafe.As<Half, ushort>(ref halfCheck), $"Half equality failed: {halfRef:R} <-> {halfCheck:R}. Binary representation: 0x{Unsafe.As<Half, ushort>(ref halfRef):X4} <-> 0x{Unsafe.As<Half, ushort>(ref halfCheck):X4}", errors);
#endif

                if (reference is decimal decimalRef && check is decimal decimalCheck)
                    return Check(Decimal.GetBits(decimalRef).SequenceEqual(Decimal.GetBits(decimalCheck)), $"Decimal equality failed: {decimalRef.ToRoundtripString()} <-> {decimalCheck.ToRoundtripString()}. Binary representation: 0x{Decimal.GetBits(decimalRef).Select(i => $"{i:X8}").Join(String.Empty)} <-> 0x{Decimal.GetBits(decimalCheck).Select(i => $"{i:X8}").Join(String.Empty)}", errors);

                if (typeRef == typeof(StringBuilder))
                {
                    StringBuilder sbRef = (StringBuilder)reference;
                    StringBuilder sbCheck = (StringBuilder)check;
                    bool result = Check(sbRef.MaxCapacity == sbCheck.MaxCapacity, $"{nameof(StringBuilder)}.{nameof(StringBuilder.MaxCapacity)} {sbRef.MaxCapacity} <-> {sbCheck.MaxCapacity}", errors);
#if !NET35 // when deserializing by fields, Capacity is lost in .NET 3.5 and will be the same as value length
                    result &= Check(sbRef.Capacity == sbCheck.Capacity, $"{nameof(StringBuilder)}.{nameof(StringBuilder.Capacity)} {sbRef.Capacity} <-> {sbCheck.Capacity}", errors); 
#endif
                    result &= Check(sbRef.Length == sbCheck.Length, $"{nameof(StringBuilder)}.{nameof(StringBuilder.Length)} {sbRef.Length} <-> {sbCheck.Length}", errors);
                    result &= Check(sbRef.ToString() == sbCheck.ToString(), $"{nameof(StringBuilder)}: {sbRef} <-> {sbCheck}", errors);
                    return result;
                }

                if (reference is Stream stream)
                    return CheckStreams(stream, (Stream)check, errors);

                if (typeRef.IsGenericTypeOf(typeof(KeyValuePair<,>))
                    || typeRef == typeof(DictionaryEntry))
                {
                    string propName = nameof(DictionaryEntry.Key);
                    bool result = CheckMemberDeepEquals($"{typeRef}.{propName}", Reflector.GetProperty(reference, propName), Reflector.GetProperty(check, propName), forceEqualityByMembers, errors, checkedObjects);
                    propName = nameof(DictionaryEntry.Value);
                    result &= CheckMemberDeepEquals($"{typeRef}.{propName}", Reflector.GetProperty(reference, propName), Reflector.GetProperty(check, propName), forceEqualityByMembers, errors, checkedObjects);

                    return result;
                }

#if WINDOWS
                if (typeRef == typeof(Bitmap))
                    return CheckImages((Bitmap)reference, (Bitmap)check, errors);
#endif

#if NETFRAMEWORK
                if (typeRef == typeof(ImageListStreamer))
                {
                    var il1 = new ImageList { ImageStream = (ImageListStreamer)reference };
                    var il2 = new ImageList { ImageStream = (ImageListStreamer)check };
                    return CheckItemsEqual(il1.Images, il2.Images, forceEqualityByMembers, errors, checkedObjects);
                }  
#endif

                bool simpleEquals = typeRef.IsEnum || typeof(Encoding).IsAssignableFrom(typeRef);

                // Structural equality if forced for non-primitive types or when Equals is not overridden
                if (forceEqualityByMembers && !typeRef.IsPrimitive && !typeof(IComparable).IsAssignableFrom(typeRef)
                    || !simpleEquals && !typeRef.GetMember(nameof(Equals), MemberTypes.Method, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Any(m => m is MethodInfo mi && mi.GetParameters() is ParameterInfo[] parameters && parameters.Length == 1 && parameters[0].ParameterType == typeof(object) && mi.DeclaringType != mi.GetBaseDefinition().DeclaringType))
                {
                    return CheckMembersEqual(reference, check, forceEqualityByMembers, errors, checkedObjects);
                }

                // Equals as fallback
                return Check(Equals(reference, check), $"Equality check failed at type {typeRef.GetName(TypeNameKind.ShortName)}: {reference} <-> {check}", errors);
            }
            finally
            {
                checkedObjects.Remove(reference);
            }
        }

        private static bool CheckItemsEqual(IEnumerable referenceObjects, IEnumerable targetObjects, bool forceEqualityByMembers, List<string> errors, HashSet<object> checkedObjects)
        {
            Type type = referenceObjects.GetType();
            if (referenceObjects is Hashtable or StringDictionary)
            {
                referenceObjects = referenceObjects.Convert<List<DictionaryEntry>>().OrderBy(i => i.Key.ToString()).ToList();
                targetObjects = targetObjects.Convert<List<DictionaryEntry>>().OrderBy(i => i.Key.ToString()).ToList();
            }
            else if (type.DeclaringType == typeof(Hashtable) // HashTable.KeyCollection/ValueCollection
#if !NET35
                || type.IsGenericTypeOf(typeof(ReadOnlyCollection<>)) // ConcurrentDictionary.Keys/Values
                || type.IsGenericTypeOf(typeof(ConcurrentBag<>))
#endif
                )
            {
                referenceObjects = referenceObjects.Cast<object>().OrderBy(i => i?.ToString()).ToList();
                targetObjects = targetObjects.Cast<object>().OrderBy(i => i?.ToString()).ToList();
            }
#if !NET35
            else if (type.IsGenericTypeOf(typeof(ConcurrentDictionary<,>))
#if NETCOREAPP
                || type.IsGenericTypeOf(typeof(ImmutableDictionary<,>.Builder))
#endif
                     )
            {
                referenceObjects = referenceObjects.Convert<List<KeyValuePair<object, object>>>().OrderBy(i => i.Key.ToString()).ToList();
                targetObjects = targetObjects.Convert<List<KeyValuePair<object, object>>>().OrderBy(i => i.Key.ToString()).ToList();
            }
#endif
            
            IEnumerator enumRef = referenceObjects.GetEnumerator();
            IEnumerator enumChk = targetObjects.GetEnumerator();

            int index = 0;
            bool result = true;
            while (enumRef.MoveNext())
            {
                if (!Check(enumChk.MoveNext(), $"{type.GetName(TypeNameKind.ShortName)}: Reference collection contains more than {index} objects.", errors))
                    return false;

                var subErrors = new List<string>();
                result &= CheckDeepEquals(enumRef.Current, enumChk.Current, forceEqualityByMembers, subErrors, checkedObjects);
                if (subErrors.Count > 0)
                    errors?.Add($"{type.GetName(TypeNameKind.ShortName)}[{index}]:{Environment.NewLine}\t{subErrors.Join($"{Environment.NewLine}\t")}");

                index++;
            }

            result &= Check(!enumChk.MoveNext(), $"{type.GetName(TypeNameKind.ShortName)}: Target collection contains more than {index} objects.", errors);
            return result;
        }

        private static bool CheckMemberDeepEquals(string name, object reference, object check, bool forceEqualityByMembers, List<string> errors, HashSet<object> checkedObjects)
        {
            var subErrors = new List<string>();
            bool result = CheckDeepEquals(reference, check, forceEqualityByMembers, subErrors, checkedObjects);
            if (subErrors.Count > 0)
                errors?.Add($"{name}:{Environment.NewLine}\t{subErrors.Join($"{Environment.NewLine}\t")}");

            return result;
        }

        private static bool CheckMembersEqual(object reference, object check, bool forceNestedEqualityByMembers, List<string> errors, HashSet<object> checkedObjects)
        {
            if (reference == null && check == null)
                return true;

            Type typeRef = reference?.GetType();
            Type typeChk = check?.GetType();
            if (!Check(typeRef != null && typeChk != null, $"{typeRef?.ToString() ?? "null"} compared to {typeChk?.ToString() ?? "null"}", errors))
                return false;

            if (!Check(typeRef == typeChk, $"Types are different. {typeRef} <-> {typeChk}", errors))
                return false;

            bool result = true;

            // public fields
            foreach (FieldInfo field in typeRef.GetFields(BindingFlags.Instance | BindingFlags.Public))
                result &= CheckMemberDeepEquals($"{typeRef.GetName(TypeNameKind.ShortName)}.{field.Name}", field.Get(reference),  field.Get(check), forceNestedEqualityByMembers, errors, checkedObjects);

            // public properties
            foreach (PropertyInfo property in reference.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.GetIndexParameters().Length == 0))
            {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                if (property.PropertyType.IsGenericTypeOf(typeof(Span<>)) || property.PropertyType.IsGenericTypeOf(typeof(ReadOnlySpan<>)))
                    continue;
#endif
                result &= CheckMemberDeepEquals($"{typeRef.GetName(TypeNameKind.ShortName)}.{property.Name}", property.Get(reference), property.Get(check), forceNestedEqualityByMembers, errors, checkedObjects);
            }

            return result;
        }

        private static bool CheckMembersAndItemsEqual(IEnumerable reference, IEnumerable check, bool forceNestedEqualityByMembers, List<string> errors, HashSet<object> checkedObjects)
        {
            // members
            bool result = CheckMembersEqual(reference, check, forceNestedEqualityByMembers, errors, checkedObjects);

            // collection elements
            if (!(reference is string or StringSegment || check is string or StringSegment))
                result &= CheckItemsEqual(reference, check, true, errors, checkedObjects);
            return result;
        }

        private static bool CheckStreams(Stream reference, Stream check, List<string> errors)
        {
            if (!Check(reference.Length == check.Length, $"Length of the streams are different: {reference.Length} <-> {check.Length}", errors))
                return false;
            if (reference.Length == 0L)
                return true;

            long origPosRef = reference.Position;
            long origPosCheck = check.Position;
            if (origPosRef != 0L)
            {
                if (!reference.CanSeek)
                {
                    errors?.Add("Cannot seek the reference stream - compare cannot be performed");
                    return true;
                }

                reference.Position = 0L;
            }

            if (origPosCheck != 0L)
            {
                if (!check.CanSeek)
                {
                    errors?.Add("Cannot seek the check stream - compare cannot be performed");
                    return true;
                }

                check.Position = 0L;
            }

            for (long i = 0; i < reference.Length; i++)
            {
                int r, c;
                if (!Check((r = reference.ReadByte()) == (c = check.ReadByte()), $"Streams are different at position {i}: {r} <-> {c}", errors))
                    return false;
            }

            if (reference.CanSeek)
                reference.Position = origPosRef;
            if (check.CanSeek)
                check.Position = origPosCheck;

            return true;
        }

#if WINDOWS
        private static bool CheckImages(Bitmap reference, Bitmap check, List<string> errors)       
        {
            // using the not so fast GetPixel compare. This works also for different pixel formats and raw formats.
            // There is a 2% brightness tolerance for icons
            if (!Check(reference.Size == check.Size, $"Images have different size: {reference.Size} <-> {check.Size}", errors))
                return false;

            bool isIcon = reference.RawFormat.Guid == ImageFormat.Icon.Guid;

            for (int y = 0; y < reference.Height; y++)
            {
                for (int x = 0; x < reference.Width; x++)
                {
                    Color c1, c2;
                    if (!Check((c1 = reference.GetPixel(x, y)) == (c2 = check.GetPixel(x, y)) || (isIcon && Math.Abs(c1.GetBrightness() - c2.GetBrightness()) < 0.02f), $"Pixels at {x};{y} are different: {c1}<->{c2}", errors))
                        return false;
                }
            }

            return true;
        }
#endif

        private static void AssertResult(bool result, List<string> errors)
        {
            if (!result)
                Assert.Fail(errors.Join(Environment.NewLine));
            else if (errors.Count > 0)
                Assert.Inconclusive(errors.Join(Environment.NewLine));
        }

        private static bool Check(bool condition, string message, List<string> errors)
        {
            if (condition)
                return true;
            Fail(message, errors);
            return false;
        }

        private static void Fail(string message, List<string> errors) => errors?.Add(message);

#if NETFRAMEWORK
        private static PermissionSet GetPermissionSet(IPermission[] permissions)
        {
            var evidence = new Evidence();
#if !NET35
            evidence.AddHostEvidence(new Zone(SecurityZone.Internet));
            var result = SecurityManager.GetStandardSandbox(evidence);
#else
            var result = new PermissionSet(PermissionState.None);
#endif
            foreach (var permission in permissions)
                result.AddPermission(permission);
            return result;
        } 
#endif

        #endregion

        #endregion
    }
}
