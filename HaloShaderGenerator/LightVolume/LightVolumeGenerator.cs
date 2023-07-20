using System;
using System.Collections.Generic;
using HaloShaderGenerator.DirectX;
using HaloShaderGenerator.Generator;
using HaloShaderGenerator.Globals;
using HaloShaderGenerator.Particle;

namespace HaloShaderGenerator.LightVolume
{
    public class LightVolumeGenerator : IShaderGenerator
    {
        private bool TemplateGenerationValid;
        private bool ApplyFixes;

        Albedo albedo;
        Blend_Mode blend_mode;
        Fog fog;

        /// <summary>
        /// Generator insantiation for shared shaders. Does not require method options.
        /// </summary>
        public LightVolumeGenerator(bool applyFixes = false) { TemplateGenerationValid = false; ApplyFixes = applyFixes; }

        /// <summary>
        /// Generator instantiation for method specific shaders.
        /// </summary>
        public LightVolumeGenerator(Albedo albedo, Blend_Mode blend_mode, Fog fog, bool applyFixes = false)
        {
            this.albedo = albedo;
            this.blend_mode = blend_mode;
            this.fog = fog;

            ApplyFixes = applyFixes;
            TemplateGenerationValid = true;
        }

        public LightVolumeGenerator(byte[] options, bool applyFixes = false)
        {
            options = ValidateOptions(options);

            this.albedo = (Albedo)options[0];
            this.blend_mode = (Blend_Mode)options[1];
            this.fog = (Fog)options[2];

            ApplyFixes = applyFixes;
            TemplateGenerationValid = true;
        }

        public ShaderGeneratorResult GeneratePixelShader(ShaderStage entryPoint)
        {
            if (!TemplateGenerationValid)
                throw new System.Exception("Generator initialized with shared shader constructor. Use template constructor.");

            List<D3D.SHADER_MACRO> macros = new List<D3D.SHADER_MACRO>();

            Shared.Blend_Mode sBlendMode = (Shared.Blend_Mode)Enum.Parse(typeof(Shared.Blend_Mode), blend_mode.ToString());

            TemplateGenerator.TemplateGenerator.CreateGlobalMacros(macros, ShaderType.LightVolume, entryPoint, sBlendMode, 
                Shader.Misc.First_Person_Never, Shared.Alpha_Test.None, ApplyFixes);

            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Albedo>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Blend_Mode>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Fog>());

            macros.Add(ShaderGeneratorBase.CreateAutoMacro("albedo", albedo.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("blend_mode", blend_mode.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("fog", fog.ToString().ToLower()));

            string entryName = entryPoint.ToString().ToLower() + "_ps";
            switch (entryPoint)
            {
                case ShaderStage.Static_Prt_Linear:
                case ShaderStage.Static_Prt_Quadratic:
                case ShaderStage.Static_Prt_Ambient:
                    entryName = "static_prt_ps";
                    break;
                case ShaderStage.Dynamic_Light_Cinematic:
                    entryName = "dynamic_light_cine_ps";
                    break;
            }

            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource($"light_volume.fx", macros, entryName, "ps_3_0");

            return new ShaderGeneratorResult(shaderBytecode);
        }

        public ShaderGeneratorResult GenerateSharedPixelShader(ShaderStage entryPoint, int methodIndex, int optionIndex)
        {
            if (!IsEntryPointSupported(entryPoint) || !IsPixelShaderShared(entryPoint))
                return null;

            List<D3D.SHADER_MACRO> macros = new List<D3D.SHADER_MACRO>();

            macros.Add(new D3D.SHADER_MACRO { Name = "_DEFINITION_HELPER_HLSLI", Definition = "1" });
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<ShaderStage>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<ShaderType>());

            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource($"glps_light_volume.hlsl", macros, "entry_" + entryPoint.ToString().ToLower(), "ps_3_0");

            return new ShaderGeneratorResult(shaderBytecode);
        }

        public ShaderGeneratorResult GenerateSharedVertexShader(VertexType vertexType, ShaderStage entryPoint)
        {
            if (!IsVertexFormatSupported(vertexType) || !IsEntryPointSupported(entryPoint))
                return null;

            List<D3D.SHADER_MACRO> macros = new List<D3D.SHADER_MACRO>();

            macros.Add(new D3D.SHADER_MACRO { Name = "_DEFINITION_HELPER_HLSLI", Definition = "1" });
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_vertex_transform", vertexType, "calc_vertex_transform_", ""));
            macros.Add(ShaderGeneratorBase.CreateMacro("transform_unknown_vector", vertexType, "transform_unknown_vector_", ""));
            macros.Add(ShaderGeneratorBase.CreateVertexMacro("input_vertex_format", vertexType));

            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource(@"glvs_light_volume.hlsl", macros, $"entry_{entryPoint.ToString().ToLower()}", "vs_3_0");

            return new ShaderGeneratorResult(shaderBytecode);
        }

        public ShaderGeneratorResult GenerateVertexShader(VertexType vertexType, ShaderStage entryPoint)
        {
            if (!TemplateGenerationValid)
                throw new System.Exception("Generator initialized with shared shader constructor. Use template constructor.");
            return null;
        }

        public int GetMethodCount()
        {
            return System.Enum.GetValues(typeof(LightVolumeMethods)).Length;
        }

        public int GetMethodOptionCount(int methodIndex)
        {
            switch ((LightVolumeMethods)methodIndex)
            {
                case LightVolumeMethods.Albedo:
                    return Enum.GetValues(typeof(Albedo)).Length;
                case LightVolumeMethods.Blend_Mode:
                    return Enum.GetValues(typeof(Blend_Mode)).Length;
                case LightVolumeMethods.Fog:
                    return Enum.GetValues(typeof(Fog)).Length;
            }

            return -1;
        }

        public int GetMethodOptionValue(int methodIndex)
        {
            switch ((LightVolumeMethods)methodIndex)
            {
                case LightVolumeMethods.Albedo:
                    return (int)albedo;
                case LightVolumeMethods.Blend_Mode:
                    return (int)blend_mode;
                case LightVolumeMethods.Fog:
                    return (int)fog;
            }
            return -1;
        }

        public bool IsEntryPointSupported(ShaderStage entryPoint)
        {
            return entryPoint == ShaderStage.Default;
        }

        public bool IsMethodSharedInEntryPoint(ShaderStage entryPoint, int method_index)
        {
            return false;
        }

        public bool IsSharedPixelShaderWithoutMethod(ShaderStage entryPoint)
        {
            return false;
        }

        public bool IsPixelShaderShared(ShaderStage entryPoint)
        {
            return false;
        }

        public bool IsVertexFormatSupported(VertexType vertexType)
        {
            return vertexType == VertexType.LightVolume;
        }

        public bool IsVertexShaderShared(ShaderStage entryPoint)
        {
            return true;
        }

        public ShaderParameters GetPixelShaderParameters()
        {
            if (!TemplateGenerationValid)
                return null;
            var result = new ShaderParameters();

            switch (albedo)
            {
                case Albedo.Diffuse_Only:
                    result.AddSamplerWithoutXFormParameter("base_map");
                    break;
                case Albedo.Circular:
                    result.AddFloatParameter("center_offset");
                    result.AddFloatParameter("falloff");
                    break;
            }

            return result;
        }

        public ShaderParameters GetVertexShaderParameters()
        {
            if (!TemplateGenerationValid)
                return null;

            var result = new ShaderParameters();

            result.AddPrefixedFloat4VertexParameter("blend_mode", "category_");
            result.AddPrefixedFloat4VertexParameter("fog", "category_");

            return result;
        }

        public ShaderParameters GetGlobalParameters()
        {
            return new ShaderParameters();
        }

        public bool IsSharedPixelShaderUsingMethods(ShaderStage entryPoint)
        {
            throw new NotImplementedException();
        }

        public ShaderParameters GetParametersInOption(string methodName, int option, out string rmopName, out string optionName)
        {
            ShaderParameters result = new ShaderParameters();
            rmopName = null;
            optionName = null;

            if (methodName == "albedo")
            {
                optionName = ((Albedo)option).ToString();

                switch ((Albedo)option)
                {
                    case Albedo.Diffuse_Only:
                        result.AddSamplerWithoutXFormParameter("base_map");
                        rmopName = @"shaders\light_volume_options\albedo_diffuse_only";
                        break;
                    case Albedo.Circular:
                        result.AddFloatParameter("center_offset");
                        result.AddFloatParameter("falloff");
                        rmopName = @"shaders\light_volume_options\albedo_circular";
                        break;
                }
            }
            if (methodName == "blend_mode")
            {
                optionName = ((Blend_Mode)option).ToString();
            }
            if (methodName == "fog")
            {
                optionName = ((Fog)option).ToString();
            }

            return result;
        }

        public Array GetMethodNames()
        {
            return Enum.GetValues(typeof(LightVolumeMethods));
        }

        public Array GetMethodOptionNames(int methodIndex)
        {
            switch ((LightVolumeMethods)methodIndex)
            {
                case LightVolumeMethods.Albedo:
                    return Enum.GetValues(typeof(Albedo));
                case LightVolumeMethods.Blend_Mode:
                    return Enum.GetValues(typeof(Blend_Mode));
                case LightVolumeMethods.Fog:
                    return Enum.GetValues(typeof(Fog));
            }

            return null;
        }

        public byte[] ValidateOptions(byte[] options)
        {
            List<byte> optionList = new List<byte>(options);

            while (optionList.Count < GetMethodCount())
                optionList.Add(0);

            return optionList.ToArray();
        }
    }
}
