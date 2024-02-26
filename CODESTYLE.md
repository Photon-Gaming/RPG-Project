# RPG Project Code Style Guide

- [RPG Project Code Style Guide](#rpg-project-code-style-guide)
  - [1. Naming](#1-naming)
  - [2. Specificity, Clarity and Shorthand](#2-specificity-clarity-and-shorthand)
  - [3. Formatting and Layout](#3-formatting-and-layout)
  - [4. General Tidiness](#4-general-tidiness)

## 1. Naming

1. Non-public fields and properties (inc. `internal`, `protected`, `private`, etc), all method parameters, and all local variables should use `camelCase`.
2. Public fields and properties, all methods, all types, and all namespaces should use `PascalCase`.
3. Manually created property backing fields should be prefixed with an underscore (`_`). Nothing else should be.
4. Public properties should be favoured over public fields, with the exception of `static readonly` fields. Non-public properties should only be used if custom getter/setter logic is required.
    - e.g. Prefer this:
        - `public string MyVar { get; set; }`
    - over this:
        - `public string MyVar;`
    - But this:
        - `private string myVar;`
    - over this:
        - `private string myVar { get; set; }`
5. No identifiers should contain underscores, with the exception of event callbacks, which should separate the object name and the event with a single `_`.
    - Event methods for specific objects should match the casing of the name of that object. Otherwise, use standard `PascalCase`.
    - e.g: An event callback for the `MouseMove` event on an element with the name `tileMapScroll` should be named `tileMapScroll_MouseMove`.
6. Interfaces should start with an uppercase `I`. The following character should then also be uppercase.
7. Avoid the use of numerals `0-9` in identifiers.
8. Identifiers should adequately explain they're doing. They shouldn't be *overly* long, but you also shouldn't need to read the code to know what a method will do.

## 2. Specificity, Clarity and Shorthand

1. Do not use `var`. Write type names explicitly.
2. When declaring and initialising on the same line, use the shorthand `new()` syntax where possible. Otherwise, write the type name explicitly.
    - e.g. Prefer this:
        - `MyType variable = new(1, 2, 3);`
    - over this:
        - `MyType variable = new MyType(1, 2, 3);`
    - But this:
        - `variable = new MyType(1, 2, 3);`
    - over this:
        - `variable = new(1, 2, 3);`
3. Do not omit access modifiers from identifiers that can take them.
    - e.g. Prefer this:
        - `private string field = "Hello";`
    - over this:
        - `string field = "Hello";`
4. If a mathematical/logical operation evaluates in an order other than purely left-to-right, use brackets to clarify it.
    - e.g. Prefer this:
        - `1 + (5 * 6)`
    - over this:
        - `1 + 5 * 6`
5. Generally prefer shorthand operators where it would reasonably make code more concise without sacrificing readability or clarity.
    - e.g. Prefer these:
        - `variable += 5;`
        - `variable[^1]`
        - `variable ??= 5;`
        - `variable = condition ? 5 : 6;`
        - `if (nullableBool ?? false)`
        - `if (variable is < 5 or > 10)`
        - <pre>variable = new MyObject()<br/>{<br/>    A = 1,<br/>    B = 2,<br/>    C = 3<br/>};</pre>
    - Over these:
        - `variable = variable + 5;`
        - `variable[variable.Length - 1]`
        - <pre>if (variable is null)<br/>{<br/>    variable = 5;<br/>}</pre>
        - <pre>if (condition)<br/>{<br/>    variable = 5;<br/>}<br/>else<br/>{<br/>    variable = 6;<br/>}</pre>
        - `if (nullableBool is null ? false : nullableBool.Value)`
        - `if (variable < 5 || variable > 10)`
        - <pre>variable = new MyObject();<br/>variable.A = 1;<br/>variable.B = 2;<br/>variable.C = 3;</pre>
6. Do not use `using static` directives to access static class members without the type name.
7. Prefer the use of `switch` statements over `if` statements unless an `if` statement feels more semantically appropriate.
8. Prefer the use of `switch` expressions over `switch` block statements.
    - e.g. Prefer this:
        - <pre>return variable switch<br/>{<br/>    1 => varOne,<br/>    2 => varTwo,<br/>    _ => varDefault<br/>};</pre>
    - Over this:
        - <pre>switch (variable)<br/>{<br/>    case 1:<br/>        return varOne;<br/>    case 2:<br/>        return varTwo;<br/>    default:<br/>        return varDefault;<br/>}</pre>
9. Mark fields that will not need to be written to after initialisation as `readonly`.
10. Similarly, only define setters on properties that will need to be written to after initialisation, and prefer `private` or `protected` setters unless public setting is desired.
11. Generally, short code is good, but it must be easily readable; one-liners are fun, right up until they're not. If it's not immediately obvious what something is doing or **why it is being done**, explain it in a comment.
12. When using value tuples, give each member a meaningful name. Don't overuse value tuples where it would be more appropriate to use a class or struct, however.
    - e.g. Prefer this:
        - `public (int MemberOne, int MemberTwo) MyVar { get; set; }`
    - Over this:
        - `public (int, int) MyVar { get; set; }`
13. Methods that do not access instance data, and predictably won't need to in the future, should be marked as `static`.
14. Public methods, unless *very* self explanatory with zero possible pitfalls, should include a documentation comment that explains: exactly what the method does; any side effects it has; what any even remotely ambiguous parameters are for and what they do; what value the method returns if not plainly obvious; and any remarks regarding possibly unexpected behaviour of the method.
15. If a method returns a value that goes unused after a call to the method, explicitly write that you are ignoring the return value with a discard (`_`).
    - e.g. Prefer this:
        - <pre>public int SomeMethod()<br/>{<br/>    ...<br/>}<br/><br/>_ = SomeMethod();<br/></pre>
    - Over this:
        - <pre>public int SomeMethod()<br/>{<br/>    ...<br/>}<br/><br/>SomeMethod();<br/></pre>

## 3. Formatting and Layout

1. Always use braces for block statements (e.g. `if`, `for`, `while`, etc) - even if the content is only a single line.
    - e.g. Prefer this:
        - <pre>if (condition)<br/>{<br/>    statement;<br/>}</pre>
    - over this:
        - <pre>if (condition)<br/>    statement;
2. Indentation should be done with spaces, with a single indentation level being 4 spaces wide.
3. Do not use `#region` directives. Consider refactoring the file layout, splitting the code between multiple files in different classes, or (as a last resort) using `partial` classes instead. Similarly, avoid extending comments for the sole purpose of code separation (no `///////////////` or `// -----------------` please).
4. Class members should be defined in the following order, from top to bottom. Within each level, more accessible items should come first (e.g. all `public` methods should come before all `private` methods). Beyond that, ordering and grouping should be meaningful and is up to your discretion.
    1. Constant fields
    2. Static fields
    3. Properties
    4. Fields
    5. Constructors
    6. Interface/Override implementations
    7. Methods
    8. Event Callback Methods
5. Keep lines *reasonably* short. Lines should never cause a fullscreen editor window to be horizontally scrolled, and should rarely require horizontal scrolling when the screen is split in half.
6. Put a single space after the start of a comment, and have two spaces between code and a comment if there is a comment on the same line as code.
    - e.g. Prefer these:
        - `// This is a comment`
        - <pre>myVar = 20;  // Sets myVar to 20</pre>
    - Over these:
        - `//This is a comment`
        - <pre>myVar = 20; // Sets myVar to 20</pre>
7. Leave a blank line between method definitions, unrelated property/field definitions, and other logically separate sections of code. Do not leave more than one blank line in a row.
8. Prefer early returns over nested `if` statements for `void` and `bool` functions. Other methods should determine whether or not multiple `return` statements are appropriate on a case-by-case basis.
9. When breaking up a single statement onto multiple lines, do not increase the indentation level by more than one on a single line.

## 4. General Tidiness

1. Remove unused `using` directives before committing.
2. Do not use `this` except in the few situations where it is required.
3. Remove unnecessary namespace qualification for namespaces that are already included with a `using` directive.
    - e.g. Prefer this:
        - <pre>using System.Drawing;<br/>...<br/>Point myVar = new();</pre>
    - Over this:
        - <pre>using System.Drawing;<br/>...<br/>System.Drawing.Point myVar = new();</pre>
4. Prefer to put namespaces that are used more than once in a `using` directive as an opposed to fully qualifying the type name, unless it would cause conflicts. The exception to this is namespaces *within* the current namespace, which in most cases should **not** be put in a `using` directive.
    - e.g. Prefer this:
        - <pre>using System.Drawing;<br/>...<br/>Point myVar = new();<br/>Point myOtherVar = new();</pre>
    - Over this:
        - <pre>System.Drawing.Point myVar = new();<br/>System.Drawing.Point myOtherVar = new();</pre>
    - But prefer this:
        - <pre>GameObject.Entity myVar = new();<br/>GameObject.Entity myOtherVar = new();</pre>
    - Over this:
        - <pre>using RPGGame.GameObject;<br/>...<br/>Entity myVar = new();<br/>Entity myOtherVar = new();</pre>
5. Do not commit dead/unreachable/otherwise unused sections of code (such as those created by `if (false)` / `if (true)` or equivalent). Utilise personal git stashes for features you want to keep around but do not wish to implement yet.
6. Additionally, code that has since been replaced does not need to be kept - remember it is already immortalised in the git history. Do not store old/unused code in comments.
