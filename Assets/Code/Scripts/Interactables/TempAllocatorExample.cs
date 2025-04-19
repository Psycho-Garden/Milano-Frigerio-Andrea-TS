using UnityEngine;
using Unity.Collections;

public class TempAllocatorExample : MonoBehaviour
{
    void Start()
    {
        UseTempArray();
    }

    void UseTempArray()
    {
        // Allocazione temporanea
        NativeArray<int> tempArray = new NativeArray<int>(10, Allocator.Temp);

        try
        {
            // Utilizzo dell'array
            for (int i = 0; i < tempArray.Length; i++)
            {
                tempArray[i] = i * 2;
                Debug.Log($"tempArray[{i}] = {tempArray[i]}");
            }
        }
        finally
        {
            // Importante: rilasciare la memoria anche in caso di eccezioni
            if (tempArray.IsCreated)
                tempArray.Dispose();
        }
    }
}
