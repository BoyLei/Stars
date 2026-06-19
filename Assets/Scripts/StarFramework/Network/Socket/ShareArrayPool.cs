using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ShareArrayPool
{
    public static T[] Rent<T>(int size)
    {
        return System.Buffers.ArrayPool<T>.Shared.Rent(size);
    }

    public static void Return<T>(T[] shareArray)
    {
        System.Buffers.ArrayPool<T>.Shared.Return(shareArray);
    }
}
