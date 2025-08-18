using System;

namespace SFramework.ECS.Runtime
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class SFRequireComponentAttribute : Attribute
    {
        public Type[] RequiredComponents;
        
        public SFRequireComponentAttribute(params Type[] requiredComponents)
        {
            this.RequiredComponents = requiredComponents;
        }
    }

}