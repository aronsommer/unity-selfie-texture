Shader "Unlit/SpriteWebCamTexture"
{
 Properties 
 {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {} 
    _BlendTex ("Blend (RGB)", 2D) = "white"
 }
 SubShader {
    Tags {"Queue"="Transparent"}
    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha
    Pass {
        SetTexture[_BlendTex] {
            ConstantColor[_Color]
            Combine texture * constant, constant
        }
    }
 }

 Fallback "Transparent/VertexLit"
 }