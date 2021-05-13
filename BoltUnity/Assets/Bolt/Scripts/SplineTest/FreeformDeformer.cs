using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using ReadOnlyAttribute = Unity.Collections.ReadOnlyAttribute;

namespace Deform.Custom
{
	[Deformer(Name = "Freeform", Description = "3D representation of bezier curves", XRotation = -90f, Type = typeof(FreeformDeformer))]
	public class FreeformDeformer : Deformer
	{
		public const int count1d = 4;
		public const int count2d = count1d * count1d;
		public const int count3d = count1d * count1d * count1d;

		internal static HashSet<FreeformDeformer> currentlyEditedFFDs = new HashSet<FreeformDeformer>();

		[ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, NumberOfItemsPerPage = 16, DraggableItems = false)]
		[SerializeField] private Vector3[] controlPoints = new Vector3[count3d];
		[HideInInspector]
		[SerializeField] private FreeformDeformEditNode[] editNodes;

		internal FreeformDeformEditNode[] EditNodes => editNodes;

		public override DataFlags DataFlags => DataFlags.Vertices;

		public Vector3 this[int x, int y, int z]
		{
			get => controlPoints[ToIndex(x, y, z)];
			set => controlPoints[ToIndex(x, y, z)] = value;
		}

		public Vector3 this[int i]
		{
			get => controlPoints[i];
			set => controlPoints[i] = value;
		}

		public static void ToXYZ(int i, out int x, out int y, out int z)
		{
			z = i % count1d;
			y = (i % count2d) / count1d;
			x = i / count2d;
		}

		public static int ToIndex(int x, int y, int z)
		{
			return x * count1d * count1d + y * count1d + z;
		}

		public Vector3 ToWorld(Vector3 localPoint)
		{
			return transform.TransformPoint(localPoint);
		}

		public Vector3 ToLocal(Vector3 worldPoint)
		{
			return transform.InverseTransformPoint(worldPoint);
		}

		private void Reset()
		{
			controlPoints = new Vector3[count3d];
			editNodes = new FreeformDeformEditNode[count3d];

			for (int x = 0; x < count1d; ++x)
				for (int y = 0; y < count1d; ++y)
					for (int z = 0; z < count1d; ++z)
					{
						this[x, y, z] = new Vector3(x, y, z) / (count1d - 1);
					}
		}

		private static float3 GetCageBezierZ(ref NativeArray<float3> ffd, int x, int y, float tZ)
		{
			int i = ToIndex(x, y, 0);
			float3 s0 = ffd[i];
			float3 s1 = ffd[i+1];
			float3 s2 = ffd[i+2];
			float3 s3 = ffd[i+3];

			return CubicBezier(s0, s1, s2, s3, tZ);
		}

		private static float3 GetCageBezierZY(ref NativeArray<float3> ffd, int x, float tY, float tZ)
		{
			float3 s0 = GetCageBezierZ(ref ffd, x, 0, tZ);
			float3 s1 = GetCageBezierZ(ref ffd, x, 1, tZ);
			float3 s2 = GetCageBezierZ(ref ffd, x, 2, tZ);
			float3 s3 = GetCageBezierZ(ref ffd, x, 3, tZ);

			return CubicBezier(s0, s1, s2, s3, tY);
		}

		private static float3 GetCageBezierZYX(ref NativeArray<float3> ffd, float3 t)
		{
			float3 s0 = GetCageBezierZY(ref ffd, 0, t.y, t.z);
			float3 s1 = GetCageBezierZY(ref ffd, 1, t.y, t.z);
			float3 s2 = GetCageBezierZY(ref ffd, 2, t.y, t.z);
			float3 s3 = GetCageBezierZY(ref ffd, 3, t.y, t.z);

			return CubicBezier(s0, s1, s2, s3, t.x);
		}



		private float3 GetCageBezierZ(int x, int y, float tZ)
		{
			float3 s0 = controlPoints[ToIndex(x, y, 0)];
			float3 s1 = controlPoints[ToIndex(x, y, 1)];
			float3 s2 = controlPoints[ToIndex(x, y, 2)];
			float3 s3 = controlPoints[ToIndex(x, y, 3)];

			Gizmos.DrawLine(transform.TransformPoint(s0), transform.TransformPoint(s1));
			Gizmos.DrawLine(transform.TransformPoint(s1), transform.TransformPoint(s2));
			Gizmos.DrawLine(transform.TransformPoint(s2), transform.TransformPoint(s3));

			return CubicBezier(s0, s1, s2, s3, tZ);
		}

		private float3 GetCageBezierZY(int x, float tY, float tZ)
		{
			float3 s0 = GetCageBezierZ(x, 0, tZ);
			float3 s1 = GetCageBezierZ(x, 1, tZ);
			float3 s2 = GetCageBezierZ(x, 2, tZ);
			float3 s3 = GetCageBezierZ(x, 3, tZ);

			Gizmos.DrawLine(transform.TransformPoint(s0), transform.TransformPoint(s1));
			Gizmos.DrawLine(transform.TransformPoint(s1), transform.TransformPoint(s2));
			Gizmos.DrawLine(transform.TransformPoint(s2), transform.TransformPoint(s3));

			return CubicBezier(s0, s1, s2, s3, tY);
		}

		private float3 GetCageBezierZYX(float3 pos)
		{
			float3 s0 = GetCageBezierZY(0, pos.y, pos.z);
			float3 s1 = GetCageBezierZY(1, pos.y, pos.z);
			float3 s2 = GetCageBezierZY(2, pos.y, pos.z);
			float3 s3 = GetCageBezierZY(3, pos.y, pos.z);

			Gizmos.DrawLine(transform.TransformPoint(s0), transform.TransformPoint(s1));
			Gizmos.DrawLine(transform.TransformPoint(s1), transform.TransformPoint(s2));
			Gizmos.DrawLine(transform.TransformPoint(s2), transform.TransformPoint(s3));

			return CubicBezier(s0, s1, s2, s3, pos.x);
		}

		public void OnDrawGizmos()
		{
			Gizmos.color = Color.yellow;
			for (int x = 0; x < count1d; ++x)
			{
				for (int y = 0; y < count1d; ++y)
				{
					for (int z = 0; z < count1d; ++z)
					{
						if (x + 1 < count1d) Gizmos.DrawLine(ToWorld(this[x, y, z]), ToWorld(this[x + 1, y, z]));
						if (y + 1 < count1d) Gizmos.DrawLine(ToWorld(this[x, y, z]), ToWorld(this[x, y + 1, z]));
						if (z + 1 < count1d) Gizmos.DrawLine(ToWorld(this[x, y, z]), ToWorld(this[x, y, z + 1]));
					}
				}
			}

			if (test == null)
			{
				test = new GameObject("TEST").transform;
			}

			Gizmos.color = Color.red;
			Vector3 worldPos = test.position;
			Vector3 pos = transform.InverseTransformPoint(worldPos);
			if (pos.x < 0 || pos.x > 1 || pos.y < 0 || pos.y > 1|| pos.z < 0 || pos.z > 1)
			{
				Gizmos.DrawWireSphere(test.position, 0.2f);
			}
			else
			{
				Vector3 newP = GetCageBezierZYX(pos);

				Gizmos.DrawWireSphere(transform.TransformPoint(newP), 0.1f);
			}
		}

		public Transform test;

		public override JobHandle Process(MeshData data, JobHandle dependency = default)
		{
			if (controlPoints.Length != count3d)
				return dependency;

			var meshToAxis = DeformerUtils.GetMeshToAxisSpace(transform, data.Target.GetTransform());

			NativeArray<float3> controlPointData = new NativeArray<float3>(count3d, Allocator.TempJob);

			for (int i = 0; i < count3d; ++i)
			{
				controlPointData[i] = controlPoints[i];
			}

			return new FFDJob
			{
				controlPoints = controlPointData,
				meshToAxis = meshToAxis,
				axisToMesh = meshToAxis.inverse,
				vertices = data.DynamicNative.VertexBuffer
			}.Schedule(data.Length, DEFAULT_BATCH_COUNT, dependency);
		}

		[BurstCompile(CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct FFDJob : IJobParallelFor
		{
			[DeallocateOnJobCompletion]
			[ReadOnly] public NativeArray<float3> controlPoints;
			[ReadOnly] public float4x4 meshToAxis;
			[ReadOnly] public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			public void Execute(int index)
			{
				float3 vert = mul(meshToAxis, float4(vertices[index], 1f)).xyz;

				float3 modified = GetCageBezierZYX(ref controlPoints, vert);

				vertices[index] = mul(axisToMesh, new float4(modified, 1)).xyz;
			}
		}

		private static float3 QuadBezier(float3 p0, float3 p1, float3 p2, float t)
		{
			float tInv = 1f - t;
			return
				p0 * (tInv * tInv) +
				p1 * (2f * tInv * t) +
				p2 * (t * t);
		}

		private static float3 CubicBezier(float3 p0, float3 p1, float3 p2, float3 p3, float t)
		{
			float tInv = 1f - t;
			float tInv2 = tInv * tInv;
			float t2 = t * t;
			return
				p0 * (tInv2 * tInv) +
				p1 * (3f * tInv2 * t) +
				p2 * (3f * tInv * t2) +
				p3 * (t2 * t);
		}

	}
}