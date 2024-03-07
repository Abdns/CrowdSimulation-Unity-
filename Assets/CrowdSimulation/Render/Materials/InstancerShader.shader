Shader"Custom/Instanced"
{
    Properties
    {
        _Textures("Textures", 2DArray) = "" {}
        _PositionTex("PositionTexture", 2D) = "white"
        _Skin("Skin", int) = 1
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
        }

        Pass
        {
            CGPROGRAM
            
            #include "UnityCG.cginc"
            #pragma vertex vert; 
            #pragma fragment frag;
            #pragma require 2darray
            #pragma multi_compile_instancing

            UNITY_DECLARE_TEX2DARRAY(_Textures);

            struct UnitData
            {
                float3 velocity;
                float3 position;
                float skin;
                float animationSpeed;
            };

            struct v2f
            {
                float4 position : SV_Position;
                float2 uv_Textures : TEXCOORD0;
                uint v_InstanceID : instanceID;
            };

            StructuredBuffer<UnitData> _UnitDataBuffer;
            sampler2D _PositionTex;
            float4 _PositionTex_TexelSize;
            int _Skin;      
   
            float4x4 eulerAnglesToRotationMatrix(float3 angles)
            {
                float ch = cos(angles.y);
                float sh = sin(angles.y); 
                float ca = cos(angles.z);
                float sa = sin(angles.z); 
                float cb = cos(angles.x);
                float sb = sin(angles.x);

                return float4x4(
				            ch * ca + sh * sb * sa, -ch * sa + sh * sb * ca, sh * cb, 0,
				            cb * sa, cb * ca, -sb, 0,
				            -sh * ca + ch * sb * sa, sh * sa + ch * sb * ca, ch * cb, 0,
				            0, 0, 0, 1
			            );
            };
       

            v2f vert (appdata_full i, uint instanceID : SV_InstanceID, uint vertexId : SV_VertexID )
            {
                                   
                UnitData unitData = _UnitDataBuffer[instanceID];
                unitData.position.y = 0;
        
                float3 worldPosition = unitData.position;
        
                float y = fmod(_Time.y / unitData.animationSpeed, 1);
                float x = (vertexId) * _PositionTex_TexelSize.x;
    
    
                float3 localPosition = tex2Dlod(_PositionTex, float4(x, y, 0, 0));
                localPosition = localPosition - float3(0.5, 0.5, 0.5); 
                localPosition = localPosition * 40;
                        
    
                float3 scale = float3(1, 1, 1);    
                float4x4 object2world = (float4x4) 0;
                object2world._11_22_33_44 = float4(scale.xyz, 1.0);
    
                float rotY = atan2(unitData.velocity.x, unitData.velocity.z);
                float rotX = atan2(unitData.velocity.x, unitData.velocity.y);
                float4x4 rotMatrix = eulerAnglesToRotationMatrix(float3(rotX, rotY, 0));
                object2world = mul(rotMatrix, object2world);                  
      
                localPosition = mul(object2world, localPosition);
                
                worldPosition += localPosition;
    
    
                float4 vPos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1));           
    
                v2f o;
                o. uv_Textures = i.texcoord;
                o.position = vPos;
                o.v_InstanceID = instanceID;
    
                return o;
            }

            fixed4 frag(v2f i, uint instanceID : SV_InstanceID) : SV_Target
            {  
                UnitData boidData = _UnitDataBuffer[i.v_InstanceID]; 
                return UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(i.uv_Textures, UNITY_ACCESS_INSTANCED_PROP( boidData.skin_arr, boidData.skin)));                                                
            }

            ENDCG
        }

    }

}
