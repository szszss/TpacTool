using System;
using System.Runtime.InteropServices;
using System.Windows;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;

namespace TpacTool
{
	public static class ShaderManager
	{
		public static Shader DefaultShader { get; private set; }

		public static Shader ImageShader { get; private set; }

		public static Shader GridShader { get; private set; }

		public static Shader MeshShader { get; private set; }

		public static void Init()
		{
			try
			{
				DefaultShader = new Shader(nameof(DefaultShader), SOURCE_DEFAULT_VERTEX, SOURCE_DEFAULT_FRAGMENT);
				ImageShader = new Shader(nameof(ImageShader), SOURCE_IMAGE_VERTEX, SOURCE_IMAGE_FRAGMENT);
				GridShader = new Shader(nameof(GridShader), SOURCE_GRID_VERTEX, SOURCE_GRID_FRAGMENT);
				MeshShader = new Shader(nameof(MeshShader), SOURCE_MESH_VERTEX, SOURCE_MESH_FRAGMENT);
			}
			catch (ShaderException e)
			{
				var info = e.IsProgramFailure
					? $"Failed to link {e.Name}\nLog:\n{e.Log}"
					: $"Failed to compile {e.Name}\nLog:\n{e.Log}";
				var title = e.IsProgramFailure ? "Shader Link Error" : "Shader Compilation Error";
				MessageBox.Show(info, title, MessageBoxButton.OK, MessageBoxImage.Error);
				throw;
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public unsafe struct LightBlock
		{
			public Vector4 Ambient;
			public fixed uint Enabled[4];
			public fixed float Position[16];
			public fixed float Color[16];
			public fixed uint IsDirectional[4];
		}

		public class Shader
		{
			private const int BP_LIGHTS = 0;

			private int _programId;

			private int _shaderVertexId;

			private int _shaderFragmentId;

			private int _vpMatrix;

			private int _worldMatrix;

			private int _normalMatrix;

			private int _color;

			private int _backColor;

			private int _colorMask;

			private int _texDiffuse;

			private int _alphaTest;

			private int _alphaTestValue;

			private int _gridLength;

			private int _blockLight;

			public Shader(string name, string vertex, string fragment)
			{
				_shaderVertexId = CreateShader(ShaderType.VertexShader);
				_shaderFragmentId = CreateShader(ShaderType.FragmentShader);
				_programId = CreateProgram();
				int result;

				void CheckShaderCompilation(int shaderId)
				{
					GetShader(shaderId, ShaderParameter.CompileStatus, out result);
					if (result == 0)
					{
						var info = GetShaderInfoLog(shaderId);
						throw new ShaderException(name, info);
					}
				}

				ShaderSource(_shaderVertexId, vertex);
				CompileShader(_shaderVertexId);
				CheckShaderCompilation(_shaderVertexId);

				ShaderSource(_shaderFragmentId, fragment);
				CompileShader(_shaderFragmentId);
				CheckShaderCompilation(_shaderFragmentId);

				AttachShader(_programId, _shaderVertexId);
				AttachShader(_programId, _shaderFragmentId);
				LinkProgram(_programId);
				GetProgram(_programId, GetProgramParameterName.LinkStatus, out result);
				if (result == 0)
				{
					var info = GetProgramInfoLog(_programId);
					throw new ShaderException(name, info, true);
				}

				_vpMatrix = GetUniformLocation(_programId, "vpMatrix");
				_worldMatrix = GetUniformLocation(_programId, "worldMatrix");
				_normalMatrix = GetUniformLocation(_programId, "normalMatrix");
				_color = GetUniformLocation(_programId, "color");
				_backColor = GetUniformLocation(_programId, "backColor");
				_colorMask = GetUniformLocation(_programId, "colorMask");
				_texDiffuse = GetUniformLocation(_programId, "texDiffuse");
				_alphaTest = GetUniformLocation(_programId, "alphaTest");
				_alphaTestValue = GetUniformLocation(_programId, "alphaTestValue");
				_gridLength = GetUniformLocation(_programId, "gridLength");
				_blockLight = GetUniformBlockIndex(_programId, "Lights");

				if (_blockLight >= 0)
					UniformBlockBinding(_programId, _blockLight, BP_LIGHTS);
			}

			public void Use()
			{
				UseProgram(_programId);
			}

			public void SetViewProjectionMatrix(ref Matrix4 vpMatrix)
			{
				if (_vpMatrix >= 0)
					UniformMatrix4(_vpMatrix, false, ref vpMatrix);
			}

			public void SetWorldMatrix(ref Matrix4 worldMatrix)
			{
				if (_worldMatrix >= 0)
					UniformMatrix4(_worldMatrix, false, ref worldMatrix);
			}

			public void SetNormalMatrix(ref Matrix3 normalMatrix)
			{
				if (_normalMatrix >= 0)
					UniformMatrix3(_normalMatrix, false, ref normalMatrix);
			}

			public void SetColor(Vector4 color)
			{
				if (_color >= 0)
					Uniform4(_color,  color);
			}

			public void SetBackColor(Vector4 color)
			{
				if (_backColor >= 0)
					Uniform4(_backColor, color);
			}

			public void SetColorMask(ref Matrix4 colorMask)
			{
				if (_colorMask >= 0)
					UniformMatrix4(_colorMask, false, ref colorMask);
			}

			public void SetTexDiffuse(int texDiffuse)
			{
				if (_texDiffuse >= 0)
					Uniform1(_texDiffuse, (int) texDiffuse);
			}

			public void SetAlphaTest(bool enableAlphaTest, float alphaTestValue = 0.1f)
			{
				if (_alphaTest >= 0)
					Uniform1(_alphaTest, enableAlphaTest ? 1 : 0);
				if (_alphaTestValue >= 0)
					Uniform1(_alphaTestValue, alphaTestValue);
			}

			public void SetGridLength(float gridLength)
			{
				if (_gridLength >= 0)
					Uniform1(_gridLength, gridLength);
			}

			public unsafe void SetLightBlock(int buffer)
			{
				if (_blockLight >= 0)
				{
					BindBufferRange(BufferRangeTarget.UniformBuffer, BP_LIGHTS, buffer, IntPtr.Zero, sizeof(LightBlock));
				}
			}
		}

		private class ShaderException : Exception
		{
			public string Name { get; }
			public string Log { get; }
			public bool IsProgramFailure { get; }

			public ShaderException(string name, string log, bool isProgramFailure = false)
			{
				Name = name;
				Log = log;
				IsProgramFailure = isProgramFailure;	
			}
		}

		private const string SOURCE_DEFAULT_VERTEX = @"
#version 330 core

layout (location = 0) in vec3 position;

uniform mat4 vpMatrix;
uniform mat4 worldMatrix;

void main()
{
	gl_Position = vpMatrix * worldMatrix * vec4(position, 1.0f);
}
";

		private const string SOURCE_DEFAULT_FRAGMENT = @"
#version 330 core

out vec4 outColor;

uniform vec4 color;
uniform vec4 backColor;

void main()
{
    outColor = gl_FrontFacing ? color : backColor;
}
";

		private const string SOURCE_IMAGE_VERTEX = @"
#version 330 core

layout (location = 0) in vec3 position;
layout (location = 2) in vec2 uv;

out vec2 outUv;

uniform mat4 vpMatrix;

void main()
{
	gl_Position = vpMatrix * vec4(position, 1.0f);
	outUv = uv;
}
";

		private const string SOURCE_IMAGE_FRAGMENT = @"
#version 330 core

in vec2 outUv;
out vec4 outColor;

uniform mat4 colorMask;
uniform sampler2D texDiffuse;
uniform bool alphaTest;

void main()
{
    outColor = colorMask * texture(texDiffuse, outUv).rgba;
	if (alphaTest && outColor.a < 0.01f)
	{
		discard;
	}
}
";

		private const string SOURCE_GRID_VERTEX = @"
#version 330 core

layout (location = 0) in vec3 vPosition;
layout (location = 1) in vec3 vNormal;
layout (location = 2) in vec2 vUv;

out vec3 normal;
out vec2 uv;

uniform float gridLength;
uniform mat4 vpMatrix;
uniform mat4 worldMatrix;

void main()
{
	vec3 pos = vPosition;
	pos.z *= gridLength;
	pos += vec3(gl_InstanceID * 1.0f, 0, 0);
	gl_Position = vpMatrix * worldMatrix * vec4(pos, 1.0f);
	uv = vUv;
}
";

		private const string SOURCE_GRID_FRAGMENT = @"
#version 330 core

in vec3 normal;
in vec2 uv;
out vec4 outColor;

uniform vec4 color;
uniform vec4 backColor;

void main()
{
    outColor = gl_FrontFacing ? color : backColor;
}
";

		private const string SOURCE_MESH_VERTEX = @"
#version 330 core

layout (location = 0) in vec3 vPosition;
layout (location = 1) in vec3 vNormal;
layout (location = 2) in vec2 vUv;
layout (location = 3) in vec3 vColor;
layout (location = 4) in vec3 vColor2;
layout (location = 5) in ivec4 boneIndice;
layout (location = 6) in vec4 boneWeights;

out vec3 pos;
out vec3 normal;
out vec2 uv;
out vec3 color;

uniform mat4 vpMatrix;
uniform mat4 worldMatrix;
uniform mat3 normalMatrix;

void main()
{
	vec4 worldPos = worldMatrix * vec4(vPosition, 1.0f);
	gl_Position = vpMatrix * worldPos;
	pos = worldPos.xyz;
	normal = normalMatrix * vNormal;
	uv = vUv;
	color = vColor;
}
";

		private const string SOURCE_MESH_FRAGMENT = @"
#version 330 core

in vec3 pos;
in vec3 normal;
in vec2 uv;
in vec3 color;

out vec4 outColor;

uniform sampler2D texDiffuse;
uniform bool alphaTest;
uniform float alphaTestValue;

layout (std140) uniform Lights
{
	vec4 ambient;
	bvec4 enabled;
	vec4 lightPos[4];
	vec4 lightColor[4];
	bvec4 isDirectional;
};

void main()
{
    outColor = texture(texDiffuse, uv).rgba;
	if (alphaTest && outColor.a < alphaTestValue)
	{
		discard;
	}
	vec3 light = ambient.rgb;
	vec3 norm = normalize(normal);
	if (!gl_FrontFacing)
		norm = -norm;
	for (int i = 0; i < 4; i++)
	{
		if (enabled[i])
		{
			vec3 lightDir = vec3(0, 0, 0);
			if (isDirectional[i])
			{
				lightDir = -lightPos[i].xyz;
			}
			else
			{
				lightDir = normalize(lightPos[i].xyz - pos);
			}
			light += max(dot(norm, lightDir), 0.0) * lightColor[i].rgb;
			//light += lightColor[i].rgb;
		}
	}
	light = min(light, 1.0);
	outColor.rgb *= light;
}
";
	}
}