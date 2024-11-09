# Logging
[![NuGet](https://img.shields.io/nuget/v/Tharga.Logging)](https://www.nuget.org/packages/Tharga.Logging)
![Nuget](https://img.shields.io/nuget/dt/Tharga.Logging)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![GitHub repo Issues](https://img.shields.io/github/issues/Tharga/Logging?style=flat&logo=github&logoColor=red&label=Issues)](https://github.com/Tharga/Logging/issues?q=is%3Aopen)

## Fluent adding of exceptions data
```
try
{

}
catch (Exception e)
{
    e.AddData("hey", "value").AddData("other-key", "value");
}
```

### Extension to measure and log time and result for action 
```
logger.Measure("action", d =>
{
    //TODO: Some action to measure
});
```