using UnityEngine;

namespace Fifbox.Attributes
{
    public class WithComponentAttribute : PropertyAttribute
    {
        public System.Type RequiredComponentType;

        public WithComponentAttribute(System.Type requiredComponentType)
        {
            RequiredComponentType = requiredComponentType;
        }
    }
}