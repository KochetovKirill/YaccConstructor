//this file was generated by RACC
//source grammar:..\Tests\RACC\test_cls_with_head\\test_cls_with_head.yrd
//date:12/15/2010 11:18:34

module RACC.Actions_Cls_head

open Yard.Generators.RACCGenerator

let value x = x.value

let s0 expr = 
    let inner  = 
        match expr with
        | RESeq [x0; x1] -> 
            let (oneMinus) =
                let yardElemAction expr = 
                    match expr with
                    | RELeaf tMINUS -> tMINUS :?> 'a
                    | x -> "Unexpected type of node\nType " + x.ToString() + " is not expected in this position\nRELeaf was expected." |> failwith

                yardElemAction(x0)
            let (lst) =
                let yardElemAction expr = 
                    match expr with
                    | REClosure(lst) -> 
                        let yardClsAction expr = 
                            match expr with
                            | RESeq [_] -> 

                                ("list minus")
                            | x -> "Unexpected type of node\nType " + x.ToString() + " is not expected in this position\nRESeq was expected." |> failwith

                        List.map yardClsAction lst 
                    | x -> "Unexpected type of node\nType " + x.ToString() + " is not expected in this position\nREClosure was expected." |> failwith

                yardElemAction(x1)
            (lst |> fun x -> "Head minus" :: x |> String.concat ";")
        | x -> "Unexpected type of node\nType " + x.ToString() + " is not expected in this position\nRESeq was expected." |> failwith
    box (inner)

let ruleToAction = dict [|("s",s0)|]

