using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace DailyReport
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class CreateableAttribute : Attribute
    {
        public bool Createable { get; private set; }

        public CreateableAttribute(bool createable)
        {
            Createable = createable;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class UpdateableAttribute : Attribute
    {
        public bool Updateable { get; private set; }

        public UpdateableAttribute(bool updateable)
        {
            Updateable = updateable;
        }
    }

    public class CreateableContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization)
                                .Where(p => IsPropertyCreatable(type, p))
                                .ToList();
        }

        private static bool IsPropertyCreatable(Type type, JsonProperty property)
        {
            var isCreateable = true;
            var propInfo = type.GetRuntimeProperty(property.PropertyName);

            if (propInfo != null)
            {
                var createableAttr = propInfo.GetCustomAttribute(typeof(CreateableAttribute), false);
                isCreateable = createableAttr == null || ((CreateableAttribute)createableAttr).Createable;
            }

            return isCreateable;
        }
    }

    public class UpdateableContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization)
                                .Where(p => IsPropertyUpdateable(type, p))
                                .ToList();
        }

        private static bool IsPropertyUpdateable(Type type, JsonProperty property)
        {
            var isUpdateable = true;
            var propInfo = type.GetRuntimeProperty(property.PropertyName);

            if (propInfo != null)
            {
                var updateableAttr = propInfo.GetCustomAttribute(typeof(UpdateableAttribute), false);
                isUpdateable = updateableAttr == null || ((UpdateableAttribute)updateableAttr).Updateable;
            }

            return isUpdateable;
        }
    }
}
