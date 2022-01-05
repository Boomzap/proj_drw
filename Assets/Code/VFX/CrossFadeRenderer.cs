using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using Sirenix.OdinInspector;

namespace Boomzap
{
    // restricted to FullRect sprites
    public class CrossFadeRenderer : MonoBehaviour
    {
        [SerializeField] MeshRenderer meshRenderer;
        [SerializeField] MeshFilter meshFilter;
        [SerializeField] Sprite sourceSprite;
        [SerializeField] Sprite targetSprite;
        [SerializeField] Material crossFadeMaterial;
        MaterialPropertyBlock materialPropertyBlock;
        float crossFade = 0.5f;
        [ShowInInspector]
        public float CrossFade
        {
            get { return crossFade; }
            set { crossFade = value; SetCrossFade(value); }
        }

        private void Awake()
        {
            materialPropertyBlock = new MaterialPropertyBlock();

            GenerateMesh();
        }

        [Button]
        void SetCrossFade(float a)
        {
            materialPropertyBlock.SetFloat("_CrossFade", a);
            meshRenderer.SetPropertyBlock(materialPropertyBlock);
        }

        [Button]
        void GenerateMesh()
        {
            if (materialPropertyBlock == null)
                materialPropertyBlock = new MaterialPropertyBlock();

            Mesh mesh = new Mesh();

            var srcVerts = sourceSprite.vertices;
            var dstVerts = targetSprite.vertices;

            float xmax = Mathf.Max(srcVerts.Max(x => x.x), dstVerts.Max(x => x.x));
            float xmin = Mathf.Min(srcVerts.Min(x => x.x), dstVerts.Min(x => x.x));
            float ymin = Mathf.Min(srcVerts.Min(x => x.y), dstVerts.Min(x => x.y));
            float ymax = Mathf.Max(srcVerts.Max(x => x.y), dstVerts.Max(x => x.y));

            List<Vector3> verts = new List<Vector3>();
            verts.Add(new Vector2(xmin, ymax));
            verts.Add(new Vector2(xmax, ymax));
            verts.Add(new Vector2(xmin, ymin));
            verts.Add(new Vector2(xmax, ymin));

            List<Vector2> dstUVsInSrc = new List<Vector2>();
            List<Vector2> srcUVsInDst = new List<Vector2>();

            // shader uniforms, only need to be set once
            Vector4 dstBounds = new Vector4();
            Vector4 srcBounds = new Vector4();

            // calc normalized uvs 
            dstBounds.x = targetSprite.textureRect.min.x / targetSprite.texture.width;
            dstBounds.y = targetSprite.textureRect.min.y / targetSprite.texture.height;
            dstBounds.z = targetSprite.textureRect.max.x / targetSprite.texture.width;
            dstBounds.w = targetSprite.textureRect.max.y / targetSprite.texture.height;

            srcBounds.x = sourceSprite.textureRect.min.x / sourceSprite.texture.width;
            srcBounds.y = sourceSprite.textureRect.min.y / sourceSprite.texture.height;
            srcBounds.z = sourceSprite.textureRect.max.x / sourceSprite.texture.width;
            srcBounds.w = sourceSprite.textureRect.max.y / sourceSprite.texture.height;

            // xlate mapping for points in mesh to uv of src/dst
            for (int i = 0; i < srcVerts.Length; i++)
            {
                var v = verts[i];
                Vector2 dstLocal = v - targetSprite.bounds.min;
                Vector2 uvLocal = dstLocal / targetSprite.bounds.size;
                uvLocal = (Vector2)targetSprite.textureRect.min + (Vector2)targetSprite.textureRect.size * uvLocal;
                uvLocal *= targetSprite.texture.texelSize;

                dstUVsInSrc.Add(uvLocal);
            }

            for (int i = 0; i < dstVerts.Length; i++)
            {
                var v = verts[i];
                Vector2 srcLocal = v - sourceSprite.bounds.min;
                Vector2 uvLocal = srcLocal / sourceSprite.bounds.size;
                uvLocal = (Vector2)sourceSprite.textureRect.min + (Vector2)sourceSprite.textureRect.size * uvLocal;
                uvLocal *= sourceSprite.texture.texelSize;

                srcUVsInDst.Add(uvLocal);
            }

            mesh.SetVertices(verts);
            mesh.SetUVs(0, srcUVsInDst);
            mesh.SetUVs(1, dstUVsInSrc);
            mesh.SetTriangles(new int[] { 0, 1, 2, 2, 1, 3 }, 0);

            meshFilter.sharedMesh = mesh;

            meshRenderer.material = new Material(crossFadeMaterial);

            materialPropertyBlock.SetTexture("_MainTex", sourceSprite.texture);
            materialPropertyBlock.SetTexture("_SecondaryTex", targetSprite.texture);
            materialPropertyBlock.SetVector("_DstBounds", dstBounds);
            materialPropertyBlock.SetVector("_SrcBounds", srcBounds);
            materialPropertyBlock.SetFloat("_CrossFade", 0f);
            meshRenderer.SetPropertyBlock(materialPropertyBlock);
        }
    }
}