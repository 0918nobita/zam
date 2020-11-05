namespace Psyche.Base

module Monad =
    [<Struct>]
    type MonadClass<'a, 'Ma, 'Mb> = {
        Bind: ('a -> 'Mb) -> 'Ma -> 'Mb
        Return: 'a -> 'Ma
    }

    type MonadBuiltin = MonadBuiltin with
        static member MonadImpl (_: _ option) =
            { Bind = Option.bind; Return = Some }

        static member MonadImpl (_: Result<_, _>) =
            { Bind = Result.bind; Return = Ok }

    let inline getImpl
        (builtin: ^Builtin)
        (dummy: MonadClass< ^a, ^Ma, ^Mb >): MonadClass< ^a, ^Ma, ^Mb > =
        ((^Builtin or ^Ma): (static member MonadImpl: ^Ma -> MonadClass< ^a, ^Ma, ^Mb >) (Unchecked.defaultof< ^Ma >))

    let inline bind_ (f: ^a -> ^Mb) (x: ^Ma): ^Mb =
        (getImpl MonadBuiltin (Unchecked.defaultof< MonadClass< ^a, ^Ma, ^Mb > >)).Bind f x

    let inline return_ (x: ^a): ^Ma =
        (getImpl MonadBuiltin (Unchecked.defaultof< MonadClass< ^a, ^Ma, _ > >)).Return x

    type MonadBuilder() =
        member inline _.Bind (x, f) = bind_ f x
        member inline _.Return x = return_ x
        member inline _.ReturnFrom mx = mx

    let monad = MonadBuilder()
