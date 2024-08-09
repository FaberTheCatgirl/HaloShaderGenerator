using HaloShaderGenerator.DirectX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloShaderGenerator
{
    public class ShaderGeneratorResult
    {

        public class ShaderRegister
        {
            public ShaderRegister(string name, RegisterType type, int register, int size)
            {
                Name = name;
                registerType = type;
                Size = size;
                Register = register;
            }

            public int Size { get; }
            public int Register { get; }
            public string Name { get; }
            public RegisterType registerType { get; }
            public bool IsXFormArgument => Name.ToLower().Contains("_xform");
            public ShaderRegisterScope Scope => GetScope();


            public enum ShaderRegisterScope
            {
                TextureSampler_Arguments,
                WaterVector,
                UnknownA,
                UnknownB,
                Vector_Arguments,
                Integer_Arguments,
                Global_Arguments,
                RenderMethodExtern_Arguments,
                UnknownD,
                UnknownE,
                RenderMethodExternVector_Arguments,
                UnknownF,
                UnknownG,
                UnknownH,
                Indirect
            }

            public enum RegisterType
            {
                Vector,
                Boolean,
                Integer,
                Sampler
            }

            private ShaderRegisterScope GetScope()
            {
                switch (this.Name)
                {
                    case "none":
                    case "texture_global_target_texaccum":
                    case "texture_global_target_normal":
                    case "texture_global_target_z":
                    case "texture_global_target_shadow_buffer1":
                    case "texture_global_target_shadow_buffer2":
                    case "texture_global_target_shadow_buffer3":
                    case "texture_global_target_shadow_buffer4":
                    case "texture_global_target_texture_camera":
                    case "texture_global_target_reflection":
                    case "texture_global_target_refraction":
                    case "texture_lightprobe_texture":
                    case "texture_dominant_light_intensity_map":
                    case "texture_unused1":
                    case "texture_unused2":
                    case "object_change_color_primary":
                    case "object_change_color_secondary":
                    case "object_change_color_tertiary":
                    case "object_change_color_quaternary":
                    case "object_change_color_quinary":
                    case "object_change_color_primary_anim":
                    case "object_change_color_secondary_anim":
                    case "texture_dynamic_environment_map_0":
                    case "texture_dynamic_environment_map_1":
                    case "texture_cook_torrance_cc0236":
                    case "texture_cook_torrance_dd0236":
                    case "texture_cook_torrance_c78d78":
                    case "light_dir_0":
                    case "light_color_0":
                    case "light_dir_1":
                    case "light_color_1":
                    case "light_dir_2":
                    case "light_color_2":
                    case "light_dir_3":
                    case "light_color_3":
                    case "texture_unused_3":
                    case "texture_unused_4":
                    case "texture_unused_5":
                    case "texture_dynamic_light_gel_0":
                    case "flat_envmap_matrix_x":
                    case "flat_envmap_matrix_y":
                    case "flat_envmap_matrix_z":
                    case "debug_tint":
                    case "screen_constants":
                    case "active_camo_distortion_texture":
                    case "scene_ldr_texture":
                    case "scene_hdr_texture":
                    case "water_memory_export_address":
                    case "tree_animation_timer":
                    case "emblem_player_shoulder_texture":
                    case "emblem_clan_chest_texture":
                        // These trigger the engine to run the Render Method External function
                        return ShaderRegisterScope.RenderMethodExtern_Arguments;
                    case "lightprobe_texture_array":
                    case "depth_buffer":
                    case "alpha_map":
                    case "palette":
                    case "albedo_texture":
                    case "environment_map":
                    case "normal_texture":
                    case "dominant_light_intensity_map":
                    case "base_map":
                    case "detail_map":
                    case "warp_map":
                    case "detail_map2":
                    case "color_mask_map":
                    case "change_color_map":
                    case "height_map":
                    case "chameleon_mask_map":
                    case "detail_map3":
                    case "bump_map":
                    case "bump_detail_map":
                    case "bump_detail_mask_map":
                    case "detail_map_overlay":
                    case "self_illum_map":
                    case "specular_mask_texture":
                    case "self_illum_detail_map":
                    case "alpha_mask_map":
                    case "noise_map_a":
                    case "noise_map_b":
                    case "material_texture":
                    case "g_sampler_cc0236":
                    case "g_sampler_dd0236":
                    case "g_sampler_c78d78":
                    case "g_tree_animation_coeff":
                    case "ibr_texture":
                    case "dynamic_environment_map_0":
                    case "dynamic_environment_map_1":
                    case "alpha_test_map":
                    case "multiply_map":
                    case "shadow_depth_map_1":
                    case "blend_map":
                    case "dynamic_light_gel_texture":
                    case "tex_ripple_buffer_slope_height":
                    case "overlay_map":
                    case "overlay_detail_map":
                    case "overlay_intensity":
                    case "overlay_tint":
                    case "meter_map":
                    case "detail_map_a":
                    case "detail_mask_a":
                    case "base_map_m_0":
                    case "detail_map_m_0":
                    case "bump_map_m_0":
                    case "detail_bump_m_0":
                    case "base_map_m_1":
                    case "detail_map_m_1":
                    case "bump_map_m_1":
                    case "detail_bump_m_1":
                    case "base_map_m_2":
                    case "detail_map_m_2":
                    case "bump_map_m_2":
                    case "detail_bump_m_2":
                    case "overlay_multiply_map":
                    case "base_map_m_3":
                    case "detail_map_m_3":
                    case "bump_map_m_3":
                    case "detail_bump_m_3":
                    case "wave_slope_array":
                    case "watercolor_texture":
                    case "global_shape_texture":
                    case "wet_material_dim_coefficient":
                    case "wet_material_dim_tint":
                    case "wet_sheen_reflection_contribution":
                    case "wet_sheen_reflection_tint":
                    case "wet_sheen_thickness":
                    case "wet_noise_boundary_map":
                    case "specular_mask_tweak_weight":
                    case "surface_tilt_tweak_weight":
                    case "wet_flood_slope_map":
                    case "ceiling":
                    case "walls":
                    case "floors":
                    case "transform":
                    case "window_property_map":
                    case "distance_fade_scale":
                    case "environment_map_specular_contribution":
                    case "waterfall_base_mask":
                    case "waterfall_layer0":
                    case "waterfall_layer1":
                    case "waterfall_layer2":
                    case "transparency_frothy_weight":
                    case "transparency_base_weight":
                    case "transparency_bias":
                    case "interier":
                    case "mask_threshold":
                    case "thin_shell_height":
                    case "wall_map":
                    case "box_size":
                    case "fog_factor":
                    case "fog_top_color":
                    case "fog_bottom_color":
                    case "hole_radius":
                    case "animation_amplitude_horizontal":
                    case "foliage_translucency":
                    case "foliage_specular_color":
                    case "foliage_specular_intensity":
                    case "foliage_specular_power":
                    case "fur_hairs_map":
                    case "fur_tint_map":
                    case "fur_deep_color":
                    case "fur_tint_color":
                    case "fur_intensity":
                    case "fur_alpha_scale":
                    case "fur_shear_x":
                    case "fur_shear_y":
                    case "fur_fix":
                    case "layer_depth":
                    case "layers_of_4":
                    case "texcoord_aspect_ratio":
                    case "depth_darken":
                    case "bump_contrast":
                    case "bump_randomness":
                    case "contrast_scale":
                    case "contrast_offset":
                    case "distortion_scale":
                    case "sphere_warp_scale":
                    case "detail_multiplier_a":
                    case "fade":
                    case "stencil_map":
                    case "palette_v":
                    case "global_albedo_tint":
                    case "blend_target_0":
                    case "blend_target_1":
                    case "blend_target_2":
                    case "blend_target_3":
                    case "blend_offset":
                    case "blend_slope":
                    case "blend_max_0":
                    case "blend_max_1":
                    case "blend_max_2":
                    case "blend_max_3":
                    case "dynamic_material":
                    case "transition_sharpness":
                    case "transition_threshold":
                        return ShaderRegisterScope.TextureSampler_Arguments;
                    case "lightprobe_texture_array_xform":
                    case "depth_buffer_xform":
                    case "alpha_map_xform":
                    case "palette_xform":
                    case "albedo_texture_xform":
                    case "environment_map_xform":
                    case "normal_texture_xform":
                    case "dominant_light_intensity_map_xform":
                    case "base_map_xform":
                    case "detail_map_xform":
                    case "warp_map_xform":
                    case "detail_map2_xform":
                    case "color_mask_map_xform":
                    case "change_color_map_xform":
                    case "height_map_xform":
                    case "chameleon_mask_map_xform":
                    case "detail_map3_xform":
                    case "bump_map_xform":
                    case "bump_detail_map_xform":
                    case "bump_detail_mask_map_xform":
                    case "detail_map_overlay_xform":
                    case "self_illum_map_xform":
                    case "specular_mask_texture_xform":
                    case "self_illum_detail_map_xform":
                    case "alpha_mask_map_xform":
                    case "noise_map_a_xform":
                    case "noise_map_b_xform":
                    case "material_texture_xform":
                    case "g_sampler_cc0236_xform":
                    case "g_sampler_dd0236_xform":
                    case "g_sampler_c78d78_xform":
                    case "ibr_texture_xform":
                    case "dynamic_environment_map_0_xform":
                    case "dynamic_environment_map_1_xform":
                    case "alpha_test_map_xform":
                    case "multiply_map_xform":
                    case "shadow_depth_map_1_xform":
                    case "blend_map_xform":
                    case "dynamic_light_gel_texture_xform":
                    case "tex_ripple_buffer_slope_height_xform":
                    case "overlay_map_xform":
                    case "overlay_detail_map_xform":
                    case "meter_map_xform":
                    case "detail_map_a_xform":
                    case "detail_mask_a_xform":
                    case "base_map_m_0_xform":
                    case "detail_map_m_0_xform":
                    case "bump_map_m_0_xform":
                    case "detail_bump_m_0_xform":
                    case "base_map_m_1_xform":
                    case "detail_map_m_1_xform":
                    case "bump_map_m_1_xform":
                    case "detail_bump_m_1_xform":
                    case "base_map_m_2_xform":
                    case "detail_map_m_2_xform":
                    case "bump_map_m_2_xform":
                    case "detail_bump_m_2_xform":
                    case "overlay_multiply_map_xform":
                    case "base_map_m_3_xform":
                    case "detail_map_m_3_xform":
                    case "bump_map_m_3_xform":
                    case "detail_bump_m_3_xform":
                    case "wave_slope_array_xform":
                    case "watercolor_texture_xform":
                    case "global_shape_texture_xform":
                    case "wet_material_dim_coefficient_xform":
				    case "wet_material_dim_tint_xform":
				    case "wet_sheen_reflection_contribution_xform":
				    case "wet_sheen_reflection_tint_xform":
				    case "wet_sheen_thickness_xform":
				    case "wet_noise_boundary_map_xform":
				    case "specular_mask_tweak_weight_xform":
				    case "surface_tilt_tweak_weight_xform":
				    case "wet_flood_slope_map_xform":
				    case "ceiling_xform":
				    case "walls_xform":
				    case "floors_xform":
				    case "transform_xform":
				    case "window_property_map_xform":
				    case "distance_fade_scale_xform":
				    case "environment_map_specular_contribution_xform":
				    case "waterfall_base_mask_xform":
				    case "waterfall_layer0_xform":
				    case "waterfall_layer1_xform":
				    case "waterfall_layer2_xform":
				    case "transparency_frothy_weight_xform":
				    case "transparency_base_weight_xform":
				    case "transparency_bias_xform":
				    case "interier_xform":
				    case "mask_threshold_xform":
				    case "thin_shell_height_xform":
				    case "wall_map_xform":
				    case "box_size_xform":
				    case "fog_factor_xform":
				    case "fog_top_color_xform":
				    case "fog_bottom_color_xform":
				    case "hole_radius_xform":
				    case "animation_amplitude_horizontal_xform":
				    case "foliage_translucency_xform":
				    case "foliage_specular_color_xform":
				    case "foliage_specular_intensity_xform":
				    case "foliage_specular_power_xform":
				    case "fur_hairs_map_xform":
				    case "fur_tint_map_xform":
				    case "fur_deep_color_xform":
				    case "fur_tint_color_xform":
				    case "fur_intensity_xform":
				    case "fur_alpha_scale_xform":
				    case "fur_shear_x_xform":
				    case "fur_shear_y_xform":
				    case "fur_fix_xform":
				    case "layer_depth_xform":
				    case "layers_of_4_xform":
				    case "texcoord_aspect_ratio_xform":
				    case "depth_darken_xform":
				    case "bump_contrast_xform":
				    case "bump_randomness_xform":
				    case "contrast_scale_xform":
				    case "contrast_offset_xform":
				    case "distortion_scale_xform":
				    case "sphere_warp_scale_xform":
				    case "detail_multiplier_a_xform":
				    case "fade_xform":
				    case "stencil_map_xform":
				    case "palette_v_xform":
				    case "global_albedo_tint_xform":
				    case "blend_target_0_xform":
				    case "blend_target_1_xform":
				    case "blend_target_2_xform":
				    case "blend_target_3_xform":
				    case "blend_offset_xform":
				    case "blend_slope_xform":
				    case "blend_max_0_xform":
				    case "blend_max_1_xform":
				    case "blend_max_2_xform":
				    case "blend_max_3_xform":
				    case "dynamic_material_xform":
				    case "transition_sharpness_xform":
				    case "transition_threshold_xform":
                    //This shit felt like Yandare Simulator Code to write, why does this engine have so manu shaders! - Faber
                        return ShaderRegisterScope.Vector_Arguments;
                    case "g_exposure":
                    case "k_ps_active_camo_factor":
                    case "texture_size":
                        return ShaderRegisterScope.Indirect;
                    default:
                        Console.WriteLine($"Warning: Unknown ShaderRegisterScope for {this.Name}");
                        switch(registerType)
                        {
                            case RegisterType.Boolean:
                                return ShaderRegisterScope.Global_Arguments;
                            case RegisterType.Sampler:
                                return ShaderRegisterScope.TextureSampler_Arguments;
                            case RegisterType.Vector:
                                return ShaderRegisterScope.Vector_Arguments;
                            case RegisterType.Integer:
                                return ShaderRegisterScope.Integer_Arguments;
                            default:
                                throw new NotImplementedException();
                        }
                }
            }
        }

        public List<ShaderRegister> GetRegisters(string input)
        {
            List<ShaderRegister> result = new List<ShaderRegister>();
            bool found_registers = false;
            using (StringReader reader = new StringReader(input))
            {
                while (true)
                {
                    if (!found_registers)
                    {
                        var line = reader.ReadLine();
                        if (line == null)
                            break;
                        found_registers = line.Contains("Registers:");
                        if (found_registers)
                        {
                            reader.ReadLine();
                            reader.ReadLine();
                            reader.ReadLine();
                        }
                    }
                    else
                    {
                        var register_line = reader.ReadLine().Replace("//", "").Trim();
                        if (string.IsNullOrWhiteSpace(register_line)) break;

                        var register_components = register_line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        ShaderRegister.RegisterType register_type = ShaderRegister.RegisterType.Vector;

                        switch (register_components[1][0])
                        {
                            case 'b':
                                register_type = ShaderRegister.RegisterType.Boolean;
                                break;
                            case 'i':
                                register_type = ShaderRegister.RegisterType.Integer;
                                break;
                            case 'c':
                                register_type = ShaderRegister.RegisterType.Vector;
                                break;
                            case 's':
                                register_type = ShaderRegister.RegisterType.Sampler;
                                break;
                        }

                        var register = Int32.Parse(register_components[1].Substring(1));
                        var size = Int32.Parse(register_components[2]);

                        result.Add(new ShaderRegister(register_components[0], register_type, register, size));
                    }
                }
            }
            return result;
        }

        public Dictionary<string, string> GetRegisterParameters(string input)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            bool found_registers = false;
            using (StringReader reader = new StringReader(input))
            {
                while (true)
                {
                    if (!found_registers)
                    {
                        var line = reader.ReadLine();
                        if (line == null)
                            break;
                        found_registers = line.Contains("Parameters:");
                        if (found_registers)
                        {
                            reader.ReadLine();
                        }
                    }
                    else
                    {
                        var register_line = reader.ReadLine().Replace("//", "").Trim();
                        if (string.IsNullOrWhiteSpace(register_line)) break;

                        var register_components = register_line.Split(new char[] { ' ', ';', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);

                        result[register_components[1]] = register_components[0];
                    }
                }
            }
            return result;
        }

        public List<ShaderRegister> Registers = new List<ShaderRegister>();
        public byte[] Bytecode = null;

        public ShaderGeneratorResult(byte[] bytecode)
        {
            Bytecode = bytecode;
            if (bytecode == null) return;
            var result = D3DCompiler.Disassemble(bytecode);
            if (result == null) return;

            var registers = GetRegisters(result);
            var parameters = GetRegisterParameters(result);

            Registers = registers;
            /*
            foreach (var parameters_kp in parameters)
            {
                var name = parameters_kp.Key;
                var typename = parameters_kp.Value;

                ShaderRegister.RegisterType register_type;
                {
                    ShaderRegister.RegisterType? _register_type = null;

                    switch (typename)
                    {
                        case "float4":
                        case "float3":
                        case "float2":
                        case "float":
                            _register_type = ShaderRegister.RegisterType.Vector;
                            break;
                        case "int":
                            _register_type = ShaderRegister.RegisterType.Integer;
                            break;
                        case "bool":
                            _register_type = ShaderRegister.RegisterType.Boolean;
                            break;
                    }
                    if (typename.StartsWith("sampler"))
                    {
                        _register_type = ShaderRegister.RegisterType.Sampler;
                    }

                    register_type = _register_type ?? ShaderRegister.RegisterType.Vector;
                }

                foreach (var register in registers)
                {
                    if (register.Name == name && register.registerType == register_type)
                    {
                        // Integers have an associated Vector, find and add it
                        if (register_type == ShaderRegister.RegisterType.Integer)
                        {
                            foreach (var register2 in registers)
                            {
                                if (register2.Name == name && register2.registerType == ShaderRegister.RegisterType.Vector)
                                {
                                    Registers.Add(register2);
                                    break;
                                }
                            }
                        }

                        Registers.Add(register);
                        break;
                    }

                    // for bool compiled as float
                    else if (register.Name == name && 
                        register.registerType == ShaderRegister.RegisterType.Vector &&
                        register_type == ShaderRegister.RegisterType.Boolean)
                    {
                        Registers.Add(register);
                        break;
                    }

                    // slight hack, for int's that get compiled only as vector
                    else if (register.Name == name && register_type == ShaderRegister.RegisterType.Integer)
                    {
                        foreach (var register2 in registers)
                        {
                            if (register2.Name == name && register2.registerType == ShaderRegister.RegisterType.Vector)
                            {
                                Registers.Add(register2);
                                break;
                            }
                        }
                    }
                }
            }
            */
        }
    }

}
