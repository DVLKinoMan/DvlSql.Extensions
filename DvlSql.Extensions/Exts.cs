using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DvlSql.Extensions
{
    public static class Exts
    {
        public static T? GetVal<T>(this IDataReader reader, string param, T? def = default) => reader[param] != DBNull.Value ? (T)reader[param] : def;

        public static async Task<T> GetAsync<T>(this ISelector @from, DvlSqlFilter filter,
            Func<IDataReader, T> selector)
        {
            //todo: more useful exception
            if (filter == null)
                throw new Exception("Must contain Id");

            var fl = JoinsAndWheresFromFiltersByParams(@from, filter);

            return await fl
                .Select()
                .FirstAsync(selector);
        }

        public static async Task<List<T>> GetListAsync<T>(this ISelector @from, IEnumerable<DvlSqlFilter> filters, Func<IDataReader, T> selector)
        {
            if (filters == null)
                return await @from
                    .Select()
                    .ToListAsync(selector);

            var fl = JoinsAndWheresFromFilters(@from, filters);

            return await fl
                .Select()
                .ToListAsync(selector);
        }

        public static async Task<Dictionary<TKey, List<TValue>>> GetDictionaryAsync<TKey, TValue>(this ISelector @from,
            IEnumerable<DvlSqlFilter> filters, Func<IDataReader, TKey> keySelector,
            Func<IDataReader, TValue> valueSelector)
        {
            if (filters == null)
                return await @from
                    .Select()
                    .ToDictionaryAsync(keySelector, valueSelector);

            var fl = JoinsAndWheresFromFilters(@from, filters);

            return await fl
                .Select()
                .ToDictionaryAsync(keySelector, valueSelector);
        }

        private static ISelectable JoinsAndWheresFromFiltersByParams(ISelector @from, params DvlSqlFilter[] filters) =>
            JoinsAndWheresFromFilters(@from, filters);

        private static ISelectable JoinsAndWheresFromFilters(ISelector @from, IEnumerable<DvlSqlFilter> filters)
        {
            var dvlSqlFilters = filters as DvlSqlFilter[] ?? filters.ToArray();

            foreach (var join in dvlSqlFilters.OfType<DvlSqlJoinFilter>()
                .Distinct(new DvlSqlJoinFilterEqualityComparer()))
                @from = @join.Join(@from);

            var fl = (ISelectable) @from;
            if (dvlSqlFilters.OfType<DvlSqlWhereFilter>().AggregateIfNotNull((f1, f2) => f1 & f2) is { } whereFilter)
                fl = @from.Where(whereFilter.BinaryExpression, whereFilter.Parameters);

            return fl;
        }

        public static IEnumerable<DvlSqlFilter> Concat(params IEnumerable<DvlSqlFilter>[] filtersArr)
        {
            foreach (var filters in filtersArr)
            foreach (var filter in filters)
                if (!(filter is DvlSqlEmptyFilter))
                    yield return filter;
        }

        public static TSource? AggregateIfNotNull<TSource>(this IEnumerable<TSource> enumerable,
            Func<TSource, TSource, TSource> func)
        {
            var source = enumerable as TSource[] ?? enumerable.ToArray();
            return source.Length == 0 ? default : source.Aggregate(func);
        }

    }
}
