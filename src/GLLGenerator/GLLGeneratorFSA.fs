﻿namespace Yard.Generators.GLL

open Mono.Addins
open Yard.Core
open IL
open Constraints
open Yard.Generators.Common
open InitialConvert
open Yard.Generators.Common.FSA
open Yard.Generators.GLL
open PrintTable
open Yard.Generators.GLL.TranslatorPrinter2
open Yard.Generators.Common.FSA.Common

open System.Collections.Generic


[<assembly:Addin>]
[<assembly:AddinDependency ("YaccConstructor", "1.0")>]
do()
[<Extension>]
type GLLFSA() = 
    inherit Generator()
        override this.Name = "GLLFSAGenerator"
        override this.Constraints = [|noMeta; noBrackets; singleModule(*; noInnerAlt*)|]
        override this.Generate (definition, args) =
            let start = System.DateTime.Now
            let args = args.Split([|' ';'\t';'\n';'\r'|]) |> Array.filter ((<>) "")
            let pairs = Array.zeroCreate <| args.Length / 2
            for i = 0 to pairs.Length - 1 do
                pairs.[i] <- args.[i * 2], args.[i * 2 + 1]
            let getOption name either f =
                match definition.options.TryFind name with
                | Some v -> f v
                | None -> either
            let getBoolOption opt either =
                getOption opt either <| function
                    | "true" -> true
                    | "false" -> false
                    | x -> failwithf "Option %s expected values true or false, but %s found." opt x
            let mapFromType t = Map.ofList ["_", Some t]
            let mutable moduleName = getOption "module" "" id

            let mutable tokenType = getOption "token" definition.tokens mapFromType
            let mutable fullPath = getBoolOption "fullpath" false
            let mutable positionType = getOption "pos" "" id
            let mutable needTranslate = getBoolOption "translate" false
            let mutable light = getBoolOption "light" true
            let mutable printInfiniteEpsilonPath = getOption "infEpsPath" "" id
            let mutable isAbstract = getBoolOption "abstract" true
            //let withoutTree = ref <| getBoolOption "withoutTree" true
            //let mutable caseSensitive = getBoolOption "caseSensitive" true
            let mutable output =
                let fstVal = getOption "output" (definition.info.fileName + ".fs") id
                getOption "o" fstVal id
            let getBoolValue name = function
                    | "true" -> true
                    | "false" -> false
                    | value -> failwithf "Unexpected %s value %s" name value

            for opt, value in pairs do
                match opt with
                | "-module" -> moduleName <- value
                | "-token" -> tokenType <- mapFromType value
                | "-pos" -> positionType <- value
                | "-o" -> if value.Trim() <> "" then output <- value
                | "-output" -> if value.Trim() <> "" then output <- value
                //| "-caseSensitive" -> caseSensitive <- getBoolValue "caseSensitive" value
                | "-fullpath" -> fullPath <- getBoolValue "fullPath" value
                | "-translate" -> needTranslate <- getBoolValue "translate" value
                | "-light" -> light <- getBoolValue "light" value
                | "-infEpsPath" -> printInfiniteEpsilonPath <- value
                | "-abstract" -> isAbstract <- getBoolValue "abstract" value
                //| "-withoutTree" -> withoutTree := getBoolValue "withoutTree" value
                | value -> failwithf "Unexpected %s option" value
                 
            //let newDefinition = initialConvert definition
            let fsa = new FSA(definition.grammar.[0].rules)

            //fsa.PrintDot @"C:\zgrviewer-0.10.0\dot\fsa_grammar.dot"
            
            use out = new System.IO.StreamWriter (output)
            let res = new System.Text.StringBuilder()
            let dummyPos = char 0
            let println (x : 'a) =
                Printf.kprintf (fun s -> res.Append(s).Append "\n" |> ignore) x
            let print (x : 'a) =
                Printf.kprintf (res.Append >> ignore) x
            let _class  =
                match moduleName with
                | "" -> if isAbstract then "AbstractParse" else "Parse"
                | s when s.Contains "." -> s.Split '.' |> Array.rev |> (fun a -> String.concat "." a.[1..])
                | s -> s
  
            let printHeaders moduleName fullPath light isAbstract =
                let n = output.Substring(0, output.IndexOf("."))
                let mName = 
                    if isAbstract then
                        "GLLFSA.AbstractParse." + n
                    else
                        "GLL.Parse"

                let fsHeaders() = 
                    //println "%s" <| getPosFromSource fullPath dummyPos (defaultSource output)
                    println "module %s"
                    <|  match moduleName with
                        | "" -> mName
                        | s -> s
                    if not light then
                        println "#light \"off\""
                    println "#nowarn \"64\";; // From fsyacc: turn off warnings that type variables used in production annotations are instantiated to concrete type"
                    //if isAbstract
                    //then
                        //println "open Yard.Generators.GLL.AbstractParser"
                        //println "open AbstractAnalysis.Common"
                    //println "open Yard.Generators.GLL.AbstractParserWithoutTreeFSAInput"
                    //else
//                        if !withoutTree
//                        then
//                            //println "open Yard.Generators.GLL.AbstractParserWithoutTree"
//                            println "open AbstractAnalysis.Common"
//                        else
//                            println "open Yard.Generators.GLL.Parser"
                    println "open Yard.Generators.GLL"
                    //println "open Yard.Generators.Common.ASTGLL"
                    println "open Yard.Generators.GLL.ParserCommon\n"

                    match definition.head with
                    | None -> ()
                    | Some (s : Source.t) ->
                        println "%s" <| getPosFromSource fullPath dummyPos s
                        println "%s" <| s.text + getPosFromSource fullPath dummyPos (defaultSource output)
                
                fsHeaders()
            
            let termToInt = new Dictionary<string,int>()

            let inline pack state token =
                if (int state < 65536) && (int token - fsa.States.Length < 65536)
                then int( (int state <<< 16) ||| (token - fsa.States.Length) )
                else failwith "State or token is greater then 65535!!"

            let printFSA () = 
                let nextInt = ref fsa.States.Length
                let eps = ref -1
                let stateTokenNewState = new ResizeArray<_>()
                           
                let fsaStatesOutNonterms = 
                    fsa.States
                    |> Array.mapi (fun i x ->
                        x
                        |> Array.collect (fun (symbol,state) -> 
                            match symbol with
                                | Nonterm s -> [|s.ToString(), state|]
                                | Term s -> 
                                    let cond, value = termToInt.TryGetValue s
                                    if cond then
                                        stateTokenNewState.Add(i, value, state)
                                    //    value.ToString(), state
                                    else
                                        termToInt.Add(s,!nextInt)
                                        stateTokenNewState.Add(i, !nextInt, state)
                                        incr nextInt
                                    //    (!nextInt-1).ToString(), state
                                    [||]
                                | Epsilon() -> failwith "Found epsilon edge while print fsa."
                                    (*if !eps = -1 then
                                        eps := !nextInt
                                        incr nextInt
                                        (!nextInt-1).ToString(), state
                                    else
                                        (!eps).ToString(), state*)
                                        ))
                    |> List.ofSeq

                let printState (state:(string * int<positionInGrammar>) []) isFirst isLast =
                    let prefix = if isFirst then "  [|" else "    "
                    let postfix = if isLast then " |]" else ";"

                    let printEdge (str,state) isFirst isLast = 
                        let prefix = if isFirst then "[|" else ""
                        let postfix = if isLast then "|]" else ";"
                        print "%s%s<positionInGrammar>,%s<positionInGrammar>%s" prefix str (state.ToString()) postfix
                    
                    print "%s" prefix

                    if state.Length = 0
                    then
                        print "[||]"
                    else
                        state
                        |> Array.iteri (fun i edge -> printEdge edge (i = 0) (i = state.Length-1))

                    println "%s" postfix
                
                println "type Token ="
                for token in termToInt.Keys do
                    println "    | %s of unit" token
                println ""

                println "let tokenToNumber = function"
                for tokenNumber in termToInt do
                    println "    | %s() -> %i" tokenNumber.Key tokenNumber.Value
                println ""

                println "let stateToNontermName = function"
                for tokenNumber in termToInt do
                    println "    | %i -> \"%s\"" tokenNumber.Value tokenNumber.Key
                let sortedStateToNontermName = 
                    fsa.StateToNontermName
                    |> Seq.sortBy (fun x -> x.Key)
                for numberNonterm in sortedStateToNontermName do
                    println "    | %i -> \"%s\"" numberNonterm.Key numberNonterm.Value
                println "    | _ -> \"\""
                println ""

                let numOfAnyState = 
                    sortedStateToNontermName
                    |> Seq.tryFind (fun x -> x.Value.Equals("any"))
                    |> (fun x -> 
                        match x with
                        | Some i -> i.Key
                        | _ -> -1<positionInGrammar>)

                println "let private numOfAnyState = %i<positionInGrammar>" numOfAnyState
                println ""

                println "let private numIsTerminal = function"
                for i in termToInt.Values do
                    println "    | %i -> true" i
                println "    | _ -> false"
                println ""
                (*
                println "let numIsEpsilon = function"
                println "    | %i -> true" !eps
                println "    | _ -> false\n"
                *)

                println "let private stateAndTokenToNewState = new System.Collections.Generic.Dictionary<int, int<positionInGrammar>>()"
                for state, token, newState in stateTokenNewState do
                    println "stateAndTokenToNewState.Add(%i, %i<positionInGrammar>)" (pack state token) newState
                println ""

                println "let private outNonterms ="
                fsaStatesOutNonterms    
                |> List.iteri (fun i state -> printState state (i = 0) (i = fsaStatesOutNonterms.Length-1))
                println ""
//                println "let states = "
//                println "    outNontermsToConvert"
//                println "    |> Array.Parallel.map (fun x -> "
//                println "        x"
//                println "        |> Array.map (fun (x,y) -> x, y * 1<positionInGrammar>))"
//                println ""

                println "let private startState = %i<positionInGrammar>" fsa.StartState
                println ""

                let printFinalStates () =
                    let printState state isFirst isLast = 
                        let prefix = if isFirst then "  new System.Collections.Generic.HashSet<int<positionInGrammar>>(\n     [|" else "       "
                        let postfix = if isLast then "|])" else ";"

                        println "%s%i<positionInGrammar>%s" prefix state postfix

                    let i = ref 0
                    for state in fsa.FinalStates do
                        printState state (!i = 0) (!i = fsa.FinalStates.Count - 1)
                        incr i

                println "let private finalStates ="
                printFinalStates ()
                println ""

                println "let private nontermCount = %i" fsa.NontermCount
                
//            let printFirstSet () =
//                println "let firstSet ="
//                
//                let printState state (terms : string []) isFirst isLast = 
//                    let prefix = if isFirst then "  new System.Collections.Generic.HashSet<int>(\n     [|" else "       "
//                    let postfix = if isLast then "|])" else ";"
//
//                    let printTerm term isFirst isLast =
//                        //let prefix = if isFirst then "" else "      "
//                        let postfix = if isLast then "" else ";"
//                        let packedValue = pack state <| termToInt.[term]
//                        print "%i%s"  packedValue postfix
//
//                    print "%s" prefix
//
//                    let i = ref 0
//                    for term in terms do
//                        printTerm term (!i = 0) (!i = terms.Length - 1)
//                        incr i
//
//                    println "%s" postfix
//
//                let i = ref 0
//                for state, stateTerms in fsa.FirstSet do
//                    printState state stateTerms (!i = 0) (!i = fsa.FirstSet.Length - 1)
//                    incr i
            
            let printParser () =
                println "let private parserSource = new FSAParserSourceGLL (outNonterms, startState, finalStates, nontermCount, numIsTerminal, stateToNontermName, numOfAnyState, stateAndTokenToNewState)"

            let printFuns () =
                println "let buildAbstract : (AbstractAnalysis.Common.BioParserInputGraph -> ParserCommon.ParseResult<_>) ="
                println "    Yard.Generators.GLL.AbstractParserWithoutTreeFSAInput.buildAbstract parserSource"

            
            let printItem printer = 
                printer ()
                println ""

            printHeaders moduleName fullPath light isAbstract
            printItem printFSA
            //printItem printFirstSet
            printItem printParser
            printItem printFuns

            let res = 
                match definition.foot with
                | None -> res
                | Some (s : Source.t) ->
                    res
                    //res + (getPosFromSource fullPath dummyPos s + "\n"
                    //            + s.text + getPosFromSource fullPath dummyPos (defaultSource output) + "\n")
            (*let res =
                let init = res.Replace("\r\n", "\n")
                let curLine = ref 1// Must be 2, but there are (maybe) some problems with F# compiler, causing to incorrect warning
                let res = new System.Text.StringBuilder(init.Length * 2)
                for c in init.ToString() do
                    match c with
                    | '\n' -> incr curLine; res.Append System.Environment.NewLine
                    | c when c = dummyPos -> res.Append (string !curLine)
                    | x -> res.Append x
                    |> ignore
                res.ToString()*)
            out.WriteLine (res.ToString())
            out.Flush()
            out.Close()
            eprintfn "Generation time: %A" <| System.DateTime.Now - start
            
            box ()
        override this.Generate definition = this.Generate (definition, "")