﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloShaderGenerator.Globals
{
    public enum ShaderSubtype
    {
        Pixel,
        Vertex,
        SharedPixel,
        SharedVertex
    }

    public enum ShaderType
    {
        Shader,
        Beam,
        Contrail,
        Decal,
        Halogram,
        LightVolume,
        Particle,
        Terrain,
        Cortana,
        Water,
        Black,
        Screen,
        Custom,
        Foliage,
        ZOnly
    }

    public enum ChudShader
    {
        chud_simple,
        chud_meter,
        chud_text_simple,
        chud_meter_shield,
        chud_meter_gradient,
        chud_crosshair,
        chud_directional_damage,
        chud_solid,
        chud_sensor,
        chud_meter_single_color,
        chud_navpoint,
        chud_medal,
        chud_texture_cam,
        chud_cortana_screen,
        chud_cortana_camera,
        chud_cortana_offscreen,
        chud_cortana_screen_final,
        chud_meter_chapter,
        chud_meter_double_gradient,
        chud_meter_radial_gradient,
        chud_turbulence,
        chud_emblem,
        chud_cortana_composite,
        chud_directional_damage_apply,
        chud_really_simple,
        chud_unknown
    }

    public enum ExplicitShader
    {
        debug,
        debug2d,
        copy_surface,
        spike_blur_vertical,
        spike_blur_horizontal,
        downsize_2x_to_bloom,
        downsize_2x_target,
        copy_rgbe_to_rgb,
        update_persistence,
        add_downsampled,
        add,
        blur_11_horizontal,
        blur_11_vertical,
        cubemap_phi_blur,
        cubemap_theta_blur,
        cubemap_clamp,
        cubemap_divide,
        write_depth,
        final_composite,
        sky_dome_simple,
        transparent,
        shield_meter,
        legacy_meter,
        overhead_map_geometry,
        legacy_hud_bitmap,
        blend3,
        particle_update,
        particle_spawn,
        screenshot_combine,
        downsample_2x2,
        rotate_2d,
        bspline_resample,
        downsample_4x4_bloom_dof,
        final_composite_dof,
        kernel_5,
        exposure_downsample,
        yuv_to_rgb,
        displacement,
        screenshot_display,
        downsample_4x4_block,
        crop,
        screenshot_combine_dof,
        gamma_correct,
        contrail_spawn,
        contrail_update,
        stencil_stipple,
        lens_flare,
        decorator_default,
        downsample_4x4_block_bloom,
        downsample_4x4_gaussian,
        apply_color_matrix,
        copy,
        shadow_geometry,
        shadow_apply,
        gradient,
        alpha_test_explicit,
        patchy_fog,
        light_volume_update,
        water_ripple,
        double_gradient,
        sniper_scope,
        shield_impact,
        player_emblem_world,
        player_emblem_screen,
        implicit_hill,
        chud_overlay_blend,
        bloom_add_alpha1,
        downsample_4x4_block_bloom_ldr,
        restore_ldr_hdr_depth,
        beam_update,
        decorator_no_wind,
        decorator_static,
        decorator_sun,
        decorator_wavy,
        final_composite_zoom,
        final_composite_debug,
        shadow_apply_point,
        shadow_apply_bilinear,
        shadow_apply_fancy,
        shadow_apply_faster,
        shadow_apply2,
        displacement_motion_blur,
        decorator_shaded,
        screenshot_memexport,
        downsample_4x4_gaussian_bloom_ldr,
        downsample_4x4_gaussian_bloom,
        downsample_4x4_block_bloom_new,
        bloom_curve,
        custom_gamma_correct,
        pixel_copy,
        unknown_5A,
        exposure_hdr_retrieve,
        unknown_debug_5C,
        fxaa,
        unknown_5E,
        unknown_5F,
        ssao_ldr,
        ssao_hdr,
        ssao_apply,
        lightshafts,
        lightshafts_blur,
        screen_space_reflection,
        unknown_66,
        halve_depth_color,
        halve_depth_normal,
        unknown_69,
        screen_space_reflection_blur,
        unknown_6B,
        hud_camera_nightvision,
        unknown_6D
    }
}
