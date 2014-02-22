using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bardez.Projects.SwordOfTheStars.SotS2.Utility
{
    /// <summary>Contains reflection helper methods to assist in accessing private members of classes</summary>
	public static class ReflectionHelper
	{
        /// <summary>Exposes a locked instance's private field</summary>
        /// <typeparam name="FieldType">Type of value to return</typeparam>
        /// <typeparam name="BaseType">Type of value to query</typeparam>
        /// <param name="instance">Object instance to extract from</param>
        /// <param name="fieldName">Name of the field to expose</param>
        /// <returns>The extracted value</returns>
        public static FieldType PrivateField<BaseType, FieldType>(BaseType instance, String fieldName)
        {
            Type t = typeof(BaseType);

            //HACK: use reflection to access a private type.
            FieldType privateVariable = default(FieldType);
            FieldInfo privateField = t.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (privateField == null)
                throw new NullReferenceException(String.Format("Could not retrieve the \"{0}\" FieldInfo for {1}.", fieldName, t.FullName));
            else
                privateVariable = (FieldType)(privateField.GetValue(instance));

            return privateVariable;
        }

        /// <summary>Sets a locked instance's private field</summary>
        /// <typeparam name="FieldType">Type of value to set</typeparam>
        /// <typeparam name="BaseType">Type of value to query</typeparam>
        /// <param name="instance">Object instance to set the value for</param>
        /// <param name="fieldName">Name of the field to expose</param>
        /// <param name="value">Value to set</param>
        public static void PrivateField<BaseType, FieldType>(BaseType instance, String fieldName, FieldType value)
        {
            Type t = typeof(BaseType);

            //HACK: use reflection to access a private type.
            FieldInfo privateField = t.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (privateField == null)
                throw new NullReferenceException(String.Format("Could not retrieve the \"{0}\" FieldInfo for {1}.", fieldName, t.FullName));

            privateField.SetValue(instance, value);
        }
        /// <summary>Exposes a locked instance's private field</summary>
        /// <typeparam name="FieldType">Type of value to return</typeparam>
        /// <typeparam name="BaseType">Type of value to query</typeparam>
        /// <param name="instance">Object instance to extract from</param>
        /// <param name="fieldName">Name of the field to expose</param>
        /// <returns>The extracted value</returns>
        public static FieldType PrivateStaticField<BaseType, FieldType>(String fieldName)
        {
            Type t = typeof(BaseType);

            //HACK: use reflection to access a private type.
            FieldType privateVariable = default(FieldType);
            FieldInfo privateField = t.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Static);
            if (privateField == null)
                throw new NullReferenceException(String.Format("Could not retrieve the \"{0}\" FieldInfo for {1}.", fieldName, t.FullName));
            else
                privateVariable = (FieldType)(privateField.GetValue(null));

            return privateVariable;
        }

        /// <summary>Exposes a FleetWidget's private property</summary>
        /// <typeparam name="FieldType">Type of value to return</typeparam>
        /// <typeparam name="BaseType">Type of value to query</typeparam>
        /// <param name="instance">Object instance to extract from</param>
        /// <param name="propertyName">Name of the property to expose</param>
        /// <returns>The extracted value</returns>
        public static FieldType PrivateProperty<BaseType, FieldType>(BaseType instance, String propertyName)
        {
            Type t = typeof(BaseType);

            //HACK: use reflection to access a private method.
            FieldType privateVariable = default(FieldType);
            PropertyInfo privateProperty = t.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (privateProperty == null)
                throw new NullReferenceException(String.Format("Could not retrieve the \"{0}\" PropertyInfo for {1}.", propertyName, t.FullName));
            else
            {
                MethodInfo mi = privateProperty.GetGetMethod(true);
                privateVariable = (FieldType)(mi.Invoke(instance, null));
            }

            return privateVariable;
        }

        /// <summary>Returns the method to invoke for a base type</summary>
        /// <param name="methodName">Name of the method to expose</param>
        /// <returns>The MethodInfo for the method to be invoked</returns>
        public static MethodInfo PrivateMethod<BaseType>(String methodName)
        {
            Type t = typeof(BaseType);

            //HACK: use reflection to access a private method.
            MethodInfo privateMethod = t.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (privateMethod == null)
                throw new NullReferenceException(String.Format("Could not retrieve the \"{0}\" MethodInfo for {1}.", methodName, t.FullName));

            return privateMethod;
        }

        /// <summary>Returns the event to invoke for a base type</summary>
        /// <param name="eventName">Name of the event to expose</param>
        /// <returns></returns>
        public static EventInfo PrivateEvent<BaseType>(String eventName)
        {
            Type t = typeof(BaseType);

            //HACK: use reflection to access a private method.
            EventInfo privateEvent = t.GetEvent(eventName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (privateEvent == null)
                throw new NullReferenceException(String.Format("Could not retrieve the \"{0}\" EventInfo for {1}.", eventName, t.FullName));

            return privateEvent;
        }
	}
}