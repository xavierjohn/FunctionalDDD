namespace FunctionalDDD.CommonValueObjectGenerator
{

    internal class RequiredToGenerate
    {
        public readonly string NameSpace;
        public readonly string ClassName;
        public readonly string ClassBase;

        public RequiredToGenerate(string nameSpace, string className, string classBase)
        {
            NameSpace = nameSpace;
            ClassName = className;
            ClassBase = classBase;
        }
    }
}
