using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloShaderGenerator.Wetness
{
    public enum WetnessMethods
    {
        //default, //Another lovely default method that does nothing. thanks csharp.
        flood,
        proof,
        simple,
        ripples
    }
}