# Documentation
## Basic writing and formatting syntax
Here you can find a [basic writing and formatting syntax of Github markdown files](https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax "Basic writing and formatting syntax").

## Documenting Analyzers
Notice that folder SQLInjectionAnalyzer/Analyzers/ contains separate analyzers and each of them is briefly described in its own `README.md`. Please, follow the practice of documenting
your own analyzers. Always create your own separate folder for your own implementation of an analyzer and document the philosophy of why this implementation is needed and how it works.
You can find more about how to document your own analyzers [HERE](../SQLInjectionAnalyzer/Analyzers/README.md).

## Mermaid
Please feel free to use Mermaid library for creating diagrams and visualizations in `Markdown` files. Here is a brief introduction
to [Mermaid library](https://mermaid.js.org/intro/ "Mermaid").

### A brief example of Mermaid diagrams [^1]
#### Flowchart

``` mermaid
graph TD;
    A-->B;
    A-->C;
    B-->D;
    C-->D;
```

#### Sequence diagram

``` mermaid
sequenceDiagram
    participant Alice
    participant Bob
    Alice->>John: Hello John, how are you?
    loop Healthcheck
        John->>John: Fight against hypochondria
    end
    Note right of John: Rational thoughts <br/>prevail!
    John-->>Alice: Great!
    John->>Bob: How about you?
    Bob-->>John: Jolly good!
```

#### Gantt diagram

``` mermaid
gantt
dateFormat  YYYY-MM-DD
title Adding GANTT diagram to mermaid
excludes weekdays 2014-01-10

section A section
Completed task            :done,    des1, 2014-01-06,2014-01-08
Active task               :active,  des2, 2014-01-09, 3d
Future task               :         des3, after des2, 5d
Future task2               :         des4, after des3, 5d
```

#### Git graph
``` mermaid
 gitGraph
       commit
       commit
       branch develop
       commit
       commit
       commit
       checkout main
       commit
       commit
```

[^1]: https://mermaid.js.org/intro/ 