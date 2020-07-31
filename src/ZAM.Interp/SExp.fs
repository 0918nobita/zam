module SExp

open Syntax

type Atom =
    | SBool of bool
    | SInt of int
    | Symbol of string

type SExp =
    | Atom of Atom
    | SList of SExp list

let strToBinOp =
    function
    | "+" -> Add
    | "-" -> Sub
    | "*" -> Mul
    | "<" -> Lt
    | "<=" -> Le
    | _ -> failwith "fatal error"

open FSharpPlus.Builders

let rec sexpToExpr (sexp: SExp): Result<Expr, string> =
    match sexp with
    | SList [ Atom(Symbol(op & ("+"
              | "-"
              | "*"
              | "<"
              | "<="))); lhs; rhs ] ->
        monad.fx' {
            let! lhs = sexpToExpr lhs
            let! rhs = sexpToExpr rhs
            return BinApp(strToBinOp op, lhs, rhs) }
    | SList [ Atom(Symbol "λ"); Atom(Symbol arg); body ] ->
        monad.fx' {
            let! body = sexpToExpr body
            return Fun(arg, body) }
    | SList [ Atom(Symbol "if"); cond; _then; _else ] ->
        monad.fx' {
            let! cond = sexpToExpr cond
            let! _then = sexpToExpr _then
            let! _else = sexpToExpr _else
            return If(cond, _then, _else) }
    | SList [ Atom(Symbol "let"); Atom(Symbol name); value; body ] ->
        monad.fx' {
            let! value = sexpToExpr value
            let! body = sexpToExpr body
            return Let(name, value, body) }
    | SList [ func; arg ] ->
        monad.fx' {
            let! func = sexpToExpr func
            let! arg = sexpToExpr arg
            return App(func, arg) }
    | SList _ -> Error(sprintf "bad syntax: %O" sexp)
    | Atom(SBool b) -> Ok(Bool(b))
    | Atom(SInt n) -> Ok(Int(n))
    | Atom(Symbol x) -> Ok(Var(x))
