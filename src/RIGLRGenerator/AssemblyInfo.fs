﻿namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("RIGLRGenerator")>]
[<assembly: AssemblyProductAttribute("YaccConstructor")>]
[<assembly: AssemblyDescriptionAttribute("Platform for parser generators and other grammarware research and development.")>]
[<assembly: AssemblyVersionAttribute("0.1.1.0")>]
[<assembly: AssemblyFileVersionAttribute("0.1.1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.1.1.0"
    let [<Literal>] InformationalVersion = "0.1.1.0"
