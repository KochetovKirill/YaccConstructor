%{
(** Module Parser
 *
 *  Author: Jk
 *)

  open IL
  open IL.Production
  open IL
  open Misc

  let getList = function Some x -> x | None -> []
  (*let parse_error (s:string):unit = ()*)
  let missing name = print_endline ("Missing " ^ name)
  let createSeqElem bnd omitted r check = 
      { binding = bnd; omit = omitted; rule = r; checker = check }
%}

%token EOF
%token COLON
%token SEMICOLON
%token EQUAL
%token BAR
%token STAR
%token PLUS
%token QUESTION
%token LPAREN
%token RPAREN
%token DGREAT
%token DLESS
%token COMMUT
%token <IL.Source.t> STRING LIDENT UIDENT
%token <IL.Source.t> PREDICATE ACTION
%token <IL.Source.t> PATTERN PARAM


%start file
%type <(IL.Source.t, IL.Source.t) IL.Definition.t> file

%%

file: action_opt 
      rule_nlist
      action_opt 
      EOF
      { { 
          Definition.head=$1
        ; Definition.grammar=$2
        ; Definition.foot=$3 
        } 
      }

action_opt: ACTION { Some $1 } 
          | /* empty */ { None }

rule_nlist: rule SEMICOLON rule_nlist { $1 :: $3 }
          | rule SEMICOLON { [$1] }
          | rule error { missing "SEMI"; raise Parsing.Parse_error  }

rule: plus_opt LIDENT formal_meta_param_opt param_opt COLON alts
      { { 
          Rule.public = $1
        ; Rule.name = Source.toString $2
        ; Rule.metaArgs = getList $3
        ; Rule.body = $6
        ; Rule.args = o2l $4
      } }

plus_opt: PLUS      {true} 
        | /*empty*/ {false}

formal_meta_param_opt: DLESS formal_meta_list DGREAT {Some $2} 
                     | /* empty */ {None} 

formal_meta_list: LIDENT {[$1]} 
                | LIDENT formal_meta_list {$1::$2}

param_opt: PARAM {Some $1} | /* empty */ {None} 

alts: seq { $1 } | seq bar_seq_nlist {PAlt ($1,$2)}

bar_seq_nlist : BAR seq bar_seq_nlist {PAlt($2,$3)}  
              | BAR seq {$2}

seq: seq_elem seq_elem_list action_opt { PSeq ($1::$2, $3) }
   | perms_list   {$1}
   | ACTION {PSeq([],Some $1)}

perms_list:perm COMMUT perms_list {PPerm($1)::PPerm($3)}
   | /* empty */ {[]}

perm: LIDENT {$1}

seq_elem_list: seq_elem seq_elem_list {$1::$2}
             | /* empty */ {[]}

seq_elem: bound predicate_opt {{$1 with checker = $2}}

predicate_opt: PREDICATE   { Some $1 }
             | /* empty */ { None    }

bound: patt EQUAL prim { createSeqElem (Some $1) false $3 None } 
     | prim            { createSeqElem None false $1 None      }

patt: LIDENT {$1} | PATTERN {$1}

prim: prim STAR           {PMany $1}
    | prim PLUS           {PSome $1} 
    | prim QUESTION       {POpt $1}
    | LPAREN alts RPAREN  {$2}
    | call                {$1}
    | STRING              {PLiteral $1}

meta_param: LIDENT {$1} 
          | UIDENT {$1}
          | STRING {$1}

meta_params: meta_param {[$1]}
           | meta_param meta_params {$1 :: $2}

meta_param_opt: /* empty */ {None}
              | DLESS meta_params DGREAT {Some $2}

call: UIDENT             {PToken $1}
    | LIDENT meta_param_opt param_opt  
      { match $2 with 
        None -> PRef  ($1, $3)
        | Some x -> PMetaRef ($1,$3,x)
      }
