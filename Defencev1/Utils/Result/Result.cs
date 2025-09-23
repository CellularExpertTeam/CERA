using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Defencev1.Utils.Result;
public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Error = "";
    }

    private Result(string error)
    {
        IsSuccess = false;
        Value = default!;
        Error = error;
    }

    public static Result<T> Ok(T value) => new(value);
    public static Result<T> Fail(string error) => new(error);
}

public static class ResultExtensions
{
    // LINQ Select
    public static Result<U> Select<T, U>(this Result<T> r, Func<T, U> f) =>
        r.IsSuccess ? Result<U>.Ok(f(r.Value)) : Result<U>.Fail(r.Error);

    // LINQ SelectMany (needed for `from … in … from … in … select …`)
    public static Result<V> SelectMany<T, U, V>(
        this Result<T> r,
        Func<T, Result<U>> bind,
        Func<T, U, V> project) =>
        r.IsSuccess
            ? bind(r.Value).Select(u => project(r.Value, u))
            : Result<V>.Fail(r.Error);
}
