﻿namespace Yard.Frontends.IronyFrontend

open Yard.Core

type IronyFrontend() = 
    interface IFrontend with
        member this.Name = "IronyFrontend"
        member this.ParseGrammar t = 
            match t with
            | :? Irony.Parsing.Grammar as g -> 
                {info = {fileName = ""};
                head = None;
                grammar = Converter.Convert g;
                foot = None;}
            | _ -> IL.Definition.empty
        member this.ProductionTypes = List.ofArray(Reflection.FSharpType.GetUnionCases typeof<IL.Production.t<string,string>>) |> List.map (fun unionCase -> unionCase.Name)
    end
