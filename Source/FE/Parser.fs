// Implementation file for parser generated by fsyacc
#light "off"
module Yard.Core.GrammarParser
#nowarn "64";; // turn off warnings that type variables used in production annotations are instantiated to concrete type
open Yard.Core
open Microsoft.FSharp.Text.Lexing
open Microsoft.FSharp.Text.Parsing.ParseHelpers
# 1 "Parser.fsy"

// Copyright 2009 Jake Kirilenko
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation.
  open Microsoft.FSharp.Text
  open Yard.Core.IL
  open Yard.Core.IL.Production
  
  exception Parse_error
  
  let o2l = function Some x -> [x] | None -> []
  let getList = function Some x -> x | None -> []
  (*let parse_error (s:string):unit = ()*)
  let missing name = System.Console.WriteLine("Missing " + name)
  let createSeqElem bnd omitted r check =
      { binding = bnd; omit = omitted; rule = r; checker = check }

# 28 "Parser.fs"
// This type is the type of tokens accepted by the parser
type token = 
  | PATTERN of (IL.Source.t)
  | PARAM of (IL.Source.t)
  | PREDICATE of (IL.Source.t)
  | ACTION of (IL.Source.t)
  | STRING of (IL.Source.t)
  | LIDENT of (IL.Source.t)
  | UIDENT of (IL.Source.t)
  | COMMUT
  | DLESS
  | DGREAT
  | RPAREN
  | LPAREN
  | QUESTION
  | PLUS
  | STAR
  | BAR
  | EQUAL
  | SEMICOLON
  | COLON
  | EOF
// This type is used to give symbolic names to token indexes, useful for error messages
type tokenId = 
    | TOKEN_PATTERN
    | TOKEN_PARAM
    | TOKEN_PREDICATE
    | TOKEN_ACTION
    | TOKEN_STRING
    | TOKEN_LIDENT
    | TOKEN_UIDENT
    | TOKEN_COMMUT
    | TOKEN_DLESS
    | TOKEN_DGREAT
    | TOKEN_RPAREN
    | TOKEN_LPAREN
    | TOKEN_QUESTION
    | TOKEN_PLUS
    | TOKEN_STAR
    | TOKEN_BAR
    | TOKEN_EQUAL
    | TOKEN_SEMICOLON
    | TOKEN_COLON
    | TOKEN_EOF
    | TOKEN_end_of_input
    | TOKEN_error
// This type is used to give symbolic names to token indexes, useful for error messages
type nonTerminalId = 
    | NONTERM__startfile
    | NONTERM_file
    | NONTERM_action_opt
    | NONTERM_rule_nlist
    | NONTERM_rule
    | NONTERM_plus_opt
    | NONTERM_formal_meta_param_opt
    | NONTERM_formal_meta_list
    | NONTERM_param_opt
    | NONTERM_alts
    | NONTERM_bar_seq_nlist
    | NONTERM_seq
    | NONTERM_seq_elem_list
    | NONTERM_seq_elem
    | NONTERM_predicate_opt
    | NONTERM_bound
    | NONTERM_patt
    | NONTERM_prim
    | NONTERM_meta_param
    | NONTERM_meta_params
    | NONTERM_meta_param_opt
    | NONTERM_call

// This function maps tokens to integers indexes
let tagOfToken (t:token) = 
  match t with
  | PATTERN _ -> 0 
  | PARAM _ -> 1 
  | PREDICATE _ -> 2 
  | ACTION _ -> 3 
  | STRING _ -> 4 
  | LIDENT _ -> 5 
  | UIDENT _ -> 6 
  | COMMUT  -> 7 
  | DLESS  -> 8 
  | DGREAT  -> 9 
  | RPAREN  -> 10 
  | LPAREN  -> 11 
  | QUESTION  -> 12 
  | PLUS  -> 13 
  | STAR  -> 14 
  | BAR  -> 15 
  | EQUAL  -> 16 
  | SEMICOLON  -> 17 
  | COLON  -> 18 
  | EOF  -> 19 

// This function maps integers indexes to symbolic token ids
let tokenTagToTokenId (tokenIdx:int) = 
  match tokenIdx with
  | 0 -> TOKEN_PATTERN 
  | 1 -> TOKEN_PARAM 
  | 2 -> TOKEN_PREDICATE 
  | 3 -> TOKEN_ACTION 
  | 4 -> TOKEN_STRING 
  | 5 -> TOKEN_LIDENT 
  | 6 -> TOKEN_UIDENT 
  | 7 -> TOKEN_COMMUT 
  | 8 -> TOKEN_DLESS 
  | 9 -> TOKEN_DGREAT 
  | 10 -> TOKEN_RPAREN 
  | 11 -> TOKEN_LPAREN 
  | 12 -> TOKEN_QUESTION 
  | 13 -> TOKEN_PLUS 
  | 14 -> TOKEN_STAR 
  | 15 -> TOKEN_BAR 
  | 16 -> TOKEN_EQUAL 
  | 17 -> TOKEN_SEMICOLON 
  | 18 -> TOKEN_COLON 
  | 19 -> TOKEN_EOF 
  | 22 -> TOKEN_end_of_input
  | 20 -> TOKEN_error
  | _ -> failwith "tokenTagToTokenId: bad token"

/// This function maps production indexes returned in syntax errors to strings representing the non terminal that would be produced by that production
let prodIdxToNonTerminal (prodIdx:int) = 
  match prodIdx with
    | 0 -> NONTERM__startfile 
    | 1 -> NONTERM_file 
    | 2 -> NONTERM_action_opt 
    | 3 -> NONTERM_action_opt 
    | 4 -> NONTERM_rule_nlist 
    | 5 -> NONTERM_rule_nlist 
    | 6 -> NONTERM_rule_nlist 
    | 7 -> NONTERM_rule 
    | 8 -> NONTERM_plus_opt 
    | 9 -> NONTERM_plus_opt 
    | 10 -> NONTERM_formal_meta_param_opt 
    | 11 -> NONTERM_formal_meta_param_opt 
    | 12 -> NONTERM_formal_meta_list 
    | 13 -> NONTERM_formal_meta_list 
    | 14 -> NONTERM_param_opt 
    | 15 -> NONTERM_param_opt 
    | 16 -> NONTERM_alts 
    | 17 -> NONTERM_alts 
    | 18 -> NONTERM_bar_seq_nlist 
    | 19 -> NONTERM_bar_seq_nlist 
    | 20 -> NONTERM_seq 
    | 21 -> NONTERM_seq 
    | 22 -> NONTERM_seq_elem_list 
    | 23 -> NONTERM_seq_elem_list 
    | 24 -> NONTERM_seq_elem 
    | 25 -> NONTERM_predicate_opt 
    | 26 -> NONTERM_predicate_opt 
    | 27 -> NONTERM_bound 
    | 28 -> NONTERM_bound 
    | 29 -> NONTERM_patt 
    | 30 -> NONTERM_patt 
    | 31 -> NONTERM_prim 
    | 32 -> NONTERM_prim 
    | 33 -> NONTERM_prim 
    | 34 -> NONTERM_prim 
    | 35 -> NONTERM_prim 
    | 36 -> NONTERM_prim 
    | 37 -> NONTERM_meta_param 
    | 38 -> NONTERM_meta_param 
    | 39 -> NONTERM_meta_param 
    | 40 -> NONTERM_meta_params 
    | 41 -> NONTERM_meta_params 
    | 42 -> NONTERM_meta_param_opt 
    | 43 -> NONTERM_meta_param_opt 
    | 44 -> NONTERM_call 
    | 45 -> NONTERM_call 
    | _ -> failwith "prodIdxToNonTerminal: bad production index"

let _fsyacc_endOfInputTag = 22 
let _fsyacc_tagOfErrorTerminal = 20

// This function gets the name of a token as a string
let token_to_string (t:token) = 
  match t with 
  | PATTERN _ -> "PATTERN" 
  | PARAM _ -> "PARAM" 
  | PREDICATE _ -> "PREDICATE" 
  | ACTION _ -> "ACTION" 
  | STRING _ -> "STRING" 
  | LIDENT _ -> "LIDENT" 
  | UIDENT _ -> "UIDENT" 
  | COMMUT  -> "COMMUT" 
  | DLESS  -> "DLESS" 
  | DGREAT  -> "DGREAT" 
  | RPAREN  -> "RPAREN" 
  | LPAREN  -> "LPAREN" 
  | QUESTION  -> "QUESTION" 
  | PLUS  -> "PLUS" 
  | STAR  -> "STAR" 
  | BAR  -> "BAR" 
  | EQUAL  -> "EQUAL" 
  | SEMICOLON  -> "SEMICOLON" 
  | COLON  -> "COLON" 
  | EOF  -> "EOF" 

// This function gets the data carried by a token as an object
let _fsyacc_dataOfToken (t:token) = 
  match t with 
  | PATTERN _fsyacc_x -> Microsoft.FSharp.Core.Operators.box _fsyacc_x 
  | PARAM _fsyacc_x -> Microsoft.FSharp.Core.Operators.box _fsyacc_x 
  | PREDICATE _fsyacc_x -> Microsoft.FSharp.Core.Operators.box _fsyacc_x 
  | ACTION _fsyacc_x -> Microsoft.FSharp.Core.Operators.box _fsyacc_x 
  | STRING _fsyacc_x -> Microsoft.FSharp.Core.Operators.box _fsyacc_x 
  | LIDENT _fsyacc_x -> Microsoft.FSharp.Core.Operators.box _fsyacc_x 
  | UIDENT _fsyacc_x -> Microsoft.FSharp.Core.Operators.box _fsyacc_x 
  | COMMUT  -> (null : System.Object) 
  | DLESS  -> (null : System.Object) 
  | DGREAT  -> (null : System.Object) 
  | RPAREN  -> (null : System.Object) 
  | LPAREN  -> (null : System.Object) 
  | QUESTION  -> (null : System.Object) 
  | PLUS  -> (null : System.Object) 
  | STAR  -> (null : System.Object) 
  | BAR  -> (null : System.Object) 
  | EQUAL  -> (null : System.Object) 
  | SEMICOLON  -> (null : System.Object) 
  | COLON  -> (null : System.Object) 
  | EOF  -> (null : System.Object) 
let _fsyacc_gotos = [| 0us; 65535us; 1us; 65535us; 0us; 1us; 3us; 65535us; 0us; 2us; 3us; 4us; 30us; 31us; 2us; 65535us; 2us; 3us; 8us; 9us; 2us; 65535us; 2us; 7us; 8us; 7us; 2us; 65535us; 2us; 11us; 8us; 11us; 1us; 65535us; 12us; 13us; 2us; 65535us; 18us; 19us; 21us; 22us; 2us; 65535us; 13us; 14us; 62us; 63us; 2us; 65535us; 15us; 16us; 47us; 48us; 2us; 65535us; 24us; 25us; 27us; 28us; 3us; 65535us; 15us; 24us; 26us; 27us; 47us; 24us; 2us; 65535us; 29us; 30us; 33us; 34us; 5us; 65535us; 15us; 29us; 26us; 29us; 29us; 33us; 33us; 33us; 47us; 29us; 1us; 65535us; 35us; 36us; 5us; 65535us; 15us; 35us; 26us; 35us; 29us; 35us; 33us; 35us; 47us; 35us; 5us; 65535us; 15us; 38us; 26us; 38us; 29us; 38us; 33us; 38us; 47us; 38us; 6us; 65535us; 15us; 41us; 26us; 41us; 29us; 41us; 33us; 41us; 39us; 40us; 47us; 41us; 2us; 65535us; 55us; 55us; 57us; 55us; 2us; 65535us; 55us; 56us; 57us; 58us; 2us; 65535us; 42us; 62us; 61us; 62us; 6us; 65535us; 15us; 50us; 26us; 50us; 29us; 50us; 33us; 50us; 39us; 50us; 47us; 50us; |]
let _fsyacc_sparseGotoTableRowOffsets = [|0us; 1us; 3us; 7us; 10us; 13us; 16us; 18us; 21us; 24us; 27us; 30us; 34us; 37us; 43us; 45us; 51us; 57us; 64us; 67us; 70us; 73us; |]
let _fsyacc_stateToProdIdxsTableElements = [| 1us; 0us; 1us; 0us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 2us; 3us; 4us; 5us; 6us; 2us; 4us; 5us; 1us; 4us; 1us; 6us; 1us; 7us; 1us; 7us; 1us; 7us; 1us; 7us; 1us; 7us; 1us; 7us; 1us; 8us; 1us; 10us; 1us; 10us; 1us; 10us; 2us; 12us; 13us; 1us; 13us; 1us; 14us; 2us; 16us; 17us; 1us; 17us; 2us; 18us; 19us; 2us; 18us; 19us; 1us; 18us; 1us; 20us; 1us; 20us; 1us; 20us; 1us; 21us; 1us; 22us; 1us; 22us; 1us; 24us; 1us; 24us; 1us; 25us; 1us; 27us; 1us; 27us; 4us; 27us; 31us; 32us; 33us; 4us; 28us; 31us; 32us; 33us; 2us; 29us; 45us; 1us; 30us; 1us; 31us; 1us; 32us; 1us; 33us; 1us; 34us; 1us; 34us; 1us; 34us; 1us; 35us; 1us; 36us; 1us; 37us; 1us; 38us; 1us; 39us; 2us; 40us; 41us; 1us; 41us; 1us; 43us; 1us; 43us; 1us; 43us; 1us; 44us; 1us; 45us; 1us; 45us; 1us; 45us; |]
let _fsyacc_stateToProdIdxsTableRowOffsets = [|0us; 2us; 4us; 6us; 8us; 10us; 12us; 14us; 18us; 21us; 23us; 25us; 27us; 29us; 31us; 33us; 35us; 37us; 39us; 41us; 43us; 45us; 48us; 50us; 52us; 55us; 57us; 60us; 63us; 65us; 67us; 69us; 71us; 73us; 75us; 77us; 79us; 81us; 83us; 85us; 87us; 92us; 97us; 100us; 102us; 104us; 106us; 108us; 110us; 112us; 114us; 116us; 118us; 120us; 122us; 124us; 127us; 129us; 131us; 133us; 135us; 137us; 139us; 141us; |]
let _fsyacc_action_rows = 64
let _fsyacc_actionTableElements = [|1us; 16387us; 3us; 6us; 0us; 49152us; 1us; 16393us; 13us; 17us; 1us; 16387us; 3us; 6us; 1us; 32768us; 19us; 5us; 0us; 16385us; 0us; 16386us; 2us; 32768us; 17us; 8us; 20us; 10us; 2us; 16389us; 5us; 16393us; 13us; 17us; 0us; 16388us; 0us; 16390us; 1us; 32768us; 5us; 12us; 1us; 16395us; 8us; 18us; 1us; 16399us; 1us; 23us; 1us; 32768us; 18us; 15us; 6us; 32768us; 0us; 43us; 3us; 32us; 4us; 51us; 5us; 42us; 6us; 60us; 11us; 47us; 0us; 16391us; 0us; 16392us; 1us; 32768us; 5us; 21us; 1us; 32768us; 9us; 20us; 0us; 16394us; 1us; 16396us; 5us; 21us; 0us; 16397us; 0us; 16398us; 1us; 16400us; 15us; 26us; 0us; 16401us; 6us; 32768us; 0us; 43us; 3us; 32us; 4us; 51us; 5us; 42us; 6us; 60us; 11us; 47us; 1us; 16403us; 15us; 26us; 0us; 16402us; 5us; 16407us; 0us; 43us; 4us; 51us; 5us; 42us; 6us; 60us; 11us; 47us; 1us; 16387us; 3us; 6us; 0us; 16404us; 0us; 16405us; 5us; 16407us; 0us; 43us; 4us; 51us; 5us; 42us; 6us; 60us; 11us; 47us; 0us; 16406us; 1us; 16410us; 2us; 37us; 0us; 16408us; 0us; 16409us; 1us; 32768us; 16us; 39us; 4us; 32768us; 4us; 51us; 5us; 61us; 6us; 60us; 11us; 47us; 3us; 16411us; 12us; 46us; 13us; 45us; 14us; 44us; 3us; 16412us; 12us; 46us; 13us; 45us; 14us; 44us; 8us; 16426us; 7us; 16413us; 8us; 57us; 9us; 16413us; 16us; 16413us; 18us; 16413us; 19us; 16413us; 21us; 16413us; 22us; 16413us; 0us; 16414us; 0us; 16415us; 0us; 16416us; 0us; 16417us; 6us; 32768us; 0us; 43us; 3us; 32us; 4us; 51us; 5us; 42us; 6us; 60us; 11us; 47us; 1us; 32768us; 10us; 49us; 0us; 16418us; 0us; 16419us; 0us; 16420us; 0us; 16421us; 0us; 16422us; 0us; 16423us; 3us; 16424us; 4us; 54us; 5us; 52us; 6us; 53us; 0us; 16425us; 3us; 32768us; 4us; 54us; 5us; 52us; 6us; 53us; 1us; 32768us; 9us; 59us; 0us; 16427us; 0us; 16428us; 1us; 16426us; 8us; 57us; 1us; 16399us; 1us; 23us; 0us; 16429us; |]
let _fsyacc_actionTableRowOffsets = [|0us; 2us; 3us; 5us; 7us; 9us; 10us; 11us; 14us; 17us; 18us; 19us; 21us; 23us; 25us; 27us; 34us; 35us; 36us; 38us; 40us; 41us; 43us; 44us; 45us; 47us; 48us; 55us; 57us; 58us; 64us; 66us; 67us; 68us; 74us; 75us; 77us; 78us; 79us; 81us; 86us; 90us; 94us; 103us; 104us; 105us; 106us; 107us; 114us; 116us; 117us; 118us; 119us; 120us; 121us; 122us; 126us; 127us; 131us; 133us; 134us; 135us; 137us; 139us; |]
let _fsyacc_reductionSymbolCounts = [|1us; 4us; 1us; 0us; 3us; 2us; 2us; 6us; 1us; 0us; 3us; 0us; 1us; 2us; 1us; 0us; 1us; 2us; 3us; 2us; 3us; 1us; 2us; 0us; 2us; 1us; 0us; 3us; 1us; 1us; 1us; 2us; 2us; 2us; 3us; 1us; 1us; 1us; 1us; 1us; 1us; 2us; 0us; 3us; 1us; 3us; |]
let _fsyacc_productionToNonTerminalTable = [|0us; 1us; 2us; 2us; 3us; 3us; 3us; 4us; 5us; 5us; 6us; 6us; 7us; 7us; 8us; 8us; 9us; 9us; 10us; 10us; 11us; 11us; 12us; 12us; 13us; 14us; 14us; 15us; 15us; 16us; 16us; 17us; 17us; 17us; 17us; 17us; 17us; 18us; 18us; 18us; 19us; 19us; 20us; 20us; 21us; 21us; |]
let _fsyacc_immediateActions = [|65535us; 49152us; 65535us; 65535us; 65535us; 16385us; 16386us; 65535us; 65535us; 16388us; 16390us; 65535us; 65535us; 65535us; 65535us; 65535us; 16391us; 16392us; 65535us; 65535us; 16394us; 65535us; 16397us; 16398us; 65535us; 16401us; 65535us; 65535us; 16402us; 65535us; 65535us; 16404us; 16405us; 65535us; 16406us; 65535us; 16408us; 16409us; 65535us; 65535us; 65535us; 65535us; 65535us; 16414us; 16415us; 16416us; 16417us; 65535us; 65535us; 16418us; 16419us; 16420us; 16421us; 16422us; 16423us; 65535us; 16425us; 65535us; 65535us; 16427us; 16428us; 65535us; 65535us; 16429us; |]
let _fsyacc_reductions ()  =    [| 
# 263 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : (IL.Source.t, IL.Source.t)IL.Definition.t)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
                      raise (Microsoft.FSharp.Text.Parsing.Accept(Microsoft.FSharp.Core.Operators.box _1))
                   )
                 : '_startfile));
# 272 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'action_opt)) in
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : 'rule_nlist)) in
            let _3 = (let data = parseState.GetInput(3) in (Microsoft.FSharp.Core.Operators.unbox data : 'action_opt)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 48 "Parser.fsy"
                             { Definition.empty with
                               Definition.head=_1
                             ; Definition.grammar=_2
                             ; Definition.foot=_3
                             }
                           
                   )
# 48 "Parser.fsy"
                 : (IL.Source.t, IL.Source.t)IL.Definition.t));
# 290 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : IL.Source.t)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 55 "Parser.fsy"
                                          Some _1 
                   )
# 55 "Parser.fsy"
                 : 'action_opt));
# 301 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 56 "Parser.fsy"
                                               None 
                   )
# 56 "Parser.fsy"
                 : 'action_opt));
# 311 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'rule)) in
            let _3 = (let data = parseState.GetInput(3) in (Microsoft.FSharp.Core.Operators.unbox data : 'rule_nlist)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 58 "Parser.fsy"
                                                             _1 :: _3 
                   )
# 58 "Parser.fsy"
                 : 'rule_nlist));
# 323 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'rule)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 59 "Parser.fsy"
                                                  [_1] 
                   )
# 59 "Parser.fsy"
                 : 'rule_nlist));
# 334 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'rule)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 60 "Parser.fsy"
                                              missing "SEMI"; raise Parse_error  
                   )
# 60 "Parser.fsy"
                 : 'rule_nlist));
# 345 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'plus_opt)) in
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : IL.Source.t)) in
            let _3 = (let data = parseState.GetInput(3) in (Microsoft.FSharp.Core.Operators.unbox data : 'formal_meta_param_opt)) in
            let _4 = (let data = parseState.GetInput(4) in (Microsoft.FSharp.Core.Operators.unbox data : 'param_opt)) in
            let _6 = (let data = parseState.GetInput(6) in (Microsoft.FSharp.Core.Operators.unbox data : 'alts)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 63 "Parser.fsy"
                             {
                               Rule._public = _1
                             ; Rule.name = Source.toString _2
                             ; Rule.metaArgs = getList _3
                             ; Rule.body = _6
                             ; Rule.args = o2l _4
                           } 
                   )
# 63 "Parser.fsy"
                 : 'rule));
# 366 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 71 "Parser.fsy"
                                          true
                   )
# 71 "Parser.fsy"
                 : 'plus_opt));
# 376 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 72 "Parser.fsy"
                                          false
                   )
# 72 "Parser.fsy"
                 : 'plus_opt));
# 386 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : 'formal_meta_list)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 74 "Parser.fsy"
                                                                           Some _2
                   )
# 74 "Parser.fsy"
                 : 'formal_meta_param_opt));
# 397 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 75 "Parser.fsy"
                                                         None
                   )
# 75 "Parser.fsy"
                 : 'formal_meta_param_opt));
# 407 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : IL.Source.t)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 77 "Parser.fsy"
                                               [_1]
                   )
# 77 "Parser.fsy"
                 : 'formal_meta_list));
# 418 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : IL.Source.t)) in
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : 'formal_meta_list)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 78 "Parser.fsy"
                                                                _1::_2
                   )
# 78 "Parser.fsy"
                 : 'formal_meta_list));
# 430 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : IL.Source.t)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 80 "Parser.fsy"
                                       Some _1
                   )
# 80 "Parser.fsy"
                 : 'param_opt));
# 441 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 80 "Parser.fsy"
                                                               None
                   )
# 80 "Parser.fsy"
                 : 'param_opt));
# 451 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'seq)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 82 "Parser.fsy"
                                 _1 
                   )
# 82 "Parser.fsy"
                 : 'alts));
# 462 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'seq)) in
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : 'bar_seq_nlist)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 82 "Parser.fsy"
                                                           PAlt (_1,_2)
                   )
# 82 "Parser.fsy"
                 : 'alts));
# 474 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : 'seq)) in
            let _3 = (let data = parseState.GetInput(3) in (Microsoft.FSharp.Core.Operators.unbox data : 'bar_seq_nlist)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 84 "Parser.fsy"
                                                            PAlt(_2,_3)
                   )
# 84 "Parser.fsy"
                 : 'bar_seq_nlist));
# 486 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : 'seq)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 85 "Parser.fsy"
                                              _2
                   )
# 85 "Parser.fsy"
                 : 'bar_seq_nlist));
# 497 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'seq_elem)) in
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : 'seq_elem_list)) in
            let _3 = (let data = parseState.GetInput(3) in (Microsoft.FSharp.Core.Operators.unbox data : 'action_opt)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 87 "Parser.fsy"
                                                              PSeq (_1::_2, _3) 
                   )
# 87 "Parser.fsy"
                 : 'seq));
# 510 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : IL.Source.t)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 88 "Parser.fsy"
                                  PSeq([],Some _1)
                   )
# 88 "Parser.fsy"
                 : 'seq));
# 521 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'seq_elem)) in
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : 'seq_elem_list)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 90 "Parser.fsy"
                                                            _1::_2
                   )
# 90 "Parser.fsy"
                 : 'seq_elem_list));
# 533 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 91 "Parser.fsy"
                                                 []
                   )
# 91 "Parser.fsy"
                 : 'seq_elem_list));
# 543 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'bound)) in
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : 'predicate_opt)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 93 "Parser.fsy"
                                                    {_1 with checker = _2}
                   )
# 93 "Parser.fsy"
                 : 'seq_elem));
# 555 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : IL.Source.t)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 95 "Parser.fsy"
                                                  Some _1 
                   )
# 95 "Parser.fsy"
                 : 'predicate_opt));
# 566 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 96 "Parser.fsy"
                                                  None    
                   )
# 96 "Parser.fsy"
                 : 'predicate_opt));
# 576 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'patt)) in
            let _3 = (let data = parseState.GetInput(3) in (Microsoft.FSharp.Core.Operators.unbox data : 'prim)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 98 "Parser.fsy"
                                              createSeqElem (Some _1) false _3 None 
                   )
# 98 "Parser.fsy"
                 : 'bound));
# 588 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'prim)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 99 "Parser.fsy"
                                              createSeqElem None false _1 None      
                   )
# 99 "Parser.fsy"
                 : 'bound));
# 599 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : IL.Source.t)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 101 "Parser.fsy"
                                   _1
                   )
# 101 "Parser.fsy"
                 : 'patt));
# 610 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : IL.Source.t)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 101 "Parser.fsy"
                                                  _1
                   )
# 101 "Parser.fsy"
                 : 'patt));
# 621 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'prim)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 103 "Parser.fsy"
                                                PMany _1
                   )
# 103 "Parser.fsy"
                 : 'prim));
# 632 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'prim)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 104 "Parser.fsy"
                                                PSome _1
                   )
# 104 "Parser.fsy"
                 : 'prim));
# 643 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'prim)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 105 "Parser.fsy"
                                                POpt _1
                   )
# 105 "Parser.fsy"
                 : 'prim));
# 654 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : 'alts)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 106 "Parser.fsy"
                                                _2
                   )
# 106 "Parser.fsy"
                 : 'prim));
# 665 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'call)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 107 "Parser.fsy"
                                                _1
                   )
# 107 "Parser.fsy"
                 : 'prim));
# 676 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : IL.Source.t)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 108 "Parser.fsy"
                                                PLiteral _1
                   )
# 108 "Parser.fsy"
                 : 'prim));
# 687 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : IL.Source.t)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 110 "Parser.fsy"
                                         _1
                   )
# 110 "Parser.fsy"
                 : 'meta_param));
# 698 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : IL.Source.t)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 111 "Parser.fsy"
                                         _1
                   )
# 111 "Parser.fsy"
                 : 'meta_param));
# 709 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : IL.Source.t)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 112 "Parser.fsy"
                                         _1
                   )
# 112 "Parser.fsy"
                 : 'meta_param));
# 720 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'meta_param)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 114 "Parser.fsy"
                                              [_1]
                   )
# 114 "Parser.fsy"
                 : 'meta_params));
# 731 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'meta_param)) in
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : 'meta_params)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 115 "Parser.fsy"
                                                          _1 :: _2
                   )
# 115 "Parser.fsy"
                 : 'meta_params));
# 743 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 117 "Parser.fsy"
                                                  None
                   )
# 117 "Parser.fsy"
                 : 'meta_param_opt));
# 753 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : 'meta_params)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 118 "Parser.fsy"
                                                               Some _2
                   )
# 118 "Parser.fsy"
                 : 'meta_param_opt));
# 764 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : IL.Source.t)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 120 "Parser.fsy"
                                               PToken _1
                   )
# 120 "Parser.fsy"
                 : 'call));
# 775 "Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : IL.Source.t)) in
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : 'meta_param_opt)) in
            let _3 = (let data = parseState.GetInput(3) in (Microsoft.FSharp.Core.Operators.unbox data : 'param_opt)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 122 "Parser.fsy"
                             match _2 with
                             None -> PRef  (_1, _3)
                             | Some x -> PMetaRef (_1,_3,x)
                           
                   )
# 122 "Parser.fsy"
                 : 'call));
|]
# 792 "Parser.fs"
let tables () : Microsoft.FSharp.Text.Parsing.Tables<_> = 
  { reductions= _fsyacc_reductions ();
    endOfInputTag = _fsyacc_endOfInputTag;
    tagOfToken = tagOfToken;
    dataOfToken = _fsyacc_dataOfToken; 
    actionTableElements = _fsyacc_actionTableElements;
    actionTableRowOffsets = _fsyacc_actionTableRowOffsets;
    stateToProdIdxsTableElements = _fsyacc_stateToProdIdxsTableElements;
    stateToProdIdxsTableRowOffsets = _fsyacc_stateToProdIdxsTableRowOffsets;
    reductionSymbolCounts = _fsyacc_reductionSymbolCounts;
    immediateActions = _fsyacc_immediateActions;
    gotos = _fsyacc_gotos;
    sparseGotoTableRowOffsets = _fsyacc_sparseGotoTableRowOffsets;
    tagOfErrorTerminal = _fsyacc_tagOfErrorTerminal;
    parseError = (fun (ctxt:Microsoft.FSharp.Text.Parsing.ParseErrorContext<_>) -> 
                              match parse_error_rich with 
                              | Some f -> f ctxt
                              | None -> parse_error ctxt.Message);
    numTerminals = 23;
    productionToNonTerminalTable = _fsyacc_productionToNonTerminalTable  }
let engine lexer lexbuf startState = (tables ()).Interpret(lexer, lexbuf, startState)
let file lexer lexbuf : (IL.Source.t, IL.Source.t)IL.Definition.t =
    Microsoft.FSharp.Core.Operators.unbox ((tables ()).Interpret(lexer, lexbuf, 0))
