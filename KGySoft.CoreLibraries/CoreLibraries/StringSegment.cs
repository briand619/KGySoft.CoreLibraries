﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: StringSegment.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2020 - All Rights Reserved
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
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using KGySoft.ComponentModel;

#endregion

namespace KGySoft.CoreLibraries
{
    /// <summary>
    /// Represents a segment of a <see cref="string"/>. This type is similar to <see cref="ReadOnlyMemory{T}"/>/<see cref="ArraySegment{T}"/>/<see cref="Span{T}"/> of <see cref="char">char</see>
    /// but <see cref="StringSegment"/> can be used in all platforms in the same way and is optimized for some dedicated string operations.
    /// <br/>To create an instance use the <see cref="O:KGySoft.CoreLibraries.StringExtensions.AsSegment"/> extension method overloads or just cast a string instance to <see cref="StringSegment"/>.
    /// <br/>See the <strong>Remarks</strong> section for details.
    /// </summary>
    /// <remarks>
    /// <para>To create a <see cref="StringSegment"/> instance from a string you can use the implicit conversion, or the <see cref="O:KGySoft.CoreLibraries.StringExtensions.AsSegment"/> extension methods.</para>
    /// <para>To convert a <see cref="StringSegment"/> instance to <see cref="string">string</see> use an explicit cast or the <see cref="ToString()">ToString</see> method.</para>
    /// <note>The <see cref="StringSegment"/> type <em>may</em> outperform <see cref="string">string</see> in scenarios when usual string splitting/trimming operations would allocate long strings.
    /// <br/>See a live example with performance test <a href="https://dotnetfiddle.net/Byk0YM" target="_blank">here</a>.</note>
    /// <para>Depending on the used platform some members of the <see cref="StringSegment"/> type may allocate a new string.
    /// The affected members are:
    /// <list type="bullet">
    /// <item><see cref="GetHashCode(StringComparison)"/>: if comparison is not <see cref="StringComparison.Ordinal"/> or <see cref="StringComparison.OrdinalIgnoreCase"/>.</item>
    /// <item><see cref="O:KGySoft.CoreLibraries.StringSegment.IndexOf"/> overloads with <see cref="StringComparison"/> parameter: if comparison is not <see cref="StringComparison.Ordinal"/>.</item>
    /// <item><see cref="O:KGySoft.CoreLibraries.StringSegment.LastIndexOf"/> overloads: affects all comparisons.</item>
    /// </list>
    /// <note>On .NET Core 3.0 and newer platforms none of the members above allocate a new string.
    /// On .NET Standard 2.1 and newer platforms the <see cref="O:KGySoft.CoreLibraries.StringSegment.IndexOf"/> overloads are not affected.</note></para>
    /// <para>As opposed to the <see cref="String"/> class, the default comparison strategy in <see cref="StringSegment"/> members is <see cref="StringComparison.Ordinal"/>.</para>
    /// <example>
    /// <para>The following example demonstrates how to use the <see cref="StringSegment"/> type:
    /// <code lang="C#"><![CDATA[
    /// using System;
    /// using KGySoft.CoreLibraries;
    /// 
    /// class Example
    /// {
    ///     public static void Main()
    ///     {
    ///         // Assignment works from string
    ///         StringSegment segment = "Some string literal";
    /// 
    ///         // Or by extension methods:
    ///         segment = "Some string literal".AsSegment(); // "Some string literal"
    ///         segment = "Some string literal".AsSegment(0, 4); // "Some"
    ///         segment = "Some string literal".AsSegment(5, 6); // "string"
    /// 
    ///         // Null assignment: all the following lines have the same effect:
    ///         segment = default(StringSegment); // the fastest way
    ///         segment = StringSegment.Null; // the recommended way
    ///         segment = null; // the cleanest way - same as segment = ((string)null).AsSegment()
    /// 
    ///         // Null check (remember, StringSegment is a value type with null semantics):
    ///         bool isNull = segment == null; // the cleanest way - same as segment.Equals(((string)null).AsSegment())
    ///         isNull = segment.IsNull; // the fastest and recommended way - same as segment.UnderlyingString == null
    /// 
    ///         // Slicing:
    ///         segment = "Some string literal";
    ///         Console.WriteLine(segment.Substring(0, 4)); // "Some"
    ///         Console.WriteLine(segment.Substring(5)); // "string literal"
    ///         Console.WriteLine(segment.Split(' ').Count); // 3
    ///         Console.WriteLine(segment.Split(' ')[2]); // "literal"
    /// 
    ///         // Slicing operations do not allocate new strings:
    ///         StringSegment subsegment = segment.Substring(5);
    ///         Console.WriteLine(subsegment); // "string literal"
    ///         Console.WriteLine(subsegment.UnderlyingString); // "Some string literal"
    ///     }
    /// }]]></code></para>
    /// <para>The following example demonstrates a possible usage of the <see cref="StringSegment"/> type:
    /// <note type="tip">Try the extended example with performance comparison <a href="https://dotnetfiddle.net/Byk0YM" target="_blank">online</a>.</note>
    /// <code lang="C#"><![CDATA[
    /// /**************************************************************
    ///  * This example retrieves values from multiline text like this:
    ///  * Key1=Value1;Value2
    ///  * Key2=SingleValue
    ///  * See a working example here: https://dotnetfiddle.net/Byk0YM
    ///  **************************************************************/
    /// // The original way:
    /// public static string[] ByString(string content, string key)
    /// {
    ///     // getting all lines, filtering the first empty line
    ///     string[] nonEmptyLines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
    ///
    ///     foreach (string line in nonEmptyLines)
    ///     {
    ///         // Separating key from values. We can use count: 2 because we split at the first '=' only.
    ///         string[] keyValues = line.Split(new[] { '=' }, count: 2);
    ///
    ///         // Removing white spaces and returning values if the key matches
    ///         if (keyValues[0].TrimStart() == key)
    ///             return keyValues[1].Split(';');
    ///     }
    ///
    ///     // key not found
    ///     return null;
    /// }
    ///
    /// // The StringSegment way: almost the same code as above
    /// public static IList<StringSegment> ByStringSegment(string content, string key)
    /// {
    ///     // getting all lines, filtering the first empty line
    ///     IList<StringSegment> nonEmptyLines = content.AsSegment().Split(Environment.NewLine, removeEmptyEntries: true);
    ///
    ///     foreach (StringSegment line in nonEmptyLines)
    ///     {
    ///         // Separating key from values. We can use maxLength: 2 because we split at the first '=' only.
    ///         IList<StringSegment> keyValues = line.Split('=', maxLength: 2);
    ///
    ///         // Removing white spaces and returning values if the key matches
    ///         if (keyValues[0].TrimStart() == key)
    ///             return keyValues[1].Split(';');
    ///     }
    ///
    ///     // key not found
    ///     return null;
    /// }]]></code>
    /// </para>
    /// </example>
    /// </remarks>
    [Serializable]
    [TypeConverter(typeof(StringSegmentConverter))]
    [SuppressMessage("Design", "CA1036:Override methods on comparable types",
            Justification = "Not implementing <, <=, >, >= operators because even string does not implement them")]
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public readonly partial struct StringSegment : IEquatable<StringSegment>, IComparable<StringSegment>, IComparable, IEnumerable<char>
#if !(NET35 || NET40)
        , IReadOnlyList<char>
#endif
    {
        #region Enumerator struct

        /// <summary>
        /// Enumerates the characters of a <see cref="StringSegment"/>.
        /// </summary>
        [Serializable]
        public struct Enumerator : IEnumerator<char>
        {
            #region Fields

            private StringSegment segment;
            private int index;
            private char current;

            #endregion

            #region Properties

            #region Public Properties

            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            public char Current => current;

            #endregion

            #region Explicitly Implemented Interface Properties

            object IEnumerator.Current
            {
                get
                {
                    if (index == 0 || index > segment.Length)
                        Throw.InvalidOperationException(Res.IEnumeratorEnumerationNotStartedOrFinished);
                    return current;
                }
            }

            #endregion

            #endregion

            #region Constructors

            internal Enumerator(in StringSegment segment)
            {
                this.segment = segment;
                index = 0;
                current = default;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Releases the enumerator
            /// </summary>
            public void Dispose()
            {
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            /// <see langword="true"/>&#160;if the enumerator was successfully advanced to the next element; <see langword="false"/>&#160;if the enumerator has passed the end of the collection.
            /// </returns>
            /// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
            public bool MoveNext()
            {
                if (index < segment.Length)
                {
                    current = segment.GetCharInternal(index);
                    index += 1;
                    return true;
                }

                current = default;
                return false;
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            /// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
            public void Reset()
            {
                index = 0;
                current = default;
            }

            #endregion
        }

        #endregion

        #region Fields

        #region Static Fields

        public static readonly StringSegment Empty = String.Empty;

        public static readonly StringSegment Null = default;

        #endregion

        #region Instance Fields

        private readonly string str;
        private readonly int offset;
        private readonly int length;

        #endregion

        #endregion

        #region Properties and Indexers

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets the length of this <see cref="StringSegment"/>.
        /// </summary>
        public int Length => length;

        /// <summary>
        /// Gets the underlying string of this <see cref="StringSegment"/>.
        /// </summary>
        public string UnderlyingString => str;

        /// <summary>
        /// Gets the offset, which denotes the start position of this <see cref="StringSegment"/> within the <see cref="UnderlyingString"/>.
        /// </summary>
        public int Offset => offset;

        /// <summary>
        /// Gets whether this <see cref="StringSegment"/> instance was created from a <see langword="null"/>&#160;<see cref="string"/>.
        /// <br/>Please note that the <see cref="ToString">ToString</see> method returns <see langword="null"/>&#160;when this property returns <see langword="true"/>.
        /// </summary>
        public bool IsNull => str == null;

        /// <summary>
        /// Gets whether this <see cref="StringSegment"/> instance represents an empty segment or was created from a <see langword="null"/>&#160;<see cref="string"/>.
        /// </summary>
        public bool IsNullOrEmpty => length == 0;

#if !(NETFRAMEWORK || NETSTANDARD2_0 || NETCOREAPP2_0)
        /// <summary>
        /// Returns the current <see cref="StringSegment"/> instance as a <see cref="ReadOnlySpan{T}"/> of characters.
        /// <br/>This member is available in .NET Core 3.0/.NET Standard 2.1 and above
        /// </summary>
        public ReadOnlySpan<char> AsSpan => str.AsSpan(offset, length);

        /// <summary>
        /// Returns the current <see cref="StringSegment"/> instance as a <see cref="ReadOnlyMemory{T}"/> of characters.
        /// <br/>This member is available in .NET Core 3.0/.NET Standard 2.1 and above
        /// </summary>
        public ReadOnlyMemory<char> AsMemory => str.AsMemory(offset, length);
#endif

        #endregion

        #region Explicitly Implemented Properties

#if !(NET35 || NET40)
        int IReadOnlyCollection<char>.Count => length;
#endif

        #endregion

        #endregion

        #region Indexers

        /// <summary>
        /// Gets the character at the specified position in this <see cref="StringSegment"/>.
        /// </summary>
        /// <param name="index">The index of the character to obtain.</param>
        /// <returns>The character at the specified position in this <see cref="StringSegment"/>.</returns>
        public char this[int index]
        {
            [MethodImpl(MethodImpl.AggressiveInlining)]
            get
            {
                // For better performance we throw an ArgumentOutOfRangeException only when a NullReferenceException
                // would come otherwise, and let the ArgumentOutOfRangeException come from string, even if a not localized one.
                if (str == null)
                    Throw.ArgumentOutOfRangeException(Argument.index);
                return GetCharInternal(index);
            }
        }

        #endregion

        #endregion

        #region Operators

        /// <summary>
        /// Performs an implicit conversion from <see cref="string">string</see> to <see cref="StringSegment"/>.
        /// </summary>
        /// <param name="s">The string to be converted to a <see cref="StringSegment"/>.</param>
        /// <returns>
        /// A <see cref="StringSegment"/> instance that represents the original string.
        /// </returns>
        [SuppressMessage("Usage", "CA2225:Operator overloads have named alternates",
                Justification = "The named alternative exists in StringExtensions.AsSegment")]
        public static implicit operator StringSegment(string s) => s == null ? Null : new StringSegment(s);

        /// <summary>
        /// Performs an explicit conversion from <see cref="StringSegment"/> to <see cref="string">string</see>.
        /// </summary>
        /// <param name="stringSegment">The <see cref="StringSegment"/> to be converted to a string.</param>
        /// <returns>
        /// A <see cref="string">string</see> instance that represents the specified <see cref="StringSegment"/>.
        /// </returns>
        public static explicit operator string(in StringSegment stringSegment) => stringSegment.ToString();

        #endregion

        #region Constructors

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal StringSegment(string s, int offset, int length)
        {
            Debug.Assert(s != null);
            str = s;
            this.offset = offset;
            this.length = length;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal StringSegment(string s)
        {
            Debug.Assert(s != null);
            str = s;
            offset = 0;
            length = s.Length;
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Returns a hash code for this <see cref="StringSegment"/> instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            if (str == null)
                return 0;

#if NETFRAMEWORK || NETCOREAPP2_0 || NETSTANDARD2_0 || NETSTANDARD2_1
            // This does not use a randomized hash but at least this way we don't allocate a new string
            var result = 13;
            for (int i = 0; i < length; i++)
                result = result * 397 + GetCharInternal(i);

            return result;
#else
            return String.GetHashCode(AsSpan);
#endif


        }

        public int GetHashCode(StringComparison comparison)
        {
            switch (comparison)
            {
                case StringComparison.Ordinal:
                    return GetHashCode();
                case StringComparison.OrdinalIgnoreCase:
                    return GetHashCodeOrdinalIgnoreCase();

                case StringComparison.CurrentCulture:
                case StringComparison.CurrentCultureIgnoreCase:
                case StringComparison.InvariantCulture:
                case StringComparison.InvariantCultureIgnoreCase:
                    return StringSegmentComparer.FromComparison(comparison).GetHashCode(this);

                default:
                    Throw.EnumArgumentOutOfRange(Argument.comparison, comparison);
                    return default;
            }
        }

        /// <summary>
        /// Gets a <see cref="string">string</see> that is represented by this <see cref="StringSegment"/> instance, or <see langword="null"/>, if
        /// this instance represents a <see langword="null"/>&#160;<see cref="string">string</see>. That is, when the <see cref="IsNull"/> property returns <see langword="true"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="string">string</see> that is represented by this <see cref="StringSegment"/> instance, or <see langword="null"/>, if
        /// this instance was created from a <see langword="null"/>&#160;<see cref="string">string</see>.
        /// </returns>
        /// <returns>
        /// <note>As opposed to the usual <a href="https://docs.microsoft.com/en-us/dotnet/api/system.object.tostring#notes-to-inheritors" target="_blank">ToString guidelines</a>
        /// this method can return <see cref="String.Empty">String.Empty</see> or even <see langword="null"/>.</note>
        /// </returns>
        public override string ToString()
            => str == null ? null
            : length == str.Length ? str
            : str.Substring(offset, length);

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="StringSegment"/> characters.
        /// </summary>
        /// <returns>An <see cref="Enumerator"/> instance that can be used to iterate though the characters of the <see cref="StringSegment"/>.</returns>
        /// <remarks>
        /// <note>The returned enumerator supports the <see cref="IEnumerator.Reset">IEnumerator.Reset</see> method.</note>
        /// </remarks>
        public Enumerator GetEnumerator() => new Enumerator(this);

        #endregion

        #region Internal Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal char GetCharInternal(int index) => str[offset + index];

        internal int GetHashCodeOrdinalIgnoreCase()
        {
            if (str == null)
                return 0;

#if NETFRAMEWORK || NETCOREAPP2_0 || NETSTANDARD2_0 || NETSTANDARD2_1
            var result = 13;
            for (int i = 0; i < length; i++)
                result = result * 397 + Char.ToUpperInvariant(GetCharInternal(i));

            return result;
#else
            return String.GetHashCode(AsSpan, StringComparison.OrdinalIgnoreCase);
#endif
        }

        #endregion

        #region Explicitly Implemented Interface Methods

        IEnumerator<char> IEnumerable<char>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #endregion
    }
}
