﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;

namespace SqlUsage
{
    [Generator]
    public class SQLiteNet_ModelMapperGenerator : ISourceGenerator
    {
        private const string namespaceToken = "$namespace$";
        private const string methodsToken = "$methods$";
        private const string typeNameToken = "$type_name$";
        private const string propertyGettersToken = "$property_getters$";
        private const string propertySetterToken = "$property_setters$";
        private const string propertyNameToken = "$property_name$";
        private const string propertyToken = "$property_name$";
        private const string sqliteGetterToken = "$sqlite_getter$";
        private const string sqliteIndexToken = "$sqlite_index$";

        private const string sqliteMapperClassName = "SqliteMapper";

        private const string FileTemplate =
@"// <auto-generated/>
using SQLite;
namespace $namespace$
{
    public static class SqliteMapper
    {
        $methods$
    }
}";

        private const string MapperMethod =
@"public static $type_name$ Map_$type_name$(sqlite3_stmt statement)
{
    $property_getters$
                                            
    return new $type_name$
    {
        $property_setters$
    }
}";

        private const string PropertyGetterStatement = "var _$property_name$ = SQLite3.$sqlite_getter$(statement, $sqlite_index$);";

        private const string PropertySetterStatement = "$property_name$ = _$property_name$,";

        private const string SqliteIgnoreAttributeTypeName = "SQLite.IgnoreAttribute";

        // SqliteMappers.g.cs 

        // Sqlite_Map_$TypeName$
        public void Execute(GeneratorExecutionContext context)
        {
            var baseNamespace = context.Compilation.GlobalNamespace;
            var modelsNamespace = baseNamespace.ToString() + "." + "Models";

            if (modelsNamespace.Contains("<global namespace>"))
            {
                modelsNamespace = modelsNamespace.Replace("<global namespace>", context.Compilation.AssemblyName);
            }

            var types = context.Compilation.GetSymbolsWithName(x => true, SymbolFilter.Type).ToList();

            var modelsTypes = types.Where(ts => ts.ToString().StartsWith(modelsNamespace)).OfType<INamedTypeSymbol>().ToList();

            List<string> mapperMethods = new List<string>();

            foreach (var type in modelsTypes)
            {
                Console.WriteLine($"-> Generating mapper for '{type.Name}'");
                var methodBody = MapperMethod.Replace(typeNameToken, type.Name);

                var properties = type.GetMembers().OfType<IPropertySymbol>().ToList();

                List<string> propertyGetters = new List<string>();
                List<string> propertySetters = new List<string>();

                var propertyIndex = 0;
                foreach (var property in properties)
                {
                    if (property.GetAttributes().Any(a => a.ToString() == SqliteIgnoreAttributeTypeName))
                    {
                        Console.WriteLine($"--> Skipped '{property.Name}' as it has the {SqliteIgnoreAttributeTypeName} attribute");
                        continue;
                    }

                    var propertyType = property.Type;
                    if (!TryGetMappingStatement(property.Name, propertyType, propertyIndex, out var mappingStatement))
                    {
                        Console.WriteLine($"--> Skipped '{property.Name}' as it's type, '{propertyType.ToString()}', could not have a sqlite mapping statement generated.");
                        continue;
                    }

                    propertyIndex++;
                    propertyGetters.Add(mappingStatement);
                    propertySetters.Add(PropertySetterStatement.Replace(propertyNameToken, property.Name));
                }

            }

            var fileContent = FileTemplate.Replace(namespaceToken, modelsNamespace)
                                          .Replace(methodsToken, string.Join(Environment.NewLine, mapperMethods));

            context.AddSource(sqliteMapperClassName + ".g.cs", fileContent);
        }

        private bool TryGetMappingStatement(string propertyName, ITypeSymbol propertyType, int propertyIndex, out string mappingStatement)
        {
            mappingStatement = string.Empty;
            // string
            // binary data
            // int
            // long
            // boolean
            // double
            // float
            // Guid
            // enum (requires a cast)

            var statement = $"SQLite3.ColumnString(statement, {sqliteIndexToken})";

            if (propertyType.SpecialType == SpecialType.None)
            {
                if (propertyType.ToString() == typeof(UriBuilder).FullName)
                {
                    statement = $"new UriBuilder(SQLite3.ColumnString(statement, {sqliteIndexToken}))";
                }
                else if (propertyType.ToString() == typeof(Uri).FullName)
                {
                    statement = $"new Uri(SQLite3.ColumnString(statement, {sqliteIndexToken}))";
                }
                else if (propertyType.ToString() == typeof(StringBuilder).FullName)
                {
                    statement = $"new StringBuilder(SQLite3.ColumnString(statement, {sqliteIndexToken}))";
                }
                else if (propertyType.ToString() == typeof(Guid).FullName)
                {
                    statement = $"new Guid(SQLite3.ColumnString(statement, {sqliteIndexToken}))";
                }
                else if (propertyType.ToString() == typeof(DateTimeOffset).FullName)
                {
                    statement = $"new Guid(SQLite3.ColumnString(statement, {sqliteIndexToken}))";
                }
                else if (propertyType.ToString() == typeof(TimeSpan).FullName)
                {
                    statement = $"new TimeSpan(SQLite3.ColumnInt64(statement, {sqliteIndexToken}))";
                }
            }
            else
            {

                switch (propertyType.SpecialType)
                {
                    case SpecialType.System_Enum:
                        statement = $"({propertyType.ToString()})SQLite3.ColumnInt(statement, {sqliteIndexToken})";
                        break;
                    case SpecialType.System_String:
                        statement = $"SQLite3.ColumnString(statement, {sqliteIndexToken})";
                        break;
                    case SpecialType.System_Boolean:
                        statement = $"SQLite3.ColumnInt(statement, {sqliteIndexToken}) == 1";
                        break;
                    case SpecialType.System_Int16:
                    case SpecialType.System_Int32:
                    case SpecialType.System_Int64:
                    case SpecialType.System_UInt16:
                    case SpecialType.System_UInt32:
                    case SpecialType.System_UInt64:
                        break;
                    case SpecialType.System_DateTime:
                        statement = $"new DateTime(SQLite3.ColumnInt64(statement, {sqliteIndexToken}))";
                        break;
                    case SpecialType.System_Decimal:
                        statement = $"(decimal)SQLite3.ColumnDouble(statement, {sqliteIndexToken})";
                        break;
                    case SpecialType.System_Double:
                        statement = $"SQLite3.ColumnDouble(statement, {sqliteIndexToken})";
                        break;
                    default:
                        return false;
                }
            }

            statement = "var _$property_name$ = " + statement;

            statement = statement.Trim();
            if (!statement.EndsWith(";"))
            {
                statement += ";";
            }

            mappingStatement = statement.Replace(sqliteIndexToken, propertyIndex.ToString())
                                        .Replace(propertyNameToken, propertyName);

            return true;
        }


        //if (clrType == typeof(String))
        //{
        //    return SQLite3.ColumnString(statement, $sqlite_index$);
        //}
        //else if (clrType == typeof(Int32))
        //{
        //    return (int)SQLite3.ColumnInt(statement, $sqlite_index$);
        //}
        //else if (clrType == typeof(double))
        //{
        //    return SQLite3.ColumnDouble(statement, $sqlite_index$);
        //}
        //else if (clrType == typeof(float))
        //{
        //    return (float)SQLite3.ColumnDouble(statement, $sqlite_index$);
        //}
        //else if (clrType == typeof(DateTimeOffset))
        //{
        //    return new DateTimeOffset(SQLite3.ColumnInt64(statement, $sqlite_index$), TimeSpan.Zero);
        //}
        //else if (clrTypeInfo.IsEnum)
        //{
        //    if (type == SQLite3.ColType.Text)
        //    {
        //        var value = SQLite3.ColumnString(statement, $sqlite_index$);
        //        return Enum.Parse(clrType, value.ToString(), true);
        //    }
        //    else
        //        return SQLite3.ColumnInt(statement, $sqlite_index$);
        //}
        //else if (clrType == typeof(Int64))
        //{
        //    return SQLite3.ColumnInt64(statement, $sqlite_index$);
        //}
        //else if (clrType == typeof(UInt32))
        //{
        //    return (uint)SQLite3.ColumnInt64(statement, $sqlite_index$);
        //}
        //else if (clrType == typeof(decimal))
        //{
        //    return (decimal)SQLite3.ColumnDouble(statement, $sqlite_index$);
        //}
        //else if (clrType == typeof(Byte))
        //{
        //    return (byte)SQLite3.ColumnInt(statement, $sqlite_index$);
        //}
        //else if (clrType == typeof(UInt16))
        //{
        //    return (ushort)SQLite3.ColumnInt(statement, $sqlite_index$);
        //}
        //else if (clrType == typeof(Int16))
        //{
        //    return (short)SQLite3.ColumnInt(statement, $sqlite_index$);
        //}
        //else if (clrType == typeof(sbyte))
        //{
        //    return (sbyte)SQLite3.ColumnInt(statement, $sqlite_index$);
        //}
        //else if (clrType == typeof(byte[]))
        //{
        //    return SQLite3.ColumnByteArray(statement, $sqlite_index$);
        //}
        //}
        //else
        //{
        //    throw new NotSupportedException("Don't know how to read " + clrType);
        //}

        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                // Debugger.Launch();
            }
#endif 
            Console.WriteLine("Initalize 'SQLiteNet_ModelMapperGenerator' code generator");
            // No initialization required for this one
        }
    }
}

