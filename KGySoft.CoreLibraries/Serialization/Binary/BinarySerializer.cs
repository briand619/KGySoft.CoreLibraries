﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BinarySerializer.cs
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

using KGySoft.CoreLibraries;
using KGySoft.Reflection;

#endregion

namespace KGySoft.Serialization.Binary
{
    /// <summary>
    /// Provides public static methods for binary serialization. Most of its methods use a <see cref="BinarySerializationFormatter"/> instance internally.
    /// <br/>See the <strong>Remarks</strong> section of the <see cref="BinarySerializationFormatter"/> class for details and examples.
    /// </summary>
    /// <seealso cref="BinarySerializationFormatter"/>
    /// <seealso cref="BinarySerializationOptions"/>
    /// <seealso cref="IBinarySerializable"/>
    public static class BinarySerializer
    {
        #region Constants

        internal const BinarySerializationOptions DefaultSerializationOptions = BinarySerializationOptions.CompactSerializationOfStructures;
        internal const BinarySerializationOptions DefaultDeserializationOptions = BinarySerializationOptions.SafeMode;

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Serializes an object into a byte array.
        /// </summary>
        /// <param name="data">The object to serialize</param>
        /// <param name="options">Options of the serialization. This parameter is optional.
        /// <br/>Default value: <see cref="BinarySerializationOptions.CompactSerializationOfStructures"/>.</param>
        /// <returns>Serialized raw data of the object</returns>
        public static byte[] Serialize(object? data, BinarySerializationOptions options = DefaultSerializationOptions)
            => new BinarySerializationFormatter(options).Serialize(data);

        /// <summary>
        /// Deserializes the specified part of a byte array into an object. If <see cref="BinarySerializationOptions.SafeMode"/> is enabled
        /// in <paramref name="options"/> and <paramref name="rawData"/> contains natively not supported types by name, then you should use
        /// the other overloads to specify the expected types.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="Deserialize{T}(byte[], int, BinarySerializationOptions, Type[])"/> overload for details.
        /// </summary>
        /// <param name="rawData">Contains the raw data representation of the object to deserialize.</param>
        /// <param name="offset">Points to the starting position of the object data in <paramref name="rawData"/>. This parameter is optional.
        /// <br/>Default value: <c>0</c>.</param>
        /// <param name="options">Options of the deserialization. This parameter is optional.
        /// <br/>Default value: <see cref="BinarySerializationOptions.SafeMode"/>.</param>
        /// <returns>The deserialized object.</returns>
        public static object? Deserialize(byte[] rawData, int offset = 0, BinarySerializationOptions options = DefaultDeserializationOptions)
            => new BinarySerializationFormatter(options).Deserialize(rawData, offset);

        /// <summary>
        /// Deserializes the specified part of a byte array into an object.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="Deserialize{T}(byte[], int, BinarySerializationOptions, Type[])"/> overload for details.
        /// </summary>
        /// <param name="rawData">Contains the raw data representation of the object to deserialize.</param>
        /// <param name="offset">Points to the starting position of the object data in <paramref name="rawData"/>.</param>
        /// <param name="options">Options of the deserialization.</param>
        /// <param name="expectedCustomTypes">The types that are expected to present in <paramref name="rawData"/> by name.
        /// If <see cref="BinarySerializationOptions.SafeMode"/> is not enabled in <paramref name="options"/>
        /// or <paramref name="rawData"/> does not contain any types by name, then this parameter is optional.</param>
        /// <returns>The deserialized object.</returns>
        public static object? Deserialize(byte[] rawData, int offset, BinarySerializationOptions options, params Type[]? expectedCustomTypes)
            => new BinarySerializationFormatter(options).Deserialize(rawData, offset, expectedCustomTypes);

        /// <summary>
        /// Deserializes the specified part of a byte array into an object using safe mode.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="Deserialize{T}(byte[], int, BinarySerializationOptions, Type[])"/> overload for details.
        /// </summary>
        /// <param name="rawData">Contains the raw data representation of the object to deserialize.</param>
        /// <param name="offset">Points to the starting position of the object data in <paramref name="rawData"/>.</param>
        /// <param name="expectedCustomTypes">The types that are expected to present in <paramref name="rawData"/> by name.
        /// If <paramref name="rawData"/> does not contain any types by name, then this parameter is optional.</param>
        /// <returns>The deserialized object.</returns>
        public static object? Deserialize(byte[] rawData, int offset, params Type[]? expectedCustomTypes)
            => new BinarySerializationFormatter(DefaultDeserializationOptions).Deserialize(rawData, offset, expectedCustomTypes);

        /// <summary>
        /// Deserializes the specified part of a byte array into an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The expected type of the result.</typeparam>
        /// <param name="rawData">Contains the raw data representation of the object to deserialize.</param>
        /// <param name="offset">Points to the starting position of the object data in <paramref name="rawData"/>.</param>
        /// <param name="options">Options of the deserialization.</param>
        /// <param name="expectedCustomTypes">The types that are expected to present in <paramref name="rawData"/> by name.
        /// If <see cref="BinarySerializationOptions.SafeMode"/> is not enabled in <paramref name="options"/>
        /// or <paramref name="rawData"/> does not contain any types by name, then this parameter is optional.</param>
        /// <returns>The deserialized instance of <typeparamref name="T"/>.</returns>
        /// <remarks>
        /// <para><paramref name="expectedCustomTypes"/> must be specified if <see cref="BinarySerializationOptions.SafeMode"/> is enabled in <paramref name="options"/>
        /// and <paramref name="rawData"/> contains types encoded by their names. Natively supported types are not needed to be included
        /// unless the original object was serialized with the <see cref="BinarySerializationOptions.ForceRecursiveSerializationOfSupportedTypes"/> option enabled.</para>
        /// <para><typeparamref name="T"/> is allowed to be an interface or abstract type but if it's different from the actual type of the result,
        /// then the actual type also might needed to be included in <paramref name="expectedCustomTypes"/>.</para>
        /// <para>You can specify <paramref name="expectedCustomTypes"/> even if <see cref="BinarySerializationOptions.SafeMode"/> is not enabled in <paramref name="options"/>
        /// as it may improve the performance of type resolving and can help avoiding possible ambiguities if types were not serialized with full assembly identity
        /// (eg. if <see cref="BinarySerializationOptions.OmitAssemblyQualifiedNames"/> was enabled on serialization).</para>
        /// <para>If a type in <paramref name="expectedCustomTypes"/> has a different assembly identity in the deserialization stream, and
        /// it is not indicated by a <see cref="TypeForwardedFromAttribute"/> declared on the type, then you should instantiate a <see cref="BinarySerializationFormatter"/> class
        /// manually and set its <see cref="BinarySerializationFormatter.Binder"/> property to a <see cref="ForwardedTypesSerializationBinder"/> instance
        /// to specify the expected types.</para>
        /// <para>For arrays it is enough to specify the element type and for generic types you can specify the
        /// natively not supported generic type definition and generic type arguments separately.
        /// If <paramref name="expectedCustomTypes"/> contains constructed generic types, then the generic type definition and
        /// the type arguments will be treated as expected types in any combination.</para>
        /// </remarks>
        public static T Deserialize<T>(byte[] rawData, int offset, BinarySerializationOptions options, params Type[]? expectedCustomTypes)
            => new BinarySerializationFormatter(options).Deserialize<T>(rawData, offset, expectedCustomTypes);

        /// <summary>
        /// Deserializes the specified part of a byte array into an instance of <typeparamref name="T"/> using safe mode.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="Deserialize{T}(byte[], int, BinarySerializationOptions, Type[])"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The expected type of the result.</typeparam>
        /// <param name="rawData">Contains the raw data representation of the object to deserialize.</param>
        /// <param name="offset">Points to the starting position of the object data in <paramref name="rawData"/>. This parameter is optional.
        /// <br/>Default value: <c>0</c>.</param>
        /// <param name="expectedCustomTypes">The types that are expected to present in <paramref name="rawData"/> by name.
        /// If <paramref name="rawData"/> does not contain any types by name, then this parameter is optional.</param>
        /// <returns>The deserialized instance of <typeparamref name="T"/>.</returns>
        public static T Deserialize<T>(byte[] rawData, int offset = 0, params Type[]? expectedCustomTypes)
            => new BinarySerializationFormatter(DefaultDeserializationOptions).Deserialize<T>(rawData, offset, expectedCustomTypes);

        /// <summary>
        /// Deserializes the specified part of a byte array into an object.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="Deserialize{T}(byte[], int, BinarySerializationOptions, Type[])"/> overload for details.
        /// </summary>
        /// <param name="rawData">Contains the raw data representation of the object to deserialize.</param>
        /// <param name="offset">Points to the starting position of the object data in <paramref name="rawData"/>.</param>
        /// <param name="options">Options of the deserialization.</param>
        /// <param name="expectedCustomTypes">The types that are expected to present in <paramref name="rawData"/> by name.
        /// If <see cref="BinarySerializationOptions.SafeMode"/> is not enabled in <paramref name="options"/>
        /// or <paramref name="rawData"/> does not contain any types by name, then this parameter can be <see langword="null"/>.</param>
        /// <returns>The deserialized object.</returns>
        public static object? Deserialize(byte[] rawData, int offset, BinarySerializationOptions options, IEnumerable<Type>? expectedCustomTypes)
            => new BinarySerializationFormatter(options).Deserialize(rawData, offset, expectedCustomTypes);

        /// <summary>
        /// Deserializes the specified part of a byte array into an object using safe mode.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="Deserialize{T}(byte[], int, BinarySerializationOptions, Type[])"/> overload for details.
        /// </summary>
        /// <param name="rawData">Contains the raw data representation of the object to deserialize.</param>
        /// <param name="offset">Points to the starting position of the object data in <paramref name="rawData"/>.</param>
        /// <param name="expectedCustomTypes">The types that are expected to present in <paramref name="rawData"/> by name.
        /// If <paramref name="rawData"/> does not contain any types by name, then this parameter can be <see langword="null"/>.</param>
        /// <returns>The deserialized object.</returns>
        public static object? Deserialize(byte[] rawData, int offset, IEnumerable<Type>? expectedCustomTypes)
            => new BinarySerializationFormatter(DefaultDeserializationOptions).Deserialize(rawData, offset, expectedCustomTypes);

        /// <summary>
        /// Deserializes the specified part of a byte array into an instance of <typeparamref name="T"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="Deserialize{T}(byte[], int, BinarySerializationOptions, Type[])"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The expected type of the result.</typeparam>
        /// <param name="rawData">Contains the raw data representation of the object to deserialize.</param>
        /// <param name="offset">Points to the starting position of the object data in <paramref name="rawData"/>.</param>
        /// <param name="options">Options of the deserialization.</param>
        /// <param name="expectedCustomTypes">The types that are expected to present in <paramref name="rawData"/> by name.
        /// If <see cref="BinarySerializationOptions.SafeMode"/> is not enabled in <paramref name="options"/>
        /// or <paramref name="rawData"/> does not contain any types by name, then this parameter can be <see langword="null"/>.</param>
        /// <returns>The deserialized instance of <typeparamref name="T"/>.</returns>
        public static T Deserialize<T>(byte[] rawData, int offset, BinarySerializationOptions options, IEnumerable<Type>? expectedCustomTypes)
            => new BinarySerializationFormatter(options).Deserialize<T>(rawData, offset, expectedCustomTypes);

        /// <summary>
        /// Deserializes the specified part of a byte array into an instance of <typeparamref name="T"/> using safe mode.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="Deserialize{T}(byte[], int, BinarySerializationOptions, Type[])"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The expected type of the result.</typeparam>
        /// <param name="rawData">Contains the raw data representation of the object to deserialize.</param>
        /// <param name="offset">Points to the starting position of the object data in <paramref name="rawData"/>.</param>
        /// <param name="expectedCustomTypes">The types that are expected to present in <paramref name="rawData"/> by name.
        /// If <paramref name="rawData"/> does not contain any types by name, then this parameter can be <see langword="null"/>.</param>
        /// <returns>The deserialized instance of <typeparamref name="T"/>.</returns>
        public static T Deserialize<T>(byte[] rawData, int offset, IEnumerable<Type>? expectedCustomTypes)
            => new BinarySerializationFormatter(DefaultDeserializationOptions).Deserialize<T>(rawData, offset, expectedCustomTypes);

        /// <summary>
        /// Serializes the given <paramref name="data"/> into a <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The stream, into which the data is written. The stream must support writing and will remain open after serialization.</param>
        /// <param name="data">The data that will be written into the stream.</param>
        /// <param name="options">Options of the serialization. This parameter is optional.
        /// <br/>Default value: <see cref="BinarySerializationOptions.CompactSerializationOfStructures"/>.</param>
        public static void SerializeToStream(Stream stream, object? data, BinarySerializationOptions options = DefaultSerializationOptions)
            => new BinarySerializationFormatter(options).SerializeToStream(stream, data);

        /// <summary>
        /// Deserializes the content of the specified serialization <paramref name="stream"/> from its current position into an object.
        /// If <see cref="BinarySerializationOptions.SafeMode"/> is enabled in <paramref name="options"/> and <paramref name="stream"/>
        /// contains natively not supported types by name, then you should use the other overloads to specify the expected types.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="DeserializeFromStream{T}(Stream, BinarySerializationOptions, Type[])"/> overload for details.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the serialized data. The stream must support reading and will remain open after deserialization.</param>
        /// <param name="options">Options of the deserialization. This parameter is optional.
        /// <br/>Default value: <see cref="BinarySerializationOptions.SafeMode"/>.</param>
        /// <returns>The deserialized data.</returns>
        public static object? DeserializeFromStream(Stream stream, BinarySerializationOptions options = DefaultDeserializationOptions)
            => new BinarySerializationFormatter(options).DeserializeFromStream(stream);

        /// <summary>
        /// Deserializes the content of the specified serialization <paramref name="stream"/> from its current position into an object.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="DeserializeFromStream{T}(Stream, BinarySerializationOptions, Type[])"/> overload for details.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the serialized data. The stream must support reading and will remain open after deserialization.</param>
        /// <param name="options">Options of the deserialization.</param>
        /// <param name="expectedCustomTypes">The types that are expected to present in the serialization <paramref name="stream"/> by name.
        /// If <see cref="BinarySerializationOptions.SafeMode"/> is not enabled in <paramref name="options"/>
        /// or <paramref name="stream"/> does not contain any types by name, then this parameter is optional.</param>
        /// <returns>The deserialized object.</returns>
        public static object? DeserializeFromStream(Stream stream, BinarySerializationOptions options, params Type[]? expectedCustomTypes)
            => new BinarySerializationFormatter(options).DeserializeFromStream(stream, expectedCustomTypes);

        /// <summary>
        /// Deserializes the content of the specified serialization <paramref name="stream"/> from its current position into an object using safe mode.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="DeserializeFromStream{T}(Stream, BinarySerializationOptions, Type[])"/> overload for details.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the serialized data. The stream must support reading and will remain open after deserialization.</param>
        /// <param name="expectedCustomTypes">The types that are expected to present in the serialization <paramref name="stream"/> by name.
        /// If <paramref name="stream"/> does not contain any types by name, then this parameter is optional.</param>
        /// <returns>The deserialized object.</returns>
        public static object? DeserializeFromStream(Stream stream, params Type[]? expectedCustomTypes)
            => new BinarySerializationFormatter(DefaultDeserializationOptions).DeserializeFromStream(stream, expectedCustomTypes);

        /// <summary>
        /// Deserializes the content of the specified serialization <paramref name="stream"/> from its current position into an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The expected type of the result.</typeparam>
        /// <param name="stream">The <see cref="Stream"/> containing the serialized data. The stream must support reading and will remain open after deserialization.</param>
        /// <param name="options">Options of the deserialization.</param>
        /// <param name="expectedCustomTypes">The types that are expected to present in the serialization <paramref name="stream"/> by name.
        /// If <see cref="BinarySerializationOptions.SafeMode"/> is not enabled in <paramref name="options"/>
        /// or <paramref name="stream"/> does not contain any types by name, then this parameter is optional.</param>
        /// <returns>The deserialized instance of <typeparamref name="T"/>.</returns>
        /// <remarks>
        /// <para><paramref name="expectedCustomTypes"/> must be specified if <see cref="BinarySerializationOptions.SafeMode"/> is enabled in <paramref name="options"/>
        /// and the serialization <paramref name="stream"/> contains types encoded by their names. Natively supported types are not needed to be included
        /// unless the original object was serialized with the <see cref="BinarySerializationOptions.ForceRecursiveSerializationOfSupportedTypes"/> option enabled.</para>
        /// <para><typeparamref name="T"/> is allowed to be an interface or abstract type but if it's different from the actual type of the result,
        /// then the actual type also might needed to be included in <paramref name="expectedCustomTypes"/>.</para>
        /// <para>You can specify <paramref name="expectedCustomTypes"/> even if <see cref="BinarySerializationOptions.SafeMode"/> is not enabled in <paramref name="options"/>
        /// as it may improve the performance of type resolving and can help avoiding possible ambiguities if types were not serialized with full assembly identity
        /// (eg. if <see cref="BinarySerializationOptions.OmitAssemblyQualifiedNames"/> was enabled on serialization).</para>
        /// <para>If a type in <paramref name="expectedCustomTypes"/> has a different assembly identity in the deserialization stream, and
        /// it is not indicated by a <see cref="TypeForwardedFromAttribute"/> declared on the type, then you should instantiate a <see cref="BinarySerializationFormatter"/> class
        /// manually and set its <see cref="BinarySerializationFormatter.Binder"/> property to a <see cref="ForwardedTypesSerializationBinder"/> instance
        /// to specify the expected types.</para>
        /// <para>For arrays it is enough to specify the element type and for generic types you can specify the
        /// natively not supported generic type definition and generic type arguments separately.
        /// If <paramref name="expectedCustomTypes"/> contains constructed generic types, then the generic type definition and
        /// the type arguments will be treated as expected types in any combination.</para>
        /// </remarks>
        public static T DeserializeFromStream<T>(Stream stream, BinarySerializationOptions options, params Type[]? expectedCustomTypes)
            => new BinarySerializationFormatter(options).DeserializeFromStream<T>(stream, expectedCustomTypes);

        /// <summary>
        /// Deserializes the content of the specified serialization <paramref name="stream"/> from its current position into an instance of <typeparamref name="T"/> using safe mode.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="DeserializeFromStream{T}(Stream, BinarySerializationOptions, Type[])"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The expected type of the result.</typeparam>
        /// <param name="stream">The <see cref="Stream"/> containing the serialized data. The stream must support reading and will remain open after deserialization.</param>
        /// <param name="expectedCustomTypes">The types that are expected to present in the serialization <paramref name="stream"/> by name.
        /// If <paramref name="stream"/> does not contain any types by name, then this parameter is optional.</param>
        /// <returns>The deserialized instance of <typeparamref name="T"/>.</returns>
        public static T DeserializeFromStream<T>(Stream stream, params Type[]? expectedCustomTypes)
            => new BinarySerializationFormatter(DefaultDeserializationOptions).DeserializeFromStream<T>(stream, expectedCustomTypes);

        /// <summary>
        /// Deserializes the content of the specified serialization <paramref name="stream"/> from its current position into an object.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="DeserializeFromStream{T}(Stream, BinarySerializationOptions, Type[])"/> overload for details.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the serialized data. The stream must support reading and will remain open after deserialization.</param>
        /// <param name="options">Options of the deserialization.</param>
        /// <param name="expectedCustomTypes">The types that are expected to present in the serialization <paramref name="stream"/> by name.
        /// If <see cref="BinarySerializationOptions.SafeMode"/> is not enabled in <paramref name="options"/>
        /// or <paramref name="stream"/> does not contain any types by name, then this parameter can be <see langword="null"/>.</param>
        /// <returns>The deserialized object.</returns>
        public static object? DeserializeFromStream(Stream stream, BinarySerializationOptions options, IEnumerable<Type>? expectedCustomTypes)
            => new BinarySerializationFormatter(options).DeserializeFromStream(stream, expectedCustomTypes);

        /// <summary>
        /// Deserializes the content of the specified serialization <paramref name="stream"/> from its current position into an object using safe mode.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="DeserializeFromStream{T}(Stream, BinarySerializationOptions, Type[])"/> overload for details.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the serialized data. The stream must support reading and will remain open after deserialization.</param>
        /// <param name="expectedCustomTypes">The types that are expected to present in the serialization <paramref name="stream"/> by name.
        /// If <paramref name="stream"/> does not contain any types by name, then this parameter can be <see langword="null"/>.</param>
        /// <returns>The deserialized object.</returns>
        public static object? DeserializeFromStream(Stream stream, IEnumerable<Type>? expectedCustomTypes)
            => new BinarySerializationFormatter(DefaultDeserializationOptions).DeserializeFromStream(stream, expectedCustomTypes);

        /// <summary>
        /// Deserializes the content of the specified serialization <paramref name="stream"/> from its current position into an instance of <typeparamref name="T"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="DeserializeFromStream{T}(Stream, BinarySerializationOptions, Type[])"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The expected type of the result.</typeparam>
        /// <param name="stream">The <see cref="Stream"/> containing the serialized data. The stream must support reading and will remain open after deserialization.</param>
        /// <param name="options">Options of the deserialization.</param>
        /// <param name="expectedCustomTypes">The types that are expected to present in the serialization <paramref name="stream"/> by name.
        /// If <see cref="BinarySerializationOptions.SafeMode"/> is not enabled in <paramref name="options"/>
        /// or <paramref name="stream"/> does not contain any types by name, then this parameter can be <see langword="null"/>.</param>
        /// <returns>The deserialized instance of <typeparamref name="T"/>.</returns>
        public static T DeserializeFromStream<T>(Stream stream, BinarySerializationOptions options, IEnumerable<Type>? expectedCustomTypes)
            => new BinarySerializationFormatter(options).DeserializeFromStream<T>(stream, expectedCustomTypes);

        /// <summary>
        /// Deserializes the content of the specified serialization <paramref name="stream"/> from its current position into an instance of <typeparamref name="T"/> using safe mode.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="DeserializeFromStream{T}(Stream, BinarySerializationOptions, Type[])"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The expected type of the result.</typeparam>
        /// <param name="stream">The <see cref="Stream"/> containing the serialized data. The stream must support reading and will remain open after deserialization.</param>
        /// <param name="expectedCustomTypes">The types that are expected to present in the serialization <paramref name="stream"/> by name.
        /// If <paramref name="stream"/> does not contain any types by name, then this parameter can be <see langword="null"/>.</param>
        /// <returns>The deserialized instance of <typeparamref name="T"/>.</returns>
        public static T DeserializeFromStream<T>(Stream stream, IEnumerable<Type>? expectedCustomTypes)
            => new BinarySerializationFormatter(DefaultDeserializationOptions).DeserializeFromStream<T>(stream, expectedCustomTypes);

        /// <summary>
        /// Serializes the given <paramref name="data"/> by using the provided <paramref name="writer"/>.
        /// </summary>
        /// <remarks>
        /// <note>This method produces compatible serialized data with <see cref="Serialize">Serialize</see>
        /// and <see cref="SerializeToStream">SerializeToStream</see> methods only when encoding of the writer is UTF-8.
        /// Otherwise, you must use <see cref="O:KGySoft.Serialization.Binary.BinarySerializationFormatter.DeserializeByReader">DeserializeByReader</see> with the same encoding as here.</note>
        /// </remarks>
        /// <param name="writer">The writer that will used to serialize data. The writer will remain opened after serialization.</param>
        /// <param name="data">The data that will be written by the writer.</param>
        /// <param name="options">Options of the serialization. This parameter is optional.
        /// <br/>Default value: <see cref="BinarySerializationOptions.CompactSerializationOfStructures"/>.</param>
        public static void SerializeByWriter(BinaryWriter writer, object? data, BinarySerializationOptions options = DefaultSerializationOptions)
            => new BinarySerializationFormatter(options).SerializeByWriter(writer, data);

        /// <summary>
        /// Deserializes the content of a serialization stream wrapped by the specified <paramref name="reader"/> from its current position into an object.
        /// If <see cref="BinarySerializationOptions.SafeMode"/> is enabled in <paramref name="options"/> and the stream
        /// contains natively not supported types by name, then you should use the other overloads to specify the expected types.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="DeserializeByReader{T}(BinaryReader, BinarySerializationOptions, Type[])"/> overload for details.
        /// </summary>
        /// <param name="reader">The reader that wraps the stream containing the serialized data. The reader will remain open after deserialization.</param>
        /// <param name="options">Options of the deserialization. This parameter is optional.
        /// <br/>Default value: <see cref="BinarySerializationOptions.SafeMode"/>.</param>
        /// <returns>The deserialized data.</returns>
        public static object? DeserializeByReader(BinaryReader reader, BinarySerializationOptions options = DefaultDeserializationOptions)
            => new BinarySerializationFormatter(options).DeserializeByReader(reader);

        /// <summary>
        /// Deserializes the content of a serialization stream wrapped by the specified <paramref name="reader"/> from its current position into an object.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="DeserializeByReader{T}(BinaryReader, BinarySerializationOptions, Type[])"/> overload for details.
        /// </summary>
        /// <param name="reader">The reader that wraps the stream containing the serialized data. The reader will remain open after deserialization.</param>
        /// <param name="options">Options of the deserialization.</param>
        /// <param name="expectedCustomTypes">The types that are expected to present in the serialization stream by name.
        /// If <see cref="BinarySerializationOptions.SafeMode"/> is not enabled in <paramref name="options"/>
        /// or the stream does not contain any types by name, then this parameter is optional.</param>
        /// <returns>The deserialized object.</returns>
        public static object? DeserializeByReader(BinaryReader reader, BinarySerializationOptions options, params Type[]? expectedCustomTypes)
            => new BinarySerializationFormatter(options).DeserializeByReader(reader, expectedCustomTypes);

        /// <summary>
        /// Deserializes the content of a serialization stream wrapped by the specified <paramref name="reader"/> from its current position into an object using safe mode.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="DeserializeByReader{T}(BinaryReader, BinarySerializationOptions, Type[])"/> overload for details.
        /// </summary>
        /// <param name="reader">The reader that wraps the stream containing the serialized data. The reader will remain open after deserialization.</param>
        /// <param name="expectedCustomTypes">The types that are expected to present in the serialization stream by name.
        /// If the stream does not contain any types by name, then this parameter is optional.</param>
        /// <returns>The deserialized object.</returns>
        public static object? DeserializeByReader(BinaryReader reader, params Type[]? expectedCustomTypes)
            => new BinarySerializationFormatter(DefaultDeserializationOptions).DeserializeByReader(reader, expectedCustomTypes);

        /// <summary>
        /// Deserializes the content of a serialization stream wrapped by the specified <paramref name="reader"/> from its current position
        /// into an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The expected type of the result.</typeparam>
        /// <param name="reader">The reader that wraps the stream containing the serialized data. The reader will remain open after deserialization.</param>
        /// <param name="options">Options of the deserialization.</param>
        /// <param name="expectedCustomTypes">The types that are expected to present in the serialization stream by name.
        /// If <see cref="BinarySerializationOptions.SafeMode"/> is not enabled in <paramref name="options"/>
        /// or the stream does not contain any types by name, then this parameter is optional.</param>
        /// <returns>The deserialized instance of <typeparamref name="T"/>.</returns>
        /// <remarks>
        /// <note>If data was serialized by <see cref="Serialize">Serialize</see> or <see cref="SerializeToStream">SerializeToStream</see> methods, then
        /// <paramref name="reader"/> must use UTF-8 encoding to get the correct result. If data was serialized by
        /// the <see cref="SerializeByWriter">SerializeByWriter</see> method, then you must use the same encoding as was used there.</note>
        /// <para><paramref name="expectedCustomTypes"/> must be specified if <see cref="BinarySerializationOptions.SafeMode"/> is enabled in <paramref name="options"/>
        /// and the serialization stream contains types encoded by their names. Natively supported types are not needed to be included
        /// unless the original object was serialized with the <see cref="BinarySerializationOptions.ForceRecursiveSerializationOfSupportedTypes"/> option enabled.</para>
        /// <para><typeparamref name="T"/> is allowed to be an interface or abstract type but if it's different from the actual type of the result,
        /// then the actual type also might needed to be included in <paramref name="expectedCustomTypes"/>.</para>
        /// <para>You can specify <paramref name="expectedCustomTypes"/> even if <see cref="BinarySerializationOptions.SafeMode"/> is not enabled in <paramref name="options"/>
        /// as it may improve the performance of type resolving and can help avoiding possible ambiguities if types were not serialized with full assembly identity
        /// (eg. if <see cref="BinarySerializationOptions.OmitAssemblyQualifiedNames"/> was enabled on serialization).</para>
        /// <para>If a type in <paramref name="expectedCustomTypes"/> has a different assembly identity in the deserialization stream, and
        /// it is not indicated by a <see cref="TypeForwardedFromAttribute"/> declared on the type, then you should instantiate a <see cref="BinarySerializationFormatter"/> class
        /// manually and set its <see cref="BinarySerializationFormatter.Binder"/> property to a <see cref="ForwardedTypesSerializationBinder"/> instance
        /// to specify the expected types.</para>
        /// <para>For arrays it is enough to specify the element type and for generic types you can specify the
        /// natively not supported generic type definition and generic type arguments separately.
        /// If <paramref name="expectedCustomTypes"/> contains constructed generic types, then the generic type definition and
        /// the type arguments will be treated as expected types in any combination.</para>
        /// </remarks>
        public static T DeserializeByReader<T>(BinaryReader reader, BinarySerializationOptions options, params Type[]? expectedCustomTypes)
            => new BinarySerializationFormatter(options).DeserializeByReader<T>(reader, expectedCustomTypes);

        /// <summary>
        /// Deserializes the content of a serialization stream wrapped by the specified <paramref name="reader"/> from its current position
        /// into an instance of <typeparamref name="T"/> using safe mode.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="DeserializeByReader{T}(BinaryReader, BinarySerializationOptions, Type[])"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The expected type of the result.</typeparam>
        /// <param name="reader">The reader that wraps the stream containing the serialized data. The reader will remain open after deserialization.</param>
        /// <param name="expectedCustomTypes">The types that are expected to present in the serialization stream by name.
        /// If the stream does not contain any types by name, then this parameter is optional.</param>
        /// <returns>The deserialized instance of <typeparamref name="T"/>.</returns>
        public static T DeserializeByReader<T>(BinaryReader reader, params Type[]? expectedCustomTypes)
            => new BinarySerializationFormatter(DefaultDeserializationOptions).DeserializeByReader<T>(reader, expectedCustomTypes);

        /// <summary>
        /// Deserializes the content of a serialization stream wrapped by the specified <paramref name="reader"/> from its current position into an object.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="DeserializeByReader{T}(BinaryReader, BinarySerializationOptions, Type[])"/> overload for details.
        /// </summary>
        /// <param name="reader">The reader that wraps the stream containing the serialized data. The reader will remain open after deserialization.</param>
        /// <param name="options">Options of the deserialization.</param>
        /// <param name="expectedCustomTypes">The types that are expected to present in the serialization stream by name.
        /// If <see cref="BinarySerializationOptions.SafeMode"/> is not enabled in <paramref name="options"/>
        /// or the stream does not contain any types by name, then this parameter can be <see langword="null"/>.</param>
        /// <returns>The deserialized object.</returns>
        public static object? DeserializeByReader(BinaryReader reader, BinarySerializationOptions options, IEnumerable<Type>? expectedCustomTypes)
            => new BinarySerializationFormatter(options).DeserializeByReader(reader, expectedCustomTypes);

        /// <summary>
        /// Deserializes the content of a serialization stream wrapped by the specified <paramref name="reader"/> from its current position into an object using safe mode.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="DeserializeByReader{T}(BinaryReader, BinarySerializationOptions, Type[])"/> overload for details.
        /// </summary>
        /// <param name="reader">The reader that wraps the stream containing the serialized data. The reader will remain open after deserialization.</param>
        /// <param name="expectedCustomTypes">The types that are expected to present in the serialization stream by name.
        /// If the stream does not contain any types by name, then this parameter can be <see langword="null"/>.</param>
        /// <returns>The deserialized object.</returns>
        public static object? DeserializeByReader(BinaryReader reader, IEnumerable<Type>? expectedCustomTypes)
            => new BinarySerializationFormatter(DefaultDeserializationOptions).DeserializeByReader(reader, expectedCustomTypes);

        /// <summary>
        /// Deserializes the content of a serialization stream wrapped by the specified <paramref name="reader"/> from its current position
        /// into an instance of <typeparamref name="T"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="DeserializeByReader{T}(BinaryReader, BinarySerializationOptions, Type[])"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The expected type of the result.</typeparam>
        /// <param name="reader">The reader that wraps the stream containing the serialized data. The reader will remain open after deserialization.</param>
        /// <param name="options">Options of the deserialization.</param>
        /// <param name="expectedCustomTypes">The types that are expected to present in the serialization stream by name.
        /// If <see cref="BinarySerializationOptions.SafeMode"/> is not enabled in <paramref name="options"/>
        /// or the stream does not contain any types by name, then this parameter can be <see langword="null"/>.</param>
        /// <returns>The deserialized instance of <typeparamref name="T"/>.</returns>
        public static T DeserializeByReader<T>(BinaryReader reader, BinarySerializationOptions options, IEnumerable<Type>? expectedCustomTypes)
            => new BinarySerializationFormatter(options).DeserializeByReader<T>(reader, expectedCustomTypes);

        /// <summary>
        /// Deserializes the content of a serialization stream wrapped by the specified <paramref name="reader"/> from its current position
        /// into an instance of <typeparamref name="T"/> using safe mode.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="DeserializeByReader{T}(BinaryReader, BinarySerializationOptions, Type[])"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The expected type of the result.</typeparam>
        /// <param name="reader">The reader that wraps the stream containing the serialized data. The reader will remain open after deserialization.</param>
        /// <param name="expectedCustomTypes">The types that are expected to present in the serialization stream by name.
        /// If the stream does not contain any types by name, then this parameter is optional.</param>
        /// <returns>The deserialized instance of <typeparamref name="T"/>.</returns>
        public static T DeserializeByReader<T>(BinaryReader reader, IEnumerable<Type>? expectedCustomTypes)
            => new BinarySerializationFormatter(DefaultDeserializationOptions).DeserializeByReader<T>(reader, expectedCustomTypes);

        /// <summary>
        /// Serializes a <see cref="ValueType"/> into a byte array. If the type of the specified instance contains any references,
        /// then it is tried to be serialized by marshaling as a fallback option.
        /// </summary>
        /// <param name="obj">The <see cref="ValueType"/> object to serialize.</param>
        /// <returns>The byte array representation of the <see cref="ValueType"/> object.</returns>
        /// <remarks>
        /// <para>If the specified instance does not have any references, then its actual raw data is returned. In this case this method is very fast.</para>
        /// <para>If the specified instance has reference types, then as a fallback option, it is attempted to be serialized by using the <see cref="Marshal"/> class.
        /// To work properly the string and array fields must be decorated by the <see cref="MarshalAsAttribute"/> using <see cref="UnmanagedType.ByValTStr"/>
        /// or <see cref="UnmanagedType.ByValArray"/> values, and the <see cref="StructLayoutAttribute"/> must be defined on referenced classes.
        /// The <see cref="MarshalAsAttribute"/> annotations are ignored if the type does not contain any references and hence it is serialized by its actual raw content.
        /// <note>Serializing by the <see cref="Marshal"/> class as a fallback option is maintained only for compatibility reasons.
        /// If a value type contains references, then it is recommended to use the <see cref="Serialize">Serialize</see> method instead.
        /// You can use the <see cref="TrySerializeValueType"/> method to serialize only pure value types without any references. </note></para>
        /// <para>If the instance cannot be serialized even by the <see cref="Marshal"/> class, then an <see cref="ArgumentException"/> is thrown.</para>
        /// <note type="caution">If packing is not defined on the type of the instance by <see cref="StructLayoutAttribute.Pack">StructLayoutAttribute.Pack</see>,
        /// or the type contains pointer fields, then the length of the result might be different on 32 and 64 bit systems.
        /// The serialized content depends also on the endianness of the executing architecture.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="obj"/> contains references and it cannot be serialized even by the <see cref="Marshal"/> class.</exception>
        [SecurityCritical]
        public static unsafe byte[] SerializeValueType(ValueType obj)
        {
            if (obj == null!)
                Throw.ArgumentNullException(Argument.obj);

            if (!obj.GetType().IsManaged())
                return SerializeValueTypeRaw(obj);

            // Fallback with marshaling. Throws an ArgumentException on error
            byte[] result = new byte[Marshal.SizeOf(obj)];
            fixed (void* ptr = result)
                Marshal.StructureToPtr(obj, new IntPtr(ptr), false);

            return result;
        }

        /// <summary>
        /// Tries to serialize a <see cref="ValueType"/> into a byte array.
        /// The operation will succeed if the type of the specified instance does not contain any references.
        /// <br/>See also the <strong>Remarks</strong> section of the <see cref="SerializeValueType"/> method for details.
        /// </summary>
        /// <param name="obj">The <see cref="ValueType"/> object to serialize.</param>
        /// <param name="result">When this method returns, the byte array representation of the <see cref="ValueType"/> instance. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/>, if the specified <see cref="ValueType"/> contains no references and could be serialized; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null"/>.</exception>
        [SecuritySafeCritical]
        public static bool TrySerializeValueType(ValueType obj, [MaybeNullWhen(false)]out byte[] result)
        {
            result = null;

            if (obj == null!)
                Throw.ArgumentNullException(Argument.obj);
            if (obj.GetType().IsManaged())
                return false;

            result = SerializeValueTypeRaw(obj);
            return true;
        }

        /// <summary>
        /// Serializes the specified <paramref name="value"/> into a byte array.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize. It must be a value type that does not contain references.</typeparam>
        /// <param name="value">The value to serialize.</param>
        /// <returns>The byte array representation of the specified <paramref name="value"/>.</returns>
        /// <remarks>
        /// <note type="security">Do not use this method with <typeparamref name="T"/> types that have references.
        /// When using this library with a compiler that recognizes the <see langword="unmanaged"/> constraint,
        /// then this is enforced for direct calls; however, by using reflection <typeparamref name="T"/> can be any value type.
        /// For performance reasons this method does not check if <typeparamref name="T"/> has references
        /// but you can call the <see cref="TrySerializeValueType{T}"/> method that performs the check.</note>
        /// </remarks>
        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static unsafe byte[] SerializeValueType<T>(in T value) where T : unmanaged
        {
            byte[] result = new byte[sizeof(T)];
#if NET5_0_OR_GREATER
            Unsafe.As<byte, T>(ref MemoryMarshal.GetArrayDataReference(result)) = value;
#elif NETCOREAPP3_0_OR_GREATER
            Unsafe.As<byte, T>(ref result[0]) = value;
#else
            fixed (byte* dst = result)
                *(T*)dst = value;
#endif
            return result;
        }

        /// <summary>
        /// Tries to serialize the specified <paramref name="value"/> into a byte array.
        /// The operation will succeed if <typeparamref name="T"/> does not contain any references.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="SerializeValueType{T}"/> method for details.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="value">The value to serialize.</param>
        /// <param name="result">When this method returns, the byte array representation of the specified <paramref name="value"/>. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/>, if <typeparamref name="T"/> contains no references and could be serialized; otherwise, <see langword="false"/>.</returns>
        [SecuritySafeCritical]
        public static bool TrySerializeValueType<T>(in T value, [MaybeNullWhen(false)]out byte[] result) where T : unmanaged
        {
            // The unmanaged constraint guards this but if used from an older compiler or by reflection, then this check matters
            if (Reflector<T>.IsManaged)
            {
                result = null;
                return false;
            }

            result = SerializeValueType(value);
            return true;
        }

        /// <summary>
        /// Serializes an <see cref="Array"/> of <see cref="ValueType"/> elements into a byte array.
        /// </summary>
        /// <param name="array">The array to serialize.</param>
        /// <typeparam name="T">Element type of the array. Must be a <see cref="ValueType"/> that has no references.</typeparam>
        /// <returns>The byte array representation of the <paramref name="array"/>.</returns>
        /// <remarks>
        /// <note type="security">Do not use this method with <typeparamref name="T"/> types that have references.
        /// When using this library with a compiler that recognizes the <see langword="unmanaged"/> constraint,
        /// then this is enforced for direct calls; however, by using reflection <typeparamref name="T"/> can be any value type.
        /// For performance reasons this method does not check if <typeparamref name="T"/> has references
        /// but you can call the <see cref="TrySerializeValueArray{T}"/> method that performs the check.</note>
        /// </remarks>
        [SecurityCritical]
        public static unsafe byte[] SerializeValueArray<T>(T[] array) where T : unmanaged
        {
            if (array == null!)
                Throw.ArgumentNullException(Argument.array);
            if (array.Length == 0)
                return Reflector.EmptyArray<byte>();

            int len = sizeof(T) * array.Length;
            byte[] result = new byte[len];
#if NET5_0_OR_GREATER
            Unsafe.CopyBlock(ref MemoryMarshal.GetArrayDataReference(result), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetArrayDataReference(array)), (uint)len);
#elif NETCOREAPP3_0_OR_GREATER
            Unsafe.CopyBlock(ref result[0], ref Unsafe.As<T, byte>(ref array[0]), (uint)len);
#else
            fixed (void* src = array)
                Marshal.Copy(new IntPtr(src), result, 0, len);
#endif

            return result;
        }

        /// <summary>
        /// Tries to serialize an <see cref="Array"/> of <see cref="ValueType"/> elements into a byte array.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="SerializeValueArray{T}"/> method for details.
        /// </summary>
        /// <param name="array">The array to serialize.</param>
        /// <typeparam name="T">Element type of the array. Must be a <see cref="ValueType"/> that has no references.</typeparam>
        /// <param name="result">When this method returns, the byte array representation of the specified <paramref name="array"/>. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/>, if <typeparamref name="T"/> contains no references and could be serialized; otherwise, <see langword="false"/>.</returns>
        [SecuritySafeCritical]
        public static bool TrySerializeValueArray<T>(T[] array, [MaybeNullWhen(false)]out byte[] result) where T : unmanaged
        {
            if (array == null!)
                Throw.ArgumentNullException(Argument.array);

            // The unmanaged constraint guards this but if used from an older compiler or by reflection, then this check matters
            if (Reflector<T>.IsManaged)
            {
                result = null;
                return false;
            }

            result = SerializeValueArray(array);
            return true;
        }

        /// <summary>
        /// Deserializes a <see cref="ValueType"/> object from a byte array that was previously serialized by <see cref="SerializeValueType">SerializeValueType</see> method.
        /// </summary>
        /// <param name="type">The type of the target object. Must be a <see cref="ValueType"/>.</param>
        /// <param name="data">The byte array that starts with byte representation of the object.</param>
        /// <returns>The deserialized <see cref="ValueType"/> object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="type"/> is not a value type
        /// <br/>-or-
        /// <br/>The length of <paramref name="data"/> is too small.
        /// <br/>-or-
        /// <br/>The specified <paramref name="type"/> contains references and it cannot be deserialized even by using the <see cref="Marshal"/> class.</exception>
        public static object DeserializeValueType(Type type, byte[] data) => DeserializeValueType(type, data, 0);

        /// <summary>
        /// Deserializes a <see cref="ValueType"/> object from a byte array that was previously serialized by <see cref="SerializeValueType">SerializeValueType</see> method
        /// beginning on a specified <paramref name="offset"/>.
        /// </summary>
        /// <param name="type">The type of the target object. Must be a <see cref="ValueType"/>.</param>
        /// <param name="data">The byte array that contains the byte representation of the object.</param>
        /// <param name="offset">The offset that points to the beginning of the serialized data.</param>
        /// <returns>The deserialized <see cref="ValueType"/> object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is negative or too large.</exception>
        /// <exception cref="ArgumentException"><paramref name="type"/> is not a value type
        /// <br/>-or-
        /// <br/>The length of <paramref name="data"/> is too small.
        /// <br/>-or-
        /// <br/>The specified <paramref name="type"/> contains references and it cannot be deserialized even by using the <see cref="Marshal"/> class.</exception>
        [SecuritySafeCritical]
        public unsafe static object DeserializeValueType(Type type, byte[] data, int offset)
        {
            if (type == null!)
                Throw.ArgumentNullException(Argument.type);
            if (!type.IsValueType)
                Throw.ArgumentException(Argument.type, Res.BinarySerializationValueTypeExpected);
            if (data == null!)
                Throw.ArgumentNullException(Argument.data);
            if ((uint)offset > (uint)data.Length)
                Throw.ArgumentOutOfRangeException(Argument.offset);

            if (!type.IsManaged())
                return DeserializeValueTypeRaw(type, data, offset);

            // Fallback with marshaling. Throws an ArgumentException on error
            int len = Marshal.SizeOf(type);
            if (offset + len > data.Length)
                Throw.ArgumentException(Argument.data, Res.BinarySerializationDataLengthTooSmall);

            fixed (void* src = &data[offset])
                return Marshal.PtrToStructure(new IntPtr(src), type)!;
        }

        /// <summary>
        /// Deserializes an instance of <typeparamref name="T"/> from a byte array that was previously serialized
        /// by the <see cref="SerializeValueType{T}"/> method.
        /// </summary>
        /// <typeparam name="T">The type of the result. It must be a value type that does not contain references.</typeparam>
        /// <param name="data">The byte array that starts with byte representation of the object.</param>
        /// <returns>The deserialized <typeparamref name="T"/> instance.</returns>
        /// <exception cref="InvalidOperationException"><typeparamref name="T"/> contains references.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The length of <paramref name="data"/> is too small.</exception>
        [SecuritySafeCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static unsafe T DeserializeValueType<T>(byte[] data) where T : unmanaged
        {
            // The unmanaged constraint is not enforced in CLR so we must check it
            if (Reflector<T>.IsManaged)
                Throw.InvalidOperationException(Res.BinarySerializationValueTypeContainsReferences<T>());
            if (data == null!)
                Throw.ArgumentNullException(Argument.data);

            int len = sizeof(T);
            if (data.Length < len)
                Throw.ArgumentException(Argument.data, Res.BinarySerializationDataLengthTooSmall);

#if NET5_0_OR_GREATER
            return Unsafe.As<byte, T>(ref MemoryMarshal.GetArrayDataReference(data));
#elif NETCOREAPP3_0_OR_GREATER
            return Unsafe.As<byte, T>(ref data[0]);
#else
            fixed (byte* src = data)
                return *(T*)src;
#endif
        }

        /// <summary>
        /// Deserializes an instance of <typeparamref name="T"/> from a byte array that was previously serialized
        /// by the <see cref="SerializeValueType{T}"/> method.
        /// </summary>
        /// <typeparam name="T">The type of the result. It must be a value type that does not contain references.</typeparam>
        /// <param name="data">The byte array that starts with byte representation of the object.</param>
        /// <param name="offset">The offset that points to the beginning of the serialized data.</param>
        /// <returns>The deserialized <typeparamref name="T"/> instance.</returns>
        /// <exception cref="InvalidOperationException"><typeparamref name="T"/> contains references.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is negative or too large.</exception>
        /// <exception cref="ArgumentException">The length of <paramref name="data"/> is too small.</exception>
        [SecuritySafeCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static unsafe T DeserializeValueType<T>(byte[] data, int offset) where T : unmanaged
        {
            // The unmanaged constraint is not enforced in CLR so we must check it
            if (Reflector<T>.IsManaged)
                Throw.InvalidOperationException(Res.BinarySerializationValueTypeContainsReferences<T>());
            if (data == null!)
                Throw.ArgumentNullException(Argument.data);
            if ((uint)offset > (uint)data.Length)
                Throw.ArgumentOutOfRangeException(Argument.offset);

            int len = sizeof(T);
            if (offset + len > data.Length)
                Throw.ArgumentException(Argument.data, Res.BinarySerializationDataLengthTooSmall);

#if NETCOREAPP3_0_OR_GREATER
            return Unsafe.As<byte, T>(ref data[offset]);
#else
            fixed (byte* src = &data[offset])
                return *(T*)src;
#endif
        }

        /// <summary>
        /// Deserializes an array of <see cref="ValueType"/> objects from a byte array
        /// that was previously serialized by <see cref="SerializeValueArray{T}">SerializeValueArray</see> method.
        /// </summary>
        /// <typeparam name="T">Type of the elements in the deserialized array. Must be a <see cref="ValueType"/>.</typeparam>
        /// <param name="data">The byte array that contains the byte representation of the structures.</param>
        /// <param name="offset">The offset that points to the beginning of the serialized data.</param>
        /// <param name="count">Number of elements to deserialize from the <paramref name="data"/>.</param>
        /// <returns>The deserialized <see cref="ValueType"/> object.</returns>
        [SecuritySafeCritical]
        public static unsafe T[] DeserializeValueArray<T>(byte[] data, int offset, int count)
            where T : unmanaged
        {
            // The unmanaged constraint is not enforced in CLR so we must check it
            if (Reflector<T>.IsManaged)
                Throw.InvalidOperationException(Res.BinarySerializationValueTypeContainsReferences<T>());
            if (data == null!)
                Throw.ArgumentNullException(Argument.data);
            if (count < 0)
                Throw.ArgumentOutOfRangeException(Argument.count);
            if ((uint)offset > (uint)data.Length)
                Throw.ArgumentOutOfRangeException(Argument.offset);

            int len = sizeof(T) * count;
            if (offset + len > data.Length)
                Throw.ArgumentException(Argument.data, Res.BinarySerializationDataLengthTooSmall);

            if (count == 0)
                return Reflector.EmptyArray<T>();

            T[] result = new T[count];
#if NET5_0_OR_GREATER
            // must use unaligned because data[offset] is not necessarily a pointer aligned address (we could check it but it isn't worth it)
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetArrayDataReference(result)), ref data[offset], (uint)len);
#elif NETCOREAPP3_0_OR_GREATER
            // must use unaligned because data[offset] is not necessarily a pointer aligned address (we could check it but it isn't worth it)
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref result[0]), ref data[offset], (uint)len);
#else
            fixed (void* dst = result)
                Marshal.Copy(data, offset, new IntPtr(dst), len);
#endif

            return result;
        }

        /// <summary>
        /// Creates a formatter that can be used for serialization and deserialization with given <paramref name="options"/>.
        /// </summary>
        /// <returns>A <see cref="BinarySerializationFormatter"/> instance that can be used for serialization and deserialization with given <paramref name="options"/>.</returns>
        /// <param name="options">Options for the created formatter. This parameter is optional.
        /// <br/>Default value: <see cref="BinarySerializationOptions.CompactSerializationOfStructures"/>.</param>
        public static BinarySerializationFormatter CreateFormatter(BinarySerializationOptions options = DefaultSerializationOptions) => new BinarySerializationFormatter(options);

        #endregion

        #region Private Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static byte[] SerializeValueTypeRaw(ValueType obj)
        {
            Debug.Assert(!obj.GetType().IsManaged(), "Unmanaged type expected");

#if NETSTANDARD2_0
            // .NET Standard 2.0: cannot use GetRawData so calling the generic version by reflection
            Type type = obj.GetType();
            return (byte[])typeof(BinarySerializer).InvokeMethod(nameof(SerializeValueType), type, type.MakeByRefType(), obj)!;
#else
            int len = obj.GetType().SizeOf();
            byte[] result = new byte[len];

#if NET5_0_OR_GREATER
            Unsafe.CopyBlock(ref MemoryMarshal.GetArrayDataReference(result), ref Reflector.GetRawData(obj), (uint)len);
#elif NETCOREAPP3_0_OR_GREATER
            Unsafe.CopyBlock(ref result[0], ref Reflector.GetRawData(obj), (uint)len);
#else
            unsafe
            {
                fixed (byte* pinnedRawData = &Reflector.GetRawData(obj))
                    Marshal.Copy((IntPtr)pinnedRawData, result, 0, len);
            }
#endif

            return result;
#endif
        }

        [SecurityCritical]
        private static object DeserializeValueTypeRaw(Type type, byte[] data, int offset)
        {
            Debug.Assert(!type.IsManaged(), "Unmanaged type expected");
            Debug.Assert(offset >= 0);

#if NETSTANDARD2_0
            // .NET Standard 2.0: cannot use GetRawData so calling the generic version by reflection
            return typeof(BinarySerializer).InvokeMethod(nameof(DeserializeValueType), type, new[] { typeof(byte[]), typeof(int) }, data, offset)!;
#else
            int len = type.SizeOf();
            if (offset + len > data.Length)
                Throw.ArgumentException(Argument.data, Res.BinarySerializationDataLengthTooSmall);

            // For structs Activator is faster than obtaining a CreateInstanceAccessor by type and invoking it.
            // Note: not an issue that possible default constructor is not executed in .NET Framework because the whole structure is overwritten
            object result = Activator.CreateInstance(type)!;

#if NETCOREAPP3_0_OR_GREATER
            ref byte src = ref data[offset];
            ref byte dst = ref Reflector.GetRawData(result);
            
            // must use unaligned because data[offset] is not necessarily a pointer aligned address (we could check it but it isn't worth it)
            Unsafe.CopyBlockUnaligned(ref dst, ref src, (uint)len);
#else
            unsafe
            {
                fixed (byte* pinnedRawData = &Reflector.GetRawData(result))
                    Marshal.Copy(data, offset, (IntPtr)pinnedRawData, len);
            }
#endif

            return result;
#endif
        }

        #endregion

        #endregion
    }
}
