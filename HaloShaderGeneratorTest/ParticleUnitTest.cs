using HaloShaderGenerator.DirectX;
using HaloShaderGenerator.Globals;
using HaloShaderGenerator.Particle;
using System.Collections.Generic;

namespace HaloShaderGenerator
{
    public class ParticleUnitTest : GenericUnitTest
    {
        public ParticleUnitTest(string referencePath) : base(referencePath, new ParticleGenerator(), "particle") { }

        public override string GeneratePixelShader(ShaderStage stage, List<int> shaderOptions)
        {
            var albedo = (Albedo)shaderOptions[0];
            var blend_mode = (Blend_Mode)shaderOptions[1];
            var specialized_rendering = (Specialized_Rendering)shaderOptions[2];
            var lighting = (Lighting)shaderOptions[3];
            var render_targets = (Render_Targets)shaderOptions[4];
            var depth_fade = (Depth_Fade)shaderOptions[5];
            var black_point = (Black_Point)shaderOptions[6];
            var fog = (Fog)shaderOptions[7];
            var frame_blend = (Frame_Blend)shaderOptions[8];
            var self_illumination = (Self_Illumination)shaderOptions[9];

            var gen = new ParticleGenerator(albedo, blend_mode, specialized_rendering, lighting, render_targets, depth_fade, black_point, fog, frame_blend, self_illumination);
            var bytecode = gen.GeneratePixelShader(stage).Bytecode;
            return D3DCompiler.Disassemble(bytecode);
        }

        public override string GenerateSharedPixelShader(ShaderStage stage, int methodIndex, int optionIndex)
        {
            var gen = new ParticleGenerator();
            var bytecode = gen.GenerateSharedPixelShader(stage, methodIndex, optionIndex).Bytecode;
            return D3DCompiler.Disassemble(bytecode);
        }

        public override string GenerateVertexShader(VertexType vertex, ShaderStage stage)
        {
            var gen = new ParticleGenerator();
            var bytecode = gen.GenerateVertexShader(vertex, stage).Bytecode;
            return D3DCompiler.Disassemble(bytecode);
        }

        public override string GenerateSharedVertexShader(VertexType vertex, ShaderStage stage)
        {
            var gen = new ParticleGenerator();
            var bytecode = gen.GenerateSharedVertexShader(vertex, stage).Bytecode;
            return D3DCompiler.Disassemble(bytecode);
        }

        public override string GenerateExplicitPixelShader(ExplicitShader explicitShader, ShaderStage stage)
        {
            throw new System.NotImplementedException();
        }

        public override string GenerateExplicitVertexShader(ExplicitShader explicitShader, ShaderStage stage, VertexType vertexType)
        {
            throw new System.NotImplementedException();
        }

        public override string GenerateChudPixelShader(ChudShader chudShader, ShaderStage entry)
        {
            throw new System.NotImplementedException();
        }

        public override string GenerateChudVertexShader(ChudShader chudShader, ShaderStage entry, VertexType vertexType)
        {
            throw new System.NotImplementedException();
        }
    }
}
