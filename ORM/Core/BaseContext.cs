using System;
using System.Collections.Generic;
using System.Reflection;
using ORM.Utils;
using ORM.CustomAttributes;

namespace ORM.Core
{
    public abstract class BaseContext
    {
        protected string connectionString;
        protected List<FieldInfo> contextModelSets;
        protected List<object> contextModelSetInstances;

        public BaseContext(string connectionString)
        {
            this.connectionString = connectionString;
            this.contextModelSets = this.GetContextModelSets();
            this.contextModelSetInstances = new List<object>();
            this.InstanciateContextModelSets();
            this.AddListenersToDbChangesEvents();
            this.CreateTables();
        }

        public void NotifyDbChanges(object sender, ModelSetEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Event: Sender: {sender.GetType()}");
            Console.WriteLine($"Event: Message: {e.Message}");
            if (e.Model != null)
            {
                Console.WriteLine("Event: Model: Properties:");
                foreach (var prop in e.Model.GetType().GetProperties())
                {
                    if (prop.PropertyType.IsConvertibleToSql() && 
                        prop.GetCustomAttribute<IgnoreAttribute>() != null)
                    {
                        Console.WriteLine($"\t{prop.Name} - {prop.GetValue(e.Model)}");
                    }
                }
            }
            
            Console.ResetColor();
        }

        private List<FieldInfo> GetContextModelSets()
        {
            var childType = this.GetType();
            var childFields = childType.GetFields();
            var contextModelSets = new List<FieldInfo>();
            foreach (var field in childFields)
            {
                if (field.FieldType.GetGenericTypeDefinition() == typeof(ModelSet<>))
                {
                    contextModelSets.Add(field);
                }
            }

            return contextModelSets;
        }

        private void InstanciateContextModelSets()
        {
            foreach(var field in this.contextModelSets)
            {
                var modelSetInstance = Activator.CreateInstance(field.FieldType, new object[] { this.connectionString });
                this.contextModelSetInstances.Add(modelSetInstance);
                field.SetValue(this, modelSetInstance);
            }
        }

        private void AddListenersToDbChangesEvents()
        {
            foreach(var instance in this.contextModelSetInstances)
            {
                var modelSetEvent = instance.GetType().GetEvent("DbChangesEvent");
                var handler = Delegate.CreateDelegate(
                    modelSetEvent.EventHandlerType,
                    this,
                    this.GetType().GetMethod(nameof(this.NotifyDbChanges), new Type[] { typeof(object), typeof(ModelSetEventArgs)}));
                modelSetEvent.AddEventHandler(instance, handler);
            }
        }

        private void CreateTables()
        {
            foreach (var instance in this.contextModelSetInstances)
            {
                instance.GetType().GetMethod("CreateTable").Invoke(instance, null);
            }
        }
    }
}
