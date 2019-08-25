﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Reactive;
using System.Text;
using System.Text.RegularExpressions;
using MathCore;
using MathCore.Annotations;
using MathCore.Values;
using static System.Diagnostics.Contracts.Contract;

// ReSharper disable once CheckNamespace
namespace System.Linq
{
    /// <summary>Класс методов-расширений для интерфейса перечисления</summary>
    public static class IEnumerableExtensions
    {
        [NotNull]
        public static IEnumerable<T> Except<T>([NotNull] this IEnumerable<T> items, T item)
        {
            foreach (var i in items)
                if (!Equals(i, item))
                    yield return i;
        }

        public static int FirstIndexOf<T>([NotNull, ItemCanBeNull] this IEnumerable<T> collection, [CanBeNull] T item)
        {
            var index = -1;
            foreach (var element in collection)
            {
                index++;
                if (Equals(element, item)) return index;
            }
            return index;
        }

        public static int LastIndexOf<T>([NotNull, ItemCanBeNull] this IEnumerable<T> collection, [CanBeNull] T item)
        {
            var i = 0;
            var index = -1;
            foreach (var element in collection)
            {
                if (Equals(element, item)) index = i;
                i++;
            }
            return index;
        }

        public static IEnumerable Concat([CanBeNull] this IEnumerable source, [CanBeNull] IEnumerable other)
        {
            if (source != null) foreach (var src in source) yield return src;
            if(other != null) foreach (var oth in other) yield return oth;
        }

        [CanBeNull]
        public static LambdaEnumerable<TValue> GetLamdaEnumerable<TObject, TValue>(this TObject obj,
            Func<TObject, IEnumerable<TValue>> Creator) => new LambdaEnumerable<TValue>(() => Creator(obj));

        [CanBeNull]
        public static IEnumerable<T> WhereNotNull<T>([CanBeNull] this IEnumerable<T> items) where T : class => items?.Where(i => !ReferenceEquals(i, null));

        /// <summary>Фильтрация последовательности строк по указанному регулярному выражению</summary>
        /// <param name="strings">Последовательность строк</param>
        /// <param name="regex">Регулярное выражение-фильтр</param>
        /// <returns>Последовательность строк, удовлетворяющая регулярному выражению</returns>
        [CanBeNull]
        public static IEnumerable<string> Where([CanBeNull] this IEnumerable<string> strings, [NotNull] Regex regex)
        {
            Requires(regex != null);
            Ensures((strings == null) == (Result<IEnumerable<string>>() == null));

            return strings?.Where((Func<string, bool>)regex.IsMatch);
        }

        /// <summary>Фильтрация последовательности строк, которые не удовлетворяют регулярному выражению</summary>
        /// <param name="strings">Фильтруемая последовательность строк</param>
        /// <param name="regex">Регулярное выражение-фильтр</param>
        /// <returns>Последовательность строк, которые не удовлетворяют регулярному выражению</returns>
        [CanBeNull]
        public static IEnumerable<string> WhereNot([CanBeNull] this IEnumerable<string> strings, [NotNull] Regex regex)
        {
            Requires(regex != null);
            Ensures((strings == null) == (Result<IEnumerable<string>>() == null));

            return strings?.WhereNot(regex.IsMatch);
        }

        /// <summary>Фильтрация последовательности строк по указанному регулярному выражению</summary>
        /// <param name="strings">Последовательность строк</param>
        /// <param name="regex">Регулярное выражение-фильтр</param>
        /// <returns>Последовательность строк, удовлетворяющая регулярному выражению</returns>
        [CanBeNull]
        public static IEnumerable<string> Where([CanBeNull] this IEnumerable<string> strings, [NotNull] string regex)
        {
            Requires(!string.IsNullOrEmpty(regex));
            Ensures((strings == null) == (Result<IEnumerable<string>>() == null));

            return strings?.Where(s => Regex.IsMatch(s, regex));
        }

        /// <summary>Фильтрация последовательности строк, которые не удовлетворяют регулярному выражению</summary>
        /// <param name="strings">Фильтруемая последовательность строк</param>
        /// <param name="regex">Регулярное выражение-фильтр</param>
        /// <returns>Последовательность строк, которые не удовлетворяют регулярному выражению</returns>
        [CanBeNull]
        public static IEnumerable<string> WhereNot([CanBeNull] this IEnumerable<string> strings, [NotNull] string regex)
        {
            Requires(!string.IsNullOrEmpty(regex));
            Ensures((strings == null) == (Result<IEnumerable<string>>() == null));

            return strings?.WhereNot(s => Regex.IsMatch(s, regex));
        }

        /// <summary>Выполняет фильтрацию последовательности значений на основе заданного предиката</summary>
        /// <returns>
        /// Объект <see cref="T:System.Collections.Generic.IEnumerable`1"/>, содержащий элементы входной последовательности, которые не удовлетворяют условию.
        /// </returns>
        /// <param name="collection">Объект <see cref="T:System.Collections.Generic.IEnumerable`1"/>, подлежащий фильтрации.</param>
        /// <param name="NotSelector">Функция для проверки каждого элемента на не соответствие условию.</param>
        /// <typeparam name="T">Тип элементов последовательности <paramref name="collection"/>.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">Значение параметра <paramref name="collection"/> или <paramref name="NotSelector"/> — null.</exception>
        [CanBeNull]
        public static IEnumerable<T> WhereNot<T>([CanBeNull]this IEnumerable<T> collection, [NotNull]Func<T, bool> NotSelector)
        {
            Requires(NotSelector != null);
            Ensures((collection == null) == (Result<IEnumerable<T>>() == null));

            return collection?.Where(t => !NotSelector(t));
        }

        /// <summary>Возвращает цепочку элементов последовательности, удовлетворяющих указанному условию</summary>
        /// <returns>
        /// Объект <see cref="T:System.Collections.Generic.IEnumerable`1"/>, содержащий элементы входной последовательности до первого элемента, который не прошел проверку.
        /// </returns>
        /// <param name="collection">Последовательность, из которой требуется возвратить элементы.</param>
        /// <param name="NotSelector">Функция для проверки каждого элемента на соответствие условию.</param>
        /// <typeparam name="T">Тип элементов последовательности <paramref name="collection"/>.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">Значение параметра <paramref name="collection"/> или <paramref name="NotSelector"/> — null.</exception>
        [CanBeNull]
        public static IEnumerable<T> TakeWhileNot<T>([CanBeNull]this IEnumerable<T> collection, [NotNull]Func<T, bool> NotSelector)
        {
            Requires(NotSelector != null);
            Ensures((collection == null) == (Result<IEnumerable<T>>() == null));

            return collection?.TakeWhile(t => !NotSelector(t));
        }

        /// <summary>Преобразование перечисления в массив с преобразованием элементов</summary>
        /// <typeparam name="TItem">Тип элементов исходного перечисления</typeparam>
        /// <typeparam name="TValue">Тип элементов результирующего массива</typeparam>
        /// <param name="collection">Исходное перечисление</param>
        /// <param name="converter">Метод преобразования элементов</param>
        /// <returns>
        /// Если ссылка на исходное перечисление не пуста, то
        ///     Результирующий массив, состоящий из элементов исходного перечисления, преобразованных указанным методом
        /// иначе
        ///     пустая ссылка на массив
        /// </returns>
        [CanBeNull]
        public static TValue[] ToArray<TItem, TValue>([CanBeNull] this IEnumerable<TItem> collection, [NotNull] Func<TItem, TValue> converter)
        {
            Requires(converter != null);
            Ensures((collection == null) == (Result<TValue[]>() == null));

            return collection?.Select(converter).ToArray();
        }

        /// <summary>Преобразование перечисления в список с преобразованием элементов</summary>
        /// <typeparam name="TItem">Тип элементов исходного перечисления</typeparam>
        /// <typeparam name="TValue">Тип элементов результирующего списка</typeparam>
        /// <param name="collection">Исходное перечисление</param>
        /// <param name="converter">Метод преобразования элементов</param>
        /// <returns>
        /// Если ссылка на исходное перечисление не пуста, то
        ///     Результирующий список, состоящий из элементов исходного перечисления, преобразованных указанным методом
        /// иначе
        ///     пустая ссылка на список
        /// </returns>
        [CanBeNull]
        public static List<TValue> ToList<TItem, TValue>([CanBeNull] this IEnumerable<TItem> collection, [NotNull] Func<TItem, TValue> converter)
        {
            Requires(converter != null);
            Ensures((collection == null) == (Result<List<TValue>>() == null));

            return collection?.Select(converter).ToList();
        }

        public static Dictionary<TKey, T> ToDictionaryDistinctKeys<T, TKey>(this IEnumerable<T> collection, Func<T, TKey> KeySelector, bool OverloadValues = false)
        {
            var dic = new Dictionary<TKey, T>();
            if (OverloadValues)
                foreach (var item in collection)
                {
                    var key = KeySelector(item);
                    if (dic.ContainsKey(key)) continue;
                    dic.Add(key, item);
                }
            else
                foreach (var item in collection)
                {
                    var key = KeySelector(item);
                    dic[key] = item;
                }
            return dic;
        }

        public static Dictionary<TKey, TValue> ToDictionaryDistinctKeys<T, TKey, TValue>(this IEnumerable<T> collection, Func<T, TKey> KeySelector, Func<T, TValue> ValueSelector, bool OverloadValues = false)
        {
            var dic = new Dictionary<TKey, TValue>();
            if (OverloadValues)
                foreach (var item in collection)
                {
                    var key = KeySelector(item);
                    if (dic.ContainsKey(key)) continue;
                    dic.Add(key, ValueSelector(item));
                }
            else
                foreach (var item in collection)
                {
                    var key = KeySelector(item);
                    dic[key] = ValueSelector(item);
                }
            return dic;
        }

        /// <summary>Объединение перечисления строк в единую строку с разделителем - переносом строки</summary>
        /// <param name="Lines">Перечисление строк</param>
        /// <returns>Если ссылка на перечисление пуста, то пустая ссылка на строку, иначе - объединение строк с разделителем - переносом строки</returns>
        [CanBeNull]
        public static string Aggregate([CanBeNull] this IEnumerable<string> Lines)
        {
            Ensures((Lines == null) == (Result<string>() == null));

            return Lines?.Aggregate(new StringBuilder(), (sb, s) => sb.AppendLine(s), sb => sb.ToString());
        }

        /// <summary>Добавить элементы перечисления в коллекцию</summary>
        /// <typeparam name="T">Тип элемента</typeparam>
        /// <param name="source">Перечисление добавляемых элементов</param>
        /// <param name="collection">Коллекция-приёмник элементов</param>
        public static void AddTo<T>([NotNull] this IEnumerable<T> source, [NotNull] ICollection<T> collection)
        {
            Requires(source != null);
            Requires(collection != null);
            foreach (var item in source) collection.Add(item);
        }

        /// <summary>Удалить перечисление элементов из коллекци</summary>
        /// <typeparam name="T">Тип элементов</typeparam>
        /// <param name="source">Перечисление удаляемых элементов</param>
        /// <param name="collection">Коллекция, из которой производится удаление</param>
        /// <returns>Перечисление результатов операций удаления для каждого из элементов исходного перечисления</returns>
        [NotNull]
        public static bool[] RemoveFrom<T>([NotNull] this IEnumerable<T> source, [NotNull] ICollection<T> collection)
        {
            Requires(source != null);
            Requires(collection != null);
            Ensures(Result<bool[]>() != null);
            return source.Select(collection.Remove).ToArray();
        }


        /// <summary>Добавить в словарь</summary>
        /// <typeparam name="TKey">Тип ключей словаря</typeparam>
        /// <typeparam name="TValue">Тип значений словаря</typeparam>
        /// <param name="collection">Коллекция элементов, добавляемых в словарь</param>
        /// <param name="dictionary">Словарь, в который добавляются элементы</param>
        /// <param name="converter">Метод определения ключа элемента</param>
        public static void AddToDictionary<TKey, TValue>
        (
            [CanBeNull] this IEnumerable<TValue> collection,
            [NotNull] IDictionary<TKey, TValue> dictionary,
            [NotNull] Func<TValue, TKey> converter
        )
        {
            Requires(dictionary != null);
            Requires(converter != null);

            if (collection == null) return;
            foreach (var value in collection)
                dictionary.Add(converter(value), value);
        }

        /// <summary>Преобразовать последовательность одних элементов в последовательность других элементов с использованием механизма конвертации</summary>
        /// <typeparam name="TItem">Тип исходных элементов</typeparam>
        /// <typeparam name="TValue">Тип элементов, в которые преобразуются исходные</typeparam>
        /// <param name="collection">Последовательность исходных элементов</param>
        /// <returns>Последовательность элементов преобразованного типа</returns>
        [CanBeNull]
        public static IEnumerable<TValue> ConvertToType<TItem, TValue>([CanBeNull] this IEnumerable<TItem> collection)
        {
            Ensures((collection == null) == (Result<IEnumerable<TValue>>() == null));

            var target_type = typeof(TValue);
            var source_type = typeof(TItem);

            if(source_type == target_type) return (IEnumerable<TValue>)collection;
            if(collection == null) return null;


            Func<object, object> type_converter = null;
            var converter = source_type == typeof(object)
                        ? o => (TValue)target_type.Cast(o)
                        : (Func<TItem, TValue>)(o => (TValue)(type_converter ?? (type_converter = target_type.GetCasterFrom(source_type)))(o));
            return collection.Select(converter);
        }

        /// <summary>Создать последовательность элементов, каждое значение в которой будет получено на основе двух значений исходной последовательности</summary>
        /// <typeparam name="TItem">Тип элементов исходной последовательности</typeparam>
        /// <typeparam name="TValue">Тип элементов последовательности сконвертированных элементов</typeparam>
        /// <param name="collection">Исходная последовательность элементов</param>
        /// <param name="converter">
        /// Метод преобразования, в который передаётся исходный элемент последовательности, предыдущий элемент последовательности, 
        /// и на основе двух этих элементов, он определяет значение элемента результирующей последовательности</param>
        /// <returns>Последовательность элементов, составляемая из преобразованных элементов исходной последовательности, где метод преобразования учитывает значение предшествующего элемента</returns>
        [NotNull]
        public static IEnumerable<TValue> SelectWithLastValue<TItem, TValue>
        (
            [CanBeNull] this IEnumerable<TItem> collection,
            [NotNull] Func<TItem, TItem, TValue> converter
        )
        {
            Requires(converter != null);

            if (collection == null) yield break;
            var first = true;
            var last = default(TItem);
            foreach(var item in collection)
            {
                if (first)
                {
                    last = item;
                    first = false;
                    continue;
                }
                yield return converter(last, item);
                last = item;
            }
        }

        /// <summary>Выполнить действие для первого элемента последовательности</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="collection">Последовательность элементов</param>
        /// <param name="action">Действие, выполняемое для первого элемента последовательности в момент её перечисления</param>
        /// <returns>Исходная последовательность элементов</returns>
        [NotNull]
        public static IEnumerable<T> AtFirst<T>([CanBeNull] this IEnumerable<T> collection, [NotNull] Action<T> action)
        {
            Requires(action != null);
            Ensures(Result<IEnumerable<T>>() != null);

            if (collection == null) yield break;
            var first = true;
            foreach (var item in collection)
            {
                if (first)
                {
                    action(item);
                    first = false;
                }
                yield return item;
            }
        }

        /// <summary>Выполнить действие для последнего элемента последовательности</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="collection">Последовательность элементов</param>
        /// <param name="action">Действие, выполняемое для последнего элемента в момент её перечисления. Если последовательность элементов просмотрена до конца</param>
        /// <returns>Исходная последовательность элементов</returns>
        [NotNull]
        public static IEnumerable<T> AtLast<T>([CanBeNull] this IEnumerable<T> collection, [NotNull] Action<T> action)
        {
            Requires(action != null);
            Ensures(Result<IEnumerable<T>>() != null);

            if (collection == null) yield break;
            var last = default(T);
            var any = false;
            foreach (var item in collection)
            {
                any = true;
                last = item;
                yield return last;
            }
            if (any) action(last);
        }

        /// <summary> Выполнить действие до начала перечисления последовательности</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="collection">Последовательность элементов</param>
        /// <param name="ActionBefore">Действие, выполняемое до начала перечисления элементов последовательности</param>
        /// <returns>Исходная последовательность элементов</returns>
        [NotNull]
        public static IEnumerable<T> Before<T>([CanBeNull] this IEnumerable<T> collection, [NotNull] Action ActionBefore)
        {
            Requires(ActionBefore != null);
            Ensures(Result<IEnumerable<T>>() != null);

            if (collection == null) yield break;
            ActionBefore();
            foreach (var item in collection) yield return item;
        }

        /// <summary>Выполнение действия по завершению перечисления коллекции</summary>
        /// <typeparam name="T">Тип элементов коллекции</typeparam>
        /// <param name="collection">Коллекция элементов</param>
        /// <param name="CompliteAction">ДЕйствие, выполняемое по завершению перечисления коллекции</param>
        /// <returns>Исходная последовательность элементов</returns>
        [NotNull]
        public static IEnumerable<T> OnComplite<T>([CanBeNull] this IEnumerable<T> collection, [NotNull] Action CompliteAction)
        {
            Requires(CompliteAction != null);
            Ensures(Result<IEnumerable<T>>() != null);

            if (collection == null) yield break;
            foreach (var item in collection) yield return item;
            CompliteAction();
        }

        /// <summary>История перечисления последовательности элементов</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        public sealed class EnumerableHystory<T> : IEnumerable<T>, IObservable<T>
        {
            /// <summary>Длина истории</summary>
            private int _HystoryLength;

            /// <summary>Список элементов в истории</summary>
            [NotNull]
            private readonly List<T> _Queue;

            /// <summary>Объект-наблюдения за историей</summary>
            [NotNull]
            private readonly SimpleObservableEx<T> _ObservableObject = new SimpleObservableEx<T>();

            /// <summary>Текущий элемент перечисления</summary>
            public T Current { get; private set; }

            /// <summary>Длина истории</summary>
            [MinValue(0)]
            public int Length { get => _HystoryLength; set { _HystoryLength = value; Check(); } }

            /// <summary>Количество элементов в истории</summary>
            [MinValue(0)]
            public int Count => _Queue.Count;

            /// <summary>Доступ к элементам истории начиная с текущего</summary>
            /// <param name="i">Индекс элемента в истории, где 0 - текущий элемент</param>
            /// <returns>Элемент истории перечисления</returns>
            public T this[[MinValue(0)] int i]
            {
                get
                {
                    Requires(i >= 0);
                    Requires(i < Count);

                    return _Queue[_Queue.Count - i];
                }
            }

            /// <summary>Инициализация нового экземпляра <see cref="EnumerableHystory{T}"/></summary>
            /// <param name="HystoryLength">Длина истории</param>
            public EnumerableHystory([MinValue(0)] int HystoryLength)
            {
                Requires(HystoryLength >= 0);
                Ensures(_Queue != null);
                Ensures(Length == HystoryLength);
                Ensures(Count == 0);

                _HystoryLength = HystoryLength;
                _Queue = new List<T>(HystoryLength);
            }

            /// <summary>Удаление лишних элементов из истории</summary>
            private void Check()
            {
                Ensures(Count <= Length);

                while (_Queue.Count > _HystoryLength) _Queue.RemoveAt(0);
            }

            /// <summary>Добавить элемент в историю перечисления</summary>
            /// <param name="item">Добавляемый элемент</param>
            [NotNull]
            public EnumerableHystory<T> Add(T item)
            {
                Ensures(Count == OldValue(Count) + 1 || Count == Length);
                Ensures(Count <= Length);
                Ensures(_Queue.Contains(item));

                _Queue.Add(item);
                Current = item;
                _ObservableObject.OnNext(item);
                Check();
                return this;
            }

            /// <summary>Получить перечислитель истории элементов</summary><returns>Перечислитель истории элементов</returns>
            [NotNull]
            public IEnumerator<T> GetEnumerator() => _Queue.GetEnumerator();

            /// <summary>Получить перечислитель истории элементов</summary><returns>Перечислитель истории элементов</returns>
            [NotNull]
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /// <summary>Подписка на изменения истории перечисления</summary>
            /// <param name="observer">Объект-подписчик, уведомляемый об изменениях в истории перечисления</param>
            /// <returns>ОБъект, осуществляющий возможность отписаться от уведомлений изменения истории перечисления</returns>
            [NotNull]
            public IDisposable Subscribe(IObserver<T> observer) => _ObservableObject.Subscribe(observer);
        }

        /// <summary>Преобразование исходной последовательности элементов с учётом указанного размера истории перечисления</summary>
        /// <typeparam name="TIn">Тип элементов исходной коллекции</typeparam>
        /// <typeparam name="TOut">Тип элементов результирующей коллекции</typeparam>
        /// <param name="collection">Исходная коллекция элементов</param>
        /// <param name="Selector">Метод преобразования элементов коллекции на основе истории их перечисления</param>
        /// <param name="HystoryLength">Максимальная длина истории перечисления</param>
        /// <returns>Коллекция элементов, сформированная на основе исходной с учётом истории процесса перчисления исходной коллекции</returns>
        [NotNull]
        public static IEnumerable<TOut> SelectWithHystory<TIn, TOut>
        (
            [NotNull] this IEnumerable<TIn> collection,
            [NotNull] Func<EnumerableHystory<TIn>, TOut> Selector,
            [MinValue(0)] int HystoryLength
        )
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");
            Requires(Selector != null);
            Requires(HystoryLength >= 0);

            var hystory = new EnumerableHystory<TIn>(HystoryLength);
            return collection.Select(item => Selector(hystory.Add(item)));
        }

        /// <summary>Оценка статистических параметров перечисления</summary>
        /// <param name="collection">Перечисление значений, статистику которых требуется получить</param>
        /// <param name="Length">Размер выборки для оценки</param>
        /// <returns>Оценка статистики</returns>
        [NotNull]
        public static StatisticValue GetStatistic([NotNull] this IEnumerable<double> collection, [MinValue(0)] int Length = 0)
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");
            Requires(Length >= 0);
            Ensures(Result<StatisticValue>() != null);

            if (Length > 0)
                return new StatisticValue(Length).InitializeObject(sv => sv.AddEnumerable(collection)) ?? throw new InvalidOperationException();
            var values = collection.ToArray();
            var result = new StatisticValue(values.Length);
            result.AddEnumerable(values);
            return result;
        }

        /// <summary>Отбросить нуллевые значения с конца перечисления</summary>
        /// <param name="collection">Фильтруемое перечисление</param>
        /// <returns>Перечисление чисел, в котором отсутствуют хвостовые нули</returns>
        [NotNull]
        public static IEnumerable<double> FilterNullValuesFromEnd([NotNull] this IEnumerable<double> collection)
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");
            Ensures(Result<IEnumerable<double>>() != null);

            var n = 0;
            foreach (var value in collection)
            {
                if (value.Equals(0)) n++;
                else
                {
                    for (; n > 0; n--) yield return 0;
                    yield return value;
                }
            }
        }

        /// <summary>Определить минимальное и максимальное значение перечисления</summary>
        /// <typeparam name="T">Тип элементов перечисления</typeparam>
        /// <param name="collection">Перечисление, минимум и максимум которого необходимо определить</param>
        /// <param name="selector">Метод преобразования объектов в вещественные числа</param>
        /// <param name="Min">Минимальный элемент перечисления</param>
        /// <param name="Max">Максимальный элемент перечисления</param>
        public static void GetMinMax<T>
        (
            [NotNull] this IEnumerable<T> collection,
            [NotNull] Func<T, double> selector,
            out T Min,
            out T Max
        )
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");
            Requires(selector != null);

            var min = new MinValue();
            var max = new MaxValue();
            Min = default;
            Max = default;
            foreach(var value in collection)
            {
                var f = selector(value);
                if (min.AddValue(f)) Min = value;
                if (max.AddValue(f)) Max = value;
            }
        }

        /// <summary>Определить минимальное и максимальное значение перечисления</summary>
        /// <typeparam name="T">Тип элементов перечисления</typeparam>
        /// <param name="collection">Перечисление, минимум и максимум которого необходимо определить</param>
        /// <param name="selector">Метод преобразования объектов в вещественные числа</param>
        /// <param name="Min">Минимальный элемент перечисления</param>
        /// <param name="MinIndex">Индекс минимального элемента в коллекции</param>
        /// <param name="Max">Максимальный элемент перечисления</param>
        /// <param name="MaxIndex">Индекс максимального элемента в коллекции</param>
        public static void GetMinMax<T>
        (
            [NotNull] this IEnumerable<T> collection,
            [NotNull] Func<T, double> selector,
            out T Min,
            out int MinIndex,
            out T Max,
            out int MaxIndex
        )
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");
            Requires(selector != null);

            var min = new MinValue();
            var max = new MaxValue();
            Min = default;
            Max = default;
            MinIndex = MaxIndex = -1;
            foreach (var value in collection.Select((v, i) => new { v, i }))
            {
                var f = selector(value.v);
                if (min.AddValue(f))
                {
                    Min = value.v;
                    MinIndex = value.i;
                }
                if (max.AddValue(f))
                {
                    Max = value.v;
                    MaxIndex = value.i;
                }
            }
        }

        /// <summary>Определение максимального элемента последовательности</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="collection">Последовательность элементов</param>
        /// <param name="selector">Метод преобразования элементов в вещественные числа</param>
        /// <returns>Максимальный элемент последовательности</returns>
        public static T GetMax<T>([NotNull] this IEnumerable<T> collection, [NotNull] Func<T, double> selector)
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");
            Requires(selector != null);

            var max = new MaxValue();
            var result = default(T);
            collection.Where(t => max.AddValue(selector(t))).Foreach(v => result = v);
            return result;
        }

        /// <summary>Определение максимального элемента последовательности</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="collection">Последовательность элементов</param>
        /// <param name="selector">Метод преобразования элементов в вещественные числа</param>
        /// <param name="index">Индекс максимального элемента в последовательности</param>
        /// <returns>Максимальный элемент последовательности</returns>
        public static T GetMax<T>([NotNull] this IEnumerable<T> collection, [NotNull] Func<T, double> selector, out int index)
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");
            Requires(selector != null);

            var max = new MaxValue();
            var result = default(T);
            var I = -1;
            collection.Select((t, i) => new { t, i })
                .Where(v => max.AddValue(selector(v.t)))
                .Foreach(v => { result = v.t; I = v.i; });
            index = I;
            return result;
        }

        /// <summary>Определение минимального элемента последовательности</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="collection">Последовательность элементов</param>
        /// <param name="selector">Метод преобразования элементов в вещественные числа</param>
        /// <returns>Минимальный элемент последовательности</returns>
        public static T GetMin<T>([NotNull] this IEnumerable<T> collection, [NotNull] Func<T, double> selector)
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");
            Requires(selector != null);

            var min = new MinValue();
            var result = default(T);
            collection.Where(t => min.AddValue(selector(t))).Foreach(v => result = v);
            return result;
        }

        /// <summary>Определение минимального элемента последовательности</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="collection">Последовательность элементов</param>
        /// <param name="selector">Метод преобразования элементов в вещественные числа</param>
        /// <param name="index">Индекс минимального элемента в последовательности</param>
        /// <returns>Минимальный элемент последовательности</returns>
        public static T GetMin<T>([NotNull] this IEnumerable<T> collection, [NotNull] Func<T, double> selector, out int index)
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");
            Requires(selector != null);

            var min = new MinValue();
            var result = default(T);
            var I = -1;
            collection.Select((t, i) => new { t, i })
                .Where(v => min.AddValue(selector(v.t)))
                .Foreach(v => { result = v.t; I = v.i; });
            index = I;
            return result;
        }


        /// <summary>Преобразвоание последовательности элементов в строку с указанной строкой-разделителем</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="collection">Последовательность элементов, преобразуемая в строку</param>
        /// <param name="Separator">Строка-разделитель</param>
        /// <returns>Строка, составленная из строковых эквивалентов элементов входного перечисления, разделённых строкой-разделителем</returns>
        public static string ToSeparatedString<T>([NotNull] IEnumerable<T> collection, string Separator)
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");

            return collection
                .Select((t, i) => new { v = t.ToString(), s = i == 0 ? "" : Separator })
                .Aggregate(new StringBuilder(), (sb, v) => sb.AppendFormat("{0}{1}", v.s, v.v), sb => sb.ToString());
        }

        /// <summary>Быстрое преобразование последовательности в список</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="Enum">Последовательность, преобразуемая в список</param>
        /// <returns>Список элементов последовательности</returns>
        [NotNull]
        public static IList<T> ToListFast<T>([NotNull] this IEnumerable<T> Enum) => Enum is IList<T> ? (IList<T>)Enum : Enum.ToList();

        /// <summary>Сумма последовательности комплексных чисел</summary>
        /// <param name="collection">Последовательность комплексных чисел</param>
        /// <returns>Комплексное число, являющееся суммой последовательности комплексных чисел</returns>
        [Pure, DebuggerStepThrough]
        public static Complex Sum([NotNull] this IEnumerable<Complex> collection) => collection.Aggregate((Z, z) => Z + z);

        [Pure, DebuggerStepThrough]
        public static Complex Sum<T>([NotNull] this IEnumerable<T> collection, Func<T, Complex> selector) => collection.Select(selector).Aggregate((Z, z) => Z + z);

        /// <summary>Объединить элементы коллеции</summary>
        /// <typeparam name="T">Тип элемента коллекции</typeparam>
        /// <typeparam name="TResult">Тип результата</typeparam>
        /// <param name="collection">Исходная коллекция элементов</param>
        /// <param name="Init">Исходное состояние результата объединения</param>
        /// <param name="func">Метод объединения</param>
        /// <param name="index">Индекс элемента коллекции</param>
        /// <returns>Результат объединения коллекции элементов</returns>
        [Pure, DebuggerStepThrough]
        public static TResult Aggregate<T, TResult>
        (
            [NotNull] this IEnumerable<T> collection,
            TResult Init,
            [NotNull] Func<TResult, T, int, TResult> func,
            int index = 0)
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");

            return collection.Aggregate(Init, (last, e) => func(last, e, index++));
        }

        /// <summary>Проверка на наличие элемиента в коллекции</summary>
        /// <typeparam name="T">Тип элемента</typeparam>
        /// <param name="collection">Проверяемая коллекция</param>
        /// <param name="selector">Метод выбора</param>
        /// <returns>Истина, если выполняется предикат хотя бы на одном элементе коллекции</returns>
        [Pure, DebuggerStepThrough]
        public static bool Contains<T>([NotNull] this IEnumerable<T> collection, [NotNull] Predicate<T> selector)
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");
            Requires(selector != null, "Отсутствует ссылка на предикат");

            if (collection is List<T> list1)
                for (var i = 0; i < list1.Count; i++)
                    if (selector(list1[i])) return true;

            if (collection is IList<T> i_list)
                for (var i = 0; i < i_list.Count; i++)
                    if (selector(i_list[i])) return true;

            if (collection is T[] array)
                for (var i = 0; i < array.Length; i++)
                    if (selector(array[i])) return true;

            return collection.Any(t => selector(t));
        }

        /// <summary>Найти элемент в перечислении, удовлетворяющий предикату</summary>
        /// <param name="collection">Перечисление элементов</param>
        /// <param name="selector">Предикат выбора</param>
        /// <typeparam name="T">Тип элементов перечисления</typeparam>
        /// <returns>Найденный элемент, либо пустая ссылка</returns>
        [Pure, DebuggerStepThrough]
        public static T Find<T>([NotNull] this IEnumerable<T> collection, [NotNull] Predicate<T> selector)
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");
            Requires(selector != null, "Отсутствует ссылка на предикат");

            foreach(var local in collection.Where(local => selector(local))) return local;
            return default;
        }

        ///<summary>Выполнение действия для всех элементов коллекции</summary>
        ///<param name="collection">Коллекция элементов</param>
        ///<param name="Action">Выполняемое действие</param>
        ///<typeparam name="T">Тип элементов коллекции</typeparam>
        [DebuggerStepThrough]
        public static void Foreach<T>([NotNull] this IEnumerable<T> collection, [NotNull] Action<T> Action)
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");
            Requires(Action != null);

            switch (collection)
            {
                case T[] array: for(int i = 0, count = array.Length; i < count; i++) Action(array[i]); break;
                case IList<T> listT: for(int i = 0, count = listT.Count; i < count; i++) Action(listT[i]); break;
                case IList list: for(int i = 0, count = list.Count; i < count; i++) Action((T)list[i]); break;
                default: foreach(var item in collection) Action(item); break;
            }
        }

        ///<summary>Выполнение действия для всех элементов коллекции</summary>
        ///<param name="collection">Коллекция элементов</param>
        ///<param name="Action">Выполняемое действие</param>
        ///<param name="parameter">Параметр действия</param>
        ///<typeparam name="T">Тип элементов коллекции</typeparam>
        ///<typeparam name="TParameter">Тип параметра процесса перебора</typeparam>
        [DebuggerStepThrough]
        public static void Foreach<T, TParameter>([NotNull] this IEnumerable<T> collection, [NotNull] Action<T, TParameter> Action, [CanBeNull] TParameter parameter)
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");
            Requires(Action != null);

            switch (collection)
            {
                case T[] array: for(int i = 0, count = array.Length; i < count; i++) Action(array[i], parameter); break;
                case IList<T> list: for(int i = 0, count = list.Count; i < count; i++) Action(list[i], parameter); break;
                case IList list: for(int i = 0, count = list.Count; i < count; i++) Action((T)list[i], parameter); break;
                default: foreach(var item in collection) Action(item, parameter); break;
            }
        }

        ///<summary>Выполнение действия для всех элементов коллекции с указанием индекса элемента</summary>
        ///<param name="collection">Коллекция элементов</param>
        ///<param name="Action">Действие над элементом</param>
        ///<param name="index">Смещение индекса элемента колеекции</param>
        ///<typeparam name="T">Тип элемента колекции</typeparam>
        [DebuggerStepThrough]
        public static void Foreach<T>([NotNull] this IEnumerable<T> collection, [NotNull] Action<T, int> Action, int index = 0)
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");
            Requires(Action != null);

            switch (collection)
            {
                case T[] array: for(int i = 0, count = array.Length; i < count; i++) Action(array[i], index++); break;
                case IList<T> list: for(int i = 0, count = list.Count; i < count; i++) Action(list[i], index++); break;
                case IList list: for(int i = 0, count = list.Count; i < count; i++) Action((T)list[i], index++); break;
                default: foreach(var item in collection) Action(item, index++); break;
            }
        }

        /// <summary>Выполнение действия для всех элементов коллекции с указанием индекса элемента</summary>
        /// <param name="collection">Коллекция элементов</param>
        /// <param name="parameter">Параметр действия</param>
        /// <param name="Action">Действие над элементом</param>
        /// <param name="index">Смещение индекса элемента колеекции</param>
        /// <typeparam name="T">Тип элемента колекции</typeparam>
        /// <typeparam name="TParameter">Тип параметра процесса перебора</typeparam>
        [DebuggerStepThrough]
        public static void Foreach<T, TParameter>(
            [NotNull] this IEnumerable<T> collection,
            [CanBeNull] TParameter parameter, 
            [NotNull] Action<T, int, TParameter> Action,
            int index = 0)
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");
            Requires(Action != null);

            switch (collection)
            {
                case T[] array: for(int i = 0, count = array.Length; i < count; i++) Action(array[i], index++, parameter); break;
                case IList<T> list: for(int i = 0, count = list.Count; i < count; i++) Action(list[i], index++, parameter); break;
                case IList list: for(int i = 0, count = list.Count; i < count; i++) Action((T)list[i], index++, parameter); break;
                default: foreach(var item in collection) Action(item, index++, parameter); break;
            }
        }

        /// <summary>Выполнение действия для всех элементов коллекции с указанием индекса элемента</summary>
        /// <param name="collection">Коллекция элементов</param>
        /// <param name="parameter1">Параметр действия 1</param>
        /// <param name="parameter2">Параметр действия 2</param>
        /// <param name="Action">Действие над элементом</param>
        /// <param name="index">Смещение индекса элемента колеекции</param>
        /// <typeparam name="T">Тип элемента колекции</typeparam>
        /// <typeparam name="TParameter1">Тип параметра процесса перебора 1</typeparam>
        /// <typeparam name="TParameter2">Тип параметра процесса перебора 2</typeparam>
        [DebuggerStepThrough]
        public static void Foreach<T, TParameter1, TParameter2>(
            [NotNull] this IEnumerable<T> collection,
            [CanBeNull] TParameter1 parameter1,
            [CanBeNull] TParameter2 parameter2,
            [NotNull] Action<T, int, TParameter1, TParameter2> Action,
            int index = 0)
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");
            Requires(Action != null);

            switch (collection)
            {
                case T[] array: for (int i = 0, count = array.Length; i < count; i++) Action(array[i], index++, parameter1, parameter2); break;
                case IList<T> list: for (int i = 0, count = list.Count; i < count; i++) Action(list[i], index++, parameter1, parameter2); break;
                case IList list: for (int i = 0, count = list.Count; i < count; i++) Action((T)list[i], index++, parameter1, parameter2); break;
                default: foreach (var item in collection) Action(item, index++, parameter1, parameter2); break;
            }
        }

        /// <summary>Ленивое преобразование типов, пропускающее непреобразуемые объекты</summary>
        /// <param name="collection">Исходное перечисление объектов</param>
        /// <typeparam name="T">Тип объектов входного перечисления</typeparam>
        /// <returns>Коллекция объектов преобразованного типа</returns>
        [Pure, DebuggerStepThrough, NotNull]
        public static IEnumerable<T> CastLazy<T>([NotNull] IEnumerable collection)
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");
            Ensures(!ReferenceEquals(Result<IEnumerable<T>>(), null));
            var result = collection as IEnumerable<T>;
            return result ?? collection.Cast<object>().Where(item => item is T).Cast<T>();
        }

        /// <summary>Ленивое преобразование типов элементов перечисления</summary>
        /// <typeparam name="TItem">Тип элементов входной перечисления</typeparam>
        /// <typeparam name="TValue">Тип элементов перечисления, в который требуется осуществить преобразвоание</typeparam>
        /// <param name="collection">Исходная перечисление элементов</param>
        /// <returns>Перечисление элементов преобразованного типа</returns>
        [NotNull]
        public static IEnumerable<TValue> CastLazy<TItem, TValue>([NotNull] IEnumerable<TItem> collection)
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");
            return typeof(TItem) == typeof(TValue) ? (IEnumerable<TValue>)collection : collection.OfType<TValue>();
        }

        /// <summary>Действие, выполняемое в процессе перебора элементов для всех элементов перечисления при условии выполнения предиката</summary>
        /// <typeparam name="T">Ип элементов перечисления</typeparam>
        /// <param name="collection">ПЕречисление элементов, для которых надо выполнить действие</param>
        /// <param name="Predicat">Условие выполнения действия</param>
        /// <param name="Action">Действие, выполняемое для всех элементов перечисления</param>
        /// <returns>Исходное перечисление</returns>
        [NotNull]
        public static IEnumerable<T> ForeachLazyIf<T>
        (
            [NotNull] this IEnumerable<T> collection,
            [NotNull] Func<T, bool> Predicat,
            [CanBeNull] Action<T> Action
        )
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");
            Requires(Predicat != null);
            return Action == null ? collection : collection.Select(t =>
            {
                if(Predicat(t)) Action(t);
                return t;
            });
        }

        /// <summary>Отложенное выполнение указанного действия для каждого элемента последовательности</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="collection">Последовательность элементов</param>
        /// <param name="Action">Выполняемое действие</param>
        /// <returns>Последовательность элементов, для элементов которой выполняется отложенное действие</returns>
        [Pure, DebuggerStepThrough, NotNull]
        public static IEnumerable<T> ForeachLazy<T>([NotNull] this IEnumerable<T> collection, [NotNull] Action<T> Action)
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");
            Ensures(ReferenceEquals(Action, null) // Если действие не указано
                                                  // то проверить, что входная коллекция соответствует выходной
                ? ReferenceEquals(Result<IEnumerable<T>>(), collection)
                // иначе - убедиться, что на выходе не нулевая ссылка
                : !ReferenceEquals(Result<IEnumerable<T>>(), null));

            return Action == null ? collection : collection.Select(t =>
            {
                Action(t);
                return t;
            });
        }

        /// <summary>Выполнение указанного действия на каждом шаге перебора последовательности после выдачи элемента</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="collection">Последовательность элементов</param>
        /// <param name="action">Действие, Выполняемое после выдачи элемента последовательности</param>
        /// <returns>Исходная последовательность элементов</returns>
        [NotNull]
        public static IEnumerable<T> ForeachLazyLast<T>([NotNull] this IEnumerable<T> collection, [NotNull] Action<T> action)
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");
            if (action == null)
                foreach (var value in collection) yield return value;
            else
                foreach (var value in collection)
                {
                    yield return value;
                    action(value);
                }
        }

        /// <summary>Отложенное выполнение действия до перебора элементов последовательности</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="collection">Последовательность элементов</param>
        /// <param name="Action">Выполняемое действие</param>
        /// <param name="index">Начальный индекс элемента последовательности</param>
        /// <returns>Последовательность элементов, для элементов которой которой выполняется действие</returns>
        [Pure, DebuggerStepThrough, NotNull]
        public static IEnumerable<T> ForeachLazy<T>
        (
            [NotNull] this IEnumerable<T> collection,
            [NotNull] Action<T, int> Action,
            int index = 0
        )
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");
            Ensures(ReferenceEquals(Action, null)
                ? ReferenceEquals(Result<IEnumerable<T>>(), collection)
                : !ReferenceEquals(Result<IEnumerable<T>>(), null));
            return Action == null ? collection : collection.Select(t =>
            {
                Action(t, index++);
                return t;
            });
        }

        ///<summary>Класс вычисления значений функции для коллекции аргументов</summary>
        ///<typeparam name="T">Тип аргумента функции</typeparam>
        ///<typeparam name="TResult">Тип значения функции</typeparam>
        private class EnumirableCollectionFunctionCalculator<T, TResult> : IEnumerable<TResult>
        {
            /* ------------------------------------------------------------------------------------------ */

            ///<summary>Перечислитель коллеции рассчитанных значений функции</summary>
            public class EnumerableCollectionFunctionCalculatorEnumirator : IEnumerator<TResult>
            {
                /* ------------------------------------------------------------------------------------------ */

                /// <summary>Перечислитель коллекции аргументов функции</summary>
                [NotNull]
                private readonly IEnumerator<T> _Enumerator;

                /// <summary>Вычисляемая функция</summary>
                [NotNull]
                private readonly Func<T, TResult> _Function;

                [NotNull]
                private readonly LazyValue<TResult> _FunctionValue;

                /* ------------------------------------------------------------------------------------------ */

                /// <summary>Текцущий элемент коллекции значений функции</summary>
                public TResult Current => _FunctionValue;

                /// <summary>Текцущий элемент коллекции значений функции</summary>
                object IEnumerator.Current => Current;

                /* ------------------------------------------------------------------------------------------ */

                ///<summary>Инициализация нового перечислителя колеекции рассчитанных значений функции</summary>
                ///<param name="Enumerator">Перечислитель коллекции аргументов области определения</param>
                ///<param name="f">Вычисляемая функция</param>
                public EnumerableCollectionFunctionCalculatorEnumirator([NotNull] IEnumerator<T> Enumerator, [NotNull] Func<T, TResult> f)
                {
                    Requires(Enumerator != null);
                    Requires(f != null);

                    _Enumerator = Enumerator;
                    _Function = f;
                    _FunctionValue = new LazyValue<TResult>(() => _Function(_Enumerator.Current));
                }

                /* ------------------------------------------------------------------------------------------ */

                /// <summary>Освобождение ресурсов</summary>
                public void Dispose() => _Enumerator.Dispose();

                /// <summary>Переход к следующему элементу коллекции</summary>
                /// <returns>Истина, если переход выполнен успешно</returns>
                public bool MoveNext()
                {
                    var can_move = _Enumerator.MoveNext();
                    if(can_move) _FunctionValue.Reset();
                    return can_move;
                }

                /// <summary>Сброс состояния перечислителя</summary>
                public void Reset()
                {
                    _Enumerator.Reset();
                    _FunctionValue.Reset();
                }

                /* ------------------------------------------------------------------------------------------ */

                /// <summary>Инвариант класса</summary>
                [ContractInvariantMethod]
                private void Invariant()
                {
                    Contract.Invariant(_Enumerator != null);
                    Contract.Invariant(_Function != null);
                    Contract.Invariant(_FunctionValue != null);
                }

                /* ------------------------------------------------------------------------------------------ */
            }

            /* ------------------------------------------------------------------------------------------ */

            /// <summary>Коллекция аргументов функции</summary>
            [NotNull]
            private readonly IEnumerable<T> _Collection;

            /// <summary>Вычисляемая функция</summary>
            [NotNull]
            private readonly Func<T, TResult> _Function;

            /// <summary>Инициализация нового потокового вычислителя функции на коллекции аргументов</summary>
            /// <param name="сollection">Коллекция аргументов</param>
            /// <param name="f">Вычисляемая функция</param>
            public EnumirableCollectionFunctionCalculator([NotNull] IEnumerable<T> сollection, [NotNull] Func<T, TResult> f)
            {
                Requires(сollection != null, "Отсутствует ссылка на перечисление");
                Requires(f != null);

                _Collection = сollection;
                _Function = f;
            }

            /// <summary>Получение перечислителя</summary>
            /// <returns>Перечислитель рассчитанных значений функции</returns>
            [NotNull]
            public IEnumerator<TResult> GetEnumerator()
            {
                Ensures(Result<IEnumerator<TResult>>() != null);

                return new EnumerableCollectionFunctionCalculatorEnumirator(_Collection.GetEnumerator(), _Function);
            }

            /// <summary>Неявное получение перечислителя</summary>
            /// <returns>Перечислитель расссчитанных значений функции</returns>
            [NotNull]
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /// <summary>Инвариант класса</summary>
            [ContractInvariantMethod]
            private void Invariant()
            {
                Contract.Invariant(_Collection != null);
                Contract.Invariant(_Function != null);
            }
        }

        /// <summary>Пересечение последовательностей</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="Source">Исходная последовательность элементов</param>
        /// <param name="Items">Последовательность элементов, пересечение с которой вычисляется</param>
        /// <returns>Последовательность элементов, входящих как в первую, так и во вторую последовательности</returns>
        public static IEnumerable<T> ExistingItems<T>([NotNull] this IEnumerable<T> Source, [NotNull] IEnumerable<T> Items)
        {
            Requires(Source != null);
            Requires(Items != null);

            var list = Items.ToListFast();
            return Source.Where(t => list.Contains(i => Equals(i, t)));
        }

        /// <summary>Последовательность уникальных элементов</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="values">Исходная последовательность элементов</param>
        /// <param name="Comparer">Метод сравнения элементов</param>
        /// <returns>Последовательность элементов, таких, что ранее они отсутствовали во входной последовательности</returns>
        [NotNull]
        public static IEnumerable<T> GetUnique<T>([NotNull] this IEnumerable<T> values, [NotNull] Func<T, T, bool> Comparer)
        {
            Requires(values != null);
            Requires(Comparer != null);

            var list = new List<T>();
            foreach (var value in values.Where(value => !list.Exists(v => Comparer(value, v))))
            {
                list.Add(value);
                yield return value;
            }
        }

        /// <summary>Последовательность уникальных элементов</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="values">Исходная последовательность элементов</param>
        /// <returns>Последовательность элементов, таких, что ранее они отсутствовали во входной последовательности</returns>
        [NotNull]
        public static IEnumerable<T> GetUnique<T>([NotNull] this IEnumerable<T> values)
        {
            Requires(values != null);

            var list = new List<T>();
            return values.Where(v => !list.Contains(v)).ForeachLazy(list.Add);
        }

        /// <summary>Найти элементы, которые не входят во вторую последовательность</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="Source">Исходная последовательность</param>
        /// <param name="Items">Последовательность элементов, которых не должно быть в выходной последовательности</param>
        /// <returns>Последовательность элементов, которые отсутствуют во второй последовательности</returns>
        [NotNull]
        public static IEnumerable<T> MisingItems<T>([NotNull] this IEnumerable<T> Source, [NotNull] IEnumerable<T> Items)
        {
            Requires(Source != null);
            Requires(Items != null);

            var list = Items.ToListFast();
            return Source.Where(t => !list.Contains(i => Equals(i, t)));
        }

        /// <summary>Пересечение последовательностей</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="A">Первая последовательность</param>
        /// <param name="B">Вторая последовательность</param>
        /// <returns>Массив элементов, входящих и в первую и во вторую последовательности</returns>
        [NotNull]
        public static T[] Intersection<T>([NotNull] this IEnumerable<T> A, [NotNull] IEnumerable<T> B)
        {
            Requires(A != null);
            Requires(B != null);

            var a = A.ToListFast();
            var b = B.ToListFast();

            var result = new List<T>(a.Count + b.Count);
            result.AddRange(a.ExistingItems(b));
            result.AddRange(b.ExistingItems(a));

            return result.ToArray();
        }

        /// <summary>Последовательности элементов поэлементно равны</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="A">Первая последовательность</param>
        /// <param name="B">Вторая последовательность</param>
        /// <returns>Истина, если последовательности равны с точностью до элементов</returns>
        public static bool ItemEquals<T>([NotNull] this IEnumerable<T> A, [NotNull] IEnumerable<T> B)
        {
            Requires(A != null);
            Requires(B != null);

            using(var a = A.GetEnumerator())
            using(var b = B.GetEnumerator())
            {
                var next_a = a.MoveNext();
                var next_b = b.MoveNext();
                while(next_a && next_b)
                {
                    if(ReferenceEquals(a.Current, null) && !ReferenceEquals(b.Current, null)) return false;
                    if(a.Current == null || !a.Current.Equals(b.Current)) return false;
                    next_a = a.MoveNext();
                    next_b = b.MoveNext();
                }
                return next_a == next_b;
            }
        }

        /// <summary>ОПределение объектов, которые не входят в пересечение двух последовательностей</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="A">Исходная последовательность</param>
        /// <param name="B">Вторичная последовательность</param>
        /// <returns>Массив элементов, входящих либо в первую, либо во вторую последовательность</returns>
        [NotNull]
        public static T[] NotIntersection<T>([NotNull] this IEnumerable<T> A, [NotNull] IEnumerable<T> B)
        {
            Requires(A != null);
            Requires(B != null);

            var a = A.ToListFast();
            var b = B.ToListFast();

            var result = new List<T>(a.Count + b.Count);
            result.AddRange(a.MisingItems(b));
            result.AddRange(b.MisingItems(a));

            return result.ToArray();
        }

        /// <summary>Нахождение пересечения элементов двух последовательностей</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="A">Исходная последовательность</param>
        /// <param name="B">Вторичная последовательность</param>
        /// <param name="MissingInAFromB">Массив элементов, отсутствующих в первой последовательности</param>
        /// <param name="MissingInBFromA">Массив элементов, отсутствующих во второй последовательности</param>
        /// <param name="ExistingInAFromB">Массив элементов, присутствующих в первой последовательности</param>
        /// <param name="ExistingInBFromA">Массив элементов, присутствующих во второй последовательности</param>
        /// <param name="Intersection">Пересечение элементов</param>
        /// <param name="NotIntersection">Остаток от пересечения элементов</param>
        public static void Xor<T>
        (
            [NotNull] this IEnumerable<T> A,
            [NotNull] IEnumerable<T> B,
            [NotNull] out T[] MissingInAFromB,
            [NotNull] out T[] MissingInBFromA,
            [NotNull] out T[] ExistingInAFromB,
            [NotNull] out T[] ExistingInBFromA,
            [NotNull] out T[] Intersection,
            [NotNull] out T[] NotIntersection
        )
        {
            Requires(A != null);
            Requires(B != null);
            Ensures(ValueAtReturn(out MissingInAFromB) != null);
            Ensures(ValueAtReturn(out MissingInBFromA) != null);
            Ensures(ValueAtReturn(out ExistingInAFromB) != null);
            Ensures(ValueAtReturn(out ExistingInBFromA) != null);
            Ensures(ValueAtReturn(out Intersection) != null);
            Ensures(ValueAtReturn(out NotIntersection) != null);


            var a = A.ToListFast();
            var b = B.ToListFast();

            var MissingInAFromB_list = new List<T>(a.Count + b.Count);
            var MissingInBFromA_list = new List<T>(a.Count + b.Count);
            var ExistingInAFromB_list = new List<T>(a.Count + b.Count);
            var ExistingInBFromA_list = new List<T>(a.Count + b.Count);
            var Intersection_list = new List<T>(a.Count + b.Count);
            var NotIntersection_list = new List<T>(a.Count + b.Count);

            var b_existing_in_a = new bool[b.Count];
            for (var i = 0; i < a.Count; i++)
            {
                var a_item = a[i];
                var a_existing_in_b = false;

                for (var j = 0; j < b.Count; j++)
                {
                    var b_item = b[j];
                    if (!a_item.Equals(b_item)) continue;

                    a_existing_in_b = b_existing_in_a[j] = true;
                    break;
                }

                if (a_existing_in_b)
                {
                    ExistingInAFromB_list.Add(a_item);
                    NotIntersection_list.Add(a_item);
                }
                else
                {
                    MissingInAFromB_list.Add(a_item);
                    Intersection_list.Add(a_item);
                }
            }

            for (var i = 0; i < b.Count; i++)
            {
                var b_item = b[i];
                if (b_existing_in_a[i])
                {
                    ExistingInBFromA_list.Add(b_item);
                    NotIntersection_list.Add(b_item);
                }
                else
                {
                    MissingInBFromA_list.Add(b_item);
                    Intersection_list.Add(b_item);
                }
            }

            ExistingInAFromB = ExistingInAFromB_list.ToArray();
            ExistingInBFromA = ExistingInBFromA_list.ToArray();
            MissingInAFromB = MissingInAFromB_list.ToArray();
            MissingInBFromA = MissingInBFromA_list.ToArray();
            Intersection = Intersection_list.ToArray();
            NotIntersection = NotIntersection_list.ToArray();
        }

        /// <summary>Преобразовать последовательность в строку с указанной строкой-разделителем</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="collection">Последовательность объектов, преобразуемая в строку с разделителями</param>
        /// <param name="Separator">Разделитель элементов в строке</param>
        /// <returns>Строка, составленная из строковых представлений объектов последовательности, разделённых указанной строкой-разделителем</returns>
        [NotNull]
        public static string ToSeparatedStr<T>([NotNull] this IEnumerable<T> collection, [CanBeNull] string Separator = "")
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");

            return string.Join(Separator, collection.Select(o => o.ToString()).ToArray());
        }

        /// <summary>Найти минимум и максимум последовательности вещественых чисел</summary>
        /// <param name="values">Последовательность вещественных чисел</param>
        /// <returns>Интервал, границы которого определяют минимум и максимум значений, которые принимала входная последовательность</returns>
        [NotNull]
        public static Interval GetMinMax([NotNull] this IEnumerable<double> values)
        {
            Requires(values != null);
            Ensures(Result<Interval>() != null);

            return new MinMaxValue(values).Interval;
        }

        /// <summary>Добавить элемент в конец последовательности</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="collection">Исходная последовательность элементов</param>
        /// <param name="obj">Добавляемый объект</param>
        /// <returns>Последовательность, составленная из элементов исходной последовательности и добавленного элемента</returns>
        [NotNull]
        public static IEnumerable<T> AppendLast<T>([CanBeNull]this IEnumerable<T> collection, [CanBeNull] T obj)
        {
            Ensures(Result<IEnumerable<T>>() != null);

            if (collection != null && !(collection is T[] && ((T[])collection).Length == 0))
                foreach (var value in collection)
                    yield return value;
            if (!Equals(obj, null))
                yield return obj;
        }

        /// <summary>Добавить последовательность элементов в конец последовательности</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="first_collection">Начальная последовательность элементов</param>
        /// <param name="last_collection">Завершающая последовательность элементов</param>
        /// <returns>Последовательность элементов, составленная из элементов первичной и вторичной последовательностей</returns>
        [NotNull]
        public static IEnumerable<T> AppendLast<T>
        (
            [CanBeNull]this IEnumerable<T> first_collection,
            [CanBeNull] IEnumerable<T> last_collection
        )
        {
            Ensures(Result<IEnumerable<T>>() != null);

            if(first_collection != null && !(first_collection is T[] && ((T[])first_collection).Length == 0))
                foreach(var value in first_collection)
                    yield return value;
            if(last_collection != null && !(last_collection is T[] && ((T[])last_collection).Length == 0))
                foreach(var value in last_collection)
                    yield return value;
        }

        /// <summary>Добавить объект в начало перечисления</summary>
        /// <typeparam name="T">Тип объектов перечисления</typeparam>
        /// <param name="collection">Основное перечисление</param>
        /// <param name="obj">Объект, добавляемый в начало перечисления</param>
        /// <returns>Перечисление объектов, составленное из первого объекта и остального перечисления</returns>
        [NotNull]
        public static IEnumerable<T> AppendFirst<T>([CanBeNull] this IEnumerable<T> collection, [CanBeNull] T obj)
        {
            Ensures(Result<IEnumerable<T>>() != null);

            if(!Equals(obj, null))
                yield return obj;
            if(collection != null && !(collection is T[] items && items.Length == 0))
                foreach(var value in collection)
                    yield return value;
        }

        /// <summary>Добавить перечисление объектов в начало основного перечисления</summary>
        /// <typeparam name="T">Тип объектов перечисления</typeparam>
        /// <param name="last_collection">Первая последовательность объектов</param>
        /// <param name="first_collection">Вторая последовательность объектов</param>
        /// <returns>Последовательность объектов, составленная из первой последовательности, за которой следует вторая последовательность</returns>
        [NotNull]
        public static IEnumerable<T> AppendFirst<T>
        (
            [CanBeNull]this IEnumerable<T> last_collection,
            [CanBeNull] IEnumerable<T> first_collection
        )
        {
            Ensures(Result<IEnumerable<T>>() != null);

            if(first_collection != null && !(first_collection is T[] && ((T[])first_collection).Length == 0))
                foreach(var value in first_collection)
                    yield return value;
            if(last_collection != null && !(last_collection is T[] && ((T[])last_collection).Length == 0))
                foreach(var value in last_collection)
                    yield return value;
        }

        /// <summary>Вставить элемент в указанное положение в последовательности</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="collection">Последовательность элементов, в которую требуется вставить новый элемент</param>
        /// <param name="obj">Элемент, добавляемый в последовательность</param>
        /// <param name="pos">Положение в которое требуется вставить элемент</param>
        /// <returns>Последовательность элементов, в указанной позиции которой будет размещён указанный элемент</returns>
        [NotNull]
        public static IEnumerable<T> InsertAtPos<T>([NotNull] this IEnumerable<T> collection, T obj, int pos)
        {
            Requires(collection != null, "Отсутствует ссылка на перечисление");
            Requires(pos >= 0);
            Ensures(Result<IEnumerable<T>>() != null);

            var i = 0;
            foreach (var value in collection)
            {
                if (i == pos) { yield return obj; i++; }
                yield return value;
                i++;
            }
        }

        /// <summary>Инверсная конкатинация перечислений</summary>
        /// <typeparam name="T">Тип элементов перечислений</typeparam>
        /// <param name="FirstCollection">Исходная последовательность, добавляемая в конец</param>
        /// <param name="SecondCollection">Вторичная последовательность, добавляемая в начало</param>
        /// <returns>Последовательность элементов, составленная из элементов вторичной последовательности и элементов первичной последовательности</returns>
        [NotNull]
        public static IEnumerable<T> ConcatInverted<T>
        (
            [NotNull] this IEnumerable<T> FirstCollection,
            [NotNull] IEnumerable<T> SecondCollection
        )
        {
            Requires(FirstCollection != null, "Отсутствует ссылка на первую входную последовательность");
            Requires(SecondCollection != null, "Отсутствует ссылка на вторую входную последовательность");
            Ensures(Result<IEnumerable<T>>() != null);

            foreach (var value in SecondCollection) yield return value;
            foreach (var value in FirstCollection) yield return value;
        }

        /// <summary>Сумма перечисления полиномов</summary>
        /// <param name="P">Перечисление полиномов, которые надо сложить</param>
        /// <returns>Полином, являющийся суммой полиномов</returns>
        [NotNull]
        public static Polynom Sum([NotNull] this IEnumerable<Polynom> P)
        {
            Requires(P != null, "Отсутствует ссылка на входную последовательность");
            Ensures(Result<Polynom>() != null);

            Polynom result = null;
            foreach (var p in P)
            {
                if (result == null) result = p;
                else
                    result += p;
            }
            return result ?? new Polynom(0);
        }

        /// <summary>Произведение перечисления полиномов</summary>
        /// <param name="P">Перечисление полиномов, которые надо перемножить</param>
        /// <returns>Полином, являющийся произведеним полиномов</returns>
        [NotNull]
        public static Polynom Multyiply([NotNull] this IEnumerable<Polynom> P)
        {
            Requires(P != null, "Отсутствует ссылка на входную последовательность");
            Ensures(Result<Polynom>() != null);

            Polynom result = null;
            foreach (var p in P)
            {
                if (result == null) result = p;
                else
                    result *= p;
            }
            return result ?? new Polynom(1);
        }

        /// <summary>Проредить последовательность</summary>
        /// <typeparam name="T">Тип элементов последовательности</typeparam>
        /// <param name="collection">Прореживаемая последовательность</param>
        /// <param name="N">Размер выборки > 0</param>
        /// <param name="k">Положение в выборке (от 0 до N-1)</param>
        /// <returns>Последовательность из N-ых элементов выборки, стоящих на k-ом месте</returns>
        [NotNull]
        public static IEnumerable<T> Decimate<T>([NotNull] this IEnumerable<T> collection, int N, int k = 0)
        {
            Requires(collection != null, "Отсутствует ссылка на входную последовательность");
            Ensures(Result<IEnumerable<T>>() != null);

            return collection.Where((v, i) => i % N == k);
        }
    }
}
