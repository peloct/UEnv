using System.Collections.Generic;
using UnityEngine;

public class NumpyArr
{
    protected int[] shape;
    protected int[] dataOffsets;

    public IReadOnlyList<int> Shape => shape;
    
    public NumpyArr(int[] shape)
    {
        this.shape = shape;
        dataOffsets = new int[shape.Length];

        int offset = 1;
        
        for (int i = dataOffsets.Length - 1; i >= 0; --i)
        {
            dataOffsets[i] = offset;
            offset *= shape[i];
        }
    }
    
    public int Length(int i)
    {
        return shape[i];
    }

    public int Length(System.Index index)
    {
        return shape[index];
    }
}

public class NumpyArr<T> : NumpyArr
{
    private T[] data;

    public IReadOnlyList<T> Data => data;
    
    public NumpyArr(T[] data, params int[] shape) : base(shape)
    {
        this.data = data;
    }

    public T GetValue(params int[] index)
    {
        Debug.Assert(index.Length == shape.Length);
        
        int idx = 0;
        for (int i = 0; i < index.Length; ++i)
            idx += index[i] * dataOffsets[i];

        return data[idx];
    }
}
