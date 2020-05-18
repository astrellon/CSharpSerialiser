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
        public readonly ClassType Type;
        #endregion

        #region Constructor
        public ClassField(string name, ClassType type)
        {
            this.Name = name;
            this.Type = type;
        }
        #endregion

        #region Methods
        #endregion
    }
}