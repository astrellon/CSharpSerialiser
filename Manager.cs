using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace CSharpSerialiser
{
    public class Manager
    {
        #region Fields
        public readonly IReadOnlyList<string> NameSpace;
        public readonly string BaseSerialiserClassName;

        private readonly Dictionary<ClassName, ClassObject> classMap = new Dictionary<ClassName, ClassObject>();

        public IReadOnlyDictionary<ClassName, ClassObject> ClassMap => this.classMap;
        #endregion

        #region Constructor
        public Manager(IReadOnlyList<string> nameSpace, string baseSerialiserClassName)
        {
            this.NameSpace = nameSpace;
            this.BaseSerialiserClassName = baseSerialiserClassName;
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
            if (type.IsEnum)
            {
                containerType = CollectionType.Enum;
            }

            return new ClassType(new ClassName(type.FullName), containerType, genericTypes);
        }

        public static ClassObject CreateObjectFromType(Type type)
        {
            var fields = new List<ClassField>();
            foreach (var field in type.GetFields())
            {
                // Static class fields should be properties as well.
                if (field.Attributes.HasFlag(FieldAttributes.Static))
                {
                    continue;
                }

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
                    var ignore = field.GetCustomAttributes(typeof(NonSerializedAttribute), false).Any();
                    if (ignore || field.Name == null)
                    {
                        continue;
                    }

                    var classType = CreateTypeFromType(fieldType);
                    fields.Add(new ClassField(field.Name, classType));
                }
            }

            var ctors = type.GetConstructors();
            var ctor = ctors.First();
            var ctorFields = new List<ClassField>();
            foreach (var ctorField in ctor.GetParameters())
            {
                var fieldType = (Type)null;
                try
                {
                    fieldType = ctorField.ParameterType;
                }
                catch (IOException)
                {
                    fieldType = null;
                }

                if (fieldType != null)
                {
                    var classType = CreateTypeFromType(fieldType);
                    ctorFields.Add(new ClassField(ctorField.Name, classType));
                }
            }

            var generics = new List<ClassGeneric>();
            foreach (var genericType in type.GetGenericArguments())
            {
                var constraints = new List<ClassType>();
                foreach (var constraint in genericType.GetGenericParameterConstraints())
                {
                    constraints.Add(CreateTypeFromType(constraint));
                }

                generics.Add(new ClassGeneric(genericType.Name, constraints));
            }
            return new ClassObject(new ClassName(type.FullName), fields, ctorFields, generics);
        }
        #endregion
    }
}