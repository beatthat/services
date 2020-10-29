using BeatThat.CollectionsExt;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeatThat.Service
{
    /// <summary>
    /// Marks a class to be autowire registered to Services.
    /// For the autowire registrations to occur, 
    /// generally, this boilerplate should be execute on the first frame of app launch:
    /// 
    /// Services.Init(() => {
    /// 	// if you need to wait for complete start here (services may have some async init behaviour)
    /// });
    /// 
    /// @param serviceInterface (optional) interface used to retrieve the Command from services, 
    /// when left null, registers to the concrete type.
    /// 
    /// 
    /// @param priority (optional/default 0) used in the case that multiple implementations 
    /// 	have autowireservice attributes for the same service interface.
    /// 	Resolves to the implementation with the LOWEST priority value.
    /// 
    /// @param proxyInterfaces (optional) list of alternative interfaces that can be used to locate the service
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class RegisterServiceAttribute : ServiceInterfaceAttribute
    {
        public RegisterServiceAttribute(
            Type serviceInterface = null,
            InterfaceRegistrationPolicy interfaceRegistrationPolicy
                = InterfaceRegistrationPolicy.RegisterInterfacesDeclaredOnTypeIfNoProxyInterfacesArgument,
            Type[] proxyInterfaces = null,
            int priority = 0)
            : base(serviceInterface, priority)
        {
            this.proxyInterfaces = proxyInterfaces;
            this.interfaceRegistrationPolicy = interfaceRegistrationPolicy;

        }

        virtual public bool hasProxyInterfacesArgument
        {
            get
            {
                return this.proxyInterfaces != null;
            }
        }

        virtual public void GetProxyInterfaces(Type registeredType, ICollection<Type> result)
        {
            if (this.proxyInterfaces != null && this.proxyInterfaces.Length > 0)
            {
                result.AddRange(this.proxyInterfaces);
            }
        }

        public enum OnProxyInterfaceMissingPolicy
        {
            NoAction = 0,
            Warn = 1,
            Throw = 2
        }

        protected void FindAndAddProxyTypes(
            Type implType,
            Type[] proxyTypes,
            Type[] searchTypes,
            ICollection<Type> toInterfaces,
            OnProxyInterfaceMissingPolicy onMissing = OnProxyInterfaceMissingPolicy.Warn)
        {
            foreach (var pt in proxyTypes)
            {
                FindAndAddProxyType(
                    implType,
                    pt,
                    searchTypes,
                    toInterfaces,
                    OnProxyInterfaceMissingPolicy.NoAction);
            }
        }

        protected void FindAndAddProxyType(
            Type implType,
            Type genericType,
            Type[] searchTypes,
            ICollection<Type> toInterfaces,
            OnProxyInterfaceMissingPolicy onMissing = OnProxyInterfaceMissingPolicy.Warn)
        {
            var iFound = Array.Find(searchTypes, intf =>
            {
                return intf.IsGenericType && !intf.ContainsGenericParameters
                           && genericType.IsAssignableFrom(intf.GetGenericTypeDefinition());
            });
            if (iFound == null)
            {
                HandleMissingProxyInterface(
                    onMissing,
                    GetType().FullName + " is applied to type "
                        + implType.FullName + " which doesn't implement "
                        + genericType.FullName);
                return;
            }
            var genArgs = iFound.GetGenericArguments();
            if (genArgs == null || genArgs.Length != 1)
            {
                HandleMissingProxyInterface(
                    onMissing,
                    GetType().FullName + " is applied to type "
                        + implType.FullName + " but "
                        + genericType.FullName
                        + " should be a single param generic type");
                return;
            }
            if (!toInterfaces.Contains(iFound))
            {
                toInterfaces.Add(iFound);
            }
        }

        private void HandleMissingProxyInterface(
            OnProxyInterfaceMissingPolicy policy,
            string message)
        {
            switch (policy)
            {
                case OnProxyInterfaceMissingPolicy.Throw:
                    throw new Exception(message);
                case OnProxyInterfaceMissingPolicy.Warn:
                    Debug.LogWarning(message);
                    break;
            }
        }

        private Type[] proxyInterfaces { get; set; }
        public InterfaceRegistrationPolicy interfaceRegistrationPolicy { get; private set; }
    }

    /// <summary>
    /// Policy on which interfaces to register as proxies for the service.
    /// Any proxy interface can be used to location or inject the service.
    /// 
    /// Examples:
    /// <code>
    /// public interface HasFoo
    /// {
    ///     void Foo();
    /// }
    /// 
    /// public interface HasBar
    /// {
    ///     void Bar();
    /// }
    /// 
    /// public class ParentService : HasFoo
    /// {
    ///     public void Foo();
    /// }
    /// 
    /// public class MyService : HasBar
    /// {
    ///     public void Bar();
    /// }
    /// 
    /// [RegisterService]
    /// </code>
    /// </summary>
    public enum InterfaceRegistrationPolicy
    {
        RegisterInterfacesDeclaredOnTypeIfNoProxyInterfacesArgument = 0,
        RegisterInterfacesDeclaredOnType = 1,
        RegisterInterfacesDeclaredOnTypeAndParents = 2,
        RegisterInterfacesSpecifiedAsProxy = 3
    }
}

