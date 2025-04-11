using UnityEngine;

namespace AF.TS.Characters
{
    public interface IUnitInventory : IUnitCommon
    {
        void AddItem(GameObject item);
        void RemoveItem(GameObject item);
    }

}
