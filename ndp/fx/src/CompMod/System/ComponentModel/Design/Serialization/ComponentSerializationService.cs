//------------------------------------------------------------------------------
// <copyright file="ComponentSerializationService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel.Design.Serialization {

    using System.Collections;
    using System.ComponentModel;
    using System.IO;
    using System.Security.Permissions;

    /// <devdoc>
    ///     This class serializes a set of components or serializable objects into a 
    ///     serialization store.  The store can then be deserialized at a later 
    ///     date.  ComponentSerializationService differs from other serialization 
    ///     schemes in that the serialization format is opaque, and it allows for 
    ///     partial serialization of objects.  For example, you can choose to 
    ///     serialize only selected properties for an object.
    ///     
    ///     This class is abstract. Typically a DesignerLoader will provide a 
    ///     concrete implementation of this class and add it as a service to 
    ///     its DesignSurface.  This allows objects to be serialized in the 
    ///     format best suited for them.
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public abstract class ComponentSerializationService
    {

        /// <devdoc>
        ///     This method creates a new SerializationStore.  The serialization store can 
        ///     be passed to any of the various Serialize methods to build up serialization 
        ///     state for a group of objects.
        /// </devdoc>
        public abstract SerializationStore CreateStore();
        
        /// <devdoc>
        ///     This method loads a SerializationStore and from the given
        ///     stream.  This store can then be used to deserialize objects by passing it to 
        ///     the various Deserialize methods.
        /// </devdoc>
        public abstract SerializationStore LoadStore(Stream stream);

        /// <devdoc>
        ///     This method serializes the given object to the store.  The store 
        ///     can be used to serialize more than one object by calling this method 
        ///     more than once. 
        /// </devdoc>
        public abstract void Serialize(SerializationStore store, object value);

        /// <devdoc>
        ///     Normal serialization only serializes values that differ from the component's default state.  
        ///     This provides the most compact serialization mechanism but assumes that during deserilalization 
        ///     a new, freshly created object will be used.  If an existing object is used properties that 
        ///     contained default values during serialization would not be reset back to their defaults.  
        ///     The SerializeAbsolute method does not require this assumption on the deserializing end.  
        ///     Instead, it saves data in the serialization store about default values as well so that 
        ///     deserialization can reset non-default properties back to their default values.  This is 
        ///     especially true for collection objects, where the collections are either cleared and 
        ///     items re-added, or individual items are removed and added.
        /// </devdoc>
        public abstract void SerializeAbsolute(SerializationStore store, object value);
        
        /// <devdoc>
        ///     This method serializes the given member on the given object.  This method 
        ///     can be invoked multiple times for the same object to build up a list of 
        ///     serialized members within the serialization store.  The member generally 
        ///     has to be a property or an event.
        /// </devdoc>
        public abstract void SerializeMember(SerializationStore store, object owningObject, MemberDescriptor member);
        
        /// <devdoc>
        ///     This method serializes the given member on the given object, 
        ///     but also serializes the member if it contains the default value.  
        ///     Note that for some members, containing the default value and setting 
        ///     the same value back to the member are different concepts.  For example, 
        ///     if a property inherits its value from a parent object if no local value 
        ///     is set, setting the value back to the property can may not be what is desired.  
        ///     SerializeMemberAbsolute takes this into account and would clear the state of 
        ///     the property in this case.
        /// </devdoc>
        public abstract void SerializeMemberAbsolute(SerializationStore store, object owningObject, MemberDescriptor member);
        
        /// <devdoc>
        ///     This method deserializes the given store to produce a collection of 
        ///     objects contained within it.  If a container is provided, objects 
        ///     that are created that implement IComponent will be added to the container. 
        /// </devdoc>
        public abstract ICollection Deserialize(SerializationStore store);
        
        /// <devdoc>
        ///     This method deserializes the given store to produce a collection of 
        ///     objects contained within it.  If a container is provided, objects 
        ///     that are created that implement IComponent will be added to the container. 
        /// </devdoc>
        public abstract ICollection Deserialize(SerializationStore store, IContainer container);
        
        /// <devdoc>
        ///     This method deserializes the given store, but rather than produce 
        ///     new objects, the data in the store is applied to an existing 
        ///     set of objects that are taken from the provided container.  This 
        ///     allows the caller to pre-create an object however it sees fit.  If
        ///     an object has deserialization state and the object is not named in 
        ///     the set of existing objects, a new object will be created.  If that 
        ///     object also implements IComponent, it will be added to the given 
        ///     container.  Objects in the container must have names that 
        ///     match objects in the serialization store in order for an existing 
        ///     object to be used.  If validateRecycledTypes is true it is guaranteed
        ///     that the deserialization will only work if applied to an object of the 
        ///     same type.
        /// </devdoc>
        public abstract void DeserializeTo(SerializationStore store, IContainer container, bool validateRecycledTypes, bool applyDefaults);

        public void DeserializeTo(SerializationStore store, IContainer container) {
            DeserializeTo(store, container, true, true);
        }

        public void DeserializeTo(SerializationStore store, IContainer container, bool validateRecycledTypes) {
            DeserializeTo(store, container, validateRecycledTypes, true);
        }
    }
}

