//this file was generated by RACC
//source grammar:..\Tests\RACC\test_l_attr\\test_l_attr.yrd
//date:12/15/2010 11:18:36

module RACC.Actions_L_attr

open Yard.Generators.RACCGenerator

let value x = (x:>Lexeme<string>).value         

let s0 expr = 
    let inner  = 
        match expr with
        | RESeq [x0] -> 
            let (res:int) =
                let yardElemAction expr = 
                    match expr with
                    | RELeaf e -> (e :?> _ )1 
                    | x -> "Unexpected type of node\nType " + x.ToString() + " is not expected in this position\nRELeaf was expected." |> failwith

                yardElemAction(x0)
            (res)
        | x -> "Unexpected type of node\nType " + x.ToString() + " is not expected in this position\nRESeq was expected." |> failwith
    box (inner)
let e1 expr = 
    let inner (i) = 
        match expr with
        | RESeq [x0] -> 
            let (n) =
                let yardElemAction expr = 
                    match expr with
                    | RELeaf tNUMBER -> tNUMBER :?> 'a
                    | x -> "Unexpected type of node\nType " + x.ToString() + " is not expected in this position\nRELeaf was expected." |> failwith

                yardElemAction(x0)
            ((value n |> int) + i)
        | x -> "Unexpected type of node\nType " + x.ToString() + " is not expected in this position\nRESeq was expected." |> failwith
    box (inner)

let ruleToAction = dict [|("e",e1); ("s",s0)|]

