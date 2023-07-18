
namespace HaloShaderGenerator.Halogram
{
    public enum HalogramMethods
    {
        Albedo,
        Self_Illumination,
        Blend_Mode,
        Misc,
        Warp,
        Overlay,
        Edge_Fade,
        Distortion,
        Soft_Fade,
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
        Multilayer_Additive,
        Ml_Add_Four_Change_Color,
        Ml_Add_Five_Change_Color,
        Scope_Blur,
        Palettized_Plasma,
        Palettized_Plasma_Change_Color,
        Palettized_Depth_Fade,
    }

    public enum Blend_Mode
    {
        Opaque,
        Additive,
        Multiply,
        Alpha_Blend,
        Double_Multiply,
    }

    public enum Misc
    {
        First_Person_Never,
        First_Person_Sometimes,
        First_Person_Always,
        First_Person_Never_With_rotating_Bitmaps,
        Always_Calc_Albedo
    }

    public enum Warp
    {
        None,
        From_Texture,
        Parallax_Simple,
    }

    public enum Overlay
    {
        None,
        Additive,
        Additive_Detail,
        Multiply,
        Multiply_And_Additive_Detail,
    }

    public enum Edge_Fade
    {
        off,
        on,
        Simple,
    }

    public enum Soft_Fade
    {
        off,
        on
    }
}
