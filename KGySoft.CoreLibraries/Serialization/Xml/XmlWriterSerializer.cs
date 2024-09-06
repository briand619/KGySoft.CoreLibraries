﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: XmlWriterSerializer.cs
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using KGySoft.CoreLibraries;
using KGySoft.Reflection;
using KGySoft.Security.Cryptography;
using KGySoft.Serialization.Binary;

#endregion

namespace KGySoft.Serialization.Xml
{
    internal class XmlWriterSerializer : XmlSerializerBase
    {
        #region SerializeObjectContext Struct

        private struct SerializeObjectContext
        {
            #region Fields

            internal object Object;
            internal Type Type;
            internal XmlWriter Writer;
            internal bool TypeNeeded;
            internal bool IsReadOnlyProperty;
            internal DesignerSerializationVisibility Visibility;

            #endregion
        }

        #endregion

        #region Constructors

        public XmlWriterSerializer(XmlSerializationOptions options) : base(options)
        {
        }

        #endregion

        #region Methods

        #region Static Methods

        /// <summary>
        /// Writer must be in parent element, which should be closed by the parent.
        /// </summary>
        private static void SerializeXmlSerializable(IXmlSerializable obj, XmlWriter writer)
        {
            writer.WriteAttributeString(XmlSerializer.AttributeFormat, XmlSerializer.AttributeValueCustom);

            Type objType = obj.GetType();
            string? contentName = null;
            object[] attrs = objType.GetCustomAttributes(typeof(XmlRootAttribute), true);
            if (attrs.Length > 0)
                contentName = ((XmlRootAttribute)attrs[0]).ElementName;

            if (String.IsNullOrEmpty(contentName))
                contentName = objType.Name;

            writer.WriteStartElement(contentName);
            obj.WriteXml(writer);
            writer.WriteFullEndElement();
        }

        #endregion

        #region Instance Methods

        #region Internal Methods

        internal void Serialize(XmlWriter writer, object? obj)
        {
            if (writer == null!)
                Throw.ArgumentNullException(Argument.writer);

            writer.WriteStartElement(XmlSerializer.ElementObject);
            if (obj == null)
            {
                writer.WriteEndElement();
                writer.Flush();
                return;
            }

            SerializeObject(obj, true, writer, DesignerSerializationVisibility.Visible);
            writer.WriteFullEndElement();
            writer.Flush();
        }

        internal void SerializeContent(XmlWriter writer, object obj)
        {
            if (obj == null!)
                Throw.ArgumentNullException(Argument.obj);
            if (writer == null!)
                Throw.ArgumentNullException(Argument.writer);
            Type objType = obj.GetType();
            try
            {
                RegisterSerializedObject(obj);

                // 1.) IXmlSerializable
                if (obj is IXmlSerializable xmlSerializable && ProcessXmlSerializable)
                {
                    SerializeXmlSerializable(xmlSerializable, writer);
                    return;
                }

                // 2.) Collection
                if (obj is IEnumerable enumerable)
                {
                    if (!objType.IsCollection())
                        Throw.NotSupportedException(Res.XmlSerializationSerializingNonPopulatableCollectionNotSupported(objType));
                    if (!ForceReadonlyMembersAndCollections && !objType.IsReadWriteCollection(obj))
                        Throw.NotSupportedException(Res.XmlSerializationSerializingReadOnlyCollectionNotSupported(objType));

                    SerializeCollection(enumerable, objType.GetCollectionElementType()!, false, writer, DesignerSerializationVisibility.Visible, ComparerType.None);
                    return;
                }

                // 3.) Any object
                SerializeMembers(obj, writer, DesignerSerializationVisibility.Visible);
            }
            finally
            {
                UnregisterSerializedObject(obj);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Serializing a collection by XmlWriter
        /// </summary>
        private void SerializeCollection(IEnumerable? collection, Type elementType, bool typeNeeded, XmlWriter writer, DesignerSerializationVisibility visibility, ComparerType comparer)
        {
            if (collection == null)
                return;

            // array collection
            if (collection is Array array)
            {
                if (typeNeeded)
                    writer.WriteAttributeString(XmlSerializer.AttributeType, GetTypeString(collection.GetType()));

                // multidimensional or nonzero-based array
                if (array.Rank > 1 || array.GetLowerBound(0) != 0)
                {
                    StringBuilder dim = new StringBuilder();
                    for (int i = 0; i < array.Rank; i++)
                    {
                        int low;
                        if ((low = array.GetLowerBound(i)) != 0)
                            dim.Append(low + ".." + (low + array.GetLength(i) - 1));
                        else
                            dim.Append(array.GetLength(i));

                        if (i < array.Rank - 1)
                            dim.Append(',');
                    }

                    writer.WriteAttributeString(XmlSerializer.AttributeDim, dim.ToString());
                }
                else
                    writer.WriteAttributeString(XmlSerializer.AttributeLength, array.Length.ToString(CultureInfo.InvariantCulture));

                if (array.Length == 0)
                {
                    // signing that collection is not null - now it will be at least <Collection></Collection> instead of <Collection />
                    writer.WriteString(String.Empty);
                    return;
                }

                // array of a primitive type
                if (elementType.IsPrimitive && (Options & XmlSerializationOptions.CompactSerializationOfPrimitiveArrays) != XmlSerializationOptions.None)
                {
                    byte[] data = new byte[Buffer.ByteLength(array)];
                    Buffer.BlockCopy(array, 0, data, 0, data.Length);
                    if ((Options & XmlSerializationOptions.OmitCrcAttribute) == XmlSerializationOptions.None)
                        writer.WriteAttributeString(XmlSerializer.AttributeCrc, Crc32.CalculateHash(data).ToString("X8", CultureInfo.InvariantCulture));
                    writer.WriteString(Convert.ToBase64String(data));

                    return;
                }

                // non-primitive type array or compact serialization is not enabled
                if (elementType.IsPointer)
                    Throw.NotSupportedException(Res.SerializationPointerArrayTypeNotSupported(collection.GetType()));
                foreach (object? item in array)
                {
                    writer.WriteStartElement(XmlSerializer.ElementItem);
                    if (item == null)
                        writer.WriteEndElement();
                    else
                    {
                        SerializeObject(item, !elementType.IsSealed && item.GetType() != elementType, writer, visibility);
                        writer.WriteFullEndElement();
                    }
                }

                return;
            }

            // non-array collection
            if (typeNeeded)
                writer.WriteAttributeString(XmlSerializer.AttributeType, GetTypeString(collection.GetType()));

            if (comparer > ComparerType.None)
                writer.WriteAttributeString(XmlSerializer.AttributeComparer, Enum<ComparerType>.ToString(comparer));

            if (collection is OrderedDictionary { IsReadOnly: true })
                writer.WriteAttributeString(XmlSerializer.AttributeReadOnly, Boolean.TrueString);

            // serializing main properties first
            SerializeMembers(collection, writer, visibility);

            // serializing items
            foreach (object? item in collection)
            {
                writer.WriteStartElement(XmlSerializer.ElementItem);
                if (item == null)
                    writer.WriteEndElement();
                else
                {
                    SerializeObject(item, !elementType.IsSealed && item.GetType() != elementType, writer, visibility);
                    writer.WriteFullEndElement();
                }
            }
        }

        /// <summary>
        /// Serializes a whole object. May throw exceptions on invalid or inappropriate options.
        /// XmlWriter version. Start element must be opened and closed by caller.
        /// obj.GetType and type can be different (properties)
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "False alarm, the new analyzer includes the complexity of local methods.")]
        private void SerializeObject(object? obj, bool typeNeeded, XmlWriter writer, DesignerSerializationVisibility visibility, bool isReadOnlyProperty = false)
        {
            #region Local Methods to reduce complexity

            bool TrySerializeKeyValue(ref SerializeObjectContext ctx)
            {
                // 1.) KeyValue 1: DictionaryEntry: can be serialized recursively. Just handling to avoid binary serialization.
                if (ctx.Type == Reflector.DictionaryEntryType)
                {
                    if (ctx.TypeNeeded)
                        ctx.Writer.WriteAttributeString(XmlSerializer.AttributeType, GetTypeString(ctx.Type));

                    SerializeMembers(ctx.Object, ctx.Writer, ctx.Visibility);
                    return true;
                }

                // 2.) KeyValue 2: KeyValuePair: properties are read-only so special support needed
                if (ctx.Type.IsGenericTypeOf(Reflector.KeyValuePairType))
                {
                    if (ctx.TypeNeeded)
                        ctx.Writer.WriteAttributeString(XmlSerializer.AttributeType, GetTypeString(ctx.Type));

                    object? key = Accessors.GetPropertyValue(ctx.Object, nameof(KeyValuePair<_, _>.Key));
                    object? value = Accessors.GetPropertyValue(ctx.Object, nameof(KeyValuePair<_, _>.Value));
                    Type[] genericArgs = ctx.Type.GetGenericArguments();

                    ctx.Writer.WriteStartElement(nameof(KeyValuePair<_,_>.Key));
                    if (key == null)
                        ctx.Writer.WriteEndElement();
                    else
                    {
                        SerializeObject(key, !genericArgs[0].IsSealed && genericArgs[0] != key.GetType(), ctx.Writer, ctx.Visibility);
                        ctx.Writer.WriteFullEndElement();
                    }

                    ctx.Writer.WriteStartElement(nameof(KeyValuePair<_,_>.Value));
                    if (value == null)
                        ctx.Writer.WriteEndElement();
                    else
                    {
                        SerializeObject(value, !genericArgs[1].IsSealed && genericArgs[1] != value.GetType(), ctx.Writer, ctx.Visibility);
                        ctx.Writer.WriteFullEndElement();
                    }

                    return true;
                }

                return false;
            }

            bool TrySerializeComplexObject(ref SerializeObjectContext ctx)
            {
                // 1.) collection: if can be trusted in all circumstances
                if (ctx.Object is IEnumerable enumerable)
                {
                    Type? elementType = null;
                    ComparerType? comparer = ComparerType.None;

                    // if can be trusted in all circumstances
                    if (IsTrustedCollection(ctx.Type)
                        // or deserialization can only use the pre-created known collection (so the comparer can be ignored)
                        || ctx.IsReadOnlyProperty && IsKnownCollection(ctx.Type)
                        // or the known collection uses a supported comparer
                        || (!ctx.IsReadOnlyProperty && ((comparer = GetComparer(enumerable)) ?? ComparerType.Unknown) != ComparerType.Unknown)
                        // or recursive serialization is requested
                        || ((ctx.Visibility == DesignerSerializationVisibility.Content || RecursiveSerializationAsFallback)
                            // and is a supported collection or serialization is forced
                            && (ForceReadonlyMembersAndCollections || ctx.Type.IsSupportedCollectionForReflection(out var _, out var _, out elementType, out var _))))
                    {
                        SerializeCollection(enumerable, elementType ?? ctx.Type.GetCollectionElementType()!, ctx.TypeNeeded, ctx.Writer, ctx.Visibility, comparer.GetValueOrDefault());
                        return true;
                    }

                    if (ctx.Visibility == DesignerSerializationVisibility.Content || RecursiveSerializationAsFallback)
                        Throw.SerializationException(Res.XmlSerializationCannotSerializeUnsupportedCollection(ctx.Type, Options));
                    Throw.SerializationException(Res.XmlSerializationCannotSerializeCollection(ctx.Type, Options));
                }

                // 2.) recursive serialization of any object, if requested
                if (RecursiveSerializationAsFallback || ctx.Visibility == DesignerSerializationVisibility.Content
                    // or when it has public properties/fields only
                    || IsTrustedType(ctx.Type))
                {
                    if (ctx.TypeNeeded)
                        ctx.Writer.WriteAttributeString(XmlSerializer.AttributeType, GetTypeString(ctx.Type));

                    SerializeMembers(ctx.Object, ctx.Writer, ctx.Visibility);
                    return true;
                }

                return false;
            }

            #endregion

            if (obj == null)
                return;

            Type type = obj.GetType();

            // a.) If type can be natively parsed, simple writing
            if (type.CanBeParsedNatively())
            {
                if (typeNeeded)
                    writer.WriteAttributeString(XmlSerializer.AttributeType, GetTypeString(type));

                WriteStringValue(obj, writer);
                return;
            }

            // b.) IXmlSerializable
            if (obj is IXmlSerializable xmlSerializable && ProcessXmlSerializable)
            {
                if (typeNeeded)
                    writer.WriteAttributeString(XmlSerializer.AttributeType, GetTypeString(type));

                SerializeXmlSerializable(xmlSerializable, writer);
                return;
            }

            // c.) Using type converter of the type if applicable
            TypeConverter converter = TypeDescriptor.GetConverter(type);
            if (converter.CanConvertTo(Reflector.StringType) && converter.CanConvertFrom(Reflector.StringType))
            {
                if (typeNeeded)
                    writer.WriteAttributeString(XmlSerializer.AttributeType, GetTypeString(type));
                WriteStringValue(converter.ConvertToInvariantString(obj), writer);
                return;
            }

            var context = new SerializeObjectContext
            {
                Object = obj,
                Type = type,
                TypeNeeded = typeNeeded,
                Writer = writer,
                Visibility = visibility,
                IsReadOnlyProperty = isReadOnlyProperty
            };

            // d.) Key/Value
            if (TrySerializeKeyValue(ref context))
                return;

            // e.) value type as binary only if enabled
            if (type.IsValueType && CompactSerializationOfStructures && BinarySerializer.TrySerializeValueType((ValueType)obj, out byte[]? data))
            {
                if (typeNeeded)
                    writer.WriteAttributeString(XmlSerializer.AttributeType, GetTypeString(type));

                writer.WriteAttributeString(XmlSerializer.AttributeFormat, XmlSerializer.AttributeValueStructBinary);
                if ((Options & XmlSerializationOptions.OmitCrcAttribute) == XmlSerializationOptions.None)
                    writer.WriteAttributeString(XmlSerializer.AttributeCrc, Crc32.CalculateHash(data).ToString("X8", CultureInfo.InvariantCulture));
                writer.WriteString(Convert.ToBase64String(data));
                return;
            }

            // f.) binary serialization: base64 format to XML
            if (BinarySerializationAsFallback && visibility != DesignerSerializationVisibility.Content)
            {
                try
                {
                    SerializeBinary(obj, writer);
                    return;
                }
                catch (Exception e)
                {
                    throw new SerializationException(Res.XmlSerializationBinarySerializationFailed(obj.GetType(), Options, e.Message), e);
                }
            }

            RegisterSerializedObject(obj);
            try
            {
                // g.) collections and other complex objects
                if (TrySerializeComplexObject(ref context))
                    return;
            }
            finally
            {
                UnregisterSerializedObject(obj);
            }

            Throw.SerializationException(Res.XmlSerializationSerializingTypeNotSupported(type, Options));
        }

        private void SerializeMembers(object obj, XmlWriter writer, DesignerSerializationVisibility parentVisibility)
        {
            foreach (Member member in GetMembersToSerialize(obj))
            {
                DesignerSerializationVisibility visibility = parentVisibility;
                if (SkipMember(obj, member.MemberInfo, out object? value, ref visibility))
                    continue;

                PropertyInfo? property = member.Property;
                FieldInfo? field = member.Field;
                Type memberType = property != null ? property.PropertyType : field!.FieldType;
                memberType = Nullable.GetUnderlyingType(memberType) ?? memberType;
                if (memberType.IsByRef)
                    memberType = memberType.GetElementType()!;

                Type actualType = value?.GetType() ?? memberType;

                // a.) Using explicitly defined type converter if can convert to and from string
                // Note: ResolveType can load assemblies here. When serializing, it is not a problem since the serialized object tree is always under the consumer's control.
                Attribute[] attrs = Reflector.GetAttributes(member.MemberInfo, typeof(TypeConverterAttribute), true);
                if (attrs.Length > 0 && attrs[0] is TypeConverterAttribute convAttr && Reflector.ResolveType(convAttr.ConverterTypeName) is Type convType)
                {
                    ConstructorInfo? ctor = convType.GetConstructor(new Type[] { Reflector.Type });
                    object[] ctorParams = { memberType };
                    if (ctor == null)
                    {
                        ctor = convType.GetDefaultConstructor();
                        ctorParams = Reflector.EmptyObjects;
                    }

                    if (ctor != null)
                    {
                        if (CreateInstanceAccessor.GetAccessor(ctor).CreateInstance(ctorParams) is TypeConverter converter
                            && converter.CanConvertTo(Reflector.StringType) && converter.CanConvertFrom(Reflector.StringType))
                        {
                            writer.WriteStartElement(member.MemberInfo.Name);
                            if (member.SpecifyDeclaringType)
                                writer.WriteAttributeString(XmlSerializer.AttributeDeclaringType, GetTypeString(member.MemberInfo.DeclaringType!));

                            if (value != null)
                                WriteStringValue(converter.ConvertToInvariantString(value), writer);
                            writer.WriteEndElement();
                            continue;
                        }
                    }
                }

                // b.) any object
                writer.WriteStartElement(member.MemberInfo.Name);
                if (member.SpecifyDeclaringType)
                    writer.WriteAttributeString(XmlSerializer.AttributeDeclaringType, GetTypeString(member.MemberInfo.DeclaringType!));

                if (value == null)
                    writer.WriteEndElement();
                else
                {
                    SerializeObject(value, memberType != actualType, writer, visibility, property?.CanWrite == false);
                    writer.WriteFullEndElement();
                }
            }
        }

        /// <summary>
        /// Serializing binary content by XmlWriter
        /// </summary>
        private void SerializeBinary(object? obj, XmlWriter writer)
        {
            writer.WriteAttributeString(XmlSerializer.AttributeFormat, XmlSerializer.AttributeValueBinary);

            if (obj == null)
                return;

            BinarySerializationOptions binSerOptions = GetBinarySerializationOptions();
            byte[] data = BinarySerializer.Serialize(obj, binSerOptions);

            if ((Options & XmlSerializationOptions.OmitCrcAttribute) == XmlSerializationOptions.None)
                writer.WriteAttributeString(XmlSerializer.AttributeCrc, Crc32.CalculateHash(data).ToString("X8", CultureInfo.InvariantCulture));
            writer.WriteString(Convert.ToBase64String(data));
        }

        private void WriteStringValue(object? obj, XmlWriter writer)
        {
            string? s = GetStringValue(obj, out bool spacePreserved, out bool escaped);
            if (s == null)
                return;
            if (spacePreserved)
                writer.WriteAttributeString(XmlSerializer.NamespaceXml, XmlSerializer.AttributeSpace, null, XmlSerializer.AttributeValuePreserve);
            if (escaped)
                writer.WriteAttributeString(XmlSerializer.AttributeEscaped, XmlSerializer.AttributeValueTrue);

            writer.WriteString(s);
        }

        #endregion

        #endregion

        #endregion
    }
}
