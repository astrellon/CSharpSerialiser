namespace CSharpSerialiser
{
    public enum CollectionType
    {
        NotACollection, Array, List, Dictionary, Enum
    }

    public class ClassField
    {

        #region Fields
        public readonly string Name;
        public readonly string CamelCaseName;
        public readonly string SafeCamelCaseName;
        public readonly ClassType Type;
        #endregion

        #region Constructor
        public ClassField(string name, ClassType type)
        {
            this.Name = name;
            this.CamelCaseName = CodeGeneratorUtils.ToCamelCase(name);

            this.SafeCamelCaseName = this.CamelCaseName;

            if (CodeGeneratorUtils.CSharpKeywords.Contains(this.CamelCaseName))
            {
                this.SafeCamelCaseName = "@" + this.SafeCamelCaseName;
            }
            this.Type = type;
        }
        #endregion

        #region Methods
        #endregion
    }
}