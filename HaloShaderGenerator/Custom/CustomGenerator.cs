using System.Collections.Generic;
using HaloShaderGenerator.DirectX;
using HaloShaderGenerator.Generator;
using HaloShaderGenerator.Globals;
using System;

namespace HaloShaderGenerator.Custom
{
    public class CustomGenerator : IShaderGenerator
    {
        private bool TemplateGenerationValid;
        private bool ApplyFixes;

        Albedo albedo;
        Bump_Mapping bump_mapping;
        Alpha_Test alpha_test;
        Specular_Mask specular_mask;
        Material_Model material_model;
        Environment_Mapping environment_mapping;
        Self_Illumination self_illumination;
        Blend_Mode blend_mode;
        Parallax parallax;
        Misc misc;

        /// <summary>
        /// Generator insantiation for shared shaders. Does not require method options.
        /// </summary>
        public CustomGenerator(bool applyFixes = false) { TemplateGenerationValid = false; ApplyFixes = applyFixes; }

        /// <summary>
        /// Generator instantiation for method specific shaders.
        /// </summary>
        public CustomGenerator(Albedo albedo, Bump_Mapping bump_mapping, Alpha_Test alpha_test, Specular_Mask specular_mask, Material_Model material_model,
            Environment_Mapping environment_mapping, Self_Illumination self_illumination, Blend_Mode blend_mode, Parallax parallax, Misc misc)
        {
            this.albedo = albedo;
            this.bump_mapping = bump_mapping;
            this.alpha_test = alpha_test;
            this.specular_mask = specular_mask;
            this.material_model = material_model;
            this.environment_mapping = environment_mapping;
            this.self_illumination = self_illumination;
            this.blend_mode = blend_mode;
            this.parallax = parallax;
            this.misc = misc;
            TemplateGenerationValid = true;
        }

        public CustomGenerator(byte[] options, bool applyFixes = false)
        {
            options = ValidateOptions(options);

            this.albedo = (Albedo)options[0];
            this.bump_mapping = (Bump_Mapping)options[1];
            this.alpha_test = (Alpha_Test)options[2];
            this.specular_mask = (Specular_Mask)options[3];
            this.material_model = (Material_Model)options[4];
            this.environment_mapping = (Environment_Mapping)options[5];
            this.self_illumination = (Self_Illumination)options[6];
            this.blend_mode = (Blend_Mode)options[7];
            this.parallax = (Parallax)options[8];
            this.misc = (Misc)options[9];

            ApplyFixes = applyFixes;
            TemplateGenerationValid = true;
        }

        public ShaderGeneratorResult GeneratePixelShader(ShaderStage entryPoint)
        {
            if (!TemplateGenerationValid)
                throw new System.Exception("Generator initialized with shared shader constructor. Use template constructor.");

            List<D3D.SHADER_MACRO> macros = new List<D3D.SHADER_MACRO>();

            //
            // Convert to shared enum
            //

            var sAlbedo = Enum.Parse(typeof(Shared.Albedo), albedo.ToString());
            var sAlphaTest = Enum.Parse(typeof(Shared.Alpha_Test), alpha_test.ToString());
            var sSelfIllumination = Enum.Parse(typeof(Shared.Self_Illumination), self_illumination.ToString());
            var sBlendMode = Enum.Parse(typeof(Shared.Blend_Mode), blend_mode.ToString());

            TemplateGenerator.TemplateGenerator.CreateGlobalMacros(macros, 
                ShaderType.Custom, 
                entryPoint, 
                (Shared.Blend_Mode)sBlendMode, 
                (Shader.Misc)misc,
                (Shared.Alpha_Test)sAlphaTest,
                ApplyFixes);
            
            //
            // The following code properly names the macros (like in rmdf)
            //

            macros.Add(ShaderGeneratorBase.CreateMacro("calc_albedo_ps", sAlbedo, "calc_albedo_", "_ps"));
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_albedo_vs", sAlbedo, "calc_albedo_", "_vs"));

            if (bump_mapping == Bump_Mapping.Standard)
            {
                macros.Add(ShaderGeneratorBase.CreateMacro("calc_bumpmap_ps", "default", "calc_bumpmap_", "_ps"));
                macros.Add(ShaderGeneratorBase.CreateMacro("calc_bumpmap_vs", "default", "calc_bumpmap_", "_vs"));
            }
            else
            {
                macros.Add(ShaderGeneratorBase.CreateMacro("calc_bumpmap_ps", bump_mapping, "calc_bumpmap_", "_ps"));
                macros.Add(ShaderGeneratorBase.CreateMacro("calc_bumpmap_vs", bump_mapping, "calc_bumpmap_", "_vs"));
            }

            switch (sAlphaTest)
            {
                case Shared.Alpha_Test.None:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_alpha_test_ps", "off", "calc_alpha_test_", "_ps"));
                    break;
                case Shared.Alpha_Test.Simple:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_alpha_test_ps", "on", "calc_alpha_test_", "_ps"));
                    break;
                default:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_alpha_test_ps", sAlphaTest, "calc_alpha_test_", "_ps"));
                    break;
            }

            switch (specular_mask)
            {
                case Specular_Mask.No_Specular_Mask:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_specular_mask_ps", specular_mask, "calc_specular_mask_", "_ps"));
                    break;
                case Specular_Mask.Specular_Mask_From_Color_Texture:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_specular_mask_ps", "color_texture", "calc_specular_mask_", "_ps"));
                    break;
                case Specular_Mask.Specular_Mask_From_Texture:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_specular_mask_ps", "texture", "calc_specular_mask_", "_ps"));
                    break;
                default: // name hack
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_specular_mask_ps", specular_mask, "calc_", "_ps"));
                    break;
            }

            //macros.Add(ShaderGeneratorBase.CreateMacro("calc_self_illumination_ps", sSelfIllumination, "calc_self_illumination_", "_ps"));
            switch (sSelfIllumination)
            {
                case Shared.Self_Illumination.Off:
                case Shared.Self_Illumination.None:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_self_illumination_ps", "none", "calc_self_illumination_", "_ps"));
                    break;
                case Shared.Self_Illumination._3_Channel_Self_Illum:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_self_illumination_ps", "three_channel", "calc_self_illumination_", "_ps"));
                    break;
                case Shared.Self_Illumination.From_Diffuse:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_self_illumination_ps", "from_albedo", "calc_self_illumination_", "_ps"));
                    break;
                case Shared.Self_Illumination.Illum_Detail:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_self_illumination_ps", "detail", "calc_self_illumination_", "_ps"));
                    break;
                case Shared.Self_Illumination.Self_Illum_Times_Diffuse:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_self_illumination_ps", "times_diffuse", "calc_self_illumination_", "_ps"));
                    break;
                case Shared.Self_Illumination.Simple_Four_Change_Color:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_self_illumination_ps", "simple", "calc_self_illumination_", "_ps"));
                    break;
                default:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_self_illumination_ps", sSelfIllumination, "calc_self_illumination_", "_ps"));
                    break;
            }

            macros.Add(ShaderGeneratorBase.CreateMacro("calc_parallax_ps", parallax, "calc_parallax_", "_ps"));

            macros.Add(ShaderGeneratorBase.CreateMacro("material_type", material_model));
            macros.Add(ShaderGeneratorBase.CreateMacro("envmap_type", environment_mapping));
            macros.Add(ShaderGeneratorBase.CreateMacro("blend_type", blend_mode));

            switch (parallax)
            {
                case Parallax.Simple_Detail:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_parallax_vs", Parallax.Simple, "calc_parallax_", "_vs"));
                    break;
                default:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_parallax_vs", parallax, "calc_parallax_", "_vs"));
                    break;
            }

            string entryName = entryPoint.ToString().ToLower() + "_ps";
            switch (entryPoint)
            {
                case ShaderStage.Static_Prt_Linear:
                case ShaderStage.Static_Prt_Quadratic:
                case ShaderStage.Static_Prt_Ambient:
                    //case ShaderStage.Static_Sh:
                    entryName = "static_prt_ps";
                    break;
                case ShaderStage.Dynamic_Light_Cinematic:
                    entryName = "dynamic_light_cine_ps";
                    break;

            }

            macros.Add(ShaderGeneratorBase.CreateMacro("bitmap_rotation", misc == Misc.First_Person_Never_With_rotating_Bitmaps ? "1" : "0"));

            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource($"custom.fx", macros, entryName, "ps_3_0");

            return new ShaderGeneratorResult(shaderBytecode);
        }

        public ShaderGeneratorResult GenerateSharedPixelShader(ShaderStage entryPoint, int methodIndex, int optionIndex)
        {
            if (!IsEntryPointSupported(entryPoint) || !IsPixelShaderShared(entryPoint))
                return null;

            Shared.Alpha_Test alphaTestOption;

            switch ((CustomMethods)methodIndex)
            {
                case CustomMethods.Alpha_Test:
                    alphaTestOption = (Shared.Alpha_Test)optionIndex;
                    break;
                default:
                    return null;
            }

            List<D3D.SHADER_MACRO> macros = new List<D3D.SHADER_MACRO>();

            macros.Add(new D3D.SHADER_MACRO { Name = "_DEFINITION_HELPER_HLSLI", Definition = "1" });
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<ShaderStage>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Globals.ShaderType>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Shared.Alpha_Test>());

            macros.Add(ShaderGeneratorBase.CreateMacro("calc_alpha_test_ps", alphaTestOption, "calc_alpha_test_", "_ps"));

            macros.Add(ShaderGeneratorBase.CreateMacro("shaderstage", entryPoint, "k_shaderstage_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("shadertype", Shared.ShaderType.Custom, "shadertype_"));

            // reuse rmsh glps
            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource($"glps_shader.hlsl", macros, "entry_" + entryPoint.ToString().ToLower(), "ps_3_0");

            return new ShaderGeneratorResult(shaderBytecode);
        }

        public ShaderGeneratorResult GenerateSharedVertexShader(VertexType vertexType, ShaderStage entryPoint)
        {
            if (!IsVertexFormatSupported(vertexType) || !IsEntryPointSupported(entryPoint))
                return null;

            List<D3D.SHADER_MACRO> macros = new List<D3D.SHADER_MACRO>();

            macros.Add(new D3D.SHADER_MACRO { Name = "_VERTEX_SHADER_HELPER_HLSLI", Definition = "1" });
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<ShaderStage>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<VertexType>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Shared.ShaderType>());
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_vertex_transform", vertexType, "calc_vertex_transform_", ""));
            macros.Add(ShaderGeneratorBase.CreateMacro("transform_dominant_light", vertexType, "transform_dominant_light_", ""));
            macros.Add(ShaderGeneratorBase.CreateVertexMacro("input_vertex_format", vertexType));

            macros.Add(ShaderGeneratorBase.CreateMacro("shaderstage", entryPoint, "k_shaderstage_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("vertextype", vertexType, "k_vertextype_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("shadertype", Shared.ShaderType.Custom, "shadertype_"));

            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource(@"glvs_custom.hlsl", macros, $"entry_{entryPoint.ToString().ToLower()}", "vs_3_0");

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
            return Enum.GetValues(typeof(CustomMethods)).Length;
        }

        public int GetMethodOptionCount(int methodIndex)
        {
            switch ((CustomMethods)methodIndex)
            {
                case CustomMethods.Albedo:
                    return Enum.GetValues(typeof(Albedo)).Length;
                case CustomMethods.Bump_Mapping:
                    return Enum.GetValues(typeof(Bump_Mapping)).Length;
                case CustomMethods.Alpha_Test:
                    return Enum.GetValues(typeof(Alpha_Test)).Length;
                case CustomMethods.Specular_Mask:
                    return Enum.GetValues(typeof(Specular_Mask)).Length;
                case CustomMethods.Material_Model:
                    return Enum.GetValues(typeof(Material_Model)).Length;
                case CustomMethods.Environment_Mapping:
                    return Enum.GetValues(typeof(Environment_Mapping)).Length;
                case CustomMethods.Self_Illumination:
                    return Enum.GetValues(typeof(Self_Illumination)).Length;
                case CustomMethods.Blend_Mode:
                    return Enum.GetValues(typeof(Blend_Mode)).Length;
                case CustomMethods.Parallax:
                    return Enum.GetValues(typeof(Parallax)).Length;
                case CustomMethods.Misc:
                    return Enum.GetValues(typeof(Misc)).Length;
            }
            return -1;
        }

        public int GetMethodOptionValue(int methodIndex)
        {
            switch ((CustomMethods)methodIndex)
            {
                case CustomMethods.Albedo:
                    return (int)albedo;
                case CustomMethods.Bump_Mapping:
                    return (int)bump_mapping;
                case CustomMethods.Alpha_Test:
                    return (int)alpha_test;
                case CustomMethods.Specular_Mask:
                    return (int)specular_mask;
                case CustomMethods.Material_Model:
                    return (int)material_model;
                case CustomMethods.Environment_Mapping:
                    return (int)environment_mapping;
                case CustomMethods.Self_Illumination:
                    return (int)self_illumination;
                case CustomMethods.Blend_Mode:
                    return (int)blend_mode;
                case CustomMethods.Parallax:
                    return (int)parallax;
                case CustomMethods.Misc:
                    return (int)misc;
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
                case ShaderStage.Static_Per_Vertex_Color:
                case ShaderStage.Dynamic_Light:
                case ShaderStage.Dynamic_Light_Cinematic:
                case ShaderStage.Static_Sh:
                case ShaderStage.Shadow_Generate:
                    return true;

                default:
                case ShaderStage.Default:
                case ShaderStage.Z_Only:
                case ShaderStage.Water_Shading:
                case ShaderStage.Water_Tessellation:
                case ShaderStage.Shadow_Apply:
                case ShaderStage.Static_Default:
                case ShaderStage.Lightmap_Debug_Mode:
                case ShaderStage.Active_Camo:
                case ShaderStage.Sfx_Distort:
                    return false;
            }
        }

        public bool IsMethodSharedInEntryPoint(ShaderStage entryPoint, int method_index)
        {
            return method_index == 2;
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

            switch (albedo)
            {
                case Albedo.Default:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("detail_map");
                    result.AddFloat4Parameter("albedo_color");
                    break;
                case Albedo.Detail_Blend:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("detail_map");
                    result.AddSamplerParameter("detail_map2");
                    break;
                case Albedo.Constant_Color:
                    result.AddFloat4Parameter("albedo_color");
                    break;
                case Albedo.Two_Change_Color:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("detail_map");
                    result.AddSamplerParameter("change_color_map");
                    result.AddFloat4Parameter("primary_change_color", RenderMethodExtern.object_change_color_primary);
                    result.AddFloat4Parameter("secondary_change_color", RenderMethodExtern.object_change_color_secondary);
                    break;
                case Albedo.Four_Change_Color:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("detail_map");
                    result.AddSamplerParameter("change_color_map");
                    result.AddFloat4Parameter("primary_change_color", RenderMethodExtern.object_change_color_primary);
                    result.AddFloat4Parameter("secondary_change_color", RenderMethodExtern.object_change_color_secondary);
                    result.AddFloat4Parameter("tertiary_change_color", RenderMethodExtern.object_change_color_tertiary);
                    result.AddFloat4Parameter("quaternary_change_color", RenderMethodExtern.object_change_color_quaternary);
                    break;
                case Albedo.Three_Detail_Blend:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("detail_map");
                    result.AddSamplerParameter("detail_map2");
                    result.AddSamplerParameter("detail_map3");
                    break;
                case Albedo.Two_Detail_Overlay:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("detail_map");
                    result.AddSamplerParameter("detail_map2");
                    result.AddSamplerParameter("detail_map_overlay");
                    break;
                case Albedo.Two_Detail:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("detail_map");
                    result.AddSamplerParameter("detail_map2");
                    break;
                case Albedo.Color_Mask:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("detail_map");
                    result.AddSamplerParameter("color_mask_map");
                    result.AddFloat4Parameter("albedo_color");
                    result.AddFloat4Parameter("albedo_color2");
                    result.AddFloat4Parameter("albedo_color3");
                    result.AddFloat4Parameter("neutral_gray");
                    break;
                case Albedo.Two_Detail_Black_Point:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("detail_map");
                    result.AddSamplerParameter("detail_map2");
                    break;
                case Albedo.Waterfall:
                    result.AddSamplerParameter("waterfall_base_mask");
                    result.AddSamplerParameter("waterfall_layer0");
                    result.AddSamplerParameter("waterfall_layer1");
                    result.AddSamplerParameter("waterfall_layer2");
                    result.AddFloatParameter("transparency_frothy_weight");
                    result.AddFloatParameter("transparency_base_weight");
                    result.AddFloatParameter("transparency_bias");
                    break;
            }
            switch (bump_mapping)
            {
                case Bump_Mapping.Standard:
                    result.AddSamplerParameter("bump_map");
                    break;
                case Bump_Mapping.Detail:
                    result.AddSamplerParameter("bump_map");
                    result.AddSamplerParameter("bump_detail_map");
                    result.AddFloatParameter("bump_detail_coefficient");
                    break;
            }
            switch (alpha_test)
            {
                case Alpha_Test.Simple:
                    result.AddSamplerParameter("alpha_test_map");
                    break;
                case Alpha_Test.Multiply_Map:
                    result.AddSamplerParameter("alpha_test_map");
                    result.AddSamplerParameter("multiply_map");
                    break;
            }
            switch (specular_mask)
            {
                case Specular_Mask.Specular_Mask_From_Texture:
                case Specular_Mask.Specular_Mask_From_Color_Texture:
                    result.AddSamplerParameter("specular_mask_texture");
                    break;
            }
            switch (material_model)
            {
                case Material_Model.Two_Lobe_Phong:
                    result.AddFloatParameter("diffuse_coefficient");
                    result.AddFloatParameter("specular_coefficient");
                    result.AddFloatParameter("normal_specular_power");
                    result.AddFloat4Parameter("normal_specular_tint");
                    result.AddFloatParameter("glancing_specular_power");
                    result.AddFloat4Parameter("glancing_specular_tint");
                    result.AddFloatParameter("fresnel_curve_steepness");
                    result.AddFloatParameter("area_specular_contribution");
                    result.AddFloatParameter("analytical_specular_contribution");
                    result.AddFloatParameter("environment_map_specular_contribution");
                    result.AddBooleanParameter("order3_area_specular");
                    result.AddBooleanParameter("no_dynamic_lights");
                    result.AddFloatParameter("albedo_specular_tint_blend");
                    result.AddFloatParameter("analytical_anti_shadow_control");
                    break;
                case Material_Model.Custom_Specular:
                    result.AddFloatParameter("diffuse_coefficient");
                    result.AddFloatParameter("specular_coefficient");
                    result.AddFloatParameter("environment_map_specular_contribution");
                    result.AddFloatParameter("area_specular_contribution");
                    result.AddFloatParameter("analytical_specular_contribution");
                    result.AddSamplerWithoutXFormParameter("specular_lobe");
                    result.AddSamplerWithoutXFormParameter("glancing_falloff");
                    result.AddSamplerParameter("material_map");
                    break;
            }
            switch (environment_mapping)
            {
                case Environment_Mapping.Per_Pixel:
                case Environment_Mapping.Per_Pixel_Mip:
                    result.AddSamplerWithoutXFormParameter("environment_map");
                    result.AddFloat4Parameter("env_tint_color");
                    result.AddFloatParameter("env_roughness_scale");
                    break;
                case Environment_Mapping.Dynamic:
                case Environment_Mapping.Dynamic_Reach:
                    result.AddFloat4Parameter("env_tint_color");
                    result.AddSamplerWithoutXFormParameter("dynamic_environment_map_0", RenderMethodExtern.texture_dynamic_environment_map_0);
                    result.AddSamplerWithoutXFormParameter("dynamic_environment_map_1", RenderMethodExtern.texture_dynamic_environment_map_1);
                    result.AddFloatParameter("env_roughness_scale");
                    break;
                case Environment_Mapping.From_Flat_Texture:
                    result.AddSamplerWithoutXFormParameter("flat_environment_map");
                    result.AddFloat4Parameter("env_tint_color");
                    result.AddFloat4Parameter("flat_envmap_matrix_x", RenderMethodExtern.flat_envmap_matrix_x);
                    result.AddFloat4Parameter("flat_envmap_matrix_y", RenderMethodExtern.flat_envmap_matrix_y);
                    result.AddFloat4Parameter("flat_envmap_matrix_z", RenderMethodExtern.flat_envmap_matrix_z);
                    result.AddFloatParameter("hemisphere_percentage");
                    result.AddFloat4Parameter("env_bloom_override");
                    result.AddFloatParameter("env_bloom_override_intensity");
                    break;
            }
            switch (self_illumination)
            {
                case Self_Illumination.Simple:
                    result.AddSamplerParameter("self_illum_map");
                    result.AddFloat4Parameter("self_illum_color");
                    result.AddFloatParameter("self_illum_intensity");
                    break;
                case Self_Illumination._3_Channel_Self_Illum:
                    result.AddSamplerParameter("self_illum_map");
                    result.AddFloat4Parameter("channel_a");
                    result.AddFloat4Parameter("channel_b");
                    result.AddFloat4Parameter("channel_c");
                    result.AddFloatParameter("self_illum_intensity");
                    break;
                case Self_Illumination.Plasma:
                    result.AddSamplerParameter("noise_map_a");
                    result.AddSamplerParameter("noise_map_b");
                    result.AddFloat4Parameter("color_medium");
                    result.AddFloat4Parameter("color_wide");
                    result.AddFloat4Parameter("color_sharp");
                    result.AddFloatParameter("self_illum_intensity");
                    result.AddSamplerParameter("alpha_mask_map");
                    result.AddFloatParameter("thinness_medium");
                    result.AddFloatParameter("thinness_wide");
                    result.AddFloatParameter("thinness_sharp");
                    break;
                case Self_Illumination.From_Diffuse:
                    result.AddFloat4Parameter("self_illum_color");
                    result.AddFloatParameter("self_illum_intensity");
                    break;
                case Self_Illumination.Illum_Detail:
                    result.AddSamplerParameter("self_illum_map");
                    result.AddSamplerParameter("self_illum_detail_map");
                    result.AddFloat4Parameter("self_illum_color");
                    result.AddFloatParameter("self_illum_intensity");
                    break;
                case Self_Illumination.Meter:
                    result.AddSamplerParameter("meter_map");
                    result.AddFloat4Parameter("meter_color_off");
                    result.AddFloat4Parameter("meter_color_on");
                    result.AddFloatParameter("meter_value");
                    break;
                case Self_Illumination.Self_Illum_Times_Diffuse:
                    result.AddSamplerParameter("self_illum_map");
                    result.AddFloat4Parameter("self_illum_color");
                    result.AddFloatParameter("self_illum_intensity");
                    result.AddFloatParameter("primary_change_color_blend");
                    break;
                case Self_Illumination.Window_Room:
                    result.AddSamplerParameter("opacity_map");
                    result.AddSamplerParameter("ceiling");
                    result.AddSamplerParameter("floors");
                    result.AddSamplerParameter("walls");
                    result.AddXFormOnlyParameter("transform");
                    result.AddFloatParameter("distance_fade_scale");
                    result.AddFloatParameter("self_illum_intensity");
                    break;
            }
            switch (parallax)
            {
                case Parallax.Simple:
                case Parallax.Interpolated:
                    result.AddSamplerParameter("height_map");
                    result.AddFloatParameter("height_scale");
                    break;
                case Parallax.Simple_Detail:
                    result.AddSamplerParameter("height_map");
                    result.AddFloatParameter("height_scale");
                    result.AddSamplerParameter("height_scale_map");
                    break;
            }

            return result;
        }

        public ShaderParameters GetVertexShaderParameters()
        {
            return new ShaderParameters();
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
            rmopName = null;
            optionName = null;

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
                    case Albedo.Detail_Blend:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerParameter("detail_map");
                        result.AddSamplerParameter("detail_map2");
                        rmopName = @"shaders\shader_options\albedo_detail_blend";
                        break;
                    case Albedo.Constant_Color:
                        result.AddFloat4Parameter("albedo_color");
                        rmopName = @"shaders\shader_options\albedo_constant";
                        break;
                    case Albedo.Two_Change_Color:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerParameter("detail_map");
                        result.AddSamplerParameter("change_color_map");
                        result.AddFloat4Parameter("primary_change_color", RenderMethodExtern.object_change_color_primary);
                        result.AddFloat4Parameter("secondary_change_color", RenderMethodExtern.object_change_color_secondary);
                        rmopName = @"shaders\shader_options\albedo_two_change_color";
                        break;
                    case Albedo.Four_Change_Color:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerParameter("detail_map");
                        result.AddSamplerParameter("change_color_map");
                        result.AddFloat4Parameter("primary_change_color", RenderMethodExtern.object_change_color_primary);
                        result.AddFloat4Parameter("secondary_change_color", RenderMethodExtern.object_change_color_secondary);
                        result.AddFloat4Parameter("tertiary_change_color", RenderMethodExtern.object_change_color_tertiary);
                        result.AddFloat4Parameter("quaternary_change_color", RenderMethodExtern.object_change_color_quaternary);
                        rmopName = @"shaders\shader_options\albedo_four_change_color";
                        break;
                    case Albedo.Three_Detail_Blend:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerParameter("detail_map");
                        result.AddSamplerParameter("detail_map2");
                        result.AddSamplerParameter("detail_map3");
                        rmopName = @"shaders\shader_options\albedo_three_detail_blend";
                        break;
                    case Albedo.Two_Detail_Overlay:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerParameter("detail_map");
                        result.AddSamplerParameter("detail_map2");
                        result.AddSamplerParameter("detail_map_overlay");
                        rmopName = @"shaders\shader_options\albedo_two_detail_overlay";
                        break;
                    case Albedo.Two_Detail:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerParameter("detail_map");
                        result.AddSamplerParameter("detail_map2");
                        rmopName = @"shaders\shader_options\albedo_two_detail";
                        break;
                    case Albedo.Color_Mask:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerParameter("detail_map");
                        result.AddSamplerParameter("color_mask_map");
                        result.AddFloat4Parameter("albedo_color");
                        result.AddFloat4Parameter("albedo_color2");
                        result.AddFloat4Parameter("albedo_color3");
                        result.AddFloat4Parameter("neutral_gray");
                        rmopName = @"shaders\shader_options\albedo_color_mask";
                        break;
                    case Albedo.Two_Detail_Black_Point:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerParameter("detail_map");
                        result.AddSamplerParameter("detail_map2");
                        rmopName = @"shaders\shader_options\albedo_two_detail_black_point";
                        break;
                    case Albedo.Waterfall:
                        result.AddSamplerParameter("waterfall_base_mask");
                        result.AddSamplerParameter("waterfall_layer0");
                        result.AddSamplerParameter("waterfall_layer1");
                        result.AddSamplerParameter("waterfall_layer2");
                        result.AddFloatParameter("transparency_frothy_weight");
                        result.AddFloatParameter("transparency_base_weight");
                        result.AddFloatParameter("transparency_bias");
                        rmopName = @"shaders\custom_options\albedo_waterfall";
                        break;
                }
            }
            if (methodName == "bump_mapping")
            {
                optionName = ((Bump_Mapping)option).ToString();
                switch ((Bump_Mapping)option)
                {
                    case Bump_Mapping.Off:
                        rmopName = @"shaders\shader_options\bump_off";
                        break;
                    case Bump_Mapping.Standard:
                        result.AddSamplerParameter("bump_map");
                        rmopName = @"shaders\shader_options\bump_default";
                        break;
                    case Bump_Mapping.Detail:
                        result.AddSamplerParameter("bump_map");
                        result.AddSamplerParameter("bump_detail_map");
                        result.AddFloatParameter("bump_detail_coefficient");
                        rmopName = @"shaders\shader_options\bump_detail";
                        break;
                }
            }
            if (methodName == "alpha_test")
            {
                optionName = ((Alpha_Test)option).ToString();
                switch ((Alpha_Test)option)
                {
                    case Alpha_Test.None:
                        rmopName = @"shaders\shader_options\alpha_test_off";
                        break;
                    case Alpha_Test.Simple:
                        result.AddSamplerParameter("alpha_test_map");
                        rmopName = @"shaders\shader_options\alpha_test_on";
                        break;
                    case Alpha_Test.Multiply_Map:
                        result.AddSamplerParameter("alpha_test_map");
                        result.AddSamplerParameter("multiply_map");
                        rmopName = @"shaders\custom_options\alpha_test_multiply_map";
                        break;
                }
            }
            if (methodName == "specular_mask")
            {
                optionName = ((Specular_Mask)option).ToString();
                switch ((Specular_Mask)option)
                {
                    case Specular_Mask.Specular_Mask_From_Texture:
                        result.AddSamplerParameter("specular_mask_texture");
                        rmopName = @"shaders\shader_options\specular_mask_from_texture";
                        break;
                }
            }
            if (methodName == "material_model")
            {
                optionName = ((Material_Model)option).ToString();
                switch ((Material_Model)option)
                {
                    case Material_Model.Two_Lobe_Phong:
                        result.AddFloatParameter("diffuse_coefficient");
                        result.AddFloatParameter("specular_coefficient");
                        result.AddFloatParameter("normal_specular_power");
                        result.AddFloat4Parameter("normal_specular_tint");
                        result.AddFloatParameter("glancing_specular_power");
                        result.AddFloat4Parameter("glancing_specular_tint");
                        result.AddFloatParameter("fresnel_curve_steepness");
                        result.AddFloatParameter("area_specular_contribution");
                        result.AddFloatParameter("analytical_specular_contribution");
                        result.AddFloatParameter("environment_map_specular_contribution");
                        result.AddBooleanParameter("order3_area_specular");
                        result.AddBooleanParameter("no_dynamic_lights");
                        result.AddFloatParameter("albedo_specular_tint_blend");
                        result.AddFloatParameter("analytical_anti_shadow_control");
                        rmopName = @"shaders\shader_options\material_two_lobe_phong_option";
                        break;
                    case Material_Model.Custom_Specular:
                        result.AddFloatParameter("diffuse_coefficient");
                        result.AddFloatParameter("specular_coefficient");
                        result.AddFloatParameter("environment_map_specular_contribution");
                        result.AddFloatParameter("area_specular_contribution");
                        result.AddFloatParameter("analytical_specular_contribution");
                        result.AddSamplerWithoutXFormParameter("specular_lobe");
                        result.AddSamplerWithoutXFormParameter("glancing_falloff");
                        result.AddSamplerParameter("material_map");
                        rmopName = @"shaders\custom_options\material_custom_specular";
                        break;
                }
            }
            if (methodName == "environment_mapping")
            {
                optionName = ((Environment_Mapping)option).ToString();
                switch ((Environment_Mapping)option)
                {
                    case Environment_Mapping.Per_Pixel:
                        result.AddSamplerWithoutXFormParameter("environment_map");
                        result.AddFloat4Parameter("env_tint_color");
                        result.AddFloatParameter("env_roughness_scale");
                        rmopName = @"shaders\shader_options\env_map_per_pixel";
                        break;
                    case Environment_Mapping.Dynamic:
                    case Environment_Mapping.Dynamic_Reach:
                        result.AddFloat4Parameter("env_tint_color");
                        result.AddSamplerWithoutXFormParameter("dynamic_environment_map_0", RenderMethodExtern.texture_dynamic_environment_map_0);
                        result.AddSamplerWithoutXFormParameter("dynamic_environment_map_1", RenderMethodExtern.texture_dynamic_environment_map_1);
                        result.AddFloatParameter("env_roughness_scale");
                        rmopName = @"shaders\shader_options\env_map_dynamic";
                        break;
                    case Environment_Mapping.From_Flat_Texture:
                        result.AddSamplerWithoutXFormParameter("flat_environment_map");
                        result.AddFloat4Parameter("env_tint_color");
                        result.AddFloat4Parameter("flat_envmap_matrix_x", RenderMethodExtern.flat_envmap_matrix_x);
                        result.AddFloat4Parameter("flat_envmap_matrix_y", RenderMethodExtern.flat_envmap_matrix_y);
                        result.AddFloat4Parameter("flat_envmap_matrix_z", RenderMethodExtern.flat_envmap_matrix_z);
                        result.AddFloatParameter("hemisphere_percentage");
                        result.AddFloat4Parameter("env_bloom_override");
                        result.AddFloatParameter("env_bloom_override_intensity");
                        rmopName = @"shaders\shader_options\env_map_from_flat_texture";
                        break;
                    case Environment_Mapping.Per_Pixel_Mip:
                        result.AddSamplerWithoutXFormParameter("environment_map");
                        result.AddFloat4Parameter("env_tint_color");
                        rmopName = @"shaders\shader_options\env_map_per_pixel";
                        break;
                }
            }
            if (methodName == "self_illumination")
            {
                optionName = ((Self_Illumination)option).ToString();
                switch ((Self_Illumination)option)
                {
                    case Self_Illumination.Simple:
                        result.AddSamplerParameter("self_illum_map");
                        result.AddFloat4Parameter("self_illum_color");
                        result.AddFloatParameter("self_illum_intensity");
                        rmopName = @"shaders\shader_options\illum_simple";
                        break;
                    case Self_Illumination._3_Channel_Self_Illum:
                        result.AddSamplerParameter("self_illum_map");
                        result.AddFloat4Parameter("channel_a");
                        result.AddFloat4Parameter("channel_b");
                        result.AddFloat4Parameter("channel_c");
                        result.AddFloatParameter("self_illum_intensity");
                        rmopName = @"shaders\shader_options\illum_3_channel";
                        break;
                    case Self_Illumination.Plasma:
                        result.AddSamplerParameter("noise_map_a");
                        result.AddSamplerParameter("noise_map_b");
                        result.AddFloat4Parameter("color_medium");
                        result.AddFloat4Parameter("color_wide");
                        result.AddFloat4Parameter("color_sharp");
                        result.AddFloatParameter("self_illum_intensity");
                        result.AddSamplerParameter("alpha_mask_map");
                        result.AddFloatParameter("thinness_medium");
                        result.AddFloatParameter("thinness_wide");
                        result.AddFloatParameter("thinness_sharp");
                        rmopName = @"shaders\shader_options\illum_plasma";
                        break;
                    case Self_Illumination.From_Diffuse:
                        result.AddFloat4Parameter("self_illum_color");
                        result.AddFloatParameter("self_illum_intensity");
                        rmopName = @"shaders\shader_options\illum_from_diffuse";
                        break;
                    case Self_Illumination.Illum_Detail:
                        result.AddSamplerParameter("self_illum_map");
                        result.AddSamplerParameter("self_illum_detail_map");
                        result.AddFloat4Parameter("self_illum_color");
                        result.AddFloatParameter("self_illum_intensity");
                        rmopName = @"shaders\shader_options\illum_detail";
                        break;
                    case Self_Illumination.Meter:
                        result.AddSamplerParameter("meter_map");
                        result.AddFloat4Parameter("meter_color_off");
                        result.AddFloat4Parameter("meter_color_on");
                        result.AddFloatParameter("meter_value");
                        rmopName = @"shaders\shader_options\illum_meter";
                        break;
                    case Self_Illumination.Self_Illum_Times_Diffuse:
                        result.AddSamplerParameter("self_illum_map");
                        result.AddFloat4Parameter("self_illum_color");
                        result.AddFloatParameter("self_illum_intensity");
                        result.AddFloatParameter("primary_change_color_blend");
                        rmopName = @"shaders\shader_options\illum_times_diffuse";
                        break;
                    case Self_Illumination.Window_Room:
                        result.AddSamplerParameter("opacity_map");
                        result.AddSamplerParameter("ceiling");
                        result.AddSamplerParameter("floors");
                        result.AddSamplerParameter("walls");
                        result.AddXFormOnlyParameter("transform");
                        result.AddFloatParameter("distance_fade_scale");
                        result.AddFloatParameter("self_illum_intensity");
                        rmopName = @"shaders\custom_options\window_room_map";
                        break;
                }
            }
            if (methodName == "blend_mode")
            {
                optionName = ((Blend_Mode)option).ToString();
            }
            if (methodName == "parallax")
            {
                optionName = ((Parallax)option).ToString();
                switch ((Parallax)option)
                {
                    case Parallax.Simple:
                    case Parallax.Interpolated:
                        result.AddSamplerParameter("height_map");
                        result.AddFloatParameter("height_scale");
                        rmopName = @"shaders\shader_options\parallax_simple";
                        break;
                    case Parallax.Simple_Detail:
                        result.AddSamplerParameter("height_map");
                        result.AddFloatParameter("height_scale");
                        result.AddSamplerParameter("height_scale_map");
                        rmopName = @"shaders\shader_options\parallax_detail";
                        break;
                }
            }
            if (methodName == "misc")
            {
                optionName = ((Misc)option).ToString();
            }

            return result;
        }

        public Array GetMethodNames()
        {
            return Enum.GetValues(typeof(CustomMethods));
        }

        public Array GetMethodOptionNames(int methodIndex)
        {
            switch ((CustomMethods)methodIndex)
            {
                case CustomMethods.Albedo:
                    return Enum.GetValues(typeof(Albedo));
                case CustomMethods.Bump_Mapping:
                    return Enum.GetValues(typeof(Bump_Mapping));
                case CustomMethods.Alpha_Test:
                    return Enum.GetValues(typeof(Alpha_Test));
                case CustomMethods.Specular_Mask:
                    return Enum.GetValues(typeof(Specular_Mask));
                case CustomMethods.Material_Model:
                    return Enum.GetValues(typeof(Material_Model));
                case CustomMethods.Environment_Mapping:
                    return Enum.GetValues(typeof(Environment_Mapping));
                case CustomMethods.Self_Illumination:
                    return Enum.GetValues(typeof(Self_Illumination));
                case CustomMethods.Blend_Mode:
                    return Enum.GetValues(typeof(Blend_Mode));
                case CustomMethods.Parallax:
                    return Enum.GetValues(typeof(Parallax));
                case CustomMethods.Misc:
                    return Enum.GetValues(typeof(Misc));
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
