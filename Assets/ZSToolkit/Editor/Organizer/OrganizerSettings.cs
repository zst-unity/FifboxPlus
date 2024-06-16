using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZSToolkit.Editor
{
    public class OrganizerSettings : ScriptableObject
    {
        [HideInInspector] public string folderPath = "Assets";
        [HideInInspector] public bool autoScan = true;
        [HideInInspector] public MarkTypes markTypes = new();

        [Serializable]
        public class MarkTypes : IEnumerable
        {
            public int Count => names.Count;

            [SerializeField]
            private List<string> names = new()
            {
                "TODO",
                "FIXME"
            };

            [SerializeField]
            private List<Color> colors = new()
            {
                new Color32(255, 174, 49, 255),
                new Color32(240, 98, 146, 255)
            };

            public string[] Names => names.ToArray();
            public Color[] Colors => colors.ToArray();

            public Color this[string name]
            {
                get
                {
                    var nameIdx = names.FindIndex(name1 => name1 == name);
                    if (nameIdx == -1) throw new ArgumentException($"Mark type \"{name}\" was not defined");
                    return colors[nameIdx];
                }
                set
                {
                    var nameIdx = names.FindIndex(name1 => name1 == name);
                    if (nameIdx == -1) throw new ArgumentException($"Mark type \"{name}\" was not defined");
                    colors[nameIdx] = value;
                }
            }

            public void Set(Index idx, string name)
            {
                names[idx] = name;
            }

            public void Set(Index idx, Color color)
            {
                colors[idx] = color;
            }

            public string GetName(Index idx)
            {
                return names[idx];
            }

            public Color GetColor(Index idx)
            {
                return colors[idx];
            }

            public void Add(string name, Color color)
            {
                if (names.Contains(name)) throw new ArgumentException($"Mark type \"{name}\" already exists");
                names.Add(name);
                colors.Add(color);
            }

            public void Remove(string name)
            {
                var idx = names.FindIndex(name1 => name1 == name);
                if (idx == -1) throw new ArgumentException($"Mark type \"{name}\" was not defined");
                names.RemoveAt(idx);
                colors.RemoveAt(idx);
            }

            public MarkTypesEnumerator GetEnumerator()
            {
                return new MarkTypesEnumerator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public class MarkTypesEnumerator : IEnumerator
        {
            private readonly MarkTypes _markTypes;
            private int _idx = -1;

            public MarkTypesEnumerator(MarkTypes markTypes)
            {
                _markTypes = markTypes;
            }

            public bool MoveNext()
            {
                _idx++;
                return _idx < _markTypes.Count;
            }

            public void Reset()
            {
                _idx = -1;
            }

            object IEnumerator.Current
            {
                get => Current;
            }

            public EnumerationMarkType Current
            {
                get
                {
                    try
                    {
                        return new EnumerationMarkType(_markTypes, _idx);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }
        }

        public readonly struct EnumerationMarkType
        {
            private readonly MarkTypes _markTypes;
            private readonly int _idx;

            public string Name
            {
                get => _markTypes.GetName(_idx);
                set => _markTypes.Set(_idx, value);
            }

            public Color Color
            {
                get => _markTypes.GetColor(_idx);
                set => _markTypes.Set(_idx, value);
            }

            public EnumerationMarkType(MarkTypes markTypes, int idx)
            {
                _markTypes = markTypes;
                _idx = idx;
            }
        }
    }
}