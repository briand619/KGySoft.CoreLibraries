﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BinarySerializerTest.TestData.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

using KGySoft.Reflection;
using KGySoft.Serialization;
using NUnit.Framework;

#endregion

namespace KGySoft.CoreLibraries.UnitTests.Serialization
{
    partial class BinarySerializerTest
    {
        #region Nested types

        #region Enumerations

        enum TestEnumSByte : sbyte
        {
            Min = SByte.MinValue,
            Max = SByte.MaxValue
        }

        enum TestEnumByte : byte
        {
            Min = Byte.MinValue,
            One = 1,
            Two,
            Max = Byte.MaxValue
        }

        enum TestEnumShort : short
        {
            Min = Int16.MinValue,
            Limit = (1 << 7) - 1,
            Treshold,
            Max = Int16.MaxValue,
        }

        enum TestEnumUShort : ushort
        {
            Min = UInt16.MinValue,
            Limit = (1 << 7) - 1,
            Treshold,
            Max = UInt16.MaxValue,
        }

        enum TestEnumInt : int
        {
            Min = Int32.MinValue,
            Limit = (1 << 21) - 1,
            Treshold,
            Max = Int32.MaxValue,
        }

        enum TestEnumUInt : uint
        {
            Min = UInt32.MinValue,
            Limit = (1 << 21) - 1,
            Treshold,
            Max = UInt32.MaxValue,
        }

        enum TestEnumLong : long
        {
            Min = Int64.MinValue,
            Limit = (1L << 49) - 1,
            Treshold,
            Max = Int64.MaxValue,
        }

        enum TestEnumULong : ulong
        {
            Min = UInt64.MinValue,
            Limit = (1UL << 49) - 1,
            Treshold,
            Max = UInt64.MaxValue,
        }

        #endregion

        #region Nested classes

        #region NonSerializableClass class

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        private class NonSerializableClass
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        {
            #region Properties

            public int IntProp { get; set; }

            public string StringProp { get; set; }

            #endregion

            #region Methods

            /// <summary>
            /// Overridden for the test equality check
            /// </summary>
            public override bool Equals(object obj)
            {
                if (!(obj is NonSerializableClass))
                    return base.Equals(obj);
                NonSerializableClass other = (NonSerializableClass)obj;
                return StringProp == other.StringProp && IntProp == other.IntProp;
            }

            #endregion
        }

        #endregion

        #region NonSerializableSealedClass class

        private sealed class NonSerializableSealedClass : NonSerializableClass
        {
            #region Fields

            #region Public Fields

            public int PublicDerivedField;

            #endregion

            #region Private Fields

            private string PrivateDerivedField;

            #endregion

            #endregion

            #region Constructors

            public NonSerializableSealedClass(int i, string s)
            {
                PublicDerivedField = i;
                PrivateDerivedField = s;
            }

            #endregion
        }

        #endregion

        #region BinarySerializableClass class

        [Serializable]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        private class BinarySerializableClass : AbstractClass, IBinarySerializable
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        {
            #region Fields

            public int PublicField;

            #endregion

            #region Properties

            public int IntProp { get; set; }

            public string StringProp { get; set; }

            #endregion

            #region Methods

            #region Public Methods

            public byte[] Serialize(BinarySerializationOptions options)
            {
                MemoryStream ms = new MemoryStream();
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(PublicField);
                    bw.Write(IntProp);
                    bw.Write(StringProp);
                }

                return ms.ToArray();
            }

            public void Deserialize(BinarySerializationOptions options, byte[] serData)
            {
                using (BinaryReader br = new BinaryReader(new MemoryStream(serData)))
                {
                    PublicField = br.ReadInt32();
                    IntProp = br.ReadInt32();
                    StringProp = br.ReadString();
                }
            }

            /// <summary>
            /// Overridden for the test equality check
            /// </summary>
            public override bool Equals(object obj)
            {
                if (!(obj is BinarySerializableClass))
                    return base.Equals(obj);
                BinarySerializableClass other = (BinarySerializableClass)obj;
                return PublicField == other.PublicField && StringProp == other.StringProp && IntProp == other.IntProp;
            }

            #endregion

            #region Private Methods

            [OnDeserializing]
            private void OnDeserializing(StreamingContext ctx)
            {
                IntProp = -1;
            }

            #endregion

            #endregion
        }

        #endregion

        #region BinarySerializableSealedClass class

        [Serializable]
        private sealed class BinarySerializableSealedClass : BinarySerializableClass
        {
            #region Constructors

            /// <summary>
            /// Non-default constructor so the class will be deserialized without constructor
            /// </summary>
            public BinarySerializableSealedClass(int intProp, string stringProp)
            {
                IntProp = intProp;
                StringProp = stringProp;
            }

            #endregion
        }

        #endregion

        #region AbstractClass class

        [Serializable]
        public abstract class AbstractClass
        {
        }

        #endregion

        #region SystemSerializableClass class

        [Serializable]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        private class SystemSerializableClass : AbstractClass
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        {
            #region Properties

            public int IntProp { get; set; }

            public string StringProp { get; set; }

            public bool? Bool { get; set; }

            #endregion

            #region Methods

            /// <summary>
            /// Overridden for the test equality check
            /// </summary>
            public override bool Equals(object obj)
            {
                if (!(obj is SystemSerializableClass))
                    return base.Equals(obj);
                SystemSerializableClass other = (SystemSerializableClass)obj;
                return StringProp == other.StringProp && IntProp == other.IntProp && Bool == other.Bool;
            }

            #endregion
        }

        #endregion

        #region NonSerializableClassWithSerializableBase class

        private sealed class NonSerializableClassWithSerializableBase : SystemSerializableClass
        {
            #region Fields

            #region Public Fields

            public int PublicDerivedField;

            #endregion

            #region Private Fields

            private string PrivateDerivedField;

            #endregion

            #endregion

            #region Constructors

            public NonSerializableClassWithSerializableBase(int i, string s)
            {
                PublicDerivedField = i;
                PrivateDerivedField = s;
            }

            #endregion
        }

        #endregion

        #region SystemSerializableSealedClass class

        [Serializable]
        private sealed class SystemSerializableSealedClass : SystemSerializableClass
        {
        }

        #endregion

        #region SerializationEventsClass class

        [Serializable]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        private class SerializationEventsClass : IDeserializationCallback
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        {
            #region Fields

            #region Static Fields

            private static int idCounter;

            #endregion

            #region Instance Fields

            #region Protected Fields

            protected readonly Collection<SerializationEventsClass> children = new Collection<SerializationEventsClass>();

            #endregion

            #region Private Fields

            [NonSerialized]
            private IntPtr privatePointer;
            [NonSerialized]
            private SerializationEventsClass parent;

            #endregion

            #endregion

            #endregion

            #region Properties

            public int Id { get; protected set; }

            public string Name { get; set; }

            public SerializationEventsClass Parent { get { return parent; } }

            public ICollection Children { get { return children; } }

            #endregion

            #region Constructors

            public SerializationEventsClass()
            {
                Id = ++idCounter;
            }

            #endregion

            #region Methods

            #region Public Methods

            public SerializationEventsClass AddChild(string name)
            {
                SerializationEventsClass child = new SerializationEventsClass { Name = name };
                children.Add(child);
                child.parent = this;
                privatePointer = new IntPtr(children.Count);
                return child;
            }

            public virtual void OnDeserialization(object sender)
            {
                //Console.WriteLine("OnDeserialization {0}", this);
                if (children != null)
                {
                    foreach (SerializationEventsClass child in children)
                    {
                        child.parent = this;
                    }
                }
            }

            public override bool Equals(object obj)
            {
                if (!(obj is SerializationEventsClass other))
                    return base.Equals(obj);

                return Id == other.Id
                    && privatePointer == other.privatePointer
                    && (parent == null && other.parent == null || parent != null && other.parent != null && parent.Id == other.parent.Id)
                    && children.SequenceEqual(other.children);
            }

            public override string ToString()
            {
                return String.Format("{0} - {1}", Id, Name ?? "<null>");
            }

            #endregion

            #region Private Methods

            [OnSerializing]
            private void OnSerializing(StreamingContext ctx)
            {
                //Console.WriteLine("OnSerializing {0}", this);
                privatePointer = IntPtr.Zero;
            }

            [OnSerialized]
            private void OnSerialized(StreamingContext ctx)
            {
                //Console.WriteLine("OnSerialized {0}", this);
                if (children.Count > 0)
                    privatePointer = new IntPtr(children.Count);
            }

            [OnDeserializing]
            private void OnDeserializing(StreamingContext ctx)
            {
                //Console.WriteLine("OnDeserializing {0}", this);
                privatePointer = new IntPtr(-1);
            }

            [OnDeserialized]
            private void OnDeserialized(StreamingContext ctx)
            {
                //Console.WriteLine("OnDeserialized {0}", this);
                if (children != null)
                    privatePointer = new IntPtr(children.Count);
            }

            #endregion

            #endregion
        }

        #endregion

        #region CustomSerializedClass class

        [Serializable]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        private class CustomSerializedClass : SerializationEventsClass, ISerializable
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        {
            #region Properties

            public bool? Bool { get; set; }

            #endregion

            #region Constructors

            #region Public Constructors

            public CustomSerializedClass()
            {
            }

            #endregion

            #region Private Constructors

            private CustomSerializedClass(SerializationInfo info, StreamingContext context)
            {
                Id = info.GetInt32("Id");
                Name = info.GetString("Name");
                Bool = (bool?)info.GetValue("Bool", typeof(bool?));
                ((Collection<SerializationEventsClass>)info.GetValue("Children", typeof(Collection<SerializationEventsClass>))).ForEach(child => children.Add(child));
            }

            #endregion

            #endregion

            #region Methods

            #region Public Methods

            [SecurityCritical]
            public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("Id", Id);
                info.AddValue("Name", Name);
                info.AddValue("Bool", Bool, typeof(bool?));
                info.AddValue("Children", Children);
                info.AddValue("dummy", null, typeof(List<string[]>));
            }

            public override bool Equals(object obj) => !(obj is CustomSerializedClass other) ? base.Equals(obj) : Bool == other.Bool && base.Equals(obj);

            #endregion

            #region Private Methods

            [OnSerialized]
            private void OnSerialized(StreamingContext ctx)
            {
                //Console.WriteLine("OnSerialized derived {0}", this);
            }

            [OnDeserialized]
            private void OnDeserialized(StreamingContext ctx)
            {
                //Console.WriteLine("OnDeserialized derived {0}", this);
            }

            #endregion

            #endregion
        }

        #endregion

        #region CustomSerializedSealedClass class

        [Serializable]
        private sealed class CustomSerializedSealedClass : CustomSerializedClass, ISerializable
        {
            #region Constructors

            #region Public Constructors

            public CustomSerializedSealedClass(string name)
            {
                Name = name;
            }

            #endregion

            #region Internal Constructors

            internal CustomSerializedSealedClass(int id, string name, IEnumerable<SerializationEventsClass> children, bool? boolean)
            {
                Id = id;
                Name = name;
                children.ForEach(child => this.children.Add(child));
                Bool = boolean;
            }

            #endregion

            #region Private Constructors

            private CustomSerializedSealedClass(SerializationInfo info, StreamingContext context)
            {
                throw new InvalidOperationException("Never executed");
            }

            #endregion

            #endregion

            #region Methods

            [SecurityCritical]
            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.SetType(typeof(CustomAdvancedSerializedClassHelper));
                base.GetObjectData(info, context);
            }

            #endregion
        }

        #endregion

        #region CustomAdvancedSerializedClassHelper class

        [Serializable]
        private class CustomAdvancedSerializedClassHelper : IObjectReference, ISerializable, IDeserializationCallback
        {
            #region Fields

            readonly CustomSerializedSealedClass toDeserialize;

            #endregion

            #region Constructors

            private CustomAdvancedSerializedClassHelper(SerializationInfo info, StreamingContext context)
            {
                toDeserialize = new CustomSerializedSealedClass(info.GetInt32("Id"), info.GetString("Name"),
                    (Collection<SerializationEventsClass>)info.GetValue("Children", typeof(Collection<SerializationEventsClass>)),
                    (bool?)info.GetValue("Bool", typeof(bool?)));
            }

            #endregion

            #region Methods

            #region Public Methods

            [SecurityCritical]
            public object GetRealObject(StreamingContext context)
            {
                return toDeserialize;
            }

            [SecurityCritical]
            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                throw new NotImplementedException("Never executed");
            }

            public void OnDeserialization(object sender)
            {
                toDeserialize.OnDeserialization(sender);
            }

            #endregion

            #region Private Methods

            [OnDeserialized]
            private void OnDeserialized(StreamingContext ctx)
            {
                //Console.WriteLine("OnDeserialized Helper");
                Reflector.SetField(toDeserialize, "privatePointer", new IntPtr(toDeserialize.Children.Count));
            }

            #endregion

            #endregion
        }

        #endregion

        #region DefaultGraphObjRef class

        [Serializable]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        private class DefaultGraphObjRef : IObjectReference
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        {
            #region Fields

            #region Static Fields

            private readonly static DefaultGraphObjRef instance = new DefaultGraphObjRef("singleton instance");

            #endregion

            #region Instance Fields

            private readonly string name;

            #endregion

            #endregion

            #region Constructors

            private DefaultGraphObjRef(string name)
            {
                this.name = name;
            }

            #endregion

            #region Methods

            #region Static Methods

            public static DefaultGraphObjRef Get()
            {
                return instance;
            }

            #endregion

            #region Instance Methods

            public override string ToString()
            {
                return name;
            }

            public override bool Equals(object obj)
            {
                return ReferenceEquals(obj, instance);
            }

            [SecurityCritical]
            public object GetRealObject(StreamingContext context)
            {
                return instance;
            }

            #endregion

            #endregion
        }

        #endregion

        #region CustomGraphDefaultObjRef class

        [Serializable]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        private sealed class CustomGraphDefaultObjRef : ISerializable
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        {
            #region Properties

            public string Name { get; set; }

            #endregion

            #region Methods

            [SecurityCritical]
            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("name", Name);
                info.SetType(typeof(CustomGraphDefaultObjRefDeserializer));
            }

            public override bool Equals(object obj)
            {
                if (!(obj is CustomGraphDefaultObjRef other))
                    return false;
                return Name == other.Name;
            }

            #endregion
        }

        #endregion

        #region CustomGraphDefaultObjRefDeserializer class

        [Serializable]
        private class CustomGraphDefaultObjRefDeserializer : IObjectReference
        {
            #region Fields

#pragma warning disable 649
            private string name;
#pragma warning restore 649

            #endregion

            #region Methods

            [SecurityCritical]
            public object GetRealObject(StreamingContext context)
            {
                return new CustomGraphDefaultObjRef { Name = name };
            }

            #endregion
        }

        #endregion

        #region CustomGenericCollection class

        [Serializable]
        private class CustomGenericCollection<T> : List<T>
        {
        }

        #endregion

        #region CustomNonGenericCollection class

        [Serializable]
        private class CustomNonGenericCollection : ArrayList
        {
        }

        #endregion

        #region CustomGenericDictionary class

        [Serializable]
        private class CustomGenericDictionary<TKey, TValue> : Dictionary<TKey, TValue>
        {
            #region Constructors

            public CustomGenericDictionary()
            {
            }

            public CustomGenericDictionary(SerializationInfo info, StreamingContext context) :
                base(info, context)
            {
            }

            #endregion
        }

        #endregion

        #region OpenGenericDictionary class

        private class OpenGenericDictionary<TValue> : Dictionary<string, TValue>
        {
        }

        #endregion

        #region CustomNonGenericDictionary class

        [Serializable]
        private class CustomNonGenericDictionary : Hashtable
        {
            #region Constructors

            public CustomNonGenericDictionary()
            {
            }

            public CustomNonGenericDictionary(SerializationInfo info, StreamingContext context) :
                base(info, context)
            {
            }

            #endregion
        }

        #endregion

        #region MemoryStreamWithEquals class

        [Serializable]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        private sealed class MemoryStreamWithEquals : MemoryStream
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        {
            #region Methods

            public override bool Equals(object obj)
            {
                if (!(obj is MemoryStreamWithEquals other))
                    return base.Equals(obj);

                return this.CanRead == other.CanRead && this.CanSeek == other.CanSeek && this.CanTimeout == other.CanTimeout && this.CanWrite == other.CanWrite
                    && this.Capacity == other.Capacity && this.Length == other.Length && this.Position == other.Position && this.GetBuffer().SequenceEqual(other.GetBuffer());
            }

            #endregion
        }

        #endregion

        #region CircularReferenceClass class

        [Serializable]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        private sealed class CircularReferenceClass
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        {
            #region Fields

            #region Static Fields

            private static int idCounter;

            #endregion

            #region Instance Fields

            private readonly Collection<CircularReferenceClass> children = new Collection<CircularReferenceClass>();

            private CircularReferenceClass parent;

            #endregion

            #endregion

            #region Properties

            public int Id { get; private set; }

            public string Name { get; set; }

            public CircularReferenceClass Parent { get { return parent; } }

            public Collection<CircularReferenceClass> Children { get { return children; } }

            #endregion

            #region Constructors

            public CircularReferenceClass()
            {
                Id = ++idCounter;
            }

            #endregion

            #region Methods

            public CircularReferenceClass AddChild(string name)
            {
                CircularReferenceClass child = new CircularReferenceClass { Name = name };
                children.Add(child);
                child.parent = this;
                return child;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is CircularReferenceClass other))
                    return base.Equals(obj);

                return Id == other.Id
                    && (parent == null && other.parent == null || parent != null && other.parent != null && parent.Id == other.parent.Id)
                    && children.SequenceEqual(other.children); // can cause stack overflow
            }

            public override string ToString()
            {
                return String.Format("{0} - {1}", Id, Name ?? "<null>");
            }

            #endregion
        }

        #endregion

        #region SelfReferencer class

        [Serializable]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        private class SelfReferencer : ISerializable
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        {
            #region Nested classes

            #region Box class

            [Serializable]
            private class Box
            {
                #region Fields

                internal SelfReferencer owner;

                #endregion
            }

            #endregion

            #endregion

            #region Fields

            private readonly Box selfReferenceFromChild;

            #endregion

            #region Properties

            public string Name { get; set; }

            public SelfReferencer Self { get; set; }

            #endregion

            #region Constructors

            #region Public Constructors

            public SelfReferencer(string name)
            {
                Name = name;
                Self = this;
                selfReferenceFromChild = new Box { owner = this };
            }

            #endregion

            #region Private Constructors

            private SelfReferencer(SerializationInfo info, StreamingContext context)
            {
                Name = info.GetString("name");
                Self = (SelfReferencer)info.GetValue("self", typeof(SelfReferencer));
                selfReferenceFromChild = (Box)info.GetValue("selfBox", typeof(Box));
            }

            #endregion

            #endregion

            #region Methods

            [SecurityCritical]
            public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("name", Name);
                info.AddValue("self", Self);
                info.AddValue("selfBox", selfReferenceFromChild);
            }

            public override bool Equals(object obj)
            {
                if (obj == null || obj.GetType() != typeof(SelfReferencer))
                    return false;

                var other = (SelfReferencer)obj;
                return other.Name == this.Name && ReferenceEquals(other, other.Self) && ReferenceEquals(this, this.Self);
            }

            #endregion
        }

        #endregion

        #region SelfReferencerEvil class

        [Serializable]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        private class SelfReferencerEvil : SelfReferencer
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        {
            #region Constructors

            public SelfReferencerEvil(string name)
                : base(name)
            {
            }

            #endregion

            #region Methods

            [SecurityCritical]
            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                base.GetObjectData(info, context);
                info.SetType(typeof(SelfReferencerEvilDeserializer));
            }

            public override bool Equals(object obj)
            {
                if (obj == null || obj.GetType() != typeof(SelfReferencerEvil))
                    return false;

                var other = (SelfReferencerEvil)obj;
                return other.Name == this.Name && ReferenceEquals(other, other.Self) && ReferenceEquals(this, this.Self);
            }

            #endregion
        }

        #endregion

        #region SelfReferencerEvilDeserializer class

        [Serializable]
        private class SelfReferencerEvilDeserializer : IObjectReference, ISerializable
        {
            #region Fields

            private SelfReferencer instance;
            private string name;

            #endregion

            #region Constructors

            protected SelfReferencerEvilDeserializer(SerializationInfo info, StreamingContext context)
            {
                name = info.GetString("name");
                instance = (SelfReferencer)info.GetValue("self", typeof(SelfReferencer));
            }

            #endregion

            #region Methods

            [SecurityCritical]
            public object GetRealObject(StreamingContext context)
            {
                return new SelfReferencerEvil(name);
            }

            [SecurityCritical]
            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        #endregion

        #region TestSurrogateSelector class

        private class TestSurrogateSelector : ISurrogateSelector, ISerializationSurrogate
        {
            #region Fields

            private ISurrogateSelector next;

            #endregion

            #region Methods

            #region Public Methods

            [SecurityCritical]
            public void ChainSelector(ISurrogateSelector selector)
            {
                next = selector;
            }

            [SecurityCritical]
            public ISurrogateSelector GetNextSelector()
            {
                return next;
            }

            [SecurityCritical]
            public ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
            {
                if (type == null)
                {
                    throw new ArgumentNullException("type");
                }

                if (!type.IsPrimitive && !type.IsArray && !typeof(ISerializable).IsAssignableFrom(type) && !type.In(typeof(string), typeof(UIntPtr)))
                {
                    selector = this;
                    return this;
                }

                if (next != null)
                {
                    return next.GetSurrogate(type, context, out selector);
                }

                selector = null;
                return null;

            }

            #endregion

            #region Explicitly Implemented Interface Methods

            [SecurityCritical]
            void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                if (obj == null)
                    throw new ArgumentNullException("obj");
                if (info == null)
                    throw new ArgumentNullException("info");

                Type type = obj.GetType();

                for (Type t = type; t != typeof(object); t = t.BaseType)
                {
                    FieldInfo[] fields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(f => !f.IsNotSerialized).ToArray();
                    foreach (FieldInfo field in fields)
                    {
                        info.AddValue(field.Name, Reflector.GetField(obj, field));
                    }
                }
            }

            [SecurityCritical]
            object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                if (obj == null)
                    throw new ArgumentNullException("obj");
                if (info == null)
                    throw new ArgumentNullException("info");

                foreach (SerializationEntry entry in info)
                {
                    Reflector.SetField(obj, entry.Name, entry.Value);
                }

                return obj;
            }

            #endregion

            #endregion
        }

        #endregion

        #endregion

        #region Nested structs

        #region NonSerializableStruct struct

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        private struct NonSerializableStruct
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        {
            #region Fields

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
            private string str10;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            private byte[] bytes3;

            #endregion

            #region Properties

            public int IntProp { get; set; }

            public string Str10
            {
                get { return str10; }
                set { str10 = value; }
            }

            public byte[] Bytes3
            {
                get { return bytes3; }
                set { bytes3 = value; }
            }

            #endregion

            #region Methods

            /// <summary>
            /// Overridden for the test equality check
            /// </summary>
            public override bool Equals(object obj)
            {
                if (!(obj is NonSerializableStruct))
                    return base.Equals(obj);
                NonSerializableStruct other = (NonSerializableStruct)obj;
                return str10 == other.str10 && IntProp == other.IntProp
                    && ((bytes3 == null && other.bytes3 == null) || (bytes3 != null && other.bytes3 != null
                                && bytes3[0] == other.bytes3[0] && bytes3[1] == other.bytes3[1] && bytes3[2] == other.bytes3[2]));
            }

            #endregion
        }

        #endregion

        #region BinarySerializableStruct struct

        [Serializable]
        private struct BinarySerializableStruct : IBinarySerializable
        {
            #region Properties

            public int IntProp { get; set; }

            public string StringProp { get; set; }

            #endregion

            #region Constructors

            public BinarySerializableStruct(BinarySerializationOptions options, byte[] serData)
                : this()
            {
                using (BinaryReader br = new BinaryReader(new MemoryStream(serData)))
                {
                    IntProp = br.ReadInt32();
                    if (br.ReadBoolean())
                        StringProp = br.ReadString();
                }
            }

            #endregion

            #region Methods

            #region Public Methods

            public byte[] Serialize(BinarySerializationOptions options)
            {
                MemoryStream ms = new MemoryStream();
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(IntProp);
                    bw.Write(StringProp != null);
                    if (StringProp != null)
                        bw.Write(StringProp);
                }

                return ms.ToArray();
            }

            public void Deserialize(BinarySerializationOptions options, byte[] serData)
            {
                throw new InvalidOperationException("This method never will be called");
            }

            #endregion

            #region Private Methods

            [OnDeserializing]
            private void OnDeserializing(StreamingContext ctx)
            {
                IntProp = -1;
            }

            #endregion

            #endregion
        }

        #endregion

        #region SystemSerializableStruct struct

        [Serializable]
        private struct SystemSerializableStruct
        {
            #region Properties

            public int IntProp { get; set; }

            public string StringProp { get; set; }

            #endregion

            #region Methods

            [OnDeserializing]
            private void OnDeserializing(StreamingContext ctx)
            {
                IntProp = -1;
            }

            #endregion
        }

        #endregion

        #region CustomSerializableStruct struct

        [Serializable]
        private struct CustomSerializableStruct : ISerializable
        {
            #region Properties

            public int IntProp { get; set; }

            public string StringProp { get; set; }

            #endregion

            #region Constructors

            private CustomSerializableStruct(SerializationInfo info, StreamingContext context)
                : this()
            {
                IntProp = info.GetInt32("Int");
                StringProp = info.GetString("String");
            }

            #endregion

            #region Methods

            #region Public Methods

            [SecurityCritical]
            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("Int", IntProp);
                info.AddValue("String", StringProp);
            }

            #endregion

            #region Private Methods

            [OnDeserializing]
            private void OnDeserializing(StreamingContext ctx)
            {
                IntProp = -1;
            }

            #endregion

            #endregion
        }

        #endregion

        #region BinarySerializableStructNoCtor struct

        [Serializable]
        private struct BinarySerializableStructNoCtor : IBinarySerializable
        {
            #region Properties

            public int IntProp { get; set; }

            public string StringProp { get; set; }

            #endregion

            #region Methods

            public byte[] Serialize(BinarySerializationOptions options)
            {
                MemoryStream ms = new MemoryStream();
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(IntProp);
                    bw.Write(StringProp);
                }

                return ms.ToArray();
            }

            public void Deserialize(BinarySerializationOptions options, byte[] serData)
            {
                using (BinaryReader br = new BinaryReader(new MemoryStream(serData)))
                {
                    IntProp = br.ReadInt32();
                    StringProp = br.ReadString();
                }
            }

            #endregion
        }

        #endregion

        #region UnsafeStruct struct

        [Serializable]
        private unsafe struct UnsafeStruct
        {
            #region Fields

            public void* VoidPointer;
            public int* IntPointer;
            public Point* StructPointer;
            public int*[] PointerArray;
            public void** PointerOfPointer;

            #endregion
        }

        #endregion

        #endregion

        #endregion
    }
}
