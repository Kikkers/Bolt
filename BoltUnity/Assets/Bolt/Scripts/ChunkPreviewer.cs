using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ChunkPreviewer : MonoBehaviour
{
	public int res = 8;

	private Vector3[] _corners = new Vector3[4];

	private MeshRenderer chunkRenderer;
	private MeshFilter filter;

	private Mesh mesh;

	private void Awake()
	{
		chunkRenderer = GetComponent<MeshRenderer>();
		filter = GetComponent<MeshFilter>();

		mesh = filter.mesh;
		if (mesh == null)
			filter.mesh = mesh = new Mesh();
	}

	private void Start()
	{
		
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		int numCorners = _corners.Length;
		for(int i = 0; i < numCorners; ++i)
		{
			Gizmos.DrawLine(_corners[i], _corners[(i + 1) % numCorners]);
		}
	}

	private void BuildMesh(int index)
	{
		GameObject obj = new GameObject("mesh"+ index, typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
		obj.transform.SetParent(transform);
		MeshFilter filter = obj.GetComponent<MeshFilter>();

		int numCorners = _corners.Length;
		for (int i = 0; i < numCorners; ++i)
		{
			_corners[i] = Random.insideUnitSphere * 5;
		}

		Mesh mesh = new Mesh
		{
			vertices = _corners,
			triangles = new int[] { 0, 1, 2, 0, 2, 3 }
		};

		filter.sharedMesh = mesh;



		
	}
}
