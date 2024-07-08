using System;
using UnityEngine;

namespace ZSToolkit.ZSTUtility
{
    [AttributeUsage(AttributeTargets.Field)]
    public class WithComponentAttribute : PropertyAttribute
    {
        public System.Type RequiredComponentType;

        public WithComponentAttribute(System.Type requiredComponentType)
        {
            RequiredComponentType = requiredComponentType;
        }
    }
}