using System;
using System.Collections.Generic;
using System.Data;
using System.Exts;

namespace DvlSql.Extensions
{
    public static class DataReader
    {
        public static Func<IDataReader, TResult> RecordReaderFunc<TResult>() =>
            reader =>
            {
                if (typeof(TResult).IsClass && typeof(TResult).Namespace != "System")
                    return reader.GetObjectOfType<TResult>();

                return (TResult)reader[0];
            };

        public static T GetObjectOfType<T>(this IDataReader r)
        {
            var instance = Activator.CreateInstance<T>();
            foreach (var innerProp in typeof(T).GetProperties())
                if (innerProp.PropertyType.Namespace == "System" &&
                    !innerProp.PropertyType.IsGenericType(typeof(ICollection<>)) &&
                    r[innerProp.Name] != DBNull.Value)
                    innerProp.SetValue(instance, r[innerProp.Name]);

            return instance;
        }

        public static Func<IDataReader, List<TResult>> AsList<TResult>(Func<IDataReader, TResult> selector) =>
            reader =>
        {
            var list = new List<TResult>();
            while (reader.Read())
                list.Add(selector(reader));

            return list;
        };

        public static Func<IDataReader, Dictionary<TKey, List<TValue>>> AsDictionary<TKey, TValue>(
            Func<IDataReader, TKey> keySelector, Func<IDataReader, TValue> valueSelector) =>
            reader =>
            {
                var dict = new Dictionary<TKey, List<TValue>>();
                while (reader.Read())
                {
                    var key = keySelector(reader);
                    var value = valueSelector(reader);
                    if (!dict.TryGetValue(key, out var collection))
                        dict.Add(key, [value]);
                    else collection.Add(value);
                }

                return dict;
            };

        public static Func<IDataReader, TResult> First<TResult>(Func<IDataReader, TResult> selector) =>
            reader => reader.Read()
                ? selector(reader)
                : throw new InvalidOperationException("There was no element in sequence");

        public static Func<IDataReader, TResult?> FirstOrDefault<TResult>(Func<IDataReader, TResult> selector) =>
            reader => reader.Read() ? selector(reader) : default;

        public static Func<IDataReader, TResult> Single<TResult>(Func<IDataReader, TResult> selector) =>
            reader =>
                IsSingleDataReader(reader, selector) switch
                {
                    (true, var value) => value!,
                    _ => throw new InvalidOperationException(
                        "There was no element in sequence or there was more than 1 elements")
                };

        public static Func<IDataReader, TResult?> SingleOrDefault<TResult>(Func<IDataReader, TResult> selector) =>
            reader =>
                IsSingleDataReader(reader, selector) switch
                {
                    (true, var value) => value,
                    _ => default
                };

        private static (bool isSingle, TResult? result) IsSingleDataReader<TResult>(IDataReader reader, Func<IDataReader, TResult> func)
        {
            if (!reader.Read())
                return (default, default);

            var firstValue = func(reader);
            return reader.Read() ? (false, firstReader: firstValue) : (true, firstReader: firstValue);
        }
        
    }
}