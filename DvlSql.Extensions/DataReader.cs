using System;
using System.Collections.Generic;
using System.Data;

namespace DvlSql.Extensions
{
    public static class DataReader
    {
        public static Func<IDataReader, TResult?> RecordReaderFunc<TResult>(Func<TResult>? defaultFunc = null) =>
          reader =>
          {
              if (typeof(TResult).IsClass && typeof(TResult).Namespace != "System")
                  return GetObjectOfType(reader, defaultFunc);

              return reader.FieldCount > 0 && reader[0] != DBNull.Value ? (TResult?)reader[0] : defaultFunc == null ? default : defaultFunc();
          };

        public static T? GetObjectOfType<T>(this IDataReader r, Func<T>? defaultFunc = null)
        {
            var instance = Activator.CreateInstance<T>();
            bool anyPropertySet = false;
            var type = typeof(T);
            for (int i = 0; i < r.FieldCount; i++)
            {
                var fieldName = r.GetName(i);
                var prop = type.GetProperty(fieldName);
                if (prop != null &&
                    prop.PropertyType.Namespace == "System" &&
                    //!prop.PropertyType.IsGenericType(typeof(ICollection<>)) &&
                    r[prop.Name] != DBNull.Value)
                {
                    anyPropertySet = true;
                    prop.SetValue(instance, r[prop.Name]);
                }
            }

            return anyPropertySet ? instance : defaultFunc == null ? default : defaultFunc();
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