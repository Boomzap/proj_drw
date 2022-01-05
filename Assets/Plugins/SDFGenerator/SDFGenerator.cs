using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

// direct copy from my sleipner3 generator (sleipner3/engine/font/signed_distance.cpp)

namespace Boomzap
{
    public static class SDFGenerator
    {
        static readonly double sqrt2 = Math.Sqrt(2);

        static public Dictionary<int, Texture2D> TextureMap = new Dictionary<int, Texture2D>();

        public static void ClearTextureCache()
        {
            foreach (var pair in TextureMap)
            {
                UnityEngine.Object.DestroyImmediate(pair.Value);
            }
            TextureMap.Clear();
        }

        public static Texture2D CreateReadableScaledTexture(Texture2D sourceTexture, float scaleFactor)
        {
            Texture2D copyTexture = new Texture2D(sourceTexture.width, sourceTexture.height,
                sourceTexture.format, false);

            Graphics.CopyTexture(sourceTexture, 0, 0, copyTexture, 0, 0);

            if (scaleFactor != 1f)
            {
                Texture2D scaledTexture = new Texture2D((int)(sourceTexture.width * scaleFactor), (int)(sourceTexture.height * scaleFactor), sourceTexture.format, false);
                Color32[] colors = new Color32[scaledTexture.width * scaledTexture.height];

                for (int y = 0; y < scaledTexture.height; y++)
                {
                    for (int x = 0; x < scaledTexture.width; x++)
                    {
                        Color32 px = copyTexture.GetPixelBilinear((float)x / (float)scaledTexture.width, (float)y / (float)scaledTexture.height);
                        colors[x + scaledTexture.width * y] = px;
                    }
                }

                scaledTexture.SetPixels32(colors);
                scaledTexture.Apply();

                return scaledTexture;
            }

            return copyTexture;           
        }

        public static Texture2D Generate(Texture2D sourceTexture, Rect textureRect, Vector2Int padding, float minAlpha = (15f/255f))
        {
            if (!sourceTexture.isReadable)
            {
//                 Texture2D copyTexture = new Texture2D(sourceTexture.width, sourceTexture.height,
//                     sourceTexture.format, false);
// 
//                 Graphics.CopyTexture(sourceTexture, 0, 0, copyTexture, 0, 0);
// 
//                 if (scaleFactor != 1f)
//                 {
//                     Texture2D scaledTexture = new Texture2D((int)(sourceTexture.width * scaleFactor), (int)(sourceTexture.height * scaleFactor), sourceTexture.format, false);
// 
//                 }
// 
//                 sourceTexture = copyTexture;
                Debug.LogError("Texture is not readable");
            }

            if (padding.x % 2 == 1)
            {
                //Debug.LogWarning("Expanding pad width by 1 pixel (use even numbers)");
                padding.x++;
            }
            if (padding.y % 2 == 1)
            {
                //Debug.LogWarning("Expanding pad height by 1 pixel (use even numbers)");
                padding.y++;
            }

            double min = 0;
            double max = 255;

            int texHeight = (int)textureRect.height;
            int texWidth = (int)textureRect.width;
            Color32[] pixels = sourceTexture.GetPixels32(0);
            double[] norm = new double[(texWidth + padding.x) * (texHeight + padding.y)];
            double invMax = 1 / max;

            int pixelIdx = 0;
            for (int y = padding.y / 2; y < (texHeight + padding.y / 2); y++)
            {
                int lineStartIdx = y * (texWidth + padding.x);
                pixelIdx = sourceTexture.width * ((y - padding.y / 2) + (int)textureRect.yMin) + (int)textureRect.xMin;

                for (int x = padding.x / 2; x < (texWidth + padding.x / 2); x++, pixelIdx++)
                {
                    norm[lineStartIdx + x] = ((double)pixels[pixelIdx].a - min) * invMax;
                    if (norm[lineStartIdx + x] < minAlpha)
                        norm[lineStartIdx + x] = 0;
                }
            }

            GenerateDistanceMap(ref norm, texWidth + padding.x, texHeight + padding.y);

            Texture2D sdfTexture = new Texture2D(texWidth + padding.x, texHeight + padding.y, TextureFormat.Alpha8, false);
            
            byte[] output = new byte[(texWidth + padding.x) * (texHeight + padding.y) * 4];

            for (int i = 0; i < norm.Length; i++)
            {
                output[i] = (byte)(255 * (1 - norm[i]));
            }

            sdfTexture.SetPixelData(output, 0);
            sdfTexture.Apply();

            return sdfTexture;
        }

        static void GenerateDistanceMap(ref double[] data, int texWidth, int texHeight)
        {
            int[] xdist = new int[texWidth * texHeight];
            int[] ydist = new int[texWidth * texHeight];
            double[] gx = new double[texWidth * texHeight];
            double[] gy = new double[texWidth * texHeight];
            double[] inside = new double[texWidth * texHeight];
            double[] outside = new double[texWidth * texHeight];

            double min = double.MaxValue;
            
            CalcGradient(data, texWidth, texHeight, ref gx, ref gy);
            Edt(data, texWidth, texHeight, ref gx, ref gy, ref xdist, ref ydist, ref inside);

            for (int i = 0; i < gx.Length; i++)
            {
                gx[i] = 0;
                gy[i] = 0;
                data[i] = 1 - data[i];
            }

            CalcGradient(data, texWidth, texHeight, ref gx, ref gy);
            Edt(data, texWidth, texHeight, ref gx, ref gy, ref xdist, ref ydist, ref outside);

            for (int i = 0; i < gx.Length; i++)
            {
                outside[i] -= inside[i];
                if (outside[i] < min)
                    min = outside[i];
            }

            min = Math.Abs(min);
            if (min != 100000)
                min = 64;

            for (int i = 0; i < gx.Length; i++)
            {
                double v = outside[i];
                if (v < -min) outside[i] = -min;
                else if (v > min) outside[i] = min;
                data[i] = (outside[i] + min) / (min * 2);
            }
        }

        static void CalcGradient(double[] data, int width, int height, ref double[] gx, ref double[] gy)
        {
            for (int y = 1; y < (height - 1); y++)
            {
                for (int x = 1; x < (width - 1); x++)
                {
                    int i = y * width + x;

                    gx[i] = -data[i - width - 1] - sqrt2 * data[i - 1] - data[i + width - 1]
                            +data[i - width + 1] + sqrt2 * data[i + 1] + data[i + width + 1];

                    gy[i] = -data[i - width - 1] - sqrt2 * data[i - width] - data[i - width + 1]
                            +data[i + width - 1] + sqrt2 * data[i + width] + data[i + width + 1];

                    double len = gx[i] * gx[i] + gy[i] * gy[i];

                    if (len > 0)
                    {
                        len = Math.Sqrt(len);
                        gx[i] /= len;
                        gy[i] /= len;
                    }
                }
            }
        }

        static void Edt(double[] data, int width, int height, ref double[] gx, ref double[] gy, ref int[] xdist, ref int[] ydist, ref double[] output)
        {
            int off_u, off_ur, off_r, off_rd, off_d, off_dl, off_l, off_lu;
	        double Eps = 1e-3;
	        int cxdist, cydist;
	        int newxdist, newydist;
	        double newdist;
	        double olddist;


	        // avoid a little bit of ugliness later
	        off_u = -(int)width;
	        off_ur = -(int)width + 1;
	        off_r = 1;
	        off_rd = (int)width + 1;
	        off_d = (int)width;
	        off_dl = (int)width - 1;
	        off_l = -1;
	        off_lu = -(int)width - 1;

	        // initialization
	        for (int i = 0; i < width * height; i++)
	        {
		        xdist[i] = ydist[i] = 0;
		        if (data[i] <= 0.0)
			        output[i] = 1000000.0;
		        else if (data[i] < 1.0)
			        output[i] = ApproximateEdgeDistance(gx[i], gy[i], data[i]);
		        else
			        output[i] = 0.0;
	        }

	        bool Changed;

	        // now the part that kills me
	        do
	        {
		        Changed = false;

		        for (int y = 1; y < (int)height; y++)
		        {
			        int i = y * width;		// start from beginning of row

			        olddist = output[i];

			        // left-most pixel distances
			        if (olddist > 0)
			        {

				        cxdist = xdist[i + off_u];
				        cydist = ydist[i + off_u];
				        newxdist = cxdist;
				        newydist = cydist + 1;
				        newdist = Distance(ref data, ref gx, ref gy, width, i + off_u, cxdist, cydist, newxdist, newydist);
				        if (newdist < (olddist - Eps))
				        {
					        xdist[i] = newxdist;
					        ydist[i] = newydist;
					        output[i] = newdist;
					        olddist = newdist;
					        Changed = true;
				        }

				        cxdist = xdist[i + off_ur];
				        cydist = ydist[i + off_ur];
				        newxdist = cxdist - 1;
				        newydist = cydist + 1;
				        newdist = Distance(ref data, ref gx, ref gy, width, i + off_ur, cxdist, cydist, newxdist, newydist);
				        if (newdist < (olddist - Eps))
				        {
					        xdist[i] = newxdist;
					        ydist[i] = newydist;
					        output[i] = newdist;
					        olddist = newdist;
					        Changed = true;
				        }
			        }

			        i++;

			        // middle pixels idstances
			        for (int x = 1; x < (int)(width - 1); i++, x++)
			        {
				        olddist = output[i];
				        if (olddist <= 0) continue;

				        // left
				        cxdist = xdist[i + off_l];
				        cydist = ydist[i + off_l];
				        newxdist = cxdist + 1;
				        newydist = cydist;
				        newdist = Distance(ref data, ref gx, ref gy, width, i + off_l, cxdist, cydist, newxdist, newydist);
				        if (newdist < (olddist - Eps))
				        {
					        xdist[i] = newxdist;
					        ydist[i] = newydist;
					        output[i] = newdist;
					        olddist = newdist;
					        Changed = true;
				        }

				        // upper left
				        cxdist = xdist[i + off_lu];
				        cydist = ydist[i + off_lu];
				        newxdist = cxdist + 1;
				        newydist = cydist + 1;
				        newdist = Distance(ref data, ref gx, ref gy, width, i + off_lu, cxdist, cydist, newxdist, newydist);
				        if (newdist < (olddist - Eps))
				        {
					        xdist[i] = newxdist;
					        ydist[i] = newydist;
					        output[i] = newdist;
					        olddist = newdist;
					        Changed = true;
				        }

				        // upper
				        cxdist = xdist[i + off_u];
				        cydist = ydist[i + off_u];
				        newxdist = cxdist;
				        newydist = cydist + 1;
				        newdist = Distance(ref data, ref gx, ref gy, width, i + off_u, cxdist, cydist, newxdist, newydist);
				        if (newdist < (olddist - Eps))
				        {
					        xdist[i] = newxdist;
					        ydist[i] = newydist;
					        output[i] = newdist;
					        olddist = newdist;
					        Changed = true;
				        }

				        // upper right
				        cxdist = xdist[i + off_ur];
				        cydist = ydist[i + off_ur];
				        newxdist = cxdist - 1;
				        newydist = cydist + 1;
				        newdist = Distance(ref data, ref gx, ref gy, width, i + off_ur, cxdist, cydist, newxdist, newydist);
				        if (newdist < (olddist - Eps))
				        {
					        xdist[i] = newxdist;
					        ydist[i] = newydist;
					        output[i] = newdist;
					        olddist = newdist;
					        Changed = true;
				        }
			        }

			        // rightmost pixel distances
			        olddist = output[i];

			        if (olddist > 0)
			        {
				        // left
				        cxdist = xdist[i + off_l];
				        cydist = ydist[i + off_l];
				        newxdist = cxdist + 1;
				        newydist = cydist;
				        newdist = Distance(ref data, ref gx, ref gy, width, i + off_l, cxdist, cydist, newxdist, newydist);
				        if (newdist < (olddist - Eps))
				        {
					        xdist[i] = newxdist;
					        ydist[i] = newydist;
					        output[i] = newdist;
					        olddist = newdist;
					        Changed = true;
				        }

				        // upper left
				        cxdist = xdist[i + off_lu];
				        cydist = ydist[i + off_lu];
				        newxdist = cxdist + 1;
				        newydist = cydist + 1;
				        newdist = Distance(ref data, ref gx, ref gy, width, i + off_lu, cxdist, cydist, newxdist, newydist);
				        if (newdist < (olddist - Eps))
				        {
					        xdist[i] = newxdist;
					        ydist[i] = newydist;
					        output[i] = newdist;
					        olddist = newdist;
					        Changed = true;
				        }

				        // upper 
				        cxdist = xdist[i + off_u];
				        cydist = ydist[i + off_u];
				        newxdist = cxdist;
				        newydist = cydist + 1;
				        newdist = Distance(ref data, ref gx, ref gy, width, i + off_u, cxdist, cydist, newxdist, newydist);
				        if (newdist < (olddist - Eps))
				        {
					        xdist[i] = newxdist;
					        ydist[i] = newydist;
					        output[i] = newdist;
					        olddist = newdist;
					        Changed = true;
				        }
			        }

			        i = y * width + width - 2;

			        for (int x = width - 2; x >= 0; x--, i--)
			        {
				        olddist = output[i];
				        if (olddist <= 0) continue;

				        // right
				        cxdist = xdist[i + off_r];
				        cydist = ydist[i + off_r];
				        newxdist = cxdist - 1;
				        newydist = cydist;
				        newdist = Distance(ref data, ref gx, ref gy, width, i + off_r, cxdist, cydist, newxdist, newydist);
				        if (newdist < (olddist - Eps))
				        {
					        xdist[i] = newxdist;
					        ydist[i] = newydist;
					        output[i] = newdist;
					        olddist = newdist;
					        Changed = true;
				        }
			        }
		        }

		        for (int y = height - 2; y >= 0; y--)
		        {
			        int i = y * width + width - 1;

			        olddist = output[i];
			        if (olddist > 0)
			        {
				        // down
				        cxdist = xdist[i + off_d];
				        cydist = ydist[i + off_d];
				        newxdist = cxdist;
				        newydist = cydist - 1;
				        newdist = Distance(ref data, ref gx, ref gy, width, i + off_d, cxdist, cydist, newxdist, newydist);
				        if (newdist < (olddist - Eps))
				        {
					        xdist[i] = newxdist;
					        ydist[i] = newydist;
					        output[i] = newdist;
					        olddist = newdist;
					        Changed = true;
				        }

				        // lower left
				        cxdist = xdist[i + off_dl];
				        cydist = ydist[i + off_dl];
				        newxdist = cxdist + 1;
				        newydist = cydist - 1;
				        newdist = Distance(ref data, ref gx, ref gy, width, i + off_dl, cxdist, cydist, newxdist, newydist);
				        if (newdist < (olddist - Eps))
				        {
					        xdist[i] = newxdist;
					        ydist[i] = newydist;
					        output[i] = newdist;
					        olddist = newdist;
					        Changed = true;
				        }
			        }

			        i--;

			        for (int x = width - 2; x > 0; x--, i--)
			        {
				        olddist = output[i];

				        if (olddist <= 0) continue;

				        // right
				        cxdist = xdist[i + off_r];
				        cydist = ydist[i + off_r];
				        newxdist = cxdist - 1;
				        newydist = cydist;
				        newdist = Distance(ref data, ref gx, ref gy, width, i + off_r, cxdist, cydist, newxdist, newydist);
				        if (newdist < (olddist - Eps))
				        {
					        xdist[i] = newxdist;
					        ydist[i] = newydist;
					        output[i] = newdist;
					        olddist = newdist;
					        Changed = true;
				        }

				        // lower right
				        cxdist = xdist[i + off_rd];
				        cydist = ydist[i + off_rd];
				        newxdist = cxdist - 1;
				        newydist = cydist - 1;
				        newdist = Distance(ref data, ref gx, ref gy, width, i + off_rd, cxdist, cydist, newxdist, newydist);
				        if (newdist < (olddist - Eps))
				        {
					        xdist[i] = newxdist;
					        ydist[i] = newydist;
					        output[i] = newdist;
					        olddist = newdist;
					        Changed = true;
				        }

				        // lower
				        cxdist = xdist[i + off_d];
				        cydist = ydist[i + off_d];
				        newxdist = cxdist;
				        newydist = cydist - 1;
				        newdist = Distance(ref data, ref gx, ref gy, width, i + off_d, cxdist, cydist, newxdist, newydist);
				        if (newdist < (olddist - Eps))
				        {
					        xdist[i] = newxdist;
					        ydist[i] = newydist;
					        output[i] = newdist;
					        olddist = newdist;
					        Changed = true;
				        }

				        // lower left
				        cxdist = xdist[i + off_dl];
				        cydist = ydist[i + off_dl];
				        newxdist = cxdist + 1;
				        newydist = cydist - 1;
				        newdist = Distance(ref data, ref gx, ref gy, width, i + off_dl, cxdist, cydist, newxdist, newydist);
				        if (newdist < (olddist - Eps))
				        {
					        xdist[i] = newxdist;
					        ydist[i] = newydist;
					        output[i] = newdist;
					        olddist = newdist;
					        Changed = true;
				        }
			        }

			        olddist = output[i];
			        if (olddist > 0)
			        {
				        // right
				        cxdist = xdist[i + off_r];
				        cydist = ydist[i + off_r];
				        newxdist = cxdist - 1;
				        newydist = cydist;
				        newdist = Distance(ref data, ref gx, ref gy, width, i + off_r, cxdist, cydist, newxdist, newydist);
				        if (newdist < (olddist - Eps))
				        {
					        xdist[i] = newxdist;
					        ydist[i] = newydist;
					        output[i] = newdist;
					        olddist = newdist;
					        Changed = true;
				        }

				        // lower right
				        cxdist = xdist[i + off_rd];
				        cydist = ydist[i + off_rd];
				        newxdist = cxdist - 1;
				        newydist = cydist - 1;
				        newdist = Distance(ref data, ref gx, ref gy, width, i + off_rd, cxdist, cydist, newxdist, newydist);
				        if (newdist < (olddist - Eps))
				        {
					        xdist[i] = newxdist;
					        ydist[i] = newydist;
					        output[i] = newdist;
					        olddist = newdist;
					        Changed = true;
				        }

				        // lower
				        cxdist = xdist[i + off_d];
				        cydist = ydist[i + off_d];
				        newxdist = cxdist;
				        newydist = cydist - 1;
				        newdist = Distance(ref data, ref gx, ref gy, width, i + off_d, cxdist, cydist, newxdist, newydist);
				        if (newdist < (olddist - Eps))
				        {
					        xdist[i] = newxdist;
					        ydist[i] = newydist;
					        output[i] = newdist;
					        olddist = newdist;
					        Changed = true;
				        }
			        }

			        i = y * width + 1;
			        for (int x = 1; x < (int)width; x++, i++)
			        {
				        olddist = output[i];
				        if (olddist <= 0) continue;

				        // left
				        cxdist = xdist[i + off_l];
				        cydist = ydist[i + off_l];
				        newxdist = cxdist + 1;
				        newydist = cydist;
				        newdist = Distance(ref data, ref gx, ref gy, width, i + off_l, cxdist, cydist, newxdist, newydist);
				        if (newdist < (olddist - Eps))
				        {
					        xdist[i] = newxdist;
					        ydist[i] = newydist;
					        output[i] = newdist;
					        olddist = newdist;
					        Changed = true;
				        }
			        }
		        }
	        } while (Changed);            
        }

        static double ApproximateEdgeDistance(double gx, double gy, double alpha)
        {
            if (gx == 0 || gy == 0) return (0.5 - alpha);

            double len = Math.Sqrt(gx * gx + gy * gy);
            if (len > 0)
            {
                gx /= len;
                gy /= len;
            }

            gx = Math.Abs(gx);
            gy = Math.Abs(gy);

            if (gx < gy)
            {
                double tt = gx;
                gx = gy;
                gy = tt;
            }

            double t = 0.5 * gy / gx;
            if (alpha < t)
                return (0.5 * (gx + gy) - Math.Sqrt(2 * gx * gy * alpha));
            if (alpha < (1 - t))
                return (0.5 - alpha) * gx;

            return -0.5 * (gx + gy) + Math.Sqrt(2 * gx * gy * (1 - alpha));
        }

        static double Distance(ref double[] data, ref double[] gxi, ref double[] gyi, int w, int c, int xc, int yc, int xi, int yi)
        {
            int closest = c - xc - yc * w;
            double a = data[closest];
            double gx = gxi[closest];
            double gy = gyi[closest];

            if (a < 0) a = 0;
            if (a > 1) a = 1;
            if (a == 0)
                return 100000;

            double dx = xi;
            double dy = yi;
            double di = Math.Sqrt(dx * dx + dy * dy);

            if (di == 0)
                return di + ApproximateEdgeDistance(gx, gy, a);
            return di + ApproximateEdgeDistance(dx, dy, a);
        }
    }
}
