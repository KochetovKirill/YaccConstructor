﻿{
(** Module Lexer
 *
 *  Author: Jk
 *)

 open Lexing
 open Parser

(* Auxiliaries for the lexical analyzer *)
let brace_depth = ref 0
and sq_br_depth = ref 0
and comment_depth = ref 0
and ang_br_depth = ref 0
and commut_depth = ref 0

exception Lexical_error of string * int 
let initial_string_buffer = String.create 256
let string_buff = ref initial_string_buffer
let string_index = ref 0

let reset_string_buffer () =
  string_buff := initial_string_buffer;
  string_index := 0

let store_string_char c =
  if !string_index >= String.length !string_buff then begin
    let new_buff = String.create (String.length !string_buff * 2) in
    String.blit !string_buff 0 new_buff 0 (String.length !string_buff);
    string_buff := new_buff
  end;
  !string_buff.[!string_index] <- c;
  incr string_index

let get_stored_string () =
  String.sub !string_buff 0 !string_index

let char_for_backslash = function
    'n' -> '\n'
  | 't' -> '\t'
  | 'b' -> '\b'
  | 'r' -> '\r'
  | c   -> c

let char_for_decimal_code lexbuf i =
  Char.chr(100 * (Char.code(Lexing.lexeme_char lexbuf i) - 48) +
               10 * (Char.code(Lexing.lexeme_char lexbuf (i+1)) - 48) +
                    (Char.code(Lexing.lexeme_char lexbuf (i+2)) - 48))

(*let line_num = ref 1
let line_pos = ref 0
*)
let handle_lexical_error fn lexbuf =
  let pos = Lexing.lexeme_start lexbuf in
  try
    fn lexbuf
  with Lexical_error(msg, _) ->
    raise(Lexical_error(msg, pos))

let warning lexbuf msg =
  Printf.eprintf "ocamllex warning:\nFile \"%s\",  character %d: %s.\n"
                  Sys.argv.(1) (Lexing.lexeme_start lexbuf) msg;
  flush stderr
;;


let lexeme lexbuf (n,n') =
  let len = n' - n in
  let s = String.create len in
  try
    String.blit lexbuf.lex_buffer ( n - lexbuf.lex_abs_pos ) s 0 len;s
  with
   Invalid_argument _ as ex -> ( Printf.eprintf "Large file? jk's bug";raise ex)

let from_lexbuf lexbuf loc = lexeme lexbuf loc, loc
let lex2source lexbuf = from_lexbuf lexbuf (Lexing.lexeme_start lexbuf,Lexing.lexeme_end lexbuf)


}


let identstart = 
  ['A'-'Z' 'a'-'z' '\192'-'\214' '\216'-'\246' '\248'-'\255']
let identbody = 
  ['A'-'Z' 'a'-'z' '_' '-' '\192'-'\214' '\216'-'\246' '\248'-'\255' '\'' '0'-'9']
let backslash_escapes =
  ['\\' '"' '\'' 'n' 't' 'b' 'r']


let blank =  [' ' '\013' '\009' '\012'] *

rule skip n = parse   _ {if n > 0 then skip (n-1) lexbuf else lexbuf}
and skipSpaces  = parse blank {lexbuf} 
and  main = parse 
       blank  { main lexbuf }
     | '\010' { lexbuf.lex_curr_p <- {lexbuf.lex_curr_p with pos_bol = 0; pos_lnum = lexbuf.lex_start_p.pos_lnum + 1 }
               ; main lexbuf
              }
     | ':'  { COLON }
     | ';'  { SEMICOLON }
     | '|'  { BAR }
     | '='  {EQUAL}
     | "<<" {DLESS}
     | ">>" {DGREAT}
     | '*'  {STAR}
     | '+'  {PLUS}
     | '?'  {QUESTION}
     | "(*" 
       { comment_depth := 1;
         handle_lexical_error comment lexbuf;
         main lexbuf 
       }
     | "=>{"
       { let n1 = Lexing.lexeme_end lexbuf in
         let n2 = handle_lexical_error predicate lexbuf in
         PREDICATE (from_lexbuf lexbuf (n1,n2))
       }
     | identstart identbody *
       {  let text = lex2source lexbuf in 
          match (fst text).[0] with 
          'a'..'z' -> LIDENT text
          | _ -> UIDENT text
       }
     | eof {EOF}
	 | "[*"
	   {
	     let n1 = Lexing.lexeme_end lexbuf in
         commut_depth := 1;
         let n2 = handle_lexical_error commut lexbuf in
		 COMMUTATION(from_lexbuf lexbuf (n1,n2))
	   }
     | '{' 
       { let n1 = Lexing.lexeme_end lexbuf in
         brace_depth := 1;
         let n2 = handle_lexical_error action lexbuf in
          ACTION (from_lexbuf lexbuf (n1,n2))
       }
    | '<' 
       { let n1 = Lexing.lexeme_end lexbuf in
         ang_br_depth := 1;
         let n2 = handle_lexical_error pattern lexbuf in
         PATTERN (from_lexbuf lexbuf (n1,n2))
       }
    | '[' 
       { let n1 = Lexing.lexeme_end lexbuf in
         sq_br_depth := 1;
         let n2 = handle_lexical_error param lexbuf in
         PARAM (from_lexbuf lexbuf (n1,n2))
       }
    | "\""
      { reset_string_buffer();
        let string_start = lexbuf.Lexing.lex_start_pos in
        string lexbuf;
		lexbuf.Lexing.lex_start_pos <- string_start;
        STRING (from_lexbuf lexbuf (string_start,Lexing.lexeme_end lexbuf))
      }
    | "(" {LPAREN}
    | ")" {RPAREN}
    | _
       { raise(Lexical_error
                ("illegal character " ^ String.escaped(Lexing.lexeme lexbuf),
                  Lexing.lexeme_start lexbuf)) }

(*  ------------------------ *)
and predicate = parse
    "}=>"    { Lexing.lexeme_start lexbuf  }
  | '"' { reset_string_buffer();
      string lexbuf;
      reset_string_buffer();
      predicate lexbuf }
  | "(*" 
    { comment_depth := 1;
      comment lexbuf;
      predicate lexbuf }
  | eof 
    { raise (Lexical_error("unterminated predicate", Lexing.lexeme_start lexbuf)) }
  | "'" [^ '\\' '\''] "'" | "'" '\\' backslash_escapes "'"  | "'" '\\' ['0'-'9'] ['0'-'9'] ['0'-'9'] "'" 
  |  _ {predicate lexbuf}
and action = parse
    '{' 
    { incr brace_depth;
      action lexbuf }
  | '}' 
    { decr brace_depth;
      if !brace_depth = 0 then Lexing.lexeme_start lexbuf else action lexbuf }
  | '"' { reset_string_buffer();
      string lexbuf;
      reset_string_buffer();
      action lexbuf }
  | "(*" 
    { comment_depth := 1;
      comment lexbuf;
      action lexbuf }
  | eof 
    { raise (Lexical_error("unterminated action", Lexing.lexeme_start lexbuf)) }
  | "'" [^ '\\' '\''] "'" | "'" '\\' backslash_escapes "'"  | "'" '\\' ['0'-'9'] ['0'-'9'] ['0'-'9'] "'" 
  |  _ {action lexbuf}
 
and pattern = parse
    '<' 
    { incr ang_br_depth;
      pattern lexbuf }
  | '>' 
    { decr ang_br_depth;
      if !ang_br_depth = 0 then Lexing.lexeme_start lexbuf else pattern lexbuf }
  | '"' { reset_string_buffer();
      string lexbuf;
      reset_string_buffer();
      pattern lexbuf }
  | "(*" 
    { comment_depth := 1;
      comment lexbuf;
      pattern lexbuf }
  | eof 
    { raise (Lexical_error("unterminated pattern", Lexing.lexeme_start lexbuf)) }
  | "'" [^ '\\' '\''] "'" | "'" '\\' backslash_escapes "'"  | "'" '\\' ['0'-'9'] ['0'-'9'] ['0'-'9'] "'" 
  |  _ {pattern lexbuf}

and param = parse
    '[' 
    { incr sq_br_depth;
      pattern lexbuf }
  | ']' 
    { decr sq_br_depth;
      if !sq_br_depth = 0 then Lexing.lexeme_start lexbuf else param lexbuf }
  | '"' { reset_string_buffer();
      string lexbuf;
      reset_string_buffer();
      param lexbuf }
  | "(*" 
    { comment_depth := 1;
      comment lexbuf;
      param lexbuf }
  | eof 
    { raise (Lexical_error("unterminated param",  Lexing.lexeme_start lexbuf)) }
  | "'" [^ '\\' '\''] "'" | "'" '\\' backslash_escapes "'"  | "'" '\\' ['0'-'9'] ['0'-'9'] ['0'-'9'] "'" 
  |  _ {param lexbuf}
      
and string = parse
    '"' 
    { () }
  | '\\' backslash_escapes
    { store_string_char(char_for_backslash(Lexing.lexeme_char lexbuf 1));
      string lexbuf }
  | '\\' ['0'-'9'] ['0'-'9'] ['0'-'9'] 
    { store_string_char(char_for_decimal_code lexbuf 1);
      string lexbuf }
  | eof 
    { raise(Lexical_error("unterminated string", Lexing.lexeme_start lexbuf)) }
  | '\\' _
    { warning lexbuf
              (Printf.sprintf "illegal backslash escape in string: `\\%c'"
                              (Lexing.lexeme_char lexbuf 1));
      store_string_char(Lexing.lexeme_char lexbuf 0);
      store_string_char(Lexing.lexeme_char lexbuf 1);
      string lexbuf }
  | _ 
    { store_string_char(Lexing.lexeme_char lexbuf 0);
      string lexbuf }

and commut = parse
    "[*"  {
	        incr commut_depth;
            commut lexbuf
	      }
   |"*]"  { 
            decr commut_depth;
            if !commut_depth = 0 then Lexing.lexeme_start lexbuf else commut lexbuf 
	      }
   | '"'  { 
            reset_string_buffer();
            string lexbuf;
            reset_string_buffer();
            commut lexbuf 
		  }
   | "(*" { 
            comment_depth := 1;
            comment lexbuf;
            commut lexbuf
		  }
  | eof 
    { raise (Lexical_error("unterminated pattern", Lexing.lexeme_start lexbuf)) }
  | "'" [^ '\\' '\''] "'" | "'" '\\' backslash_escapes "'"  | "'" '\\' ['0'-'9'] ['0'-'9'] ['0'-'9'] "'" 
  |  _ {commut lexbuf}

and comment = parse
    "(*" 
    { incr comment_depth; comment lexbuf }
  | "*)" 
    { decr comment_depth;
      if !comment_depth = 0 then () else comment lexbuf }
  | '"' 
    { reset_string_buffer();
      string lexbuf;
      reset_string_buffer();
      comment lexbuf }
  | "''"
      { comment lexbuf }
  | "'" [^ '\\' '\''] "'"
      { comment lexbuf }
  | "'\\" backslash_escapes "'"
      { comment lexbuf }
  | "'\\" ['0'-'9'] ['0'-'9'] ['0'-'9'] "'"
      { comment lexbuf }
  | eof 
    { raise(Lexical_error("unterminated comment", Lexing.lexeme_start lexbuf)) }
  | _ 
    { comment lexbuf }

{
}