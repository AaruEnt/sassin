// Shader created with Shader Forge v1.40 
// Shader Forge (c) Freya Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.40;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,cpap:True,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:4013,x:37480,y:32090,varname:node_4013,prsc:2|diff-8614-OUT,spec-6785-OUT,normal-5866-OUT;n:type:ShaderForge.SFN_FragmentPosition,id:5150,x:31934,y:32633,varname:node_5150,prsc:2;n:type:ShaderForge.SFN_Add,id:354,x:32137,y:32633,varname:node_354,prsc:2|A-5150-XYZ,B-4102-OUT;n:type:ShaderForge.SFN_Slider,id:4102,x:31777,y:32787,ptovrint:False,ptlb:UV_Offset,ptin:_UV_Offset,varname:_UV_Offset,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:10;n:type:ShaderForge.SFN_Multiply,id:7626,x:32324,y:32572,varname:node_7626,prsc:2|A-4985-OUT,B-354-OUT;n:type:ShaderForge.SFN_Slider,id:4985,x:31980,y:32527,ptovrint:False,ptlb:UV_Tile,ptin:_UV_Tile,varname:_UV_Tile,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.08,max:5;n:type:ShaderForge.SFN_ComponentMask,id:6353,x:32537,y:32404,varname:node_6353,prsc:2,cc1:0,cc2:1,cc3:2,cc4:-1|IN-7626-OUT;n:type:ShaderForge.SFN_ComponentMask,id:510,x:32537,y:32571,varname:node_510,prsc:2,cc1:0,cc2:1,cc3:2,cc4:-1|IN-7626-OUT;n:type:ShaderForge.SFN_ComponentMask,id:2000,x:32537,y:32725,varname:node_2000,prsc:2,cc1:0,cc2:1,cc3:2,cc4:-1|IN-7626-OUT;n:type:ShaderForge.SFN_Code,id:4859,x:32722,y:32425,varname:node_4859,prsc:2,code:cgBlAHQAdQByAG4AIABmAGwAbwBhAHQAMgAoAHgALAAgAHkAKQA7AAoA,output:1,fname:Vector2_With_Input_1,width:247,height:112,input:0,input:0,input_1_label:x,input_2_label:y|A-6353-R,B-6353-G;n:type:ShaderForge.SFN_Code,id:9549,x:32722,y:32587,varname:node_9549,prsc:2,code:cgBlAHQAdQByAG4AIABmAGwAbwBhAHQAMgAoAHgALAAgAHkAKQA7AA==,output:1,fname:Vector2_With_Input_2,width:247,height:112,input:0,input:0,input_1_label:x,input_2_label:y|A-510-B,B-510-G;n:type:ShaderForge.SFN_Code,id:6144,x:32722,y:32745,varname:node_6144,prsc:2,code:cgBlAHQAdQByAG4AIABmAGwAbwBhAHQAMgAoAHgALAAgAHkAKQA7AA==,output:1,fname:Vector2_With_Input_3,width:247,height:112,input:0,input:0,input_1_label:x,input_2_label:y|A-2000-R,B-2000-B;n:type:ShaderForge.SFN_Multiply,id:8440,x:34014,y:31764,varname:node_8440,prsc:2|A-5175-RGB,B-1854-OUT;n:type:ShaderForge.SFN_Multiply,id:1813,x:34014,y:31983,varname:node_1813,prsc:2|A-6900-RGB,B-9234-OUT;n:type:ShaderForge.SFN_Multiply,id:2658,x:34014,y:32186,varname:node_2658,prsc:2|A-348-RGB,B-6298-OUT;n:type:ShaderForge.SFN_Add,id:861,x:34212,y:31881,varname:node_861,prsc:2|A-8440-OUT,B-1813-OUT;n:type:ShaderForge.SFN_Add,id:6193,x:34398,y:32171,varname:node_6193,prsc:2|A-861-OUT,B-2658-OUT;n:type:ShaderForge.SFN_Multiply,id:9432,x:34089,y:32633,varname:node_9432,prsc:2|A-335-RGB,B-1854-OUT;n:type:ShaderForge.SFN_Multiply,id:8463,x:34089,y:32793,varname:node_8463,prsc:2|A-2238-RGB,B-9234-OUT;n:type:ShaderForge.SFN_Multiply,id:9354,x:34089,y:33008,varname:node_9354,prsc:2|A-1115-RGB,B-6298-OUT;n:type:ShaderForge.SFN_Add,id:216,x:34284,y:32735,varname:node_216,prsc:2|A-9432-OUT,B-8463-OUT;n:type:ShaderForge.SFN_Add,id:4631,x:34476,y:32986,varname:node_4631,prsc:2|A-216-OUT,B-9354-OUT;n:type:ShaderForge.SFN_NormalVector,id:1933,x:31829,y:34084,prsc:2,pt:False;n:type:ShaderForge.SFN_Dot,id:1113,x:32311,y:33807,varname:node_1113,prsc:2,dt:0|A-4795-OUT,B-1933-OUT;n:type:ShaderForge.SFN_Vector3,id:4795,x:32133,y:33774,varname:node_4795,prsc:2,v1:0,v2:0,v3:1;n:type:ShaderForge.SFN_Dot,id:8653,x:32311,y:34061,varname:node_8653,prsc:2,dt:0|A-1008-OUT,B-1933-OUT;n:type:ShaderForge.SFN_Vector3,id:1008,x:32122,y:34137,varname:node_1008,prsc:2,v1:1,v2:0,v3:0;n:type:ShaderForge.SFN_Dot,id:4067,x:32312,y:34401,varname:node_4067,prsc:2,dt:0|A-4825-OUT,B-1933-OUT;n:type:ShaderForge.SFN_Vector3,id:4825,x:32142,y:34454,varname:node_4825,prsc:2,v1:0,v2:1,v3:0;n:type:ShaderForge.SFN_Smoothstep,id:1854,x:33455,y:33750,varname:node_1854,prsc:2|A-2192-OUT,B-7164-OUT,V-4150-OUT;n:type:ShaderForge.SFN_Smoothstep,id:9234,x:33450,y:34018,varname:node_9234,prsc:2|A-2192-OUT,B-7164-OUT,V-9550-OUT;n:type:ShaderForge.SFN_Smoothstep,id:6298,x:33455,y:34346,varname:node_6298,prsc:2|A-2192-OUT,B-7164-OUT,V-6798-OUT;n:type:ShaderForge.SFN_Slider,id:6481,x:32770,y:34108,ptovrint:False,ptlb:Proj_Mask_Smooth,ptin:_Proj_Mask_Smooth,varname:node_6481,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0.2,cur:0.7,max:0.7;n:type:ShaderForge.SFN_Slider,id:9427,x:32770,y:34011,ptovrint:False,ptlb:Proj_Mask_Threshold,ptin:_Proj_Mask_Threshold,varname:_Proj_Mask_Smooth_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0.6,cur:0.8,max:0.8;n:type:ShaderForge.SFN_Subtract,id:2192,x:33109,y:33938,varname:node_2192,prsc:2|A-9427-OUT,B-6481-OUT;n:type:ShaderForge.SFN_Add,id:7164,x:33109,y:34094,varname:node_7164,prsc:2|A-9427-OUT,B-6481-OUT;n:type:ShaderForge.SFN_Multiply,id:2773,x:35684,y:32481,varname:node_2773,prsc:2|A-5672-OUT,B-4192-OUT;n:type:ShaderForge.SFN_Vector1,id:4192,x:35505,y:32538,varname:node_4192,prsc:2,v1:0.1;n:type:ShaderForge.SFN_Tex2d,id:5175,x:33396,y:31769,varname:node_5175,prsc:2,ntxv:0,isnm:False|UVIN-4859-OUT,TEX-8053-TEX;n:type:ShaderForge.SFN_Tex2d,id:6900,x:33396,y:31979,varname:_Normal_Map_2,prsc:2,ntxv:0,isnm:False|UVIN-9549-OUT,TEX-8053-TEX;n:type:ShaderForge.SFN_Tex2d,id:348,x:33396,y:32178,varname:_Normal_Map_3,prsc:2,ntxv:0,isnm:False|UVIN-6144-OUT,TEX-8053-TEX;n:type:ShaderForge.SFN_Tex2d,id:335,x:33449,y:32627,varname:node_335,prsc:2,ntxv:0,isnm:False|UVIN-4859-OUT,TEX-9860-TEX;n:type:ShaderForge.SFN_Tex2d,id:2238,x:33449,y:32803,varname:_AO_Texture_2,prsc:2,ntxv:0,isnm:False|UVIN-9549-OUT,TEX-9860-TEX;n:type:ShaderForge.SFN_Tex2d,id:1115,x:33449,y:33013,varname:_AO_Texture_3,prsc:2,ntxv:0,isnm:False|UVIN-6144-OUT,TEX-9860-TEX;n:type:ShaderForge.SFN_Tex2dAsset,id:8053,x:33064,y:32008,ptovrint:False,ptlb:Normal_Map,ptin:_Normal_Map,varname:node_8053,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:True,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2dAsset,id:9860,x:33158,y:32830,ptovrint:False,ptlb:AO_Texture,ptin:_AO_Texture,varname:node_9860,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:True,tagnrm:False,ntxv:3,isnm:False;n:type:ShaderForge.SFN_Add,id:5234,x:35871,y:32541,varname:node_5234,prsc:2|A-2773-OUT,B-1354-RGB;n:type:ShaderForge.SFN_VertexColor,id:1354,x:35684,y:32621,varname:node_1354,prsc:2;n:type:ShaderForge.SFN_Smoothstep,id:3995,x:36396,y:32504,varname:node_3995,prsc:2|A-9160-OUT,B-1487-OUT,V-6722-OUT;n:type:ShaderForge.SFN_Smoothstep,id:7772,x:36236,y:32853,varname:node_7772,prsc:2|A-562-OUT,B-6345-OUT,V-6722-OUT;n:type:ShaderForge.SFN_Add,id:6345,x:35866,y:32964,varname:node_6345,prsc:2|A-5724-OUT,B-7093-OUT;n:type:ShaderForge.SFN_Slider,id:1537,x:35364,y:32808,ptovrint:False,ptlb:Snow_Amount,ptin:_Snow_Amount,varname:_Proj_Mask_Threshold_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.05,max:0.1;n:type:ShaderForge.SFN_Subtract,id:562,x:35866,y:32808,varname:node_562,prsc:2|A-5724-OUT,B-7093-OUT;n:type:ShaderForge.SFN_OneMinus,id:5724,x:35684,y:32808,varname:node_5724,prsc:2|IN-1537-OUT;n:type:ShaderForge.SFN_Lerp,id:8614,x:36913,y:32808,varname:node_8614,prsc:2|A-4137-OUT,B-7686-OUT,T-7772-OUT;n:type:ShaderForge.SFN_Vector1,id:9160,x:36221,y:32504,varname:node_9160,prsc:2,v1:0.99;n:type:ShaderForge.SFN_Vector1,id:1487,x:36221,y:32578,varname:node_1487,prsc:2,v1:1.03;n:type:ShaderForge.SFN_Slider,id:7093,x:35364,y:32980,ptovrint:False,ptlb:Snow_Edge_Smooth,ptin:_Snow_Edge_Smooth,varname:node_7093,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0.005,cur:0.08,max:0.15;n:type:ShaderForge.SFN_VertexColor,id:5496,x:35673,y:34117,varname:node_5496,prsc:2;n:type:ShaderForge.SFN_Smoothstep,id:8286,x:35892,y:33975,varname:node_8286,prsc:2|A-488-OUT,B-1230-OUT,V-5496-R;n:type:ShaderForge.SFN_Slider,id:973,x:35300,y:33906,ptovrint:False,ptlb:Rock_Color_Pos,ptin:_Rock_Color_Pos,varname:node_973,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.8,max:1;n:type:ShaderForge.SFN_Slider,id:4426,x:35300,y:34017,ptovrint:False,ptlb:Rock_Color_Smooth,ptin:_Rock_Color_Smooth,varname:node_4426,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.3,max:1;n:type:ShaderForge.SFN_Subtract,id:488,x:35673,y:33834,varname:node_488,prsc:2|A-973-OUT,B-4426-OUT;n:type:ShaderForge.SFN_Add,id:1230,x:35673,y:33973,varname:node_1230,prsc:2|A-973-OUT,B-4426-OUT;n:type:ShaderForge.SFN_Color,id:5676,x:35892,y:33601,ptovrint:False,ptlb:Rock_Bottom_Color,ptin:_Rock_Bottom_Color,varname:node_5676,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0.5215687,c3:0.8392158,c4:1;n:type:ShaderForge.SFN_Color,id:7070,x:35892,y:33782,ptovrint:False,ptlb:Rock_Top_Color,ptin:_Rock_Top_Color,varname:_node_5676_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Lerp,id:5823,x:36148,y:33792,varname:node_5823,prsc:2|A-5676-RGB,B-7070-RGB,T-8286-OUT;n:type:ShaderForge.SFN_Color,id:3547,x:36375,y:32150,ptovrint:False,ptlb:Snow_Color_2,ptin:_Snow_Color_2,varname:_Color_5,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0.7215686,c3:1,c4:1;n:type:ShaderForge.SFN_Vector1,id:1605,x:37278,y:32015,varname:node_1605,prsc:2,v1:1;n:type:ShaderForge.SFN_Lerp,id:7686,x:36582,y:32391,varname:node_7686,prsc:2|A-3547-RGB,B-4819-RGB,T-3995-OUT;n:type:ShaderForge.SFN_Color,id:4819,x:36375,y:32322,ptovrint:False,ptlb:Snow_Color_1,ptin:_Snow_Color_1,varname:_Color_3,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Slider,id:3067,x:34151,y:33220,ptovrint:False,ptlb:AO_Intensity,ptin:_AO_Intensity,varname:node_3067,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.7,max:1;n:type:ShaderForge.SFN_OneMinus,id:9979,x:34487,y:33172,varname:node_9979,prsc:2|IN-3067-OUT;n:type:ShaderForge.SFN_Add,id:4721,x:34673,y:33134,varname:node_4721,prsc:2|A-4631-OUT,B-9979-OUT;n:type:ShaderForge.SFN_Clamp01,id:813,x:34837,y:33134,varname:node_813,prsc:2|IN-4721-OUT;n:type:ShaderForge.SFN_OneMinus,id:6903,x:35003,y:33134,varname:node_6903,prsc:2|IN-813-OUT;n:type:ShaderForge.SFN_Vector1,id:6785,x:37262,y:32129,varname:node_6785,prsc:2,v1:0;n:type:ShaderForge.SFN_Abs,id:4150,x:32482,y:33807,varname:node_4150,prsc:2|IN-1113-OUT;n:type:ShaderForge.SFN_Abs,id:9550,x:32507,y:34061,varname:node_9550,prsc:2|IN-8653-OUT;n:type:ShaderForge.SFN_Abs,id:6798,x:32545,y:34401,varname:node_6798,prsc:2|IN-4067-OUT;n:type:ShaderForge.SFN_Code,id:5866,x:34965,y:31913,varname:node_5866,prsc:2,code:CgBmAGwAbwBhAHQAMwAgAG0AYQBnACAAPQAgAGYAbABvAGEAdAAzACgAMQAqAHMAdAByAGUAbgBnAHQAaAAsADEAKgBzAHQAcgBlAG4AZwB0AGgALAAxACkAOwAKAHIAZQB0AHUAcgBuACAATgAgACoAIABtAGEAZwA7AA==,output:2,fname:NormalMap_Strength_1,width:247,height:126,input:2,input:0,input_1_label:N,input_2_label:strength|A-6193-OUT,B-3279-OUT;n:type:ShaderForge.SFN_Slider,id:3279,x:34542,y:31879,ptovrint:False,ptlb:Normal_Map_Intensity,ptin:_Normal_Map_Intensity,varname:node_3279,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.5,max:1;n:type:ShaderForge.SFN_Code,id:5672,x:34959,y:32171,varname:node_5672,prsc:2,code:CgBmAGwAbwBhAHQAMwAgAG0AYQBnACAAPQAgAGYAbABvAGEAdAAzACgAMQAqAHMAdAByAGUAbgBnAHQAaAAsADEAKgBzAHQAcgBlAG4AZwB0AGgALAAxACkAOwAKAHIAZQB0AHUAcgBuACAATgAgACoAIABtAGEAZwA7AA==,output:2,fname:NormalMap_Strength_2,width:247,height:126,input:2,input:0,input_1_label:N,input_2_label:strength|A-6193-OUT,B-2014-OUT;n:type:ShaderForge.SFN_Vector1,id:2014,x:34761,y:32245,varname:node_2014,prsc:2,v1:1.5;n:type:ShaderForge.SFN_ComponentMask,id:6722,x:36027,y:32534,varname:node_6722,prsc:2,cc1:0,cc2:-1,cc3:-1,cc4:-1|IN-5234-OUT;n:type:ShaderForge.SFN_Multiply,id:4137,x:36393,y:33322,varname:node_4137,prsc:2|A-813-OUT,B-5823-OUT;proporder:7070-5676-973-4426-9860-3067-8053-3279-4819-3547-1537-7093-6481-9427-4985-4102;pass:END;sub:END;*/

Shader "Shader Forge/SnowyRock02" {
    Properties {
        _Rock_Top_Color ("Rock_Top_Color", Color) = (0,1,1,1)
        _Rock_Bottom_Color ("Rock_Bottom_Color", Color) = (0,0.5215687,0.8392158,1)
        _Rock_Color_Pos ("Rock_Color_Pos", Range(0, 1)) = 0.8
        _Rock_Color_Smooth ("Rock_Color_Smooth", Range(0, 1)) = 0.3
        [NoScaleOffset]_AO_Texture ("AO_Texture", 2D) = "bump" {}
        _AO_Intensity ("AO_Intensity", Range(0, 1)) = 0.7
        [NoScaleOffset]_Normal_Map ("Normal_Map", 2D) = "white" {}
        _Normal_Map_Intensity ("Normal_Map_Intensity", Range(0, 1)) = 0.5
        _Snow_Color_1 ("Snow_Color_1", Color) = (1,1,1,1)
        _Snow_Color_2 ("Snow_Color_2", Color) = (0,0.7215686,1,1)
        _Snow_Amount ("Snow_Amount", Range(0, 0.1)) = 0.05
        _Snow_Edge_Smooth ("Snow_Edge_Smooth", Range(0.005, 0.15)) = 0.08
        _Proj_Mask_Smooth ("Proj_Mask_Smooth", Range(0.2, 0.7)) = 0.7
        _Proj_Mask_Threshold ("Proj_Mask_Threshold", Range(0.6, 0.8)) = 0.8
        _UV_Tile ("UV_Tile", Range(0, 5)) = 0.08
        _UV_Offset ("UV_Offset", Range(0, 10)) = 0
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma target 3.0
            uniform float4 _LightColor0;
            float2 Vector2_With_Input_1( float x , float y ){
            return float2(x, y);
            
            }
            
            float2 Vector2_With_Input_2( float x , float y ){
            return float2(x, y);
            }
            
            float2 Vector2_With_Input_3( float x , float y ){
            return float2(x, y);
            }
            
            uniform sampler2D _Normal_Map;
            uniform sampler2D _AO_Texture;
            float3 NormalMap_Strength_1( float3 N , float strength ){
            
            float3 mag = float3(1*strength,1*strength,1);
            return N * mag;
            }
            
            float3 NormalMap_Strength_2( float3 N , float strength ){
            
            float3 mag = float3(1*strength,1*strength,1);
            return N * mag;
            }
            
            UNITY_INSTANCING_BUFFER_START( Props )
                UNITY_DEFINE_INSTANCED_PROP( float, _UV_Offset)
                UNITY_DEFINE_INSTANCED_PROP( float, _UV_Tile)
                UNITY_DEFINE_INSTANCED_PROP( float, _Proj_Mask_Smooth)
                UNITY_DEFINE_INSTANCED_PROP( float, _Proj_Mask_Threshold)
                UNITY_DEFINE_INSTANCED_PROP( float, _Snow_Amount)
                UNITY_DEFINE_INSTANCED_PROP( float, _Snow_Edge_Smooth)
                UNITY_DEFINE_INSTANCED_PROP( float, _Rock_Color_Pos)
                UNITY_DEFINE_INSTANCED_PROP( float, _Rock_Color_Smooth)
                UNITY_DEFINE_INSTANCED_PROP( float4, _Rock_Bottom_Color)
                UNITY_DEFINE_INSTANCED_PROP( float4, _Rock_Top_Color)
                UNITY_DEFINE_INSTANCED_PROP( float4, _Snow_Color_2)
                UNITY_DEFINE_INSTANCED_PROP( float4, _Snow_Color_1)
                UNITY_DEFINE_INSTANCED_PROP( float, _AO_Intensity)
                UNITY_DEFINE_INSTANCED_PROP( float, _Normal_Map_Intensity)
            UNITY_INSTANCING_BUFFER_END( Props )
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 posWorld : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
                float3 tangentDir : TEXCOORD2;
                float3 bitangentDir : TEXCOORD3;
                float4 vertexColor : COLOR;
                LIGHTING_COORDS(4,5)
                UNITY_FOG_COORDS(6)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                UNITY_SETUP_INSTANCE_ID( v );
                UNITY_TRANSFER_INSTANCE_ID( v, o );
                o.vertexColor = v.vertexColor;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                UNITY_SETUP_INSTANCE_ID( i );
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float _UV_Tile_var = UNITY_ACCESS_INSTANCED_PROP( Props, _UV_Tile );
                float _UV_Offset_var = UNITY_ACCESS_INSTANCED_PROP( Props, _UV_Offset );
                float3 node_7626 = (_UV_Tile_var*(i.posWorld.rgb+_UV_Offset_var));
                float3 node_6353 = node_7626.rgb;
                float2 node_4859 = Vector2_With_Input_1( node_6353.r , node_6353.g );
                float4 node_5175 = tex2D(_Normal_Map,node_4859);
                float _Proj_Mask_Threshold_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Proj_Mask_Threshold );
                float _Proj_Mask_Smooth_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Proj_Mask_Smooth );
                float node_2192 = (_Proj_Mask_Threshold_var-_Proj_Mask_Smooth_var);
                float node_7164 = (_Proj_Mask_Threshold_var+_Proj_Mask_Smooth_var);
                float node_1854 = smoothstep( node_2192, node_7164, abs(dot(float3(0,0,1),i.normalDir)) );
                float3 node_510 = node_7626.rgb;
                float2 node_9549 = Vector2_With_Input_2( node_510.b , node_510.g );
                float4 _Normal_Map_2 = tex2D(_Normal_Map,node_9549);
                float node_9234 = smoothstep( node_2192, node_7164, abs(dot(float3(1,0,0),i.normalDir)) );
                float3 node_2000 = node_7626.rgb;
                float2 node_6144 = Vector2_With_Input_3( node_2000.r , node_2000.b );
                float4 _Normal_Map_3 = tex2D(_Normal_Map,node_6144);
                float node_6298 = smoothstep( node_2192, node_7164, abs(dot(float3(0,1,0),i.normalDir)) );
                float3 node_6193 = (((node_5175.rgb*node_1854)+(_Normal_Map_2.rgb*node_9234))+(_Normal_Map_3.rgb*node_6298));
                float _Normal_Map_Intensity_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Normal_Map_Intensity );
                float3 normalLocal = NormalMap_Strength_1( node_6193 , _Normal_Map_Intensity_var );
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
///////// Gloss:
                float gloss = 0.5;
                float specPow = exp2( gloss * 10.0 + 1.0 );
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float node_6785 = 0.0;
                float3 specularColor = float3(node_6785,node_6785,node_6785);
                float3 directSpecular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularColor;
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                float4 node_335 = tex2D(_AO_Texture,node_4859);
                float4 _AO_Texture_2 = tex2D(_AO_Texture,node_9549);
                float4 _AO_Texture_3 = tex2D(_AO_Texture,node_6144);
                float _AO_Intensity_var = UNITY_ACCESS_INSTANCED_PROP( Props, _AO_Intensity );
                float3 node_813 = saturate(((((node_335.rgb*node_1854)+(_AO_Texture_2.rgb*node_9234))+(_AO_Texture_3.rgb*node_6298))+(1.0 - _AO_Intensity_var)));
                float4 _Rock_Bottom_Color_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Rock_Bottom_Color );
                float4 _Rock_Top_Color_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Rock_Top_Color );
                float _Rock_Color_Pos_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Rock_Color_Pos );
                float _Rock_Color_Smooth_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Rock_Color_Smooth );
                float4 _Snow_Color_2_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Snow_Color_2 );
                float4 _Snow_Color_1_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Snow_Color_1 );
                float node_6722 = ((NormalMap_Strength_2( node_6193 , 1.5 )*0.1)+i.vertexColor.rgb).r;
                float _Snow_Amount_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Snow_Amount );
                float node_5724 = (1.0 - _Snow_Amount_var);
                float _Snow_Edge_Smooth_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Snow_Edge_Smooth );
                float3 diffuseColor = lerp((node_813*lerp(_Rock_Bottom_Color_var.rgb,_Rock_Top_Color_var.rgb,smoothstep( (_Rock_Color_Pos_var-_Rock_Color_Smooth_var), (_Rock_Color_Pos_var+_Rock_Color_Smooth_var), i.vertexColor.r ))),lerp(_Snow_Color_2_var.rgb,_Snow_Color_1_var.rgb,smoothstep( 0.99, 1.03, node_6722 )),smoothstep( (node_5724-_Snow_Edge_Smooth_var), (node_5724+_Snow_Edge_Smooth_var), node_6722 ));
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            #pragma target 3.0
            uniform float4 _LightColor0;
            float2 Vector2_With_Input_1( float x , float y ){
            return float2(x, y);
            
            }
            
            float2 Vector2_With_Input_2( float x , float y ){
            return float2(x, y);
            }
            
            float2 Vector2_With_Input_3( float x , float y ){
            return float2(x, y);
            }
            
            uniform sampler2D _Normal_Map;
            uniform sampler2D _AO_Texture;
            float3 NormalMap_Strength_1( float3 N , float strength ){
            
            float3 mag = float3(1*strength,1*strength,1);
            return N * mag;
            }
            
            float3 NormalMap_Strength_2( float3 N , float strength ){
            
            float3 mag = float3(1*strength,1*strength,1);
            return N * mag;
            }
            
            UNITY_INSTANCING_BUFFER_START( Props )
                UNITY_DEFINE_INSTANCED_PROP( float, _UV_Offset)
                UNITY_DEFINE_INSTANCED_PROP( float, _UV_Tile)
                UNITY_DEFINE_INSTANCED_PROP( float, _Proj_Mask_Smooth)
                UNITY_DEFINE_INSTANCED_PROP( float, _Proj_Mask_Threshold)
                UNITY_DEFINE_INSTANCED_PROP( float, _Snow_Amount)
                UNITY_DEFINE_INSTANCED_PROP( float, _Snow_Edge_Smooth)
                UNITY_DEFINE_INSTANCED_PROP( float, _Rock_Color_Pos)
                UNITY_DEFINE_INSTANCED_PROP( float, _Rock_Color_Smooth)
                UNITY_DEFINE_INSTANCED_PROP( float4, _Rock_Bottom_Color)
                UNITY_DEFINE_INSTANCED_PROP( float4, _Rock_Top_Color)
                UNITY_DEFINE_INSTANCED_PROP( float4, _Snow_Color_2)
                UNITY_DEFINE_INSTANCED_PROP( float4, _Snow_Color_1)
                UNITY_DEFINE_INSTANCED_PROP( float, _AO_Intensity)
                UNITY_DEFINE_INSTANCED_PROP( float, _Normal_Map_Intensity)
            UNITY_INSTANCING_BUFFER_END( Props )
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 posWorld : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
                float3 tangentDir : TEXCOORD2;
                float3 bitangentDir : TEXCOORD3;
                float4 vertexColor : COLOR;
                LIGHTING_COORDS(4,5)
                UNITY_FOG_COORDS(6)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                UNITY_SETUP_INSTANCE_ID( v );
                UNITY_TRANSFER_INSTANCE_ID( v, o );
                o.vertexColor = v.vertexColor;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                UNITY_SETUP_INSTANCE_ID( i );
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float _UV_Tile_var = UNITY_ACCESS_INSTANCED_PROP( Props, _UV_Tile );
                float _UV_Offset_var = UNITY_ACCESS_INSTANCED_PROP( Props, _UV_Offset );
                float3 node_7626 = (_UV_Tile_var*(i.posWorld.rgb+_UV_Offset_var));
                float3 node_6353 = node_7626.rgb;
                float2 node_4859 = Vector2_With_Input_1( node_6353.r , node_6353.g );
                float4 node_5175 = tex2D(_Normal_Map,node_4859);
                float _Proj_Mask_Threshold_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Proj_Mask_Threshold );
                float _Proj_Mask_Smooth_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Proj_Mask_Smooth );
                float node_2192 = (_Proj_Mask_Threshold_var-_Proj_Mask_Smooth_var);
                float node_7164 = (_Proj_Mask_Threshold_var+_Proj_Mask_Smooth_var);
                float node_1854 = smoothstep( node_2192, node_7164, abs(dot(float3(0,0,1),i.normalDir)) );
                float3 node_510 = node_7626.rgb;
                float2 node_9549 = Vector2_With_Input_2( node_510.b , node_510.g );
                float4 _Normal_Map_2 = tex2D(_Normal_Map,node_9549);
                float node_9234 = smoothstep( node_2192, node_7164, abs(dot(float3(1,0,0),i.normalDir)) );
                float3 node_2000 = node_7626.rgb;
                float2 node_6144 = Vector2_With_Input_3( node_2000.r , node_2000.b );
                float4 _Normal_Map_3 = tex2D(_Normal_Map,node_6144);
                float node_6298 = smoothstep( node_2192, node_7164, abs(dot(float3(0,1,0),i.normalDir)) );
                float3 node_6193 = (((node_5175.rgb*node_1854)+(_Normal_Map_2.rgb*node_9234))+(_Normal_Map_3.rgb*node_6298));
                float _Normal_Map_Intensity_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Normal_Map_Intensity );
                float3 normalLocal = NormalMap_Strength_1( node_6193 , _Normal_Map_Intensity_var );
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
///////// Gloss:
                float gloss = 0.5;
                float specPow = exp2( gloss * 10.0 + 1.0 );
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float node_6785 = 0.0;
                float3 specularColor = float3(node_6785,node_6785,node_6785);
                float3 directSpecular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularColor;
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float4 node_335 = tex2D(_AO_Texture,node_4859);
                float4 _AO_Texture_2 = tex2D(_AO_Texture,node_9549);
                float4 _AO_Texture_3 = tex2D(_AO_Texture,node_6144);
                float _AO_Intensity_var = UNITY_ACCESS_INSTANCED_PROP( Props, _AO_Intensity );
                float3 node_813 = saturate(((((node_335.rgb*node_1854)+(_AO_Texture_2.rgb*node_9234))+(_AO_Texture_3.rgb*node_6298))+(1.0 - _AO_Intensity_var)));
                float4 _Rock_Bottom_Color_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Rock_Bottom_Color );
                float4 _Rock_Top_Color_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Rock_Top_Color );
                float _Rock_Color_Pos_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Rock_Color_Pos );
                float _Rock_Color_Smooth_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Rock_Color_Smooth );
                float4 _Snow_Color_2_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Snow_Color_2 );
                float4 _Snow_Color_1_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Snow_Color_1 );
                float node_6722 = ((NormalMap_Strength_2( node_6193 , 1.5 )*0.1)+i.vertexColor.rgb).r;
                float _Snow_Amount_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Snow_Amount );
                float node_5724 = (1.0 - _Snow_Amount_var);
                float _Snow_Edge_Smooth_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Snow_Edge_Smooth );
                float3 diffuseColor = lerp((node_813*lerp(_Rock_Bottom_Color_var.rgb,_Rock_Top_Color_var.rgb,smoothstep( (_Rock_Color_Pos_var-_Rock_Color_Smooth_var), (_Rock_Color_Pos_var+_Rock_Color_Smooth_var), i.vertexColor.r ))),lerp(_Snow_Color_2_var.rgb,_Snow_Color_1_var.rgb,smoothstep( 0.99, 1.03, node_6722 )),smoothstep( (node_5724-_Snow_Edge_Smooth_var), (node_5724+_Snow_Edge_Smooth_var), node_6722 ));
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(finalColor * 1,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
