using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace VrTeamTesting.Base
{
        internal class MappingAttribute : Attribute
        {
            public string SourceAttribute { get; private set; }

            public MappingAttribute(string sourceAttribute)
            {
                SourceAttribute = sourceAttribute;
            }
        }

    public class Model
    {
        [JsonIgnore]
        public string EntityLogicalName;

        public static readonly List<string> blacklistedAttributes = new List<string> { "statecode", "statuscode" };

        [Mapping("Id")]
        public Guid id { get; set; }

        [Mapping("name")]
        public string name { get; set; }

        //[Mapping("statecode")]
        //public RecordState state { get; set; }

        //[Base.Mapping("createdby")]
        //public Models.SystemUserModel createdby { get; set; }

        //[Base.Mapping("createdon")]
        //public DateTime createdon { get; set; }

        //[Base.Mapping("modifiedby")]
        //public Models.SystemUserModel? modifiedby { get; set; }

        //[Base.Mapping("modifiedon")]
        //public DateTime? modifiedon { get; set; }

        public Model() { }

        public Model(Guid Id)
        {
            id = Id;
        }

        protected Model(Entity entity)
        {
            EntityLogicalName = entity.LogicalName;
            id = entity.Id;
            From(entity);
        }

        protected Model(string entityLogicalName)
        {
            EntityLogicalName = entityLogicalName;
        }

        public Entity To(Entity entity, string prefix = "")
        {
            Entity resultEntity;
            List<string> entityAttributes;
            if (entity == null || Guid.Empty.Equals(entity.Id))
            { //Create Mode
                resultEntity = new Entity(entity.LogicalName);
                entityAttributes = Utils.Common.GetEntityAttributes(entity.LogicalName);
            }
            else
            { //Update
                resultEntity = new Entity(entity.LogicalName, entity.Id);
                //entityAttributes = entity.Attributes.Keys.ToList();
                entityAttributes = Utils.Common.GetEntityAttributes(entity.LogicalName);
            }

            var mappingProperties =
                GetType().GetProperties().Where(p => Attribute.IsDefined(p, typeof(MappingAttribute)));

            foreach (var p in mappingProperties)
            {
                MappingAttribute modelMappingAttribute =
                    p.GetCustomAttributes(true).FirstOrDefault(a => a.GetType() == typeof(MappingAttribute)) as MappingAttribute;

                var entityAttribute = modelMappingAttribute.SourceAttribute;

                entityAttribute = entityAttribute.Replace((prefix + "."), "");

                if (modelMappingAttribute == null)
                    continue;
                else if (!entityAttributes.Contains(modelMappingAttribute.SourceAttribute))
                    continue;
                else if (blacklistedAttributes.Contains(modelMappingAttribute.SourceAttribute))
                    continue;

                var modelAttributeValue = p.GetValue(this, null);

                if (modelMappingAttribute == null || modelAttributeValue == null) //no value in model's attribute
                    continue;
                else if (modelAttributeValue is bool)
                {
                    var value = (bool)modelAttributeValue;
                    if (entity.Contains(entityAttribute))
                    {
                        var entityValue = (bool)entity[entityAttribute];
                        if (value != entityValue)
                        {
                            resultEntity[entityAttribute] = value;
                        }
                    }
                    else
                    {
                        resultEntity[entityAttribute] = value;
                    }
                }
                else if (modelAttributeValue is string)
                {
                    var value = (string)modelAttributeValue;
                    if (entity.Contains(entityAttribute))
                    {
                        var entityValue = (string)entity[entityAttribute];
                        if (string.Compare(value, entityValue) != 0)
                        {
                            resultEntity[entityAttribute] = value;
                        }
                    }
                    else
                    {
                        resultEntity[entityAttribute] = value;
                    }
                }
                else if (modelAttributeValue is Model)
                {
                    var value = (Model)modelAttributeValue;

                    if (entity.Contains(entityAttribute))
                    {
                        var entityValue = (EntityReference)entity[entityAttribute];

                        if (!entityValue.Id.Equals(value.id))
                        {
                            resultEntity[entityAttribute] = new EntityReference(entityValue.LogicalName, value.id);
                        }

                    }
                    else
                    {
                        resultEntity[entityAttribute] = new EntityReference(value.EntityLogicalName, value.id);
                    }
                }
                else if (modelAttributeValue is DateTime)
                {
                    var value = (DateTime)modelAttributeValue;
                    if (entity.Contains(entityAttribute))
                    {
                        var entityValue = (DateTime)entity[entityAttribute];
                        if (DateTime.Compare(value, entityValue) != 0)
                        {
                            resultEntity[entityAttribute] = value;
                        }
                    }
                    else
                    {
                        resultEntity[entityAttribute] = value;
                    }
                }
                else if (modelAttributeValue is decimal)
                {
                    var value = (decimal)modelAttributeValue;
                    if (entity.Contains(entityAttribute))
                    {
                        var entityValue = (decimal)entity[entityAttribute];
                        if (decimal.Compare(value, entityValue) != 0)
                        {
                            resultEntity[entityAttribute] = value;
                        }
                    }
                    else
                    {
                        resultEntity[entityAttribute] = value;
                    }
                }
                else if (modelAttributeValue is int)
                {
                    var value = (int)modelAttributeValue;
                    if (entity.Contains(entityAttribute))
                    {
                        var entityValue = (int)entity[entityAttribute];
                        if (value != entityValue)
                        {
                            resultEntity[entityAttribute] = value;
                        }
                    }
                    else
                    {
                        resultEntity[entityAttribute] = value;
                    }
                }
                else if (p.PropertyType.IsEnum)
                {
                    if (entity.Contains(entityAttribute) && entity[entityAttribute] is OptionSetValue)
                    {
                        int value = (int)modelAttributeValue;
                        int entityValue = (entity[entityAttribute] as OptionSetValue).Value;
                        if (value != entityValue)
                        {
                            resultEntity[entityAttribute] = value;
                        }
                    }
                    else
                    {
                        int value = (int)modelAttributeValue;
                        resultEntity[entityAttribute] = new OptionSetValue(value);
                    }
                }
                else
                {
                    continue;
                }

            }

            return resultEntity;
        }

        public Model From(Entity entity, string prefix = null, int depth = 1)
        {
            if (depth > 2)
                return null;

            if (entity == null)
                throw new ArgumentException("Cannot convert to Model. No Entity");

            id = entity.Id;

            if (prefix != null && entity.Attributes.Contains($"{prefix}.{EntityLogicalName}id"))
            {
                id = new Guid(entity.GetAttributeValue<AliasedValue>($"{prefix}.{EntityLogicalName}id").Value.ToString());
            }

            var allMappingProperties =
                GetType().GetProperties().Where(p => Attribute.IsDefined(p, typeof(MappingAttribute)));

            foreach (var p in allMappingProperties)
            {

                MappingAttribute mappingAttribute =
                    p.GetCustomAttributes(true).FirstOrDefault(a => a.GetType() == typeof(MappingAttribute)) as MappingAttribute;

                if (mappingAttribute == null)
                    continue;

                var entityAttribute = mappingAttribute.SourceAttribute;

                //Prioritize prefix whenever provided!
                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    if (entity.Contains(prefix + "." + entityAttribute))
                    {
                        entityAttribute = prefix + "." + entityAttribute;
                    }
                    else
                    {
                        //if (!entity.Contains(entityAttribute))
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    if (!entity.Contains(entityAttribute))
                    {
                        continue;
                    }
                }

                // OBSOLETE
                //    if (!entity.Contains(entityAttribute))
                //    {
                //        if (!entity.Contains(prefix + "." + entityAttribute))
                //            continue;
                //        else
                //        {
                //            entityAttribute = prefix + "." + entityAttribute;
                //        }
                //    }

                var crmValue = entity[entityAttribute];

                if (crmValue is AliasedValue)
                {
                    crmValue = (crmValue as AliasedValue).Value;
                }

                if (crmValue is EntityReference)
                {
                    object instance = null;

                    EntityReference reference = (EntityReference)crmValue;

                    if (entityAttribute.Equals("createdby") || entityAttribute.Equals("modifiedby") || entityAttribute.Equals("ownerid"))
                    {
                        instance = new Common.Models.SystemUserModel(reference.Id, reference.Name);
                    }
                    else
                    {
                        entity.Id = reference.Id;
                        entity["name"] = reference.Name;
                        entity["chps_name"] = reference.Name;
                        entity.LogicalName = reference.LogicalName;
                        //entity.Attributes.Remove(entityAttribute);
                        instance = Activator.CreateInstance(p.PropertyType);

                        Model instanceModel = (Base.Model)instance;

                        //Avoiding passing prefix to children transformations

                        instance = instanceModel.From(entity, null, (depth + 1));
                    }

                    crmValue = instance;
                }
                else if (crmValue is OptionSetValue)
                {
                    if (p.PropertyType.IsEnum)
                    {
                        crmValue = (crmValue as OptionSetValue).Value;
                    }
                    else
                    {
                        crmValue = entity.FormattedValues[entityAttribute];
                    }
                }

                if (crmValue is bool)
                    crmValue = (bool)crmValue;

                else if (crmValue is DateTime)
                    crmValue = (DateTime)crmValue; //.ToString("MM/dd/yyyy hh:mm tt").Replace('.', '/');

                else if (crmValue is Money)
                    crmValue = ((Money)crmValue).Value;

                if (!(crmValue is Common.Models.SystemUserModel) && !(crmValue is Model) && !(crmValue is int) && !(crmValue is decimal) && !(crmValue is string) && !(crmValue is bool) && !(crmValue is Boolean) && !(crmValue is string) && !(crmValue is Guid) && !(crmValue is DateTime))
                    continue;

                p.SetValue(this, crmValue);
            }

            return this;

        }

        public T GetMappingProperty<T>(string mappingPropertyName)
        {
            var mappingProperties =
                this.GetType().GetProperties().Where(p => Attribute.IsDefined(p, typeof(MappingAttribute)));

            foreach (var p in mappingProperties)
            {
                MappingAttribute mappingAttribute =
                    p.GetCustomAttributes(true).FirstOrDefault(a => a.GetType() == typeof(MappingAttribute)) as MappingAttribute;

                if (mappingAttribute == null)
                    continue;

                var _crmAttribute = mappingAttribute.SourceAttribute;
                if (_crmAttribute == mappingPropertyName)
                {
                    return (T)this.GetType().GetProperty(p.Name).GetValue(this, null);
                }
            }
            return default;
        }

        public bool Has<T>(T child)
        {
            var properties =
                this.GetType().GetProperties();

            foreach (var p in properties)
            {
                Equals(typeof(T), p.GetType());
            }
            return false;
        }

        public TResult GetProperty<TResult>(string propertyName)
        {
            return (TResult)this.GetType().GetProperty(propertyName).GetValue(this, null);
        }

    }


    //============================================
    //Model Class End
}
