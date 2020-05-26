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

        private readonly Dictionary<ClassName, ClassBaseObject> classBaseObjectMap = new Dictionary<ClassName, ClassBaseObject>();
        public IReadOnlyDictionary<ClassName, ClassBaseObject> ClassBaseObjectMap => this.classBaseObjectMap;

        private readonly Dictionary<ClassName, ClassStub> classStubMap = new Dictionary<ClassName, ClassStub>();
        public IReadOnlyDictionary<ClassName, ClassStub> ClassStubMap => this.classStubMap;

        #endregion

        #region Constructor
        public Manager(IReadOnlyList<string> nameSpace, string baseSerialiserClassName)
        {
            this.NameSpace = nameSpace;
            this.BaseSerialiserClassName = baseSerialiserClassName;
        }
        #endregion

        #region Methods
        public void AddClassStub(ClassStub classStub)
        {
            Console.WriteLine($"Adding class stub: {classStub.FullName}");
            this.classStubMap[classStub.FullName] = classStub;
        }

        public void AddClass(ClassObject classObject)
        {
            Console.WriteLine($"Adding class: {classObject.FullName.Value}");
            this.classMap[classObject.FullName] = classObject;
        }

        public void AddBaseClass(ClassBaseObject classBaseObject)
        {
            Console.WriteLine($"Adding base class: {classBaseObject.FullName.Value}");
            this.classBaseObjectMap[classBaseObject.FullName] = classBaseObject;

            if (!classBaseObject.InterfaceName.IsEmpty)
            {
                this.classBaseObjectMap[classBaseObject.InterfaceName] = classBaseObject;
            }
        }

        public bool IsKnownClassOrBase(ClassName className)
        {
            return this.classMap.ContainsKey(className) ||
                this.classBaseObjectMap.ContainsKey(className) ||
                this.classStubMap.ContainsKey(className);
        }

        public ClassStub CreateStubFromType(Type type)
        {
            return new ClassStub(new ClassName(type.FullName));
        }

        public ClassObject CreateObjectFromType(Type type, ClassBaseObject baseObject = null)
        {
            var fields = new List<ClassField>();
            foreach (var field in type.GetFields())
            {
                // Static class fields should be properties as well.
                if (field.Attributes.HasFlag(FieldAttributes.Static))
                {
                    continue;
                }

                if (TryGetValidFieldType(field, out var fieldType))
                {
                    if (field.Name == null)
                    {
                        continue;
                    }

                    try
                    {
                        var ignore = field.GetCustomAttribute<NonSerializedAttribute>(true);
                        if (ignore != null)
                        {
                            continue;
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        // Ignore type load exception
                    }

                    var classType = CreateTypeFromType(fieldType);
                    fields.Add(new ClassField(field.Name, classType));
                }
            }

            var ctors = type.GetConstructors();
            var allCtorFields = new List<List<ClassField>>();
            foreach (var ctor in ctors)
            {
                var ctorFields = new List<ClassField>();
                allCtorFields.Add(ctorFields);

                foreach (var ctorField in ctor.GetParameters())
                {
                    if (TryGetValidParameterType(ctorField, out var fieldType))
                    {
                        var classType = CreateTypeFromType(fieldType);
                        ctorFields.Add(new ClassField(ctorField.Name, classType));
                    }
                }
            }

            var classGenerics = CreateGenericsFromType(type);
            return new ClassObject(new ClassName(type.FullName), fields, allCtorFields, classGenerics, baseObject);
        }

        public ClassBaseObject AddBaseObjectFromType(Type type, string typeDiscriminatorName, Type interfaceBase = null)
        {
            var subclasses = Manager.GetEnumerableOfType(type);
            if (!subclasses.Any())
            {
                throw new Exception("BaseClass has no known sub classes");
            }

            var typeDiscriminatorField = subclasses.First().GetField(typeDiscriminatorName, BindingFlags.Public | BindingFlags.Static);

            if (typeDiscriminatorField == null)
            {
                throw new Exception("Unable to find type discriminator in sub class");
            }

            if (!TryGetValidFieldType(typeDiscriminatorField, out var typeDiscriminatorType))
            {
                throw new Exception("Unable to get valid field type for type discrimination");
            }

            // Make sure that all comps have the same type discrimination and that it's on all sub classes.
            foreach (var subclass in subclasses.Skip(1))
            {
                var checkTypeDiscriminatorField = subclass.GetField(typeDiscriminatorName, BindingFlags.Public | BindingFlags.Static);

                if (checkTypeDiscriminatorField == null)
                {
                    throw new Exception("Not all sub classes have type discriminator field");
                }

                if (!TryGetValidFieldType(checkTypeDiscriminatorField, out var checkTypeDiscriminatorType))
                {
                    throw new Exception("Unable to get valid field type for sub class type discrimination");
                }

                if (checkTypeDiscriminatorType != typeDiscriminatorType)
                {
                    throw new Exception("Not all sub classes have the same type for type discrimination");
                }
            }

            var classType = CreateTypeFromType(typeDiscriminatorType);
            var classField = new ClassField(typeDiscriminatorName, classType);

            var interfaceName = ClassName.Empty;
            if (interfaceBase != null)
            {
                interfaceName = new ClassName(interfaceBase.FullName);
            }

            var classGenerics = CreateGenericsFromType(type);

            // Bit sneaky to give the class base object a mutable list even though it wants a readonly one
            // and to update it after creation.
            var subclassObjects = new List<ClassBaseObject.SubclassPair>();
            var result = new ClassBaseObject(new ClassName(type.FullName), classField, classGenerics, subclassObjects, interfaceName);

            foreach (var subclass in subclasses)
            {
                var classObject = this.CreateObjectFromType(subclass, result);
                var typeValue = subclass.GetField(typeDiscriminatorName, BindingFlags.Public | BindingFlags.Static).GetValue(null);
                subclassObjects.Add(new ClassBaseObject.SubclassPair(classObject, typeValue));

                this.AddClass(classObject);
            }

            this.AddBaseClass(result);

            return result;
        }

        public static ClassType CreateTypeFromType(Type type)
        {
            var containerType = CollectionType.NotACollection;
            var genericTypes = new List<ClassType>();
            var enumUnderlayingType = (ClassType)null;
            if (type.IsGenericType)
            {
                var genericDef = type.GetGenericTypeDefinition();
                genericTypes.AddRange(type.GenericTypeArguments.Select(CreateTypeFromType));

                if (genericDef.IsArray)
                {
                    containerType = CollectionType.Array;
                }
                else if (genericDef.IsAssignableFrom(typeof(IReadOnlyDictionary<,>)))
                {
                    containerType = CollectionType.Dictionary;
                }
                else if (genericDef.IsAssignableFrom(typeof(IList<>)) ||
                    genericDef.IsAssignableFrom(typeof(List<>)) ||
                    genericDef.IsAssignableFrom(typeof(IReadOnlyList<>)))
                {
                    containerType = CollectionType.List;
                }
            }
            if (type.IsEnum)
            {
                enumUnderlayingType = CreateTypeFromType(Enum.GetUnderlyingType(type));
                containerType = CollectionType.Enum;
            }

            return new ClassType(new ClassName(type.FullName), containerType, genericTypes, enumUnderlayingType);
        }

        public static IEnumerable<Type> GetEnumerableOfType(Type input)
        {
            var types = Utils.GetLoadedTypes(input);

            foreach (var type in types)
            {
                var result = (Type)null;
                try
                {
                    if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(input))
                    {
                        result = type;
                    }
                }
                catch (IOException)
                {

                }

                if (result != null)
                {
                    yield return result;
                }
            }
        }

        private static IReadOnlyList<ClassGeneric> CreateGenericsFromType(Type type)
        {
            var classGenerics = new List<ClassGeneric>();
            foreach (var genericType in type.GetGenericArguments())
            {
                var constraints = new List<ClassType>();
                foreach (var constraint in genericType.GetGenericParameterConstraints())
                {
                    constraints.Add(CreateTypeFromType(constraint));
                }

                classGenerics.Add(new ClassGeneric(genericType.Name, constraints));
            }

            return classGenerics;
        }

        private static bool TryGetValidFieldType(FieldInfo field, out Type result)
        {
            try
            {
                result = field.FieldType;
            }
            catch (IOException)
            {
                result = null;
            }

            return result != null;
        }

        private static bool TryGetValidParameterType(ParameterInfo parameter, out Type result)
        {
            try
            {
                result = parameter.ParameterType;
            }
            catch (IOException)
            {
                result = null;
            }

            return result != null;
        }
        #endregion
    }
}