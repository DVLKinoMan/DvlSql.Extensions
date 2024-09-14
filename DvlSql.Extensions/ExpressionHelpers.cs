using DvlSql.Expressions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using static System.Exts.Extensions;

namespace DvlSql;

public static class ExpressionHelpers
{
    public static string AvgExp(string param) => $"AVG({param})";
    public static string CountExp(string param = "*") => $"COUNT({param})";
    public static string MaxExp(string param) => $"MAX({param})";
    public static string MinExp(string param) => $"MIN({param})";
    public static string SumExp(string param) => $"SUM({param})";
    public static string DistinctExp(string param) => $"DISTINCT({param})";
    public static string AsExp(string field, string @as) =>
        @as != null ? $"{field} AS {@as.WithAliasBrackets()}" : field;
    public static string CoalesceExp<T>(string param, T change) =>
        typeof(T) == typeof(string) || typeof(T) == typeof(Guid) || typeof(T) == typeof(Guid?)
        || typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?)
        ? $"COALESCE({param}, '{(change is DateTime dt ? dt.ToString(CultureInfo.InvariantCulture) : change!.ToString())}')"
        : $"COALESCE({param}, {change})";

    public static DvlSqlAsExpression AsExp(string @as, IEnumerable<string>? @params = null, bool useAs = true) =>
        new(@as, @params, useAs);

    public static string MonthExp(string param) => $"MONTH({param})";
    public static string DayExp(string param) => $"DAY({param})";
    public static string YearExp(string param) => $"YEAR({param})";
    public static string GetDateExp() => $"GETDATE()";
    public static string IsNullExp(string param1, string param2) => $"ISNULL({param1}, {param2})";

    public static string DateDiffExp(string interval, string starting, string ending) =>
        $"DATEDIFF({interval}, {starting}, {ending})";

    public static string ConvertExp(string type, string name) => $"CONVERT({type}, {name})";

    public static DvlSqlWhereExpression WhereExp(DvlSqlBinaryExpression innerExpression, bool isRoot = false) =>
        new DvlSqlWhereExpression(innerExpression).WithRoot(isRoot);

    public static DvlSqlAndExpression AndExp(params DvlSqlBinaryExpression[] innerExpressions) =>
        new(innerExpressions);

    public static DvlSqlAndExpression AndExp(IEnumerable<DvlSqlBinaryExpression> innerExpressions) =>
        new(innerExpressions);

    public static DvlSqlComparisonExpression<T> ComparisonExp<T>(DvlSqlComparableExpression<T> leftExp,
        SqlComparisonOperator op,
        DvlSqlComparableExpression<T> rightExp)
        => new(leftExp, op, rightExp);

    public static DvlSqlConstantExpression<TValue> ConstantExp<TValue>(TValue value) =>
        new(value);

    public static DvlSqlMemberExpression<TValue> MemberExp<TValue>(string memberName) =>
        new(memberName);

    public static DvlSqlOrExpression OrExp(params DvlSqlBinaryExpression[] innerExpressions) =>
        new(innerExpressions);

    public static DvlSqlInExpression InExp(string parameterName, params DvlSqlExpression[] innerExpressions) =>
        new(parameterName, innerExpressions);

    public static DvlSqlSelectExpression SelectExp(int? topNum = null) =>
        new(topNum);

    public static DvlSqlSelectExpression SelectExp(int? topNum = null,
        params string[] paramNames) =>
        new(paramNames.ToHashSet(), topNum);

    public static DvlSqlSelectExpression SelectExp(IEnumerable<string> paramNames,
        int? topNum = null) =>
        new(paramNames.ToHashSet(), topNum);

    public static DvlSqlFromWithTableExpression FromExp(string tableName, string @as, bool withNoLock = false)
    {
        var from = new DvlSqlFromWithTableExpression(tableName, withNoLock)
        {
            As = AsExp(@as)
        };
        return from;
    }

    public static DvlSqlFromWithTableExpression FromExp(string tableName, bool withNoLock = false) =>
        new(tableName, withNoLock);

    public static DvlSqlFromExpression FromExp(DvlSqlFullSelectExpression select, string? @as = null)
    {
        if (@as != null)
            select.As = AsExp(@as);
        return select;
    }

    public static DvlSqlFromExpression FromExp(DvlSqlFromWithTableExpression fromWithTable, string? @as = null)
    {
        if (@as != null)
            fromWithTable.As = AsExp(@as);
        return fromWithTable;
    }

    public static DvlSqlFromExpression ValuesExp<T>(T[] values, DvlSqlAsExpression @as) where T : ITuple =>
        new DvlSqlValuesExpression<T>(values, @as);

    public static DvlSqlFromExpression ValuesExp<T>(T[] values, DvlSqlAsExpression @as, List<DvlSqlParameter> sqlParameters) where T : ITuple =>
        new DvlSqlValuesExpression<T>(values, @as, sqlParameters);

    public static DvlSqlSelectExpression SelectExp(params string[] paramNames) =>
        new(paramNames);

    public static DvlSqlSelectExpression SelectTopExp(int topNum,
        params string[] paramNames) =>
        new(paramNames.ToHashSet(), topNum);

    public static DvlSqlLikeExpression LikeExp(string field, string pattern) =>
        new(field, pattern);

    public static DvlSqlBinaryExpression NotExp(DvlSqlBinaryExpression binaryExpression) =>
        !binaryExpression;

    public static DvlSqlBinaryExpression
        NotInExp(string parameterName, params DvlSqlExpression[] innerExpressions) =>
        NotExp(InExp(parameterName, innerExpressions));

    public static DvlSqlBinaryExpression NotLikeExp(string field, string pattern) =>
        NotExp(LikeExp(field, pattern));

    public static DvlSqlIsNullExpression IsNullExp(DvlSqlExpression expression) =>
        new(expression);

    public static DvlSqlBinaryExpression IsNotNullExp(DvlSqlExpression expression) => NotExp(IsNullExp(expression));

    public static DvlSqlFullSelectExpression FullSelectExp(
        DvlSqlSelectExpression @select,
        DvlSqlFromExpression @from,
        DvlSqlAsExpression? @as = null,
        List<DvlSqlJoinExpression>? @join = null, DvlSqlWhereExpression? @where = null,
        DvlSqlGroupByExpression? groupBy = null, DvlSqlOrderByExpression? orderBy = null,
        DvlSqlSkipExpression? skip = null) =>
        new(@from, @join, @where, groupBy,
            @select, orderBy, skip, @as);

    public static DvlSqlOrderByExpression OrderByExp(params (string column, Ordering ordering)[] @params) =>
        new(@params);

    public static DvlSqlExistsExpression ExistsExp(DvlSqlFullSelectExpression select) =>
        new(select);

    public static DvlSqlSkipExpression SkipExp(int offsetRows, int? fetchNextRows = null) =>
        new(offsetRows, fetchNextRows);

    public static DvlSqlGroupByExpression GroupByExp(params string[] paramNames) =>
        new(paramNames);

    public static DvlSqlTableDeclarationExpression DeclareTableExp(string name) =>
        new(name);

    public static DvlSqlOutputExpression
        OutputExp(DvlSqlTableDeclarationExpression intoTable, params string[] cols) =>
        new(intoTable, cols);

    public static DvlSqlOutputExpression OutputExp(params string[] cols) =>
        new(cols);

    public static DvlSqlValuesExpression<T> ValuesExp<T>(params T[] values) where T : ITuple =>
        new(values);

    public static DvlSqlInnerJoinExpression<T> InnerJoinExp<T>(string tableName,
        string firstTableMatchingCol, string secondTableMatchingCol)
        => new(tableName,
            MemberExp<T>(firstTableMatchingCol) == MemberExp<T>(secondTableMatchingCol));

    public static DvlSqlLeftJoinExpression<T> LeftJoinExp<T>(string tableName,
        string firstTableMatchingCol, string secondTableMatchingCol)
        => new(tableName,
            MemberExp<T>(firstTableMatchingCol) == MemberExp<T>(secondTableMatchingCol));

    public static DvlSqlRightJoinExpression<T> RightJoinExp<T>(string tableName,
        string firstTableMatchingCol, string secondTableMatchingCol)
        => new(tableName,
            MemberExp<T>(firstTableMatchingCol) == MemberExp<T>(secondTableMatchingCol));

    public static DvlSqlFullJoinExpression<T> FullJoinExp<T>(string tableName,
        string firstTableMatchingCol, string secondTableMatchingCol)
        => new(tableName,
            MemberExp<T>(firstTableMatchingCol) == MemberExp<T>(secondTableMatchingCol));

    public static DvlSqlInnerJoinExpression<T> InnerJoinExp<T>(string tableName,
        DvlSqlComparisonExpression<T> comparisonExpression)
        => new(tableName, comparisonExpression);

    public static DvlSqlLeftJoinExpression<T> LeftJoinExp<T>(string tableName,
        DvlSqlComparisonExpression<T> comparisonExpression)
        => new(tableName, comparisonExpression);

    public static DvlSqlRightJoinExpression<T> RightJoinExp<T>(string tableName, DvlSqlComparisonExpression<T> comparisonExpression)
        => new(tableName, comparisonExpression);

    public static DvlSqlFullJoinExpression<T> FullJoinExp<T>(string tableName,
        DvlSqlComparisonExpression<T> comparisonExpression)
        => new(tableName, comparisonExpression);

    public static DvlSqlBinaryEmptyExpression EmptyExp() => new();
}