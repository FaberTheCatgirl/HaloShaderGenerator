using HaloShaderGenerator.DirectX;
using HaloShaderGenerator.Globals;
using HaloShaderGenerator.Halogram;
using System.Collections.Generic;

namespace HaloShaderGenerator
{
    public class HalogramUnitTest : GenericUnitTest
    {
        public HalogramUnitTest(string referencePath) : base(referencePath, new HalogramGenerator(), "halogram") { }

        public override string GeneratePixelShader(ShaderStage stage, List<int> shaderOptions)
        {
            var albedo = (Albedo)shaderOptions[0];
            var self_illumination = (Self_Illumination)shaderOptions[1];
            var blend_mode = (Blend_Mode)shaderOptions[2];
            var misc = (Misc)shaderOptions[3];
            var warp = (Warp)shaderOptions[4];
            var overlay = (Overlay)shaderOptions[5];
            var edge_fade = (Edge_Fade)shaderOptions[6];
            var distortion = (Shared.Distortion)shaderOptions[7];
            var soft_fade = (Shared.Soft_Fade)shaderOptions[8];

            var gen = new HalogramGenerator(albedo, self_illumination, blend_mode, misc, warp, overlay, edge_fade, distortion, soft_fade);
            var bytecode = gen.GeneratePixelShader(stage).Bytecode;
            return D3DCompiler.Disassemble(bytecode);
        }

        public override string GenerateSharedPixelShader(ShaderStage stage, int methodIndex, int optionIndex)
        {
            var gen = new HalogramGenerator();
            var bytecode = gen.GenerateSharedPixelShader(stage, methodIndex, optionIndex).Bytecode;
            return D3DCompiler.Disassemble(bytecode);
        }

        public override string GenerateVertexShader(VertexType vertex, ShaderStage stage)
        {
            var gen = new HalogramGenerator();
            var bytecode = gen.GenerateVertexShader(vertex, stage).Bytecode;
            return D3DCompiler.Disassemble(bytecode);
        }

        public override string GenerateSharedVertexShader(VertexType vertex, ShaderStage stage)
        {
            var gen = new HalogramGenerator();
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
