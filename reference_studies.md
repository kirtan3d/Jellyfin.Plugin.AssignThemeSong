# Reference Plugin Analysis

## 1. File Transformation Plugin (https://github.com/IAmParadox27/jellyfin-plugin-file-transformation)

### Plugin Structure
- **Plugin Class**: `FileTransformationPlugin` extends `BasePlugin<PluginConfiguration>`
- **Constructor**: Takes `IApplicationPaths`, `IXmlSerializer`, `IServiceProvider`
- **GUID**: `5e87cc92-571a-4d8d-8d98-d2d4147f9f90`
- **Name**: "File Transformation"

### Key Features
- **Service Provider**: Uses `IServiceProvider` for dependency injection
- **Static Instance**: Maintains `Instance` property for global access
- **Plugin Interface**: Provides static `PluginInterface.RegisterTransformation(JObject payload)` method

### Registration Process
1. Plugins use reflection to find File Transformation assembly
2. Call `PluginInterface.RegisterTransformation()` with JObject payload
3. Payload structure:
   ```json
   {
     "id": "plugin-guid-as-string",
     "fileNamePattern": "index.html", // Plain filename, not regex
     "callbackAssembly": "Assembly.FullName",
     "callbackClass": "Full.ClassName",
     "callbackMethod": "MethodName"
   }
   ```

### Important Notes
- Uses `IServiceProvider` in constructor, not `IServerConfigurationManager`
- Registration happens via static method, not through constructor
- File Transformation itself doesn't register transformations in its constructor

## 2. HoverTrailer Plugin (https://github.com/Fovty/HoverTrailer)

### Plugin Structure
- **Plugin Class**: `Plugin` extends `BasePlugin<PluginConfiguration>`
- **Constructor**: Takes `IApplicationPaths`, `IXmlSerializer`, `ILogger<Plugin>`, `IServerConfigurationManager`
- **GUID**: `82c71cde-a52b-44f1-a18e-d93eb6a35ed0`
- **Name**: "HoverTrailer"

### Key Features
- **Constructor Parameters**: Includes `IServerConfigurationManager` for network config
- **File Transformation Registration**: Uses reflection in constructor
- **Fallback Mechanism**: Direct file injection if File Transformation not available
- **Comprehensive Error Handling**: Never re-throws exceptions in constructor

### Registration Process
1. In constructor, calls `TryRegisterFileTransformation()`
2. Uses reflection to find File Transformation assembly
3. Calls `PluginInterface.RegisterTransformation()` with JObject
4. Uses plugin's own GUID as transformation ID
5. Callback method signature: `public static string TransformIndexHtmlCallback(PatchRequestPayload payload)`

### Important Notes
- Constructor includes `IServerConfigurationManager` parameter
- Never re-throws exceptions - allows plugin to load even if registration fails
- Uses static callback method
- Transformation ID matches plugin GUID

## 3. Jellyfin Enhanced (https://github.com/n00bcodr/Jellyfin-Enhanced)

### Plugin Structure
- **Plugin Class**: `JellyfinEnhanced` extends `BasePlugin<PluginConfiguration>`
- **Constructor**: Takes `IApplicationPaths`, `IXmlSerializer`, `Logger` (custom logger)
- **GUID**: `f69e946a-4b3c-4e9a-8f0a-8d7c1b2c4d9b`
- **Name**: "Jellyfin Enhanced"

### Key Features
- **Custom Logger**: Uses custom `Logger` class instead of `ILogger<T>`
- **Direct File Injection**: Modifies index.html directly
- **No File Transformation**: Does not use File Transformation plugin
- **Cleanup on Uninstall**: Removes script tags when uninstalling

### Registration Process
- **No File Transformation registration**
- Directly modifies `index.html` file
- Uses custom script injection method

### Important Notes
- Uses custom logger, not standard `ILogger<T>`
- No dependency on File Transformation plugin
- Direct file manipulation approach

## 4. Collection Sections Plugin (https://github.com/IAmParadox27/jellyfin-plugin-collection-sections)

### Plugin Structure
- **Plugin Class**: `CollectionSectionPlugin` extends `BasePlugin<PluginConfiguration>`
- **Constructor**: Takes `IApplicationPaths`, `IXmlSerializer`, `IServerApplicationHost`, `ILogger<CollectionSectionPlugin>`
- **GUID**: `043b2c48-b3e0-4610-b398-8217b146d1a4`
- **Name**: "Collection Sections"

### Key Features
- **Constructor Parameters**: Includes `IServerApplicationHost`
- **Configuration-Driven**: Registers sections when configuration changes
- **HTTP Registration**: Uses HTTP client to register with Home Screen Sections
- **Reflection Fallback**: Also uses reflection for registration

### Registration Process
1. Configuration change triggers registration
2. Uses HTTP client to call `/HomeScreen/RegisterSection`
3. Also uses reflection as fallback
4. Registration happens asynchronously after plugin loads

### Important Notes
- Registration happens in configuration change handler, not constructor
- Uses both HTTP and reflection approaches
- Includes `IServerApplicationHost` in constructor

## Critical Observations

### Constructor Patterns
1. **File Transformation**: `(IApplicationPaths, IXmlSerializer, IServiceProvider)`
2. **HoverTrailer**: `(IApplicationPaths, IXmlSerializer, ILogger<Plugin>, IServerConfigurationManager)`
3. **Jellyfin Enhanced**: `(IApplicationPaths, IXmlSerializer, Logger)` - custom logger
4. **Collection Sections**: `(IApplicationPaths, IXmlSerializer, IServerApplicationHost, ILogger<CollectionSectionPlugin>)`

### Registration Timing
- **File Transformation**: No registration in constructor
- **HoverTrailer**: Registration in constructor
- **Collection Sections**: Registration on configuration change
- **Jellyfin Enhanced**: No registration (direct file injection)

### Error Handling
- **HoverTrailer**: Never re-throws exceptions, always allows plugin to load
- **All others**: No explicit exception handling in constructors

### Service Dependencies
- **File Transformation**: Uses `IServiceProvider`
- **HoverTrailer**: Uses `IServerConfigurationManager` for network config
- **Collection Sections**: Uses `IServerApplicationHost` for server info
- **Jellyfin Enhanced**: Uses custom `Logger`

## Key Takeaways for xThemeSong

1. **Constructor parameters vary** between plugins - no single standard
2. **HoverTrailer pattern** is most similar to what we're trying to achieve
3. **Registration in constructor** is acceptable for HoverTrailer
4. **Never re-throw exceptions** in constructor - always allow plugin to load
5. **Use plugin's own GUID** as transformation ID
6. **Callback method must be static** and match expected signature
7. **File Transformation expects plain filename** "index.html", not regex pattern
