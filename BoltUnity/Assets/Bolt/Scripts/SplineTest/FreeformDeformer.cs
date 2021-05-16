using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using ReadOnlyAttribute = Unity.Collections.ReadOnlyAttribute;
using Helpers;

namespace Deform.Custom
{
	[Deformer(Name = "Freeform", Description = "3D representation of bezier curves", XRotation = -90f, Type = typeof(FreeformDeformer))]
	public class FreeformDeformer : Deformer, IFactor
	{
		[SerializeField] private float factor = 1;
		[SerializeField] private int3 count = new int3(4, 4, 4);

		[ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, DraggableItems = false)]
		[SerializeField] private Vector3[] controlPoints;

		public event System.Action NumControlPointsChanged;

		public int3 Count => count;
		public int Count3d => count.x * count.y * count.z;
		public float Factor { get => factor; set => factor = value; }

		public Vector3 this[int x, int y, int z]
		{
			get => controlPoints[ToIndex(count.y, count.z, x, y, z)];
			set => controlPoints[ToIndex(count.y, count.z, x, y, z)] = value;
		}

		public Vector3 this[int i]
		{
			get => controlPoints[i];
			set => controlPoints[i] = value;
		}

		public static int ToIndex(int sizeY, int sizeZ, int x, int y, int z) => x * sizeY * sizeZ + y * sizeZ + z;

		public int ToIndex(int x, int y, int z) => ToIndex(count.y, count.z, x, y, z);

		public Vector3 ToWorld(Vector3 localPoint) => transform.TransformPoint(localPoint);


		private void Reset()
		{
			controlPoints = new Vector3[Count3d];

			for (int x = 0; x < count.x; ++x)
				for (int y = 0; y < count.y; ++y)
					for (int z = 0; z < count.z; ++z)
					{
						this[x, y, z] = new Vector3(
							x / (count.x - 1), 
							y / (count.y - 1), 
							z / (count.z - 1));
					}

			NumControlPointsChanged?.Invoke();
		}

		//public void OnDrawGizmos()
		//{
		//	Gizmos.color = Color.yellow;
		//	for (int x = 0; x < Count.x; ++x)
		//	{
		//		for (int y = 0; y < Count.y; ++y)
		//		{
		//			for (int z = 0; z < Count.z; ++z)
		//			{
		//				if (x + 1 < Count.x) Gizmos.DrawLine(ToWorld(this[x, y, z]), ToWorld(this[x + 1, y, z]));
		//				if (y + 1 < Count.y) Gizmos.DrawLine(ToWorld(this[x, y, z]), ToWorld(this[x, y + 1, z]));
		//				if (z + 1 < Count.z) Gizmos.DrawLine(ToWorld(this[x, y, z]), ToWorld(this[x, y, z + 1]));
		//			}
		//		}
		//	}
		//
		//	if (test == null)
		//	{
		//		test = new GameObject("TEST").transform;
		//	}
		//
		//	Gizmos.color = Color.red;
		//	Vector3 worldPos = test.position;
		//	Vector3 pos = transform.InverseTransformPoint(worldPos);
		//	if (pos.x < 0 || pos.x > 1 || pos.y < 0 || pos.y > 1|| pos.z < 0 || pos.z > 1)
		//	{
		//		Gizmos.DrawWireSphere(test.position, 0.2f);
		//	}
		//	else
		//	{
		//		Vector3 newP = new FFDJob().Manual(this, worldPos);
		//		Gizmos.DrawWireSphere(newP, 0.1f);
		//	}
		//}

		public override DataFlags DataFlags => DataFlags.Vertices;

		public override JobHandle Process(MeshData data, JobHandle dependency = default)
		{
			if (controlPoints.Length != Count3d)
				return dependency;

			var meshToAxis = DeformerUtils.GetMeshToAxisSpace(transform, data.Target.GetTransform());

			NativeArray<float3> controlPointData = new NativeArray<float3>(Count3d, Allocator.TempJob);

			for (int i = 0; i < Count3d; ++i)
			{
				controlPointData[i] = controlPoints[i];
			}

			return new FFDJob
			{
				controlPoints = controlPointData,
				factor = factor,
				totalSize = count,
				influenceMaxSize = count,
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
			[ReadOnly] public float factor;
			[ReadOnly] public int3 totalSize;
			[ReadOnly] public int3 influenceMaxSize;
			[ReadOnly] public float4x4 meshToAxis;
			[ReadOnly] public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			private struct SampleParams
			{
				public int minX;
				public int minY;
				public int minZ;
				public int sizeX;
				public int sizeY;
				public int sizeZ;
				public float tX;
				public float tY;
				public float tZ;
			}

			public void Execute(int index)
			{
				float3 vert = mul(meshToAxis, float4(vertices[index], 1f)).xyz;
				vert = Deform(vert);
				vertices[index] = mul(axisToMesh, new float4(vert, 1)).xyz;
			}

			private float3 Deform(float3 vert)
			{
				SampleParams sampleParams = new SampleParams
				{
					minX = 0,
					minY = 0,
					minZ = 0,
					sizeX = influenceMaxSize.x,
					sizeY = influenceMaxSize.y,
					sizeZ = influenceMaxSize.z,
					tX = vert.x,
					tY = vert.y,
					tZ = vert.z,
				};
				float3 deformed = GetCageBezierZYX(ref sampleParams);
				return lerp(vert, deformed, factor);
			}


			private float3 ManualSingle(FreeformDeformer deformer, float3 worldPos)
			{
				float3 vert = deformer.transform.InverseTransformPoint(worldPos);
				vert = Deform(vert);
				return deformer.transform.TransformPoint(vert);
			}

			private void ManualPrepareJob(FreeformDeformer deformer)
			{
				factor = deformer.factor;
				influenceMaxSize = new int3(4, 4, 4);
				for (int i = 0; i < deformer.Count3d; ++i)
					controlPoints[i] = deformer[i];
			}

			public void Manual(FreeformDeformer deformer, IList<Vector3> inOutWorldPositions)
			{
				using (controlPoints = new NativeArray<float3>(deformer.Count3d, Allocator.Temp))
				{
					ManualPrepareJob(deformer);

					for (int i = 0; i < inOutWorldPositions.Count; ++i)
						inOutWorldPositions[i] = ManualSingle(deformer, inOutWorldPositions[i]);
				}
			}

			public void Manual(FreeformDeformer deformer, IList<float3> inOutWorldPositions)
			{
				using (controlPoints = new NativeArray<float3>(deformer.Count3d, Allocator.Temp))
				{
					ManualPrepareJob(deformer);

					for (int i = 0; i < inOutWorldPositions.Count; ++i)
						inOutWorldPositions[i] = ManualSingle(deformer, inOutWorldPositions[i]);
				}
			}

			public float3 Manual(FreeformDeformer deformer, float3 worldPos)
			{
				using (controlPoints = new NativeArray<float3>(deformer.Count3d, Allocator.Temp))
				{
					ManualPrepareJob(deformer);

					worldPos = ManualSingle(deformer, worldPos);
				}
				return worldPos;
			}

			private float3 GetCageBezierZ(ref SampleParams sampleParams, int x, int y)
			{
				int minZ = ToIndex(totalSize.y, totalSize.z, x, y, sampleParams.minZ);
				int sizeZ = sampleParams.sizeZ;
				float t = sampleParams.tZ;

				float3 c0 = controlPoints[minZ];
				float3 c1 = controlPoints[minZ + 1];

				if (sizeZ > 2)
				{
					float3 c2 = controlPoints[minZ + 2];

					if (sizeZ > 3)
					{
						float3 c3 = controlPoints[minZ + 3];
						return MathHelper.CubicBezier(c0, c1, c2, c3, t);
					}
					return MathHelper.QuadBezier(c0, c1, c2, t);
				}
				return lerp(c0, c1, t);
			}

			private float3 GetCageBezierZY(ref SampleParams sampleParams, int x)
			{
				int minY = sampleParams.minY;
				int sizeY = sampleParams.sizeY;
				float t = sampleParams.tY;

				float3 c0 = GetCageBezierZ(ref sampleParams, x, minY);
				float3 c1 = GetCageBezierZ(ref sampleParams, x, minY + 1);

				if (sizeY > 2)
				{
					float3 c2 = GetCageBezierZ(ref sampleParams, x, minY + 2);

					if (sizeY > 3)
					{
						float3 c3 = GetCageBezierZ(ref sampleParams, x, minY + 3);
						return MathHelper.CubicBezier(c0, c1, c2, c3, t);
					}
					return MathHelper.QuadBezier(c0, c1, c2, t);
				}
				return lerp(c0, c1, t);
			}

			private float3 GetCageBezierZYX(ref SampleParams sampleParams)
			{
				int minX = sampleParams.minX;
				int sizeX = sampleParams.sizeX;
				float t = sampleParams.tX;

				float3 c0 = GetCageBezierZY(ref sampleParams, minX);
				float3 c1 = GetCageBezierZY(ref sampleParams, minX + 1);

				if (sizeX > 2)
				{
					float3 c2 = GetCageBezierZY(ref sampleParams, minX + 2);

					if (sizeX > 3)
					{
						float3 c3 = GetCageBezierZY(ref sampleParams, minX + 3);
						return MathHelper.CubicBezier(c0, c1, c2, c3, t);
					}
					return MathHelper.QuadBezier(c0, c1, c2, t);
				}
				return lerp(c0, c1, t);
			}

		}

	}
}