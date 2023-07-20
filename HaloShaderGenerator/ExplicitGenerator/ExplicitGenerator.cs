using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HaloShaderGenerator.DirectX;
using HaloShaderGenerator.Generator;
using HaloShaderGenerator.Globals;
using HaloShaderGenerator.Shared;

namespace HaloShaderGenerator
{
    public class ExplicitGenerator
    {
        public ExplicitShader GetExplicitIndex(string explicitShaderName)
        {
            return (ExplicitShader)Enum.Parse(typeof(ExplicitShader), explicitShaderName, true);
        }

        public List<ShaderStage> ScrapeEntryPoints(ExplicitShader explicitShader)
        {
            // get main file as text, scrape for @entry. if none, return default.
            List<ShaderStage> result = new List<ShaderStage>();

            string[] sourceFileLines = ShaderGeneratorBase.GetSourceFile(explicitShader.ToString() + ".hlsl").Split(
                new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < sourceFileLines.Length; i++)
            {
                if (sourceFileLines[i].StartsWith("//@entry"))
                {
                    string line = sourceFileLines[i].Remove(0, 9); // remove "//@entry "

                    ShaderStage entry = (ShaderStage)Enum.Parse(typeof(ShaderStage), line, true);
                    result.Add(entry);
                }
            }

            if (result.Count == 0)
                result.Add(ShaderStage.Default);

            return result;
        }

        public List<VertexType> ScrapeVertexTypes(ExplicitShader explicitShader)
        {
            // get main file as text, scrape for @generate.
            List<VertexType> result = new List<VertexType>();

            string[] sourceFileLines = ShaderGeneratorBase.GetSourceFile(explicitShader.ToString() + ".hlsl").Split(
                new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < sourceFileLines.Length; i++)
            {
                if (sourceFileLines[i].StartsWith("//@generate"))
                {
                    string line = sourceFileLines[i].Remove(0, 12); // remove "//@generate "

                    if (line.StartsWith("s_"))
                    {
                        line = line.Remove(0, 2); // remove "s_"
                        line = line.Remove(line.Length - 7, 7); // remove "_vertex"
                    }

                    line = line.Replace("_", ""); // camel case (with no capitals)

                    switch (line)
                    {
                        case "tinypositiononly":
                            line = "tinyposition";
                            break;
                    }

                    VertexType vertexType = (VertexType)Enum.Parse(typeof(VertexType), line, true);
                    result.Add(vertexType);
                }
            }

            return result;
        }

        public void CreateExplicitMacros(List<D3D.SHADER_MACRO> macros, 
            ShaderStage entryPoint, 
            bool applyFixes, 
            bool vs = false,
            VertexType vertexType = VertexType.World)
        {
            string vertexTypeSnake = string.Concat(vertexType.ToString().Select(
                (x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();

            macros.Add(ShaderGeneratorBase.CreateMacro(vs ? "VERTEX_SHADER" : "PIXEL_SHADER", "1"));
            macros.Add(ShaderGeneratorBase.CreateMacro("entry_point", entryPoint.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateMacro("vertex_type", vertexTypeSnake, "s_", "_vertex"));
            macros.Add(ShaderGeneratorBase.CreateMacro("deform", vertexTypeSnake, "deform_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("SHADER_30", "1"));
            macros.Add(ShaderGeneratorBase.CreateMacro("pc", "1"));
            macros.Add(ShaderGeneratorBase.CreateMacro("DX_VERSION", "9"));
            macros.Add(ShaderGeneratorBase.CreateMacro("disable_register_reorder", "1"));
            macros.Add(ShaderGeneratorBase.CreateMacro("EXPLICIT_COMPILER", "1"));

            if (applyFixes)
                macros.Add(ShaderGeneratorBase.CreateMacro("APPLY_FIXES", "1"));
        }

        public ShaderGeneratorResult GeneratePixelShader(ExplicitShader explicitShader, ShaderStage entryPoint, 
            bool applyFixes = true)
        {
            List<D3D.SHADER_MACRO> macros = new List<D3D.SHADER_MACRO>();

            CreateExplicitMacros(macros, entryPoint, applyFixes, false);

            string entryName = entryPoint.ToString().ToLower() + "_ps";
            string filename = explicitShader.ToString().ToLower() + ".hlsl";
            byte[] bytecode = ShaderGeneratorBase.GenerateSource(filename, macros, entryName, "ps_3_0");

            return new ShaderGeneratorResult(bytecode);
        }

        public ShaderGeneratorResult GenerateVertexShader(ExplicitShader explicitShader, ShaderStage entryPoint, 
            VertexType vertexType, bool applyFixes = true)
        {
            List<D3D.SHADER_MACRO> macros = new List<D3D.SHADER_MACRO>();

            CreateExplicitMacros(macros, entryPoint, applyFixes, true, vertexType);

            string entryName = entryPoint.ToString().ToLower() + "_vs";
            string filename = explicitShader.ToString().ToLower() + ".hlsl";
            byte[] bytecode = ShaderGeneratorBase.GenerateSource(filename, macros, entryName, "vs_3_0");

            return new ShaderGeneratorResult(bytecode);
        }
    }
}
