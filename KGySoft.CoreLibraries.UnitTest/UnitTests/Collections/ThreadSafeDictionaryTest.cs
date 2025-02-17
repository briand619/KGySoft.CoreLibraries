﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ThreadSafeDictionaryTest.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using KGySoft.Collections;
using KGySoft.Reflection;
using KGySoft.Threading;

using NUnit.Framework;

#endregion

namespace KGySoft.CoreLibraries.UnitTests.Collections
{
    [TestFixture]
    public class ThreadSafeDictionaryTest
    {
        #region Fields

        private static readonly TimeSpan defaultTimeout = TimeSpan.FromMilliseconds(100);
        private static readonly TimeSpan infiniteTimeout =
#if NET35 || NET40
            TimeSpan.FromMilliseconds(Timeout.Infinite);
#else
            Timeout.InfiniteTimeSpan;
#endif

        private static readonly object[] usageTestSource =
        {
            new object[] { "Default settings", null, HashingStrategy.Auto, defaultTimeout },
            new object[] { "Explicit comparer", ComparerHelper<string>.EqualityComparer, HashingStrategy.Auto, defaultTimeout },
            new object[] { "MOD hashing", null, HashingStrategy.Modulo, defaultTimeout },
            new object[] { "AND hashing", null, HashingStrategy.And, defaultTimeout },
            new object[] { "No merge", null, HashingStrategy.And, infiniteTimeout },
            new object[] { "Immediate merge", null, HashingStrategy.And, TimeSpan.Zero },
            new object[] { "1 ms merge", null, HashingStrategy.And, TimeSpan.FromMilliseconds(1) },
        };

        #endregion

        #region Methods

        [TestCase(false)]
        [TestCase(true)]
        public void FixedSizeStorageUsageTest(bool ignoreCase)
        {
            Assert.IsTrue(ThreadSafeDictionary<string, int>.FixedSizeStorage.TryInitialize(new Dictionary<string, int>
            {
                ["alpha"] = 1,
                ["beta"] = 2,
                ["gamma"] = 3,
            }, default, ignoreCase ? StringComparer.OrdinalIgnoreCase : ComparerHelper<string>.EqualityComparer, out var dict));

            Assert.AreEqual(3, dict.Count);
            Assert.AreEqual(1, dict["alpha"]);

            // TryAdd
            Assert.IsFalse(dict.TryAdd("alpha", 11));
            Assert.IsFalse(dict.TryAdd("delta", 4));
            Assert.AreEqual(3, dict.Count);

            // Update
            dict["alpha"] = 11;
            Assert.AreEqual(3, dict.Count);
            Assert.AreEqual(11, dict["alpha"]);

            // Remove
            Assert.IsFalse(dict.TryRemove("alpha", 1));
            Assert.IsTrue(dict.TryRemove("alpha", out int value));
            Assert.AreEqual(11, value);
            Assert.AreEqual(2, dict.Count);
            Assert.IsFalse(dict.TryRemove("alpha", out value));
            Assert.AreEqual(0, value);
            Assert.IsTrue(dict.Remove("beta"));

            // Re-add
            Assert.IsTrue(dict.TryAdd("alpha", -1));
            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual(-1, dict["alpha"]);

            // Replace
            Assert.IsFalse(dict.TryReplace("alpha", 1, 11));
            Assert.IsTrue(dict.TryReplace("alpha", 1, -1));
            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual(1, dict["alpha"]);

            // Clear
            dict.Clear();
            Assert.AreEqual(0, dict.Count);
            Assert.IsTrue(dict.TryAdd("alpha", 42));
            Assert.AreEqual(42, dict["alpha"]);
            Assert.AreEqual(1, dict.Count);

            // GetOrAdd
            Assert.IsTrue(dict.TryGetOrAdd("alpha", 100, out value));
            Assert.AreEqual(42, value);
            Assert.AreEqual(1, dict.Count);
            Assert.IsTrue(dict.TryGetOrAdd("beta", 100, out value));
            Assert.AreEqual(100, value);
            Assert.AreEqual(2, dict.Count);
            Assert.IsFalse(dict.TryGetOrAdd("delta", 100, out var _));
            Assert.AreEqual(2, dict.Count);

            // AddOrUpdate
            Assert.IsTrue(dict.TryAddOrUpdate("alpha", 100, (_, v) => v + 1, out value));
            Assert.AreEqual(43, value);
            Assert.AreEqual(2, dict.Count);
            Assert.IsTrue(dict.TryAddOrUpdate("gamma", 100, (_, v) => v + 1, out value));
            Assert.AreEqual(100, value);
            Assert.AreEqual(3, dict.Count);
            Assert.IsFalse(dict.TryAddOrUpdate("delta", 100, (_, v) => v + 1, out value));
            Assert.AreEqual(3, dict.Count);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TempStorageUsageTest(bool ignoreCase)
        {
            var dict = new ThreadSafeDictionary<string, int>.TempStorage(2, ignoreCase ? StringComparer.OrdinalIgnoreCase : ComparerHelper<string>.EqualityComparer, default);

            // Add
            dict.Add("alpha", 1);
            dict.Add("beta", 2);
            dict.Add("gamma", 3); // here resize is needed
            Assert.AreEqual(3, dict.Count);
            Assert.AreEqual(1, dict["alpha"]);

            // TryAdd
            Assert.IsFalse(dict.TryAdd("alpha", 11));
            Assert.IsTrue(dict.TryAdd("delta", 4));
            Assert.AreEqual(4, dict.Count);

            // Update
            dict["alpha"] = 11;
            Assert.AreEqual(4, dict.Count);
            Assert.AreEqual(11, dict["alpha"]);

            // Remove
            Assert.IsFalse(dict.TryRemove("alpha", 1));
            Assert.IsTrue(dict.TryRemove("alpha", out int value));
            Assert.AreEqual(11, value);
            Assert.AreEqual(3, dict.Count);
            Assert.IsFalse(dict.TryRemove("alpha", out value));
            Assert.AreEqual(0, value);
            Assert.IsTrue(dict.Remove("beta"));

            // Re-add
            dict.Add("alpha", -1);
            Assert.AreEqual(3, dict.Count);
            Assert.AreEqual(-1, dict["alpha"]);

            // Replace
            Assert.IsFalse(dict.TryReplace("alpha", 1, 11));
            Assert.IsTrue(dict.TryReplace("alpha", 1, -1));
            Assert.AreEqual(3, dict.Count);
            Assert.AreEqual(1, dict["alpha"]);

            // Clear
            dict.Clear();
            Assert.AreEqual(0, dict.Count);
            dict.Add("alpha", 42);
            Assert.AreEqual(42, dict["alpha"]);
            Assert.AreEqual(1, dict.Count);

            // GetOrAdd
            Assert.AreEqual(42, dict.GetOrAdd("alpha", 100));
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual(100, dict.GetOrAdd("beta", 100));
            Assert.AreEqual(2, dict.Count);

            // AddOrUpdate
            Assert.AreEqual(43, dict.AddOrUpdate("alpha", 100, (_, v) => v + 1));
            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual(100, dict.AddOrUpdate("gamma", 100, (_, v) => v + 1));
            Assert.AreEqual(3, dict.Count);
        }

        [TestCaseSource(nameof(usageTestSource))]
        public void UsageTestRefType(string testName, IEqualityComparer<string> comparer, HashingStrategy strategy, TimeSpan mergeInterval)
        {
            Console.WriteLine(testName);

            var initialValues = new Dictionary<string, int>
            {
                ["alpha"] = 1,
                ["beta"] = 2,
                ["gamma"] = 3,
            };

            var dict = new ThreadSafeDictionary<string, int>(initialValues, comparer, strategy) { MergeInterval = mergeInterval };

            Assert.AreEqual(3, dict.Count);
            Assert.AreEqual(1, dict["alpha"]);

            // TryAdd
            Assert.IsFalse(dict.TryAdd("alpha", 11));
            Assert.IsTrue(dict.TryAdd("delta", 4)); // to locking
            Assert.AreEqual(4, dict.Count);

            // Update
            dict["alpha"] = 11; // in non-locking
            Assert.AreEqual(4, dict.Count);
            Assert.AreEqual(11, dict["alpha"]);
            dict["delta"] = 44; // maybe in locking
            Assert.AreEqual(4, dict.Count);
            Assert.AreEqual(44, dict["delta"]);

            // Remove
            Assert.IsFalse(dict.TryRemove("alpha", 1));
            Assert.IsTrue(dict.TryRemove("alpha", out int value));
            Assert.AreEqual(11, value);
            Assert.AreEqual(3, dict.Count);
            Assert.IsFalse(dict.TryRemove("alpha", out value));
            Assert.AreEqual(0, value);
            Assert.IsFalse(dict.TryRemove("delta", 4));
            Assert.IsTrue(dict.TryRemove("delta")); // maybe from locking
            Assert.AreEqual(2, dict.Count);

            // Re-add
            Assert.IsTrue(dict.TryAdd("alpha", -1)); // in non-locking
            Assert.AreEqual(3, dict.Count);
            Assert.AreEqual(-1, dict["alpha"]);
            Assert.IsTrue(dict.TryAdd("delta", -4)); // maybe in locking unless already merged
            Assert.AreEqual(4, dict.Count);
            Assert.AreEqual(-4, dict["delta"]);

            // Replace
            Assert.IsFalse(dict.TryUpdate("alpha", 1, 11));
            Assert.IsTrue(dict.TryUpdate("alpha", 1, -1)); // in non-locking
            Assert.AreEqual(4, dict.Count);
            Assert.AreEqual(1, dict["alpha"]);
            Assert.IsTrue(dict.TryUpdate("delta", 4, -4)); // maybe in locking
            Assert.AreEqual(4, dict.Count);
            Assert.AreEqual(4, dict["delta"]);

            // Clear
            dict.Clear();
            Assert.AreEqual(0, dict.Count);
            Assert.IsTrue(dict.TryAdd("alpha", 42)); // in non-locking, even after clear
            Assert.AreEqual(42, dict["alpha"]);
            Assert.AreEqual(1, dict.Count);
            Assert.IsTrue(dict.TryAdd("delta", 13)); // maybe in locking unless already merged
            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual(13, dict["delta"]);

            // GetOrAdd
            Assert.AreEqual(42, dict.GetOrAdd("alpha", 100));
            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual(100, dict.GetOrAdd("beta", 100)); // maybe in locking
            Assert.AreEqual(3, dict.Count);

            // AddOrUpdate
            Assert.AreEqual(43, dict.AddOrUpdate("alpha", 100, (_, v) => v + 1)); // in non-locking
            Assert.AreEqual(3, dict.Count);
            Assert.AreEqual(100, dict.AddOrUpdate("epsilon", 100, (_, v) => v + 1)); // maybe in locking
            Assert.AreEqual(4, dict.Count);

            // Forcing merge
            dict.EnsureMerged();
            Assert.AreEqual(4, dict.Count);
            Assert.AreEqual(43, dict["alpha"]);
            Assert.AreEqual(13, dict["delta"]);
        }

        [TestCase(true)]
        [TestCase(false)]
        [TestCase(null)]
        public void UsageTestValueType(bool? customComparer)
        {
            var initialValues = new Dictionary<ConsoleColor, string>
            {
                [ConsoleColor.Red] = "Red",
                [ConsoleColor.Green] = "Green",
                [ConsoleColor.Blue] = "Blue",
            };

            IEqualityComparer<ConsoleColor> comparer = customComparer switch
            {
                true => EnumComparer<ConsoleColor>.Comparer,
                false => EqualityComparer<ConsoleColor>.Default,
                _ => null,
            };
            var dict = new ThreadSafeDictionary<ConsoleColor, string>(initialValues, comparer);

            Assert.AreEqual(3, dict.Count);
            Assert.AreEqual("Red", dict[ConsoleColor.Red]);

            // TryAdd
            Assert.IsFalse(dict.TryAdd(ConsoleColor.Red, "red"));
            Assert.IsTrue(dict.TryAdd(ConsoleColor.Cyan, "Cyan")); // to locking
            Assert.AreEqual(4, dict.Count);

            // Update
            dict[ConsoleColor.Red] = "red"; // in non-locking
            Assert.AreEqual(4, dict.Count);
            Assert.AreEqual("red", dict[ConsoleColor.Red]);
            dict[ConsoleColor.Cyan] = "cyan"; // maybe in locking
            Assert.AreEqual(4, dict.Count);
            Assert.AreEqual("cyan", dict[ConsoleColor.Cyan]);

            // Remove
            Assert.IsFalse(dict.TryRemove(ConsoleColor.Red, "Red"));
            Assert.IsTrue(dict.TryRemove(ConsoleColor.Red, out string value));
            Assert.AreEqual("red", value);
            Assert.AreEqual(3, dict.Count);
            Assert.IsFalse(dict.TryRemove(ConsoleColor.Red, out value));
            Assert.AreEqual(null, value);
            Assert.IsFalse(dict.TryRemove(ConsoleColor.Cyan, "Cyan"));
            Assert.IsTrue(dict.TryRemove(ConsoleColor.Cyan)); // maybe from locking
            Assert.AreEqual(2, dict.Count);

            // Re-add
            Assert.IsTrue(dict.TryAdd(ConsoleColor.Red, "RED")); // in non-locking
            Assert.AreEqual(3, dict.Count);
            Assert.AreEqual("RED", dict[ConsoleColor.Red]);
            Assert.IsTrue(dict.TryAdd(ConsoleColor.Cyan, "CYAN")); // maybe in locking unless already merged
            Assert.AreEqual(4, dict.Count);
            Assert.AreEqual("CYAN", dict[ConsoleColor.Cyan]);

            // Replace
            Assert.IsFalse(dict.TryUpdate(ConsoleColor.Red, "Red", "red"));
            Assert.IsTrue(dict.TryUpdate(ConsoleColor.Red, "Red", "RED")); // in non-locking
            Assert.AreEqual(4, dict.Count);
            Assert.AreEqual("Red", dict[ConsoleColor.Red]);
            Assert.IsTrue(dict.TryUpdate(ConsoleColor.Cyan, "Cyan", "CYAN")); // maybe in locking
            Assert.AreEqual(4, dict.Count);
            Assert.AreEqual("Cyan", dict[ConsoleColor.Cyan]);

            // Clear
            dict.Clear();
            Assert.AreEqual(0, dict.Count);
            Assert.IsTrue(dict.TryAdd(ConsoleColor.Red, "reD")); // in non-locking, even after clear
            Assert.AreEqual("reD", dict[ConsoleColor.Red]);
            Assert.AreEqual(1, dict.Count);
            Assert.IsTrue(dict.TryAdd(ConsoleColor.Cyan, "cyan")); // maybe in locking unless already merged
            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual("cyan", dict[ConsoleColor.Cyan]);

            // GetOrAdd
            Assert.AreEqual("reD", dict.GetOrAdd(ConsoleColor.Red, "red"));
            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual("green", dict.GetOrAdd(ConsoleColor.Green, "green")); // maybe in locking
            Assert.AreEqual(3, dict.Count);

            // AddOrUpdate
            Assert.AreEqual("Der", dict.AddOrUpdate(ConsoleColor.Red, "Red", (_, v) => new String(v.Reverse().ToArray()))); // in non-locking
            Assert.AreEqual(3, dict.Count);
            Assert.AreEqual("Blue", dict.AddOrUpdate(ConsoleColor.Blue, "Blue", (_, v) => new String(v.Reverse().ToArray()))); // maybe in locking
            Assert.AreEqual(4, dict.Count);

            // Forcing merge
            dict.EnsureMerged();
            Assert.AreEqual(4, dict.Count);
            Assert.AreEqual("Der", dict[ConsoleColor.Red]);
            Assert.AreEqual("cyan", dict[ConsoleColor.Cyan]);
        }

        [Test]
        public void InitFromCollectionTest()
        {
            var dict = new Dictionary<string, int>
            {
                ["alpha"] = 1,
                ["beta"] = 2,
                ["gamma"] = 3,
            };

            // initializing as collection
            Assert.IsTrue(dict.SequenceEqual(new ThreadSafeDictionary<string, int>(dict)));

            // initializing as enumerable
            Assert.IsTrue(dict.SequenceEqual(new ThreadSafeDictionary<string, int>(dict.Where(_ => true))));
        }

        [Test]
        public void EnumerationTest()
        {
            var referenceValues = new Dictionary<string, int>
            {
                ["alpha"] = 1,
                ["beta"] = 2,
                ["gamma"] = 3,
            };

            var dict = new ThreadSafeDictionary<string, int>(referenceValues);
            Assert.IsTrue(referenceValues.SequenceEqual(dict));

            var keys = dict.Select(c => c.Key);
            Assert.IsTrue(keys.SequenceEqual(dict.Keys));
            var values = dict.Select(c => c.Value);
            Assert.IsTrue(values.SequenceEqual(dict.Values));
        }

        [Test]
        public void CopyToTest()
        {
            var dict = new ThreadSafeDictionary<string, int>
            {
                ["alpha"] = 1,
                ["beta"] = 2,
                ["gamma"] = 3,
            };

            var arr = new KeyValuePair<string, int>[dict.Count];
            ((ICollection<KeyValuePair<string, int>>)dict).CopyTo(arr, 0);
            Assert.IsTrue(arr.SequenceEqual(dict));
        }

        [Test]
        public void ToArrayTest()
        {
            var dict = new Dictionary<string, int>
            {
                ["alpha"] = 1,
                ["beta"] = 2,
                ["gamma"] = 3,
            };

            var tDict = new ThreadSafeDictionary<string, int>(dict);

            Assert.IsTrue(dict.SequenceEqual(tDict));
            Assert.IsTrue(dict.ToArray().SequenceEqual(tDict.ToArray()));

            tDict.TryRemove("alpha");
            Assert.AreEqual(2, tDict.Count);
            Assert.AreEqual(2, tDict.ToArray().Length);
        }

#if !NETFRAMEWORK
        [Obsolete]
#endif
        [Test]
        public void SerializationTest()
        {
            var dict = new ThreadSafeDictionary<string, int>
            {
                ["alpha"] = 1,
                ["beta"] = 2,
                ["gamma"] = 3,
            };

            ThreadSafeDictionary<string, int> clone = dict.DeepClone(false);
            Assert.IsTrue(dict.SequenceEqual(clone));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void PreserveMergedKeysTest(bool preserveMergedKeys)
        {
            const int count = 100;
            var dict = new ThreadSafeDictionary<int, string>(Enumerable.Range(0, count).ToDictionary(i => i, i => i.ToString()))
            {
                MergeInterval = infiniteTimeout,
                PreserveMergedKeys = preserveMergedKeys
            };

            dict.EnsureMerged();

            // removing half of the elements + 1 from the lock free storage
            for (int i = 0; i < count / 2 + 1; i++)
                Assert.IsTrue(dict.TryRemove(i));

            // adding an element to the locking storage so there will be something to merge
            dict[-1] = "-1";

            // performing a new merge with 51 deleted keys
            dict.EnsureMerged();

            Assert.AreEqual(count / 2, dict.Count);
            Assert.AreEqual(preserveMergedKeys ? count / 2 + 1 : 0, ((ThreadSafeDictionary<int, string>.FixedSizeStorage)Reflector.GetField(dict, "fixedSizeStorage"))!.DeletedCount);
            dict.TrimExcess();
            Assert.AreEqual(0, ((ThreadSafeDictionary<int, string>.FixedSizeStorage)Reflector.GetField(dict, "fixedSizeStorage"))!.DeletedCount);
            Assert.AreEqual(count / 2, dict.Count);
        }

        [Test]
        public void LookupMergeRaceConditionTest()
        {
            const string key = "Key";

            // Initializing without passing a collection to the constructor and then adding one element: it will be in the temp locking storage
            var dict = new ThreadSafeDictionary<string, object> { [key] = true };

            // Waiting for the merge timeout
            Thread.Sleep(dict.MergeInterval);

            bool[] results = new bool[100];
            ParallelHelper.For(0, results.Length,
                y => results[y] = dict.ContainsKey(key));

            CollectionAssert.DoesNotContain(results, false);
        }

        [Test]
        public void InsertRaceConditionTest()
        {
            const string key = "Key";

            // Initializing without passing a collection to the constructor and then adding one element: it will be in the temp locking storage
            var dict = new ThreadSafeDictionary<string, object> { [key] = true };

            // Waiting for the merge timeout
            Thread.Sleep(dict.MergeInterval);

            bool[] results = new bool[100];
            ParallelHelper.For(0, results.Length,
                y => results[y] = dict.TryAdd(key, false));

            CollectionAssert.DoesNotContain(results, true);
        }

        [Test]
        public void ReplaceRaceConditionTest()
        {
            const string key = "Key";

            // Initializing without passing a collection to the constructor and then adding one element: it will be in the temp locking storage
            var dict = new ThreadSafeDictionary<string, object> { [key] = true };

            // Waiting for the merge timeout
            Thread.Sleep(dict.MergeInterval);

            bool[] results = new bool[100];
            ParallelHelper.For(0, results.Length,
                y => results[y] = dict.TryUpdate(key, true, false));

            CollectionAssert.DoesNotContain(results, true);
        }

        [Test]
        public void RemoveRaceConditionTest()
        {
            const string key = "Key";

            // Initializing without passing a collection to the constructor and then adding one element: it will be in the temp locking storage
            var dict = new ThreadSafeDictionary<string, object> { [key] = true };

            // Waiting for the merge timeout
            Thread.Sleep(dict.MergeInterval);

            bool[] results = new bool[100];
            ParallelHelper.For(0, results.Length,
                y => results[y] = dict.TryRemove(key));

            Assert.AreEqual(1, results.Count(r => r));
        }

        #endregion
    }
}
