# Model
Data models for diagnostics, taint propagation rules and input.

## Directory structure
- `CSProject/` - data structure for cs project scan result.
- `Method/` - data structure for method scan result.
- `Rules/` - rules for solving taint variables propagation.
- `SyntaxTree/` - data structure for syntax tree (one file) scan result. 
- `Diagnostics.cs` - data structure for all diagnostics measured during analysis.
- `Input.cs` - data structure for input received from console.
- `Scope.cs` - enumeration of possible scopes of analysis.