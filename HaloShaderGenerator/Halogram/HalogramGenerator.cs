using System;
using System.Collections.Generic;
using HaloShaderGenerator.DirectX;
using HaloShaderGenerator.Generator;
using HaloShaderGenerator.Globals;
using HaloShaderGenerator.TemplateGenerator;

namespace HaloShaderGenerator.Halogram
{
    public class HalogramGenerator : IShaderGenerator
    {
        private bool TemplateGenerationValid;
        private bool ApplyFixes;

        Albedo albedo;
        Self_Illumination self_illumination;
        Blend_Mode blend_mode;
        Misc misc;
        Warp warp;
        Overlay overlay;
        Edge_Fade edge_fade;
        Shared.Distortion distortion;
        Shared.Soft_Fade soft_fade;

        /// <summary>
        /// Generator insantiation for shared shaders. Does not require method options.
        /// </summary>
        public HalogramGenerator(bool applyFixes = false) { TemplateGenerationValid = false; ApplyFixes = applyFixes; }

        /// <summary>
        /// Generator instantiation for method specific shaders.
        /// </summary>
        public HalogramGenerator(Albedo albedo, Self_Illumination self_illumination, Blend_Mode blend_mode, Misc misc, Warp warp, Overlay overlay, 
            Edge_Fade edge_fade, Shared.Distortion distortion, Shared.Soft_Fade soft_fade, bool applyFixes = false)
        {
            this.albedo = albedo;
            this.self_illumination = self_illumination;
            this.blend_mode = blend_mode;
            this.misc = misc;
            this.warp = warp;
            this.overlay = overlay;
            this.edge_fade = edge_fade;
            this.distortion = distortion;
            this.soft_fade = soft_fade;

            ApplyFixes = applyFixes;
            TemplateGenerationValid = true;
        }

        public HalogramGenerator(byte[] options, bool applyFixes = false)
        {
            options = ValidateOptions(options);

            this.albedo = (Albedo)options[0];
            this.self_illumination = (Self_Illumination)options[1];
            this.blend_mode = (Blend_Mode)options[2];
            this.misc = (Misc)options[3];
            this.warp = (Warp)options[4];
            this.overlay = (Overlay)options[5];
            this.edge_fade = (Edge_Fade)options[6];
            this.distortion = (Shared.Distortion)options[7];
            this.soft_fade = (Shared.Soft_Fade)options[8];

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
            var sSelfIllumination = Enum.Parse(typeof(Shared.Self_Illumination), self_illumination.ToString());
            var sBlendMode = Enum.Parse(typeof(Shared.Blend_Mode), blend_mode.ToString());

            TemplateGenerator.TemplateGenerator.CreateGlobalMacros(macros, ShaderType.Halogram, entryPoint, (Shared.Blend_Mode)sBlendMode, 
                (Shader.Misc)misc, Shared.Alpha_Test.None, Shared.Alpha_Blend_Source.Albedo_Alpha_Without_Fresnel, ApplyFixes);

            //
            // The following code properly names the macros (like in rmdf)
            //

            macros.Add(ShaderGeneratorBase.CreateMacro("calc_albedo_ps", sAlbedo, "calc_albedo_", "_ps"));
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_albedo_vs", sAlbedo, "calc_albedo_", "_vs"));

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
                case Shared.Self_Illumination.Multilayer_Additive:
                case Shared.Self_Illumination.Ml_Add_Four_Change_Color:
                case Shared.Self_Illumination.Ml_Add_Five_Change_Color:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_self_illumination_ps", "multilayer", "calc_self_illumination_", "_ps"));
                    break;
                default:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_self_illumination_ps", sSelfIllumination, "calc_self_illumination_", "_ps"));
                    break;
            }

            macros.Add(ShaderGeneratorBase.CreateMacro("distort_proc_ps", distortion, "distort_", "_ps"));
            macros.Add(ShaderGeneratorBase.CreateMacro("blend_type", blend_mode));

            switch (warp)
            {
                case Warp.None:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_parallax_ps", "calc_parallax_off_ps"));
                    break;
                case Warp.From_Texture:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_parallax_ps", "calc_warp_from_texture_ps"));
                    break;
                case Warp.Parallax_Simple:
                    macros.Add(ShaderGeneratorBase.CreateMacro("calc_parallax_ps", "calc_parallax_simple_ps"));
                    break;
            }

            macros.Add(ShaderGeneratorBase.CreateMacro("calc_overlay_ps", overlay, "calc_overlay_", "_ps"));
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_edge_fade_ps", edge_fade, "calc_edge_fade_", "_ps"));

            macros.Add(ShaderGeneratorBase.CreateMacro("material_type", "none"));
            macros.Add(ShaderGeneratorBase.CreateMacro("envmap_type", "none"));

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

            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource($"halogram.fx", macros, entryName, "ps_3_0");

            return new ShaderGeneratorResult(shaderBytecode);
        }

        public ShaderGeneratorResult GenerateSharedPixelShader(ShaderStage entryPoint, int methodIndex, int optionIndex)
        {
            if (!IsEntryPointSupported(entryPoint) || !IsPixelShaderShared(entryPoint))
                return null;

            List<D3D.SHADER_MACRO> macros = new List<D3D.SHADER_MACRO>();

            macros.Add(new D3D.SHADER_MACRO { Name = "_DEFINITION_HELPER_HLSLI", Definition = "1" });

            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource($"glps_halogram.hlsl", macros, "entry_" + entryPoint.ToString().ToLower(), "ps_3_0");

            return new ShaderGeneratorResult(shaderBytecode);
        }

        public ShaderGeneratorResult GenerateSharedVertexShader(VertexType vertexType, ShaderStage entryPoint)
        {
            if (!IsVertexFormatSupported(vertexType) || !IsEntryPointSupported(entryPoint))
                return null;

            List<D3D.SHADER_MACRO> macros = new List<D3D.SHADER_MACRO>();

            TemplateGenerator.TemplateGenerator.CreateGlobalMacros(macros, Globals.ShaderType.Halogram, entryPoint,
                Shared.Blend_Mode.Opaque, Shader.Misc.First_Person_Never, Shared.Alpha_Test.None, Shared.Alpha_Blend_Source.Albedo_Alpha_Without_Fresnel, false, true, vertexType);

            macros.Add(ShaderGeneratorBase.CreateMacro("calc_albedo_ps", Albedo.Default, "calc_albedo_", "_ps"));
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_albedo_vs", Albedo.Default, "calc_albedo_", "_vs"));

            macros.Add(ShaderGeneratorBase.CreateMacro("calc_self_illumination_ps", "calc_self_illumination_none_ps"));
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_parallax_ps", "calc_parallax_off_ps"));
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_overlay_ps", "calc_overlay_none_ps"));
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_edge_fade_ps", "calc_edge_fade_none_ps"));

            macros.Add(ShaderGeneratorBase.CreateMacro("distort_proc_ps", Shared.Distortion.Off, "distort_", "_ps"));
            macros.Add(ShaderGeneratorBase.CreateMacro("distort_proc_vs", "distort_nocolor_vs"));

            macros.Add(ShaderGeneratorBase.CreateMacro("blend_type", Blend_Mode.Opaque));

            string entryName = TemplateGenerator.TemplateGenerator.GetEntryName(entryPoint, true);
            string filename = TemplateGenerator.TemplateGenerator.GetSourceFilename(Globals.ShaderType.Halogram);
            byte[] bytecode = ShaderGeneratorBase.GenerateSource(filename, macros, entryName, "vs_3_0");

            return new ShaderGeneratorResult(bytecode);
        }

        public ShaderGeneratorResult GenerateVertexShader(VertexType vertexType, ShaderStage entryPoint)
        {
            if (!TemplateGenerationValid)
                throw new System.Exception("Generator initialized with shared shader constructor. Use template constructor.");
            return null;
        }

        public int GetMethodCount()
        {
            return System.Enum.GetValues(typeof(HalogramMethods)).Length;
        }

        public int GetMethodOptionCount(int methodIndex)
        {
            switch ((HalogramMethods)methodIndex)
            {
                case HalogramMethods.Albedo:
                    return Enum.GetValues(typeof(Albedo)).Length;
                case HalogramMethods.Self_Illumination:
                    return Enum.GetValues(typeof(Self_Illumination)).Length;
                case HalogramMethods.Blend_Mode:
                    return Enum.GetValues(typeof(Blend_Mode)).Length;
                case HalogramMethods.Misc:
                    return Enum.GetValues(typeof(Misc)).Length;
                case HalogramMethods.Warp:
                    return Enum.GetValues(typeof(Warp)).Length;
                case HalogramMethods.Overlay:
                    return Enum.GetValues(typeof(Overlay)).Length;
                case HalogramMethods.Edge_Fade:
                    return Enum.GetValues(typeof(Edge_Fade)).Length;
                case HalogramMethods.Distortion:
                    return Enum.GetValues(typeof(Shared.Distortion)).Length;
                case HalogramMethods.Soft_Fade:
                    return Enum.GetValues(typeof(Shared.Soft_Fade)).Length;
            }

            return -1;
        }

        public int GetMethodOptionValue(int methodIndex)
        {
            switch ((HalogramMethods)methodIndex)
            {
                case HalogramMethods.Albedo:
                    return (int)albedo;
                case HalogramMethods.Self_Illumination:
                    return (int)self_illumination;
                case HalogramMethods.Blend_Mode:
                    return (int)blend_mode;
                case HalogramMethods.Misc:
                    return (int)misc;
                case HalogramMethods.Warp:
                    return (int)warp;
                case HalogramMethods.Overlay:
                    return (int)overlay;
                case HalogramMethods.Edge_Fade:
                    return (int)edge_fade;
                case HalogramMethods.Distortion:
                    return (int)distortion;
                case HalogramMethods.Soft_Fade:
                    return (int)soft_fade;
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
            return entryPoint == ShaderStage.Shadow_Generate;
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
            }

            return false;
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
                case Self_Illumination.Ml_Add_Four_Change_Color:
                    result.AddSamplerParameter("self_illum_map");
                    result.AddFloat4Parameter("self_illum_color", RenderMethodExtern.object_change_color_quaternary);
                    result.AddFloatParameter("self_illum_intensity");
                    result.AddFloatParameter("layer_depth");
                    result.AddFloatParameter("layer_contrast");
                    result.AddFloatParameter("layers_of_4");
                    result.AddIntegerParameter("layers_of_4");
                    result.AddFloatParameter("texcoord_aspect_ratio");
                    result.AddFloatParameter("depth_darken");
                    break;
                case Self_Illumination.Ml_Add_Five_Change_Color:
                    result.AddSamplerParameter("self_illum_map");
                    result.AddFloat4Parameter("self_illum_color", RenderMethodExtern.object_change_color_quinary);
                    result.AddFloatParameter("self_illum_intensity");
                    result.AddFloatParameter("layer_depth");
                    result.AddFloatParameter("layer_contrast");
                    result.AddFloatParameter("layers_of_4");
                    result.AddIntegerParameter("layers_of_4");
                    result.AddFloatParameter("texcoord_aspect_ratio");
                    result.AddFloatParameter("depth_darken");
                    break;
                case Self_Illumination.Scope_Blur:
                    result.AddSamplerParameter("self_illum_map");
                    result.AddFloat4Parameter("self_illum_color");
                    result.AddFloatParameter("self_illum_intensity");
                    result.AddFloat4Parameter("self_illum_heat_color");
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
                case Self_Illumination.Palettized_Plasma_Change_Color:
                    result.AddSamplerParameter("noise_map_a");
                    result.AddSamplerParameter("noise_map_b");
                    result.AddSamplerWithoutXFormParameter("palette");
                    result.AddSamplerParameter("alpha_mask_map");
                    result.AddFloatParameter("alpha_modulation_factor");
                    result.AddSamplerParameter("depth_buffer", RenderMethodExtern.texture_global_target_z);
                    result.AddFloatParameter("depth_fade_range");
                    result.AddFloat4Parameter("self_illum_color", RenderMethodExtern.object_change_color_primary);
                    result.AddFloatParameter("self_illum_intensity");
                    result.AddFloatParameter("v_coordinate");
                    result.AddFloat3Parameter("global_depth_constants", RenderMethodExtern.global_depth_constants);
                    result.AddFloat3Parameter("global_camera_forward", RenderMethodExtern.global_camera_forward);
                    break;
                case Self_Illumination.Palettized_Depth_Fade:
                    result.AddSamplerParameter("noise_map_a");
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
            }
            switch (warp)
            {
                case Warp.From_Texture:
                    result.AddSamplerParameter("warp_map");
                    result.AddFloatParameter("warp_amount_x");
                    result.AddFloatParameter("warp_amount_y");
                    break;
            }
            switch (overlay)
            {
                case Overlay.Additive:
                    result.AddSamplerParameter("overlay_map");
                    result.AddFloat4Parameter("overlay_tint");
                    result.AddFloatParameter("overlay_intensity");
                    break;
                case Overlay.Additive_Detail:
                    result.AddSamplerParameter("overlay_map");
                    result.AddSamplerParameter("overlay_detail_map");
                    result.AddFloat4Parameter("overlay_tint");
                    result.AddFloatParameter("overlay_intensity");
                    break;
                case Overlay.Multiply:
                    result.AddSamplerParameter("overlay_map");
                    result.AddFloat4Parameter("overlay_tint");
                    result.AddFloatParameter("overlay_intensity");
                    break;
                case Overlay.Multiply_And_Additive_Detail:
                    result.AddSamplerParameter("overlay_multiply_map");
                    result.AddSamplerParameter("overlay_map");
                    result.AddSamplerParameter("overlay_detail_map");
                    result.AddFloat4Parameter("overlay_tint");
                    result.AddFloatParameter("overlay_intensity");
                    break;
            }
            switch (edge_fade)
            {
                case Edge_Fade.Simple:
                    result.AddFloat4Parameter("edge_fade_edge_tint");
                    result.AddFloat4Parameter("edge_fade_center_tint");
                    result.AddFloatParameter("edge_fade_power");
                    break;
            }

            switch (distortion)
            {
                case Shared.Distortion.On:
                    result.AddSamplerParameter("distort_map");
                    result.AddFloatParameter("distort_scale");
                    result.AddFloatParameter("distort_fadeoff");
                    result.AddBooleanParameter("distort_selfonly");
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
                    case Self_Illumination.Ml_Add_Four_Change_Color:
                        result.AddSamplerParameter("self_illum_map");
                        result.AddFloat4Parameter("self_illum_color", RenderMethodExtern.object_change_color_quaternary);
                        result.AddFloatParameter("self_illum_intensity");
                        result.AddFloatParameter("layer_depth");
                        result.AddFloatParameter("layer_contrast");
                        result.AddFloatParameter("layers_of_4");
                        result.AddIntegerParameter("layers_of_4");
                        result.AddFloatParameter("texcoord_aspect_ratio");
                        result.AddFloatParameter("depth_darken");
                        rmopName = @"shaders\shader_options\illum_ml_add_four_change_color";
                        break;
                    case Self_Illumination.Ml_Add_Five_Change_Color:
                        result.AddSamplerParameter("self_illum_map");
                        result.AddFloat4Parameter("self_illum_color", RenderMethodExtern.object_change_color_quinary);
                        result.AddFloatParameter("self_illum_intensity");
                        result.AddFloatParameter("layer_depth");
                        result.AddFloatParameter("layer_contrast");
                        result.AddFloatParameter("layers_of_4");
                        result.AddIntegerParameter("layers_of_4");
                        result.AddFloatParameter("texcoord_aspect_ratio");
                        result.AddFloatParameter("depth_darken");
                        rmopName = @"shaders\shader_options\illum_ml_add_five_change_color";
                        break;
                    case Self_Illumination.Scope_Blur:
                        result.AddSamplerParameter("self_illum_map");
                        result.AddFloat4Parameter("self_illum_color");
                        result.AddFloatParameter("self_illum_intensity");
                        result.AddFloat4Parameter("self_illum_heat_color");
                        rmopName = @"shaders\shader_options\illum_scope_blur";
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
                    case Self_Illumination.Palettized_Plasma_Change_Color:
                        result.AddSamplerParameter("noise_map_a");
                        result.AddSamplerParameter("noise_map_b");
                        result.AddSamplerWithoutXFormParameter("palette");
                        result.AddSamplerParameter("alpha_mask_map");
                        result.AddFloatParameter("alpha_modulation_factor");
                        result.AddSamplerParameter("depth_buffer", RenderMethodExtern.texture_global_target_z);
                        result.AddFloatParameter("depth_fade_range");
                        result.AddFloat4Parameter("self_illum_color", RenderMethodExtern.object_change_color_primary);
                        result.AddFloatParameter("self_illum_intensity");
                        result.AddFloatParameter("v_coordinate");
                        result.AddFloat3Parameter("global_depth_constants", RenderMethodExtern.global_depth_constants);
                        result.AddFloat3Parameter("global_camera_forward", RenderMethodExtern.global_camera_forward);
                        rmopName = @"shaders\screen_options\illum_palettized_plasma_change_color";
                        break;
                    case Self_Illumination.Palettized_Depth_Fade:
                        result.AddSamplerParameter("noise_map_a");
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
                        rmopName = @"shaders\shader_options\illum_palettized_depth_fade";
                        break;
                }
            }
            if (methodName == "blend_mode")
            {
                optionName = ((Blend_Mode)option).ToString();
            }
            if (methodName == "misc")
            {
                optionName = ((Misc)option).ToString();
            }
            if (methodName == "warp")
            {
                optionName = ((Warp)option).ToString();

                switch ((Warp)option)
                {
                    case Warp.From_Texture:
                        result.AddSamplerParameter("warp_map");
                        result.AddFloatParameter("warp_amount_x");
                        result.AddFloatParameter("warp_amount_y");
                        rmopName = @"shaders\shader_options\warp_from_texture";
                        break;
                }
            }
            if (methodName == "overlay")
            {
                optionName = ((Overlay)option).ToString();

                switch ((Overlay)option)
                {
                    case Overlay.Additive:
                        result.AddSamplerParameter("overlay_map");
                        result.AddFloat4Parameter("overlay_tint");
                        result.AddFloatParameter("overlay_intensity");
                        rmopName = @"shaders\shader_options\overlay_additive";
                        break;
                    case Overlay.Additive_Detail:
                        result.AddSamplerParameter("overlay_map");
                        result.AddSamplerParameter("overlay_detail_map");
                        result.AddFloat4Parameter("overlay_tint");
                        result.AddFloatParameter("overlay_intensity");
                        rmopName = @"shaders\shader_options\overlay_additive_detail";
                        break;
                    case Overlay.Multiply:
                        result.AddSamplerParameter("overlay_map");
                        result.AddFloat4Parameter("overlay_tint");
                        result.AddFloatParameter("overlay_intensity");
                        rmopName = @"shaders\shader_options\overlay_additive";
                        break;
                    case Overlay.Multiply_And_Additive_Detail:
                        result.AddSamplerParameter("overlay_multiply_map");
                        result.AddSamplerParameter("overlay_map");
                        result.AddSamplerParameter("overlay_detail_map");
                        result.AddFloat4Parameter("overlay_tint");
                        result.AddFloatParameter("overlay_intensity");
                        rmopName = @"shaders\shader_options\overlay_multiply_additive_detail";
                        break;
                }
            }
            if (methodName == "edge_fade")
            {
                optionName = ((Edge_Fade)option).ToString();

                switch ((Edge_Fade)option)
                {
                    case Edge_Fade.Simple:
                        result.AddFloat4Parameter("edge_fade_edge_tint");
                        result.AddFloat4Parameter("edge_fade_center_tint");
                        result.AddFloatParameter("edge_fade_power");
                        rmopName = @"shaders\shader_options\edge_fade_simple";
                        break;
                }
            }
            if (methodName == "distortion")
            {
                optionName = ((Shared.Distortion)option).ToString();

                switch ((Shared.Distortion)option)
                {
                    case Shared.Distortion.On:
                        result.AddSamplerParameter("distort_map");
                        result.AddFloatParameter("distort_scale");
                        result.AddFloatParameter("distort_fadeoff");
                        result.AddBooleanParameter("distort_selfonly");
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
            return Enum.GetValues(typeof(HalogramMethods));
        }

        public Array GetMethodOptionNames(int methodIndex)
        {
            switch ((HalogramMethods)methodIndex)
            {
                case HalogramMethods.Albedo:
                    return Enum.GetValues(typeof(Albedo));
                case HalogramMethods.Self_Illumination:
                    return Enum.GetValues(typeof(Self_Illumination));
                case HalogramMethods.Blend_Mode:
                    return Enum.GetValues(typeof(Blend_Mode));
                case HalogramMethods.Misc:
                    return Enum.GetValues(typeof(Misc));
                case HalogramMethods.Warp:
                    return Enum.GetValues(typeof(Warp));
                case HalogramMethods.Overlay:
                    return Enum.GetValues(typeof(Overlay));
                case HalogramMethods.Edge_Fade:
                    return Enum.GetValues(typeof(Edge_Fade));
                case HalogramMethods.Distortion:
                    return Enum.GetValues(typeof(Shared.Distortion));
                case HalogramMethods.Soft_Fade:
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
