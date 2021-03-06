﻿//using System;
//using System.Linq;
//using System.Linq.Expressions;

//namespace System.Linq.ToSQL
//{
//    public class PropertyProvider : IQueryProvider
//    {
//        private readonly IQueryProvider _Provider;
//        private PropertyVisitor _PropertyVisitor;

//        public PropertyProvider(IQueryProvider provider)
//        {
//            _Provider = provider;
//        }

//        public IQueryable CreateQuery(Expression expression)
//        {
//            _PropertyVisitor = new PropertyVisitor();
//            expression = _PropertyVisitor.Visit(expression);
//            var queryable = _Provider.CreateQuery(expression);
//            return queryable;
//        }

//        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
//        {
//            return (IQueryable<TElement>)CreateQuery(expression);
//        }

//        public object Execute(Expression expression)
//        {
//            return _Provider.Execute(expression);
//        }

//        public TResult Execute<TResult>(Expression expression)
//        {
//            return (TResult)_Provider.Execute(expression);
//        }
//    }
//}