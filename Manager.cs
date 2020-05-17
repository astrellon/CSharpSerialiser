using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace CSharpSerialiser
{
    public class Manager
    {
        #region Fields
        public readonly IReadOnlyList<string> NameSpace;

        private readonly Dictionary<ClassName, ClassObject> classMap = new Dictionary<ClassName, ClassObject>();

        public IReadOnlyDictionary<ClassName, ClassObject> ClassMap => this.classMap;
        #endregion

        #region Constructor
        public Manager(IReadOnlyList<string> nameSpace)
        {
            this.NameSpace = nameSpace;
        }
        #endregion

        #region Methods
        public void AddClass(ClassObject classObject)
        {
            this.classMap[classObject.FullName] = classObject;
        }

        public static ClassType CreateTypeFromType(Type type)
        {
            var containerType = CollectionType.NotACollection;
            var genericTypes = new List<ClassType>();
            if (type.IsGenericType)
            {
                var genericDef = type.GetGenericTypeDefinition();
                genericTypes.AddRange(type.GenericTypeArguments.Select(CreateTypeFromType));

                if (genericDef.IsAssignableFrom(typeof(IReadOnlyList<>)))
                {
                    containerType = CollectionType.Array;
                    if (genericTypes.First().CollectionType != CollectionType.NotACollection)
                    {
                        containerType = CollectionType.List;
                    }
                }
                else if (genericDef.IsAssignableFrom(typeof(IList<>)))
                {
                    containerType = CollectionType.List;
                }
                else if (genericDef.IsAssignableFrom(typeof(IReadOnlyDictionary<,>)))
                {
                    containerType = CollectionType.Dictionary;
                }
            }

            return new ClassType(new ClassName(type.FullName), containerType, genericTypes);
        }

        public static ClassObject CreateObjectFromType(Type type)
        {
            var fields = new List<ClassField>();
            foreach (var field in type.GetFields())
            {
                var fieldType = (Type)null;
                try
                {
                    fieldType = field.FieldType;
                }
                catch (IOException)
                {
                    fieldType = null;
                }

                if (fieldType != null)
                {
                    var classType = CreateTypeFromType(field.FieldType);
                    fields.Add(new ClassField(field.Name, classType));
                }
            }

            var ctors = type.GetConstructors();
            var ctor = ctors.First();
            var ctorFields = ctor.GetParameters().Select(p => p.Name).ToList();

            return new ClassObject(new ClassName(type.FullName), fields, ctorFields);
        }
        #endregion
    }
}