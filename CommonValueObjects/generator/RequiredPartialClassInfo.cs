namespace FunctionalDDD.CommonValueObjectGenerator
{
    internal class RequiredPartialClassInfo
    {
        public readonly string NameSpace;
        public readonly string ClassName;
        public readonly string ClassBase;
        public readonly string ClassType;
        public readonly string Accessibility;

        public RequiredPartialClassInfo(string nameSpace, string className, string classBase, string classType, string accessibility)
        {
            NameSpace = nameSpace;
            ClassName = className;
            ClassBase = classBase;
            ClassType = classType;
            Accessibility = accessibility;
        }

    }
}
