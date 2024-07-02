using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fifbox.UI
{
    public class FramerateDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text _framerateText;
        private readonly Stack<float> _framerateRecord = new();
        private float _fps;

        private void Update()
        {
            var framerate = 1f / Time.deltaTime;

            if (_framerateRecord.Count < 10) _framerateRecord.Push(framerate);
            else
            {
                _fps = 0f;
                while (_framerateRecord.Count > 0)
                {
                    _fps = (_fps + _framerateRecord.Pop()) / 2f;
                }
            }

            _fps = Mathf.Round(_fps);
            _framerateText.text = $"<color=yellow>{_fps}</color> fps";
        }
    }
}