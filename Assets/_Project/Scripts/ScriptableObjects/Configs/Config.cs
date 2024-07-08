using System;
using System.Linq;
using UnityEngine;

namespace Fifbox.ScriptableObjects.Configs
{
    public abstract class ConfigBase : ScriptableObject
    {
        public abstract Type ConfigType { get; }
    }

    public abstract class Config<T> : ConfigBase where T : Config<T>
    {
        public override Type ConfigType => typeof(T);

        public T Clone()
        {
            if (typeof(T) != GetType())
            {
                throw new("Clone failed, types mismatch");
            }

            return Instantiate(this) as T;
        }

        public void CopyFrom(T source)
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

            var fields = typeof(T).GetFields().ToList();
            foreach (var field in fields)
            {
                field.SetValue(this, field.GetValue(source));
            }
        }
    }
}