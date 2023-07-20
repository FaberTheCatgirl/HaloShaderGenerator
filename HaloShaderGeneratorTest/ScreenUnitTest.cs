using HaloShaderGenerator.DirectX;
using HaloShaderGenerator.Globals;
using HaloShaderGenerator.Screen;
using System.Collections.Generic;

namespace HaloShaderGenerator
{
    public class ScreenUnitTest : GenericUnitTest
    {
        public ScreenUnitTest(string referencePath) : base(referencePath, new ScreenGenerator(), "screen") { }

        public override string GeneratePixelShader(ShaderStage stage, List<int> shaderOptions)
        {
            var warp = (Warp)shaderOptions[0];
            var _base = (Base)shaderOptions[1];
            var overlay_a = (Overlay_A)shaderOptions[2];
            var overlay_b = (Overlay_B)shaderOptions[3];
            var blend_type = (Blend_Mode)shaderOptions[4];

            var gen = new ScreenGenerator(warp, _base, overlay_a, overlay_b, blend_type);
            var bytecode = gen.GeneratePixelShader(stage).Bytecode;
            return D3DCompiler.Disassemble(bytecode);
        }

        public override string GenerateSharedPixelShader(ShaderStage stage, int methodIndex, int optionIndex)
        {
            var gen = new ScreenGenerator();
            var bytecode = gen.GenerateSharedPixelShader(stage, methodIndex, optionIndex).Bytecode;
            return D3DCompiler.Disassemble(bytecode);
        }

        public override string GenerateVertexShader(VertexType vertex, ShaderStage stage)
        {
            var gen = new ScreenGenerator();
            var bytecode = gen.GenerateVertexShader(vertex, stage).Bytecode;
            return D3DCompiler.Disassemble(bytecode);
        }

        public override string GenerateSharedVertexShader(VertexType vertex, ShaderStage stage)
        {
            var gen = new ScreenGenerator();
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
