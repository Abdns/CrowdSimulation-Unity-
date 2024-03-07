using UnityEngine;

public class UnitInstancer : MonoBehaviour
{
    [SerializeField] private UnitDataInitializer _unitData;
    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;
    [SerializeField] private Texture2D[] _textures;

    private Material _materialInstancer;
    private ComputeBuffer _argsBuffer;
    private uint[] _args = { 0, 0, 0, 0, 0 };

    private void Start()
    {
        MaterialInitialize();
        CreateArgsBuffer();
    }
   
    private void Update()
    {
        Simulate();
    }

    private void CreateArgsBuffer() => _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

    private void Simulate()
    {
        UpdateBufferArgs(_mesh, _materialInstancer);
        Graphics.DrawMeshInstancedIndirect(_mesh, 0, _materialInstancer, new Bounds(_unitData.BoundCenter, _unitData.BoundSize), _argsBuffer);
    }

    private void MaterialInitialize()
    {
        _materialInstancer = Instantiate(_material);
        SetTextureArray(_materialInstancer, 2048);
    }

    private void SetTextureArray(Material material, int textureSize)
    {
        Texture2DArray textureArray = new Texture2DArray(textureSize, textureSize, _textures.Length, TextureFormat.RGBA32, false);

        for (int i = 0; i < _textures.Length; i++)
        {
            Graphics.CopyTexture(_textures[i], 0, 0, textureArray, i, 0);
        }

        material.SetTexture("_Textures", textureArray);

    }

    private void UpdateBufferArgs(Mesh mesh, Material meterial)
    {   
        _args[0] = mesh.GetIndexCount(0);
        _args[1] = (uint)_unitData.MaxObjectCount;
        _args[2] = mesh.GetIndexStart(0);
        _args[3] = mesh.GetBaseVertex(0);

        if(_argsBuffer == null)
        {
            return;
        }

        _argsBuffer.SetData(_args);
        meterial.SetBuffer("_UnitDataBuffer", _unitData.GetUnitDataBuffer());

    }
}
