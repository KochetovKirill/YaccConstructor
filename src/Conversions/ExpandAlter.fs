﻿//   Copyright 2013, 2014 YaccConstructor Software Foundation
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

module Yard.Core.Conversions.ExpandTopLevelAlt

open Yard.Core
open Yard.Core.IL
open Yard.Core.IL.Production
open Yard.Core.IL.Production
open Mono.Addins

open System

let extractOneRule (rule:Rule.t<'a,'b>) = 
    let rec expand = function
    | PAlt (a,b) -> expand a @ expand b
    | a   -> [{rule with body = a}]
    expand rule.body


[<assembly:Addin>]
[<assembly:AddinDependency ("YaccConstructor", "1.0")>]
do()

[<Extension>]
type ExpandTopLevelAlt() = 
    inherit Conversion()
        override this.Name = "ExpandTopLevelAlt"
        override this.ConvertGrammar (grammar,_) = mapGrammar (List.collect extractOneRule) grammar
