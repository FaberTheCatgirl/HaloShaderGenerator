using System;
using System.Collections.Generic;
using HaloShaderGenerator.DirectX;
using HaloShaderGenerator.Generator;
using HaloShaderGenerator.Globals;
using HaloShaderGenerator.Water;

namespace HaloShaderGenerator.Decal
{
    public class DecalGenerator : IShaderGenerator
    {
        private bool TemplateGenerationValid;
        private bool ApplyFixes;
        private bool DecalIsSimple;

        Albedo albedo;
        Blend_Mode blend_mode;
        Render_Pass render_pass;
        Specular specular;
        Bump_Mapping bump_mapping;
        Tinting tinting;

        /// <summary>
        /// Generator insantiation for shared shaders. Does not require method options.
        /// </summary>
        public DecalGenerator(bool applyFixes = false) { TemplateGenerationValid = false; ApplyFixes = applyFixes; }

        /// <summary>
        /// Generator instantiation for method specific shaders.
        /// </summary>
        public DecalGenerator(Albedo albedo, Blend_Mode blend_mode, Render_Pass render_pass, Specular specular, Bump_Mapping bump_mapping, Tinting tinting, bool applyFixes = false)
        {
            this.albedo = albedo;
            this.blend_mode = blend_mode;
            this.render_pass = render_pass;
            this.specular = specular;
            this.bump_mapping = bump_mapping;
            this.tinting = tinting;

            ApplyFixes = applyFixes;
            DecalIsSimple = this.render_pass == Render_Pass.Pre_Lighting && this.bump_mapping == Bump_Mapping.Leave;
            TemplateGenerationValid = true;
        }

        public DecalGenerator(byte[] options, bool applyFixes = false)
        {
            options = ValidateOptions(options);

            this.albedo = (Albedo)options[0];
            this.blend_mode = (Blend_Mode)options[1];
            this.render_pass = (Render_Pass)options[2];
            this.specular = (Specular)options[3];
            this.bump_mapping = (Bump_Mapping)options[4];
            this.tinting = (Tinting)options[5];

            ApplyFixes = applyFixes;
            DecalIsSimple = this.render_pass == Render_Pass.Pre_Lighting && this.bump_mapping == Bump_Mapping.Leave;
            TemplateGenerationValid = true;
        }

        public ShaderGeneratorResult GeneratePixelShader(ShaderStage entryPoint)
        {
            if (!TemplateGenerationValid)
                throw new System.Exception("Generator initialized with shared shader constructor. Use template constructor.");

            List<D3D.SHADER_MACRO> macros = new List<D3D.SHADER_MACRO>();

            Shared.Blend_Mode sBlendMode = (Shared.Blend_Mode)Enum.Parse(typeof(Shared.Blend_Mode), blend_mode.ToString());

            TemplateGenerator.TemplateGenerator.CreateGlobalMacros(macros, ShaderType.Decal, entryPoint, sBlendMode, 
                Shader.Misc.First_Person_Never, Shared.Alpha_Test.None, ApplyFixes);

            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Albedo>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Blend_Mode>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Render_Pass>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Specular>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Bump_Mapping>());
            macros.AddRange(ShaderGeneratorBase.CreateAutoMacroMethodEnumDefinitions<Tinting>());

            macros.Add(ShaderGeneratorBase.CreateAutoMacro("albedo", albedo.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("blend_mode", blend_mode.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("render_pass", render_pass.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("specular", specular.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("bump_mapping", bump_mapping.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateAutoMacro("tinting", tinting.ToString().ToLower()));

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

            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource($"decal.fx", macros, entryName, "ps_3_0");

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

            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource($"glps_decal.hlsl", macros, "entry_" + entryPoint.ToString().ToLower(), "ps_3_0");

            return new ShaderGeneratorResult(shaderBytecode);
        }

        public ShaderGeneratorResult GenerateSharedVertexShader(VertexType vertexType, ShaderStage entryPoint)
        {
            if (!IsVertexFormatSupported(vertexType) || !IsEntryPointSupported(entryPoint))
                return null;

            List<D3D.SHADER_MACRO> macros = new List<D3D.SHADER_MACRO>();

            macros.Add(new D3D.SHADER_MACRO { Name = "_DEFINITION_HELPER_HLSLI", Definition = "1" });
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<ShaderStage>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<VertexType>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Shared.ShaderType>());
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_vertex_transform", vertexType, "calc_vertex_transform_", ""));
            macros.Add(ShaderGeneratorBase.CreateMacro("transform_unknown_vector", vertexType, "transform_unknown_vector_", ""));
            macros.Add(ShaderGeneratorBase.CreateVertexMacro("input_vertex_format", vertexType));
            macros.Add(ShaderGeneratorBase.CreateMacro("transform_dominant_light", vertexType, "transform_dominant_light_", ""));

            macros.Add(ShaderGeneratorBase.CreateMacro("shaderstage", entryPoint, "k_shaderstage_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("vertextype", vertexType, "k_vertextype_"));
            macros.Add(ShaderGeneratorBase.CreateMacro("shadertype", Shared.ShaderType.Decal, "shadertype_"));

            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource(@"glvs_decal.hlsl", macros, $"entry_{entryPoint.ToString().ToLower()}", "vs_3_0");

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
            return System.Enum.GetValues(typeof(DecalMethods)).Length;
        }

        public int GetMethodOptionCount(int methodIndex)
        {
            switch ((DecalMethods)methodIndex)
            {
                case DecalMethods.Albedo:
                    return Enum.GetValues(typeof(Albedo)).Length;
                case DecalMethods.Blend_Mode:
                    return Enum.GetValues(typeof(Blend_Mode)).Length;
                case DecalMethods.Render_Pass:
                    return Enum.GetValues(typeof(Render_Pass)).Length;
                case DecalMethods.Specular:
                    return Enum.GetValues(typeof(Specular)).Length;
                case DecalMethods.Bump_Mapping:
                    return Enum.GetValues(typeof(Bump_Mapping)).Length;
                case DecalMethods.Tinting:
                    return Enum.GetValues(typeof(Tinting)).Length;
            }

            return -1;
        }

        public int GetMethodOptionValue(int methodIndex)
        {
            switch ((DecalMethods)methodIndex)
            {
                case DecalMethods.Albedo:
                    return (int)albedo;
                case DecalMethods.Blend_Mode:
                    return (int)blend_mode;
                case DecalMethods.Render_Pass:
                    return (int)render_pass;
                case DecalMethods.Specular:
                    return (int)specular;
                case DecalMethods.Bump_Mapping:
                    return (int)bump_mapping;
                case DecalMethods.Tinting:
                    return (int)tinting;
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
            return vertexType == VertexType.World || vertexType == VertexType.Rigid || vertexType == VertexType.Skinned ||
                vertexType == VertexType.FlatWorld || vertexType == VertexType.FlatRigid || vertexType == VertexType.FlatSkinned;
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
                case Albedo.Palettized:
                    result.AddSamplerWithoutXFormParameter("base_map");
                    result.AddSamplerWithoutXFormParameter("palette");
                    break;
                case Albedo.Palettized_Plus_Alpha:
                    result.AddSamplerWithoutXFormParameter("base_map");
                    result.AddSamplerWithoutXFormParameter("palette");
                    result.AddSamplerWithoutXFormParameter("alpha_map");
                    break;
                case Albedo.Diffuse_Plus_Alpha:
                    result.AddSamplerWithoutXFormParameter("base_map");
                    result.AddSamplerWithoutXFormParameter("alpha_map");
                    break;
                case Albedo.Emblem_Change_Color:
                    result.AddSamplerWithoutXFormParameter("tex0_sampler", RenderMethodExtern.emblem_player_shoulder_texture);
                    result.AddSamplerWithoutXFormParameter("tex1_sampler", RenderMethodExtern.emblem_player_shoulder_texture);
                    result.AddFloat4Parameter("emblem_color_background_argb");
                    result.AddFloat4Parameter("emblem_color_icon1_argb");
                    result.AddFloat4Parameter("emblem_color_icon2_argb");
                    break;
                case Albedo.Change_Color:
                    result.AddSamplerWithoutXFormParameter("change_color_map");
                    result.AddFloat3Parameter("primary_change_color", RenderMethodExtern.object_change_color_primary);
                    result.AddFloat3Parameter("secondary_change_color", RenderMethodExtern.object_change_color_secondary);
                    result.AddFloat3Parameter("tertiary_change_color", RenderMethodExtern.object_change_color_tertiary);
                    break;
                case Albedo.Diffuse_Plus_Alpha_Mask:
                    result.AddSamplerWithoutXFormParameter("base_map");
                    result.AddSamplerWithoutXFormParameter("alpha_map");
                    break;
                case Albedo.Palettized_Plus_Alpha_Mask:
                    result.AddSamplerWithoutXFormParameter("base_map");
                    result.AddSamplerWithoutXFormParameter("palette");
                    result.AddSamplerWithoutXFormParameter("alpha_map");
                    break;
                case Albedo.Vector_Alpha:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerWithoutXFormParameter("vector_map");
                    result.AddFloatParameter("vector_sharpness");
                    result.AddFloatParameter("antialias_tweak");
                    break;
                case Albedo.Vector_Alpha_Drop_Shadow:
                    result.AddSamplerParameter("base_map");
                    result.AddSamplerWithoutXFormParameter("vector_map");
                    result.AddFloatParameter("vector_sharpness");
                    result.AddSamplerWithoutXFormParameter("shadow_vector_map");
                    result.AddFloatParameter("shadow_darkness");
                    result.AddFloatParameter("shadow_sharpness");
                    result.AddFloatParameter("antialias_tweak");
                    break;
            }
            switch (specular)
            {
                case Specular.Modulate:
                    result.AddFloatParameter("specular_multiplier");
                    break;
            }
            switch (bump_mapping)
            {
                case Bump_Mapping.Standard:
                    result.AddSamplerParameter("bump_map");
                    break;
                case Bump_Mapping.Standard_Mask:
                    result.AddSamplerParameter("bump_map");
                    break;
            }
            switch (tinting)
            {
                case Tinting.Unmodulated:
                    result.AddFloat4Parameter("tint_color");
                    result.AddFloatParameter("intensity");
                    break;
                case Tinting.Partially_Modulated:
                    result.AddFloat4Parameter("tint_color");
                    result.AddFloatParameter("intensity");
                    result.AddFloatParameter("modulation_factor");
                    break;
                case Tinting.Fully_Modulated:
                    result.AddFloat4Parameter("tint_color");
                    result.AddFloatParameter("intensity");
                    break;
            }

            return result;
        }

        public ShaderParameters GetVertexShaderParameters()
        {
            if (!TemplateGenerationValid)
                return null;

            var result = new ShaderParameters();

            result.AddFloatVertexParameter("u_tiles");
            result.AddFloatVertexParameter("v_tiles");

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
                        result.AddFloatVertexParameter("u_tiles");
                        result.AddFloatVertexParameter("v_tiles");
                        rmopName = @"shaders\decal_options\albedo_diffuse_only";
                        break;
                    case Albedo.Palettized:
                        result.AddSamplerWithoutXFormParameter("base_map");
                        result.AddSamplerWithoutXFormParameter("palette");
                        result.AddFloatVertexParameter("u_tiles");
                        result.AddFloatVertexParameter("v_tiles");
                        rmopName = @"shaders\decal_options\albedo_palettized";
                        break;
                    case Albedo.Palettized_Plus_Alpha:
                        result.AddSamplerWithoutXFormParameter("base_map");
                        result.AddSamplerWithoutXFormParameter("palette");
                        result.AddSamplerWithoutXFormParameter("alpha_map");
                        result.AddFloatVertexParameter("u_tiles");
                        result.AddFloatVertexParameter("v_tiles");
                        rmopName = @"shaders\decal_options\albedo_palettized_plus_alpha";
                        break;
                    case Albedo.Diffuse_Plus_Alpha:
                        result.AddSamplerWithoutXFormParameter("base_map");
                        result.AddSamplerWithoutXFormParameter("alpha_map");
                        result.AddFloatVertexParameter("u_tiles");
                        result.AddFloatVertexParameter("v_tiles");
                        rmopName = @"shaders\decal_options\albedo_diffuse_plus_alpha";
                        break;
                    case Albedo.Emblem_Change_Color:
                        result.AddSamplerWithoutXFormParameter("tex0_sampler", RenderMethodExtern.emblem_player_shoulder_texture);
                        result.AddSamplerWithoutXFormParameter("tex1_sampler", RenderMethodExtern.emblem_player_shoulder_texture);
                        result.AddFloat4Parameter("emblem_color_background_argb");
                        result.AddFloat4Parameter("emblem_color_icon1_argb");
                        result.AddFloat4Parameter("emblem_color_icon2_argb");
                        result.AddFloatVertexParameter("u_tiles");
                        result.AddFloatVertexParameter("v_tiles");
                        rmopName = @"shaders\decal_options\albedo_emblem_change_color";
                        break;
                    case Albedo.Change_Color:
                        result.AddSamplerWithoutXFormParameter("change_color_map");
                        result.AddFloat3Parameter("primary_change_color", RenderMethodExtern.object_change_color_primary);
                        result.AddFloat3Parameter("secondary_change_color", RenderMethodExtern.object_change_color_secondary);
                        result.AddFloat3Parameter("tertiary_change_color", RenderMethodExtern.object_change_color_tertiary);
                        result.AddFloatVertexParameter("u_tiles");
                        result.AddFloatVertexParameter("v_tiles");
                        rmopName = @"shaders\decal_options\albedo_change_color";
                        break;
                    case Albedo.Diffuse_Plus_Alpha_Mask:
                        result.AddSamplerWithoutXFormParameter("base_map");
                        result.AddSamplerWithoutXFormParameter("alpha_map");
                        result.AddFloatVertexParameter("u_tiles");
                        result.AddFloatVertexParameter("v_tiles");
                        rmopName = @"shaders\decal_options\albedo_diffuse_plus_alpha_mask";
                        break;
                    case Albedo.Palettized_Plus_Alpha_Mask:
                        result.AddSamplerWithoutXFormParameter("base_map");
                        result.AddSamplerWithoutXFormParameter("palette");
                        result.AddSamplerWithoutXFormParameter("alpha_map");
                        result.AddFloatVertexParameter("u_tiles");
                        result.AddFloatVertexParameter("v_tiles");
                        rmopName = @"shaders\decal_options\albedo_palettized_plus_alpha_mask";
                        break;
                    case Albedo.Vector_Alpha:
                        result.AddSamplerParameter("base_map");
                        result.AddFloatVertexParameter("u_tiles");
                        result.AddFloatVertexParameter("v_tiles");
                        result.AddSamplerWithoutXFormParameter("vector_map");
                        result.AddFloatParameter("vector_sharpness");
                        result.AddFloatParameter("antialias_tweak");
                        rmopName = @"shaders\decal_options\albedo_vector_alpha";
                        break;
                    case Albedo.Vector_Alpha_Drop_Shadow:
                        result.AddSamplerParameter("base_map");
                        result.AddFloatVertexParameter("u_tiles");
                        result.AddFloatVertexParameter("v_tiles");
                        result.AddSamplerWithoutXFormParameter("vector_map");
                        result.AddFloatParameter("vector_sharpness");
                        result.AddSamplerWithoutXFormParameter("shadow_vector_map");
                        result.AddFloatParameter("shadow_darkness");
                        result.AddFloatParameter("shadow_sharpness");
                        result.AddFloatParameter("antialias_tweak");
                        rmopName = @"shaders\decal_options\albedo_vector_alpha_drop_shadow";
                        break;
                }
            }
            if (methodName == "blend_mode")
            {
                optionName = ((Blend_Mode)option).ToString();
            }
            if (methodName == "render_pass")
            {
                optionName = ((Render_Pass)option).ToString();
            }
            if (methodName == "specular")
            {
                optionName = ((Specular)option).ToString();

                switch ((Specular)option)
                {
                    case Specular.Modulate:
                        result.AddFloatParameter("specular_multiplier");
                        rmopName = @"shaders\decal_options\specular_modulate";
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
                        rmopName = @"shaders\decal_options\bump_mapping_standard";
                        break;
                    case Bump_Mapping.Standard_Mask:
                        result.AddSamplerParameter("bump_map");
                        rmopName = @"shaders\decal_options\bump_mapping_standard_mask";
                        break;
                }
            }
            if (methodName == "tinting")
            {
                optionName = ((Tinting)option).ToString();

                switch ((Tinting)option)
                {
                    case Tinting.Unmodulated:
                        result.AddFloat4Parameter("tint_color");
                        result.AddFloatParameter("intensity");
                        rmopName = @"shaders\decal_options\tinting_unmodulated";
                        break;
                    case Tinting.Partially_Modulated:
                        result.AddFloat4Parameter("tint_color");
                        result.AddFloatParameter("intensity");
                        result.AddFloatParameter("modulation_factor");
                        rmopName = @"shaders\decal_options\tinting_partially_modulated";
                        break;
                    case Tinting.Fully_Modulated:
                        result.AddFloat4Parameter("tint_color");
                        result.AddFloatParameter("intensity");
                        rmopName = @"shaders\decal_options\tinting_fully_modulated";
                        break;
                }
            }

            return result;
        }

        public Array GetMethodNames()
        {
            return Enum.GetValues(typeof(DecalMethods));
        }

        public Array GetMethodOptionNames(int methodIndex)
        {
            switch ((DecalMethods)methodIndex)
            {
                case DecalMethods.Albedo:
                    return Enum.GetValues(typeof(Albedo));
                case DecalMethods.Blend_Mode:
                    return Enum.GetValues(typeof(Blend_Mode));
                case DecalMethods.Render_Pass:
                    return Enum.GetValues(typeof(Render_Pass));
                case DecalMethods.Specular:
                    return Enum.GetValues(typeof(Specular));
                case DecalMethods.Bump_Mapping:
                    return Enum.GetValues(typeof(Bump_Mapping));
                case DecalMethods.Tinting:
                    return Enum.GetValues(typeof(Tinting));
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
