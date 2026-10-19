using System;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.Text;
using WezProjectifier.Models;

namespace WezProjectifierBLL.Models
{
    public sealed record Result<T>(T? Value, enOperationResult OperationResult)
    {
        public bool IsSuccess => OperationResult == enOperationResult.Success;

        public static Result<T> Success(T value) => new(value, enOperationResult.Success);
        public static Result<T> Failure(enOperationResult op) => new(default, op);
    }
}
