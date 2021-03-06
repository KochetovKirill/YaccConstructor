﻿namespace Yard.Generators.GLL.ParserCommon

open AbstractAnalysis.Common
open Yard.Generators.Common.ASTGLL
open FSharpx.Collections.Experimental



[<Measure>] type vertexMeasure
[<Measure>] type nodeMeasure
[<Measure>] type labelMeasure
[<Measure>] type stackMeasure

[<Struct>]
type Vertex =
    val Level            : int
    val NontermLabel     : int
    new (level, nonterm) = {Level = level; NontermLabel = nonterm}

[<Struct>]
type CallStackVertex =
    val ReturnLabel : int
    val Predecessor : int<stackMeasure>
    new (label, pred) = {ReturnLabel = label; Predecessor = pred}

[<Struct>]
type Context(*<'TokenType>*) =
    val Index         : int
    val Label         : int<labelMeasure>
    val Vertex        : Vertex
    val Ast           : int<nodeMeasure>
    //val CurrentR      : int<nodeMeasure>
    //val Path          : List<ParserEdge<'TokenType*ref<bool>>>
    new (index, label, vertex, ast(*, curR*)) = {Index = index; Label = label; Vertex = vertex; Ast = ast(*; CurrentR = curR*)} // Path = List.empty<ParserEdge<'TokenType*ref<bool>>>
    //new (index, label, vertex, ast, path) = {Index = index; Label = label; Vertex = vertex; Ast = ast; Path = path}

[<Struct>]
type ContextGFG =
    val Index           : int
    val Label           : int<labelMeasure>
    val Vertex          : Vertex
    val CallStackVertex : int<stackMeasure>
    val Ast             : int<nodeMeasure>
    new (index, label, vertex, csVertex, ast) = {Index = index; Label = label; Vertex = vertex; CallStackVertex = csVertex; Ast = ast}

type ParseResult<'TokenType> =
    | Success of Tree<'TokenType>
    | Error of string


module CommonFuns = 

    let inline pack left right : int64 =  ((int64 left <<< 32) ||| int64 right)
    let inline pack3 l m r : int64 =  ((int64 l <<< 42) ||| (int64 m <<< 21) ||| int64 r)
    let inline getRight (long : int64) = int <| ((int64 long) &&& 0xffffffffL)
    let inline getLeft (long : int64)  = int <| ((int64 long) >>> 32)

    let inline packVertex level label: int64<vertexMeasure> =  LanguagePrimitives.Int64WithMeasure ((int64 level <<< 32) ||| int64 label)
    let inline getIndex1Vertex (long : int64<vertexMeasure>)       = int <| ((int64 long) &&& 0xffffffffL)
    let inline getIndex2Vertex (long : int64<vertexMeasure>)       = int <| ((int64 long) >>> 32)
 

    let inline packLabel rule position = ((int rule <<< 16) ||| int position)*1<labelMeasure>
    let inline getRule (packedValue : int<labelMeasure>)  = int packedValue >>> 16
    let inline getPosition (packedValue : int<labelMeasure>) = int (int packedValue &&& 0xffff)

type ParserStructures<'TokenType> (inputLength : int, currentRule : int)=
    let sppfNodes = new BlockResizeArray<INode>()
    let dummyAST = new TerminalNode(-1, packExtension -1 -1)
    let setP = new System.Collections.Generic.Dictionary<int64, Yard.Generators.Common.DataStructures.ResizableUsualOne<int<nodeMeasure>>>(500)//list<int<nodeMeasure>>> (500)
    let epsilonNode = new TerminalNode(-1, packExtension 0 0)
    let setR = new System.Collections.Generic.Queue<Context(*<'TokenType>*)>(100)  
    let dummy = 0<nodeMeasure>
    let currentN = ref <| dummy
    let currentR = ref <| dummy
    let resultAST = ref None
    do 
        sppfNodes.Add(dummyAST)
        sppfNodes.Add(epsilonNode)

    let currentLabel = ref <| CommonFuns.packLabel currentRule 0
    
    let getNodeP findSppfNode (findSppfPackedNode : int<nodeMeasure> -> int<labelMeasure> -> int64<extension> -> int64<extension> -> INode -> INode -> int<nodeMeasure>) dummy (label : int<labelMeasure>) (left : int<nodeMeasure>) (right : int<nodeMeasure>) : int<nodeMeasure> =
            let currentRight = sppfNodes.Item (int right)
            let rightExt = 
                match currentRight with                    
                    | :? NonTerminalNode as nonTerm ->
                        nonTerm.Extension
                    | :? IntermidiateNode as interm ->
                        interm.Extension
                    | :? TerminalNode as term ->
                        term.Extension   
                    | _ -> failwith "Smth strange, Nastya"             
            if left <> dummy
            then
                let currentLeft = sppfNodes.Item (int left)
                let leftExt =
                    match currentLeft with                    
                    | :? NonTerminalNode as nonTerm ->
                        nonTerm.Extension
                    | :? IntermidiateNode as interm ->
                        interm.Extension
                    | :? TerminalNode as term ->
                        term.Extension 
                    | _ -> failwith "Smth strange, Nastya" 
                    
                let y = findSppfNode label (getLeftExtension leftExt) (getRightExtension rightExt)
                ignore <| findSppfPackedNode y label leftExt rightExt currentLeft currentRight
                y
            else
                let y = findSppfNode label (getLeftExtension rightExt) (getRightExtension rightExt)
                ignore <| findSppfPackedNode y label rightExt rightExt dummyAST currentRight 
                y
                                 
    let containsContext (setU : System.Collections.Generic.Dictionary<_, System.Collections.Generic.Dictionary<_, ResizeArray<_>>>[]) inputIndex (label : int<labelMeasure>) (vertex : Vertex) (ast : int<nodeMeasure>) =
        if inputIndex <= inputLength
        then
            let vertexKey = CommonFuns.pack vertex.Level vertex.NontermLabel
            if setU.[inputIndex] <> Unchecked.defaultof<_>
            then
                let cond, current = setU.[inputIndex].TryGetValue(int label) 
                if  cond then
                    if current.ContainsKey(vertexKey) then
                        let trees = current.[vertexKey]
                        if not <| trees.Contains(ast)
                        then 
                            trees.Add(ast)
                            false
                        else
                            true
                    else 
                        let arr = new ResizeArray<int<nodeMeasure>>()
                        arr.Add(ast)
                        current.Add(vertexKey, arr)                    
                        false
                else 
                    let dict = new System.Collections.Generic.Dictionary<_, ResizeArray<_>>()
                    setU.[inputIndex].Add(int label, dict)
                    let arr = new ResizeArray<int<nodeMeasure>>()
                    arr.Add(ast)
                    dict.Add(vertexKey, arr) 
                    false
            else 
                let dict1 =  new System.Collections.Generic.Dictionary<_, _>()
                setU.[inputIndex] <- dict1
                let dict2 = new System.Collections.Generic.Dictionary<_, ResizeArray<_>>()
                dict1.Add(int label, dict2)
                let arr = new ResizeArray<int<nodeMeasure>>()
                arr.Add(ast)
                dict2.Add(vertexKey, arr)
                false
        else true

    let pushContext (inputVertex : int) (label : int<labelMeasure>) vertex ast (*curR*) =
        setR.Enqueue(new Context(inputVertex, label, vertex, ast(*, curR*)))
        
    let addContext (setU : System.Collections.Generic.Dictionary<_, System.Collections.Generic.Dictionary<_, ResizeArray<_>>>[]) (inputVertex : int) (label : int<labelMeasure>) vertex ast (*currentPath*) =
        if not <| containsContext setU inputVertex label vertex ast
        then
            setR.Enqueue(new Context(inputVertex, label, vertex, ast(*, dummy, currentPath*)))

    let containsEdge (dict1 : System.Collections.Generic.Dictionary<int<nodeMeasure>, System.Collections.Generic.Dictionary<int, ResizeArray<int>>>) ast (e : Vertex) =
        if dict1 <> Unchecked.defaultof<_>
            then
                if dict1.ContainsKey(ast)
                then
                    let dict2 = dict1.[ast]
                    if dict2.ContainsKey(e.NontermLabel)
                    then
                        let t = dict2.[e.NontermLabel]
                        if t.Contains(e.Level) then true, None
                        else 
                            t.Add(e.Level) 
                            false, None 
                    else
                        let arr = new ResizeArray<int>()
                        arr.Add(e.Level) 
                        dict2.Add(e.NontermLabel, arr)
                        false, None
                else
                    let d = new System.Collections.Generic.Dictionary<int, ResizeArray<int>>()
                    dict1.Add(ast, d)
                    let l = new ResizeArray<int>()
                    l.Add(e.Level)
                    d.Add(e.NontermLabel, l)
                    false, None
            else
                let newDict1 = new System.Collections.Generic.Dictionary<int<nodeMeasure>, System.Collections.Generic.Dictionary<int, ResizeArray<int>>>()
                let newDict2 = new System.Collections.Generic.Dictionary<int, ResizeArray<int>>()
                let newArr = new ResizeArray<int>()
                newArr.Add(e.Level)
                newDict2.Add(e.NontermLabel, newArr)
                newDict1.Add(ast, newDict2)
                false, Some newDict1   
                
    let getTreeExtension (node : int<nodeMeasure>) =
        match sppfNodes.Item (int node) with
            | :? TerminalNode as t ->
                t.Extension
            | :? IntermidiateNode as i ->
                i.Extension
            | :? NonTerminalNode as n ->
                n.Extension
            | _ -> failwith "Bad type for tree node"   

    let finalMatching (curRight : INode) nontermName finalExtensions findSppfNode findSppfPackedNode currentGSSNode currentVertexInInput (pop : Vertex -> int -> int<nodeMeasure> -> unit)  = 
        match curRight with
        | :? TerminalNode as t ->
            currentN := getNodeP findSppfNode findSppfPackedNode dummy !currentLabel !currentR !currentN
            let r = (sppfNodes.Item (int !currentN)) :?> NonTerminalNode 
            pop !currentGSSNode !currentVertexInInput !currentN
        | :? NonTerminalNode as r ->
            if (r.Name = nontermName) && (Array.exists (fun a -> a = r.Extension) finalExtensions)
            then 
                match !resultAST with
                    | None ->  
                        resultAST := Some r
                    | Some a -> 
                         a.AddChild r.First
                
                         
            pop !currentGSSNode !currentVertexInInput !currentN
        | x -> failwithf "Unexpected node type in ASTGLL: %s" <| x.GetType().ToString()
          
                 
             
    member this.GetNodeP = getNodeP
    member this.SetP = setP
    member this.EpsilonNode = epsilonNode
    member this.SetR = setR
    member this.SppfNodes = sppfNodes
    member this.DummyAST = dummyAST
    member this.PushContext = pushContext
    member this.ContainsContext = containsContext
    member this.AddContext = addContext
    member this.ContainsEdge = containsEdge
    member this.GetTreeExtension = getTreeExtension
    member this.Dummy = dummy
    member this.CurrentN = currentN
    member this.CurrentR = currentR 
    member this.ResultAST = resultAST
    member this.CurrentLabel = currentLabel
    member this.FinalMatching = finalMatching