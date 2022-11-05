﻿using SQLEConnect.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SQLEConnect.Parsers.Base
{
    internal abstract class BaseObjectParser<TModel> : BaseParser
    {
        /// <summary>
        /// Helper method to determinate for which type the parser is responisble.
        /// </summary>
        /// <returns>Type - for which type the parser is responisble.</returns>
        public Type Type() => typeof(TModel);

        protected Func<TModel> BuildNewFunc()
        {
            IEnumerable<ConstructorInfo> constructorInfos = this.Type()
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .OrderBy(ci => ci.GetParameters()?.Length ?? 0);

            NewExpression constructorExpression;

            if (constructorInfos.Any())
            {
                foreach (ConstructorInfo constructorInfo in constructorInfos)
                {
                    if (constructorInfo.GetParameters().Length == 0)
                    {
                        constructorExpression = Expression.New(constructorInfo);

                        return Expression.Lambda<Func<TModel>>(constructorExpression, new ParameterExpression[] { }).Compile();
                    }
                    else
                    {
                        List<Expression> parameterExpressions = new List<Expression>();

                        foreach (ParameterInfo parameter in constructorInfo.GetParameters())
                        {
                            parameterExpressions.Add(Expression.Default(parameter.ParameterType));
                        }

                        constructorExpression = Expression.New(constructorInfo, parameterExpressions);

                        return Expression.Lambda<Func<TModel>>(constructorExpression, new ParameterExpression[] { }).Compile();
                    }

                }
            }

            throw new InvalidOperationException($"To build rules for the type {this.GetType().Name}, it must have an empty constructor or one which accept default values.");
        }

        protected Dictionary<string, Func<TModel, DbDataReader, int, TModel>> BuildObjectPropertiesExpressions()
        {
            Type type = typeof(TModel);
            string xParameter = "x";
            string yParameter = "y";
            string zParameter = "z";

            Dictionary<string, Func<TModel, DbDataReader, int, TModel>> funcsByName = new Dictionary<string, Func<TModel, DbDataReader, int, TModel>>();

            IEnumerable<PropertyInfo> properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(prop => prop.CanWrite);

            ParameterExpression tParameterExpression = Expression.Parameter(type, xParameter);
            ParameterExpression dbDataReaderExpression = Expression.Parameter(typeof(DbDataReader), yParameter);
            ParameterExpression intExpression = Expression.Parameter(BaseTypeHelper.IntegerType, zParameter);

            if (properties != null)
            {
                foreach (PropertyInfo propertyInfo in properties)
                {
                    MemberExpression propertyExpression = Expression.Property(tParameterExpression, propertyInfo);
                    LabelTarget lblTarget = Expression.Label(type, xParameter);
                    LabelExpression lblExpression = Expression.Label(lblTarget, tParameterExpression);
                    GotoExpression gotoExpression = Expression.Goto(lblTarget, tParameterExpression);
                    BinaryExpression assignExpression = null;
                    BlockExpression blockExpression;

                    if (propertyInfo.PropertyType == BaseTypeHelper.StringType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetString), BaseTypeHelper.StringType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.CharType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, Expression.Call(null, typeof(CharsHelperParser).GetMethod(nameof(CharsHelperParser.GetCharFromString), BindingFlags.Static | BindingFlags.NonPublic), dbDataReaderExpression, intExpression));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.NullableCharType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, Expression.Call(null, typeof(CharsHelperParser).GetMethod(nameof(CharsHelperParser.GetNullableCharFromString), BindingFlags.Static | BindingFlags.NonPublic), dbDataReaderExpression, intExpression));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.ByteType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetByte), BaseTypeHelper.ByteType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.NullableByteType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetByte), BaseTypeHelper.NullableByteType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.SByteType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetGenericConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetValue), nameof(Convert.ToSByte), BaseTypeHelper.SByteType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.NullableSByteType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetGenericConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetValue), nameof(Convert.ToSByte), BaseTypeHelper.NullableSByteType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.ShortType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetInt16), BaseTypeHelper.ShortType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.NullableShortType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetInt16), BaseTypeHelper.NullableShortType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.UShortType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetGenericConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetValue), nameof(Convert.ToUInt16), BaseTypeHelper.UShortType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.NullableUShortType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetGenericConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetValue), nameof(Convert.ToUInt16), BaseTypeHelper.NullableUShortType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.IntegerType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetInt32), BaseTypeHelper.IntegerType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.NullableIntegerType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetInt32), BaseTypeHelper.NullableIntegerType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.UIntegerType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetGenericConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetValue), nameof(Convert.ToUInt32), BaseTypeHelper.UIntegerType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.NullableUIntegerType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetGenericConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetValue), nameof(Convert.ToUInt32), BaseTypeHelper.NullableUIntegerType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.LongType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetInt64), BaseTypeHelper.LongType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.NullableLongType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetInt64), BaseTypeHelper.NullableLongType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.ULongType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetGenericConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetValue), nameof(Convert.ToUInt64), BaseTypeHelper.ULongType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.NullableULongType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetGenericConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetValue), nameof(Convert.ToUInt64), BaseTypeHelper.NullableULongType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.FloatType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetFloat), BaseTypeHelper.FloatType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.NullableFloatType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetFloat), BaseTypeHelper.NullableFloatType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.DoubleType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetDouble), BaseTypeHelper.DoubleType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.NullableDoubleType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetDouble), BaseTypeHelper.NullableDoubleType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.DecimalType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetDecimal), BaseTypeHelper.DecimalType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.NullableDecimalType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetDecimal), BaseTypeHelper.NullableDecimalType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.DateTimeType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetDateTime), BaseTypeHelper.DateTimeType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.NullableDateTimeType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetDateTime), BaseTypeHelper.NullableDateTimeType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.BooleanType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetBoolean), BaseTypeHelper.BooleanType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.NullableBooleanType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetBoolean), BaseTypeHelper.NullableBooleanType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.GuidType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetGuid), BaseTypeHelper.GuidType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.NullableGuidType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetGuid), BaseTypeHelper.NullableGuidType));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.StreamType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, Expression.Call(null, typeof(StreamHelperParser).GetMethod(nameof(StreamHelperParser.GetStream), BindingFlags.Static | BindingFlags.NonPublic), dbDataReaderExpression, intExpression));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.ByteArrayType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, Expression.Call(null, typeof(BytesHelperParser).GetMethod(nameof(BytesHelperParser.GetBytes), BindingFlags.Static | BindingFlags.NonPublic), dbDataReaderExpression, intExpression));
                    }
                    else if (propertyInfo.PropertyType == BaseTypeHelper.CharArrayType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, Expression.Call(null, typeof(CharsHelperParser).GetMethod(nameof(CharsHelperParser.GetChars), BindingFlags.Static | BindingFlags.NonPublic), dbDataReaderExpression, intExpression));
                    }
                    else
                    {
                        continue;
                    }

                    blockExpression = Expression.Block(assignExpression, gotoExpression, lblExpression);

                    Func<TModel, DbDataReader, int, TModel> propertySetterFunc = Expression
                        .Lambda<Func<TModel, DbDataReader, int, TModel>>(blockExpression, new ParameterExpression[] {
                            tParameterExpression,
                            dbDataReaderExpression,
                            intExpression })
                        .Compile();

                    funcsByName.Add(propertyInfo.Name.ToLower(), propertySetterFunc);
                }
            }

            return funcsByName;
        }

        protected Dictionary<string, Func<TModel, DbDataReader, int, TModel>> BuildObjectFieldsExpressions()
        {
            Type type = typeof(TModel);
            string xParameter = "x";
            string yParameter = "y";
            string zParameter = "z";

            Dictionary<string, Func<TModel, DbDataReader, int, TModel>> funcsByName = new Dictionary<string, Func<TModel, DbDataReader, int, TModel>>();

            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            ParameterExpression tParameterExpression = Expression.Parameter(type, xParameter);
            ParameterExpression dbDataReaderExpression = Expression.Parameter(typeof(DbDataReader), yParameter);
            ParameterExpression intExpression = Expression.Parameter(BaseTypeHelper.IntegerType, zParameter);

            if (fields != null)
            {
                foreach (FieldInfo fieldInfo in fields)
                {
                    MemberExpression propertyExpression = Expression.Field(tParameterExpression, fieldInfo);
                    LabelTarget lblTarget = Expression.Label(type, xParameter);
                    LabelExpression lblExpression = Expression.Label(lblTarget, tParameterExpression);
                    GotoExpression gotoExpression = Expression.Goto(lblTarget, tParameterExpression);
                    BinaryExpression assignExpression = null;
                    BlockExpression blockExpression;

                    if (fieldInfo.FieldType == BaseTypeHelper.StringType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetString), BaseTypeHelper.StringType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.CharType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, Expression.Call(null, typeof(CharsHelperParser).GetMethod(nameof(CharsHelperParser.GetCharFromString), BindingFlags.Static | BindingFlags.NonPublic), dbDataReaderExpression, intExpression));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.NullableCharType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, Expression.Call(null, typeof(CharsHelperParser).GetMethod(nameof(CharsHelperParser.GetNullableCharFromString), BindingFlags.Static | BindingFlags.NonPublic), dbDataReaderExpression, intExpression));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.ByteType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetByte), BaseTypeHelper.ByteType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.NullableByteType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetByte), BaseTypeHelper.NullableByteType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.SByteType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetGenericConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetValue), nameof(Convert.ToSByte), BaseTypeHelper.SByteType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.NullableSByteType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetGenericConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetValue), nameof(Convert.ToSByte), BaseTypeHelper.NullableSByteType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.ShortType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetInt16), BaseTypeHelper.ShortType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.NullableShortType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetInt16), BaseTypeHelper.NullableShortType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.UShortType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetGenericConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetValue), nameof(Convert.ToUInt16), BaseTypeHelper.UShortType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.NullableUShortType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetGenericConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetValue), nameof(Convert.ToUInt16), BaseTypeHelper.NullableUShortType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.IntegerType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetInt32), BaseTypeHelper.IntegerType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.NullableIntegerType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetInt32), BaseTypeHelper.NullableIntegerType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.UIntegerType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetGenericConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetValue), nameof(Convert.ToUInt32), BaseTypeHelper.UIntegerType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.NullableUIntegerType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetGenericConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetValue), nameof(Convert.ToUInt32), BaseTypeHelper.NullableUIntegerType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.LongType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetInt64), BaseTypeHelper.LongType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.NullableLongType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetInt64), BaseTypeHelper.NullableLongType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.ULongType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetGenericConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetValue), nameof(Convert.ToUInt64), BaseTypeHelper.ULongType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.NullableULongType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetGenericConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetValue), nameof(Convert.ToUInt64), BaseTypeHelper.NullableULongType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.FloatType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetFloat), BaseTypeHelper.FloatType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.NullableFloatType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetFloat), BaseTypeHelper.NullableFloatType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.DoubleType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetDouble), BaseTypeHelper.DoubleType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.NullableDoubleType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetDouble), BaseTypeHelper.NullableDoubleType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.DecimalType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetDecimal), BaseTypeHelper.DecimalType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.NullableDecimalType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetDecimal), BaseTypeHelper.NullableDecimalType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.DateTimeType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetDateTime), BaseTypeHelper.DateTimeType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.NullableDateTimeType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetDateTime), BaseTypeHelper.NullableDateTimeType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.BooleanType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetBoolean), BaseTypeHelper.BooleanType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.NullableBooleanType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetBoolean), BaseTypeHelper.NullableBooleanType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.GuidType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetGuid), BaseTypeHelper.GuidType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.NullableGuidType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, this.GetConditionalExpression(dbDataReaderExpression, intExpression, nameof(DbDataReader.GetGuid), BaseTypeHelper.NullableGuidType));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.StreamType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, Expression.Call(null, typeof(StreamHelperParser).GetMethod(nameof(StreamHelperParser.GetStream), BindingFlags.Static | BindingFlags.NonPublic), dbDataReaderExpression, intExpression));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.ByteArrayType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, Expression.Call(null, typeof(BytesHelperParser).GetMethod(nameof(BytesHelperParser.GetBytes), BindingFlags.Static | BindingFlags.NonPublic), dbDataReaderExpression, intExpression));
                    }
                    else if (fieldInfo.FieldType == BaseTypeHelper.CharArrayType)
                    {
                        assignExpression = Expression.Assign(propertyExpression, Expression.Call(null, typeof(CharsHelperParser).GetMethod(nameof(CharsHelperParser.GetChars), BindingFlags.Static | BindingFlags.NonPublic), dbDataReaderExpression, intExpression));
                    }
                    else
                    {
                        continue;
                    }

                    blockExpression = Expression.Block(assignExpression, gotoExpression, lblExpression);

                    Func<TModel, DbDataReader, int, TModel> propertySetterFunc = Expression
                        .Lambda<Func<TModel, DbDataReader, int, TModel>>(blockExpression, new ParameterExpression[] {
                            tParameterExpression,
                            dbDataReaderExpression,
                            intExpression })
                        .Compile();

                    funcsByName.Add(fieldInfo.Name.ToLower(), propertySetterFunc);
                }
            }

            return funcsByName;
        }

        private ConditionalExpression GetConditionalExpression(ParameterExpression dbDataReaderExpression,
            ParameterExpression indexExpression,
            string methodName,
            Type type)
        {
            Type dbDataReaderType = typeof(DbDataReader);

            return Expression.Condition(Expression.IsFalse(Expression.Call(dbDataReaderExpression, dbDataReaderType.GetMethod(nameof(DbDataReader.IsDBNull), new Type[] { typeof(int) }), new ParameterExpression[] { indexExpression })),
                Expression.Convert(Expression.Call(dbDataReaderExpression, dbDataReaderType.GetMethod(methodName, new Type[] { typeof(int) }), new ParameterExpression[] { indexExpression }), type),
                Expression.Default(type));
        }

        private ConditionalExpression GetGenericConditionalExpression(ParameterExpression dbDataReaderExpression,
            ParameterExpression indexExpression,
            string methodName,
            string convertName,
            Type type)
        {
            Type dbDataReaderType = typeof(DbDataReader);
            Type convertType = typeof(Convert);

            return Expression.Condition(Expression.IsFalse(Expression.Call(dbDataReaderExpression, dbDataReaderType.GetMethod(nameof(DbDataReader.IsDBNull), new Type[] { typeof(int) }), new ParameterExpression[] { indexExpression })),
                Expression.Convert(Expression.Call(null, convertType.GetMethod(convertName, BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object) }, null), new Expression[] { Expression.Call(dbDataReaderExpression, dbDataReaderType.GetMethod(methodName, new Type[] { typeof(int) }), new ParameterExpression[] { indexExpression }) }), type),
                Expression.Default(type));
        }
    }
}