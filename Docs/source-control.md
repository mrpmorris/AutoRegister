# Source control

* [File contents](#file-contents)
* [Ensuring the manifest is up to date in your build pipeline](#build-pipeline)

<a id="file-contents"></a>
## File contents
When building a project with **AutoRegister** enabled, **AutoRegister**
will generate a manifest text file with the name
`{ProjectName}.Morris.AutoRegister.manifest` along side your
`{ProjectName}.csproj` file.

The contents of the file are a CSV formatted text file, containing the
following information.

* Module
* Attribute
* Scope
* ServiceType
* ServiceImplementation

This can be used as a text representation of what has been registered that you can check into source control.

## Example
```c#
namespace NS1
{
   [AutoRegister(Find.DescendantsOf, typeof(BaseClass), RegisterAs.ImplementingClass, WithLifetime.Scoped)]
   [AutoRegister(Find.AnyTypeOf, typeof(IPaymentStrategy), RegisterAs.SearchedType, WithLifetime.Singleton)]
   public partial class DependencyRegistration {}
}
```
|Module|Attribute|Scope|ServiceType|ServiceImplementation|
|-|-|-|-|-|
|NS1.DependencyRegistration|
||Find DescendantsOf NS1.BaseClass RegisterAs ImplementingClass|
|||Scoped|ChildClass1|ChildClass1|
|||Scoped|ChildClass2|ChildClass2|
||Find AnyTypeOf NS1.IPaymentStrategy RegisterAs SearchedType|
|||Singleton|IPaymentStrategy|PaymentStrategy1|
|||Singleton|IPaymentStrategy|PaymentStrategy2|
|Next module here|

* Modules are listed in alphabetical order of their full class name.
* Attributes are listed in the same order as in the source code.
* Services are listed in alphabetical order of their full class name.

<a id="build-pipeline"></a>
## Ensuring the manifest is up to date in your build pipeline
The manifest is generated whenever you build you project. There is a possibility that a developmer
might make an alteration to your codebase and then check in the changes without including the
updated manifest text file, or possibly without even bothering to build the project at all.

You can check for this scenario in your build pipeline quite easily. Once the pipeline has
built the manifest file you can check if there were any changes to the file with the following
command

### Azure
```
- script: |
    result=$(git status *.Morris.AutoRegister.manifest --porcelain)
    if [ -n "$result" ]; then
      echo "Error: Uncommitted changes detected in .Morris.AutoRegister.manifest files:"
      echo "$result"
      exit 1
    else
      echo "AutoRegister manifests are up to date."
    fi
  displayName: 'Ensure AutoRegister manifests are up to date'
```

### Github Actions
```
 - name: Ensure AutoRegister manifests are up to date
      run: |
        result=$(git status *.Morris.AutoRegister.manifest --porcelain)
        if [ -n "$result" ]; then
          echo "Error: Uncommitted changes detected in .Morris.AutoRegister.manifest files:"
          echo "$result"
          exit 1
        else
          echo "AutoRegister manifests are up to date."
        fi
```
