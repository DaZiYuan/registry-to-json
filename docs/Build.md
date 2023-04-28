# Pack
```
dotnet pack

```
# Local Test
```
dotnet pack -o ./packages/ -c Release -p:PackageID=RTJ
dotnet tool uninstall -g RTJ
dotnet tool install --global --add-source ./packages/ RTJ

```
# Upload 
dotnet pack -o ../../LocalNuget/packages -c Release -p:PackageID=RTJ