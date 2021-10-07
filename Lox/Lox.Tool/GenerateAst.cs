﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox.Tool
{
    class GenerateAst
    {
        public static void Main(string[] args)
        {
            string outputDir = "../../../../Lox";
            defineAst(outputDir, "Expr", new List<string>() {
              "Binary   : Expr left, Token _operator, Expr right",
              "Grouping : Expr expression",
              "Literal  : Object value",
              "Unary    : Token _operator, Expr right"
            });
        }

        private static void defineAst(string outputDir, string baseName, List<string> types)
        {
            string path = outputDir + "/" + baseName + ".cs";
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using Lox;");
            sb.AppendLine("namespace Lox {");
            sb.AppendLine("public abstract class " + baseName + " {");

            defineVisitor(sb, baseName, types);

            // The AST classes.
            foreach (string type in types)
            {
                string className = type.Split(':')[0].Trim();
                string fields = type.Split(':')[1].Trim();
                defineType(sb, baseName, className, fields);
            }
            sb.AppendLine();
            sb.AppendLine("  public abstract R accept<R>(Visitor<R> visitor);");
            sb.AppendLine("}");
            sb.AppendLine("}");
            File.WriteAllText(path, sb.ToString());
        }

        private static void defineType(StringBuilder sb, string baseName, string className, string fieldList)
        {
            sb.AppendLine("  public class " + className + " : " +
                baseName + " {");

            // Constructor.
            sb.AppendLine("    public " + className + "(" + fieldList + ") {");

            // Store parameters in fields.
            string[] fields = fieldList.Split(", ");
            foreach (string field in fields)
            {
                string name = field.Split(" ")[1];
                sb.AppendLine("      this." + name + " = " + name + ";");
            }

            sb.AppendLine("    }");

            // Visitor pattern.
            sb.AppendLine();
            sb.AppendLine("    public override R accept<R>(Visitor<R> visitor) {");
            sb.AppendLine("      return visitor.visit" + className + baseName + "(this);");
            sb.AppendLine("    }");

            // Fields.
            sb.AppendLine();
            foreach (string field in fields)
            {
                sb.AppendLine("   public readonly " + field + ";");
            }

            sb.AppendLine("  }");
        }

        private static void defineVisitor(StringBuilder sb, String baseName, List<String> types)
        {
            sb.AppendLine("  public interface Visitor<R> {");

            foreach (String type in types)
            {
                String typeName = type.Split(':')[0].Trim();
                sb.AppendLine("    R visit" + typeName + baseName + "(" +
                    typeName + " " + baseName.ToLower() + ");");
            }

            sb.AppendLine("  }");
        }
    }
}