using System.Collections.Generic;
using HaloShaderGenerator.DirectX;
using HaloShaderGenerator.Generator;
using HaloShaderGenerator.Globals;
using System;

namespace HaloShaderGenerator.Cortana
{
    public class CortanaGenerator : IShaderGenerator
    {
        private bool TemplateGenerationValid;
        private bool ApplyFixes;

        Albedo albedo;
        Bump_Mapping bump_mapping;
        Alpha_Test alpha_test;
        Material_Model material_model;
        Environment_Mapping environment_mapping;
        Warp warp;
        Lighting lighting;
        Scanlines scanlines;
        Transparency transparency;

        public CortanaGenerator(bool applyFixes = false) { TemplateGenerationValid = false; ApplyFixes = applyFixes; }

        public CortanaGenerator(Albedo albedo, Bump_Mapping bump_mapping, Alpha_Test alpha_test, Material_Model material_model, 
            Environment_Mapping environment_mapping, Warp warp, Lighting lighting, Scanlines scanlines, Transparency transparency, bool applyFixes = false)
        {
            this.albedo = albedo;
            this.bump_mapping = bump_mapping;
            this.alpha_test = alpha_test;
            this.material_model = material_model;
            this.environment_mapping = environment_mapping;
            this.warp = warp;
            this.lighting = lighting;
            this.scanlines = scanlines;
            this.transparency = transparency;

            ApplyFixes = applyFixes;
            TemplateGenerationValid = true;
        }

        public CortanaGenerator(byte[] options, bool applyFixes = false)
        {
            options = ValidateOptions(options);

            this.albedo = (Albedo)options[0];
            this.bump_mapping = (Bump_Mapping)options[1];
            this.alpha_test = (Alpha_Test)options[2];
            this.material_model = (Material_Model)options[3];
            this.environment_mapping = (Environment_Mapping)options[4];
            this.warp = (Warp)options[5];
            this.lighting = (Lighting)options[6];
            this.scanlines = (Scanlines)options[7];
            this.transparency = (Transparency)options[8];

            ApplyFixes = applyFixes;
            TemplateGenerationValid = true;
        }

        public ShaderGeneratorResult GeneratePixelShader(ShaderStage entryPoint)
        {
            if (!TemplateGenerationValid)
                throw new System.Exception("Generator initialized with shared shader constructor. Use template constructor.");

            List<D3D.SHADER_MACRO> macros = new List<D3D.SHADER_MACRO>();

            macros.Add(new D3D.SHADER_MACRO { Name = "_DEFINITION_HELPER_HLSLI", Definition = "1" });
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<ShaderStage>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Shared.ShaderType>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Albedo>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Bump_Mapping>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Shared.Alpha_Test>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Shared.Material_Model>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Shared.Environment_Mapping>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Shared.Self_Illumination>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Shared.Blend_Mode>());

            macros.Add(ShaderGeneratorBase.CreateMacro("APPLY_HLSL_FIXES", ApplyFixes ? 1 : 0));

            Shared.Environment_Mapping sEnvironmentMapping = (Shared.Environment_Mapping)Enum.Parse(typeof(Shared.Environment_Mapping), environment_mapping.ToString());
            if (environment_mapping == Environment_Mapping.Dynamic_Reach)
            {
                sEnvironmentMapping = Shared.Environment_Mapping.Dynamic;
                macros.Add(ShaderGeneratorBase.CreateMacro("REACH_ENV_DYNAMIC", 1));
            }

            //
            // Convert to shared enum
            //

            var sAlphaTest = Enum.Parse(typeof(Shared.Alpha_Test), alpha_test.ToString());
            var sMaterialModel = Enum.Parse(typeof(Shared.Material_Model), material_model.ToString());
            var sBlendMode = Shared.Blend_Mode.Opaque;

            //
            // The following code properly names the macros (like in rmdf)
            //

            macros.Add(ShaderGeneratorBase.CreateMacro("calc_albedo_ps", albedo, "calc_albedo_", "_ps"));

            macros.Add(ShaderGeneratorBase.CreateMacro("calc_bumpmap_ps", bump_mapping, "calc_bumpmap_", "_ps"));
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_bumpmap_vs", bump_mapping, "calc_bumpmap_", "_vs"));
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_alpha_test_ps", sAlphaTest, "calc_alpha_test_", "_ps"));

            macros.Add(ShaderGeneratorBase.CreateMacro("calc_material_analytic_specular", sMaterialModel, "calc_material_analytic_specular_", "_ps"));
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_material_area_specular", sMaterialModel, "calc_material_area_specular_", "_ps"));
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_lighting_ps", sMaterialModel, "calc_lighting_", "_ps"));
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_dynamic_lighting_ps", sMaterialModel, "calc_dynamic_lighting_", "_ps"));

            macros.Add(ShaderGeneratorBase.CreateMacro("material_type", sMaterialModel, "material_type_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("envmap_type", sEnvironmentMapping, "envmap_type_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("blend_type", sBlendMode, "blend_type_"));

            macros.Add(ShaderGeneratorBase.CreateMacro("shaderstage", entryPoint, "k_shaderstage_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("shadertype", Shared.ShaderType.Cortana, "k_shadertype_"));

            macros.Add(ShaderGeneratorBase.CreateMacro("albedo_arg", albedo, "k_albedo_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("material_type_arg", sMaterialModel, "k_material_model_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("envmap_type_arg", sEnvironmentMapping, "k_environment_mapping_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("blend_type_arg", sBlendMode, "k_blend_mode_"));

            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource($"pixl_cortana.hlsl", macros, "entry_" + entryPoint.ToString().ToLower(), "ps_3_0");

            return new ShaderGeneratorResult(shaderBytecode);
        }

        public ShaderGeneratorResult GenerateSharedPixelShader(ShaderStage entryPoint, int methodIndex, int optionIndex)
        {
            return null;
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
            macros.Add(ShaderGeneratorBase.CreateMacro("shadertype", Shared.ShaderType.Cortana, "shadertype_"));

            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource(@"glvs_cortana.hlsl", macros, $"entry_{entryPoint.ToString().ToLower()}", "vs_3_0");

            return new ShaderGeneratorResult(shaderBytecode);
        }

        public ShaderGeneratorResult GenerateVertexShader(VertexType vertexType, ShaderStage entryPoint)
        {
            return null;
        }

        public int GetMethodCount()
        {
            return Enum.GetValues(typeof(CortanaMethods)).Length;
        }

        public int GetMethodOptionCount(int methodIndex)
        {
            switch ((CortanaMethods)methodIndex)
            {
                case CortanaMethods.Albedo:
                    return Enum.GetValues(typeof(Albedo)).Length;
                case CortanaMethods.Bump_Mapping:
                    return Enum.GetValues(typeof(Bump_Mapping)).Length;
                case CortanaMethods.Alpha_Test:
                    return Enum.GetValues(typeof(Alpha_Test)).Length;
                case CortanaMethods.Material_Model:
                    return Enum.GetValues(typeof(Material_Model)).Length;
                case CortanaMethods.Environment_Mapping:
                    return Enum.GetValues(typeof(Environment_Mapping)).Length;
                case CortanaMethods.Warp:
                case CortanaMethods.Lighting:
                case CortanaMethods.Scanlines:
                case CortanaMethods.Transparency:
                    return 1;
            }
            return -1;
        }

        public int GetMethodOptionValue(int methodIndex)
        {
            switch ((CortanaMethods)methodIndex)
            {
                case CortanaMethods.Albedo:
                    return (int)albedo;
                case CortanaMethods.Bump_Mapping:
                    return (int)bump_mapping;
                case CortanaMethods.Alpha_Test:
                    return (int)alpha_test;
                case CortanaMethods.Material_Model:
                    return (int)material_model;
                case CortanaMethods.Environment_Mapping:
                    return (int)environment_mapping;
                case CortanaMethods.Warp:
                    return (int)warp;
                case CortanaMethods.Lighting:
                    return (int)lighting;
                case CortanaMethods.Scanlines:
                    return (int)scanlines;
                case CortanaMethods.Transparency:
                    return (int)transparency;
            }
            return -1;
        }

        public bool IsEntryPointSupported(ShaderStage entryPoint)
        {
            switch (entryPoint)
            {
                case ShaderStage.Albedo:
                case ShaderStage.Static_Sh:
                case ShaderStage.Static_Prt_Ambient:
                case ShaderStage.Active_Camo:
                    return true;
            }

            return false;
        }

        public bool IsMethodSharedInEntryPoint(ShaderStage entryPoint, int method_index)
        {
            return false;
        }

        public bool IsSharedPixelShaderUsingMethods(ShaderStage entryPoint)
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
                    result.AddFloat4ColorParameter("albedo_color");
                    result.AddFloat4ColorParameter("detail_color");
                    result.AddFloatParameter("layer_depth");
                    result.AddFloatParameter("layer_contrast");
                    result.AddIntegerParameter("layer_count");
                    result.AddFloatParameter("texcoord_aspect_ratio");
                    result.AddFloatParameter("depth_darken");
                    break;
            }

            switch (bump_mapping)
            {
                case Bump_Mapping.Standard:
                    result.AddSamplerParameter("bump_map");
                    break;
            }

            switch (alpha_test)
            {
                case Alpha_Test.Simple:
                    result.AddSamplerParameter("alpha_test_map");
                    break;
            }

            switch (material_model)
            {
                case Material_Model.Cook_Torrance:
                    result.AddFloatParameter("diffuse_coefficient");
                    result.AddFloatParameter("specular_coefficient");
                    result.AddFloat3ColorParameter("specular_tint");
                    result.AddFloat3ColorParameter("fresnel_color");
                    result.AddFloatParameter("use_fresnel_color_environment");
                    result.AddFloat3ColorParameter("fresnel_color_environment");
                    result.AddFloatParameter("fresnel_power");
                    result.AddFloatParameter("roughness");
                    result.AddFloatParameter("area_specular_contribution");
                    result.AddFloatParameter("analytical_specular_contribution");
                    result.AddFloatParameter("environment_map_specular_contribution");
                    result.AddBooleanParameter("order3_area_specular");
                    result.AddBooleanParameter("use_material_texture");
                    result.AddSamplerParameter("material_texture");
                    result.AddBooleanParameter("no_dynamic_lights");
                    result.AddSamplerWithoutXFormParameter("g_sampler_cc0236", RenderMethodExtern.texture_cook_torrance_cc0236);
                    result.AddSamplerWithoutXFormParameter("g_sampler_dd0236", RenderMethodExtern.texture_cook_torrance_dd0236);
                    result.AddSamplerWithoutXFormParameter("g_sampler_c78d78", RenderMethodExtern.texture_cook_torrance_c78d78);
                    result.AddFloatParameter("albedo_blend_with_specular_tint");
                    result.AddFloatParameter("albedo_blend");
                    result.AddFloatParameter("analytical_anti_shadow_control");
                    result.AddFloatParameter("rim_fresnel_coefficient");
                    result.AddFloat3ColorParameter("rim_fresnel_color");
                    result.AddFloatParameter("rim_fresnel_power");
                    result.AddFloatParameter("rim_fresnel_albedo_blend");
                    break;
            }

            switch (environment_mapping)
            {
                case Environment_Mapping.None:
                    break;
                case Environment_Mapping.Per_Pixel:
                    result.AddSamplerWithoutXFormParameter("environment_map");
                    result.AddFloat3ColorParameter("env_tint_color");
                    result.AddFloatParameter("env_roughness_scale");
                    break;
                case Environment_Mapping.Dynamic:
                case Environment_Mapping.Dynamic_Reach:
                    result.AddFloat3ColorParameter("env_tint_color");
                    result.AddSamplerParameter("dynamic_environment_map_0", RenderMethodExtern.texture_dynamic_environment_map_0);
                    result.AddSamplerParameter("dynamic_environment_map_1", RenderMethodExtern.texture_dynamic_environment_map_1);
                    result.AddFloatParameter("env_roughness_scale");
                    break;
            }

            switch (warp)
            {
                case Warp.Default:
                    result.AddFloatParameter("warp_amount");
                    break;
            }

            switch (scanlines)
            {
                case Scanlines.Default:
                    result.AddSamplerParameter("scanline_map");
                    result.AddFloatParameter("scanline_amount_opaque");
                    result.AddFloatParameter("scanline_amount_transparent");
                    result.AddFloat4Parameter("ss_constants", RenderMethodExtern.screen_constants);
                    break;
            }

            switch (transparency)
            {
                case Transparency.Default:
                    result.AddSamplerParameter("fade_gradient_map");
                    result.AddFloatParameter("fade_gradient_scale");
                    result.AddFloatParameter("warp_fade_offset");
                    break;
            }

            return result;
        }

        public ShaderParameters GetVertexShaderParameters()
        {
            var result = new ShaderParameters();

            switch (transparency)
            {
                case Transparency.Default:
                    result.AddFloatVertexParameter("noise_amount");
                    result.AddSamplerVertexParameter("fade_noise_map");
                    result.AddFloatVertexParameter("fade_offset");
                    result.AddFloat4VertexParameter("screen_constants", RenderMethodExtern.screen_constants);
                    break;
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
                        result.AddFloat4ColorParameter("albedo_color");
                        result.AddFloat4ColorParameter("detail_color");
                        result.AddFloatParameter("layer_depth");
                        result.AddFloatParameter("layer_contrast");
                        result.AddIntegerParameter("layer_count");
                        result.AddFloatParameter("texcoord_aspect_ratio");
                        result.AddFloatParameter("depth_darken");
                        rmopName = @"shaders\shader_options\cortana_albedo";
                        break;
                }
            }
            if (methodName == "bump_mapping")
            {
                optionName = ((Bump_Mapping)option).ToString();

                switch ((Bump_Mapping)option)
                {
                    case Bump_Mapping.Standard:
                        result.AddSamplerParameter("bump_map");
                        rmopName = @"shaders\shader_options\bump_default";
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
                }
            }
            if (methodName == "material_model")
            {
                optionName = ((Material_Model)option).ToString();

                switch ((Material_Model)option)
                {
                    case Material_Model.Cook_Torrance:
                        result.AddFloatParameter("diffuse_coefficient");
                        result.AddFloatParameter("specular_coefficient");
                        result.AddFloat3ColorParameter("specular_tint");
                        result.AddFloat3ColorParameter("fresnel_color");
                        result.AddFloatParameter("use_fresnel_color_environment");
                        result.AddFloat3ColorParameter("fresnel_color_environment");
                        result.AddFloatParameter("fresnel_power");
                        result.AddFloatParameter("roughness");
                        result.AddFloatParameter("area_specular_contribution");
                        result.AddFloatParameter("analytical_specular_contribution");
                        result.AddFloatParameter("environment_map_specular_contribution");
                        result.AddBooleanParameter("order3_area_specular");
                        result.AddBooleanParameter("use_material_texture");
                        result.AddSamplerParameter("material_texture");
                        result.AddBooleanParameter("no_dynamic_lights");
                        result.AddSamplerWithoutXFormParameter("g_sampler_cc0236", RenderMethodExtern.texture_cook_torrance_cc0236);
                        result.AddSamplerWithoutXFormParameter("g_sampler_dd0236", RenderMethodExtern.texture_cook_torrance_dd0236);
                        result.AddSamplerWithoutXFormParameter("g_sampler_c78d78", RenderMethodExtern.texture_cook_torrance_c78d78);
                        result.AddFloatParameter("albedo_blend_with_specular_tint");
                        result.AddFloatParameter("albedo_blend");
                        result.AddFloatParameter("analytical_anti_shadow_control");
                        result.AddFloatParameter("rim_fresnel_coefficient");
                        result.AddFloat3ColorParameter("rim_fresnel_color");
                        result.AddFloatParameter("rim_fresnel_power");
                        result.AddFloatParameter("rim_fresnel_albedo_blend");
                        rmopName = @"shaders\shader_options\material_cook_torrance_option";
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
                        result.AddFloat3ColorParameter("env_tint_color");
                        result.AddFloatParameter("env_roughness_scale");
                        rmopName = @"shaders\shader_options\env_map_per_pixel";
                        break;
                    case Environment_Mapping.Dynamic:
                    case Environment_Mapping.Dynamic_Reach:
                        result.AddFloat3ColorParameter("env_tint_color");
                        result.AddSamplerParameter("dynamic_environment_map_0", RenderMethodExtern.texture_dynamic_environment_map_0);
                        result.AddSamplerParameter("dynamic_environment_map_1", RenderMethodExtern.texture_dynamic_environment_map_1);
                        result.AddFloatParameter("env_roughness_scale");
                        rmopName = @"shaders\shader_options\env_map_dynamic";
                        break;
                }
            }
            if (methodName == "warp")
            {
                optionName = ((Warp)option).ToString();

                switch ((Warp)option)
                {
                    case Warp.Default:
                        result.AddFloatParameter("warp_amount");
                        rmopName = @"shaders\shader_options\warp_cortana_default";
                        break;
                }
            }
            if (methodName == "lighting")
            {
                optionName = ((Lighting)option).ToString();
            }
            if (methodName == "scanlines")
            {
                optionName = ((Scanlines)option).ToString();

                switch ((Scanlines)option)
                {
                    case Scanlines.Default:
                        result.AddSamplerParameter("scanline_map");
                        result.AddFloatParameter("scanline_amount_opaque");
                        result.AddFloatParameter("scanline_amount_transparent");
                        result.AddFloat4Parameter("screen_constants", RenderMethodExtern.screen_constants);
                        rmopName = @"shaders\shader_options\cortana_screenspace";
                        break;
                }
            }
            if (methodName == "transparency")
            {
                optionName = ((Transparency)option).ToString();

                switch ((Transparency)option)
                {
                    case Transparency.Default:
                        result.AddSamplerParameter("fade_gradient_map");
                        result.AddFloatParameter("fade_gradient_scale");
                        result.AddFloatVertexParameter("noise_amount");
                        result.AddSamplerVertexParameter("fade_noise_map");
                        result.AddFloatVertexParameter("fade_offset");
                        result.AddFloatParameter("warp_fade_offset");
                        rmopName = @"shaders\shader_options\cortana_transparency";
                        break;
                }
            }

            return result;
        }

        public Array GetMethodNames()
        {
            return Enum.GetValues(typeof(CortanaMethods));
        }

        public Array GetMethodOptionNames(int methodIndex)
        {
            switch ((CortanaMethods)methodIndex)
            {
                case CortanaMethods.Albedo:
                    return Enum.GetValues(typeof(Albedo));
                case CortanaMethods.Bump_Mapping:
                    return Enum.GetValues(typeof(Bump_Mapping));
                case CortanaMethods.Alpha_Test:
                    return Enum.GetValues(typeof(Alpha_Test));
                case CortanaMethods.Material_Model:
                    return Enum.GetValues(typeof(Material_Model));
                case CortanaMethods.Environment_Mapping:
                    return Enum.GetValues(typeof(Environment_Mapping));
                case CortanaMethods.Warp:
                    return Enum.GetValues(typeof(Warp));
                case CortanaMethods.Lighting:
                    return Enum.GetValues(typeof(Lighting));
                case CortanaMethods.Scanlines:
                    return Enum.GetValues(typeof(Scanlines));
                case CortanaMethods.Transparency:
                    return Enum.GetValues(typeof(Transparency));
                default:
                    return null;
            }
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
