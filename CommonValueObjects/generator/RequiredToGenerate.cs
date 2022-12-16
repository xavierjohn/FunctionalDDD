namespace FunctionalDDD.CommonValueObjectGenerator
{
    internal class RequiredToGenerate
    {
        public readonly string NameSpace;
        public readonly string ClassName;
        public readonly string ClassBase;
        public readonly string ClassType;

        public RequiredToGenerate(string nameSpace, string className, string classBase, string classType)
        {
            NameSpace = nameSpace;
            ClassName = className;
            ClassBase = classBase;
            ClassType = classType;
        }
    }
}
