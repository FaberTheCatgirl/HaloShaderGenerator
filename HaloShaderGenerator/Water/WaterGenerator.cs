using System.Collections.Generic;
using HaloShaderGenerator.DirectX;
using HaloShaderGenerator.Generator;
using HaloShaderGenerator.Globals;
using System;

namespace HaloShaderGenerator.Water
{
    public class WaterGenerator : IShaderGenerator
    {
        private bool TemplateGenerationValid;
        private bool ApplyFixes;

        Waveshape waveshape;
        Watercolor watercolor;
        Reflection reflection;
        Refraction refraction;
        Bankalpha bankalpha;
        Appearance appearance;
        Global_Shape global_shape;
        Foam foam;
        Reach_Compatibility reach_compatibility;

        public WaterGenerator(bool applyFixes = false) { TemplateGenerationValid = false; ApplyFixes = applyFixes; }

        public WaterGenerator(Waveshape waveshape, Watercolor watercolor, Reflection reflection, Refraction refraction, 
            Bankalpha bankalpha, Appearance appearance, Global_Shape global_shape, Foam foam, Reach_Compatibility reach_compatibility, bool applyFixes = false)
        {
            this.waveshape = waveshape;
            this.watercolor = watercolor;
            this.reflection = reflection;
            this.refraction = refraction;
            this.bankalpha = bankalpha;
            this.appearance = appearance;
            this.global_shape = global_shape;
            this.foam = foam;
            this.reach_compatibility = reach_compatibility;

            ApplyFixes = applyFixes;
            TemplateGenerationValid = true;
        }

        public WaterGenerator(byte[] options, bool applyFixes = false)
        {
            options = ValidateOptions(options);

            this.waveshape = (Waveshape)options[0];
            this.watercolor = (Watercolor)options[1];
            this.reflection = (Reflection)options[2];
            this.refraction = (Refraction)options[3];
            this.bankalpha = (Bankalpha)options[4];
            this.appearance = (Appearance)options[5];
            this.global_shape = (Global_Shape)options[6];
            this.foam = (Foam)options[7];
            this.reach_compatibility = options.Length > 8 ? (Reach_Compatibility)options[8] : Reach_Compatibility.Disabled;

            ApplyFixes = applyFixes;
            TemplateGenerationValid = true;
        }

        public ShaderGeneratorResult GeneratePixelShader(ShaderStage entryPoint)
        {
            if (!TemplateGenerationValid)
                throw new System.Exception("Generator initialized with shared shader constructor. Use template constructor.");

            List<D3D.SHADER_MACRO> macros = new List<D3D.SHADER_MACRO>();

            TemplateGenerator.TemplateGenerator.CreateGlobalMacros(macros, ShaderType.Water, entryPoint, Shared.Blend_Mode.Opaque, 
                Shader.Misc.First_Person_Never, Shared.Alpha_Test.None, ApplyFixes);

            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Waveshape>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Watercolor>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Reflection>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Refraction>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Bankalpha>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Appearance>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Global_Shape>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Foam>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Reach_Compatibility>());

            macros.Add(ShaderGeneratorBase.CreateAutoMacro("waveshape", waveshape.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("watercolor", watercolor.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("reflection", reflection.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("refraction", refraction.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("bankalpha", bankalpha.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("appearance", appearance.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("global_shape", global_shape.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("foam", foam.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("reach_compatibility", reach_compatibility.ToString().ToLower()));

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

            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource($"water.fx", macros, entryName, "ps_3_0");

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

            TemplateGenerator.TemplateGenerator.CreateGlobalMacros(macros, ShaderType.Shader, entryPoint,
                Shared.Blend_Mode.Opaque, Shader.Misc.First_Person_Never, Shared.Alpha_Test.None, false, true, vertexType);

            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Waveshape>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Watercolor>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Reflection>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Refraction>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Bankalpha>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Appearance>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Global_Shape>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Foam>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Reach_Compatibility>());

            //macros.Add(ShaderGeneratorBase.CreateAutoMacro("waveshape", waveshape.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("watercolor", Watercolor.Pure.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("reflection", Reflection.None.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("refraction", Refraction.None.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("bankalpha", Bankalpha.None.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("appearance", Appearance.Default.ToString().ToLower()));
            //macros.Add(ShaderGeneratorBase.CreateAutoMacro("global_shape", global_shape.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("foam", Foam.None.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("reach_compatibility", Reach_Compatibility.Disabled.ToString().ToLower()));

            string entryName = TemplateGenerator.TemplateGenerator.GetEntryName(entryPoint, true);
            string filename = TemplateGenerator.TemplateGenerator.GetSourceFilename(Globals.ShaderType.Water);
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
            return Enum.GetValues(typeof(WaterMethods)).Length;
        }

        public int GetMethodOptionCount(int methodIndex)
        {
            switch ((WaterMethods)methodIndex)
            {
                case WaterMethods.Waveshape:
                    return Enum.GetValues(typeof(Waveshape)).Length;
                case WaterMethods.Watercolor:
                    return Enum.GetValues(typeof(Watercolor)).Length;
                case WaterMethods.Reflection:
                    return Enum.GetValues(typeof(Reflection)).Length;
                case WaterMethods.Refraction:
                    return Enum.GetValues(typeof(Refraction)).Length;
                case WaterMethods.Bankalpha:
                    return Enum.GetValues(typeof(Bankalpha)).Length;
                case WaterMethods.Appearance:
                    return Enum.GetValues(typeof(Appearance)).Length;
                case WaterMethods.Global_Shape:
                    return Enum.GetValues(typeof(Global_Shape)).Length;
                case WaterMethods.Foam:
                    return Enum.GetValues(typeof(Foam)).Length;
                case WaterMethods.Reach_Compatibility:
                    return Enum.GetValues(typeof(Reach_Compatibility)).Length;
            }
            return -1;
        }

        public int GetMethodOptionValue(int methodIndex)
        {
            switch ((WaterMethods)methodIndex)
            {
                case WaterMethods.Waveshape:
                    return (int)waveshape;
                case WaterMethods.Watercolor:
                    return (int)watercolor;
                case WaterMethods.Reflection:
                    return (int)reflection;
                case WaterMethods.Refraction:
                    return (int)refraction;
                case WaterMethods.Bankalpha:
                    return (int)bankalpha;
                case WaterMethods.Appearance:
                    return (int)appearance;
                case WaterMethods.Global_Shape:
                    return (int)global_shape;
                case WaterMethods.Foam:
                    return (int)foam;
                case WaterMethods.Reach_Compatibility:
                    return (int)reach_compatibility;
            }
            return -1;
        }

        public bool IsEntryPointSupported(ShaderStage entryPoint)
        {
            switch (entryPoint)
            {
                case ShaderStage.Static_Per_Pixel:
                case ShaderStage.Static_Per_Vertex:
                case ShaderStage.Water_Tessellation:
                    return true;
            }
            return false;
        }

        public bool IsMethodSharedInEntryPoint(ShaderStage entryPoint, int method_index) => false;

        public bool IsSharedPixelShaderUsingMethods(ShaderStage entryPoint) => false;

        public bool IsSharedPixelShaderWithoutMethod(ShaderStage entryPoint) => false;

        public bool IsPixelShaderShared(ShaderStage entryPoint) => false;

        public bool IsVertexFormatSupported(VertexType vertexType) => vertexType == VertexType.Water;

        public bool IsVertexShaderShared(ShaderStage entryPoint)
        {
            return IsEntryPointSupported(entryPoint);
        }

        public ShaderParameters GetPixelShaderParameters()
        {
            if (!TemplateGenerationValid)
                return null;
            var result = new ShaderParameters();

            switch (waveshape)
            {
                case Waveshape.Default:
                    result.AddFloatParameter("displacement_range_x");
                    result.AddFloatParameter("displacement_range_y");
                    result.AddFloatParameter("displacement_range_z");
                    result.AddFloatParameter("slope_range_x");
                    result.AddFloatParameter("slope_range_y");
                    result.AddSamplerParameter("wave_displacement_array");
                    result.AddFloatParameter("wave_height");
                    result.AddFloatParameter("time_warp");
                    result.AddSamplerParameter("wave_slope_array");
                    result.AddFloatParameter("wave_height_aux");
                    result.AddFloatParameter("time_warp_aux");
                    result.AddFloatParameter("detail_slope_scale_x");
                    result.AddFloatParameter("detail_slope_scale_y");
                    result.AddFloatParameter("detail_slope_scale_z");
                    result.AddFloatParameter("detail_slope_steepness");
                    break;
                case Waveshape.Bump:
                    result.AddSamplerParameter("bump_map");
                    result.AddSamplerParameter("bump_detail_map");
                    break;
            }

            switch (watercolor)
            {
                case Watercolor.Pure:
                    result.AddFloat4Parameter("water_color_pure");
                    break;
                case Watercolor.Texture:
                    result.AddSamplerWithoutXFormParameter("watercolor_texture");
                    result.AddFloatParameter("watercolor_coefficient");
                    break;
            }

            switch (reflection)
            {
                case Reflection.Static:
                    result.AddSamplerWithoutXFormParameter("environment_map");
                    result.AddFloatParameter("reflection_coefficient");
                    result.AddFloatParameter("sunspot_cut");
                    result.AddFloatParameter("shadow_intensity_mark");
                    break;
                case Reflection.Dynamic:
                    result.AddFloatParameter("reflection_coefficient");
                    break;
            }

            switch (refraction)
            {
                case Refraction.Dynamic:
                    result.AddFloatParameter("refraction_texcoord_shift");
                    result.AddFloatParameter("water_murkiness");
                    result.AddFloatParameter("refraction_extinct_distance");
                    result.AddFloatParameter("minimal_wave_disturbance");
                    result.AddFloatParameter("refraction_depth_dominant_ratio");
                    break;
            }

            switch (bankalpha)
            {
                case Bankalpha.Depth:
                    result.AddFloatParameter("bankalpha_infuence_depth");
                    break;
                case Bankalpha.Paint:
                    result.AddSamplerParameter("watercolor_texture");
                    break;
                case Bankalpha.From_Shape_Texture_Alpha:
                    result.AddSamplerParameter("global_shape_texture"); //v
                    break;
            }

            switch (appearance)
            {
                case Appearance.Default:
                    result.AddFloatParameter("fresnel_coefficient");
                    result.AddFloat4Parameter("water_diffuse");
                    result.AddBooleanParameter("no_dynamic_lights");
                    break;
            }

            switch (global_shape)
            {
                case Global_Shape.Paint:
                    result.AddSamplerParameter("global_shape_texture"); //v
                    break;
                case Global_Shape.Depth:
                    result.AddFloatParameter("globalshape_infuence_depth"); //v
                    break;
            }

            switch (foam)
            {
                case Foam.None:
                    result.AddSamplerParameter("foam_texture");
                    result.AddSamplerParameter("foam_texture_detail");
                    break;
                case Foam.Auto:
                    result.AddSamplerParameter("foam_texture");
                    result.AddSamplerParameter("foam_texture_detail");
                    result.AddFloatParameter("foam_height");
                    result.AddFloatParameter("foam_pow");
                    break;
                case Foam.Paint:
                    result.AddSamplerParameter("foam_texture");
                    result.AddSamplerParameter("foam_texture_detail");
                    result.AddSamplerParameter("global_shape_texture"); //v
                    break;
                case Foam.Both:
                    result.AddSamplerParameter("foam_texture");
                    result.AddSamplerParameter("foam_texture_detail");
                    result.AddSamplerParameter("global_shape_texture"); //v
                    result.AddFloatParameter("foam_height");
                    result.AddFloatParameter("foam_pow");
                    break;
            }

            switch (reach_compatibility)
            {
                case Reach_Compatibility.Enabled:
                    result.AddFloatParameter("slope_scaler");
                    result.AddFloatParameter("normal_variation_tweak");
                    result.AddFloatParameter("fresnel_dark_spot");
                    result.AddFloatParameter("foam_coefficient");
                    result.AddFloatParameter("foam_cut");
                    break;
            }

            return result;
        }

        public ShaderParameters GetVertexShaderParameters()
        {
            if (!TemplateGenerationValid)
                return null;
            var result = new ShaderParameters();

            //result.AddPrefixedFloat4VertexParameter("waveshape", "category_");
            //result.AddPrefixedFloat4VertexParameter("global_shape", "category_");
            //result.AddPrefixedFloat4VertexParameter("reach_compatibility", "category_");

            result.AddCategoryVertexParameter("waveshape");
            result.AddCategoryVertexParameter("global_shape");
            result.AddCategoryVertexParameter("reach_compatibility");

            switch (waveshape)
            {
                case Waveshape.Default:
                    result.AddFloatVertexParameter("displacement_range_x");
                    result.AddFloatVertexParameter("displacement_range_y");
                    result.AddFloatVertexParameter("displacement_range_z");
                    result.AddSamplerVertexParameter("wave_displacement_array");
                    result.AddFloatVertexParameter("wave_height");
                    result.AddFloatVertexParameter("time_warp");
                    result.AddSamplerVertexParameter("wave_slope_array");
                    result.AddFloatVertexParameter("wave_height_aux");
                    result.AddFloatVertexParameter("time_warp_aux");
                    result.AddFloatVertexParameter("choppiness_forward");
                    result.AddFloatVertexParameter("choppiness_backward");
                    result.AddFloatVertexParameter("choppiness_side");
                    result.AddFloatVertexParameter("wave_visual_damping_distance");
                    break;
            }

            switch (global_shape)
            {
                case Global_Shape.Paint:
                    result.AddSamplerVertexParameter("global_shape_texture"); //v
                    break;
                case Global_Shape.Depth:
                    result.AddFloatVertexParameter("globalshape_infuence_depth"); //v
                    break;
            }

            switch (foam)
            {
                case Foam.Paint:
                    result.AddSamplerVertexParameter("global_shape_texture"); //v
                    break;
                case Foam.Both:
                    result.AddSamplerVertexParameter("global_shape_texture"); //v
                    break;
            }

            return result;
        }

        public ShaderParameters GetGlobalParameters()
        {
            var result = new ShaderParameters();
            result.AddFloat4Parameter("water_memory_export_addr", RenderMethodExtern.water_memory_export_address);
            result.AddSamplerWithoutXFormParameter("scene_ldr_texture", RenderMethodExtern.scene_ldr_texture);
            result.AddSamplerWithoutXFormParameter("scene_hdr_texture", RenderMethodExtern.scene_hdr_texture);
            result.AddSamplerWithoutXFormParameter("depth_buffer", RenderMethodExtern.texture_global_target_z);
            result.AddSamplerWithoutXFormParameter("lightprobe_texture_array", RenderMethodExtern.texture_lightprobe_texture);
            return result;
        }

        public ShaderParameters GetParametersInOption(string methodName, int option, out string rmopName, out string optionName)
        {
            ShaderParameters result = new ShaderParameters();
            rmopName = null;
            optionName = null;

            if (methodName == "waveshape")
            {
                optionName = ((Waveshape)option).ToString();

                switch ((Waveshape)option)
                {
                    case Waveshape.Default:
                        result.AddFloatParameter("displacement_range_x");
                        result.AddFloatParameter("displacement_range_y");
                        result.AddFloatParameter("displacement_range_z");
                        result.AddFloatParameter("slope_range_x");
                        result.AddFloatParameter("slope_range_y");
                        result.AddSamplerParameter("wave_displacement_array");
                        result.AddFloatParameter("wave_height");
                        result.AddFloatParameter("time_warp");
                        result.AddSamplerParameter("wave_slope_array");
                        result.AddFloatParameter("wave_height_aux");
                        result.AddFloatParameter("time_warp_aux");
                        result.AddFloatVertexParameter("choppiness_forward");
                        result.AddFloatVertexParameter("choppiness_backward");
                        result.AddFloatVertexParameter("choppiness_side");
                        result.AddFloatParameter("detail_slope_scale_x");
                        result.AddFloatParameter("detail_slope_scale_y");
                        result.AddFloatParameter("detail_slope_scale_z");
                        result.AddFloatParameter("detail_slope_steepness");
                        result.AddFloatVertexParameter("wave_visual_damping_distance");
                        rmopName = @"shaders\water_options\waveshape_default";
                        break;
                    case Waveshape.Bump:
                        result.AddSamplerParameter("bump_map");
                        result.AddSamplerParameter("bump_detail_map");
                        rmopName = @"shaders\water_options\waveshape_bump";
                        break;
                }
            }
            if (methodName == "watercolor")
            {
                optionName = ((Watercolor)option).ToString();

                switch ((Watercolor)option)
                {
                    case Watercolor.Pure:
                        result.AddFloat4Parameter("water_color_pure");
                        rmopName = @"shaders\water_options\watercolor_pure";
                        break;
                    case Watercolor.Texture:
                        result.AddSamplerWithoutXFormParameter("watercolor_texture");
                        result.AddFloatParameter("watercolor_coefficient");
                        rmopName = @"shaders\water_options\watercolor_texture";
                        break;
                }
            }
            if (methodName == "reflection")
            {
                optionName = ((Reflection)option).ToString();

                switch ((Reflection)option)
                {
                    case Reflection.Static:
                        result.AddSamplerWithoutXFormParameter("environment_map");
                        result.AddFloatParameter("reflection_coefficient");
                        result.AddFloatParameter("sunspot_cut");
                        result.AddFloatParameter("shadow_intensity_mark");
                        rmopName = @"shaders\water_options\reflection_static";
                        break;
                    case Reflection.Dynamic:
                        result.AddFloatParameter("reflection_coefficient");
                        rmopName = @"shaders\water_options\reflection_dynamic";
                        break;
                }
            }
            if (methodName == "refraction")
            {
                optionName = ((Refraction)option).ToString();

                switch ((Refraction)option)
                {
                    case Refraction.Dynamic:
                        result.AddFloatParameter("refraction_texcoord_shift");
                        result.AddFloatParameter("water_murkiness");
                        result.AddFloatParameter("refraction_extinct_distance");
                        result.AddFloatParameter("minimal_wave_disturbance");
                        result.AddFloatParameter("refraction_depth_dominant_ratio");
                        rmopName = @"shaders\water_options\refraction_dynamic";
                        break;
                }
            }
            if (methodName == "bankalpha")
            {
                optionName = ((Bankalpha)option).ToString();

                switch ((Bankalpha)option)
                {
                    case Bankalpha.Depth:
                        result.AddFloatParameter("bankalpha_infuence_depth");
                        rmopName = @"shaders\water_options\bankalpha_depth";
                        break;
                    case Bankalpha.Paint:
                        result.AddSamplerParameter("watercolor_texture");
                        rmopName = @"shaders\water_options\bankalpha_paint";
                        break;
                    case Bankalpha.From_Shape_Texture_Alpha:
                        result.AddSamplerParameter("global_shape_texture");
                        rmopName = @"shaders\water_options\bankalpha_from_shape_texture_alpha";
                        break;
                }
            }
            if (methodName == "appearance")
            {
                optionName = ((Appearance)option).ToString();

                switch ((Appearance)option)
                {
                    case Appearance.Default:
                        result.AddFloatParameter("fresnel_coefficient");
                        result.AddFloat4Parameter("water_diffuse");
                        result.AddBooleanParameter("no_dynamic_lights");
                        rmopName = @"shaders\water_options\appearance_default";
                        break;
                }
            }
            if (methodName == "global_shape")
            {
                optionName = ((Global_Shape)option).ToString();

                switch ((Global_Shape)option)
                {
                    case Global_Shape.Paint:
                        result.AddSamplerParameter("global_shape_texture");
                        rmopName = @"shaders\water_options\globalshape_paint";
                        break;
                    case Global_Shape.Depth:
                        result.AddFloatParameter("globalshape_infuence_depth");
                        rmopName = @"shaders\water_options\globalshape_depth";
                        break;
                }
            }
            if (methodName == "foam")
            {
                optionName = ((Foam)option).ToString();

                switch ((Foam)option)
                {
                    case Foam.None:
                        result.AddSamplerParameter("foam_texture");
                        result.AddSamplerParameter("foam_texture_detail");
                        rmopName = @"shaders\water_options\foam_none";
                        break;
                    case Foam.Auto:
                        result.AddSamplerParameter("foam_texture");
                        result.AddSamplerParameter("foam_texture_detail");
                        result.AddFloatParameter("foam_height");
                        result.AddFloatParameter("foam_pow");
                        rmopName = @"shaders\water_options\foam_auto";
                        break;
                    case Foam.Paint:
                        result.AddSamplerParameter("foam_texture");
                        result.AddSamplerParameter("foam_texture_detail");
                        result.AddSamplerParameter("global_shape_texture");
                        rmopName = @"shaders\water_options\foam_paint";
                        break;
                    case Foam.Both:
                        result.AddSamplerParameter("foam_texture");
                        result.AddSamplerParameter("foam_texture_detail");
                        result.AddSamplerParameter("global_shape_texture");
                        result.AddFloatParameter("foam_height");
                        result.AddFloatParameter("foam_pow");
                        rmopName = @"shaders\water_options\foam_both";
                        break;
                }
            }
            if (methodName == "reach_compatibility")
            {
                optionName = ((Reach_Compatibility)option).ToString();

                switch ((Reach_Compatibility)option)
                {
                    case Reach_Compatibility.Enabled:
                        result.AddFloatParameter("slope_scaler");
                        result.AddFloatParameter("normal_variation_tweak");
                        result.AddFloatParameter("fresnel_dark_spot");
                        result.AddFloatParameter("foam_coefficient");
                        result.AddFloatParameter("foam_cut");
                        rmopName = @"shaders\water_options\reach_compatibility_enabled";
                        break;
                }
            }

            return result;
        }

        public Array GetMethodNames()
        {
            return Enum.GetValues(typeof(WaterMethods));
        }

        public Array GetMethodOptionNames(int methodIndex)
        {
            switch ((WaterMethods)methodIndex)
            {
                case WaterMethods.Waveshape:
                    return Enum.GetValues(typeof(Waveshape));
                case WaterMethods.Watercolor:
                    return Enum.GetValues(typeof(Watercolor));
                case WaterMethods.Reflection:
                    return Enum.GetValues(typeof(Reflection));
                case WaterMethods.Refraction:
                    return Enum.GetValues(typeof(Refraction));
                case WaterMethods.Bankalpha:
                    return Enum.GetValues(typeof(Bankalpha));
                case WaterMethods.Appearance:
                    return Enum.GetValues(typeof(Appearance));
                case WaterMethods.Global_Shape:
                    return Enum.GetValues(typeof(Global_Shape));
                case WaterMethods.Foam:
                    return Enum.GetValues(typeof(Foam));
                case WaterMethods.Reach_Compatibility:
                    return Enum.GetValues(typeof(Reach_Compatibility));
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
