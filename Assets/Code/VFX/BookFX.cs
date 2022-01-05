using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine.Events;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ho
{
    public class BookFX : MonoBehaviour
    {
        [SerializeField]
        RectTransform   bookRect;

        [SerializeField]
        MeshRenderer    meshRendererPageTo;
        [SerializeField]
        MeshRenderer    meshRendererPageFrom;

        [SerializeField]
        MeshFilter      meshFilterPageTo;
        [SerializeField]
        MeshFilter      meshFilterPageFrom;

        [SerializeField]
        Material        renderMat;

        Mesh            pageToMesh;
        Mesh            pageFromMesh;

        RenderTexture   fromTexture;
        RenderTexture   toTexture;

        MaterialPropertyBlock materialPropertyBlockFrom;
        MaterialPropertyBlock materialPropertyBlockTo;

        [SerializeField]
        GameObject      fromCapture;
        [SerializeField]
        GameObject      toCapture;

        [SerializeField]
        Camera          captureCamera;

        [SerializeField]
        float           flipTime = 0.5f;

        private void OnEnable()
        {
            meshRendererPageTo.sharedMaterial = new Material(renderMat);
            meshRendererPageFrom.sharedMaterial = new Material(renderMat);


            meshRendererPageFrom.sortingOrder = 100;
            meshRendererPageTo.sortingOrder = 99;


            materialPropertyBlockFrom = new MaterialPropertyBlock();
            materialPropertyBlockTo = new MaterialPropertyBlock();

            fromTexture = RenderTexture.GetTemporary((int)bookRect.sizeDelta.x, (int)bookRect.sizeDelta.y);
            toTexture = RenderTexture.GetTemporary((int)bookRect.sizeDelta.x, (int)bookRect.sizeDelta.y);
        }

        private void OnDisable()
        {
            RenderTexture.ReleaseTemporary(fromTexture);
            RenderTexture.ReleaseTemporary(toTexture);
        }

        IEnumerator FlipCor(bool RTL, UnityAction onDone)
        {
            meshRendererPageFrom.gameObject.SetActive(true);
            meshRendererPageTo.gameObject.SetActive(true);

            materialPropertyBlockTo.SetTexture("_MainTex", !RTL ? fromTexture : toTexture);
            materialPropertyBlockFrom.SetTexture("_MainTex", !RTL ? toTexture : fromTexture);

            meshRendererPageTo.SetPropertyBlock(materialPropertyBlockTo);
            meshRendererPageFrom.SetPropertyBlock(materialPropertyBlockFrom);

            float t = 0f;

            while (t < flipTime)
            {
                float a = t / flipTime;

                UpdateFlipMesh(bookRect.sizeDelta, meshRendererPageTo.transform.position, a, RTL);
                t += Time.deltaTime;

                yield return null;
            }

            meshRendererPageFrom.gameObject.SetActive(false);
            meshRendererPageTo.gameObject.SetActive(false);

            onDone?.Invoke();
        }

        public void Hide()
        {
            enabled = false;
        }

        public void Show()
        {
            enabled = true;
        }

        // sleipner3/engine/math/intersection.cpp
        bool LineLine2D(Vector2 P1, Vector2 P2, Vector2 P3, Vector2 P4, out Vector2 Intersect)
        {
            //                          P3
            //               line a      |
            //      P1 ------------------x----------P2
            //                           |
            //                           | line b
            //                           |
            //                           |
            //                           P4
            //
            //	Pa = P1 + u1 * (P2-P1)
            //	Pb = P3 + u2 * (P4-P3)

            Intersect = new Vector2();

            float d = ((P4.y - P3.y) * (P2.x - P1.x)) - ((P4.x - P3.x) * (P2.y - P1.y));
            if (Mathf.Approximately(d, 0f))
                return false; // lines are parallel

            float inv_d = 1f / d;
            float ux1 = ((P4.x - P3.x) * (P1.y - P3.y)) - ((P4.y - P3.y) * (P1.x - P3.x));
            float ux2 = ((P2.x - P1.x) * (P1.y - P3.y)) - ((P2.y - P1.y) * (P1.x - P3.x));

            float u1 = ux1 * inv_d;
            float u2 = ux2 * inv_d;

            if ((u1 < 0.0f) || (u1 > 1.0f)) return false; // does not intersect on line P1-P2
            if ((u2 < 0.0f) || (u2 > 1.0f)) return false; // does not intersect on line P3-P4

            Intersect.x = P1.x + (u1 * (P2.x - P1.x));
            Intersect.y = P1.y + (u1 * (P2.y - P1.y));

            return true;
        }

        void UpdateFlipMesh(Vector2 size, Vector3 pos, float alpha, bool RTL)
        {
            pageToMesh = new Mesh();
            pageFromMesh = new Mesh();

            Vector2 rsize = size;

            if (RTL)
            {
                alpha = 1f - alpha;
            }

            List<Vector2> FlipVerts = new List<Vector2>();
            List<Vector2> FlipUV = new List<Vector2>();
            List<Vector2> FGVerts = new List<Vector2>();

            Vector2 TL = new Vector2(0f, 0f);
            Vector2 TR = new Vector2(1f, 0f);
            Vector2 BL = new Vector2(0f, 1f);
            Vector2 BR = new Vector2(1f, 1f);

            Vector2 TC = new Vector2(0.5f, 0f);
            Vector2 BC = new Vector2(0.5f, 1f);

            size = Vector2.one;
            float halfSize = size.x * 0.5f;

            Vector2 P1 = TC;
            Vector2 P2 = TR;
            Vector2 P3 = BR;
            Vector2 P4 = BC;
            Vector2 Current = TR;

            if (RTL)
            {
                P1 = TC;
                P2 = TL;
                P3 = BL;
                P4 = BC;
                Current = TL;
            }


            Vector2 P = new Vector2(TL.x + size.x * alpha, TL.y + Mathf.Sin(alpha * Mathf.PI) * 0.1f);

            //P.x = Mathf.Clamp(P.x, TL.x, TR.x);
            //P.y = Mathf.Clamp(P.y, TL.y, BL.y);

            //if ((P - TC).magnitude > halfSize)
            //{
            //    Vector2 toCenter = P - TC;
            //    toCenter.Normalize();
            //    P = TC + toCenter * halfSize;
            //}

            Vector2 Dir = Current - P;
            Vector2 Right = new Vector2(Dir.y, -Dir.x);
            Vector2 MidPoint = P + Dir * 0.5f;

            Vector2 Inter;
            FlipUV.Add(Vector2.zero);
            FlipVerts.Add(P);
            FGVerts.Add(P1);

            Right.Normalize();
            if (RTL)
                Right = -Right;

            if (LineLine2D(P1, P2, MidPoint, MidPoint + Right * 2f, out Inter))
            {
                FGVerts.Add(Inter);
                FlipVerts.Add(Inter);

                float x = (P2.x - Inter.x) / size.x;
                FlipUV.Add(new Vector2(x, 0f));


            } else
            {
                FlipVerts.Add(P1);
                float x = (P2.x - P1.x) / size.x;
                FlipUV.Add(new Vector2(x, 0f));
            }

            if (LineLine2D(P2, P3, MidPoint, MidPoint - Right * 2f, out Inter))
            {
                FGVerts.Add(Inter);
                FGVerts.Add(P3);
                FlipVerts.Add(Inter);

                float y = (P-Inter).magnitude / size.y;
                FlipUV.Add(new Vector2(0f, y));
            }

            if (LineLine2D(P4, P3, MidPoint, MidPoint - Right * 2f, out Inter))
            {
                FGVerts.Add(Inter);

                FlipVerts.Add(Inter);
                float x = (P2.x - Inter.x) / size.x;
                FlipUV.Add(new Vector2(x, 1f));
                Dir.Normalize();

                Vector2 Target = new Vector2();
                if (!RTL)
                    Target = Inter + (Inter - BR).magnitude * -Dir;
                else
                    Target = Inter + (Inter - BL).magnitude * -Dir;

                FlipVerts.Add(Target);
                FlipUV.Add(new Vector2(0f, 1f));
            }

            FGVerts.Add(P4);

            List<Vector3> MeshVertsFG = new List<Vector3>();
            List<int> MeshVertTris = new List<int>();
            List<Vector2> MeshVertUVs = new List<Vector2>();

            for (int i = 0; i < (FGVerts.Count-1); i++)
            {
                int[] TriIdx = { 0, i, i+1};

                //if (IsCCW(FGVerts[TriIdx[0]], FGVerts[TriIdx[1]], FGVerts[TriIdx[2]], rsize))
                if (!RTL)
                {
                    TriIdx = new int[] { i + 1, i, 0 };
                }

                if (FGVerts[TriIdx[0]] == FGVerts[TriIdx[1]] ||
                    FGVerts[TriIdx[0]] == FGVerts[TriIdx[2]] ||
                    FGVerts[TriIdx[2]] == FGVerts[TriIdx[1]])
                    continue;

                for (int j = 0; j < 3; j++)
                {
                    Vector2 T = FGVerts[TriIdx[j]];
                    Vector2 UV = new Vector2((T.x - TL.x) / size.x, (T.y - TL.y) / size.y);

                    MeshVertsFG.Add(T);
                    MeshVertUVs.Add(UV);
                }

                int offset = MeshVertsFG.Count - 3;
                MeshVertTris.AddRange(new int[] { offset + 0, offset + 1, offset + 2 });
            }

            List<Vector3> MeshVertsFlip = new List<Vector3>();
            List<int> MeshVertFlipTris = new List<int>();
            List<Vector2> MeshVertFlipUVs = new List<Vector2>();

            for (int i = 0; i < (FlipVerts.Count-1); i++)
            {
                int[] TriIdx = { 0, i + 1, i  };

                //if (IsCCW(FlipVerts[TriIdx[0]], FlipVerts[TriIdx[1]], FlipVerts[TriIdx[2]], rsize))
                //if (!RTL)
//                 {
//                     TriIdx = new int[] { i+1, i, 0 };
//                 }

                if (FlipVerts[TriIdx[0]] == FlipVerts[TriIdx[1]] ||
                    FlipVerts[TriIdx[0]] == FlipVerts[TriIdx[2]] ||
                    FlipVerts[TriIdx[2]] == FlipVerts[TriIdx[1]])
                continue;


                for (int j = 0; j < 3; j++)
                {
                    Vector2 T = FlipVerts[TriIdx[j]];

                    Vector2 UV = FlipUV[TriIdx[j]];

                    if (RTL)
                    {
                        UV.x += 1f;
                    }

                    MeshVertFlipUVs.Add(UV);
                    MeshVertsFlip.Add(T);
                }

                int offset = MeshVertsFlip.Count - 3;
                MeshVertFlipTris.AddRange(new int[] { offset + 0, offset + 1, offset + 2 });
            }

            for (int i = 0; i < MeshVertFlipUVs.Count; i++)
            {
                MeshVertFlipUVs[i] = new Vector2(MeshVertFlipUVs[i].x, 1f - MeshVertFlipUVs[i].y);
            }
            for (int i = 0; i < MeshVertUVs.Count; i++)
            {
                MeshVertUVs[i] = new Vector2(MeshVertUVs[i].x, 1f - MeshVertUVs[i].y);
            }

            for (int i = 0; i < MeshVertsFG.Count; i++)
            {
                MeshVertsFG[i] = new Vector2(MeshVertsFG[i].x, 1f - MeshVertsFG[i].y) * rsize - rsize * new Vector2(0.5f, 0.5f);
            }

            for (int i = 0; i < MeshVertsFlip.Count; i++)
            {
                MeshVertsFlip[i] = new Vector2(MeshVertsFlip[i].x, 1f - MeshVertsFlip[i].y) * rsize - rsize * new Vector2(0.5f, 0.5f);
            }

            pageToMesh.vertices = MeshVertsFG.ToArray();
            pageToMesh.uv = MeshVertUVs.ToArray();
            pageToMesh.triangles = MeshVertTris.ToArray();
            //             bookMesh.vertices = new Vector3[] { BL, BR, TL, TR };
            //             bookMesh.uv = new Vector2[] { Vector2.zero, new Vector2(1f, 0f), new Vector2(0f, 1f), Vector2.one };
            //             bookMesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
            meshFilterPageTo.mesh = pageToMesh;

            pageFromMesh.vertices = MeshVertsFlip.ToArray();
            pageFromMesh.uv = MeshVertFlipUVs.ToArray();
            pageFromMesh.triangles = MeshVertFlipTris.ToArray();

            meshFilterPageFrom.mesh = pageFromMesh;

            pageFromMesh.RecalculateNormals();
            pageToMesh.RecalculateNormals();
        }

        public void Capture()
        {
            bool wasFromActive = fromCapture.gameObject.activeSelf;
            bool wasToActive = toCapture.gameObject.activeSelf;

            meshRendererPageFrom.gameObject.SetActive(false);
            meshRendererPageTo.gameObject.SetActive(false);

            captureCamera.aspect = bookRect.sizeDelta.x / bookRect.sizeDelta.y;
            captureCamera.orthographicSize = bookRect.sizeDelta.y * 0.5f;

            fromCapture.gameObject.SetActive(true);
            toCapture.gameObject.SetActive(false);

            captureCamera.targetTexture = fromTexture;
            captureCamera.Render();

            fromCapture.gameObject.SetActive(false);
            toCapture.gameObject.SetActive(true);

            captureCamera.targetTexture = toTexture;
            captureCamera.Render();

            fromCapture.gameObject.SetActive(wasFromActive);
            toCapture.gameObject.SetActive(wasToActive);

            captureCamera.targetTexture = null;
        }

        public void Animate(bool RTL, UnityAction onDone)
        {
            StopAllCoroutines();
            Capture();
            StartCoroutine(FlipCor(RTL, onDone));
        }

    }
}
