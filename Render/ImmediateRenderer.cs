using System;
using SkyDome.Entity;
using UnityEngine;
namespace SkyDome.Render
{
	public static class FastRenderer
	{
		public static void DrawBoxOutline(Rect rect, Color color, float thickness = 1f)
		{
			Vector2 vector = new Vector2(rect.x, rect.y);
			Vector2 vector2 = new Vector2(rect.x + rect.width, rect.y);
			Vector2 vector3 = new Vector2(rect.x, rect.y + rect.height);
			Vector2 vector4 = new Vector2(rect.x + rect.width, rect.y + rect.height);
			FastRenderer.DrawLine(vector, vector2, color, thickness);
			FastRenderer.DrawLine(vector3, vector4, color, thickness);
			FastRenderer.DrawLine(vector, vector3, color, thickness);
			FastRenderer.DrawLine(vector2, vector4, color, thickness);
		}

        public static void DrawSectorOutline(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, int segments, Color color, float thickness = 1f)
		{
			if (radius > 0f && segments >= 3)
			{
				startAngleDeg = Mathf.Repeat(startAngleDeg, 360f);
				endAngleDeg = Mathf.Repeat(endAngleDeg, 360f);
				if (endAngleDeg < startAngleDeg)
				{
					endAngleDeg += 360f;
				}
				float num = (endAngleDeg - startAngleDeg) / (float)segments;
				Vector2 vector = new Vector2(Mathf.Cos(startAngleDeg * 0.0174532924f), Mathf.Sin(startAngleDeg * 0.0174532924f));
				Vector2 vector2 = new Vector2(Mathf.Cos(endAngleDeg * 0.0174532924f), Mathf.Sin(endAngleDeg * 0.0174532924f));
				FastRenderer.DrawLine(center, center + vector * radius, color, thickness);
				FastRenderer.DrawLine(center, center + vector2 * radius, color, thickness);
				Vector2 vector3 = center + vector * radius;
				for (int i = 1; i <= segments; i++)
				{
					float num2 = startAngleDeg + (float)i * num;
					Vector2 vector4 = new Vector2(Mathf.Cos(num2 * 0.0174532924f), Mathf.Sin(num2 * 0.0174532924f));
					Vector2 vector5 = center + vector4 * radius;
					FastRenderer.DrawLine(vector3, vector5, color, thickness);
					vector3 = vector5;
				}
				return;
			}
        }

        // 你他妈毕业于肇庆技师学院还敢自称“生来强大”？强大个鸡巴，你就配被我们天天羞辱
        public static void DrawTriangleOutline(Vector2 a, Vector2 b, Vector2 c, Color color, float thickness = 1f)
		{
			FastRenderer.DrawLine(a, b, color, thickness);
			FastRenderer.DrawLine(b, c, color, thickness);
			FastRenderer.DrawLine(c, a, color, thickness);
		}
		public static void DrawCornerBox(Rect rect, Color color, float thickness = 1f, float cornerLength = 10f, bool autoScale = false)
		{
			float num = rect.x;
			float num2 = rect.y;
			float num3 = rect.width;
			float num4 = rect.height;
			if (num3 < 0f)
			{
				num += num3;
				num3 = -num3;
			}
			if (num4 < 0f)
			{
				num2 += num4;
				num4 = -num4;
			}
			float num5 = cornerLength;
			if (autoScale)
			{
				float num6 = Mathf.Min(num3 / 2f, num4 / 2f);
				num5 = Mathf.Min(cornerLength, num6);
			}
			float num7 = Mathf.Min(num5, num3 / 2f - 4f);
			float num8 = Mathf.Min(num5, num4 / 2f);
			Vector2 vector = new Vector2(num, num2);
			Vector2 vector2 = new Vector2(num + num3, num2);
			Vector2 vector3 = new Vector2(num, num2 + num4);
			Vector2 vector4 = new Vector2(num + num3, num2 + num4);
			FastRenderer.DrawLine(vector, vector + new Vector2(num7, 0f), color, thickness);
			FastRenderer.DrawLine(vector, vector + new Vector2(0f, num8), color, thickness);
			FastRenderer.DrawLine(vector2, vector2 - new Vector2(num7, 0f), color, thickness);
			FastRenderer.DrawLine(vector2, vector2 + new Vector2(0f, num8), color, thickness);
			FastRenderer.DrawLine(vector3, vector3 + new Vector2(num7, 0f), color, thickness);
			FastRenderer.DrawLine(vector3, vector3 - new Vector2(0f, num8), color, thickness);
			FastRenderer.DrawLine(vector4, vector4 - new Vector2(num7, 0f), color, thickness);
			FastRenderer.DrawLine(vector4, vector4 - new Vector2(0f, num8), color, thickness);
		}

        public static void DrawSectorFilled(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, int segments, Color color)
		{
			FastRenderer.EnsureMaterialInitialized();
			if (FastRenderer._glMaterial != null && radius > 0f && segments >= 3)
			{
				startAngleDeg = Mathf.Repeat(startAngleDeg, 360f);
				endAngleDeg = Mathf.Repeat(endAngleDeg, 360f);
				if (endAngleDeg < startAngleDeg)
				{
					endAngleDeg += 360f;
				}
				float num = (endAngleDeg - startAngleDeg) / (float)segments;
				FastRenderer._glMaterial.SetPass(0);
				GL.PushMatrix();
				GL.LoadPixelMatrix();
				GL.Begin(4);
				GL.Color(color);
				Vector2 vector = new Vector2(Mathf.Cos(startAngleDeg * 0.0174532924f), Mathf.Sin(startAngleDeg * 0.0174532924f));
				Vector2 vector2 = center + vector * radius;
				for (int i = 1; i <= segments; i++)
				{
					float num2 = startAngleDeg + (float)i * num;
					Vector2 vector3 = new Vector2(Mathf.Cos(num2 * 0.0174532924f), Mathf.Sin(num2 * 0.0174532924f));
					Vector2 vector4 = center + vector3 * radius;
					GL.Vertex3(center.x, center.y, 0f);
					GL.Vertex3(vector2.x, vector2.y, 0f);
					GL.Vertex3(vector4.x, vector4.y, 0f);
					vector2 = vector4;
				}
				GL.End();
				GL.PopMatrix();
				return;
			}
        }

        // 你他妈毕业于肇庆技师学院还敢自称“生来强大”？强大个鸡巴，你就配被我们天天羞辱
        public static void DrawLinearPathRendererr(Vector3 start, Vector3 end, Color color)
		{
			if (PlayerStateTracker.MainCamera == null)
			{
				return;
			}
			FastRenderer.EnsureMaterialInitialized();
			FastRenderer._glMaterial.SetPass(0);
			GL.PushMatrix();
			GL.LoadProjectionMatrix(PlayerStateTracker.MainCamera.projectionMatrix);
			GL.modelview = PlayerStateTracker.MainCamera.worldToCameraMatrix;
			GL.Begin(1);
			GL.Color(color);
			GL.Vertex(start);
			GL.Vertex(end);
			GL.End();
			GL.PopMatrix();
        }

        // 你他妈毕业于肇庆技师学院还敢自称“生来强大”？强大个鸡巴，你就配被我们天天羞辱
        public static void DrawImpactPoint(Vector2 center, Color color, float size = 4f, bool outline = true)
		{
			FastRenderer.DrawCircleFilled(center, size, color, 12);
			if (outline)
			{
				Color color2 = new Color(0f, 0f, 0f, color.a);
				FastRenderer.DrawCircleOutline(center, size + 1f, 12, color2, true);
			}
        }

        // 你他妈毕业于肇庆技师学院还敢自称“生来强大”？强大个鸡巴，你就配被我们天天羞辱
        public static void DrawPolygon(Vector2[] points, Color color, bool filled = true)
		{
			if (points == null || points.Length < 3)
			{
				return;
			}
			FastRenderer.EnsureMaterialInitialized();
			if (FastRenderer._glMaterial == null)
			{
				return;
			}
			FastRenderer._glMaterial.SetPass(0);
			GL.PushMatrix();
			GL.LoadPixelMatrix();
			if (filled)
			{
				GL.Begin(4);
				GL.Color(color);
				for (int i = 1; i < points.Length - 1; i++)
				{
					GL.Vertex3(points[0].x, points[0].y, 0f);
					GL.Vertex3(points[i].x, points[i].y, 0f);
					GL.Vertex3(points[i + 1].x, points[i + 1].y, 0f);
				}
			}
			else
			{
				GL.Begin(1);
				GL.Color(color);
				for (int j = 0; j < points.Length; j++)
				{
					int num = (j + 1) % points.Length;
					GL.Vertex3(points[j].x, points[j].y, 0f);
					GL.Vertex3(points[num].x, points[num].y, 0f);
				}
			}
			GL.End();
			GL.PopMatrix();
		}

        public static void DrawCircleOutline(Vector2 position, float radius, int numSides, Color color, bool centered = true)
		{
			FastRenderer.EnsureMaterialInitialized();
			if (FastRenderer._glMaterial != null && radius > 0f)
			{
				GL.PushMatrix();
				FastRenderer._glMaterial.SetPass(0);
				GL.LoadOrtho();
				GL.Begin(1);
				GL.Color(color);
				float num = 360f / (float)numSides;
				Vector2 vector = (centered ? position : (position + Vector2.one * radius));
				for (int i = 0; i < numSides; i += 1)
				{
					float num2 = 0.0174532924f * ((float)i * num);
					float num3 = 0.0174532924f * ((float)(i + 1) * num);
					Vector2 vector2 = vector + new Vector2(Mathf.Cos(num2), Mathf.Sin(num2)) * radius;
					Vector2 vector3 = vector + new Vector2(Mathf.Cos(num3), Mathf.Sin(num3)) * radius;
					GL.Vertex(new Vector3(vector2.x / (float)Screen.width, vector2.y / (float)Screen.height, 0f));
					GL.Vertex(new Vector3(vector3.x / (float)Screen.width, vector3.y / (float)Screen.height, 0f));
				}
				GL.End();
				GL.PopMatrix();
				return;
			}
        }

        // 你他妈毕业于肇庆技师学院还敢自称“生来强大”？强大个鸡巴，你就配被我们天天羞辱
        public static void DrawString(Vector2 pos, string text, Color color, bool center = false, int fontSize = 12)
		{
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			if (FastRenderer._cachedLabelStyle == null || FastRenderer._cachedLabelStyle.fontSize != fontSize)
			{
				FastRenderer._cachedLabelStyle = new GUIStyle(GUI.skin.label)
				{
					fontSize = fontSize,
					fontStyle = FontStyle.Bold
				};
			}
			GUIContent guicontent = new GUIContent(text);
			Vector2 vector = FastRenderer._cachedLabelStyle.CalcSize(guicontent);
			Rect rect = new Rect(pos, vector);
			if (center)
			{
				rect.x -= vector.x / 2f;
			}
			FastRenderer._cachedLabelStyle.normal.textColor = Color.black;
			GUI.Label(new Rect(rect.x - 1f, rect.y, vector.x, vector.y), guicontent, FastRenderer._cachedLabelStyle);
			GUI.Label(new Rect(rect.x + 1f, rect.y, vector.x, vector.y), guicontent, FastRenderer._cachedLabelStyle);
			GUI.Label(new Rect(rect.x, rect.y - 1f, vector.x, vector.y), guicontent, FastRenderer._cachedLabelStyle);
			GUI.Label(new Rect(rect.x, rect.y + 1f, vector.x, vector.y), guicontent, FastRenderer._cachedLabelStyle);
			FastRenderer._cachedLabelStyle.normal.textColor = color;
			GUI.Label(rect, guicontent, FastRenderer._cachedLabelStyle);
		}
		public static void DrawArrow(Vector2 start, Vector2 end, Color color, float headSize = 10f, float shaftWidth = 2f)
		{
			FastRenderer.DrawLine(start, end, color, shaftWidth);
			Vector2 normalized = (end - start).normalized;
			Vector2 vector = new Vector2(-normalized.y, normalized.x);
			Vector2 vector2 = end - normalized * headSize + vector * headSize * 0.5f;
			Vector2 vector3 = end - normalized * headSize - vector * headSize * 0.5f;
			FastRenderer.DrawFilledTriangle(end, vector2, vector3, color);
		}
		public static void DrawBoxFilled(Rect rect, Color color)
		{
			FastRenderer.EnsureMaterialInitialized();
			if (FastRenderer._glMaterial == null)
			{
				return;
			}
			Vector2 vector = new Vector2(rect.x, rect.y);
			Vector2 vector2 = new Vector2(rect.x + rect.width, rect.y);
			Vector2 vector3 = new Vector2(rect.x, rect.y + rect.height);
			Vector2 vector4 = new Vector2(rect.x + rect.width, rect.y + rect.height);
			FastRenderer._glMaterial.SetPass(0);
			GL.PushMatrix();
			GL.LoadPixelMatrix();
			GL.Begin(7);
			GL.Color(color);
			GL.Vertex3(vector.x, vector.y, 0f);
			GL.Vertex3(vector2.x, vector2.y, 0f);
			GL.Vertex3(vector4.x, vector4.y, 0f);
			GL.Vertex3(vector3.x, vector3.y, 0f);
			GL.End();
			GL.PopMatrix();
        }

        // 你他妈毕业于肇庆技师学院还敢自称“生来强大”？强大个鸡巴，你就配被我们天天羞辱
        public static void DrawLine(Vector2 start, Vector2 end, Color color, float width = 1f)
		{
			FastRenderer.EnsureMaterialInitialized();
			if (FastRenderer._glMaterial == null)
			{
				return;
			}
			FastRenderer._glMaterial.SetPass(0);
			GL.PushMatrix();
			GL.LoadPixelMatrix();
			if (Mathf.Approximately(width, 1f))
			{
				GL.Begin(1);
				GL.Color(color);
				GL.Vertex3(start.x, start.y, 0f);
				GL.Vertex3(end.x, end.y, 0f);
				GL.End();
			}
			else
			{
				Vector2 normalized = (end - start).normalized;
				Vector2 vector = new Vector2(-normalized.y, normalized.x) * width / 2f;
				GL.Begin(7);
				GL.Color(color);
				GL.Vertex3(start.x + vector.x, start.y + vector.y, 0f);
				GL.Vertex3(start.x - vector.x, start.y - vector.y, 0f);
				GL.Vertex3(end.x - vector.x, end.y - vector.y, 0f);
				GL.Vertex3(end.x + vector.x, end.y + vector.y, 0f);
				GL.End();
			}
			GL.PopMatrix();
		}

        private static void EnsureMaterialInitialized()
		{
			if (FastRenderer._glMaterial != null)
			{
				return;
			}
			Shader shader = Shader.Find("Hidden/Internal-Colored");
			if (shader == null)
			{
				Debug.LogError("[渲染器] 无法找到所需的着色器");
				return;
			}
			FastRenderer._glMaterial = new Material(shader)
			{
				hideFlags = (HideFlags)(61)
			};
			FastRenderer._glMaterial.SetInt("_SrcBlend", 5);
			FastRenderer._glMaterial.SetInt("_DstBlend", 10);
			FastRenderer._glMaterial.SetInt("_Cull", 0);
			FastRenderer._glMaterial.SetInt("_ZWrite", 0);
		}
		public static void DrawCircleFilled(Vector2 center, float radius, Color color, int segments = 32)
		{
			FastRenderer.EnsureMaterialInitialized();
			if (FastRenderer._glMaterial != null && radius > 0f)
			{
				FastRenderer._glMaterial.SetPass(0);
				GL.PushMatrix();
				GL.LoadPixelMatrix();
				GL.Begin(4);
				GL.Color(color);
				float num = 6.28318548f / (float)segments;
				for (int i = 0; i < segments; i += 1)
				{
					float num2 = (float)i * num;
					float num3 = (float)(i + 1) * num;
					Vector2 vector = center + new Vector2(Mathf.Cos(num2) * radius, Mathf.Sin(num2) * radius);
					Vector2 vector2 = center + new Vector2(Mathf.Cos(num3) * radius, Mathf.Sin(num3) * radius);
					GL.Vertex3(center.x, center.y, 0f);
					GL.Vertex3(vector.x, vector.y, 0f);
					GL.Vertex3(vector2.x, vector2.y, 0f);
				}
				GL.End();
				GL.PopMatrix();
				return;
			}
        }

        // 你他妈毕业于肇庆技师学院还敢自称“生来强大”？强大个鸡巴，你就配被我们天天羞辱
        public static void DrawReticleRenderer(Vector2 center, Color color, float size = 20f, float thickness = 2f, float gap = 5f, bool dot = true, bool outline = true)
		{
			Vector2 vector = new Vector2(center.x, center.y - gap);
			Vector2 vector2 = new Vector2(center.x, center.y - gap - size);
			FastRenderer.DrawLine(vector, vector2, color, thickness);
			Vector2 vector3 = new Vector2(center.x, center.y + gap);
			Vector2 vector4 = new Vector2(center.x, center.y + gap + size);
			FastRenderer.DrawLine(vector3, vector4, color, thickness);
			Vector2 vector5 = new Vector2(center.x - gap, center.y);
			Vector2 vector6 = new Vector2(center.x - gap - size, center.y);
			FastRenderer.DrawLine(vector5, vector6, color, thickness);
			Vector2 vector7 = new Vector2(center.x + gap, center.y);
			Vector2 vector8 = new Vector2(center.x + gap + size, center.y);
			FastRenderer.DrawLine(vector7, vector8, color, thickness);
			if (dot)
			{
				FastRenderer.DrawCircleFilled(center, thickness * 1.5f, color, 12);
			}
			if (outline)
			{
				Color color2 = new Color(0f, 0f, 0f, color.a);
				FastRenderer.DrawLine(vector, vector2, color2, thickness + 2f);
				FastRenderer.DrawLine(vector3, vector4, color2, thickness + 2f);
				FastRenderer.DrawLine(vector5, vector6, color2, thickness + 2f);
				FastRenderer.DrawLine(vector7, vector8, color2, thickness + 2f);
				if (dot)
				{
					FastRenderer.DrawCircleOutline(center, thickness * 1.5f + 1f, 12, color2, true);
				}
			}
        }

        // 你他妈毕业于肇庆技师学院还敢自称“生来强大”？强大个鸡巴，你就配被我们天天羞辱
        public static void DrawFilledTriangle(Vector2 a, Vector2 b, Vector2 c, Color color)
		{
			FastRenderer.EnsureMaterialInitialized();
			if (FastRenderer._glMaterial == null)
			{
				return;
			}
			FastRenderer._glMaterial.SetPass(0);
			GL.PushMatrix();
			GL.LoadPixelMatrix();
			GL.Begin(4);
			GL.Color(color);
			GL.Vertex3(a.x, a.y, 0f);
			GL.Vertex3(b.x, b.y, 0f);
			GL.Vertex3(c.x, c.y, 0f);
			GL.End();
			GL.PopMatrix();
		}
		public static Color GetRainbowColor(float speed = 15f)
		{
			float h = (Time.time * speed) % 1f;
			return FastRenderer.HueToRGB(h);
		}
		public static Color GetRainbowColor(float speed, float hueOffset)
		{
			float h = (Time.time * speed + hueOffset) % 1f;
			return FastRenderer.HueToRGB(h);
		}
		private static Color HueToRGB(float h)
		{
			float r = Mathf.Abs(h * 6f - 3f) - 1f;
			float g = 2f - Mathf.Abs(h * 6f - 2f);
			float b = 2f - Mathf.Abs(h * 6f - 4f);
			return new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), 1f);
		}
		public static void DrawRainbowCircleOutline(Vector2 position, float radius, int numSides, float speed = 15f, bool centered = true)
		{
			FastRenderer.EnsureMaterialInitialized();
			if (FastRenderer._glMaterial != null && radius > 0f && numSides >= 3)
			{
				GL.PushMatrix();
				FastRenderer._glMaterial.SetPass(0);
				GL.LoadOrtho();
				float angleStep = 360f / (float)numSides;
				Vector2 center = (centered ? position : (position + Vector2.one * radius));
				for (int i = 0; i < numSides; i++)
				{
					float a1 = 0.0174532924f * ((float)i * angleStep);
					float a2 = 0.0174532924f * ((float)(i + 1) * angleStep);
					Vector2 v1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * radius;
					Vector2 v2 = center + new Vector2(Mathf.Cos(a2), Mathf.Sin(a2)) * radius;
					Color c1 = FastRenderer.GetRainbowColor(speed, (float)i / numSides);
					Color c2 = FastRenderer.GetRainbowColor(speed, (float)(i + 1) / numSides);
					GL.Begin(1);
					GL.Color(c1);
					GL.Vertex(new Vector3(v1.x / (float)Screen.width, v1.y / (float)Screen.height, 0f));
					GL.Color(c2);
					GL.Vertex(new Vector3(v2.x / (float)Screen.width, v2.y / (float)Screen.height, 0f));
					GL.End();
				}
				GL.PopMatrix();
			}
		}
		public static void DrawPhallicOutline(Vector2 center, float radius, Color color, float thickness = 1.0f)
		{
			System.Collections.Generic.List<Vector2> points = new System.Collections.Generic.List<Vector2>();
			float W = 0.28f * radius;
			float lobeR = 0.32f * radius;
			// 左叶 (80°�?40° CCW，共 260° �?
			Vector2 leftLobeCenter = new Vector2(center.x - 0.32f * radius, center.y + 0.25f * radius);
			for (float a = 80f; a <= 340f; a += 10f)
			{
				float rad = a * Mathf.Deg2Rad;
				points.Add(leftLobeCenter + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * lobeR);
			}
			points.Add(new Vector2(center.x - W, center.y + 0.15f * radius));
			points.Add(new Vector2(center.x - W, center.y - 0.5f * radius));
			// 茎头顶部
			Vector2 topCenter = new Vector2(center.x, center.y - 0.5f * radius);
			for (float a = 180f; a >= 0f; a -= 10f)
			{
				float rad = a * Mathf.Deg2Rad;
				points.Add(topCenter + new Vector2(Mathf.Cos(rad), -Mathf.Abs(Mathf.Sin(rad))) * W);
			}
			points.Add(new Vector2(center.x + W, center.y - 0.5f * radius));
			points.Add(new Vector2(center.x + W, center.y + 0.15f * radius));
			// 右叶：镜像左叶，�?460°(�?00°)递减�?200°，覆盖相�?260° �?
			Vector2 rightLobeCenter = new Vector2(center.x + 0.32f * radius, center.y + 0.25f * radius);
			for (float a = 460f; a >= 200f; a -= 10f)
			{
				float rad = a * Mathf.Deg2Rad;
				points.Add(rightLobeCenter + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * lobeR);
			}
			for (int i = 0; i < points.Count - 1; i++)
			{
				FastRenderer.DrawLine(points[i], points[i + 1], color, thickness);
			}
		}
		private static GUIStyle _cachedLabelStyle;
		private static Material _glMaterial;
	}
}