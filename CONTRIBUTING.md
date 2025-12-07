# Contributing to CmdPal History Extension

Thank you for your interest in contributing! This document provides guidelines and instructions for contributing to the project.

## 🤝 How to Contribute

### Reporting Issues

1. Check existing issues first
2. Use the issue template
3. Provide detailed information:
   - Windows version
   - PowerToys version
   - Steps to reproduce
   - Expected vs actual behavior
   - Screenshots if applicable

### Suggesting Features

1. Check if the feature already exists or is planned
2. Create a detailed feature request
3. Explain the use case and benefits
4. Provide examples if possible

### Contributing Code

1. **Fork the repository**
   ```bash
   git clone https://github.com/yourusername/CmdPal-History-Extension.git
   ```

2. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Make your changes**
   - Follow existing code style
   - Add tests for new functionality
   - Update documentation

4. **Run tests**
   ```bash
   dotnet test
   ```

5. **Commit your changes**
   ```bash
   git commit -m "Add: Brief description of changes"
   ```

6. **Push to your fork**
   ```bash
   git push origin feature/your-feature-name
   ```

7. **Create a Pull Request**
   - Describe your changes
   - Reference related issues
   - Wait for review

## 📝 Code Style

### C# Guidelines

- Follow [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable names
- Add XML documentation comments for public APIs
- Keep methods focused and small
- Use LINQ where appropriate

**Example:**
```csharp
/// <summary>
/// Adds a command to the history database
/// </summary>
/// <param name="command">The command to add</param>
/// <param name="context">Optional context information</param>
public void AddCommand(string command, string context = "")
{
    if (string.IsNullOrWhiteSpace(command))
    {
        return;
    }
    
    // Implementation...
}
```

### XAML Guidelines

- Use proper indentation (4 spaces)
- Group related properties
- Use meaningful names for UI elements
- Follow WPF best practices

### Testing Guidelines

- Write unit tests for new features
- Aim for high code coverage
- Use descriptive test names
- Follow AAA pattern (Arrange, Act, Assert)

**Example:**
```csharp
[Fact]
public void AddCommand_WithValidCommand_ShouldAddToHistory()
{
    // Arrange
    var command = "test command";
    
    // Act
    _historyManager.AddCommand(command);
    
    // Assert
    var history = _historyManager.GetAllHistory();
    Assert.Single(history);
    Assert.Equal(command, history[0].Command);
}
```

## 🔍 Code Review Process

1. All pull requests require review
2. Address review comments promptly
3. Keep discussions focused and respectful
4. Be open to feedback

## 📚 Documentation

When adding features:
- Update README.md if user-facing
- Update ARCHITECTURE.md if technical
- Add inline code comments where needed
- Update configuration examples

## 🐛 Bug Fix Checklist

- [ ] Issue exists and is assigned
- [ ] Bug is reproducible
- [ ] Root cause identified
- [ ] Fix implemented
- [ ] Tests added/updated
- [ ] Documentation updated
- [ ] No regressions introduced

## ✨ Feature Checklist

- [ ] Feature request approved
- [ ] Design discussed
- [ ] Implementation complete
- [ ] Tests added
- [ ] Documentation updated
- [ ] Examples provided
- [ ] Performance considered

## 🏗️ Development Setup

### Prerequisites

- Visual Studio 2022 or VS Code
- .NET 8.0 SDK
- PowerShell 7+
- Git

### Local Development

1. **Clone and setup**
   ```bash
   git clone https://github.com/digitalsorc/CmdPal-History-Extension.git
   cd CmdPal-History-Extension
   dotnet restore
   ```

2. **Build**
   ```bash
   dotnet build
   ```

3. **Run tests**
   ```bash
   dotnet test
   ```

4. **Debug**
   - Open in Visual Studio
   - Set breakpoints
   - Press F5 to debug

### Project Structure

```
src/
├── Models/          - Data models
├── Services/        - Business logic
├── UI/              - WPF interfaces
└── CmdPalPlugin.cs  - Main entry point

tests/               - Unit tests
docs/                - Documentation
config/              - Configuration files
```

## 🧪 Testing

### Running Tests

```bash
# All tests
dotnet test

# Specific test file
dotnet test --filter "FullyQualifiedName~HistoryManagerTests"

# With coverage
dotnet test /p:CollectCoverage=true
```

### Writing Tests

- Use xUnit framework
- One test file per class
- Test public APIs
- Test edge cases
- Mock external dependencies when needed

## 📦 Building

### Debug Build

```bash
dotnet build --configuration Debug
```

### Release Build

```bash
dotnet build --configuration Release
```

### Create Package

```bash
.\build.ps1 -Configuration Release -Package
```

## 🔒 Security

- No sensitive data in commits
- Use parameterized queries
- Validate all user inputs
- Follow secure coding practices
- Report security issues privately

## 📋 Commit Message Guidelines

Use conventional commits:

- `Add:` New feature
- `Fix:` Bug fix
- `Docs:` Documentation changes
- `Test:` Test additions/changes
- `Refactor:` Code refactoring
- `Style:` Formatting changes
- `Perf:` Performance improvements

**Example:**
```
Add: History search with regex support

- Implement regex pattern matching
- Add tests for regex search
- Update documentation
```

## 🎯 Pull Request Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Documentation update
- [ ] Performance improvement

## Testing
- [ ] Tests added/updated
- [ ] All tests pass
- [ ] Manual testing completed

## Checklist
- [ ] Code follows style guidelines
- [ ] Documentation updated
- [ ] No breaking changes
- [ ] Commit messages are clear
```

## 💬 Communication

- Be respectful and professional
- Keep discussions focused
- Ask questions if unclear
- Help others when possible

## 📞 Getting Help

- Check documentation first
- Search existing issues
- Ask in discussions
- Contact maintainers

## 🎉 Recognition

Contributors will be recognized in:
- CONTRIBUTORS.md file
- Release notes
- Project documentation

Thank you for contributing to CmdPal History Extension!
