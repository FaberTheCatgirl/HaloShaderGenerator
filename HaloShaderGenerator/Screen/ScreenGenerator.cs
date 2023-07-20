using System;
using System.Collections.Generic;
using HaloShaderGenerator.DirectX;
using HaloShaderGenerator.Generator;
using HaloShaderGenerator.Globals;
using HaloShaderGenerator.Particle;

namespace HaloShaderGenerator.Screen
{
    public class ScreenGenerator : IShaderGenerator
    {
        private bool TemplateGenerationValid;
        private bool ApplyFixes;

        Warp warp;
        Base _base;
        Overlay_A overlay_a;
        Overlay_B overlay_b;
        Blend_Mode blend_mode;

        /// <summary>
        /// Generator insantiation for shared shaders. Does not require method options.
        /// </summary>
        public ScreenGenerator(bool applyFixes = false) { TemplateGenerationValid = false; ApplyFixes = applyFixes; }

        /// <summary>
        /// Generator instantiation for method specific shaders.
        /// </summary>
        public ScreenGenerator(Warp warp, Base _base, Overlay_A overlay_a, Overlay_B overlay_b, Blend_Mode blend_mode, bool applyFixes = false)
        {
            this.warp = warp;
            this._base = _base;
            this.overlay_a = overlay_a;
            this.overlay_b = overlay_b;
            this.blend_mode = blend_mode;

            ApplyFixes = applyFixes;
            TemplateGenerationValid = true;
        }

        public ScreenGenerator(byte[] options, bool applyFixes = false)
        {
            options = ValidateOptions(options);

            this.warp = (Warp)options[0];
            this._base = (Base)options[1];
            this.overlay_a = (Overlay_A)options[2];
            this.overlay_b = (Overlay_B)options[3];
            this.blend_mode = (Blend_Mode)options[4];

            ApplyFixes = applyFixes;
            TemplateGenerationValid = true;
        }

        public ShaderGeneratorResult GeneratePixelShader(ShaderStage entryPoint)
        {
            if (!TemplateGenerationValid)
                throw new System.Exception("Generator initialized with shared shader constructor. Use template constructor.");

            List<D3D.SHADER_MACRO> macros = new List<D3D.SHADER_MACRO>();

            Shared.Blend_Mode sBlendMode = (Shared.Blend_Mode)Enum.Parse(typeof(Shared.Blend_Mode), blend_mode.ToString());

            TemplateGenerator.TemplateGenerator.CreateGlobalMacros(macros, ShaderType.Screen, entryPoint, sBlendMode, 
                Shader.Misc.First_Person_Never, Shared.Alpha_Test.None, ApplyFixes);

            macros.Add(ShaderGeneratorBase.CreateMacro("warp_type", warp.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateMacro("base_type", _base.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateMacro("overlay_a_type", overlay_a.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateMacro("overlay_b_type", overlay_b.ToString().ToLower()));
            macros.Add(ShaderGeneratorBase.CreateMacro("blend_type", blend_mode.ToString().ToLower()));

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

            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource($"screen.fx", macros, entryName, "ps_3_0");

            return new ShaderGeneratorResult(shaderBytecode);
        }

        public ShaderGeneratorResult GenerateSharedPixelShader(ShaderStage entryPoint, int methodIndex, int optionIndex)
        {
            if (!IsEntryPointSupported(entryPoint) || !IsPixelShaderShared(entryPoint))
                return null;

            List<D3D.SHADER_MACRO> macros = new List<D3D.SHADER_MACRO>();

            macros.Add(new D3D.SHADER_MACRO { Name = "_DEFINITION_HELPER_HLSLI", Definition = "1" });
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<ShaderStage>());
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Shared.ShaderType>());

            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource($"glps_screen.hlsl", macros, "entry_" + entryPoint.ToString().ToLower(), "ps_3_0");

            return new ShaderGeneratorResult(shaderBytecode);
        }

        public ShaderGeneratorResult GenerateSharedVertexShader(VertexType vertexType, ShaderStage entryPoint)
        {
            if (!IsVertexFormatSupported(vertexType) || !IsEntryPointSupported(entryPoint))
                return null;

            List<D3D.SHADER_MACRO> macros = new List<D3D.SHADER_MACRO>();
            macros.AddRange(ShaderGeneratorBase.CreateMethodEnumDefinitions<Shared.ShaderType>());

            macros.Add(new D3D.SHADER_MACRO { Name = "_DEFINITION_HELPER_HLSLI", Definition = "1" });
            macros.Add(ShaderGeneratorBase.CreateMacro("calc_vertex_transform", vertexType, "calc_vertex_transform_", ""));
            macros.Add(ShaderGeneratorBase.CreateMacro("transform_unknown_vector", vertexType, "transform_unknown_vector_", ""));
            macros.Add(ShaderGeneratorBase.CreateVertexMacro("input_vertex_format", vertexType));

            macros.Add(ShaderGeneratorBase.CreateMacro("shadertype", Shared.ShaderType.Screen, "shadertype_"));

            byte[] shaderBytecode = ShaderGeneratorBase.GenerateSource(@"glvs_screen.hlsl", macros, $"entry_{entryPoint.ToString().ToLower()}", "vs_3_0");

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
            return System.Enum.GetValues(typeof(ScreenMethods)).Length;
        }

        public int GetMethodOptionCount(int methodIndex)
        {
            switch ((ScreenMethods)methodIndex)
            {
                case ScreenMethods.Warp:
                    return Enum.GetValues(typeof(Warp)).Length;
                case ScreenMethods.Base:
                    return Enum.GetValues(typeof(Base)).Length;
                case ScreenMethods.Overlay_A:
                    return Enum.GetValues(typeof(Overlay_A)).Length;
                case ScreenMethods.Overlay_B:
                    return Enum.GetValues(typeof(Overlay_B)).Length;
                case ScreenMethods.Blend_Mode:
                    return Enum.GetValues(typeof(Blend_Mode)).Length;
            }

            return -1;
        }

        public int GetMethodOptionValue(int methodIndex)
        {
            switch ((ScreenMethods)methodIndex)
            {
                case ScreenMethods.Warp:
                    return (int)warp;
                case ScreenMethods.Base:
                    return (int)_base;
                case ScreenMethods.Overlay_A:
                    return (int)overlay_a;
                case ScreenMethods.Overlay_B:
                    return (int)overlay_b;
                case ScreenMethods.Blend_Mode:
                    return (int)blend_mode;
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
            return vertexType == VertexType.Screen;
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

            switch (warp)
            {
                case Warp.Pixel_Space:
                case Warp.Screen_Space:
                    result.AddSamplerParameter("warp_map");
                    result.AddFloatParameter("warp_amount");
                    break;
            }
            switch (_base)
            {
                case Base.Single_Screen_Space:
                case Base.Single_Pixel_Space:
                    result.AddSamplerParameter("base_map");
                    break;
            }
            switch (overlay_a)
            {
                case Overlay_A.Tint_Add_Color:
                    result.AddFloat4ColorParameter("tint_color");
                    result.AddFloat4ColorParameter("add_color");
                    break;
                case Overlay_A.Detail_Screen_Space:
                case Overlay_A.Detail_Pixel_Space:
                    result.AddSamplerParameter("detail_map_a");
                    result.AddFloatParameter("detail_fade_a");
                    result.AddFloatParameter("detail_multiplier_a");
                    break;
                case Overlay_A.Detail_Masked_Screen_Space:
                    result.AddSamplerParameter("detail_map_a");
                    result.AddSamplerParameter("detail_mask_a");
                    result.AddFloatParameter("detail_fade_a");
                    result.AddFloatParameter("detail_multiplier_a");
                    break;
            }
            switch (overlay_b)
            {
                case Overlay_B.Tint_Add_Color when overlay_a != Overlay_A.Tint_Add_Color:
                    result.AddFloat4ColorParameter("tint_color");
                    result.AddFloat4ColorParameter("add_color");
                    break;
            }
            switch (blend_mode)
            {
                case Blend_Mode.Opaque:
                    break;
                default:
                    result.AddFloatParameter("fade");
                    break;
            }

            return result;
        }

        public ShaderParameters GetVertexShaderParameters()
        {
            if (!TemplateGenerationValid)
                return null;
            var result = new ShaderParameters();

            return result;
        }

        public ShaderParameters GetGlobalParameters()
        {
            var result = new ShaderParameters();
            return result;
        }

        public ShaderParameters GetParametersInOption(string methodName, int option, out string rmopName, out string optionName)
        {
            ShaderParameters result = new ShaderParameters();
            rmopName = null;
            optionName = null;

            if (methodName == "warp")
            {
                optionName = ((Warp)option).ToString();

                switch ((Warp)option)
                {
                    case Warp.Pixel_Space:
                    case Warp.Screen_Space:
                        result.AddSamplerParameter("warp_map");
                        result.AddFloatParameter("warp_amount");
                        rmopName = @"shaders\screen_options\warp_simple";
                        break;
                }
            }
            if (methodName == "base")
            {
                optionName = ((Base)option).ToString();

                switch ((Base)option)
                {
                    case Base.Single_Screen_Space:
                    case Base.Single_Pixel_Space:
                        result.AddSamplerParameter("base_map");
                        rmopName = @"shaders\screen_options\base_single";
                        break;
                }
            }
            if (methodName == "overlay_a")
            {
                optionName = ((Overlay_A)option).ToString();

                switch ((Overlay_A)option)
                {
                    case Overlay_A.Tint_Add_Color:
                        result.AddFloat4ColorParameter("tint_color");
                        result.AddFloat4ColorParameter("add_color");
                        rmopName = @"shaders\screen_options\overlay_tint_add_color";
                        break;
                    case Overlay_A.Detail_Screen_Space:
                    case Overlay_A.Detail_Pixel_Space:
                        result.AddSamplerParameter("detail_map_a");
                        result.AddFloatParameter("detail_fade_a");
                        result.AddFloatParameter("detail_multiplier_a");
                        rmopName = @"shaders\screen_options\detail_a";
                        break;
                    case Overlay_A.Detail_Masked_Screen_Space:
                        result.AddSamplerParameter("detail_map_a");
                        result.AddSamplerParameter("detail_mask_a");
                        result.AddFloatParameter("detail_fade_a");
                        result.AddFloatParameter("detail_multiplier_a");
                        rmopName = @"shaders\screen_options\detail_mask_a";
                        break;
                }
            }
            if (methodName == "overlay_b")
            {
                optionName = ((Overlay_B)option).ToString();

                switch ((Overlay_B)option)
                {
                    case Overlay_B.Tint_Add_Color:
                        result.AddFloat4ColorParameter("tint_color");
                        result.AddFloat4ColorParameter("add_color");
                        rmopName = @"shaders\screen_options\overlay_tint_add_color";
                        break;
                }
            }
            if (methodName == "blend_mode")
            {
                optionName = ((Blend_Mode)option).ToString();

                switch ((Blend_Mode)option)
                {
                    case Blend_Mode.Opaque:
                        break;
                    default:
                        result.AddFloatParameter("fade");
                        rmopName = @"shaders\screen_options\blend";
                        break;
                }
            }

            return result;
        }

        public Array GetMethodNames()
        {
            return Enum.GetValues(typeof(ScreenMethods));
        }

        public Array GetMethodOptionNames(int methodIndex)
        {
            switch ((ScreenMethods)methodIndex)
            {
                case ScreenMethods.Warp:
                    return Enum.GetValues(typeof(Warp));
                case ScreenMethods.Base:
                    return Enum.GetValues(typeof(Base));
                case ScreenMethods.Overlay_A:
                    return Enum.GetValues(typeof(Overlay_A));
                case ScreenMethods.Overlay_B:
                    return Enum.GetValues(typeof(Overlay_B));
                case ScreenMethods.Blend_Mode:
                    return Enum.GetValues(typeof(Blend_Mode));
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
