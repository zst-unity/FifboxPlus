using System;
using System.Linq;
using UnityEngine;

namespace Fifbox.API.ScriptableObjects.Configs
{
    public abstract class ConfigBase : ScriptableObject
    {
        public abstract Type ConfigType { get; }
    }

    public abstract class Config<T1> : ConfigBase where T1 : Config<T1>
    {
        public override Type ConfigType => typeof(T1);

        public static T2 Create<T2>() where T2 : Config<T2>
        {
            return CreateInstance<T2>();
        }

        public T1 Clone()
        {
            if (typeof(T1) != GetType())
            {
                throw new("Clone failed, types mismatch");
            }

            return Instantiate(this) as T1;
        }

        public void CopyFrom(T1 source)
        {
            if (!source)
            {
                Debug.LogError("Cant copy from null config");
                return;
            }

            if (!source.GetType().IsSubclassOf(GetType()))
            {
                Debug.LogError("Copy failed, types mismatch");
                return;
            }

            var fields = typeof(T1).GetFields().ToList();
            foreach (var field in fields)
            {
                field.SetValue(this, field.GetValue(source));
            }
        }
    }
}