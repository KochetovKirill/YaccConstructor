//this file was generated by GNESCC
//source grammar:grlang.yrd
//date:10/23/2011 7:38:05 PM

module GNESCC.Actions

open Yard.Generators.GNESCCGenerator

let getUnmatched x expectedType =
    "Unexpected type of node\nType " + x.ToString() + " is not expected in this position\n" + expectedType + " was expected." |> failwith

open Yard.Core.IL
open Yard.Core.IL.Production
open Yard.Core.IL.Rule

let value x = (x:>Lexer.MyLexeme).MValue

let elem t =
    {    
        omit    = false
        rule    = t        
        binding = None        
        checker = None
    }

let rule (nterm,rSeq) isStart=
    {       
        name    = nterm
        args    = []
        body    = PSeq (List.map elem rSeq,None)
        _public = isStart
        metaArgs= []
    }
    
let grammar rLst =
    { 
     Definition.empty with
       Definition.info = { fileName = "" }
       Definition.head = None
       Definition.grammar = List.mapi (fun i x -> rule x (i=0)) rLst
       Definition.foot = None
    }    

let grammar0 expr = 
    let inner  = 
        match expr with
        | RESeq [x0] -> 
            let (lst) =
                let yardElemAction expr = 
                    match expr with
                    | REClosure(lst) -> 
                        let yardClsAction expr = 
                            match expr with
                            | RESeq [x0; _] -> 
                                let (r) =
                                    let yardElemAction expr = 
                                        match expr with
                                        | RELeaf rule -> (rule :?> _ ) 
                                        | x -> getUnmatched x "RELeaf"

                                    yardElemAction(x0)

                                (r)
                            | x -> getUnmatched x "RESeq"

                        List.map yardClsAction lst 
                    | x -> getUnmatched x "REClosure"

                yardElemAction(x0)
            (grammar lst)
        | x -> getUnmatched x "RESeq"
    box (inner)
let rule1 expr = 
    let inner  = 
        match expr with
        | RESeq [x0; gnescc_x1; x2] -> 
            let (nterm) =
                let yardElemAction expr = 
                    match expr with
                    | RELeaf tNTERM -> tNTERM :?> 'a
                    | x -> getUnmatched x "RELeaf"

                yardElemAction(x0)
            let (gnescc_x1) =
                let yardElemAction expr = 
                    match expr with
                    | RELeaf tDRV -> tDRV :?> 'a
                    | x -> getUnmatched x "RELeaf"

                yardElemAction(gnescc_x1)
            let (lst) =
                let yardElemAction expr = 
                    match expr with
                    | REClosure(lst) -> 
                        let yardClsAction expr = 
                            match expr with
                            | REAlt(Some(x), None) -> 
                                let yardLAltAction expr = 
                                    match expr with
                                    | RESeq [x0] -> 
                                        let (nt) =
                                            let yardElemAction expr = 
                                                match expr with
                                                | RELeaf tNTERM -> tNTERM :?> 'a
                                                | x -> getUnmatched x "RELeaf"

                                            yardElemAction(x0)
                                        (PRef((value nt,(0,0)),None))
                                    | x -> getUnmatched x "RESeq"

                                yardLAltAction x 
                            | REAlt(None, Some(x)) -> 
                                let yardRAltAction expr = 
                                    match expr with
                                    | RESeq [x0] -> 
                                        let (t) =
                                            let yardElemAction expr = 
                                                match expr with
                                                | RELeaf tTERM -> tTERM :?> 'a
                                                | x -> getUnmatched x "RELeaf"

                                            yardElemAction(x0)
                                        (PToken((value t,(0,0))))
                                    | x -> getUnmatched x "RESeq"

                                yardRAltAction x 
                            | x -> getUnmatched x "REAlt"

                        List.map yardClsAction lst 
                    | x -> getUnmatched x "REClosure"

                yardElemAction(x2)
            (value nterm,lst)
        | x -> getUnmatched x "RESeq"
    box (inner)

let ruleToAction = dict [|(2,rule1); (1,grammar0)|]

