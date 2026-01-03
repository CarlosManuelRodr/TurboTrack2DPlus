/// This shader solves the problem of visual artifacts on quadrilateral
/// interpolation.
/// For more info: https://www.reedbeta.com/blog/quadrilateral-interpolation-part-1/
Shader "Custom/TiledQuadShader"
{
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _TileX ("Tiling X", Range(1, 10)) = 1
        _TileY ("Tiling Y", Range(1, 10)) = 1
    }
 
    // Source: https://forum.unity.com/threads/correcting-affine-texture-mapping-for-trapezoids.151283/#post-2580356
    SubShader {
     
        pass{
            CGPROGRAM
 
            uniform sampler2D _MainTex;
            float _TileX;
            float _TileY;
         
            #pragma vertex vert          
            #pragma fragment frag
            
            struct vertexInput {
                float4 vertex : POSITION;        
                float4 texcoord  : TEXCOORD0;
                float4 texcoord1  : TEXCOORD1;      
            };
 
            struct vertexOutput {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float2 uv2  : TEXCOORD1;        
            };
 
            vertexOutput vert(vertexInput input)
            {
                vertexOutput output;
                output.pos = UnityObjectToClipPos(input.vertex);
                output.uv = input.texcoord;
                output.uv2  = input.texcoord1;
                return output;
            }
     
            float4 frag(vertexOutput input) : COLOR
            {
                // These are calculated on QuadCollection.cs
                const float2 shiftedPosition = input.uv;
                const float2 width_height = input.uv2;

                // Apply tiling to the texture coordinates
                float2 tiledUV = shiftedPosition / width_height;
                tiledUV.x = tiledUV.x * _TileX % 1.0;
                tiledUV.y = tiledUV.y * _TileY % 1.0;
                
                return tex2D(_MainTex, tiledUV);    
            }
            
            ENDCG
        }
    }
}
