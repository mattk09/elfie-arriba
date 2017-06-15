﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using V5.Data;

namespace V5.Test
{
    [TestClass]
    public class SortBucketColumnNTests
    {
        [TestMethod]
        public void SortBucketColumnN_BinarySearch()
        {
            long[] buckets = new long[256];
            for (int i = 0; i < buckets.Length; ++i) buckets[i] = 2 * i;

            Assert.AreEqual(-1, SortBucketColumnN.BucketIndex(buckets, -1));
            for (int i = 0; i < 510; ++i)
            {
                int expected = i / 2;
                if ((i & 1) != 0) expected = ~expected;

                Assert.AreEqual(expected, SortBucketColumnN.BucketIndex(buckets, i));
            }
            Assert.AreEqual(~254, SortBucketColumnN.BucketIndex(buckets, 510));
            Assert.AreEqual(~254, SortBucketColumnN.BucketIndex(buckets, 512));

            buckets = new long[] { -1, 10, 20, 30, 50, 100, 1000, 1200 };
            Assert.AreEqual(~1, SortBucketColumnN.BucketIndex(buckets, 11));
            Assert.AreEqual(1, SortBucketColumnN.BucketIndex(buckets, 10));
            Assert.AreEqual(-1, SortBucketColumnN.BucketIndex(buckets, -2));
            Assert.AreEqual(0, SortBucketColumnN.BucketIndex(buckets, -1));
            Assert.AreEqual(2, SortBucketColumnN.BucketIndex(buckets, 20));
            Assert.AreEqual(~5, SortBucketColumnN.BucketIndex(buckets, 999));
            Assert.AreEqual(6, SortBucketColumnN.BucketIndex(buckets, 1000));
            Assert.AreEqual(~6, SortBucketColumnN.BucketIndex(buckets, 1001));
            Assert.AreEqual(~6, SortBucketColumnN.BucketIndex(buckets, 1200));
            Assert.AreEqual(~6, SortBucketColumnN.BucketIndex(buckets, 1201));
        }

        [TestMethod]
        public void SortBucketColumn_Basics()
        {
            // Try buckets for all values from 0 - 9999 [should all be multi-value; min and max should've been found]
            int[] allUnique = Enumerable.Range(0, 10000).ToArray();
            SortBucketColumn<int> sbc = SortBucketColumn<int>.Build(allUnique, 256, new Random(5));
            Validate(sbc, allUnique);

            // Try buckets for only 32 unique values
            int[] fewDistinct = new int[10000];
            for(int i = 0; i < fewDistinct.Length; ++i)
            {
                fewDistinct[i] = i % 32;
            }

            sbc = SortBucketColumn<int>.Build(fewDistinct, 256, new Random(5));
            Validate(sbc, fewDistinct);

            // Should have 33 buckets (each distinct value and a copy of the max)
            Assert.AreEqual(33, sbc.Minimum.Length);
            
            // Every bucket should be single value
            for(int i = 0; i < sbc.Minimum.Length - 1; ++i)
            {
                Assert.IsFalse(sbc.IsMultiValue[i]);
            }
        }

        private static void Validate<T>(SortBucketColumn<T> sbc, T[] values) where T : IComparable<T>
        {
            T min = values[0];
            T max = values[0];

            bool[] isMultiValue = new bool[sbc.Minimum.Length];
            int[] countPerBucket = new int[sbc.Minimum.Length];

            // Iterate over items and check bucket values
            for(int i = 0; i < values.Length; ++i)
            {
                // Track the minimum and maximum in the set
                if (values[i].CompareTo(min) < 0) min = values[i];
                if (values[i].CompareTo(max) > 0) max = values[i];

                int bucketIndex = sbc.RowBucketIndex[i];

                // Verify the value is within boundaries
                Assert.IsTrue(values[i].CompareTo(sbc.Minimum[bucketIndex]) >= 0);
                Assert.IsTrue(values[i].CompareTo(sbc.Minimum[bucketIndex + 1]) < 0 || values[i].CompareTo(sbc.Max) == 0);

                // Track row counts
                countPerBucket[bucketIndex]++;

                // Track bucket multi-value-ness
                if (values[i].CompareTo(sbc.Minimum[bucketIndex]) != 0) isMultiValue[bucketIndex] = true;
            }

            // Validate bucket aggregates (except last sentinel bucket)
            for(int i = 0; i < sbc.Minimum.Length - 1; ++i)
            {
                Assert.AreEqual(countPerBucket[i], sbc.RowCount[i]);
                Assert.AreEqual(isMultiValue[i], sbc.IsMultiValue[i]);
            }

            // Verify the min and max were found
            Assert.AreEqual(min, sbc.Min);
            Assert.AreEqual(max, sbc.Max);

            // Verify the total is correct
            Assert.AreEqual(values.Length, sbc.Total);

            // Verify the rowCounts add up to the total (and the last rowCount is the total)
            Assert.AreEqual(values.Length, sbc.RowCount[sbc.RowCount.Length - 1]);
            Assert.AreEqual(2 * values.Length, sbc.RowCount.Sum());
        }
    }
}