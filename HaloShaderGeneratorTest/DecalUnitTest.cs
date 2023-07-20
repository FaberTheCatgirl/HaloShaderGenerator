using HaloShaderGenerator.DirectX;
using HaloShaderGenerator.Globals;
using HaloShaderGenerator.Decal;
using System.Collections.Generic;

namespace HaloShaderGenerator
{
    public class DecalUnitTest : GenericUnitTest
    {
        public DecalUnitTest(string referencePath) : base(referencePath, new DecalGenerator(), "decal") { }

        public override string GeneratePixelShader(ShaderStage stage, List<int> shaderOptions)
        {
            var albedo = (Albedo)shaderOptions[0];
            var blend_mode = (Blend_Mode)shaderOptions[1];
            var render_pass = (Render_Pass)shaderOptions[2];
            var specular = (Specular)shaderOptions[3];
            var bump_mapping = (Bump_Mapping)shaderOptions[4];
            var tinting = (Tinting)shaderOptions[5];

            var gen = new DecalGenerator(albedo, blend_mode, render_pass, specular, bump_mapping, tinting);
            var bytecode = gen.GeneratePixelShader(stage).Bytecode;
            return D3DCompiler.Disassemble(bytecode);
        }

        public override string GenerateSharedPixelShader(ShaderStage stage, int methodIndex, int optionIndex)
        {
            var gen = new DecalGenerator();
            var bytecode = gen.GenerateSharedPixelShader(stage, methodIndex, optionIndex).Bytecode;
            return D3DCompiler.Disassemble(bytecode);
        }

        public override string GenerateVertexShader(VertexType vertex, ShaderStage stage)
        {
            var gen = new DecalGenerator();
            var bytecode = gen.GenerateVertexShader(vertex, stage).Bytecode;
            return D3DCompiler.Disassemble(bytecode);
        }

        public override string GenerateSharedVertexShader(VertexType vertex, ShaderStage stage)
        {
            var gen = new DecalGenerator();
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
