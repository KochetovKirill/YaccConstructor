﻿module AntlrToYard.Parser

open Microsoft.FSharp.Text.Lexing
open FParsec.CharParsers

open AntlrToYard.Lexer
open AntlrToYard.Tokens

let testParser = function
    | Success (v, _, _)  -> printfn "%s" (v.ToString())
    | Failure (msg, err, _) -> printf "%s" msg
    | _ -> printf "???"


let () =

    let testPath = ref @"..\.."
    let testFile = ref "test.g"

    let commandLineSpecs =
        ["--testpath", ArgType.String (fun s -> testPath := s), "Directory where test files are placed"
         "-t", ArgType.String (fun s -> testFile := s), "Name of test file"
         ] |> List.map (fun (shortcut, argtype, description) -> ArgInfo(shortcut, argtype, description))
    let commandLineArgs = System.Environment.GetCommandLineArgs()
    ArgParser.Parse commandLineSpecs

    let content = System.IO.File.ReadAllText(!testPath + "\\" + !testFile)
    let reader = new System.IO.StringReader(content)
    let lexbuf = LexBuffer<_>.FromTextReader reader//LexBuffer<_>.FromChars  ("abc/* def */foo".ToCharArray())
    let lexems = seq {
                       while not lexbuf.IsPastEndOfStream do
                             yield AntlrToYard.Lexer.main lexbuf  
                      }
    //let token = AntlrLexer.main lexbuf
    //testParser k
    let a = Seq.toList lexems
    let b = sprintf "%A" a
    printf "%s" b