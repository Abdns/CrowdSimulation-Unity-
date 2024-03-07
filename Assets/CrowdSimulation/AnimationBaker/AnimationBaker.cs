using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AnimationBaker : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;

    void Start()
    {
        StartCoroutine(Bake());
    }

    private IEnumerator Bake()
    {
        Mesh mesh = new Mesh();
        int VertCount = Mathf.NextPowerOfTwo(_skinnedMeshRenderer.sharedMesh.vertexCount);

        foreach (AnimationClip clip in _animator.runtimeAnimatorController.animationClips)
        {
            int frameCount = Mathf.NextPowerOfTwo(Mathf.RoundToInt(clip.length * 60));
            Texture2D positionTexture = new Texture2D(VertCount, frameCount, TextureFormat.RGBAFloat, false);
            positionTexture.name = clip.name;

            for (int i = 0; i < frameCount; i++)
            {
                float fraction = (float)i / frameCount;
                _animator.SetFloat("Progress", fraction);
                yield return null;

                _skinnedMeshRenderer.BakeMesh(mesh);

                for (int j = 0; j < mesh.vertexCount; j++)
                {
                    Vector3 vert = mesh.vertices[j];

                    vert /= 40f;
                    vert += new Vector3(0.5f, 0.5f, 0.5f);

                    positionTexture.SetPixel(j, i, new Color(vert.x, vert.y, vert.z));
                }

                var positionsData = positionTexture.EncodeToPNG();
                File.WriteAllBytes(Path.Combine("Assets", positionTexture.name + "00" + ".png"), positionsData);

            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
