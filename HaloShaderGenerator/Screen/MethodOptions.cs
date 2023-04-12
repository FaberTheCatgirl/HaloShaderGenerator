using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloShaderGenerator.Screen
{
    public enum ScreenMethods
    {
        Warp,
        Base,
        Overlay_A,
        Overlay_B,
        Blend_Mode
    }

    public enum Warp
    {
        None,
        Pixel_Space,
        Screen_Space
    }

    public enum Base
    {
        Single_Screen_Space,
        Single_Pixel_Space
    }

    public enum Overlay_A
    {
        None,
        Tint_Add_Color,
        Detail_Screen_Space,
        Detail_Pixel_Space,
        Detail_Masked_Screen_Space,
    }

    public enum Overlay_B
    {
        None,
        Tint_Add_Color,
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
}
