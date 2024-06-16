using TMPro;
using UnityEngine;

namespace ZSToolkit.ConviUI
{
    [AddComponentMenu("UI/TextMeshPro - Text Wobbler (UI)", 11)]
    public class TMP_TextWobbler : MonoBehaviour
    {
        [field: SerializeField] public TMP_Text TextMesh { get; private set; }
        [field: SerializeField] public float Speed { get; set; } = 1.5f;
        [field: SerializeField] public float Amplitude { get; set; } = 15.75f;

        private void OnValidate()
        {
            if (TryGetComponent(out TMP_Text textMesh)) TextMesh = textMesh;
        }

        private void Update()
        {
            ApplyEffect();
        }

        private void ApplyEffect()
        {
            TextMesh.ForceMeshUpdate();
            var textInfo = TextMesh.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];

                if (!charInfo.isVisible) { continue; }

                var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

                for (int v = 0; v < 4; v++)
                {
                    var orig = verts[charInfo.vertexIndex + v];
                    verts[charInfo.vertexIndex + v] = orig + Vector3.up * Mathf.Sin(Time.time * Speed + verts[charInfo.vertexIndex].x * 0.01f) * Amplitude;
                }
            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                var meshInfo = textInfo.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;

                TextMesh.UpdateGeometry(meshInfo.mesh, i);
            }
        }
    }
}