using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloShaderGenerator.WetnessOptions //Add Temp Option to apease compiler for now
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