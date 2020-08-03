module TypedExpr

module BOption = Base.Option
module BResult = Base.Result

open Type
open UntypedExpr

type TEVarId = string

type TEBinOp =
    | TEAdd
    | TESub
    | TEMul
    | TEEq
    | TELt
    | TELe

type TypedExpr =
    | TEUnit
    | TEBool of bool
    | TEInt of int
    | TEBinApp of op: TEBinOp * lhs: TypedExpr * rhs: TypedExpr
    | TEVar of TEVarId
    | TEFun of arg: TEVarId * argType: Type * body: TypedExpr
    | TEApp of func: TypedExpr * actualArg: TypedExpr
    | TEIf of cond: TypedExpr * _then: TypedExpr * _else: TypedExpr
    | TELet of name: TEVarId * typeOfName: Type * expr1: TypedExpr * expr2: TypedExpr
    | TEBegin of TypedExpr * List<TypedExpr>
    | TEMakeRef of TypedExpr
    | TEDeref of TypedExpr
    | TEMut of TypedExpr * TypedExpr

let assertType (expected: Type) (actual: Type): Result<unit, string> =
    if expected = actual
    then Ok()
    else Error(sprintf "expected: %O, actual: %O" expected actual)

let rec typeCheck (env: TypeEnv) (expr: TypedExpr): Result<Type * UntypedExpr, string> =
    match expr with
    | TEUnit -> Ok(TUnit, UUnit)
    | TEBool b -> Ok(TBool, UBool b)
    | TEInt n -> Ok(TInt, UInt n)
    | TEBinApp(op, lhs, rhs) -> typeCheckBinApp env op lhs rhs
    | TEVar x ->
        BResult.result {
            let! ty = List.tryFind (fst >> (=) x) env
                      |> Option.map snd
                      |> BOption.toResult
                      |> Result.mapError
                          (fun () -> sprintf "(TypeError) Unbound identifier: %s" x)
            return (ty, UVar x)
        }
    | TEFun(x, ty, body) ->
        BResult.result {
            let! (bodyType, body) = typeCheck ((x, ty) :: env) body
            return (TFun(ty, bodyType), UFun(x, body)) }
    | TELet(x, ty, e1, e2) ->
        let mapError =
            Result.mapError (sprintf "(TypeError) Let:\n\t%O")
        BResult.result {
            let! (e1Type, e1) = typeCheck env e1
            do! mapError (assertType ty e1Type)
            let! (e2Type, e2) = typeCheck ((x, ty) :: env) e2
            return (e2Type, ULet(x, e1, e2))
        }
    | _ -> Error "unimplemented"

and typeCheckBinApp (env: TypeEnv) (op: TEBinOp) (lhs: TypedExpr) (rhs: TypedExpr) =
    let mapError =
        Result.mapError (sprintf "(TypeError) BinApp:\n\t%O")
    BResult.result {
        let! (lhsType, lhs) = typeCheck env lhs
        let! (rhsType, rhs) = typeCheck env rhs
        match op with
        | TEAdd ->
            do! mapError (assertType TInt lhsType)
            do! mapError (assertType TInt rhsType)
            return (TInt, UBinApp(Add, lhs, rhs))
        | TESub ->
            do! mapError (assertType TInt lhsType)
            do! mapError (assertType TInt rhsType)
            return (TInt, UBinApp(Sub, lhs, rhs))
        | TEMul ->
            do! mapError (assertType TInt lhsType)
            do! mapError (assertType TInt rhsType)
            return (TInt, UBinApp(Mul, lhs, rhs))
        | TEEq ->
            do! mapError (assertType TInt lhsType)
            do! mapError (assertType TInt rhsType)
            return (TBool, UBinApp(Eq, lhs, rhs))
        | TELt ->
            do! mapError (assertType TInt lhsType)
            do! mapError (assertType TInt rhsType)
            return (TBool, UBinApp(Lt, lhs, rhs))
        | TELe ->
            do! mapError (assertType TInt lhsType)
            do! mapError (assertType TInt rhsType)
            return (TBool, UBinApp(Le, lhs, rhs))
    }
