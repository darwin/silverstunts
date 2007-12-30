using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using IronPython;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System.IO;
using System.Text;

namespace SilverStunts
{
    public class ScriptShell
    {
        private string languageId;
        private string moduleName;
        private ScriptModule module;
        public ScriptModule Module { get {return module;} }
        private ScriptEngine engine;
        public ScriptEngine Engine { get { return engine; } }
        private LanguageProvider provider;

        private delegate string ReprDelegate(object arg);
        private ReprDelegate reprDelegate;
        private string multiline = ""; 

        public ScriptShell(string languageId, string name)
        {
            this.languageId = languageId;
            this.moduleName = name;
            this.provider = ScriptDomainManager.CurrentManager.GetLanguageProvider(languageId);
            this.engine = provider.GetEngine();
            this.module = ScriptDomainManager.CurrentManager.CreateModule(name);
            this.module.FileName = "SilverStunts";

            reprDelegate = engine.EvaluateAs<ReprDelegate>("repr");

            // bring WPF symbols into scripting namespace
            ScriptModule wpf = ScriptDomainManager.CurrentManager.CreateModule("wpf");
            Stream s = this.GetType().Assembly.GetManifestResourceStream("SilverStunts.WPF.py");
            string code = new StreamReader(s).ReadToEnd();
            engine.Execute(code, wpf);
            module.SetVariable("wpf", wpf);
        }

        public bool IsComplete(string text, bool allowIncomplete)
        {
            InteractiveCodeProperties props = engine.GetInteractiveCodeProperties(text);
            return InteractiveCodePropertiesEnum.IsValidAndComplete(props, allowIncomplete);
        }

        public void Execute(string code)
        {
            engine.Execute(code, module);
        }

        public string ExecuteAsString(string code)
        {
            string res = "OK\n";
            try
            {
                Execute(code);
            }
            catch (Exception e)
            {
                res = ExceptionToString(e);
                Page.Current.PrintConsole(res, Page.ConsoleOutputKind.Exception);
                return res;
            }
            Page.Current.PrintConsole(res, Page.ConsoleOutputKind.Interactive);
            return res;
        }
        
        public bool TryExpression(string expression)
        {
            CompilerOptions copts = engine.GetDefaultCompilerOptions();
            ErrorSink sink = new ErrorSink();
        
            SourceCodeUnit scu = new SourceCodeUnit(engine, expression);
            CompilerContext cc = new CompilerContext(scu, copts, sink);
            engine.Compiler.ParseExpressionCode(cc);
            return !sink.AnyError;
        }
   
        public object Evaluate(string expression)
        {
            return engine.Evaluate(expression, module);
        }

        public string EvaluateAsString(string expression)
        {
            string res;
            try
            {
                object ret = Evaluate(expression);
                if (ret == null) res = "null\n";
                else
                {
                    // get text representation as is seen by Python
                    res = reprDelegate(ret)+"\n";
                }
            }
            catch (Exception e)
            {
                res = ExceptionToString(e);
                Page.Current.PrintConsole(res, Page.ConsoleOutputKind.Exception);
                return res;
            }
            Page.Current.PrintConsole(res, Page.ConsoleOutputKind.Interactive);
            return res;
        }

        public string EvaluatePartialExpression(string line, bool force)
        {
            if (multiline == "")
            {
                if (TryExpression(line) || force)
                {
                    return EvaluateAsString(line);
                }
                else
                {
                    multiline = line;
                }
            }
            else multiline += "\n" + line;

            bool complete = true;
            try
            {
                complete = force || IsComplete(line, multiline.EndsWith("\n"));
            }
            catch (Exception)
            {
                complete = true;
            }

            if (complete)
            {
                string res = ExecuteAsString(multiline);
                multiline = "";
                return res;
            }

            return "";
        }

        public void SetVariable(string name, object value)
        {
            module.SetVariable(name, value);
        }

        public object GetVariable(string name)
        {
            return module.LookupVariable(name);
        }

        public bool VariableExists(string name)
        {
            return module.VariableExists(name);
        }

        public bool RemoveVariable(string name)
        {
            return module.RemoveVariable(name);
        }

        public void ClearVariables()
        {
            module.ClearVariables();
        }

        public IScriptEngine GetEngine(string languageId)
        {
            return engine;
        }

        public string ExceptionToString(Exception e)
        {
            string msg;
            DynamicStackFrame[] dfs = DynamicHelpers.GetDynamicStackFrames(e);
            if (dfs==null || dfs.Length == 0)
            {
                msg = engine.FormatException(e);
                if (!msg.EndsWith("\n")) msg += "\n";
            }
            else
            {   
                msg = e.Message + "\n";
            }
            return msg;
        }
    }
}
