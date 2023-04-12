using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloShaderGenerator.Shader
{
    public enum ShaderMethods
    {
        Albedo,
        Bump_Mapping,
        Alpha_Test,
        Specular_Mask,
        Material_Model,
        Environment_Mapping,
        Self_Illumination,
        Blend_Mode,
        Parallax,
        Misc,
        Misc_Attr_Animation,
        Distortion,
        Soft_Fade
    }

    public enum Albedo
    {
        Default,
        Detail_Blend,
        Constant_Color,
        Two_Change_Color,
        Four_Change_Color,
        Three_Detail_Blend,
        Two_Detail_Overlay,
        Two_Detail,
        Color_Mask,
        Two_Detail_Black_Point,
        Two_Change_Color_Anim_Overlay,
        Chameleon,
        Two_Change_Color_Chameleon,
        Chameleon_Masked,
        Color_Mask_Hard_Light,
        Four_Change_Color_Applying_To_Specular,
        Simple,
        Two_Change_Color_Tex_Overlay,
        Chameleon_Albedo_Masked,
        Custom_Cube,
        Two_Color,
        Emblem,
        Scrolling_Cube,
        Scrolling_Texture_Uv,
        Texture_From_Misc
    }

    public enum Bump_Mapping
    {
        Off,
        Standard,
        Detail,
        Detail_Masked,
        Detail_Plus_Detail_Masked,
        detail_plus_unorm //  typo in the shader, not my fault. waiting on a fix from the devs at 343 industries.
    }

    public enum Alpha_Test
    {
        None,
        Simple
    }

    public enum Specular_Mask
    {
        No_Specular_Mask,
        Specular_Mask_From_Diffuse,
        Specular_Mask_From_Texture,
        Specular_Mask_From_Color_Texture
    }

    public enum Material_Model
    {
        Diffuse_Only,
        Cook_Torrance,
        Two_Lobe_Phong,
        Two_Lobe_Phong_Tint_Map,
        Foliage,
        None,
        Glass,
        Organism,
        Single_Lobe_Phong,
        Car_Paint,
        Hair,
        Cook_Torrance_Odst,
        Cook_Torrance_Custom_Cube,
        Cook_Torrance_Pbr_Maps,
        Cook_Torrance_Two_Color_Spec_Tint,
        Cook_Torrance_Scrolling_Cube,
        Cook_Torrance_Scrolling_Cube_Mask,
        Cook_Torrance_Rim_Fresnel,
        Cook_Torrance_From_Albedo,
        Constant
    }

    public enum Environment_Mapping
    {
        None,
        Per_Pixel,
        Dynamic,
        From_Flat_Texture,
        From_Flat_Texture_As_Cubemap,
        Custom_Map,
        Dynamic_Reach
    }

    public enum Self_Illumination
    {
        Off,
        Simple,
        _3_Channel_Self_Illum,
        Plasma,
        From_Diffuse,
        Illum_Detail,
        Meter,
        Self_Illum_Times_Diffuse,
        Simple_With_Alpha_Mask,
        Simple_Four_Change_Color,
        Illum_Detail_World_Space_Four_Cc,
        Illum_Change_Color,
        Multilayer_Additive,
        Palettized_Plasma,
        Change_Color_Detail,
    }

    public enum Blend_Mode
    {
        Opaque,
        Additive,
        Multiply,
        Alpha_Blend,
        Double_Multiply,
        Pre_Multiplied_Alpha
    }

    public enum Parallax
    {
        Off,
        Simple,
        Interpolated,
        Simple_Detail
    }

    public enum Misc
    {
        First_Person_Never,
        First_Person_Sometimes,
        First_Person_Always,
        First_Person_Never_With_rotating_Bitmaps,
        Always_Calc_Albedo
    }
    public enum Misc_Attr_Animation
    {
        Off,
        Scrolling_Cube,
        Scrolling_Projected
    }

    public enum DistortionOptions
    {
        Off,
        On
    }
}
