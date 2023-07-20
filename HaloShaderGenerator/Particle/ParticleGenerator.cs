using System;
using System.Collections.Generic;
using HaloShaderGenerator.Decal;
using HaloShaderGenerator.DirectX;
using HaloShaderGenerator.Generator;
using HaloShaderGenerator.Globals;

namespace HaloShaderGenerator.Particle
{
    public class ParticleGenerator : IShaderGenerator
    {
        private bool TemplateGenerationValid;
        private bool ApplyFixes;

        Albedo albedo;
        Blend_Mode blend_mode;
        Specialized_Rendering specialized_rendering;
        Lighting lighting;
        Render_Targets render_targets;
        Depth_Fade depth_fade;
        Black_Point black_point;
        Fog fog;
        Frame_Blend frame_blend;
        Self_Illumination self_illumination;

        /// <summary>
        /// Generator insantiation for shared shaders. Does not require method options.
        /// </summary>
        public ParticleGenerator(bool applyFixes = false) { TemplateGenerationValid = false; ApplyFixes = applyFixes; }

        /// <summary>
        /// Generator instantiation for method specific shaders.
        /// </summary>
        public ParticleGenerator(Albedo albedo, Blend_Mode blend_mode, Specialized_Rendering specialized_rendering, Lighting lighting, Render_Targets render_targets,
            Depth_Fade depth_fade, Black_Point black_point, Fog fog, Frame_Blend frame_blend, Self_Illumination self_illumination, bool applyFixes = false)
        {
            this.albedo = albedo;
            this.blend_mode = blend_mode;
            this.specialized_rendering = specialized_rendering;
            this.lighting = lighting;
            this.render_targets = render_targets;
            this.depth_fade = depth_fade;
            this.black_point = black_point;
            this.fog = fog;
            this.frame_blend = frame_blend;
            this.self_illumination = self_illumination;

            ApplyFixes = applyFixes;
            TemplateGenerationValid = true;
        }

        public ParticleGenerator(byte[] options, bool applyFixes = false)
        {
            options = ValidateOptions(options);

            this.albedo = (Albedo)options[0];
            this.blend_mode = (Blend_Mode)options[1];
            this.specialized_rendering = (Specialized_Rendering)options[2];
            this.lighting = (Lighting)options[3];
            this.render_targets = (Render_Targets)options[4];
            this.depth_fade = (Depth_Fade)options[5];
            this.black_point = (Black_Point)options[6];
            this.fog = (Fog)options[7];
            this.frame_blend = (Frame_Blend)options[8];
            this.self_illumination = (Self_Illumination)options[9];

            ApplyFixes = applyFixes;
            TemplateGenerationValid = true;
        }

        public ShaderGeneratorResult GeneratePixelShader(ShaderStage entryPoint)
        {
            if (!TemplateGenerationValid)
                throw new System.Exception("Generator initialized with shared shader constructor. Use template constructor.");

            List<D3D.SHADER_MACRO> macros = new List<D3D.SHADER_MACRO>();

            Shared.Blend_Mode sBlendMode = (Shared.Blend_Mode)Enum.Parse(typeof(Shared.Blend_Mode), blend_mode.ToString());

            TemplateGenerator.TemplateGenerator.CreateGlobalMacros(macros, ShaderType.Particle, entryPoint, sBlendMode, 
                Shader.Misc.First_Person_Never, Shared.Alpha_Test.None, ApplyFixes);

            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Albedo>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Blend_Mode>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Specialized_Rendering>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Lighting>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Render_Targets>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Depth_Fade>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Black_Point>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Fog>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Frame_Blend>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Self_Illumination>());

            macros.Add(ShaderGeneratorBase.CreateAutoMacro("albedo", albedo.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("blend_mode", blend_mode.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("specialized_rendering", specialized_rendering.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("lighting", lighting.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("render_targets", render_targets.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("depth_fade", depth_fade.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("black_point", black_point.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("fog", fog.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("frame_blend", frame_blend.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("self_illumination", self_illumination.ToString().ToLower()));

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

            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource($"particle.fx", macros, entryName, "ps_3_0");

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

            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource($"glps_particle.hlsl", macros, "entry_" + entryPoint.ToString().ToLower(), "ps_3_0");

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

            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource(@"glvs_particle.hlsl", macros, $"entry_{entryPoint.ToString().ToLower()}", "vs_3_0");

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
            return System.Enum.GetValues(typeof(ParticleMethods)).Length;
        }

        public int GetMethodOptionCount(int methodIndex)
        {
            switch ((ParticleMethods)methodIndex)
            {
                case ParticleMethods.Albedo:
                    return Enum.GetValues(typeof(Albedo)).Length;
                case ParticleMethods.Blend_Mode:
                    return Enum.GetValues(typeof(Blend_Mode)).Length;
                case ParticleMethods.Specialized_Rendering:
                    return Enum.GetValues(typeof(Specialized_Rendering)).Length;
                case ParticleMethods.Lighting:
                    return Enum.GetValues(typeof(Lighting)).Length;
                case ParticleMethods.Render_Targets:
                    return Enum.GetValues(typeof(Render_Targets)).Length;
                case ParticleMethods.Depth_Fade:
                    return Enum.GetValues(typeof(Depth_Fade)).Length;
                case ParticleMethods.Black_Point:
                    return Enum.GetValues(typeof(Black_Point)).Length;
                case ParticleMethods.Fog:
                    return Enum.GetValues(typeof(Fog)).Length;
                case ParticleMethods.Frame_Blend:
                    return Enum.GetValues(typeof(Frame_Blend)).Length;
                case ParticleMethods.Self_Illumination:
                    return Enum.GetValues(typeof(Self_Illumination)).Length;
            }

            return -1;
        }

        public int GetMethodOptionValue(int methodIndex)
        {
            switch ((ParticleMethods)methodIndex)
            {
                case ParticleMethods.Albedo:
                    return (int)albedo;
                case ParticleMethods.Blend_Mode:
                    return (int)blend_mode;
                case ParticleMethods.Specialized_Rendering:
                    return (int)specialized_rendering;
                case ParticleMethods.Lighting:
                    return (int)lighting;
                case ParticleMethods.Render_Targets:
                    return (int)render_targets;
                case ParticleMethods.Depth_Fade:
                    return (int)depth_fade;
                case ParticleMethods.Black_Point:
                    return (int)black_point;
                case ParticleMethods.Fog:
                    return (int)fog;
                case ParticleMethods.Frame_Blend:
                    return (int)frame_blend;
                case ParticleMethods.Self_Illumination:
                    return (int)self_illumination;
            }
            return -1;
        }

        public bool IsEntryPointSupported(ShaderStage entryPoint)
        {
            if (entryPoint == ShaderStage.Default)
                return true;
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
                case VertexType.Particle:
                case VertexType.ParticleModel:
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
                case Albedo.Diffuse_Only:
                    result.AddSamplerParameter("base_map");
                    break;
                case Albedo.Diffuse_Plus_Billboard_Alpha:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("alpha_map");
                    break;
                case Albedo.Palettized:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerWithoutXFormParameter("palette");
                    break;
                case Albedo.Palettized_Plus_Billboard_Alpha:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerWithoutXFormParameter("palette");
                    result.AddSamplerParameter("alpha_map");
                    break;
                case Albedo.Diffuse_Plus_Sprite_Alpha:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("alpha_map");
                    break;
                case Albedo.Palettized_Plus_Sprite_Alpha:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerWithoutXFormParameter("palette");
                    result.AddSamplerParameter("alpha_map");
                    break;
                case Albedo.Diffuse_Modulated:
                    result.AddSamplerParameter("base_map");
                    result.AddFloat4Parameter("tint_color");
                    result.AddFloatParameter("modulation_factor");
                    break;
                case Albedo.Palettized_Plasma:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("base_map2");
                    result.AddSamplerWithoutXFormParameter("palette");
                    result.AddSamplerParameter("alpha_map");
                    result.AddFloatParameter("alpha_modulation_factor");
                    break;
                case Albedo.Palettized_2d_Plasma:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerParameter("base_map2");
                    result.AddSamplerWithoutXFormParameter("palette");
                    result.AddSamplerParameter("alpha_map");
                    break;
            }
            switch (specialized_rendering)
            {
                case Specialized_Rendering.Distortion:
                case Specialized_Rendering.Distortion_Expensive:
                case Specialized_Rendering.Distortion_Diffuse:
                case Specialized_Rendering.Distortion_Expensive_Diffuse:
                    result.AddFloatParameter("distortion_scale");
                    break;
            }
            switch (depth_fade)
            {
                case Depth_Fade.On:
                    result.AddFloatParameter("depth_fade_range");
                    break;
            }

            return result;
        }

        public ShaderParameters GetVertexShaderParameters()
        {
            if (!TemplateGenerationValid)
                return null;
            var result = new ShaderParameters();

            result.AddPrefixedFloat4VertexParameter("albedo", "category_");
            result.AddPrefixedFloat4VertexParameter("blend_mode", "category_");
            result.AddPrefixedFloat4VertexParameter("specialized_rendering", "category_");
            result.AddPrefixedFloat4VertexParameter("lighting", "category_");
            result.AddPrefixedFloat4VertexParameter("fog", "category_");
            switch (frame_blend)
            {
                case Frame_Blend.On:
                    result.AddFloatVertexParameter("starting_uv_scale");
                    result.AddFloatVertexParameter("ending_uv_scale");
                    break;
            }
            result.AddPrefixedFloat4VertexParameter("frame_blend", "category_");
            switch (self_illumination)
            {
                case Self_Illumination.Constant_Color:
                    result.AddFloat4VertexParameter("self_illum_color");
                    break;
            }
            result.AddPrefixedFloat4VertexParameter("self_illumination", "category_");

            return result;
        }

        public ShaderParameters GetGlobalParameters()
        {
            var result = new ShaderParameters();
            result.AddSamplerWithoutXFormParameter("depth_buffer", RenderMethodExtern.texture_global_target_z);
            result.AddFloat4Parameter("screen_constants", RenderMethodExtern.screen_constants);
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
                    case Albedo.Diffuse_Only:
                        result.AddSamplerParameter("base_map");
                        rmopName = @"shaders\particle_options\albedo_diffuse_only";
                        break;
                    case Albedo.Diffuse_Plus_Billboard_Alpha:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerParameter("alpha_map");
                        rmopName = @"shaders\particle_options\albedo_diffuse_plus_billboard_alpha";
                        break;
                    case Albedo.Palettized:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerWithoutXFormParameter("palette");
                        rmopName = @"shaders\particle_options\albedo_palettized";
                        break;
                    case Albedo.Palettized_Plus_Billboard_Alpha:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerWithoutXFormParameter("palette");
                        result.AddSamplerParameter("alpha_map");
                        rmopName = @"shaders\particle_options\albedo_palettized_plus_billboard_alpha";
                        break;
                    case Albedo.Diffuse_Plus_Sprite_Alpha:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerParameter("alpha_map");
                        rmopName = @"shaders\particle_options\albedo_diffuse_plus_sprite_alpha";
                        break;
                    case Albedo.Palettized_Plus_Sprite_Alpha:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerWithoutXFormParameter("palette");
                        result.AddSamplerParameter("alpha_map");
                        rmopName = @"shaders\particle_options\albedo_palettized_plus_sprite_alpha";
                        break;
                    case Albedo.Diffuse_Modulated:
                        result.AddSamplerParameter("base_map");
                        result.AddFloat4Parameter("tint_color");
                        result.AddFloatParameter("modulation_factor");
                        rmopName = @"shaders\particle_options\albedo_diffuse_modulated";
                        break;
                    case Albedo.Palettized_Glow:
                        result.AddSamplerParameter("base_map");
                        result.AddFloat4Parameter("tint_color");
                        rmopName = @"shaders\particle_options\albedo_palettized_glow";
                        break;
                    case Albedo.Palettized_Plasma:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerParameter("base_map2");
                        result.AddSamplerWithoutXFormParameter("palette");
                        result.AddSamplerParameter("alpha_map");
                        result.AddFloatParameter("alpha_modulation_factor");
                        rmopName = @"shaders\particle_options\albedo_palettized_plasma";
                        break;
                    case Albedo.Palettized_2d_Plasma:
                        result.AddSamplerParameter("base_map");
                        result.AddSamplerParameter("base_map2");
                        result.AddSamplerWithoutXFormParameter("palette");
                        result.AddSamplerParameter("alpha_map");
                        rmopName = @"shaders\particle_options\albedo_palettized_plasma";
                        break;
                }
            }
            if (methodName == "blend_mode")
            {
                optionName = ((Blend_Mode)option).ToString();
            }
            if (methodName == "specialized_rendering")
            {
                optionName = ((Specialized_Rendering)option).ToString();
                switch ((Specialized_Rendering)option)
                {
                    case Specialized_Rendering.Distortion:
                    case Specialized_Rendering.Distortion_Expensive:
                    case Specialized_Rendering.Distortion_Diffuse:
                    case Specialized_Rendering.Distortion_Expensive_Diffuse:
                        result.AddFloatParameter("distortion_scale");
                        rmopName = @"shaders\particle_options\distortion_diffuse";
                        break;
                }
            }
            if (methodName == "lighting")
            {
                optionName = ((Lighting)option).ToString();
            }
            if (methodName == "render_targets")
            {
                optionName = ((Render_Targets)option).ToString();
            }
            if (methodName == "depth_fade")
            {
                optionName = ((Depth_Fade)option).ToString();
                switch ((Depth_Fade)option)
                {
                    case Depth_Fade.On:
                        result.AddFloatParameter("depth_fade_range");
                        rmopName = @"shaders\particle_options\depth_fade_on";
                        break;
                }
            }
            if (methodName == "black_point")
            {
                optionName = ((Black_Point)option).ToString();
            }
            if (methodName == "fog")
            {
                optionName = ((Fog)option).ToString();
            }
            if (methodName == "frame_blend")
            {
                optionName = ((Frame_Blend)option).ToString();
                switch ((Frame_Blend)option)
                {
                    case Frame_Blend.On:
                        result.AddFloatVertexParameter("starting_uv_scale");
                        result.AddFloatVertexParameter("ending_uv_scale");
                        rmopName = @"shaders\particle_options\frame_blend_on";
                        break;
                }
            }
            if (methodName == "self_illumination")
            {
                optionName = ((Self_Illumination)option).ToString();
                switch ((Self_Illumination)option)
                {
                    case Self_Illumination.Constant_Color:
                        result.AddFloat4VertexParameter("self_illum_color");
                        rmopName = @"shaders\particle_options\self_illumination_constant_color";
                        break;
                }
            }
            return result;
        }

        public Array GetMethodNames()
        {
            return Enum.GetValues(typeof(ParticleMethods));
        }

        public Array GetMethodOptionNames(int methodIndex)
        {
            switch ((ParticleMethods)methodIndex)
            {
                case ParticleMethods.Albedo:
                    return Enum.GetValues(typeof(Albedo));
                case ParticleMethods.Blend_Mode:
                    return Enum.GetValues(typeof(Blend_Mode));
                case ParticleMethods.Specialized_Rendering:
                    return Enum.GetValues(typeof(Specialized_Rendering));
                case ParticleMethods.Lighting:
                    return Enum.GetValues(typeof(Lighting));
                case ParticleMethods.Render_Targets:
                    return Enum.GetValues(typeof(Render_Targets));
                case ParticleMethods.Depth_Fade:
                    return Enum.GetValues(typeof(Depth_Fade));
                case ParticleMethods.Black_Point:
                    return Enum.GetValues(typeof(Black_Point));
                case ParticleMethods.Fog:
                    return Enum.GetValues(typeof(Fog));
                case ParticleMethods.Frame_Blend:
                    return Enum.GetValues(typeof(Frame_Blend));
                case ParticleMethods.Self_Illumination:
                    return Enum.GetValues(typeof(Self_Illumination));
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
