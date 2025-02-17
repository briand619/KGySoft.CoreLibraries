﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: XmlSerializerTest.Tests.cs
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

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
#if !NET35
using System.Numerics;
#endif
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using KGySoft.Collections;
using KGySoft.ComponentModel;
using KGySoft.Reflection;
using KGySoft.Serialization.Binary;
using KGySoft.Serialization.Xml;

using NUnit.Framework;

#endregion

namespace KGySoft.CoreLibraries.UnitTests.Serialization.Xml
{
    /// <summary>
    /// Test for XmlSerializer
    /// </summary>
    [TestFixture]
    public partial class XmlSerializerTest : TestBase
    {
        #region Methods

        [Test]
        public void SerializeSimpleTypes()
        {
            object[] referenceObjects =
            {
                null,
                new object(),
                true,
                (sbyte)1,
                (byte)1,
                (short)1,
                (ushort)1,
                1,
                (uint)1,
                (long)1,
                (ulong)1,
                'a',
                "alpha",
                (float)1,
                (double)1,
                (decimal)1,
                DateTime.UtcNow,
                DateTime.Now,
                Guid.NewGuid(),
            };

            SystemSerializeObject(referenceObjects);
            SystemSerializeObjects(referenceObjects);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None, expectedTypes: Reflector.EmptyArray<Type>());
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None, false, expectedTypes: Reflector.EmptyArray<Type>());

            // These types cannot be serialized with system serializer
            referenceObjects = new object[]
            {
                new IntPtr(1),
                new UIntPtr(1),
                new DateTimeOffset(DateTime.Now),
                new DateTimeOffset(DateTime.UtcNow),
                new DateTimeOffset(DateTime.Now.Ticks, new TimeSpan(1, 1, 0)),
                new TimeSpan(1, 2, 3, 4, 5),
                new DictionaryEntry(1, "alpha"),
                new KeyValuePair<int?,string>(1, "alpha"), // this includes Nullable<T>
#if !NET35
                new BigInteger(1),
#endif
#if NETCOREAPP3_0_OR_GREATER && !NETSTANDARD_TEST
                new Rune('a'),
#endif
#if NET5_0_OR_GREATER
                (Half)1,
#endif
#if NET6_0_OR_GREATER
                DateOnly.FromDateTime(DateTime.Today),
                TimeOnly.FromDateTime(DateTime.Now),
#endif
#if NET7_0_OR_GREATER
                (Int128)1,
                (UInt128)1,
#endif
            };

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None, expectedTypes: Reflector.EmptyArray<Type>());
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None, false, expectedTypes: Reflector.EmptyArray<Type>());
        }

        [Test]
        public void SerializeFloats()
        {
            object[] referenceObjects =
            {
                +0.0f,
                -0.0f,
                Single.NegativeInfinity,
                Single.PositiveInfinity,
                Single.NaN,
                Single.MinValue,
                Single.MaxValue,

                +0.0d,
                -0.0d,
                Double.NegativeInfinity,
                Double.PositiveInfinity,
                Double.NaN,
                Double.MinValue,
                Double.MaxValue,

                +0m,
                -0m,
                +0.0m,
                -0.0m,
                Decimal.MinValue,
                Decimal.MaxValue,
            };

            SystemSerializeObject(referenceObjects);
            SystemSerializeObjects(referenceObjects);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None, false);

#if NET5_0_OR_GREATER
            referenceObjects = new object[]
            {
                (Half)(+0.0f),
                (Half)(-0.0f),
                Half.NegativeInfinity,
                Half.PositiveInfinity,
                Half.NaN,
                Half.MinValue,
                Half.MaxValue,
            };

            SystemSerializeObjects(referenceObjects);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None, false);
#endif
        }

        [Test]
        public void SerializeChars()
        {
            object[] referenceObjects =
            {
                'a',
                'á',
                ' ',
                '\'',
                '<',
                '>',
                '"',
                '{',
                '}',
                '&',
                '\0',
                '\t', // U+0009 = <control> HORIZONTAL TAB
                '\n', // U+000a = <control> LINE FEED
                '\v', // U+000b = <control> VERTICAL TAB
                '\f', // U+000c = <control> FORM FEED
                '\r', // U+000d = <control> CARRIAGE RETURN
                '\x85', // U+0085 = <control> NEXT LINE
                '\xa0', // U+00a0 = NO-BREAK SPACE
                '\xffff', // U+FFFF = <noncharacter-FFFF>
                Char.ConvertFromUtf32(0x1D161)[0], // unpaired surrogate
                ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '​', '\u2028', '\u2029', '　', '﻿'
            };

            SystemSerializeObject(referenceObjects);
            SystemSerializeObjects(referenceObjects);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.EscapeNewlineCharacters);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.EscapeNewlineCharacters, false);
        }

#if NETCOREAPP3_0_OR_GREATER && !NETSTANDARD_TEST
        [Test]
        public void SerializeRunes()
        {
            object[] referenceObjects =
            {
                new Rune('a'),
                new Rune(' '),
                new Rune('\r'),
                new Rune('\n'),
                Rune.GetRuneAt("🏯", 0)
            };

            KGySerializeObject(referenceObjects, XmlSerializationOptions.EscapeNewlineCharacters);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.EscapeNewlineCharacters, false);
        }
#endif

        [Test]
        public void SerializeStrings()
        {
            string[] referenceObjects =
            {
                null,
                String.Empty,
                "One",
                "Two",
                " space ",
                "space after ",
                "space  space",
                "<>\\'\"&{}{{}}",
                "tab\ttab",
                "🏯", // paired surrogate
            };

            SystemSerializeObject(referenceObjects);
            SystemSerializeObjects(referenceObjects);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None, false);

            // These strings cannot be (de)serialized with system serializer
            referenceObjects = new string[]
            {
                Environment.NewLine,
                @"new

                    lines  ",
                " ",
                "\t",
                "\n",
                "\n\n",
                "\r",
                "\r\r",
                "\0",
                "\xFDD0", // U+FDD0 - <noncharacter-FDD0>
                "\xffff", // U+FFFF = <noncharacter-FFFF>
                "🏯"[0].ToString(null), // unpaired surrogate
                "<>\\'\"&{}{{}}\0\\0000",
                new string(new char[] { '\t', '\n', '\v', '\f', '\r', ' ', '\x0085', '\x00a0', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '​', '\u2028', '\u2029', '　', '﻿' }),
                "🏯" + "🏯"[0].ToString(null) + " b 🏯 " + "🏯"[1].ToString(null) + "\xffff \0 <>'\"&" // string containing unpaired surrogates
            };

            KGySerializeObject(referenceObjects, XmlSerializationOptions.EscapeNewlineCharacters);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.EscapeNewlineCharacters, false);
        }

        [Test]
        public void SerializeTypes()
        {
            object[] referenceObjects =
            {
                // Simple types
                typeof(int),
                typeof(int?),

                typeof(int).MakeByRefType(),
                typeof(int).MakePointerType(),
                typeof(SystemSerializableClass),
                typeof(SystemSerializableStruct?),
                Reflector.RuntimeType,
                typeof(void),
                typeof(TypedReference),

                // Arrays
                typeof(int[]),
                typeof(int[,]),
                typeof(int).MakeArrayType(1), // int[*]
                typeof(SystemSerializableClass[]), // custom array
                typeof(Array), // unspecified array

                // Pointers and References
                typeof(int*),
                typeof(int**),
                typeof(void*),
                typeof(void**),
                typeof(int*[]),
                typeof(int**[,]),
                typeof(int*[][]),
                typeof(int).MakeByRefType(), // int&
                typeof(int*).MakeByRefType(), // int*&
                typeof(int[]).MakePointerType(), // int[]* - actually not a valid type
                typeof(int[]).MakePointerType().MakePointerType(), // int[]** - actually not a valid type
                typeof(int[]).MakePointerType().MakePointerType().MakeByRefType(), // int[]**& - actually not a valid type

                // Closed Constructed Generics
                typeof(List<int>), // supported generic
                typeof(CustomGenericCollection<SystemSerializableClass>), // custom generic
                typeof(CustomGenericCollection<int>), // custom generic with supported parameter
                typeof(List<SystemSerializableClass>), // supported generic with custom parameter
                typeof(Dictionary<string, SystemSerializableClass>), // supported generic with mixed parameters
                typeof(List<Array>),
                typeof(List<int[]>),
                typeof(List<Array[]>),
                typeof(List<int>).MakeArrayType().MakePointerType().MakeArrayType(2).MakePointerType().MakeByRefType(), // List`1[System.Int32][]*[,]*&

                // Nullable collections
                typeof(DictionaryEntry?),
                typeof(KeyValuePair<int, string>?),
                typeof(KeyValuePair<int, SystemSerializableClass>?), // supported generic with mixed parameters

                // Generic Type Definitions
                typeof(List<>), // List`1, supported generic type definition
                typeof(List<>).MakeArrayType(), // List`1[] - does not really make sense
                typeof(List<>).MakeByRefType(), // List`1& - does not really make sense
                typeof(List<>).MakePointerType(), // List`1* - not really valid
                typeof(List<>).MakeArrayType().MakeByRefType(), // List`1[]& - does not really make sense
                typeof(List<>).MakeArrayType().MakePointerType().MakeArrayType(2).MakePointerType().MakeByRefType(), // List`1[]*[,]*&
                typeof(Dictionary<,>), // supported generic type definition
                typeof(CustomGenericCollection<>), // CustomGenericCollection`1, custom generic type definition
                typeof(CustomGenericCollection<>).MakeArrayType(), // CustomGenericCollection`1[] - does not really make sense
                typeof(CustomGenericCollection<>).MakeByRefType(), // CustomGenericCollection`1& - does not really make sense
                typeof(CustomGenericCollection<>).MakePointerType(), // CustomGenericCollection`1* - not really valid
                typeof(Nullable<>), // known special type definition
                typeof(Nullable<>).MakeArrayType(),
                typeof(Nullable<>).MakeByRefType(),
                typeof(Nullable<>).MakePointerType(),
                typeof(KeyValuePair<,>), // supported special type definition

                // Generic Type Parameters
                typeof(List<>).GetGenericArguments()[0], // T of supported generic type definition argument
                typeof(List<>).GetGenericArguments()[0].MakeArrayType(), // T[]
                typeof(List<>).GetGenericArguments()[0].MakeByRefType(), // T&
                typeof(List<>).GetGenericArguments()[0].MakePointerType(), // T*
                typeof(List<>).GetGenericArguments()[0].MakeArrayType().MakeByRefType(), // T[]&
                typeof(List<>).GetGenericArguments()[0].MakeArrayType().MakePointerType().MakeArrayType(2).MakePointerType().MakeByRefType(), // T[]*[,]*&
                typeof(CustomGenericCollection<>).GetGenericArguments()[0], // T of custom generic type definition argument
                typeof(CustomGenericCollection<>).GetGenericArguments()[0].MakeArrayType(), // T[]

                // Open Constructed Generics
                typeof(List<>).MakeGenericType(typeof(KeyValuePair<,>)), // List<KeyValuePair<,>>
                typeof(List<>).MakeGenericType(typeof(List<>)), // List<List<>>
                typeof(List<>).MakeGenericType(typeof(List<>).GetGenericArguments()[0]), // List<T>
                typeof(OpenGenericDictionary<>).BaseType, // open constructed generic (Dictionary<string, TValue>)
                typeof(KeyValuePair<,>).MakeGenericType(typeof(int), typeof(KeyValuePair<,>).GetGenericArguments()[1]), // open constructed generic (KeyValuePair<int, TValue>)
                typeof(Nullable<>).MakeGenericType(typeof(KeyValuePair<,>)), // open constructed generic (KeyValuePair<,>?)
                typeof(Nullable<>).MakeGenericType(typeof(KeyValuePair<,>).MakeGenericType(typeof(int), typeof(KeyValuePair<,>).GetGenericArguments()[1])), // open constructed generic (KeyValuePair<int, TValue>?)

                // Generic Method Parameters
                typeof(Array).GetMethod(nameof(Array.Resize)).GetGenericArguments()[0], // T of Array.Resize, unique generic method definition argument
                typeof(Array).GetMethod(nameof(Array.Resize)).GetGenericArguments()[0].MakeArrayType(), // T[] of Array.Resize, unique generic method definition argument - System and forced recursive serialization fails here
                typeof(DictionaryExtensions).GetMethods().Where(mi => mi.Name == nameof(DictionaryExtensions.GetValueOrDefault)).ElementAt(2).GetGenericArguments()[0] // TKey of a GetValueOrDefault overload, ambiguous generic method definition argument
            };

            var expectedTypes = referenceObjects.Cast<Type>().Where(t => t.FullName != null).Concat(new[] { typeof(OpenGenericDictionary<>), typeof(DictionaryExtensions) }).ToList();
            KGySerializeObject(referenceObjects, XmlSerializationOptions.None, expectedTypes: expectedTypes);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None, false, expectedTypes: expectedTypes);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.FullyQualifiedNames, expectedTypes: expectedTypes);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.FullyQualifiedNames, false, expectedTypes: expectedTypes);
        }

        [Test]
        public void SerializeByTypeConverter()
        {
#if !NETCOREAPP3_0_OR_GREATER
            typeof(Version).RegisterTypeConverter<VersionConverter>();
#endif
            typeof(Encoding).RegisterTypeConverter<EncodingConverter>();

            object[] referenceObjects =
            {
                new Guid("ca761232ed4211cebacd00aa0057b223"),
                new Point(13, 13),
            };

            // SystemSerializeObject(referenceObjects); - InvalidOperationException: The type System.Drawing.Point was not expected.
            SystemSerializeObjects(referenceObjects);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None, false);

            // These objects cannot be (de)serialized with system serializer
            referenceObjects = new object[]
            {
                EnvironmentHelper.IsMono ? new Uri("file:///x:/test") : new Uri(@"x:\test") ,
                EnvironmentHelper.IsMono ? new Uri("ftp://myurl.com/regular/path") : new Uri("ftp://myUrl.com/%2E%2E/%2E%2E"),
                new Version(1, 2, 3, 4),
#if !NET
		        Encoding.UTF7,
#endif
                Color.Blue
            };

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None, false);

            // Type converter as property
            referenceObjects = new object[]
            {
                new BinarySerializableClass { ObjectProp = new Point(1, 2) }, // Point has self type converter
                new ExplicitTypeConverterHolder { ExplicitTypeConverterProperty = 13 } // converter on property
            };

            // even escape can be omitted if deserialization is by XmlTextReader, which does not normalize newlines
            var expectedTypes = new[] { typeof(BinarySerializableClass), typeof(ExplicitTypeConverterHolder), typeof(Point) };
            KGySerializeObject(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback | XmlSerializationOptions.EscapeNewlineCharacters, expectedTypes: expectedTypes);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback | XmlSerializationOptions.EscapeNewlineCharacters, expectedTypes: expectedTypes);
        }

        [Test]
        public void SerializeEnums()
        {
            Enum[] referenceObjects =
            {
                TestEnum.One, // local enum
                TestEnum.Two, // local enum

                ConsoleColor.White, // mscorlib enum
                ConsoleColor.Black, // mscorlib enum

                UriKind.Absolute, // System enum
                UriKind.Relative, // System enum

                HandleInheritability.Inheritable, // System.Core enum

                ActionTargets.Default, // NUnit.Framework enum

                BinarySerializationOptions.RecursiveSerializationAsFallback, // KGySoft.CoreLibraries enum
                BinarySerializationOptions.RecursiveSerializationAsFallback | BinarySerializationOptions.IgnoreIObjectReference, // KGySoft.CoreLibraries enum, multiple flags
            };

            // SystemSerializeObject(referenceObjects); - InvalidOperationException: The type _LibrariesTest.Libraries.Serialization.XmlSerializerTest+TestEnum may not be used in this context.
            SystemSerializeObjects(referenceObjects);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.FullyQualifiedNames); // FullyQualifiedNames: DataAccessMethod.Random: 10.0.0.0 <-> 10.1.0.0 if executed from ReSharper test
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.FullyQualifiedNames, false); // FullyQualifiedNames: DataAccessMethod.Random: 10.0.0.0 <-> 10.1.0.0 if executed from ReSharper test

            // These values cannot be serialized with system serializer
            referenceObjects = new Enum[]
            {
#pragma warning disable 618
                BinarySerializationOptions.ForcedSerializationValueTypesAsFallback, // KGySoft.CoreLibraries enum, obsolete element
#pragma warning restore 618
                (BinarySerializationOptions)(-1), // KGySoft.CoreLibraries enum, non-existing value
            };

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None, false);
        }

        [Test]
        public void SerializeKeyValues()
        {
            ValueType[] referenceObjects =
            {
                new DictionaryEntry(),
                new DictionaryEntry(1, "alpha"),
                new DictionaryEntry(new object(), "alpha"),
            };

            // SystemSerializeObject(referenceObjects); - NotSupportedException: System.ValueType is an unsupported type.
            SystemSerializeObjects(referenceObjects);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.AutoGenerateDefaultValuesAsFallback);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.AutoGenerateDefaultValuesAsFallback);

            // These types cannot be serialized with system serializer
            referenceObjects = new ValueType[]
            {
                new KeyValuePair<object, string>(),
                new KeyValuePair<int, string>(1, "alpha"),
                new KeyValuePair<object, object>(1, " "),
            };

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None, false);
        }

        [Test]
        public void SerializeComplexTypes()
        {
            object[] referenceObjects =
            {
                new BinarySerializableClass { IntProp = 1, StringProp = "alpha", ObjectProp = " . " },
                new BinarySerializableStruct { IntProp = 2, StringProp = "beta" },
                new SystemSerializableClass { IntProp = 3, StringProp = "gamma" },
            };

            //SystemSerializeObject(referenceObjects); // InvalidOperationException: The type _LibrariesTest.Libraries.Serialization.XmlSerializerTest+BinarySerializableClass was not expected.
            SystemSerializeObjects(referenceObjects);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback); // BinarySerializableStruct, NonSerializableStruct
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback); // BinarySerializableStruct, NonSerializableStruct

            KGySerializeObject(referenceObjects, XmlSerializationOptions.CompactSerializationOfStructures); //  NonSerializableStruct
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.CompactSerializationOfStructures); // NonSerializableStruct

            KGySerializeObject(referenceObjects, XmlSerializationOptions.CompactSerializationOfStructures | XmlSerializationOptions.OmitCrcAttribute); // BinarySerializableStruct, NonSerializableStruct
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.CompactSerializationOfStructures | XmlSerializationOptions.OmitCrcAttribute); // BinarySerializableStruct, NonSerializableStruct

            KGySerializeObject(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback); // everything
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback); // every element

            KGySerializeObject(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback | XmlSerializationOptions.OmitCrcAttribute); // everything
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback | XmlSerializationOptions.OmitCrcAttribute); // every element

            referenceObjects = new object[]
            {
                new EmptyType(),
#if !NET35
                new StrongBox<int?>(5),
                new StrongBox<int?>(), 
#endif
                new NonSerializableStruct { IntProp = 1, Point = new(10, 20) },
            };

            //SystemSerializeObject(referenceObjects); // InvalidOperationException: The type _LibrariesTest.Libraries.Serialization.XmlSerializerTest+EmptyType was not expected.
            SystemSerializeObjects(referenceObjects);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback); // BinarySerializableStruct, NonSerializableStruct
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback); // BinarySerializableStruct, NonSerializableStruct

            KGySerializeObject(referenceObjects, XmlSerializationOptions.CompactSerializationOfStructures); //  NonSerializableStruct
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.CompactSerializationOfStructures); // NonSerializableStruct

            KGySerializeObject(referenceObjects, XmlSerializationOptions.CompactSerializationOfStructures | XmlSerializationOptions.OmitCrcAttribute); // BinarySerializableStruct, NonSerializableStruct
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.CompactSerializationOfStructures | XmlSerializationOptions.OmitCrcAttribute); // BinarySerializableStruct, NonSerializableStruct

            KGySerializeObject(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback, safeMode: false); // everything
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback, safeMode: false); // every element

            KGySerializeObject(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback | XmlSerializationOptions.OmitCrcAttribute, safeMode: false); // everything
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback | XmlSerializationOptions.OmitCrcAttribute, safeMode: false); // every element
        }

        [Test]
        public void SerializeByteArrays()
        {
            IList[] referenceObjects =
            {
                Reflector.EmptyArray<byte>(), // empty array
                new byte[] { 1, 2, 3 }, // single byte array
                new byte[][] { new byte[] { 11, 12, 13 }, new byte[] { 21, 22, 23, 24, 25 }, null }, // jagged byte array
            };

            // SystemSerializeObject(referenceObjects); - InvalidOperationException: System.Collections.IList cannot be serialized because it does not have a parameterless constructor.
            SystemSerializeObjects(referenceObjects);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.CompactSerializationOfPrimitiveArrays); // simple array, inner array of jagged array
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.CompactSerializationOfPrimitiveArrays); // simple array, inner array of jagged array

            KGySerializeObject(referenceObjects, XmlSerializationOptions.CompactSerializationOfPrimitiveArrays // simple array, inner array of jagged array
                | XmlSerializationOptions.OmitCrcAttribute); // compact parts
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.CompactSerializationOfPrimitiveArrays // simple array, inner array of jagged array
                | XmlSerializationOptions.OmitCrcAttribute); // compact parts

            KGySerializeObject(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback | XmlSerializationOptions.OmitCrcAttribute);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback | XmlSerializationOptions.OmitCrcAttribute);

            // These arrays cannot be serialized with system serializer
            referenceObjects = new IList[]
            {
                new byte[,] { { 11, 12, 13 }, { 21, 22, 23 } }, // multidimensional byte array
                new byte[][,] { new byte[,] { { 11, 12, 13 }, { 21, 22, 23 } }, new byte[,] { { 11, 12, 13, 14 }, { 21, 22, 23, 24 }, { 31, 32, 33, 34 } } }, // crazy jagged byte array 1 (2D matrix of 1D arrays)
                new byte[,][] { { new byte[] { 11, 12, 13 }, new byte[] { 21, 22, 23 } }, { new byte[] { 11, 12, 13, 14 }, new byte[] { 21, 22, 23, 24 } } }, // crazy jagged byte array 2 (1D array of 2D matrices)
                new byte[][,,] { new byte[,,] { { { 11, 12, 13 }, { 21, 21, 23 } } }, null }, // crazy jagged byte array containing null reference
                Array.CreateInstance(typeof(byte), new[] { 3 }, new[] { -1 }), // array with -1..1 index interval
                Array.CreateInstance(typeof(byte), new[] { 3, 3 }, new[] { -1, 1 }) // array with [-1..1 and 1..3] index interval
            };

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.CompactSerializationOfPrimitiveArrays);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.CompactSerializationOfPrimitiveArrays);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback | XmlSerializationOptions.OmitCrcAttribute);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback | XmlSerializationOptions.OmitCrcAttribute);
        }

        /// <summary>
        /// String has variable length and can be null.
        /// </summary>
        [Test]
        public void SerializeStringArrays()
        {
            IList[] referenceObjects =
            {
                new string[] { "One", "Two" }, // single string array
                new string[][] { new string[] { "One", "Two", "Three" }, new string[] { "One", "Two", null }, null }, // jagged string array with null values (first null as string, second null as array)
            };

            //SystemSerializeObject(referenceObjects); - InvalidOperationException: System.Collections.IList cannot be serialized because it does not have a parameterless constructor.
            SystemSerializeObjects(referenceObjects);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback);

            referenceObjects = new IList[]
            {
                new string[,] { { "One", "Two" }, { "One", "Two" } }, // multidimensional string array
                Array.CreateInstance(typeof(string), new int[] { 3 }, new int[] { -1 }) // array with -1..1 index interval
            };

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback);
        }

        [Test]
        public void SerializeSimpleArrays()
        {
#if !NETCOREAPP3_0_OR_GREATER
            typeof(Version).RegisterTypeConverter<VersionConverter>();
#endif
            IList[] referenceObjects =
            {
                Reflector.EmptyObjects,
                new object[] { new object(), null },
                new bool[] { true, false },
                new sbyte[] { 1, 2 },
                new byte[] { 1, 2 },
                new short[] { 1, 2 },
                new ushort[] { 1, 2 },
                new int[] { 1, 2 },
                new uint[] { 1, 2 },
                new long[] { 1, 2 },
                new ulong[] { 1, 2 },
                new char[] { 'a', Char.ConvertFromUtf32(0x1D161)[0] }, //U+1D161 = MUSICAL SYMBOL SIXTEENTH NOTE, serializing its low-surrogate
                new string[] { "alpha", null },
                new float[] { 1, 2 },
                new double[] { 1, 2 },
                new decimal[] { 1, 2 },
                new DateTime[] { DateTime.UtcNow, DateTime.Now },
            };

            // SystemSerializeObject(referenceObjects); - InvalidOperationException: System.Collections.IList cannot be serialized because it does not have a parameterless constructor.
            SystemSerializeObjects(referenceObjects);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.CompactSerializationOfPrimitiveArrays); // simple arrays
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.CompactSerializationOfPrimitiveArrays); // simple arrays

            KGySerializeObject(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback); // every element
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback); // every element

            // these types cannot be serialized by system serializer
            referenceObjects = new IList[]
            {
                new IntPtr[] { new IntPtr(1), IntPtr.Zero },
                new UIntPtr[] { new UIntPtr(1), UIntPtr.Zero },
                new Version[] { new Version(1, 2, 3, 4), null },
            };

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.CompactSerializationOfPrimitiveArrays); // simple arrays
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.CompactSerializationOfPrimitiveArrays); // simple arrays

            KGySerializeObjects(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback); // every element
        }

        /// <summary>
        /// Enum types must be described explicitly
        /// </summary>
        [Test]
        public void SerializeEnumArrays()
        {
            object[] referenceObjects =
            {
                new TestEnum[] { TestEnum.One, TestEnum.Two }, // single enum array
                new TestEnum[][] { new TestEnum[] { TestEnum.One }, new TestEnum[] { TestEnum.Two } }, // jagged enum array
            };

            // SystemSerializeObject(referenceObjects); - InvalidOperationException: The type _LibrariesTest.Libraries.Serialization.XmlSerializerTest+TestEnum[] may not be used in this context.
            SystemSerializeObjects(referenceObjects);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None);

            referenceObjects = new object[]
            {
                new TestEnum[,] { { TestEnum.One }, { TestEnum.Two } }, // multidimensional enum array
                new object[] { TestEnum.One, null },
                new IConvertible[] { TestEnum.One, null },
                new Enum[] { TestEnum.One, null },
                new ValueType[] { TestEnum.One, null },
            };

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None);
        }

        [Test]
        public void SerializeNullableArrays()
        {
            IList[] referenceObjects =
            {
                new bool?[] { true, false, null },
                new sbyte?[] { 1, 2, null },
                new byte?[] { 1, 2, null },
                new short?[] { 1, 2, null },
                new ushort?[] { 1, 2, null },
                new int?[] { 1, 2, null },
                new uint?[] { 1, 2, null },
                new long?[] { 1, 2, null },
                new ulong?[] { 1, 2, null },
                new char?[] { 'a', /*Char.ConvertFromUtf32(0x1D161)[0],*/ null },
                new float?[] { 1, 2, null },
                new double?[] { 1, 2, null },
                new decimal?[] { 1, 2, null },
                new DateTime?[] { DateTime.UtcNow, DateTime.Now, null },
                new Guid?[] { new Guid("ca761232ed4211cebacd00aa0057b223"), Guid.NewGuid(), null },

                new TestEnum?[] { TestEnum.One, TestEnum.Two, null },

                new DictionaryEntry?[] { new DictionaryEntry(1, "alpha"), null },

                new BinarySerializableStruct?[] { new BinarySerializableStruct { IntProp = 1, StringProp = "alpha" }, null },
                new SystemSerializableStruct?[] { new SystemSerializableStruct { IntProp = 1, StringProp = "alpha" }, null },
                new NonSerializableStruct?[] { new NonSerializableStruct { IntProp = 10, Bool = true, Point = new(10, 20) }, null },
            };

            // SystemSerializeObject(referenceObjects); - InvalidOperationException: System.Collections.IList cannot be serialized because it does not have a parameterless constructor.
            SystemSerializeObjects(referenceObjects);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback); // BinarySerializableStruct, SystemSerializableStruct, NonSerializableStruct
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback); // BinarySerializableStruct, SystemSerializableStruct, NonSerializableStruct

            KGySerializeObject(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback); // all
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback); // as content, custom structs; otherwise, all

            referenceObjects = new IList[]
            {
                new BinarySerializableStruct?[] { new BinarySerializableStruct { IntProp = 1, StringProp = "alpha" }, null },
                new SystemSerializableStruct?[] { new SystemSerializableStruct { IntProp = 1, StringProp = "alpha" }, null },
                new NonSerializableStruct?[] { new NonSerializableStruct { IntProp = 10, Point = new(13, 43) }, null },
            };

            KGySerializeObjects(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback // as content, SystemSerializableStruct; otherwise, all
                | XmlSerializationOptions.CompactSerializationOfStructures); // as content, BinarySerializableStruct, NonSerializableStruct; otherwise, all

            KGySerializeObject(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback // SystemSerializableStruct
                | XmlSerializationOptions.CompactSerializationOfStructures); // BinarySerializableStruct, NonSerializableStruct
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback // SystemSerializableStruct
                | XmlSerializationOptions.CompactSerializationOfStructures); // // BinarySerializableStruct, NonSerializableStruct

            // these types cannot be serialized by system serializer
            referenceObjects = new IList[]
            {
                new IntPtr?[] { new IntPtr(1), IntPtr.Zero, null },
                new UIntPtr?[] { new UIntPtr(1), UIntPtr.Zero, null },
                new TimeSpan?[] { new TimeSpan(1, 1, 1), new TimeSpan(DateTime.UtcNow.Ticks), null },
                new DateTimeOffset?[] { new DateTimeOffset(DateTime.Now), new DateTimeOffset(DateTime.UtcNow), new DateTimeOffset(DateTime.Now.Ticks, new TimeSpan(1, 1, 0)), null },

                new KeyValuePair<int, string>?[] { new KeyValuePair<int, string>(1, "alpha"), null },
                new KeyValuePair<int?, int?>?[] { new KeyValuePair<int?, int?>(1, 2), new KeyValuePair<int?, int?>(2, null), null },
            };

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None);

            // these types cannot be serialized recursively and without a typeconverter are not supported natively
            referenceObjects = new IList[]
            {
                new BitVector32?[] { new BitVector32(13), null },
                new BitVector32.Section?[] { BitVector32.CreateSection(13), null },
            };

            KGySerializeObject(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.CompactSerializationOfStructures); // non-null array elements
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.CompactSerializationOfStructures); // non-null array elements
        }

        [Test]
        public void IXmlSerializableTest()
        {
            object[] referenceObjects =
            {
                new XmlSerializableClass(1, 2, 3),
                new XmlSerializableStruct(1, 2, 3),
            };

            //SystemSerializeObject(referenceObjects); - InvalidOperationException: The type _LibrariesTest.Libraries.Serialization.XmlSerializerTest+XmlSerializableClass may not be used in this context. To use _LibrariesTest.Libraries.Serialization.XmlSerializerTest+XmlSerializableClass as a parameter, return type, or member of a class or struct, the parameter, return type, or member must be declared as type _LibrariesTest.Libraries.Serialization.XmlSerializerTest+XmlSerializableClass (it cannot be object). Objects of type _LibrariesTest.Libraries.Serialization.XmlSerializerTest+XmlSerializableClass may not be used in un-typed collections, such as ArrayLists.
            SystemSerializeObjects(referenceObjects);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None);

            referenceObjects = new[]
            {
                new ReadOnlyProperties().Init(xmlSerializableClass:new XmlSerializableClass(3, 2, 1))
            };

            KGySerializeObject(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback);
        }

        [Test]
        public void IXmlSerializableCollectionsTest()
        {
            IList<XmlSerializableClass>[] referenceObjects =
            {
                new XmlSerializableClass[] { new XmlSerializableClass(1, 2, 3) },
                new List<XmlSerializableClass> { new XmlSerializableClass(1, 2, 3) }
            };

            //SystemSerializeObject(referenceObjects); - NotSupportedException: Cannot serialize interface System.Collections.Generic.IList`1[[_LibrariesTest.Libraries.Serialization.XmlSerializerTest+XmlSerializableClass, _LibrariesTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=b45eba277439ddfe]].
            SystemSerializeObjects(referenceObjects);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None);
        }

        /// <summary>
        /// Arrays of complex types
        /// </summary>
        [Test]
        public void SerializeComplexArrays()
        {
            IList[] referenceObjects =
            {
                new BinarySerializableStruct[] { new BinarySerializableStruct { IntProp = 1, StringProp = "alpha" }, new BinarySerializableStruct { IntProp = 2, StringProp = "beta" } }, // array of a BinarySerializable struct
                new BinarySerializableClass[] { new BinarySerializableClass { IntProp = 1, StringProp = "alpha" }, new BinarySerializableClass { IntProp = 2, StringProp = "beta", ObjectProp = DateTime.Now } }, // array of a BinarySerializable non sealed class
                new BinarySerializableSealedClass[] { new BinarySerializableSealedClass { IntProp = 1, StringProp = "alpha" }, new BinarySerializableSealedClass { IntProp = 2, StringProp = "beta" }, new BinarySerializableSealedClass { IntProp = 3, StringProp = "gamma" } }, // array of a BinarySerializable sealed class
                new SystemSerializableClass[] { new SystemSerializableClass { IntProp = 1, StringProp = "alpha" }, new SystemSerializableClass { IntProp = 2, StringProp = "beta" } }, // array of a [Serializable] object - will be serialized by BinaryFormatter
                new NonSerializableStruct[] { new NonSerializableStruct { IntProp = 1, Point = new(1, 2) }, new NonSerializableStruct { IntProp = 2, Bool = true, Point = new(3, 4) } }, // array of any struct
            };

            //SystemSerializeObject(referenceObjects); - InvalidOperationException: System.Collections.IList cannot be serialized because it does not have a parameterless constructor.
            SystemSerializeObjects(referenceObjects);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.CompactSerializationOfStructures); // NonSerializableStruct
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.CompactSerializationOfStructures); // NonSerializableStruct

            KGySerializeObject(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback // everything
                | XmlSerializationOptions.CompactSerializationOfStructures); // nothing
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback // as content, non-structs; otherwise everything
                | XmlSerializationOptions.CompactSerializationOfStructures); // as content, NonSerializableStruct; otherwise, nothing

            // These collections cannot be serialized with system serializer
            referenceObjects = new IList[]
            {
                new BinarySerializableClass[] { new BinarySerializableSealedClass { IntProp = 1, StringProp = "alpha" }, new BinarySerializableSealedClass { IntProp = 2, StringProp = "beta" } }, // array of a BinarySerializable non sealed class with derived elements
                new IBinarySerializable[] { new BinarySerializableStruct { IntProp = 1, StringProp = "alpha" }, new BinarySerializableClass { IntProp = 2, StringProp = "beta" }, new BinarySerializableSealedClass { IntProp = 3, StringProp = "gamma" } }, // IBinarySerializable array
                new AbstractClass[] { new SystemSerializableClass { IntProp = 1, StringProp = "alpha" }, new SystemSerializableSealedClass { IntProp = 2, StringProp = "beta" } }, // array of a [Serializable] object
                new AbstractClass[] { new BinarySerializableClass { IntProp = 1, StringProp = "alpha" }, new SystemSerializableSealedClass { IntProp = 2, StringProp = "beta" } }, // array of a [Serializable] object, with an IBinarySerializable element
                new IBinarySerializable[][] { new IBinarySerializable[] { new BinarySerializableStruct { IntProp = 1, StringProp = "alpha" } }, null }, // IBinarySerializable array
                new NonSerializableStruct[] { new NonSerializableStruct { IntProp = 1, Point = new(1, 2) }, new NonSerializableStruct { IntProp = 2, Bool = true, Point = new(3, 4) } }, // array of any struct

                new ValueType[] { new BinarySerializableStruct { IntProp = 1, StringProp = "alpha" }, new SystemSerializableStruct { IntProp = 2, StringProp = "beta" }, null, 1 },
                new IConvertible[] { null, 1 },
                new IConvertible[][] { null, new IConvertible[] { null, 1 }, },
            };

            var expectedTypes = GetExpectedTypes(referenceObjects).Concat(new[] { typeof(BinarySerializableSealedClass), typeof(BinarySerializableStruct), typeof(SystemSerializableClass), typeof(SystemSerializableSealedClass), typeof(SystemSerializableStruct) }).ToList();
            KGySerializeObject(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback, expectedTypes: expectedTypes); // BinarySerializableStruct, NonSerializableStruct, SystemSerializableStruct
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback, expectedTypes: expectedTypes); // BinarySerializableStruct, NonSerializableStruct, SystemSerializableStruct

            KGySerializeObject(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback // SystemSerializableStruct
                | XmlSerializationOptions.CompactSerializationOfStructures, expectedTypes: expectedTypes); // BinarySerializableStruct, NonSerializableStruct
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback // SystemSerializableStruct
                | XmlSerializationOptions.CompactSerializationOfStructures, expectedTypes: expectedTypes); // BinarySerializableStruct, NonSerializableStruct

            KGySerializeObject(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback // everything
                | XmlSerializationOptions.CompactSerializationOfStructures, expectedTypes: expectedTypes); // nothing
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback // as content, non-structs; otherwise everything
                | XmlSerializationOptions.CompactSerializationOfStructures, expectedTypes: expectedTypes); // as content, structs; otherwise, nothing
        }

        /// <summary>
        /// Simple generic collections
        /// </summary>
        [Test]
        public void SerializeTrustedGenericCollections()
        {
            // natively supported collections, also by system serializer
            object[] referenceObjects =
            {
                // unconditionally trusted
                new List<int> { 1, 2, 3 },
                new List<int?> { 1, 2, null },
                new List<int[]> { new int[] { 1, 2, 3 }, null },

                new CircularList<int?> { 1, 2, 3 },

                // trusted with a known comparer
                new HashSet<string> { "alpha", "beta", "gamma" },
            };

            // SystemSerializeObject(referenceObjects); // InvalidOperationException: The type may not be used in this context.
            SystemSerializeObjects(referenceObjects);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None, expectedTypes: Reflector.EmptyArray<Type>());
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None, expectedTypes: Reflector.EmptyArray<Type>());

            // these collections are not supported by system serializer
            referenceObjects = new object[]
            {
                // unconditionally trusted
                new LinkedList<int>(new[] { 1, 2, 3 }),
                new LinkedList<int[]>(new int[][] { new int[] { 1, 2, 3 }, null }),

                // trusted with a known comparer
                new ThreadSafeHashSet<int> { 1, 2, 3 },

                new Dictionary<int, string> { { 1, "alpha" }, { 2, "beta" }, { 3, "gamma" } },
                new Dictionary<int[], string[]> { { new int[] { 1 }, new string[] { "alpha" } }, { new int[] { 2 }, null } },
                new Dictionary<object, object> { { 1, "alpha" }, { "beta", DateTime.Now }, { new object(), new object() }, { 4, new object[] { 1, "alpha", DateTime.Now, null } }, { 5, null } },

                new SortedList<int, string> { { 1, "alpha" }, { 2, "beta" }, { 3, "gamma" } },
                new SortedList<int, string[]> { { 1, new string[] { "alpha" } }, { 2, null } },

                new SortedDictionary<int, string> { { 1, "alpha" }, { 2, "beta" }, { 3, "gamma" } },
                new SortedDictionary<int, string[]> { { 1, new string[] { "alpha" } }, { 2, null } },

                new CircularSortedList<int, string> { { 1, "alpha" }, { 2, "beta" }, { 3, "gamma" } },
                new CircularSortedList<int, string[]> { { 1, new string[] { "alpha" } }, { 2, null } },

                new ThreadSafeDictionary<int, string> { { 1, "alpha" }, { 2, "beta" }, { 3, "gamma" } },

                new StringKeyedDictionary<int> { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },

                new AllowNullDictionary<int?, string> { { 1, "alpha" }, { 2, "beta" }, { 3, "gamma" }, { null, "null" } },

#if !NET35
                new SortedSet<int> { 1, 2, 3 },
                new ConcurrentDictionary<int, string>(new Dictionary<int, string> { { 1, "alpha" }, { 2, "beta" }, { 3, "gamma" } }),
#endif
#if NET9_0_OR_GREATER
                new OrderedDictionary<int, int?> { { 0, null }, { 1, 1 } },
#endif
            };

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None, expectedTypes: Reflector.EmptyArray<Type>());
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None, expectedTypes: Reflector.EmptyArray<Type>());

            // these cannot be serialized as content because they implement neither ICollection<T> nor IList so they can be deserialized by initializer constructor only
            referenceObjects = new object[]
            {
                // non-populatable
                new Queue<int>(new[] { 1, 2, 3 }),
                new Queue<int[]>(new int[][] { new int[] { 1, 2, 3 }, null }),
                new Queue<int>[] { new Queue<int>(new int[] { 1, 2, 3 }) },
                new Queue<int>[][] { new Queue<int>[] { new Queue<int>(new int[] { 1, 2, 3 }) } },
#if !NET35
                new ConcurrentQueue<int>(new[] { 1, 2, 3 }),
                new ConcurrentBag<int> { 1, 2, 3 },  
#endif

                // non-populatable, reverse
                new Stack<int>(new[] { 1, 2, 3 }),
                new Stack<int[]>(new int[][] { new int[] { 1, 2, 3 }, null }),
#if !NET35
                new ConcurrentStack<int>(new[] { 1, 2, 3 }),
#endif
            };

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None, expectedTypes: Reflector.EmptyArray<Type>());
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None, false, expectedTypes: Reflector.EmptyArray<Type>());
        }

        /// <summary>
        /// Simple non-generic collections
        /// </summary>
        [Test]
        public void SerializeTrustedNonGenericCollections()
        {
            // natively supported collections, also by system serializer
            object[] referenceObjects =
            {
                new ArrayList { 1, "alpha", DateTime.Now },
                new StringCollection { "alpha", "beta", "gamma" },
            };

            //SystemSerializeObject(referenceObjects); // InvalidOperationException: The type System.Collections.ArrayList may not be used in this context.
            SystemSerializeObjects(referenceObjects);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None, expectedTypes: Reflector.EmptyArray<Type>());
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None, expectedTypes: Reflector.EmptyArray<Type>());

            // these collections are not supported by system serializer
            referenceObjects = new object[]
            {
                new Hashtable { { 1, "alpha" }, { (byte)2, "beta" }, { 3m, "gamma" } },
                new SortedList { { 1, "alpha" }, { 2, "beta" }, { 3, "gamma" } },
                new ListDictionary { { 1, "alpha" }, { 2, "beta" }, { 3, "gamma" } },
                new HybridDictionary(false) { { "alpha", 1 }, { "Alpha", 2 }, { "ALPHA", 3 } },
                new OrderedDictionary { { "alpha", 1 }, { "Alpha", 2 }, { "ALPHA", 3 } },
            };

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None, expectedTypes: Reflector.EmptyArray<Type>());
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None, expectedTypes: Reflector.EmptyArray<Type>());

            // these cannot be serialized as content because they implement neither ICollection<T> nor IList so they can be deserialized by initializer constructor only
            referenceObjects = new object[]
            {
                new Queue(new object[] { 1, (byte)2, 3m, new string[] { "alpha", "beta", "gamma" } }),
                new Stack(new object[] { 1, (byte)2, 3m, new string[] { "alpha", "beta", "gamma" } }),
                new BitArray(new[] { true, false, true })
            };

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None, expectedTypes: Reflector.EmptyArray<Type>());
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None, false, expectedTypes: Reflector.EmptyArray<Type>());

            // StringDictionary is not supported at all because it implements only IEnumerable and has no collection initializer constructor.
            // Binary fallback can be used though.
            referenceObjects = new object[]
            {
                new StringDictionary { { "a", "alpha" }, { "b", "beta" }, { "c", "gamma" }, { "x", null } },
            };

            Throws<SerializationException>(() => KGySerializeObjects(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback), "Serialization of collection \"System.Collections.Specialized.StringDictionary\" is not supported with following options: \"RecursiveSerializationAsFallback\", because it does not implement IList, IDictionary or ICollection<T> interfaces and has no initializer constructor that can accept an array or list.");
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback, false, expectedTypes: Reflector.EmptyArray<Type>());
            KGySerializeObject(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback, expectedTypes: Reflector.EmptyArray<Type>());
        }

        /// <summary>
        /// Simple generic collections
        /// </summary>
        [Test]
        public void SerializeTrustedCollectionsWithComparer()
        {
            object[] referenceObjects =
            {
                new HashSet<byte>(EqualityComparer<byte>.Default) { 1, 2, 3 },
                new HashSet<ConsoleColor>(EnumComparer<ConsoleColor>.Comparer) { ConsoleColor.Red, ConsoleColor.Green, ConsoleColor.Blue },
                new HashSet<string>(StringComparer.Ordinal) { "alpha", "beta", "gamma" },
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "alpha", "beta", "gamma" },
                new HashSet<string>(StringComparer.InvariantCulture) { "alpha", "beta", "gamma" },
                new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "alpha", "beta", "gamma" },
                new HashSet<string>(StringSegmentComparer.Ordinal) { "alpha", "beta", "gamma" },
                new HashSet<string>(StringSegmentComparer.OrdinalIgnoreCase) { "alpha", "beta", "gamma" },
                new HashSet<string>(StringSegmentComparer.InvariantCulture) { "alpha", "beta", "gamma" },
                new HashSet<string>(StringSegmentComparer.InvariantCultureIgnoreCase) { "alpha", "beta", "gamma" },
                new HashSet<string>(StringSegmentComparer.OrdinalRandomized) { "alpha", "beta", "gamma" },
                new HashSet<string>(StringSegmentComparer.OrdinalIgnoreCaseRandomized) { "alpha", "beta", "gamma" },
                new HashSet<string>(StringSegmentComparer.OrdinalNonRandomized) { "alpha", "beta", "gamma" },
                new HashSet<string>(StringSegmentComparer.OrdinalIgnoreCaseNonRandomized) { "alpha", "beta", "gamma" },

                new ThreadSafeHashSet<byte>(EqualityComparer<byte>.Default) { 1, 2, 3 },
                new ThreadSafeHashSet<ConsoleColor>(ComparerHelper<ConsoleColor>.EqualityComparer) { ConsoleColor.Red, ConsoleColor.Green, ConsoleColor.Blue },

                new Dictionary<string, int>(EqualityComparer<string>.Default) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },
                new Dictionary<string, int>(StringComparer.Ordinal) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },
                new Dictionary<ConsoleColor, int>(EnumComparer<ConsoleColor>.Comparer) { { ConsoleColor.Red, 1 }, { ConsoleColor.Green, 2 }, { ConsoleColor.Blue, 3 } },

                new SortedList<string, int>(Comparer<string>.Default) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },
                new SortedList<string, int>(StringComparer.Ordinal) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },
                new SortedList<ConsoleColor, int>(EnumComparer<ConsoleColor>.Comparer) { { ConsoleColor.Red, 1 }, { ConsoleColor.Green, 2 }, { ConsoleColor.Blue, 3 } },

                new SortedDictionary<string, int>(Comparer<string>.Default) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },
                new SortedDictionary<string, int>(StringComparer.Ordinal) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },
                new SortedDictionary<ConsoleColor, int>(EnumComparer<ConsoleColor>.Comparer) { { ConsoleColor.Red, 1 }, { ConsoleColor.Green, 2 }, { ConsoleColor.Blue, 3 } },

                new CircularSortedList<string, int>(Comparer<string>.Default) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },
                new CircularSortedList<string, int>(StringComparer.Ordinal) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },
                new CircularSortedList<ConsoleColor, int>(EnumComparer<ConsoleColor>.Comparer) { { ConsoleColor.Red, 1 }, { ConsoleColor.Green, 2 }, { ConsoleColor.Blue, 3 } },

                new ThreadSafeDictionary<string, int>(EqualityComparer<string>.Default) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },
                new ThreadSafeDictionary<string, int>(StringComparer.Ordinal) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },
                new ThreadSafeDictionary<ConsoleColor, int>(ComparerHelper<ConsoleColor>.EqualityComparer) { { ConsoleColor.Red, 1 }, { ConsoleColor.Green, 2 }, { ConsoleColor.Blue, 3 } },

                new StringKeyedDictionary<int>(StringSegmentComparer.Ordinal) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },
                new StringKeyedDictionary<int>(StringSegmentComparer.OrdinalIgnoreCase) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },

#if !NET35
                new SortedSet<byte>(Comparer<byte>.Default) { 1, 2, 3 },
                new SortedSet<ConsoleColor>(EnumComparer<ConsoleColor>.Comparer) { ConsoleColor.Red, ConsoleColor.Green, ConsoleColor.Blue },

                new ConcurrentDictionary<string, int>(EqualityComparer<string>.Default) { ["alpha"] = 1, ["beta"] = 2, ["gamma"] = 3 },
                new ConcurrentDictionary<string, int>(StringComparer.Ordinal) { ["alpha"] = 1, ["beta"] = 2, ["gamma"] = 3 },
                new ConcurrentDictionary<ConsoleColor, int>(EnumComparer<ConsoleColor>.Comparer) { [ConsoleColor.Red] = 1, [ConsoleColor.Green] = 2, [ConsoleColor.Blue] = 3 },
#endif

                new Hashtable(StringComparer.Ordinal) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },
                new Hashtable(StringSegmentComparer.Ordinal) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },

                new SortedList(Comparer.Default) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },
                new SortedList(Comparer.DefaultInvariant) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },
                new SortedList(CaseInsensitiveComparer.Default) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },
                new SortedList(CaseInsensitiveComparer.DefaultInvariant) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },
                new SortedList(StringComparer.Ordinal) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },
                new SortedList(StringSegmentComparer.Ordinal) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },

                new ListDictionary(Comparer.Default) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },
                new ListDictionary(StringComparer.Ordinal) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },
                new ListDictionary(StringSegmentComparer.Ordinal) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },

                new HybridDictionary(false) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },
                new HybridDictionary(true) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },

                new OrderedDictionary(StringComparer.Ordinal) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },
                new OrderedDictionary(StringSegmentComparer.Ordinal) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } },
                new OrderedDictionary(StringComparer.Ordinal) { { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 } }.AsReadOnly(),
            };

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None, expectedTypes: new[] { typeof(ConsoleColor) });
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None, alsoAsContent: false, expectedTypes: new[] { typeof(ConsoleColor) });

            // unsupported (culture-aware) comparer
            referenceObjects = new object[]
            {
                new HashSet<string>(StringComparer.CurrentCulture) { "alpha", "beta", "gamma" },
            };

            // Default: serialization denied
            Throws<SerializationException>(() => KGySerializeObjects(referenceObjects, XmlSerializationOptions.None, alsoAsContent: false), "unsupported comparer");

            // Forced recursive: the deserialized instance uses a default comparer
            Throws<AssertionException>(() => KGySerializeObjects(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback, alsoAsContent: false),
#if NET9_0_OR_GREATER
                "Types are different. System.CultureAwareComparer <-> System.Collections.Generic.StringEqualityComparer"
#else
                "Types are different. System.CultureAwareComparer <-> System.Collections.Generic.GenericEqualityComparer`1[System.String]"
#endif
                );

            // Forced binary: works correctly
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback, alsoAsContent: false);
        }

        [Test]
        public void SerializeObjectsWithReadonlyProperties()
        {
            object[] referenceObjects =
            {
                new ReadOnlyProperties().Init(
                    xmlSerializableClass:new XmlSerializableClass(1, 2, 3),
                    array:new object[] { 1, "string", DateTime.Now },
                    toCache:new[] { 1, 2, 3 },
                    readOnlyCollection:new ReadOnlyCollection<object>(new object[] { 'x', 1, "abc" })
                ),
                new PopulatableCollectionWithReadOnlyProperties { "one", "two" }.Init(
                    xmlSerializableClass:new XmlSerializableClass(1, 2, 3),
                    array:new object[] { 1, "string", DateTime.Now },
                    toCache:new[] { 1, 2, 3 },
                    readOnlyCollection:new ReadOnlyCollection<object>(new object[] { 'x', 1, "abc" })
                ),
                new ReadOnlyCollectionWithInitCtorAndReadOnlyProperties(new[] { "one", "two" }).Init(
                    xmlSerializableClass:new XmlSerializableClass(1, 2, 3),
                    array:new object[] { 1, "string", DateTime.Now },
                    toCache:new[] { 1, 2, 3 },
                    readOnlyCollection:new ReadOnlyCollection<object>(new object[] { 'x', 1, "abc" })),
            };

            //SystemSerializeObject(referenceObjects); // InvalidOperationException: The type _LibrariesTest.Libraries.Serialization.XmlSerializerTest+ReadOnlyProperties was not expected. Use the XmlInclude or SoapInclude attribute to specify types that are not known statically.
            //SystemSerializeObjects(referenceObjects); // InvalidOperationException: There was an error reflecting type '_LibrariesTest.Libraries.Serialization.XmlSerializerTest.ReadOnlyProperties'. ---> System.NotSupportedException: Cannot serialize member _LibrariesTest.Libraries.Serialization.XmlSerializerTest+ReadOnlyProperties.Cache of type KGySoft.CoreLibraries.Collections.Cache`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], because it implements IDictionary.

            KGySerializeObject(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback, false); // false for ReadOnlyCollectionWithReadOnlyProperties

            referenceObjects = new[]
            {
                new ReadOnlyCollectionWithoutInitCtorAndReadOnlyProperties().Init(
                    xmlSerializableClass:new XmlSerializableClass(1, 2, 3),
                    array:new object[] { 1, "string", DateTime.Now },
                    toCache:new[] { 1, 2, 3 },
                    readOnlyCollection:new ReadOnlyCollection<object>(new object[] { 'x', 1, "abc" }))
            };

            Throws<SerializationException>(() => KGySerializeObjects(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback),
                "Serialization of collection \"KGySoft.CoreLibraries.UnitTests.Serialization.Xml.XmlSerializerTest+ReadOnlyCollectionWithoutInitCtorAndReadOnlyProperties\" is not supported with following options: \"RecursiveSerializationAsFallback\", because it does not implement IList, IDictionary or ICollection<T> interfaces and has no initializer constructor that can accept an array or list.");
            KGySerializeObject(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback, safeMode: false);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback, false, false);
        }

        [Test]
        public void SerializeObjectsWithMemberNameCollision()
        {
            ConflictNameBase[] referenceObjects =
            {
                new ConflictNameBase { item = 13 },
                new ConflictNameChild { ConflictingField = "ChildField", ConflictingProperty = "ChildProp", item = "itemChild" }.SetBase(-13, "BaseField", "BaseProp"),
                new ConflictingCollection<string> { "item", "item2" }.SetChild("ChildItem", "ChildField", "ChildProp").SetBase(-5, "BaseFieldFromCollection", "CollectionBaseProp")
            };

            //SystemSerializeObject(referenceObjects); // InvalidOperationException: _LibrariesTest.Libraries.Serialization.XmlSerializerTest+ConflictNameBase is inaccessible due to its protection level. Only public types can be processed.
            //SystemSerializeObjects(referenceObjects); // InvalidOperationException: _LibrariesTest.Libraries.Serialization.XmlSerializerTest+ConflictNameBase is inaccessible due to its protection level. Only public types can be processed.

            KGySerializeObject(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback); // ConflictingCollection
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback); // ConflictingCollection

            referenceObjects = new[]
            {
                new ConflictNameBase { ConflictingProperty = "PropValue" },
                new ConflictNameChild { ConflictingProperty = "ChildProp" }.SetBase(null, null, "BaseProp"),
                new ConflictingCollection<string> { "item", "item2" }.SetChild(null, null, "ChildProp").SetBase(null, null, "CollectionBaseProp")
            };

            KGySerializeObject(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback | XmlSerializationOptions.ExcludeFields); // ConflictingCollection
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback | XmlSerializationOptions.ExcludeFields); // ConflictingCollection
        }

        [Test]
        public void SerializeBinaryTypeConverterProperties()
        {
            object[] referenceObjects =
            {
                new BinaryMembers("One", "Two") { BinProp = ConsoleColor.Blue }
            };

            var expectedTypes = new[] { typeof(BinaryMembers) };
            KGySerializeObject(referenceObjects, XmlSerializationOptions.ForcedSerializationOfReadOnlyMembersAndCollections, expectedTypes: expectedTypes); // Queue as readonly property
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.ForcedSerializationOfReadOnlyMembersAndCollections, expectedTypes: expectedTypes); // Queue as readonly property
        }

        [Test]
        public void SerializeFields()
        {
            object[] referenceObjects =
            {
                (13, "alpha")
            };

#if !(NET35 || NET40 || NET45) // InvalidOperationException: System.ValueTuple`2 is inaccessible due to its protection level. Only public types can be processed.  
            //SystemSerializeObject(referenceObjects); // InvalidOperationException: The type System.ValueTuple`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] was not expected. Use the XmlInclude or SoapInclude attribute to specify types that are not known statically.
            SystemSerializeObjects(referenceObjects);
#endif

            KGySerializeObject(referenceObjects, XmlSerializationOptions.None);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.None);

            Throws<AssertionException>(() => KGySerializeObjects(referenceObjects, XmlSerializationOptions.ExcludeFields), "Equality check failed");
        }

        /// <summary>
        /// Complex generic collections
        /// </summary>
        [Test]
        public void SerializeComplexGenericCollections()
        {
#if !NETCOREAPP3_0_OR_GREATER
            typeof(Version).RegisterTypeConverter<VersionConverter>();
#endif
            ICollection[] referenceObjects =
            {
                new List<byte>[] { new List<byte> { 11, 12, 13 }, new List<byte> { 21, 22 } }, // array of lists
                new List<byte[]> { new byte[] { 11, 12, 13 }, new byte[] { 21, 22 } }, // list of arrays

                new Collection<KeyValuePair<int, object>> { new KeyValuePair<int, object>(1, "alpha"), new KeyValuePair<int, object>(2, DateTime.Now), new KeyValuePair<int, object>(3, new object()), new KeyValuePair<int, object>(4, new object[] { 1, "alpha", DateTime.Now, null }), new KeyValuePair<int, object>(5, null) },

                // dictionary with dictionary<int, string> value
                new Dictionary<string, Dictionary<int, string>> { { "hu", new Dictionary<int, string> { { 1, "alpha" }, { 2, "beta" }, { 3, "gamma" } } }, { "en", new Dictionary<int, string> { { 1, "apple" }, { 2, "frog" }, { 3, "cat" } } } },

                // dictionary with array key
                new Dictionary<string[], Dictionary<int, string>> { { new string[] { "hu" }, new Dictionary<int, string> { { 1, "alpha" }, { 2, "beta" }, { 3, "gamma" } } }, { new string[] { "en" }, new Dictionary<int, string> { { 1, "apple" }, { 2, "frog" }, { 3, "cat" } } } },

                // dictionary with dictionary key and value
                new Dictionary<Dictionary<int[], string>, Dictionary<int, string>> { { new Dictionary<int[], string> { { new int[] { 1 }, "key.value1" } }, new Dictionary<int, string> { { 1, "alpha" }, { 2, "beta" }, { 3, "gamma" } } }, { new Dictionary<int[], string> { { new int[] { 2 }, "key.value2" } }, new Dictionary<int, string> { { 1, "apple" }, { 2, "frog" }, { 3, "cat" } } } },

                // object list with various elements
                new List<object> { 1, "alpha", new Version(13, 0), new object[] { 3, "gamma", null }, new object(), null },

                // dictionary with object key and value
                new Dictionary<object, object> { { 1, "alpha" }, { new object(), "beta" }, { new int[] { 3, 4 }, null }, { TestEnum.One, "gamma" } },

                // non-sealed collections with base and derived elements
                new List<BinarySerializableClass> { new BinarySerializableSealedClass { IntProp = 1, StringProp = "alpha" }, new BinarySerializableSealedClass { IntProp = 2, StringProp = "beta" } },
                new Dictionary<object, BinarySerializableClass> { { new object(), new BinarySerializableSealedClass { IntProp = 1, StringProp = "alpha" } }, { 2, new BinarySerializableSealedClass { IntProp = 2, StringProp = "beta" } } },

                new IList<int>[] { new int[] { 1, 2, 3 }, new List<int> { 1, 2, 3 } },
                new List<IList<int>> { new int[] { 1, 2, 3 }, new List<int> { 1, 2, 3 } }
            };

            //SystemSerializeObject(referenceObjects); - InvalidOperationException: You must implement a default accessor on System.Collections.ICollection because it inherits from ICollection.
            //SystemSerializeObjects(referenceObjects); - NullReferenceException

            var expectedTypes = GetExpectedTypes(referenceObjects).Concat(new[] { typeof(Version), typeof(TestEnum), typeof(BinarySerializableSealedClass) }).ToList();
            KGySerializeObject(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback, expectedTypes: expectedTypes);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback, expectedTypes: expectedTypes);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback, expectedTypes: expectedTypes); // everything
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback, expectedTypes: expectedTypes); // as content, nested collections and non-simple types; otherwise every element
        }

        /// <summary>
        /// Custom collections
        /// </summary>
        [Test]
        public void SerializeCustomCollections()
        {
            object[] referenceObjects =
            {
                new Collection<int> { 1, 2, 3 },
                new Collection<int[]> { new int[] { 1, 2, 3 }, null },
                new Cache<int, string> { { 1, "alpha" }, { 2, "beta" }, { 3, "gamma" } },
                new CustomGenericCollection<KeyValuePair<int, object>> { new KeyValuePair<int, object>(1, "alpha"), new KeyValuePair<int, object>(2, DateTime.Now), new KeyValuePair<int, object>(3, new object()), new KeyValuePair<int, object>(4, new object[] { 1, "alpha", DateTime.Now, null }), new KeyValuePair<int, object>(5, null) },
                new CustomNonGenericCollection { new KeyValuePair<int, object>(1, "alpha"), new KeyValuePair<int, object>(2, DateTime.Now), new KeyValuePair<int, object>(3, new object()), new KeyValuePair<int, object>(4, new object[] { 1, "alpha", DateTime.Now, null }), new KeyValuePair<int, object>(5, null) },
                new CustomGenericDictionary<string, Dictionary<int, string>> { { "hu", new Dictionary<int, string> { { 1, "alpha" }, { 2, "beta" }, { 3, "gamma" } } }, { "en", new Dictionary<int, string> { { 1, "apple" }, { 2, "frog" }, { 3, "cat" } } } },
                new CustomNonGenericDictionary { { 1, "alpha" }, { 2u, 'b' } },

                // read-only
                new ReadOnlyCollection<int>(new[] { 1, 2, 3 }),

#if !(NET35 || NET40)
                new ReadOnlyDictionary<int, string>(new Dictionary<int, string> { { 1, "One" }, { 2, "Two" } }),
#endif
            };

            // SystemSerializeObject(referenceObjects); // InvalidOperationException: You must implement a default accessor on System.Collections.ICollection because it inherits from ICollection.
            // SystemSerializeObjects(referenceObjects); // InvalidOperationException: _LibrariesTest.Libraries.Serialization.XmlSerializerTest+CustomGenericCollection`1[[System.Collections.Generic.KeyValuePair`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] is inaccessible due to its protection level. Only public types can be processed.

            KGySerializeObject(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback); // all
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback, false); // all

#if !NET35
            // these collections are not supported recursively at all
            referenceObjects = new IEnumerable[]
            {
#if !NET40
                new ArraySegment<int>(new[] { 1, 2, 3 }, 1, 1), // initializer collection has 3 elements, while the segment has only 1
#endif
                new BlockingCollection<int> { 1, 2, 3 }, // no initializer constructor of array or list
            };

            KGySerializeObject(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback, safeMode: false);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback, false, false);
#endif // !NET35
        }

        [Test]
        public void FullExtraComponentSerializationTest()
        {
            FullExtraComponent[] referenceObjects =
            {
                new FullExtraComponent(true),
                new FullExtraComponent(false),
            };

            //SystemSerializeObject(referenceObjects); // InvalidOperationException: You must implement a default accessor on System.Collections.Generic.LinkedList`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] because it inherits from ICollection.
            //SystemSerializeObjects(referenceObjects); // InvalidOperationException: You must implement a default accessor on System.Collections.Generic.LinkedList`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] because it inherits from ICollection.

            var expectedTypes = new[] { typeof(FullExtraComponent), typeof(Point), typeof(FullExtraComponent.TestInner), typeof(FullExtraComponent.InnerStructure) };
            XElement xml = KGySerializeObject(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback, expectedTypes: expectedTypes);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback, expectedTypes: expectedTypes);

            // validating [DefaultValue(0)] on IntProp and explicit null on IntArray
            XElement[] xItems = xml.Elements().ToArray();
            Assert.AreEqual(referenceObjects[0].IntProp.ToString(CultureInfo.InvariantCulture), xItems[0].Element(nameof(FullExtraComponent.IntProp))!.Value);
            Assert.IsFalse(xItems[0].Element(nameof(FullExtraComponent.IntArray))!.IsEmpty);
            Assert.IsNull(xItems[1].Element(nameof(FullExtraComponent.IntProp)));
            Assert.IsTrue(xItems[1].Element(nameof(FullExtraComponent.IntArray))!.IsEmpty);

            xml = KGySerializeObject(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback // every non-trusted type
                | XmlSerializationOptions.AutoGenerateDefaultValuesAsFallback // properties without DefaultAttribute
                | XmlSerializationOptions.CompactSerializationOfPrimitiveArrays, expectedTypes: expectedTypes); // IntArray
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback // every non-trusted type
                | XmlSerializationOptions.AutoGenerateDefaultValuesAsFallback // properties without DefaultAttribute
                | XmlSerializationOptions.CompactSerializationOfPrimitiveArrays, expectedTypes: expectedTypes); // IntArray

            // validating auto generated default value on IntArray
            xItems = xml.Elements().ToArray();
            Assert.IsFalse(xItems[0].Element(nameof(FullExtraComponent.IntArray))!.IsEmpty);
            Assert.IsNull(xItems[1].Element(nameof(FullExtraComponent.IntArray)));

            xml = KGySerializeObject(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback | XmlSerializationOptions.IgnoreDefaultValueAttribute, expectedTypes: expectedTypes);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback | XmlSerializationOptions.IgnoreDefaultValueAttribute, expectedTypes: expectedTypes);

            // validating ignored default value on IntProp
            xItems = xml.Elements().ToArray();
            Assert.AreEqual(referenceObjects[0].IntProp.ToString(CultureInfo.InvariantCulture), xItems[0].Element(nameof(FullExtraComponent.IntProp))!.Value);
            Assert.AreEqual("0", xItems[1].Element(nameof(FullExtraComponent.IntProp))!.Value);

            KGySerializeObject(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback, safeMode: false);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.BinarySerializationAsFallback, safeMode: false);
        }

        [Test]
        public unsafe void SerializePointers()
        {
            if (EnvironmentHelper.IsMono)
                Assert.Inconclusive("Mono does not support pointer serialization.");

            object[] referenceObjects =
            {
                // Pointer fields
                new UnsafeStruct(),
                new UnsafeStruct
                {
                    VoidPointer = (void*)new IntPtr(1),
                    IntPointer = (int*)new IntPtr(1),
                    PointerArray = null, // new int*[] { (int*)new IntPtr(1), null }, - not supported
                    PointerOfPointer = (void**)new IntPtr(1)
                },
            };

            //SystemSerializeObjects(referenceObjects); // InvalidOperationException: System.Void* cannot be serialized because it does not have a parameterless constructor.

            KGySerializeObject(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback);

            referenceObjects = new object[]
            {
                // Pointer Array
                new int*[] { (int*)IntPtr.Zero },
            };

            Throws<NotSupportedException>(() => KGySerializeObject(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback), "Array of pointer type 'System.Int32*[]' is not supported.");
            Throws<NotSupportedException>(() => KGySerializeObjects(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback), "Array of pointer type 'System.Int32*[]' is not supported.");
        }

        [Test]
        public void SerializeForwardedTypes()
        {
            object[] referenceObjects =
            {
#if !NET35
                new ObservableCollection<int> { 1, 2, 3 }, // WindowsBase -> System/System.ObjectModel  
#endif
                new BitArray(new[] { true }), // mscorlib -> System.Collections
                new HashSet<int> { 1, 2, 3 }, // System.Core -> System.Collections
                new LinkedList<int>(new[] { 1, 2, 3 }), // System -> System.Collections
            };

            //SystemSerializeObject(referenceObjects); // There was an error generating the XML document.
            KGySerializeObject(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback | XmlSerializationOptions.FullyQualifiedNames);
            KGySerializeObject(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback | XmlSerializationOptions.FullyQualifiedNames | XmlSerializationOptions.IgnoreTypeForwardedFromAttribute);
        }

        [Test]
        public void SerializeRecords()
        {
            object[] referenceObjects =
            {
                new ClassRecord("alpha", 1),
                new ValueRecord("alpha", 1),
            };

            KGySerializeObject(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback);
        }

        [Test]
        public void SerializeTrustedComplexObject()
        {
            var testObj = new TrustedComplexClass
            {
                DictionaryProperty = { { "alpha", 1 }, { "beta", 2 } }
            };

            // Adding 4 elements just because of Capacity property in .NET 9+ so they will not be different.
            testObj.QueueField.Enqueue(1);
            testObj.QueueField.Enqueue(2);
            testObj.QueueField.Enqueue(3);
            testObj.QueueField.Enqueue(4);

            KGySerializeObject(testObj, XmlSerializationOptions.None);
        }

#if !(NETCOREAPP2_0 && NETSTANDARD_TEST)
        [Test]
        public void SerializeRefProperty()
        {
            var testObj = new RefPropertyClass
            {
                RefProperty = 1,
                RefReadOnlyCollection = { 1, 2, 3 }
            };

            // Ref properties are ignored by default
            Throws<AssertionException>(() => KGySerializeObject(testObj, XmlSerializationOptions.RecursiveSerializationAsFallback), "Equality check failed");

            // But they can be included. Ref readonly is handled as normal read-only: they are considered for collections
            KGySerializeObject(testObj, XmlSerializationOptions.RecursiveSerializationAsFallback | XmlSerializationOptions.IncludeRefProperties);

            // Binary: IncludeRefProperties is needed for content serialization
            KGySerializeObject(testObj, XmlSerializationOptions.BinarySerializationAsFallback | XmlSerializationOptions.IncludeRefProperties, safeMode: false);
        }
#endif

#if NET47_OR_GREATER || NETCOREAPP
        [Test]
        public void SerializeTuples()
        {
            object[] referenceObjects =
            {
                ValueTuple.Create(),
                ValueTuple.Create(1),
                ValueTuple.Create(1, 2u),
                ValueTuple.Create(1, 2u, 3L),
                ValueTuple.Create(1, 2u, 3L, 4ul),
                ValueTuple.Create(1, 2u, 3L, 4ul, "5"),
                ValueTuple.Create(1, 2u, 3L, 4ul, "5", '6'),
                ValueTuple.Create(1, 2u, 3L, 4ul, "5", '6', 7f),
                ValueTuple.Create(1, 2u, 3L, 4ul, "5", '6', 7f, 8d), // TRest is is ValueTuple`1
                (1, 2u, 3L, 4ul, "5", '6', 7f, 8d, 9m), // TRest is ValueTuple`2
                new ValueTuple<int, uint, long, ulong, string, char, float, double> { Item1 = 1, Item2 = 2u, Item3 = 3L, Item4 = 4ul, Item5 = "5", Item6 = '6', Item7 = 7f, Rest = 8d, }, // TRest is not a nested tuple
            };

            KGySerializeObject(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback);
            KGySerializeObjects(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback);
        }
#endif

        [Test]
        public void UsePropertySetterIfPossibleTest()
        {
            string referenceValue = ValueWithStaticMember.Predefined.Value;
            Assert.AreEqual(nameof(ValueWithStaticMember.Predefined), referenceValue);

            object[] referenceObjects =
            {
                new PreInitializedProperties { PreInitializedProperty = new ValueWithStaticMember { Value = "Custom value" } }
            };

            KGySerializeObjects(referenceObjects, XmlSerializationOptions.RecursiveSerializationAsFallback);

            Assert.AreEqual(referenceValue, ValueWithStaticMember.Predefined.Value, "Deserialization has overwritten an existing instance with read-write property");
        }

        [Test]
        public void SafeModeTypeResolveTest()
        {
            var xml = @"<object type=""MyNamespace.DangerousType, DangerousAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null""></object>";
            Console.WriteLine(xml);
            Throws<InvalidOperationException>(() => XmlSerializer.DeserializeSafe(new StringReader(xml)));
        }

        [Test]
        public void SafeModeBinarySerializedContentTest()
        {
            var obj = new CustomGenericCollection<int> { 1, 2, 3 };

            XElement xml = XmlSerializer.Serialize(obj, XmlSerializationOptions.BinarySerializationAsFallback);
            Console.WriteLine(xml);

            // in safe mode, expected types must be specified
            // 1.) by XElement
            Throws<SerializationException>(() => XmlSerializer.DeserializeSafe(xml), "In safe mode you should specify the expected types in the expectedCustomTypes parameter of the deserialization methods.");

            // 2.) by reader
            using (var reader = XmlReader.Create(new StringReader(xml.ToString()), new XmlReaderSettings { CloseInput = true }))
                Throws<SerializationException>(() => XmlSerializer.DeserializeSafe(reader), "In safe mode you should specify the expected types in the expectedCustomTypes parameter of the deserialization methods.");

            // but it works if the expected type (or root type if no other is needed) is specified
            object deserialized = XmlSerializer.DeserializeSafe<CustomGenericCollection<int>>(xml);
            AssertDeepEquals(obj, deserialized);

            // it works also in non-safe mode
            deserialized = XmlSerializer.Deserialize(xml);
            AssertDeepEquals(obj, deserialized);
        }

        [TestCase(XmlSerializationOptions.None)]
        [TestCase(XmlSerializationOptions.CompactSerializationOfPrimitiveArrays)]
        public void SafeModeLargeArrayTest(XmlSerializationOptions options)
        {
            // Array size is above 8K so in SafeMode it is built rather than allocated at once
            long[] obj = Enumerable.Range(0, 1025).Select(i => (long)i).ToArray();
            XElement xml = XmlSerializer.Serialize(obj, options);
            Console.WriteLine(xml);

            // 1.) by XElement
            var arr = (long[])XmlSerializer.DeserializeSafe(xml);
            AssertItemsEqual(obj, arr);

            // 2.) by reader
            using (var reader = XmlReader.Create(new StringReader(xml.ToString()), new XmlReaderSettings { CloseInput = true }))
                arr = (long[])XmlSerializer.DeserializeSafe(reader);
            AssertItemsEqual(obj, arr);
        }

        [TestCase(XmlSerializationOptions.None)]
        [TestCase(XmlSerializationOptions.CompactSerializationOfPrimitiveArrays)]
        public void SafeModeArrayOutOfMemoryAttackTest(XmlSerializationOptions options)
        {
            var obj = new[] { 1, 2, 3 };
            XElement xml = XmlSerializer.Serialize(obj, options);

            // Injecting invalid length: in SafeMode this is detected without attempting to allocate the array
            // <object type="System.Int32[]" length="2147483647">...</object>
            xml.Attribute("length").Value = Int32.MaxValue.ToString(CultureInfo.InvariantCulture);
            Console.WriteLine(xml);

            // 1.) by XElement
            if (!EnvironmentHelper.IsMono) // In Mono the array is simply allocated so no exception occurs
                Throws<OutOfMemoryException>(() => XmlSerializer.Deserialize(xml));
            Throws<ArgumentException>(() => XmlSerializer.DeserializeSafe(xml), "Array items length mismatch. Expected items: 2147483647, found items: 3.");

            // 2.) by reader
            if (!EnvironmentHelper.IsMono) // In Mono the array is simply allocated so no exception occurs
            {
                using var reader = XmlReader.Create(new StringReader(xml.ToString()), new XmlReaderSettings { CloseInput = true });
                Throws<OutOfMemoryException>(() => XmlSerializer.Deserialize(reader));
            }

            using (var reader = XmlReader.Create(new StringReader(xml.ToString()), new XmlReaderSettings { CloseInput = true }))
                Throws<ArgumentException>(() => XmlSerializer.DeserializeSafe(reader), "Array items length mismatch. Expected items: 2147483647, found items: 3.");
        }

        [Test]
        public void SafeModeCollectionOutOfMemoryAttackTest()
        {
            var obj = new List<int> { 1, 2, 3 };
            XElement xml = XmlSerializer.Serialize(obj, XmlSerializationOptions.None);

            // Injecting invalid capacity: In SafeMode this is simply ignored so the deserialization will succeed
            // <object type="System.Collections.Generic.List`1[System.Int32]">
            //   <Capacity>2147483647</Capacity>
            //   <item>1</item>
            //   <item>2</item>
            //   <item>3</item>
            // </object>
            xml.Element("Capacity").Value = Int32.MaxValue.ToString(CultureInfo.InvariantCulture);
            Console.WriteLine(xml);

            // 1.) by XElement
            if (!EnvironmentHelper.IsMono) // In Mono the list is simply allocated so no exception occurs
                Throws<OutOfMemoryException>(() => XmlSerializer.Deserialize(xml));
            var list = XmlSerializer.DeserializeSafe<List<int>>(xml);
            AssertItemsEqual(obj, list);

            // 2.) by reader
            if (!EnvironmentHelper.IsMono) // In Mono the list is simply allocated so no exception occurs
            {
                using var reader = XmlReader.Create(new StringReader(xml.ToString()), new XmlReaderSettings { CloseInput = true });
                Throws<OutOfMemoryException>(() => XmlSerializer.Deserialize(reader));
            }

            using (var reader = XmlReader.Create(new StringReader(xml.ToString()), new XmlReaderSettings { CloseInput = true }))
                list = XmlSerializer.DeserializeSafe<List<int>>(reader);
            AssertItemsEqual(obj, list);
        }

        #endregion
    }
}
