module Syntax

type VarId = string

type BinOp =
    | Add
    | Sub
    | Mul
    | Lt
    | Le

    override this.ToString() =
        match this with
        | Add -> "+"
        | Sub -> "-"
        | Mul -> "*"
        | Lt -> "<"
        | Le -> "<="

type Expr =
    | Bool of value: bool
    | Int of value: int
    | BinApp of op: BinOp * lhs: Expr * rhs: Expr
    | Var of id: VarId
    | Fun of arg: VarId * body: Expr
    | App of func: Expr * actualArg: Expr
    | If of cond: Expr * _then: Expr * _else: Expr
    | Let of VarId * Expr * Expr

    override this.ToString() =
        match this with
        | Bool true -> "true"
        | Bool false -> "false"
        | Int n -> string n
        | BinApp(op, lhs, rhs) -> sprintf "(%O %O %O)" op lhs rhs
        | Var x -> x
        | Fun(arg, body) -> sprintf "(fun %O %O)" arg body
        | App(func, arg) -> sprintf "(%O %O)" func arg
        | If(cond, _then, _else) -> sprintf "(if %O %O %O)" cond _then _else
        | Let(ident, e1, e2) -> sprintf "(let %O %O %O)" ident e1 e2