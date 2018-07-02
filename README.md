Enables you define and then locate global services via dependency injection or direct lookup.

## Install

From your unity project folder:

    npm init
    npm install TEMPLATE --save
    echo Assets/packages >> .gitignore
    echo Assets/packages.meta >> .gitignore

The package and all its dependencies will be installed under Assets/Plugins/packages.

In case it helps, a quick video of the above: https://youtu.be/Uss_yOiLNw8

## USAGE

First, any service you want to use must be registered.

#### Register Services to make them available for dependency injection and lookup.

The simplest way to register a service is with the [RegisterService] attribute

```c#
using BeatThat.Services;
[RegisterService]public class Foo {}

public class UsesLookup
{
    void MyMethod()
    {
        // any service that is registered
        // can be looked up directly
        var foo = Services.Require<Foo>();
    }
}
```

#### Use [Inject] for cleaner code and less tight coupling

Dependency injection is a cleaner way to use services. It has a smaller code footprint and less direct dependencies on BeatThat.Services code. Down the road, if you wanted to switch to some other IoC container it would be easier to hack a patch to work with [Inject] tags than, say, Services.Require<SomeClass> calls.

```c#
using BeatThat.Services;
[RegisterService]public class Foo{}

public class UsesInjection_Manual
{
    [Inject]Foo myFoo; // will be set by injection

    void Start()
    {
        // Something needs to call DependencyInjection.InjectDependencies.
        // One option is to call it in Start...
        DependencyInjection.InjectDependencies(this);
    }
}
```

...the above example still directly calls DependencyInjection.InjectDependencies. You can better contain that dependency by using a base class. One also comes provided

```c#
using BeatThat.Services;
[RegisterService]public class Foo{}

public class UsesInjection_WithBaseClass : DependencyInjectedBehaviour
{
    [Inject]Foo myFoo; // will be set by injection
}
```

#### Use interfaces for less tight coupling and easier refactoring

It's generally a good idea to use narrow interfaces for services to avoid tight coupling. More concretely, accessing services as interfaces makes it easier to swap the implementation, and, assuming the service interfaces are narrowly defined, also makes it much easier to understand at a glance what your service-dependent code really depends upon. This can be a big time saver when you're refactoring and using tools like 'Find References' to try to go through all the classes that depend on some service.

The [RegisterService] attribute provides a couple of features to make it easier to use interfaces.

###### Use injection freely on interfaces defined directly on the service class

```c#
using BeatThat.Services;
public interface Bar {}

[RegisterService]public class Foo : Bar {}

public class UsesBar : DependencyInjectedBehaviour
{
    [Inject]Bar myBar; // will be set by injection to Foo
}
````

###### Use 'proxyInterfaces' to register interfaces further up the chain

```c#
using BeatThat.Services;
public interface Bar {}

public class FooBase : Bar {}

[RegisterService(
    // interface Bar will not be auto registered
    // because it is not defined directly on class Foo
    proxyInterfaces: new System.Type[] { typeof(Bar) }
)]
public class Foo : FooBase {}

public class UsesBar : DependencyInjectedBehaviour
{
    [Inject]Bar myBar; // will be set by injection to Foo
}
````

###### Use 'interfaceRegistrationPolicy' to just register all interfaces
```c#
using BeatThat.Services;
public interface Bar {}

public class FooBase : Bar {}

[RegisterService(
    // register all interfaces on class and parents
    interfaceRegistrationPolicy: InterfaceRegistrationPolicy.RegisterInterfacesDeclaredOnTypeAndParents
)]
public class Foo : FooBase {}

public class UsesBar : DependencyInjectedBehaviour
{
    [Inject]Bar myBar; // will be set by injection to Foo
}
````

## SAMPLES

This package installs with a Samples that has a few basic examples
that demonstrate how to use services, dependency injection etc.

Each Sample also has a short video covering what's going on in the example.

* Example_01_DependencyInjection: https://youtu.be/oaZdGZ98fdA
