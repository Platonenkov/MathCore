﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MathCore.Annotations;
using DST = System.Diagnostics.DebuggerStepThroughAttribute;

namespace System
{
    public static class TypeExtentions
    {
        private sealed class PairOfTypes
        {
            private readonly Type _First;
            private readonly Type _Second;
            public PairOfTypes(Type first, Type second)
            {
                _First = first;
                _Second = second;
            }
            public override int GetHashCode() => 31 * _First.GetHashCode() + _Second.GetHashCode();

            public override bool Equals(object obj) =>
                ReferenceEquals(obj, this) ||
                obj is PairOfTypes other && _First == other._First && _Second == other._Second;
        }

        private static readonly IDictionary<PairOfTypes, Func<object, object>> __CastersDictionary = new Dictionary<PairOfTypes, Func<object, object>>();

        private static readonly ParameterExpression __ConvParameter = Expression.Parameter(typeof(object), "value");

        private static Func<object, object> GetCasterFrom([NotNull]Type TargetType, [CanBeNull]object Source) => TargetType.GetCasterFrom(Source?.GetType() ?? typeof(object));

        public static Func<object, object> GetCasterTo([NotNull] this Type SourceType, [NotNull]Type TargetType) => TargetType.GetCasterFrom(SourceType);
        public static Func<object, object> GetCasterFrom([NotNull] this Type TargetType, [NotNull]Type SourceType)
        {
            Func<object, object> res;
            var key = new PairOfTypes(SourceType, TargetType);
            lock (__CastersDictionary) if(!__CastersDictionary.TryGetValue(key, out res))
            {
                var object_2_source = Expression.Convert(__ConvParameter, SourceType);
                var source_2_target = Expression.Convert(object_2_source, TargetType);
                var target_2_object = Expression.Convert(source_2_target, typeof(object));
                res = Expression.Lambda<Func<object, object>>(target_2_object, __ConvParameter).Compile();
                __CastersDictionary.Add(key, res);
            }
            return res;
        }

        public static object Cast(this Type type, object obj) => GetCasterFrom(type, obj)(obj);

        private static readonly IDictionary<PairOfTypes, Func<object, object>> __ConvertersDictionary = new Dictionary<PairOfTypes, Func<object, object>>();

        public static Func<object, object> GetConverterTo([NotNull] this Type SourceType, [NotNull] Type TargetType)
            => TargetType.GetConverterFrom(SourceType);

        public static Func<object, object> GetConverterFrom([NotNull] this Type TargetType, [NotNull] Type SourceType)
        {
            Func<object, object> res;
            var key = new PairOfTypes(SourceType, TargetType);
            lock (__ConvertersDictionary)
                if(!__ConvertersDictionary.TryGetValue(key, out res))
                    __ConvertersDictionary.Add(key, res = SourceType.GetConvertExpression_Object(TargetType).Compile());
            return res;
        }


        public static Expression GetCastExpression(this Type FromType, Type ToType, ref ParameterExpression parameter)
        {
            if(parameter is null) parameter = Expression.Parameter(typeof(object), "value");
            return Expression.Convert(Expression.Convert(Expression.Convert(__ConvParameter, FromType), ToType), typeof(object));
        }

        public static LambdaExpression GetConvertExpression(this Type FromType, Type ToType)
        {
            var c = FromType.GetTypeConverter();
            TypeConverter c_to = null;
            if(!c.CanConvertTo(ToType) && !(c_to = ToType.GetTypeConverter()).CanConvertFrom(FromType))
                throw new NotSupportedException($"Преобразование из {FromType} в {ToType} не поддерживается");
            var expr_pFrom = Expression.Parameter(FromType, "pFrom");
            var expr_tFrom2tObject = Expression.Convert(expr_pFrom, typeof(object));
            var expr_cConverter = Expression.Constant(c_to ?? c);
            var method = (c_to is null
                            ? (Delegate)(Func<object, Type, object>)c.ConvertTo
                            : (Func<object, object>)c_to.ConvertFrom)
                            .Method;
            var exprs_pConverter = c_to is null
                ? new Expression[] { expr_tFrom2tObject, Expression.Constant(ToType) }
                : new Expression[] { expr_tFrom2tObject };
            var expr_conversation = Expression.Call(expr_cConverter, method, exprs_pConverter);

            return Expression.Lambda(Expression.Convert(expr_conversation, ToType), expr_pFrom);
        }

        public static Expression<Func<object, object>> GetConvertExpression_Object(this Type FromType, Type ToType)
        {
            var c = FromType.GetTypeConverter();
            TypeConverter c_to = null;
            if(!c.CanConvertTo(ToType) && !(c_to = ToType.GetTypeConverter()).CanConvertFrom(FromType))
                throw new NotSupportedException($"Преобразование из {FromType} в {ToType} не поддерживается");
            var expr_pFrom = Expression.Parameter(typeof(object), "pFrom");
            var expr_cConverter = Expression.Constant(c_to ?? c);
            var method = (c_to is null
                            ? (Delegate)(Func<object, Type, object>)c.ConvertTo
                            : (Func<object, object>)c_to.ConvertFrom)
                            .Method;
            var exprs_pConverter = c_to is null
                ? new Expression[] { expr_pFrom, Expression.Constant(ToType) }
                : new Expression[] { expr_pFrom };
            var expr_Conversation = Expression.Call(expr_cConverter, method, exprs_pConverter);

            return Expression.Lambda<Func<object, object>>(Expression.Convert(expr_Conversation, typeof(object)), expr_pFrom);
        }

        /// <summary>Получить конвертер значений для указанного типа данных</summary>
        /// <param name="type">Тип, для которого требуется получить конвертер</param>
        /// <returns>Конвертер указанного типа данных</returns>
        public static TypeConverter GetTypeConverter(this Type type) => TypeDescriptor.GetConverter(type);

        /// <summary>Получить тип по его имени из всех загруженных сборок</summary>
        /// <param name="TypeName">Имя типа</param>
        /// <returns>Тип</returns>
        [DST]
        public static Type GetType(string TypeName)
        {
            var type_array = AppDomain.CurrentDomain.GetAssemblies().
                SelectMany((a, i) => a.GetTypes()).Where(t => t.Name == TypeName).ToArray();
            return type_array.Length != 0 ? type_array[0] : null;
        }

        /// <summary>Получить все атрибуты типа указанного типа</summary>
        /// <typeparam name="TAttribute">Тип требуемых атрибутов</typeparam>
        /// <param name="T">Тип, атрибуты которого требуется получить</param>
        /// <returns>Массив атрибутов типа указанного типа</returns>
        [DST]
        public static TAttribute[] GetCustomAttributes<TAttribute>(this Type T)
            where TAttribute : Attribute => GetCustomAttributes<TAttribute>(T, false);

        [DST]
        public static TAttribute[] GetCustomAttributes<TAttribute>(this Type T, bool Inherited)
             where TAttribute : Attribute => T.GetCustomAttributes(typeof(TAttribute), Inherited).OfType<TAttribute>().ToArray();

        [DST]
        public static object CreateObject(this Type type)
        {
            //var lv_Info = type.GetConstructor(new Type[] { });
            //if(lv_Info is null)
            //    throw new InvalidOperationException("Не найден конструктор типа " +
            //        type + " без параметров. Для данного типа доступны следующие конструкторы " +
            //        type.GetConstructors().ConvertObjectTo(CInfo =>
            //        {
            //            if(CInfo.Length == 0) return "{}";
            //            var Result = "{" + CInfo[0].ToString();
            //            for(var i = 1; i < CInfo.Length; i++)
            //                Result += "; " + CInfo[i].ToString();
            //            return Result + "}";
            //        }));
            //return lv_Info.Invoke(new object[] { });
            Contract.Requires(type != null);
            return Activator.CreateInstance(type);
        }

        [DST]
        public static T Create<T>(this Type type) => (T)type.CreateObject();

        [DST]
        public static T Create<T>(this Type type, params object[] Params) => (T)type.CreateObject(Params);

        [DST]
        public static object CreateObject(this Type type, params object[] Params)
        {
            Contract.Requires(type != null);
            return Activator.CreateInstance(type, Params);
        }

        [DST]
        public static object CreateObject(this Type type, BindingFlags Flags, Binder binder, params object[] Params)
        {
            Contract.Requires(type != null);
            return Activator.CreateInstance(type, Flags, binder, Params);
        }

        [DST]
        public static T Create<T>(params object[] Params) => (T)CreateObject(typeof(T), Params);

        [DST]
        public static T Create<T>(BindingFlags Flags, Binder binder, params object[] Params) => (T)CreateObject(typeof(T), Flags, binder, Params);

        public static void AddConvreter(this Type type, Type ConverterType) => TypeDescriptor.AddAttributes(type, new TypeConverterAttribute(ConverterType));

        public static void AddConvreter(this Type type, params Type[] ConverterTypes) => 
            TypeDescriptor.AddAttributes(type, ConverterTypes.Select(t => new TypeConverterAttribute(t)).Cast<Attribute>().ToArray());

        public static TypeDescriptionProvider GetProvider(this Type type) => TypeDescriptor.GetProvider(type);

        public static void AddProvider(this Type type, TypeDescriptionProvider provider) => TypeDescriptor.AddProvider(provider, type);
    }
}
