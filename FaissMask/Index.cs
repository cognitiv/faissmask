using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using FaissMask.Extensions;
using FaissMask.Internal;

namespace FaissMask
{
    public class Index : IDisposable
    {
        internal readonly IndexSafeHandle Handle;

        public long Count
        {
            get => Handle.NTotal();
        }

        public bool IsTrained
        {
            get => Handle.IsTrained();
        }

        protected Index(object handle)
        {
            Handle = handle as IndexSafeHandle;
        }

        public void Add(float[] vector)
        {
            Add(1, vector);
        }

        public void Add(IReadOnlyCollection<float[]> vectors)
        {
            var count = vectors.Count;
            Add(count, vectors.SelectMany(v => v).ToArray());
        }

        private void Add(long count, float[] vectors)
        {
            Handle.Add(count, vectors);
        }

        public long[] Assign(float[] vectors, int k)
        {
            var labels = new long[k];
            Handle.Assign(vectors, labels, k);
            return labels;
        }

        public IEnumerable<SearchResult> Search(float[] vector, long kneigbors)
        {
            return Search(1, vector, kneigbors);
        }

        public IEnumerable<SearchResult> Search(IReadOnlyCollection<float[]> vectors, long kneighbors)
        {
            int count = vectors.Count;
            var vectorsFlattened = vectors.Flatten();

            return Search(count, vectorsFlattened, kneighbors);
        }

        private IEnumerable<SearchResult> Search(long count, float[] vectorsFlattened, long kneighbors)
        {
            float[] distances = new float[kneighbors * count];
            long[] labels = new long[kneighbors * count];
            Handle.Search(count, vectorsFlattened, kneighbors, distances, labels);
            var labelDistanceZip = labels.Zip(distances, (l, d) => new
            {
                Label = l,
                Distance = d
            });

            using var enumerable = labelDistanceZip.GetEnumerator();
            for (int i = 0; i < count; i++)
            {
                for (int r = 0; r < kneighbors; r++)
                {
                    enumerable.MoveNext();
                    if (enumerable.Current.Label == -1)
                    {
                        yield break;
                    }
                    
                    yield return new SearchResult
                    {
                        Label = enumerable.Current!.Label,
                        Distance = enumerable.Current.Distance
                    };
                }
            }
        }

        public RangeSearchResult RangeSearch(IReadOnlyCollection<float[]> vectors, float radius)
        {
            int count = vectors.Count;
            var vectorsFlattened = vectors.Flatten();

            return RangeSearch(count, vectorsFlattened, radius);
        }

        public RangeSearchResult RangeSearch(long count, float[] vectors, float radius)
        {
            return new RangeSearchResult(Handle.RangeSearch(count, vectors, radius));
        }

        public int Dimensions => Handle.Dimensions;

        public virtual bool CanRangeSearch => false;

        public float[] ReconstructVector(long key)
        {
            var ret = Handle.ReconstructVector(key);
            return ret;
        }

        public float[][] ReconstructVectors(long startKey, long amount)
        {
            var ret = Handle.ReconstructVectors(startKey, amount);
            return ret;
        }

        public static Index Create(int dimensions, string indexDescription, MetricType metric)
        {
            return new Index(IndexSafeHandle.FactoryCreate(dimensions, indexDescription, metric));
        }

        public ulong SaCodeSize => Handle.SaCodeSize;

        public byte[] EncodeVector(float[] vector, int numberOfCodes)
        {
            return Handle.EncodeVector(vector, numberOfCodes);
        }

        public float[] DecodeVector(byte[] bytes)
        {
            return Handle.DecodeVector(bytes);
        }

        public void Dispose()
        {
            Handle?.Dispose();
        }
    }
}