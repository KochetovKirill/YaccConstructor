﻿module AbstractFsLex.Test

open Graphviz4Net.Dot.AntlrParser
open System.IO
open Graphviz4Net.Dot
open QuickGraph
open NUnit.Framework
open AbstractLexer.Common
open AbstractLexer.Core
open QuickGraph.Algorithms
open AbstractLexer.Test.Calc.Parser
open QuickGraph.Algorithms
open QuickGraph.Graphviz

let loadGraphFromDOT filePath = 
    let parser = AntlrParserAdapter<string>.GetParser()
    parser.Parse(new StreamReader(File.OpenRead filePath))

let baseInputGraphsPath = "../../../Tests/AbstractLexing/DOT"
let eofToken = AbstractLexer.Test.Calc.Parser.RNGLR_EOF ("",[||])
let literalEofToken = AbstractLexer.Test.Literals.Parser.RNGLR_EOF

[<TestFixture>]
type ``Abstract lexer tests`` () =    

    let printG res (fName:string) =
        let f = GraphvizAlgorithm(res)
        let printEdg (e:AbstractParsing.Common.ParserEdge<_>) =
            let printBrs brs =
                "["
                + (brs |> Array.map (fun (pos:Position<_>) -> pos.back_ref ) |> String.concat "; ")
                + "]"
            match e.Tag with
            | NUMBER(v,br) -> "NUM: " + v + "; br= " + printBrs br
            | PLUS(v,br)   -> "+: " + v  + printBrs br
            | MULT(v,br)   ->  "*: " + v  + printBrs br
            | DIV(v,br)   ->  "/: " + v  + printBrs br
            | LBRACE(v,br)   ->  "(: " + v  + printBrs br
            | RBRACE(v,br)   ->  "): " + v  + printBrs br
            | e -> string e 
        f.FormatEdge.Add(fun e -> (e.EdgeFormatter.Label.Value <- printEdg e.Edge))
        let str = f.Generate()
        let c = System.IO.Path.GetInvalidFileNameChars()
        let fName1 = c |> Array.fold (
                                       fun (name:string) ch -> name.Replace(ch,'_')) fName
        System.IO.File.WriteAllText(@"../../" + fName1 + ".dot" ,str)

    let path name = System.IO.Path.Combine(baseInputGraphsPath,name)

    let loadDotToQG gFile =
        let g = loadGraphFromDOT(path gFile)
        let qGraph = new AdjacencyGraph<int, TaggedEdge<_,string>>()
        g.Edges 
        |> Seq.iter(
            fun e -> 
                let edg = e :?> DotEdge<string>
                qGraph.AddVertex(int edg.Source.Id) |> ignore
                qGraph.AddVertex(int edg.Destination.Id) |> ignore
                qGraph.AddEdge(new TaggedEdge<_,_>(int edg.Source.Id,int edg.Destination.Id,edg.Label)) |> ignore)
        qGraph

    let loadLexerInputGraph gFile =
        let qGraph = loadDotToQG gFile
        let lexerInputG = new LexerInputGraph<_>()
        lexerInputG.StartVertex <- 0
        for e in qGraph.Edges do lexerInputG.AddEdgeForsed (new LexerEdge<_,_>(e.Source,e.Target,Some(e.Tag, e.Tag+"|")))
        lexerInputG
        
    let checkArr expectedArr actualArr =
        if Array.length expectedArr = Array.length actualArr
        then 
            Array.iteri2 (
                fun i x1 x2 -> 
                if x1 <> x2 then Assert.Fail ("Arrays differ at position: " + string i)) expectedArr actualArr
            Assert.Pass()
        else Assert.Fail ("Arrays have different length")

    let check_brs = 
       Seq.iter
        (fun (e:AbstractParsing.Common.ParserEdge<_>) -> 
                match e.Tag with
                | NUMBER (n,brs) 
                | PLUS (n,brs) ->
                    Assert.AreEqual(brs.Length, n.Length)
                    Assert.IsTrue(brs |> Array.map (fun i -> i.back_ref)|>Array.forall((=) (n + "|")))
                | RNGLR_EOF _ -> () 
                | t -> Assert.Fail(sprintf "Unexpected token: %A" t) 
                )

    let checkGraph (g : AdjacencyGraph<_,_>) eCount vCount =
        Assert.AreEqual(eCount, g.Edges |> Seq.length, "Wrong edges count.")
        Assert.AreEqual(vCount, g.Vertices |> Seq.length, "Wrong verticies count")

    let calcTokenizationTest path eCount vCount =
        let lexerInputGraph = loadLexerInputGraph path
        let res = Calc.Lexer._fslex_tables.Tokenize(Calc.Lexer.fslex_actions_token, lexerInputGraph, eofToken)
        checkGraph res eCount vCount

    let literalsTokenizationTest path eCount vCount =
        let lexerInputGraph = loadLexerInputGraph path
        let res = Literals.Lexer._fslex_tables.Tokenize(Literals.Lexer.fslex_actions_token, lexerInputGraph, literalEofToken)
        checkGraph res eCount vCount

    [<Test>]
    member this.``Load graph test from DOT`` () =
        let g = loadGraphFromDOT(path "test_00.dot")
        //checkGraph g 4 4
        Assert.AreEqual(g.Edges |> Seq.length, 4)
        Assert.AreEqual(g.Vertices |> Seq.length, 4)

    [<Test>]
    member this.``Load graph test from DOT to QuickGraph`` () =
        let qGraph = loadDotToQG "test_00.dot"
        //checkGraph g 4 4
        Assert.AreEqual(qGraph.Edges |> Seq.length, 4)
        Assert.AreEqual(qGraph.Vertices |> Seq.length, 4)

    [<Test>]
    member this.``Load graph test from DOT to lexer input graph`` () =
        let qGraph = loadDotToQG "test_00.dot"
        let lexerInputG = new LexerInputGraph<_>()
        lexerInputG.StartVertex <- 0
        for e in qGraph.Edges do lexerInputG.AddEdgeForsed (new LexerEdge<_,_>(e.Source,e.Target,Some(e.Tag, e.Tag)))
        checkGraph lexerInputG 4 4

    [<Test>]
    member this.``Load graph test from DOT to lexer inner graph`` () =
        let lexerInnerGraph = new LexerInnerGraph<_>(loadLexerInputGraph "test_00.dot")
        Assert.AreEqual(lexerInnerGraph.StartVertex, 0)
        checkGraph lexerInnerGraph 6 6

    [<Test>]
    member this.``Calc. Simple number.`` () =
        let lexerInputGraph = loadLexerInputGraph "test_0.dot"
        let res = Calc.Lexer._fslex_tables.Tokenize(Calc.Lexer.fslex_actions_token, lexerInputGraph, eofToken)
        checkGraph res 2 3        

    [<Test>]
    member this.``Test with position.`` () =
        let lexerInputGraph = loadLexerInputGraph "test_with_pos_0.dot"
        let res = Calc.Lexer._fslex_tables.Tokenize(Calc.Lexer.fslex_actions_token, lexerInputGraph, eofToken)
        checkGraph res 2 3
        res.Edges 
          |> Seq.iter
              (fun e -> 
                match e.Tag with
                | NUMBER (n, brs)->
                    Assert.AreEqual(brs.Length, n.Length)
                    let brs' = brs |> Array.map (fun i -> i.back_ref)
                    checkArr [|"12|"; "12|"|] brs'
                    let pos' = brs |> Array.map (fun i -> i.pos_cnum)
                    checkArr [|0; 1|] pos'
                | RNGLR_EOF _ -> ()
                | t -> Assert.Fail(sprintf "Unexpected token: %A" t)) 
   
    [<Test>]
    member this.``Test with position. Ident on two edgs`` () =
        let lexerInputGraph = loadLexerInputGraph "test_with_pos_1.dot"
        let res = Calc.Lexer._fslex_tables.Tokenize(Calc.Lexer.fslex_actions_token, lexerInputGraph, eofToken)
        checkGraph res 2 3        
        res.Edges 
          |> Seq.iter
              (fun e -> 
                match e.Tag with
                | NUMBER (n,brs)->
                    Assert.AreEqual(brs.Length,n.Length)
                    let brs' = brs |> Array.map (fun i -> i.back_ref)
                    checkArr [|"12|"; "12|"; "3|"|] brs'
                    let pos' = brs |> Array.map (fun i -> i.pos_cnum)
                    checkArr [|0; 1; 0|] pos'
                | RNGLR_EOF _ -> ()
                | t -> Assert.Fail(sprintf "Unexpected token: %A" t)) 

    [<Test>]
    member this.``Test with position. Ident on edgs with branch`` () =
        let lexerInputGraph = loadLexerInputGraph "test_with_pos_2.dot"
        let res = Calc.Lexer._fslex_tables.Tokenize(Calc.Lexer.fslex_actions_token, lexerInputGraph, eofToken)
        checkGraph res 3 3
        res.Edges 
          |> Seq.iter
              (fun e -> 
                match e.Tag with
                | NUMBER (n,brs)->
                    Assert.AreEqual(brs.Length,n.Length)
                    let brs' = brs |> Array.map (fun i -> i.back_ref)
                    Assert.IsTrue(brs'.[0] = "12|")
                    Assert.IsTrue(brs'.[1] = "12|")
                    Assert.IsTrue(brs'.[2] = "3|" || brs'.[2] = "4|")
                    let pos' = brs |> Array.map (fun i -> i.pos_cnum)
                    checkArr [|0; 1; 0|] pos'
                | RNGLR_EOF _ -> ()
                | t -> Assert.Fail(sprintf "Unexpected token: %A" t)) 

    [<Test>]
    member this.``Test with position. Ident and plus on edgs with branch`` () =
        let lexerInputGraph = loadLexerInputGraph "test_with_pos_3.dot"
        let res = Calc.Lexer._fslex_tables.Tokenize(Calc.Lexer.fslex_actions_token, lexerInputGraph, eofToken)
        checkGraph res 4 4
        printG res "test_with_pos_3_res"
        let positions =
            res.Edges 
              |> Seq.collect
                  (fun e -> 
                    match e.Tag with
                    | NUMBER (n,brs)
                    | PLUS (n,brs)->
                        brs |> Array.map (fun p -> p.pos_cnum)
                    | RNGLR_EOF _ -> [||]
                    | t -> failwith (sprintf "Unexpected token: %A" t))
            |> Array.ofSeq
        checkArr [|0; 1; 0; 0; 1; 0|] positions

    [<Test>]
    member this.``Test with position. Ident on edgs with branch in begin.`` () =
        let lexerInputGraph = loadLexerInputGraph "test_with_pos_4.dot"
        let res = Calc.Lexer._fslex_tables.Tokenize(Calc.Lexer.fslex_actions_token, lexerInputGraph, eofToken)
        checkGraph res 3 3
        res.Edges 
          |> Seq.iter
              (fun e -> 
                match e.Tag with
                | NUMBER (n,brs)->
                    Assert.AreEqual(brs.Length,n.Length)
                    let pos' = brs |> Array.map (fun i -> i.pos_cnum)
                    Assert.IsTrue(pos'.[0] = 0)
                    Assert.IsTrue(pos'.[1] = 0)
                | RNGLR_EOF _ -> ()
                | t -> Assert.Fail(sprintf "Unexpected token: %A" t)) 

    [<Test>]
    member this.``Test with position. Ident on edgs with branch in begin_1.`` () =
        let lexerInputGraph = loadLexerInputGraph "test_with_pos_5.dot"
        let res = Calc.Lexer._fslex_tables.Tokenize(Calc.Lexer.fslex_actions_token, lexerInputGraph, eofToken)
        checkGraph res 3 3
        let positions =
            res.Edges 
              |> Seq.collect
                  (fun e -> 
                    match e.Tag with
                    | NUMBER (n,brs)->
                        brs |> Array.map (fun p -> p.pos_cnum)
                    | RNGLR_EOF _ -> [||]
                    | t -> failwith (sprintf "Unexpected token: %A" t))
            |> Array.ofSeq
        checkArr [|0; 0; 0; 0; 0|] positions

    [<Test>]
    member this.``Positions. Simple binop.`` () =
        let lexerInputGraph = loadLexerInputGraph "test_with_pos_6.dot"
        let res = Calc.Lexer._fslex_tables.Tokenize(Calc.Lexer.fslex_actions_token, lexerInputGraph, eofToken)
        checkGraph res 4 5
        let positions =
            res.Edges 
              |> Seq.collect
                  (fun e -> 
                    match e.Tag with
                    | NUMBER (n,brs)
                    | MULT (n,brs) ->
                        brs |> Array.map (fun p -> p.pos_cnum)
                    | RNGLR_EOF _ -> [||]
                    | t -> failwith (sprintf "Unexpected token: %A" t))
            |> Array.ofSeq
        checkArr [|0; 1; 2|] positions

    [<Test>]
    member this.``Test with position. Two tokens on the one edge.`` () =
        let lexerInputGraph = loadLexerInputGraph "test_with_pos_7.dot"
        let res = Calc.Lexer._fslex_tables.Tokenize(Calc.Lexer.fslex_actions_token, lexerInputGraph, eofToken)
        Assert.AreEqual(res.Edges |> Seq.length, 4)
        Assert.AreEqual(res.Vertices |> Seq.length, 5)
        let positions =
            res.Edges 
              |> Seq.collect
                  (fun e -> 
                    match e.Tag with
                    | NUMBER (n,brs)->
                        brs |> Array.map (fun p -> p.pos_cnum)
                    | PLUS (n,brs)->
                        brs |> Array.map (fun p -> p.pos_cnum)
                    | RNGLR_EOF _ -> [||]
                    | t -> failwith (sprintf "Unexpected token: %A" t))
            |> Array.ofSeq
        checkArr [|0; 1; 0; 1; 2|] positions

    [<Test>]
    member this.``Test with position. With branch and several tokens on the one edge``() =
        let lexerInputGraph = loadLexerInputGraph "test_with_pos_8.dot"
        let res = Calc.Lexer._fslex_tables.Tokenize(Calc.Lexer.fslex_actions_token, lexerInputGraph, eofToken)
        Assert.AreEqual(res.Edges |> Seq.length, 7)
        Assert.AreEqual(res.Vertices |> Seq.length, 7)
        let positions =
            res.Edges 
              |> Seq.collect
                  (fun e -> 
                    match e.Tag with
                    | NUMBER (n,brs)->
                        brs |> Array.map (fun p -> p.pos_cnum)
                    | PLUS (n,brs)->
                        brs |> Array.map (fun p -> p.pos_cnum)
                    | RNGLR_EOF _ -> [||]
                    | t -> failwith (sprintf "Unexpected token: %A" t))
            |> Array.ofSeq
        checkArr [|0; 1; 0; 0; 1; 1; 0; 2|] positions

    [<Test>]
    member this.``Test with position. Several tokens on the one edge``() =
        let lexerInputGraph = loadLexerInputGraph "test_with_pos_9.dot"
        let res = Calc.Lexer._fslex_tables.Tokenize(Calc.Lexer.fslex_actions_token, lexerInputGraph, eofToken)
        Assert.AreEqual(res.Edges |> Seq.length, 6)
        Assert.AreEqual(res.Vertices |> Seq.length, 7)
        let positions =
            res.Edges 
              |> Seq.collect
                  (fun e -> 
                    match e.Tag with
                    | NUMBER (n,brs)->
                        brs |> Array.map (fun p -> p.pos_cnum)
                    | PLUS (n,brs)->
                        brs |> Array.map (fun p -> p.pos_cnum)
                    | MULT (n,brs)->
                        brs |> Array.map (fun p -> p.pos_cnum)
                    | RNGLR_EOF _ -> [||]
                    | t -> failwith (sprintf "Unexpected token: %A" t))
            |> Array.ofSeq
        checkArr [|0; 1; 0; 1; 2; 0|] positions

    //[<Test>]
    member this.``Test with position. With branch and several tokens on the one edge_1``() =
        let lexerInputGraph = loadLexerInputGraph "test_with_pos_10.dot"
        let res = Calc.Lexer._fslex_tables.Tokenize(Calc.Lexer.fslex_actions_token, lexerInputGraph, eofToken)
        Assert.AreEqual(res.Edges |> Seq.length, 7)
        Assert.AreEqual(res.Vertices |> Seq.length, 7)
        let positions =
            res.Edges 
              |> Seq.collect
                  (fun e -> 
                    match e.Tag with
                    | NUMBER (n,brs)->
                        brs |> Array.map (fun p -> p.pos_cnum)
                    | PLUS (n,brs)->
                        brs |> Array.map (fun p -> p.pos_cnum)
                    | MINUS (n,brs)->
                        brs |> Array.map (fun p -> p.pos_cnum)
                    | RNGLR_EOF _ -> [||]
                    | t -> failwith (sprintf "Unexpected token: %A" t))
            |> Array.ofSeq
        checkArr [|0; 1; 0; 0; 1; 1; 0; 2|] positions

    [<Test>]
    member this.``Calc. Simple sum.`` () =
        calcTokenizationTest "test_1.dot" 4 5

    [<Test>]
    member this.``Calc. Simple sum. Check back refs.`` () =
        let lexerInputGraph = loadLexerInputGraph "test_1.dot"
        let res = Calc.Lexer._fslex_tables.Tokenize(Calc.Lexer.fslex_actions_token, lexerInputGraph, eofToken)
        checkGraph res 4 5
        check_brs res.Edges

    [<Test>]
    member this.``Calc. Start from PLUS.`` () =
        calcTokenizationTest "test_2.dot" 3 4

    [<Test>]
    member this.``Calc. Two-digit numbers sum.`` () =
        calcTokenizationTest "test_3.dot" 4 5

    [<Test>]
    member this.``Calc. Two-digit numbers sum. Check back refs.`` () =
        let lexerInputGraph = loadLexerInputGraph "test_3.dot"
        let res = Calc.Lexer._fslex_tables.Tokenize(Calc.Lexer.fslex_actions_token, lexerInputGraph, eofToken)
        checkGraph res 4 5
        check_brs res.Edges

    [<Test>]
    member this.``Multi-digit with branch.`` () =
        let lexerInputGraph = loadLexerInputGraph "test_14.dot"
        let res = Calc.Lexer._fslex_tables.Tokenize(Calc.Lexer.fslex_actions_token, lexerInputGraph, eofToken)
        checkGraph res 3 3
        res.Edges 
          |> Seq.iter
              (fun e -> 
                match e.Tag with
                | NUMBER (n, brs)
                | PLUS (n, brs)->
                    Assert.AreEqual(brs.Length,n.Length)
                    let brs' = brs |> Array.map (fun i -> i.back_ref)
                    checkArr [|"12|"; "12|"; string n.[2] + "|"|] brs'
                | RNGLR_EOF _ -> ()
                | t -> Assert.Fail(sprintf "Unexpected token: %A" t)) 
               
    [<Test>]
    member this.``Print info on edges.`` () =
        let lexerInputGraph = loadLexerInputGraph "test_15.dot"
        let ig = LexerInnerGraph lexerInputGraph
        let res = Calc.Lexer._fslex_tables.Tokenize(Calc.Lexer.fslex_actions_token, lexerInputGraph, eofToken)
        let prSeq = seq { for v in ig.TopologicalSort() do
                              for e in ig.OutEdges v do
                                  yield e.Label
                        }
        checkGraph res 7 3
        for x in prSeq do
            printfn "%A" x

    [<Test>]
    member this.``Calc. Branched multy-digit numbers.`` () =
        calcTokenizationTest "test_4_1.dot" 3 3 
        
    [<Test>]
    member this.``Calc. Branched multy-digit numbers with Binop.`` () =
        calcTokenizationTest "test_4_2.dot" 4 4

    [<Test>]
    member this.``Calc. Branched multy-digit numbers sum 1.`` () =
        calcTokenizationTest "test_4_3.dot" 5 5       

    [<Test>]
    member this.``Calc. Branched multy-digit numbers sum 2.`` () =
        calcTokenizationTest "test_4_4.dot" 6 5

    [<Test>]
    member this.``Calc. Branched binop.`` () =
        calcTokenizationTest "test_5.dot" 5 5

    [<Test>]
    member this.``Calc. Branched binop or negation.`` () =
        calcTokenizationTest "test_6.dot" 5 5

    [<Test>]
    member this.``Calc. Complex branched 1.`` () =
        calcTokenizationTest "test_7.dot" 8 5       

    [<Test>]
    member this.``Calc. Complex branched 2.`` () =
        calcTokenizationTest "test_8.dot" 5 5

    [<Test>]
    member this.``Calc. Complex branched 3.`` () =
        calcTokenizationTest "test_9.dot" 7 7

    [<Test>]
    member this.``Calc. Complex 0`` () =
        calcTokenizationTest "test_12.dot" 6 6

    [<Test>]
    member this.``Calc. test 100`` () =
        let lexerInputGraph = loadLexerInputGraph "test_100.dot"
        let start = System.DateTime.Now
        for i in 1..1000 do Calc.Lexer._fslex_tables.Tokenize(Calc.Lexer.fslex_actions_token, lexerInputGraph, eofToken) |> ignore
        printfn "Time = %A" ((System.DateTime.Now - start).TotalMilliseconds / 1000.0)
        Assert.True(true)

    [<Test>]
    member this.``Calc. Epsilon edge 1.`` () =
        calcTokenizationTest "test_10_1.dot" 3 4

    [<Test>]
    member this.``Calc. Epsilon edge 2.`` () =
        calcTokenizationTest "test_10_2.dot" 4 4

    [<Test>]
    member this.``Literals. Simple.`` () =
        literalsTokenizationTest "literals_simple.dot" 2 3

    [<Test>]
    member this.``Literals. Inner branch.`` () =
        literalsTokenizationTest "literals_inner_branch.dot" 3 3

    [<Test>]
    member this.``Literals. Outer branch.`` () =
        literalsTokenizationTest "literals_outer_branch.dot" 3 3

    [<Test>]
    member this.``Literals. Splitted.`` () =
        literalsTokenizationTest "literals_splitted.dot" 3 3

    [<Test>]
    member this.``Test with space and idents on edge.`` () =
        calcTokenizationTest "test_with_space_0.dot" 3 4

    [<Test>]
    member this.``Test with space with branch.`` () =
        calcTokenizationTest "test_with_space_1.dot" 4 4

    //[<Test>]
    member this.``Test with space at the end of previous tokens at the end of branch.`` () =
        calcTokenizationTest "test_with_space_at_end_of_prev_token.dot." 4 4

    [<Test>]
    member this.``Calc with braces.`` () =
        calcTokenizationTest "calc_1.dot." 10 10

    [<Test>]
    member this.``Calc with braces 2.`` () =
        calcTokenizationTest "calc_0.dot." 4 4

    [<Test>]
    member this.``Example with eps.`` () =
        calcTokenizationTest "example_eps.dot." 4 5
        
    [<Test>]
    member this.``Eps_closure_1.`` () =
        calcTokenizationTest "eps_closure_1.dot" 4 5

    [<Test>]
    member this.``Eps_closure_2.`` () =
        calcTokenizationTest "eps_closure_2.dot" 5 5

    [<Test>]
    member this.``Eps_closure_3.`` () =
        calcTokenizationTest "eps_closure_3.dot" 2 3

    [<Test>]
    member this.``Eps_closure_4.`` () =
        calcTokenizationTest "eps_closure_4.dot" 4 4 

    [<Test>]
    member this.``Eps_closure_5.`` () =
        let lexerInputGraph = loadLexerInputGraph "eps_closure_5.dot"
        let res = Calc.Lexer._fslex_tables.Tokenize(Calc.Lexer.fslex_actions_token, lexerInputGraph, eofToken)
        printG res "eps_res_closure_5"
        checkGraph res 1 2
        Assert.AreEqual(eofToken, (res.Edges |> Seq.nth 0).Tag)


//[<EntryPoint>]
//let f x =
//      let t = new ``Abstract lexer tests`` () 
//      t.``Test with position. With branch and several tokens on the one edge_1``()
//      //``Test with space at the end of previous tokens at the end of branch.``()
//      //let t = Literals.Lexer222.token <| Lexing.LexBuffer<_>.FromString ( "+1+")
//     // printfn "%A" t
//      1