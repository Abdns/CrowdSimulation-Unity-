using System.Runtime.InteropServices;
using UnityEngine;

public class UnitDataInitializer : MonoBehaviour
{
    const int SIMULATION_BLOCK_SIZE = 128;

    public int MaxObjectCount => _maxObjectCount;
    public Vector3 MoveDirection => _moveDirection;
    public Vector3 BoundCenter => transform.position;
    public Vector3 BoundSize => _boundSize;

    [SerializeField] [Range(128, 32768)] private int _maxObjectCount = 4096;
    [SerializeField] private Vector3 _moveDirection;
    [SerializeField] private Vector3 _boundSize;
    [SerializeField] private float _maxSpeed = 5.0f;
    [SerializeField] private ComputeShader _computeCS;

    private ComputeBuffer _unitDataBuffer;

    struct UnitData
    {
        public Vector3 Velocity;
        public Vector3 Position;
        public float Skin;
        public float AnimationSpeed;
    }

    public ComputeBuffer GetUnitDataBuffer() => _unitDataBuffer != null ? _unitDataBuffer : null;

    private void Start()
    {
        InitBuffer();
    }

    private void Update()
    {
        Simulate();
    }
    private void OnDestroy()
    {
        ReleaseBuffer();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(BoundCenter, BoundSize);
    }

    private void InitBuffer()
    {
        _unitDataBuffer = new ComputeBuffer(_maxObjectCount, Marshal.SizeOf(typeof(UnitData)));

        Vector3[] forceArray = new Vector3[_maxObjectCount];
        UnitData[] unitDataArray = new UnitData[_maxObjectCount];

        for (var i = 0; i < _maxObjectCount; i++)
        {
            forceArray[i] = Vector3.zero;
            unitDataArray[i].Position = GetUnitSeparatePosition();
            unitDataArray[i].Velocity = _moveDirection;
            unitDataArray[i].Skin = Random.Range(0, 3);
            unitDataArray[i].AnimationSpeed = Random.Range(1, 3);
        }

        _unitDataBuffer.SetData(unitDataArray);

        forceArray = null;
        unitDataArray = null;

    }

    private void Simulate()
    {
        ComputeShader cs = _computeCS;
        int id = -1;

        int threadGroupSize = Mathf.CeilToInt(_maxObjectCount / SIMULATION_BLOCK_SIZE);

        id = cs.FindKernel("CSMain");

        cs.SetInt("_MaxBoidObjectNum", _maxObjectCount);
        cs.SetVector("_BoundCenter", BoundCenter);
        cs.SetVector("_BoundSize", BoundSize);
        cs.SetFloat("_MaxSpeed", _maxSpeed);
        cs.SetFloat("_DeltaTime", Time.deltaTime);
        cs.SetBuffer(id, "_UnitDataBufferWrite", _unitDataBuffer);

        cs.Dispatch(id, threadGroupSize, 1, 1);
    }

    private Vector3 GetUnitSeparatePosition()
    {
        float xClampMin = (BoundCenter.x + BoundSize.x) / 2;
        float xClampMax = (BoundCenter.x + (-BoundSize.x)) / 2;

        float zClampMin = (BoundCenter.z + (BoundSize.z)) / 2;
        float zClampMax = (BoundCenter.z + (-BoundSize.z)) / 2;

        Vector3 startPosition = new Vector3(Random.Range(xClampMin, xClampMax), 0, Random.Range(zClampMin, zClampMax));

        return startPosition;
    }

    private void ReleaseBuffer()
    {
        if (_unitDataBuffer != null)
        {
            _unitDataBuffer.Release();
            _unitDataBuffer = null;
        }   
    }
}
