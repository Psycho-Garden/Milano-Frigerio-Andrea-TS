using System;

namespace AF.TS.Weapons
{
    public class MultiTypeAttribute : Attribute
    {
        public Type[] AllowedTypes;

        public MultiTypeAttribute(params Type[] allowedTypes)
        {
            AllowedTypes = allowedTypes;
        }
    }

}
