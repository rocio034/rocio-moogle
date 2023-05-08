## Proyecto de Programacion I
> Facultad de Matemática y Computación - Universidad de La Habana.
> Cursos: 2023 - 2024.  
> Autor: Rocio Serrano Laguna.

El presente proyecto consiste en desarrollar un motor de busqueda de texto en ficheros `.txt` para la aplicación web `Moogle`.

## Que es Moogle?
Moogle! es una aplicación *totalmente original* cuyo propósito es buscar inteligentemente un texto en un conjunto de documentos.

Es una aplicación web, desarrollada con tecnología .NET Core 6.0, específicamente usando Blazor como *framework* web para la interfaz gráfica, y en el lenguaje C#.
La aplicación está dividida en dos componentes fundamentales:

- `MoogleServer` es un servidor web que renderiza la interfaz gráfica y sirve los resultados.
- `MoogleEngine` es una biblioteca de clases donde está... ehem... casi implementada la lógica del algoritmo de búsqueda.

## Solucion aplicada
Para la integración del proyecto con Moogle se realizó la modificación del método `Moogle.Query` que está en la clase `Moogle` del proyecto `MoogleEngine`.

Este método devuelve un objeto de tipo `SearchResult`. Este objeto contiene los resultados de la búsqueda realizada por el usuario, que viene en un parámetro de tipo `string` llamado `query`.

La implementación del proyecto consiste en una clase llamada TFIDF la cual contiene todos los métodos y algoritmos necesarios para realizar las operaciones de búsquedas.

## Método de búsqueda
Para realizar la búsqueda teniendo en cuenta los requerimientos planteados,se realizó un estudio de  diferentes algoritmos de búsquedas. Como resultado se decidio utilizar un modelo algebraico conocido como modelo de espacio vectorial el cual representa documentos mediante el uso de vectores en un espacio lineal multidimensional. Para llevar a cabo el metodo de busqueda seleccionado se implemento la clase TFIDF.

## Ejecutando el proyecto

Como prerrequisito hay que tener instalado .NET Core 6.0. Luego, colocarse en la carpeta del proyecto y ejecutar:

### Para Linux
```bash
make dev
```
### Para windows
```bash
dotnet watch run --project MoogleServer
```
