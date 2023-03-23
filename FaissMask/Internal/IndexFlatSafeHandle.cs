using System;

namespace FaissMask.Internal
{
    internal class IndexFlatSafeHandle : IndexSafeHandle
    {
        public static IndexFlatSafeHandle New()
        {
            return New(d: 0, metric: MetricType.MetricL2);
        }
        public static IndexFlatSafeHandle New(long d)
        {
            return New(d, metric: MetricType.MetricL2);
        }
        public static IndexFlatSafeHandle New(long d, MetricType metric)
        {
            var index = new IndexFlatSafeHandle();
            NativeMethods.faiss_IndexFlat_new_with(ref index, d, metric);
            return index;
        }
        
        public IndexFlatSafeHandle() {}
        public IndexFlatSafeHandle(IntPtr pointer) : base(pointer) {}
        
        public override void Free()
        {
            NativeMethods.faiss_IndexFlat_free(handle);
            IsFree = true;
        }

        protected override bool ReleaseHandle()
        {
            if (!IsFree)
                Free();
            return true;
        }
    }
}