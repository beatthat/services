Enables you register and then locate global services.

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

#### Register Services to make them available for and lookup.

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

#### Use interfaces for less tight coupling and easier refactoring

It's generally a good idea to use narrow interfaces for services to avoid tight coupling. More concretely, accessing services as interfaces makes it easier to swap the implementation, and, assuming the service interfaces are narrowly defined, also makes it much easier to understand at a glance what your service-dependent code really depends upon. This can be a big time saver when you're refactoring and using tools like 'Find References' to try to go through all the classes that depend on some service.

The [RegisterService] attribute provides a couple of features to make it easier to use interfaces.

###### Interfaces defined directly on the service class are registered by default

```c#
using BeatThat.Services;
public interface Bar {}

[RegisterService]public class Foo : Bar {}

public class UsesBar
{
    void GetBar()
    {
      Bar bar = Services.Require<Bar>(); // returns instance of Foo
    }
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

public class UsesBar
{
  void GetBar()
  {
    Bar bar = Services.Require<Bar>(); // returns instance of Foo
  }
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
  void GetBar()
  {
    Bar bar = Services.Require<Bar>(); // returns instance of Foo
  }
}
````
