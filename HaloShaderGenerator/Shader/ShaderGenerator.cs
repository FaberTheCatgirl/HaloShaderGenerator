using System.Collections.Generic;
using HaloShaderGenerator.DirectX;
using HaloShaderGenerator.Generator;
using HaloShaderGenerator.Globals;
using System;

namespace HaloShaderGenerator.Shader
{
    public class ShaderGenerator : IShaderGenerator
    {
        private bool TemplateGenerationValid;
        private bool ApplyFixes;
        private bool MS30 = false;

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
        Misc_Attr_Animation misc_attr_animation;
        Shared.Distortion distortion;
        Shared.Soft_Fade soft_fade;

        /// <summary>
        /// Generator insantiation for shared shaders. Does not require method options.
        /// </summary>
        public ShaderGenerator(bool applyFixes = false) { TemplateGenerationValid = false; ApplyFixes = applyFixes; }

        /// <summary>
        /// Generator instantiation for method specific shaders.
        /// </summary>
        public ShaderGenerator(Albedo albedo, Bump_Mapping bump_mapping, Alpha_Test alpha_test, Specular_Mask specular_mask, Material_Model material_model,
            Environment_Mapping environment_mapping, Self_Illumination self_illumination, Blend_Mode blend_mode, Parallax parallax, Misc misc,
            Shared.Distortion distortion, Shared.Soft_Fade soft_fade, Misc_Attr_Animation misc_attr_animation, bool applyFixes = false)
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
            this.misc_attr_animation = misc_attr_animation;
            this.distortion = distortion;
            this.soft_fade = soft_fade;

            ApplyFixes = applyFixes;
            TemplateGenerationValid = true;
        }

        public ShaderGenerator(byte[] options, bool applyFixes = false)
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
            this.misc_attr_animation = (Misc_Attr_Animation)options[10];
            this.distortion = (Shared.Distortion)options[11];
            this.soft_fade = (Shared.Soft_Fade)options[12];

            ApplyFixes = applyFixes;
            TemplateGenerationValid = true;
        }

        public void SetMS30(bool ms30) => MS30 = ms30;

        public ShaderGeneratorResult GeneratePixelShader(ShaderStage entryPoint)
        {
            if (!TemplateGenerationValid)
                throw new System.Exception("Generator initialized with shared shader constructor. Use template constructor.");

            List<D3D.SHADER_MACRO> macros = new List<D3D.SHADER_MACRO>();

            macros.Add(new D3D.SHADER_MACRO { Name = "_DEFINITION_HELPER_HLSLI", Definition = "1" });
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<ShaderStage>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Shared.ShaderType>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Shared.Albedo>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Bump_Mapping>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Shared.Alpha_Test>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Specular_Mask>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Shared.Material_Model>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Shared.Environment_Mapping>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Shared.Self_Illumination>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Shared.Blend_Mode>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Parallax>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Misc>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Misc_Attr_Animation>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Shared.Distortion>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Shared.Soft_Fade>());

            macros.Add(ShaderGeneratorBase.CreateMacro("APPLY_HLSL_FIXES", ApplyFixes ? 1 : 0));

            Shared.Material_Model sMaterialModel;
            if (material_model == Material_Model.Cook_Torrance_Odst)
            {
                sMaterialModel = Shared.Material_Model.Cook_Torrance;
                macros.Add(ShaderGeneratorBase.CreateMacro("ODST_COOK_TORRANCE", 1)); //More of Pedros work
            }
            else
            {
                sMaterialModel = (Shared.Material_Model)Enum.Parse(typeof(Shared.Material_Model), material_model.ToString());
            }

            Shared.Environment_Mapping sEnvironmentMapping = (Shared.Environment_Mapping)Enum.Parse(typeof(Shared.Environment_Mapping), environment_mapping.ToString());
            if (environment_mapping == Environment_Mapping.Dynamic_Reach)
            {
                sEnvironmentMapping = Shared.Environment_Mapping.Dynamic;
                macros.Add(ShaderGeneratorBase.CreateMacro("REACH_ENV_DYNAMIC", 1));
            }

            //
            // Convert to shared enum
            //

            var sAlbedo = Enum.Parse(typeof(Shared.Albedo), albedo.ToString());
            var sAlphaTest = Enum.Parse(typeof(Shared.Alpha_Test), alpha_test.ToString());
            var sSelfIllumination = Enum.Parse(typeof(Shared.Self_Illumination), self_illumination.ToString());
            var sBlendMode = Enum.Parse(typeof(Shared.Blend_Mode), blend_mode.ToString());

            //
            // The following code properly names the macros (like in rmdf)
            //

            macros.Add(ShaderGeneratorBase.CreateMacro("calc_albedo_ps", sAlbedo, "calc_albedo_", "_ps"));
            if (albedo == Albedo.Constant_Color)
                macros.Add(ShaderGeneratorBase.CreateMacro("calc_albedo_vs", sAlbedo, "calc_albedo_", "_vs"));

            macros.Add(ShaderGeneratorBase.CreateMacro("calc_bumpmap_ps", bump_mapping, "calc_bumpmap_", "_ps"));
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_bumpmap_vs", bump_mapping, "calc_bumpmap_", "_vs"));
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_alpha_test_ps", sAlphaTest, "calc_alpha_test_", "_ps"));
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_specular_mask_ps", specular_mask, "calc_", "_ps"));
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_self_illumination_ps", sSelfIllumination, "calc_self_illumination_", "_ps"));
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_parallax_ps", parallax, "calc_parallax_", "_ps"));

            switch (parallax)
            {
                case Parallax.Simple_Detail:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_parallax_vs", Parallax.Simple, "calc_parallax_", "_vs"));
                    break;
                default:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_parallax_vs", parallax, "calc_parallax_", "_vs"));
                    break;
            }

            macros.Add(ShaderGeneratorBase.CreateMacro("calc_material_analytic_specular", sMaterialModel, "calc_material_analytic_specular_", "_ps"));
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_material_area_specular", sMaterialModel, "calc_material_area_specular_", "_ps"));
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_lighting_ps", sMaterialModel, "calc_lighting_", "_ps"));
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_dynamic_lighting_ps", sMaterialModel, "calc_dynamic_lighting_", "_ps"));

            macros.Add(ShaderGeneratorBase.CreateMacro("material_type", sMaterialModel, "material_type_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("envmap_type", sEnvironmentMapping, "envmap_type_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("blend_type", sBlendMode, "blend_type_"));

            macros.Add(ShaderGeneratorBase.CreateMacro("shaderstage", entryPoint, "k_shaderstage_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("shadertype", Shared.ShaderType.Shader, "k_shadertype_"));

            macros.Add(ShaderGeneratorBase.CreateMacro("albedo_arg", sAlbedo, "k_albedo_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("material_type_arg", sMaterialModel, "k_material_model_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("envmap_type_arg", sEnvironmentMapping, "k_environment_mapping_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("self_illumination_arg", sSelfIllumination, "k_self_illumination_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("blend_type_arg", sBlendMode, "k_blend_mode_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("misc_arg", misc, "k_misc_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("misc_attr_animation", misc_attr_animation, "k_misc_attr_animation_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("distortion_arg", distortion, "k_distortion_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("soft_fade_arg", soft_fade, "k_soft_fade_"));

            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource($"pixl_shader.hlsl", macros, "entry_" + entryPoint.ToString().ToLower(), "ps_3_0");

            return new ShaderGeneratorResult(shaderBytecode);
        }

        public ShaderGeneratorResult GenerateSharedPixelShader(ShaderStage entryPoint, int methodIndex, int optionIndex)
        {
            if (!IsEntryPointSupported(entryPoint) || !IsPixelShaderShared(entryPoint))
                return null;

            Alpha_Test alphaTestOption;

            switch ((ShaderMethods)methodIndex)
            {
                case ShaderMethods.Alpha_Test:

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
            
            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource($"glps_shader.hlsl", macros, "entry_" + entryPoint.ToString().ToLower(), "ps_3_0");

            return new ShaderGeneratorResult(shaderBytecode);
        }

        public ShaderGeneratorResult GenerateSharedVertexShader(VertexType vertexType, ShaderStage entryPoint)
        {
            if (!IsVertexFormatSupported(vertexType) || !IsEntryPointSupported(entryPoint))
                return null;

            List<D3D.SHADER_MACRO> macros = new List<D3D.SHADER_MACRO>();

            if (MS30)
                macros.Add(ShaderGeneratorBase.CreateMacro("MS30_SHADER", 1));

            macros.Add(new D3D.SHADER_MACRO { Name = "_VERTEX_SHADER_HELPER_HLSLI", Definition = "1" });
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<ShaderStage>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<VertexType>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Shared.ShaderType>());
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_vertex_transform", vertexType, "calc_vertex_transform_", ""));
            macros.Add(ShaderGeneratorBase.CreateMacro("transform_dominant_light", vertexType, "transform_dominant_light_", ""));
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_distortion", vertexType, "calc_distortion_", ""));
            macros.Add(ShaderGeneratorBase.CreateVertexMacro("input_vertex_format", vertexType));

            macros.Add(ShaderGeneratorBase.CreateMacro("shaderstage", entryPoint, "k_shaderstage_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("vertextype", vertexType, "k_vertextype_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("shadertype", Shared.ShaderType.Shader, "shadertype_"));

            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource(@"glvs_shader.hlsl", macros, $"entry_{entryPoint.ToString().ToLower()}", "vs_3_0");

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
            return Enum.GetValues(typeof(ShaderMethods)).Length;
        }

        public int GetMethodOptionCount(int methodIndex)
        {
            switch ((ShaderMethods)methodIndex)
            {
                case ShaderMethods.Albedo:
                    return Enum.GetValues(typeof(Albedo)).Length;
                case ShaderMethods.Bump_Mapping:
                    return Enum.GetValues(typeof(Bump_Mapping)).Length;
                case ShaderMethods.Alpha_Test:
                    return Enum.GetValues(typeof(Alpha_Test)).Length;
                case ShaderMethods.Specular_Mask:
                    return Enum.GetValues(typeof(Specular_Mask)).Length;
                case ShaderMethods.Material_Model:
                    return Enum.GetValues(typeof(Material_Model)).Length;
                case ShaderMethods.Environment_Mapping:
                    return Enum.GetValues(typeof(Environment_Mapping)).Length;
                case ShaderMethods.Self_Illumination:
                    return Enum.GetValues(typeof(Self_Illumination)).Length;
                case ShaderMethods.Blend_Mode:
                    return Enum.GetValues(typeof(Blend_Mode)).Length;
                case ShaderMethods.Parallax:
                    return Enum.GetValues(typeof(Parallax)).Length;
                case ShaderMethods.Misc:
                    return Enum.GetValues(typeof(Misc)).Length;
                case ShaderMethods.Misc_Attr_Animation:
                    return Enum.GetValues(typeof(Misc_Attr_Animation)).Length;
                case ShaderMethods.Distortion:
                    return Enum.GetValues(typeof(Shared.Distortion)).Length; //Soft Fade and Distortion need to be shared... why?
                case ShaderMethods.Soft_Fade:
                    return Enum.GetValues(typeof(Shared.Soft_Fade)).Length;
            }
            return -1;
        }

        public int GetMethodOptionValue(int methodIndex)
        {
            switch ((ShaderMethods)methodIndex)
            {
                case ShaderMethods.Albedo:
                    return (int)albedo;
                case ShaderMethods.Bump_Mapping:
                    return (int)bump_mapping;
                case ShaderMethods.Alpha_Test:
                    return (int)alpha_test;
                case ShaderMethods.Specular_Mask:
                    return (int)specular_mask;
                case ShaderMethods.Material_Model:
                    return (int)material_model;
                case ShaderMethods.Environment_Mapping:
                    return (int)environment_mapping;
                case ShaderMethods.Self_Illumination:
                    return (int)self_illumination;
                case ShaderMethods.Blend_Mode:
                    return (int)blend_mode;
                case ShaderMethods.Parallax:
                    return (int)parallax;
                case ShaderMethods.Misc:
                    return (int)misc;
                case ShaderMethods.Distortion:
                    return (int)distortion;
                case ShaderMethods.Soft_Fade:
                    return (int)soft_fade;
                case ShaderMethods.Misc_Attr_Animation:
                    return (int)misc_attr_animation;
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
                case ShaderStage.Active_Camo:
                case ShaderStage.Sfx_Distort:
                case ShaderStage.Dynamic_Light:
                case ShaderStage.Dynamic_Light_Cinematic:
                case ShaderStage.Lightmap_Debug_Mode:
                case ShaderStage.Static_Sh:
                case ShaderStage.Shadow_Generate:
                    return true;
                    
                default:
                case ShaderStage.Default:
                case ShaderStage.Z_Only:
                case ShaderStage.Water_Shading:
                case ShaderStage.Water_Tesselation:
                case ShaderStage.Shadow_Apply:
                case ShaderStage.Static_Default:
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
                    result.AddFloat4ColorParameter("albedo_color");
                    break;
                case Albedo.Two_Detail_Black_Point:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("detail_map");
                    result.AddSamplerParameter("detail_map2");
                    break;
                case Albedo.Two_Detail:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("detail_map");
                    result.AddSamplerParameter("detail_map2");
                    break;
                case Albedo.Detail_Blend:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("detail_map");
                    result.AddSamplerParameter("detail_map2");
                    break;
                case Albedo.Constant_Color:
                    result.AddFloat4ColorParameter("albedo_color");
                    break;
                case Albedo.Two_Change_Color:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("detail_map");
                    result.AddSamplerParameter("change_color_map");
                    result.AddFloat3ColorParameter("primary_change_color", RenderMethodExtern.object_change_color_primary);
                    result.AddFloat3ColorParameter("secondary_change_color", RenderMethodExtern.object_change_color_secondary);
                    break;
                case Albedo.Four_Change_Color:
                case Albedo.Four_Change_Color_Applying_To_Specular:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("detail_map");
                    result.AddSamplerParameter("change_color_map");
                    result.AddFloat3ColorParameter("primary_change_color", RenderMethodExtern.object_change_color_primary);
                    result.AddFloat3ColorParameter("secondary_change_color", RenderMethodExtern.object_change_color_secondary);
                    result.AddFloat3ColorParameter("tertiary_change_color", RenderMethodExtern.object_change_color_tertiary);
                    result.AddFloat3ColorParameter("quaternary_change_color", RenderMethodExtern.object_change_color_quaternary);
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
                case Albedo.Color_Mask:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("detail_map");
                    result.AddSamplerParameter("color_mask_map");
                    result.AddFloat4ColorParameter("albedo_color");
                    result.AddFloat4ColorParameter("albedo_color2");
                    result.AddFloat4ColorParameter("albedo_color3");
                    result.AddFloat4ColorParameter("neutral_gray");
                    break;
                case Albedo.Two_Change_Color_Anim_Overlay:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("detail_map");
                    result.AddSamplerParameter("change_color_map");
                    result.AddFloat3ColorParameter("primary_change_color", RenderMethodExtern.object_change_color_primary);
                    result.AddFloat3ColorParameter("secondary_change_color", RenderMethodExtern.object_change_color_secondary);
                    result.AddFloat4Parameter("primary_change_color_anim", RenderMethodExtern.object_change_color_primary_anim);
                    result.AddFloat4Parameter("secondary_change_color_anim", RenderMethodExtern.object_change_color_secondary_anim);
                    break;
                case Albedo.Chameleon:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("detail_map");
                    result.AddFloat4ColorParameter("chameleon_color0");
                    result.AddFloat4ColorParameter("chameleon_color1");
                    result.AddFloat4ColorParameter("chameleon_color2");
                    result.AddFloat4ColorParameter("chameleon_color3");
                    result.AddFloatParameter("chameleon_color_offset1");
                    result.AddFloatParameter("chameleon_color_offset2");
                    result.AddFloatParameter("chameleon_fresnel_power");
                    break;
                case Albedo.Two_Change_Color_Chameleon:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("detail_map");
                    result.AddSamplerParameter("change_color_map");
                    result.AddFloat3ColorParameter("primary_change_color", RenderMethodExtern.object_change_color_primary);
                    result.AddFloat3ColorParameter("secondary_change_color", RenderMethodExtern.object_change_color_secondary);
                    result.AddFloat4ColorParameter("chameleon_color0");
                    result.AddFloat4ColorParameter("chameleon_color1");
                    result.AddFloat4ColorParameter("chameleon_color2");
                    result.AddFloat4ColorParameter("chameleon_color3");
                    result.AddFloatParameter("chameleon_color_offset1");
                    result.AddFloatParameter("chameleon_color_offset2");
                    result.AddFloatParameter("chameleon_fresnel_power");
                    break;
                case Albedo.Chameleon_Masked:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("detail_map");
                    result.AddSamplerParameter("chameleon_mask_map");
                    result.AddFloat4ColorParameter("chameleon_color0");
                    result.AddFloat4ColorParameter("chameleon_color1");
                    result.AddFloat4ColorParameter("chameleon_color2");
                    result.AddFloat4ColorParameter("chameleon_color3");
                    result.AddFloatParameter("chameleon_color_offset1");
                    result.AddFloatParameter("chameleon_color_offset2");
                    result.AddFloatParameter("chameleon_fresnel_power");
                    break;
                case Albedo.Color_Mask_Hard_Light:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("detail_map");
                    result.AddSamplerParameter("color_mask_map");
                    result.AddFloat4ColorParameter("albedo_color");
                    break;
                case Albedo.Simple:
                    result.AddSamplerParameter("base_map");
                    result.AddFloat4Parameter("albedo_color");
                    break;
                case Albedo.Two_Change_Color_Tex_Overlay:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("detail_map");
                    result.AddSamplerParameter("change_color_map");
                    result.AddFloat3ColorParameter("primary_change_color", RenderMethodExtern.object_change_color_primary);
                    result.AddSamplerParameter("secondary_change_color_map"); // might be without xform
                    break;
                case Albedo.Chameleon_Albedo_Masked:
                    result.AddSamplerParameter("base_map");
                    result.AddFloat4Parameter("albedo_color");
                    result.AddSamplerParameter("base_masked_map");
                    result.AddFloat4Parameter("albedo_masked_color");
                    result.AddSamplerParameter("chameleon_mask_map");
                    result.AddFloat4ColorParameter("chameleon_color0");
                    result.AddFloat4ColorParameter("chameleon_color1");
                    result.AddFloat4ColorParameter("chameleon_color2");
                    result.AddFloat4ColorParameter("chameleon_color3");
                    result.AddFloatParameter("chameleon_color_offset1");
                    result.AddFloatParameter("chameleon_color_offset2");
                    result.AddFloatParameter("chameleon_fresnel_power");
                    break;
                case Albedo.Custom_Cube:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerWithoutXFormParameter("custom_cube");
                    result.AddFloat4Parameter("albedo_color");
                    break;
                case Albedo.Two_Color:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("detail_map");
                    result.AddFloat4Parameter("albedo_color");
                    result.AddSamplerWithoutXFormParameter("blend_map");
                    result.AddFloat4Parameter("albedo_second_color");
                    break;
                case Albedo.Emblem:
                    result.AddSamplerWithoutXFormParameter("emblem_map", RenderMethodExtern.emblem_player_shoulder_texture);
                    break;
                case Albedo.Scrolling_Cube_Mask:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("detail_map");
                    result.AddFloat4Parameter("albedo_color");
                    result.AddSamplerWithoutXFormParameter("color_blend_mask_cubemap");
                    result.AddFloat4Parameter("albedo_second_color");
                    break;
                case Albedo.Scrolling_Cube:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("detail_map");
                    result.AddSamplerWithoutXFormParameter("color_cubemap");
                    break;
                case Albedo.Scrolling_Texture_Uv:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerWithoutXFormParameter("color_texture");
                    result.AddFloatParameter("u_speed");
                    result.AddFloatParameter("v_speed");
                    break;
                case Albedo.Texture_From_Misc:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerWithoutXFormParameter("color_texture");
                    break;
                
            }

            switch (bump_mapping)
            {
                case Bump_Mapping.Off:
                    break;
                case Bump_Mapping.Standard:
                    result.AddSamplerParameter("bump_map");
                    break;
                case Bump_Mapping.Detail:
                    result.AddSamplerParameter("bump_map");
                    result.AddSamplerParameter("bump_detail_map");
                    result.AddFloatParameter("bump_detail_coefficient");
                    break;
                case Bump_Mapping.Detail_Masked:
                    result.AddSamplerParameter("bump_map");
                    result.AddSamplerParameter("bump_detail_map");
                    result.AddSamplerParameter("bump_detail_mask_map");
                    result.AddFloatParameter("bump_detail_coefficient");
                    result.AddBooleanParameter("invert_mask");
                    break;
            }

            switch (alpha_test)
            {
                case Alpha_Test.None:
                    break;
                case Alpha_Test.Simple:
                    result.AddSamplerParameter("alpha_test_map");
                    break;
            }

            switch (specular_mask)
            {
                case Specular_Mask.No_Specular_Mask:
                    break;
                case Specular_Mask.Specular_Mask_From_Diffuse:
                    break;
                case Specular_Mask.Specular_Mask_From_Texture:
                case Specular_Mask.Specular_Mask_From_Color_Texture:
                    result.AddSamplerParameter("specular_mask_texture");
                    break;
            }

            switch (material_model)
            {
                case Material_Model.Diffuse_Only:
                    result.AddBooleanParameter("no_dynamic_lights");
                    break;
                case Material_Model.Cook_Torrance_Odst: //Nothing is defined... Why is this here?
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
                case Material_Model.Cook_Torrance_From_Albedo:
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
                case Material_Model.Cook_Torrance_Rim_Fresnel: //Pedro Defined normal Cook_Torrance with Rim Fresnel + From Albedo, Why?
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
                case Material_Model.Cook_Torrance_Two_Color_Spec_Tint:
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
                    result.AddFloat3ColorParameter("specular_second_tint");
                    break;
                case Material_Model.Cook_Torrance_Scrolling_Cube:
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
                    result.AddSamplerParameter("spec_tint_cubemap");
                    break;
                case Material_Model.Cook_Torrance_Scrolling_Cube_Mask:
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
                    result.AddSamplerParameter("tint_blend_mask_cubemap");
                    result.AddSamplerParameter("specular_second_tint");
                    break;
                case Material_Model.Cook_Torrance_Custom_Cube:
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
                    result.AddSamplerParameter("custom_cube_map");
                    break;
                case Material_Model.Cook_Torrance_Pbr_Maps:
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
                    result.AddSamplerParameter("spec_tint_map");
                    break;
                case Material_Model.Two_Lobe_Phong:
                    result.AddFloatParameter("diffuse_coefficient");
                    result.AddFloatParameter("specular_coefficient");
                    result.AddFloatParameter("normal_specular_power");
                    result.AddFloat3ColorParameter("normal_specular_tint");
                    result.AddFloatParameter("glancing_specular_power");
                    result.AddFloat3ColorParameter("glancing_specular_tint");
                    result.AddFloatParameter("fresnel_curve_steepness");
                    result.AddFloatParameter("area_specular_contribution");
                    result.AddFloatParameter("analytical_specular_contribution");
                    result.AddFloatParameter("environment_map_specular_contribution");
                    result.AddBooleanParameter("order3_area_specular");
                    result.AddBooleanParameter("no_dynamic_lights");
                    result.AddFloatParameter("albedo_specular_tint_blend");
                    result.AddFloatParameter("analytical_anti_shadow_control");
                    break;
                case Material_Model.Foliage:
                    result.AddBooleanParameter("no_dynamic_lights");
                    break;
                case Material_Model.None:
                    break;

                case Material_Model.Glass:
                    result.AddFloatParameter("diffuse_coefficient");
                    result.AddFloatParameter("specular_coefficient");
                    result.AddFloatParameter("fresnel_coefficient");
                    result.AddFloatParameter("fresnel_curve_steepness");
                    result.AddFloatParameter("fresnel_curve_bias");
                    result.AddFloatParameter("roughness");
                    result.AddFloatParameter("analytical_specular_contribution");
                    result.AddFloatParameter("area_specular_contribution");
                    result.AddBooleanParameter("no_dynamic_lights");
                    break;
                case Material_Model.Organism:
                    result.AddFloatParameter("diffuse_coefficient");
                    result.AddFloat3ColorParameter("diffuse_tint");
                    result.AddFloatParameter("analytical_specular_coefficient");
                    result.AddFloatParameter("area_specular_coefficient");
                    result.AddFloat3ColorParameter("specular_tint");
                    result.AddFloatParameter("specular_power");
                    result.AddSamplerParameter("specular_map");
                    result.AddFloatParameter("environment_map_coefficient");
                    result.AddFloat3ColorParameter("environment_map_tint");
                    result.AddFloatParameter("fresnel_curve_steepness");
                    result.AddFloatParameter("rim_coefficient");
                    result.AddFloat3ColorParameter("rim_tint");
                    result.AddFloatParameter("rim_power");
                    result.AddFloatParameter("rim_start");
                    result.AddFloatParameter("rim_maps_transition_ratio");
                    result.AddFloatParameter("ambient_coefficient");
                    result.AddFloat3ColorParameter("ambient_tint");
                    result.AddSamplerParameter("occlusion_parameter_map");

                    result.AddFloatParameter("subsurface_coefficient");
                    result.AddFloat3ColorParameter("subsurface_tint");
                    result.AddFloatParameter("subsurface_propagation_bias");
                    result.AddFloatParameter("subsurface_normal_detail");
                    result.AddSamplerParameter("subsurface_map");

                    result.AddFloatParameter("transparence_coefficient");
                    result.AddFloat3ColorParameter("transparence_tint");
                    result.AddFloatParameter("transparence_normal_bias");
                    result.AddFloatParameter("transparence_normal_detail");
                    result.AddSamplerParameter("transparence_map");

                    result.AddFloat3ColorParameter("final_tint");
                    result.AddBooleanParameter("no_dynamic_lights");
                    break;
                case Material_Model.Single_Lobe_Phong:
                    result.AddFloatParameter("diffuse_coefficient");
                    result.AddFloatParameter("specular_coefficient");
                    result.AddFloatParameter("roughness");
                    result.AddFloatParameter("analytical_specular_contribution");
                    result.AddFloatParameter("area_specular_contribution");
                    result.AddFloatParameter("environment_map_specular_contribution");
                    result.AddFloat3ColorParameter("specular_tint");
                    result.AddBooleanParameter("order3_area_specular");
                    result.AddBooleanParameter("no_dynamic_lights");
                    break;
                case Material_Model.Car_Paint:
                    result.AddFloatParameter("diffuse_coefficient"); // TODO: add parameters as this is placeholder
                    break;
                case Material_Model.Hair:
                    result.AddFloatParameter("diffuse_coefficient");
                    result.AddFloat3ColorParameter("diffuse_tint");
                    result.AddFloatParameter("analytical_specular_coefficient");
                    result.AddFloatParameter("area_specular_coefficient");
                    result.AddFloat3ColorParameter("specular_tint");
                    result.AddFloatParameter("specular_power");
                    result.AddSamplerWithoutXFormParameter("specular_map");
                    result.AddSamplerWithoutXFormParameter("specular_shift_map");
                    result.AddSamplerWithoutXFormParameter("specular_noise_map");
                    result.AddFloatParameter("environment_map_coefficient");
                    result.AddFloat3ColorParameter("environment_map_tint");
                    result.AddFloat3ColorParameter("final_tint");
                    result.AddBooleanParameter("no_dynamic_lights");
                    break;
                case Material_Model.Constant:
                    result.AddFloat3ColorParameter("albedo_color");
                    break;
            }

            switch (environment_mapping)
            {
                case Environment_Mapping.None:
                    break;
                case Environment_Mapping.Per_Pixel:
                case Environment_Mapping.Custom_Map:
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
                case Environment_Mapping.From_Flat_Texture:
                    result.AddSamplerWithoutXFormParameter("flat_environment_map");
                    result.AddFloat3ColorParameter("env_tint_color");
                    result.AddFloat3Parameter("flat_envmap_matrix_x", RenderMethodExtern.flat_envmap_matrix_x);
                    result.AddFloat3Parameter("flat_envmap_matrix_y", RenderMethodExtern.flat_envmap_matrix_y);
                    result.AddFloat3Parameter("flat_envmap_matrix_z", RenderMethodExtern.flat_envmap_matrix_z);
                    result.AddFloatParameter("hemisphere_percentage");
                    result.AddFloat4Parameter("env_bloom_override");
                    result.AddFloatParameter("env_bloom_override_intensity");
                    break;
            }

            switch (self_illumination)
            {
                case Self_Illumination.Off:
                    break;
                case Self_Illumination.Simple:
                case Self_Illumination.Simple_With_Alpha_Mask:
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

                case Self_Illumination.Simple_Four_Change_Color:
                    result.AddSamplerParameter("self_illum_map");
                    result.AddFloat4Parameter("self_illum_color", RenderMethodExtern.object_change_color_quaternary);
                    result.AddFloatParameter("self_illum_intensity");
                    break;

                case Self_Illumination.Illum_Change_Color:
                    result.AddSamplerParameter("self_illum_map");
                    result.AddFloat4Parameter("self_illum_color", RenderMethodExtern.object_change_color_primary);
                    result.AddFloatParameter("self_illum_intensity");
                    result.AddFloatParameter("primary_change_color_blend");
                    break;

                case Self_Illumination.Multilayer_Additive:
                    result.AddSamplerParameter("self_illum_map");
                    result.AddFloat4Parameter("self_illum_color");
                    result.AddFloatParameter("self_illum_intensity");
                    result.AddFloatParameter("layer_depth");
                    result.AddFloatParameter("layer_contrast");
                    result.AddFloatParameter("layers_of_4");
                    result.AddIntegerParameter("layers_of_4");
                    result.AddFloatParameter("texcoord_aspect_ratio");
                    result.AddFloatParameter("depth_darken");
                    break;

                case Self_Illumination.Palettized_Plasma:
                    result.AddSamplerParameter("noise_map_a");
                    result.AddSamplerParameter("noise_map_b");
                    result.AddSamplerWithoutXFormParameter("palette");
                    result.AddSamplerParameter("alpha_mask_map");
                    result.AddFloatParameter("alpha_modulation_factor");
                    result.AddSamplerParameter("depth_buffer", RenderMethodExtern.texture_global_target_z);
                    result.AddFloatParameter("depth_fade_range");
                    result.AddFloat4Parameter("self_illum_color");
                    result.AddFloatParameter("self_illum_intensity");
                    result.AddFloatParameter("v_coordinate");
                    result.AddFloat3Parameter("global_depth_constants", RenderMethodExtern.global_depth_constants);
                    result.AddFloat3Parameter("global_camera_forward", RenderMethodExtern.global_camera_forward);
                    break;

                case Self_Illumination.Change_Color_Detail:
                    result.AddSamplerParameter("self_illum_map");
                    result.AddSamplerParameter("self_illum_detail_map");
                    result.AddFloat4Parameter("self_illum_color", RenderMethodExtern.object_change_color_primary);
                    result.AddFloatParameter("self_illum_intensity");
                    break;
            }

            switch (parallax)
            {
                case Parallax.Off:
                    break;
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

            switch (distortion)
            {
                case Shared.Distortion.On:
                    result.AddSamplerParameter("distort_map");
                    result.AddFloatParameter("distort_scale");
                    //result.AddFloatParameter("distort_fadeoff");
                    //result.AddBooleanParameter("distort_selfonly");
                    break;
            }

            switch (soft_fade)
            {
                case Shared.Soft_Fade.On:
                    result.AddBooleanParameter("soft_fresnel_enabled");
                    result.AddFloatParameter("soft_fresnel_power");
                    result.AddBooleanParameter("soft_z_enabled");
                    result.AddFloatParameter("soft_z_range");
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
                    case Albedo.Four_Change_Color_Applying_To_Specular:
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
                        result.AddFloat4ColorParameter("albedo_color");
                        result.AddFloat4ColorParameter("albedo_color2");
                        result.AddFloat4ColorParameter("albedo_color3");
                        result.AddFloat4ColorParameter("neutral_gray");
                        rmopName = @"shaders\shader_options\albedo_color_mask";
                        break;
                    case Albedo.Two_Detail_Black_Point:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerParameter("detail_map");
                        result.AddSamplerParameter("detail_map2");
                        rmopName = @"shaders\shader_options\albedo_two_detail_black_point";
                        break;
                    case Albedo.Two_Change_Color_Anim_Overlay:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerParameter("detail_map");
                        result.AddSamplerParameter("change_color_map");
                        result.AddFloat3ColorParameter("primary_change_color", RenderMethodExtern.object_change_color_primary);
                        result.AddFloat3ColorParameter("secondary_change_color", RenderMethodExtern.object_change_color_secondary);
                        result.AddFloat4Parameter("primary_change_color_anim", RenderMethodExtern.object_change_color_primary_anim);
                        result.AddFloat4Parameter("secondary_change_color_anim", RenderMethodExtern.object_change_color_secondary_anim);
                        rmopName = @"shaders\shader_options\albedo_two_change_color_anim_overlay";
                        break;
                    case Albedo.Chameleon:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerParameter("detail_map");
                        result.AddFloat4ColorParameter("chameleon_color0");
                        result.AddFloat4ColorParameter("chameleon_color1");
                        result.AddFloat4ColorParameter("chameleon_color2");
                        result.AddFloat4ColorParameter("chameleon_color3");
                        result.AddFloatParameter("chameleon_color_offset1");
                        result.AddFloatParameter("chameleon_color_offset2");
                        result.AddFloatParameter("chameleon_fresnel_power");
                        rmopName = @"shaders\shader_options\albedo_chameleon";
                        break;
                    case Albedo.Two_Change_Color_Chameleon:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerParameter("detail_map");
                        result.AddSamplerParameter("change_color_map");
                        result.AddFloat3ColorParameter("primary_change_color", RenderMethodExtern.object_change_color_primary);
                        result.AddFloat3ColorParameter("secondary_change_color", RenderMethodExtern.object_change_color_secondary);
                        result.AddFloat4ColorParameter("chameleon_color0");
                        result.AddFloat4ColorParameter("chameleon_color1");
                        result.AddFloat4ColorParameter("chameleon_color2");
                        result.AddFloat4ColorParameter("chameleon_color3");
                        result.AddFloatParameter("chameleon_color_offset1");
                        result.AddFloatParameter("chameleon_color_offset2");
                        result.AddFloatParameter("chameleon_fresnel_power");
                        rmopName = @"shaders\shader_options\albedo_two_change_color_chameleon";
                        break;
                    case Albedo.Chameleon_Masked:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerParameter("detail_map");
                        result.AddSamplerParameter("chameleon_mask_map");
                        result.AddFloat4ColorParameter("chameleon_color0");
                        result.AddFloat4ColorParameter("chameleon_color1");
                        result.AddFloat4ColorParameter("chameleon_color2");
                        result.AddFloat4ColorParameter("chameleon_color3");
                        result.AddFloatParameter("chameleon_color_offset1");
                        result.AddFloatParameter("chameleon_color_offset2");
                        result.AddFloatParameter("chameleon_fresnel_power");
                        rmopName = @"shaders\shader_options\albedo_chameleon_masked";
                        break;
                    case Albedo.Color_Mask_Hard_Light:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerParameter("detail_map");
                        result.AddSamplerParameter("color_mask_map");
                        result.AddFloat4ColorParameter("albedo_color");
                        rmopName = @"shaders\shader_options\albedo_color_mask_hard_light";
                        break;
                    case Albedo.Simple:
                        result.AddSamplerParameter("base_map");
                        result.AddFloat4Parameter("albedo_color");
                        rmopName = @"shaders\shader_options\albedo_simple";
                        break;
                    case Albedo.Two_Change_Color_Tex_Overlay:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerParameter("detail_map");
                        result.AddSamplerParameter("change_color_map");
                        result.AddFloat3ColorParameter("primary_change_color", RenderMethodExtern.object_change_color_primary);
                        result.AddSamplerParameter("secondary_change_color_map"); // might be without xform
                        rmopName = @"shaders\shader_options\albedo_two_change_color_tex_overlay";
                        break;
                    case Albedo.Chameleon_Albedo_Masked:
                        result.AddSamplerParameter("base_map");
                        result.AddFloat4Parameter("albedo_color");
                        result.AddSamplerParameter("base_masked_map");
                        result.AddFloat4Parameter("albedo_masked_color");
                        result.AddSamplerParameter("chameleon_mask_map");
                        result.AddFloat4ColorParameter("chameleon_color0");
                        result.AddFloat4ColorParameter("chameleon_color1");
                        result.AddFloat4ColorParameter("chameleon_color2");
                        result.AddFloat4ColorParameter("chameleon_color3");
                        result.AddFloatParameter("chameleon_color_offset1");
                        result.AddFloatParameter("chameleon_color_offset2");
                        result.AddFloatParameter("chameleon_fresnel_power");
                        rmopName = @"shaders\shader_options\albedo_chameleon_albedo_masked";
                        break;
                    case Albedo.Custom_Cube:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerWithoutXFormParameter("custom_cube");
                        result.AddFloat4Parameter("albedo_color");
                        rmopName = @"shaders\shader_options\albedo_custom_cube";
                        break;
                    case Albedo.Two_Color:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerParameter("detail_map");
                        result.AddFloat4Parameter("albedo_color");
                        result.AddSamplerWithoutXFormParameter("blend_map");
                        result.AddFloat4Parameter("albedo_second_color");
                        rmopName = @"shaders\shader_options\albedo_two_color";
                        break;
                    case Albedo.Emblem:
                        result.AddSamplerWithoutXFormParameter("emblem_map", RenderMethodExtern.emblem_player_shoulder_texture);
                        rmopName = @"shaders\shader_options\albedo_emblem";
                        break;
                        //case Albedo.Scrolling_Cube_Mask:
                        //    result.AddSamplerParameter("base_map");
                        //    result.AddSamplerParameter("detail_map");
                        //    result.AddFloat4Parameter("albedo_color");
                        //    result.AddSamplerWithoutXFormParameter("color_blend_mask_cubemap");
                        //    result.AddFloat4Parameter("albedo_second_color");
                        //    rmopName = @"shaders\shader_options\albedo_scrolling_cube_mask";
                        //    break;
                        //case Albedo.Scrolling_Cube:
                        //    result.AddSamplerParameter("base_map");
                        //    result.AddSamplerParameter("detail_map");
                        //    result.AddSamplerWithoutXFormParameter("color_cubemap");
                        //    rmopName = @"shaders\shader_options\albedo_scrolling_cube";
                        //    break;
                        //case Albedo.Scrolling_Texture_Uv:
                        //    result.AddSamplerParameter("base_map");
                        //    result.AddSamplerWithoutXFormParameter("color_texture");
                        //    result.AddFloatParameter("u_speed");
                        //    result.AddFloatParameter("v_speed");
                        //    rmopName = @"shaders\shader_options\albedo_scrolling_texture_uv";
                        //    break;
                        //case Albedo.Texture_From_Misc:
                        //    result.AddSamplerParameter("base_map");
                        //    result.AddSamplerWithoutXFormParameter("color_texture");
                        //    rmopName = @"shaders\shader_options\albedo_texture_from_misc";
                        //    break;
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
                    case Bump_Mapping.Detail_Masked:
                        result.AddSamplerParameter("bump_map");
                        result.AddSamplerParameter("bump_detail_map");
                        result.AddSamplerParameter("bump_detail_mask_map");
                        result.AddFloatParameter("bump_detail_coefficient");
                        result.AddBooleanParameter("invert_mask");
                        rmopName = @"shaders\shader_options\bump_detail_masked";
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
            if (methodName == "specular_mask")
            {
                optionName = ((Specular_Mask)option).ToString();

                switch ((Specular_Mask)option)
                {
                    case Specular_Mask.Specular_Mask_From_Texture:
                    case Specular_Mask.Specular_Mask_From_Color_Texture:
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
                    case Material_Model.Diffuse_Only:
                        result.AddBooleanParameter("no_dynamic_lights");
                        rmopName = @"shaders\shader_options\material_diffuse_only";
                        break;
                    case Material_Model.Cook_Torrance_Odst:
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
                    case Material_Model.Two_Lobe_Phong:
                        result.AddFloatParameter("diffuse_coefficient");
                        result.AddFloatParameter("specular_coefficient");
                        result.AddFloatParameter("normal_specular_power");
                        result.AddFloat3ColorParameter("normal_specular_tint");
                        result.AddFloatParameter("glancing_specular_power");
                        result.AddFloat3ColorParameter("glancing_specular_tint");
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
                    case Material_Model.Foliage:
                        result.AddBooleanParameter("no_dynamic_lights");
                        rmopName = @"shaders\shader_options\material_foliage";
                        break;
                    case Material_Model.Glass:
                        result.AddFloatParameter("diffuse_coefficient");
                        result.AddFloatParameter("specular_coefficient");
                        result.AddFloatParameter("fresnel_coefficient");
                        result.AddFloatParameter("fresnel_curve_steepness");
                        result.AddFloatParameter("fresnel_curve_bias");
                        result.AddFloatParameter("roughness");
                        result.AddFloatParameter("analytical_specular_contribution");
                        result.AddFloatParameter("area_specular_contribution");
                        result.AddBooleanParameter("no_dynamic_lights");
                        rmopName = @"shaders\shader_options\glass_material";
                        break;
                    case Material_Model.Organism:
                        result.AddFloatParameter("diffuse_coefficient");
                        result.AddFloat3ColorParameter("diffuse_tint");
                        result.AddFloatParameter("analytical_specular_coefficient");
                        result.AddFloatParameter("area_specular_coefficient");
                        result.AddFloat3ColorParameter("specular_tint");
                        result.AddFloatParameter("specular_power");
                        result.AddSamplerParameter("specular_map");
                        result.AddFloatParameter("environment_map_coefficient");
                        result.AddFloat3ColorParameter("environment_map_tint");
                        result.AddFloatParameter("fresnel_curve_steepness");
                        result.AddFloatParameter("rim_coefficient");
                        result.AddFloat3ColorParameter("rim_tint");
                        result.AddFloatParameter("rim_power");
                        result.AddFloatParameter("rim_start");
                        result.AddFloatParameter("rim_maps_transition_ratio");
                        result.AddFloatParameter("ambient_coefficient");
                        result.AddFloat3ColorParameter("ambient_tint");
                        result.AddSamplerParameter("occlusion_parameter_map");
                        result.AddFloatParameter("subsurface_coefficient");
                        result.AddFloat3ColorParameter("subsurface_tint");
                        result.AddFloatParameter("subsurface_propagation_bias");
                        result.AddFloatParameter("subsurface_normal_detail");
                        result.AddSamplerParameter("subsurface_map");
                        result.AddFloatParameter("transparence_coefficient");
                        result.AddFloat3ColorParameter("transparence_tint");
                        result.AddFloatParameter("transparence_normal_bias");
                        result.AddFloatParameter("transparence_normal_detail");
                        result.AddSamplerParameter("transparence_map");
                        result.AddFloat3ColorParameter("final_tint");
                        result.AddBooleanParameter("no_dynamic_lights");
                        rmopName = @"shaders\shader_options\material_organism_option";
                        break;
                    case Material_Model.Single_Lobe_Phong:
                        result.AddFloatParameter("diffuse_coefficient");
                        result.AddFloatParameter("specular_coefficient");
                        result.AddFloatParameter("roughness");
                        result.AddFloatParameter("analytical_specular_contribution");
                        result.AddFloatParameter("area_specular_contribution");
                        result.AddFloatParameter("environment_map_specular_contribution");
                        result.AddFloat3ColorParameter("specular_tint");
                        result.AddBooleanParameter("order3_area_specular");
                        result.AddBooleanParameter("no_dynamic_lights");
                        rmopName = @"shaders\shader_options\single_lobe_phong";
                        break;
                    case Material_Model.Car_Paint:
                        result.AddBooleanParameter("use_material_texture0");
                        result.AddBooleanParameter("use_material_texture1");
                        result.AddSamplerParameter("material_texture");
                        result.AddBooleanParameter("no_dynamic_lights");
                        result.AddSamplerWithoutXFormParameter("g_sampler_cc0236", RenderMethodExtern.texture_cook_torrance_cc0236);
                        result.AddSamplerWithoutXFormParameter("g_sampler_dd0236", RenderMethodExtern.texture_cook_torrance_dd0236);
                        result.AddSamplerWithoutXFormParameter("g_sampler_c78d78", RenderMethodExtern.texture_cook_torrance_c78d78);
                        result.AddSamplerParameter("bump_detail_map0");
                        result.AddFloatParameter("bump_detail_map0_blend_factor");
                        result.AddFloatParameter("diffuse_coefficient0");
                        result.AddFloatParameter("specular_coefficient0");
                        result.AddFloat3ColorParameter("specular_tint0");
                        result.AddFloat3ColorParameter("fresnel_color0");
                        result.AddFloatParameter("fresnel_power0");
                        result.AddFloatParameter("albedo_blend0");
                        result.AddFloatParameter("roughness0");
                        result.AddFloatParameter("area_specular_contribution0");
                        result.AddFloatParameter("analytical_specular_contribution0");
                        result.AddBooleanParameter("order3_area_specular0");
                        result.AddFloatParameter("diffuse_coefficient1");
                        result.AddFloatParameter("specular_coefficient1");
                        result.AddFloat3ColorParameter("specular_tint1");
                        result.AddFloat3ColorParameter("fresnel_color1");
                        result.AddFloat3ColorParameter("fresnel_color_environment1");
                        result.AddFloatParameter("fresnel_power1");
                        result.AddFloatParameter("albedo_blend1");
                        result.AddFloatParameter("roughness1");
                        result.AddFloatParameter("area_specular_contribution1");
                        result.AddFloatParameter("analytical_specular_contribution1");
                        result.AddFloatParameter("environment_map_specular_contribution1");
                        result.AddBooleanParameter("order3_area_specular1");
                        result.AddFloatParameter("rim_fresnel_coefficient1");
                        result.AddFloat3ColorParameter("rim_fresnel_color1");
                        result.AddFloatParameter("rim_fresnel_power1");
                        result.AddFloatParameter("rim_fresnel_albedo_blend1");
                        rmopName = @"shaders\shader_options\material_car_paint";
                        break;
                    case Material_Model.Hair:
                        result.AddFloatParameter("diffuse_coefficient");
                        result.AddFloat3ColorParameter("diffuse_tint");
                        result.AddFloatParameter("analytical_specular_coefficient");
                        result.AddFloatParameter("area_specular_coefficient");
                        result.AddFloat3ColorParameter("specular_tint");
                        result.AddFloatParameter("specular_power");
                        result.AddSamplerWithoutXFormParameter("specular_map");
                        result.AddSamplerWithoutXFormParameter("specular_shift_map");
                        result.AddSamplerWithoutXFormParameter("specular_noise_map");
                        result.AddFloatParameter("environment_map_coefficient");
                        result.AddFloat3ColorParameter("environment_map_tint");
                        result.AddFloat3ColorParameter("final_tint");
                        result.AddBooleanParameter("no_dynamic_lights");
                        rmopName = @"shaders\shader_options\material_hair_option";
                        break;
                }
            }
            if (methodName == "environment_mapping")
            {
                optionName = ((Environment_Mapping)option).ToString();

                switch ((Environment_Mapping)option)
                {
                    case Environment_Mapping.Per_Pixel:
                    case Environment_Mapping.Custom_Map:
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
                    case Environment_Mapping.From_Flat_Texture:
                        result.AddSamplerWithoutXFormParameter("flat_environment_map");
                        result.AddFloat3ColorParameter("env_tint_color");
                        result.AddFloat3Parameter("flat_envmap_matrix_x", RenderMethodExtern.flat_envmap_matrix_x);
                        result.AddFloat3Parameter("flat_envmap_matrix_y", RenderMethodExtern.flat_envmap_matrix_y);
                        result.AddFloat3Parameter("flat_envmap_matrix_z", RenderMethodExtern.flat_envmap_matrix_z);
                        result.AddFloatParameter("hemisphere_percentage");
                        result.AddFloat4Parameter("env_bloom_override");
                        result.AddFloatParameter("env_bloom_override_intensity");
                        rmopName = @"shaders\shader_options\env_map_from_flat_texture";
                        break;
                }
            }
            if (methodName == "self_illumination")
            {
                optionName = ((Self_Illumination)option).ToString();

                switch ((Self_Illumination)option)
                {
                    case Self_Illumination.Simple:
                    case Self_Illumination.Simple_With_Alpha_Mask:
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
                    case Self_Illumination.Simple_Four_Change_Color:
                        result.AddSamplerParameter("self_illum_map");
                        result.AddFloat4Parameter("self_illum_color", RenderMethodExtern.object_change_color_quaternary);
                        result.AddFloatParameter("self_illum_intensity");
                        rmopName = @"shaders\shader_options\illum_simple_four_change_color";
                        break;
                    case Self_Illumination.Illum_Change_Color:
                        result.AddSamplerParameter("self_illum_map");
                        result.AddFloat4Parameter("self_illum_color", RenderMethodExtern.object_change_color_primary);
                        result.AddFloatParameter("self_illum_intensity");
                        result.AddFloatParameter("primary_change_color_blend");
                        rmopName = @"shaders\shader_options\illum_change_color";
                        break;
                    case Self_Illumination.Multilayer_Additive:
                        result.AddSamplerParameter("self_illum_map");
                        result.AddFloat4Parameter("self_illum_color");
                        result.AddFloatParameter("self_illum_intensity");
                        result.AddFloatParameter("layer_depth");
                        result.AddFloatParameter("layer_contrast");
                        result.AddFloatParameter("layers_of_4");
                        result.AddIntegerParameter("layers_of_4");
                        result.AddFloatParameter("texcoord_aspect_ratio");
                        result.AddFloatParameter("depth_darken");
                        rmopName = @"sshaders\shader_options\illum_multilayer";
                        break;
                    case Self_Illumination.Palettized_Plasma:
                        result.AddSamplerParameter("noise_map_a");
                        result.AddSamplerParameter("noise_map_b");
                        result.AddSamplerWithoutXFormParameter("palette");
                        result.AddSamplerParameter("alpha_mask_map");
                        result.AddFloatParameter("alpha_modulation_factor");
                        result.AddSamplerParameter("depth_buffer", RenderMethodExtern.texture_global_target_z);
                        result.AddFloatParameter("depth_fade_range");
                        result.AddFloat4Parameter("self_illum_color");
                        result.AddFloatParameter("self_illum_intensity");
                        result.AddFloatParameter("v_coordinate");
                        result.AddFloat3Parameter("global_depth_constants", RenderMethodExtern.global_depth_constants);
                        result.AddFloat3Parameter("global_camera_forward", RenderMethodExtern.global_camera_forward);
                        rmopName = @"shaders\shader_options\illum_palettized_plasma";
                        break;
                    case Self_Illumination.Change_Color_Detail:
                        result.AddSamplerParameter("self_illum_map");
                        result.AddSamplerParameter("self_illum_detail_map");
                        result.AddFloat4Parameter("self_illum_color", RenderMethodExtern.object_change_color_primary);
                        result.AddFloatParameter("self_illum_intensity");
                        rmopName = @"shaders\shader_options\illum_change_color_detail";
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
            if (methodName == "distortion")
            {
                optionName = ((Shared.Distortion)option).ToString();

                switch ((Shared.Distortion)option)
                {
                    case Shared.Distortion.On:
                        result.AddSamplerParameter("distort_map");
                        result.AddFloatParameter("distort_scale");
                        //result.AddFloatParameter("distort_fadeoff");
                        //result.AddBooleanParameter("distort_selfonly");
                        rmopName = @"shaders\shader_options\sfx_distort";
                        break;
                }
            }
            if (methodName == "soft_fade")
            {
                optionName = ((Shared.Soft_Fade)option).ToString();

                switch ((Shared.Soft_Fade)option)
                {
                    case Shared.Soft_Fade.On:
                        result.AddBooleanParameter("soft_fresnel_enabled");
                        result.AddFloatParameter("soft_fresnel_power");
                        result.AddBooleanParameter("soft_z_enabled");
                        result.AddFloatParameter("soft_z_range");
                        rmopName = @"shaders\shader_options\soft_fade";
                        break;
                }
            }

            return result;
        }

        public Array GetMethodNames()
        {
            return Enum.GetValues(typeof(ShaderMethods));
        }

        public Array GetMethodOptionNames(int methodIndex)
        {
            switch ((ShaderMethods)methodIndex)
            {
                case ShaderMethods.Albedo:
                    return Enum.GetValues(typeof(Albedo));
                case ShaderMethods.Bump_Mapping:
                    return Enum.GetValues(typeof(Bump_Mapping));
                case ShaderMethods.Alpha_Test:
                    return Enum.GetValues(typeof(Alpha_Test));
                case ShaderMethods.Specular_Mask:
                    return Enum.GetValues(typeof(Specular_Mask));
                case ShaderMethods.Material_Model:
                    return Enum.GetValues(typeof(Material_Model));
                case ShaderMethods.Environment_Mapping:
                    return Enum.GetValues(typeof(Environment_Mapping));
                case ShaderMethods.Self_Illumination:
                    return Enum.GetValues(typeof(Self_Illumination));
                case ShaderMethods.Blend_Mode:
                    return Enum.GetValues(typeof(Blend_Mode));
                case ShaderMethods.Parallax:
                    return Enum.GetValues(typeof(Parallax));
                case ShaderMethods.Misc:
                    return Enum.GetValues(typeof(Misc));
                case ShaderMethods.Distortion:
                    return Enum.GetValues(typeof(Shared.Distortion));
                case ShaderMethods.Soft_Fade:
                    return Enum.GetValues(typeof(Shared.Soft_Fade));
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
