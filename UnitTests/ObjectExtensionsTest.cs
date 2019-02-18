﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XstarS
{
    [TestClass]
    public class ObjectExtensionsTest
    {
        [TestMethod]
        public void DeepClone_Int32Array_WorksProperly()
        {
            var o1 = new int[] { 1 };
            var o2 = o1.Clone() as int[];
            var o3 = o1.DeepClone() as int[];
            o2[0] = 2;
            o3[0] = 3;
            Assert.AreEqual(o1[0], 1);
            Assert.AreEqual(o2[0], 2);
            Assert.AreEqual(o3[0], 3);
        }

        [TestMethod]
        public void DeepClone_NestedInt32Array_WorksProperly()
        {
            var o1 = new int[][] { new int[] { 1 } };
            var o2 = o1.Clone() as int[][];
            var o3 = o1.DeepClone() as int[][];
            o2[0][0] = 2;
            o3[0][0] = 3;
            Assert.AreNotEqual(o1[0][0], 1);
            Assert.AreEqual(o1[0][0], 2);
            Assert.AreEqual(o2[0][0], 2);
            Assert.AreEqual(o3[0][0], 3);
        }

        [TestMethod]
        public void ValueEquals_String_ReturnsTrue()
        {
            var o1 = "object";
            var o2 = "object";
            Assert.IsTrue(object.ReferenceEquals(o1, o2));
            Assert.IsTrue(o1.Equals(o2));
            Assert.IsTrue(o1.ValueEquals(o2));
        }

        [TestMethod]
        public void ValueEquals_Object_ReturnsTrue()
        {
            var o1 = new object();
            var o2 = new object();
            Assert.IsFalse(o1.Equals(o2));
            Assert.IsTrue(o1.ValueEquals(o2));
        }

        [TestMethod]
        public void ValueEquals_RandomBySeed_ReturnsTrue()
        {
            var o1 = new Random(typeof(Random).GetHashCode());
            var o2 = new Random(typeof(Random).GetHashCode());
            Assert.IsFalse(o1.Equals(o2));
            Assert.IsTrue(o1.ValueEquals(o2));
        }

        [TestMethod]
        public void ValueEquals_KeyValuePair_ReturnsTrue()
        {
            var o1 = new KeyValuePair<string, int>("0", 0);
            var o2 = new KeyValuePair<string, int>("0", 0);
            Assert.IsTrue(o1.Equals(o2));
            Assert.IsTrue(o1.ValueEquals(o2));
        }

        [TestMethod]
        public void ValueEquals_Int32Array_ReturnsTrue()
        {
            var o1 = new int[100];
            var o2 = new int[100];
            Assert.IsFalse(o1.Equals(o2));
            Assert.IsTrue(o1.SequenceEqual(o2));
            Assert.IsTrue(o1.ValueEquals(o2));
        }

        [TestMethod]
        public void ValueEquals_Int32MultiDimArray_ReturnsFalse()
        {
            var o1 = new int[100, 2, 3];
            var o2 = new int[2, 3, 100];
            Assert.IsFalse(o1.Equals(o2));
            var etor1 = o1.GetEnumerator();
            var etor2 = o2.GetEnumerator();
            while (etor1.MoveNext() & etor2.MoveNext())
            {
                Assert.IsTrue(etor1.Current.Equals(etor2.Current));
            }
            Assert.IsTrue(etor1.MoveNext() == etor2.MoveNext());
            Assert.IsFalse(o1.ValueEquals(o2));
        }

        [TestMethod]
        public void ValueEquals_StringList_ReturnsTrue()
        {
            var o1 = new List<string>() { "list", "set", "dictionary" };
            var o2 = new List<string>() { "list", "set", "dictionary" };
            Assert.IsFalse(o1.Equals(o2));
            Assert.IsTrue(o1.SequenceEqual(o2));
            Assert.IsTrue(o1.ValueEquals(o2));
        }

        [TestMethod]
        public void ValueEquals_Int32HashSet_ReturnsFalse()
        {
            var o1 = new HashSet<int>() { 1, 2, 3 };
            var o2 = new HashSet<int>() { 2, 3, 1 };
            Assert.IsFalse(o1.Equals(o2));
            Assert.IsTrue(o1.SetEquals(o2));
            Assert.IsFalse(o1.SequenceEqual(o2));
            Assert.IsFalse(o1.ValueEquals(o2));
        }

        [TestMethod]
        public void ValueEquals_StringSortedSet_ReturnsTrue()
        {
            var o1 = new SortedSet<string>() { "list", "set", "dictionary" };
            var o2 = new SortedSet<string>() { "set", "dictionary", "list" };
            Assert.IsFalse(o1.Equals(o2));
            Assert.IsTrue(o1.SetEquals(o2));
            Assert.IsTrue(o1.SequenceEqual(o2));
            Assert.IsTrue(o1.ValueEquals(o2));
        }
    }
}
