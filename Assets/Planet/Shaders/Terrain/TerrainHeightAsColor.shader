﻿/*
 * Proland: a procedural landscape rendering library.
 * Copyright (c) 2008-2011 INRIA
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

/*
 * Proland is distributed under a dual-license scheme.
 * You can obtain a specific license from Inria: proland-licensing@inria.fr.
 */

/*
 * Authors: Eric Bruneton, Antoine Begault, Guillaume Piolat.
 * Modified and ported to Unity by Justin Hawkins 2014
 */

Shader "Proland/Terrain/TerrainHeightAsColor" 
{

	Properties
	{
		_MaxHeight("MaxHeight", float) = 5000.0
	}
	SubShader 
	{
		Tags { "Queue" = "Geometry" "RenderType"="" }
		
    	Pass 
    	{
    		//cull front

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			
			#include "Assets/Proland/Shaders/Core/Utility.cginc"
			
			float _MaxHeight;
			
			uniform float2 _Deform_Blending;
			uniform float4x4 _Deform_LocalToWorld;
			uniform float4x4 _Deform_LocalToScreen;
			uniform float4 _Deform_Offset;
			uniform float4 _Deform_Camera;
			uniform float4x4 _Deform_ScreenQuadCorners;
			uniform float4x4 _Deform_ScreenQuadVerticals;
			uniform float _Deform_Radius;
			uniform float4x4 _Deform_TangentFrameToWorld; 
			uniform float4x4 _Deform_TileToTangent;
			
			uniform sampler2D _Elevation_Tile;
			uniform float3 _Elevation_TileSize;
			uniform float3 _Elevation_TileCoords;
			
			uniform float3 _Globals_WorldCameraPos;
			
			uniform float3 _Sun_WorldSunDir;
			
			struct v2f 
			{
    			float4  pos : SV_POSITION;
    			float2  uv : TEXCOORD0;
    			float3 p : TEXCOORD1;
    			float3 q : TEXCOORD2;
			};
			
			// returns content of currently selected tile, at uv coordinates (in [0,1]^2; relatively to this tile)
			float4 texTileLod(sampler2D tile, float2 uv, float3 tileCoords, float3 tileSize) {
			    uv = tileCoords.xy + uv * tileSize.xy;
			    return tex2Dlod(tile, float4(uv,0,0));
			}

			v2f vert(appdata_base v)
			{
			
			    float2 zfc = texTileLod(_Elevation_Tile, v.texcoord.xy, _Elevation_TileCoords, _Elevation_TileSize).xy;
			    
				float2 vert = abs(_Deform_Camera.xy - v.vertex.xy);
			    float d = max(max(vert.x, vert.y), _Deform_Camera.z);
			    float _blend = clamp((d - _Deform_Blending.x) / _Deform_Blending.y, 0.0, 1.0);
			    float h = zfc.x * (1.0 - _blend) + zfc.y * _blend;
			    
			    float4x4 C = _Deform_ScreenQuadCorners;
			    float4x4 N = _Deform_ScreenQuadVerticals;
			
			    float4 uvUV = float4(v.vertex.xy, float2(1.0,1.0) - v.vertex.xy);
			    float4 alpha = uvUV.zxzx * uvUV.wwyy;
			    
			    v2f OUT;

			    OUT.p = float3(v.vertex.xy * _Deform_Offset.z + _Deform_Offset.xy, h);
			    OUT.pos = mul(C + h * N,  alpha);
			    OUT.uv = v.texcoord.xy;
			    
			    float3x3 TTT = _Deform_TileToTangent;
			    OUT.q = float3(mul(TTT, float3(v.vertex.xy, 1.0)).xy, h);
			    
			    return OUT;
			}
			
			float4 texTile(sampler2D tile, float2 uv, float3 tileCoords, float3 tileSize) {
			    uv = tileCoords.xy + uv * tileSize.xy;
			    return tex2D(tile, uv);
			}
			
			float4 frag(v2f IN) : COLOR
			{
			
    			float ht = texTile(_Elevation_Tile, IN.uv, _Elevation_TileCoords, _Elevation_TileSize).x;
    			
				return ht.xxxx/ _MaxHeight;
			}
			
			ENDCG

    	}
	}
}

























