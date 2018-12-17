﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Res.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2017 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using KGySoft.Annotations;
using KGySoft.CoreLibraries;
using KGySoft.Reflection;
using KGySoft.Resources;
using KGySoft.Serialization;

#endregion

namespace KGySoft
{
    /// <summary>
    /// Contains the IDs of string resources.
    /// </summary>
    internal static class Res
    {
        #region Constants

        private const string unavailableResource = "Resource ID not found: {0}";
        private const string invalidResource = "Resource text is not valid for {0} arguments: {1}";

        #endregion

        #region Fields

        private static readonly DynamicResourceManager resourceManager = new DynamicResourceManager("KGySoft.CoreLibraries.Messages", Reflector.KGySoftLibrariesAssembly);

        #endregion

        #region Properties

        #region Private Properties

        private static string QuoteStart => Get("General_QuoteStart");
        private static string QuoteEnd => Get("General_QuoteEnd");

        #endregion

        #region Internal Properties

        #region General

        /// <summary>&lt;undefined&gt;</summary>
        internal static string Undefined => Get("General_Undefined");

        /// <summary>&lt;null&gt;</summary>
        internal static string NullReference => Get("General_NullReference");

        /// <summary>Value cannot be null.</summary>
        internal static string ArgumentNull => Get("General_ArgumentNull");

        /// <summary>Value cannot be empty.</summary>
        internal static string ArgumentEmpty => Get("General_ArgumentEmpty");

        /// <summary>The collection contains no elements.</summary>
        internal static string CollectionEmpty => Get("General_CollectionEmpty");

        /// <summary>Specified argument contains a null element.</summary>
        internal static string ArgumentContainsNull => Get("General_ArgumentContainsNull");

        /// <summary>Specified argument was out of the range of valid values.</summary>
        internal static string ArgumentOutOfRange => Get("General_ArgumentOutOfRange");

        /// <summary>Cannot access a disposed object.</summary>
        internal static string ObjectDisposed => Get("General_ObjectDisposed");

        /// <summary>This operation is not supported.</summary>
        internal static string NotSupported => Get("General_NotSupported");

        /// <summary>Input string contains an invalid value.</summary>
        internal static string ArgumentInvalidString => Get("General_ArgumentInvalidString");

        /// <summary>Maximum value must be greater than or equal to minimum value.</summary>
        internal static string MaxValueLessThanMinValue => Get("General_MaxValueLessThanMinValue");

        /// <summary>Maximum length must be greater than or equal to minimum length.</summary>
        internal static string MaxLengthLessThanMinLength => Get("General_MaxLengthLessThanMinLength");

        /// <summary>Enumeration has either not started or has already finished.</summary>
        internal static string IEnumeratorEnumerationNotStartedOrFinished => Get("IEnumerator_EnumerationNotStartedOrFinished");

        /// <summary>Collection was modified; enumeration operation may not execute.</summary>
        internal static string IEnumeratorCollectionModified => Get("IEnumerator_CollectionModified");

        /// <summary>The given key was not present in the dictionary.</summary>
        internal static string IDictionaryKeyNotFound => Get("IDictionary_KeyNotFound");

        /// <summary>An item with the same key has already been added.</summary>
        internal static string IDictionaryDuplicateKey => Get("IDictionary_DuplicateKey");

        /// <summary>Destination array is not long enough to copy all the items in the collection. Check array index and length.</summary>
        internal static string ICollectionCopyToDestArrayShort => Get("ICollection_CopyToDestArrayShort");

        /// <summary>Only single dimensional arrays are supported for the requested action.</summary>
        internal static string ICollectionCopyToSingleDimArrayOnly => Get("ICollection_CopyToSingleDimArrayOnly");

        /// <summary>Target array type is not compatible with the type of items in the collection.</summary>
        internal static string ICollectionArrayTypeInvalid => Get("ICollection_ArrayTypeInvalid");

        /// <summary>Modifying a read-only collection is not supported.</summary>
        internal static string ICollectionReadOnlyModifyNotSupported => Get("ICollection_ReadOnlyModifyNotSupported");

        #endregion

        #region BinarySerialization

        /// <summary>Invalid stream data.</summary>
        internal static string BinarySerializationInvalidStreamData => Get("BinarySerialization_InvalidStreamData");

        /// <summary>Deserialization of an IObjectReference instance has a circular reference to itself.</summary>
        internal static string BinarySerializationCircularIObjectReference => Get("BinarySerialization_CircularIObjectReference");

        /// <summary>Unexpected id on deserialization. Serialization stream corrupted?</summary>
        internal static string BinarySerializationDeserializeUnexpectedId => Get("BinarySerialization_DeserializeUnexpectedId");

        /// <summary>Specified type must be a value type.</summary>
        internal static string BinarySerializationValueTypeExpected => Get("BinarySerialization_ValueTypeExpected");

        /// <summary>Data length is too small.</summary>
        internal static string BinarySerializationDataLenghtTooSmall => Get("BinarySerialization_DataLenghtTooSmall");

        #endregion

        #region ByteArrayExtensions

        /// <summary>The separator contains invalid characters. Hex digits are not allowed in separator.</summary>
        internal static string ByteArrayExtensionsSeparatorInvalidHex => Get("ByteArrayExtensions_SeparatorInvalidHex");

        /// <summary>The separator is empty or contains invalid characters. Decimal digits are not allowed in separator.</summary>
        internal static string ByteArrayExtensionsSeparatorInvalidDec => Get("ByteArrayExtensions_SeparatorInvalidDec");

        #endregion

        #region Cache<TKey, TValue>

        /// <summary>Cache&lt;TKey, TValue&gt; was initialized without an item loader so elements must be added explicitly either by the Add method or by setting the indexer.</summary>
        internal static string CacheNullLoaderInvoke => Get("Cache_NullLoaderInvoke");

        /// <summary>The given key was not found in the cache.</summary>
        internal static string CacheKeyNotFound => Get("Cache_KeyNotFound");

        /// <summary>Minimum cache size is 1.</summary>
        internal static string CacheMinSize => Get("Cache_MinSize");

        #endregion

        #region CircularList<T>

        /// <summary>Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.</summary>
        internal static string CircularListInvalidOffsLen => Get("CircularList_InvalidOffsLen");

        /// <summary>Failed to compare two elements in the collection.</summary>
        internal static string CircularListComparerFail => Get("CircularList_ComparerFail");

        /// <summary>Capacity cannot be less than number of stored elements.</summary>
        internal static string CircularListCapacityTooSmall => Get("CircularList_CapacityTooSmall");

        #endregion

        #region CircularSortedList<T>

        /// <summary>Adding an element by index is not supported.</summary>
        internal static string CircularSortedListInsertByIndexNotSupported => Get("CircularSortedList_InsertByIndexNotSupported");

        #endregion

        #region ComponentModel

        /// <summary>Cannot add new item to the binding list because AllowNew is false.</summary>
        internal static string ComponentModelAddNewDisabled => Get("ComponentModel_AddNewDisabled");

        /// <summary>Cannot remove item from the binding list because AllowRemove is false.</summary>
        internal static string ComponentModelRemoveDisabled => Get("ComponentModel_RemoveDisabled");

        /// <summary>Cannot change ObservableBindingList during a CollectionChanged or ListChanged event.</summary>
        internal static string ComponentModelReentrancyNotAllowed => Get("ComponentModel_ReentrancyNotAllowed");

        /// <summary>Object is not in editing state.</summary>
        internal static string ComponentModelNotEditing => Get("ComponentModel_NotEditing");

        /// <summary>&lt;Missing property value&gt;</summary>
        internal static string ComponentModelMissingPropertyReference => Get("ComponentModel_MissingPropertyReference");

        /// <summary>The DoValidation method returned null.</summary>
        internal static string ComponentModelDoValidationNull => Get("ComponentModel_DoValidationNull");

        /// <summary>'Enabled' state must have a boolean value.</summary>
        internal static string ComponentModelEnabledMustBeBool => Get("ComponentModel_EnabledMustBeBool");

        #endregion

        #region Enum/EnumComparer

        /// <summary>Type parameter is expected to be a System.Enum type.</summary>
        internal static string EnumTypeParameterInvalid => Get("Enum_TypeParameterInvalid");

        #endregion

        #region Profiler

        /// <summary>&lt;Uncategorized&gt;</summary>
        internal static string ProfilerUncategorized => Get("Profiler_Uncategorized");

        #endregion

        #region Reflection

        /// <summary>MethodInfo or ConstructorInfo expected.</summary>
        internal static string ReflectionInvalidMethodBase => Get("Reflection_InvalidMethodBase");

        /// <summary>Cannot treat method as a property setter.</summary>
        internal static string ReflectionCannotTreatPropertySetter => Get("Reflection_CannotTreatPropertySetter");

        /// <summary>Argument must be either Type or ConstructorInfo.</summary>
        internal static string ReflectionTypeOrCtorInfoExpected => Get("Reflection_TypeOrCtorInfoExpected");

        /// <summary>Getting property via TypeDescriptor is not supported in this overload of GetProperty method.</summary>
        internal static string ReflectionGetPropertyTypeDescriptorNotSupported => Get("Reflection_GetPropertyTypeDescriptorNotSupported");

        /// <summary>Setting property via TypeDescriptor is not supported in this overload of SetProperty method.</summary>
        internal static string ReflectionSetPropertyTypeDescriptorNotSupported => Get("Reflection_SetPropertyTypeDescriptorNotSupported");

        /// <summary>A static property cannot be retrieved via TypeDescriptor.</summary>
        internal static string ReflectionCannotGetStaticPropertyTypeDescriptor => Get("Reflection_CannotGetStaticPropertyTypeDescriptor");

        /// <summary>A static property cannot be set via TypeDescriptor.</summary>
        internal static string ReflectionCannotSetStaticPropertyTypeDescriptor => Get("Reflection_CannotSetStaticPropertyTypeDescriptor");

        /// <summary>Indexer parameters are empty.</summary>
        internal static string ReflectionEmptyIndices => Get("Reflection_EmptyIndices");

        /// <summary>An indexer cannot be retrieved via TypeDescriptor.</summary>
        internal static string ReflectionGetIndexerTypeDescriptorNotSupported => Get("Reflection_GetIndexerTypeDescriptorNotSupported");

        /// <summary>An indexer cannot be set via TypeDescriptor.</summary>
        internal static string ReflectionSetIndexerTypeDescriptorNotSupported => Get("Reflection_SetIndexerTypeDescriptorNotSupported");

        /// <summary>Index parameters cannot be converted to integer values.</summary>
        internal static string ReflectionIndexParamsTypeMismatch => Get("Reflection_IndexParamsTypeMismatch");

        /// <summary>Instance is null for a non-static member.</summary>
        internal static string ReflectionInstanceIsNull => Get("Reflection_InstanceIsNull");

        /// <summary>Method to invoke is generic but no type parameters are passed.</summary>
        internal static string ReflectionTypeParamsAreNull => Get("Reflection_TypeParamsAreNull");

        /// <summary>Could not create generic method. For details see inner exception.</summary>
        internal static string ReflectionCannotCreateGenericMethod => Get("Reflection_CannotCreateGenericMethod");

        /// <summary>Invoking a method via TypeDescriptor is not supported.</summary>
        internal static string ReflectionInvokeMethodTypeDescriptorNotSupported => Get("Reflection_InvokeMethodTypeDescriptorNotSupported");

        /// <summary>A field cannot be set via TypeDescriptor.</summary>
        internal static string ReflectionSetFieldTypeDescriptorNotSupported => Get("Reflection_SetFieldTypeDescriptorNotSupported");

        /// <summary>A field cannot be retrieved via TypeDescriptor.</summary>
        internal static string ReflectionGetFieldTypeDescriptorNotSupported => Get("Reflection_GetFieldTypeDescriptorNotSupported");

        /// <summary>Expression is not a method call.</summary>
        internal static string ReflectionNotAMethod => Get("Reflection_NotAMethod");

        /// <summary>In this ResolveType overload the type name should not contain the assembly name.</summary>
        internal static string ReflectionTypeWithAssemblyName => Get("Reflection_TypeWithAssemblyName");

        /// <summary>DeclaringType of the provided member should not be null.</summary>
        internal static string ReflectionDeclaringTypeExpected => Get("Reflection_DeclaringTypeExpected");

        #endregion

        #region Resources

        /// <summary>Property can be changed only before the enumeration.</summary>
        internal static string ResourcesInvalidResXReaderPropertyChange => Get("Resources_InvalidResXReaderPropertyChange");

        /// <summary>Property can be changed only before adding a row or generating any content.</summary>
        internal static string ResourcesInvalidResXWriterPropertyChange => Get("Resources_InvalidResXWriterPropertyChange");

        /// <summary>Resource writer has already been saved. You may not edit it.</summary>
        internal static string ResourcesWriterSaved => Get("Resources_WriterSaved");

        /// <summary>This operation is invalid when Source is CompiledOnly.</summary>
        internal static string ResourcesHybridResSourceBinary => Get("Resources_HybridResSourceBinary");

        /// <summary>Setting this property is invalid when UseLanguageSettings is true.</summary>
        internal static string ResourcesInvalidDrmPropertyChange => Get("Resources_InvalidDrmPropertyChange");

        #endregion

        #region StreamExtensions

        /// <summary>Source stream cannot be read.</summary>
        internal static string StreamExtensionsStreamCannotRead => Get("StreamExtensions_StreamCannotRead");

        /// <summary>Destination stream cannot be written.</summary>
        internal static string StreamExtensionsStreamCannotWrite => Get("StreamExtensions_StreamCannotWrite");

        /// <summary>Cannot seek to the beginning of the stream.</summary>
        internal static string StreamExtensionsStreamCannotSeek => Get("StreamExtensions_StreamCannotSeek");

        #endregion

        #region StringExtensions

        /// <summary>Separator is null or empty.</summary>
        internal static string StringExtensionsSeparatorNullOrEmpty => Get("StringExtensions_SeparatorNullOrEmpty");

        /// <summary>Source length must consist of even amount of bytes.</summary>
        internal static string StringExtensionsSourceLengthNotEven => Get("StringExtensions_SourceLengthNotEven");

        #endregion

        #region XmlSerialization

        /// <summary>Type of the root element is not specified.</summary>
        internal static string XmlSerializationRootTypeMissing => Get("XmlSerialization_RootTypeMissing");

        /// <summary>Array length or dimensions are not specified.</summary>
        internal static string XmlSerializationArrayNoLength => Get("XmlSerialization_ArrayNoLength");

        /// <summary>Corrupt array data: Bad CRC.</summary>
        internal static string XmlSerializationCrcError => Get("XmlSerialization_CrcError");

        /// <summary>Mixed compact and non-compact array content found.</summary>
        internal static string XmlSerializationMixedArrayFormats => Get("XmlSerialization_MixedArrayFormats");

        /// <summary>Key element not found in key/value pair element.</summary>
        internal static string XmlSerializationKeyValueMissingKey => Get("XmlSerialization_KeyValueMissingKey");

        /// <summary>Value element not found in key/value pair element.</summary>
        internal static string XmlSerializationKeyValueMissingValue => Get("XmlSerialization_KeyValueMissingValue");

        /// <summary>Multiple Key elements occurred in key-value element.</summary>
        internal static string XmlSerializationMultipleKeys => Get("XmlSerialization_MultipleKeys");

        /// <summary>Multiple Value elements occurred in key-value element.</summary>
        internal static string XmlSerializationMultipleValues => Get("XmlSerialization_MultipleValues");

        /// <summary>Unexpected end of XML content.</summary>
        internal static string XmlSerializationUnexpectedEnd => Get("XmlSerialization_UnexpectedEnd");

        #endregion

        #endregion

        #endregion

        #region Methods

        #region Internal Methods

        #region General

        /// <summary>Specified argument must be greater or equal than {0}.</summary>
        internal static string ArgumentMustBeGreaterOrEqualThan(object limit) => Get("General_ArgumentMustBeGreaterOrEqualThanFormat", limit);

        /// <summary>Specified argument must be between {0} and {1}.</summary>
        internal static string ArgumentMustBeBetween(object low, object high) => Get("General_ArgumentMustBeBetweenFormat", low, high);

        /// <summary>Enum instance of '{0}' type must be one of the following values: {1}.</summary>
        internal static string EnumOutOfRange<TEnum>(TEnum value) where TEnum : struct, IConvertible => Get("General_EnumOutOfRangeFormat", value.GetType().Name, FormatValues<TEnum>());

        /// <summary>Enum instance of '{0}' type must consist of the following flags: {1}.</summary>
        internal static string FlagsEnumOutOfRange<TEnum>(TEnum value) where TEnum : struct, IConvertible => Get("General_EnumFlagsOutOfRangeFormat", value.GetType().Name, FormatFlags<TEnum>());

        /// <summary>Specified argument is expected to be an instance of type {0}.</summary>
        internal static string NotAnInstanceOfType(Type type) => Get("General_NotAnInstanceOfTypeFormat", type);

        /// <summary>Value "{0}" contains illegal path characters.</summary>
        internal static string ValueContainsIllegalPathCharacters(string path) => Get("General_ValueContainsIllegalPathCharactersFormat", path);

        /// <summary>The value "{0}" is not of type "{1}" and cannot be used in this generic collection.</summary>
        internal static string ICollectionNongenericValueTypeInvalid(object value, Type type) => Get("ICollection_NongenericValueTypeInvalidFormat", value, type);

        /// <summary>The key "{0}" is not of type "{1}" and cannot be used in this generic collection.</summary>
        internal static string IDictionaryNongenericKeyTypeInvalid(object key, Type type) => Get("Collection_NongenericKeyTypeInvalidFormat", key, type);

        #endregion

        #region BinarySerialization

        /// <summary>Serialization of type {0} is not supported with following serialization options: {1}. Try to enable RecursiveSerializationAsFallback flag.</summary>
        internal static string BinarySerializationNotSupported(Type type, BinarySerializationOptions options) => Get("BinarySerialization_NotSupportedFormat", type, options.ToString<BinarySerializationOptions>());

        /// <summary>An IEnumerable type expected but {0} found during deserialization.</summary>
        internal static string BinarySerializationIEnumerableExpected(Type type) => Get("BinarySerialization_IEnumerableExpectedFormat", type);

        /// <summary>Invalid enum base type: {0}. Serialization stream corrupted?</summary>
        internal static string BinarySerializationInvalidEnumBase(string dataType) => Get("BinarySerialization_InvalidEnumBaseFormat", dataType);

        /// <summary>Cannot deserialize as standalone object: {0}</summary>
        internal static string BinarySerializationCannotDeserializeObject(string dataType) => Get("BinarySerialization_CannotDeserializeObjectFormat", dataType);

        /// <summary>Type "{0}" cannot be deserialized because its type hierarchy has been changed since serialization. Use IgnoreObjectChanges option to suppress this exception.</summary>
        internal static string BinarySerializationObjectHierarchyChanged(Type type) => Get("BinarySerialization_ObjectHierarchyChangedFormat", type);

        /// <summary>Type "{0}" cannot be deserialized because it has no field "{1}". Use IgnoreObjectChanges option to suppress this exception.</summary>
        internal static string BinarySerializationMissingField(Type type, string field) => Get("BinarySerialization_MissingFieldFormat", type, field);

        /// <summary>Type "{0}" cannot be deserialized because field "{1}" not found in type "{2}". Use IgnoreObjectChanges option to suppress this exception.</summary>
        internal static string BinarySerializationMissingFieldBase(Type type, string field, Type baseType) => Get("BinarySerialization_MissingFieldBaseFormat", type, field, baseType);

        /// <summary>Type "{0}" does not have a special constructor to deserialize it as ISerializable</summary>
        internal static string BinarySerializationMissingISerializableCtor(Type type) => Get("BinarySerialization_MissingISerializableCtorFormat", type);

        /// <summary>The serialization surrogate has changed the reference of the result object, which is not supported. Object type: {0}</summary>
        internal static string BinarySerializationSurrogateChangedObject(Type type) => Get("BinarySerialization_SurrogateChangedObjectFormat", type);

        /// <summary>Could not decode data type: {0}. Serialization stream corrupted?</summary>
        internal static string BinarySerializationCannotDecodeDataType(string dataType) => Get("BinarySerialization_CannotDecodeDataTypeFormat", dataType);

        /// <summary>Could not decode collection type: {0}. Serialization stream corrupted?</summary>
        internal static string BinarySerializationCannotDecodeCollectionType(string dataType) => Get("BinarySerialization_CannotDecodeCollectionTypeFormat", dataType);

        /// <summary>Creating read-only collection of type "{0}" is not supported. Serialization stream corrupted?</summary>
        internal static string BinarySerializationReadOnlyCollectionNotSupported(string dataType) => Get("BinarySerialization_ReadOnlyCollectionNotSupportedFormat", dataType);

        /// <summary>Could not resolve type name "{0}".</summary>
        internal static string BinarySerializationCannotResolveType(string dataType) => Get("BinarySerialization_CannotResolveTypeFormat", dataType);

        /// <summary>Could not resolve type "{0}" in assembly "{1}".</summary>
        internal static string BinarySerializationCannotResolveTypeInAssembly(string typeName, string asmName) => Get("BinarySerialization_CannotResolveTypeInAssemblyFormat", typeName, asmName);

        /// <summary>Unexpected element in serialization info: {0}. Maybe the instance was not serialized by NameInvariantSurrogateSelector.</summary>
        internal static string BinarySerializationUnexpectedSerializationInfoElement(string elementName) => Get("BinarySerialization_UnexpectedSerializationInfoElementFormat", elementName);

        /// <summary>Object hierarchy has been changed since serialization of type "{0}".</summary>
        internal static string BinarySerializationObjectHierarchyChangedSurrogate(Type type) => Get("BinarySerialization_ObjectHierarchyChangedSurrogateFormat", type);

        /// <summary>Number of serializable fields in type "{0}" has been decreased since serialization so cannot deserialize type "{1}".</summary>
        internal static string BinarySerializationMissingFieldSurrogate(Type baseType, Type type) => Get("BinarySerialization_MissingFieldSurrogateFormat", baseType, type);

        /// <summary>Fields might have been reordered since serialization. Cannot deserialize type "{0}" because cannot assign value "{1}" to field "{2}.{3}".</summary>
        internal static string BinarySerializationUnexpectedFieldType(Type type, object value, Type declaringType, string fieldName) => Get("BinarySerialization_UnexpectedFieldTypeFormat", type, value, declaringType, fieldName);

        #endregion

        #region Cache<TKey, TValue>

        /// <summary>Cache&lt;{0}, {1}&gt; cache statistics:
        /// <br/>Count: {2}
        /// <br/>Capacity: {3}
        /// <br/>Number of writes: {4}
        /// <br/>Number of reads: {5}
        /// <br/>Number of cache hits: {6}
        /// <br/>Number of deletes: {7}
        /// <br/>Hit rate: {8:P2}</summary>
        internal static string CacheStatistics(string keyName, string valueName, int count, int capacity, int writes, int reads, int hits, int deletes, float rate) => Get("Cache_StatisticsFormat", keyName, valueName, count, capacity, writes, reads, hits, deletes, rate);

        #endregion

        #region CircularSortedList<T>

        /// <summary>Type of value should be either {0} or DictionaryEntry.</summary>
        internal static string CircularSortedListInvalidKeyValueType(Type type) => Get("CircularSortedList_InvalidKeyValueTypeFormat", type);

        #endregion

        #region ComponentModel

        /// <summary>Property '{0}' of descriptor type '{1}' does not belong to type '{2}'.</summary>
        internal static string ComponentModelInvalidProperty(PropertyDescriptor property, Type t) => Get("ComponentModel_InvalidPropertyFormat", property.Name, property.GetType(), t);

        /// <summary>Cannot add new item to the binding list because type '{0}' cannot be constructed without parameters. Subscribe the AddingNew event or override the AddNewCore or OnAddingNew methods to create a new item to add.</summary>
        internal static string ComponentModelCannotAddNewFastBindingList(Type t) => Get("ComponentModel_CannotAddNewFormatFastBindingListFormat", t);

        /// <summary>No property descriptor found for property name '{0}' in type '{1}'.</summary>
        internal static string ComponentModelPropertyNotExists(string propertyName, Type type) => Get("ComponentModel_PropertyNotExistsFormat", propertyName, type);

        /// <summary>Cannot add new item to the binding list because type '{0}' cannot be constructed without parameters.</summary>
        internal static string ComponentModelCannotAddNewObservableBindingList(Type t) => Get("ComponentModel_CannotAddNewObservableBindingListFormat", t);

        /// <summary>The property binding command state does not contain the expected entry '{0}'.</summary>
        internal static string ComponentModelMissingState(string stateName) => Get("ComponentModel_MissingStateFormat", stateName);

        /// <summary>There is no event '{0}' in type '{1}'.</summary>
        internal static string ComponentModelMissingEvent(string eventName, Type type) => Get("ComponentModel_MissingEventFormat", eventName, type);

        /// <summary>Event '{0}' does not have regular event handler delegate type.</summary>
        internal static string ComponentModelInvalidEvent(string eventName) => Get("ComponentModel_InvalidEventFormat", eventName);

        /// <summary>Cannot get property '{0}'.</summary>
        internal static string ComponentModelCannotGetProperty(string propertyName) => Get("ComponentModel_CannotGetPropertyFormat", propertyName);

        /// <summary>Cannot set property '{0}'.</summary>
        internal static string ComponentModelCannotSetProperty(string propertyName) => Get("ComponentModel_CannotSetPropertyFormat", propertyName);

        /// <summary>The returned value is not compatible with type {0}</summary>
        internal static string ComponentModelReturnedTypeInvalid(Type type) => Get("ComponentModel_ReturnedTypeInvalidFormat", type);

        /// <summary>No value exists for property '{0}'.</summary>
        internal static string ComponentModelPropertyValueNotExist(string propertyName) => Get("ComponentModel_PropertyValueNotExistFormat", propertyName);

        /// <summary>The type has no parameterless constructor and thus cannot be cloned: {0}</summary>
        internal static string ComponentModelObservableObjectHasNoDefaultCtor(Type type) => Get("ComponentModel_ObservableObjectHasNoDefaultCtorFormat", type);

        #endregion

        #region Enum

        /// <summary>Value '{0}' cannot be parsed as enumeration type {1}</summary>
        internal static string EnumValueCannotBeParsedAsEnum(string value, Type enumType) => Get("Enum_ValueCannotBeParsedAsEnumFormat", value, enumType);

        #endregion

        #region EnumerableExtensions

        /// <summary>Cannot add element to type {0} because it implements neither IList nor ICollection&lt;T&gt; interfaces.</summary>
        internal static string EnumerableExtensionsCannotAdd(Type type) => Get("EnumerableExtensions_CannotAddFormat", type);

        /// <summary>Cannot clear items of type {0} because it implements neither IList nor ICollection&lt;T&gt; interfaces.</summary>
        internal static string EnumerableExtensionsCannotClear(Type type) => Get("EnumerableExtensions_CannotClearFormat", type);

        #endregion

        #region ObjectExtensions

        /// <summary>The specified argument cannot be converted to type {0}.</summary>
        internal static string ObjectExtensionsCannotConvertToType(Type type) => Get("ObjectExtensions_CannotConvertToTypeFormat", type);

        #endregion

        #region Reflection

        /// <summary>The constant field cannot be set: {0}.{1}</summary>
        internal static string ReflectionCannotSetConstantField(Type type, string memberName) => Get("Reflection_CannotSetConstantFieldFormat", type, memberName);

        /// <summary>Member type {0} is not supported.</summary>
        internal static string ReflectionNotSupportedMemberType(MemberTypes memberType) => Get("Reflection_NotSupportedMemberTypeFormat", memberType);

        /// <summary>Property has no getter accessor: {0}.{1}</summary>
        internal static string ReflectionPropertyHasNoGetter(Type type, string memberName) => Get("Reflection_PropertyHasNoGetterFormat", type, memberName);

        /// <summary>Property has no setter accessor: {0}.{1}</summary>
        internal static string ReflectionPropertyHasNoSetter(Type type, string memberName) => Get("Reflection_PropertyHasNoSetterFormat", type, memberName);

        /// <summary>Value "{0}" cannot be resolved as a System.Type.</summary>
        internal static string ReflectionNotAType(string value) => Get("Reflection_NotATypeFormat", value);

        /// <summary>Property "{0}" not found and cannot be set via TypeDescriptor on type "{1}".</summary>
        internal static string ReflectionPropertyNotFoundTypeDescriptor(string propertyName, Type type) => Get("Reflection_PropertyNotFoundTypeDescriptorFormat", propertyName, type);

        /// <summary>No suitable instance property "{0}" found on type "{1}".</summary>
        internal static string ReflectionInstancePropertyDoesNotExist(string propertyName, Type type) => Get("Reflection_InstancePropertyDoesNotExistFormat", propertyName, type);

        /// <summary>No suitable static property "{0}" found on type "{1}".</summary>
        internal static string ReflectionStaticPropertyDoesNotExist(string propertyName, Type type) => Get("Reflection_StaticPropertyDoesNotExistFormat", propertyName, type);

        /// <summary>Expected number of array index arguments: {0}.</summary>
        internal static string ReflectionIndexParamsLengthMismatch(int length) => Get("Reflection_IndexParamsLengthMismatchFormat", length);

        /// <summary>No suitable indexer found on type "{0}" for the passed parameters.</summary>
        internal static string ReflectionIndexerNotFound(Type type) => Get("Reflection_IndexerNotFoundFormat", type);

        /// <summary>Property "{0}" not found and cannot be retrieved via TypeDescriptor on type "{1}".</summary>
        internal static string ReflectionCannotGetPropertyTypeDescriptor(string propertyName, Type type) => Get("Reflection_CannotGetPropertyTypeDescriptorFormat", propertyName, type);

        /// <summary>Expected number of type arguments: {0}.</summary>
        internal static string ReflectionTypeArgsLengthMismatch(int length) => Get("Reflection_TypeArgsLengthMismatchFormat", length);

        /// <summary>No suitable instance method "{0}" found on type "{1}" for the given parameters.</summary>
        internal static string ReflectionInstanceMethodNotFound(string methodName, Type type) => Get("Reflection_InstanceMethodNotFoundFormat", methodName, type);

        /// <summary>No suitable static method "{0}" found on type "{1}" for the given parameters.</summary>
        internal static string ReflectionStaticMethodNotFound(string methodName, Type type) => Get("Reflection_StaticMethodNotFoundFormat", methodName, type);

        /// <summary>No suitable constructor found on type "{0}" for the given parameters.</summary>
        internal static string ReflectionCtorNotFound(Type type) => Get("Reflection_CtorNotFoundFormat", type);

        /// <summary>Instance field "{0}" not found on type "{1}".</summary>
        internal static string ReflectionInstanceFieldDoesNotExist(string fieldName, Type type) => Get("Reflection_InstanceFieldDoesNotExistFormat", fieldName, type);

        /// <summary>Static field "{0}" not found on type "{1}".</summary>
        internal static string ReflectionStaticFieldDoesNotExist(string fieldName, Type type) => Get("Reflection_StaticFieldDoesNotExistFormat", fieldName, type);

        /// <summary>"{0}" is not a generic type, however, it is used so in the definition "{1}".</summary>
        internal static string ReflectionResolveNotAGenericType(string elementTypeName, string typeName) => Get("Reflection_ResolveNotAGenericTypeFormat", elementTypeName, typeName);

        /// <summary>Number of awaited and actual type parameters mismatch in type definition "{0}". Expected number of type arguments: {1}.</summary>
        internal static string ReflectionResolveTypeArgsLengthMismatch(string typeName, int length) => Get("Reflection_ResolveTypeArgsLengthMismatchFormat", typeName, length);

        /// <summary>Cannot resolve type parameter "{0}" in generic type "{1}".</summary>
        internal static string ReflectionCannotResolveTypeArg(string elementTypeName, string typeName) => Get("Reflection_CannotResolveTypeArgFormat", elementTypeName, typeName);

        /// <summary>Syntax error in generic/array type: "{0}".</summary>
        internal static string ReflectionTypeSyntaxError(string typeName) => Get("Reflection_TypeSyntaxErrorFormat", typeName);

        /// <summary>No MemberInfo can be returned from expression type "{0}".</summary>
        internal static string ReflectionNotAMember(Type type) => Get("Reflection_NotAMemberFormat", type);

        /// <summary>Failed to load assembly by name: "{0}".</summary>
        internal static string ReflectionCannotLoadAssembly(string name) => Get("Reflection_CannotLoadAssemblyFormat", name);

        #endregion

        #region Resources

        /// <summary>Unexpected element: "{0}" at line {1}, position {2}.</summary>
        internal static string ResourcesUnexpectedElementAt(string elementName, int line, int pos) => Get("Resources_UnexpectedElementAtFormat", elementName, line, pos);

        /// <summary>Resource file not found: {0}</summary>
        internal static string ResourcesNeutralResourceFileNotFoundResX(string fileName) => Get("Resources_NeutralResourceFileNotFoundResXFormat", fileName);

        /// <summary>Could not find any resources appropriate for the specified culture or the neutral culture. Make sure "{0}" was correctly embedded or linked into assembly "{1}" at compile time, or that all the satellite assemblies required are loadable and fully signed.</summary>
        internal static string ResourcesNeutralResourceNotFoundCompiled(string baseNameField, string fileName) => Get("Resources_NeutralResourceNotFoundCompiledFormat", baseNameField, fileName);

        /// <summary>Could not find any resources appropriate for the specified culture or the neutral culture. Make sure "{0}" was correctly embedded or linked into assembly "{1}" at compile time, or that all the satellite assemblies required are loadable and fully signed, or that XML resource file exists: {2}</summary>
        internal static string ResourcesNeutralResourceNotFoundHybrid(string baseNameField, string assemblyFile, string resxFile) => Get("Resources_NeutralResourceNotFoundHybridFormat", baseNameField, assemblyFile, resxFile);

        /// <summary>Cannot find a name for the resource at line {0}, position {1}.</summary>
        internal static string ResourcesNoResXName(int line, int pos) => Get("Resources_NoResXNameFormat", line, pos);

        /// <summary>"{0}" attribute is missing at line {1}, position {2}.</summary>
        internal static string ResourcesMissingAttribute(string name, int line, int pos) => Get("Resources_MissingAttributeFormat", name, line, pos);

        /// <summary>Unsupported ResX header mime type "{0}" at line {1}, position {2}.</summary>
        internal static string ResourcesHeaderMimeTypeNotSupported(string mimeType, int line, int pos) => Get("Resources_HeaderMimeTypeNotSupportedFormat", mimeType, line, pos);

        /// <summary>Unsupported mime type "{0}" at line {1}, position {2}.</summary>
        internal static string ResourcesMimeTypeNotSupported(string mimeType, int line, int pos) => Get("Resources_MimeTypeNotSupportedFormat", mimeType, line, pos);

        /// <summary>Unsupported ResX reader "{0}" at line {1}, position {2}.</summary>
        internal static string ResourcesResXReaderNotSupported(string reader, int line, int pos) => Get("Resources_ResXReaderNotSupportedFormat", reader, line, pos);

        /// <summary>Unsupported ResX writer "{0}" at line {1}, position {2}.</summary>
        internal static string ResourcesResXWriterNotSupported(string writer, int line, int pos) => Get("Resources_ResXWriterNotSupportedFormat", writer, line, pos);

        /// <summary>Type "{0}" in the data at line {1}, position {2} cannot be resolved.</summary>
        internal static string ResourcesTypeLoadExceptionAt(string typeName, int line, int pos) => Get("Resources_TypeLoadExceptionAtFormat", typeName, line, pos);

        /// <summary>Type "{0}" cannot be resolved.</summary>
        internal static string ResourcesTypeLoadException(string typeName) => Get("Resources_TypeLoadExceptionFormat", typeName);

        /// <summary>Type of resource "{0}" is not string but "{1}" - use GetObject instead.</summary>
        internal static string ResourcesNonStringResourceWithType(string name, string typeName) => Get("Resources_NonStringResourceWithTypeFormat", name, typeName);

        /// <summary>Attempting to convert type "{0}" from string on line {1}, position {2} has failed: {3}</summary>
        internal static string ResourcesConvertFromStringNotSupportedAt(string typeName, int line, int pos, string message) => Get("Resources_ConvertFromStringNotSupportedAtFormat", typeName, line, pos, message);

        /// <summary>Converting from string is not supported by {0}.</summary>
        internal static string ResourcesConvertFromStringNotSupported(Type converterType) => Get("Resources_ConvertFromStringNotSupportedFormat", converterType);

        /// <summary>Attempting to convert type "{0}" from byte array on line {1}, position {2} has failed: {3}</summary>
        internal static string ResourcesConvertFromByteArrayNotSupportedAt(string typeName, int line, int pos, string message) => Get("Resources_ConvertFromByteArrayNotSupportedAtFormat", typeName, line, pos, message);

        /// <summary>Converting from byte array is not supported by {0}.</summary>
        internal static string ResourcesConvertFromByteArrayNotSupported(Type converterType) => Get("Resources_ConvertFromByteArrayNotSupportedFormat", converterType);

        /// <summary>File in ResX file reference cannot be found: {0}. Is base path set correctly?</summary>
        internal static string ResourcesFileRefFileNotFound(string path) => Get("Resources_FileRefFileNotFoundFormat", path);

        #endregion

        #region StringExtensions

        /// <summary>The specified string '{0}' cannot be parsed as type {1}.</summary>
        internal static string StringExtensionsCannotParseAsType(string s, Type type) => Get("StringExtensions_CannotParseAsTypeFormat", s, type);

        #endregion

        #region XmlSerialization

        /// <summary>Serializing type "{0}" is not supported with following options: {1}. You may either use fallback options or provide a type converter for the type.</summary>
        internal static string XmlSerializationSerializingTypeNotSupported(Type type, XmlSerializationOptions options) => Get("XmlSerialization_SerializingTypeNotSupportedFormat", type, options.ToString<XmlSerializationOptions>());

        /// <summary>Root named "object" expected but "{0}" found.</summary>
        internal static string XmlSerializationRootExpected(string name) => Get("XmlSerialization_RootObjectExpectedFormat", name);

        /// <summary>Could not resolve type: "{0}". Maybe fully qualified assembly name is needed at serialization.</summary>
        internal static string XmlSerializationCannotResolveType(string typeName) => Get("XmlSerialization_CannotResolveTypeFormat", typeName);

        /// <summary>Deserializing type "{0}" is not supported.</summary>
        internal static string XmlSerializationDeserializingTypeNotSupported(Type type) => Get("XmlSerialization_DeserializingTypeNotSupportedFormat", type);

        /// <summary>Content serialization of read-only collection type "{0}" is not supported because populating will not work at deserialization.
        /// If the collection has an initializer constructor, then using XmlSerializer.Serialize method overloads instead of SerializeContent can work.</summary>
        internal static string XmlSerializationSerializingReadOnlyCollectionNotSupported(Type type) => Get("XmlSerialization_SerializingReadOnlyCollectionNotSupportedFormat", type);

        /// <summary>Binary serialization of type "{0}" failed with options "{1}": {2}</summary>
        internal static string XmlSerializationBinarySerializationFailed(Type type, XmlSerializationOptions options, string errorMessage) => Get("XmlSerialization_BinarySerializationFailedFormat", type, options.ToString<XmlSerializationOptions>(), errorMessage);

        /// <summary>Cannot serialize collection "{0}" with following options: "{1}". You may either use fallback options or provide a type converter or apply DesignerSerializationVisibilityAttribute with value Content on the container collection property.</summary>
        internal static string XmlSerializationCannotSerializeCollection(Type type, XmlSerializationOptions options) => Get("XmlSerialization_CannotSerializeCollectionFormat", type, options.ToString<XmlSerializationOptions>());

        /// <summary>Serialization of collection "{0}" is not supported with following options: "{1}", because it does not implement IList, IDictionary or ICollection&lt;T&gt; interfaces and has no initializer constructor that can accept an array or list.
        /// To force the recursive serialization of the collection enable both RecursiveSerializationAsFallback and ForcedSerializationOfReadOnlyMembersAndCollections options; however, deserialization will likely fail in this case. Using BinarySerializationAsFallback option may also work.</summary>
        internal static string XmlSerializationCannotSerializeUnsupportedCollection(Type type, XmlSerializationOptions options) => Get("XmlSerialization_CannotSerializeUnsupportedCollectionFormat", type, options.ToString<XmlSerializationOptions>());

        /// <summary>Type "{0}" does not implement IXmlSerializable.</summary>
        internal static string XmlSerializationNotAnIXmlSerializable(Type type) => Get("XmlSerialization_NotAnIXmlSerializableFormat", type);

        /// <summary>Type "{0}" does not have a parameterless constructor so it can be (de-)serialized either as a root element by SerializeContent and DeserializeContent or as a public property/field value in a parent object if the member value is not null after creating the parent object.</summary>
        internal static string XmlSerializationNoDefaultCtor(Type type) => Get("XmlSerialization_NoDefaultCtorFormat", type);

        /// <summary>Property value of "{0}.{1}" is expected to be a type of "{2}" but was "{3}".</summary>
        internal static string XmlSerializationPropertyTypeMismatch(Type declaringType, string propertyName, Type expectedType, Type actualType) => Get("XmlSerialization_PropertyTypeMismatchFormat", declaringType, propertyName, expectedType, actualType);

        /// <summary>Collection "{0}" is read-only so its content cannot be restored.</summary>
        internal static string XmlSerializationCannotDeserializeReadOnlyCollection(Type type) => Get("XmlSerialization_CannotDeserializeReadOnlyCollectionFormat", type);

        /// <summary>Content serialization of collection type "{0}" is not supported because it cannot be populated by standard interfaces.
        /// If the collection has an initializer constructor, then using XmlSerializer.Serialize method overloads instead of SerializeContent can work.</summary>
        internal static string XmlSerializationSerializingNonPopulatableCollectionNotSupported(Type type) => Get("XmlSerialization_SerializingNonPopulatableCollectionNotSupportedFormat", type);

        /// <summary>Cannot restore property "{0}" in type "{1}" because it has no setter.</summary>
        internal static string XmlSerializationPropertyHasNoSetter(string propertyName, Type type) => Get("XmlSerialization_PropertyHasNoSetterFormat", propertyName, type);

        /// <summary>Cannot set null to non-null property "{0}" in type "{1}" because it has no setter.</summary>
        internal static string XmlSerializationPropertyHasNoSetterCantSetNull(string propertyName, Type type) => Get("XmlSerialization_PropertyHasNoSetterCantSetNullFormat", propertyName, type);

        /// <summary>Cannot restore property "{0}" in type "{1}" because it has no setter and it returned null.</summary>
        internal static string XmlSerializationPropertyHasNoSetterGetsNull(string propertyName, Type type) => Get("XmlSerialization_PropertyHasNoSetterGetsNullFormat", propertyName, type);

        /// <summary>Collection item expected but "{0}" found.</summary>
        internal static string XmlSerializationItemExpected(string name) => Get("XmlSerialization_ItemExpectedFormat", name);

        /// <summary>Could not determine type of element in collection "{0}".</summary>
        internal static string XmlSerializationCannotDetermineElementType(Type type) => Get("XmlSerialization_CannotDetermineElementTypeFormat", type);

        /// <summary>Type "{0}" is not a regular collection so items cannot be added to it.</summary>
        internal static string XmlSerializationNotACollection(Type type) => Get("XmlSerialization_NotACollectionFormat", type);

        /// <summary>Type "{0}" has no public property or field "{1}".</summary>
        internal static string XmlSerializationHasNoMember(Type type, string name) => Get("XmlSerialization_HasNoMemberFormat", type, name);

        /// <summary>Serialized content of type "{0}" not found.</summary>
        internal static string XmlSerializationNoContent(Type type) => Get("XmlSerialization_NoContentFormat", type);

        /// <summary>Length attribute should be an integer but "{0}" found.</summary>
        internal static string XmlSerializationLengthInvalidType(string content) => Get("XmlSerialization_LengthInvalidTypeFormat", content);

        /// <summary>Cannot restore array "{0}" because size does not match. Expected length: "{1}".</summary>
        internal static string XmlSerializationArraySizeMismatch(Type type, int length) => Get("XmlSerialization_ArraySizeMismatchFormat", type, length);

        /// <summary>Cannot restore array "{0}" because rank does not match. Expected rank: "{1}".</summary>
        internal static string XmlSerializationArrayRankMismatch(Type type, int rank) => Get("XmlSerialization_ArrayRankMismatchFormat", type, rank);

        /// <summary>Cannot restore array "{0}" because length of the {1}. dimension does not match.</summary>
        internal static string XmlSerializationArrayDimensionSizeMismatch(Type type, int length) => Get("XmlSerialization_ArrayDimensionSizeMismatchFormat", type, length);

        /// <summary>Cannot restore array "{0}" because lower bound of the {1}. dimension does not match.</summary>
        internal static string XmlSerializationArrayLowerBoundMismatch(Type type, int dimension) => Get("XmlSerialization_ArrayLowerBoundMismatchFormat", type, dimension);

        /// <summary>Array items length mismatch. Expected items: {0}, found items: {1}.</summary>
        internal static string XmlSerializationInconsistentArrayLength(int expected, int actual) => Get("XmlSerialization_InconsistentArrayLengthFormat", expected, actual);

        /// <summary>The crc attribute should be a hex value but "{0}" found.</summary>
        internal static string XmlSerializationCrcHexExpected(string content) => Get("XmlSerialization_CrcHexExpectedFormat", content);

        /// <summary>Unexpected element: "{0}".</summary>
        internal static string XmlSerializationUnexpectedElement(string elementName) => Get("XmlSerialization_UnexpectedElementFormat", elementName);

        /// <summary>Invalid escaped string content: "{0}".</summary>
        internal static string XmlSerializationInvalidEscapedContent(string content) => Get("XmlSerialization_InvalidEscapedContentFormat", content);

        /// <summary>Circular reference found during serialization. Object is already serialized: "{0}". To avoid circular references use DesignerSerializationVisibilityAttribute with Hidden value on members directly or indirectly reference themselves.</summary>
        internal static string XmlSerializationCircularReference(object obj) => Get("XmlSerialization_CircularReferenceFormat", obj);

        #endregion

        #endregion

        #region Private Methods

        private static string Get([NotNull]string id)
        {
            return resourceManager.GetString(id, LanguageSettings.DisplayLanguage) ?? String.Format(unavailableResource, id);
        }

        private static string Get([NotNull]string id, params object[] args)
        {
            string format = Get(id);
            return args == null || args.Length == 0 ? format : SafeFormat(format, args);
        }

        private static string FormatValues<TEnum>() where TEnum : struct, IConvertible
            => String.Join(", ", Enum<TEnum>.GetNames().Select(v => QuoteStart + v + QuoteEnd));

        private static string FormatFlags<TEnum>() where TEnum : struct, IConvertible
            => String.Join(", ", Enum<TEnum>.GetFlags().Select(f => QuoteStart + f + QuoteEnd));

        private static string SafeFormat(string format, object[] args)
        {
            try
            {
                int i = Array.IndexOf(args, null);
                if (i >= 0)
                {
                    string nullRef = NullReference;
                    for (; i < args.Length; i++)
                    {
                        if (args[i] == null)
                            args[i] = nullRef;
                    }
                }

                return String.Format(LanguageSettings.FormattingLanguage, format, args);
            }
            catch (FormatException)
            {
                return String.Format(invalidResource, args.Length, format);
            }
        }

        #endregion

        #endregion
    }
}
