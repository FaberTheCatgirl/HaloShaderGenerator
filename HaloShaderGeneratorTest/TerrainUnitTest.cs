using HaloShaderGenerator.DirectX;
using HaloShaderGenerator.Globals;
using HaloShaderGenerator.Terrain;
using System.Collections.Generic;

namespace HaloShaderGenerator
{
    public class TerrainUnitTest : GenericUnitTest
    {
        public TerrainUnitTest(string referencePath) : base(referencePath, new TerrainGenerator(), "terrain") { }

        public override string GeneratePixelShader(ShaderStage stage, List<int> shaderOptions)
        {
            var blend_type = (Blending)shaderOptions[0];
            var env_map = (Environment_Mapping)shaderOptions[1];
            var material_0 = (Material)shaderOptions[2];
            var material_1 = (Material1)shaderOptions[3];
            var material_2 = (Material2)shaderOptions[4];
            var material_3 = (Material_No_Detail_Bump)shaderOptions[5];

            var gen = new TerrainGenerator(blend_type, env_map, material_0, material_1, material_2, material_3);
            var bytecode = gen.GeneratePixelShader(stage).Bytecode;
            return D3DCompiler.Disassemble(bytecode);
        }

        public override string GenerateSharedPixelShader(ShaderStage stage, int methodIndex, int optionIndex)
        {
            var gen = new TerrainGenerator();
            var bytecode = gen.GenerateSharedPixelShader(stage, methodIndex, optionIndex).Bytecode;
            return D3DCompiler.Disassemble(bytecode);
        }

        public override string GenerateVertexShader(VertexType vertex, ShaderStage stage)
        {
            var gen = new TerrainGenerator();
            var bytecode = gen.GenerateVertexShader(vertex, stage).Bytecode;
            return D3DCompiler.Disassemble(bytecode);
        }

        public override string GenerateSharedVertexShader(VertexType vertex, ShaderStage stage)
        {
            var gen = new TerrainGenerator();
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
