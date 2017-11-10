using System;
using System.Runtime.InteropServices;

namespace Secp256k1Proxy.Helpers
{
    public class JaggedArrayMarshaler : ICustomMarshaler
    {
        public static ICustomMarshaler GetInstance(string cookie)
        {
            return new JaggedArrayMarshaler();
        }

        private GCHandle[] handles;
        private GCHandle buffer;
        private Array[] array;
        public void CleanUpManagedData(object managedObj)
        {
        }
        public void CleanUpNativeData(IntPtr pNativeData)
        {
            buffer.Free();
            foreach (var handle in handles)
            {
                handle.Free();
            }
        }
        public int GetNativeDataSize()
        {
            return 4;
        }
        public IntPtr MarshalManagedToNative(object managedObj)
        {
            array = (Array[])managedObj;
            handles = new GCHandle[array.Length];
            for (var i = 0; i < array.Length; i++)
            {
                handles[i] = GCHandle.Alloc(array[i], GCHandleType.Pinned);
            }
            var pointers = new IntPtr[handles.Length];
            for (var i = 0; i < handles.Length; i++)
            {
                pointers[i] = handles[i].AddrOfPinnedObject();
            }
            buffer = GCHandle.Alloc(pointers, GCHandleType.Pinned);
            return buffer.AddrOfPinnedObject();
        }
        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            return array;
        }
    }
}
