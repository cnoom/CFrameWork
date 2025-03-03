using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Editor.AssetsManager
{
    /// <summary>
    ///     命名验证器
    /// </summary>
    public static class NamingValidator
    {
        // C# 关键字列表（部分示例）
        private static readonly HashSet<string> _csharpKeywords = new HashSet<string>
        {
            "abstract",
            "as",
            "base",
            "bool",
            "break",
            "byte",
            "case",
            "catch",
            "char",
            "checked",
            "class",
            "const",
            "continue",
            "decimal",
            "default",
            "delegate",
            "do",
            "double",
            "else",
            "enum",
            "event",
            "explicit",
            "extern",
            "false",
            "finally",
            "fixed",
            "float",
            "for",
            "foreach",
            "goto",
            "if",
            "implicit",
            "in",
            "int",
            "interface",
            "internal",
            "is",
            "lock",
            "long",
            "namespace",
            "new",
            "null",
            "object",
            "operator",
            "out",
            "override",
            "params",
            "private",
            "protected",
            "public",
            "readonly",
            "ref",
            "return",
            "sbyte",
            "sealed",
            "short",
            "sizeof",
            "stackalloc",
            "static",
            "string",
            "struct",
            "switch",
            "this",
            "throw",
            "true",
            "try",
            "typeof",
            "uint",
            "ulong",
            "unchecked",
            "unsafe",
            "ushort",
            "using",
            "virtual",
            "void",
            "volatile",
            "while"
        };

        /// <summary>
        ///     生成合法的C#变量名称
        /// </summary>
        public static string SanitizeVariableName(string input)
        {
            if(string.IsNullOrEmpty(input)) return "INVALID_NAME";

            // 第一步：基础清理
            StringBuilder cleanedName = new StringBuilder();
            foreach (char c in input)
            {
                cleanedName.Append(char.IsLetterOrDigit(c) ? c : '_');
            }

            // 第二步：处理特殊开头
            var sanitized = cleanedName.ToString();

            // 处理数字开头
            if(char.IsDigit(sanitized[0]))
            {
                sanitized = "_" + sanitized;
            }

            // 第三步：处理保留关键字
            if(_csharpKeywords.Contains(sanitized.ToLower()))
            {
                sanitized = "@" + sanitized;
            }

            // 第四步：格式规范化
            sanitized = ProcessCamelCase(sanitized);

            // 第五步：去除连续下划线
            sanitized = Regex.Replace(sanitized, @"_+", "_");

            // 第六步：全大写格式
            return sanitized;
        }

        /// <summary>
        ///     将字段转换为驼峰格式（处理包含路径分隔符的情况）
        /// </summary>
        public static string ProcessCamelCase(string input)
        {
   
            StringBuilder result = new StringBuilder();

            int index = 0;
            foreach (char part in input)
            {
                // 首字母大写处理
                if(index++ == 0)
                    result.Append(char.ToUpper(part));
                else
                    result.Append(char.ToLower(part));
            }

            return result.ToString();
        }
    }
}