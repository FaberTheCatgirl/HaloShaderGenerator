using System;
using System.Linq;
using System.Collections.Generic;
using HaloShaderGenerator.DirectX;
using HaloShaderGenerator.Generator;
using HaloShaderGenerator.Globals;
using HaloShaderGenerator.Terrain;

namespace HaloShaderGenerator.Foliage
{
    public class FoliageGenerator : IShaderGenerator
    {
        private bool TemplateGenerationValid;
        private bool ApplyFixes;

        Albedo albedo;
        Alpha_Test alpha_test;
        Material_Model material_model;

        /// <summary>
        /// Generator insantiation for shared shaders. Does not require method options.
        /// </summary>
        public FoliageGenerator(bool applyFixes = false) { TemplateGenerationValid = false; ApplyFixes = applyFixes; }

        /// <summary>
        /// Generator instantiation for method specific shaders.
        /// </summary>
        public FoliageGenerator(Albedo albedo, Alpha_Test alpha_test, Material_Model material_model, bool applyFixes = false)
        {
            this.albedo = albedo;
            this.alpha_test = alpha_test;
            this.material_model = material_model;

            ApplyFixes = applyFixes;
            TemplateGenerationValid = true;
        }

        public FoliageGenerator(byte[] options, bool applyFixes = false)
        {
            options = ValidateOptions(options);

            this.albedo = (Albedo)options[0];
            this.alpha_test = (Alpha_Test)options[1];
            this.material_model = (Material_Model)options[2];

            ApplyFixes = applyFixes;
            TemplateGenerationValid = true;
        }

        public ShaderGeneratorResult GeneratePixelShader(ShaderStage entryPoint)
        {
            if (!TemplateGenerationValid)
                throw new System.Exception("Generator initialized with shared shader constructor. Use template constructor.");

            List<D3D.SHADER_MACRO> macros = new List<D3D.SHADER_MACRO>();

            var sAlphaTest = Enum.Parse(typeof(Shared.Alpha_Test), alpha_test.ToString());

            TemplateGenerator.TemplateGenerator.CreateGlobalMacros(macros, ShaderType.Foliage, entryPoint, Shared.Blend_Mode.Opaque, 
                Shader.Misc.First_Person_Never, (Shared.Alpha_Test)sAlphaTest, ApplyFixes);

            macros.Add(ShaderGeneratorBase.CreateMacro("calc_albedo_ps", albedo, "calc_albedo_", "_ps"));
            macros.Add(ShaderGeneratorBase.CreateMacro("calculate_material", material_model, "calculate_material_"));

            switch (alpha_test)
            {
                case Alpha_Test.None:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_alpha_test_ps", "off", "calc_alpha_test_", "_ps"));
                    break;
                case Alpha_Test.Simple:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_alpha_test_ps", "on", "calc_alpha_test_", "_ps"));
                    break;
                default:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_alpha_test_ps", alpha_test, "calc_alpha_test_", "_ps"));
                    break;
            }

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

            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource($"foliage.fx", macros, entryName, "ps_3_0");

            return new ShaderGeneratorResult(shaderBytecode);
        }

        public ShaderGeneratorResult GenerateSharedPixelShader(ShaderStage entryPoint, int methodIndex, int optionIndex)
        {
            if (!IsEntryPointSupported(entryPoint) || !IsPixelShaderShared(entryPoint))
                return null;

            Alpha_Test alphaTestOption = Alpha_Test.None;

            switch ((FoliageMethods)methodIndex)
            {
                case FoliageMethods.Alpha_Test:
                    alphaTestOption = (Alpha_Test)optionIndex;
                    break;
                default:
                    return null;
            }

            List<D3D.SHADER_MACRO> macros = new List<D3D.SHADER_MACRO>();

            macros.Add(new D3D.SHADER_MACRO { Name = "_DEFINITION_HELPER_HLSLI", Definition = "1" });
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<ShaderStage>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<ShaderType>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Alpha_Test>());

            macros.Add(ShaderGeneratorBase.CreateMacro("calc_alpha_test_ps", alphaTestOption, "calc_alpha_test_", "_ps"));

            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource($"glps_foliage.hlsl", macros, "entry_" + entryPoint.ToString().ToLower(), "ps_3_0");

            return new ShaderGeneratorResult(shaderBytecode);
        }

        public ShaderGeneratorResult GenerateSharedVertexShader(VertexType vertexType, ShaderStage entryPoint)
        {
            return null;
        }

        public ShaderGeneratorResult GenerateVertexShader(VertexType vertexType, ShaderStage entryPoint)
        {
            if (!TemplateGenerationValid)
                throw new Exception("Generator initialized with shared shader constructor. Use template constructor.");
            return null;
        }

        public int GetMethodCount()
        {
            return Enum.GetValues(typeof(FoliageMethods)).Length;
        }

        public int GetMethodOptionCount(int methodIndex)
        {
            switch ((FoliageMethods)methodIndex)
            {
                case FoliageMethods.Albedo:
                    return Enum.GetValues(typeof(Albedo)).Length;
                case FoliageMethods.Alpha_Test:
                    return Enum.GetValues(typeof(Alpha_Test)).Length;
                case FoliageMethods.Material_Model:
                    return Enum.GetValues(typeof(Material_Model)).Length;
            }

            return -1;
        }

        public int GetMethodOptionValue(int methodIndex)
        {
            switch ((FoliageMethods)methodIndex)
            {
                case FoliageMethods.Albedo:
                    return (int)albedo;
                case FoliageMethods.Alpha_Test:
                    return (int)alpha_test;
                case FoliageMethods.Material_Model:
                    return (int)material_model;
            }
            return -1;
        }

        public bool IsEntryPointSupported(ShaderStage entryPoint)
        {
            switch (entryPoint)
            {
                case ShaderStage.Albedo:
                case ShaderStage.Static_Prt_Ambient:
                case ShaderStage.Static_Prt_Linear:
                case ShaderStage.Static_Prt_Quadratic:
                case ShaderStage.Static_Per_Pixel:
                case ShaderStage.Static_Per_Vertex:
                case ShaderStage.Static_Sh:
                case ShaderStage.Shadow_Generate:
                    return true;
                default:
                    return false;
            }
        }

        public bool IsMethodSharedInEntryPoint(ShaderStage entryPoint, int method_index)
        {
            return method_index == 1;
        }

        public bool IsSharedPixelShaderUsingMethods(ShaderStage entryPoint)
        {
            return entryPoint == ShaderStage.Shadow_Generate;
        }

        public bool IsSharedPixelShaderWithoutMethod(ShaderStage entryPoint)
        {
            return false;
        }

        public bool IsPixelShaderShared(ShaderStage entryPoint)
        {
            switch (entryPoint)
            {
                case ShaderStage.Shadow_Generate:
                    return true;
                default:
                    return false;
            }
        }

        public bool IsVertexFormatSupported(VertexType vertexType)
        {
            switch (vertexType)
            {
                case VertexType.World:
                case VertexType.Rigid:
                case VertexType.Skinned:
                    return true;
                default:
                    return false;
            }
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

            List<int> optionIndices = new List<int> { (int)albedo, (int)alpha_test, (int)material_model };
            for (int i = 0; i < Enum.GetValues(typeof(FoliageMethods)).Length; i++)
            {
                string methodName = ((FoliageMethods)i).ToString().ToLower();

                var parameters = GetParametersInOption(methodName, optionIndices[i], out _, out _).Parameters;
                parameters = parameters.Where(x => !x.Flags.HasFlag(ShaderParameterFlags.IsVertexShader)).ToList();

                result.Parameters.AddRange(parameters);
            }

            return result;
        }

        public ShaderParameters GetVertexShaderParameters()
        {
            var result = new ShaderParameters();

            List<int> optionIndices = new List<int> { (int)albedo, (int)alpha_test, (int)material_model };
            for (int i = 0; i < Enum.GetValues(typeof(FoliageMethods)).Length; i++)
            {
                string methodName = ((FoliageMethods)i).ToString().ToLower();

                var parameters = GetParametersInOption(methodName, optionIndices[i], out _, out _).Parameters;
                parameters = parameters.Where(x => x.Flags.HasFlag(ShaderParameterFlags.IsVertexShader)).ToList();

                result.Parameters.AddRange(parameters);
            }

            return result;
        }

        public ShaderParameters GetGlobalParameters()
        {
            var result = new ShaderParameters();
            result.AddSamplerWithoutXFormParameter("albedo_texture", RenderMethodExtern.texture_global_target_texaccum);
            result.AddSamplerWithoutXFormParameter("normal_texture", RenderMethodExtern.texture_global_target_normal);
            result.AddSamplerWithoutXFormParameter("lightprobe_texture_array", RenderMethodExtern.texture_lightprobe_texture);
            result.AddSamplerWithoutXFormParameter("shadow_depth_map_1", RenderMethodExtern.texture_global_target_shadow_buffer1);
            result.AddSamplerWithoutXFormParameter("dynamic_light_gel_texture", RenderMethodExtern.texture_dynamic_light_gel_0);
            result.AddFloat4Parameter("debug_tint", RenderMethodExtern.debug_tint);
            result.AddSamplerWithoutXFormParameter("active_camo_distortion_texture", RenderMethodExtern.active_camo_distortion_texture);
            result.AddSamplerWithoutXFormParameter("scene_ldr_texture", RenderMethodExtern.scene_ldr_texture);
            result.AddSamplerWithoutXFormParameter("scene_hdr_texture", RenderMethodExtern.scene_hdr_texture);
            result.AddSamplerWithoutXFormParameter("dominant_light_intensity_map", RenderMethodExtern.texture_dominant_light_intensity_map);
            return result;
        }

        public ShaderParameters GetParametersInOption(string methodName, int option, out string rmopName, out string optionName)
        {
            ShaderParameters result = new ShaderParameters();
            rmopName = "";
            optionName = "";

            if (methodName == "albedo")
            {
                optionName = ((Albedo)option).ToString();
                switch ((Albedo)option)
                {
                    case Albedo.Default:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerParameter("detail_map");
                        result.AddFloat4Parameter("albedo_color");
                        rmopName = @"shaders\shader_options\albedo_default";
                        break;
                    case Albedo.Simple:
                        result.AddSamplerParameter("base_map");
                        result.AddFloat4Parameter("albedo_color");
                        rmopName = @"shaders\shader_options\albedo_simple";
                        break;
                }
            }
            if (methodName == "alpha_test")
            {
                optionName = ((Alpha_Test)option).ToString();
                switch ((Alpha_Test)option)
                {
                    case Alpha_Test.None:
                    case Alpha_Test.From_Albedo_Alpha:
                        rmopName = @"shaders\shader_options\alpha_test_off";
                        break;
                    case Alpha_Test.Simple:
                        result.AddSamplerParameter("alpha_test_map");
                        rmopName = @"shaders\shader_options\alpha_test_on";
                        break;
                }
            }
            if (methodName == "material_model")
            {
                optionName = ((Material_Model)option).ToString();
                switch ((Material_Model)option)
                {
                    case Material_Model.Default:
                        result.AddFloat4VertexParameter("back_light");
                        result.AddFloat4VertexParameter("g_tree_animation_coeff", RenderMethodExtern.tree_animation_timer);
                        result.AddFloatVertexParameter("animation_amplitude_horizontal");
                        rmopName = @"shaders\foliage_options\material_default";
                        break;
                    //case Material_Model.Flat:
                    //    result.AddFloat4VertexParameter("g_tree_animation_coeff", RenderMethodExtern.tree_animation_timer);
                    //    result.AddFloatParameter("animation_amplitude_horizontal");
                    //    rmopName = @"shaders\foliage_options\material_flat";
                    //    break;
                    //case Material_Model.Specular:
                    //    result.AddFloat4VertexParameter("g_tree_animation_coeff", RenderMethodExtern.tree_animation_timer);
                    //    result.AddFloatVertexParameter("animation_amplitude_horizontal");
                    //    result.AddFloatParameter("foliage_translucency");
                    //    result.AddFloat4Parameter("foliage_specular_color");
                    //    result.AddFloatParameter("foliage_specular_intensity");
                    //    result.AddFloatParameter("foliage_specular_power");
                    //    rmopName = @"shaders\foliage_options\material_specular";
                    //    break;
                    //case Material_Model.Translucent:
                    //    result.AddFloat4VertexParameter("g_tree_animation_coeff", RenderMethodExtern.tree_animation_timer);
                    //    result.AddFloatVertexParameter("animation_amplitude_horizontal");
                    //    result.AddFloatParameter("foliage_translucency");
                    //    rmopName = @"shaders\foliage_options\material_translucent";
                    //    break;
                }
            }

            return result;
        }

        public Array GetMethodNames()
        {
            return Enum.GetValues(typeof(FoliageMethods));
        }

        public Array GetMethodOptionNames(int methodIndex)
        {
            switch ((FoliageMethods)methodIndex)
            {
                case FoliageMethods.Albedo:
                    return Enum.GetValues(typeof(Albedo));
                case FoliageMethods.Alpha_Test:
                    return Enum.GetValues(typeof(Alpha_Test));
                case FoliageMethods.Material_Model:
                    return Enum.GetValues(typeof(Material_Model));
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
