using System.Collections.Generic;

namespace CSharpSerialiser
{
    public class Config
    {
        public class FindBaseClass
        {
            public readonly string TypeNameRegex;
            public readonly string TypeField;
            public readonly string InterfaceBase;

            public FindBaseClass(string typeNameRegex, string typeField, string interfaceBase)
            {
                this.TypeNameRegex = typeNameRegex;
                this.TypeField = typeField;
                this.InterfaceBase = interfaceBase;
            }
        }

        public class FindClass
        {
            public readonly string TypeNameRegex;

            public FindClass(string typeNameRegex)
            {
                this.TypeNameRegex = typeNameRegex;
            }
        }

        public abstract class FormatConfig
        {
            public readonly string OutputFolder;

            public FormatConfig(string outputFolder)
            {
                this.OutputFolder = outputFolder;
            }
        }

        public class BinaryFormatConfig : FormatConfig
        {
            public const string Type = "binary";

            public BinaryFormatConfig(string outputFolder) : base(outputFolder)
            {

            }
        }

        public class JsonFormatConfig : FormatConfig
        {
            public const string Type = "json";

            public JsonFormatConfig(string outputFolder) : base(outputFolder)
            {

            }
        }

        public class SimpleJsonFormatConfig : FormatConfig
        {
            public const string Type = "simplejson";

            public SimpleJsonFormatConfig(string outputFolder) : base(outputFolder)
            {

            }
        }

        #region Fields
        public readonly IReadOnlyList<string> NameSpace;
        public readonly string BaseSerialiserClassName;
        public readonly string TargetProject;

        public readonly IReadOnlyList<FindBaseClass> FindBaseClasses;
        public readonly IReadOnlyList<FindClass> FindClasses;
        public readonly IReadOnlyList<FindClass> FindClassStubs;
        public readonly IReadOnlyList<FormatConfig> FormatConfigs;
        #endregion

        #region Constructor
        public Config(IReadOnlyList<string> nameSpace, string baseSerialiserClassName, string targetProject, IReadOnlyList<FindBaseClass> findBaseClasses, IReadOnlyList<FindClass> findClasses, IReadOnlyList<FindClass> findClassStubs, IReadOnlyList<FormatConfig> formatConfigs)
        {
            this.NameSpace = nameSpace;
            this.BaseSerialiserClassName = baseSerialiserClassName;
            this.TargetProject = targetProject;
            this.FindBaseClasses = findBaseClasses;
            this.FindClasses = findClasses;
            this.FindClassStubs = findClassStubs;
            this.FormatConfigs = formatConfigs;
        }
        #endregion

        #region Methods
        #endregion
    }
}